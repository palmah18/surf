using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Emgu.CV.Structure;
using HoornIsBoss;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Win32;
using Emgu.CV.Util;
using System.Diagnostics;
using SURFFeatureExample;


namespace Shapes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private static Image<Bgr, byte> detectCircle(Image<Bgr, byte> image)
        {
            HoornTimer timer = HoornTimer.Instance;
           StringBuilder msgBuilder = new StringBuilder("Performance: ");
            timer.Start();
            //Convert the image to grayscale and filter out the noise
            UMat uimage = new UMat();
            CvInvoke.CvtColor(image, uimage, ColorConversion.Bgr2Gray);
            timer.Stop();
            timer.calculateDiff("1");

            //use image pyr to remove noise
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);

            Image<Gray, Byte> gray = image.Convert<Gray, Byte>().PyrDown().PyrUp();

            #region circle detection
            Stopwatch watch = Stopwatch.StartNew();
            double cannyThreshold = 180.0;
            double circleAccumulatorThreshold = 120;
            CircleF[] circles = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 2.0, 20.0, cannyThreshold, circleAccumulatorThreshold, 5);

            watch.Stop();
            msgBuilder.Append(String.Format("Hough circles - {0} ms; ", watch.ElapsedMilliseconds));
            #endregion

            #region Canny and edge detection
            watch.Reset(); watch.Start();
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            LineSegment2D[] lines = CvInvoke.HoughLinesP(
               cannyEdges,
               1, //Distance resolution in pixel-related units
               Math.PI / 45.0, //Angle resolution measured in radians.
               20, //threshold
               30, //min Line width
               10); //gap between lines


            watch.Stop();
            msgBuilder.Append(String.Format("Canny & Hough lines - {0} ms; ", watch.ElapsedMilliseconds));
            #endregion


            #region draw circles
            timer.Start();
            Image<Bgr, Byte> circleImage = image;
            Image<Bgr, Byte> mask = new Image<Bgr, byte>(image.Width, image.Height);
            Image<Bgr, byte> dest = new Image<Bgr, byte>(image.Width, image.Height);
            Image<Bgr, Byte> newUimage = cannyEdges.ToImage<Bgr, Byte>();
       
         
            foreach (CircleF circle in circles)
            {
                //circleImage.Draw(circle, new Bgr(System.Drawing.Color.Blue), -2);
                CvInvoke.Circle(mask, System.Drawing.Point.Round(circle.Center), (int)circle.Radius, new Bgr(System.Drawing.Color.Brown).MCvScalar, -1); // -1 fill ellipse
                dest = image.And(image, mask.Convert<Gray, byte>());

            }
            timer.Stop();
            timer.calculateDiff("2");
            return dest;
            #endregion

        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            
            OpenFileDialog openPic = new OpenFileDialog();
            if (openPic.ShowDialog() == true)

            {
                //load image into window
                Image<Bgr, byte> originalImage = new Image<Bgr, byte>(openPic.FileName);
                Image<Bgr, byte> croppedImage = detectCircle(originalImage);
                detectRectangle.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(croppedImage);
                HoornTimer timer = HoornTimer.Instance;
                timer.Start();
                 myImage.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(originalImage);
                 Image<Bgr, byte> compareImage = new Image<Bgr, byte>(@"C:\Users\Palmah\Desktop\exjobb\30skylt.jpg");
                 Image<Gray, byte> grayCompareImage = compareImage.Convert<Gray, byte>();
                timer.Stop();
                timer.calculateDiff("ToBitmapSource");
                 //change image to gray scale
                 Image<Gray, byte> grayImage = croppedImage.Convert<Gray, byte>();
                myGreyImage.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(grayImage);
                // myGreyImage.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(grayImage);

                long matchTime;
                 VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
                 Mat img = new Mat();
                 Mat originalMat = new Mat();
                 Mat result = new Mat();
                 originalMat = grayImage.Mat;
                 img = grayCompareImage.Mat;
                 result = DrawMatches.Draw(originalMat, img, out matchTime);
                              
                 myGreyImage.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(result);

                
            }

        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
