// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Sample
{
    public class Functions
    {
        [FunctionName("Function1")]
        public static void Run([ServiceBusTrigger("myqueue", Connection = "AzureWebJobsServiceBus")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
