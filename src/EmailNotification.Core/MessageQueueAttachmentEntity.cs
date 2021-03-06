﻿using System.IO;
using System.Net.Mime;

namespace EmailNotification.Core
{
    public class MessageQueueAttachmentEntity
    {
        public string FileName { get; set; }
        public ContentType ContentType { get; set; }
        public Stream ContentStream { get; set; }
    }
}
