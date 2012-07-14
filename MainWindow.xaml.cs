using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
namespace KinectSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member variables
        private KinectSensor _Kinect;
        private WriteableBitmap _colorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        #endregion Member variables

        #region Constructor
        
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => { DiscoverKinectSensor(); };
            this.Unloaded += (s, e) => { this.Kinect = null; };
        }

        #endregion Constructor

        #region Methods
        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged+= KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            MessageBox.Show("Kinect is connected");
        }

        private void KinectSensors_StatusChanged(Object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.Kinect == null)
                    {
                        this.Kinect = e.Sensor;
                        MessageBox.Show("Kinect is connected");
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (this.Kinect == e.Sensor)
                    {
                        this.Kinect = null;
                        this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Disconnected);
                          
                        if(this.Kinect == null)
                        { 
                            //Notify the user that the sensor is disconnected 
                            MessageBox.Show ("Kinect is Disconnected");

                        }
                    }
                    break;

            }
        }

        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable();
               
                this._colorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96,
                                                             PixelFormats.Bgr32, null);
                this._ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                this._ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorImageElement.Source = this._colorImageBitmap;
                sensor.ColorFrameReady += Kinect_ColorFrameReady;
                sensor.Start();
            }

        }

        private void UninitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.ColorFrameReady += Kinect_ColorFrameReady;
            }
        }

        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);

                    for (int i = 0; i < pixelData.Length; i += frame.BytesPerPixel)
                    {
                        //pixelData[i] = (byte)~pixelData[i];
                        //pixelData[i+1] = (byte)~pixelData[i+1];
                        //pixelData[i + 2] = (byte)~pixelData[i + 2];

                        byte gray = Math.Min(pixelData[i], pixelData[i + 1]);
                        gray = Math.Min(pixelData[i + 2], gray);

                        pixelData[i] = gray;
                        pixelData[i + 1] = gray;
                        pixelData[i + 2] = gray;

                        //if (i > 0 && i < pixelData.Length / 3)
                        //{
                            //pixelData[i + 2] = 0x00; //Red
                            //pixelData[i + 1] = 0x00; //Green
                        //}
                        //if (i > pixelData.Length / 3 && i < 2 * pixelData.Length / 3)
                        //{
                        //    pixelData[i] = 0x00; //Blue
                        //    pixelData[i + 1] = 0x00; //Green
                        //}
                       //else 
                       // {
                       //     pixelData[i + 2] = 0x00; //Red
                       //     pixelData[i] = 0x00; //Blue
                       // }
                    }

                    //ColorImageElement.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32,
                    //                                                null, pixelData, frame.Width * frame.BytesPerPixel);

                    this._colorImageBitmap.WritePixels(this._ColorImageBitmapRect, pixelData, 
                                                                this._ColorImageStride, 0);

                }

            }
        }


        #endregion Methods
        #region Properties
        public KinectSensor Kinect
        {
            get { return this._Kinect; }
            set 
            {
                if (this._Kinect != value)
                {
                    if (this._Kinect != null)
                    {
                        UninitializeKinectSensor(this._Kinect);
                        this._Kinect = null;
                    }

                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this._Kinect = value;
                        InitializeKinectSensor(this._Kinect);
                    }
                }
            }
        }


        #endregion Properties




    }
}
