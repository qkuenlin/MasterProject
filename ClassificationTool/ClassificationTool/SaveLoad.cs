using Lerc2017;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static alglib;
using static ClassificationTool.ImageHelper;

namespace ClassificationTool
{
    class SaveLoad
    {

        private static char Escape = '|';
        private static readonly char Space0 = '§';
        internal static char Space1 = '¦';
        internal static char Space2 = '@';
        internal static char Space3 = '#';


        public static void SaveFile(MainWindow window)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Classification Files (*.clf)|*.clf",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                BackgroundWorker worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                };

                worker.DoWork += Saving;
                worker.ProgressChanged += window.ProgressChanged;

                object[] arg = new object[2];

                arg[0] = window;
                arg[1] = saveFileDialog;

                worker.RunWorkerAsync(arg);
            }
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        private static string UnCompress(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        private static byte[] Compress(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        private static void Saving(object sender, DoWorkEventArgs e)
        {
            MainWindow window = (e.Argument as object[])[0] as MainWindow;

            DateTime time = DateTime.Now;

            (sender as BackgroundWorker).ReportProgress(-1, "Saving File ... ");

            string images = "";

            MainWindow.Log("Writing ImageGrids");

            //Writing ImagesGrid
            foreach (string key in imageGrids.Keys)
            {
                bool test = false;
                bool train = false;
                foreach (string s in window.TestingGrids)
                {
                    if (s == key)
                    {
                        test = true;
                        break;
                    }
                }

                foreach (string s in window.TrainingGrids)
                {
                    if (s == key)
                    {
                        train = true;
                        break;
                    }
                }

                images += (key + Space0 + ImageHelper.imageGrids[key].ToString() + Space0 + test + Space0 + train) + Escape;
            }
            images += Escape.ToString();

            MainWindow.Log("Done Writing ImageGrids in " + MainWindow.TimeToString(DateTime.Now - time));
            time = DateTime.Now;

            MainWindow.Log("Writing Classes");

            string classe = "";

            //Writing Classes

            string[] classes = new string[MainWindow.classesList.Count];
            Thread[] children = new Thread[MainWindow.classesList.Count];
            int i = 0;

            foreach (Classes c in (MainWindow.classesList))
            {
                int index = i;
                children[index] = (new Thread(() => classes[index] = (c.classId + Space0 + c.classNumber + Space0 + c.className + Space0 + c.classColor.ToString() + Space0 + c.FeaturesPointToString())));
                children[index].Start();
                i++;
            }

            i = 0;
            foreach (Thread child in children)
            {
                child.Join();
                int index = i;
                classe += classes[index] + Escape;
                i++;
            }

            classe += (Escape.ToString());


            MainWindow.Log("Done Writing Classes in " + MainWindow.TimeToString(DateTime.Now - time));
            time = DateTime.Now;

            MainWindow.Log("Writing Forest");

            string forest = "";
            //Writing Decision Forest

            if (window.DecisionForest_Trained)
            {
                forest += window.DecisionForest.innerobj.bufsize + "" + Space0;
                forest += window.DecisionForest.innerobj.nclasses + "" + Space0;
                forest += window.DecisionForest.innerobj.ntrees + "" + Space0;
                forest += window.DecisionForest.innerobj.nvars + "" + Space0;

                forest += ClassificationHelper.TreeString();
            }

            forest += (Escape.ToString());
            forest += (Escape.ToString());
            MainWindow.Log("Done Writing Forest in " + MainWindow.TimeToString(DateTime.Now - time));

            string nn = "";
            //Writing Neural Network
            /*
            if (window.NeuralNetwork_Trained)
            {
                mlpserialize(window.NeuralNetwork, out string str);
                nn += str;
            }
            */
            nn += (Escape.ToString());
            nn += (Escape.ToString());

            //Writing Features Options
            nn += "" + FeaturePoint.RGBEnable + Space0 + FeaturePoint.HSLEnable + Space0 + FeaturePoint.XYZEnable + Space0 + FeaturePoint.GLCMEnable + Escape;

            nn += (Escape.ToString());

            MainWindow.Log("Compressing and Saving");
            time = DateTime.Now;


            File.WriteAllBytes(((e.Argument as object[])[1] as SaveFileDialog).FileName, Compress(images + classe + forest + nn));

            MainWindow.Log("Done Compressing and Saving in " + MainWindow.TimeToString(DateTime.Now - time));

            (sender as BackgroundWorker).ReportProgress(100, "File Saved");
        }

        internal static void LoadFile(MainWindow window)
        {
            window.ProgressBar.IsIndeterminate = true;
            window.ProgressText.Text = "Loading File ... ";

            OpenFileDialog op = new OpenFileDialog
            {
                Filter = "Classification Files (*.clf)|*.clf",
            };

            if (op.ShowDialog() == true)
            {
                window.EraseAll();

                byte[] bytes = File.ReadAllBytes(op.FileName);

                string[] content = UnCompress(bytes).Split(Escape);

                int attribCounter = 0;

                foreach (string line in content)
                {
                    if (line == "")
                    {
                        attribCounter++;
                        continue;
                    }

                    switch (attribCounter)
                    {
                        case 0: //Loading ImagesGrid
                            {
                                LoadGrid(window, line);
                                break;
                            }

                        case 1: //Loading Classes
                            {
                                string[] splitted = line.Split(Space0);
                                int class_id = int.Parse(splitted[0].Substring(5));

                                Classes c = new Classes(window, splitted[2], class_id)
                                {
                                    classColor = new ImageHelper.PixelColor(splitted[3]),
                                    classNumber = int.Parse(splitted[1])
                                };

                                c.SetColorPicker();
                                c.FeaturePointsFromString(splitted[4]);

                                MainWindow.classesList.Add(c);

                                MainWindow.classesIdCount = Math.Max(MainWindow.imagesIdCount, class_id) + 1;
                                break;
                            }

                        case 2: //Loading Decision Forest
                            {
                                string[] splitted = line.Split(Space0);
                                window.DecisionForest = new alglib.decisionforest();

                                window.DecisionForest.innerobj.bufsize = int.Parse(splitted[0]);
                                window.DecisionForest.innerobj.nclasses = int.Parse(splitted[1]);
                                window.DecisionForest.innerobj.ntrees = int.Parse(splitted[2]);
                                window.DecisionForest.innerobj.nvars = int.Parse(splitted[3]);

                                List<double> trees = new List<double>();
                                foreach (string str in splitted[4].Split(Space1))
                                {
                                    if (str != "") trees.Add(double.Parse(str));
                                }

                                window.DecisionForest.innerobj.trees = trees.ToArray();

                                window.DecisionForest_Trained = true;

                                break;
                            }

                        case 3: //Loading Neural Network
                            {
                                if (line != "")
                                {
                                    mlpunserialize(line, out window.NeuralNetwork);
                                    window.NeuralNetwork_Trained = true;
                                }
                                break;
                            }

                        case 4: //Loading Features Options
                            {
                                string[] splitted = line.Split(Space0);

                                try
                                {
                                    window.RGBCheck.IsChecked = bool.Parse(splitted[0]);
                                    window.HSLCheck.IsChecked = bool.Parse(splitted[1]);
                                    window.XYZCheck.IsChecked = bool.Parse(splitted[2]);
                                    window.GLCMCheck.IsChecked = bool.Parse(splitted[3]);
                                }
                                catch
                                {
                                    window.RGBCheck.IsChecked = true;
                                    window.HSLCheck.IsChecked = true;
                                    window.XYZCheck.IsChecked = true;
                                    window.GLCMCheck.IsChecked = true;

                                    MainWindow.Log("Warning: Error when reading features options");
                                }

                                break;
                            }


                        default: break;
                    }
                }

                window.UpdateClassTree();
            }

            window.ProgressBar.IsIndeterminate = false;
            window.ProgressBar.Value = 100;
            window.ProgressText.Text = "File Loaded ";
        }

        internal static HeightData LoadHeightMap(int level, long x, long y)
        {
            string id = "hd_" + level + "_" + x + "_" + y;
            string uri = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer/tile/" + level + "/" + y + "/" + x;
            string filename = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\tmp\\hm\\" + level + "_" + y + "_" + x + ".lerc2";

            if (!File.Exists(filename)) SaveLoad.DownloadRemoteFile(uri, filename);

            byte[] pLercBlob = File.ReadAllBytes(filename);

            String[] infoLabels = { "version", "data type", "nDim", "nCols", "nRows", "nBands", "num valid pixels", "blob size" };
            String[] dataRangeLabels = { "zMin", "zMax", "maxZErrorUsed" };

            int infoArrSize = infoLabels.Count();
            int dataRangeArrSize = dataRangeLabels.Count();

            UInt32[] infoArr = new UInt32[infoArrSize];
            double[] dataRangeArr = new double[dataRangeArrSize];

            UInt32 hr = LercDecode.lerc_getBlobInfo(pLercBlob, (UInt32)pLercBlob.Length, infoArr, dataRangeArr, infoArrSize, dataRangeArrSize);
            if (hr > 0)
            {
                MainWindow.Log("function lerc_getBlobInfo(...) failed with error code " + hr);
                //throw new Exception();
            }

            int lercVersion = (int)infoArr[0];
            int dataType = (int)infoArr[1];
            int nDim = (int)infoArr[2];
            int nCols = (int)infoArr[3];
            int nRows = (int)infoArr[4];
            int nBands = (int)infoArr[5];

            byte[] pValidBytes = new byte[nCols * nRows];
            uint nValues = (uint)(nDim * nCols * nRows * nBands);

            double[] pData = new double[nValues];
            hr = LercDecode.lerc_decodeToDouble(pLercBlob, (UInt32)pLercBlob.Length, pValidBytes, nDim, nCols, nRows, nBands, pData);

            return new HeightData(filename, pData, nCols, nRows, pValidBytes, dataRangeArr[0], dataRangeArr[1]);
        }

        internal static void ExportClassification(MainWindow window)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string gridId = "grid_" + (window.ImageTab.SelectedItem as TabItem).Name.Substring(4);

                foreach (ImageTile it in imageGrids[gridId].tiles)
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


                        string[] splitted = saveFileDialog.FileName.Split('.');
                        string filename = splitted[0] + "_" + it.id + "_" + classesNames + "." + splitted[1];

                        ImageHelper.ExportClassification(it.id, c0, c1, c2, c3, filename);
                    }
                }
            }
        }

        internal static void DownloadRemoteFile(string uri, string filename)
        {
            MainWindow.Log("Downloading image from " + uri + " to " + filename);
            using (WebClient client = new WebClient())
            {
                Uri u = new Uri(uri);
                client.DownloadFile(new Uri(uri), @filename);
            }
        }

        internal static void LoadGrid(MainWindow window, string line)
        {
            string[] splitted = line.Split(Space0);

            ImageGrid imageGrid = ImageGrid.FromString(splitted[1], window);

            int id = int.Parse(imageGrid.id.Substring(5));

            TabItem newTab = new TabItem
            {
                Background = window.Transparent,
                Foreground = window.White,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };

            newTab.Visibility = Visibility.Visible;

            StackPanel stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
            };

            StackPanel sp = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            TextBox tb = new TextBox
            {
                Text = imageGrid.name,
                Background = window.Transparent,
                Foreground = window.White,
                Margin = new Thickness(0, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 130,
            };
            sp.Children.Add(tb);

            tb.TextChanged += ((s, e) => imageGrid.name = tb.Text);

            StackPanel cp = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            CheckBox cbtraining = new CheckBox
            {
                Content = "Training",
                Foreground = window.White,
                Name = "train" + id,
            };
            cbtraining.Checked += window.Cbtraining_Checked;
            cbtraining.Unchecked += window.Cbtraining_Unchecked;

            cp.Children.Add(cbtraining);

            CheckBox cbtesting = new CheckBox
            {
                Content = "Testing",
                Foreground = window.White,
                Name = "test" + id,
            };
            cbtesting.Checked += window.Cbtesting_Checked;
            cbtesting.Unchecked += window.Cbtesting_Unchecked;

            cp.Children.Add(cbtesting);


            if (bool.Parse(splitted[2])) cbtesting.IsChecked = true;
            if (bool.Parse(splitted[3])) cbtraining.IsChecked = true;

            sp.Children.Add(cp);

            Button deleteImage = new Button
            {
                Content = "x",
                Name = "deleteIm" + id,
                Background = window.SteelRed,
                Foreground = window.White,
                Margin = new Thickness(5, 0, 0, 0),
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center
            };

            deleteImage.Click += window.DeleteImage;

            sp.Children.Add(deleteImage);

            stack.Children.Add(sp);

            StackPanel SubOptions = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Name = "sub",
                Background = window.Transparent,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(55, 5, 5, 5),
            };


            stack.Children.Add(SubOptions);

            newTab.Header = stack;

            window.ImageTab.Items.Add(newTab);

            newTab.Name = "tab_" + id;

            Grid grid = new Grid
            {
                Background = window.DarkBackground,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            grid.Children.Add(imageGrid.grid);

            Grid.SetColumn(imageGrid.grid, 0);
            Grid.SetRow(imageGrid.grid, 0);

            newTab.Content = grid;

            window.ImageTab.SelectedItem = newTab;

            CheckBox checkbox = new CheckBox
            {
                Name = "BaseLayer" + id,
                Content = "Base Layer",
                Foreground = window.White,
                IsChecked = true,
            };

            checkbox.Checked += ((s, e) =>
            {
                foreach (ImageTile it in imageGrid.tiles)
                {
                    it.baseImage.Visibility = Visibility.Visible;
                }
            });

            checkbox.Unchecked += ((s, e) =>
            {
                foreach (ImageTile it in imageGrid.tiles)
                {
                    it.baseImage.Visibility = Visibility.Hidden;
                }
            });

            SubOptions.Children.Add(checkbox);

            CheckBox checkbox2 = new CheckBox
            {
                Name = "Heightmap" + id,
                Content = "Heightmap",
                Foreground = window.White,
                IsChecked = true,
            };

            checkbox2.Checked += ((s, e) =>
            {
                foreach (ImageTile it in imageGrid.tiles)
                {
                    it.heightmap.Visibility = Visibility.Visible;
                }
            });

            checkbox2.Unchecked += ((s, e) =>
            {
                foreach (ImageTile it in imageGrid.tiles)
                {
                    it.heightmap.Visibility = Visibility.Hidden;
                }
            });

            SubOptions.Children.Add(checkbox2);

            MainWindow.imageGridIdCount = Math.Max(id + 1, MainWindow.imageGridIdCount + 1);
        }

        internal static void LoadImageRange(MainWindow window, int id, string[,] FileNames, int width, int height, int level)
        {
            ImageGrid imageGrid = null;

            MainWindow.dispatcher.Invoke(() =>
            {
                TabItem newTab = new TabItem
                {
                    Background = window.Transparent,
                    Foreground = window.White,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                };

                newTab.Visibility = Visibility.Visible;

                StackPanel stack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                };

                StackPanel sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                TextBox tb = new TextBox
                {
                    Text = "Image " + id,
                    Background = window.Transparent,
                    Foreground = window.White,
                    Margin = new Thickness(0, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 130,
                };
                sp.Children.Add(tb);

                StackPanel cp = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };

                CheckBox cbtraining = new CheckBox
                {
                    Content = "Training",
                    Foreground = window.White,
                    Name = "train" + id,
                };
                cbtraining.Checked += window.Cbtraining_Checked;
                cbtraining.Unchecked += window.Cbtraining_Unchecked;

                cp.Children.Add(cbtraining);

                CheckBox cbtesting = new CheckBox
                {
                    Content = "Testing",
                    Foreground = window.White,
                    Name = "test" + id,
                };
                cbtesting.Checked += window.Cbtesting_Checked;
                cbtesting.Unchecked += window.Cbtesting_Unchecked;

                cp.Children.Add(cbtesting);

                sp.Children.Add(cp);

                Button deleteImage = new Button
                {
                    Content = "x",
                    Name = "deleteIm" + id,
                    Background = window.SteelRed,
                    Foreground = window.White,
                    Margin = new Thickness(5, 0, 0, 0),
                    Width = 20,
                    Height = 20,
                    VerticalAlignment = VerticalAlignment.Center
                };

                deleteImage.Click += window.DeleteImage;

                sp.Children.Add(deleteImage);

                stack.Children.Add(sp);

                StackPanel SubOptions = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Name = "sub",
                    Background = window.Transparent,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(55, 5, 5, 5),
                };


                stack.Children.Add(SubOptions);

                newTab.Header = stack;
                newTab.Name = "tab_" + id;

                window.ImageTab.Items.Add(newTab);

                string gridId = "grid_" + id;
                imageGrid = new ImageGrid(window, gridId, tb.Text, width, height);

                tb.TextChanged += ((s, e) => imageGrid.name = tb.Text);

                Grid grid = new Grid
                {
                    Background = window.DarkBackground,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };

                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());

                grid.Children.Add(imageGrid.grid);

                Grid.SetColumn(imageGrid.grid, 0);
                Grid.SetRow(imageGrid.grid, 0);

                newTab.Content = grid;

                window.ImageTab.SelectedItem = newTab;

                CheckBox checkbox = new CheckBox
                {
                    Name = "BaseLayer" + id,
                    Content = "Base Layer",
                    Foreground = window.White,
                    IsChecked = true,
                };

                checkbox.Checked += ((s, e) =>
                {
                    foreach (ImageTile it in imageGrid.tiles)
                    {
                        it.baseImage.Visibility = Visibility.Visible;
                    }
                });

                checkbox.Unchecked += ((s, e) =>
                {
                    foreach (ImageTile it in imageGrid.tiles)
                    {
                        it.baseImage.Visibility = Visibility.Hidden;
                    }
                });

                SubOptions.Children.Add(checkbox);

                CheckBox checkbox2 = new CheckBox
                {
                    Name = "Heightmap" + id,
                    Content = "Heightmap",
                    Foreground = window.White,
                    IsChecked = true,
                };

                checkbox2.Checked += ((s, e) =>
                {
                    foreach (ImageTile it in imageGrid.tiles)
                    {
                        if (it != null) it.heightmap.Visibility = Visibility.Visible;
                    }
                });

                checkbox2.Unchecked += ((s, e) =>
                {
                    foreach (ImageTile it in imageGrid.tiles)
                    {
                        if(it != null) it.heightmap.Visibility = Visibility.Hidden;
                    }
                });

                SubOptions.Children.Add(checkbox2);
            });

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string imgId = "img_" + id + "_" + x + "_" + y;
                    imageGrid.AddTile(new ImageTile(window, imageGrid, FileNames[x, y], imgId, level, x, y));
                }
            }

        }
    }
}
