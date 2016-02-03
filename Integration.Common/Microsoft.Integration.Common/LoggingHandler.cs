//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// A DelegatingHandler that captures traces associated to a request into MDS.
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        /// <summary>
        /// Calls in the next handler.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            string requestStartTime = DateTime.UtcNow.ToString("o");

            Stopwatch sw = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            sw.Stop();

            object perRequestTrace;
            if (request.Properties.TryGetValue("perRequestTrace", out perRequestTrace))
            {
                var mdsRow = perRequestTrace as MdsRow;
                if (mdsRow != null)
                {
                    mdsRow.HttpMethod = request.Method.ToString();
                    string details = JsonConvert.SerializeObject(mdsRow);
                    
                }
            }

            return response;
        }
    }
}
