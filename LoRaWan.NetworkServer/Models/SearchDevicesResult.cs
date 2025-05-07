// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaWan.NetworkServer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using LoRaWANContainer.LoRaWan.NetworkServer.Interfaces;

    /// <summary>
    /// Results of a <see cref="LoRaDeviceAPIServiceBase.SearchDevicesAsync"/> call.
    /// </summary>
    public class SearchDevicesResult : IReadOnlyList<DeviceInfo>
    {
        /// <summary>
        /// Gets list of devices that match the criteria.
        /// </summary>
        public IReadOnlyList<DeviceInfo> Devices { get; } = Array.Empty<DeviceInfo>();

        /// <summary>
        /// Gets or sets a value indicating whether the dev nonce was already used.
        /// </summary>
        public bool IsDevNonceAlreadyUsed { get; set; }

        public string RefusedMessage { get; set; }

        public int Count => Devices.Count;

        public DeviceInfo this[int index] => Devices[index];

        public SearchDevicesResult() { }

        public SearchDevicesResult(IReadOnlyList<DeviceInfo> devices)
        {
            Devices = devices ?? Array.Empty<DeviceInfo>();
        }

        public IEnumerator<DeviceInfo> GetEnumerator() =>
            Devices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
