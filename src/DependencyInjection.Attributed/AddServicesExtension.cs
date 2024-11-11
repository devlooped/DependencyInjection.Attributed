using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Devlooped.Extensions.DependencyInjection.Attributed
{
    /// <summary>
    /// Contains the <see cref="AddServices(IServiceCollection)"/> extension methods to register 
    /// compile-time discovered services to an <see cref="IServiceCollection"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    static partial class AddServicesExtension
    {
        /// <summary>
        /// Adds the automatically discovered services that were annotated with a <see cref="ServiceAttribute"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        [DDIAddServices]
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            AddScopedServices(services);
            AddSingletonServices(services);
            AddTransientServices(services);

            AddKeyedScopedServices(services);
            AddKeyedSingletonServices(services);
            AddKeyedTransientServices(services);

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