﻿using BasicWebServer.Server.HTTP;

namespace BasicWebServer.Server.Responses
{
    public class TextFileResponse : Response
    {
        public string FileName { get; init; }
        public TextFileResponse(string fileName) : base(StatusCode.OK)
        {
            this.FileName = fileName;

            this.Headers.Add(Header.CONTENT_TYPE, ContentType.PLAIN_TEXT);
        }
        public override string ToString()
        {
            if (File.Exists(this.FileName))
            {
                this.Body = File.ReadAllTextAsync(this.FileName).Result;

                var fileBytesCount = new FileInfo(this.FileName).Length;
                this.Headers.Add(Header.CONTENT_LENGTH, fileBytesCount.ToString());

                this.Headers.Add(Header.CONTENT_DISPOSITION, $"attachmentl filename=\"{this.FileName}\"");
            }

            return base.ToString();
        }
    }
}
