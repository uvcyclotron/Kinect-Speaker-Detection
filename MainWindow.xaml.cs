//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AudioBasics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;


 
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class AudioKinect : Window
    {
       

        /// <summary>
        /// Width of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        public const int EnergyBitmapWidth = 780;

        /// <summary>
        /// Height of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        private const int EnergyBitmapHeight = 195;

        /// <summary>
        /// Bitmap that contains constructed visualization for audio stream energy, ready to
        /// be displayed. It is a 2-color bitmap with white as background color and blue as
        /// foreground color.
        /// </summary>
        private readonly WriteableBitmap energyBitmap;

        /// <summary>
        /// Rectangle representing the entire energy bitmap area. Used when drawing background
        /// for energy visualization.
        /// </summary>
        private readonly Int32Rect fullEnergyRect = new Int32Rect(0, 0, EnergyBitmapWidth, EnergyBitmapHeight);

        /// <summary>
        /// Array of background-color pixels corresponding to an area equal to the size of whole energy bitmap.
        /// </summary>
        private readonly byte[] backgroundPixels = new byte[EnergyBitmapWidth * EnergyBitmapHeight];

       

        /// <summary>
        /// AudioKinect objects.
        /// </summary>
        private InitializeKinect[] audioKinect = new InitializeKinect[2];

        /// <summary>
        /// Count of number of AudioKinect objects.
        /// </summary>
        private int deviceCount = 0; //count of KinectCamera
        private int firstCheck = 0; //0 for first time, 1 for handler atached, 2 for after that

        private int byteArraySize = 0;

        private int[] voiceDetectionArr;
        
        /// <summary>
        /// Array of foreground-color pixels corresponding to a line as long as the energy bitmap is tall.
        /// This gets re-used while constructing the energy visualization.
        /// </summary>
        private byte[] foregroundPixels;

        /// <summary>
        /// Number of samples captured from Kinect audio stream each millisecond.
        /// </summary>
        private const int SamplesPerMillisecond = 16;

        /// <summary>
        /// Number of bytes in each Kinect audio stream sample.
        /// </summary>
        private const int BytesPerSample = 2;

        /// <summary>
        /// Number of audio samples represented by each column of pixels in wave bitmap.
        /// </summary>
        private const int SamplesPerColumn = 40;
        private byte[] colorPixels;

        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public AudioKinect()
        {
            InitializeComponent();
            this.energyBitmap = new WriteableBitmap(EnergyBitmapWidth, EnergyBitmapHeight, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.White, (Color)this.Resources["KinectPurpleColor"] }));

        }

        /// <summary>
        /// Execute initialization tasks.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    audioKinect[deviceCount] = new InitializeKinect(potentialSensor);
                    deviceCount++;
                    if (deviceCount >= 3)
                    {
                        break;
                    }
                }

                voiceDetectionArr = new int[deviceCount];


            }

            //if (null != this.sensor)
            //{
            //    try
            //    {
            //        // Start the sensor!
            //        this.sensor.Start();
            //    }
            //    catch (IOException)
            //    {
            //        // Some other application is streaming from the same Kinect sensor
            //        this.sensor = null;
            //    }
            //}

            //if (null == this.sensor)
            //{
            //    this.statusBarText.Text = Properties.Resources.NoKinectReady;
            //    return;
            //}

            // Initialize foreground pixels
            this.foregroundPixels = new byte[EnergyBitmapHeight];
            for (int i = 0; i < this.foregroundPixels.Length; ++i)
            {
                this.foregroundPixels[i] = 0xff;
            }

            this.waveDisplay.Source = this.energyBitmap;

            CompositionTarget.Rendering += UpdateEnergy;

            for (int j = 0; j < deviceCount; j++)
            {

                audioKinect[j].sensor.AudioSource.BeamAngleChanged += this.AudioSourceBeamChanged;
                audioKinect[j].sensor.AudioSource.SoundSourceAngleChanged += this.AudioSourceSoundSourceAngleChanged;
                audioKinect[j].sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            }

            //audioKinect[1].sensor.AudioSource.BeamAngleChanged += this.AudioSourceBeamChanged;
            //audioKinect[1].sensor.AudioSource.SoundSourceAngleChanged += this.AudioSourceSoundSourceAngleChanged;
            //audioKinect[0].sensor.AudioSource.BeamAngleChanged += this.AudioSourceBeamChanged;
            //audioKinect[0].sensor.AudioSource.SoundSourceAngleChanged += this.AudioSourceSoundSourceAngleChanged;

            /////start colorstream
            //audioKinect[0].sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            //audioKinect[1].sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            //define byteArraySize
            byteArraySize = this.audioKinect[0].sensor.ColorStream.FramePixelDataLength;

            //0 for now
            this.colorBitmap = new WriteableBitmap(audioKinect[0].sensor.ColorStream.FrameWidth, audioKinect[0].sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            //image window
            this.Image.Source = this.colorBitmap;
            

           
        }

        /// <summary>
        /// Execute uninitialization tasks.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            // Tell audio reading thread to stop and wait for it to finish.
            CompositionTarget.Rendering -= UpdateEnergy;

            //audioKinect[0].sensor.AudioSource.BeamAngleChanged -= this.AudioSourceBeamChanged;
            //audioKinect[0].sensor.AudioSource.SoundSourceAngleChanged -= this.AudioSourceSoundSourceAngleChanged;

            for (int j = 0; j < deviceCount; j++)
            {
                audioKinect[j].sensor.ColorStream.Disable();
                audioKinect[j].closeKinect();
            }

            //audioKinect[0].sensor.ColorStream.Disable();
            //audioKinect[1].sensor.ColorStream.Disable();
            //audioKinect[0].closeKinect();
            //audioKinect[1].closeKinect();

            //this.reading = false;
            //if (null != readingThread)
            //{
            //    readingThread.Join();
            //}

          
        }

        /// <summary>
        /// Handles event triggered when audio beam angle changes.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void AudioSourceBeamChanged(object sender, BeamAngleChangedEventArgs e)
        {
            beamRotation.Angle = -e.Angle;
            
            beamAngleText.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.BeamAngle, e.Angle.ToString("0", CultureInfo.CurrentCulture));
        }

        int count1 = 0;
        int count2 = 0;
        /// <summary>
        /// Handles event triggered when sound source angle changes.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void AudioSourceSoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            // Maximum possible confidence corresponds to this gradient width
            const double MinGradientWidth = 0.04;
            int maxIntensityDevice = 0;
            for (int j = 0; j < deviceCount; j++)
            {
                if (audioKinect[j].energy[audioKinect[j].energyIndex] > audioKinect[maxIntensityDevice].energy[audioKinect[maxIntensityDevice].energyIndex])
                    maxIntensityDevice = j;
            }

            if (audioKinect[maxIntensityDevice].energy[audioKinect[maxIntensityDevice].energyIndex] > 0.2)
            {
                System.Diagnostics.Debug.Print("Highest intensity on device#: " + audioKinect[0].sensor.UniqueKinectId);
                voiceDetectionArr[maxIntensityDevice]++;
                if (firstCheck == 0)
                {
                    audioKinect[maxIntensityDevice].sensor.ColorFrameReady += this.ColorFrameReady;
                    firstCheck = 1;
                }
                if (voiceDetectionArr[maxIntensityDevice] == 3) //this device has recorded highest intensity for this number of continuous frames 
                {
                    if (firstCheck == 1)
                    {
                        audioKinect[maxIntensityDevice].sensor.ColorFrameReady -= this.ColorFrameReady;
                        firstCheck = 2;
                    }

                    //audioKinect[1].sensor.ColorFrameReady -= this.ColorFrameReady;
                    //count2 = 0;
                    removeAllHandlers(maxIntensityDevice);
                    audioKinect[maxIntensityDevice].sensor.ColorFrameReady += this.ColorFrameReady;

                }
            }

            //if (audioKinect[0].energy[audioKinect[0].energyIndex] > 0.2 && audioKinect[0].energy[audioKinect[0].energyIndex] > audioKinect[1].energy[audioKinect[1].energyIndex])
            //{
            //    System.Diagnostics.Debug.Print("more:" + audioKinect[0].sensor.UniqueKinectId);
            //    //count2--;
            //    count1++;
            //    if (firstCheck == 0)
            //    {
            //        audioKinect[0].sensor.ColorFrameReady += this.ColorFrameReady;
            //        firstCheck = 1;
            //    }
            //    if (count1 == 3)
            //    {
            //        if (firstCheck ==1)
            //        {   audioKinect[0].sensor.ColorFrameReady -= this.ColorFrameReady;
            //            firstCheck =2;
            //        }

            //        audioKinect[1].sensor.ColorFrameReady -=  this.ColorFrameReady;
            //        count2 = 0;
            //        audioKinect[0].sensor.ColorFrameReady += this.ColorFrameReady;

            //    }
            //}

            //if (audioKinect[1].energy[audioKinect[1].energyIndex] > 0.2 && audioKinect[1].energy[audioKinect[1].energyIndex] > audioKinect[0].energy[audioKinect[0].energyIndex])
            //{
            //    System.Diagnostics.Debug.Print("more:" + audioKinect[1].sensor.UniqueKinectId);
            //    //count1--;
            //    count2++;
            //    if (firstCheck == 0)
            //    {
            //        audioKinect[1].sensor.ColorFrameReady += this.ColorFrameReady;
            //        firstCheck = 1;
            //    }

            //    if (count2 == 3)
            //    {
            //        if (firstCheck == 1)
            //        {
            //            audioKinect[1].sensor.ColorFrameReady -= this.ColorFrameReady;
            //            firstCheck = 2;
            //        }
            //        audioKinect[0].sensor.ColorFrameReady -= this.ColorFrameReady;
            //        count1 = 0;
            //        audioKinect[1].sensor.ColorFrameReady += this.ColorFrameReady;
            //    }
            //}

            // Set width of mark based on confidence.
            // A confidence of 0 would give us a gradient that fills whole area diffusely.
            // A confidence of 1 would give us the narrowest allowed gradient width.
            double halfWidth = Math.Max((1 - e.ConfidenceLevel), MinGradientWidth) / 2;

            // Update the gradient representing sound source position to reflect confidence
            this.sourceGsPre.Offset = Math.Max(this.sourceGsMain.Offset - halfWidth, 0);
            this.sourceGsPost.Offset = Math.Min(this.sourceGsMain.Offset + halfWidth, 1);

            // Rotate gradient to match angle
            sourceRotation.Angle = -e.Angle;

            sourceAngleText.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SourceAngle, e.Angle.ToString("0", CultureInfo.CurrentCulture));
            sourceConfidenceText.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SourceConfidence, e.ConfidenceLevel.ToString("0.00", CultureInfo.CurrentCulture));
        }

        private void removeAllHandlers(int deviceNo)
        {
            for (int i = 0; i < deviceCount; i++)
            {
                //remove event handler and reset detection count for all devices other than the device with max intensiy
                if (i != deviceNo)
                {
                    audioKinect[i].sensor.ColorFrameReady -= this.ColorFrameReady;
                    voiceDetectionArr[i] = 0;
                }
            }
        }


        private void ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    this.colorPixels = new byte[byteArraySize];

                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                    this.Image.Source = this.colorBitmap;
                }
            }
        }

        /// <summary>
        /// Handles rendering energy visualization into a bitmap.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void UpdateEnergy(object sender, EventArgs e)
        {
            lock (audioKinect[0].energyLock)
            {
                // Calculate how many energy samples we need to advance since the last update in order to
                // have a smooth animation effect
                DateTime now = DateTime.UtcNow;
                DateTime? previousRefreshTime = audioKinect[0].lastEnergyRefreshTime;
                audioKinect[0].lastEnergyRefreshTime = now;

                // No need to refresh if there is no new energy available to render
                if (audioKinect[0].newEnergyAvailable <= 0)
                {
                    return;
                }

                if (previousRefreshTime != null)
                {
                    double energyToAdvance = audioKinect[0].energyError + (((now - previousRefreshTime.Value).TotalMilliseconds * SamplesPerMillisecond) / SamplesPerColumn);
                    int energySamplesToAdvance = Math.Min(audioKinect[0].newEnergyAvailable, (int)Math.Round(energyToAdvance));
                    audioKinect[0].energyError = energyToAdvance - energySamplesToAdvance;
                    audioKinect[0].energyRefreshIndex = (audioKinect[0].energyRefreshIndex + energySamplesToAdvance) % audioKinect[0].energy.Length;
                    audioKinect[0].newEnergyAvailable -= energySamplesToAdvance;
                }

                // clear background of energy visualization area
                this.energyBitmap.WritePixels(fullEnergyRect, this.backgroundPixels, EnergyBitmapWidth, 0);

                // Draw each energy sample as a centered vertical bar, where the length of each bar is
                // proportional to the amount of energy it represents.
                // Time advances from left to right, with current time represented by the rightmost bar.
                int baseIndex = (audioKinect[0].energyRefreshIndex + audioKinect[0].energy.Length - EnergyBitmapWidth) % audioKinect[0].energy.Length;
                for (int i = 0; i < EnergyBitmapWidth; ++i)
                {
                    const int HalfImageHeight = EnergyBitmapHeight / 2;

                    // Each bar has a minimum height of 1 (to get a steady signal down the middle) and a maximum height
                    // equal to the bitmap height.
                    int barHeight = (int)Math.Max(1.0, (audioKinect[0].energy[(baseIndex + i) % audioKinect[0].energy.Length] * EnergyBitmapHeight));

                    // Center bar vertically on image
                    var barRect = new Int32Rect(i, HalfImageHeight - (barHeight / 2), 1, barHeight);

                    // Draw bar in foreground color
                    this.energyBitmap.WritePixels(barRect, foregroundPixels, 1, 0);
                }
            }
        }
    }


    class InitializeKinect
    {

        /// <summary>
        /// Number of milliseconds between each read of audio data from the stream.
        /// Faster polling (few tens of ms) ensures a smoother audio stream visualization.
        /// </summary>
        private const int AudioPollingInterval = 50;

        /// <summary>
        /// Number of samples captured from Kinect audio stream each millisecond.
        /// </summary>
        private const int SamplesPerMillisecond = 16;

        /// <summary>
        /// Number of bytes in each Kinect audio stream sample.
        /// </summary>
        private const int BytesPerSample = 2;

        /// <summary>
        /// Number of audio samples represented by each column of pixels in wave bitmap.
        /// </summary>
        private const int SamplesPerColumn = 40;

        /// <summary>
        /// Buffer used to hold audio data read from audio stream.
        /// </summary>
        private readonly byte[] audioBuffer = new byte[AudioPollingInterval * SamplesPerMillisecond * BytesPerSample];

        /// <summary>
        /// Sum of squares of audio samples being accumulated to compute the next energy value.
        /// </summary>
        private double accumulatedSquareSum;

        /// <summary>
        /// Number of audio samples accumulated so far to compute the next energy value.
        /// </summary>
        private int accumulatedSampleCount;

        /// <summary>
        /// Index of next element available in audio energy buffer.
        /// </summary>
        public int energyIndex;

        /// <summary>
        /// Number of newly calculated audio stream energy values that have not yet been
        /// displayed.
        /// </summary>
        public int newEnergyAvailable;

        /// <summary>
        /// Error between time slice we wanted to display and time slice that we ended up
        /// displaying, given that we have to display in integer pixels.
        /// </summary>
        public double energyError;

        /// <summary>
        /// Last time energy visualization was rendered to screen.
        /// </summary>
        public DateTime? lastEnergyRefreshTime;

        /// <summary>
        /// Index of first energy element that has never (yet) been displayed to screen.
        /// </summary>
        public int energyRefreshIndex;

        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        public KinectSensor sensor;

        /// <summary>
        /// Stream of audio being captured by Kinect sensor.
        /// </summary>
        private Stream audioStream;

        /// <summary>
        /// <code>true</code> if audio is currently being read from Kinect stream, <code>false</code> otherwise.
        /// </summary>
        private bool reading;

        /// <summary>
        /// Thread that is reading audio from Kinect stream.
        /// </summary>
        private Thread readingThread;

        public const int EnergyBitmapWidth = 780;


        /// <summary>
        /// Buffer used to store audio stream energy data as we read audio.
        /// 
        /// We store 25% more energy values than we strictly need for visualization to allow for a smoother
        /// stream animation effect, since rendering happens on a different schedule with respect to audio
        /// capture.
        /// </summary>
        public readonly double[] energy = new double[(uint)(EnergyBitmapWidth * 1.25)];

        /// <summary>
        /// Object for locking energy buffer to synchronize threads.
        /// </summary>
        public readonly object energyLock = new object();
        private KinectSensor potentialSensor;

        InitializeKinect()
        {
            this.sensor = null;

        }

        public InitializeKinect(KinectSensor sensor)
        {
            this.sensor = sensor;
            if (null != this.sensor)
            {
                try
                {
                    // Start the sensor!
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    // Some other application is streaming from the same Kinect sensor
                    this.sensor = null;
                }
            }
            

            // Start streaming audio!
            this.audioStream = this.sensor.AudioSource.Start();
            // Use a separate thread for capturing audio because audio stream read operations
            // will block, and we don't want to block main UI thread.
            this.reading = true;
            this.readingThread = new Thread(AudioReadingThread);
            this.readingThread.Start();
        }

        public void closeKinect()
        {
            this.reading = false;
            if (null != readingThread)
            {
                readingThread.Join();
            }

            if (null != this.sensor)
            {
                
                this.sensor.AudioSource.Stop();
                this.sensor.Stop();
                
            }
        }

        /// <summary>
        /// Handles polling audio stream and updating visualization every tick.
        /// </summary>
        private void AudioReadingThread()
        {
            // Bottom portion of computed energy signal that will be discarded as noise.
            // Only portion of signal above noise floor will be displayed.
            const double EnergyNoiseFloor = 0.25; //def was 2

            while (this.reading)
            {
                
                int readCount = audioStream.Read(audioBuffer, 0, audioBuffer.Length);
                
                // Calculate energy corresponding to captured audio.
                // In a computationally intensive application, do all the processing like
                // computing energy, filtering, etc. in a separate thread.
                
                    for (int i = 0; i < readCount; i += 2)
                    {
                        // compute the sum of squares of audio samples that will get accumulated
                        // into a single energy value.
                        short audioSample = BitConverter.ToInt16(audioBuffer, i);
                        this.accumulatedSquareSum += audioSample * audioSample;
                        ++this.accumulatedSampleCount;

                        if (this.accumulatedSampleCount < SamplesPerColumn)
                        {
                            continue;
                        }

                        // Each energy value will represent the logarithm of the mean of the
                        // sum of squares of a group of audio samples.
                        double meanSquare = this.accumulatedSquareSum / SamplesPerColumn;
                        double amplitude = Math.Log(meanSquare) / Math.Log(int.MaxValue);

                        // Renormalize signal above noise floor to [0,1] range.
                        this.energy[this.energyIndex] = Math.Max(0, amplitude - EnergyNoiseFloor) / (1 - EnergyNoiseFloor);
                        this.energyIndex = (this.energyIndex + 1) % this.energy.Length;

                        this.accumulatedSquareSum = 0;
                        this.accumulatedSampleCount = 0;
                        ++this.newEnergyAvailable;
                    }
                
            }
        }


    }
}

//changes noisefloor value to 4