﻿using System;
using Antlr4.Runtime;
using System.Collections.Generic;
using JetBrains.Annotations;
using TypeCobol.Compiler.CodeElements;
using TypeCobol.Compiler.CodeElements.Expressions;
using TypeCobol.Compiler.Parser;
using TypeCobol.Compiler.Nodes;
using TypeCobol.Compiler.CodeModel;
using System.Linq;

namespace TypeCobol.Compiler.Diagnostics {


class ReadOnlyPropertiesChecker: NodeListener {

	private static string[] READONLY_DATATYPES = { "DATE", };

	public void OnNode([NotNull] Node node, ParserRuleContext context, [NotNull] CodeModel.Program program) {
	    VariableWriter variableWriter = node as VariableWriter;
	    if (variableWriter == null) {
	        return; //not our job
	    }
        var element = node.CodeElement as VariableWriter;
		var table = program.SymbolTable;
		foreach (var pair in element.VariablesWritten) {
			if (pair.Key == null) continue; // no receiving item
			var lr = table.GetVariable(pair.Key);
			if (lr.Count != 1) continue; // ambiguity or not referenced; not my job
			var receiving = lr[0];
			checkReadOnly(node.CodeElement, receiving);
		}
	}
	private void checkReadOnly(CodeElement ce, [NotNull] Node receiving) {
		var rtype = receiving.Parent as ITypedNode;
		if (rtype == null) return;
		foreach(var type in READONLY_DATATYPES) {
			if (type.Equals(rtype.DataType.Name.ToUpper()))
				DiagnosticUtils.AddError(ce, type+" properties are read-only");
		}
	}
}


    class FunctionCallChecker : NodeListener {

        public void OnNode(Node node, ParserRuleContext context, CodeModel.Program program)
        {
            var procedureStyleCall = node as ProcedureStyleCall;
            if (procedureStyleCall == null)
                return;

            List<FunctionDeclaration> functionDeclarations = new List<FunctionDeclaration>();

            if (procedureStyleCall.FunctionDeclaration == null)
            {
                var procedureCall = ((ProcedureStyleCallStatement)node.CodeElement).ProcedureCall;

                //Get Funtion just by name and profile (matches on precise parameters)
                functionDeclarations = node.SymbolTable.GetFunction(new URI(procedureCall.FunctionName), (procedureCall as ProcedureCall).AsProfile(node.SymbolTable));
                //Check if there is more than one FunctionDeclaration
                if (CheckFunctionAmbiguity(functionDeclarations, node).Count > 1)
                    return; //Do not continue, the fonction is ambigous


                //Get function just by name (no matches on parameters)
                functionDeclarations = node.SymbolTable.GetFunction(new URI(procedureCall.FunctionName));

                //Check if there is more than one FunctionDeclaration
                if (CheckFunctionAmbiguity(functionDeclarations, node).Count > 1)
                    return; //Do not continue, the fonction is ambigous

                if (functionDeclarations.Count == 0)
                {
                    var m = string.Format("Function {0} does not exists", procedureCall.FunctionName);
                            DiagnosticUtils.AddError(node.CodeElement, m);
                    return;
                }

                procedureStyleCall.FunctionDeclaration = functionDeclarations.FirstOrDefault();
                //If function is not ambigous and exists, lets check the parameters
                Check(node.CodeElement, node.SymbolTable, procedureCall, procedureStyleCall.FunctionDeclaration);
                        }
                }

        private List<FunctionDeclaration> CheckFunctionAmbiguity(List<FunctionDeclaration> functionDeclarations, Node node)
        {
            if (functionDeclarations.Count > 1)
            {
                var m = string.Format("Function {0} is ambigous", ((ProcedureStyleCallStatement)node.CodeElement).ProcedureCall.FunctionName);
                DiagnosticUtils.AddError(node.CodeElement, m);
            }

            return functionDeclarations;
	}

