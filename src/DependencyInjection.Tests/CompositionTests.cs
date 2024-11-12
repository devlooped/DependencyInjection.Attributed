using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Composition;

public class CompositionTests
{
    [Fact]
    public void CompositionLifetimes()
    {
        var config = new ContainerConfiguration().WithAssembly(Assembly.GetExecutingAssembly());
        var container = config.CreateContainer();

        var singleton = container.GetExport<SingletonService>();
        var transient = container.GetExport<TransientService>();

        Assert.Same(singleton, transient.Singleton);
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
        Assert.Same(instance, services.GetRequiredService<ICompositionSingleton>());
    }

    [Fact]
    public void RegisterTransientService()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var instance = services.GetRequiredService<TransientService>();

        Assert.NotSame(instance, services.GetRequiredService<TransientService>());
        Assert.NotSame(instance, services.GetRequiredService<ICompositionTransient>());
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

public interface ICompositionSingleton { }
public interface ICompositionTransient { }

[Shared]
[Export]
[Export(typeof(ICompositionSingleton))]
public class SingletonService : ICompositionSingleton
{
}

[Export]
[Export(typeof(ICompositionTransient))]
public class TransientService : ICompositionTransient
{
    public TransientService(SingletonService singleton, CancellationTokenSource source) => Singleton = singleton;

    [ImportingConstructor]
    public TransientService(ICompositionSingleton singleton) => Singleton = singleton;
    public ICompositionSingleton Singleton { get; }
}