﻿using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains the <see cref="AddServices(IServiceCollection)"/> extension methods to register 
    /// compile-time discovered services to an <see cref="IServiceCollection"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    static partial class AddServicesNoReflectionExtension
    {
        static readonly ServiceDescriptor servicesAddedDescriptor = new ServiceDescriptor(typeof(DDIAddServicesAttribute), _ => new DDIAddServicesAttribute(), ServiceLifetime.Singleton);

        /// <summary>
        /// Adds the services that are assignable to <paramref name="assignableTo"/> to the collection, 
        /// in addition to the discovered services that were annotated with a <see cref="ServiceAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Note that NO runtime reflection is performed when using this method. A compile-time source 
        /// generator will emit the relevant registration methods for all matching types (in the current 
        /// assembly or any referenced assemblies) at build time, resulting in maximum startup performance 
        /// as well as AOT-safety.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="assignableTo">The type that services must be assignable to in order to be registered.</param>
        /// <param name="lifetime">The service lifetime to register.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        [DDIAddServices]
        public static IServiceCollection AddServices(this IServiceCollection services, Type assignableTo, ServiceLifetime lifetime = ServiceLifetime.Singleton) => services.AddServices();

        /// <summary>
        /// Adds the services that are assignable to <paramref name="assignableTo"/> to the collection, 
        /// in addition to the discovered services that were annotated with a <see cref="ServiceAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Note that NO runtime reflection is performed when using this method. A compile-time source 
        /// generator will emit the relevant registration methods for all matching types (in the current 
        /// assembly or any referenced assemblies) at build time, resulting in maximum startup performance 
        /// as well as AOT-safety.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="fullNameExpression">Regular expression to match against the full name of the type to determine if it should be registered as a service.</param>
        /// <param name="lifetime">The service lifetime to register.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        [DDIAddServices]
        public static IServiceCollection AddServices(this IServiceCollection services, string fullNameExpression, ServiceLifetime lifetime = ServiceLifetime.Singleton) => services.AddServices();

        /// <summary>
        /// Adds the services that are assignable to <paramref name="assignableTo"/> to the collection, 
        /// in addition to the discovered services that were annotated with a <see cref="ServiceAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Note that NO runtime reflection is performed when using this method. A compile-time source 
        /// generator will emit the relevant registration methods for all matching types (in the current 
        /// assembly or any referenced assemblies) at build time, resulting in maximum startup performance 
        /// as well as AOT-safety.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="assignableTo">The type that services must be assignable to in order to be registered.</param>
        /// <param name="fullNameExpression">Regular expression to match against the full name of the type to determine if it should be registered as a service, in addition to being assignable to <paramref name="assignableTo"/>.</param>
        /// <param name="lifetime">The service lifetime to register.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        [DDIAddServices]
        public static IServiceCollection AddServices(this IServiceCollection services, Type assignableTo, string fullNameExpression, ServiceLifetime lifetime = ServiceLifetime.Singleton) => services.AddServices();

        /// <summary>
        /// Adds the automatically discovered services that were annotated with a <see cref="ServiceAttribute"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        [DDIAddServices]
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            if (services.Contains(servicesAddedDescriptor))
                return services;

            AddScopedServices(services);
            AddSingletonServices(services);
            AddTransientServices(services);

            AddKeyedScopedServices(services);
            AddKeyedSingletonServices(services);
            AddKeyedTransientServices(services);

            services.Add(servicesAddedDescriptor);

            return services;
        }

        /// <summary>
        /// Adds discovered scoped services to the collection.
        /// </summary>
        static partial void AddScopedServices(IServiceCollection services);

        /// <summary>
        /// Adds discovered singleton services to the collection.
        /// </summary>
        static partial void AddSingletonServices(IServiceCollection services);

        /// <summary>
        /// Adds discovered transient services to the collection.
        /// </summary>
        static partial void AddTransientServices(IServiceCollection services);

        /// <summary>
        /// Adds discovered keyed scoped services to the collection.
        /// </summary>
        static partial void AddKeyedScopedServices(IServiceCollection services);

        /// <summary>
        /// Adds discovered keyed singleton services to the collection.
        /// </summary>
        static partial void AddKeyedSingletonServices(IServiceCollection services);

        /// <summary>
        /// Adds discovered keyed transient services to the collection.
        /// </summary>
        static partial void AddKeyedTransientServices(IServiceCollection services);

        [AttributeUsage(AttributeTargets.Method)]
        class DDIAddServicesAttribute : Attribute { }
    }
}