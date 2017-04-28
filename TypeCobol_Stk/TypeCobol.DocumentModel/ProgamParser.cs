
//----------------------------------------------------
// The following code was generated by C# CUP v0.1
// 27/04/2017 14:35:11
//----------------------------------------------------

namespace TUVienna.CS_CUP.simple_calc
 {

using System;
using TUVienna.CS_CUP.Runtime;

/** C# CUP v0.1 generated parser.
  * @version 27/04/2017 14:35:11
  */
public class ProgamParser : TUVienna.CS_CUP.Runtime.lr_parser {

  /** Default constructor. */
  public ProgamParser():base() {;}

  /** Constructor which sets the default scanner. */
  public ProgamParser(TUVienna.CS_CUP.Runtime.Scanner s): base(s) {;}

  /** Production table. */
  protected static readonly short[][] _production_table = 
    unpackFromStrings(new string[] {
    "/000/010/000/002/003/003/000/002/002/004/000/002/004" +
    "/004/000/002/005/004/000/002/007/002/000/002/007/003" +
    "/000/002/006/002/000/002/006/003" });

  /** Access to production table. */
  public override short[][] production_table() {return _production_table;}

  /** Parse-action table. */
  protected static readonly short[][] _action_table = 
    unpackFromStrings(new string[] {
    "/000/012/000/004/004/005/001/002/000/004/002/014/001" +
    "/002/000/010/002/ufffd/005/ufffd/173/012/001/002/000/006" +
    "/002/ufffb/005/011/001/002/000/004/002/001/001/002/000" +
    "/004/002/uffff/001/002/000/004/002/ufffa/001/002/000/006" +
    "/002/ufffc/005/ufffc/001/002/000/006/002/ufffe/005/ufffe/001" +
    "/002/000/004/002/000/001/002" });

  /** Access to parse-action table. */
  public override short[][] action_table() {return _action_table;}

  /** <code>reduce_goto</code> table. */
  protected static readonly short[][] _reduce_table = 
    unpackFromStrings(new string[] {
    "/000/012/000/010/003/003/004/006/005/005/001/001/000" +
    "/002/001/001/000/004/007/012/001/001/000/004/006/007" +
    "/001/001/000/002/001/001/000/002/001/001/000/002/001" +
    "/001/000/002/001/001/000/002/001/001/000/002/001/001" +
    "" });

  /** Access to <code>reduce_goto</code> table. */
  public override short[][] reduce_table() {return _reduce_table;}

  /** Instance of action encapsulation class. */
  protected CUP_ProgamParser_actions action_obj;

  /** Action encapsulation object initializer. */
  protected override void init_actions()
    {
      action_obj = new CUP_ProgamParser_actions(this);
    }

  /** Invoke a user supplied parse action. */
  public override TUVienna.CS_CUP.Runtime.Symbol do_action(
    int                        act_num,
    TUVienna.CS_CUP.Runtime.lr_parser parser,
    System.Collections.Stack            xstack1,
    int                        top)
  {
  mStack CUP_parser_stack= new mStack(xstack1);
    /* call code in generated class */
    return action_obj.CUP_ProgamParser_do_action(act_num, parser, stack, top);
  }

  /** Indicates start state. */
  public override int start_state() {return 0;}
  /** Indicates start production. */
  public override int start_production() {return 1;}

  /** <code>EOF</code> Symbol index. */
  public override int EOF_sym() {return 0;}

  /** <code>error</code> Symbol index. */
  public override int error_sym() {return 1;}

}

/** Cup generated class to encapsulate user supplied action code.*/
public class CUP_ProgamParser_actions {
  private ProgamParser my_parser;

  /** Constructor */
  public CUP_ProgamParser_actions(ProgamParser t_parser) {
    this.my_parser = t_parser;
  }

  /** Method with the actual generated action code. */
  public   TUVienna.CS_CUP.Runtime.Symbol CUP_ProgamParser_do_action(
    int                        CUP_ProgamParser_act_num,
    TUVienna.CS_CUP.Runtime.lr_parser CUP_ProgamParser_parser,
    System.Collections.Stack            xstack1,
    int                        CUP_ProgamParser_top)
    {
      /* Symbol object for return from actions */
      mStack CUP_ProgamParser_stack =new mStack(xstack1);
      TUVienna.CS_CUP.Runtime.Symbol CUP_ProgamParser_result;

      /* select the action based on the action number */
      switch (CUP_ProgamParser_act_num)
        {
          /*. . . . . . . . . . . . . . . . . . . .*/
          case 7: // ProgramEndOpt ::= ProgramEnd 
            {
              CodeElement RESULT = null;

              CUP_ProgamParser_result = new TUVienna.CS_CUP.Runtime.Symbol(4/*ProgramEndOpt*/, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).left, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, RESULT);
            }
          return CUP_ProgamParser_result;

          /*. . . . . . . . . . . . . . . . . . . .*/
          case 6: // ProgramEndOpt ::= 
            {
              CodeElement RESULT = null;

              CUP_ProgamParser_result = new TUVienna.CS_CUP.Runtime.Symbol(4/*ProgramEndOpt*/, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, RESULT);
            }
          return CUP_ProgamParser_result;

          /*. . . . . . . . . . . . . . . . . . . .*/
          case 5: // LibraryCopyOpt ::= LibraryCopy 
            {
              CodeElement RESULT = null;

              CUP_ProgamParser_result = new TUVienna.CS_CUP.Runtime.Symbol(5/*LibraryCopyOpt*/, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).left, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, RESULT);
            }
          return CUP_ProgamParser_result;

          /*. . . . . . . . . . . . . . . . . . . .*/
          case 4: // LibraryCopyOpt ::= 
            {
              CodeElement RESULT = null;

              CUP_ProgamParser_result = new TUVienna.CS_CUP.Runtime.Symbol(5/*LibraryCopyOpt*/, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, RESULT);
            }
          return CUP_ProgamParser_result;

          /*. . . . . . . . . . . . . . . . . . . .*/
          case 3: // programAttributes ::= ProgramIdentification LibraryCopyOpt 
            {
              CodeElement RESULT = null;

              CUP_ProgamParser_result = new TUVienna.CS_CUP.Runtime.Symbol(3/*programAttributes*/, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-1)).left, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, RESULT);
            }
          return CUP_ProgamParser_result;

          /*. . . . . . . . . . . . . . . . . . . .*/
          case 2: // cobolProgram ::= programAttributes ProgramEndOpt 
            {
              CodeElement RESULT = null;

              CUP_ProgamParser_result = new TUVienna.CS_CUP.Runtime.Symbol(2/*cobolProgram*/, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-1)).left, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, RESULT);
            }
          return CUP_ProgamParser_result;

          /*. . . . . . . . . . . . . . . . . . . .*/
          case 1: // $START ::= cobolCompilationUnit EOF 
            {
              object RESULT = null;
		int start_valleft = ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-1)).left;
		int start_valright = ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-1)).right;
		CodeElement start_val = (CodeElement)((TUVienna.CS_CUP.Runtime.Symbol) CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-1)).value;
		RESULT = start_val;
              CUP_ProgamParser_result = new TUVienna.CS_CUP.Runtime.Symbol(0/*$START*/, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-1)).left, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, RESULT);
            }
          /* ACCEPT */
          CUP_ProgamParser_parser.done_parsing();
          return CUP_ProgamParser_result;

          /*. . . . . . . . . . . . . . . . . . . .*/
          case 0: // cobolCompilationUnit ::= cobolProgram 
            {
              CodeElement RESULT = null;

              CUP_ProgamParser_result = new TUVienna.CS_CUP.Runtime.Symbol(1/*cobolCompilationUnit*/, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).left, ((TUVienna.CS_CUP.Runtime.Symbol)CUP_ProgamParser_stack.elementAt(CUP_ProgamParser_top-0)).right, RESULT);
            }
          return CUP_ProgamParser_result;

          /* . . . . . .*/
          default:
            throw new System.Exception(
               "Invalid action number found in internal parse table");

        }
    }
}

}
