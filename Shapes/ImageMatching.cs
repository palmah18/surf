using System;
using System.Collections.Generic;
using System.Linq;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.Flann;
#if !__IOS__
using Emgu.CV.Cuda;
#endif
using Emgu.CV.XFeatures2D;
namespace Shapes
{
    class ImageMatching
    {
        private const double surfHessianThresh = 300;
        private const bool surfExtendedFlag = true;
        private const int nOctaves = 4;
        private const int nOctaveLayers = 2;
        private SURF detector = new SURF(surfHessianThresh, nOctaves, nOctaveLayers, surfExtendedFlag);

        public class IndecesMapping
        {
            public int IndexStart { get; set; }
            public int IndexEnd { get; set; }
            public int Similarity { get; set; }
            public string fileName { get; set; }
        }

        public IList<IndecesMapping> Match()
        {
           /* string path = @"C:\Users\Palmah\Desktop\vägskyltar\";
            string[] dbImages = { path + "10kmh.jpg", path + "10kmh.jpg", path + "10kmh.jpg" }; */
            string queryImage = @"C:\Users\Palmah\Desktop\Magdalena\Photos\15071856.jpg";
            string[] dbImages = { @"C:\Users\Palmah\Desktop\vägskyltar\10kmh.jpg", @"C:\Users\Palmah\Desktop\vägskyltar\20kmh.jpg", @"C:\Users\Palmah\Desktop\vägskyltar\30kmh.jpg" };

            Console.WriteLine(1);
            IList<IndecesMapping> imap;

            // compute descriptors for each image
            var dbDescsList = ComputeMultipleDescriptors(dbImages, out imap);

            // concatenate all DB images descriptors into single Matrix
            Matrix<float> dbDescs = ConcatDescriptors(dbDescsList);

            // compute descriptors for the query image
            Matrix<float> queryDescriptors = ComputeSingleDescriptors(queryImage);

            FindMatches(dbDescs, queryDescriptors, ref imap);
            
            return imap;
        }

        

        public Matrix<float> ComputeSingleDescriptors(string fileName) // old return Matrix<float>
        {
            Mat descsTmp = new Mat();


            using (Image < Gray, byte> img = new Image<Gray, byte> (fileName))
{
                #region depreciated
                //VectorOfKeyPoint keyPoints = detector.DetectKeyPointsRaw(img, null);
                //descs = detector.ComputeDescriptorsRaw(img, null, keyPoints);
                #endregion

                VectorOfKeyPoint keyPoints = new VectorOfKeyPoint();
                detector.DetectAndCompute(img, null, keyPoints, descsTmp, false);
            }

            Matrix<float> descs = new Matrix<float>(descsTmp.Rows, descsTmp.Cols);
            descsTmp.CopyTo(descs);

            return descs;
        }

        public IList<Matrix<float>> ComputeMultipleDescriptors(string[] fileNames, out IList<IndecesMapping> imap)
        {
            imap = new List<IndecesMapping>();

            IList<Matrix<float>> descs = new List<Matrix<float>>();

            int r = 0;

            for (int i = 0; i < fileNames.Length; i++)
            {
                var desc = ComputeSingleDescriptors(fileNames[i]);
                descs.Add(desc);

                imap.Add(new IndecesMapping()
                {
                    fileName = fileNames[i],
                    IndexStart = r,
                    IndexEnd = r + desc.Rows - 1
                });

                r += desc.Rows;
            }

            return descs;
        }

        public void FindMatches(Matrix<float> dbDescriptors, Matrix<float> queryDescriptors, ref IList<IndecesMapping> imap)
        {
            var indices = new Matrix<int>(queryDescriptors.Rows, 2); // matrix that will contain indices of the 2-nearest neighbors found
            var dists = new Matrix<float>(queryDescriptors.Rows, 2); // matrix that will contain distances to the 2-nearest neighbors found
            Console.WriteLine(2);
            // create FLANN index with 4 kd-trees and perform KNN search over it look for 2 nearest neighbours
            //var indexParams = new LshIndexParams(10, 10, 0);
            KdTreeIndexParams indexParams = new KdTreeIndexParams(4);
            var flannIndex = new Index(dbDescriptors, indexParams);
            flannIndex.KnnSearch(queryDescriptors, indices, dists, 2, 24);
            Console.WriteLine(3);
            for (int i = 0; i < indices.Rows; i++)
            {
                // filter out all inadequate pairs based on distance between pairs
                if (dists.Data[i, 0] < (0.6 * dists.Data[i, 1]))
                {
                    // find image from the db to which current descriptor range belongs and increment similarity value.
                    // in the actual implementation this should be done differently as it's not very efficient for large image collections.
                    foreach (var img in imap)
                    {
                        if (img.IndexStart <= i && img.IndexEnd >= i)
                        {
                            img.Similarity++;
                            break;
                        }
                    }
                }
            }
        }

        public Matrix<float> ConcatDescriptors(IList<Matrix<float>> descriptors)
        {
            int cols = descriptors[0].Cols;
            int rows = descriptors.Sum(a => a.Rows);

            float[,] concatedDescs = new float[rows, cols];

            int offset = 0;

            foreach (var descriptor in descriptors)
            {
                // append new descriptors
                Buffer.BlockCopy(descriptor.ManagedArray, 0, concatedDescs, offset, sizeof(float) * descriptor.ManagedArray.Length);
                offset += sizeof(float) * descriptor.ManagedArray.Length;
            }

            return new Matrix<float>(concatedDescs);
        }

    }

}
