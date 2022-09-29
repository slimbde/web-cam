using System;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using AForge.Video.DirectShow;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace Publisher
{
    class Program
    {
        private static IPEndPoint subscriberEndPoint;
        private static UdpClient udpClient;

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        static void Main(string[] args)
        {
            try
            {
                ShowWindow(GetConsoleWindow(), SW_HIDE);

                var ip = ConfigurationManager.AppSettings.Get("subscriberIp");
                var port = int.Parse(ConfigurationManager.AppSettings.Get("subscriberPort"));

                subscriberEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                udpClient = new UdpClient();

                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

                videoSource.NewFrame += videoSource_NewFrame;
                videoSource.Start();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        static void videoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            var bmp = new Bitmap(eventArgs.Frame, 640, 480);
            try
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();
                    udpClient.Send(bytes, bytes.Length, subscriberEndPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Concat("[Exception]: ", ex.Message));
            }
        }
    }
}
