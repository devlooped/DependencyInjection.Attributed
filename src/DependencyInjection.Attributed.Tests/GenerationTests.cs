using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.DependencyInjection;

public record GenerationTests(ITestOutputHelper Output)
{
    [Fact]
    public void RegisterSingletonService()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var instance = services.GetRequiredService<SingletonService>();

        Assert.Same(instance, services.GetRequiredService<SingletonService>());
        Assert.Same(instance, services.GetRequiredService<Func<SingletonService>>().Invoke());
        Assert.Same(instance, services.GetRequiredService<Lazy<SingletonService>>().Value);

        Assert.Same(instance, services.GetRequiredService<IFormattable>());
        Assert.Same(instance, services.GetRequiredService<Func<IFormattable>>().Invoke());
        Assert.Same(instance, services.GetRequiredService<Lazy<IFormattable>>().Value);
    }

    [Fact]
    public void RegisterTransientService()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var instance = services.GetRequiredService<TransientService>();

        Assert.NotSame(instance, services.GetRequiredService<TransientService>());
        Assert.NotSame(instance, services.GetRequiredService<Func<TransientService>>().Invoke());
        Assert.NotSame(instance, services.GetRequiredService<Lazy<TransientService>>().Value);

        Assert.NotSame(instance, services.GetRequiredService<ICloneable>());
        Assert.NotSame(instance, services.GetRequiredService<Func<ICloneable>>().Invoke());
        Assert.NotSame(instance, services.GetRequiredService<Lazy<ICloneable>>().Value);
    }

    [Fact]
    public void RegisterScopedService()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        using var scope = services.CreateScope();

        var instance = scope.ServiceProvider.GetRequiredService<ScopedService>();

        // Within the scope, we get the same instance
        Assert.Same(instance, scope.ServiceProvider.GetRequiredService<ScopedService>());
        Assert.Same(instance, scope.ServiceProvider.GetRequiredService<Func<ScopedService>>().Invoke());
        Assert.Same(instance, scope.ServiceProvider.GetRequiredService<Lazy<ScopedService>>().Value);

        Assert.Same(instance, scope.ServiceProvider.GetRequiredService<IComparable>());
        Assert.Same(instance, scope.ServiceProvider.GetRequiredService<Func<IComparable>>().Invoke());
        Assert.Same(instance, scope.ServiceProvider.GetRequiredService<Lazy<IComparable>>().Value);

        // Outside the scope, we don't
        Assert.NotSame(instance, services.GetRequiredService<IComparable>());
        Assert.NotSame(instance, services.GetRequiredService<Func<IComparable>>().Invoke());
        Assert.NotSame(instance, services.GetRequiredService<Lazy<IComparable>>().Value);
    }

    [Fact]
    public void ResolvesDependency()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var singleton = services.GetRequiredService<SingletonService>();

        using var scope = services.CreateScope();

        var instance = scope.ServiceProvider.GetRequiredService<ScopedService>();

        // Within the scope, we get the same instance
        Assert.Same(singleton, instance.Singleton);
    }

    [Fact]
    public void RetrieveMany()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var instances = services.GetServices<IDisposable>();

        Assert.Equal(3, instances.Count());
    }

    [Fact]
    public void RegisterWithGenericOutParameterHierarchy()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        Assert.NotNull(services.GetService<IObservable<MyEvent>>());
        Assert.NotNull(services.GetService<IObservable<BaseEvent>>());
        Assert.NotNull(services.GetService<IObservable<IEvent>>());
        Assert.Null(services.GetService<IObservable<object>>());
    }

    [Fact]
    public void RegisterWithCustomServiceAttribute()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var instance = services.GetRequiredService<MyAttributedService>();

        Assert.Same(instance, services.GetRequiredService<MyAttributedService>());
        Assert.Same(instance, services.GetRequiredService<IAsyncDisposable>());
    }

    [GenerationTests.Service(ServiceLifetime.Singleton)]
    public class MyAttributedService : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => throw new NotImplementedException();
    }

    class ServiceAttribute : Attribute
    {
        public ServiceAttribute(ServiceLifetime lifetime) { }
    }
}

[Service(ServiceLifetime.Singleton)]
public class SingletonService : IDisposable, IFormattable
{
    public void Dispose() => throw new NotImplementedException();
    public string ToString(string? format, IFormatProvider? formatProvider) => throw new NotImplementedException();
}

[Service(ServiceLifetime.Transient)]
public class TransientService : ICloneable, IDisposable
{
    public object Clone() => throw new NotImplementedException();
    public void Dispose() => throw new NotImplementedException();
}

[Service(ServiceLifetime.Scoped)]
public class ScopedService : IComparable, IDisposable
{
    public ScopedService(SingletonService singleton) => Singleton = singleton;
    // Will not be picked up because it's not accessible from within the assembly
    // If it were internal, it would be the selected one instead (most args)
    protected ScopedService(SingletonService singleton, CancellationTokenSource source) => Singleton = singleton;
    public SingletonService Singleton { get; }
    public int CompareTo(object? obj) => throw new NotImplementedException();
    public void Dispose() { }
}

public interface IEvent { }
public class BaseEvent : IEvent { }
public class MyEvent : BaseEvent { }
[Service]
public class ObservableService : IObservable<MyEvent>
{
    public IDisposable Subscribe(IObserver<MyEvent> observer) => throw new NotImplementedException();
}