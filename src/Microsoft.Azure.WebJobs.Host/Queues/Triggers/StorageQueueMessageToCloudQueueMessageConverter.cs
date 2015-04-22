﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Converters;
using Microsoft.Azure.WebJobs.Host.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Azure.WebJobs.Host.Queues.Triggers
{
    internal class StorageQueueMessageToCloudQueueMessageConverter : IConverter<IStorageQueueMessage, CloudQueueMessage>
    {
        public CloudQueueMessage Convert(IStorageQueueMessage input)
        {
            if (input == null)
            {
                return null;
            }

            return input.SdkObject;
        }
    }
}
