﻿using BasicWebServer.Server.Common;
using BasicWebServer.Server.HTTP;
using System.Text;

namespace BasicWebServer.Server.Responses
{
    public class ContentResponse : Response
    {
        public ContentResponse(string content, string contentType)
            : base(StatusCode.OK)
        {
            Guard.AgainstNull(content);
            Guard.AgainstNull(contentType);

            this.Headers.Add(Header.CONTENT_TYPE, contentType);

            this.Body = content;
        }

        public override string ToString()
        {
            if (this.Body != null)
            {
                string contentLength = Encoding.UTF8.GetByteCount(this.Body).ToString();
                this.Headers.Add(Header.CONTENT_LENGTH, contentLength);
            }

            return base.ToString();
        }
    }
}
