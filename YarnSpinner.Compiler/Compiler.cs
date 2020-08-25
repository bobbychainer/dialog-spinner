﻿namespace Yarn.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using static Yarn.Instruction.Types;

    /// <summary>Specifies the result of compiling Yarn code.</summary>
    /// <remarks>This enum specifies the _type_ of success that resulted.
    /// Compilation failures will result in a <see cref="ParseException"/>,
    /// so they don't get a Status.</remarks>
    public enum CompilationStatus
    {
        /// <summary>The compilation succeeded with no errors.</summary>
        Succeeded,

        /// <summary>The compilation succeeded, but some strings do not
        /// have string tags.</summary>
        SucceededUntaggedStrings,
    }

    /// <summary>
    /// Information about a string. Stored inside a string table, which is
    /// produced from the Compiler.
    /// </summary>
    /// <remarks>
    /// You do not create instances of this class yourself. They are
    /// generated by the <see cref="Compiler"/>.
    /// </remarks>
    public struct StringInfo
    {
        /// <summary>
        /// The original text of the string.
        /// </summary>
        public string text;

        /// <summary>
        /// The name of the node that this string was found in.
        /// </summary>
        public string nodeName;

        /// <summary>
        /// The line number at which this string was found in the file.
        /// </summary>
        public int lineNumber;

        /// <summary>
        /// The name of the file this string was found in.
        /// </summary>
        public string fileName;

        /// <summary>
        /// Indicates whether this string's line ID was implicitly
        /// generated.
        /// </summary>
        /// <remarks>
        /// Implicitly generated line IDs are not guaranteed to remain the
        /// same across multiple compilations. To ensure that a line ID
        /// remains the same, you must define it by adding a [line
        /// tag]({{|ref "/docs/unity/localisation.md"|}}) to the line.
        /// </remarks>
        public bool isImplicitTag;

        /// <summary>
        /// The metadata (i.e. hashtags) associated with this string.
        /// </summary>
        /// <remarks>
        /// This array will contain any hashtags associated with this
        /// string besides the `#line:` hashtag.
        /// </remarks>
        public string[] metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringInfo"/>
        /// struct.
        /// </summary>
        /// <param name="text">The text of the string.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="nodeName">The node name.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="isImplicitTag">If `true`, this string info is
        /// stored with an implicit line ID.</param>
        /// <param name="metadata">The string's metadata.</param>
        internal StringInfo(string text, string fileName, string nodeName, int lineNumber, bool isImplicitTag, string[] metadata)
        {
            this.text = text;
            this.nodeName = nodeName;
            this.lineNumber = lineNumber;
            this.fileName = fileName;
            this.isImplicitTag = isImplicitTag;

            if (metadata != null)
            {
                this.metadata = metadata;
            }
            else
            {
                this.metadata = new string[] { };
            }

        }
    }

    public struct VariableDeclaration {
        public string name;
        public Value defaultValue;
        public Value.Type type;

        public override string ToString() {
            return $"{name} : {type} = {defaultValue}";
        }
    }

    public struct CompilationJob {

        /// <summary>
        /// Represents the contents of a file to compile.
        /// </summary>
        public struct File {
            public string FileName;
            public string Source;
        }

        /// <summary>
        /// The <see cref="File"/> structs that represent the content to
        /// parse..
        /// </summary>
        public IEnumerable<File> Files;

        /// <summary>
        /// The <see cref="Library"/> that contains declarations for
        /// functions.
        /// </summary>
        public Library Library;

        /// <summary>
        /// The declarations for variables.
        /// </summary>
        public IEnumerable<VariableDeclaration> VariableDeclarations;

        /// <summary>
        /// Creates a new <see cref="CompilationJob"/> using the contents
        /// of a collection of files.
        /// </summary>
        /// <param name="paths">The paths to the files.</param>
        /// <returns>A new <see cref="CompilationJob"/>.</returns>
        public static CompilationJob CreateFromFiles(IEnumerable<string> paths, Library library = null)
        {
            var fileList = new List<File>();

            // Read every file and add it to the file list
            foreach (var path in paths)
            {
                fileList.Add(new File
                {
                    FileName = System.IO.Path.GetFileNameWithoutExtension(path),
                    Source = System.IO.File.ReadAllText(path),
                });
            }

            return new CompilationJob
            {
                Files = fileList.ToArray(),
                Library = library,
            };
        }

        public static CompilationJob CreateFromFiles(params string[] paths) {
            return CreateFromFiles((IEnumerable<string>) paths);
        }

        /// <summary>
        /// Creates a new <see cref="CompilationJob"/> using the contents
        /// of a string.
        /// </summary>
        /// <param name="fileName">The name to assign to the compiled file.</param>
        /// <param name="source">The text to compile.</param>
        /// <returns>A new <see cref="CompilationJob"/>.</returns>
        public static CompilationJob CreateFromString(string fileName, string source, Library library = null) {
            return new CompilationJob
            {
                Files = new List<File>
                {
                    new File {
                        Source = source, FileName = fileName
                    },
                },
                Library = library,
            };
        }
    }

    public struct CompilationResult
    {
        public Program Program;
        public IDictionary<string, StringInfo> StringTable;
        public IEnumerable<VariableDeclaration> Declarations;
        public CompilationStatus Status;

        internal static CompilationResult CombineCompilationResults(IEnumerable<CompilationResult> results)
        {
            CompilationResult finalResult;

            var programs = new List<Program>();
            var declarations = new List<VariableDeclaration>();
            var mergedStringTable = new Dictionary<string, StringInfo>();

            var status = CompilationStatus.Succeeded;

            foreach (var result in results)
            {
                programs.Add(result.Program);
                
                if (result.Declarations != null) {
                    declarations.AddRange(result.Declarations);
                }

                foreach (var entry in result.StringTable)
                {
                    mergedStringTable.Add(entry.Key, entry.Value);
                }

                if (result.Status != CompilationStatus.Succeeded) {
                    status = result.Status;
                }
            }

            return new CompilationResult
            {
                Program = Program.Combine(programs.ToArray()),
                StringTable = mergedStringTable,
                Declarations = declarations,
                Status = status,
            };
        }
    }

    /// <summary>
    /// Compiles Yarn code.
    /// </summary>
    public class Compiler : YarnSpinnerParserBaseListener
    {
        /// <summary>A regular expression used to detect illegal characters
        /// in node titles.</summary>
        private readonly Regex invalidNodeTitleNameRegex = new Regex(@"[\[<>\]{}\|:\s#\$]");

        private int labelCount = 0;

        /// <summary>
        /// Gets the current node to which instructions are being added.
        /// </summary>
        /// <value>The current node.</value>
        internal Node CurrentNode { get; private set; }

        /// <summary>
        /// Gets whether we are currently parsing the current node as a
        /// 'raw text' node, or as a fully syntactic node.
        /// </summary>
        /// <value>Whether this is a raw text node or not.</value>
        internal bool RawTextNode { get; set; } = false;

        /// <summary>
        /// Gets the program being generated by the compiler.
        /// </summary>
        internal Program Program { get; private set; }

        /// <summary>
        /// The name of the file we are currently parsing from.
        /// </summary>
        private readonly string fileName;

        /// <summary>
        /// Indicates whether the file we are currently parsing contains
        /// string tags that were implicitly generated.
        /// </summary>
        private bool containsImplicitStringTags;

        /// <summary>
        /// The list of variable declarations known to the compiler.
        /// Supplied as part of a <see cref="CompilationJob"/>, or by <see
        /// cref="DeriveVariableDeclarations"/>
        /// </summary>
        internal IEnumerable<VariableDeclaration> VariableDeclarations = new List<VariableDeclaration>();

        /// <summary>
        /// The Library, which contains the function declarations known to the compiler. Supplied as part of a <see cref="CompilationJob"/>.
        /// </summary>
        internal Library Library { get; private set; }

        internal Compiler(string fileName)
        {
            Program = new Program();
            this.fileName = fileName;
        }

#if DEBUG
        internal string parseTree;
        internal List<string> tokens;
#endif

        public static CompilationResult Compile(CompilationJob compilationJob)
        {
            var results = new List<CompilationResult>();

            // All variable declarations that we've encountered during this compilation job
            var derivedVariableDeclarations = new List<VariableDeclaration>();

            // All variable declarations that we've encountered, PLUS the ones we knew about before
            var knownVariableDeclarations = new List<VariableDeclaration>();
            if (compilationJob.VariableDeclarations != null) {
                knownVariableDeclarations.AddRange(compilationJob.VariableDeclarations);
            }

            var compiledTrees = new List<(string name, IParseTree tree)>();

            // First pass: parse all files, generate their syntax trees, and figure out what variables they've delcared
            foreach (var file in compilationJob.Files) {
                var tree = ParseSyntaxTree(file);
                IEnumerable<VariableDeclaration> newDeclarations = DeriveVariableDeclarations(tree, knownVariableDeclarations);

                derivedVariableDeclarations.AddRange(newDeclarations);
                knownVariableDeclarations.AddRange(newDeclarations);

                compiledTrees.Add((file.FileName, tree));
            }

            foreach (var parsedFile in compiledTrees) {
                CompilationResult compilationResult = GenerateCode(parsedFile.name, knownVariableDeclarations, compilationJob, parsedFile.tree);
                results.Add(compilationResult);
            }            

            var finalResult = CompilationResult.CombineCompilationResults(results);
            finalResult.Declarations = derivedVariableDeclarations;

            return finalResult;
        }

        private static IEnumerable<VariableDeclaration> DeriveVariableDeclarations(IParseTree tree, IEnumerable<VariableDeclaration> existingDeclarations)
        {
            var variableDeclarationVisitor = new VariableDeclarationVisitor(existingDeclarations);

            variableDeclarationVisitor.Visit(tree);

            // Upon exit, declarations will now contain every variable declaration we found
            return variableDeclarationVisitor.NewVariableDeclarations;
        }

        private static CompilationResult GenerateCode(string fileName, IEnumerable<VariableDeclaration> variableDeclarations, CompilationJob job, IParseTree tree)
        {
            Compiler compiler = new Compiler(fileName);

            compiler.Library = job.Library;
            compiler.VariableDeclarations = variableDeclarations;

            compiler.Compile(tree);

            return new CompilationResult
            {
                Program = compiler.Program,
                StringTable = compiler.StringTable,
                Status = compiler.containsImplicitStringTags ? CompilationStatus.SucceededUntaggedStrings : CompilationStatus.Succeeded,                
            };
        }

        private static IParseTree ParseSyntaxTree(CompilationJob.File file)
        {
            ICharStream input = CharStreams.fromstring(file.Source);

            YarnSpinnerLexer lexer = new YarnSpinnerLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);

            YarnSpinnerParser parser = new YarnSpinnerParser(tokens);

            // turning off the normal error listener and using ours
            parser.RemoveErrorListeners();
            parser.AddErrorListener(ParserErrorListener.Instance);

            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(LexerErrorListener.Instance);

            IParseTree tree;
            try
            {
                tree = parser.dialogue();
            }
            catch (ParseException e)
            {
#if DEBUG
                var tokenStringList = new List<string>();
                tokens.Reset();
                foreach (var token in tokens.GetTokens())
                {
                    tokenStringList.Add($"{token.Line}:{token.Column} {YarnSpinnerLexer.DefaultVocabulary.GetDisplayName(token.Type)} \"{token.Text}\"");
                }

                throw new ParseException($"{e.Message}\n\nTokens:\n{string.Join("\n", tokenStringList)}");
#else
                throw new ParseException(e.Message);
#endif // DEBUG
            }

            return tree;
        }

        /// <summary>
        /// Lexes a string containing source code, and returns a list of
        /// tokens found in the source code.
        /// </summary>
        /// <param name="path">The path of the file containing source code
        /// to extract tokens from.</param>
        /// <returns>The list of tokens extracted from the source
        /// code.</returns>
        internal static List<string> GetTokensFromFile(string path)
        {
            var text = File.ReadAllText(path);
            return GetTokensFromString(text);
        }

        /// <summary>
        /// Lexes a string containing source code, and returns a list of
        /// tokens found in the source code.
        /// </summary>
        /// <param name="text">The source code to extract tokens
        /// from.</param>
        /// <returns>The list of tokens extracted from the source
        /// code.</returns>
        internal static List<string> GetTokensFromString(string text)
        {
            ICharStream input = CharStreams.fromstring(text);

            YarnSpinnerLexer lexer = new YarnSpinnerLexer(input);

            var tokenStringList = new List<string>();

            var tokens = lexer.GetAllTokens();
            foreach (var token in tokens)
            {
                tokenStringList.Add($"{token.Line}:{token.Column} {YarnSpinnerLexer.DefaultVocabulary.GetDisplayName(token.Type)} \"{token.Text}\"");
            }

            return tokenStringList;
        }

        internal Dictionary<string, StringInfo> StringTable = new Dictionary<string, StringInfo>();

        /// <summary>
        /// The number of strings encountered so far during compilation.
        /// </summary>
        private int stringCount = 0;

        /// <summary>
        /// Registers a new string in the string table.
        /// </summary>
        /// <param name="text">The text of the string to register.</param>
        /// <param name="nodeName">The name of the node that this string
        /// was found in.</param>
        /// <param name="lineID">The line ID to use for this entry in the
        /// string table.</param>
        /// <param name="lineNumber">The line number that this string was
        /// found in.</param>
        /// <param name="tags">The tags to associate with this string in
        /// the string table.</param>
        /// <returns>The string ID for the newly registered
        /// string.</returns>
        /// <remarks>If `lineID` is `null`, a line ID will be generated
        /// from <see cref="fileName"/>, the `nodeName`, and <see
        /// cref="stringCount"/>.
        internal string RegisterString(string text, string nodeName, string lineID, int lineNumber, string[] tags)
        {
            string lineIDUsed;

            bool isImplicit;

            if (lineID == null)
            {
                lineIDUsed = $"{this.fileName}-{nodeName}-{this.stringCount}";

                this.stringCount += 1;

                // Note that we had to make up a tag for this string, which
                // may not be the same on future compilations
                this.containsImplicitStringTags = true;

                isImplicit = true;
            }
            else
            {
                lineIDUsed = lineID;

                isImplicit = false;
            }

            var theString = new StringInfo(text, this.fileName, nodeName, lineNumber, isImplicit, tags);

            // Finally, add this to the string table, and return the line
            // ID.
            this.StringTable.Add(lineIDUsed, theString);

            return lineIDUsed;
        }


        /// <summary>
        /// Generates a unique label name to use in the program.
        /// </summary>
        /// <param name="commentary">Any additional text to append to the
        /// end of the label.</param>
        /// <returns>The new label name.</returns>
        internal string RegisterLabel(string commentary = null)
        {
            return "L" + labelCount++ + commentary;
        }

        /// <summary>
        /// Creates a new instruction, and appends it to a node in the <see
        /// cref="Program" />.
        /// </summary>
        /// <param name="node">The node to append instructions to.</param>
        /// <param name="code">The opcode of the instruction.</param>
        /// <param name="operands">The operands to associate with the
        /// instruction.</param>
        void Emit(Node node, OpCode code, params Operand[] operands)
        {
            var instruction = new Instruction
            {
                Opcode = code
            };

            instruction.Operands.Add(operands);

            node.Instructions.Add(instruction);
        }

        /// <summary>
        /// Creates a new instruction, and appends it to the current node
        /// in the <see cref="Program"/>. Called by instances of <see
        /// cref="CodeGenerationVisitor"/> while walking the parse tree.
        /// </summary>
        /// <param name="code">The opcode of the instruction.</param>
        /// <param name="operands">The operands to associate with the
        /// instruction.</param>
        internal void Emit(OpCode code, params Operand[] operands)
        {
            Emit(this.CurrentNode, code, operands);
        }

        /// <summary>
        /// Extracts a line ID from a <see
        /// cref="YarnSpinnerParser.HashtagContext"/>, if one exists.
        /// </summary>
        /// <param name="context">The hashtag parsing context.</param>
        /// <returns>The line ID if one is present in the hashtag context,
        /// otherwise `null`.</returns>
        internal string GetLineID(YarnSpinnerParser.HashtagContext[] context)
        {
            // if there are any hashtags
            if (context != null)
            {
                foreach (var hashtag in context)
                {
                    string tagText = hashtag.text.Text;
                    if (tagText.StartsWith("line:", StringComparison.InvariantCulture))
                    {
                        return tagText;
                    }
                }
            }
            return null;
        }

        // this replaces the CompileNode from the old compiler will start
        // walking the parse tree emitting byte code as it goes along this
        // will all get stored into our program var needs a tree to walk,
        // this comes from the ANTLR Parser/Lexer steps
        internal void Compile(IParseTree tree)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(this, tree);
        }

        // we have found a new node set up the currentNode var ready to
        // hold it and otherwise continue
        public override void EnterNode(YarnSpinnerParser.NodeContext context)
        {
            CurrentNode = new Node();
            RawTextNode = false;
        }
        // have left the current node store it into the program wipe the
        // var and make it ready to go again
        public override void ExitNode(YarnSpinnerParser.NodeContext context)
        {
            Program.Nodes[CurrentNode.Name] = CurrentNode;
            CurrentNode = null;
            RawTextNode = false;
        }


        // have finished with the header so about to enter the node body
        // and all its statements do the initial setup required before
        // compiling that body statements eg emit a new startlabel
        public override void ExitHeader(YarnSpinnerParser.HeaderContext context)
        {
            var headerKey = context.header_key.Text;

            // Use the header value if provided, else fall back to the
            // empty string. This means that a header like "foo: \n" will
            // be stored as 'foo', '', consistent with how it was typed.
            // That is, it's not null, because a header was provided, but
            // it was written as an empty line.
            var headerValue = context.header_value?.Text ?? "";

            if (headerKey.Equals("title", StringComparison.InvariantCulture))
            {
                // Set the name of the node
                CurrentNode.Name = headerValue;

                // Throw an exception if this node name contains illegal
                // characters
                if (invalidNodeTitleNameRegex.IsMatch(CurrentNode.Name))
                {
                    throw new ParseException($"The node '{CurrentNode.Name}' contains illegal characters in its title.");
                }
            }

            if (headerKey.Equals("tags", StringComparison.InvariantCulture))
            {
                // Split the list of tags by spaces, and use that
                var tags = headerValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                CurrentNode.Tags.Add(tags);

                if (CurrentNode.Tags.Contains("rawText"))
                {
                    // This is a raw text node. Flag it as such for future
                    // compilation.
                    RawTextNode = true;
                }

            }

        }

        // have entered the body the header should have finished being
        // parsed and currentNode ready all we do is set up a body visitor
        // and tell it to run through all the statements it handles
        // everything from that point onwards
        public override void EnterBody(YarnSpinnerParser.BodyContext context)
        {
            // if it is a regular node
            if (!RawTextNode)
            {
                // This is the start of a node that we can jump to. Add a
                // label at this point.
                CurrentNode.Labels.Add(RegisterLabel(), CurrentNode.Instructions.Count);

                CodeGenerationVisitor visitor = new CodeGenerationVisitor(this);

                foreach (var statement in context.statement())
                {
                    visitor.Visit(statement);
                }
            }
            // we are a rawText node turn the body into text save that into
            // the node perform no compilation TODO: oh glob! there has to
            // be a better way
            else
            {
                CurrentNode.SourceTextStringID = RegisterString(context.GetText(), CurrentNode.Name, "line:" + CurrentNode.Name, context.Start.Line, null);
            }
        }

        // exiting the body of the node, time for last minute work before
        // moving onto the next node Does this node end after emitting
        // AddOptions codes without calling ShowOptions?
        public override void ExitBody(YarnSpinnerParser.BodyContext context)
        {
            // if it is a regular node
            if (!RawTextNode)
            {
                // Note: this only works when we know that we don't have
                // AddOptions and then Jump up back into the code to run
                // them. TODO: A better solution would be for the parser to
                // flag whether a node has Options at the end.
                var hasRemainingOptions = false;
                foreach (var instruction in CurrentNode.Instructions)
                {
                    if (instruction.Opcode == OpCode.AddOption)
                    {
                        hasRemainingOptions = true;
                    }
                    if (instruction.Opcode == OpCode.ShowOptions)
                    {
                        hasRemainingOptions = false;
                    }
                }

                // If this compiled node has no lingering options to show
                // at the end of the node, then stop at the end
                if (hasRemainingOptions == false)
                {
                    Emit(CurrentNode, OpCode.Stop);
                }
                else
                {
                    // Otherwise, show the accumulated nodes and then jump
                    // to the selected node
                    Emit(CurrentNode, OpCode.ShowOptions);

                    // Showing options will make the execution stop; the
                    // user will have invoked code that pushes the name of
                    // a node onto the stack, which RunNode handles
                    Emit(CurrentNode, OpCode.RunNode);
                }
            }
        }
    }

    public class Graph
    {
        public ArrayList<String> nodes = new ArrayList<String>();
        public MultiMap<String, String> edges = new MultiMap<String, String>();
        public string graphName = "G";

        public void edge(String source, String target)
        {
            edges.Map(source, target);
        }
        public String toDot()
        {
            StringBuilder buf = new StringBuilder();
            buf.AppendFormat("digraph {0} ", graphName);
            buf.Append("{\n");
            buf.Append("  ");
            foreach (String node in nodes)
            { // print all nodes first
                buf.Append(node);
                buf.Append("; ");
            }
            buf.Append("\n");
            foreach (String src in edges.Keys)
            {
                IList<string> output;
                if (edges.TryGetValue(src, out output))
                {
                    foreach (String trg in output)
                    {
                        buf.Append("  ");
                        buf.Append(src);
                        buf.Append(" -> ");
                        buf.Append(trg);
                        buf.Append(";\n");
                    }
                }
            }
            buf.Append("}\n");
            return buf.ToString();
        }
    }
    public class GraphListener : YarnSpinnerParserBaseListener
    {
        String currentNode = null;
        public Graph graph = new Graph();

        public override void EnterHeader(YarnSpinnerParser.HeaderContext context)
        {
            if (context.header_key.Text == "title")
            {
                currentNode = context.header_value.Text;
            }
        }

        public override void ExitNode(YarnSpinnerParser.NodeContext context)
        {
            // Add this node to the graph
            graph.nodes.Add(currentNode);
        }
        public override void ExitOptionJump(YarnSpinnerParser.OptionJumpContext context)
        {
            graph.edge(currentNode, context.NodeName.Text);
        }

        public override void ExitOptionLink(YarnSpinnerParser.OptionLinkContext context)
        {
            graph.edge(currentNode, context.NodeName.Text);
        }

    }
}
