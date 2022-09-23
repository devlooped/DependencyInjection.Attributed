using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configures the registration of a service in an <see cref="IServiceCollection"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// Annotates the service with the lifetime.
        /// </summary>
        public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            => Lifetime = lifetime;

        /// <summary>
        /// <see cref="ServiceLifetime"/> associated with a registered service 
        /// in an <see cref="IServiceCollection"/>.
        /// </summary>
        public ServiceLifetime Lifetime { get; }
    }
}