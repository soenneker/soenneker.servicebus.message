using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Soenneker.ServiceBus.Message.Abstract;
using Soenneker.ServiceBus.Message.Tests.Messages;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Json;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.ServiceBus.Message.Tests;

[Collection("ServiceBusMessageUtilCollection")]
public class ServiceBusMessageUtilTests : FixturedUnitTest
{
    private readonly IServiceBusMessageUtil _util;

    public ServiceBusMessageUtilTests(ServiceBusMessageUtilFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
        _util = Resolve<IServiceBusMessageUtil>();
    }

    [Fact]
    public async Task Parallel_serialization_should_deserialize_all()
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

        //ParallelOptions parallelOptions = new()
        //{
        //    MaxDegreeOfParallelism = 200
        //};

        //await Parallel.ForEachAsync(parallelTasks, parallelOptions, async (task, cancellationToken) =>
        //{
        //    var result = await task;

        //    if (result == null)
        //        throw new Exception("Service bus message was null, stopping test");

        //    results.Add(result);
        //});

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
    }
}