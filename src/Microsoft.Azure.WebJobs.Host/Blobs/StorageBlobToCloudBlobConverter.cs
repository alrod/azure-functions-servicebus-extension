﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Converters;
using Microsoft.Azure.WebJobs.Host.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.WebJobs.Host.Blobs
{
    internal class StorageBlobToCloudBlobConverter : IConverter<IStorageBlob, ICloudBlob>
    {
        public ICloudBlob Convert(IStorageBlob input)
        {
            if (input == null)
            {
                return null;
            }

            return input.SdkObject;
        }
    }
}
