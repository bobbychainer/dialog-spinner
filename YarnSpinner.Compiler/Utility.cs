// Copyright Yarn Spinner Pty Ltd
// Licensed under the MIT License. See LICENSE.md in project root for license information.

namespace Yarn.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// Utility methods for working with line tags.
    /// </summary>
    public static class Utility
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Generates a Yarn script that contains a node that declares
        /// variables.
        /// </summary>
        /// <remarks>This method is intended to be called by tools that let the
        /// user manage variable declarations. Such tools can read the existing
        /// variable declarations in from a script (by compiling the script with
        /// the <see cref="CompilationJob.CompilationType"/> value set to  <see
        /// cref="CompilationJob.Type.TypeCheck"/>), allow the user to
        /// make changes, and then write the changes to disk by calling this
        /// method and saving the results.</remarks>
        /// <param name="declarations">The collection of <see
        /// cref="Declaration"/> objects to include in the output.</param>
        /// <param name="title">The title of the node that should be
        /// generated.</param>
        /// <param name="tags">The collection of tags that should be generated
        /// for the node. If this is <see langword="null"/>, no tags will be
        /// generated.</param>
        /// <param name="headers">The collection of additional headers that
        /// should be generated for the node. If this is <see langword="null"/>,
        /// no additional headers will be generated.</param>
        /// <returns>A string containing a Yarn script that declares the
        /// specified variables.</returns>
        /// <throws cref="ArgumentOutOfRangeException">Thrown when any of the
        /// <see cref="Declaration"/> objects in <paramref name="declarations"/>
        /// is not a variable declaration, or if the <see
        /// cref="Declaration.Type"/> of any of the declarations is an
        /// invalid value.</throws>
        public static string GenerateYarnFileWithDeclarations(
            IEnumerable<Yarn.Compiler.Declaration> declarations,
            string title = "Program",
            IEnumerable<string>? tags = null,
            IDictionary<string, string>? headers = null)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"title: {title}");

            if (tags != null)
            {
                stringBuilder.AppendLine($"tags: {string.Join(" ", tags)}");
            }

            if (headers != null)
            {
                foreach (var kvp in headers)
                {
                    stringBuilder.AppendLine($"{kvp.Key}: {kvp.Value}");
                }
            }

            stringBuilder.AppendLine("---");

            int count = 0;

            foreach (var decl in declarations)
            {
                if (decl.Type is FunctionType)
                {
                    // Ignore function types; they can't be declared in Yarn
                    // script
                    continue;
                }

                if (string.IsNullOrEmpty(decl.Description) == false)
                {
                    if (count > 0)
                    {
                        // Insert a blank line above this comment, for readibility
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.AppendLine($"/// {decl.Description}");
                }

                stringBuilder.Append($"<<declare {decl.Name} = ");

                if (decl.Type == Types.Number)
                {
                    stringBuilder.Append(decl.DefaultValue);
                }
                else if (decl.Type == Types.String)
                {
                    stringBuilder.Append('"' + (string)(decl.DefaultValue ?? string.Empty) + '"');
                }
                else if (decl.Type == Types.Boolean)
                {
                    stringBuilder.Append((bool)(decl.DefaultValue ?? false) ? "true" : "false");
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Declaration {decl.Name}'s type must not be {decl.Type.Name}.");
                }

                stringBuilder.AppendLine(">>");

                count += 1;
            }

            stringBuilder.AppendLine("===");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Given Yarn source code, adds line tags to the ends of all lines
        /// that need one and do not already have one.
        /// </summary>
        /// <remarks><para>
        /// This method ensures that it does not generate line
        /// tags that are already present in the file, or present in the
        /// <paramref name="existingLineTags"/> collection.
        /// </para>
        /// <para>
        /// Line tags are added to any line of source code that contains
        /// user-visible text: lines, options, and shortcut options.
        /// </para>
        /// </remarks>
        /// <param name="contents">The source code to add line tags
        /// to.</param>
        /// <param name="existingLineTags">The collection of line tags
        /// already exist elsewhere in the source code; the newly added
        /// line tags will not be duplicates of any in this
        /// collection.</param>
        /// <returns>The modified source code, with line tags
        /// added.</returns>
        [Obsolete("This method doesn't return the new tags, just the modified text which can cause issues with multiple files. Please use TagLines instead")]
        public static string AddTagsToLines(string contents, ICollection<string> existingLineTags = null)
        {
            // First, get the parse tree for this source code.
            var (parseSource, diagnostics) = ParseSource(contents);

            // Were there any error-level diagnostics?
            if (diagnostics.Any(d => d.Severity == Diagnostic.DiagnosticSeverity.Error))
            {
                // We encountered a parse error. Bail here; we aren't confident
                // in our ability to correctly insert a line tag.
                return contents;
            }

            // Make sure we have a list of line tags to work with.
            if (existingLineTags == null)
            {
                existingLineTags = new List<string>();
            }

            // Create the line listener, which will produce TextReplacements for
            // each new line tag.
            var untaggedLineListener = new UntaggedLineListener(new List<string>(existingLineTags), parseSource.Tokens);

            // Walk the tree with this listener, and generate text replacements
            // containing line tags.
            var walker = new Antlr4.Runtime.Tree.ParseTreeWalker();
            walker.Walk(untaggedLineListener, parseSource.Tree);

            // Apply these text replacements to the original source and return
            // it.
            return untaggedLineListener.RewrittenNodes().Item1;
        }

        /// <summary>
        /// Given Yarn source code, adds line tags to the ends of all lines
        /// that need one and do not already have one.
        /// </summary>
        /// <remarks><para>
        /// This method ensures that it does not generate line
        /// tags that are already present in the file, or present in the
        /// <paramref name="existingLineTags"/> collection.
        /// </para>
        /// <para>
        /// Line tags are added to any line of source code that contains
        /// user-visible text: lines, options, and shortcut options.
        /// </para>
        /// </remarks>
        /// <param name="contents">The source code to add line tags
        /// to.</param>
        /// <param name="existingLineTags">The collection of line tags
        /// already exist elsewhere in the source code; the newly added
        /// line tags will not be duplicates of any in this
        /// collection.</param>
        /// <returns>Tuple of the modified source code, with line tags
        /// added and the list of new line tags generated.
        /// </returns>
        public static (string, IList<string>) TagLines(string contents, ICollection<string> existingLineTags = null)
        {
            // First, get the parse tree for this source code.
            var (parseSource, diagnostics) = ParseSource(contents);

            // Were there any error-level diagnostics?
            if (diagnostics.Any(d => d.Severity == Diagnostic.DiagnosticSeverity.Error))
            {
                // We encountered a parse error. Bail here; we aren't confident
                // in our ability to correctly insert a line tag.
                return (null, null);
            }

            // Make sure we have a list of line tags to work with.
            if (existingLineTags == null)
            {
                existingLineTags = new List<string>();
            }

            // Create the line listener, which will produce TextReplacements for
            // each new line tag.
            var untaggedLineListener = new UntaggedLineListener(new List<string>(existingLineTags), parseSource.Tokens);

            // Walk the tree with this listener, and generate text replacements
            // containing line tags.
            var walker = new Antlr4.Runtime.Tree.ParseTreeWalker();
            walker.Walk(untaggedLineListener, parseSource.Tree);

            // Apply these text replacements to the original source and return
            // it.
            return untaggedLineListener.RewrittenNodes();
        }

        /// <summary>
        /// Parses a string of Yarn source code, and produces a FileParseResult
        /// and (if there were any problems) a collection of diagnostics.
        /// </summary>
        /// <param name="source">The source code to parse.</param>
        /// <returns>A tuple containing a <see cref="FileParseResult"/> that
        /// stores the parse tree and tokens, and a collection of <see
        /// cref="Diagnostic"/> objects that describe problems in the source
        /// code.</returns>
        public static (FileParseResult, IEnumerable<Diagnostic>) ParseSource(string source)
        {
            var diagnostics = new List<Diagnostic>();
            var result = Compiler.ParseSyntaxTree("<input>", source, ref diagnostics);

            return (result, diagnostics);
        }

        /// <summary>
        /// Generates a new unique line tag that is not present in
        /// <c>existingKeys</c>.
        /// </summary>
        /// <param name="existingKeys">The collection of keys that should be
        /// considered when generating a new, unique line tag.</param>
        /// <returns>A unique line tag that is not already present in <paramref
        /// name="existingKeys"/>.</returns>
        private static string GenerateString(ICollection<string> existingKeys)
        {
            string tag;
            do
            {
                tag = string.Format(CultureInfo.InvariantCulture, "line:{0:x7}", Random.Next(0x1000000));
            }
            while (existingKeys.Contains(tag));

            return tag;
        }

        /// <summary>
        /// An <see cref="IYarnSpinnerParserListener"/> that produces line tags.
        /// </summary>
        private class UntaggedLineListener : YarnSpinnerParserBaseListener
        {
            private readonly IList<string> existingStrings;

            private readonly CommonTokenStream TokenStream;

            private TokenStreamRewriter rewriter;

            /// <summary>
            /// Initializes a new instance of the <see
            /// cref="UntaggedLineListener"/> class.
            /// </summary>
            /// <param name="existingStrings">A collection of line IDs that
            /// should not be used. This list will be added to as this instance
            /// works.</param>
            /// <param name="tokenStream">The token stream used to generate the
            /// <see cref="IParseTree"/> this instance is operating on.</param>
            public UntaggedLineListener(IList<string> existingStrings, CommonTokenStream tokenStream)
            {
                this.existingStrings = existingStrings;
                this.TokenStream = tokenStream;
                this.rewriter = new TokenStreamRewriter(TokenStream);
            }

            /// <inheritdoc/>
            public override void ExitLine_statement([NotNull] YarnSpinnerParser.Line_statementContext context)
            {
                // We're looking at a complete line statement.

                // First, figure out if this line statement already has a line
                // tag. Start by taking the hashtags...
                var hashtags = context.hashtag();

                // Get the text for all of these hashtags...
                var texts = StringTableGeneratorVisitor.GetHashtagTexts(hashtags);

                // And then look for a line ID hashtag.
                foreach (var text in texts)
                {
                    if (text.StartsWith("line:"))
                    {
                        // This line contains a line code. Nothing left to do.
                        return;
                    }
                }
                
                // Find the index of the first token on the default channel to
                // the left of the newline.
                var previousTokenIndex = IndexOfPreviousTokenOnChannel(
                    TokenStream,
                    context.NEWLINE().Symbol.TokenIndex,
                    YarnSpinnerLexer.DefaultTokenChannel
                );

                // Did we find one?
                if (previousTokenIndex == -1)
                {
                    // No token was found before this newline. This is an
                    // internal error - there must be at least one symbol
                    // besides the terminating newline.
                    throw new InvalidOperationException($"Internal error: failed to find any tokens before the newline in line statement on line {context.Start.Line}");
                }

                // Get the token at this index. We'll put our tag after it.
                var previousToken = TokenStream.Get(previousTokenIndex);

                // Generate a new, unique line ID.
                string newLineID = Utility.GenerateString(existingStrings);

                // Record that we've used this new line ID, so that we don't
                // accidentally use it twice.
                existingStrings.Add(newLineID);

                this.rewriter.InsertAfter(previousToken, $" #{newLineID} ");
            }

            /// <summary>
            /// Gets the index of the first token to the left of the token at
            /// <paramref name="index"/> that's on <paramref name="channel"/>.
            /// If there are no tokens that match, return -1.
            /// </summary>
            /// <param name="tokenStream">The token stream to search
            /// within.</param>
            /// <param name="index">The index of the token to start searching
            /// from.</param>
            /// <param name="channel">The channel to find tokens on.</param>
            /// <returns>The index of the first token before the token at
            /// <paramref name="index"/> that is on the channel <paramref
            /// name="channel"/>. If none is found, returns -1. If <paramref
            /// name="index"/> is beyond the size of <paramref
            /// name="tokenStream"/>, returns the index of the last token in the
            /// stream.</returns>
            private static int IndexOfPreviousTokenOnChannel(CommonTokenStream tokenStream, int index, int channel)
            {
                // Are we beyond the list of tokens?
                if (index >= tokenStream.Size)
                {
                    // Return the final token in the channel, which will be an
                    // EOF.
                    return tokenStream.Size - 1;
                }

                // 'index' is the token we want to start searching from. We want
                // to find items before it, so start looking from the token
                // before it.
                var currentIndex = index -= 1;

                // Walk backwards through the tokens list.
                while (currentIndex >= 0)
                {
                    IToken token = tokenStream.Get(currentIndex);

                    // Is this token on the channel we're looking for?
                    if (token.Channel == channel)
                    {
                        // We're done - we found one! Return it.
                        return currentIndex;
                    }
                    currentIndex -= 1;
                }

                // We found nothing. Return the 'not found' value.
                return -1;
            }

            public (string, IList<string>) RewrittenNodes()
            {
                return (this.rewriter.GetText(), existingStrings);
            }
        }

        /// <summary>
        /// Gets the collection of contiguous runs of lines in the provided
        /// nodes. Each run of lines is guaranteed to run to completion once
        /// entered.
        /// </summary>
        /// <param name="nodes">The nodes to get string blocks for.</param>
        /// <returns>A collection of runs of lines.</returns>
        public static List<List<string>> ExtractStringBlocks(IEnumerable<Node> nodes, ProjectDebugInfo projectDebugInfo)
        {
            List<List<string>> lineBlocks = new List<List<string>>();

            foreach (var node in nodes)
            {
                var nodeDebugInfo = projectDebugInfo.Nodes.Single(n => n.NodeName == node.Name);
                var blocks = InstructionCollectionExtensions.GetBasicBlocks(node, nodeDebugInfo);
                var visited = new HashSet<string>();
                foreach (var block in blocks)
                {
                    RunBlock(block, blocks, visited);
                }
            }

            void RunBlock(BasicBlock block, IEnumerable<BasicBlock> blocks, HashSet<string> visited, string? openingLineID = null)
            {
                if (block.PlayerVisibleContent.Count() == 0)
                {
                    // skipping this block because it has no user content within
                    return;
                }

                if (visited.Contains(block.Name))
                {
                    // we have already visited this one so we can go on without it
                    return;
                }
                visited.Add(block.Name);

                var runOfLines = new List<string>();

                // if we are given an opening line ID we need to add that in at the top
                // this handles the case where we want options to open the set associated lines
                if (openingLineID != null && !string.IsNullOrEmpty(openingLineID))
                {
                    runOfLines.Add(openingLineID);
                }

                foreach (var content in block.PlayerVisibleContent)
                {
                    // I really really dislike using objects in this manner
                    // it just feels oh so very strange to me
                    if (content is BasicBlock.LineElement line)
                    {
                        // lines just get added to the current collection of content
                        runOfLines.Add(line.LineID);
                    }
                    else if (content is BasicBlock.OptionsElement options)
                    {
                        // options are special cased because of how they work
                        // an option will always be put into a block by themselves and any child content they have
                        // so this means we close off the current run of content and add it to the overall container
                        // and then make a new one for each option in the option set
                        if (runOfLines.Count() > 0)
                        {
                            lineBlocks.Add(runOfLines);
                            runOfLines = new List<string>();
                        }

                        var jumpOptions = new Dictionary<string, BasicBlock>();
                        foreach (var option in options.Options)
                        {
                            var destination = blocks.First(b => b.FirstInstructionIndex == option.Destination);
                            if (destination != null && destination.PlayerVisibleContent.Count() > 0)
                            {
                                // there is a valid jump we need to deal with
                                // we store this and will handle it later
                                jumpOptions[option.LineID] = destination;
                            }
                            else
                            {
                                // there is no jump for this option
                                // we just add it to the collection and continue
                                runOfLines.Add(option.LineID);
                                lineBlocks.Add(runOfLines);
                                runOfLines = new List<string>();
                            }
                        }

                        // now any options without a child block have been handled we need to handle those with children
                        // in that case we want to run through each of those as if they are a new block but with the option at the top
                        foreach (var pair in jumpOptions)
                        {
                            RunBlock(pair.Value, blocks, visited, pair.Key);
                        }
                    }
                    else if (content is BasicBlock.CommandElement)
                    {
                        // skipping commands as they aren't lines
                        continue;
                    }
                    else
                    {
                        // encountered an unknown type, this is an error
                        // but for now we will skip over it
                        continue;
                    }
                }

                if (runOfLines.Count() > 0)
                {
                    lineBlocks.Add(runOfLines);
                }
            }

            return lineBlocks;
        }

        /// <summary>
        /// Finds and collates every jump in every node.
        /// </summary>
        /// <param name="YarnFileContents">The collection of yarn file content to parse and walk</param>
        /// <returns>A list of lists of GraphingNode each containing a node, its jumps, and any positional info.</returns>
        public static List<List<GraphingNode>> DetermineNodeConnections(string[] YarnFileContents)
        {
            var walker = new Antlr4.Runtime.Tree.ParseTreeWalker();

            // alright so the change is instead of making it a list
            // we make it a list of lists
            List<List<GraphingNode>> cluster = new List<List<GraphingNode>>();
            foreach (var contents in YarnFileContents)
            {
                var (parseSource, diagnostics) = ParseSource(contents);

                List<GraphingNode> connections = new List<GraphingNode>();
                var jumpListener = new JumpGraphListener(connections);
                walker.Walk(jumpListener, parseSource.Tree);

                cluster.Add(connections);
            }

            return cluster;
        }

        /// <summary>
        /// Gets a string containing a representation of the compiled bytecode
        /// for a <see cref="Program"/>.
        /// </summary>
        /// <param name="program"></param>
        /// <param name="l"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string GetCompiledCodeAsString(Program program, Library? l = null, CompilationResult? result = null) {
            return program.DumpCode(l, result);
        }

        /// <summary>
        /// Returns an <see cref="IYarnValue"/> representation of the provided
        /// value.
        /// </summary>
        /// <param name="clrValue">The value to get a Yarn representation
        /// of.</param>
        /// <returns>An <see cref="IYarnValue"/> representation of <paramref
        /// name="clrValue"/>.</returns>
        public static IYarnValue? GetYarnValue(IConvertible clrValue)
        {
            if (Types.TypeMappings.TryGetValue(clrValue.GetType(), out var yarnType))
            {
                Value yarnValue = new Value(yarnType, clrValue);

                return yarnValue;
            } else {
                return null;
            }
        }
    }

    public struct GraphingNode
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        public string node;

        /// <summary>
        /// The list of nodes that this node jumps to.
        /// </summary>
        public string[] jumps;

        /// <summary>
        /// <see langword="true"/> if this <see cref="GraphingNode"/>'s <see
        /// cref="position"/> field contains valid information.
        /// </summary>
        public bool hasPositionalInformation;

        /// <summary>
        /// The position of this <see cref="GraphingNode"/>. Only valid when
        /// <see cref="hasPositionalInformation"/> is <see langword="true"/>.
        /// </summary>
        public (int x, int y) position;
    }

    /// <summary>
    /// Contains extension methods for producing <see cref="BasicBlock"/>
    /// objects from a Node.
    /// </summary>
    public static class InstructionCollectionExtensions
    {
        /// <summary>
        /// Produces <see cref="BasicBlock"/> objects from a Node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static IEnumerable<BasicBlock> GetBasicBlocks(this Node node, NodeDebugInfo info)
        {
            // If we don't have any instructions, return an empty collection
            if (node == null || node.Instructions == null || node.Instructions.Count == 0)
            {
                return Enumerable.Empty<BasicBlock>();
            }

            var result = new List<BasicBlock>();

            var leaderIndices = new HashSet<int>
            {
                // The first instruction is a leader.
                0,
            };

            for (int i = 0; i < node.Instructions.Count; i++)
            {
                switch (node.Instructions[i].InstructionTypeCase)
                {
                    case Instruction.InstructionTypeOneofCase.JumpTo:
                    case Instruction.InstructionTypeOneofCase.PeekAndJump:
                    case Instruction.InstructionTypeOneofCase.JumpIfFalse:
                    case Instruction.InstructionTypeOneofCase.Stop:
                    case Instruction.InstructionTypeOneofCase.RunNode:
                        // Every instruction after a jump (conditional or
                        // nonconditional) is a leader.
                        leaderIndices.Add(i + 1);
                        break;
                    default:
                        // nothing to do
                        break;
                }

                // If the instruction is labelled (i.e. it is the target of a
                // jump), it is a leader.
                if (info.GetLabel(i) != null) {
                    leaderIndices.Add(i);
                }
            }

            // Now that we know what the leaders are, run through the
            // instructions; every time we encounter a leader, start a new basic
            // block.
            var currentBlockInstructions = new List<Instruction>();

            int lastLeader = 0;

            for (int i = 0; i < node.Instructions.Count; i++)
            {
                // The current instruction is a leader! If we have accumulated
                // instructions, create a new block from them, store it, and
                // start a new list of instructions.
                if (leaderIndices.Contains(i))
                {
                    if (currentBlockInstructions.Count > 0)
                    {
                        var block = new BasicBlock
                        {
                            Node = node,
                            Instructions = new List<Instruction>(currentBlockInstructions),
                            FirstInstructionIndex = lastLeader,
                            LabelName = info.GetLabel(lastLeader) ?? null,
                        };
                        result.Add(block);
                    }

                    lastLeader = i;
                    currentBlockInstructions.Clear();
                }

                // Add the current instruction to our current accumulation.
                currentBlockInstructions.Add(node.Instructions[i]);
            }

            // We've reached the end of the instruction list. If we have any
            // accumulated instructions, create a final block here.
            if (currentBlockInstructions.Count > 0)
            {
                var block = new BasicBlock
                {
                    Node = node,
                    Instructions = new List<Instruction>(currentBlockInstructions),
                    FirstInstructionIndex = lastLeader,
                    LabelName = info.GetLabel(lastLeader) ?? null,
                };
                result.Add(block);
            }

            BasicBlock GetBlock(int startInstructionIndex)
            {

                try
                {
                    return result.First(block => block.FirstInstructionIndex == startInstructionIndex);
                }
                catch (System.InvalidOperationException)
                {
                    // nothing found
                    throw new System.InvalidOperationException($"No block in {node.Name} starts at index {startInstructionIndex}");
                }
            }

            // Final pass: now that we have all the blocks, go through each of
            // them and build the links between them
            foreach (var block in result)
            {
                var optionDestinations = new List<(int DestinationInstruction, string OptionLineID)>();
                string? currentStringAtTopOfStack = null;
                int count = 0;
                foreach (var instruction in block.Instructions)
                {
                    switch (instruction.InstructionTypeCase)
                    {
                        case Instruction.InstructionTypeOneofCase.AddOption:
                            {
                                // Track the destination that this instruction says
                                // it'll jump to. 
                                var destinationIndex = instruction.AddOption.Destination;
                                optionDestinations.Add((destinationIndex, instruction.AddOption.LineID));
                                break;
                            }
                        case Instruction.InstructionTypeOneofCase.PeekAndJump:
                            {
                                // We're jumping to a labeled section of the same node.

                                // PeekAndJump is really only used inside option
                                // selection handlers, so we can confidently
                                // assume that a PeekAndJump is an option.
                                foreach (var destination in optionDestinations)
                                {
                                    var (destinationIndex, destinationLineID) = destination;
                                    var destinationBlock = GetBlock(destinationIndex);

                                    block.AddDestination(destinationBlock, BasicBlock.Condition.Option, destinationLineID);
                                }
                                break;
                            }
                        case Instruction.InstructionTypeOneofCase.JumpTo:
                            {
                                var destinationIndex = GetBlock(instruction.JumpTo.Destination);
                                block.AddDestination(destinationIndex, BasicBlock.Condition.DirectJump);
                                break;
                            }
                        case Instruction.InstructionTypeOneofCase.PushString:
                            {
                                // The top of the stack is now a string. (This
                                // isn't perfect, because it doesn't handle
                                // stuff like functions, which modify the stack,
                                // but the most common case is <<jump
                                // NodeName>>, which is a combination of 'push
                                // string' followed by 'run node at top of
                                // stack')
                                currentStringAtTopOfStack = instruction.PushString.Value;
                                break;
                            }
                        case Instruction.InstructionTypeOneofCase.PushBool:
                        case Instruction.InstructionTypeOneofCase.PushFloat:
                        case Instruction.InstructionTypeOneofCase.PushVariable:
                            {
                                // The top of the stack is now no longer a
                                // string. Again, not a fully accurate
                                // representation of what's going on, but for
                                // the moment, we're not supporting 'jump to
                                // expression' here.
                                currentStringAtTopOfStack = null;
                                break;
                            }
                        case Instruction.InstructionTypeOneofCase.RunNode:
                            block.AddDestination(instruction.RunNode.NodeName, BasicBlock.Condition.DirectJump);
                            break;
                        case Instruction.InstructionTypeOneofCase.PeekAndRunNode:
                            {
                                if (currentStringAtTopOfStack != null)
                                {
                                    block.AddDestination(currentStringAtTopOfStack, BasicBlock.Condition.DirectJump);
                                }
                                break;
                            }
                        case Instruction.InstructionTypeOneofCase.JumpIfFalse:
                            {
                                var destinationIndex = instruction.JumpIfFalse.Destination;

                                // Jump-if-false falls through to the next
                                // instruction if the top of the stack is true,
                                // so the true block is whatever block is
                                // started by the next instruction.
                                var destinationTrueBlock = GetBlock(block.FirstInstructionIndex + count + 1);

                                // The false block is whichever block is started
                                // by the instruction's destination.
                                var destinationFalseBlock = GetBlock(destinationIndex);

                                block.AddDestination(destinationFalseBlock, BasicBlock.Condition.ExpressionIsFalse);
                                block.AddDestination(destinationTrueBlock, BasicBlock.Condition.ExpressionIsTrue);
                                break;
                            }
                    }
                    count += 1;
                }

                if (block.Destinations.Count() == 0)
                {
                    // We've reached the end of this block, and don't have any
                    // destinations. If our last destination isn't 'stop', then
                    // we'll fall through to the next node.
                    if (block.Instructions.Last().InstructionTypeCase != Instruction.InstructionTypeOneofCase.Stop)
                    {
                        var nextBlockStartInstruction = block.FirstInstructionIndex + block.Instructions.Count();

                        if (nextBlockStartInstruction >= node.Instructions.Count)
                        {
                            // We've reached the very end of the node's
                            // instructions. There are no blocks to jump to.
                        }
                        else
                        {

                            var destination = GetBlock(nextBlockStartInstruction);
                            block.AddDestination(destination, BasicBlock.Condition.Fallthrough);
                        }
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// A basic block is a run of instructions inside a Node. Basic blocks group
    /// instructions up into segments such that execution only ever begins at
    /// the start of a block (that is, a program never jumps into the middle of
    /// a block), and execution only ever leaves at the end of a block.
    /// </summary>
    public class BasicBlock
    {
        /// <summary>
        /// Gets the name of the label that this block begins at, or null if this basic block does not begin at a labelled instruction.
        /// </summary>
        public string? LabelName { get; set; }

        /// <summary>
        /// Gets the name of the node that this block is in.
        /// </summary>
        public string NodeName => Node?.Name ?? "(Unknown)";

        /// <summary>
        /// Gets the index of the first instruction of the node that this block is in.
        /// </summary>
        public int FirstInstructionIndex { get; set; }

        /// <summary>
        /// Gets the Node that this block was extracted from.
        /// </summary>
        public Node? Node;

        /// <summary>
        /// Gets a descriptive name for the block.
        /// </summary>
        /// <remarks>
        /// If this block begins at a labelled instruction, the name will be <c>[NodeName].[LabelName]</c>. Otherwise, it will be <c>[NodeName].[FirstInstructionIndex]</c>.
        /// </remarks>
        public string Name 
        {
            get
            {
                if (LabelName != null) 
                {
                    return $"{NodeName}.{LabelName}";
                }
                else
                {
                    return $"{NodeName}.{FirstInstructionIndex}";
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString() => this.Name;

        /// <summary>
        /// Gets a string containing the textual description of the instructions
        /// in this <see cref="BasicBlock"/>.
        /// </summary>
        /// <param name="library">The <see cref="Library"/> to use when
        /// converting instructions to strings.</param>
        /// <param name="compilationResult">The <see cref="CompilationResult"/>
        /// that produced <see cref="Node"/>.</param>
        /// <returns>A string containing the text version of the
        /// instructions.</returns>
        public string ToString(Library? library, CompilationResult? compilationResult)
        {
            var sb = new StringBuilder();
            foreach (var i in this.Instructions)
            {
                var desc = i.ToDescription(this.Node, library, compilationResult);
                sb.Append($"{desc.Type} {string.Join(",", desc.Operands)}");
                if (desc.Comments.Count() > 0) {
                    sb.AppendLine(" ; " + string.Join(", ", desc.Comments));
                } else {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the ancestors of this block - that is, blocks that may run immediately before this block.
        /// </summary>
        public IEnumerable<BasicBlock> Ancestors => ancestors;

        /// <summary>
        /// Gets the destinations of this block - that is, blocks or nodes that
        /// may run immediately after this block.
        /// </summary>
        /// <seealso cref="Destination"/>
        public IEnumerable<Destination> Destinations => destinations;

        /// <summary>
        /// Gets the Instructions that form this block.
        /// </summary>
        public IEnumerable<Instruction> Instructions { get; set; } = new List<Instruction>();

        /// <summary>
        /// Adds a new destination to this block, that points to another block.
        /// </summary>
        /// <param name="descendant">The new descendant node.</param>
        /// <param name="condition">The condition under which <paramref
        /// name="descendant"/> will be run.</param>
        /// <exception cref="ArgumentNullException">Thrown when descendant is
        /// <see langword="null"/>.</exception>
        public void AddDestination(BasicBlock descendant, Condition condition)
        {
            if (descendant is null)
            {
                throw new ArgumentNullException(nameof(descendant));
            }

            destinations.Add(new BlockDestination(descendant, condition));
            descendant.ancestors.Add(this);
        }

        /// <summary>
        /// Adds a new destination to this block, that points to a node.
        /// </summary>
        /// <param name="nodeName">The name of the destination node.</param>
        /// <param name="condition">The condition under which <paramref
        /// name="descendant"/> will be run.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref
        /// name="nodeName"/> is <see langword="null"/>.</exception>
        public void AddDestination(string nodeName, Condition condition)
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                throw new ArgumentException($"'{nameof(nodeName)}' cannot be null or empty.", nameof(nodeName));
            }

            destinations.Add(new NodeDestination(nodeName, condition));
        }

        /// <summary>
        /// Adds a new destination to this block that points to a node, with a
        /// option's line ID for context.
        /// </summary>
        /// <param name="descendant">The new descendant node.</param>
        /// <param name="condition">The condition under which the node <paramref
        /// name="descendant"/> will be run.</param>
        /// <param name="lineID">The line ID of the option that must be selected
        /// in order for <paramref name="descendant"/> to run.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref
        /// name="descendant"/> is <see langword="null"/>.</exception>
        public void AddDestination(BasicBlock descendant, Condition condition, string lineID)
        {
            if (descendant is null)
            {
                throw new ArgumentNullException(nameof(descendant));
            }

            destinations.Add(new OptionDestination(lineID, descendant));
            descendant.ancestors.Add(this);
        }

        private HashSet<BasicBlock> ancestors = new HashSet<BasicBlock>();

        private HashSet<Destination> destinations = new HashSet<Destination>();

        /// <summary>
        /// A destination represents a <see cref="BasicBlock"/> or node that may
        /// be run, following the execution of a <see cref="BasicBlock"/>.
        /// </summary>
        /// <remarks>
        /// Destination objects represent links between blocks, or between
        /// blocks and nodes.
        /// </remarks>
        public abstract class Destination
        {
            protected Destination(Condition condition)
            {
                this.Condition = condition;
            }

            /// <summary>
            /// The condition that causes this destination to be reached.
            /// </summary>
            public Condition Condition { get; set; }

            public override string? ToString()
            {
                switch (this.Condition)
                {
                    case Condition.ExpressionIsTrue:
                        return "true";
                    case Condition.ExpressionIsFalse:
                        return "false";
                    case Condition.Option:
                        return "(option)";
                    case Condition.Fallthrough:
                    case Condition.DirectJump:
                    default:
                        return null;
                }
            }


        }

        public class NodeDestination : Destination
        {
            public NodeDestination(string nodeName, Condition condition) : base(condition)
            {
                this.NodeName = nodeName;
            }

            /// <summary>
            /// The name of the node that this destination refers to.
            /// </summary>
            /// <remarks>This value is only valid when <see cref="Type"/> is
            /// <see cref="DestinationType.Node"/>.</remarks>
            public string NodeName { get; set; }
        }

        public class BlockDestination : Destination
        {
            /// <summary>
            /// The block that this destination refers to.
            /// </summary>
            /// <remarks>This value is only valid when <see cref="Type"/> is
            /// <see cref="DestinationType.Block"/>.</remarks>
            public BasicBlock Block { get; set; }

            public int DestinationInstructionIndex { get => Block.FirstInstructionIndex; }            

            public BlockDestination(BasicBlock block, Condition condition) : base(condition)
            {
                this.Block = block;
            }
        }

        public class OptionDestination : BlockDestination
        {
            public OptionDestination(string optionLineID, BasicBlock block) : base(block, Condition.Option)
            {
                this.OptionLineID = optionLineID;
            }

            public string OptionLineID { get; set; }

            public override string ToString()
            {
                return this.OptionLineID;
            }
        }

        /// <summary>
        /// Gets all descendants (that is, destinations, and destinations of
        /// those destinations, and so on), recursively.
        /// </summary>
        /// <remarks>
        /// Cycles are detected and avoided.
        /// </remarks>
        public IEnumerable<BasicBlock> Descendants
        {
            get
            {
                // Start with a queue of immediate children that link to blocks
                Queue<BasicBlock> candidates = new Queue<BasicBlock>(this.Destinations.OfType<BlockDestination>().Select(d => d.Block));

                List<BasicBlock> descendants = new List<BasicBlock>();

                while (candidates.Count > 0)
                {
                    var next = candidates.Dequeue();
                    if (descendants.Contains(next))
                    {
                        // We've already seen this one - skip it.
                        continue;
                    }
                    descendants.Add(next);
                    foreach (var destination in next.Destinations.OfType<BlockDestination>().Select(d => d.Block))
                    {
                        candidates.Enqueue(destination);
                    }
                }

                return descendants;

            }
        }

        /// <summary>
        /// Gets all descendants (that is, destinations, and destinations of
        /// those destinations, and so on) that have any player-visible content,
        /// recursively.
        /// </summary>
        /// <remarks>
        /// Cycles are detected and avoided.
        /// </remarks>
        public IEnumerable<BasicBlock> DescendantsWithPlayerVisibleContent
        {
            get
            {
                return Descendants.Where(d => d.PlayerVisibleContent.Any());
            }
        }

        /// <summary>
        /// The conditions under which a <see cref="Destination"/> may be
        /// reached at the end of a BasicBlock.
        /// </summary>
        public enum Condition
        {
            /// <summary>
            /// The Destination is reached because the preceding BasicBlock
            /// reached the end of its execution, and the Destination's target
            /// is the block immediately following.
            /// </summary>
            Fallthrough,

            /// <summary>
            /// The Destination is reached beacuse of an explicit instruction to
            /// go to this block.
            /// </summary>
            DirectJump,

            /// <summary>
            /// The Destination is reached because an expression evaluated to
            /// true.
            /// </summary>
            ExpressionIsTrue,

            /// <summary>
            /// The Destination is reached because an expression evaluated to
            /// false.
            /// </summary>
            ExpressionIsFalse,

            /// <summary>
            /// The Destination is reached because the player made an in-game
            /// choice to go to it.
            /// </summary>
            Option,
        }

        /// <summary>
        /// An abstract class that represents some content that is shown to the
        /// player.
        /// </summary>
        /// <remarks>
        /// This class is used, rather than the runtime classes Yarn.Line or
        /// Yarn.OptionSet, because when the program is being analysed, no
        /// values for any substitutions are available. Instead, these classes
        /// represent the data that is available offline.
        /// </remarks>
        public abstract class PlayerVisibleContentElement
        {
        }

        /// <summary>
        /// A line of dialogue that should be shown to the player.
        /// </summary>
        public class LineElement : PlayerVisibleContentElement
        {
            /// <summary>
            /// The string table ID of the line that will be shown to the player.
            /// </summary>
            public string LineID;
        }

        /// <summary>
        /// A collection of options that should be shown to the player.
        /// </summary>
        public class OptionsElement : PlayerVisibleContentElement
        {
            /// <summary>
            /// Represents a single option that may be presented to the player.
            /// </summary>
            public struct Option
            {
                /// <summary>
                /// The string table ID that will be shown to the player.
                /// </summary>
                public string LineID;

                /// <summary>
                /// The destination that will be run if this option is selected
                /// by the player.
                /// </summary>
                public int Destination;
            }

            /// <summary>
            /// The collection of options that will be delivered to the player.
            /// </summary>
            public IEnumerable<Option> Options;
        }

        /// <summary>
        /// A command that will be executed.
        /// </summary>
        public class CommandElement : PlayerVisibleContentElement
        {
            /// <summary>
            /// The text of the command.
            /// </summary>
            public string CommandText;
        }

        /// <summary>
        /// Gets the collection of player-visible content that will be delivered
        /// when this block is run.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Player-visible content means lines, options and commands. When this
        /// block is run, the entire contents of this collection will be
        /// displayed to the player, in the same order as they appear in this
        /// collection.
        /// </para>
        /// <para>
        /// If this collection is empty, then the block contains no visible
        /// content. This is the case for blocks that only contain logic, and do
        /// not contain any lines, options or commands.
        /// </para>
        /// <example>
        /// To tell the difference between the different kinds of content, use
        /// the <see langword="is"/> operator to check the type of each item:
        /// <code>
        /// foreach (var item in block.PlayerVisibleContent) { if (item is
        /// LineElement line) { // Do something with line } }
        /// </code>
        /// </example>
        /// </remarks>
        public IEnumerable<PlayerVisibleContentElement> PlayerVisibleContent
        {
            get
            {
                var accumulatedOptions = new List<(string LineID, int Destination)>();
                foreach (var instruction in Instructions)
                {
                    switch (instruction.InstructionTypeCase)
                    {
                        case Instruction.InstructionTypeOneofCase.RunLine:
                            yield return new LineElement
                            {
                                LineID = instruction.RunLine.LineID,
                            };
                            break;

                        case Instruction.InstructionTypeOneofCase.RunCommand:
                            yield return new CommandElement
                            {
                                CommandText = instruction.RunCommand.CommandText,
                            };
                            break;

                        case Instruction.InstructionTypeOneofCase.AddOption:
                            accumulatedOptions.Add(
                                (
                                    instruction.AddOption.LineID,
                                    instruction.AddOption.Destination
                                )
                            );
                            break;

                        case Instruction.InstructionTypeOneofCase.ShowOptions:
                            yield return new OptionsElement
                            {
                                Options = accumulatedOptions.Select(o => new OptionsElement.Option
                                {
                                    Destination = o.Destination,
                                    LineID = o.LineID,
                                })
                            };
                            accumulatedOptions.Clear();
                            break;
                    }
                }
            }
        }
    }
}
