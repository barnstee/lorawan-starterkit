// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using LoRaWan.NetworkServer;

namespace LoRaWANContainer.LoRaWan.NetworkServer.Interfaces
{
    using LoRaTools.LoRaMessage;
    using LoRaWANContainer.LoRaWan.NetworkServer.Models;

    public interface IFunctionBundlerProvider
    {
        FunctionBundler CreateIfRequired(string gatewayId, LoRaPayloadData loRaPayload, LoRaDevice loRaDevice, IDeduplicationStrategyFactory deduplicationFactory, LoRaRequest request);
    }
}