	private void Check(CodeElement e, SymbolTable table, [NotNull] FunctionCall call,
	    [NotNull] FunctionDeclaration definition) {
		var parameters = definition.Profile.Parameters;
        var callArgsCount = call.Arguments != null ? call.Arguments.Length : 0;
        if (callArgsCount > parameters.Count) {
			var m = string.Format("Function {0} only takes {1} parameters", definition.Name, parameters.Count);
			DiagnosticUtils.AddError(e, m);
		}
		for (int c = 0; c < parameters.Count; c++) {
			var expected = parameters[c];
			if (c < callArgsCount) {
				var actual = call.Arguments[c].StorageAreaOrValue;
				if (actual.IsLiteral) continue;
                var callArgName = actual.MainSymbolReference != null ? actual.MainSymbolReference.Name : null;
                var found = table.GetVariable(actual);
                    if (found.Count < 1) DiagnosticUtils.AddError(e, "Parameter " + callArgName + " is not referenced");
                    if (found.Count > 1) DiagnosticUtils.AddError(e, "Ambiguous reference to parameter " + callArgName);
                    if (found.Count != 1) continue;
				var type = found[0] as ITypedNode;
				// type check. please note:
				// 1- if only one of [actual|expected] types is null, overriden DataType.!= operator will detect it
				// 2- if both are null, we WANT it to break: in TypeCobol EVERYTHING should be typed,
				//    and things we cannot know their type as typed as DataType.Unknown (which is a non-null valid type).
				if (type == null || type.DataType != expected.DataType) {
                        var m = string.Format("Function {0} expected parameter {1} of type {2} (actual: {3})", definition.Name, c + 1, expected.DataType, type.DataType);
					DiagnosticUtils.AddError(e, m);
				}
				if (type.Length > expected.Length) {
                        var m = string.Format("Function {0} expected parameter {1} of max length {2} (actual: {3})", definition.Name, c + 1, expected.Length, type.Length);
					DiagnosticUtils.AddError(e, m);
				}
			} else {
                    var m = string.Format("Function {0} is missing parameter {1} of type {2}", definition.Name, c + 1, expected.DataType);
				DiagnosticUtils.AddError(e, m);
			}
		}
	}
}


class FunctionDeclarationTypeChecker: CodeElementListener {

	public void OnCodeElement(CodeElement ce, ParserRuleContext context) {
		var function = ce as FunctionDeclarationHeader;
	    if (function == null) {
                return;//not my job
	    }
		if (function.ActualType == FunctionType.Undefined) {
			DiagnosticUtils.AddError(ce, "Incompatible parameter clauses for "+ToString(function.UserDefinedType)+" \""+function.Name+"\"", context);
		} else
		if (  (function.ActualType == FunctionType.Function && function.UserDefinedType == FunctionType.Procedure)
			||(function.ActualType == FunctionType.Procedure && function.UserDefinedType == FunctionType.Function) ) {
			var message = "Symbol \""+function.Name+"\" is defined as "+ToString(function.UserDefinedType)
						+", but parameter clauses describe a "+ToString(function.ActualType);
			DiagnosticUtils.AddError(ce, message, context);
		}
	}
	private string ToString(FunctionType type) {
		if (type == FunctionType.Undefined) return "symbol";
		if (type == FunctionType.Function) return "function";
		if (type == FunctionType.Procedure) return "procedure";
		return "function or procedure";
	}
}

class FunctionDeclarationChecker: NodeListener {

