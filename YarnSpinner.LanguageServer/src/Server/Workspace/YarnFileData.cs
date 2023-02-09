﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Yarn.Compiler;
using YarnLanguageServer.Diagnostics;

// Disambiguate between
// OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic and
// Yarn.Compiler.Diagnostic
using Diagnostic = OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic;
using Position = OmniSharp.Extensions.LanguageServer.Protocol.Models.Position;

namespace YarnLanguageServer
{
    internal class YarnFileData
    {
        public YarnSpinnerLexer Lexer { get; protected set; }
        public YarnSpinnerParser Parser { get; protected set; }
        public YarnSpinnerParser.DialogueContext ParseTree { get; protected set; }
        public IList<IToken> Tokens { get; protected set; }
        public IList<IToken> CommentTokens { get; protected set; }
        public IEnumerable<DocumentSymbol> DocumentSymbols { get; protected set; }

        public IEnumerable<Diagnostic> CompilerDiagnostics { get; protected set; }
        public bool HasSemanticDiagnostics { get; protected set;  }
        public ImmutableArray<int> LineStarts { get; protected set; }
        // public CodeCompletionCore CodeCompletionCore { get; protected set; }

        public List<NodeInfo> NodeInfos { get; protected set; }

        public Uri Uri { get; set; }
        public Workspace Workspace { get; protected set; }

        public string Text { get; set; }

        public YarnFileData(string text, Uri uri, Workspace workspace)
        {
            Uri = uri;
            Workspace = workspace;
            Text = text;

            Update(text, workspace);

            // maybe we do the initial parsing, but don't do diagnostics / symbolic tokens until it's actually opened?
            // for now maybe let's just do it all in one go and get lazy if things are slow
        }

        public void Open(string text, Workspace workspace)
        {
            Update(text, workspace);
        }

        public void Update(string text, Workspace workspace)
        {
            LineStarts = TextCoordinateConverter.GetLineStarts(text);

            // Lex tokens and comments
            var commentLexer = new YarnSpinnerLexer(CharStreams.fromstring(text));
            var commentTokenStream = new CommonTokenStream(commentLexer);
            CommentTokens = new List<IToken>();
            commentTokenStream.Fill();
            CommentTokens = commentTokenStream.GetTokens()
                .Where(token =>
                    token.Channel == 2 &&
                    token.Type != YarnSpinnerLexer.Eof)
                .ToList();
            Tokens = commentTokenStream.GetTokens()
                .Where(token =>
                    token.Type != YarnSpinnerLexer.Eof)
                .ToList();

            // Now onto the real parsing
            Lexer = new YarnSpinnerLexer(CharStreams.fromstring(text));
            var tokenStream = new CommonTokenStream(Lexer);
            Parser = new YarnSpinnerParser(tokenStream);

            // Turn off compiler error listeners, and replace with our friendly / error tolerant ones
            Lexer.RemoveErrorListeners();

            // Attempt actual parse
            ParseTree = Parser.dialogue(); // Dialogue is the root node of the syntax tree

            // should probably just set these directly inside the visit function, or refactor all these into a references object
            NodeInfos = ReferencesVisitor.Visit(this, tokenStream).ToList();

            DocumentSymbols = DocumentSymbolsVisitor.Visit(this);

            PublishNodeInfos();
        }

        /// <summary>
        /// Sends the DidChangeNodesNotification message to the client, which
        /// contains semantic information about the nodes in this file.
        /// </summary>
        /// <seealso cref="Commands.DidChangeNodesNotification"/>
        private void PublishNodeInfos()
        {
            Workspace.LanguageServer.SendNotification(Commands.DidChangeNodesNotification, new NodesChangedParams
            {
                Uri = this.Uri,
                Nodes = this.NodeInfos,
            });
        }

        internal void ApplyContentChange(TextDocumentContentChangeEvent contentChange)
        {
            if (contentChange.Range == null) {
                this.Text = contentChange.Text;
                return;
            } else {
                var range = contentChange.Range;

                var startIndex = LineStarts[range.Start.Line] + range.Start.Character;
                var endIndex = LineStarts[range.End.Line] + range.End.Character;

                var stringBuilder = new System.Text.StringBuilder();

                stringBuilder.Append(this.Text, 0, startIndex)
                    .Append(contentChange.Text)
                    .Append(this.Text, endIndex, this.Text.Length - endIndex);

                this.Text = stringBuilder.ToString();
            }
        }

        
        public int? GetRawToken(Position position)
        {
            // TODO: Not sure if it's even worth using a visitor vs just iterating through the token list.
            var result = TokenPositionVisitor.Visit(this, position);
            if (result != null) { return result; }

            // The parse tree doesn't have whitespace tokens so need to manually search sometimes
            var match = this.Tokens.FirstOrDefault(t => PositionHelper.DoesPositionContainToken(position, t));
            result = match?.TokenIndex;
            return result;
        }

        /// <summary>
        /// Gets the collection of all references to commands in this file.
        /// </summary>
        public IEnumerable<YarnFunctionCall> CommandReferences => NodeInfos
            .SelectMany(n => n.CommandCalls);

        /// <summary>
        /// Gets the collection of all jumps to nodes in this file.
        /// </summary>
        public IEnumerable<NodeJump> NodeJumps => NodeInfos
            .SelectMany(n => n.Jumps);

