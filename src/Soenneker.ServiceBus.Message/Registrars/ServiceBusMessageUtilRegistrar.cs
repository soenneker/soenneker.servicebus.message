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
    /// Registers Service Bus Message Util with a singleton lifetime.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddServiceBusMessageUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IServiceBusMessageUtil, ServiceBusMessageUtil>();

        return services;
    }

    /// <summary>
    /// Registers Service Bus Message Util with a scoped lifetime.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddServiceBusMessageUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IServiceBusMessageUtil, ServiceBusMessageUtil>();

        return services;
    }
}
