﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace Dashboard.ViewModels
{
    public class BlobBoundParamModel
    {
        public bool IsBlobOwnedByCurrentFunctionInstance { get; set; }
        public bool IsBlobMissing { get; set; }
        public bool IsConnectionStringMissing { get; set; }
        public string ConnectionStringKey { get; set; }
        public Guid OwnerId { get; set; }
        public bool IsOutput { get; set; }
    }
}
