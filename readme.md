![Icon](https://raw.githubusercontent.com/devlooped/DependencyInjection.Attributed/main/assets/img/icon-32.png) .NET DependencyInjection via [Service] Attribute
============

[![Version](https://img.shields.io/nuget/vpre/Devlooped.Extensions.DependencyInjection.Attributed.svg)](https://www.nuget.org/packages/Devlooped.Extensions.DependencyInjection.Attributed)
[![Downloads](https://img.shields.io/nuget/dt/Devlooped.Extensions.DependencyInjection.Attributed.svg)](https://www.nuget.org/packages/Devlooped.Extensions.DependencyInjection.Attributed)
[![License](https://img.shields.io/github/license/devlooped/DependencyInjection.Attributed.svg?color=blue)](https://github.com//devlooped/DependencyInjection.Attributed/blob/main/license.txt)
[![Build](https://github.com/devlooped/DependencyInjection.Attributed/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/DependencyInjection.Attributed/actions)

<!-- #content -->

Automatic compile-time service registrations for Microsoft.Extensions.DependencyInjection with no run-time dependencies.

## Usage

After [installing the nuget package](https://www.nuget.org/packages/Devlooped.Extensions.DependencyInjection.Attributed), 
a new `[Service(ServiceLifetime)]` attribute will be available to annotate your types:

```csharp
[Service(ServiceLifetime.Scoped)]
public class MyService : IMyService, IDisposable
{
    public string Message => "Hello World";

    public void Dispose() { }
}

public interface IMyService 
{
    string Message { get; }
}
```

The `ServiceLifetime` argument is optional and defaults to [ServiceLifetime.Singleton](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.servicelifetime?#fields).

> NOTE: The attribute is matched by simple name, so you can define your own attribute 
> in your own assembly. It only has to provide a constructor receiving a 
> [ServiceLifetime](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.servicelifetime) argument.

A source generator will emit (at compile-time) an `AddServices` extension method for 
[IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) 
which you can call from your startup code that sets up your services, like:

```csharp
var builder = WebApplication.CreateBuilder(args);

// NOTE: **Adds discovered services to the container**
builder.Services.AddServices();
// ...

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", (IMyService service) => service.Message);

// ...
app.Run();
```

> NOTE: the service is available automatically for the scoped request, because 
> we called the generated `AddServices` that registers the discovered services. 

And that's it. The source generator will discover annotated types in the current 
project and all its references too. Since the registration code is generated at 
compile-time, there is no run-time reflection (or dependencies) whatsoever.

## How It Works

The generated code that implements the registration looks like the following:

```csharp
static partial class AddServicesExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped(s => new MyService());
        services.AddScoped<IMyService>(s => s.GetRequiredService<MyService>());
        services.AddScoped<IDisposable>(s => s.GetRequiredService<MyService>());
        
        return services;
    }
```

Note how the service is registered as scoped with its own type first, and the 
other two registrations just retrieve the same (according to its defined 
lifetime). This means the instance is reused and properly registered under 
all implemented interfaces automatically.

> NOTE: you can inspect the generated code by setting `EmitCompilerGeneratedFiles=true` 
> in your project file and browsing the `generated` subfolder under `obj`.

If the service type has dependencies, they will be resolved from the service 
provider by the implementation factory too, like:

```csharp
services.AddScoped(s => new MyService(s.GetRequiredService<IMyDependency>(), ...));
```

## MEF Compatibility

Given the (more or less broad?) adoption of 
[MEF attribute](https://learn.microsoft.com/en-us/dotnet/framework/mef/attributed-programming-model-overview-mef)
(whether [.NET MEF, NuGet MEF or VS MEF](https://github.com/microsoft/vs-mef/blob/main/doc/mef_library_differences.md)) in .NET, 
the generator also supports the `[Export]` attribute to denote a service (the 
type argument as well as contract name are ignored, since those aren't supported 
in the DI container). 

In order to specify a singleton (shared) instance in MEF, you have to annotate the 
type with an extra attribute: `[Shared]` in NuGet MEF (from [System.Composition](http://nuget.org/packages/System.Composition.AttributedModel)) 
or `[PartCreationPolicy(CreationPolicy.Shared)]` in .NET MEF 
(from [System.ComponentModel.Composition](https://www.nuget.org/packages/System.ComponentModel.Composition)).


## Advanced Scenarios

### `Lazy<T>` and `Func<T>` Dependencies

A `Lazy<T>` for each interface (and main implementation) is automatically provided 
too, so you can take a lazy dependency out of the box too. In this case, the lifetime 
of the dependency `T` becomes tied to the lifetime of the component taking the lazy 
dependency, for obvious reasons. The `Lazy<T>` is merely a lazy resolving of the 
dependency via the service provider. The lazy itself isn't costly to construct, and 
since the lifetime of the underlying service, plus the lifetime of the consuming 
service determine the ultimate lifetime of the lazy, no additional configuration is 
necessary for it, as it's always registered as a transient component. Generated code 
looks like the following:

```csharp
services.AddTransient(s => new Lazy<IMyService>(s.GetRequiredService<MyService>));
```

A `Func<T>` is also automatically registered, but it is just a delegate to the 
actual `IServiceProvider.GetRequiredService<T>`. Generated code looks like the 
following:


```csharp
services.AddTransient<Func<IMyService>>(s => s.GetRequiredService<MyService>);
```

Repeatedly invoking the function will result in an instance of the required 
service that depends on the registered lifetime for it. If it was registered 
as a singleton, for example, you would get the same value every time, just 
as if you had used a dependency of `Lazy<T>` instead, but invoking the 
service provider each time, instead of only once. This makes this pattern 
more useful for transient services that you intend to use for a short time 
(and potentially dispose afterwards).


### Your Own ServiceAttribute

If you want to declare your own `ServiceAttribute` and reuse from your projects, 
so as to avoid taking a (compile-time) dependency on this package from your library 
projects, you can just declare it like so:

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute : Attribute
{
    public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton) { }
}
```

> NOTE: since the constructor argument is only used by the source generation to 
> detemine the registration style, but never at run-time, you don't even need 
> to keep it around in a field or property!

With this in place, you only need to add this package to the top-level project 
that is adding the services to the collection!

The attribute is matched by simple name, so it can exist in any namespace. 

If you want to avoid adding the attribute to the project referencing this package, 
set the `$(AddServiceAttribute)` to `true` via MSBuild:

```xml
<PropertyGroup>
  <AddServiceAttribute>false</AddServiceAttribute>
</PropertyGroup>
```

### Choose Constructor

If you want to choose a specific constructor to be used for the service implementation 
factory registration (instead of the default one which will be the one with the most 
parameters), you can annotate it with `[ImportingConstructor]` from either NuGet MEF 
([System.Composition](http://nuget.org/packages/System.Composition.AttributedModel)) 
or .NET MEF ([System.ComponentModel.Composition](https://www.nuget.org/packages/System.ComponentModel.Composition)).


### Customize Generated Class

You can customize the generated class namespace and name with the following 
MSBuild properties:

```xml
<PropertyGroup>
    <AddServicesNamespace>MyNamespace</AddServicesNamespace>
    <AddServicesClassName>MyExtensions</AddServicesClassName>
</PropertyGroup>
```

They default to `Microsoft.Extensions.DependencyInjection` and `AddServicesExtension` 
respectively.

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![C. Augusto Proiete](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/augustoproiete.png "C. Augusto Proiete")](https://github.com/augustoproiete)
[![Kirill Osenkov](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KirillOsenkov.png "Kirill Osenkov")](https://github.com/KirillOsenkov)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![SandRock](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/sandrock.png "SandRock")](https://github.com/sandrock)
[![Andy Gocke](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agocke.png "Andy Gocke")](https://github.com/agocke)
[![Stephen Shaw](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/decriptor.png "Stephen Shaw")](https://github.com/decriptor)
[![Torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek-gh.png "Torutek")](https://github.com/torutek-gh)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
