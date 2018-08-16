using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static ClassificationTool.ImageHelper;


namespace ClassificationTool
{
    public class FeaturePoint
    {
        public static int NFEATURES
        {
            get
            {
                int tmp = 0;
                if (RGBEnable) tmp += 3;
                if (HSLEnable) tmp += 3;
                if (XYZEnable) tmp += 3;
                if (GLCMEnable) tmp += 24;
                if (HOGEnable) tmp += 9;
                if (LBPEnable) tmp += 24;
                if (HeightEnable) tmp += 1;
                if (NormalEnable) tmp += 3;
                if (SlopeEnable) tmp += 1;

                return tmp;
            }
        }

        public static bool RGBEnable = true;
        public static bool HSLEnable = true;
        public static bool XYZEnable = true;
        public static bool GLCMEnable = true;
        public static bool HOGEnable = true;
        public static bool LBPEnable = true;
        public static bool HeightEnable = true;
        public static bool NormalEnable = true;
        public static bool SlopeEnable = true;



        private double[] GLCMResults;
        private double[] HOGResults;
        private double[] LBPResults;


        public string imageId;
        public int x;
        public int y;

        private static ConcurrentDictionary<string, FeaturePoint[,]> AllFeaturesPoint = new ConcurrentDictionary<string, FeaturePoint[,]>();

        public static FeaturePoint GetOrAddFeaturePoint(int _x, int _y, string _imageKey)
        {
            FeaturePoint f = AllFeaturesPoint.GetOrAdd(ImageHelper.imageTiles[_imageKey].baseFileName, new FeaturePoint[256, 256])[_x, _y];

            if (f == null)
            {
                f = new FeaturePoint(_x, _y, _imageKey);
                AllFeaturesPoint[ImageHelper.imageTiles[_imageKey].baseFileName][_x, _y] = f;
            }
            return f;
        }

        public FeaturePoint(int _x, int _y, string _imageKey)
        {
            x = _x;
            y = _y;
            imageId = _imageKey;
        }

        internal double[] GetFeatures()
        {
            List<double> features = new List<double>();

            PixelColor pixel = ImageHelper.Get(imageId, x, y);

            // RGB [0-2]
            if (RGBEnable)
            {
                features.Add(pixel.r);
                features.Add(pixel.g);
                features.Add(pixel.b);
            }

            // HSL [3-5]
            if (HSLEnable)
            {
                features.Add(pixel.H);
                features.Add(pixel.S);
                features.Add(pixel.L);
            }

            //XYZ [6-8]
            if (XYZEnable)
            {
                features.Add(pixel.X);
                features.Add(pixel.Y);
                features.Add(pixel.Z);
            }

            //GLCM values [9-24]
            if (GLCMEnable)
            {
                if (GLCMResults == null)
                {
                    GLCMResults = new double[24];
                    ALL_GLCM(7, new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { 1, 1 }, new int[] { -1, 1 }, ref GLCMResults);
                }

                features.AddRange(GLCMResults);
            }

            if (HOGEnable)
            {
                if (HOGResults == null)
                {
                    HOGResults = HOG(7);
                }

                features.AddRange(HOGResults);
            }

            if (LBPEnable)
            {
                if (LBPResults == null)
                {
                    LBPResults = ALL_LBP();
                }

                features.AddRange(LBPResults);
            }

            if (HeightEnable || NormalEnable || SlopeEnable)
            {
                features.AddRange(HeightValue());
            }

            return features.ToArray();
        }

