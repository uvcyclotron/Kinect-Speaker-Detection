using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging; 
using System.Runtime.InteropServices;

//import kinect libs
using Microsoft.Kinect;

namespace KSpeakerDetec
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //init 3 instances of kinect sensor..
        KinectSensor _kinectNui0;
        KinectSensor _kinectNui1;
        KinectSensor _kinectNui2;
        Bitmap bm,bmap;
        byte[] pixeldata;

        private int DeviceCount { get; set; }
        List<KinectSensor> kinectDevices = new List<KinectSensor>();

        private const string DeviceName = "Kinect Sensors Device";

        public MainWindow()
        {
            InitializeComponent();
           // Loaded += new RoutedEventHandler(MainWindow_Loaded);
           // Unloaded += new RoutedEventHandler(MainWindow_Unloaded);

        }

        //del
        /*
        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (KinectSensor device in kinectDevices)
            {
                //device.KinectRuntime.Uninitialize(); //legacy code
                device.Stop();
            }
        }
        
        //del
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.buttonKinectOne.IsEnabled = false;
            this.buttonKinectTwo.IsEnabled = false;
        }
         */

        private void btn_Detect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                KinectSensor device = new KinectSensor();
                this.DeviceCount = KinectSensor.KinectSensors.Count;
                this.btn_Detect.Content = string.Format("Kinect Device Detected {0}", this.DeviceCount.ToString());
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Error Occured in Device Detection : ", exp.Message));
            }
        }

       /* private void buttonInitializeRuntime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < this.DeviceCount; i++)
                {
                    kinectDevices.Add(new KinectSensor
                    {
                        KinectRuntime = new Runtime(i),
                        DeviceName = string.Format("{0} {1}",
                        DeviceName, i.ToString())
                    });
                }

               // this.buttonKinectOne.IsEnabled = true;
               // this.buttonKinectTwo.IsEnabled = true;
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Runtime Initialization Failed : ", exp.Message));
            }
        }*/

        private void InitializeNui()
        {
            try
            {   
                //check for 3 sensors
                if(DeviceCount >=3)
                {

                _kinectNui0 = KinectSensor.KinectSensors[0];
                _kinectNui1 = KinectSensor.KinectSensors[1];
                _kinectNui2 = KinectSensor.KinectSensors[2];

                _kinectNui0.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                _kinectNui0.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorFrameReady0);
                _kinectNui0.Start();

                _kinectNui1.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                _kinectNui1.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorFrameReady1);
                _kinectNui1.Start();

                _kinectNui2.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                _kinectNui2.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorFrameReady2);
                _kinectNui2.Start();


                    /*
                _kinectNui0.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                _kinectNui0.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthFrameReady);
                _kinectNui0.Start();

                _kinectNui1.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                _kinectNui1.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthFrameReady);
                _kinectNui1.Start();

                _kinectNui2.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                _kinectNui2.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthFrameReady);
                _kinectNui2.Start();*/

                 }
            }//end try block
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        } 

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            InitializeNui();
        }

        void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            DepthImageFrame Image;
           /* using (Image = e.OpenDepthImageFrame())
            {
                if (Image != null)
                {
                    image0.Source = Image; //this wont work

                }
            }*/
        }

        void ColorFrameReady0(object sender, ColorImageFrameReadyEventArgs e)
        {
            ColorImageFrame Image;
            using (Image = e.OpenColorImageFrame())
            {
                if (Image != null)
                {
                    //image0.Source = ImageToBitmap(Image);

                    byte[] bits = new byte[Image.PixelDataLength];
                    Image.CopyPixelDataTo(bits);
                    image0.Source = BitmapSource.Create(Image.Width, Image.Height, 96, 96,PixelFormats.Bgr32, null, bits, Image.Width * Image.BytesPerPixel);
                    }
                }
            }
        void ColorFrameReady1(object sender, ColorImageFrameReadyEventArgs e)
        {
            ColorImageFrame Image;
            using (Image = e.OpenColorImageFrame())
            {
                if (Image != null)
                {
                    //image0.Source = ImageToBitmap(Image);

                    byte[] bits = new byte[Image.PixelDataLength];
                    Image.CopyPixelDataTo(bits);
                    image1.Source = BitmapSource.Create(Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, bits, Image.Width * Image.BytesPerPixel);
                }
            }
        }
        void ColorFrameReady2(object sender, ColorImageFrameReadyEventArgs e)
        {
            ColorImageFrame Image;
            using (Image = e.OpenColorImageFrame())
            {
                if (Image != null)
                {
                    //image0.Source = ImageToBitmap(Image);

                    byte[] bits = new byte[Image.PixelDataLength];
                    Image.CopyPixelDataTo(bits);
                    image2.Source = BitmapSource.Create(Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, bits, Image.Width * Image.BytesPerPixel);
                }
            }
        }

        
        private void BtnStopClick(object sender, RoutedEventArgs e)
        {
            try
            {

                _kinectNui0.Stop();
                _kinectNui1.Stop();
                _kinectNui2.Stop();
                btnStop.IsEnabled = false;
                btnStart.IsEnabled = true;
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Some error occurred. Please ensure that Kinect is connected properly.");
            }

        }

        private void BtnStartClick(object sender, RoutedEventArgs e)
        {


            InitializeNui();
            //btnStop.IsEnabled = true;
            //btnStart.IsEnabled = false;
        }

        Bitmap ImageToBitmap(ColorImageFrame Image)
        {

            if (Image != null)
            {
                if (pixeldata == null)
                {
                    pixeldata = new byte[Image.PixelDataLength];
                }

                Image.CopyPixelDataTo(pixeldata);
                bmap = new Bitmap(Image.Width, Image.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                BitmapData bmapdata = bmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height),
                    ImageLockMode.WriteOnly,
                    bmap.PixelFormat);
                IntPtr ptr = bmapdata.Scan0;
                Marshal.Copy(pixeldata, 0, ptr, Image.PixelDataLength);
                bmap.UnlockBits(bmapdata);
                return bmap;
            }
            // byte[] pixeldata = new byte[Image.PixelDataLength];
            // Image.CopyPixelDataTo(pixeldata);
            else
            {
                return null;
            }
        }

        Bitmap DepthToBitmap(DepthImageFrame imageFrame)
        {
            short[] pixelData = new short[imageFrame.PixelDataLength];
            imageFrame.CopyPixelDataTo(pixelData);

            Bitmap bmap = new Bitmap(
            imageFrame.Width,
            imageFrame.Height,
            System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            BitmapData bmapdata = bmap.LockBits(
             new System.Drawing.Rectangle(0, 0, imageFrame.Width,
                                    imageFrame.Height),
             ImageLockMode.WriteOnly,
             bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            Marshal.Copy(pixelData,
             0,
             ptr,
             imageFrame.Width *
               imageFrame.Height);
            bmap.UnlockBits(bmapdata);
            return bmap;
        }
        /*
        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                     ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                int f = API.DeleteObject(ptr);

                return bs;
            }
        }*/



    }
}
