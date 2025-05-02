// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace LoRaWan.NetworkServer
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using StackExchange.Redis;
    using LoRaTools;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics.Metrics;

    internal interface ILnsRemoteCallListener
    {
        Task SubscribeAsync(string lns, Func<LnsRemoteCall, Task> function, CancellationToken cancellationToken);

        Task UnsubscribeAsync(string lns, CancellationToken cancellationToken);
    }

    internal sealed class RedisRemoteCallListener(ConnectionMultiplexer redis, ILogger<RedisRemoteCallListener> logger, Meter meter) : ILnsRemoteCallListener
    {
        private readonly Counter<int> unhandledExceptionCount = meter.CreateCounter<int>(MetricRegistry.UnhandledExceptions);

        // Cancellation token to be passed when/if a future update to SubscribeAsync is allowing to use it
        public async Task SubscribeAsync(string lns, Func<LnsRemoteCall, Task> function, CancellationToken cancellationToken)
        {
            var channelMessage = await redis.GetSubscriber().SubscribeAsync(RedisChannel.Literal(lns));
            channelMessage.OnMessage(value =>
            {
                try
                {
                    if (value is { Message: { } m } && !m.IsNullOrEmpty)
                    {
                        var lnsRemoteCall = JsonSerializer.Deserialize<LnsRemoteCall>(m.ToString()) ?? throw new InvalidOperationException("Deserialization produced an empty LnsRemoteCall.");
                        return function(lnsRemoteCall);
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(value));
                    }
                }
                catch (Exception ex) when (ExceptionFilterUtility.False(() => logger.LogError(ex, $"An exception occurred when reacting to a Redis message: '{ex}'."),
                                                                        () => this.unhandledExceptionCount.Add(1)))
                {
                    throw;
                }
            });
        }

        // Cancellation token to be passed when/if a future update to UnsubscribeAsync is allowing to use it
        public async Task UnsubscribeAsync(string lns, CancellationToken cancellationToken)
        {
            await redis.GetSubscriber().UnsubscribeAsync(RedisChannel.Literal(lns));
        }
    }
}