        private void ALL_GLCM(int half_size, int[] disp0, int[] disp1, int[] disp2, int[] disp3, ref double[] ret)
        {
            int MATRIX_SIZE = 8;
            double[,] matrix0 = new double[MATRIX_SIZE, MATRIX_SIZE];
            double[,] matrix1 = new double[MATRIX_SIZE, MATRIX_SIZE];
            double[,] matrix2 = new double[MATRIX_SIZE, MATRIX_SIZE];
            double[,] matrix3 = new double[MATRIX_SIZE, MATRIX_SIZE];

            double total0 = 0;
            double total1 = 0;
            double total2 = 0;
            double total3 = 0;

            ImageTile it = ImageHelper.imageTiles[imageId];

            for (int px = x - half_size; px < x + half_size; px++)
            {
                if (px < 0 && it.tileX == 0) continue;
                if (px > 255 && it.tileX > it.grid.width) continue;

                for (int py = y - half_size; py < y + half_size; py++)
                {
                    if (py < 0 && it.tileY == 0) continue;
                    if (py > 255 && it.tileY > it.grid.height) continue;

                    int i = (int)Math.Round(Luminance(ImageHelper.Get(imageId, px, py)) * (MATRIX_SIZE - 1));

                    int px2 = px + disp0[0];
                    int py2 = py + disp0[1];
                    int j = (int)Math.Round(Luminance(ImageHelper.Get(imageId, px2, py2)) * (MATRIX_SIZE - 1));
                    matrix0[i, j]++;
                    total0++;

                    px2 = px + disp1[0];
                    py2 = py + disp1[1];

                    j = (int)Math.Round(Luminance(ImageHelper.Get(imageId, px2, py2)) * (MATRIX_SIZE - 1));
                    matrix1[i, j]++;
                    total1++;

                    px2 = px + disp2[0];
                    py2 = py + disp2[1];
                    j = (int)Math.Round(Luminance(ImageHelper.Get(imageId, px2, py2)) * (MATRIX_SIZE - 1));
                    matrix2[i, j]++;
                    total2++;

                    px2 = px + disp3[0];
                    py2 = py + disp3[1];
                    j = (int)Math.Round(Luminance(ImageHelper.Get(imageId, px2, py2)) * (MATRIX_SIZE - 1));
                    matrix3[i, j]++;
                    total3++;
                }
            }

            double mu0x = 0;
            double sig0x = 0;
            double mu1x = 0;
            double sig1x = 0;
            double mu2x = 0;
            double sig2x = 0;
            double mu3x = 0;
            double sig3x = 0;


            double mu0y = 0;
            double sig0y = 0;
            double mu1y = 0;
            double sig1y = 0;
            double mu2y = 0;
            double sig2y = 0;
            double mu3y = 0;
            double sig3y = 0;

            for (int i = 0; i < MATRIX_SIZE; i++)
            {
                for (int j = 0; j < MATRIX_SIZE; j++)
                {
                    if (total0 != 0) matrix0[i, j] /= total0;
                    if (total1 != 0) matrix1[i, j] /= total1;
                    if (total2 != 0) matrix2[i, j] /= total2;
                    if (total3 != 0) matrix3[i, j] /= total3;

                    mu0x += (i + 1) * matrix0[i, j];
                    mu1x += (i + 1) * matrix1[i, j];
                    mu2x += (i + 1) * matrix2[i, j];
                    mu3x += (i + 1) * matrix3[i, j];

                    mu0y += (j + 1) * matrix0[i, j];
                    mu1y += (j + 1) * matrix1[i, j];
                    mu2y += (j + 1) * matrix2[i, j];
                    mu3y += (j + 1) * matrix3[i, j];
                }
            }

            for (int i = 0; i < MATRIX_SIZE; i++)
            {
                for (int j = 0; j < MATRIX_SIZE; j++)
                {
                    sig0x += (i + 1 - mu0x) * (i + 1 - mu0x) * matrix0[i, j];
                    sig1x += (i + 1 - mu1x) * (i + 1 - mu1x) * matrix1[i, j];
                    sig2x += (i + 1 - mu2x) * (i + 1 - mu2x) * matrix2[i, j];
                    sig3x += (i + 1 - mu3x) * (i + 1 - mu3x) * matrix3[i, j];

                    sig0y += (j + 1 - mu0y) * (j + 1 - mu0y) * matrix0[i, j];
                    sig1y += (j + 1 - mu1y) * (j + 1 - mu1y) * matrix1[i, j];
                    sig2y += (j + 1 - mu2y) * (j + 1 - mu2y) * matrix2[i, j];
                    sig3y += (j + 1 - mu3y) * (j + 1 - mu3y) * matrix3[i, j];
                }
            }

            double sig0 = Math.Sqrt(sig0x * sig0y);
            double sig1 = Math.Sqrt(sig1x * sig1y);
            double sig2 = Math.Sqrt(sig2x * sig2y);
            double sig3 = Math.Sqrt(sig3x * sig3y);


            for (int i = 0; i < MATRIX_SIZE; i++)
            {
                for (int j = 0; j < MATRIX_SIZE; j++)
                {
                    double ij2 = Math.Pow(i - j, 2);
                    double inv_abs = 1.0 / (1 + (i - j) * (i - j));

                    ret[0] += ij2 * matrix0[i, j]; //Contrast
                    ret[1] -= matrix0[i, j] <= 0 ? 0 : matrix0[i, j] * Math.Log(matrix0[i, j]); //Entropy
                    ret[2] += Math.Pow(matrix0[i, j], 2); //Uniformity;
                    ret[3] += matrix0[i, j] * inv_abs; // Homogeneity
                    ret[4] += sig0 == 0 ? 0 : matrix0[i, j] * (i + 1 - mu0x) * (j + 1 - mu0y) / sig0; //Correlation


                    ret[6] += ij2 * matrix1[i, j]; //Contrast
                    ret[7] -= matrix1[i, j] <= 0 ? 0 : matrix1[i, j] * Math.Log(matrix1[i, j]); //Entropy
                    ret[8] += Math.Pow(matrix1[i, j], 2); //Uniformity;
                    ret[9] += matrix1[i, j] * inv_abs; // Homogeneity
                    ret[10] += sig1 == 0 ? 0 : matrix1[i, j] * (i + 1 - mu1x) * (j + 1 - mu1y) / sig1; //Correlation


                    ret[12] += ij2 * matrix2[i, j]; //Contrast
                    ret[13] -= matrix2[i, j] <= 0 ? 0 : matrix2[i, j] * Math.Log(matrix2[i, j]); //Entropy
                    ret[14] += Math.Pow(matrix2[i, j], 2); //Uniformity;
                    ret[15] += matrix2[i, j] * inv_abs; // Homogeneity
                    ret[16] += sig2 == 0 ? 0 : matrix2[i, j] * (i + 1 - mu2x) * (j + 1 - mu2y) / sig2; //Correlation


                    ret[18] += ij2 * matrix3[i, j]; //Contrast
                    ret[19] -= matrix3[i, j] <= 0 ? 0 : matrix3[i, j] * Math.Log(matrix3[i, j]); //Entropy
                    ret[20] += Math.Pow(matrix3[i, j], 2); //Uniformity;
                    ret[21] += matrix3[i, j] * inv_abs; // Homogeneity
                    ret[22] += sig3 == 0 ? 0 : matrix3[i, j] * (i + 1 - mu3x) * (j + 1 - mu3y) / sig3; //Correlation
                }
            }

            for (int i = 0; i < MATRIX_SIZE; i++)
            {
                for (int j = 0; j < MATRIX_SIZE; j++)
                {
                    ret[5] += sig0 == 0 ? 0 : Math.Pow(i + 1 + j + 1 - 2 * mu0x, 3) * matrix0[i, j] / (Math.Pow(sig0 * 2 * (1 + ret[4]), 1.5));
                    ret[11] += sig1 == 0 ? 0 : Math.Pow(i + 1 + j + 1 - 2 * mu1x, 3) * matrix0[i, j] / (Math.Pow(sig1 * 2 * (1 + ret[10]), 1.5));
                    ret[17] += sig2 == 0 ? 0 : Math.Pow(i + 1 + j + 1 - 2 * mu2x, 3) * matrix0[i, j] / (Math.Pow(sig2 * 2 * (1 + ret[16]), 1.5));
                    ret[23] += sig3 == 0 ? 0 : Math.Pow(i + 1 + j + 1 - 2 * mu3x, 3) * matrix0[i, j] / (Math.Pow(sig3 * 2 * (1 + ret[22]), 1.5));
                }
            }

            try
            {
                ret[5] = Math.Sign(ret[5]) * Math.Pow(Math.Abs(ret[5]), 1.0 / 3.0);
                ret[11] = Math.Sign(ret[11]) * Math.Pow(Math.Abs(ret[11]), 1.0 / 3.0);
                ret[17] = Math.Sign(ret[17]) * Math.Pow(Math.Abs(ret[17]), 1.0 / 3.0);
                ret[23] = Math.Sign(ret[23]) * Math.Pow(Math.Abs(ret[23]), 1.0 / 3.0);
            }
            catch
            {
                ret[5] = 0;
                ret[11] = 0;
                ret[17] = 0;
                ret[23] = 0;
            }
        }

