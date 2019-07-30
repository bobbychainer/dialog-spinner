//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/desplesda/Work/yarnspinner/YarnSpinner/YarnSpinnerParser.g4 by ANTLR 4.7.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="YarnSpinnerParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.1")]
[System.CLSCompliant(false)]
public interface IYarnSpinnerParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.dialogue"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDialogue([NotNull] YarnSpinnerParser.DialogueContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.dialogue"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDialogue([NotNull] YarnSpinnerParser.DialogueContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.node"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNode([NotNull] YarnSpinnerParser.NodeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.node"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNode([NotNull] YarnSpinnerParser.NodeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.header"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterHeader([NotNull] YarnSpinnerParser.HeaderContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.header"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitHeader([NotNull] YarnSpinnerParser.HeaderContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.header_title"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterHeader_title([NotNull] YarnSpinnerParser.Header_titleContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.header_title"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitHeader_title([NotNull] YarnSpinnerParser.Header_titleContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.header_tag"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterHeader_tag([NotNull] YarnSpinnerParser.Header_tagContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.header_tag"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitHeader_tag([NotNull] YarnSpinnerParser.Header_tagContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.header_line"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterHeader_line([NotNull] YarnSpinnerParser.Header_lineContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.header_line"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitHeader_line([NotNull] YarnSpinnerParser.Header_lineContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBody([NotNull] YarnSpinnerParser.BodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBody([NotNull] YarnSpinnerParser.BodyContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStatement([NotNull] YarnSpinnerParser.StatementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStatement([NotNull] YarnSpinnerParser.StatementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.shortcut_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterShortcut_statement([NotNull] YarnSpinnerParser.Shortcut_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.shortcut_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitShortcut_statement([NotNull] YarnSpinnerParser.Shortcut_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.shortcut"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterShortcut([NotNull] YarnSpinnerParser.ShortcutContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.shortcut"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitShortcut([NotNull] YarnSpinnerParser.ShortcutContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.shortcut_conditional"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterShortcut_conditional([NotNull] YarnSpinnerParser.Shortcut_conditionalContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.shortcut_conditional"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitShortcut_conditional([NotNull] YarnSpinnerParser.Shortcut_conditionalContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.shortcut_text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterShortcut_text([NotNull] YarnSpinnerParser.Shortcut_textContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.shortcut_text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitShortcut_text([NotNull] YarnSpinnerParser.Shortcut_textContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.if_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIf_statement([NotNull] YarnSpinnerParser.If_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.if_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIf_statement([NotNull] YarnSpinnerParser.If_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.if_clause"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIf_clause([NotNull] YarnSpinnerParser.If_clauseContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.if_clause"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIf_clause([NotNull] YarnSpinnerParser.If_clauseContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.else_if_clause"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterElse_if_clause([NotNull] YarnSpinnerParser.Else_if_clauseContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.else_if_clause"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitElse_if_clause([NotNull] YarnSpinnerParser.Else_if_clauseContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.else_clause"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterElse_clause([NotNull] YarnSpinnerParser.Else_clauseContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.else_clause"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitElse_clause([NotNull] YarnSpinnerParser.Else_clauseContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.set_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSet_statement([NotNull] YarnSpinnerParser.Set_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.set_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSet_statement([NotNull] YarnSpinnerParser.Set_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.option_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterOption_statement([NotNull] YarnSpinnerParser.Option_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.option_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitOption_statement([NotNull] YarnSpinnerParser.Option_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.function"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunction([NotNull] YarnSpinnerParser.FunctionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.function"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunction([NotNull] YarnSpinnerParser.FunctionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunction_statement([NotNull] YarnSpinnerParser.Function_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunction_statement([NotNull] YarnSpinnerParser.Function_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.action_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAction_statement([NotNull] YarnSpinnerParser.Action_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.action_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAction_statement([NotNull] YarnSpinnerParser.Action_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterText([NotNull] YarnSpinnerParser.TextContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitText([NotNull] YarnSpinnerParser.TextContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.line_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLine_statement([NotNull] YarnSpinnerParser.Line_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.line_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLine_statement([NotNull] YarnSpinnerParser.Line_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.hashtag_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterHashtag_block([NotNull] YarnSpinnerParser.Hashtag_blockContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.hashtag_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitHashtag_block([NotNull] YarnSpinnerParser.Hashtag_blockContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.hashtag"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterHashtag([NotNull] YarnSpinnerParser.HashtagContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.hashtag"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitHashtag([NotNull] YarnSpinnerParser.HashtagContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expParens</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpParens([NotNull] YarnSpinnerParser.ExpParensContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expParens</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpParens([NotNull] YarnSpinnerParser.ExpParensContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expMultDivMod</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpMultDivMod([NotNull] YarnSpinnerParser.ExpMultDivModContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expMultDivMod</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpMultDivMod([NotNull] YarnSpinnerParser.ExpMultDivModContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expMultDivModEquals</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpMultDivModEquals([NotNull] YarnSpinnerParser.ExpMultDivModEqualsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expMultDivModEquals</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpMultDivModEquals([NotNull] YarnSpinnerParser.ExpMultDivModEqualsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expComparison</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpComparison([NotNull] YarnSpinnerParser.ExpComparisonContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expComparison</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpComparison([NotNull] YarnSpinnerParser.ExpComparisonContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expNegative</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpNegative([NotNull] YarnSpinnerParser.ExpNegativeContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expNegative</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpNegative([NotNull] YarnSpinnerParser.ExpNegativeContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expAndOrXor</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpAndOrXor([NotNull] YarnSpinnerParser.ExpAndOrXorContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expAndOrXor</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpAndOrXor([NotNull] YarnSpinnerParser.ExpAndOrXorContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expPlusMinusEquals</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpPlusMinusEquals([NotNull] YarnSpinnerParser.ExpPlusMinusEqualsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expPlusMinusEquals</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpPlusMinusEquals([NotNull] YarnSpinnerParser.ExpPlusMinusEqualsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expAddSub</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpAddSub([NotNull] YarnSpinnerParser.ExpAddSubContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expAddSub</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpAddSub([NotNull] YarnSpinnerParser.ExpAddSubContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expNot</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpNot([NotNull] YarnSpinnerParser.ExpNotContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expNot</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpNot([NotNull] YarnSpinnerParser.ExpNotContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expValue</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpValue([NotNull] YarnSpinnerParser.ExpValueContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expValue</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpValue([NotNull] YarnSpinnerParser.ExpValueContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>expEquality</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpEquality([NotNull] YarnSpinnerParser.ExpEqualityContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expEquality</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpEquality([NotNull] YarnSpinnerParser.ExpEqualityContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>valueNumber</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValueNumber([NotNull] YarnSpinnerParser.ValueNumberContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>valueNumber</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValueNumber([NotNull] YarnSpinnerParser.ValueNumberContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>valueTrue</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValueTrue([NotNull] YarnSpinnerParser.ValueTrueContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>valueTrue</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValueTrue([NotNull] YarnSpinnerParser.ValueTrueContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>valueFalse</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValueFalse([NotNull] YarnSpinnerParser.ValueFalseContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>valueFalse</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValueFalse([NotNull] YarnSpinnerParser.ValueFalseContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>valueVar</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValueVar([NotNull] YarnSpinnerParser.ValueVarContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>valueVar</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValueVar([NotNull] YarnSpinnerParser.ValueVarContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>valueString</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValueString([NotNull] YarnSpinnerParser.ValueStringContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>valueString</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValueString([NotNull] YarnSpinnerParser.ValueStringContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>valueFunc</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValueFunc([NotNull] YarnSpinnerParser.ValueFuncContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>valueFunc</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValueFunc([NotNull] YarnSpinnerParser.ValueFuncContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>valueNull</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValueNull([NotNull] YarnSpinnerParser.ValueNullContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>valueNull</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValueNull([NotNull] YarnSpinnerParser.ValueNullContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.variable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariable([NotNull] YarnSpinnerParser.VariableContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.variable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariable([NotNull] YarnSpinnerParser.VariableContext context);
}
