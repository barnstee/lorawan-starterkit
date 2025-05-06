// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaWan.NetworkServer
{
    using System.Collections.Generic;
    using System.Text.Json;
    using LoRaTools.LoRaMessage;
    using LoRaWANContainer.LoRaWan.NetworkServer.Interfaces;
    using Microsoft.Extensions.Logging;

    public class FunctionBundlerProvider(LoRaDeviceAPIServiceBase deviceApi,
                                   ILoggerFactory loggerFactory,
                                   ILogger<FunctionBundlerProvider> logger) : IFunctionBundlerProvider
    {
        private static readonly List<IFunctionBundlerExecutionItem> FunctionItems = new List<IFunctionBundlerExecutionItem>
        {
            new FunctionBundlerDeduplicationExecutionItem(),
            new FunctionBundlerADRExecutionItem(),
            new FunctionBundlerFCntDownExecutionItem(),
            new FunctionBundlerPreferredGatewayExecutionItem(),
        };

        public FunctionBundler CreateIfRequired(
                    string gatewayId,
                    LoRaPayloadData loRaPayload,
                    LoRaDevice loRaDevice,
                    IDeduplicationStrategyFactory deduplicationFactory,
                    LoRaRequest request)
        {
            System.ArgumentNullException.ThrowIfNull(loRaPayload);
            System.ArgumentNullException.ThrowIfNull(loRaDevice);

            if (!string.IsNullOrEmpty(loRaDevice.GatewayID))
            {
                // single gateway mode
                return null;
            }

            var context = new FunctionBundlerExecutionContext(gatewayId, loRaPayload.Fcnt, loRaDevice.FCntDown,
                                                              loRaPayload, loRaDevice, deduplicationFactory, request);

            var qualifyingExecutionItems = new List<IFunctionBundlerExecutionItem>(FunctionItems.Count);
            for (var i = 0; i < FunctionItems.Count; i++)
            {
                var itm = FunctionItems[i];
                if (itm.RequiresExecution(context))
                {
                    qualifyingExecutionItems.Add(itm);
                }
            }

            if (qualifyingExecutionItems.Count == 0)
            {
                return null;
            }

            var bundlerRequest = new FunctionBundlerRequest
            {
                ClientFCntDown = context.FCntDown,
                ClientFCntUp = context.FCntUp,
                GatewayId = gatewayId,
                Rssi = context.Request.RadioMetadata.UpInfo.ReceivedSignalStrengthIndication,
            };

            for (var i = 0; i < qualifyingExecutionItems.Count; i++)
            {
                qualifyingExecutionItems[i].Prepare(context, bundlerRequest);
            }

            logger.LogDebug("Finished preparing {NumberOfExecutionItems} FunctionBundler requests.", qualifyingExecutionItems.Count);

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug($"FunctionBundler request: {JsonSerializer.Serialize(bundlerRequest)}");

            return new FunctionBundler(loRaDevice.DevEUI, deviceApi, bundlerRequest, qualifyingExecutionItems, context, loggerFactory.CreateLogger<FunctionBundler>());
        }
    }
}
