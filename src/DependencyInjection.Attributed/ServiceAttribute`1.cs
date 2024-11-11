﻿// <auto-generated />
#if DDI_ADDSERVICE
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configures the registration of a keyed service in an <see cref="IServiceCollection"/>.
    /// Requires v8 or later of Microsoft.Extensions.DependencyInjection package.
    /// </summary>
    /// <typeparam name="TKey">Type of service key.</typeparam>
    [AttributeUsage(AttributeTargets.Class)]
    partial class ServiceAttribute<TKey> : Attribute
    {
        /// <summary>
        /// Annotates the service with the lifetime.
        /// </summary>
        public ServiceAttribute(TKey key, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            => (Key, Lifetime)
            = (key, lifetime);

        /// <summary>
        /// The key used to register the service in an <see cref="IServiceCollection"/>.
        /// </summary>
        public TKey Key { get; }
                
        /// <summary>
        /// <see cref="ServiceLifetime"/> associated with a registered service 
        /// in an <see cref="IServiceCollection"/>.
        /// </summary>
        public ServiceLifetime Lifetime { get; }
    }
}
#endif