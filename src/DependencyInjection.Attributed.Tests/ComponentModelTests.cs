using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.ComponentModel;

public class ComponentModelTests
{
    [Fact]
    public void Composition()
    {
        var container = new CompositionContainer(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

        var singleton = container.GetExportedValue<SingletonService>();
        var transient = container.GetExportedValue<TransientService>();

        Assert.Same(singleton, transient?.Singleton);
        Assert.NotSame(transient, container.GetExport<TransientService>());
    }

    [Fact]
    public void RegisterSingletonService()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var instance = services.GetRequiredService<SingletonService>();

        Assert.Same(instance, services.GetRequiredService<SingletonService>());
        Assert.Same(instance, services.GetRequiredService<IMefSingleton>());
    }

    [Fact]
    public void RegisterTransientService()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var instance = services.GetRequiredService<TransientService>();

        Assert.NotSame(instance, services.GetRequiredService<TransientService>());
        Assert.NotSame(instance, services.GetRequiredService<IMefTransient>());
    }

    [Fact]
    public void ResolvesDependency()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var singleton = services.GetRequiredService<SingletonService>();

        using var scope = services.CreateScope();

        var instance = scope.ServiceProvider.GetRequiredService<TransientService>();

        Assert.Same(singleton, instance.Singleton);
    }
}

public interface IMefSingleton { }

[PartCreationPolicy(CreationPolicy.Shared)]
[Export]
public class SingletonService : IMefSingleton
{
}

public interface IMefTransient { }

[Export]
[Export(typeof(IMefTransient))]
public class TransientService : IMefTransient
{
    public TransientService(SingletonService singleton, CancellationTokenSource source) => Singleton = singleton;

    [ImportingConstructor]
    public TransientService(SingletonService singleton) => Singleton = singleton;
    public SingletonService Singleton { get; }
}