using System;
using System.Runtime.Serialization;

namespace IisFtpEventGridNotifier
{
    [DataContract]
    public class EventGridEvent
    {
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [DataMember(Name = "eventTime")]
        public DateTime EventTime { get; set; } = DateTime.UtcNow;

        [DataMember(Name = "eventType")]
        public string EventType { get; set; }

        [DataMember(Name = "data")]
        public EventGridEventData EventData { get; set; }
    }

    [DataContract]
    public class EventGridEventData
    {
        [DataMember(Name = "statusCode")]
        public int StatusCode { get; set; }

        [DataMember(Name = "username")]
        public string UserName { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "size")]
        public long Size { get; set; }
    }
}
