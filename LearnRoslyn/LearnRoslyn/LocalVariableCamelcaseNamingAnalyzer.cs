using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LearnRoslyn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalVariableCamelcaseNamingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LearnRoslyn";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "CamelcaseNaming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LocalDeclarationStatement);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext obj)
        {

            LocalDeclarationStatementSyntax localDeclaration = (LocalDeclarationStatementSyntax)obj.Node;

            foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
            {

                if (!(char.IsLower(variable.Identifier.ValueText[0])))
                {
                    obj.ReportDiagnostic(Diagnostic.Create(Rule, obj.Node.GetLocation()));
                }

            }

        }

    }
}
