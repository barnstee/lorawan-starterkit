// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaWan.NetworkServer
{
    using LoRaTools.CommonAPI;
    using LoRaWan.NetworkServer.ADR;
    using LoRaWANContainer.LoRaWan.NetworkServer.Interfaces;
    using System;

    public class FunctionBundlerADRExecutionItem : IFunctionBundlerExecutionItem
    {
        public void Prepare(FunctionBundlerExecutionContext context, FunctionBundlerRequest request)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(request);

            request.AdrRequest = new LoRaADRRequest
            {
                DataRate = context.Request.RadioMetadata.DataRate,
                FCntDown = context.FCntDown,
                FCntUp = context.FCntUp,
                GatewayId = context.GatewayId,
                MinTxPowerIndex = context.Request.Region.TXPowertoMaxEIRP.Count - 1,
                PerformADRCalculation = context.LoRaPayload.IsAdrAckRequested,
                RequiredSnr = (float)context.Request.Region.RequiredSnr(context.Request.RadioMetadata.DataRate)
            };

            request.FunctionItems |= FunctionBundlerItemType.ADR;
        }

        public void ProcessResult(FunctionBundlerExecutionContext context, FunctionBundlerResult result)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(result);

            if (result.AdrResult != null)
            {
                if (result.AdrResult.CanConfirmToDevice && result.AdrResult.FCntDown > 0)
                {
                    context.LoRaDevice.SetFcntDown(context.LoRaDevice.FCntDown);
                }
            }
        }

        public bool RequiresExecution(FunctionBundlerExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.LoRaPayload.IsDataRateNetworkControlled;
        }
    }
}
