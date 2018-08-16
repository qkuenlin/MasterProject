using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static alglib;
using static ClassificationTool.ImageHelper;

namespace ClassificationTool
{
    class ClassificationHelper
    {
        public class BatchProcessParam
        {
            public int zoom;
            public int x0, x1;
            public int y0, y1;

            public BatchProcessParam(int zoom_, int x0_, int x1_, int y0_, int y1_)
            {
                zoom = zoom_;
                x0 = x0_;
                x1 = x1_;
                y0 = y0_;
                y1 = y1_;
            }
        }

        public class ClassifyProcessParam
        {
            public int zoom;
            public int WorldX;
            public int WorldY;
            public int TileX;
            public int TileY;

            public ClassifyProcessParam(int zoom_, int TileX_, int TileY_, int x_, int y_)
            {
                zoom = zoom_;
                TileX = TileX_;
                TileY = TileY_;
                WorldX = x_;
                WorldY = y_;
            }

            public override bool Equals(object obj)
            {
                ClassifyProcessParam o = obj as ClassifyProcessParam;
                return (o is ClassifyProcessParam) && zoom == o.zoom && o.TileX == TileX && o.TileY == TileY;
            }

            public override int GetHashCode()
            {
                return zoom << 16 + TileX << 8 + TileY;
            }
        }

        public static Thread TreeToStringThread;

        private static bool writingTree = false;
        public static bool stopPreWriting = false;
        public static bool treeWritten = false;
        public static string treeString = "";

        private static volatile int classificationQueue = 0;
        private static volatile int classificationDone = 0;

        private static volatile int postprocessQueue = 0;
        private static volatile int postprocessgDone = 0;

        private static volatile int outputDone = 0;
        private static volatile float averageOutputTime = 0;

        private static volatile bool[,] postProcessed;


        private static volatile float averageClassificationTime = 0;
        private static volatile float averagePostprocessTime = 0;

        static int Size;

        static int MaxConcurrent = 7;

        static ConcurrentQueue<ClassifyProcessParam> errorClassificationRecoveryQueue;
        static ConcurrentQueue<string> classificationDoneQueue;
        static ConcurrentQueue<string> postProcessingQueue;
        static ConcurrentQueue<string> ErrorPostProcessingQueue;
        static ConcurrentQueue<string> postprocessDoneQueue;

        static List<WriteableBitmap> Stitch;
        static SpinLock StichLock;

        public static string TreeString()
        {
            if (writingTree) MainWindow.Log("Waiting on Tree String");
            while (writingTree) Thread.Sleep(100);

            return treeString;
        }

        public static void TreeToString(MainWindow window)
        {
            writingTree = true;

            int length = 64;

            int blockSize = (int)Math.Ceiling(1.0 * window.DecisionForest.innerobj.trees.Length / length);

            string[] seq = new string[length];

            DateTime time = DateTime.Now;
            MainWindow.Log("Prewriting Tree -- BlockSize: " + blockSize + "  --  length: " + length + "  --  trees: " + window.DecisionForest.innerobj.trees.Length);

            Parallel.For(0, length, j =>
            {
                string s = "";

                for (int k = j * blockSize; k < (j + 1) * blockSize && k < window.DecisionForest.innerobj.trees.Length; k++)
                {
                    if (stopPreWriting)
                    {
                        writingTree = false;
                        stopPreWriting = false;
                        return;
                    }
                    s += window.DecisionForest.innerobj.trees[k] + "" + SaveLoad.Space1;
                    if (s.Count() > 4096)
                    {
                        seq[j] += s;
                        s = "";
                    }
                }
                seq[j] += s;
            });
            treeString = seq[0].Length > 0 ? seq.Aggregate((a, b) => a + b) : "";

            MainWindow.Log("Prewriting Tree done in " + MainWindow.TimeToString(DateTime.Now - time));

            writingTree = false;
            treeWritten = true;
        }

