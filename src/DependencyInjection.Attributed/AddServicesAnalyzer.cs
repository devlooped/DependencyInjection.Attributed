using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Devlooped.Extensions.DependencyInjection.Attributed;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public class AddServicesAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor NoAddServicesCall { get; } =
        new DiagnosticDescriptor(
        "DDI001",
        "No call to IServiceCollection.AddServices found.",
        "The AddServices extension method must be invoked in order for discovered services to be properly registered.",
        "Build",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(NoAddServicesCall);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        var usesServiceCollection = false;
        var usesAddServices = false;

        context.RegisterCompilationStartAction(startContext =>
        {
            var servicesCollection = startContext.Compilation.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.IServiceCollection");
            if (servicesCollection == null)
                return;

            Location? location = default;

            startContext.RegisterSemanticModelAction(semanticContext =>
            {
                var semantic = semanticContext.SemanticModel;
                var invocations = semantic.SyntaxTree
                    .GetRoot(semanticContext.CancellationToken)
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Select(invocation => new { Invocation = invocation, semantic.GetSymbolInfo(invocation, semanticContext.CancellationToken).Symbol })
                    .Where(x => x.Symbol is IMethodSymbol)
                    .Select(x => new { x.Invocation, Method = (IMethodSymbol)x.Symbol! });

                bool IsServiceCollectionExtension(IMethodSymbol method) => method.IsExtensionMethod &&
                        method.ReducedFrom != null &&
                        method.ReducedFrom.Parameters.Length > 0 &&
                        method.ReducedFrom.Parameters[0].Type.Equals(servicesCollection, SymbolEqualityComparer.Default);

                if (!usesServiceCollection &&
                    invocations.Where(x => x.Method.ContainingType.Is(servicesCollection) || IsServiceCollectionExtension(x.Method))
                    .FirstOrDefault() is { } invocation)
                {
                    // Remove diagnostic, we found an invocation
                    usesServiceCollection = true;
                    if (location == null)
                        location = invocation.Invocation.GetLocation();
                }

                if (!usesAddServices &&
                    invocations.Where(x =>
                        IsServiceCollectionExtension(x.Method) &&
                        x.Method.ReducedFrom!.GetAttributes().Any(attr => attr.AttributeClass?.Name == "DDIAddServicesAttribute"))
                    .Any())
                {
                    usesAddServices = true;
                }
            });

            startContext.RegisterCompilationEndAction(endContext =>
            {
                if (usesServiceCollection && !usesAddServices)
                    endContext.ReportDiagnostic(Diagnostic.Create(NoAddServicesCall, location));
            });
        });
    }
}
