﻿using System;
using Antlr4.Runtime;
using System.Collections.Generic;
using TypeCobol.Compiler.AntlrUtils;
using TypeCobol.Compiler.CodeElements;
using TypeCobol.Compiler.CodeElements.Expressions;
using TypeCobol.Compiler.Parser;
using TypeCobol.Compiler.Parser.Generated;
using TypeCobol.Compiler.CodeElements.Functions;
using TypeCobol.Compiler.Nodes;
using TypeCobol.Compiler.CodeModel;

namespace TypeCobol.Compiler.Diagnostics {


class ReadOnlyPropertiesChecker: NodeListener {

	private static string[] READONLY_DATATYPES = { "DATE", };

	public IList<Type> GetNodes() {
		return new List<Type> { typeof(TypeCobol.Compiler.CodeModel.SymbolWriter), };
	}
	public void OnNode(Node node, ParserRuleContext context, CodeModel.Program program) {
		var element = node.CodeElement as TypeCobol.Compiler.CodeModel.SymbolWriter;
		var table = program.SymbolTable;
		foreach (var pair in element.Symbols) {
			if (pair.Item2 == null) continue; // no receiving item
			var lr = table.GetVariable(pair.Item2);
			if (lr.Count != 1) continue; // ambiguity or not referenced; not my job
			var receiving = lr[0];
//TODO#249			checkReadOnly(node.CodeElement, receiving);
		}
	}
/*
	private static void checkReadOnly(CodeElement e, DataDescriptionEntry receiving) {
		if (receiving.TopLevel == null) return;
	    foreach (var type in READONLY_DATATYPES) {
			if (type.Equals(receiving.TopLevel.DataType.Name.ToUpper())) {
				DiagnosticUtils.AddError(e, type + " properties are read-only");
			}
		}
	}
*/
}


class FunctionCallChecker: NodeListener {

	public IList<Type> GetNodes() {
		return new List<Type> { typeof(FunctionCaller), };
	}
	public void OnNode(Node node, ParserRuleContext context, CodeModel.Program program) {
		var statement = node as FunctionCaller;

		var functions = new List<FunctionCall>();
		foreach(var fun in statement.FunctionCalls) {
			var found = node.SymbolTable.GetFunction(fun.QualifiedName);
			if (found.Count != 1) continue; // ambiguity is not our job
			var declaration = (FunctionDeclaration)found[0];
			Check(node.CodeElement, node.SymbolTable, fun, declaration);
		}
	}
	private void Check(CodeElement e, SymbolTable table, FunctionCall call, FunctionDeclaration definition) {
		var parameters = definition.Profile.Parameters;
		if (call.InputParameters.Count > parameters.Count) {
			var m = System.String.Format("Function {0} only takes {1} parameters", definition.Name, parameters.Count);
			DiagnosticUtils.AddError(e, m);
		}
		for (int c = 0; c < parameters.Count; c++) {
			var expected = parameters[c];
			if (c < call.InputParameters.Count) {
				var actual = call.InputParameters[c];
				if (actual.IsLiteral) continue;
				var found = table.GetVariable(new URI(actual.Value));
				if (found.Count < 1) DiagnosticUtils.AddError(e, "Parameter "+actual.Value+" is not referenced");
				if (found.Count > 1) DiagnosticUtils.AddError(e, "Ambiguous reference to parameter "+actual.Value);
				if (found.Count!= 1) continue;
				var type = found[0] as Typed;
				// type check. please note:
				// 1- if only one of [actual|expected] types is null, overriden DataType.!= operator will detect it
				// 2- if both are null, we WANT it to break: in TypeCobol EVERYTHING should be typed,
				//    and things we cannot know their type as typed as DataType.Unknown (which is a non-null valid type).
				if (type == null || type.DataType != expected.DataType) {
					var m = System.String.Format("Function {0} expected parameter {1} of type {2} (actual: {3})", definition.Name, c+1, expected.DataType, type.DataType);
					DiagnosticUtils.AddError(e, m);
				}
				if (type != null && type.Length > expected.Length) {
					var m = System.String.Format("Function {0} expected parameter {1} of max length {2} (actual: {3})", definition.Name, c+1, expected.Length, type.Length);
					DiagnosticUtils.AddError(e, m);
				}
			} else {
				var m = System.String.Format("Function {0} is missing parameter {1} of type {2}", definition.Name, c+1, expected.DataType);
				DiagnosticUtils.AddError(e, m);
			}
		}
	}
}



class FunctionDeclarationChecker: NodeListener {

	public IList<Type> GetNodes() {
		return new List<Type> { typeof(FunctionDeclaration), };
	}
	public void OnNode(Node node, ParserRuleContext context, CodeModel.Program program) {
		var header = node.CodeElement as FunctionDeclarationHeader;
		FunctionDeclarationProfile profile = null;
		var profiles = node.GetChildren<FunctionDeclarationProfile>();
		if (profiles.Count < 1) // no PROCEDURE DIVISION internal to function
			DiagnosticUtils.AddError(header, "Function \""+header.Name+"\" has no parameters and does nothing.");
		else if (profiles.Count > 1)
			foreach(var p in profiles)
				DiagnosticUtils.AddError(p.CodeElement(), "Function \""+header.Name+"\" can have only one parameters profile.");
		else profile = profiles[0].CodeElement();

		var filesection = node.Get("file");
		if (filesection != null) // TCRFUN_DECLARATION_NO_FILE_SECTION
			DiagnosticUtils.AddError(filesection.CodeElement, "Illegal FILE SECTION in function \""+header.Name+"\" declaration", context);

		CheckNoGlobalOrExternal(node.Get<DataDivision>("data-division"));

		CheckParameters(profile.Profile, header);
		CheckNoLinkageItemIsAParameter(node.Get<LinkageSection>("linkage"), profile.Profile);
/*
		var functions = node.SymbolTable.GetFunction(header.Name, profile.Profile);
		if (functions.Count > 1)
			DiagnosticUtils.AddError(profile, "A function with the same name and profile already exists.", context);
		foreach(var function in functions)
			if (!function.IsProcedure && !function.IsFunction)
				DiagnosticUtils.AddError(profile, "\""+header.Name+"\" is neither procedure nor function.", context);
*/	}

