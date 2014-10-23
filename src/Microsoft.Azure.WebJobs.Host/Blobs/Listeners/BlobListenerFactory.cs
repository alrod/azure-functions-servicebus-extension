﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Queues;
using Microsoft.Azure.WebJobs.Host.Queues.Listeners;
using Microsoft.Azure.WebJobs.Host.Storage;
using Microsoft.Azure.WebJobs.Host.Storage.Blob;
using Microsoft.Azure.WebJobs.Host.Storage.Queue;
using Microsoft.Azure.WebJobs.Host.Timers;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.WebJobs.Host.Blobs.Listeners
{
    internal class BlobListenerFactory : IListenerFactory
    {
        private readonly IHostIdProvider _hostIdProvider;
        private readonly IQueueConfiguration _queueConfiguration;
        private readonly IBackgroundExceptionDispatcher _backgroundExceptionDispatcher;
        private readonly string _functionId;
        private readonly IStorageAccount _account;
        private readonly CloudBlobContainer _container;
        private readonly IBlobPathSource _input;
        private readonly ITriggeredFunctionInstanceFactory<IStorageBlob> _instanceFactory;

        public BlobListenerFactory(IHostIdProvider hostIdProvider,
            IQueueConfiguration queueConfiguration,
            IBackgroundExceptionDispatcher backgroundExceptionDispatcher,
            string functionId,
            IStorageAccount account,
            CloudBlobContainer container,
            IBlobPathSource input,
            ITriggeredFunctionInstanceFactory<IStorageBlob> instanceFactory)
        {
            if (hostIdProvider == null)
            {
                throw new ArgumentNullException("hostIdProvider");
            }

            if (queueConfiguration == null)
            {
                throw new ArgumentNullException("queueConfiguration");
            }

            if (backgroundExceptionDispatcher == null)
            {
                throw new ArgumentNullException("backgroundExceptionDispatcher");
            }

            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (instanceFactory == null)
            {
                throw new ArgumentNullException("instanceFactory");
            }

            _hostIdProvider = hostIdProvider;
            _queueConfiguration = queueConfiguration;
            _backgroundExceptionDispatcher = backgroundExceptionDispatcher;
            _functionId = functionId;
            _account = account;
            _container = container;
            _input = input;
            _instanceFactory = instanceFactory;
        }

        public async Task<IListener> CreateAsync(IFunctionExecutor executor, ListenerFactoryContext context)
        {
            SharedQueueWatcher sharedQueueWatcher = context.SharedListeners.GetOrCreate<SharedQueueWatcher>(
                new SharedQueueWatcherFactory(context));
            SharedBlobListener sharedBlobListener = context.SharedListeners.GetOrCreate<SharedBlobListener>(
                new SharedBlobListenerFactory(_account, _backgroundExceptionDispatcher, context));

            // Note that these clients are intentionally for the storage account rather than for the dashboard account.
            // We use the storage, not dashboard, account for the blob receipt container and blob trigger queues.
            IStorageQueueClient queueClient = _account.CreateQueueClient();
            IStorageBlobClient blobClient = _account.CreateBlobClient();
            CloudBlobClient sdkBlobClient = _account.SdkObject.CreateCloudBlobClient();

            string hostId = await _hostIdProvider.GetHostIdAsync(context.CancellationToken);
            string hostBlobTriggerQueueName = HostQueueNames.GetHostBlobTriggerQueueName(hostId);
            IStorageQueue hostBlobTriggerQueue = queueClient.GetQueueReference(hostBlobTriggerQueueName);

            IListener blobDiscoveryToQueueMessageListener = await CreateBlobDiscoveryToQueueMessageListenerAsync(
                hostId, context, sharedBlobListener, sdkBlobClient, hostBlobTriggerQueue, sharedQueueWatcher,
                context.CancellationToken);
            IListener queueMessageToTriggerExecutionListener = CreateQueueMessageToTriggerExecutionListener(executor,
                context, sharedQueueWatcher, queueClient, hostBlobTriggerQueue, blobClient,
                sharedBlobListener.BlobWritterWatcher);
            IListener compositeListener = new CompositeListener(
                blobDiscoveryToQueueMessageListener,
                queueMessageToTriggerExecutionListener);
            return compositeListener;
        }

        private async Task<IListener> CreateBlobDiscoveryToQueueMessageListenerAsync(string hostId,
            ListenerFactoryContext context,
            SharedBlobListener sharedBlobListener,
            CloudBlobClient blobClient,
            IStorageQueue hostBlobTriggerQueue,
            IMessageEnqueuedWatcher messageEnqueuedWatcher,
            CancellationToken cancellationToken)
        {
            BlobTriggerExecutor triggerExecutor = new BlobTriggerExecutor(hostId, _functionId, _input,
                BlobETagReader.Instance, new BlobReceiptManager(blobClient),
                new BlobTriggerQueueWriter(hostBlobTriggerQueue, messageEnqueuedWatcher));
            await sharedBlobListener.RegisterAsync(_container, triggerExecutor, cancellationToken);
            return new BlobListener(sharedBlobListener);
        }

        private IListener CreateQueueMessageToTriggerExecutionListener(IFunctionExecutor executor,
            ListenerFactoryContext context,
            SharedQueueWatcher sharedQueueWatcher,
            IStorageQueueClient queueClient,
            IStorageQueue hostBlobTriggerQueue,
            IStorageBlobClient blobClient,
            IBlobWrittenWatcher blobWrittenWatcher)
        {
            SharedBlobQueueListener sharedListener = context.SharedListeners.GetOrCreate<SharedBlobQueueListener>(
                new SharedBlobQueueListenerFactory(executor, context, sharedQueueWatcher, queueClient,
                    hostBlobTriggerQueue, blobClient, _queueConfiguration, _backgroundExceptionDispatcher,
                    blobWrittenWatcher));
            sharedListener.Register(_functionId, _instanceFactory);
            return new BlobListener(sharedListener);
        }
    }
}
