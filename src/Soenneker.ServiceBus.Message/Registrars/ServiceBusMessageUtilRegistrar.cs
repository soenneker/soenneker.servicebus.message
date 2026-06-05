using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.ServiceBus.Message.Abstract;

namespace Soenneker.ServiceBus.Message.Registrars;

/// <summary>
/// A utility library for building Azure Service messages
/// </summary>
public static class ServiceBusMessageUtilRegistrar
{
    /// <summary>
    /// Adds service bus message util as singleton.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The result of the operation.</returns>
    public static IServiceCollection AddServiceBusMessageUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IServiceBusMessageUtil, ServiceBusMessageUtil>();

        return services;
    }

    /// <summary>
    /// Adds service bus message util as scoped.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The result of the operation.</returns>
    public static IServiceCollection AddServiceBusMessageUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IServiceBusMessageUtil, ServiceBusMessageUtil>();

        return services;
    }
}