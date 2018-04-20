using FileDownloader;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AudioVideoMaker
{
    public class QueueDownloader
    {
        public String GetBody(BrokeredMessage brokeredMessage)
        {
            Stream stream = brokeredMessage.GetBody<Stream>();
            StreamReader reader = new StreamReader(stream);
            string s = reader.ReadToEnd();
            return s;
        }
        public async Task<string> DownloadFromQueue()
        {
            BlobStorageClient blobClient = new BlobStorageClient();
            string connectionString = Convert.ToString(ConfigurationManager.AppSettings["ServiceBus.ConnectionString"]);
            QueueClient qClient = QueueClient.CreateFromConnectionString(connectionString);
            // Continuously process messages sent to the "TestQueue" 
            StringBuilder sb = new StringBuilder();

            BrokeredMessage message = null;
            do
            {
                message = qClient.Receive();
                if (message != null)
                {
                    try
                    {
                        String msg = message.GetBody<String>();
                        var slMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackMessage>(msg);
                        if (!String.IsNullOrEmpty(slMsg.filePath))
                        {
                            string fn = Path.GetFileName(slMsg.filePath);
                            var res = await blobClient.DownloadImage(fn);
                            Console.WriteLine("File found :" + slMsg.filePath);
                        }
                        else if (!String.IsNullOrEmpty(slMsg.text) && !String.IsNullOrWhiteSpace(slMsg.text))
                        {
                            Console.WriteLine("Text found :" + slMsg.text);
                            sb.Append(slMsg.text);
                            if (!slMsg.text.EndsWith("."))
                            {
                                sb.Append(". ");
                            }
                        }
                        message.Complete();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        // Indicate a problem, unlock message in queue                      
                        message.Abandon();
                    }
                }
            } while (message != null);

            string textString = sb.ToString();
            return textString;
        }

    }
}
