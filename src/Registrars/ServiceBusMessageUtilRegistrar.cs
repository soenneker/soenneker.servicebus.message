using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.ServiceBus.Message.Abstract;

namespace Soenneker.ServiceBus.Message.Registrars;

/// <summary>
/// A utility library for building Azure Service messages
/// </summary>
public static class ServiceBusMessageUtilRegistrar
{
    public static void AddServiceBusMessageUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IServiceBusMessageUtil, ServiceBusMessageUtil>();
    }

    public static void AddServiceBusMessageUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IServiceBusMessageUtil, ServiceBusMessageUtil>();
    }
}