	public IList<Type> GetNodes() {
		return new List<Type> { typeof(FunctionDeclaration), };
	}
	public void OnNode([NotNull] Node node, ParserRuleContext context, CodeModel.Program program) {
		var header = node.CodeElement as FunctionDeclarationHeader;
	    if (header == null) return; //not my job
		var filesection = node.Get<FileSection>("file");
		if (filesection != null) // TCRFUN_DECLARATION_NO_FILE_SECTION
			DiagnosticUtils.AddError(filesection.CodeElement, "Illegal FILE SECTION in function \""+header.Name+"\" declaration", context);

		CheckNoGlobalOrExternal(node.Get<DataDivision>("data-division"));

		CheckParameters(header.Profile, header, context);
		CheckNoLinkageItemIsAParameter(node.Get<LinkageSection>("linkage"), header.Profile);

		CheckNoPerform(node.SymbolTable.EnclosingScope, node);

	    var headerNameURI = new URI(header.Name);
	    var functions = node.SymbolTable.GetFunction(headerNameURI, header.Profile);
		if (functions.Count > 1)
			DiagnosticUtils.AddError(header, "A function \""+headerNameURI.Head+"\" with the same profile already exists in namespace \""+headerNameURI.Tail+"\".", context);
//		foreach(var function in functions) {
//			if (!function.IsProcedure && !function.IsFunction)
//				DiagnosticUtils.AddError(header, "\""+header.Name.Head+"\" is neither procedure nor function.", context);
//		}
	}

	private void CheckNoGlobalOrExternal(DataDivision node) {
		if (node == null) return; // no DATA DIVISION
		foreach(var section in node.Children()) { // "storage" sections
			foreach(var child in section.Children) {
			        var data = child.CodeElement as DataDescriptionEntry;
			        if (data == null) continue;
			        if (data.IsGlobal) // TCRFUN_DECLARATION_NO_GLOBAL
			            DiagnosticUtils.AddError(data, "Illegal GLOBAL clause in function data item.");
			        if (data.IsExternal) // TCRFUN_DECLARATION_NO_EXTERNAL
			            DiagnosticUtils.AddError(data, "Illegal EXTERNAL clause in function data item.");
			}
		}
	}

	private void CheckParameters([NotNull] ParametersProfile profile, CodeElement ce, ParserRuleContext context) {
		foreach(var parameter in profile.InputParameters)  CheckParameter(parameter, ce, context);
		foreach(var parameter in profile.InoutParameters)  CheckParameter(parameter, ce, context);
		foreach(var parameter in profile.OutputParameters) CheckParameter(parameter, ce, context);
		if (profile.ReturningParameter != null) CheckParameter(profile.ReturningParameter, ce, context);
	}
	private void CheckParameter([NotNull] ParameterDescriptionEntry parameter, CodeElement ce, ParserRuleContext context) {
		// TCRFUN_LEVEL_88_PARAMETERS
		if (parameter.LevelNumber.Value != 1) {
		    DiagnosticUtils.AddError(ce, "Condition parameter \""+parameter.Name+"\" must be subordinate to another parameter.", context);
		}
	    if (parameter.DataConditions != null)
        {
            foreach (var condition in parameter.DataConditions)
            {
                if (condition.LevelNumber.Value != 88)
                    DiagnosticUtils.AddError(ce, "Condition parameter \"" + condition.Name + "\" must be level 88.");
            }
        }
	}
	/// <summary>TCRFUN_DECLARATION_NO_DUPLICATE_NAME</summary>
	/// <param name="node">LINKAGE SECTION node</param>
	/// <param name="profile">Parameters for original function</param>
	private void CheckNoLinkageItemIsAParameter(LinkageSection node, ParametersProfile profile) {
		if (node == null) return; // no LINKAGE SECTION
		var linkage = new List<DataDefinition>();
		AddEntries(linkage, node);
		foreach(var description in linkage) {
			var used = Validate(profile.ReturningParameter, description.Name);
			if (used != null) { AddErrorAlreadyParameter(description, description.QualifiedName); continue; }
			used = GetParameter(profile.InputParameters,  description.Name);
			if (used != null) { AddErrorAlreadyParameter(description, description.QualifiedName); continue; }
			used = GetParameter(profile.OutputParameters, description.Name);
			if (used != null) { AddErrorAlreadyParameter(description, description.QualifiedName); continue; }
			used = GetParameter(profile.InoutParameters,  description.Name);
			if (used != null) { AddErrorAlreadyParameter(description, description.QualifiedName); continue; }
		}
	}
	private void AddEntries(List<DataDefinition> linkage, LinkageSection node) {
		foreach(var definition in node.Children())
			AddEntries(linkage, definition);
	}
	private void AddEntries([NotNull] List<DataDefinition> linkage, DataDefinition node) {
		linkage.Add(node);
		foreach(var child in node.Children())
			AddEntries(linkage, child);
	}
	private ParameterDescriptionEntry GetParameter(IList<ParameterDescriptionEntry> parameters, string name) {
		if (name == null) return null;
		foreach(var p in parameters)
			if (Validate(p, name) != null) return p;
		return null;
	}
	private ParameterDescriptionEntry Validate(ParameterDescriptionEntry parameter, string name) {
		if (parameter != null && parameter.Name.Equals(name)) return parameter;
		return null;
	}
	private void AddErrorAlreadyParameter([NotNull] Node node, [NotNull] QualifiedName name) {
		DiagnosticUtils.AddError(node.CodeElement, name.Head+" is already a parameter.");
	}

