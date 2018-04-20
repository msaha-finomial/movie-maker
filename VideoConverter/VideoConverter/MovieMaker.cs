using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Video.FFMPEG;
using AForge.Video;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;

//using Accord.Audio.Windows;
//using Microsoft.Xna.Framework.Audio;
//using AForge.Video.DirectShow;
using AForge.Video;
using Accord.Video.FFMPEG;

//using Accord.Controls.Audio;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Runtime.InteropServices;
using System.Media;
using AviFile;

namespace VideoConverter
{
    public class MovieMaker
    {
        int width = 320;
        int height = 240;
        int frameRate = 15;

        public Bitmap ToBitmap(byte[] byteArrayIn)
        {
            var ms = new System.IO.MemoryStream(byteArrayIn);
            var returnImage = Image.FromStream(ms);
            var bitmap = new Bitmap(returnImage);

            return bitmap;
        }

        public Bitmap ReduceBitmap(Bitmap original, int reducedWidth, int reducedHeight)
        {
            var reduced = new Bitmap(reducedWidth, reducedHeight);
            using (var dc = Graphics.FromImage(reduced))
            {
                // you might want to change properties like
                dc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                dc.DrawImage(original, new Rectangle(0, 0, reducedWidth, reducedHeight), new Rectangle(0, 0, original.Width, original.Height), GraphicsUnit.Pixel);
            }

            return reduced;
        }

        /*END OF COPIED CODE BLOCK*/
        public void CreateVideoFromImages(String dirFullPath, string outputVideoFullPath, int secsToPlay = 4)
        {
            try
            {
                using (var vFWriter = new VideoFileWriter())
                {
                    // create new video file
                    vFWriter.Open(outputVideoFullPath, width, height, frameRate);
                    DirectoryInfo d = new DirectoryInfo(dirFullPath);//Assuming Test is your Folder
                    FileInfo[] Files = d.GetFiles("*.jpg"); //Getting Text files
                    foreach (FileInfo file in Files)
                    {
                        Bitmap bmp = new Bitmap(file.FullName);
                        for (int i = 0; i < frameRate * secsToPlay; i++)
                        {
                            vFWriter.WriteVideoFrame(ReduceBitmap(bmp, width, height));
                        }
                    }
                    vFWriter.Close();
                }

            }
            catch (Exception e)
            {
            }
        }

        public bool AudioVideoJoiner(String videoPath, String audioPath)
        {
            bool success = false;
            try
            {
                AviManager aviManager = new AviManager(videoPath, true);
                aviManager.AddAudioStream(audioPath, 0);
                aviManager.Close();
                success = true;
            }
            catch(Exception e)
            {
                success = false;
            }
            return success;
        }
    }
}
