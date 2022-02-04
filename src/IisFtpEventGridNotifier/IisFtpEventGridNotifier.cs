using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Web.FtpServer;

namespace IisFtpEventGridNotifier
{
    public class IisFtpEventGridNotifier : BaseProvider, IFtpLogProvider
    {
        private string logFilePath;
        private string eventGridEndPoint;
        private string eventGridKey;
        private string eventGridSubjectPrefix;

        /* We are using DataContractJsonSerializer to get as minimal dependencies as possible */
        private readonly DataContractJsonSerializer eventSerializer = new DataContractJsonSerializer(typeof(EventGridEvent[]), new DataContractJsonSerializerSettings
        {
            DateTimeFormat = new DateTimeFormat("yyyy-MM-dd HH:mm:ss"),
        });

        private string SerializeEvent(EventGridEvent eventGridEvent)
        {
            using (var ms = new MemoryStream())
            {
                eventSerializer.WriteObject(ms, new EventGridEvent[] { eventGridEvent });
                /* DataContractJsonSerializer escapes forward slashes (wich is allowed cf. JSON spec), but we don't care for it here. */
                return Encoding.UTF8.GetString(ms.ToArray()).Replace("\\/", "/");
            }
        }

        private void LogToFile(string eventJson)
        {
            try
            {
                if (!string.IsNullOrEmpty(logFilePath))
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                        writer.WriteLine(eventJson);
                    }
                }
            }
            catch 
            { 
                /* NO-OP */
            }
        }

        /// <summary>
        /// Public Configuration, which allow us to test this more easily.
        /// </summary>
        /// <param name="config">Configuration Object</param>
        public void Configure(StringDictionary config)
        {
            // Retrieve the provider settings from configuration.
            if (config.ContainsKey("logFilePath"))
                logFilePath = config["logFilePath"];
            eventGridEndPoint = config["eventGridEndPoint"];
            eventGridKey = config["eventGridKey"];
            eventGridSubjectPrefix = config["eventGridSubjectPrefix"];

            if (string.IsNullOrEmpty(eventGridEndPoint))
            {
                throw new ArgumentException(
                  $"Missing {nameof(eventGridEndPoint)} value in configuration.");
            }

            if (string.IsNullOrEmpty(eventGridKey))
            {
                throw new ArgumentException(
                  $"Missing {nameof(eventGridKey)} value in configuration.");
            }

            if (string.IsNullOrEmpty(eventGridSubjectPrefix))
            {
                throw new ArgumentException(
                  $"Missing {nameof(eventGridSubjectPrefix)} value in configuration.");
            }
        }

        protected override void Initialize(StringDictionary config)
        {
            Configure(config);
        }

        public void Log(FtpLogEntry logEntry)
        {
            try
            {
                // Test for a file upload operation.
                if (logEntry.Command == "STOR")
                {
                    var eventGridEvent = new EventGridEvent
                    {
                        Subject = eventGridSubjectPrefix + logEntry.FullPath,
                        EventData = new EventGridEventData()
                        {
                            StatusCode = logEntry.FtpStatus,
                            UserName = logEntry.UserName,
                            Path = logEntry.FullPath,
                            Size = logEntry.BytesReceived
                        }
                    };

                    if (logEntry.FtpStatus == 226)
                        eventGridEvent.EventType = "FileAvailable";
                    else
                        eventGridEvent.EventType = "Other:" + logEntry.FtpStatus.ToString();

                    var eventJson = SerializeEvent(eventGridEvent);

                    /* Log Event */
                    LogToFile(eventJson);

                    /* Send to Event Grid */
                    var httpClient = new HttpClient();
                    var httpContent = new StringContent(eventJson, Encoding.UTF8, "application/json");
                    httpContent.Headers.Add("aeg-sas-key", eventGridKey);

                    var sendTask = Task.Run(() => httpClient.PostAsync(eventGridEndPoint, httpContent));
                    sendTask.Wait();
                    var response = sendTask.Result;

                    var receiveTask = Task.Run(() => response.Content.ReadAsStringAsync());
                    receiveTask.Wait();
                    var responseString = receiveTask.Result;

                    /* Log Response */
                    LogToFile(responseString);
                }
            }
            catch (Exception ex)
            {
                LogToFile("ERROR: " + ex.Message);
            }
        }       
    }
}
