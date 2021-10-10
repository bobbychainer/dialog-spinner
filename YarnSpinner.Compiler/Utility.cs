namespace Yarn.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

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
        /// <remarks>This method is intended to be called by tools that let
        /// the user manage variable declarations. Such tools can read the
        /// existing variable declarations in from a script (by compiling
        /// the script with the `DeclarationsOnly` <see
        /// cref="CompilationJob.CompilationType"/>), allow the user to
        /// make changes, and then write the changes to disk by calling
        /// this method and saving the results.</remarks>
        /// <param name="declarations">The collection of <see
        /// cref="Declaration"/> objects to include in the output.</param>
        /// <param name="title">The title of the node that should be
        /// generated.</param>
        /// <param name="tags">The collection of tags that should be
        /// generated for the node. If this is <see langword="null"/>, no
        /// tags will be generated.</param>
        /// <param name="headers">The collection of additional headers that
        /// should be generated for the node. If this is <see
        /// langword="null"/>, no additional headers will be
        /// generated.</param>
        /// <returns>A string containing a Yarn script that declares the
        /// specified variables.</returns>
        /// <throws cref="ArgumentOutOfRangeException">Thrown when any of
        /// the <see cref="Declaration"/> objects in <paramref
        /// name="declarations"/> is not a variable declaration, or if the
        /// <see cref="Declaration.ReturnType"/> of any of the declarations
        /// is an invalid value.</throws>
        public static string GenerateYarnFileWithDeclarations(
            IEnumerable<Yarn.Compiler.Declaration> declarations,
            string title = "Program",
            IEnumerable<string> tags = null,
            IDictionary<string, string> headers = null)
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
                    throw new ArgumentOutOfRangeException($"Declaration {decl.Name} is a {decl.Type.Name}; it must be a variable.");
                }

                if (string.IsNullOrEmpty(decl.Description) == false)
                {
                    if (count > 0) {
                        // Insert a blank line above this comment, for readibility
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.AppendLine($"/// {decl.Description}");
                }

                stringBuilder.Append($"<<declare {decl.Name} = ");

                if (decl.Type == BuiltinTypes.Number) {
                    stringBuilder.Append(decl.DefaultValue);
                } else if (decl.Type == BuiltinTypes.String) {
                    stringBuilder.Append('"' + (string)decl.DefaultValue + '"');
                } else if (decl.Type == BuiltinTypes.Boolean) {
                    stringBuilder.Append((bool)decl.DefaultValue ? "true" : "false");
                } else {
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
        /// <remarks>This method ensures that it does not generate line
        /// tags that are already present in the file, or present in the
        /// <paramref name="existingLineTags"/> collection.
        ///
        /// Line tags are added to any line of source code that contains
        /// user-visible text: lines, options, and shortcut options.
        /// </remarks>
        /// <param name="contents">The source code to add line tags
        /// to.</param>
        /// <param name="existingLineTags">The collection of line tags
        /// already exist elsewhere in the source code; the newly added
        /// line tags will not be duplicates of any in this
        /// collection.</param>
        /// <returns>The modified source code, with line tags
        /// added.</returns>
        public static string AddTagsToLines(string contents, ICollection<string> existingLineTags = null)
        {
            var compileJob = CompilationJob.CreateFromString("input", contents);

            compileJob.CompilationType = CompilationJob.Type.StringsOnly;

            var result = Compiler.Compile(compileJob);

            var untaggedLines = result.StringTable.Where(entry => entry.Value.isImplicitTag);

            var allSourceLines = contents.Split(new[] { "\n", "\r\n", "\n" }, StringSplitOptions.None);


            HashSet<string> existingLines;
            if (existingLineTags != null) {
                existingLines = new HashSet<string>(existingLineTags);
            } else {
                existingLines = new HashSet<string>();
            }

            foreach (var untaggedLine in untaggedLines)
            {
                var lineNumber = untaggedLine.Value.lineNumber;
                var tag = "#" + GenerateString(existingLines);

                var sourceLine = allSourceLines[lineNumber - 1];
                var updatedSourceLine = sourceLine.Replace(untaggedLine.Value.text, $"{untaggedLine.Value.text} {tag}");

                allSourceLines[lineNumber - 1] = updatedSourceLine;

                existingLines.Add(tag);
            }

            return string.Join(Environment.NewLine, allSourceLines);
        }

        /// <summary>
        /// Gets an <see cref="Antlr4.Runtime.Tree.IParseTree"/> generated
        /// from parsing the specified text in <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source code to parse.</param>
        /// <returns>A source tree.</returns>
        /// <exception cref="ParseException">Thrown when source contains
        /// compilation errors.</exception>
        public static Antlr4.Runtime.Tree.IParseTree GetParseTree(string source)
        {
            var (tree, _, _) = Compiler.ParseSyntaxTree("<input>", source);

            return tree;
        }

        /// <summary>
        /// Gets an enumeration containing the <see
        /// cref="Antlr4.Runtime.IToken"/> objects generated from parsing
        /// the specified text in <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source code to parse.</param>
        /// <returns>An enumeration containing tokens.</returns>
        /// <exception cref="ParseException">Thrown when source contains
        /// compilation errors.</exception>
        public static IEnumerable<Antlr4.Runtime.IToken> GetTokens(string source)
        {
            var (_, tokens, _) = Compiler.ParseSyntaxTree("<input>", source);

            return tokens.Get(0, tokens.Size - 1);
        }

        /// <summary>
        /// Generates a new unique line tag that is not present in
        /// `existingKeys`.
        /// </summary>
        /// <param name="existingKeys">The collection of keys that should
        /// be considered when generating a new, unique line tag.</param>
        /// <returns>A unique line tag that is not already present in
        /// `existingKeys`.</returns>
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
    }
}
