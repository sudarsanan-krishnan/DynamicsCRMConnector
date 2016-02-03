//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a trace in MDS.
    /// </summary>
    public class MdsRow
    {
        /// <summary>
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// </summary>
        public List<TraceMessage> Messages { get; set; }

        /// <summary>
        /// </summary>
        public MdsRow()
        {
            Schema = "2015-03-01-preview_1.0";
            Messages = new List<TraceMessage>();
        }
    }

    /// <summary>
    /// Represents a single trace line.
    /// </summary>
    public class TraceMessage
    {
        /// <summary>
        /// </summary>
        public string TimeStamp { get; set; }

        /// <summary>
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// </summary>
        public string Message { get; set; }
    }
}
