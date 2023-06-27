// Copyright Yarn Spinner Pty Ltd
// Licensed under the MIT License. See LICENSE.md in project root for license information.

namespace Yarn.Compiler.Upgrader
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;

    internal class VariableDeclarationUpgrader : ILanguageUpgrader
    {
        public UpgradeResult Upgrade(UpgradeJob upgradeJob)
        {
            var potentialTypeBindings = new OrderedSet<TypeBinding>();
            var allSeenVariables = new OrderedSet<string>();

            foreach (var file in upgradeJob.Files)
            {
                ICharStream input = CharStreams.fromstring(file.Source);
                YarnSpinnerV1Lexer lexer = new YarnSpinnerV1Lexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                YarnSpinnerV1Parser parser = new YarnSpinnerV1Parser(tokens);

                YarnSpinnerV1Parser.DialogueContext tree = parser.dialogue();

                var declarationVisitor = new VariableTypeBindingVisitor(potentialTypeBindings, allSeenVariables);
                declarationVisitor.Visit(tree);
            }

            // We now have information on which variables exist, and
            // potentially what type they are. We can now do a pass,
            // rewriting certain expressions.
            var outputFiles = new List<UpgradeResult.OutputFile>();
            foreach (var file in upgradeJob.Files)
            {
                ICharStream input = CharStreams.fromstring(file.Source);
                YarnSpinnerV1Lexer lexer = new YarnSpinnerV1Lexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                YarnSpinnerV1Parser parser = new YarnSpinnerV1Parser(tokens);

                YarnSpinnerV1Parser.DialogueContext tree = parser.dialogue();

                var replacements = new List<TextReplacement>();

                var declarationVisitorWithReplacements = new VariableTypeBindingVisitor(potentialTypeBindings, allSeenVariables, replacements);
                declarationVisitorWithReplacements.Visit(tree);

                outputFiles.Add(new UpgradeResult.OutputFile(file.FileName, replacements, file.Source));
            }

            List<TypeBinding> unifiedVariableBindings = TypeBinding.UnifyBindings(potentialTypeBindings);

            // We finally have our bindings. Generate declarations for them.
            var allBindings = unifiedVariableBindings.ToDictionary(b => b.VariableName, b => b.Type);

            if (allSeenVariables.Count() > 0)
            {
                // If we're here, we've got variables, and we need to
                // generate declarations for them. We'll do this by
                // generating a node in a new file that contains them.
                var declarationsNodeStringBuilder = new System.Text.StringBuilder();

                // declarationsNodeStringBuilder.AppendLine
                var preambleLines = new[]
                {
                    "title: _AutoGeneratedVariableDeclarations",
                    "tags: generated ys-upgrade-v1-to-v2",
                    "---",
                    "// NOTE: These variable declarations were automatically generated as part",
                    "// of the upgrade process. Please check these before using them in your",
                    "// game.",
                };

                foreach (var line in preambleLines)
                {
                    declarationsNodeStringBuilder.AppendLine(line);
                }

                declarationsNodeStringBuilder.AppendLine();

                // Create a dictionary that maps a type to a string that
                // will be used in declarations. This string contains the
                // name of the type as defined in Yarn, as well as a
                // default value.
                var typesToStrings = new Dictionary<IType, string>
                {
                    { BuiltinTypes.Boolean, "false as bool" },
                    { BuiltinTypes.Number, "0 as number" },
                    { BuiltinTypes.String, @""""" as string" },
                };

                foreach (var variableName in allSeenVariables)
                {
                    IType type;

                    if (allBindings.ContainsKey(variableName))
                    {
                        type = allBindings[variableName];
                    }
                    else
                    {
                        type = BuiltinTypes.Undefined;
                    }

                    var typeIsDefined = type != BuiltinTypes.Undefined;

                    declarationsNodeStringBuilder.Append("<<declare ");
                    declarationsNodeStringBuilder.Append(variableName);
                    declarationsNodeStringBuilder.Append(" = ");

                    if (typeIsDefined)
                    {
                        declarationsNodeStringBuilder.Append(typesToStrings[type]);
                    }
                    else
                    {
                        declarationsNodeStringBuilder.Append("undefined");
                    }
                    
                    declarationsNodeStringBuilder.Append(">>");
                    declarationsNodeStringBuilder.AppendLine();
                }

                declarationsNodeStringBuilder.AppendLine("===");

                var outputDirectory = System.IO.Path.GetDirectoryName(upgradeJob.Files.First().FileName);
                var outputFileName = System.IO.Path.Combine(outputDirectory, "Program.yarnproject");

                var declarationsOutputFile = new UpgradeResult.OutputFile(outputFileName, declarationsNodeStringBuilder.ToString());

                outputFiles.Add(declarationsOutputFile);
            }

            return new UpgradeResult
            {
                Files = outputFiles,
            };
        }

        /// <summary>
        /// A Visitor that walks an expression parse tree and returns its type.
        /// Call the <see cref="AbstractParseTreeVisitor{Result}.Visit(Antlr4.Runtime.Tree.IParseTree)"/> method to begin checking. If a single
        /// valid type for the parse tree can't be found, a TypeException is
        /// thrown.
        /// </summary>
        private class VariableTypeBindingVisitor : YarnSpinnerV1ParserBaseVisitor<Yarn.IType>
        {
            private readonly OrderedSet<TypeBinding> potentialTypeBindings;

            private readonly OrderedSet<string> allSeenVariables;

            private readonly ICollection<TextReplacement> replacements;

            public VariableTypeBindingVisitor(
                [NotNull] OrderedSet<TypeBinding> potentialTypeBindings,
                [NotNull] OrderedSet<string> allSeenVariables,
                ICollection<TextReplacement> replacements = null)
            {
                this.potentialTypeBindings = potentialTypeBindings;
                this.allSeenVariables = allSeenVariables;
                this.replacements = replacements;
            }

            // This class generates replacements if a Replacements collection was provided.
            public bool GenerateReplacements => this.replacements != null;

            protected override Yarn.IType DefaultResult => BuiltinTypes.Undefined;

            public override Yarn.IType VisitValueString(YarnSpinnerV1Parser.ValueStringContext context)
            {
                return BuiltinTypes.String;
            }

            public override Yarn.IType VisitValueTrue(YarnSpinnerV1Parser.ValueTrueContext context)
            {
                return BuiltinTypes.Boolean;
            }

            public override Yarn.IType VisitValueFalse(YarnSpinnerV1Parser.ValueFalseContext context)
            {
                return BuiltinTypes.Boolean;
            }

            public override Yarn.IType VisitValueNumber(YarnSpinnerV1Parser.ValueNumberContext context)
            {
                return BuiltinTypes.Number;
            }

            public override Yarn.IType VisitValueVar(YarnSpinnerV1Parser.ValueVarContext context)
            {
                return this.VisitVariable(context.variable());
            }

            public override Yarn.IType VisitVariable(YarnSpinnerV1Parser.VariableContext context)
            {
                // The type of the value depends on the declared type of the
                // variable.

                // Note that we've seen a variable by this name, in case we
                // don't see it again and we never decide on a type for it
                var name = context.VAR_ID().GetText();
                this.allSeenVariables.Add(name);

                // Search our list of variable type bindings. If we have
                // precisely one binding at the moment for the variable, we
                // assume that we know its type, and return that value.
                // Otherwise, we return 'undefined' for this value (either
                // because we have no binding for this variable, or because
                // we have more than one.)
                var bindings = this.potentialTypeBindings.Where(b => b.VariableName == name);

                return bindings.Count() != 1 ? BuiltinTypes.Undefined : bindings.First().Type;
            }

            public override Yarn.IType VisitValueNull(YarnSpinnerV1Parser.ValueNullContext context)
            {
                // Null is not a permitted type in Yarn Spinner, so we have
                // to return undefined here
                return BuiltinTypes.Undefined;
            }

            public override Yarn.IType VisitValueFunc(YarnSpinnerV1Parser.ValueFuncContext context)
            {
                // In Yarn Spinner 1, we don't know the return type OR the
                // parameter types of functions. This means that we can't
                // use return type information, and we'll always return
                // Undefined. We will, however, make sure we visit all
                // parameters, since it contain expressions that result in
                // a variable being bound to a type.

                // Check each parameter of the function
                var suppliedParameters = context.function().expression();

                for (int i = 0; i < suppliedParameters.Length; i++)
                {
                    var suppliedParameter = suppliedParameters[i];

                    this.Visit(suppliedParameter);
                }

                return BuiltinTypes.Undefined;
            }

            public override Yarn.IType VisitExpValue(YarnSpinnerV1Parser.ExpValueContext context)
            {
                // Value expressions have the type of their inner value
                return Visit(context.value());
            }

            public override Yarn.IType VisitExpParens(YarnSpinnerV1Parser.ExpParensContext context)
            {
                // Parens expressions have the type of their inner expression
                return Visit(context.expression());
            }

            public override Yarn.IType VisitExpAndOrXor(YarnSpinnerV1Parser.ExpAndOrXorContext context)
            {
                return CheckOperation(context, context.expression(), context.op.Text, BuiltinTypes.Boolean);
            }

            private Yarn.IType CheckOperation(ParserRuleContext context, ParserRuleContext[] terms, string operationType, params Yarn.IType[] permittedTypes)
            {

                var types = new List<Yarn.IType>();

                // Get all defined types from our terms
                var termsToTypes = terms.ToDictionary(t => t, t => this.Visit(t));

                var termTypes = termsToTypes.Values;

                // If any of our terms are undefined, and they are NOT
                // variables (which is ok, because they might be defined
                // later!), then this expression is itself undefined.
                var nonVariableTerms = terms.OfType<YarnSpinnerV1Parser.ExpressionContext>()
                    .Where(c => c.GetChild<YarnSpinnerV1Parser.ValueVarContext>(0) == null);
                
                foreach (var term in nonVariableTerms)
                {
                    if (termsToTypes[term] == BuiltinTypes.Undefined)
                    {
                        return BuiltinTypes.Undefined;
                    }
                }
                
                // For any terms that are variables, check to see if we
                // have any knowledge of what types they are.

                // Start by getting all variable terms - that is, entries
                // in term that contain a ValueVarContext (which will
                // itself have a VariableContext in it); with these, take
                // their names
                var variableNames = terms
                    .OfType<YarnSpinnerV1Parser.ExpressionContext>()
                    .Select(c => c.GetChild<YarnSpinnerV1Parser.ValueVarContext>(0))
                    .Where (c => c != null)
                    .Select(v => v.variable().VAR_ID().GetText())
                    .Distinct();

                // Next, get the potential types of these variables where
                // we know anything about their type
                var variableTypes = this.potentialTypeBindings
                    .Where(b => variableNames.Contains(b.VariableName))
                    .Select(b => b.Type)
                    .Distinct();

                // The collection of types participating in this expression
                // is equal to the collection of the constant values and
                // sub-expressions of the terms present, as well as the
                // types of the variables present that we know about. This
                // will ideally be exactly one.
                var potentialExpressionTypes = termTypes
                    .Concat(variableTypes)
                    .Where(t => t != BuiltinTypes.Undefined)
                    .Distinct();

                // If we have precisely one type of this expression, then
                // we can conclude that any variables participating in that
                // expression should be bound to that type.
                if (potentialExpressionTypes.Count() == 1)
                {
                    IType type = potentialExpressionTypes.First();

                    if (permittedTypes.Contains(type) == false)
                    {
                        // We resolved to a specific type, but this
                        // operation doesn't permit that type. This
                        // expression is therefore invalid, and therefore
                        // its type is undefined.
                        return BuiltinTypes.Undefined;
                    }

                    // Create a type binding for each variable to this
                    // type.
                    foreach (var variableName in variableNames)
                    {
                        potentialTypeBindings.Add(new TypeBinding
                        {
                            Type = type,
                            VariableName = variableName,
                        });
                    }

                    return type;
                }
                else
                {
                    // TODO: consider handling the situation where we have
                    // multiple types participating in this expression, or
                    // have no information on what the type could be.
                    //
                    // For example, an expression like 1 + "1" in Yarn
                    // Spinner 1 was handled by converting the non-string
                    // operand to a string.
                    //
                    // For the moment, return Undefined.
                    return BuiltinTypes.Undefined;
                }
            }

            public override Yarn.IType VisitIf_clause(YarnSpinnerV1Parser.If_clauseContext context)
            {
                YarnSpinnerV1Parser.ExpressionContext expressionContext = context.expression();
                this.CheckAndRewriteIfClause(context, expressionContext);

                return BuiltinTypes.Boolean;
            }

            public override Yarn.IType VisitElse_if_clause(YarnSpinnerV1Parser.Else_if_clauseContext context)
            {
                YarnSpinnerV1Parser.ExpressionContext expressionContext = context.expression();
                this.CheckAndRewriteIfClause(context, expressionContext);

                return BuiltinTypes.Boolean;
            }

            private void CheckAndRewriteIfClause(ParserRuleContext context, YarnSpinnerV1Parser.ExpressionContext expressionContext)
            {
                // Evaluate the if statement's expression. In YS2, if
                // statement expressions are required to be bool, but in
                // YS1, they can be any type. In this upgrader, we 'permit'
                // the type to also be int, and if it is, we'll later
                // rewrite the expression to be of the type "EXP != 0" by
                // running it through the checker with 'upgrades' enabled
                var expressions = new[] { expressionContext };

                var type = this.CheckOperation(context, expressions, "if statement", BuiltinTypes.Boolean, BuiltinTypes.Number);

                if (this.GenerateReplacements && type == BuiltinTypes.Number)
                {

                    // This if statement resolved to a number expression,
                    // and we want to generate replacements. We'll generate
                    // a replacement that converts it to a bool.

                    // A 'simple expression' is on where the expression is
                    // just a value, eg <<if $x>>. A complex expression is
                    // one with operators (<<if $x + 1>>). We'll wrap
                    // complex expressions in parentheses.
                    var isSimpleExpression = expressionContext is YarnSpinnerV1Parser.ExpValueContext;

                    // Get the original text of expressionContext. We can't
                    // use "expressionContext.GetText()" here, because that
                    // just concatenates the text of all captured tokens,
                    // and doesn't include text on hidden channels (e.g.
                    // whitespace and comments).
                    var interval = new Interval(expressionContext.Start.StartIndex, expressionContext.Stop.StopIndex);
                    string originalText = expressionContext.Start.InputStream.GetText(interval);

                    string replacementText;

                    if (isSimpleExpression)
                    {
                        // No need to wrap
                        replacementText = originalText + " != 0";
                    }
                    else
                    {
                        // Wrap with parentheses
                        replacementText = "(" + originalText + ") != 0";
                    }

                    this.replacements.Add(new TextReplacement
                    {
                        Comment = "Converting if statement expression to a boolean",
                        OriginalText = originalText,
                        StartLine = expressionContext.Start.Line,
                        Start = expressionContext.Start.StartIndex,
                        ReplacementText = replacementText,
                    });
                }
            }

            public override Yarn.IType VisitExpAddSub(YarnSpinnerV1Parser.ExpAddSubContext context)
            {

                var expressions = context.expression();

                switch (context.op.Text)
                {
                    case "+":
                        // + supports strings and numbers
                        return CheckOperation(context, expressions, context.op.Text, BuiltinTypes.String, BuiltinTypes.Number);
                    case "-":
                        // - supports only numbers
                        return CheckOperation(context, expressions, context.op.Text, BuiltinTypes.Number);
                    default:
                        throw new InvalidOperationException($"Internal error: {nameof(VisitExpAddSub)} got unexpected op {context.op.Text}");
                }
            }

            public override Yarn.IType VisitExpMultDivMod(YarnSpinnerV1Parser.ExpMultDivModContext context)
            {
                var expressions = context.expression();

                // *, /, % all support numbers only
                return CheckOperation(context, expressions, context.op.Text, BuiltinTypes.Number);
            }

            public override Yarn.IType VisitExpPlusMinusEquals(YarnSpinnerV1Parser.ExpPlusMinusEqualsContext context)
            {
                ParserRuleContext[] terms = { context.variable(), context.expression() };

                switch (context.op.Text)
                {
                    case "+=":
                        // + supports strings and numbers
                        return CheckOperation(context, terms, context.op.Text, BuiltinTypes.String, BuiltinTypes.Number);
                    case "-=":
                        // - supports only numbers
                        return CheckOperation(context, terms, context.op.Text, BuiltinTypes.Number);
                    default:
                        throw new InvalidOperationException($"Internal error: {nameof(VisitExpMultDivMod)} got unexpected op {context.op.Text}");
                }
            }

            public override Yarn.IType VisitExpMultDivModEquals(YarnSpinnerV1Parser.ExpMultDivModEqualsContext context)
            {
                ParserRuleContext[] terms = { context.variable(), context.expression() };

                // *, /, % all support numbers only
                return CheckOperation(context, terms, context.op.Text, BuiltinTypes.Number);
            }

            public override Yarn.IType VisitExpComparison(YarnSpinnerV1Parser.ExpComparisonContext context)
            {
                ParserRuleContext[] terms = context.expression();

                // <, <=, >, >= all support numbers only
                CheckOperation(context, terms, context.op.Text, BuiltinTypes.Number);

                // Comparisons always return bool
                return BuiltinTypes.Boolean;
            }

            public override Yarn.IType VisitExpEquality(YarnSpinnerV1Parser.ExpEqualityContext context)
            {
                ParserRuleContext[] terms = context.expression();

                // == and != support any defined type, as long as terms are the
                // same type
                CheckOperation(context, terms, context.op.Text, BuiltinTypes.Number, BuiltinTypes.String, BuiltinTypes.Boolean);

                // Equality checks always return bool
                return BuiltinTypes.Boolean;
            }

            public override Yarn.IType VisitExpNegative(YarnSpinnerV1Parser.ExpNegativeContext context)
            {
                ParserRuleContext[] terms = new[] { context.expression() };

                // - supports only number types
                return CheckOperation(context, terms, "-", BuiltinTypes.Number);

            }

            public override Yarn.IType VisitExpNot(YarnSpinnerV1Parser.ExpNotContext context)
            {
                ParserRuleContext[] terms = new[] { context.expression() };

                // ! supports only bool types
                return CheckOperation(context, terms, "!", BuiltinTypes.Boolean);
            }

            public override IType VisitSetExpression([NotNull] YarnSpinnerV1Parser.SetExpressionContext context)
            {
                // This is just an expression; visit it
                return this.Visit(context.expression());
            }

            public override IType VisitSetVariableToValue([NotNull] YarnSpinnerV1Parser.SetVariableToValueContext context)
            {
                // Determine the type of the value
                var valueType = this.Visit(context.expression());

                this.allSeenVariables.Add(context.VAR_ID().GetText());

                // Bind the variable to this value
                this.potentialTypeBindings.Add(new TypeBinding
                {
                    VariableName = context.VAR_ID().GetText(),
                    Type = valueType,
                });

                return valueType;
            }
        }
    }

    public class OrderedSet<T> : ICollection<T>
    {
        private readonly IDictionary<T, LinkedListNode<T>> Dictionary;
        private readonly LinkedList<T> LinkedList;
    
        public OrderedSet()
            : this(EqualityComparer<T>.Default)
        {
        }
    
        public OrderedSet(IEqualityComparer<T> comparer)
        {
            Dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
            LinkedList = new LinkedList<T>();
        }

        public int Count => Dictionary.Count;

        public virtual bool IsReadOnly => Dictionary.IsReadOnly;

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Add(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
    
        public void Clear()
        {
            LinkedList.Clear();
            Dictionary.Clear();
        }
    
        public bool Remove(T item)
        {
            bool found = Dictionary.TryGetValue(item, out LinkedListNode<T> node);
            if (!found)
            {
                return false;
            }

            Dictionary.Remove(item);
            LinkedList.Remove(node);
            return true;
        }
    
        public IEnumerator<T> GetEnumerator()
        {
            return LinkedList.GetEnumerator();
        }
    
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    
        public bool Contains(T item)
        {
            return Dictionary.ContainsKey(item);
        }
    
        public void CopyTo(T[] array, int arrayIndex)
        {
            LinkedList.CopyTo(array, arrayIndex);
        }
    
        public bool Add(T item)
        {
            if (Dictionary.ContainsKey(item))
            {
                return false;
            }

            LinkedListNode<T> node = LinkedList.AddLast(item);
            Dictionary.Add(item, node);
            return true;
        }
    }
}
