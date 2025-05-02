// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace LoRaWan.NetworkServer.BasicsStation
{
    using System;
    using System.Linq;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using LoRaTools;
    using Microsoft.Extensions.Logging;

    internal sealed partial class ClientCertificateValidatorService(IBasicsStationConfigurationService stationConfigurationService,
                                             ILogger<ClientCertificateValidatorService> logger) : IClientCertificateValidatorService
    {
        public async Task<bool> ValidateAsync(X509Certificate2 certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(certificate);
            ArgumentNullException.ThrowIfNull(chain);

            var commonName = certificate.GetNameInfo(X509NameType.SimpleName, false);
            var regex = MyRegex().Match(commonName);
            var parseSuccess = StationEui.TryParse(regex.Value, out var stationEui);

            if (!parseSuccess)
            {
                logger.LogError("Could not find a possible StationEui in '{CommonName}'.", commonName);
                return false;
            }

            using var scope = logger.BeginEuiScope(stationEui);

            // Logging any chain related issue that is causing verification to fail
            if (chain.ChainStatus.Any(s => s.Status != X509ChainStatusFlags.NoError))
            {
                foreach (var status in chain.ChainStatus)
                {
                    logger.LogError("{Status} {StatusInformation}", status.Status, status.StatusInformation);
                }
                logger.LogError("Some errors were found in the chain.");
                return false;
            }

            // Additional validation is done on certificate thumprint
            try
            {
                var thumbprints = await stationConfigurationService.GetAllowedClientThumbprintsAsync(stationEui, token);
                var thumbprintFound = thumbprints.Any(t => t.Equals(certificate.Thumbprint, StringComparison.OrdinalIgnoreCase));
                if (!thumbprintFound)
                    logger.LogDebug($"'{certificate.Thumbprint}' was not found as allowed thumbprint for {stationEui}");
                return thumbprintFound;
            }
            catch (Exception ex) when (ExceptionFilterUtility.False(() => logger.LogError(ex, "An exception occurred while processing requests: {Exception}.", ex)))
            {
                return false;
            }
        }

        [GeneratedRegex("([a-fA-F0-9]{2}[-:]?){8}")]
        private static partial Regex MyRegex();
    }
}
