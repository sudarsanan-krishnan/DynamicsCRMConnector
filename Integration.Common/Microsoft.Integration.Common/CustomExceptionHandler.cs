//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;

    /// <summary>
    /// Handling all the exceptions from the controllers and returning appropriate exception messages.
    /// </summary>
    public class CustomExceptionHandler : ExceptionHandler
    {
        /// <summary>
        /// Should we handle the exceptions or not
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
        }

        /// <summary>
        /// Handle the Exception
        /// </summary>
        /// <param name="context"></param>
        public override void Handle(ExceptionHandlerContext context)
        {
            context.Result = new ErrorResult(context.Request, context.Exception);
        }

        private class ErrorResult : IHttpActionResult
        {
            public ErrorResult(HttpRequestMessage request, Exception exception)
            {
                Request = request;
                Exception = exception;
            }

            public Exception Exception { get; set; }

            public HttpRequestMessage Request { get; set; }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                HttpResponseMessage responseMessage = HttpHelpers.CreateResponseMessageFromException(Exception, Request);
                return Task.FromResult(responseMessage);
            }
        }
    }
}
