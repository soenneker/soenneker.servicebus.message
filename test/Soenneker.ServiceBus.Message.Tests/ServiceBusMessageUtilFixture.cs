using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.XUnit.Injectable;
using Serilog.Sinks.XUnit.Injectable.Extensions;
using Soenneker.Fixtures.Unit;
using Soenneker.ServiceBus.Message.Registrars;
using Soenneker.Utils.Test;
using ILogger = Serilog.ILogger;

namespace Soenneker.ServiceBus.Message.Tests;

public class ServiceBusMessageUtilFixture : UnitFixture
{
    public override async System.Threading.Tasks.ValueTask InitializeAsync()
    {
        SetupIoC(Services);

        await base.InitializeAsync();
    }

    private static void SetupIoC(IServiceCollection services)
    {
        IConfiguration config = TestUtil.BuildConfig();

        services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
        services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

        var injectableTestOutputSink = new InjectableTestOutputSink();

        ILogger serilogLogger = new LoggerConfiguration()
            .WriteTo.InjectableTestOutput(injectableTestOutputSink)
            .CreateLogger();

        Log.Logger = serilogLogger;

        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });

        services.AddSingleton(config);
        services.AddServiceBusMessageUtil();
    }
}