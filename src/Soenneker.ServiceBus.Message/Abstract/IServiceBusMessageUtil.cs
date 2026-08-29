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
    /// <summary>
    /// Builds message.
    /// </summary>
    /// <typeparam name="TMessage">Type of message used by the operation.</typeparam>
    /// <param name="message">Message content to send.</param>
    /// <param name="type">Runtime type to inspect or construct.</param>
    /// <returns>The resulting service Bus Message.</returns>
    [Pure]
    ServiceBusMessage? BuildMessage<TMessage>(TMessage message, string type) where TMessage : Messages.Base.Message;
}
