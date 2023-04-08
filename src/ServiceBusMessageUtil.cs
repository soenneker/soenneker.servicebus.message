﻿using System;
using System.Runtime.Serialization;
using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Enums.JsonLibrary;
using Soenneker.Enums.JsonOptions;
using Soenneker.ServiceBus.Message.Abstract;
using Soenneker.Utils.Json;

namespace Soenneker.ServiceBus.Message;

///<inheritdoc cref="IServiceBusMessageUtil"/>
public class ServiceBusMessageUtil : IServiceBusMessageUtil
{
    private readonly bool _log;
    private readonly JsonOptionType _jsonOptionType;

    /// <summary>
    /// Maximum message size 256kB -  2kB (true header limit is 64kB, but this is a realistic expected value)
    /// </summary>
    private const int _messageLimitBytes = 260096;

    private readonly ILogger<ServiceBusMessageUtil> _logger;

    public ServiceBusMessageUtil(IConfiguration config, ILogger<ServiceBusMessageUtil> logger)
    {
        _logger = logger;

        _log = config.GetValue<bool>("Azure:ServiceBus:Log");

        _jsonOptionType = _log ? JsonOptionType.Pretty : JsonOptionType.Web;
    }

    public ServiceBusMessage? BuildMessage<TMessage>(TMessage message, Type type) where TMessage : Messages.Base.Message
    {
        string? serializedMessage = null;

        try
        {
            if (message.NewtonsoftSerialize)
                serializedMessage = JsonUtil.Serialize(message, libraryType: JsonLibraryType.Newtonsoft);
            else
                serializedMessage = JsonUtil.Serialize(message, _jsonOptionType);

            if (serializedMessage == null)
                throw new SerializationException($"== SERVICEBUS: Couldn't serialize message of type {type.FullName}");

            int serializedMessageBytes = Encoding.UTF8.GetByteCount(serializedMessage);

            if (serializedMessageBytes > _messageLimitBytes)
            {
                _logger.LogError("== SERVICEBUS: Message size is over {limit}kB limit: {messageSize}kB, skipping. type: {type}", _messageLimitBytes, serializedMessageBytes, type.ToString());
                return null;
            }

            if (_log)
                _logger.LogDebug("== SERVICEBUS: Creating message ({name}): {message}", type.Name, serializedMessage);

            var serviceBusMessage = new ServiceBusMessage(serializedMessage);
            serviceBusMessage.ApplicationProperties.Add("type", type.AssemblyQualifiedName);
            
            return serviceBusMessage;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "== SERVICEBUS: Error building service bus message type: {type} message: {message}", type.ToString(), serializedMessage);
            return null;
        }
    }
}