        public static double[][] ConvertFeaturesToArray(object sender, List<String> GridIds)
        {
            ConcurrentBag<double[]> features = new ConcurrentBag<double[]>();

            int n = 0;
            foreach (string gridId in GridIds)
            {
                n++;
                (sender as BackgroundWorker).ReportProgress(0, "Retrieving Features (Grid " + n + "/" + GridIds.Count + ")");
                int i = 0;

                for (int x = 0; x < ImageHelper.imageGrids[gridId].width; x++)
                {
                    for (int y = 0; y < ImageHelper.imageGrids[gridId].height; y++)
                    {
                        int x2 = x + 1 < ImageHelper.imageGrids[gridId].width ? x + 1 : 0;
                        int y2 = x2 == 0 ? y + 1 < ImageHelper.imageGrids[gridId].height ? y + 1 : -1 : y;

                        if (y2 != -1)
                        {
                            Thread th = new Thread(() => ImageHelper.imageGrids[gridId].tiles[y2, x2].Add());
                            th.Start();
                        }

                        ImageHelper.ImageTile it = ImageHelper.imageGrids[gridId].tiles[y, x];
                        it.Lock();
                        if ((sender as BackgroundWorker).CancellationPending)
                        {
                            return new double[0][];
                        }

                        int k = 0;

                        int total = 0;
                        foreach (Classes c in MainWindow.classesList)
                        {
                            if (c.featurePoints.ContainsKey(it.id))
                            {
                                total += c.featurePoints[it.id].Count;
                            }
                        }

                        Parallel.ForEach(MainWindow.classesList, c =>
                        {
                            if (c.featurePoints.ContainsKey(it.id))
                            {
                                foreach (FeaturePoint fp in c.featurePoints[it.id])
                                {
                                    double[] feat = fp.GetFeatures();
                                    double[] feats = new double[feat.Length + 1];
                                    for (int j = 0; j < FeaturePoint.NFEATURES; j++)
                                    {
                                        feats[j] = feat[j];
                                    }

                                    feats[FeaturePoint.NFEATURES] = c.classNumber;
                                    features.Add(feats);

                                    k++;

                                    (sender as BackgroundWorker).ReportProgress((i * 100 + k * 100 / total) / ImageHelper.imageGrids[gridId].tiles.Length, "*");

                                }
                            }
                        });

                        it.Unlock();
                        i++;
                        (sender as BackgroundWorker).ReportProgress((i) * 100 / ImageHelper.imageGrids[gridId].tiles.Length, "*");
                    }

                }
            }


            return features.ToArray();
        }

