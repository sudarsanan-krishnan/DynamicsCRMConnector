//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common.Messaging
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Net.Mime;
    using Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator;

    /// <summary>
    /// This class denotes the content of file/document/message
    /// For a content: we need to know the stream/content string, its encoding type and the actual content-type
    /// </summary>
    public class Content
    {
        /// <summary>
        /// Initializes a new instance of the Content class.
        /// </summary>
        public Content()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Content class. To be used for string content.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        public Content(string content, string contentType) : this(content, contentType, TransferEncoding.QuotedPrintable)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the Content class. To be used for string content.
        /// </summary>
        public Content(string content, string contentType, TransferEncoding contentTransferEncoding)
        {
            this.ContentData = content;
            this.ContentTransferEncoding = contentTransferEncoding.ToString();
            this.ContentType = contentType;
        }

        /// <summary>
        /// /// <summary>
        /// Initializes a new instance of the Content class. To be used for stream content. Stream is converted to base64 encoded string and is saved to content data
        /// </summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="contentType"></param>
        public Content(Stream stream, string contentType)
            : this(stream, contentType, TransferEncoding.Base64)
        {
        }

        /// <summary>
        /// Constructor that when given a stream, converts to encoded string based on encoding provided and saves it to content data
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="contentType"></param>
        /// <param name="encoding"></param>
        private Content(Stream stream, string contentType, TransferEncoding encoding)
        {
            this.ContentData = encoding == TransferEncoding.Base64 ? System.Convert.ToBase64String(ReadStreamAsByteArray(stream)) : ReadStreamAsString(stream);
            this.ContentType = contentType;
            this.ContentTransferEncoding = encoding.ToString();
        }

        /// <summary>
        /// This will store the information in base64 encoded for streams and as-is for string type of data
        /// </summary>
        [Required]
        [CustomSummary("Content Data")]
        public string ContentData { get; set; }

        /// <summary>
        /// This denotes the Content-Type of Content-Data when it is decoded.
        /// </summary>
        [Required]
        [CustomSummary("Content Type")]
        public string ContentType { get; set; }

        /// <summary>
        /// This denotes the content-transfer encoding used to transfer content-data for base64 it is Base64, keep empty if no encoding is used.
        /// </summary>
        [Required]
        [CustomSummary("Content Transfer Encoding")]
        public string ContentTransferEncoding { get; set; }

        /// <summary>
        /// From ContentData converts to Stream format based on Content-Transfer Encoding
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Needed"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Needed")]
        public Stream GetStream()
        {
            MemoryStream stream = new MemoryStream();
            if (!string.IsNullOrEmpty(this.ContentData))
            {
                if (!string.IsNullOrEmpty(this.ContentTransferEncoding) && string.Equals(this.ContentTransferEncoding.ToString(), TransferEncoding.Base64.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    // to do change this to virtual file stream
                    byte[] bytes = System.Convert.FromBase64String(this.ContentData);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
                else
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(this.ContentData);
                    writer.Flush();
                }
            }

            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Utility function needed to read stream as string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>string</returns>
        /// <exception cref="System.ArgumentNullException">throws ArgumentNullException when stream is null</exception>
        public static string ReadStreamAsString(System.IO.Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
            }

            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        /// <summary>
        /// Utility functions needed to read stream as bytes
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>byte[]</returns>
        /// <exception cref="System.ArgumentNullException">throws ArgumentNullException when stream is null</exception>
        public static byte[] ReadStreamAsByteArray(System.IO.Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }

                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}