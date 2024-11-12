using Microsoft.CodeAnalysis;

namespace Devlooped.Extensions.DependencyInjection;

[Generator(LanguageNames.CSharp)]
public class StaticGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(c =>
        {
            c.AddSource("ServiceAttribute.g", ThisAssembly.Resources.ServiceAttribute.Text);
            c.AddSource("ServiceAttribute`1.g", ThisAssembly.Resources.ServiceAttribute_1.Text);
        });
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var rootNs = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AddServicesNamespace", out var value) && !string.IsNullOrEmpty(value)
            ? value
            : "Microsoft.Extensions.DependencyInjection";

        var className = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AddServicesClassName", out value) && !string.IsNullOrEmpty(value) ?
            value : "AddServicesExtension";

        context.AddSource("AddServicesExtension.g", ThisAssembly.Resources.AddServicesExtension.Text
                .Replace("Devlooped.Extensions.DependencyInjection", rootNs)
                .Replace("AddServicesExtension", className));
    }
}
