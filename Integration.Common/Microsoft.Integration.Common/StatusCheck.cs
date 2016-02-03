//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System.Configuration;
    using System.Net.Http;

    /// <summary>
    /// Levels of a connector health status
    /// </summary>
    public enum StatusLevel
    {
        /// <summary>
        /// Error
        /// </summary>
        Error,

        /// <summary>
        /// Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Informational - all's well
        /// </summary>
        Info
    }

    /// <summary>
    /// Overall status of a connector's health - may include multiple status records
    /// </summary>
    public class StatusCheck
    {
        /// <summary>
        /// Collection of status records
        /// </summary>
        public StatusCheckEntry[] Status { get; set; }

        /// <summary>
        /// Log the status check entry details.
        /// </summary>
        /// <param name="request"></param>
        public void LogMessage(HttpRequestMessage request)
        {
            if (request != null)
            {
                int statusCheckEntryIndex = 0;
                foreach (StatusCheckEntry statusCheckEntry in this.Status)
                {
                    Logger.LogMessage(request, false, string.Format("Status check entry[{0}]", statusCheckEntryIndex));
                    statusCheckEntry.LogMessage(request);
                    ++statusCheckEntryIndex;
                }
            }
        }
    }

    /// <summary>
    /// Status data for a particular status check
    /// </summary>
    public class StatusCheckEntry
    {
        /// <summary>
        /// A name for the status code. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Human-readable message. 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Level of connector health status
        /// </summary>
        public StatusLevel Level { get; set; }

        /// <summary>
        /// Logs the details of this status check entry
        /// </summary>
        /// <param name="request"></param>
        public void LogMessage(HttpRequestMessage request)
        {
            Logger.LogMessage(request, false, string.Format("Level: {0}\nName: {1}\nMessage: {2}", this.Level.ToString(), this.Name, this.Message));
        }

        /// <summary>
        /// Constructs a status check entry with status Running.
        /// </summary>
        /// <returns>Status check entry</returns>
        public static StatusCheckEntry GetRunningStatusCheckEntry()
        {
            StatusCheckEntry statusCheckEntry = new StatusCheckEntry()
            {
                Level = StatusLevel.Info,
                Message = CommonResource.Running,
                Name = StatusMessages.ConnectorConfigurationValid
            };

            return statusCheckEntry;
        }
    }

    /// <summary>
    /// Status messages that could be used for logging
    /// </summary>
    public class StatusMessages
    {
        // Common status messages

        /// <summary>
        /// Access token is valid.
        /// </summary>
        public const string AccessTokenValid = "Access token is valid";

        /// <summary>
        /// Accesss token invalid.
        /// </summary>
        public const string AccessTokenInvalid = "Access token is invalid";

        /// <summary>
        /// Status check failed due to some unknown reason.
        /// </summary>
        public const string StatusCheckFailed = "Failed to determine status of connector";

        /// <summary>
        /// Connector configuration is valid.
        /// </summary>
        public const string ConnectorConfigurationValid = "Connector configuration is valid";

        /// <summary>
        /// Connector configuration is invalid.
        /// </summary>
        public const string ConnectorConfigurationInvalid = "Connector configuration is invalid";
    }
}
