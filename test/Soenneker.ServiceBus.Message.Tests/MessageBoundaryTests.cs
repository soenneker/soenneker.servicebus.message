using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.ServiceBus.Message;
using Soenneker.Utils.Json;
using Soenneker.Enums.JsonLibrary;

namespace Audit;

public class MessageBoundaryTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    [Test]
    public void SerializationPreservesDerivedPropertiesAndUtf8Limit()
    {
        var builder = new ServiceBusMessageUtil(Fixture.Config(), NullLogger<ServiceBusMessageUtil>.Instance);
        foreach (bool newtonsoft in new[] { false, true })
        {
            Soenneker.Messages.Base.Message model = Payload.Create("日本語🙂"); model.NewtonsoftSerialize = newtonsoft;
            var built = builder.BuildMessage(model, model.Type)!;
            Check(JsonUtil.Deserialize<Payload>(built.Body.ToString(), newtonsoft ? JsonLibraryType.Newtonsoft : JsonLibraryType.SystemTextJson)!.Content == "日本語🙂", "Derived content or Unicode lost");
            var tooBig = Payload.Create(new string('x', 260_096)); tooBig.NewtonsoftSerialize = newtonsoft;
            Check(builder.BuildMessage(tooBig, tooBig.Type) is null, "Oversized body accepted");
        }
    }

    [Test]
    public void BodyLimitAcceptsExactBoundaryForBothSerializers()
    {
        var builder = new ServiceBusMessageUtil(Fixture.Config(), NullLogger<ServiceBusMessageUtil>.Instance);
        foreach (bool newtonsoft in new[] { false, true })
        {
            var model = Payload.Create(""); model.NewtonsoftSerialize = newtonsoft;
            int overhead = builder.BuildMessage(model, model.Type)!.Body.ToMemory().Length;
            model.Content = new string('x', 260_096 - overhead);
            Check(builder.BuildMessage(model, model.Type)!.Body.ToMemory().Length == 260_096, "Exact boundary rejected");
            model.Content += "x";
            Check(builder.BuildMessage(model, model.Type) is null, "Body one byte over limit accepted");
        }
    }
}
public sealed class Payload : Soenneker.Messages.Base.Message
{
    public string Content { get; set; } = "hello";
    public static Payload Create(string content = "hello") => new()
    {
        Type = "audit.v1", Queue = "audit", Id = "id", Sender = "audit", CreatedAt = DateTimeOffset.UnixEpoch, Content = content
    };
}


internal static class Fixture
{
    public static Microsoft.Extensions.Configuration.IConfiguration Config(bool logging = false, bool counts = true) =>
        new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Azure:ServiceBus:Enable"] = "true", ["Azure:ServiceBus:TransmitterLogging"] = logging.ToString(),
            ["Background:QueueLength"] = "32", ["Background:LockCounts"] = counts.ToString()
        }).Build();
}