        internal static void Remove(string filename)
        {
            AllFeaturesPoint.TryRemove(filename, out FeaturePoint[,] t);
            if (t != null) t = null;
        }

        private double[] HOG(int half_size)
        {
            int MATRIX_SIZE = 9;
            double[] ret = new double[MATRIX_SIZE];
            int[] imgSize = ImageHelper.imagesSize[ImageHelper.imageTiles[imageId].baseFileName];

            for (int px = x - half_size; px < x + half_size; px++)
            {
                int px02 = px - 1;
                int px22 = px + 1;

                for (int py = y - half_size; py < y + half_size; py++)
                {
                    int py02 = py - 1;
                    int py22 = py + 1;

                    double gx = -Luminance(ImageHelper.Get(imageId, px02, py)) + Luminance(ImageHelper.Get(imageId, px22, py));
                    double gy = -Luminance(ImageHelper.Get(imageId, px, py02)) + Luminance(ImageHelper.Get(imageId, px, py22));

                    double g = Math.Sqrt(gx * gx + gy * gy);
                    double theta = Math.Atan2(gy, gx);
                    theta = theta < 0 ? Math.PI + theta : theta;
                    int index = (int)Math.Floor(theta * (MATRIX_SIZE - 1) / Math.PI);

                    ret[index] += g;
                }
            }
            
            double l = 0;

            for (int i = 0; i < ret.Length; i++)
            {
                l += ret[i]*ret[i];
            }

            l = Math.Sqrt(l);

            for (int i=0; i<ret.Length && l > 0; i++)
            {
                ret[i] /= l;
            }

            return ret;
        }

