// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaWan.NetworkServer.BasicsStation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Jacob;
    using LoRaTools.Regions;

    internal sealed class BasicsStationConfigurationService() : IBasicsStationConfigurationService, IDisposable
    {
        internal const string RouterConfigPropertyName = "routerConfig";
        internal const string CupsPropertyName = "cups";
        internal const string ClientThumbprintPropertyName = "clientThumbprint";

        private static readonly IJsonReader<DwellTimeSetting> DwellTimeConfigurationReader =
            JsonReader.Object(JsonReader.Property("downlinkDwellLimit", JsonReader.Boolean()),
                              JsonReader.Property("uplinkDwellLimit", JsonReader.Boolean()),
                              JsonReader.Property("eirp", JsonReader.UInt32()),
                              (downlinkDwellLimit, uplinkDwellLimit, eirp) => new DwellTimeSetting(downlinkDwellLimit, uplinkDwellLimit, eirp));

        private static readonly TimeSpan CacheTimeout = TimeSpan.FromHours(2);
        private readonly SemaphoreSlim cacheSemaphore = new SemaphoreSlim(1);

        public void Dispose() => this.cacheSemaphore.Dispose();

        Task<string[]> IBasicsStationConfigurationService.GetAllowedClientThumbprintsAsync(StationEui stationEui, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<CupsTwinInfo> IBasicsStationConfigurationService.GetCupsConfigAsync(StationEui stationEui, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<Region> IBasicsStationConfigurationService.GetRegionAsync(StationEui stationEui, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string> IBasicsStationConfigurationService.GetRouterConfigMessageAsync(StationEui stationEui, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IBasicsStationConfigurationService.SetReportedPackageVersionAsync(StationEui stationEui, string package, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
