using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.IO;
using System.Media;
using System.Threading;
using static Azure.SpeechToText.InputOptions;

namespace Azure.SpeechToText
{
    public class Program
    {
        static String audioSavePath = "";
        static string filename = "audio.wav";
        private static void initialize()
        {
            string saveTo = Path.GetDirectoryName(Environment.CurrentDirectory) + @"\audio\";  //Folder to Save
            if (!Directory.Exists(saveTo))
            {
                Directory.CreateDirectory(saveTo);
            }
            audioSavePath = saveTo + filename;//@"\MSaha" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav";
        }
        static void Main(string[] args)
        {
            initialize();
            string text = "Hi this is Krishna from Finomial Technology pvt. Ltd., I hope you like my demo";
            TextToSpeech(text);
            Console.Read();
        }

        private static void TextToSpeech(String text)
        {
            try
            {
                var srService = new TextToSpeechService(Constants.Speech_API_Key);
                srService.OnAudioAvailable += SaveAudio;
                srService.OnError += ErrorHandler;
                Authorization auth = new Authorization(Constants.Speech_API_Key);
                var authToken = auth.GetAuthorizationTokenAsync().Result;
                var inputOptions = new InputOptions()
                {
                    RequestUri = new Uri(Constants.BASE_URI + "synthesize"),
                    Text = text,
                    Locale = "en-US",
                    VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)",
                    OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                    AuthorizationToken = "Bearer " + authToken,
                };
                srService.Speak(CancellationToken.None, inputOptions).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private static void SaveAudio(object sender, GenericEventArgs<Stream> args)
        {
            Stream readStream = args.EventData;

            try
            {
                FileStream writeStream = File.Create(audioSavePath);
                int Length = 256;
                Byte[] buffer = new Byte[Length];
                int bytestoRead = readStream.Read(buffer, 0, Length);
                while (bytestoRead > 0)
                {
                    writeStream.Write(buffer, 0, bytestoRead);
                    bytestoRead = readStream.Read(buffer, 0, Length);
                }
                readStream.Close();
                writeStream.Close();
                //Play audio
                //SoundPlayer player = new System.Media.SoundPlayer(filename);
                //player.PlaySync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                args.EventData.Dispose();
            }
        }
        private static void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
            Console.WriteLine("Unable to complete the TTS request: [{0}]", e.ToString());
        }

    }
}