        private double[] ALL_LBP()
        {
            List<double> ret = new List<double>();

            ret.AddRange(LBP(8, 1));
            ret.AddRange(LBP(8, 2));
            ret.AddRange(LBP(8, 4));
            ret.AddRange(LBP(8, 8));

            /*
            ret.AddRange(LBP(16, 16));
            ret.AddRange(LBP(16, 32));
            ret.AddRange(LBP(16, 64));
            */
            return ret.ToArray();
        }

        private double[] LBP(int nPoints, int radius)
        {
            double[] ret = new double[6];

            int[] imgSize = ImageHelper.imagesSize[ImageHelper.imageTiles[imageId].baseFileName];

            PixelColor color = ImageHelper.Get(imageId, x, y);
            int counter = 1;
            for (double theta = 0; theta < 2 * Math.PI; theta += 2 * Math.PI / nPoints)
            {
                double cos = Math.Cos(theta);
                double sin = Math.Sin(theta);

                int dx = Math.Abs(cos) < 0.01 ? 0 : cos > 0 ? (int)Math.Ceiling(cos * radius) : (int)Math.Floor(cos * radius);
                int dy = Math.Abs(sin) < 0.01 ? 0 : sin > 0 ? (int)Math.Ceiling(sin * radius) : (int)Math.Floor(sin * radius);

                int px = dx + x;
                int py = dy + y;

                PixelColor color2 = ImageHelper.Get(imageId, px, py);

                if (color.R > color2.R) ret[0] += counter;
                if (color.G > color2.G) ret[1] += counter;
                if (color.B > color2.B) ret[2] += counter;

                if (color.H > color2.H) ret[3] += counter;
                if (color.S > color2.S) ret[4] += counter;
                if (color.L > color2.L) ret[5] += counter;

                counter*=2;
            }

            return ret;
        }

        public double Luminance(PixelColor color)
        {
            return 0.2126 * color.R / 255.0 + 0.7152 * color.G / 255.0 + 0.0722 * color.B / 255.0;
        }

        public override string ToString()
        {
            return imageId + SaveLoad.Space2 + x + SaveLoad.Space2 + y;
        }

        private double[] HeightValue()
        {
            int[] size = imagesSize[imageTiles[imageId].baseFileName];

            int dx = x == 0 || x == size[0] - 1 ? 1 : 2;
            int dy = y == 0 || y == size[1] - 1 ? 1 : 2;

            int s = 0;

            if (HeightEnable) s += 1;
            if (NormalEnable) s += 3;
            if (SlopeEnable) s += 1;

            double[] values = new double[s];

            double z11 = ImageHelper.imageTiles[imageId].GetHeightValue(x, y);
            int index = 0;

            if (HeightEnable)
            {
                index++;
                values[0] = z11;
            }
            if (NormalEnable || SlopeEnable)
            {
                double z01 = x == 0 ? z11 : ImageHelper.imageTiles[imageId].GetHeightValue(x - 1, y);
                double z21 = x == size[0] - 1 ? z11 : ImageHelper.imageTiles[imageId].GetHeightValue(x + 1, y);
                double z10 = y == 0 ? z11 : ImageHelper.imageTiles[imageId].GetHeightValue(x, y - 1);
                double z12 = y == size[1] - 1 ? z11 : ImageHelper.imageTiles[imageId].GetHeightValue(x, y + 1);

                double v0x = dx * imageTiles[imageId].Precision;
                double v0z = z21 - z01;
                double l0 = Math.Sqrt(v0x * v0x + v0z*v0z);
                v0x /= l0;
                v0z /= l0;

                double v1y = dy * imageTiles[imageId].Precision;
                double v1z = z12 - z10;
                double l1 = Math.Sqrt(v1y * v1y + v1z * v1z);
                v1y /= l1;
                v1z /= l1;

                double nx = -v0z * v1y;
                double ny = -v0x * v1z;
                double nz = v0x * v1y;

                double l = Math.Sqrt(nx * nx + ny * ny + nz * nz);

                nx /= l;
                ny /= l;
                nz /= l;

                if (NormalEnable)
                {
                    values[index] = nx;
                    index++;
                    values[index] = ny;
                    index++;
                    values[index] = nz;
                    index++;
                }

                if (SlopeEnable)
                {
                    values[index] = Math.Acos(nz);
                }
            }
            return values;
        }
    }
}