	private void CheckNoGlobalOrExternal(DataDivision node) {
		if (node == null) return; // no DATA DIVISION
		foreach(var section in node.Children()) { // "storage" sections
			foreach(var child in section.Children()) {
				var data = (DataDescriptionEntry)child.CodeElement;
				if (data.IsGlobal) // TCRFUN_DECLARATION_NO_GLOBAL
					DiagnosticUtils.AddError(data, "Illegal GLOBAL clause in function data item.");
				if (data.IsExternal) // TCRFUN_DECLARATION_NO_EXTERNAL
					DiagnosticUtils.AddError(data, "Illegal EXTERNAL clause in function data item.");
			}
		}
	}

	private void CheckParameters(ParametersProfile profile, CodeElement ce) {
		foreach(var parameter in profile.InputParameters)  CheckParameter(parameter, ce);
		foreach(var parameter in profile.InoutParameters)  CheckParameter(parameter, ce);
		foreach(var parameter in profile.OutputParameters) CheckParameter(parameter, ce);
		if (profile.ReturningParameter != null) CheckParameter(profile.ReturningParameter, ce);
	}
	private void CheckParameter(ParameterDescription parameter, CodeElement ce) {
//		if (parameter.IsConditionNameDescription) {// TCRFUN_LEVEL_88_PARAMETERS
//			if (parameter.TopLevel == null) DiagnosticUtils.AddError(ce, "Condition parameter \""+parameter.Name.Name+"\" must be subordinate to another parameter.");
//			if (parameter.LevelNumber != 88) DiagnosticUtils.AddError(ce, "Condition parameter \""+parameter.Name.Name+"\" must be level 88.");
//		}
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
			if (used != null) { AddErrorAlreadyParameter(node, description.Name); continue; }
			used = GetParameter(profile.InputParameters,  description.Name);
			if (used != null) { AddErrorAlreadyParameter(node, description.Name); continue; }
			used = GetParameter(profile.OutputParameters, description.Name);
			if (used != null) { AddErrorAlreadyParameter(node, description.Name); continue; }
			used = GetParameter(profile.InoutParameters,  description.Name);
			if (used != null) { AddErrorAlreadyParameter(node, description.Name); continue; }
		}
	}
	private void AddEntries(List<DataDefinition> linkage, LinkageSection node) {
		foreach(var definition in node.Children())
			AddEntries(linkage, definition);
	}
	private void AddEntries(List<DataDefinition> linkage, DataDefinition node) {
		linkage.Add(node);
		foreach(var child in node.Children())
			AddEntries(linkage, child);
	}
	private ParameterDescription GetParameter(IList<ParameterDescription> parameters, string name) {
		if (name == null) return null;
		foreach(var p in parameters)
			if (Validate(p, name) != null) return p;
		return null;
	}
	private ParameterDescription Validate(ParameterDescription parameter, string name) {
		if (parameter != null && parameter.Name.Equals(name)) return parameter;
		return null;
	}
	private void AddErrorAlreadyParameter(LinkageSection node, string name) {
		DiagnosticUtils.AddError(GetParameter(node, name), name+" is already a parameter.");
	}
	/// <param name="linkage">LINKAGE SECTION, presumably</param>
	/// <param name="name">Parameter we want</param>
	/// <returns>Parameter as declared in DATA DIVISION</returns>
	private DataDefinitionEntry GetParameter(LinkageSection linkage, string name) {
		foreach(var data in linkage.Children()) {
			var found = GetParameter(data, name);
			if (found != null) return found;
		}
		return null;
	}
	private DataDefinitionEntry GetParameter(DataDefinition node,string name) {
		var data = (DataDefinitionEntry)node.CodeElement;
		if (data != null && name.Equals(data.Name)) return data;
		foreach(var subordinate in node.Children()) {
			var found = GetParameter(subordinate, name);
			if (found != null) return found;
		}
		return null;
	}
}



/// <summary>Checks the TypeCobol custom functions rule: TCRFUN_NO_SECTION_OR_PARAGRAPH_IN_LIBRARY.</summary>
class LibraryChecker: NodeListener {
	public IList<Type> GetNodes() {
		return new List<Type> { typeof(ProcedureDivision), };
	}
	public void OnNode(Node node, ParserRuleContext context, CodeModel.Program program) {
		var pdiv = node.CodeElement as ProcedureDivisionHeader;
		bool isPublicLibrary = false;
		var elementsInError = new List<CodeElement>();
		var errorMessages = new List<string>();
		foreach(var child in node.Children) {
			var ce = child.CodeElement;
			if (child.CodeElement == null) {
				elementsInError.Add(node.CodeElement);
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
		if (isPublicLibrary) {
			for(int c = 0; c < errorMessages.Count; c++)
				DiagnosticUtils.AddError(elementsInError[c], errorMessages[c], context);
		}
	}
}


}
