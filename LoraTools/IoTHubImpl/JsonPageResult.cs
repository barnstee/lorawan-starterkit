// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaTools.IoTHubImpl
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;

    internal sealed class JsonPageResult(IQuery originalQuery) : IoTHubRegistryPageResult<string>(originalQuery)
    {
        public override Task<IEnumerable<string>> GetNextPageAsync()
        {
            return this.OriginalQuery.GetNextAsJsonAsync();
        }
    }
}
