//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides logging capability to both MDS via Antares as well as .Net Trace.
    /// Its preferable to use the static logging methods over the instance methods, since they are more efficient.
    /// </summary>
    public class Logger
    {
        private Logger(string requestUri)
        {
            this.RequestUri = requestUri;
        }

        private string RequestUri { get; set; }

        #region Static Methods

        /// <summary>
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public static Logger Create(string requestUri)
        {
            return new Logger(requestUri);
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void LogMessage(HttpRequestMessage request, string message, params object[] parameters)
        {
            LogMessage(request, true, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="logToMdsOnly"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void LogMessage(HttpRequestMessage request, bool logToMdsOnly, string message, params object[] parameters)
        {
            LogInternal(request, string.Empty, EventLevel.Informational, logToMdsOnly, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void LogWarning(HttpRequestMessage request, string message, params object[] parameters)
        {
            LogWarning(request, true, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="logToMdsOnly"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void LogWarning(HttpRequestMessage request, bool logToMdsOnly, string message, params object[] parameters)
        {
            LogInternal(request, string.Empty, EventLevel.Warning, logToMdsOnly, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void LogError(HttpRequestMessage request, string message, params object[] parameters)
        {
            LogError(request, true, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="logToMdsOnly"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void LogError(HttpRequestMessage request, bool logToMdsOnly, string message, params object[] parameters)
        {
            LogInternal(request, string.Empty, EventLevel.Error, logToMdsOnly, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ex"></param>
        public static void LogException(HttpRequestMessage request, Exception ex)
        {
            LogInternal(request, string.Empty, EventLevel.Error, true, FormatException(ex));
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="logToMdsOnly"></param>
        /// <param name="ex"></param>
        public static void LogException(HttpRequestMessage request, bool logToMdsOnly, Exception ex)
        {
            LogInternal(request, request.RequestUri.ToString(), EventLevel.Error, logToMdsOnly, FormatException(ex));
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ex"></param>
        public static void LogExceptionAsWarning(HttpRequestMessage request, Exception ex)
        {
            LogInternal(request, string.Empty, EventLevel.Warning, true, FormatException(ex));
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="logToMdsOnly"></param>
        /// <param name="ex"></param>
        public static void LogExceptionAsWarning(HttpRequestMessage request, bool logToMdsOnly, Exception ex)
        {
            LogInternal(request, request.RequestUri.ToString(), EventLevel.Warning, logToMdsOnly, FormatException(ex));
        }

        /// <summary>
        /// Logs User error into application events. It also logs it into Mds logs as Warning (to avoid noise in the Mds logs).
        /// </summary>
        /// <param name="request"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void LogUserError(HttpRequestMessage request, string message, params object[] parameters)
        {
            string formattedMessage = GetFormattedMessage(message, parameters);
            LogApplicationEvent(EventLevel.Error, formattedMessage);
            LogMdsEvent(request, request.RequestUri.ToString(), EventLevel.Warning, formattedMessage, parameters);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public void LogMessage(string message, params object[] parameters)
        {
            LogMessage(true, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="logToMdsOnly"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public void LogMessage(bool logToMdsOnly, string message, params object[] parameters)
        {
            LogInternal(null, this.RequestUri, EventLevel.Informational, logToMdsOnly, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public void LogWarning(string message, params object[] parameters)
        {
            LogWarning(true, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="logToMdsOnly"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public void LogWarning(bool logToMdsOnly, string message, params object[] parameters)
        {
            LogInternal(null, this.RequestUri, EventLevel.Warning, logToMdsOnly, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public void LogError(string message, params object[] parameters)
        {
            LogError(true, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="logToMdsOnly"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public void LogError(bool logToMdsOnly, string message, params object[] parameters)
        {
            LogInternal(null, this.RequestUri, EventLevel.Error, logToMdsOnly, message, parameters);
        }

        /// <summary>
        /// </summary>
        /// <param name="ex"></param>
        public void LogException(Exception ex)
        {
            LogInternal(null, this.RequestUri, EventLevel.Error, true, FormatException(ex));
        }

        /// <summary>
        /// </summary>
        /// <param name="ex"></param>
        public void LogExceptionAsWarning(Exception ex)
        {
            LogInternal(null, this.RequestUri, EventLevel.Warning, true, FormatException(ex));
        }

        /// <summary>
        /// </summary>
        /// <param name="logToMdsOnly"></param>
        /// <param name="ex"></param>
        public void LogException(bool logToMdsOnly, Exception ex)
        {
            LogInternal(null, this.RequestUri, EventLevel.Error, logToMdsOnly, FormatException(ex));
        }

        #endregion

        private static void LogInternal(HttpRequestMessage request, string requestUri, EventLevel level, bool logToMdsOnly, string message, params object[] parameters)
        {
            string formattedMessage = GetFormattedMessage(message, parameters);

            LogMdsEvent(request, requestUri, level, formattedMessage, parameters);

            if (!logToMdsOnly)
            {
                LogApplicationEvent(level, formattedMessage);
            }
        }

        private static void LogMdsEvent(HttpRequestMessage request, string requestUri, EventLevel level, string formattedMessage, object[] parameters)
        {
            if (request != null)
            {
                object perRequestTrace;
                if (!request.Properties.TryGetValue("perRequestTrace", out perRequestTrace))
                {
                    perRequestTrace = new MdsRow();
                    request.Properties["perRequestTrace"] = perRequestTrace;
                }

                if (perRequestTrace != null)
                {
                    var mdsRow = perRequestTrace as MdsRow;
                    if (mdsRow != null)
                    {
                        mdsRow.Messages.Add(new TraceMessage()
                        {
                            TimeStamp = DateTime.UtcNow.ToString("o"),
                            Level = level.ToString(),
                            Message = formattedMessage
                        });
                    }
                }
            }
            else
            {
                
            }
        }

        private static void LogApplicationEvent(EventLevel level, string formattedMessage)
        {
            switch (level)
            {
                case EventLevel.Critical:
                case EventLevel.Error:
                    Trace.TraceError(formattedMessage);
                    break;
                case EventLevel.Warning:
                    Trace.TraceWarning(formattedMessage);
                    break;
                case EventLevel.Informational:
                    Trace.TraceInformation(formattedMessage);
                    break;
                default:
                    Trace.WriteLine(formattedMessage);
                    break;
            }
        }

        private static string GetFormattedMessage(string message, object[] parameters)
        {
            string formattedMessage;
            try
            {
                if (parameters != null && parameters.Length > 0)
                {
                    formattedMessage = string.Format(CultureInfo.InvariantCulture, message, parameters);
                }
                else
                {
                    formattedMessage = message;
                }
            }
            catch (FormatException)
            {
                formattedMessage = "Invalid trace parameters at - " + Environment.StackTrace;
            }
            catch (ArgumentNullException)
            {
                formattedMessage = "Invalid trace parameters at - " + Environment.StackTrace;
            }
            return formattedMessage;
        }

        private static string FormatException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            while (ex is AggregateException)
            {
                ex = ex.InnerException;
            }

            sb.Append("**Exception thrown: ");
            while (ex != null)
            {
                sb.AppendFormat("{0}, {1}, {2}", ex.GetType().FullName, ex.Message, ex.StackTrace);
                ex = ex.InnerException;
                if (ex != null)
                {
                    sb.AppendLine();
                    sb.Append("Inner: ");
                }
            }

            return sb.ToString();
        }
    }
}
