// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaWan.NetworkServer
{
    using System;

    public class FrameCounterLoRaDeviceInitializer(ILoRaDeviceFrameCounterUpdateStrategyProvider frameCounterUpdateStrategyProvider) : ILoRaDeviceInitializer
    {
        public void Initialize(LoRaDevice loRaDevice)
        {
            ArgumentNullException.ThrowIfNull(loRaDevice);

            if (loRaDevice.IsOurDevice)
            {
                var strategy = frameCounterUpdateStrategyProvider.GetStrategy(loRaDevice.GatewayID);
                if (strategy is not null and ILoRaDeviceInitializer initializer)
                {
                    initializer.Initialize(loRaDevice);
                }
            }
        }
    }
}
