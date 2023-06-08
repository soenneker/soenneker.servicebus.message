using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Soenneker.ServiceBus.Message.Abstract;
using Soenneker.ServiceBus.Message.Tests.Messages;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Json;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Concurrent;

namespace Soenneker.ServiceBus.Message.Tests;

[Collection("ServiceBusMessageUtilCollection")]
public class ServiceBusMessageUtilTests : FixturedUnitTest
{
    private readonly IServiceBusMessageUtil _util;

    private readonly ITestOutputHelper _output;

    public ServiceBusMessageUtilTests(ServiceBusMessageUtilFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
        _output = outputHelper;
        _util = Resolve<IServiceBusMessageUtil>();
    }

    [Fact]
    public async Task Parallel_serialization_should_deserialize_all()
    {
        int iterations = 10000;

        List<TestMessage> messages = new List<TestMessage>(iterations);

        for (int i = 0; i < iterations; i++)
        {
            var message = new TestMessage("queue")
            {
                Contents = Faker.Random.AlphaNumeric(50)
            };
            messages.Add(message);
        }

        await Task.Delay(1000);

        messages = messages.OrderBy(c => c.Contents).ToList();

        foreach (var message in messages)
        {
           // output.WriteLine(message.Contents);
        }

        _output.WriteLine($"count: {messages.Count}");

        var parallelTasks = new List<Task<ServiceBusMessage?>>();

        for (int i = 0; i < iterations; i++)
        {
            var task = _util.BuildMessage(messages[i], typeof(TestMessage));
            parallelTasks.Add(task);
        }

        var concurrentBag = new ConcurrentBag<ServiceBusMessage?>();

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 200
        };

        await Parallel.ForEachAsync(parallelTasks, parallelOptions, async (task, cancellationToken) =>
        {
            ServiceBusMessage? result = await task;

            if (result == null)
                throw new Exception("Service bus message was null, stopping test");

            concurrentBag.Add(result);
        });

        var results = concurrentBag.ToList();

        var deserialized = new List<TestMessage>();

        _output.WriteLine($"count: {results.Count}");

        for (int i = 0; i < results.Count; i++)
        {
            var str = results[i].Body.ToString();
            var objDeserialized = JsonUtil.Deserialize<TestMessage>(str);
            deserialized.Add(objDeserialized);
        }

        deserialized = deserialized.OrderBy(c => c.Contents).ToList();

        _output.WriteLine("-------------------");

        foreach (var message in deserialized)
        {
        //    output.WriteLine(message.Contents);
        }

        _output.WriteLine($"count: {deserialized.Count}");

        for (int i = 0; i < results.Count - 1; i++)
        {
            messages[i].Contents.Should().Be(deserialized[i].Contents);
        }

        await Task.Delay(1000);
    }

    [Fact]
    public async Task Sync_serialization_should_deserialize_all()
    {
        int iterations = 1000;

        List<TestMessage> messages = new List<TestMessage>(iterations);

        for (int i = 0; i < iterations; i++)
        {
            var message = new TestMessage("queue")
            {
                Contents = Faker.Random.AlphaNumeric(50)
            };
            messages.Add(message);
        }

        var parallelTasks = new List<Task<ServiceBusMessage?>>();

        for (int i = 0; i < iterations; i++)
        {
            var task = _util.BuildMessage(messages[i], typeof(TestMessage));
            parallelTasks.Add(task);
        }

        var results = new List<ServiceBusMessage?>(iterations);

        foreach (Task<ServiceBusMessage?> task in parallelTasks)
        {
            results.Add(await task);
        }

        var deserialized = new List<TestMessage>(iterations);

        for (int i = 0; i < results.Count - 1; i++)
        {
            var str = results[i].Body.ToString();
            var objDeserialized = JsonUtil.Deserialize<TestMessage>(str);
            deserialized.Add(objDeserialized);
        }

        for (int i = 0; i < results.Count - 1; i++)
        {
            messages[i].Contents.Should().Be(deserialized[i].Contents);
        }

        await Task.Delay(1000);
    }

    [Fact]
    public async Task Async_serialization_should_deserialize_all()
    {
        int iterations = 1000;

        List<TestMessage> messages = new List<TestMessage>(iterations);

        for (int i = 0; i < iterations; i++)
        {
            var message = new TestMessage("queue")
            {
                Contents = Faker.Random.AlphaNumeric(50)
            };
            messages.Add(message);
        }

        var parallelTasks = new List<Task<ServiceBusMessage?>>();

        for (int i = 0; i < iterations; i++)
        {
            var task = _util.BuildMessage(messages[i], typeof(TestMessage));
            parallelTasks.Add(task);
        }

        List<ServiceBusMessage?> results = (await Task.WhenAll(parallelTasks)).ToList();

        var deserialized = new List<TestMessage>(iterations);

        for (int i = 0; i < results.Count - 1; i++)
        {
            var str = results[i].Body.ToString();
            var objDeserialized = JsonUtil.Deserialize<TestMessage>(str);
            deserialized.Add(objDeserialized);
        }

        for (int i = 0; i < results.Count - 1; i++)
        {
            messages[i].Contents.Should().Be(deserialized[i].Contents);
        }

        await Task.Delay(1000);
    }
}