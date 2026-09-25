using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
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

public sealed class ServiceBusMessageUtil : IServiceBusMessageUtil
{
    private readonly JsonSerializerContext _jsonContext;


    private const int _messageLimitBytes = 260_096;
    private static readonly Encoding _utf8 = Encoding.UTF8;

    private readonly JsonSerializerContext[] _jsonContexts;
    private readonly bool _log;
    private readonly JsonOptionType _jsonOptionType;
    private readonly ILogger<ServiceBusMessageUtil> _logger;

    public ServiceBusMessageUtil(JsonSerializerContext jsonContext, IConfiguration config, ILogger<ServiceBusMessageUtil> logger, IEnumerable<JsonSerializerContext>? jsonContexts = null)
    {
        _jsonContext = jsonContext ?? throw new System.ArgumentNullException(nameof(jsonContext));
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _jsonContexts = jsonContexts?.ToArray() ?? [];
        _log = config.GetValue<bool>("Azure:ServiceBus:Log");
        _jsonOptionType = _log ? JsonOptionType.Pretty : JsonOptionType.Web;
    }

    public ServiceBusMessage? BuildMessage<TMessage>(TMessage message, string type) where TMessage : Messages.Base.Message
    {
        return !message.NewtonsoftSerialize ? BuildMessageStjUtf8(message, type) : BuildMessageNewtonsoftString(message, type);
    }

    private ServiceBusMessage? BuildMessageStjUtf8<TMessage>(TMessage message, string type) where TMessage : Messages.Base.Message
    {
        try
        {
            byte[]? utf8Bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(message, GetTypeInfo(message.GetType()));

            if (utf8Bytes is null)
                throw new SerializationException("Couldn't serialize message of type " + type);

            int size = utf8Bytes.Length;

            if (size > _messageLimitBytes)
            {
                _logger.LogError("== ServiceBusMessageUtil: Message size is over limit. Type: {Type}, Size: {SizeBytes} bytes", type, size);

                return null;
            }

            if (_log && _logger.IsEnabled(LogLevel.Debug))
            {
                string payload = _utf8.GetString(utf8Bytes);
                _logger.LogDebug("Creating message ({Type}): {Message}", type, payload);
            }

            var sbMessage = new ServiceBusMessage(utf8Bytes)
            {
                ApplicationProperties =
                {
                    ["type"] = type
                }
            };

            return sbMessage;
        }
        catch (Exception ex)
        {
            LogCriticalError(ex, type, message, JsonLibraryType.SystemTextJson);
            return null;
        }
    }

    private ServiceBusMessage? BuildMessageNewtonsoftString<TMessage>(TMessage message, string type) where TMessage : Messages.Base.Message
    {
        try
        {
            // Required for messages explicitly opting into Newtonsoft contracts and converters.
            string? serialized = JsonUtil.Serialize(message, _jsonOptionType, JsonLibraryType.Newtonsoft);

            if (serialized is null)
                throw new SerializationException("Couldn't serialize message of type " + type);

            int byteCount = _utf8.GetByteCount(serialized);

            if (byteCount > _messageLimitBytes)
            {
                _logger.LogError("== ServiceBusMessageUtil: Message size is over limit. Type: {Type}, Size: {SizeBytes} bytes", type, byteCount);

                return null;
            }

            if (_log && _logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Creating message ({Type}): {Message}", type, serialized);

            var sbMessage = new ServiceBusMessage(serialized)
            {
                ApplicationProperties =
                {
                    ["type"] = type
                }
            };

            return sbMessage;
        }
        catch (Exception ex)
        {
            LogCriticalError(ex, type, message, JsonLibraryType.Newtonsoft);
            return null;
        }
    }

    private JsonTypeInfo GetTypeInfo(Type type)
    {
        foreach (JsonSerializerContext context in _jsonContexts)
        {
            JsonTypeInfo? typeInfo = context.GetTypeInfo(type);
            if (typeInfo is not null)
                return typeInfo;
        }

        return _jsonContext.GetTypeInfo(type) ?? throw new NotSupportedException($"No generated JSON metadata for {type}.");
    }
    private void LogCriticalError(Exception ex, string type, object message, JsonLibraryType libraryType)
    {
        if (!_logger.IsEnabled(LogLevel.Critical))
            return;

        if (_log)
        {
            try
            {
                string? serialized = libraryType == JsonLibraryType.Newtonsoft
                    ? JsonUtil.Serialize(message, _jsonOptionType, libraryType)
                    : System.Text.Json.JsonSerializer.Serialize(message, GetTypeInfo(message.GetType()));

                _logger.LogCritical(ex, "== ServiceBusMessageUtil: Error building service bus message. Type: {Type}, Message: {Message}", type, serialized);
            }
            catch
            {
                _logger.LogCritical(ex, "== ServiceBusMessageUtil: Error building service bus message. Type: {Type}; payload could not be serialized for logging", type);
            }
        }
        else
        {
            _logger.LogCritical(ex, "== ServiceBusMessageUtil: Error building service bus message. Type: {Type}", type);
        }
    }
}
