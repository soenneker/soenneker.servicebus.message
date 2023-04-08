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
    /// As Singleton
    /// </summary>
    public static void AddServiceBusMessageUtil(this IServiceCollection services)
    {
        services.TryAddSingleton<IServiceBusMessageUtil, ServiceBusMessageUtil>();
    }
}