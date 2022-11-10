using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Devlooped.Extensions.DependencyInjection.Attributed;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Abstractions;
using Test = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<Devlooped.Extensions.DependencyInjection.Attributed.AddServicesAnalyzer, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Devlooped.Extensions.DependencyInjection.Attributed.AddServicesAnalyzer, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

namespace Tests.CodeAnalysis;

public record AddServicesAnalyzerTests(ITestOutputHelper Output)
{
    [Fact]
    public async Task NoWarningIfAddServicesPresent()
    {
        var test = new Test
        {
            TestCode = """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            
            public record Command;
            
            public static class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();
                    services.AddServices();
                }
            }
            """,
            TestState =
            {
                ReferenceAssemblies = new ReferenceAssemblies(
                    "net6.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref", "6.0.0"),
                        Path.Combine("ref", "net6.0"))
                    .AddPackages(ImmutableArray.Create(
                        new PackageIdentity("Microsoft.Extensions.DependencyInjection", "6.0.0")))
            },
            CompilationTransforms =
            {
                (compilation, _) =>
                {
                    CSharpGeneratorDriver
                        .Create(new StaticGenerator())
                        .RunGeneratorsAndUpdateCompilation(compilation, out var staticPass, out var _);

                    CSharpGeneratorDriver
                        .Create(new IncrementalGenerator())
                        .RunGeneratorsAndUpdateCompilation(staticPass, out var incrementalPass, out var _);

                    return incrementalPass;
                }
            }
        };

        //var expected = Verifier.Diagnostic(AddServicesAnalyzer.NoAddServicesCall).WithLocation(0);

        //test.ExpectedDiagnostics.Add(expected);
        //test.ExpectedDiagnostics.Add(new DiagnosticResult("CS1503", DiagnosticSeverity.Error).WithLocation(0));

        await test.RunAsync();
    }

    [Fact]
    public async Task NoWarningIfNoServiceCollectionCalls()
    {
        var test = new Test
        {
            TestCode = """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            
            public record Command;
            
            public static class Program
            {
                public static void Main()
                {
                    Console.WriteLine("Hello World!");
                }
            }
            """,
            TestState =
            {
                ReferenceAssemblies = new ReferenceAssemblies(
                    "net6.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref", "6.0.0"),
                        Path.Combine("ref", "net6.0"))
                    .AddPackages(ImmutableArray.Create(
                        new PackageIdentity("Microsoft.Extensions.DependencyInjection.Abstractions", "6.0.0")))
            },
            CompilationTransforms =
            {
                (compilation, _) =>
                {
                    CSharpGeneratorDriver
                        .Create(new StaticGenerator())
                        .RunGeneratorsAndUpdateCompilation(compilation, out var staticPass, out var _);

                    CSharpGeneratorDriver
                        .Create(new IncrementalGenerator())
                        .RunGeneratorsAndUpdateCompilation(staticPass, out var incrementalPass, out var _);

                    return incrementalPass;
                }
            }
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task WarnIfAddServicesMissing()
    {
        var test = new Test
        {
            TestCode = """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            
            public record Command;
            
            public static class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();
                    {|#0:services.AddSingleton(new Command())|};
                }
            }
            """,
            TestState =
            {
                ReferenceAssemblies = new ReferenceAssemblies(
                    "net6.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref", "6.0.0"),
                        Path.Combine("ref", "net6.0"))
                    .AddPackages(ImmutableArray.Create(
                        new PackageIdentity("Microsoft.Extensions.DependencyInjection", "6.0.0")))
            },
            CompilationTransforms =
            {
                (compilation, _) =>
                {
                    CSharpGeneratorDriver
                        .Create(new StaticGenerator())
                        .RunGeneratorsAndUpdateCompilation(compilation, out var staticPass, out var _);

                    CSharpGeneratorDriver
                        .Create(new IncrementalGenerator())
                        .RunGeneratorsAndUpdateCompilation(staticPass, out var incrementalPass, out var _);

                    return incrementalPass;
                }
            }
        };

        var expected = Verifier.Diagnostic(AddServicesAnalyzer.NoAddServicesCall).WithLocation(0);
        test.ExpectedDiagnostics.Add(expected);

        await test.RunAsync();
    }

    [Fact]
    public async Task WarnIfAddServicesMissingMultipleLocations()
    {
        var test = new Test
        {
            TestCode = """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            
            public record First;
            public record Second;
            
            public static class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();
                    {|#0:services.AddSingleton(new First())|};
                    services.AddSingleton(new Second());
                }
            }
            """,
            TestState =
            {
                ReferenceAssemblies = new ReferenceAssemblies(
                    "net6.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref", "6.0.0"),
                        Path.Combine("ref", "net6.0"))
                    .AddPackages(ImmutableArray.Create(
                        new PackageIdentity("Microsoft.Extensions.DependencyInjection", "6.0.0")))
            },
            CompilationTransforms =
            {
                (compilation, _) =>
                {
                    CSharpGeneratorDriver
                        .Create(new StaticGenerator())
                        .RunGeneratorsAndUpdateCompilation(compilation, out var staticPass, out var _);

                    CSharpGeneratorDriver
                        .Create(new IncrementalGenerator())
                        .RunGeneratorsAndUpdateCompilation(staticPass, out var incrementalPass, out var _);

                    return incrementalPass;
                }
            }
        };

        var expected = Verifier.Diagnostic(AddServicesAnalyzer.NoAddServicesCall).WithLocation(0);
        test.ExpectedDiagnostics.Add(expected);

        await test.RunAsync();
    }
}
