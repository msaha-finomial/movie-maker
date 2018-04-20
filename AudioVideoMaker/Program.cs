using Azure.SpeechToText;
using FileDownloader;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VideoConverter;

namespace AudioVideoMaker
{

    class Program
    {
        const string queueConString = "Endpoint=sb://azureboot.servicebus.windows.net/;SharedAccessKeyName=admin;SharedAccessKey=2Pc/6iF4Tb5jV4mDmB/V15uyYTk5wrrFA3fj03+33ts=;EntityPath=slack";
        public static string GetPath(string filename)
        {
            string saveTo = Path.GetDirectoryName(Environment.CurrentDirectory) + @"\temp\";  //Folder to Save
            Console.WriteLine($"Saving to {saveTo}");
            if (!Directory.Exists(saveTo))
            {
                Directory.CreateDirectory(saveTo);
            }
            return saveTo + filename;
        }


        public static void Main(string[] args)
        {
            string audioPath = GetPath("audio.wav");
            string imageFolder = GetPath("");
            string outputVideoPath = GetPath($"bootcamp-video-{DateTime.Now.ToString("yyyyMMddHHmmSS")}.avi");

            var oQueueDownloader = new QueueDownloader();
            String textString = String.Empty;
            textString = oQueueDownloader.DownloadFromQueue().GetAwaiter().GetResult();

            bool isAudio = false;
            if (!String.IsNullOrEmpty(textString))
            {
                isAudio = true;
                AudioGenerator ag = new AudioGenerator();
                ag.Generate(textString, audioPath);
            }

            MovieMaker mm = new MovieMaker();
            mm.CreateVideoFromImages(imageFolder, outputVideoPath, 4);

            if (isAudio)
            {
                mm.AudioVideoJoiner(outputVideoPath, audioPath);
            }

            UploadVideo(outputVideoPath).GetAwaiter().GetResult();

            Console.WriteLine("Press any key to continue.");
            Console.Read();
        }

        static async Task UploadVideo(string outputVideoPath)
        {
            BlobStorageClient blobClient = new BlobStorageClient();
            string path = await blobClient.UploadVideo(File.ReadAllBytes(outputVideoPath), Path.GetFileName(outputVideoPath));
        }

    }
}
