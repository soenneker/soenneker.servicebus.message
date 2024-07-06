using System;
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
    private const int _messageLimitBytes = 260096; // 256kB -  2kB (true header limit is 64kB, but this is a realistic expected value)
    private readonly bool _log;
    private readonly JsonOptionType _jsonOptionType;
    private readonly ILogger<ServiceBusMessageUtil> _logger;

    public ServiceBusMessageUtil(IConfiguration config, ILogger<ServiceBusMessageUtil> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _log = config.GetValue<bool>("Azure:ServiceBus:Log");
        _jsonOptionType = _log ? JsonOptionType.Pretty : JsonOptionType.Web;
    }

    public ServiceBusMessage? BuildMessage<TMessage>(TMessage message, Type type) where TMessage : Messages.Base.Message
    {
        return !message.NewtonsoftSerialize
            ? BuildMessageViaSerializer(message, type, JsonLibraryType.SystemTextJson)
            : BuildMessageViaSerializer(message, type, JsonLibraryType.Newtonsoft);
    }

    private ServiceBusMessage? BuildMessageViaSerializer<TMessage>(TMessage message, Type type, JsonLibraryType libraryType)
    {
        try
        {
            string? serializedMessage = JsonUtil.Serialize(message, _jsonOptionType, libraryType);

            if (serializedMessage == null)
                throw new SerializationException($"Couldn't serialize message of type {type.FullName}");

            if (IsMessageSizeExceedLimit(serializedMessage))
            {
                LogError($"Message size is over limit. Type: {type.FullName}, Size: {Encoding.UTF8.GetByteCount(serializedMessage)} bytes");
                return null;
            }

            if (_log)
                _logger.LogDebug("Creating message ({Name}): {Message}", type.Name, serializedMessage);

            var serviceBusMessage = new ServiceBusMessage(serializedMessage);
            serviceBusMessage.ApplicationProperties.Add("type", type.AssemblyQualifiedName);
            return serviceBusMessage;
        }
        catch (Exception ex)
        {
            LogCriticalError(ex, type, message);
            return null;
        }
    }

    private static bool IsMessageSizeExceedLimit(string message)
    {
        return Encoding.UTF8.GetByteCount(message) > _messageLimitBytes;
    }

    private void LogError(string message)
    {
        _logger.LogError("== ServiceBusMessageUtil: {Message}", message);
    }

    private void LogCriticalError(Exception ex, Type type, object message)
    {
        _logger.LogCritical(ex, "== ServiceBusMessageUtil: Error building service bus message. Type: {Type}, Message: {Message}", type.FullName, JsonUtil.Serialize(message, _jsonOptionType));
    }
}