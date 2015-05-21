﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Converters;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace Microsoft.Azure.WebJobs.Host.Bindings.Invoke
{
    class ClassInvokeBinding<TValue> : IBinding
        where TValue : class
    {
        private static readonly IObjectToTypeConverter<TValue> _converter =
            ObjectToTypeConverterFactory.CreateForClass<TValue>();

        private readonly string _parameterName;

        public ClassInvokeBinding(string parameterName)
        {
            _parameterName = parameterName;
        }

        public bool FromAttribute
        {
            get { return false; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private Task<IValueProvider> BindAsync(TValue value, ValueBindingContext context)
        {
            IValueProvider provider = new ObjectValueProvider(value, typeof(TValue));
            return Task.FromResult(provider);
        }

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            TValue typedValue = null;

            if (!_converter.TryConvert(value, out typedValue))
            {
                throw new InvalidOperationException("Unable to convert value to " + typeof(TValue).Name + ".");
            }

            return BindAsync(typedValue, context);
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            throw new InvalidOperationException("No value was provided for parameter '" + _parameterName + "'.");
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new CallerSuppliedParameterDescriptor
            {
                Name = _parameterName
            };
        }
    }
}
