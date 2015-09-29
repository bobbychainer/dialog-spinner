using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

// TODO: Comments (// and /*  */)
// TODO: Convert tabs to spaces

namespace Yarn {

	public class TokeniserException : InvalidOperationException  {
		public TokeniserException (string message) : base (message) {}
	}

	// save some typing, we deal with lists of tokens a LOT
	public class TokenList : List<Token> {}

	public enum TokenType {
		// Special tokens
		Whitespace,
		Indent,
		Dedent,
		EndOfLine,
		EndOfInput,

		// Numbers. Everybody loves a number
		Number,

		// Command syntax ("<<foo>>")
		BeginCommand,
		EndCommand,

		// Variables ("$foo")
		Variable,

		// Shortcut syntax ("->")
		ShortcutOption,

		// Option syntax ("[[Let's go here|Destination]]")
		OptionStart, // [[
		OptionDelimit, // |
		OptionEnd, // ]]

		// Command types (specially recognised command word)
		If,
		ElseIf,
		Else,
		EndIf,
		Set,

		// Parentheses
		LeftParen,
		RightParen,

		// Operators
		EqualTo, // ==, eq, is
		GreaterThan, // >, gt
		GreaterThanOrEqualTo, // >=, gte
		LessThan, // <, lt
		LessThanOrEqualTo, // <=, lte
		NotEqualTo, // !=, neq

		// Logical operators
		Or, // ||, or
		And, // &&, and
		Xor, // ^, xor
		Not, // !, not


		// this guy's special because '=' means 'equal to' or 'becomes' depending on context
		EqualToOrAssign, // =, to

		Add, // +
		Minus, // -
		Multiply, // *
		Divide, // /
		
		AddAssign, // +=
		MinusAssign, // -=
		MultiplyAssign, // *=
		DivideAssign, // /=

		Comment, // a run of text that we ignore

		Identifier, // a single word (used for functions)

		Text // a run of text until we hit other syntax
	}
	
	// A parsed token.
	public class Token {
		
		public TokenType type;
		public object value; // optional

		// Where we found this token
		public int lineNumber;
		public int columnNumber;
		
		public Token(TokenType type, object value=null) {
			this.type = type;
			this.value = value;
		}
		
		public override string ToString() {
			if (this.value != null) {
				return string.Format("{0} ({1})", type.ToString(), value.ToString());
			} else {
				return type.ToString();
			}
		}
	}


	// Lean, mean, string-readin' machine
	public class Tokeniser {

		// A defined rule for matching a token
		public class TokenRule {
			public TokenType type; // what token is this rule for?
			public Regex regex; // what should we look for?
			public bool discard; // should we throw away this token if we match it?

			// Some tokens are words (like "not", "and", "or") - if these
			// are the first word in what's otherwise a line of text,
			// the lexer will read it as "<AND> <TEXT>" instead of "<TEXT>".
			// So, we need to mark certain rules as "this token can start a line"

			public bool canBeginLine; 
			public bool resetsLine;

			public override string ToString ()
			{
				return string.Format ("TokenRule: {0}", type.ToString());
			}
		}
		
		// The list of all known token types
		List<TokenRule> tokenRules = new List<TokenRule>();

		public Tokeniser() {

			// Load the token rules for this language
			PrepareTokenRules();

			// Ensure that all token types have a rule
			// Obtain the string names of all the elements within myEnum 
			String[] names = Enum.GetNames( typeof( TokenType ) );
			
			// Obtain the values of all the elements within myEnum 
			TokenType[] values = (TokenType[])Enum.GetValues( typeof( TokenType ) );
			
			// Print the names and values to file
			for ( int i = 0; i < names.Length; i++ )
			{
				bool found = false;
				foreach (var tokenRule in tokenRules) {
					if (tokenRule.type == values[i]) {
						// Found a match, carry on
						found = true;
					}
				}
				// noooooo we forgot to add a rule in PrepareTokenRules()
				if (found == false)
					throw new TokeniserException("Missing rule for token type " + names[i]);
			}


		}
		
		// Define a new token rule, with a name and an optional rule
		public TokenRule AddTokenRule(TokenType type, string rule, bool canBeginLine = false, bool resetsLine = false) {
			
			var newTokenRule = new TokenRule();
			newTokenRule.type = type;

			// Set up a regex if we have a rule for it
			if (rule != null) {
				
				newTokenRule.regex = new Regex(rule);
			}
			newTokenRule.resetsLine = resetsLine;
			newTokenRule.canBeginLine = canBeginLine;
			
			// Store it in the list and return
			tokenRules.Add(newTokenRule);
			return newTokenRule;
		}

