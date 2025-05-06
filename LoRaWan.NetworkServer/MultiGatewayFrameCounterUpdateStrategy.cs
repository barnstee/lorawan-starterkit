// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaWan.NetworkServer
{
    using System.Threading.Tasks;
    using LoRaWANContainer.LoRaWan.NetworkServer.Interfaces;

    // Frame counter strategy for multi gateway scenarios
    // Frame Down counters is resolved by calling the LoRa device API. Only a single caller will received a valid frame counter (> 0)
    public class MultiGatewayFrameCounterUpdateStrategy(string gatewayID, LoRaDeviceAPIServiceBase loRaDeviceAPIService) : ILoRaDeviceFrameCounterUpdateStrategy
    {
        public async Task<bool> ResetAsync(LoRaDevice loRaDevice, uint fcntUp, string gatewayId)
        {
            System.ArgumentNullException.ThrowIfNull(loRaDevice);

            loRaDevice.ResetFcnt();

            return await loRaDeviceAPIService.ABPFcntCacheResetAsync(loRaDevice.DevEUI, fcntUp, gatewayId);
        }

        public async ValueTask<uint> NextFcntDown(LoRaDevice loRaDevice, uint messageFcnt)
        {
            System.ArgumentNullException.ThrowIfNull(loRaDevice);

            var result = await loRaDeviceAPIService.NextFCntDownAsync(
                devEUI: loRaDevice.DevEUI,
                fcntDown: loRaDevice.FCntDown,
                fcntUp: messageFcnt,
                gatewayId: gatewayID);

            if (result > 0)
            {
                loRaDevice.SetFcntDown(result);
            }

            return result;
        }

        public Task<bool> SaveChangesAsync(LoRaDevice loRaDevice)
        {
            System.ArgumentNullException.ThrowIfNull(loRaDevice);

            return InternalSaveChangesAsync(loRaDevice, force: false);
        }

        private static async Task<bool> InternalSaveChangesAsync(LoRaDevice loRaDevice, bool force)
        {
            return await loRaDevice.SaveChangesAsync(force: force);
        }
    }
}
