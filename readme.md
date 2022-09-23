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
by default a new `[Service(ServiceLifetime)]` attribute will be available to annotate your types:

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

A source generator will emit (at compile-time) an `AddServices` extension method for 
[IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) 
which you can call from your startup code that sets up your services, like:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add discovered services to the container.
builder.Services.AddServices();
// ...

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", (IMyService service) => service.Message);

// ...
app.Run();
```

> NOTE: the service is available automatically for the scoped request.

And that's it. The source generator will discover annotated types in the current 
project and all its references too. Since the registration code is generated at 
compile-time, there is no run-time reflection (or dependencies) whatsoever.

## How It Works

The generated code that implements the registration looks like the following:

```csharp
services.AddScoped<MyService>();
services.AddScoped<IMyService>(s => s.GetRequiredService<MyService>());
services.AddScoped<IDisposable>(s => s.GetRequiredService<MyService>());
```

Note how the service is registered as scoped with its own type first, and the 
other two registrations just retrieve the same (according to its defined 
lifetime). This means the instance is reused and properly registered under 
all implemented interfaces automatically.

> NOTE: you can inspect the generated code by setting `EmitCompilerGeneratedFiles=true` 
> in your project file and browsing the `generated` subfolder under `obj`.

A linked source file named `ServiceAttribute` is also added to the project by 
the nuget package, but the source generator does not require it, since it matches 
the annotation by attribute name.

## Advanced Scenarios

### Your Own ServiceAttribute

If you want to declare your own `ServiceAttribute` and reuse from your projects, 
you can do it too. Just exclude the automatic `contentFiles` that the package 
reference contributes to your project:

```xml
  <ItemGroup>
    <PackageReference Include="Devlooped.Extensions.DependencyInjection.Attributed" ExcludeAssets="contentFiles" Version="..." />
  </ItemGroup>
```

This will not add the attribute to the project. You can now create the attribute 
in your own shared library project like so:

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute : Attribute
{
    public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton) { }
}
```

> NOTE: since the constructor argument is only used by the source generation to 
> detemine the registration style, but never at run-time, you don't even need 
> to keep it in a field or property!

With this in place, you only need to add the package to the top-level project 
that is adding the services to the collection!

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
[![Christian Findlay](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MelbourneDeveloper.png "Christian Findlay")](https://github.com/MelbourneDeveloper)
[![C. Augusto Proiete](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/augustoproiete.png "C. Augusto Proiete")](https://github.com/augustoproiete)
[![Kirill Osenkov](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KirillOsenkov.png "Kirill Osenkov")](https://github.com/KirillOsenkov)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![SandRock](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/sandrock.png "SandRock")](https://github.com/sandrock)
[![Eric C](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eeseewy.png "Eric C")](https://github.com/eeseewy)
[![Andy Gocke](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agocke.png "Andy Gocke")](https://github.com/agocke)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