		// Given an input string, parse it and return the list of tokens
		public TokenList Tokenise(string input) {
			
			// The total collection of all tokens in this input
			var tokens = new TokenList();
			
			// Start by chopping up the input into lines
			var lines = input.Split(new char[] {'\n'} , StringSplitOptions.RemoveEmptyEntries);

			// Keep track of which column each new indent started
			var indentLevels = new Stack<int>();
			
			// Start at indent 0
			indentLevels.Push(0);

			var lineNum = 0;
			foreach (var line in lines) {
				
				int newIndentLevel;
				
				// Get the tokens, plus the indentation level of this line
				var lineTokens = TokeniseLine(line, out newIndentLevel, lineNum);
				
				if (newIndentLevel > indentLevels.Peek()) {
					// We are now more indented than the last indent.
					// Emit a "indent" token, and push this new indent onto the stack.
					var indent = new Token(TokenType.Indent);
					indent.lineNumber = lineNum;
					tokens.Add(indent);
					indentLevels.Push(newIndentLevel);
				} else if (newIndentLevel < indentLevels.Peek()) {
					// We are less indented than the last indent.
					// We may have indented more than a single indent, though, so
					// check this against all indents we know about
					
					while (newIndentLevel < indentLevels.Peek()) {
						// We've gone down an indent, holy crap, dedent it!
						var dedent = new Token(TokenType.Dedent);
						dedent.lineNumber = lineNum;
						tokens.Add(dedent);
						indentLevels.Pop();
					}
				}
				
				// Add the list of tokens that were in this line
				tokens.AddRange(lineTokens);

				// Update line number
				lineNum++;
				
			}


			// Dedent if there's any indentations left (ie we reached the 
			// end of the file and it was still indented)
			// (we stop at the second-last one because we pushed 'indent 0' at the start,
			// and popping that would emit an unbalanced dedent token
			while (indentLevels.Count > 1) {
				indentLevels.Pop();
				var dedent = new Token(TokenType.Dedent);
				dedent.lineNumber = lineNum;
				tokens.Add(dedent);
			}

			// Finish up with an ending token
			tokens.Add(new Token(TokenType.EndOfInput));

			// yay we're done
			return tokens;
		}

		// Tokenise a single line, and also report on how indented this line is
		private TokenList TokeniseLine(string input, out int lineIndentation, int lineNumber) {
			
			// The tokens we found on this line
			var tokens = new TokenList();

			// Replace tabs with four spaces
			input = input.Replace ("\t", "    ");
			
			// Find whitespace at the start of a line
			var initialIndentRule = new Regex("^\\s+");
			
			// If there's whitespace at the start of the line, it's indented
			if (initialIndentRule.IsMatch(input)) {
				// Record how indented this line is
				lineIndentation = initialIndentRule.Match(input).Length;
			} else {
				// There's no whitespace at the start of the line,
				// so this line's indentation level is zero.
				lineIndentation = 0;
			}
			
			// Keeps track of how much of the line we have left to parse

			int columnNumber = lineIndentation;
			// Consume the string

			bool startOfLine = true;
			while (columnNumber < input.Length) {
				
				// Keep track of whether we successfully found a rule to parse the next token
				var matched = false;
				
				foreach (var tokenRule in tokenRules) {
					
					// Is the next chunk of text a token?
					if (tokenRule.regex != null) {

						// Attempt to match this
						var match = tokenRule.regex.Match(input, columnNumber);

						// Bail out if this either failed to match, or matched
						// further out in the text
						if (match.Success == false || match.Index > columnNumber)
							continue;

						// Bail out if this is the first token and we aren't allowed to
						// match this rule at the start
						if (tokenRule.canBeginLine == false && startOfLine == true) {
							continue;
						}

						// Record the token only if we care
						// about it (ie it's not whitespace)
						if (tokenRule.discard == false) {
							Token token;
							
							// If this token type's rule had a capture group,
							// store that
							if (match.Captures.Count > 0) {
								token = new Token(tokenRule.type, match.Captures[0].Value);
							} else {
								token = new Token(tokenRule.type);
							}

							// Record where the token was found
							token.lineNumber = lineNumber;
							token.columnNumber = columnNumber;

							// Add it to the token stream
							tokens.Add(token);

							// If this is a token that 'resets' the fact
							// that we're at the start of the line (like '->' does),
							// then record that; otherwise, record that we're past
							// the start of the line and are now allowed to start
							// matching additional types of tokens
							if (tokenRule.resetsLine) {
								startOfLine = true;
							} else {
								startOfLine = false;
							}
							
						}

						// Update the column number
						columnNumber += match.Length;


						// Record that we successfully found a type for this token
						matched = true;

						// We've matched a token type, stop trying to
						// match it against others
						break;
					}
				}
				
				if (matched == false) {
					// We've exhausted the list of possible token types - failed to interpret the string!
					throw new InvalidOperationException("Failed to interpret token " + input);
				}
			}
			
			// Return the list of tokens we found
			return tokens;
		}

