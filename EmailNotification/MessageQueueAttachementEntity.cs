using System.IO;
using System.Net.Mime;

namespace EmailNotification
{
    public class MessageQueueAttachementEntity
    {
        public string FileName { get; set; }
        public ContentType ContentType { get; set; }
        public Stream ContentStream { get; set; }
    }
}
