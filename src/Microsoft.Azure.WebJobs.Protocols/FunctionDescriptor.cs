﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;

#if PUBLICPROTOCOL
namespace Microsoft.Azure.WebJobs.Protocols
#else
namespace Microsoft.Azure.WebJobs.Host.Protocols
#endif
{
    /// <summary>Represents an Azure WebJobs SDK function.</summary>
#if PUBLICPROTOCOL
    public class FunctionDescriptor
#else
    public class FunctionDescriptor
#endif
    {
        /// <summary>Gets or sets the ID of the function.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the fully qualified name of the function.</summary>
        public string FullName { get; set; }

        /// <summary>Gets or sets the display name of the function.</summary>
        public string ShortName { get; set; }

        /// <summary>Gets or sets the function's parameters.</summary>
        public IEnumerable<ParameterDescriptor> Parameters { get; set; }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> for this function
        /// </summary>
        [JsonIgnore]
        internal MethodInfo Method { get; set; }

#if PUBLICPROTOCOL
#else
        /// <summary>
        /// Gets the <see cref="Protocols.TriggerParameterDescriptor"/> for this function
        /// </summary>
        [JsonIgnore]
        internal TriggerParameterDescriptor TriggerParameterDescriptor { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Diagnostics.TraceLevel"/> for this function
        /// </summary>
        [JsonIgnore]
        internal System.Diagnostics.TraceLevel TraceLevel { get; set; }

        /// <summary>
        /// Gets the <see cref="WebJobs.TimeoutAttribute"/> for this function
        /// </summary>
        [JsonIgnore]
        internal TimeoutAttribute TimeoutAttribute { get; set; }

        /// <summary>
        /// Gets any <see cref="SingletonAttribute"/>s for this function
        /// </summary>
        [JsonIgnore]
        internal IEnumerable<SingletonAttribute> SingletonAttributes { get; set; }
#endif
    }
}
