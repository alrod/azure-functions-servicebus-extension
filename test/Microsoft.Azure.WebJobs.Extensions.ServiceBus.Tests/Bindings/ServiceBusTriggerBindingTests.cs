﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs.ServiceBus.Triggers;
using Microsoft.Azure.ServiceBus;
using Xunit;
using Microsoft.Azure.ServiceBus.Core;
using Moq;

namespace Microsoft.Azure.WebJobs.ServiceBus.UnitTests.Bindings
{
    public class ServiceBusTriggerBindingTests
    {
        [Fact]
        public void CreateBindingDataContract_ReturnsExpectedValue()
        {
            IReadOnlyDictionary<string, Type> argumentContract = null;
            var bindingDataContract = ServiceBusTriggerBinding.CreateBindingDataContract(argumentContract);
            Assert.Equal(15, bindingDataContract.Count);
            Assert.Equal(bindingDataContract["DeliveryCount"], typeof(int));
            Assert.Equal(bindingDataContract["DeadLetterSource"], typeof(string));
            Assert.Equal(bindingDataContract["LockToken"], typeof(string));
            Assert.Equal(bindingDataContract["ExpiresAtUtc"], typeof(DateTime));
            Assert.Equal(bindingDataContract["EnqueuedTimeUtc"], typeof(DateTime));
            Assert.Equal(bindingDataContract["MessageId"], typeof(string));
            Assert.Equal(bindingDataContract["ContentType"], typeof(string));
            Assert.Equal(bindingDataContract["ReplyTo"], typeof(string));
            Assert.Equal(bindingDataContract["SequenceNumber"], typeof(long));
            Assert.Equal(bindingDataContract["To"], typeof(string));
            Assert.Equal(bindingDataContract["Label"], typeof(string));
            Assert.Equal(bindingDataContract["CorrelationId"], typeof(string));
            Assert.Equal(bindingDataContract["UserProperties"], typeof(IDictionary<string, object>));
            Assert.Equal(bindingDataContract["MessageReceiver"], typeof(MessageReceiver));
            Assert.Equal(bindingDataContract["MessageSession"], typeof(IMessageSession));

            // verify that argument binding values override built ins
            argumentContract = new Dictionary<string, Type>
            {
                { "DeliveryCount", typeof(string) },
                { "NewProperty", typeof(decimal) }
            };
            bindingDataContract = ServiceBusTriggerBinding.CreateBindingDataContract(argumentContract);
            Assert.Equal(16, bindingDataContract.Count);
            Assert.Equal(bindingDataContract["DeliveryCount"], typeof(string));
            Assert.Equal(bindingDataContract["NewProperty"], typeof(decimal));
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("text/plain")]
        public void CreateBindingData_ReturnsExpectedValue(string contentType)
        {
            Message message = new Message(Encoding.UTF8.GetBytes("Test Message"))
            {
                ReplyTo = "test-queue",
                To = "test",
                ContentType = contentType,
                Label = "test label",
                CorrelationId = Guid.NewGuid().ToString(),
            };

            IReadOnlyDictionary<string, object> valueBindingData = null;

            var config = new ServiceBusOptions
            {
                ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=TestKey;SharedAccessKey=00000000000000000"
            };

            var messageReceiver = new MessageReceiver(config.ConnectionString, "test");
            var clientEntity = new QueueClient(config.ConnectionString, "test");
            Mock<IMessageSession> messageSession = new Mock<IMessageSession>();
            var bindingData = ServiceBusTriggerBinding.CreateBindingData(message, messageReceiver, messageSession.Object, valueBindingData);
            Assert.Equal(10, bindingData.Count);
            Assert.Equal(message.ReplyTo, bindingData["ReplyTo"]);
            Assert.Equal(string.Empty, bindingData["lockToken"]);
            Assert.Equal(message.To, bindingData["To"]);
            Assert.Equal(message.MessageId, bindingData["MessageId"]);
            Assert.Equal(message.ContentType, bindingData["ContentType"]);
            Assert.Equal(message.Label, bindingData["Label"]);
            Assert.Equal(message.CorrelationId, bindingData["CorrelationId"]);
            Assert.Same(message.UserProperties, bindingData["UserProperties"]);
            Assert.Same(messageReceiver, bindingData["MessageReceiver"]);
            Assert.Same(messageSession.Object, bindingData["MessageSession"]);

            // verify that value binding data overrides built ins
            valueBindingData = new Dictionary<string, object>
            {
                { "ReplyTo",  "override" },
                { "NewProperty", 123 }
            };
            bindingData = ServiceBusTriggerBinding.CreateBindingData(message, messageReceiver, messageSession.Object, valueBindingData);
            Assert.Equal(11, bindingData.Count);
            Assert.Equal("override", bindingData["ReplyTo"]);
            Assert.Equal(123, bindingData["NewProperty"]);
        }
    }
}