        /// <summary>
        /// Gets the collection of all tokens in this file that represent the
        /// title in a node definition.
        /// </summary>
        public IEnumerable<IToken> NodeDefinitions => NodeInfos.Where(n => n.HasTitle).Select(n => n.TitleToken);

        /// <summary>
        /// Gets the collection of all function references in this file.
        /// </summary>
        public IEnumerable<YarnFunctionCall> FunctionReferences => NodeInfos
            .SelectMany(n => n.FunctionCalls);


        /// <summary>
        /// Gets the collection of all tokens in this file that represent
        /// variables.
        /// </summary>
        public IEnumerable<IToken> VariableReferences => NodeInfos
            .SelectMany(n => n.VariableReferences)
            .Select(variableToken => variableToken);

        /// <summary>
        /// Gets the number of lines in this file.
        /// </summary>
        public int LineCount => LineStarts.Length;

        /// <summary>
        /// Gets the length of the line at the specified index, up to but not
        /// including the line terminator.
        /// </summary>
        /// <param name="lineIndex">The zero-based index of the line to get the
        /// length of.</param>
        /// <returns>The length of the line.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref
        /// name="lineIndex"/> is less than zero, or equal to or greater than
        /// <see cref="LineCount"/>.</exception>
        public int GetLineLength(int lineIndex) {
            if (lineIndex < 0 || lineIndex >= LineCount) {
                throw new ArgumentOutOfRangeException(nameof(lineIndex), $"Must be between zero and {nameof(LineCount)}");
            }

            var start = LineStarts[lineIndex];
            var offset = 0;

            if (Text.Length == 0) {
                return 0;
            }

            var chars = Text.ToCharArray();

            while ((start + offset) < chars.Length && chars[start + offset] != '\r' && chars[start + offset] != '\n') {
                offset += 1;
            }

            return offset;
        }

        /// <summary>
        /// Given a position in the file, returns the type of symbol it
        /// represents (if any), and the token at that position (if any).
        /// </summary>
        /// <param name="position">The position to query for.</param>
        /// <returns>A tuple containing the type and token of the symbol at
        /// <paramref name="position"/>.</returns>
        public (YarnSymbolType yarnSymbolType, IToken token) GetTokenAndType(Position position)
        {
            Func<IToken, bool> isTokenMatch = (IToken t) => PositionHelper.DoesPositionContainToken(position, t);

            var allSymbolTokens = new IEnumerable<(IToken Token, YarnSymbolType Type)>[] {
                // Jumps
                NodeInfos.SelectMany(n => n.Jumps).Select(j => (j.DestinationToken, YarnSymbolType.Node)),

                // Commands
                NodeInfos.SelectMany(n => n.CommandCalls).Select(c => (c.NameToken, YarnSymbolType.Command)),

                // Variables
                NodeInfos.SelectMany(n => n.VariableReferences).Select(v => (v, YarnSymbolType.Variable)),

                // Functions
                NodeInfos.SelectMany(n => n.FunctionCalls).Select(f => (f.NameToken, YarnSymbolType.Function)),
            };


            foreach (var tokenInfo in allSymbolTokens.SelectMany(g => g)) {
                if (isTokenMatch(tokenInfo.Token)) {
                    return (tokenInfo.Type, tokenInfo.Token);
                }
            }

            // TODO Speed these searches up using binary search on the token positions
            // see getTokenFromList() in PositionHelper.cs

            return (YarnSymbolType.Unknown, null);
        }

        public (YarnFunctionCall?, int? activeParameterIndex) GetParameterInfo(Position position)
        {
            var info = GetFunctionInfo(position);
            if (info == null)
            {
                return (null, null);
            }

            if (!info.Value.ParameterRanges.Any()
                || position < info.Value.ParameterRanges.First().Start
                || position > info.Value.ParametersRange.End)
            {
                return (info, info.Value.ParametersRange.Contains(position) ? 0 : null);
            }

            int parameterIndex = 0;
            foreach (var parameter in info.Value.ParameterRanges)
            {
                if (parameter.Contains(position)) {
                    return (info, parameterIndex);
                }

                parameterIndex++;
            }

            return (info, null);
        }

        public void ClearDiagnostics(Workspace workspace)
        {
            workspace.LanguageServer.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams { Uri = this.Uri });
        }

        private YarnFunctionCall? GetFunctionInfo(Position position)
        {
            // Strategy is to look for rightmost start function parameter, and if there are none, check command parameters
            var functionMatches = NodeInfos.SelectMany(n => n.FunctionCalls).Where(fi => fi.ExpressionRange.Contains(position)).OrderByDescending(fi => fi.ExpressionRange.Start);
            if (functionMatches.Any())
            {
                return functionMatches.FirstOrDefault();
            }

            var commandMatches = NodeInfos.SelectMany(n => n.CommandCalls).Where(fi => fi.ExpressionRange.Contains(position));
            if (commandMatches.Any())
            {
                return commandMatches.FirstOrDefault();
            }

            return null;
        }
    }

    public enum YarnSymbolType
    {
        Node,
        Command,
        Variable,
        Function,
        Unknown,
    }
}