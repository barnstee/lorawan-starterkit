// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaWan.NetworkServer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Server.Kestrel.Https;

    // Network server configuration
    public class NetworkServerConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether the server is running as an IoT Edge module.
        /// </summary>
        public bool RunningAsIoTEdgeModule { get; set; }

        /// <summary>
        /// Gets or sets the iot hub host name.
        /// </summary>
        public string IoTHubHostName { get; set; }

        /// <summary>
        /// Gets or sets the gateway host name.
        /// </summary>
        public string GatewayHostName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the gateway (edgeHub) is enabled.
        /// </summary>
        public bool EnableGateway { get; set; } = true;

        /// <summary>
        /// Gets or sets the gateway identifier.
        /// </summary>
        public string GatewayID { get; set; }

        /// <summary>
        /// Gets or sets the HTTP proxy url.
        /// </summary>
        public string HttpsProxy { get; set; }

        /// <summary>
        /// Gets or sets the 2nd receive windows datarate.
        /// </summary>
        public DataRateIndex? Rx2DataRate { get; set; }

        /// <summary>
        /// Gets or sets the 2nd receive windows data frequency.
        /// </summary>
        public Hertz? Rx2Frequency { get; set; }

        /// <summary>
        /// Gets or sets the IoT Edge timeout in milliseconds, 0 keeps default value,.
        /// </summary>
        public uint IoTEdgeTimeout { get; set; }

        /// <summary>
        /// Gets or sets the Azure Facade function URL.
        /// </summary>
        public Uri FacadeServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the Azure Facade Function auth code.
        /// </summary>
        public string FacadeAuthCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logging to console is enabled.
        /// </summary>
        public bool LogToConsole { get; set; } = true;

        /// <summary>
        /// Gets or sets  the logging level.
        /// Default: 4 (Log level: Error).
        /// </summary>
        public string LogLevel { get; set; } = "4";

        /// <summary>
        /// Gets or sets a value indicating whether logging to TCP is enabled (used for integration tests mainly).
        /// Default is false.
        /// </summary>
        public bool LogToTcp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logging to IoT Hub is enabled.
        /// Default is false.
        /// </summary>
        public bool LogToHub { get; set; }

        /// <summary>
        /// Gets or sets TCP address to send log to.
        /// </summary>
        public string LogToTcpAddress { get; set; }

        /// <summary>
        /// Gets or sets TCP port to send logs to.
        /// </summary>
        public int LogToTcpPort { get; set; } = 6000;

        /// <summary>
        /// Gets or sets the gateway netword id.
        /// </summary>
        public NetId NetId { get; set; } = new NetId(1);

        /// <summary>
        /// Gets list of allowed dev addresses.
        /// </summary>
        public HashSet<DevAddr> AllowedDevAddresses { get; internal set; }

        /// <summary>
        /// Path of the .pfx certificate to be used for LNS Server endpoint
        /// </summary>
        public string LnsServerPfxPath { get; internal set; }

        /// <summary>
        /// Password of the .pfx certificate to be used for LNS Server endpoint
        /// </summary>
        public string LnsServerPfxPassword { get; internal set; }

        /// <summary>
        /// Specifies the client certificate mode with which the server should be run
        /// Allowed values can be found at https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.server.kestrel.https.clientcertificatemode?view=aspnetcore-6.0
        /// </summary>
        public ClientCertificateMode ClientCertificateMode { get; internal set; }

        /// <summary>
        /// Gets the version of the LNS.
        /// </summary>
        public string LnsVersion { get; private set; }

        /// <summary>
        /// Gets the connection string of Redis server for Pub/Sub functionality in Cloud only deployments.
        /// </summary>
        public string RedisConnectionString { get; private set; }

        /// <summary>
        /// Specifies the pool size for upstream AMQP connection
        /// </summary>
        public uint IotHubConnectionPoolSize { get; internal set; } = 1;

        /// <summary>
        /// Specificies wether we are running in local development mode.
        /// </summary>
        public bool IsLocalDevelopment { get; set; }


        /// <summary>
        /// Specifies the Processing Delay in Milliseconds
        /// </summary>
        public int ProcessingDelayInMilliseconds { get; set; } = Constants.DefaultProcessingDelayInMilliseconds;

        // Creates a new instance of NetworkServerConfiguration by reading values from environment variables
        public static NetworkServerConfiguration CreateFromEnvironmentVariables()
        {
            var config = new NetworkServerConfiguration
            {
                // Create case insensitive dictionary from environment variables
                ProcessingDelayInMilliseconds = int.Parse(Environment.GetEnvironmentVariable("PROCESSING_DELAY_IN_MS"), NumberFormatInfo.InvariantInfo),
                IsLocalDevelopment = true,
                GatewayHostName = Environment.GetEnvironmentVariable("IOTEDGE_GATEWAYHOSTNAME"),
                EnableGateway = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_GATEWAY"))
            };

            if (!config.RunningAsIoTEdgeModule && config.EnableGateway)
            {
                throw new NotSupportedException("ENABLE_GATEWAY cannot be true if RunningAsIoTEdgeModule is false.");
            }

            config.HttpsProxy = Environment.GetEnvironmentVariable("HTTPS_PROXY");

            // Fix for CS1955: Non-invocable member 'DataRateIndex' cannot be used like a method.
            // The issue is that 'DataRateIndex' is an enum, and enums cannot be invoked like methods.
            // The correct approach is to cast or parse the integer value to the enum type.

            config.Rx2DataRate = (DataRateIndex)int.Parse(Environment.GetEnvironmentVariable("RX2_DATR"), NumberFormatInfo.InvariantInfo);

            config.Rx2Frequency = double.Parse(Environment.GetEnvironmentVariable("RX2_FREQ"), NumberFormatInfo.InvariantInfo) is { } someFreq ? Hertz.Mega(someFreq) : null;

            // facadeurl is allowed to be null as the value is coming from the twin in production.
            var facadeUrl = Environment.GetEnvironmentVariable("FACADE_SERVER_URL");
            config.FacadeServerUrl = string.IsNullOrEmpty(facadeUrl) ? null : new Uri(Environment.GetEnvironmentVariable("FACADE_SERVER_URL"));
            config.FacadeAuthCode = Environment.GetEnvironmentVariable("FACADE_AUTH_CODE");
            config.LogLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");
            config.LogToConsole = true;
            config.LogToTcp = false;
            config.LogToHub = false;
            config.NetId = new NetId(int.Parse(Environment.GetEnvironmentVariable("NETID"), NumberFormatInfo.InvariantInfo));
            config.AllowedDevAddresses = [.. Environment.GetEnvironmentVariable("AllowedDevAddresses")
                                                .Split(";")
                                                .Select(s => DevAddr.TryParse(s, out var devAddr) ? (true, Value: devAddr) : default)
                                                .Where(a => a is (true, _))
                                                .Select(a => a.Value)];
            config.LnsServerPfxPath = Environment.GetEnvironmentVariable("LNS_SERVER_PFX_PATH");
            config.LnsServerPfxPassword = Environment.GetEnvironmentVariable("LNS_SERVER_PFX_PASSWORD");
            var clientCertificateModeString = Environment.GetEnvironmentVariable("CLIENT_CERTIFICATE_MODE");
            config.ClientCertificateMode = Enum.Parse<ClientCertificateMode>(clientCertificateModeString, true);
            config.LnsVersion = Environment.GetEnvironmentVariable("LNS_VERSION");

            return config;
        }
    }
}
