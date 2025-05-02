// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using LoRaWan.NetworkServer;
using LoRaWan.NetworkServer.BasicsStation;

using var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

var configuration = NetworkServerConfiguration.CreateFromEnvironmentVariables();
await BasicsStationNetworkServer.RunServerAsync(configuration, cancellationToken);

Thread.Sleep(Timeout.Infinite); // Keep the process alive
