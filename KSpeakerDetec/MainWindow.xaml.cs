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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeNui()
        {
            try
            {
                _kinectNui0 = KinectSensor.KinectSensors[0];
                _kinectNui1 = KinectSensor.KinectSensors[1];
                _kinectNui2 = KinectSensor.KinectSensors[2];

                _kinectNui0.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                _kinectNui0.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthFrameReady);
                _kinectNui0.Start();





            }
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
            using (Image = e.OpenDepthImageFrame())
            {
                if (Image != null)
                {
                    image0.Source = Image;
                }
            }
        }

    }
}