        public static void BatchClassification(BackgroundWorker sender, ImageGrid g, Queue<ClassifyProcessParam> q, int classifier, string path)
        {
            int NStep = 0;
            int blurRadius = 0;
            errorClassificationRecoveryQueue = new ConcurrentQueue<ClassifyProcessParam>();
            classificationDoneQueue = new ConcurrentQueue<string>();
            postProcessingQueue = new ConcurrentQueue<string>();
            ErrorPostProcessingQueue = new ConcurrentQueue<string>();
            postprocessDoneQueue = new ConcurrentQueue<string>();
            outputDone = 0;

            MainWindow.dispatcher.Invoke(() =>
            {
                NStep = int.Parse(MainWindow.Instance.NSteps.Text);
                blurRadius = int.Parse(MainWindow.Instance.BlurRadius.Text);
                string[] thresholds = MainWindow.Instance.LonelinessThreshold.Text.Split(';');
                string[] radius = MainWindow.Instance.LookRadius.Text.Split(';');

                for (int i = 0; i < MainWindow.classesList.Count; i++)
                {
                    int i_t = i < thresholds.Length ? i : thresholds.Length - 1;
                    int i_r = i < radius.Length ? i : radius.Length - 1;

                    MainWindow.classesList[i].threshold = int.Parse(thresholds[i_t]);
                    MainWindow.classesList[i].radius = int.Parse(radius[i_r]);
                }

                Stitch = new List<WriteableBitmap>();
                StichLock = new SpinLock();
                for (int i = 0; i < MainWindow.classesList.Count; i += 4)
                {
                    Stitch.Add(new WriteableBitmap(256 * g.width, 256 * g.height, 96, 96, PixelFormats.Bgra32, null));
                }
            });
            postprocessQueue = 0;
            classificationQueue = 0;

            Size = g.width * g.height;

            postProcessed = new bool[g.width, g.height];
            for (int i = 0; i < g.width; i++)
            {
                for (int j = 0; j < g.height; j++)
                {
                    postProcessed[i, j] = false;
                }
            }

            while (outputDone != Size)
            {
                //PostProcess is prioritize
                if (postprocessQueue < MaxConcurrent)
                {
                    if (ErrorPostProcessingQueue.Count > 0)
                    {
                        ErrorPostProcessingQueue.TryDequeue(out string s);
                        if (s != null)
                        {
                            Task.Run(() => PostProcess(sender, g, s, NStep, blurRadius));
                            postprocessQueue++;
                        }
                    }
                    else if (postProcessingQueue.Count > 0)
                    {
                        postProcessingQueue.TryDequeue(out string s);
                        if (s != null)
                        {
                            Task.Run(() => PostProcess(sender, g, s, NStep, blurRadius));
                            postprocessQueue++;
                        }
                    }
                }

                //Output whenever it is possible, even if all thread are in use
                int n = postprocessDoneQueue.Count;
                for (int i = 0; i < n; i++)
                {
                    postprocessDoneQueue.TryDequeue(out string s);
                    if (s != null)
                    {
                        if (canOutput(g, imageTiles[s])) Task.Run(() => Output(sender, g, s, path));
                        else postprocessDoneQueue.Enqueue(s);
                    }
                }

                if (classificationQueue < MaxConcurrent)
                {
                    if (errorClassificationRecoveryQueue.Count > 0)
                    {
                        errorClassificationRecoveryQueue.TryDequeue(out ClassifyProcessParam p_);
                        if (p_ != null)
                        {
                            Task.Run(() => Classifiy(sender, g, p_, classifier, NStep, path, blurRadius));
                            classificationQueue++;
                        }
                    }
                    else if (q.Count > 0)
                    {
                        ClassifyProcessParam p = q.Dequeue();
                        Task.Run(() => Classifiy(sender, g, p, classifier, NStep, path, blurRadius));
                        classificationQueue++;
                    }
                }

                n = classificationDoneQueue.Count;
                for (int i = 0; i < n; i++)
                {
                    classificationDoneQueue.TryDequeue(out string s);
                    if (s != null)
                    {
                        if (canPostProcess(g, imageTiles[s])) postProcessingQueue.Enqueue(s);
                        else classificationDoneQueue.Enqueue(s);
                    }
                }
            }

            while (outputDone != Size)
            {
                Thread.Yield();
                int n = postprocessDoneQueue.Count;
                for (int i = 0; i < n; i++)
                {
                    postprocessDoneQueue.TryDequeue(out string s);
                    if (s != null)
                    {
                        if (canOutput(g, imageTiles[s])) Task.Run(() => Output(sender, g, s, path));
                        else postprocessDoneQueue.Enqueue(s);
                    }
                }
            }

            OutputStitch(sender, path, blurRadius);

            Thread.Sleep(500);
        }

        private static void OutputStitch(BackgroundWorker sender, string path, int blurRadius)
        {
            for (int i = 0; i < MainWindow.classesList.Count; i += 4)
            {
                Classes c0 = MainWindow.classesList[i];
                Classes c1 = i + 1 < MainWindow.classesList.Count ? MainWindow.classesList[i + 1] : null;
                Classes c2 = i + 2 < MainWindow.classesList.Count ? MainWindow.classesList[i + 2] : null;
                Classes c3 = i + 3 < MainWindow.classesList.Count ? MainWindow.classesList[i + 3] : null;

                string classesNames = c0.className.Replace(' ', '_');
                classesNames += c1 != null ? c1.className.Replace(' ', '_') : "";
                classesNames += c2 != null ? c2.className.Replace(' ', '_') : "";
                classesNames += c3 != null ? c3.className.Replace(' ', '_') : "";

                string filePath = path + "\\Stich" + classesNames + ".png";

                MainWindow.dispatcher.Invoke(() =>
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(Stitch[i / 4]));
                        encoder.Save(fileStream);
                    }

                    filePath = path + "\\Stich" + classesNames + "_blurred" + blurRadius/2 + ".png";

                    BluringEdges_4(MainWindow.Instance, ref Stitch, i / 4, blurRadius/2);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(Stitch[i / 4]));
                        encoder.Save(fileStream);
                    }

                    filePath = path + "\\Stich" + classesNames + "_blurred"+blurRadius+".png";                    

