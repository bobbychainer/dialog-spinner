﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using YarnLanguageServer.Diagnostics;

namespace YarnLanguageServer.Handlers
{
    internal class CodeActionHandler : ICodeActionHandler
    {
        private Workspace workspace;

        public CodeActionHandler(Workspace workspace)
        {
            this.workspace = workspace;
        }

        public Task<CommandOrCodeActionContainer> Handle(CodeActionParams request, CancellationToken cancellationToken)
        {
            var results = new List<CommandOrCodeAction>();
            foreach (var diagnostic in request.Context.Diagnostics)
            {
                if (!diagnostic.Code.HasValue || !diagnostic.Code.Value.IsString)
                {
                    continue;
                }

                switch (diagnostic.Code.Value.String)
                {
                    case nameof(YarnDiagnosticCode.YRNMsngCmdDef):
                        results.AddRange(HandleYRNMsngCmdDef(diagnostic, request.TextDocument.Uri));
                        break;
                    case nameof(YarnDiagnosticCode.YRNMsngVarDec):
                        results.AddRange(HandleYRNMsngVarDec(diagnostic, request.TextDocument.Uri));
                        break;
                }
            }

            return Task.FromResult<CommandOrCodeActionContainer>(results);
        }

        public CodeActionRegistrationOptions GetRegistrationOptions(CodeActionCapability capability, ClientCapabilities clientCapabilities)
        {
            return new CodeActionRegistrationOptions
            {
                DocumentSelector = Utils.YarnDocumentSelector,
                ResolveProvider = false,
                CodeActionKinds = new Container<CodeActionKind>(CodeActionKind.QuickFix),

                // TODO Consider implementing CodeActionKind.SourceFixAll available in proposed lsp 3.17
            };
        }

        private IEnumerable<CommandOrCodeAction> HandleYRNMsngVarDec(Diagnostic diagnostic, DocumentUri uri)
        {
            var name = diagnostic.Data?.ToString();
            if (string.IsNullOrEmpty(name)) { return Enumerable.Empty<CommandOrCodeAction>(); }

            var suggestions =
                workspace.GetVariables(name, true).DistinctBy(d => d.Name)
                .Take(10)
                .Select(declaration =>
                {
                    var edits = new Dictionary<DocumentUri, IEnumerable<TextEdit>>();
                    edits[uri] = new List<TextEdit> { new TextEdit { NewText = declaration.Name, Range = diagnostic.Range } };

                    var replaceAction = new CodeAction
                    {
                        Title = $"Rename to '{declaration.Name}'",
                        Kind = CodeActionKind.QuickFix,
                        Edit = new WorkspaceEdit { Changes = edits },
                    };
                    return replaceAction;
                })
                .Select(s => new CommandOrCodeAction(s));

            var insertDeclarationEdit = new Dictionary<DocumentUri, IEnumerable<TextEdit>>();
            var insertPosition = new Position(diagnostic.Range.Start.Line, 0);
            insertDeclarationEdit[uri] = new List<TextEdit> {
                new TextEdit {
                    NewText = $"<<declare {name} = 0>>\n", // TODO: possible to indent / space in line with other statements?
                    Range = new Range(insertPosition, insertPosition), }, };
            suggestions = suggestions.Prepend(
                new CommandOrCodeAction(
                    new CodeAction
                    {
                        Title = $"Generate variable declaration '{name}'",
                        Kind = CodeActionKind.QuickFix,
                        IsPreferred = true,
                        Edit = new WorkspaceEdit { Changes = insertDeclarationEdit },
                    }
                )
                );
            return suggestions;
        }

        private IEnumerable<CommandOrCodeAction> HandleYRNMsngCmdDef(Diagnostic diagnostic, DocumentUri uri)
        {
            if (diagnostic.Data?.HasValues != true) { return Enumerable.Empty<CommandOrCodeAction>(); }

            var functionInfo = diagnostic.Data.ToObject<(string Name, bool IsCommand)>();
            var suggestions =
                workspace.LookupFunctions(functionInfo.Name, true)
                .Where(match => match.IsCommand == functionInfo.IsCommand)
                .Take(10)
                .Select(f =>
                {
                    var edits = new Dictionary<DocumentUri, IEnumerable<TextEdit>>();
                    edits[uri] = new List<TextEdit> { new TextEdit { NewText = f.YarnName, Range = diagnostic.Range } };

                    var replaceAction = new CodeAction
                    {
                        Title = $"Rename to '{f.YarnName}'",
                        Kind = CodeActionKind.QuickFix,
                        Edit = new WorkspaceEdit { Changes = edits },
                    };
                    return replaceAction;
                })
                .Select(s => new CommandOrCodeAction(s));

            return suggestions;
        }
    }
}