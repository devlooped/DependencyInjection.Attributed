using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using KeyedService = (Microsoft.CodeAnalysis.INamedTypeSymbol Type, Microsoft.CodeAnalysis.TypedConstant? Key);

namespace Devlooped.Extensions.DependencyInjection.Attributed;

/// <summary>
/// Discovers annotated services during compilation and generates the partial method 
/// implementations for <c>AddServices</c> to invoke.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class IncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.CompilationProvider.SelectMany((x, c) =>
        {
            var visitor = new TypesVisitor(s => x.IsSymbolAccessible(s), c);
            x.GlobalNamespace.Accept(visitor);
            // Also visit aliased references, which will not become part of the global:: namespace
            foreach (var symbol in x.References
                .Where(r => !r.Properties.Aliases.IsDefaultOrEmpty)
                .Select(r => x.GetAssemblyOrModuleSymbol(r)))
            {
                symbol?.Accept(visitor);
            }

            return visitor.TypeSymbols;
        });

        bool IsService(AttributeData attr) =>
            (attr.AttributeClass?.Name == "ServiceAttribute" || attr.AttributeClass?.Name == "Service") &&
            attr.ConstructorArguments.Length == 1 &&
            attr.ConstructorArguments[0].Kind == TypedConstantKind.Enum &&
            attr.ConstructorArguments[0].Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime";

        bool IsKeyedService(AttributeData attr) =>
            (attr.AttributeClass?.Name == "ServiceAttribute" || attr.AttributeClass?.Name == "Service") &&
            attr.AttributeClass?.IsGenericType == true &&
            attr.ConstructorArguments.Length == 2 &&
            attr.ConstructorArguments[1].Kind == TypedConstantKind.Enum &&
            attr.ConstructorArguments[1].Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime";

        bool IsExport(AttributeData attr)
        {
            var attrName = attr.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return attrName == "global::System.Composition.ExportAttribute" ||
                attrName == "global::System.ComponentModel.Composition.ExportAttribute";
        };

        // NOTE: we recognize the attribute by name, not precise type. This makes the generator 
        // more flexible and avoids requiring any sort of run-time dependency.
        var services = types
            .Select((x, _) =>
            {
                var name = x.Name;
                var attrs = x.GetAttributes();
                var serviceAttr = attrs.FirstOrDefault(IsService) ?? attrs.FirstOrDefault(IsKeyedService);
                var service = serviceAttr != null || attrs.Any(IsExport);

                if (!service)
                    return null;

                TypedConstant? key = default;

                // Default lifetime is singleton for [Service], Transient for MEF
                var lifetime = serviceAttr != null ? 0 : 2;
                if (serviceAttr != null)
                {
                    if (IsKeyedService(serviceAttr))
                    {
                        key = serviceAttr.ConstructorArguments[0];
                        lifetime = (int)serviceAttr.ConstructorArguments[1].Value!;
                    }
                    else
                    {
                        lifetime = (int)serviceAttr.ConstructorArguments[0].Value!;
                    }
                }
                else
                {
                    // In NuGet MEF, [Shared] makes exports singleton
                    if (attrs.Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Composition.SharedAttribute"))
                    {
                        lifetime = 0;
                    }
                    // In .NET MEF, [PartCreationPolicy(CreationPolicy.Shared)] does it.
                    else if (attrs.Any(a =>
                        a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.ComponentModel.Composition.PartCreationPolicyAttribute" &&
                        a.ConstructorArguments.Length == 1 &&
                        a.ConstructorArguments[0].Kind == TypedConstantKind.Enum &&
                        a.ConstructorArguments[0].Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.ComponentModel.Composition.CreationPolicy" &&
                        (int)a.ConstructorArguments[0].Value! == 1))
                    {
                        lifetime = 0;
                    }

                    // Consider the [Export(contractName)] as a keyed service with the contract name as the key.
                    if (attrs.FirstOrDefault(IsExport) is { } export &&
                        export.ConstructorArguments.Length > 0 &&
                        export.ConstructorArguments[0].Kind == TypedConstantKind.Primitive)
                    {
                        key = export.ConstructorArguments[0];
                    }
                }

                return new
                {
                    Type = x,
                    Key = key,
                    Lifetime = lifetime
                };
            })
            .Where(x => x != null);

        var options = context.AnalyzerConfigOptionsProvider.Combine(context.CompilationProvider);

        // Only requisite is that we define Scoped = 0, Singleton = 1 and Transient = 2.
        // This matches https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.servicelifetime?view=dotnet-plat-ext-6.0#fields

        context.RegisterImplementationSourceOutput(
            services.Where(x => x!.Lifetime == 0 && x.Key is null).Select((x, _) => new KeyedService(x!.Type, x.Key!)).Collect().Combine(options),
            (ctx, data) => AddPartial("AddSingleton", ctx, data));

        context.RegisterImplementationSourceOutput(
            services.Where(x => x!.Lifetime == 1 && x.Key is null).Select((x, _) => new KeyedService(x!.Type, x.Key!)).Collect().Combine(options),
            (ctx, data) => AddPartial("AddScoped", ctx, data));

        context.RegisterImplementationSourceOutput(
            services.Where(x => x!.Lifetime == 2 && x.Key is null).Select((x, _) => new KeyedService(x!.Type, x.Key!)).Collect().Combine(options),
            (ctx, data) => AddPartial("AddTransient", ctx, data));

        context.RegisterImplementationSourceOutput(
            services.Where(x => x!.Lifetime == 0 && x.Key is not null).Select((x, _) => new KeyedService(x!.Type, x.Key!)).Collect().Combine(options),
            (ctx, data) => AddPartial("AddKeyedSingleton", ctx, data));

        context.RegisterImplementationSourceOutput(
            services.Where(x => x!.Lifetime == 1 && x.Key is not null).Select((x, _) => new KeyedService(x!.Type, x.Key!)).Collect().Combine(options),
            (ctx, data) => AddPartial("AddKeyedScoped", ctx, data));

        context.RegisterImplementationSourceOutput(
            services.Where(x => x!.Lifetime == 2 && x.Key is not null).Select((x, _) => new KeyedService(x!.Type, x.Key!)).Collect().Combine(options),
            (ctx, data) => AddPartial("AddKeyedTransient", ctx, data));
    }

    void AddPartial(string methodName, SourceProductionContext ctx, (ImmutableArray<KeyedService> Types, (AnalyzerConfigOptionsProvider Config, Compilation Compilation) Options) data)
    {
        var builder = new StringBuilder()
            .AppendLine("// <auto-generated />");

        var rootNs = data.Options.Config.GlobalOptions.TryGetValue("build_property.AddServicesNamespace", out var value) && !string.IsNullOrEmpty(value)
            ? value
            : "Microsoft.Extensions.DependencyInjection";

        var className = data.Options.Config.GlobalOptions.TryGetValue("build_property.AddServicesClassName", out value) && !string.IsNullOrEmpty(value) ?
            value : "AddServicesExtension";

        foreach (var alias in data.Options.Compilation.References.SelectMany(r => r.Properties.Aliases))
        {
            builder.AppendLine($"extern alias {alias};");
        }

        builder.AppendLine(
          $$"""
            using Microsoft.Extensions.DependencyInjection;
            using System;
            
            namespace {{rootNs}}
            {
                static partial class {{className}}
                {
                    static partial void {{methodName}}Services(IServiceCollection services)
                    {
            """);

        AddServices(data.Types.Where(x => x.Key is null).Select(x => x.Type), data.Options.Compilation, methodName, builder);
        AddKeyedServices(data.Types.Where(x => x.Key is not null), data.Options.Compilation, methodName, builder);

        builder.AppendLine(
        """
                    }
                }
            }
            """);

        ctx.AddSource(methodName + ".g", builder.ToString().Replace("\r\n", "\n").Replace("\n", Environment.NewLine));
    }

    void AddServices(IEnumerable<INamedTypeSymbol> services, Compilation compilation, string methodName, StringBuilder output)
    {
        bool isAccessible(ISymbol s) => compilation.IsSymbolAccessible(s);

        foreach (var type in services)
        {
            var impl = type.ToFullName(compilation);
            var registered = new HashSet<string>();

            var importing = type.InstanceConstructors.FirstOrDefault(m =>
                m.GetAttributes().Any(a =>
                    a.AttributeClass?.ToFullName(compilation) == "global::System.Composition.ImportingConstructorAttribute" ||
                    a.AttributeClass?.ToFullName(compilation) == "global::System.ComponentModel.Composition.ImportingConstructorAttribute"));

            var ctor = importing ?? type.InstanceConstructors
                .Where(isAccessible)
                .OrderByDescending(m => m.Parameters.Length)
                .FirstOrDefault();

            if (ctor != null && ctor.Parameters.Length > 0)
            {
                var args = string.Join(", ", ctor.Parameters.Select(p =>
                {
                    var fromKeyed = p.GetAttributes().FirstOrDefault(IsFromKeyed);
                    if (fromKeyed is not null)
                        return $"s.GetRequiredKeyedService<{p.Type.ToFullName(compilation)}>({fromKeyed.ConstructorArguments[0].ToCSharpString()})";

                    return $"s.GetRequiredService<{p.Type.ToFullName(compilation)}>()";
                }));
                output.AppendLine($"            services.{methodName}(s => new {impl}({args}));");
            }
            else
            {
                output.AppendLine($"            services.{methodName}(s => new {impl}());");
            }

            output.AppendLine($"            services.AddTransient<Func<{impl}>>(s => s.GetRequiredService<{impl}>);");
            output.AppendLine($"            services.AddTransient(s => new Lazy<{impl}>(s.GetRequiredService<{impl}>));");

            foreach (var iface in type.AllInterfaces)
            {
                var ifaceName = iface.ToFullName(compilation);
                if (!registered.Contains(ifaceName))
                {
                    output.AppendLine($"            services.{methodName}<{ifaceName}>(s => s.GetRequiredService<{impl}>());");
                    output.AppendLine($"            services.AddTransient<Func<{ifaceName}>>(s => s.GetRequiredService<{ifaceName}>);");
                    output.AppendLine($"            services.AddTransient(s => new Lazy<{ifaceName}>(s.GetRequiredService<{ifaceName}>));");
                    registered.Add(ifaceName);
                }

                // Register covariant interfaces too, for at most one type parameter.
                // TODO: perhaps explore registering for the full permutation of all out params?
                if (iface.IsGenericType &&
                    iface.TypeParameters.Length == 1 &&
                    iface.TypeParameters[0].Variance == VarianceKind.Out)
                {
                    var typeParam = iface.TypeArguments[0];
                    var candidates = typeParam.AllInterfaces.ToList();
                    var baseType = typeParam.BaseType;
                    while (baseType != null && baseType.SpecialType != SpecialType.System_Object)
                    {
                        candidates.Add(baseType);
                        baseType = baseType.BaseType;
                    }

                    foreach (var candidate in candidates.Select(x => iface.ConstructedFrom.Construct(x))
                        .ToImmutableHashSet(SymbolEqualityComparer.Default)
                        .Where(x => x != null)
                        .Select(x => x!.ToFullName(compilation)))
                    {
                        if (!registered.Contains(candidate))
                        {
                            output.AppendLine($"            services.{methodName}<{candidate}>(s => s.GetRequiredService<{impl}>());");
                            output.AppendLine($"            services.AddTransient<Func<{candidate}>>(s => s.GetRequiredService<{candidate}>);");
                            output.AppendLine($"            services.AddTransient(s => new Lazy<{candidate}>(s.GetRequiredService<{candidate}>));");
                            registered.Add(candidate);
                        }
                    }
                }
            }
        }
    }

    void AddKeyedServices(IEnumerable<KeyedService> services, Compilation compilation, string methodName, StringBuilder output)
    {
        bool isAccessible(ISymbol s) => compilation.IsSymbolAccessible(s);

        foreach (var type in services)
        {
            var impl = type.Type.ToFullName(compilation);
            var registered = new HashSet<string>();
            var key = type.Key!.Value.ToCSharpString();

            var importing = type.Type.InstanceConstructors.FirstOrDefault(m =>
                m.GetAttributes().Any(a =>
                    a.AttributeClass?.ToFullName(compilation) == "global::System.Composition.ImportingConstructorAttribute" ||
                    a.AttributeClass?.ToFullName(compilation) == "global::System.ComponentModel.Composition.ImportingConstructorAttribute"));

            var ctor = importing ?? type.Type.InstanceConstructors
                .Where(isAccessible)
                .OrderByDescending(m => m.Parameters.Length)
                .FirstOrDefault();

            if (ctor != null && ctor.Parameters.Length > 0)
            {
                var args = string.Join(", ", ctor.Parameters.Select(p =>
                {
                    var fromKeyed = p.GetAttributes().FirstOrDefault(IsFromKeyed);
                    if (fromKeyed is not null)
                        return $"s.GetRequiredKeyedService<{p.Type.ToFullName(compilation)}>({fromKeyed.ConstructorArguments[0].ToCSharpString()})";

                    return $"s.GetRequiredService<{p.Type.ToFullName(compilation)}>()";
                }));
                output.AppendLine($"            services.{methodName}({key}, (s, k) => new {impl}({args}));");
            }
            else
            {
                output.AppendLine($"            services.{methodName}({key}, (s, k) => new {impl}());");
            }

            output.AppendLine($"            services.AddKeyedTransient<Func<{impl}>>({key}, (s, k) => () => s.GetRequiredKeyedService<{impl}>(k));");
            output.AppendLine($"            services.AddKeyedTransient({key}, (s, k) => new Lazy<{impl}>(s.GetRequiredKeyedService<{impl}>(k)));");

            foreach (var iface in type.Type.AllInterfaces)
            {
                var ifaceName = iface.ToFullName(compilation);
                if (!registered.Contains(ifaceName))
                {
                    output.AppendLine($"            services.{methodName}<{ifaceName}>({key}, (s, k) => s.GetRequiredKeyedService<{impl}>(k));");
                    output.AppendLine($"            services.AddKeyedTransient<Func<{ifaceName}>>({key}, (s, k) => () => s.GetRequiredKeyedService<{ifaceName}>(k));");
                    output.AppendLine($"            services.AddKeyedTransient({key}, (s, k) => new Lazy<{ifaceName}>(s.GetRequiredKeyedService<{ifaceName}>(k)));");
                    registered.Add(ifaceName);
                }

                // Register covariant interfaces too, for at most one type parameter.
                // TODO: perhaps explore registering for the full permutation of all out params?
                if (iface.IsGenericType &&
                    iface.TypeParameters.Length == 1 &&
                    iface.TypeParameters[0].Variance == VarianceKind.Out)
                {
                    var typeParam = iface.TypeArguments[0];
                    var candidates = typeParam.AllInterfaces.ToList();
                    var baseType = typeParam.BaseType;
                    while (baseType != null && baseType.SpecialType != SpecialType.System_Object)
                    {
                        candidates.Add(baseType);
                        baseType = baseType.BaseType;
                    }

                    foreach (var candidate in candidates.Select(x => iface.ConstructedFrom.Construct(x))
                        .ToImmutableHashSet(SymbolEqualityComparer.Default)
                        .Where(x => x != null)
                        .Select(x => x!.ToFullName(compilation)))
                    {
                        if (!registered.Contains(candidate))
                        {
                            output.AppendLine($"            services.{methodName}<{candidate}>({key}, (s, k) => s.GetRequiredKeyedService<{impl}>(k));");
                            output.AppendLine($"            services.AddKeyedTransient<Func<{candidate}>>({key}, (s, k) => () => s.GetRequiredKeyedService<{candidate}>(k));");
                            output.AppendLine($"            services.AddKeyedTransient({key}, (s, k) => new Lazy<{candidate}>(k, s.GetRequiredKeyedService<{candidate}>(k)));");
                            registered.Add(candidate);
                        }
                    }
                }
            }
        }
    }

    bool IsFromKeyed(AttributeData attr)
    {
        var attrName = attr.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return attrName == "global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute" ||
            (attrName == "global::System.ComponentModel.Composition.ImportAttribute" &&
             // In this case, the Import attribute ctor can only have a primitive string value, not enum.
             attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Kind == TypedConstantKind.Primitive);
    }


    class TypesVisitor : SymbolVisitor
    {
        Func<ISymbol, bool> isAccessible;
        CancellationToken cancellation;
        HashSet<INamedTypeSymbol> types = new(SymbolEqualityComparer.Default);

        public TypesVisitor(Func<ISymbol, bool> isAccessible, CancellationToken cancellation)
        {
            this.isAccessible = isAccessible;
            this.cancellation = cancellation;
        }

        public HashSet<INamedTypeSymbol> TypeSymbols => types;

        public override void VisitAlias(IAliasSymbol symbol)
        {
            base.VisitAlias(symbol);
        }

        public override void VisitModule(IModuleSymbol symbol)
            => base.VisitModule(symbol);

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            cancellation.ThrowIfCancellationRequested();
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var namespaceOrType in symbol.GetMembers())
            {
                cancellation.ThrowIfCancellationRequested();
                namespaceOrType.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol type)
        {
            cancellation.ThrowIfCancellationRequested();

            if (!isAccessible(type) || !types.Add(type))
                return;

            var nestedTypes = type.GetTypeMembers();
            if (nestedTypes.IsDefaultOrEmpty)
                return;

            foreach (var nestedType in nestedTypes)
            {
                cancellation.ThrowIfCancellationRequested();
                nestedType.Accept(this);
            }
        }
    }
}