                    BluringEdges_4(MainWindow.Instance, ref Stitch, i/4, blurRadius);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(Stitch[i / 4]));
                        encoder.Save(fileStream);
                    }
                });
            }

        }

        public static void Output(BackgroundWorker sender, ImageGrid g, string id_, string path)
        {
            ImageTile it = imageTiles[id_];
            DateTime time = DateTime.Now;

            MainWindow.Log("Saving Image (" + it.worldX + " " + it.worldY + ")");

            time = DateTime.Now;
           
            {

                for (int i = 0; i < MainWindow.classesList.Count; i += 4)
                {
                    Classes c0 = MainWindow.classesList[i];
                    Classes c1 = i + 1 < MainWindow.classesList.Count ? MainWindow.classesList[i + 1] : null;
                    Classes c2 = i + 2 < MainWindow.classesList.Count ? MainWindow.classesList[i + 2] : null;
                    Classes c3 = i + 3 < MainWindow.classesList.Count ? MainWindow.classesList[i + 3] : null;

                    string classesNames = c0.className.Replace(' ', '_');
                    classesNames += c1 != null ? c1.className.Replace(' ', '_') : "";
                    classesNames += c2 != null ? c2.className.Replace(' ', '_') : "";
                    classesNames += c3 != null ? c3.className.Replace(' ', '_') : "";

                    string filename = path + "\\" + (it.worldY) + "_" + (it.worldX) + "_" + classesNames + ".png";

                    MainWindow.dispatcher.Invoke(() =>
                    {
                        ImageHelper.ExportClassification(it.id, c0, c1, c2, c3, filename, ref Stitch, i / 4, ref StichLock);
                    });
                }

            }
            

            MainWindow.dispatcher.Invoke(() =>
               g.RemoveTile(it.tileX, it.tileY)
            );

            float t = (float)(DateTime.Now - time).TotalSeconds;

            averageOutputTime = (averageOutputTime * outputDone + t) / (outputDone + 1);
            outputDone++;
            ReportProgress(sender);

            MainWindow.Log("Image (" + it.worldX + " " + it.worldY + ") saved in " + t + "s. Done");

            GC.Collect();
            
        }

        public static void PostProcess(BackgroundWorker sender, ImageGrid g, string id_, int NStep, int blurRadius)
        {
            ImageTile it = imageTiles[id_];
            DateTime time = DateTime.Now;
            MainWindow.Log("Postprocessing Image (" + it.worldX + " " + it.worldY + ")");

            try
            {
                //First Erase the not more useful color image
                images.TryRemove(it.baseFileName, out PixelColor[,] r);


                it.Status(new PixelColor(255, 0, 120, 100));

                for (int step = 0; step < NStep; step++)
                {
                    for (int _x = 0; _x < 256; _x++)
                    {
                        for (int _y = 0; _y < 256; _y++)
                        {
                            int centralClass = -1;
                            int maxClass = -1;
                            int maxValue = -1;

                            int[] counters = new int[MainWindow.classesList.Count];

                            for (int i = 0; i < counters.Length; i++)
                            {
                                if (MainWindow.classesList[i].classifiedPointsList[it.id][_y, _x] != null)
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

                                    int tileX = it.tileX;
                                    int tileY = it.tileY;

                                    int x_ = x;
                                    int y_ = y;

                                    if (x >= 0 && x < 256 && y > 0 && y < 256)
                                    {
                                        found = true;
                                    }
                                    else if (x < 0 && it.tileX > 0)
                                    {
                                        tileX -= 1;
                                        x_ += 256;
                                        if (y < 0 && it.tileY > 0)
                                        {
                                            tileY -= 1;
                                            y_ += 256;
                                        }
                                        else if (y >= 256 && it.tileY < g.height - 1)
                                        {
                                            tileY += 1;
                                            y_ -= 256;

                                        }
                                        else continue;
                                    }
                                    else if (x >= 256 && it.tileX < g.width - 1)
                                    {
                                        tileX += 1;
                                        x_ -= 256;
                                        if (y < 0 && it.tileY > 0)
                                        {
                                            tileY -= 1;
                                            y_ += 256;

                                        }
                                        else if (y >= 256 && it.tileY < g.height - 1)
                                        {
                                            tileY += 1;
                                            y_ -= 256;

                                        }
                                        else continue;
                                    }
                                    else if (x >= 0 && x < 256)
                                    {
                                        if (y < 0 && it.tileY > 0)
                                        {
                                            tileY -= 1;
                                            y_ += 256;

                                        }
                                        else if (y >= 255 && it.tileY < g.height - 1)
                                        {
                                            tileY += 1;
                                            y_ -= 256;

                                        }
                                        else continue;
                                    }
                                    else continue;

                                    for (int i = 0; i < counters.Length; i++)
                                    {
                                        string id;
                                        if (found)
                                        {
                                            id = it.id;
                                        }
                                        else
                                        {
                                            id = g.tiles[tileX, tileY].id;
                                        }
                                        if (MainWindow.classesList[i].classifiedPointsList.ContainsKey(id))
                                        {
                                            if (MainWindow.classesList[i].classifiedPointsList[id][y_, x_] != null)
                                            {
                                                counters[i]++;
                                                if (maxValue < counters[i])
                                                {
                                                    maxValue = counters[i];
                                                    maxClass = i;
                                                }
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            MainWindow.Log("ERROR with Image (" + it.worldX + " " + it.worldY + ") while postprocessing: Image with id " + id + " is not present in classified points. Trying to continue execution without it.");
                                        }

                                    }
                                }
                            }

                            if (counters[centralClass] < MainWindow.classesList[centralClass].threshold)
                            {
                                MainWindow.classesList[centralClass].classifiedPointsList[it.id][_y, _x] = null;
                                MainWindow.classesList[maxClass].classifiedPointsList[it.id][_y, _x] = (FeaturePoint.GetOrAddFeaturePoint(_x, _y, it.id));
                            }
                        }
                    }
                }

                MainWindow.dispatcher.Invoke(() =>
                {
                    foreach (Classes c in MainWindow.classesList)
                    {
                        for (int i = 0; i < 256; i++)
                        {
                            for (int j = 0; j < 256; j++)
                            {
                                FeaturePoint fp = c.classifiedPointsList[it.id][i, j];
                                if (fp != null)
                                {
                                    ImageHelper.DrawOverlayClass(it.id, fp.x, fp.y, c, c.classColor);
                                }
                            }
                        }
                    }

                   // BluringEdges_4(MainWindow.Instance, it.id, blurRadius);
                });

                postProcessed[it.tileX, it.tileY] = true;

                float t = (float)(DateTime.Now - time).TotalSeconds;

                averagePostprocessTime = (averagePostprocessTime * postprocessgDone + t) / (postprocessgDone + 1);
                postprocessgDone++;
                ReportProgress(sender);
                postprocessQueue--;

                MainWindow.Log("Image (" + it.worldX + " " + it.worldY + ") postprocessing done in " + t + "s. Now waiting for output and dispose");

                postprocessDoneQueue.Enqueue(id_);

                it.Status(new PixelColor(0, 0, 0, 0));

                GC.Collect();

            }
            catch (Exception e)
            {
                MainWindow.Log("ERROR when postprocessing Image (" + it.worldX + " " + it.worldY + "): " + e.Message);

                it.Status(new PixelColor(0, 0, 255, 100));

                if (postProcessed[it.tileX, it.tileY]) { postprocessQueue++; postprocessgDone--; }
                postProcessed[it.tileX, it.tileY] = false;

                it.Unlock();

                ErrorPostProcessingQueue.Enqueue(id_);

                GC.Collect();
            }
        }

        public static void Classifiy(BackgroundWorker sender, ImageGrid g, ClassifyProcessParam p, int classifier, int NStep, string path, int blurRadius)
        {

            MainWindow.Log("Loading Image (" + p.WorldX + " " + p.WorldY + ")");

            DateTime time = DateTime.Now;

            ImageTile it = null;
            int max = (int)Math.Pow(2, p.zoom);

            int TileX = p.TileX - 1;
            for (int x = p.WorldX - 1; x <= p.WorldX + 1; x++, TileX++)
            {
                int TileY = p.TileY - 1;
                for (int y = p.WorldY - 1; y <= p.WorldY + 1; y++, TileY++)
                {
                    if (x >= 0 && x < max && y >= 0 && y < max && TileX >= 0 && TileX < g.width && TileY >= 0 && TileY < g.height)
                    {
                        string id = g.id + "_" + p.zoom + "_" + y + "_" + x;
                        string filename = "tmp\\sat\\" + (p.zoom) + "_" + (y) + "_" + (x) + ".png";

                        if (x == p.WorldX && y == p.WorldY)
                        {
                            MainWindow.dispatcher.Invoke(() =>
                            {
                                if (imageTiles.ContainsKey(id)) it = imageTiles[id];
                                else it = new ImageTile(MainWindow.Instance, g, filename, id, p.zoom, TileX, TileY, false);
                            });
                        }
                        else
                        {
                            MainWindow.dispatcher.Invoke(() =>
                            {
                                if (!imageTiles.ContainsKey(id))
                                {
                                    ImageTile it_ = new ImageTile(MainWindow.Instance, g, filename, id, p.zoom, TileX, TileY, false);
                                    g.AddTile(it_);
                                }
                            });
                        }
                    }
                }
            }

            if (it == null) return;
            g.AddTile(it);
            it.Status(new PixelColor(0, 255, 255, 100));

            it.classified = false;
            it.Lock();

            MainWindow.Log("Classifying Image (" + p.WorldX + " " + p.WorldY + ")");

            try
            {
                MainWindow.dispatcher.Invoke(() =>
                 {
                     foreach (Classes c in MainWindow.classesList)
                     {
                         g.AddOverlayClass(it, c, MainWindow.Instance);

                         if (c.classifiedPointsList.ContainsKey(it.id)) c.classifiedPointsList.Remove(it.id);

                         c.classifiedPointsList.Add(it.id, new FeaturePoint[256, 256]);
                     }

                 });

                Parallel.For(0, 256, x =>
                {
                    for (int y = 0; y < 256; y++)
                    {
                        FeaturePoint fp = FeaturePoint.GetOrAddFeaturePoint(x, y, it.id);
                        var output = Array.Empty<double>();

                        switch (classifier)
                        {
                            case 0:
                                dfprocess(MainWindow.Instance.DecisionForest, fp.GetFeatures(), ref output);
                                break;
                            case 1:
                                mlpprocess(MainWindow.Instance.NeuralNetwork, fp.GetFeatures(), ref output);
                                break;
                            case 2:
                                mlpeprocess(MainWindow.Instance.NeuralNetworkEnsemble, fp.GetFeatures(), ref output);
                                break;
                            default: break;
                        }

                        int predictedClass = 0;
                        for (int k = 1; k < output.Length; k++)
                        {
                            if (output[k] > output[predictedClass])
                            {
                                predictedClass = k;
                            }
                        }

                        MainWindow.GetClassByNum(predictedClass).classifiedPointsList[it.id][fp.y, fp.x] = fp;
                    }
                });

                it.Unlock();

                it.classified = true;

                float t = (float)(DateTime.Now - time).TotalSeconds;

                averageClassificationTime = (averageClassificationTime * classificationDone + t) / (classificationDone + 1);

                classificationDone++;
                ReportProgress(sender);

                classificationQueue--;

                MainWindow.Log("Image (" + p.WorldX + " " + p.WorldY + ") Classification done in " + t + "s. Now waiting for postprocessing");

                GC.Collect();

                it.Status(new PixelColor(0, 255, 0, 100));

                classificationDoneQueue.Enqueue(it.id);
            }
            catch (Exception e)
            {
                MainWindow.Log("ERROR when classifying Image (" + p.WorldX + " " + p.WorldY + "): " + e.Message);

                it.Status(new PixelColor(0, 0, 255, 100));

                if (it.classified) { classificationQueue++; classificationDone--; }

                it.classified = false;
                it.Unlock();

                errorClassificationRecoveryQueue.Enqueue(p);

                GC.Collect();
            }
        }

        private static bool canPostProcess(ImageGrid g, ImageTile it)
        {
            if (!it.classified) return false;

            for (int x = Math.Max(0, it.tileX - 1); x <= Math.Min(g.width - 1, it.tileX + 1); x++)
            {
                for (int y = Math.Max(0, it.tileY - 1); y <= Math.Min(g.height - 1, it.tileY + 1); y++)
                {
                    if (!g.tiles[x, y].classified && !postProcessed[it.tileX, it.tileY]) return false;
                }
            }

            return true;
        }

        private static bool canOutput(ImageGrid g, ImageTile it)
        {
            if (!postProcessed[it.tileX, it.tileY]) return false;

            for (int x = Math.Max(0, it.tileX - 1); x <= Math.Min(g.width - 1, it.tileX + 1); x++)
            {
                for (int y = Math.Max(0, it.tileY - 1); y <= Math.Min(g.height - 1, it.tileY + 1); y++)
                {
                    if (!postProcessed[x, y]) return false;
                }
            }

            return true;
        }

        private static void ReportProgress(BackgroundWorker sender)
        {
            float estimatedTime = (Size - classificationDone) * averageClassificationTime / (MaxConcurrent - 1) + ((Size - postprocessgDone) * averagePostprocessTime + (Size - outputDone) * averageOutputTime) / (MaxConcurrent - 2);

            sender.ReportProgress((90 * classificationDone + 5 * postprocessgDone + 5 * outputDone) / (Size), "Batch Processing: Classified " + classificationDone + "/" + Size + " -- Postprocessed " + postprocessgDone + "/" + Size + " -- Output " + outputDone + "/" + Size + " (Estimated time remaining: " + MainWindow.TimeToString(TimeSpan.FromSeconds(estimatedTime)) + ")");
        }

        internal static Dictionary<int, List<double[]>> GetAllFeatures(object sender)
        {
            Dictionary<int, List<double[]>> features = new Dictionary<int, List<double[]>>();

            int n = 0;
            foreach (string gridId in ImageHelper.imageGrids.Keys)
            {
                n++;
                (sender as BackgroundWorker).ReportProgress(0, "Retrieving Features (Grid " + n + "/" + ImageHelper.imageGrids.Count + ")");
                int i = 0;

                for (int x = 0; x < ImageHelper.imageGrids[gridId].width; x++)
                {
                    for (int y = 0; y < ImageHelper.imageGrids[gridId].height; y++)
                    {
                        int x2 = x + 1 < ImageHelper.imageGrids[gridId].width ? x + 1 : 0;
                        int y2 = x2 == 0 ? y + 1 < ImageHelper.imageGrids[gridId].height ? y + 1 : -1 : y;

                        if (y2 != -1)
                        {
                            Thread th = new Thread(() => ImageHelper.imageGrids[gridId].tiles[y2, x2].Add());
                            th.Start();
                        }

                        ImageHelper.ImageTile it = ImageHelper.imageGrids[gridId].tiles[y, x];
                        it.Lock();
                        if ((sender as BackgroundWorker).CancellationPending)
                        {
                            return null;
                        }

                        int k = 0;

                        int total = 0;
                        foreach (Classes c in MainWindow.classesList)
                        {
                            if (!features.ContainsKey(c.classNumber)) features.Add(c.classNumber, new List<double[]>());

                            if (c.featurePoints.ContainsKey(it.id))
                            {
                                total += c.featurePoints[it.id].Count;
                            }
                        }

                        Parallel.ForEach(MainWindow.classesList, c =>
                        {
                            if (c.featurePoints.ContainsKey(it.id))
                            {
                                foreach (FeaturePoint fp in c.featurePoints[it.id])
                                {
                                    double[] feat = fp.GetFeatures();
                                    double[] feats = new double[feat.Length];
                                    for (int j = 0; j < FeaturePoint.NFEATURES; j++)
                                    {
                                        feats[j] = feat[j];
                                    }

                                    features[c.classNumber].Add(feats);

                                    k++;

                                    (sender as BackgroundWorker).ReportProgress((i * 100 + k * 100 / total) / ImageHelper.imageGrids[gridId].tiles.Length, "*");

                                }
                            }
                        });

                        it.Unlock();
                        i++;
                        (sender as BackgroundWorker).ReportProgress((i) * 100 / ImageHelper.imageGrids[gridId].tiles.Length, "*");
                    }

                }
            }


            return features;
        }

        internal static void PartitionFeatures(Dictionary<int, List<double[]>> features, out double[,] train_features, out List<double[]> test_features, double proportion)
        {
            int count = 0;
            foreach (List<double[]> l in features.Values)
            {
                count += l.Count;
            }

            int train_count = (int)(count * proportion);

            train_features = new double[train_count, FeaturePoint.NFEATURES + 1];
            test_features = new List<double[]>();

            int train_i = 0;

            Random rnd = new Random();

            foreach (int k in features.Keys)
            {
                double[][] l0 = new double[features[k].Count][];
                features[k].CopyTo(l0);
                List<double[]> l = l0.ToList();

                int train_count_ = (int)(l.Count * proportion);

                for (int i = 0; i < train_count_ && train_i < train_count; i++)
                {
                    int index = rnd.Next(0, l.Count - 1);
                    int a = 0;
                    foreach (double d in l[index])
                    {
                        train_features[train_i, a] = d;
                        a++;
                    }
                    train_features[train_i, a] = k;
                    train_i++;
                    l.RemoveAt(index);
                }

                foreach (double[] t in l)
                {
                    double[] f = new double[FeaturePoint.NFEATURES + 1];
                    int a = 0;
                    foreach (double d in t)
                    {
                        f[a] = d;
                        a++;
                    }
                    f[a] = k;

                    test_features.Add(f);
                }
            }
        }
    }
}


