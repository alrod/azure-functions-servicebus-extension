﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace Microsoft.Azure.WebJobs.Host.Executors
{
    internal class TriggeredFunctionExecutor<TTriggerValue> : ITriggeredFunctionExecutor<TTriggerValue>
    {
        private FunctionDescriptor _descriptor;
        private ITriggeredFunctionInstanceFactory<TTriggerValue> _instanceFactory;
        private IFunctionExecutor _executor;

        public TriggeredFunctionExecutor(FunctionDescriptor descriptor, IFunctionExecutor executor, ITriggeredFunctionInstanceFactory<TTriggerValue> instanceFactory)
        {
            _descriptor = descriptor;
            _executor = executor;
            _instanceFactory = instanceFactory;
        }

        public FunctionDescriptor Function
        {
            get
            {
                return _descriptor;
            }
        }

        public async Task<bool> TryExecuteAsync(TriggeredFunctionData<TTriggerValue> input, CancellationToken cancellationToken)
        {
            IFunctionInstance instance = _instanceFactory.Create(input.TriggerValue, input.ParentId);
            IDelayedException exception = await _executor.TryExecuteAsync(instance, cancellationToken);

            return exception == null;
        }

        public async Task<bool> TryExecuteAsync(TriggeredFunctionData input, CancellationToken cancellationToken)
        {
            return await TryExecuteAsync(input, cancellationToken);
        }
    }
}
