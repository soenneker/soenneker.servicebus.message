using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.ServiceBus.Message;
using System.Text.Json;

namespace Audit;

public class MessageBoundaryTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public void SerializationPreservesDerivedPropertiesAndUtf8Limit(bool useContext)
    {
        var builder = new ServiceBusMessageUtil(Fixture.Config(), NullLogger<ServiceBusMessageUtil>.Instance);
        Soenneker.Messages.Base.Message model = Payload.Create("日本語🙂");
        var built = (useContext ? builder.BuildMessage(model, model.Type, TestJsonContext.Default) : builder.BuildMessage(model, model.Type))!;
        Payload? roundTrip = JsonSerializer.Deserialize(built.Body.ToString(), TestJsonContext.Default.Payload);
        Check(roundTrip!.Content == "日本語🙂", "Derived content or Unicode lost");
        var tooBig = Payload.Create(new string('x', 260_096));
        Check((useContext ? builder.BuildMessage(tooBig, tooBig.Type, TestJsonContext.Default) : builder.BuildMessage(tooBig, tooBig.Type)) is null, "Oversized body accepted");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public void BodyLimitAcceptsExactBoundaryForBothMetadataPaths(bool useContext)
    {
        var builder = new ServiceBusMessageUtil(Fixture.Config(), NullLogger<ServiceBusMessageUtil>.Instance);
        var model = Payload.Create("");
        int overhead = (useContext ? builder.BuildMessage(model, model.Type, TestJsonContext.Default) : builder.BuildMessage(model, model.Type))!.Body.ToMemory().Length;
        model.Content = new string('x', 260_096 - overhead);
        Check((useContext ? builder.BuildMessage(model, model.Type, TestJsonContext.Default) : builder.BuildMessage(model, model.Type))!.Body.ToMemory().Length == 260_096, "Exact boundary rejected");
        model.Content += "x";
        Check((useContext ? builder.BuildMessage(model, model.Type, TestJsonContext.Default) : builder.BuildMessage(model, model.Type)) is null, "Body one byte over limit accepted");
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
