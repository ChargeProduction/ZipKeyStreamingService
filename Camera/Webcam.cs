using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;
using ZipKeyStreamingService.Interface;
using ZipKeyStreamingService.Interface.Payload;

namespace ZipKeyStreamingService.Camera
{
    public class Webcam
    {
        private static VideoCaptureDevice videoDevice;
        private static bool hasStarted = false;

        public static void Init()
        {
            //Anlegen einer Liste mit allen Videoquellen. (Hier können neben der gewünschten Webcam
            //auch TV-Karten, etc. auftauchen)
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            //Überprüfen, ob mindestens eine Aufnahmequelle vorhanden ist
            if (videoDevices != null)
            {
                //Die erste Aufnahmequelle an unser Webcam Objekt binden
                //(habt ihr mehrere Quellen, muss nicht immer die erste Quelle die
                //gewünschte Webcam sein!)
                videoDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);

                try
                {
                    //Überprüfen ob die Aufnahmequelle eine Liste mit möglichen Aufnahme-
                    //Auflösungen mitliefert.
                    if (videoDevice.VideoCapabilities.Length > 0)
                    {
                        string highestSolution = "0;0";
                        //Das Profil mit der höchsten Auflösung suchen
                        for (int i = 0; i < videoDevice.VideoCapabilities.Length; i++)
                        {
                            if (videoDevice.VideoCapabilities[i].FrameSize.Width > 1920) continue;
                            if (videoDevice.VideoCapabilities[i].FrameSize.Width >
                                System.Convert.ToInt32(highestSolution.Split(';')[0]))
                                highestSolution = videoDevice.VideoCapabilities[i].FrameSize.Width.ToString() + ";" +
                                                  i.ToString();
                        }

                        //Dem Webcam Objekt ermittelte Auflösung übergeben
                        videoDevice.VideoResolution =
                            videoDevice.VideoCapabilities[System.Convert.ToInt32(highestSolution.Split(';')[1])];
                    }
                }
                catch
                {
                }

                videoDevice.NewFrame += new AForge.Video.NewFrameEventHandler(OnFrame);
                new Thread(CheckClientStatesThread)
                {
                    Priority = ThreadPriority.Lowest
                }.Start();
            }

        }

        private static void CheckClientStatesThread()
        {
            while (Server.IsRunning)
            {
                var activeClient = Server.GetClient(client => client.State.ReceiveCameraOutput) != null;

                if (activeClient)
                {
                    if (!hasStarted)
                    {
                        hasStarted = true;
                        videoDevice.Start();
                    }
                }
                else if (hasStarted)
                {
                    videoDevice.Stop();
                    hasStarted = false;
                }

                Thread.Sleep(100); // Updates every 100ms
            }
            if (videoDevice.IsRunning)
            {
                videoDevice.Stop();
                hasStarted = false;
            }
        }

        private static void OnFrame(object sender, NewFrameEventArgs eventargs)
        {
            using (var memStream = new MemoryStream())
            {
                eventargs.Frame.Save(memStream, ImageFormat.Jpeg);

                var cameraDataPayload = new CameraDataPayload
                {
                    JpegData = memStream.GetBuffer()
                };

                Server.Broadcast(client =>
                {
                    if (client.State.ReceiveCameraOutput)
                    {
                        client.Send(cameraDataPayload);
                    }
                });
            }
        }
    }
}
