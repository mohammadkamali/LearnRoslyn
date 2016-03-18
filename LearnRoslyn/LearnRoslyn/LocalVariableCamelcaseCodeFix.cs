using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Options;

namespace LearnRoslyn
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LocalVariableCamelcaseCodeFix)), Shared]
    public class LocalVariableCamelcaseCodeFix : CodeFixProvider
    {
        private const string title = "Make Camelcase Format";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(LocalVariableCamelcaseNamingAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();

            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            LocalDeclarationStatementSyntax localDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c =>
                    MakeCamelcase(context.Document, localDeclaration, c),
                    equivalenceKey: title),
                diagnostic);
        }



        private async Task<Solution> MakeCamelcase(Document document, LocalDeclarationStatementSyntax localDeclaration, CancellationToken cancellationToken)
        {
            VariableDeclaratorSyntax variable = localDeclaration.Declaration.Variables.First();

            SyntaxToken identifierToken = variable.Identifier;

            string newName = identifierToken.Text.Replace(identifierToken.Text[0], ((char)(identifierToken.Text[0] + 32)));

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            ISymbol typeSymbol = semanticModel.GetDeclaredSymbol(variable, cancellationToken);

            Solution originalSolution = document.Project.Solution;

            OptionSet optionSet = originalSolution.Workspace.Options;

            Solution newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            return newSolution;
        }
    }
}