// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaTools
{
    using System;

    public interface ITwinProperties
    {
        long Version { get; }

        dynamic this[string propertyName] { get; set; }

        bool ContainsKey(string propertyName);

        DateTime GetLastUpdated();

        bool TryGetValue(string propertyName, out object item);
    }
}
