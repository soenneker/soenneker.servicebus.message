using System;
using System.Diagnostics.Contracts;
using Azure.Messaging.ServiceBus;

namespace Soenneker.ServiceBus.Message.Abstract;

/// <summary>
/// A utility library for building Azure Service messages <para/>
/// Singleton IoC
/// </summary>
public interface IServiceBusMessageUtil
{
    [Pure]
    ServiceBusMessage? BuildMessage<TMessage>(TMessage message, Type type) where TMessage : Messages.Base.Message;
}