using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection;

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
        Assert.Same(instance, services.GetRequiredService<IFormattable>());
    }

    [Fact]
    public void RegisterTransientService()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var services = collection.BuildServiceProvider();

        var instance = services.GetRequiredService<TransientService>();

        Assert.NotSame(instance, services.GetRequiredService<TransientService>());
        Assert.NotSame(instance, services.GetRequiredService<ICloneable>());
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
        Assert.Same(instance, scope.ServiceProvider.GetRequiredService<IComparable>());

        // Outside the scope, we don't
        Assert.NotSame(instance, services.GetRequiredService<IComparable>());
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
    public int CompareTo(object? obj) => throw new NotImplementedException();
    public void Dispose() { }
}
