//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/desplesda/Work/yarnspinner/YarnSpinner.Compiler/YarnSpinnerParser.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Yarn.Compiler {
using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="YarnSpinnerParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
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
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.file_hashtag"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFile_hashtag([NotNull] YarnSpinnerParser.File_hashtagContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.file_hashtag"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFile_hashtag([NotNull] YarnSpinnerParser.File_hashtagContext context);
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
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.line_formatted_text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLine_formatted_text([NotNull] YarnSpinnerParser.Line_formatted_textContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.line_formatted_text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLine_formatted_text([NotNull] YarnSpinnerParser.Line_formatted_textContext context);
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
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.line_condition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLine_condition([NotNull] YarnSpinnerParser.Line_conditionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.line_condition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLine_condition([NotNull] YarnSpinnerParser.Line_conditionContext context);
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
	/// Enter a parse tree produced by the <c>expTypeConversion</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpTypeConversion([NotNull] YarnSpinnerParser.ExpTypeConversionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>expTypeConversion</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpTypeConversion([NotNull] YarnSpinnerParser.ExpTypeConversionContext context);
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
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.variable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariable([NotNull] YarnSpinnerParser.VariableContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.variable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariable([NotNull] YarnSpinnerParser.VariableContext context);
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
	/// Enter a parse tree produced by the <c>setVariableToValue</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.set_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSetVariableToValue([NotNull] YarnSpinnerParser.SetVariableToValueContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>setVariableToValue</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.set_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSetVariableToValue([NotNull] YarnSpinnerParser.SetVariableToValueContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>setExpression</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.set_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSetExpression([NotNull] YarnSpinnerParser.SetExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>setExpression</c>
	/// labeled alternative in <see cref="YarnSpinnerParser.set_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSetExpression([NotNull] YarnSpinnerParser.SetExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.call_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCall_statement([NotNull] YarnSpinnerParser.Call_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.call_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCall_statement([NotNull] YarnSpinnerParser.Call_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.command_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCommand_statement([NotNull] YarnSpinnerParser.Command_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.command_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCommand_statement([NotNull] YarnSpinnerParser.Command_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.command_formatted_text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCommand_formatted_text([NotNull] YarnSpinnerParser.Command_formatted_textContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.command_formatted_text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCommand_formatted_text([NotNull] YarnSpinnerParser.Command_formatted_textContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.shortcut_option_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterShortcut_option_statement([NotNull] YarnSpinnerParser.Shortcut_option_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.shortcut_option_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitShortcut_option_statement([NotNull] YarnSpinnerParser.Shortcut_option_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.shortcut_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterShortcut_option([NotNull] YarnSpinnerParser.Shortcut_optionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.shortcut_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitShortcut_option([NotNull] YarnSpinnerParser.Shortcut_optionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.declare_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclare_statement([NotNull] YarnSpinnerParser.Declare_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.declare_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclare_statement([NotNull] YarnSpinnerParser.Declare_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.jump_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterJump_statement([NotNull] YarnSpinnerParser.Jump_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.jump_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitJump_statement([NotNull] YarnSpinnerParser.Jump_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="YarnSpinnerParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterType([NotNull] YarnSpinnerParser.TypeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="YarnSpinnerParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitType([NotNull] YarnSpinnerParser.TypeContext context);
}
} // namespace Yarn.Compiler
