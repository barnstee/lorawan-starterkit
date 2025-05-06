// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using LoRaWan.NetworkServer;

namespace LoRaWANContainer.LoRaWan.NetworkServer.Interfaces
{
    public interface IJoinRequestMessageHandler
    {
        void DispatchRequest(LoRaRequest request);
    }
}