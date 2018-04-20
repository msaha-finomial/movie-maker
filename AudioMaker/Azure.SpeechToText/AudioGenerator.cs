using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.IO;
using System.Media;
using System.Threading;
using static Azure.SpeechToText.InputOptions;

namespace Azure.SpeechToText
{
    public class AudioGenerator
    {
        string audioSavePath = "";
        public bool Generate(String text, string path)
        {
            bool success = false;
            try
            {
                audioSavePath = path;
                //initialize();                
                TextToSpeech(text);
                success = true;
            }
            catch (Exception e)
            {
                //Console.WriteLine("Error: " + e.Message);
            }

            return success;
        }


        //string filename = @"audio.wav";
        //private void initialize()
        //{
        //    string saveTo = Path.GetDirectoryName(Environment.CurrentDirectory) + @"\audio\";  //Folder to Save
        //    if (!Directory.Exists(saveTo))
        //    {
        //        Directory.CreateDirectory(saveTo);
        //    }
        //    audioSavePath = saveTo + filename;
        //}


        private  void TextToSpeech(String text)
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
                //Console.WriteLine("Error: " + ex.Message);
            }
        }
        private void SaveAudio(object sender, GenericEventArgs<Stream> args)
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
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
            }
            finally
            {
                args.EventData.Dispose();
            }
        }
        private void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
            //Console.WriteLine("Unable to complete the TTS request: [{0}]", e.ToString());
        }

    }
}

