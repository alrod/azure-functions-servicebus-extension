﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Storage.Blob;
using Microsoft.Azure.WebJobs.Host.Timers;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Host.Loggers
{
    internal sealed class UpdateParameterLogCommand : IRecurrentCommand
    {
        private readonly IReadOnlyDictionary<string, IWatcher> _watches;
        private readonly IStorageBlockBlob _parameterLogBlob;
        private readonly TraceWriter _trace;

        private string _lastContent;

        public UpdateParameterLogCommand(IReadOnlyDictionary<string, IWatcher> watches,
            IStorageBlockBlob parameterLogBlob, TraceWriter trace)
        {
            if (parameterLogBlob == null)
            {
                throw new ArgumentNullException("parameterLogBlob");
            }
            else if (trace == null)
            {
                throw new ArgumentNullException("trace");
            }
            else if (watches == null)
            {
                throw new ArgumentNullException("watches");
            }

            _parameterLogBlob = parameterLogBlob;
            _trace = trace;
            _watches = watches;
        }

        public static void AddLogs(IReadOnlyDictionary<string, IWatcher> watches,
            IDictionary<string, ParameterLog> collector)
        {
            foreach (KeyValuePair<string, IWatcher> item in watches)
            {
                IWatcher watch = item.Value;

                if (watch == null)
                {
                    continue;
                }

                ParameterLog status = watch.GetStatus();

                if (status == null)
                {
                    continue;
                }

                collector.Add(item.Key, status);
            }
        }

        public async Task<bool> TryExecuteAsync(CancellationToken cancellationToken)
        {
            Dictionary<string, ParameterLog> logs = new Dictionary<string, ParameterLog>();
            AddLogs(_watches, logs);
            string content = JsonConvert.SerializeObject(logs, JsonSerialization.Settings);

            try
            {
                if (_lastContent == content)
                {
                    // If it hasn't change, then don't re upload stale content.
                    return true;
                }

                _lastContent = content;
                await _parameterLogBlob.UploadTextAsync(content, cancellationToken: cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                // Not fatal if we can't update parameter status. 
                // But at least log what happened for diagnostics in case it's an infrastructure bug.
                _trace.Error("---- Parameter status update failed ----", e, TraceSource.Execution);
                return false;
            }
        }
    }
}
