// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaWan.NetworkServer.BasicsStation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using LoRaTools.Regions;
    using LoRaWANContainer.LoRaWan.NetworkServer.Interfaces;
    using LoRaWANContainer.LoRaWan.NetworkServer.Models;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal sealed class BasicsStationConfigurationService(LoRaDeviceAPIServiceBase loRaDeviceApiService,
                                             IMemoryCache cache,
                                             ILogger<BasicsStationConfigurationService> logger) : IBasicsStationConfigurationService, IDisposable
    {
        internal const string RouterConfigPropertyName = "routerConfig";
        private const string DwellTimeConfigurationPropertyName = "desiredTxParams";
        private const string ConcentratorTwinCachePrefixName = "concentratorTwin:";
        internal const string CupsPropertyName = "cups";
        internal const string ClientThumbprintPropertyName = "clientThumbprint";

        private static readonly TimeSpan CacheTimeout = TimeSpan.FromHours(2);
        private readonly SemaphoreSlim cacheSemaphore = new SemaphoreSlim(1);

        public void Dispose() => this.cacheSemaphore.Dispose();

        private async Task<object> GetTwinDesiredPropertiesAsync(StationEui? stationEui, CancellationToken cancellationToken)
        {
            var cacheKey = $"{ConcentratorTwinCachePrefixName}{stationEui}";

            if (cache.TryGetValue(cacheKey, out var result))
                return result;

            await this.cacheSemaphore.WaitAsync(cancellationToken);

            try
            {
                return await cache.GetOrCreateAsync<object>(cacheKey, async cacheEntry =>
                {
                    _ = cacheEntry.SetAbsoluteExpiration(CacheTimeout);
                    var key = await loRaDeviceApiService.GetPrimaryKeyByEuiAsync(stationEui);
                    if (string.IsNullOrEmpty(key))
                    {
                        throw new LoRaProcessingException($"The configuration request of station '{stationEui}' did not match any configuration in IoT Hub. If you expect this connection request to succeed, make sure to provision the Basics Station in the device registry.",
                                                          LoRaProcessingErrorCode.InvalidDeviceConfiguration);
                    }

                    // TODO: Get desired properties using the key
                    return null;
                });
            }
            finally
            {
                _ = this.cacheSemaphore.Release();
            }
        }

        public async Task<string> GetRouterConfigMessageAsync(StationEui stationEui, CancellationToken cancellationToken)
        {
            var routerConfig = LnsStationConfiguration.GetConfiguration(await GetDesiredPropertyStringAsync(stationEui, RouterConfigPropertyName, cancellationToken));
            return JsonConvert.SerializeObject(routerConfig);
        }

        public async Task<Region> GetRegionAsync(StationEui stationEui, CancellationToken cancellationToken)
        {
            var config = await GetRouterConfigMessageAsync(stationEui, cancellationToken);
            var region = LnsStationConfiguration.GetRegion(config);
            if (region is DwellTimeLimitedRegion someRegion)
            {
                var dwellTimeSettings = await GetDesiredPropertyStringAsync(stationEui, DwellTimeConfigurationPropertyName, cancellationToken);
                someRegion.DesiredDwellTimeSetting = JsonConvert.DeserializeObject<DwellTimeSetting>(dwellTimeSettings);
            }
            return region;
        }

        public async Task<string[]> GetAllowedClientThumbprintsAsync(StationEui stationEui, CancellationToken cancellationToken)
        {
            var desiredProperties = await GetTwinDesiredPropertiesAsync(stationEui, cancellationToken);
            if (desiredProperties.ToString().Contains(ClientThumbprintPropertyName))
            {
                try
                {
                    var thumbprints = (JArray)desiredProperties;
                    return thumbprints.ToObject<string[]>();
                }
                catch (Exception ex) when (ex is InvalidCastException)
                {
                    throw new LoRaProcessingException($"'{ClientThumbprintPropertyName}' format is invalid. An array is expected.", ex, LoRaProcessingErrorCode.InvalidDeviceConfiguration);
                }
            }

            throw new LoRaProcessingException($"Property '{ClientThumbprintPropertyName}' was not present in device twin.", LoRaProcessingErrorCode.InvalidDeviceConfiguration);
        }

        public async Task<CupsTwinInfo> GetCupsConfigAsync(StationEui? stationEui, CancellationToken cancellationToken)
        {
            return JsonConvert.DeserializeObject<CupsTwinInfo>(await GetDesiredPropertyStringAsync(stationEui, CupsPropertyName, cancellationToken));
        }

        private async Task<string> GetDesiredPropertyStringAsync(StationEui? stationEui, string propertyName, CancellationToken cancellationToken)
        {
            var desiredProperties = await GetTwinDesiredPropertiesAsync(stationEui, cancellationToken);
            return desiredProperties.ToString();
        }

        public async Task SetReportedPackageVersionAsync(StationEui stationEui, string package, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(package))
            {
                logger.LogDebug($"Station did not report any 'package' field. Skipping reported property update.");
                return;
            }

            var key = await loRaDeviceApiService.GetPrimaryKeyByEuiAsync(stationEui);
            if (string.IsNullOrEmpty(key))
            {
                throw new LoRaProcessingException($"The configuration request of station '{stationEui}' did not match any configuration in IoT Hub. If you expect this connection request to succeed, make sure to provision the Basics Station in the device registry.",
                                                  LoRaProcessingErrorCode.InvalidDeviceConfiguration);
            }

            // TODO: persist reported package version using stationEui and key
        }
    }
}
