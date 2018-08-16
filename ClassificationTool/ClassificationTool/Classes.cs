using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Wpf.Toolkit;
using static ClassificationTool.ImageHelper;

namespace ClassificationTool
{
    public class Classes
    {
        public string className;
        public int featuresCount = 0;
        public string classId;
        public int classNumber;
        public ImageHelper.PixelColor classColor;
        public Dictionary<string, List<FeaturePoint>> featurePoints = new Dictionary<string, List<FeaturePoint>>();
        public Dictionary<string, ConcurrentBag<FeaturePoint>> classifiedPointsConc = new Dictionary<string, ConcurrentBag<FeaturePoint>>();
        public Dictionary<string, FeaturePoint[,]> classifiedPointsList = new Dictionary<string, FeaturePoint[,]>();

        TreeViewItem tree;

        public int radius = 1;
        public int threshold = 3;

        private ColorPicker colorPicker;

        public bool hasDeleteButton = false;

        public Classes(MainWindow window, string _name, int _id)
        {
            className = _name;
            classId = "class" + _id;

            StackPanel sp = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            TextBox tb = new TextBox
            {
                Text = _name,
                Background = new SolidColorBrush(Colors.Transparent),
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                Foreground = new SolidColorBrush(Colors.White),
                Width = 100
            };

            tb.TextChanged += NameChanged;

            sp.Children.Add(tb);

            tree = new TreeViewItem
            {
                Margin = new Thickness(0, 0, 0, 5)
            };

            tree.Header = sp;
            tree.Name = "tree" + classId;

            Random rnd = new Random();

            classColor.R = (byte)rnd.Next(255);
            classColor.G = (byte)rnd.Next(255);
            classColor.B = (byte)rnd.Next(255);
            classColor.A = 255;

            colorPicker = new ColorPicker
            {
                DisplayColorAndName = false,
                ShowDropDownButton = false,
                Width = 25,
                Name = "colorPicker" + classId.Substring(5),
                SelectedColor = Color.FromArgb(255, classColor.R, classColor.G, classColor.B),
            };

            colorPicker.SelectedColorChanged += window.Color_SelectedColorChanged;

            (tree.Header as StackPanel).Children.Add(colorPicker);

            Button addPoints = new Button
            {
                Content = "Add points",
                Name = "point" + classId.Substring(5),
                Background = window.SteelBlue,
                Foreground = window.White,
                Width = 70,
                Margin = new Thickness(5, 0, 0, 0)
            };

            addPoints.Click += window.AddFeaturesPointEnable;

            (tree.Header as StackPanel).Children.Add(addPoints);

            Button deleteClass = new Button
            {
                Content = "x",
                Name = "delete" + classId.Substring(5),
                Background = window.SteelRed,
                Foreground = window.White,
                Margin = new Thickness(5, 0, 0, 0),
                Width = 20
            };

            deleteClass.Click += window.DeleteClass;

            (tree.Header as StackPanel).Children.Add(deleteClass);

        }

        ~Classes()
        {
            foreach (List<FeaturePoint> fpList in featurePoints.Values)
            {
                fpList.Clear();
            }
            featurePoints.Clear();

        }

        internal void SetColorPicker()
        {
            colorPicker.SelectedColor = Color.FromArgb(255, classColor.R, classColor.G, classColor.B);
        }

        private void NameChanged(object sender, TextChangedEventArgs e)
        {
            className = (e.Source as TextBox).Text;
        }

        public void AddFeaturePoint(int x, int y, string imageId)
        {
            ImageHelper.DrawOverlayTrain(imageId, x, y, 1, 1, classColor);

            if (!featurePoints.ContainsKey(imageId)) featurePoints.Add(imageId, new List<FeaturePoint>());
            if (featurePoints[imageId].Exists(p => p.y == y && p.x == x))
            {
                return;
            }

            featurePoints[imageId].Add(FeaturePoint.GetOrAddFeaturePoint(x, y, imageId));
            featuresCount++;
        }