	private void CheckNoPerform(SymbolTable table, [NotNull] Node node) {
		if (node is PerformProcedure) {
			var perform = (PerformProcedureStatement)node.CodeElement;
			CheckNotInTable(table, perform.Procedure, perform);
			CheckNotInTable(table, perform.ThroughProcedure, perform);
		}
		foreach(var child in node.Children) CheckNoPerform(table, child);
	}
	private void CheckNotInTable(SymbolTable table, SymbolReference symbol, CodeElement ce) {
		if (symbol == null) return;
		string message = "TCRFUN_NO_PERFORM_OF_ENCLOSING_PROGRAM";
		var found = table.GetSection(symbol.Name);
		if (found.Count > 0) DiagnosticUtils.AddError(ce, message);
		else {
			var paragraphFounds = table.GetParagraph(symbol.Name);
			if (paragraphFounds.Count > 0) DiagnosticUtils.AddError(ce, message);
		}
	}
}



/// <summary>Checks the TypeCobol custom functions rules:
/// * TCRFUN_NO_SECTION_OR_PARAGRAPH_IN_LIBRARY
/// * TCRFUN_LIBRARY_COPY
/// </summary>
class LibraryChecker: NodeListener {

	public void OnNode([NotNull] Node node, ParserRuleContext context, CodeModel.Program program) {
        ProcedureDivision procedureDivision = node as ProcedureDivision;
	    if (procedureDivision == null) {
	        return; //not our job
	    }
        var pdiv = procedureDivision.CodeElement as ProcedureDivisionHeader;
		bool isPublicLibrary = false;
		var elementsInError = new List<CodeElement>();
		var errorMessages = new List<string>();
		foreach(var child in procedureDivision.Children) {
			var ce = child.CodeElement;
			if (child.CodeElement == null) {
				elementsInError.Add(procedureDivision.CodeElement);
				errorMessages.Add("Illegal default section in library.");
			} else {
				var function = child.CodeElement as FunctionDeclarationHeader;
				if (function != null) {
					isPublicLibrary = isPublicLibrary || function.Visibility == AccessModifier.Public;
				} else {
					elementsInError.Add(child.CodeElement);
					errorMessages.Add("Illegal non-function item in library");
				}
			}
		}
		var pgm = (Nodes.Program)procedureDivision.Root.GetChildren<ProgramIdentification>()[0];
		var copies = pgm.GetChildren<LibraryCopyCodeElement>();
		var copy = copies.Count > 0? ((LibraryCopy)copies[0]) : null;
		if (isPublicLibrary) {
//			if (copy == null || copy.CodeElement().Name == null) // TCRFUN_LIBRARY_COPY
//				DiagnosticUtils.AddError(pgm.CodeElement, "Missing library copy in IDENTIFICATION DIVISION.", context);

			if (pdiv.UsingParameters != null && pdiv.UsingParameters.Count > 0)
				DiagnosticUtils.AddError(pdiv, "Illegal "+pdiv.UsingParameters.Count+" USING in library PROCEDURE DIVISION.", context);

			for(int c = 0; c < errorMessages.Count; c++)
				DiagnosticUtils.AddError(elementsInError[c], errorMessages[c], context);
		}
	}
}


}
