using Lerc2017;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ClassificationTool
{
    public class ImageHelper
    {
        public class HeightData
        {
            public double[] data;
            public int width;
            public int height;
            byte[] validBytes;
            private readonly double max;
            private readonly double min;
            public string filename;
            private readonly bool valid;

            public HeightData(string _filename, double[] _data, int _width, int _height, byte[] _validBytes, double _min, double _max)
            {
                data = _data;
                width = _width;
                height = _height;
                validBytes = _validBytes;
                min = _min;
                max = _max;
                filename = _filename;
                valid = validBytes.Length > 0;
            }

            public double GetValue(double x, double y)
            {
                if (!valid) return 0;

                int x0 = (int)Math.Floor(x);
                int x1 = x0 + 1;
                int y0 = (int)Math.Floor(y);
                int y1 = y0 + 1;

                double v00 = validBytes[x0 + y0 * width] == 1 ? data[x0 + y0 * width] : (min + max) / 2;
                double v10 = validBytes[x1 + y0 * width] == 1 ? data[x1 + y0 * width] : (min + max) / 2;

                double v01 = validBytes[x0 + y1 * width] == 1 ? data[x0 + y1 * width] : (min + max) / 2;
                double v11 = validBytes[x1 + y1 * width] == 1 ? data[x1 + y1 * width] : (min + max) / 2;

                double v0 = v00 + (x - x0) * (v10 - v00);
                double v1 = v01 + (x - x0) * (v11 - v01);

                return v0 + (y - y0) * (v0 - v1);
            }

            public double GetValue(int level, long tileX, long tileY, double x, double y)
            {
                if (level <= 14)
                {
                    return GetValue(x, y);
                }
                else
                {
                    for (int i = level; i > 14; i--)
                    {
                        x /= 2;
                        y /= 2;

                        if (tileX % 2 != 0) x += 128;
                        if (tileY % 2 != 0) y += 128;

                        tileX = (int)Math.Floor(tileX / 2.0);
                        tileY = (int)Math.Floor(tileY / 2.0);
                    }

                    return GetValue(x, y);
                }
            }

            internal WriteableBitmap GetHeightmap(int level, long x, long y)
            {
                string h_id = "" + level + "_" + x + "_" + y;

                if (heightmaps.ContainsKey(h_id)) return heightmaps[h_id];
                else
                {
                    WriteableBitmap bitmap = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Gray8, null);

                    for (int i = 0; i < height - 1; i++)
                    {
                        for (int j = 0; j < width - 1; j++)
                        {
                            byte[] Pixel = new byte[1];

                            Pixel[0] = (byte)(255 * (GetValue(level, x, y, j, i)) / 8000.0);
                            bitmap.WritePixels(new Int32Rect(j, i, 1, 1), Pixel, 4, 0);
                        }
                    }

                    heightmaps.Add(h_id, bitmap);

                    return bitmap;
                }
            }

            internal static HeightData GetOrLoad(int level, long x, long y)
            {
                if (level > 14)
                {
                    for (int i = level; i > 14; i--)
                    {
                        x = (int)Math.Floor(x / 2.0);
                        y = (int)Math.Floor(y / 2.0);
                    }

                    level = 14;
                }

                if (heightDataList.ContainsKey("hd_" + level + "_" + x + "_" + y)) return heightDataList["hd_" + level + "_" + x + "_" + y];

                else
                {
                    return SaveLoad.LoadHeightMap(level, x, y);
                }
            }
        }

        public struct PixelColor
        {
            public byte B;
            public byte G;
            public byte R;
            public byte A;

            public double r;
            public double g;
            public double b;

            public double H;
            public double S;
            public double L;

            public double X;
            public double Y;
            public double Z;

            public PixelColor(byte color)
            {
                B = G = R = A = color;
                r = g = b = H = S = L = X = Y = Z = 0;
                UpdateComponents();
            }

            public PixelColor(byte _B, byte _G, byte _R, byte _A)
            {
                A = _A;
                R = _R;
                G = _G;
                B = _B;

                r = g = b = H = S = L = X = Y = Z = 0;

                UpdateComponents();
            }

            public PixelColor(string s)
            {
                string[] c = s.Split(SaveLoad.Space1);

                A = byte.Parse(c[0]);
                R = byte.Parse(c[1]);
                G = byte.Parse(c[2]);
                B = byte.Parse(c[3]);

                r = g = b = H = S = L = X = Y = Z = 0;

                UpdateComponents();
            }

            private void UpdateComponents()
            {
                r = 1.0 * R / 255.0;
                g = 1.0 * G / 255.0;
                b = 1.0 * B / 255.0;

                double max = Math.Max(r, Math.Max(g, b));
                double min = Math.Min(r, Math.Min(g, b));
                L = (max + min) / 2;
                S = 0;
                H = 0;

                if (max != min) // not achromatic                
                {
                    double d = max - min;
                    S = L > 0.5 ? d / (2 - max - min) : d / (max + min);

                    if (max == r) H = (g - b) / d + (g < b ? 6 : 0);
                    else if (max == g) H = (b - r) / d + 2;
                    else if (max == b) H = (r - g) / d + 4;

                    H /= 6.0;
                }

                double tmpR = r, tmpG = g, tmpB = b;

                if (tmpR > 0.04045) tmpR = Math.Pow((tmpR + 0.055) / 1.055, 2.4);
                else tmpR = tmpR / 12.92;
                if (tmpG > 0.04045) tmpG = Math.Pow((tmpG + 0.055) / 1.055, 2.4);
                else tmpG = tmpG / 12.92;
                if (tmpB > 0.04045) tmpB = Math.Pow((tmpB + 0.055) / 1.055, 2.4);
                else tmpB = tmpB / 12.92;

                tmpR *= 100;
                tmpG *= 100;
                tmpB *= 100;

                X = (tmpR * 0.4124 + tmpG * 0.3576 + tmpB * 0.1805); //X
                Y = (tmpR * 0.2126 + tmpG * 0.7152 + tmpB * 0.0722); //Y
                Z = (tmpR * 0.0193 + tmpG * 0.1192 + tmpB * 0.9505); //Z
            }

            internal void Set(Color color)
            {
                B = color.B;
                G = color.G;
                R = color.R;
                A = color.A;

                UpdateComponents();
            }

            public override string ToString()
            {
                return "" + A + SaveLoad.Space1 + R + SaveLoad.Space1 + G + SaveLoad.Space1 + B;
            }
        }

        public class ImageGrid
        {

            public string id;
            public string name;
            public ImageTile[,] tiles;
            public Grid grid;
            public int width, height;

            private readonly Label[] lx;
            private readonly Label[] ly;

            public ImageGrid(MainWindow window, string _id, string _name, int _width, int _height)
            {
                id = _id;
                name = _name;

                width = _width;
                height = _height;

                tiles = new ImageTile[width, height];

                ScaleTransform sf = new ScaleTransform
                {
                    ScaleX = 1,
                    ScaleY = 1,
                };

                TranslateTransform tf = new TranslateTransform
                {
                    X = 0,
                    Y = 0
                };

                TransformGroup transform = new TransformGroup();
                transform.Children.Add(sf);
                transform.Children.Add(tf);
                grid = new Grid
                {
                    Name = _id,
                    Background = window.DarkBackground,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    RenderTransform = transform,
                };
                grid.MouseWheel += window.Grid_MouseWheel;

                int scale = Math.Max(width, height);

                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(23),
                });

                for (int x = 0; x < width; x++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(950 / scale),
                    });
                }

                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(23),
                });

                for (int y = 0; y < height; y++)
                {
                    grid.RowDefinitions.Add(new RowDefinition
                    {
                        Height = new GridLength(950 / scale),
                    });
                }

                lx = new Label[width];
                ly = new Label[height];

                for (int x = 0; x < width; x++)
                {
                    lx[x] = new Label()
                    {
                        FontSize = Math.Min(12, 130 / scale),
                        Foreground = window.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                    };

                    grid.Children.Add(lx[x]);
                    Grid.SetColumn(lx[x], x + 1);
                    Grid.SetRow(lx[x], 0);
                }

                RotateTransform rotate = new RotateTransform(-90);

                for (int y = 0; y < height; y++)
                {
                    ly[y] = new Label()
                    {
                        FontSize = Math.Min(12, 130 / scale),
                        Foreground = window.White,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        LayoutTransform = rotate
                    };
                    grid.Children.Add(ly[y]);
                    Grid.SetColumn(ly[y], 0);
                    Grid.SetRow(ly[y], y + 1);
                }

                imageGrids.Add(id, this);
            }

            internal void AddTile(ImageTile imageTile)
            {
                MainWindow.dispatcher.Invoke(() =>
                {
                    if (!grid.Children.Contains(imageTile.baseImage))
                    {
                        grid.Children.Add(imageTile.baseImage);
                        Grid.SetColumn(imageTile.baseImage, imageTile.tileX + 1);
                        Grid.SetRow(imageTile.baseImage, imageTile.tileY + 1);

                        if (imageTile.heightmap != null)
                        {
                            grid.Children.Add(imageTile.heightmap);
                            Grid.SetColumn(imageTile.heightmap, imageTile.tileX + 1);
                            Grid.SetRow(imageTile.heightmap, imageTile.tileY + 1);
                        }

                        grid.Children.Add(imageTile.statusImage);
                        Grid.SetColumn(imageTile.statusImage, imageTile.tileX + 1);
                        Grid.SetRow(imageTile.statusImage, imageTile.tileY + 1);

                        grid.Children.Add(imageTile.trainImage);
                        Grid.SetColumn(imageTile.trainImage, imageTile.tileX + 1);
                        Grid.SetRow(imageTile.trainImage, imageTile.tileY + 1);

                        lx[imageTile.tileX].Content = imageTile.worldX;
                        ly[imageTile.tileY].Content = imageTile.worldY;


                        tiles[imageTile.tileX, imageTile.tileY] = imageTile;
                    }
                });
            }
            internal void AddOverlayClass(ImageTile it, Classes c, MainWindow window)
            {
                string classId = c.classId;

                if (it.overlaysImages.ContainsKey(classId))
                {
                    it.overlaysImages.TryRemove(classId, out Image r);
                    grid.Children.Remove(r);

                    DrawOverlayClass(it.id, 0, 0, 256, 256, c, new PixelColor(0));
                }
                {
                    WriteableBitmap Source = it.overlaysClasses.GetOrAdd(classId, new WriteableBitmap(256, 256, 96, 96, PixelFormats.Bgra32, null));

                    Image overlayClass = new Image
                    {
                        Stretch = Stretch.Uniform,
                        Opacity = 0.7,
                        Name = "ove_" + id + "_" + classId,
                        Source = Source,
                        IsHitTestVisible = false,
                    };
                    RenderOptions.SetBitmapScalingMode(overlayClass, BitmapScalingMode.NearestNeighbor);

                    it.overlaysImages.AddOrUpdate(classId, overlayClass, (key, oldValue) => overlayClass);
                    it.classified = false;
                    grid.Children.Add(it.overlaysImages[classId]);
                    Grid.SetColumn(it.overlaysImages[classId], it.tileX + 1);
                    Grid.SetRow(it.overlaysImages[classId], it.tileY + 1);
                }
            }

            internal void AddOverlayClass(Classes c, MainWindow window)
            {
                foreach (ImageTile it in tiles)
                {
                    AddOverlayClass(it, c, window);
                }
            }

            internal void RemoveTile(int x, int y)
            {
                ImageTile it = tiles[x, y];
                if (it != null)
                {
                    string filename = it.baseFileName;
                    imageTiles.TryRemove(it.id, out ImageTile value);
                    if (!imageTiles.Values.Any((e) => e.baseFileName == filename))
                    {
                        images.TryRemove(filename, out PixelColor[,] r);
                        r = null;
                        imagesSize.Remove(filename);
                        FeaturePoint.Remove(filename);
                        bitmapImages.Remove(id);
                        if (heightmaps.ContainsKey(id)) heightmaps.Remove(id);
                        if (heightDataList.ContainsKey(id)) heightDataList.Remove(id);
                    }

                    if (it.baseImage != null) grid.Children.Remove(it.baseImage);
                    if (it.trainImage != null) grid.Children.Remove(it.trainImage);
                    if (it.heightmap != null) grid.Children.Remove(it.heightmap);
                    if (it.statusImage != null) grid.Children.Remove(it.statusImage);


                    tiles[x, y] = null;

                    foreach (Classes c in MainWindow.classesList)
                    {
                        if (it.overlaysImages.ContainsKey(c.classId))
                        {
                            it.overlaysImages.TryRemove(c.classId, out Image overlay);
                            if (overlay != null) grid.Children.Remove(overlay);
                        }
                        c.RemoveImageFeature(it.id);
                    }

                    GC.Collect();
                }
            }

            public override string ToString()
            {
                string s = "";
                s += id + SaveLoad.Space1;
                s += name + SaveLoad.Space1;
                s += width;
                s += SaveLoad.Space1;
                s += height;
                s += SaveLoad.Space1;

                foreach (ImageTile it in tiles)
                {
                    s += it.ToString() + SaveLoad.Space2;
                }
                return s;
            }

            static public ImageGrid FromString(string s, MainWindow window)
            {
                string[] splitted = s.Split(SaveLoad.Space1);
                ImageGrid grid = new ImageGrid(window, splitted[0], splitted[1], int.Parse(splitted[2]), int.Parse(splitted[3]));

                string[] splitted2 = splitted[4].Split(SaveLoad.Space2);

                foreach (string e in splitted2)
                {
                    if (e != "") grid.AddTile(ImageTile.FromString(e, window, grid));
                }

                return grid;
            }
        }

        public class ImageTile
        {
            public string id;
            public string baseFileName;
            public int level;

            public ImageGrid grid;

            public Image baseImage;
            public Image trainImage;
            public Image heightmap;
            public Image statusImage;


            public HeightData heightdata;

            public List<string> layersFileNames;
            public WriteableBitmap trainingOverlay;
            public WriteableBitmap status;

            public ConcurrentDictionary<string, WriteableBitmap> overlaysClasses; // class name => overlay
            public ConcurrentDictionary<string, Image> overlaysImages; // class name => overlay

            public bool classified = false;

            private bool locked = false;

            public int tileX, tileY;
            public long worldX, worldY;

            public double Precision => 156543.03392800014 / Math.Pow(2, level);

            private static SpinLock spinLock = new SpinLock();

            public override string ToString()
            {
                string s = "";
                s += id + SaveLoad.Space3;
                s += baseFileName + SaveLoad.Space3;
                s += level.ToString() + SaveLoad.Space3;
                s += tileX.ToString() + SaveLoad.Space3;
                s += tileY.ToString() + SaveLoad.Space3;
                return s;
            }

            static public ImageTile FromString(string s, MainWindow window, ImageGrid grid)
            {
                string[] splitted = s.Split(SaveLoad.Space3);

                return new ImageTile(window, grid, splitted[1], splitted[0], int.Parse(splitted[2]), int.Parse(splitted[3]), int.Parse(splitted[4]));
            }

            public ImageTile(MainWindow window, ImageGrid _grid, string filename, string _id, int _level, int _x, int _y, bool hm = true)
            {
                id = _id;
                if (filename[0] != 't') // Check if relative or absolute path
                {
                    baseFileName = (new Uri(window.MAIN_DIRECTORY).MakeRelativeUri(new Uri(filename))).ToString();
                }
                else
                {
                    baseFileName = filename;
                }

                layersFileNames = new List<string>();
                level = _level;
                tileX = _x;
                tileY = _y;
                grid = _grid;

                string[] tmp = baseFileName.Split(new char[] { '_', '.' });

                worldX = long.Parse(tmp[2]);
                worldY = long.Parse(tmp[1]);

                MainWindow.dispatcher.Invoke(() =>
                {
                    BitmapImage bitmapImage;

                    if (bitmapImages.ContainsKey(baseFileName)) bitmapImage = bitmapImages[baseFileName];
                    else
                    {
                        string uri = "http://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/tile/" + (level) + "/" + (worldY) + "/" + (worldX);

                        if (!File.Exists(filename)) SaveLoad.DownloadRemoteFile(uri, baseFileName);

                        bitmapImage = new BitmapImage((new Uri(System.IO.Path.GetDirectoryName(window.MAIN_DIRECTORY) + "\\" + baseFileName)));
                        bitmapImages.Add(baseFileName, bitmapImage);
                    }


                    baseImage = new Image
                    {
                        Name = id,
                        Stretch = Stretch.Uniform,
                        Source = bitmapImage,
                    };
                    RenderOptions.SetBitmapScalingMode(baseImage, BitmapScalingMode.NearestNeighbor);

                    int[] size = new int[] { (int)baseImage.Source.Width, (int)baseImage.Source.Height };

                    status = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);

                    statusImage = new Image
                    {
                        Name = id,
                        Stretch = Stretch.Uniform,
                        Source = status,
                    };
                    RenderOptions.SetBitmapScalingMode(status, BitmapScalingMode.NearestNeighbor);

                    trainingOverlay = new WriteableBitmap(size[0], size[1], 96, 96, PixelFormats.Bgra32, null);

                    trainImage = new Image
                    {
                        Name = id,
                        Stretch = Stretch.Uniform,
                        Source = trainingOverlay,
                    };
                    RenderOptions.SetBitmapScalingMode(trainImage, BitmapScalingMode.NearestNeighbor);

                    trainImage.MouseLeftButtonDown += window.Img_MouseLeftButtonDown;
                    trainImage.MouseLeftButtonUp += window.Img_MouseLeftButtonUp;

                    trainImage.MouseRightButtonDown += window.Img_MouseRightButtonDown;
                    trainImage.MouseRightButtonUp += window.Img_MouseRightButtonUp;

                    overlaysClasses = new ConcurrentDictionary<string, WriteableBitmap>();
                    overlaysImages = new ConcurrentDictionary<string, Image>();
                });

                heightdata = HeightData.GetOrLoad(level, worldX, worldY);
                heightDataList.Add(id, heightdata);
                MainWindow.dispatcher.Invoke(() =>
                {
                    if (hm)
                    {
                        heightmap = new Image
                        {
                            Name = id,
                            Stretch = Stretch.Uniform,
                            Source = heightdata.GetHeightmap(level, worldX, worldY),
                        };
                        RenderOptions.SetBitmapScalingMode(heightmap, BitmapScalingMode.NearestNeighbor);
                    }
                });

                classified = false;

                imageTiles.TryAdd(id, this);
            }

            internal bool IsLocked()
            {
                return locked;
            }

            internal void Lock()
            {
                locked = true;
            }

            internal void Unlock()
            {
                locked = false;
            }

            internal void Status(PixelColor color)
            {
                MainWindow.dispatcher.Invoke(() =>
                {
                    byte[] ColorData = new byte[4];
                    ColorData[0] = color.B;
                    ColorData[1] = color.G;
                    ColorData[2] = color.R;
                    ColorData[3] = color.A;

                    status.WritePixels(new System.Windows.Int32Rect(0, 0, 1, 1), ColorData, 4, 0);
                });
            }

            internal void Add()
            {
                bool lockAcquired = false;
                DateTime time = DateTime.Now;

                //Console.WriteLine(" ********** " + id + " trying to acquire lock");

                spinLock.Enter(ref lockAcquired);

                //Console.WriteLine(" ++++++++++ " + id + " lock acquired after " + MainWindow.TimeToString(DateTime.Now - time));
                time = DateTime.Now;

                try
                {
                    if (!images.ContainsKey(baseFileName))
                    {
                        MainWindow.dispatcher.Invoke(new Action(() =>
                        {
                            int width = bitmapImages[baseFileName].PixelWidth;
                            int height = bitmapImages[baseFileName].PixelHeight;

                            PixelColor[,] result = new PixelColor[width, height];

                            CopyPixels(bitmapImages[baseFileName], result, width * 4, 0);

                            if (imagesQueue.Count > 32)
                            {
                                for (int i = 0; i < 32; i++)
                                {
                                    string it_id = imagesQueue.Dequeue();

                                    if (imageTiles.ContainsKey(it_id))
                                    {
                                        if (imageTiles[it_id].IsLocked())
                                        {
                                            imagesQueue.Enqueue(it_id);
                                        }
                                        else
                                        {
                                            images.TryRemove(imageTiles[it_id].baseFileName, out PixelColor[,] test);
                                        }
                                    }
                                }
                            }

                            imagesQueue.Enqueue(id);
                            images.TryAdd(baseFileName, result);

                            if (!imagesSize.ContainsKey(baseFileName)) imagesSize.Add(baseFileName, new int[] { width, height });
                        }));
                    }

                    //Console.WriteLine(" ---------- " + id + " lock released after " + MainWindow.TimeToString(DateTime.Now - time));

                    spinLock.Exit();
                }
                catch (Exception e)
                {
                    //Console.WriteLine(" ---------- " + id + " lock released after " + MainWindow.TimeToString(DateTime.Now - time) + " --- ");
                    spinLock.Exit();
                    throw (e);
                }
            }


            internal void RemoveClass(string classId)
            {
                overlaysClasses.TryRemove(classId, out WriteableBitmap ove);

                overlaysImages.TryRemove(classId, out Image img);

                (img.Parent as Grid).Children.Remove(img);
            }

            internal double GetHeightValue(int x, int y)
            {
                if (heightDataList.ContainsKey(id))
                    return heightDataList[id].GetValue(level, worldX, worldY, x, y);
                else return -1e30;
            }

            internal string LayerString()
            {
                string s = "";

                foreach (string l in layersFileNames)
                {
                    s += l + SaveLoad.Space1;
                }

                return s;
            }
        }

        public volatile static ConcurrentDictionary<string, PixelColor[,]> images = new ConcurrentDictionary<string, PixelColor[,]>(); // filename to color
        public static Dictionary<string, int[]> imagesSize = new Dictionary<string, int[]>(); // filename to size

        public static ConcurrentDictionary<string, ImageTile> imageTiles = new ConcurrentDictionary<string, ImageTile>();  //imageId to ImageTile;
        public static Dictionary<string, ImageGrid> imageGrids = new Dictionary<string, ImageGrid>();
        public volatile static Dictionary<string, BitmapImage> bitmapImages = new Dictionary<string, BitmapImage>();
        public volatile static Queue<string> imagesQueue = new Queue<string>();

        public static Dictionary<string, HeightData> heightDataList = new Dictionary<string, HeightData>();

        public static Dictionary<string, WriteableBitmap> heightmaps = new Dictionary<string, WriteableBitmap>();

        private static SpinLock spinLock = new SpinLock();



        public static WriteableBitmap DrawOverlayTrain(string id, int x, int y, PixelColor color)
        {
            return DrawOverlayTrain(id, x, y, 1, 1, color);
        }

        public static WriteableBitmap DrawOverlayClass(string id, int x, int y, Classes c, PixelColor color)
        {
            return DrawOverlayClass(id, x, y, 1, 1, c, color);
        }

        public static WriteableBitmap DrawOverlayTrain(string id, int x, int y, int width, int height, PixelColor color)
        {
            WriteableBitmap img = imageTiles[id].trainingOverlay;

            byte[] ColorData = new byte[height * width * 4];

            for (int i = 0; i < ColorData.Length; i += 4)
            {
                ColorData[i + 0] = color.B;
                ColorData[i + 1] = color.G;
                ColorData[i + 2] = color.R;
                ColorData[i + 3] = color.A;
            }

            img.WritePixels(new System.Windows.Int32Rect(x, y, width, height), ColorData, width * 4, 0);

            return img;
        }

        public static WriteableBitmap DrawOverlayClass(string id, int x, int y, int width, int height, Classes c, PixelColor color)
        {
            var img = imageTiles[id].overlaysClasses[c.classId];

            if (img is WriteableBitmap)
            {
                byte[] ColorData = new byte[height * width * 4];

                for (int i = 0; i < ColorData.Length; i += 4)
                {
                    ColorData[i + 0] = color.B;
                    ColorData[i + 1] = color.G;
                    ColorData[i + 2] = color.R;
                    ColorData[i + 3] = color.A;
                }

                img.WritePixels(new System.Windows.Int32Rect(x, y, width, height), ColorData, width * 4, 0);

                return img;
            }
            return null;
        }

        public static PixelColor Get(string _imageId, int x, int y)
        {
            string imageId = _imageId;

            ImageTile it = imageTiles[imageId];

            if (x < 0 && it.tileX == 0) x = 0;
            else if (x < 0)
            {
                it = it.grid.tiles[it.tileX - 1, it.tileY];
                x = 256 + x;
            }

            if (x > 255 && it.tileX == it.grid.width - 1) x = 255;
            else if (x > 255)
            {
                it = it.grid.tiles[it.tileX + 1, it.tileY];
                x = x - 256;

            }

            if (y < 0 && it.tileY == 0) y = 0;
            else if (y < 0)
            {
                it = it.grid.tiles[it.tileX, it.tileY - 1];
                y = 256 + y;
            }

            if (y > 255 && it.tileY == it.grid.height - 1) y = 255;
            else if (y > 255)
            {
                it = it.grid.tiles[it.tileX, it.tileY + 1];
                y = y - 256;
            }

            if (!images.ContainsKey(it.baseFileName))
            {
                it.Add();
            }

            return images[it.baseFileName][x, y];
        }

        private static void CopyPixels(BitmapImage bitmapImage, PixelColor[,] pixels, int stride, int offset)
        {
            int width = bitmapImage.PixelWidth;
            int height = bitmapImage.PixelHeight;

            byte[] pixelBytes = new byte[height * width * 4];

            bitmapImage.CopyPixels(pixelBytes, stride, 0);

            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[x + x0, y + y0] = new PixelColor
                    (
                        pixelBytes[(y * width + x) * 4 + 0],
                        pixelBytes[(y * width + x) * 4 + 1],
                        pixelBytes[(y * width + x) * 4 + 2],
                        pixelBytes[(y * width + x) * 4 + 3]
                    );
                }
            }

        }
        public static void ExportClassification(string imgId, Classes c0, Classes c1, Classes c2, Classes c3, string filePath)
        {
            List<WriteableBitmap> tmp = new List<WriteableBitmap>();
            SpinLock sl = new SpinLock();
            ExportClassification(imgId, c0, c1, c2, c3, filePath, ref tmp, -1, ref sl);
        }


        public static void ExportClassification(string imgId, Classes c0, Classes c1, Classes c2, Classes c3, string filePath, ref List<WriteableBitmap> Stitch, int index, ref SpinLock StitchLock)
        {
            int[] image_size = imagesSize[imageTiles[imgId].baseFileName];
            WriteableBitmap img = new WriteableBitmap(image_size[0], image_size[1], 96, 96, PixelFormats.Bgra32, null);

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    byte[] ColorData0 = new byte[4];
                    byte[] ColorData1 = new byte[4];
                    byte[] ColorData2 = new byte[4];
                    byte[] ColorData3 = new byte[4];


                    if (c0 != null) imageTiles[imgId].overlaysClasses[c0.classId].CopyPixels(new System.Windows.Int32Rect(x, y, 1, 1), ColorData0, 4, 0);
                    if (c1 != null) imageTiles[imgId].overlaysClasses[c1.classId].CopyPixels(new System.Windows.Int32Rect(x, y, 1, 1), ColorData1, 4, 0);
                    if (c2 != null) imageTiles[imgId].overlaysClasses[c2.classId].CopyPixels(new System.Windows.Int32Rect(x, y, 1, 1), ColorData2, 4, 0);
                    if (c3 != null) imageTiles[imgId].overlaysClasses[c3.classId].CopyPixels(new System.Windows.Int32Rect(x, y, 1, 1), ColorData3, 4, 0);

                    byte[] ColorData = new byte[4];

                    ColorData[2] = ColorData0[3]; //R
                    ColorData[1] = ColorData1[3]; //G
                    ColorData[0] = ColorData2[3]; //B
                    ColorData[3] = (byte)(255 - ColorData3[3]); //A (255 - a to have non transparent result)

                    img.WritePixels(new System.Windows.Int32Rect(x, y, 1, 1), ColorData, 4, 0);

                }
            }

            if (index >= 0)
            {
                bool t = false;
                StitchLock.Enter(ref t);
                byte[] pixelsArray = new byte[4 * 256 * 256];
                img.CopyPixels(pixelsArray, 4 * 256, 0);

                ImageTile it = imageTiles[imgId];
                Stitch[index].WritePixels(new System.Windows.Int32Rect(it.tileX * 256, it.tileY * 256, 256, 256), pixelsArray, 256 * 4, 0);
                StitchLock.Exit();

            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(fileStream);
            }
        }

        public static void DeleteImage(MainWindow window, string id)
        {
            foreach (ImageTile it in imageGrids[id].tiles)
            {
                if (it != null)
                {
                    string filename = it.baseFileName;
                    imageTiles.TryRemove(it.id, out ImageTile value);
                    if (!imageTiles.Values.Any((e) => e.baseFileName == filename))
                    {
                        images.TryRemove(filename, out PixelColor[,] r);
                        imagesSize.Remove(filename);
                        FeaturePoint.Remove(filename);
                    }

                    foreach (Classes c in MainWindow.classesList)
                    {
                        c.RemoveImageFeature(it.id);
                    }
                }
            }

            imageGrids.Remove(id);

        }

        public static void DeleteAll()
        {
            images.Clear();
            imagesSize.Clear();
            imageTiles.Clear();
            imageGrids.Clear();
            heightDataList.Clear();
        }

        private struct ImagePartialResults
        {
            public WriteableBitmap[] images;
            public int from;
            public int to;

            public ImagePartialResults(WriteableBitmap[] _images, int _from, int _to)
            {
                images = _images;
                from = _from;
                to = _to;
            }
        }

        private static ImagePartialResults RemoveLonelyPixelsPartial(MainWindow window, string imgId, int from, int to)
        {
            int[] size = imagesSize[imageTiles[imgId].baseFileName];
            WriteableBitmap[] outImg = new WriteableBitmap[MainWindow.classesList.Count];

            for (int i = 0; i < outImg.Length; i++)
            {
                outImg[i] = imageTiles[imgId].overlaysClasses[MainWindow.classesList[i].classId].Clone();
            }

            for (int _x = 0; _x < size[0]; _x++)
            {
                for (int _y = from; _y < to; _y++)
                {
                    int[] counters = new int[MainWindow.classesList.Count];
                    byte[] transparent = new byte[] { 0, 0, 0, 0 };
                    WriteableBitmap[] img = new WriteableBitmap[MainWindow.classesList.Count];

                    for (int t = 0; t < img.Length; t++)
                    {
                        img[t] = imageTiles[imgId].overlaysClasses[MainWindow.classesList[t].classId];
                    }

                    int centralClass = -1;
                    int maxClass = -1;
                    int max = -1;

                    for (int i = 0; i < counters.Length; i++)
                    {
                        byte[] Pixel = new byte[4];

                        img[i].CopyPixels(new System.Windows.Int32Rect(_x, _y, 1, 1), Pixel, 1 * 4, 0);

                        if (!Pixel.SequenceEqual(transparent))
                        {
                            centralClass = i;
                            break;
                        }
                    }

                    int radius = MainWindow.classesList[centralClass].radius;

                    for (int x = _x - radius; x <= _x + radius; x++)
                    {
                        for (int y = _y - radius; y <= _y + radius; y++)
                        {

                            bool found = false;

                            int tileX = imageTiles[imgId].tileX;
                            int tileY = imageTiles[imgId].tileY;

                            int x_ = x;
                            int y_ = y;

                            WriteableBitmap wb;

                            if (x >= 0 && x < size[0] && y > 0 && y < size[1])
                            {
                                found = true;
                            }
                            else if (x < 0 && imageTiles[imgId].tileX > 0)
                            {
                                tileX -= 1;
                                x_ += size[0];
                                if (y < 0 && imageTiles[imgId].tileY > 0)
                                {
                                    tileY -= 1;
                                    y_ += size[1];
                                }
                                else if (y >= size[1] && imageTiles[imgId].tileY < imageTiles[imgId].grid.height - 1)
                                {
                                    tileY += 1;
                                    y_ -= size[1];

                                }
                                else continue;
                            }
                            else if (x >= size[0] && imageTiles[imgId].tileX < imageTiles[imgId].grid.width - 1)
                            {
                                tileX += 1;
                                x_ -= size[0];
                                if (y < 0 && imageTiles[imgId].tileY > 0)
                                {
                                    tileY -= 1;
                                    y_ += size[1];

                                }
                                else if (y >= size[1] && imageTiles[imgId].tileY < imageTiles[imgId].grid.height - 1)
                                {
                                    tileY += 1;
                                    y_ -= size[1];

                                }
                                else continue;
                            }
                            else if (x >= 0 && x < size[0])
                            {
                                if (y < 0 && imageTiles[imgId].tileY > 0)
                                {
                                    tileY -= 1;
                                    y_ += size[1];

                                }
                                else if (y >= size[1] && imageTiles[imgId].tileY < imageTiles[imgId].grid.height - 1)
                                {
                                    tileY += 1;
                                    y_ -= size[1];

                                }
                                else continue;
                            }
                            else continue;

                            for (int i = 0; i < counters.Length; i++)
                            {
                                byte[] Pixel = new byte[4];
                                if (found)
                                {
                                    wb = img[i];
                                    wb.CopyPixels(new System.Windows.Int32Rect(x, y, 1, 1), Pixel, 1 * 4, 0);
                                }
                                else
                                {
                                    wb = imageTiles[imageTiles[imgId].grid.tiles[tileY, tileX].id].overlaysClasses[MainWindow.classesList[i].classId];

                                    MainWindow.dispatcher.Invoke(() =>
                                    {
                                        wb.CopyPixels(new System.Windows.Int32Rect(x_, y_, 1, 1), Pixel, 1 * 4, 0);
                                    });
                                }

                                if (!Pixel.SequenceEqual(transparent))
                                {
                                    counters[i]++;
                                    if (max < counters[i])
                                    {
                                        max = counters[i];
                                        maxClass = i;
                                    }
                                }
                            }
                        }
                    }

                    if (counters[centralClass] < MainWindow.classesList[centralClass].threshold)
                    {

                        outImg[centralClass].WritePixels(new System.Windows.Int32Rect(_x, _y, 1, 1), transparent, 1 * 4, 0);

                        Classes c = MainWindow.classesList[maxClass];
                        byte[] color = new byte[] { c.classColor.B, c.classColor.G, c.classColor.R, c.classColor.A };

                        outImg[maxClass].WritePixels(new System.Windows.Int32Rect(_x, _y, 1, 1), color, 1 * 4, 0);

                        bool lockAcquired = false;

                        spinLock.Enter(ref lockAcquired);

                        MainWindow.classesList[centralClass].classifiedPointsList[imgId][_y, _x] = null;
                        MainWindow.classesList[maxClass].classifiedPointsList[imgId][_y, _x] = (FeaturePoint.GetOrAddFeaturePoint(_x, _y, imgId));

                        spinLock.Exit();

                    }
                }

            }

            foreach (WriteableBitmap im in outImg)
            {
                im.Freeze();
            }

            return new ImagePartialResults(outImg, from, to);
        }

        public static void RemoveLonelyPixels(MainWindow window, string imgId, int nSteps, BackgroundWorker sender)
        {
            for (int step = 0; step < nSteps; step++)
            {
                WriteableBitmap[] img = new WriteableBitmap[MainWindow.classesList.Count];

                for (int t = 0; t < MainWindow.classesList.Count; t++)
                {
                    MainWindow.dispatcher.Invoke(() =>
                    {
                        imageTiles[imgId].overlaysClasses[MainWindow.classesList[t].classId].Freeze();
                        img[t] = imageTiles[imgId].overlaysClasses[MainWindow.classesList[t].classId].Clone();
                    });
                }
                int i = 0;

                int baseBlockSize = 8;
                int pCount = Environment.ProcessorCount;

                List<int> threadCount = new List<int>();
                List<int> blockSize = new List<int>();

                for (int pc = pCount; pc > 1; pc = (int)Math.Ceiling(pc / 2.0))
                {
                    threadCount.Add(pc);
                    blockSize.Add(baseBlockSize);
                }

                threadCount.Add(1);
                blockSize.Add(1);

                double c = 0;
                double total = 256 * 256;

                for (int k = 0; k < threadCount.Count && i < 256; k++)
                {
                    for (; i < 256 - (threadCount[k] - 1) * blockSize[k]; i += threadCount[k] * blockSize[k])
                    {
                        if (sender != null && sender.CancellationPending)
                        {
                            return;
                        }

                        Task<ImagePartialResults>[] childrenThread = new Task<ImagePartialResults>[threadCount[k]];
                        for (int n = 0; n < threadCount[k]; n++)
                        {

                            int indice = i + n * blockSize[k];

                            childrenThread[n] = Task.Run(() => RemoveLonelyPixelsPartial(window, imgId, indice, indice + blockSize[k]));
                        }

                        for (int n = 0; n < threadCount[k]; n++)
                        {
                            childrenThread[n].Wait();
                            ImagePartialResults r = childrenThread[n].Result;

                            int patchSize = 4 * 256 * (r.to - r.from + 1);

                            for (int t = 0; t < MainWindow.classesList.Count; t++)
                            {
                                byte[] Pixel = new byte[patchSize];

                                Int32Rect rect = new Int32Rect(0, r.from, 256, r.to - r.from);

                                r.images[t].CopyPixels(rect, Pixel, 4 * 256, 0); MainWindow.dispatcher.Invoke(() =>
                                {
                                    img[t].WritePixels(rect, Pixel, 4 * 256, 0);
                                });
                            }
                            c += 256 * (r.to - r.from + 1);

                            if (sender != null) sender.ReportProgress((int)((step + c / total) * 100 / nSteps), "*");
                        }
                    }
                }

                for (int t = 0; t < MainWindow.classesList.Count; t++)
                {
                    MainWindow.dispatcher.Invoke(() =>
                    {
                        imageTiles[imgId].overlaysClasses.AddOrUpdate(MainWindow.classesList[t].classId, img[t], (key, oldValue) => img[t]);
                        imageTiles[imgId].overlaysImages[MainWindow.classesList[t].classId].Source = img[t];
                    });
                }

            }
        }

        private static int[] BoxesForGauss(float sigma, int n)
        {
            double idealWidth = Math.Sqrt((12 * sigma * sigma / n) + 1);
            int widthL = (int)Math.Floor(idealWidth);
            if (widthL % 2 == 0) widthL--;

            int widthU = 2 * widthL;

            double idealM = (12.0 * sigma * sigma - n * widthL * widthL - 4 * n * widthL - 3 * n) / (-4 * widthL - 4);

            int m = (int)Math.Round(idealM);

            int[] sizes = new int[n];

            for (int i = 0; i < n; i++)
            {
                sizes[i] = i < m ? widthL : widthU;
            }

            return sizes;
        }

        private static void BoxBlurH_4(ref WriteableBitmap source, ref WriteableBitmap target, int radius, int from, int to)
        {
            double iarr = 1.0 / (radius * 2 + 1);
            for (int y = from; y < to; y++)
            {
                int ti = 0;
                int li = 0;
                int ri = ti + radius;

                byte[] Pixel = new byte[4];
                source.CopyPixels(new System.Windows.Int32Rect(0, y, 1, 1), Pixel, 1 * 4, 0);
                double fv0 = Pixel[0];
                double fv1 = Pixel[1];
                double fv2 = Pixel[2];
                double fv3 = Pixel[3];

                double val0 = (radius + 1) * fv0;
                double val1 = (radius + 1) * fv1;
                double val2 = (radius + 1) * fv2;
                double val3 = (radius + 1) * fv3;


                source.CopyPixels(new System.Windows.Int32Rect(source.PixelWidth - 1, y, 1, 1), Pixel, 1 * 4, 0);
                double lv0 = Pixel[0];
                double lv1 = Pixel[1];
                double lv2 = Pixel[2];
                double lv3 = Pixel[3];



                for (int j = 0; j < radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(j, y, 1, 1), Pixel, 1 * 4, 0);
                    val0 += Pixel[0];
                    val1 += Pixel[1];
                    val2 += Pixel[2];
                    val3 += Pixel[3];

                }

                for (int j = 0; j <= radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(ri++, y, 1, 1), Pixel, 1 * 4, 0);
                    val0 += Pixel[0] - fv0;
                    val1 += Pixel[1] - fv1;
                    val2 += Pixel[2] - fv2;
                    val3 += Pixel[3] - fv3;


                    Pixel[0] = (byte)(val0 * iarr);
                    Pixel[1] = (byte)(val1 * iarr);
                    Pixel[2] = (byte)(val2 * iarr);
                    Pixel[3] = (byte)(val3 * iarr);

                    target.WritePixels(new System.Windows.Int32Rect(ti++, y, 1, 1), Pixel, 1 * 4, 0);
                }

                for (int j = radius + 1; j < source.PixelWidth - radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(ri++, y, 1, 1), Pixel, 1 * 4, 0);
                    val0 += Pixel[0];
                    val1 += Pixel[1];
                    val2 += Pixel[2];
                    val3 += Pixel[3];


                    source.CopyPixels(new System.Windows.Int32Rect(li++, y, 1, 1), Pixel, 1 * 4, 0);
                    val0 -= Pixel[0];
                    val1 -= Pixel[1];
                    val2 -= Pixel[2];
                    val3 -= Pixel[3];


                    Pixel[0] = (byte)(val0 * iarr);
                    Pixel[1] = (byte)(val1 * iarr);
                    Pixel[2] = (byte)(val2 * iarr);
                    Pixel[3] = (byte)(val3 * iarr);

                    target.WritePixels(new System.Windows.Int32Rect(ti++, y, 1, 1), Pixel, 1 * 4, 0);
                }

                for (int j = source.PixelWidth - radius; j < source.PixelWidth; j++)
                {
                    val0 += lv0;
                    val1 += lv1;
                    val2 += lv2;
                    val3 += lv3;

                    source.CopyPixels(new System.Windows.Int32Rect(li++, y, 1, 1), Pixel, 1 * 4, 0);
                    val0 -= Pixel[0];
                    val1 -= Pixel[1];
                    val2 -= Pixel[2];
                    val3 -= Pixel[3];


                    Pixel[0] = (byte)(val0 * iarr);
                    Pixel[1] = (byte)(val1 * iarr);
                    Pixel[2] = (byte)(val2 * iarr);
                    Pixel[3] = (byte)(val3 * iarr);
                    target.WritePixels(new System.Windows.Int32Rect(ti++, y, 1, 1), Pixel, 1 * 4, 0);
                }
            }
        }

        private static void BoxBlurH_4(ref WriteableBitmap source, ref WriteableBitmap target, Classes c, int radius, int from, int to)
        {
            double iarr = 1.0 / (radius * 2 + 1);
            for (int y = from; y < to; y++)
            {
                int ti = 0;
                int li = 0;
                int ri = ti + radius;

                byte[] Pixel = new byte[4];
                source.CopyPixels(new System.Windows.Int32Rect(0, y, 1, 1), Pixel, 1 * 4, 0);
                double fv = Pixel[3];
                double val = (radius + 1) * fv;

                source.CopyPixels(new System.Windows.Int32Rect(source.PixelWidth - 1, y, 1, 1), Pixel, 1 * 4, 0);
                double lv = Pixel[3];


                for (int j = 0; j < radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(j, y, 1, 1), Pixel, 1 * 4, 0);
                    val += Pixel[3];
                }

                for (int j = 0; j <= radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(ri++, y, 1, 1), Pixel, 1 * 4, 0);
                    val += Pixel[3] - fv;

                    Pixel[0] = c.classColor.B;
                    Pixel[1] = c.classColor.G;
                    Pixel[2] = c.classColor.R;

                    Pixel[3] = (byte)(val * iarr);
                    target.WritePixels(new System.Windows.Int32Rect(ti++, y, 1, 1), Pixel, 1 * 4, 0);
                }

                for (int j = radius + 1; j < source.PixelWidth - radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(ri++, y, 1, 1), Pixel, 1 * 4, 0);
                    val += Pixel[3];

                    source.CopyPixels(new System.Windows.Int32Rect(li++, y, 1, 1), Pixel, 1 * 4, 0);
                    val -= Pixel[3];

                    Pixel[0] = c.classColor.B;
                    Pixel[1] = c.classColor.G;
                    Pixel[2] = c.classColor.R;

                    Pixel[3] = (byte)(val * iarr);
                    target.WritePixels(new System.Windows.Int32Rect(ti++, y, 1, 1), Pixel, 1 * 4, 0);
                }

                for (int j = source.PixelWidth - radius; j < source.PixelWidth; j++)
                {
                    val += lv;

                    source.CopyPixels(new System.Windows.Int32Rect(li++, y, 1, 1), Pixel, 1 * 4, 0);
                    val -= Pixel[3];

                    Pixel[0] = c.classColor.B;
                    Pixel[1] = c.classColor.G;
                    Pixel[2] = c.classColor.R;

                    Pixel[3] = (byte)(val * iarr);
                    target.WritePixels(new System.Windows.Int32Rect(ti++, y, 1, 1), Pixel, 1 * 4, 0);
                }
            }
        }

        private static void BoxBlurV_4(ref WriteableBitmap source, ref WriteableBitmap target, int radius, int from, int to)
        {
            double iarr = 1.0 / (radius * 2 + 1);
            for (int x = from; x < to; x++)
            {
                int ti = 0;
                int li = 0;
                int ri = radius;

                byte[] Pixel = new byte[4];
                source.CopyPixels(new System.Windows.Int32Rect(x, 0, 1, 1), Pixel, 1 * 4, 0);
                double fv0 = Pixel[0];
                double fv1 = Pixel[1];
                double fv2 = Pixel[2];
                double fv3 = Pixel[3];

                double val0 = (radius + 1) * fv0;
                double val1 = (radius + 1) * fv1;
                double val2 = (radius + 1) * fv2;
                double val3 = (radius + 1) * fv3;


                source.CopyPixels(new System.Windows.Int32Rect(x, source.PixelHeight - 1, 1, 1), Pixel, 1 * 4, 0);
                double lv0 = Pixel[0];
                double lv1 = Pixel[1];
                double lv2 = Pixel[2];
                double lv3 = Pixel[3];



                for (int j = 0; j < radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(x, j, 1, 1), Pixel, 1 * 4, 0);
                    val0 += Pixel[0];
                    val1 += Pixel[1];
                    val2 += Pixel[2];
                    val3 += Pixel[3];
                }

                for (int j = 0; j <= radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(x, ri++, 1, 1), Pixel, 1 * 4, 0);
                    val0 += Pixel[0] - fv0;
                    val1 += Pixel[1] - fv1;
                    val2 += Pixel[2] - fv2;
                    val3 += Pixel[3] - fv3;

                    Pixel[0] = (byte)(val0 * iarr);
                    Pixel[1] = (byte)(val1 * iarr);
                    Pixel[2] = (byte)(val2 * iarr);
                    Pixel[3] = (byte)(val3 * iarr);

                    target.WritePixels(new System.Windows.Int32Rect(x, ti++, 1, 1), Pixel, 1 * 4, 0);
                }

                for (int j = radius + 1; j < source.PixelHeight - radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(x, ri++, 1, 1), Pixel, 1 * 4, 0);
                    val0 += Pixel[0];
                    val1 += Pixel[1];
                    val2 += Pixel[2];
                    val3 += Pixel[3];


                    source.CopyPixels(new System.Windows.Int32Rect(x, li++, 1, 1), Pixel, 1 * 4, 0);
                    val0 -= Pixel[0];
                    val1 -= Pixel[1];
                    val2 -= Pixel[2];
                    val3 -= Pixel[3];


                    Pixel[0] = (byte)(val0 * iarr);
                    Pixel[1] = (byte)(val1 * iarr);
                    Pixel[2] = (byte)(val2 * iarr);
                    Pixel[3] = (byte)(val3 * iarr);

                    target.WritePixels(new System.Windows.Int32Rect(x, ti++, 1, 1), Pixel, 1 * 4, 0);
                }

                for (int j = source.PixelHeight - radius; j < source.PixelHeight; j++)
                {
                    val0 += lv0;
                    val1 += lv1;
                    val2 += lv2;
                    val3 += lv3;


                    source.CopyPixels(new System.Windows.Int32Rect(x, li++, 1, 1), Pixel, 1 * 4, 0);
                    val0 -= Pixel[0];
                    val1 -= Pixel[1];
                    val2 -= Pixel[2];
                    val3 -= Pixel[3];


                    Pixel[0] = (byte)(val0 * iarr);
                    Pixel[1] = (byte)(val1 * iarr);
                    Pixel[2] = (byte)(val2 * iarr);
                    Pixel[3] = (byte)(val3 * iarr);

                    target.WritePixels(new System.Windows.Int32Rect(x, ti++, 1, 1), Pixel, 1 * 4, 0);
                }
            }
        }

        private static void BoxBlurV_4(ref WriteableBitmap source, ref WriteableBitmap target, Classes c, int radius, int from, int to)
        {
            double iarr = 1.0 / (radius * 2 + 1);
            for (int x = from; x < to; x++)
            {
                int ti = 0;
                int li = 0;
                int ri = radius;

                byte[] Pixel = new byte[4];
                source.CopyPixels(new System.Windows.Int32Rect(x, 0, 1, 1), Pixel, 1 * 4, 0);
                double fv = Pixel[3];
                double val = (radius + 1) * fv;

                source.CopyPixels(new System.Windows.Int32Rect(x, source.PixelHeight - 1, 1, 1), Pixel, 1 * 4, 0);
                double lv = Pixel[3];


                for (int j = 0; j < radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(x, j, 1, 1), Pixel, 1 * 4, 0);
                    val += Pixel[3];
                }

                for (int j = 0; j <= radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(x, ri++, 1, 1), Pixel, 1 * 4, 0);
                    val += Pixel[3] - fv;

                    Pixel[0] = c.classColor.B;
                    Pixel[1] = c.classColor.G;
                    Pixel[2] = c.classColor.R;

                    Pixel[3] = (byte)(val * iarr);

                    target.WritePixels(new System.Windows.Int32Rect(x, ti++, 1, 1), Pixel, 1 * 4, 0);
                }

                for (int j = radius + 1; j < source.PixelHeight - radius; j++)
                {
                    source.CopyPixels(new System.Windows.Int32Rect(x, ri++, 1, 1), Pixel, 1 * 4, 0);
                    val += Pixel[3];

                    source.CopyPixels(new System.Windows.Int32Rect(x, li++, 1, 1), Pixel, 1 * 4, 0);
                    val -= Pixel[3];

                    Pixel[0] = c.classColor.B;
                    Pixel[1] = c.classColor.G;
                    Pixel[2] = c.classColor.R;

                    Pixel[3] = (byte)(val * iarr);
                    target.WritePixels(new System.Windows.Int32Rect(x, ti++, 1, 1), Pixel, 1 * 4, 0);
                }

                for (int j = source.PixelHeight - radius; j < source.PixelHeight; j++)
                {
                    val += lv;

                    source.CopyPixels(new System.Windows.Int32Rect(x, li++, 1, 1), Pixel, 1 * 4, 0);
                    val -= Pixel[3];

                    Pixel[0] = c.classColor.B;
                    Pixel[1] = c.classColor.G;
                    Pixel[2] = c.classColor.R;

                    Pixel[3] = (byte)(val * iarr);
                    target.WritePixels(new System.Windows.Int32Rect(x, ti++, 1, 1), Pixel, 1 * 4, 0);
                }
            }
        }

        private static void BoxBlur_4(ref WriteableBitmap source, ref WriteableBitmap target, int radius, int width, int height)
        {
            BoxBlurH_4(ref target, ref source, radius, 0, height);
            BoxBlurV_4(ref source, ref target, radius, 0, width);
        }

        private static void BoxBlur_4(ref WriteableBitmap source, ref WriteableBitmap target, Classes c, int radius)
        {
            BoxBlurH_4(ref target, ref source, c, radius, 0, 256);
            BoxBlurV_4(ref source, ref target, c, radius, 0, 256);
        }

        private static void BoxBlur_2(ref WriteableBitmap source, ref WriteableBitmap target, Classes c, int radius)
        {
            for (int _x = 0; _x < source.PixelWidth; _x++)
            {
                for (int _y = 0; _y < source.PixelHeight; _y++)
                {
                    byte[] Pixel = new byte[4];

                    double val = 0;
                    int counter = 0;

                    for (int x = Math.Max(_x - radius, 0); x <= Math.Min(_x + radius, source.Width - 1); x++)
                    {
                        for (int y = Math.Max(_y - radius, 0); y <= Math.Min(_y + radius, source.Height - 1); y++)
                        {
                            source.CopyPixels(new System.Windows.Int32Rect(x, y, 1, 1), Pixel, 1 * 4, 0);

                            val += Pixel[3];
                            counter++;
                        }
                    }
                    Pixel[0] = c.classColor.B;
                    Pixel[1] = c.classColor.G;
                    Pixel[2] = c.classColor.R;

                    Pixel[3] = (byte)(val / counter);
                    target.WritePixels(new System.Windows.Int32Rect(_x, _y, 1, 1), Pixel, 1 * 4, 0);
                }

            }
        }

        public static void BluringEdges_2(MainWindow window, string imgId, int radius)
        {
            if (radius > 0)
            {
                int[] boxes = BoxesForGauss(radius, 3);

                foreach (Classes c in MainWindow.classesList)
                {
                    WriteableBitmap img = imageTiles[imgId].overlaysClasses[c.classId];

                    WriteableBitmap img2 = img.Clone();

                    BoxBlur_2(ref img, ref img2, c, (boxes[0] - 1) / 2);
                    BoxBlur_2(ref img2, ref img, c, (boxes[1] - 1) / 2);
                    BoxBlur_2(ref img, ref img2, c, (boxes[2] - 1) / 2);

                    imageTiles[imgId].overlaysClasses[c.classId] = img2;
                    imageTiles[imgId].overlaysImages[c.classId].Source = img2;

                }
            }
        }

        public static void BluringEdges_4(MainWindow window, ref List<WriteableBitmap> imgs, int index, int radius)
        {
            if (radius > 0)
            {
                int[] boxes = BoxesForGauss(radius, 3);
                WriteableBitmap img = imgs[index];

                WriteableBitmap img2 = img.Clone();

                BoxBlur_4(ref img, ref img2, (boxes[0] - 1) / 2, img.PixelWidth, img.PixelHeight);
                BoxBlur_4(ref img, ref img2, (boxes[1] - 1) / 2, img.PixelWidth, img.PixelHeight);
                BoxBlur_4(ref img, ref img2, (boxes[2] - 1) / 2, img.PixelWidth, img.PixelHeight);

                imgs[index] = img2;
            }
        }

        public static void BluringEdges_4(MainWindow window, string imgId, int radius)
        {
            if (radius > 0)
            {
                int[] boxes = BoxesForGauss(radius, 3);

                foreach (Classes c in MainWindow.classesList)
                {
                    WriteableBitmap img = imageTiles[imgId].overlaysClasses[c.classId].Clone();
                    WriteableBitmap img2 = img.Clone();

                    BoxBlur_4(ref img, ref img2, c, (boxes[0] - 1) / 2);
                    BoxBlur_4(ref img2, ref img, c, (boxes[1] - 1) / 2);
                    BoxBlur_4(ref img, ref img2, c, (boxes[2] - 1) / 2);

                    imageTiles[imgId].overlaysClasses[c.classId] = img2;
                    imageTiles[imgId].overlaysImages[c.classId].Source = img2;
                }
            }
        }

        public static void BluringEdges(MainWindow window, string imgId, int radius)
        {
            if (radius > 0)
            {
                int rs = (int)Math.Ceiling(radius * 2.57);

                foreach (Classes c in MainWindow.classesList)
                {
                    WriteableBitmap img = imageTiles[imgId].overlaysClasses[c.classId];

                    WriteableBitmap img2 = img.Clone();

                    for (int _x = 0; _x < img.Width; _x++)
                    {
                        for (int _y = 0; _y < img.Height; _y++)
                        {
                            byte[] Pixel = new byte[4];

                            double val = 0, weightSum = 0;
                            for (int x = Math.Max(_x - rs, 0); x <= Math.Min(_x + rs, img.Width - 1); x++)
                            {
                                for (int y = Math.Max(_y - rs, 0); y <= Math.Min(_y + rs, img.Height - 1); y++)
                                {
                                    double norm = (x - _x) * (x - _x) + (y - _y) * (y - _y);
                                    double weight = (Math.Exp(-norm / (2.0 * radius * radius)) / (Math.PI * 2.0 * radius * radius));

                                    img.CopyPixels(new System.Windows.Int32Rect(x, y, 1, 1), Pixel, 1 * 4, 0);

                                    val += Pixel[3] * weight;
                                    weightSum += weight;
                                }
                            }
                            Pixel[0] = c.classColor.B;
                            Pixel[1] = c.classColor.G;
                            Pixel[2] = c.classColor.R;

                            Pixel[3] = (byte)val;
                            img2.WritePixels(new System.Windows.Int32Rect(_x, _y, 1, 1), Pixel, 1 * 4, 0);
                        }
                    }

                    imageTiles[imgId].overlaysClasses[c.classId] = img2;
                    imageTiles[imgId].overlaysImages[c.classId].Source = img2;
                }
            }
        }
    }
}