        public void AddFeaturePointRange(int x, int y, int width, int height, string imageId)
        {
            ImageHelper.DrawOverlayTrain(imageId, x, y, width, height, classColor);

            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    if (!featurePoints.ContainsKey(imageId)) featurePoints.Add(imageId, new List<FeaturePoint>());
                    if (featurePoints[imageId].Exists(p => p.y == j && p.x == i))
                    {
                        continue;
                    }

                    featurePoints[imageId].Add(FeaturePoint.GetOrAddFeaturePoint(i, j, imageId));
                    featuresCount++;
                }
            }
        }

        public void RemoveFeaturePointRange(int x, int y, int width, int height, string imageId)
        {
            if (featurePoints.ContainsKey(imageId))
            {
                for (int i = x; i < x + width; i++)
                {
                    for (int j = y; j < y + height; j++)
                    {
                        featuresCount -= featurePoints[imageId].RemoveAll(p => p.y == j && p.x == i);
                    }
                }
            }
        }


        public TreeViewItem GetTree(string tabId)
        {
            tree.Items.Clear();

            TreeViewItem newItem = new TreeViewItem
            {
                Foreground = new SolidColorBrush(Colors.White)
            };

            int features = 0;

            if (imageGrids.ContainsKey("grid_" + tabId.Substring(4)))
                foreach (ImageTile it in imageGrids["grid_" + tabId.Substring(4)].tiles)
                {
                    if (it!=null && featurePoints.ContainsKey(it.id)) features += featurePoints[it.id].Count;
                }


            newItem.Header = features + " / " + featuresCount + " points";

            tree.Items.Add(newItem);

            return tree;
        }

        public void InitClassifiedPoints(string gridId, MainWindow window)
        {
            foreach (ImageTile it in imageGrids[gridId].tiles)
            {
                if (classifiedPointsConc.ContainsKey(it.id)) classifiedPointsConc.Remove(it.id);
                classifiedPointsConc.Add(it.id, new ConcurrentBag<FeaturePoint>());

                if (classifiedPointsList.ContainsKey(it.id)) classifiedPointsList.Remove(it.id);
                classifiedPointsList.Add(it.id, new FeaturePoint[256,256]);
            }

            imageGrids[gridId].AddOverlayClass(this, window);
        }

        public void AddClassifiedPoints(FeaturePoint fp, string imgId)
        {
            if (!classifiedPointsConc.ContainsKey(imgId)) throw new Exception();

            classifiedPointsConc[imgId].Add(fp);
        }

        internal void ChangeColor(Color? selectedColor)
        {
            classColor.Set(selectedColor.Value);
            classColor.A = 255;

            foreach (string imgId in featurePoints.Keys)
            {
                foreach (FeaturePoint fp in featurePoints[imgId])
                {
                    ImageHelper.DrawOverlayTrain(imgId, fp.x, fp.y, classColor);
                }
            }

            foreach (string imgId in classifiedPointsList.Keys)
            {
                foreach (FeaturePoint fp in classifiedPointsList[imgId])
                {
                    ImageHelper.DrawOverlayClass(imgId, fp.x, fp.y, this, classColor);
                }
            }
        }

        internal void ErasePoints()
        {
            foreach (string imgId in featurePoints.Keys)
            {
                foreach (FeaturePoint fp in featurePoints[imgId])
                {
                    DrawOverlayTrain(imgId, fp.x, fp.y, new ImageHelper.PixelColor(0));
                }
            }

            foreach (string imgId in classifiedPointsList.Keys)
            {
                imageTiles[imgId].RemoveClass(classId);
            }
        }

        internal void RemoveImageFeature(string imgId)
        {
            if (featurePoints.ContainsKey(imgId)) featuresCount -= featurePoints[imgId].Count;
            featurePoints.Remove(imgId);
            classifiedPointsList.Remove(imgId);
        }

        internal int FeaturesCount(List<string> testingGrid)
        {
            int count = 0;
            foreach (string id in testingGrid)
            {
                foreach (ImageTile it in imageGrids[id].tiles)
                {
                    if (featurePoints.ContainsKey(it.id)) count += featurePoints[it.id].Count;
                }
            }
            return count;
        }

        internal string FeaturesPointToString()
        {
            ConcurrentBag<string> seq = new ConcurrentBag<string>();
            Parallel.ForEach(featurePoints.Keys, image_id =>
             {
                 string s = "";
                 foreach (FeaturePoint fp in featurePoints[image_id])
                 {
                     s += (image_id + SaveLoad.Space2 + fp.ToString() + SaveLoad.Space1);
                     if(s.Count() > 4096)
                     {
                         seq.Add(s);
                         s = "";
                     }

                 }
                 seq.Add(s);
             });
            
            if (seq.Count == 0) return "";
            return seq.Aggregate((a,b) => a+b);            
        }

        internal string ClassifiedPointsToString()
        {
            string s = "";
            foreach (string image_id in classifiedPointsList.Keys)
            {
                foreach (FeaturePoint fp in classifiedPointsList[image_id])
                {
                    s += image_id + SaveLoad.Space2 + fp.ToString() + SaveLoad.Space1;
                }
            }

            return s;
        }

        internal void FeaturePointsFromString(string v)
        {
            string[] splitted = v.Split(SaveLoad.Space1);
            foreach (string s in splitted)
            {
                string[] fp = s.Split(SaveLoad.Space2);
                if (fp.Length != 4) return;
                AddFeaturePoint(int.Parse(fp[2]), int.Parse(fp[3]), fp[0]);
            }
        }

        internal void ClassifiedPointsFromString(string v)
        {
            string[] splitted = v.Split(SaveLoad.Space1);
            foreach (string s in splitted)
            {
                string[] fp = s.Split(SaveLoad.Space2);
                if (fp.Length != 4) break;
                AddClassifiedPoints(FeaturePoint.GetOrAddFeaturePoint(int.Parse(fp[2]), int.Parse(fp[3]), fp[1]), fp[0]);
                ImageHelper.DrawOverlayClass(fp[0], int.Parse(fp[2]), int.Parse(fp[3]), this, classColor);
            }
        }

        internal void FinalizeClassifiedFeatures(string imgId)
        {
            if (classifiedPointsList.ContainsKey(imgId)) classifiedPointsList.Remove(imgId);

            FeaturePoint[,] fps = new FeaturePoint[256, 256];

            Parallel.ForEach(classifiedPointsConc[imgId], fp =>
            {
                fps[fp.y, fp.x] = fp;
            });

            classifiedPointsList.Add(imgId, fps);

            classifiedPointsConc.Remove(imgId);
        }
    }
}