		// Prepare the rules for matching tokens.
		private void PrepareTokenRules() {

			// The order of these rules is important - rules that were added first
			// get matched first.

			// Set up the whitespace token, which is discarded
			AddTokenRule(TokenType.Whitespace, "\\s+", canBeginLine:true)
				.discard = true;
			
			// Set up the special begin and end indentation tokens - 
			// these aren't matched by regexes, but rather emitted
			// during lexing by keeping track of the number of spaces
			AddTokenRule(TokenType.Indent, null, canBeginLine:true);
			AddTokenRule(TokenType.Dedent, null, canBeginLine:true);
			
			// Set up the end-of-line token
			AddTokenRule (TokenType.EndOfLine, "\\n", canBeginLine:true)
				.discard = true;
			
			// Set up the end-of-file token
			AddTokenRule(TokenType.EndOfInput, null);

			// Comments
			AddTokenRule (TokenType.Comment, "\\/\\/.*", canBeginLine: true)
				.discard = true;

			// Basic syntax
			AddTokenRule(TokenType.Number, "((?<!\\[\\[)\\d+)");
			AddTokenRule(TokenType.BeginCommand, "\\<\\<", canBeginLine:true);
			AddTokenRule(TokenType.EndCommand, "\\>\\>");
			AddTokenRule(TokenType.Variable, "\\$[A-z\\d]+");
			
			// Options
			AddTokenRule(TokenType.ShortcutOption, "-\\>", canBeginLine:true, resetsLine:true);
			AddTokenRule(TokenType.OptionStart, "\\[\\[", canBeginLine:true, resetsLine:true);
			AddTokenRule(TokenType.OptionDelimit, "\\|");
			AddTokenRule(TokenType.OptionEnd, "\\]\\]");
			
			// Reserved words
			AddTokenRule(TokenType.If, "if");
			AddTokenRule(TokenType.ElseIf, "elseif");
			AddTokenRule(TokenType.Else, "else");
			AddTokenRule(TokenType.EndIf, "endif");
			
			AddTokenRule(TokenType.Set, "set");
			
			// Operators
			AddTokenRule(TokenType.EqualTo, "(==|eq|is)");
			AddTokenRule(TokenType.GreaterThanOrEqualTo, "(\\>=|gte)");
			AddTokenRule(TokenType.LessThanOrEqualTo, "(\\<=|lte)");
			AddTokenRule(TokenType.GreaterThan, "(\\>|gt)");
			AddTokenRule(TokenType.LessThan, "(\\<|lt)");
			AddTokenRule(TokenType.NotEqualTo, "(\\!=|neq)");

			AddTokenRule(TokenType.And, "(\\&\\&|and)");
			AddTokenRule(TokenType.Or, "(\\|\\||or)");
			AddTokenRule(TokenType.Xor, "(\\^|xor)");
			AddTokenRule(TokenType.Not, "(\\!|not)");

			// Assignment operators
			AddTokenRule (TokenType.EqualToOrAssign, "(=|to)");

			AddTokenRule(TokenType.AddAssign, "\\+=");
			AddTokenRule(TokenType.MinusAssign, "-=");
			AddTokenRule(TokenType.MultiplyAssign, "\\*=");
			AddTokenRule(TokenType.DivideAssign, "\\/=");

			AddTokenRule(TokenType.Add, "\\+");
			AddTokenRule(TokenType.Minus, "-");
			AddTokenRule(TokenType.Multiply, "\\*");
			AddTokenRule(TokenType.Divide, "\\/");

			AddTokenRule (TokenType.LeftParen, "\\(");
			AddTokenRule (TokenType.RightParen, "\\)");

			AddTokenRule (TokenType.Identifier, "(\\w|_)+");


			// Free text - match anything except command or option syntax
			// This always goes last so that anything else will preferably
			// match it
			AddTokenRule(TokenType.Text, "[^\\<\\>\\[\\]\\|]*", canBeginLine:true);
		}
	}


	
}

