using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tests.DependencyInjection;

public class ConventionsTests(ITestOutputHelper Output)
{
    [Fact]
    public void RegisterRepositoryServices()
    {
        var conventions = new ServiceCollection();
        conventions.AddSingleton(Output);
        conventions.AddServices(typeof(IRepository));
        var services = conventions.BuildServiceProvider();

        var instance = services.GetServices<IRepository>().ToList();

        Assert.Equal(2, instance.Count);
    }

    [Fact]
    public void RegisterServiceByRegex()
    {
        var conventions = new ServiceCollection();
        conventions.AddSingleton(Output);
        conventions.AddServices(nameof(ConventionsTests), ServiceLifetime.Transient);
        var services = conventions.BuildServiceProvider();

        var instance = services.GetRequiredService<ConventionsTests>();
        var instance2 = services.GetRequiredService<ConventionsTests>();

        Assert.NotSame(instance, instance2);
    }

    [Fact]
    public void RegisterGenericServices()
    {
        var conventions = new ServiceCollection();

#pragma warning disable DDI003
        conventions.AddServices(typeof(IGenericRepository<>), ServiceLifetime.Scoped);
#pragma warning restore DDI003

        var services = conventions.BuildServiceProvider();

        var scope = services.CreateScope();
        var instance = scope.ServiceProvider.GetRequiredService<IGenericRepository<string>>();
        var instance2 = scope.ServiceProvider.GetRequiredService<IGenericRepository<int>>();

        Assert.NotNull(instance);
        Assert.NotNull(instance2);

        Assert.Same(instance, scope.ServiceProvider.GetRequiredService<IGenericRepository<string>>());
        Assert.Same(instance2, scope.ServiceProvider.GetRequiredService<IGenericRepository<int>>());
    }
}

public interface IRepository { }
public class FooRepository : IRepository { }
public class BarRepository : IRepository { }

public interface IGenericRepository<T> { }
public class FooGenericRepository : IGenericRepository<string> { }
public class BarGenericRepository : IGenericRepository<int> { }
