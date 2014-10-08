﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Converters;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.WebJobs.Host.Tables.Converters
{
    internal class Int32ToEntityPropertyConverter : IConverter<int, EntityProperty>
    {
        public EntityProperty Convert(int input)
        {
            return new EntityProperty(input);
        }
    }
}
