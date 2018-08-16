using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Statistics.Kernels;
using Lerc2017;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using Xceed.Wpf.Toolkit;
using static alglib;
using static ClassificationTool.ClassificationHelper;
using static ClassificationTool.ImageHelper;

namespace ClassificationTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public SolidColorBrush SteelBlue = new SolidColorBrush(Colors.SteelBlue);
        public SolidColorBrush SteelRed = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0x41, 0x41));
        public SolidColorBrush SteelGreen = new SolidColorBrush(Color.FromArgb(0xFF, 0x64, 0xB4, 0x46));
        public SolidColorBrush SteelYellow = new SolidColorBrush(Color.FromArgb(0xFF, 0xB4, 0xAF, 0x46)); //#FFB4AF46
        public SolidColorBrush SteelOrange = new SolidColorBrush(Color.FromArgb(0xFF, 0xB4, 0x73, 0x46)); //##FFB47346

        public SolidColorBrush DarkBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0x34, 0x34, 0x34));

        public SolidColorBrush White = new SolidColorBrush(Colors.White);
        public SolidColorBrush Transparent = new SolidColorBrush(Colors.Transparent);

        public SolidColorBrush LockedGray = new SolidColorBrush(Colors.DarkGray);

        public static Dispatcher dispatcher;

        private bool Thresholding = false;
        private BackgroundWorker thresholdWorker;

        public List<Image> images = new List<Image>();
        public static List<Classes> classesList = new List<Classes>();

        public List<string> TestingGrids = new List<string>();
        public List<string> TrainingGrids = new List<string>();

        public static int classesIdCount = 0;
        public static int imagesIdCount = 0;
        public static int testResultCount = 0;
        public static int imageGridIdCount = 0;

        public string CurrentDeleteButtonId;

        public bool addingPointEnabled = false;
        public string addingPointClassId = "";
        public Button activatedAddButton = new Button();

        int click_x;
        int click_y;
        bool clickLeft = false;
        bool clickRight = false;
        bool Classifying = false;
        bool Testing = false;
        bool CancelClassification = false;
        bool CancelTesting = false;
        string ClassifyingTabId = "";

        bool trainingTesting = false;

        public GeneralConfusionMatrix TestResults;
        public bool tested = false;

        public bool Training = false;
        BackgroundWorker TrainingWorker;
        BackgroundWorker TrainingTestingWorker;

        internal decisionforest DecisionForest;
        internal int DecisionForest_NTree = 10;
        internal double DecisionForest_R = 0.5;

        internal int TrainTestIterations = 4;

        private bool dirty = true;
        private bool df_trained = false;
        internal bool DecisionForest_Trained
        {
            get { return df_trained; }
            set
            {
                if (value)
                {
                    ResetButton(TrainButton);
                    ResetButton(ClassifyButton);
                }
                else
                {
                    LockButton(TrainButton);
                    LockButton(ClassifyButton);
                }
                df_trained = value;
            }
        }

        internal multilayerperceptron NeuralNetwork;
        internal int NeuralNetwork_Layer1 = 5;
        internal int NeuralNetwork_Layer2 = 5;
        internal double NeuralNetwork_WStep = 0.000;
        internal int NeuralNetwork_MaxIts = 100;
        internal double NeuralNetwork_Decay = 0.001;

        private bool nn_trained = false;
        internal bool NeuralNetwork_Trained
        {
            get { return nn_trained; }
            set
            {
                if (value)
                {
                    ResetButton(TrainButton);
                    ResetButton(ClassifyButton);
                }
                else
                {
                    LockButton(TrainButton);
                    LockButton(ClassifyButton);
                }
                nn_trained = value;
            }
        }

        internal mlpensemble NeuralNetworkEnsemble;
        internal int NeuralNetworkEnsembleSize = 50;
        internal int NeuralNetworkEnsemble_Layer1 = 5;
        internal int NeuralNetworkEnsemble_Layer2 = 5;
        internal double NeuralNetworkEnsemble_WStep = 0.000;
        internal int NeuralNetworkEnsemble_MaxIts = 100;
        internal double NeuralNetworkEnsemble_Decay = 0.001;

        internal int SVMComplexity = 100;
        internal double SVMEpsilon = 1e-5;
        internal double SVMTolerance = 1e-5;
        internal double SVMGausG = 7;
        internal int SVMPolyP = 2;



        internal int SVM_kernel = 0;

        internal string MAIN_DIRECTORY;
        internal static string logPath;

        private bool nne_trained = false;
        internal bool NeuralNetworkEnsemble_Trained
        {
            get { return nne_trained; }
            set
            {
                if (value)
                {
                    ResetButton(TrainButton);
                    ResetButton(ClassifyButton);
                }
                else
                {
                    LockButton(TrainButton);
                    LockButton(ClassifyButton);
                }
                nne_trained = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            classesList.Add(new Classes(this, "default", ++classesIdCount));

            MAIN_DIRECTORY = (System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            System.IO.Directory.CreateDirectory(@"tmp/sat");
            System.IO.Directory.CreateDirectory(@"tmp/hm");

            System.IO.Directory.CreateDirectory(@"logs");

            logPath = @"logs//" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + ".txt";

            dispatcher = Dispatcher.CurrentDispatcher;
            Instance = this;
        }

        internal void EraseAll()
        {
            ImageTab.SelectedItem = AddImage;
            foreach (string image_id in ImageHelper.imageTiles.Keys)
            {
                foreach (Classes c in classesList)
                {
                    c.RemoveImageFeature(image_id);
                }

                for (int i = ImageTab.Items.Count - 1; i >= 0; i--)
                {
                    TabItem tb = ImageTab.Items[i] as TabItem;
                    if (tb.Name != "AddImage")
                    {
                        images.Remove((tb.Content as Grid).Children[0] as Image);
                        ImageTab.Items.Remove(tb);
                    }
                }
            }

            ImageHelper.DeleteAll();

            ClassesTree.Items.Clear();

            foreach (Classes c in classesList)
            {
                c.ErasePoints();
            }

            classesList.Clear();

            TestingGrids.Clear();
            TrainingGrids.Clear();

            if (DecisionForest != null) DecisionForest._deallocate();

            GC.Collect();
        }

        internal void UpdateClassTree()
        {
            if (ClassesTree != null)
            {
                ClassesTree.Items.Clear();
                foreach (Classes c in classesList)
                {
                    if ((ImageTab.SelectedItem is TabItem))
                    {
                        TreeViewItem ct = c.GetTree((ImageTab.SelectedItem as TabItem).Name);

                        ClassesTree.Items.Add(ct);
                    }
                }

                Button addClassButton = new Button
                {
                    Content = "Add New Class"
                };

                addClassButton.Click += AddClassButton_Click;

                ClassesTree.Items.Add(addClassButton);
            }
        }

        public void Color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            foreach (Classes c in classesList)
            {
                if (c.classId == "class" + (e.Source as ColorPicker).Name.Substring(11))
                {
                    c.ChangeColor((e.Source as ColorPicker).SelectedColor);
                }
            }
        }

        public void AddFeaturesPointEnable(object sender, RoutedEventArgs e)
        {
            string tmp = "class" + (e.Source as Button).Name.Substring(5);
            if (tmp == addingPointClassId)
            {
                (e.Source as Button).Content = "Add Points";
                addingPointEnabled = false;
                addingPointClassId = "";
                activatedAddButton = new Button();
            }
            else
            {
                activatedAddButton.Content = "Add Points";
                (e.Source as Button).Content = "Done";
                addingPointClassId = tmp;
                addingPointEnabled = true;

                activatedAddButton = e.Source as Button;
            }
        }

        public void Cbtesting_Unchecked(object sender, RoutedEventArgs e)
        {
            string id = "grid_" + (sender as CheckBox).Name.Substring(4);
            TestingGrids.Remove(id);
        }

        public void Cbtraining_Unchecked(object sender, RoutedEventArgs e)
        {
            string id = "grid_" + (sender as CheckBox).Name.Substring(5);
            TrainingGrids.Remove(id);
        }

        public void Cbtesting_Checked(object sender, RoutedEventArgs e)
        {
            string id = "grid_" + (sender as CheckBox).Name.Substring(4);
            TestingGrids.Add(id);
        }

        public void Cbtraining_Checked(object sender, RoutedEventArgs e)
        {
            string id = "grid_" + (sender as CheckBox).Name.Substring(5);
            TrainingGrids.Add(id);
        }

        public void Img_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (clickRight)
            {

                int PosX = (int)(e.GetPosition((Image)sender).X * (((Image)sender).Source as WriteableBitmap).PixelWidth / (sender as Image).RenderSize.Width);
                int PosY = (int)(e.GetPosition((Image)sender).Y * (((Image)sender).Source as WriteableBitmap).PixelHeight / (sender as Image).RenderSize.Height);

                int x = PosX > click_x ? click_x : PosX;
                int y = PosY > click_y ? click_y : PosY;

                foreach (Classes c in classesList)
                {
                    c.RemoveFeaturePointRange(x, y, Math.Abs(PosX - click_x) + 1, Math.Abs(PosY - click_y) + 1, (sender as Image).Name);
                }

                UpdateClassTree();

                ImageHelper.DrawOverlayTrain((sender as Image).Name, x, y, Math.Abs(PosX - click_x) + 1, Math.Abs(PosY - click_y) + 1, new ImageHelper.PixelColor(0));
            }
        }

        public void Img_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            click_x = (int)(e.GetPosition((Image)sender).X * (((Image)sender).Source as WriteableBitmap).PixelWidth / (sender as Image).RenderSize.Width);
            click_y = (int)(e.GetPosition((Image)sender).Y * (((Image)sender).Source as WriteableBitmap).PixelHeight / (sender as Image).RenderSize.Height);

            clickRight = true;
        }

        public void Img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (clickLeft && addingPointEnabled)
            {
                foreach (Classes c in classesList)
                {
                    int PosX = (int)(e.GetPosition((Image)sender).X * (((Image)sender).Source as WriteableBitmap).PixelWidth / (sender as Image).RenderSize.Width);
                    int PosY = (int)(e.GetPosition((Image)sender).Y * (((Image)sender).Source as WriteableBitmap).PixelHeight / (sender as Image).RenderSize.Height);

                    int x = PosX > click_x ? click_x : PosX;
                    int y = PosY > click_y ? click_y : PosY;

                    if (c.classId == addingPointClassId)
                    {
                        c.AddFeaturePointRange(x, y, Math.Abs(PosX - click_x) + 1, Math.Abs(PosY - click_y) + 1, (sender as Image).Name);
                    }
                    else
                    {
                        c.RemoveFeaturePointRange(x, y, Math.Abs(PosX - click_x) + 1, Math.Abs(PosY - click_y) + 1, (sender as Image).Name);
                    }
                }

                UpdateClassTree();
            }
        }

        public void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (addingPointEnabled)
            {
                foreach (Classes c in classesList)
                {
                    if (c.classId == addingPointClassId)
                    {
                        click_x = (int)(e.GetPosition((Image)sender).X * (((Image)sender).Source as WriteableBitmap).PixelWidth / (sender as Image).RenderSize.Width);
                        click_y = (int)(e.GetPosition((Image)sender).Y * (((Image)sender).Source as WriteableBitmap).PixelHeight / (sender as Image).RenderSize.Height);

                        clickLeft = true;
                    }
                }
            }
        }

        private void AddClassButton_Click(object sender, RoutedEventArgs e)
        {
            classesList.Add(new Classes(this, "new class", ++classesIdCount));
            UpdateClassTree();
        }

        internal void DeleteClass(object sender, RoutedEventArgs e)
        {
            DeleteClassPopup.IsOpen = true;

            CurrentDeleteButtonId = (e.Source as Button).Name;
        }

        private void CarryOnDeleteClass(object sender, RoutedEventArgs e)
        {
            DeleteClassPopup.IsOpen = false;

            foreach (TreeViewItem tv in ClassesTree.Items)
            {
                if (tv.Name == "treeclass" + CurrentDeleteButtonId.Substring(6))
                {
                    ClassesTree.Items.Remove(tv);
                    break;
                }
            }

            foreach (Classes c in classesList)
            {
                if (c.classId == "class" + (CurrentDeleteButtonId.Substring(6)))
                {
                    c.ErasePoints();
                    classesList.Remove(c);
                    break;
                }
            }

            GC.Collect();
        }

        public void DeleteImage(object sender, RoutedEventArgs e)
        {
            DeleteImagePopup.IsOpen = true;

            CurrentDeleteButtonId = (e.Source as Button).Name;
        }

        private void CarryOnDeleteImage(object sender, RoutedEventArgs e)
        {
            Log("Deleting image");
            DeleteImagePopup.IsOpen = false;

            ImageHelper.DeleteImage(this, "grid_" + CurrentDeleteButtonId.Substring(8));

            foreach (TabItem tb in ImageTab.Items)
            {
                if (tb.Name == "tab_" + CurrentDeleteButtonId.Substring(8))
                {
                    images.Remove((tb.Content as Grid).Children[0] as Image);

                    if (ImageTab.SelectedItem as TabItem == tb) ImageTab.SelectedItem = ImageTab.Items[ImageTab.Items.Count - 1];
                    ImageTab.Items.Remove(tb);

                    break;
                }
            }

            GC.Collect();

            UpdateClassTree();

            Log("Done Deleting image");

        }

        private void CancelDelete(object sender, RoutedEventArgs e)
        {
            DeleteClassPopup.IsOpen = false;
            DeleteImagePopup.IsOpen = false;
        }

        private void ImageTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClassTree();
            foreach (TabItem tab in ImageTab.Items)
            {
                if (tab.Name != "AddImage" && tab.Name.Substring(0, 5) != "Batch")
                {
                    StackPanel extended = ((tab.Header as StackPanel).Children[1] as StackPanel);
                    if (extended != null)
                    {
                        if (tab.IsSelected)
                        {
                            extended.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            extended.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }

            dirty = true;
        }

        private void TrainButtonClick(object sender, RoutedEventArgs e)
        {
            if (Training)
            {
                TrainingWorker.CancelAsync();

                ProgressText.Text = "Training Canceling ...";
                ProgressBar.IsIndeterminate = true;
            }
            else
            {
                TrainingWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                TrainingWorker.DoWork += Train_Classification;
                TrainingWorker.ProgressChanged += ProgressChanged;

                NegateButton(sender as Button);
                LockButton(ClassifyButton);
                LockButton(TestingButton);

                Training = true;

                TrainingWorker.RunWorkerCompleted += TrainingWorker_RunWorkerCompleted;
                TrainingWorker.WorkerSupportsCancellation = true;

                int arg = ClassifierSelection.SelectedIndex;

                DecisionForest_NTree = int.Parse(DF_Trees.Text);
                DecisionForest_R = double.Parse(DF_R.Text);

                NeuralNetwork_Decay = double.Parse(NN_Decay.Text);
                NeuralNetwork_WStep = double.Parse(NN_Step.Text);
                NeuralNetwork_MaxIts = int.Parse(NN_MaxIts.Text);
                NeuralNetwork_Layer1 = int.Parse(NN_L1.Text);
                NeuralNetwork_Layer2 = int.Parse(NN_L2.Text);

                NeuralNetworkEnsemble_Decay = double.Parse(NNE_Decay.Text);
                NeuralNetworkEnsemble_WStep = double.Parse(NNE_Step.Text);
                NeuralNetworkEnsemble_MaxIts = int.Parse(NNE_MaxIts.Text);
                NeuralNetworkEnsemble_Layer1 = int.Parse(NNE_L1.Text);
                NeuralNetworkEnsemble_Layer2 = int.Parse(NNE_L2.Text);
                NeuralNetworkEnsembleSize = int.Parse(NNE_Size.Text);

                TrainingWorker.RunWorkerAsync(arg);
            }

        }

        private void TrainingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (((int[])e.Result)[0] == 1)
            {
                switch (((int[])e.Result)[1])
                {
                    case 0:
                        {
                            DecisionForest_Trained = true;
                            ClassificationHelper.treeWritten = false;
                            ClassificationHelper.TreeToStringThread = new Thread(() => ClassificationHelper.TreeToString(this))
                            {
                                Priority = ThreadPriority.BelowNormal
                            };
                            ClassificationHelper.TreeToStringThread.Start();
                            break;
                        }
                    case 1: NeuralNetwork_Trained = true; break;
                    case 2: NeuralNetworkEnsemble_Trained = true; break;
                    default: break;
                }
            }

            Training = false;

            ResetButton(TrainButton);
            ResetButton(ClassifyButton);
            ResetButton(TestingButton);

        }

        private void Train_Classification(object sender, DoWorkEventArgs e)
        {
            dirty = true;

            try
            {
                for (int n = 0; n < classesList.Count; n++)
                {
                    classesList[n].classNumber = n;
                }

                int classifer = (int)e.Argument;

                DateTime time = System.DateTime.Now;

                (sender as BackgroundWorker).ReportProgress(-1, "Starting Training ...");

                double[][] f;
                int featuresCount = 0;
                int i = 0;

                try
                {
                    f = ClassificationHelper.ConvertFeaturesToArray(sender, TrainingGrids);
                    if (f.Length == 0)
                    {
                        (sender as BackgroundWorker).ReportProgress((i + 1) * 100, "Training Canceled after " + TimeToString(System.DateTime.Now - time));
                        e.Result = new int[] { -1, classifer };

                        return;
                    }
                    featuresCount = f.Length;
                }
                catch (Exception ex)
                {
                    (sender as BackgroundWorker).ReportProgress((i + 1) * 100, "Error: " + ex.Message);
                    e.Result = new int[] { -1, classifer };

                    return;
                }

                if (featuresCount == 0)
                {
                    (sender as BackgroundWorker).ReportProgress(0, "Error: There are not Features available for training");
                    e.Result = new int[] { -1, classifer };
                    return;
                }

                double[,] features = new double[featuresCount, FeaturePoint.NFEATURES + 1];

                i = 0;
                (sender as BackgroundWorker).ReportProgress(0, "Building Features Array ... ");


                for (int y = 0; y < f.Length; y++)
                {
                    (sender as BackgroundWorker).ReportProgress((i + 1) * 100 / featuresCount, "*");

                    for (int x = 0; x <= FeaturePoint.NFEATURES; x++)
                    {
                        if ((sender as BackgroundWorker).CancellationPending)
                        {
                            (sender as BackgroundWorker).ReportProgress((i + 1) * 100, "Training Canceled after " + TimeToString(System.DateTime.Now - time));
                            e.Result = new int[] { -1, classifer };

                            return;
                        }

                        features[i, x] = f[y][x];
                    }
                    i++;
                }

            (sender as BackgroundWorker).ReportProgress(-1, "Training ... ");
                int outInfo = -1;
                switch (classifer)
                {
                    case 0:
                        Log("Training Decison Forest");
                        if (ClassificationHelper.TreeToStringThread != null) ClassificationHelper.TreeToStringThread.Abort();

                        alglib.dfbuildrandomdecisionforest(features, featuresCount, FeaturePoint.NFEATURES, classesList.Count, DecisionForest_NTree, DecisionForest_R, out outInfo, out DecisionForest, out dfreport report);


                        // Log("Training average error " + report.avgerror.ToString());
                        e.Result = new int[] { 1, 0 };
                        break;


                    case 1:
                        Log("Training Neural Network");

                        mlptrainer trn;
                        mlpcreatetrainercls(FeaturePoint.NFEATURES, classesList.Count, out trn);
                        mlpsetdataset(trn, features, featuresCount);
                        mlpsetdecay(trn, NeuralNetwork_Decay);
                        mlpsetcond(trn, NeuralNetwork_WStep, NeuralNetwork_MaxIts);

                        if (NeuralNetwork_Layer1 <= 0 && NeuralNetwork_Layer2 <= 0) mlpcreatec0(FeaturePoint.NFEATURES, classesList.Count, out NeuralNetwork);
                        else if (NeuralNetwork_Layer1 <= 0 || NeuralNetwork_Layer2 <= 0) mlpcreatec1(FeaturePoint.NFEATURES, Math.Max(NeuralNetwork_Layer1, NeuralNetwork_Layer2), classesList.Count, out NeuralNetwork);
                        else if (NeuralNetwork_Layer1 > 0 && NeuralNetwork_Layer2 > 0) mlpcreatec2(FeaturePoint.NFEATURES, NeuralNetwork_Layer1, NeuralNetwork_Layer2, classesList.Count, out NeuralNetwork);

                        smp_mlptrainnetwork(trn, NeuralNetwork, 5, out mlpreport reportNN);
                        Log(reportNN.avgerror.ToString());

                        e.Result = new int[] { 1, 1 };
                        break;


                    case 2:
                        mlptrainer trn2;
                        mlpcreatetrainercls(FeaturePoint.NFEATURES, classesList.Count, out trn2);
                        mlpsetdataset(trn2, features, featuresCount);
                        mlpsetdecay(trn2, NeuralNetwork_Decay);
                        mlpsetcond(trn2, NeuralNetwork_WStep, NeuralNetwork_MaxIts);

                        if (NeuralNetwork_Layer1 <= 0 && NeuralNetwork_Layer2 <= 0) mlpecreatec0(FeaturePoint.NFEATURES, classesList.Count, NeuralNetworkEnsembleSize, out NeuralNetworkEnsemble);
                        if (NeuralNetwork_Layer1 <= 0 || NeuralNetwork_Layer2 <= 0) mlpecreatec1(FeaturePoint.NFEATURES, Math.Max(NeuralNetwork_Layer1, NeuralNetwork_Layer2), classesList.Count, NeuralNetworkEnsembleSize, out NeuralNetworkEnsemble);
                        else if (NeuralNetwork_Layer1 > 0 && NeuralNetwork_Layer2 > 0) mlpecreatec2(FeaturePoint.NFEATURES, NeuralNetwork_Layer1, NeuralNetwork_Layer2, classesList.Count, NeuralNetworkEnsembleSize, out NeuralNetworkEnsemble);


                        mlptrainensemblees(trn2, NeuralNetworkEnsemble, 5, out mlpreport report2);

                        e.Result = new int[] { 1, 2 };
                        break;


                    default: throw new Exception();

                }

                (sender as BackgroundWorker).ReportProgress(100, "Training Completed in " + TimeToString(System.DateTime.Now - time));
            }
            catch (Exception exc)
            {
                (sender as BackgroundWorker).ReportProgress(0, "Training Error: Something Unexpected Happened! " + exc.Message);
                Log("Exception thrown : " + exc.Message);
                e.Result = new int[] { -1, -1 };

            }
        }

        internal void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1) ProgressBar.IsIndeterminate = true;
            else
            {
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = e.ProgressPercentage;
            }

            if ((string)e.UserState != "*")
            {
                ProgressText.Text = (string)e.UserState;
                Log((string)e.UserState);
            }
        }

        private void ClassifyButtonClick(object sender, RoutedEventArgs e)
        {
            if (Classifying)
            {
                CancelClassification = true;
                ProgressText.Text = "Classification Canceling ...";
                ProgressBar.IsIndeterminate = true;
            }
            else
            {
                Classifying = true;
                NegateButton(e.Source as Button);
                LockButton(TrainButton);
                LockButton(TestingButton);

                BackgroundWorker worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                worker.DoWork += Classify;
                worker.ProgressChanged += ProgressChanged;

                int[] arg = new int[2];

                try
                {
                    ClassifyingTabId = (ImageTab.SelectedItem as TabItem).Name;
                    arg[0] = int.Parse((ImageTab.SelectedItem as TabItem).Name.Substring(4));
                    arg[1] = ClassifierSelection.SelectedIndex;
                }
                catch
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Value = 0;


                    ProgressText.Text = "Error During Classification Preprocessing: No Image Available";
                    Classifying = false;

                    ResetButton(sender as Button);
                    ResetButton(TrainButton);

                    return;
                }
                worker.RunWorkerCompleted += ClassificationWorker_RunWorkerCompleted;



                if (dirty)
                {
                    foreach (Classes c in classesList)
                    {
                        c.InitClassifiedPoints("grid_" + (ImageTab.SelectedItem as TabItem).Name.Substring(4), this);
                    }
                }
                dirty = false;

#pragma warning disable CS0618 // Type or member is obsolete
                if (ClassificationHelper.TreeToStringThread != null && ClassificationHelper.TreeToStringThread.IsAlive) TreeToStringThread.Suspend();
#pragma warning restore CS0618 // Type or member is obsolete

                worker.RunWorkerAsync(arg);
            }
        }

        private void NegateButton(Button button)
        {
            button.Background = SteelRed;
            button.Content = "Cancel";
        }

        private void ResetButton(Button button)
        {
            button.Background = SteelBlue;

            if (button.Name == "ClassifyButton") button.Content = "Classify";
            else if (button.Name == "TrainButton") button.Content = "Train";
            else if (button.Name == "TestingButton") button.Content = "Test";
            else if (button.Name == "TrainTestButton") button.Content = "Cross Validation";
            else if (button.Name == "Lonely") button.Content = "Remove Lonely Pixels";
            else if (button.Name == "Blur") button.Content = "Blur";
            else if (button.Name == "ExportButton") button.Content = "Export Classification";

            button.IsEnabled = true;
        }

        private void LockButton(Button button)
        {
            button.Background = LockedGray;
            button.IsEnabled = false;
        }

        private void ClassificationWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Classifying = false;
            CancelClassification = false;
            DrawClassificationMap();
            ResetButton(TrainButton);
            ResetButton(ClassifyButton);
            ResetButton(TestingButton);

#pragma warning disable CS0618 // Type or member is obsolete
            if (ClassificationHelper.TreeToStringThread != null && ClassificationHelper.TreeToStringThread.IsAlive) TreeToStringThread.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        internal void DrawClassificationMap()
        {
            try
            {
                string gridId = "grid_" + ClassifyingTabId.Substring(4);
                foreach (ImageTile it in imageGrids[gridId].tiles)
                {
                    if (it.classified)
                    {
                        foreach (Classes c in classesList)
                        {
                            foreach (FeaturePoint fp in c.classifiedPointsList[it.id])
                            {
                                if (fp != null) ImageHelper.DrawOverlayClass(it.id, fp.x, fp.y, c, c.classColor);
                            }
                        }
                    }

                }
            }
            catch
            {
                ProgressText.Text = "Error No Data Avalaible";
                ResetButton(ClassifyButton);
            }
        }

        private void Classify(object sender, DoWorkEventArgs e)
        {
            int[] arg = e.Argument as int[];

            bool trained = false;
            switch (arg[1])
            {
                case 0: trained = DecisionForest_Trained; break;
                case 1: trained = NeuralNetwork_Trained; break;
                case 2: trained = NeuralNetworkEnsemble_Trained; break;
                default: break;
            }
            if (trained)
            {
                DateTime time = System.DateTime.Now;

                (sender as BackgroundWorker).ReportProgress(0, "Classifying ... ");
                string gridId = "grid_" + arg[0];


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
                int imgCount = 0;
                foreach (ImageTile it in imageGrids[gridId].tiles)
                {
                    int i = 0;
                    if (!it.classified)
                    {
                        for (int k = 0; k < threadCount.Count && i < 256; k++)
                        {
                            for (; i < 256 - (threadCount[k] - 1) * blockSize[k]; i += threadCount[k] * blockSize[k])
                            {
                                Thread[] childrenThread = new Thread[threadCount[k]];
                                for (int n = 0; n < threadCount[k]; n++)
                                {
                                    int indice = i + n * blockSize[k];
                                    childrenThread[n] = new Thread(() => ClassificationSubThread(indice, blockSize[k], it.id, 256, arg[1]))
                                    {
                                        Name = it.id + "_" + indice + "+" + blockSize[k]
                                    };
                                    childrenThread[n].Start();
                                }

                                for (int n = 0; n < threadCount[k]; n++)
                                {
                                    childrenThread[n].Join();
                                }

                                int percentage = (int)((100 * (i + threadCount[k] * blockSize[k])) / (256));

                                if (CancelClassification)
                                {
                                    (sender as BackgroundWorker).ReportProgress(percentage, "Classification Canceled after " + TimeToString(System.DateTime.Now - time));
                                    return;
                                }

                                double estimatedTime = percentage == 0 ? 10000 : ((System.DateTime.Now - time).TotalSeconds * (100 * imageGrids[gridId].tiles.Length - 100 * imgCount - percentage)) / (100 * imgCount + percentage);

                                (sender as BackgroundWorker).ReportProgress(percentage, "Classifying ... " + (imgCount + 1) + "/" + imageGrids[gridId].tiles.Length + " (Estimated time remaining: " + TimeToString(TimeSpan.FromSeconds(estimatedTime)) + ")");
                            }
                        }

                        foreach (Classes c in classesList)
                        {
                            c.FinalizeClassifiedFeatures(it.id);
                        }
                    }

                    imgCount++;
                    it.classified = true;
                }


                (sender as BackgroundWorker).ReportProgress(100, "Classification Completed in " + TimeToString(System.DateTime.Now - time));
            }
            else
            {
                (sender as BackgroundWorker).ReportProgress(0, "Classification failed: No Trained Classifier Available");
            }
        }

        public void ClassificationSubThread(int i, int block_size, string imgId, int height, int classifier)
        {
            decisionforest DecisionForest_ = DecisionForest_Trained ? DecisionForest.make_copy() as decisionforest : null;
            multilayerperceptron NeuralNetwork_ = NeuralNetwork_Trained ? NeuralNetwork.make_copy() as multilayerperceptron : null;
            mlpensemble NeuralNetworkEnsemble_ = NeuralNetworkEnsemble_Trained ? NeuralNetworkEnsemble.make_copy() as mlpensemble : null;


            for (int n = 0; n < block_size; n++)
            {
                for (int j = 0; j < height; j++)
                {
                    FeaturePoint fp = FeaturePoint.GetOrAddFeaturePoint(j, i + n, imgId);
                    var output = Array.Empty<double>();

                    switch (classifier)
                    {
                        case 0:
                            dfprocess(DecisionForest, fp.GetFeatures(), ref output);
                            break;
                        case 1:
                            mlpprocess(NeuralNetwork_, fp.GetFeatures(), ref output);
                            break;
                        case 2:
                            mlpeprocess(NeuralNetworkEnsemble_, fp.GetFeatures(), ref output);
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

                    GetClassByNum(predictedClass).AddClassifiedPoints(fp, imgId);

                }
            }

            if (DecisionForest_Trained) DecisionForest_._deallocate();
            if (NeuralNetwork_Trained) NeuralNetwork_._deallocate();
        }

        static internal string TimeToString(TimeSpan t)
        {
            if (t.TotalSeconds < 60) return t.TotalSeconds + "s";
            else if (t.TotalMinutes < 60) return t.Minutes + "min " + t.Seconds + "s";
            else return t.TotalHours + "h " + t.Minutes + "min " + t.Seconds + "s";
        }

        internal static Classes GetClassByNum(int predictedClass)
        {
            foreach (Classes c in classesList)
            {
                if (c.classNumber == predictedClass) return c;
            }

            return null;
        }

        private void ClassificationCheck(object sender, RoutedEventArgs e)
        {
            string gridId = "grid_" + (ImageTab.SelectedItem as TabItem).Name.Substring(4);

            if (imageGrids.ContainsKey(gridId))
                foreach (ImageTile it in imageGrids[gridId].tiles)
                {
                    foreach (Image im in it.overlaysImages.Values)
                    {
                        im.Visibility = Visibility.Visible;
                    }
                }
        }

        private void ClassificationUncheck(object sender, RoutedEventArgs e)
        {
            string gridId = "grid_" + (ImageTab.SelectedItem as TabItem).Name.Substring(4);

            if (imageGrids.ContainsKey(gridId))
                foreach (ImageTile it in imageGrids[gridId].tiles)
                {
                    foreach (Image im in it.overlaysImages.Values)
                    {
                        im.Visibility = Visibility.Hidden;
                    }
                }
        }

        private void TrainingCheck(object sender, RoutedEventArgs e)
        {
            string gridId = "grid_" + (ImageTab.SelectedItem as TabItem).Name.Substring(4);

            if (imageGrids.ContainsKey(gridId))
                foreach (ImageTile it in imageGrids[gridId].tiles)
                {
                    it.trainImage.Visibility = Visibility.Visible;
                }
        }

        private void TrainingUncheck(object sender, RoutedEventArgs e)
        {
            string gridId = "grid_" + (ImageTab.SelectedItem as TabItem).Name.Substring(4);

            if (imageGrids.ContainsKey(gridId))
                foreach (ImageTile it in imageGrids[gridId].tiles)
                {
                    it.trainImage.Visibility = Visibility.Hidden;
                }
        }

        private void TestButtonClick(object sender, RoutedEventArgs e)
        {
            if (Testing)
            {
                CancelTesting = true;
                ProgressText.Text = "Testing Canceling ...";
                ProgressBar.IsIndeterminate = true;
            }
            else if (TestingGrids.Count > 0)
            {
                Testing = true;
                NegateButton(e.Source as Button);
                LockButton(TrainButton);
                LockButton(ClassifyButton);

                BackgroundWorker worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                worker.DoWork += Test;
                worker.ProgressChanged += ProgressChanged;

                int[] arg = new int[2];

                try
                {
                    arg[0] = ClassifierSelection.SelectedIndex;
                }
                catch
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Value = 0;


                    ProgressText.Text = "Error During Testing Preprocessing: No Image Available";
                    Testing = false;

                    ResetButton(sender as Button);
                    ResetButton(TrainButton);
                    ResetButton(ClassifyButton);

                    return;
                }

                worker.RunWorkerCompleted += TestingWorker_RunWorkerCompleted;

                worker.RunWorkerAsync(arg);
            }
            else
            {
                ProgressText.Text = "There are no Images set for Testing";
                Testing = false;

                ResetButton(sender as Button);
                ResetButton(TrainButton);
                ResetButton(ClassifyButton);
            }

        }

        private void Test(object sender, DoWorkEventArgs e)
        {
            int[] arg = e.Argument as int[];

            bool trained = false;
            switch (arg[0])
            {
                case 0: trained = DecisionForest_Trained; break;
                case 1: trained = NeuralNetwork_Trained; break;
                case 2: trained = NeuralNetworkEnsemble_Trained; break;

                default: break;
            }

            if (TestingGrids.Count == 0)
            {
                (sender as BackgroundWorker).ReportProgress(0, "Testing failed: No Image Set For Testing");
            }
            else if (trained)
            {
                DateTime time = System.DateTime.Now;

                (sender as BackgroundWorker).ReportProgress(0, "Testing ... ");

                int[,] TestResults_ = new int[classesList.Count, classesList.Count];

                int count = 0;
                try
                {
                    foreach (Classes c in classesList)
                    {
                        count += c.FeaturesCount(TestingGrids);
                    }
                    int n = 0;
                    foreach (string gridId in TestingGrids)
                    {
                        foreach (ImageTile it in imageGrids[gridId].tiles)
                        {
                            for (int i = 0; i < classesList.Count; i++)
                            {
                                Classes c = classesList[i];

                                if (c.featurePoints.ContainsKey(it.id))
                                    foreach (FeaturePoint fp in c.featurePoints[it.id])
                                    {
                                        if (CancelTesting)
                                        {
                                            (sender as BackgroundWorker).ReportProgress(0, "Testing Canceled");
                                            tested = false;
                                            return;
                                        }

                                        var output = Array.Empty<double>();

                                        switch (arg[0])
                                        {
                                            case 0:
                                                dfprocess(DecisionForest, fp.GetFeatures(), ref output);
                                                break;
                                            case 1:
                                                mlpprocess(NeuralNetwork, fp.GetFeatures(), ref output);
                                                break;
                                            case 2:
                                                mlpeprocess(NeuralNetworkEnsemble, fp.GetFeatures(), ref output);
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

                                        TestResults_[i, predictedClass]++;


                                        (sender as BackgroundWorker).ReportProgress((int)n * 100 / count, "*");
                                        n++;
                                    }

                            }
                        }
                    }
                }
                catch
                {
                    (sender as BackgroundWorker).ReportProgress(0, "Error During Testing: No Features are set for Testing");
                    tested = false;
                    return;
                }

                TestResults = new GeneralConfusionMatrix(TestResults_);

                (sender as BackgroundWorker).ReportProgress(100, "Testing Completed in " + TimeToString(System.DateTime.Now - time));
                tested = true;
                e.Result = arg[0];
            }
            else
            {
                (sender as BackgroundWorker).ReportProgress(0, "Testing failed: No Trained Classifier Available");
                tested = false;
            }
        }

        private void TestingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (tested)
            {
                //Statistics
                double[] Recall = TestResults.Recall;
                double[] Precision = TestResults.Precision;

                double Accuracy = Math.Round(100.0 * TestResults.Accuracy, 2);
                double Kappa = Math.Round(100.0 * TestResults.Kappa, 2);
                double Variance = Math.Round(100.0 * TestResults.Variance, 4);

                double AverageRecall = 0;
                double AveragePrecision = 0;

                for (int i = 0; i < Recall.Length; i++)
                {
                    Recall[i] = Math.Round(100.0 * Recall[i], 2);
                    Precision[i] = Math.Round(100.0 * Precision[i], 2);

                    AverageRecall += Recall[i];
                    AveragePrecision += Precision[i];
                }

                AveragePrecision = Math.Round(AveragePrecision / Precision.Length, 2);
                AverageRecall = Math.Round(AverageRecall / Recall.Length, 2);


                StackPanel sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                TextBlock tb = new TextBlock
                {
                    Text = "Test Results " + ++testResultCount,
                    Background = new SolidColorBrush(Colors.Transparent),
                    Foreground = White,
                    Margin = new Thickness(0, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sp.Children.Add(tb);

                Button deleteImage = new Button
                {
                    Content = "x",
                    Name = "deleteRe" + testResultCount,
                    Background = SteelRed,
                    Foreground = White,
                    Margin = new Thickness(5, 0, 0, 0),
                    Width = 20,
                    Height = 20,
                    VerticalAlignment = VerticalAlignment.Center
                };

                deleteImage.Click += DeleteResult;

                sp.Children.Add(deleteImage);

                TabItem newTab = new TabItem
                {
                    Background = Transparent,
                    Foreground = White,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Header = sp,
                    Name = "TestResults" + testResultCount
                };

                Grid grid = new Grid
                {
                    Background = DarkBackground,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };

                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(45)
                });
                grid.RowDefinitions.Add(new RowDefinition
                {
                });
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(45)
                });
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(30)
                });
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(30)
                });
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(30)
                });
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(30)
                });
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(30)
                });
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(30)
                });

                Grid resutlTable = new Grid
                {
                    Background = DarkBackground,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                //Create Columns
                resutlTable.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(50),
                });

                resutlTable.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(110),
                });

                for (int i = 0; i < classesList.Count + 1; i++)
                {
                    resutlTable.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(100),
                    });
                }

                // Create Rows
                resutlTable.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(50)
                });

                for (int i = 0; i < classesList.Count + 2; i++)
                {
                    resutlTable.RowDefinitions.Add(new RowDefinition
                    {
                        Height = new GridLength(45)
                    });
                }

                TextBlock predictedText = new TextBlock
                {
                    Text = "Predicted Class",
                    TextAlignment = TextAlignment.Center,
                    LayoutTransform = new RotateTransform(-90),
                    FontSize = 14,
                    FontWeight = FontWeights.ExtraBold,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                resutlTable.Children.Add(predictedText);
                Grid.SetRow(predictedText, 1);
                Grid.SetRowSpan(predictedText, classesList.Count + 1);
                Grid.SetColumn(predictedText, 0);

                TextBlock actualText = new TextBlock
                {
                    Text = "Actual Class",
                    TextAlignment = TextAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.ExtraBold,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                resutlTable.Children.Add(actualText);
                Grid.SetRow(actualText, 0);
                Grid.SetColumn(actualText, 1);

                Grid.SetColumnSpan(actualText, classesList.Count + 1);

                //Add Headers
                for (int i = 0; i < classesList.Count + 1; i++)
                {
                    TextBlock header1 = new TextBlock
                    {
                        FontWeight = FontWeights.Bold
                    };
                    TextBlock header2 = new TextBlock
                    {
                        FontWeight = FontWeights.Bold
                    };

                    if (i == classesList.Count)
                    {
                        header2.Text = "Recall";
                        header1.Text = "Precision";
                    }
                    else
                    {
                        header1.Text = classesList[i].className;
                        header2.Text = classesList[i].className;
                    }

                    Grid.SetRow(header1, 1);
                    Grid.SetColumn(header1, i + 2);

                    resutlTable.Children.Add(header1);


                    Grid.SetRow(header2, i + 2);
                    Grid.SetColumn(header2, 1);

                    resutlTable.Children.Add(header2);
                }

                for (int i = 0; i < classesList.Count + 1; i++)
                {
                    for (int j = 0; j < classesList.Count + 1; j++)
                    {
                        TextBlock text = new TextBlock();

                        if (i == classesList.Count && j != classesList.Count)
                        {
                            text.Text = "" + Precision[j] + "%";
                            if (Precision[j] >= 95) text.Foreground = SteelGreen;
                            else if (Precision[j] >= 90) text.Foreground = SteelYellow;
                            else if (Precision[j] >= 85) text.Foreground = SteelOrange;
                            else text.Foreground = SteelRed;

                        }
                        else if (i != classesList.Count && j == classesList.Count)
                        {
                            text.Text = "" + Recall[i] + "%";
                            if (Recall[i] >= 95) text.Foreground = SteelGreen;
                            else if (Recall[i] >= 90) text.Foreground = SteelYellow;
                            else if (Recall[i] >= 85) text.Foreground = SteelOrange;
                            else text.Foreground = SteelRed;

                        }
                        else if (i != classesList.Count && j != classesList.Count)
                        {
                            text.Text = "" + TestResults.Matrix[i, j];
                            if (TestResults.Matrix[i, j] > 0) text.Foreground = SteelRed;
                        }

                        if (i == j)
                        {
                            text.Foreground = SteelBlue;
                        }

                        Grid.SetColumn(text, i + 2);
                        Grid.SetRow(text, j + 2);

                        resutlTable.Children.Add(text);
                    }
                }

                grid.Children.Add(resutlTable);
                Grid.SetRow(resutlTable, 1);

                string classifier = "";
                switch ((int)e.Result)
                {
                    case 0: classifier = "Decision Forest"; break;
                    case 1: classifier = "Neural Network"; break;
                    case 2: classifier = "Neural Network Ensemble"; break;

                    default: break;
                }

                TextBlock title = new TextBlock
                {
                    Text = "Test Results for " + classifier,
                    FontSize = 16,
                    FontWeight = FontWeights.ExtraBold,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 20, 0, 0),
                };
                grid.Children.Add(title);
                Grid.SetRow(title, 0);

                TextBlock Stats = new TextBlock
                {
                    Text = "Overal Statistics",
                    FontSize = 16,
                    FontWeight = FontWeights.ExtraBold,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 20, 0, 0),
                };

                grid.Children.Add(Stats);
                Grid.SetRow(Stats, 2);

                TextBlock averagePrecision = new TextBlock
                {
                    Text = "Average Precision : " + AveragePrecision + "%",
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                if (AveragePrecision >= 95) averagePrecision.Foreground = SteelGreen;
                else if (AveragePrecision >= 90) averagePrecision.Foreground = SteelYellow;
                else if (AveragePrecision >= 85) averagePrecision.Foreground = SteelOrange;
                else averagePrecision.Foreground = SteelRed;

                grid.Children.Add(averagePrecision);
                Grid.SetRow(averagePrecision, 3);

                TextBlock averageRecall = new TextBlock
                {
                    Text = "Average Recall : " + AverageRecall + "%",
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                if (AverageRecall >= 95) averageRecall.Foreground = SteelGreen;
                else if (AverageRecall >= 90) averageRecall.Foreground = SteelYellow;
                else if (AverageRecall >= 85) averageRecall.Foreground = SteelOrange;
                else averageRecall.Foreground = SteelRed;

                grid.Children.Add(averageRecall);
                Grid.SetRow(averageRecall, 4);

                TextBlock accuracy = new TextBlock
                {
                    Text = "Accuracy : " + Accuracy + "%",
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                if (Accuracy >= 95) accuracy.Foreground = SteelGreen;
                else if (Accuracy >= 90) accuracy.Foreground = SteelYellow;
                else if (Accuracy >= 85) accuracy.Foreground = SteelOrange;
                else accuracy.Foreground = SteelRed;

                grid.Children.Add(accuracy);
                Grid.SetRow(accuracy, 5);


                TextBlock variance = new TextBlock
                {
                    Text = "Variance : " + Variance + "%",
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };

                grid.Children.Add(variance);
                Grid.SetRow(variance, 6);


                TextBlock kappa = new TextBlock
                {
                    Text = "Kappa * 100 : " + Kappa,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                if (Kappa >= 95) kappa.Foreground = SteelGreen;
                else if (Kappa >= 90) kappa.Foreground = SteelYellow;
                else if (Kappa >= 85) kappa.Foreground = SteelOrange;
                else kappa.Foreground = SteelRed;


                grid.Children.Add(kappa);
                Grid.SetRow(kappa, 7);

                newTab.Content = grid;
                ImageTab.Items.Add(newTab);
                ImageTab.SelectedItem = newTab;
            }

            Testing = false;
            trainingTesting = false;
            ResetButton(TestingButton);
            ResetButton(TrainButton);
            ResetButton(ClassifyButton);
            ResetButton(TrainTestButton);

        }

        private void DeleteResult(object sender, RoutedEventArgs e)
        {
            string id = "TestResults" + (sender as Button).Name.Substring(8);

            foreach (TabItem tb in ImageTab.Items)
            {
                if (tb.Name == id)
                {
                    ImageTab.Items.Remove(tb); break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLoad.SaveFile(this);
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLoad.LoadFile(this);
        }

        private void ImageUncheck(object sender, RoutedEventArgs e)
        {
            try
            {
                var children = ((ImageTab.SelectedItem as TabItem).Content as Grid).Children;
                foreach (var el in children)
                {
                    if ((el as Image).Name == "img" + (ImageTab.SelectedItem as TabItem).Name.Substring(3))
                    {
                        (el as Image).Visibility = Visibility.Hidden;
                    }
                }
            }
            catch { }
        }

        private void ImageCheck(object sender, RoutedEventArgs e)
        {
            try
            {
                var children = ((ImageTab.SelectedItem as TabItem).Content as Grid).Children;
                foreach (var el in children)
                {
                    if ((el as Image).Name == "img" + (ImageTab.SelectedItem as TabItem).Name.Substring(3))
                    {
                        (el as Image).Visibility = Visibility.Visible;
                    }
                }
            }
            catch { }
        }

        private void RGBCheck_Checked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.RGBEnable = true;
        }

        private void RGBCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.RGBEnable = false;
        }

        private void HSLCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.HSLEnable = false;
        }

        private void HSLCheck_Checked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.HSLEnable = true;
        }

        private void XYZCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.XYZEnable = false;
        }

        private void XYZCheck_Checked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.XYZEnable = true;
        }

        private void GLCMCheck_Checked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.GLCMEnable = true;
        }

        private void GLCMCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.GLCMEnable = false;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+;");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ClassifierSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selection = (sender as ComboBox).SelectedIndex;
            if (selection == 0)
            {
                NN_Options.Visibility = Visibility.Collapsed;
                DF_Options.Visibility = Visibility.Visible;
                NNE_Options.Visibility = Visibility.Collapsed;
                SVM_Options.Visibility = Visibility.Collapsed;

                if (!DecisionForest_Trained)
                {
                    LockButton(TestingButton);
                    LockButton(ClassifyButton);
                }
                else
                {
                    ResetButton(TestingButton);
                    ResetButton(ClassifyButton);
                }
            }
            else if (selection == 1)
            {
                NN_Options.Visibility = Visibility.Visible;
                DF_Options.Visibility = Visibility.Collapsed;
                NNE_Options.Visibility = Visibility.Collapsed;
                SVM_Options.Visibility = Visibility.Collapsed;

                if (!NeuralNetwork_Trained)
                {
                    LockButton(TestingButton);
                    LockButton(ClassifyButton);
                }
                else
                {
                    ResetButton(TestingButton);
                    ResetButton(ClassifyButton);
                }
            }
            else if (selection == 2)
            {
                NN_Options.Visibility = Visibility.Collapsed;
                DF_Options.Visibility = Visibility.Collapsed;
                NNE_Options.Visibility = Visibility.Visible;
                SVM_Options.Visibility = Visibility.Collapsed;

                if (!NeuralNetwork_Trained)
                {
                    LockButton(TestingButton);
                    LockButton(ClassifyButton);
                }
                else
                {
                    ResetButton(TestingButton);
                    ResetButton(ClassifyButton);
                }
            }

            else if (selection == 3)
            {
                NN_Options.Visibility = Visibility.Collapsed;
                DF_Options.Visibility = Visibility.Collapsed;
                NNE_Options.Visibility = Visibility.Collapsed;
                SVM_Options.Visibility = Visibility.Visible;

                /*
                if (!NeuralNetwork_Trained)
                {
                    LockButton(TestingButton);
                    LockButton(ClassifyButton);
                }
                else
                {
                    ResetButton(TestingButton);
                    ResetButton(ClassifyButton);
                }
                */
            }
        }

        internal void Img_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double width = (sender as Image).RenderSize.Width;
            double height = (sender as Image).RenderSize.Height;

            double click_x = e.GetPosition((Image)sender).X / (sender as Image).RenderSize.Width;
            double click_y = e.GetPosition((Image)sender).Y / (sender as Image).RenderSize.Height;

            foreach (object obj in ((sender as Image).Parent as Grid).Children)
            {
                if (obj is Image)
                {
                    (obj as Image).RenderTransformOrigin = new Point(click_x, click_y);
                    ScaleTransform st = (obj as Image).RenderTransform as ScaleTransform;

                    double zoom = e.Delta > 0 ? 1.2 : 0.8;

                    st.ScaleX *= zoom;
                    st.ScaleY *= zoom;

                    if (st.ScaleX < 1) st.ScaleX = 1;
                    if (st.ScaleY < 1) st.ScaleY = 1;

                    if (st.ScaleX > 50) st.ScaleX = 50;
                    if (st.ScaleY > 50) st.ScaleY = 50;
                }
            }
        }

        internal void Grid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double width = (sender as Grid).RenderSize.Width;
            double height = (sender as Grid).RenderSize.Height;

            double click_x = e.GetPosition((Grid)sender).X / (sender as Grid).RenderSize.Width;
            double click_y = e.GetPosition((Grid)sender).Y / (sender as Grid).RenderSize.Height;


            (sender as Grid).RenderTransformOrigin = new Point(click_x, click_y);
            TransformGroup transform = (sender as Grid).RenderTransform as TransformGroup;
            ScaleTransform st = transform.Children[0] as ScaleTransform;
            TranslateTransform tf = transform.Children[1] as TranslateTransform;

            st.CenterX = click_x;
            st.CenterY = click_y;

            double zoom = e.Delta > 0 ? 1.2 : 0.8;

            st.ScaleX *= zoom;
            st.ScaleY *= zoom;

            if (st.ScaleX < 1) st.ScaleX = 1;
            if (st.ScaleY < 1) st.ScaleY = 1;

            if (st.ScaleX > 50) st.ScaleX = 50;
            if (st.ScaleY > 50) st.ScaleY = 50;


        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLoad.ExportClassification(this);
        }

        private void HOGCheck_Checked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.HOGEnable = true;
        }

        private void HOGCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.HOGEnable = false;
        }

        private void LBPCheck_Checked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.LBPEnable = true;
        }

        private void LBPCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.LBPEnable = false;
        }

        private void HCheck_Checked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.HeightEnable = true;
        }

        private void HCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.HeightEnable = false;
        }

        private void NCheck_Checked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.NormalEnable = true;
        }

        private void NCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.NormalEnable = false;
        }


        private void SlopeCheck_Checked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.SlopeEnable = true;
        }


        private void SlopeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FeaturePoint.SlopeEnable = false;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            int level = int.Parse(zoomlevel.Text);

            string[] x_ = x_coord.Text.Split('-');
            string[] y_ = y_coord.Text.Split('-');

            long x0 = long.Parse(x_[0]);
            long x1 = x_.Length == 2 ? long.Parse(x_[1]) : x0;
            long y0 = long.Parse(y_[0]);
            long y1 = y_.Length == 2 ? long.Parse(y_[1]) : y0;

            long size_x = x1 - x0 + 1;
            long size_y = y1 - y0 + 1;


            string[,] FileNames = new string[size_x, size_y];

            Task.Run(() =>
            {

                for (long x = x0; x <= x1; x++)
                {
                    for (long y = y0; y <= y1; y++)
                    {
                        //Load Satellite Image
                        //  string uri = "http://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/tile/" + (level) + "/" + (y) + "/" + (x);
                        string filename = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\tmp\\sat\\" + (level) + "_" + (y) + "_" + (x) + ".png";

                        //  if (!File.Exists(filename)) SaveLoad.DownloadRemoteFile(uri, filename);

                        FileNames[x - x0, y - y0] = filename;
                    }
                }

                SaveLoad.LoadImageRange(this, ++imageGridIdCount, FileNames, (int)(x1 - x0 + 1), (int)(y1 - y0 + 1), level);
            });
        }

        private void PostProcessPixel_Click(object sender, RoutedEventArgs e)
        {
            if (Thresholding)
            {
                thresholdWorker.CancelAsync();
            }
            else
            {
                NegateButton(e.Source as Button);

                LockButton(TrainButton);
                LockButton(ClassifyButton);
                LockButton(TestingButton);
                LockButton(TrainTestButton);
                LockButton(Blur);
                LockButton(ExportButton);

                Thresholding = true;

                thresholdWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                thresholdWorker.DoWork += PostProcessPixels;
                thresholdWorker.ProgressChanged += ProgressChanged;
                thresholdWorker.RunWorkerCompleted += ThresholdWorker_RunWorkerCompleted;

                thresholdWorker.RunWorkerAsync();
            }
        }

        private void ThresholdWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ResetButton(Lonely);
            ResetButton(TrainButton);
            ResetButton(ClassifyButton);
            ResetButton(TestingButton);
            ResetButton(TrainTestButton);
            ResetButton(Blur);
            ResetButton(ExportButton);

            Thresholding = false;
        }

        private void PostProcessPixels(object sender, DoWorkEventArgs e)
        {
            if (imageGrids.Count == 0)
            {
                (sender as BackgroundWorker).ReportProgress(0, "Error: No grids detected");
                return;
            }



            (sender as BackgroundWorker).ReportProgress(-1, "Starting Thresholding ...");
            DateTime time = DateTime.Now;
            string gridId = "";
            int NStep = 0;
            dispatcher.Invoke(() =>
            {
                gridId = "grid_" + (ImageTab.SelectedItem as TabItem).Name.Substring(4);
                NStep = int.Parse(NSteps.Text);

                string[] thresholds = LonelinessThreshold.Text.Split(';');
                string[] radius = LookRadius.Text.Split(';');

                for (int i = 0; i < classesList.Count; i++)
                {
                    int i_t = i < thresholds.Length ? i : thresholds.Length - 1;
                    int i_r = i < radius.Length ? i : radius.Length - 1;

                    classesList[i].threshold = int.Parse(thresholds[i_t]);
                    Log("Threshold " + int.Parse(thresholds[i_t]));
                    classesList[i].radius = int.Parse(radius[i_r]);
                    Log("Radius " + int.Parse(radius[i_t]));

                }
            });

            int j = 1;
            foreach (ImageTile it in imageGrids[gridId].tiles)
            {
                if ((sender as BackgroundWorker).CancellationPending)
                {
                    (sender as BackgroundWorker).ReportProgress(0, "Thresholding canceled after " + TimeToString(DateTime.Now - time));
                    return;
                }

                (sender as BackgroundWorker).ReportProgress(0, "Thresholding (Image " + j + "/" + imageGrids[gridId].tiles.Length + ")");

                if (it.classified)
                {
                    ImageHelper.RemoveLonelyPixels(this, it.id, NStep, sender as BackgroundWorker);
                }
                j++;
            }

            (sender as BackgroundWorker).ReportProgress(100, "Thresholding finished in " + TimeToString(DateTime.Now - time));

        }

        private void PostProcessBlur(object sender, RoutedEventArgs e)
        {
            string gridId = "grid_" + (ImageTab.SelectedItem as TabItem).Name.Substring(4);

            foreach (ImageTile it in imageGrids[gridId].tiles)
            {
                if (it.classified)
                {
                    ProgressText.Text = "PostProcessing: Bluring Edges";
                    ImageHelper.BluringEdges_4(this, it.id, (int)(double.Parse(BlurRadius.Text)));
                }
            }
        }

        private void TrainTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (trainingTesting)
            {
                TrainingTestingWorker.CancelAsync();

                ProgressText.Text = "Train&Test Canceling ...";
                ProgressBar.IsIndeterminate = true;
            }
            else
            {
                if (TestingGrids.Count < 0 || TrainingGrids.Count < 0) return;

                TrainingTestingWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                TrainingTestingWorker.DoWork += TrainTest;
                TrainingTestingWorker.ProgressChanged += ProgressChanged;

                NegateButton(sender as Button);
                LockButton(ClassifyButton);
                LockButton(TestingButton);
                LockButton(TrainButton);

                trainingTesting = true;

                TrainingTestingWorker.RunWorkerCompleted += TestingWorker_RunWorkerCompleted;
                TrainingTestingWorker.WorkerSupportsCancellation = true;

                int arg = ClassifierSelection.SelectedIndex;

                DecisionForest_NTree = int.Parse(DF_Trees.Text);
                DecisionForest_R = double.Parse(DF_R.Text);

                NeuralNetwork_Decay = double.Parse(NN_Decay.Text);
                NeuralNetwork_WStep = double.Parse(NN_Step.Text);
                NeuralNetwork_MaxIts = int.Parse(NN_MaxIts.Text);
                NeuralNetwork_Layer1 = int.Parse(NN_L1.Text);
                NeuralNetwork_Layer2 = int.Parse(NN_L2.Text);

                NeuralNetworkEnsemble_Decay = double.Parse(NNE_Decay.Text);
                NeuralNetworkEnsemble_WStep = double.Parse(NNE_Step.Text);
                NeuralNetworkEnsemble_MaxIts = int.Parse(NNE_MaxIts.Text);
                NeuralNetworkEnsemble_Layer1 = int.Parse(NNE_L1.Text);
                NeuralNetworkEnsemble_Layer2 = int.Parse(NNE_L2.Text);
                NeuralNetworkEnsembleSize = int.Parse(NNE_Size.Text);

                SVM_kernel = KernelSelection.SelectedIndex;
                SVMGausG = double.Parse(SVM_gamma.Text);
                SVMPolyP = int.Parse(SVM_P.Text);
                SVMComplexity = int.Parse(SVM_C.Text);

                TrainTestIterations = int.Parse(tt_iteration.Text);

                TrainingTestingWorker.RunWorkerAsync(arg);
            }
        }

        private void TrainTest(object sender, DoWorkEventArgs e)
        {
            try
            {
                for (int n = 0; n < classesList.Count; n++)
                {
                    classesList[n].classNumber = n;
                }

                int classifer = (int)e.Argument;
                e.Result = classifer;

                DateTime time = System.DateTime.Now;

                (sender as BackgroundWorker).ReportProgress(-1, "Starting Training & Testing ...");

                ConcurrentBag<int[,]> TestResultsList = new ConcurrentBag<int[,]>();

                Dictionary<int, List<double[]>> features;
                int featuresCount = 0;
                int i = 0;

                try
                {
                    features = GetAllFeatures(sender);
                }
                catch (Exception ex)
                {
                    (sender as BackgroundWorker).ReportProgress((i + 1) * 100, "Error: " + ex.Message);

                    return;
                }

             (sender as BackgroundWorker).ReportProgress(-1, "Training & Testing... ");
                int outInfo = -1;

                TimeSpan[] trainingTime = new TimeSpan[TrainTestIterations];
                TimeSpan[] testingTime = new TimeSpan[TrainTestIterations];
                int done = 0;

                Parallel.For(0, TrainTestIterations, index =>
                {
                    DateTime time2 = System.DateTime.Now;

                    multilayerperceptron NeuralNetwork = new multilayerperceptron();
                    decisionforest DecisionForest = new decisionforest();
                    mlpensemble NeuralNetworkEnsemble = new mlpensemble();
                    MulticlassSupportVectorMachine<Linear> SVMLinear = null;
                    MulticlassSupportVectorMachine<Gaussian> SVMGaussian = null;
                    MulticlassSupportVectorMachine<Polynomial> SVMPoly = null;



                    double proportion = classifer == 3 ? (SVM_kernel == 1 ? 0.01 : 0.01) : 0.1;

                    ClassificationHelper.PartitionFeatures(features, out double[,] train_features, out List<double[]> test_features, proportion);

                    // TRAINING
                    try
                    {
                        switch (classifer)
                        {
                            case 0:
                                alglib.dfbuildrandomdecisionforest(train_features, train_features.Length / (FeaturePoint.NFEATURES + 1), FeaturePoint.NFEATURES, classesList.Count, DecisionForest_NTree, DecisionForest_R, out outInfo, out DecisionForest, out dfreport report);
                                break;

                            case 1:
                                mlptrainer trn;
                                mlpcreatetrainercls(FeaturePoint.NFEATURES, classesList.Count, out trn);
                                mlpsetdataset(trn, train_features, featuresCount);
                                mlpsetdecay(trn, NeuralNetwork_Decay);
                                mlpsetcond(trn, NeuralNetwork_WStep, NeuralNetwork_MaxIts);

                                if (NeuralNetwork_Layer1 <= 0 && NeuralNetwork_Layer2 <= 0) mlpcreatec0(FeaturePoint.NFEATURES, classesList.Count, out NeuralNetwork);
                                else if (NeuralNetwork_Layer1 <= 0 || NeuralNetwork_Layer2 <= 0) mlpcreatec1(FeaturePoint.NFEATURES, Math.Max(NeuralNetwork_Layer1, NeuralNetwork_Layer2), classesList.Count, out NeuralNetwork);
                                else if (NeuralNetwork_Layer1 > 0 && NeuralNetwork_Layer2 > 0) mlpcreatec2(FeaturePoint.NFEATURES, NeuralNetwork_Layer1, NeuralNetwork_Layer2, classesList.Count, out NeuralNetwork);

                                smp_mlptrainnetwork(trn, NeuralNetwork, 5, out mlpreport reportNN);
                                break;

                            case 2:
                                mlptrainer trn2;
                                mlpcreatetrainercls(FeaturePoint.NFEATURES, classesList.Count, out trn2);
                                mlpsetdataset(trn2, train_features, featuresCount);
                                mlpsetdecay(trn2, NeuralNetwork_Decay);
                                mlpsetcond(trn2, NeuralNetwork_WStep, NeuralNetwork_MaxIts);

                                if (NeuralNetwork_Layer1 <= 0 && NeuralNetwork_Layer2 <= 0) mlpecreatec0(FeaturePoint.NFEATURES, classesList.Count, NeuralNetworkEnsembleSize, out NeuralNetworkEnsemble);
                                if (NeuralNetwork_Layer1 <= 0 || NeuralNetwork_Layer2 <= 0) mlpecreatec1(FeaturePoint.NFEATURES, Math.Max(NeuralNetwork_Layer1, NeuralNetwork_Layer2), classesList.Count, NeuralNetworkEnsembleSize, out NeuralNetworkEnsemble);
                                else if (NeuralNetwork_Layer1 > 0 && NeuralNetwork_Layer2 > 0) mlpecreatec2(FeaturePoint.NFEATURES, NeuralNetwork_Layer1, NeuralNetwork_Layer2, classesList.Count, NeuralNetworkEnsembleSize, out NeuralNetworkEnsemble);

                                mlptrainensemblees(trn2, NeuralNetworkEnsemble, 5, out mlpreport report2);
                                break;

                            case 3:
                                double[][] inputs = new double[train_features.Length / (FeaturePoint.NFEATURES + 1)][];
                                int[] outputs = new int[train_features.Length / (FeaturePoint.NFEATURES + 1)];

                                for (int x = 0; x < outputs.Length; x++)
                                {
                                    inputs[x] = new double[FeaturePoint.NFEATURES];

                                    for (int y = 0; y < FeaturePoint.NFEATURES; y++)
                                    {
                                        inputs[x][y] = train_features[x, y];
                                    }

                                    outputs[x] = (int)train_features[x, FeaturePoint.NFEATURES];


                                }
                                switch (SVM_kernel)
                                {
                                    case 0:
                                        IKernel k0 = new Linear();
                                        var c0 = k0.EstimateComplexity(inputs);
                                        Log("SVM Complexity: " + c0);
                                        var teacher0 = new MulticlassSupportVectorLearning<Linear>()
                                        {
                                            Learner = (param) => new LinearDualCoordinateDescent<Linear>()
                                            {
                                                Complexity = c0,
                                                Tolerance = SVMTolerance,
                                                Loss = Loss.L2,
                                            }
                                        };
                                        SVMLinear = teacher0.Learn(inputs, outputs);

                                        break;
                                    case 1:
                                        Polynomial k1 = new Polynomial(SVMPolyP, SVMPolyP==2 ? 1:0);

                                        double[] test = k1.Transform(inputs[0]);

                                        var c1 = k1.EstimateComplexity(inputs);
                                        Log("SVM Complexity: " + c1);

                                        var teacher1 = new MulticlassSupportVectorLearning<Polynomial>()
                                        {
                                            Learner = (param) => new SequentialMinimalOptimization<Polynomial>()
                                            {                                                
                                                Complexity = c1,
                                                Kernel = k1
                                            }
                                        };

                                        SVMPoly = teacher1.Learn(inputs, outputs);

                                        Log("Error : " + new Accord.Math.Optimization.Losses.ZeroOneLoss(outputs).Loss(SVMPoly.Decide(inputs)));

                                        break;
                                    case 2:
                                        Gaussian k2 = new Gaussian(SVMGausG);

                                        var c2 = k2.EstimateComplexity(inputs);
                                        Log("SVM Complexity: " + c2);

                                        var teacher2 = new MulticlassSupportVectorLearning<Gaussian>()
                                        {
                                            Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
                                            {
                                               Complexity = c2,
                                               // Tolerance = SVMTolerance,
                                               // Epsilon = SVMEpsilon,
                                                Kernel = k2,
                                            }
                                        };

                                        SVMGaussian = teacher2.Learn(inputs, outputs); break;


                                    default: throw new Exception();
                                }

                                break;

                            default: throw new Exception();

                        }


                        trainingTime[index] = System.DateTime.Now - time2;

                        //TESTING

                        DateTime time3 = System.DateTime.Now;

                        int[,] TestResult = new int[classesList.Count, classesList.Count];

                        foreach (double[] f in test_features)
                        {
                            double[] x = new double[f.Length - 1];
                            for (int x_i = 0; x_i < x.Length; x_i++)
                            {
                                x[x_i] = f[x_i];
                            }

                            if (CancelTesting)
                            {
                                tested = false;
                                return;
                            }

                            var output = Array.Empty<double>();

                            switch (classifer)
                            {
                                case 0:
                                    dfprocess(DecisionForest, x, ref output);
                                    break;
                                case 1:
                                    mlpprocess(NeuralNetwork, x, ref output);
                                    break;
                                case 2:
                                    mlpeprocess(NeuralNetworkEnsemble, x, ref output);
                                    break;

                                case 3:
                                    int predictedClass = 0;

                                    for (int x_i = 0; x_i < x.Length; x_i++)
                                    {
                                        x[x_i] = f[x_i];
                                    }

                                    switch (SVM_kernel)
                                    {
                                        case 0:
                                            predictedClass = SVMLinear.Decide(x);
                                            break;
                                        case 1:
                                            predictedClass = SVMPoly.Decide(x);
                                            break;
                                        case 2:
                                            predictedClass = SVMGaussian.Decide(x);
                                            break;
                                        default:
                                            throw new Exception();
                                    }

                                    TestResult[(int)f[f.Length - 1], predictedClass]++;
                                    break;

                                default: break;
                            }

                            if (classifer != 3)
                            {
                                int predictedClass = 0;
                                for (int k = 1; k < output.Length; k++)
                                {
                                    if (output[k] > output[predictedClass])
                                    {
                                        predictedClass = k;
                                    }
                                }
                                TestResult[(int)f[f.Length - 1], predictedClass]++;
                            }
                        }

                        TestResultsList.Add(TestResult);

                        testingTime[index] = System.DateTime.Now - time3;

                        done++;
                        double percentage = 100.0 * done / TrainTestIterations;
                        double estimatedTime = (System.DateTime.Now - time).TotalSeconds * (TrainTestIterations - done) / done;

                        (sender as BackgroundWorker).ReportProgress((int)percentage, "Cross Validation: Fold " + done + "/" + TrainTestIterations + " (Estimated time remaining: " + TimeToString(TimeSpan.FromSeconds(estimatedTime)) + ")");
                    }
                    catch (Exception ex)
                    {
                        if(ex.InnerException != null)
                            Log("Error: " + ex.InnerException.Message);
                        else
                            Log("Error: " + ex.Message);   
                    }
                });

                GeneralConfusionMatrix[] gcms = new GeneralConfusionMatrix[TestResultsList.Count];
                double[] accuracies = new double[TestResultsList.Count];
                double avgAccuracy = 0;

                int c = 0;
                foreach (int[,] t in TestResultsList)
                {
                    gcms[c] = new GeneralConfusionMatrix(t);
                    accuracies[c] = gcms[c].Accuracy;
                    avgAccuracy += accuracies[c] / accuracies.Length;
                    c++;
                }
                double variance = 0;

                foreach (double a in accuracies)
                {
                    variance = (a - avgAccuracy) * (a - avgAccuracy) / accuracies.Length;
                }

                Log("Average Accuracy = " + avgAccuracy + " -- Variance = " + variance);

                TestResults = GeneralConfusionMatrix.Combine(gcms);

                double avgtrain = 0.0;
                double avgtest = 0.0;
                for (int t = 0; t < TrainTestIterations; t++)
                {
                    avgtest += 1.0 * testingTime[t].TotalSeconds / TrainTestIterations;
                    avgtrain += 1.0 * trainingTime[t].TotalSeconds / TrainTestIterations;
                }

                trainingTesting = false;
                tested = true;
                e.Result = classifer;
                (sender as BackgroundWorker).ReportProgress(100, "Testing & Training Completed in " + TimeToString(System.DateTime.Now - time) + " (Training: " + TimeToString(TimeSpan.FromSeconds(avgtrain)) + " Testing: " + TimeToString(TimeSpan.FromSeconds(avgtest)) + ")");
            }
            catch (Exception exc)
            {
                trainingTesting = false;
                (sender as BackgroundWorker).ReportProgress(0, "Training Error: Something Unexpected Happened! " + exc.Message);
                Log("Exception thrown : " + exc.Message);
            }
        }

        public static void Log(string s)
        {
            string line = "[" + DateTime.Now.ToLongTimeString() + "] " + s;
            Trace.WriteLine(line);

            dispatcher.Invoke(() =>
            {
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(logPath, true))
                {
                    file.WriteLine(line);
                }
            });

        }

        private void CreateQueue(BatchProcessParam p, ref Queue<ClassifyProcessParam> queue, Queue<ClassifyProcessParam> queue2)
        {
            if (queue2.Count == 0) return;
            ClassifyProcessParam param = queue2.Dequeue();
            if (!queue.Contains(param)) queue.Enqueue(param);

            for (int x_ = Math.Max(p.x0, param.WorldX - 1); x_ <= Math.Min(p.x1, param.WorldX + 1); x_++)
            {
                for (int y_ = Math.Max(p.y0, param.WorldY - 1); y_ <= Math.Min(p.y1, param.WorldY + 1); y_++)
                {
                    if (x_ != param.WorldX || y_ != param.WorldY)
                    {
                        ClassifyProcessParam param2 = new ClassifyProcessParam(p.zoom, x_ - p.x0, y_ - p.y0, x_, y_);
                        if (!queue.Contains(param2))
                        {
                            if (!queue2.Contains(param2))
                            {
                                queue2.Enqueue(param2);
                            }
                        }
                    }
                }
            }

            CreateQueue(p, ref queue, queue2);
        }

        public void BatchProcess(object sender, DoWorkEventArgs e)
        {
            (sender as BackgroundWorker).ReportProgress(-1, "Starting Batch Process ...");

            DateTime time = DateTime.Now;
            BatchProcessParam p = e.Argument as BatchProcessParam;
            Queue<ClassifyProcessParam> queue = new Queue<ClassifyProcessParam>();
            Queue<ClassifyProcessParam> queue2 = new Queue<ClassifyProcessParam>();
            queue2.Enqueue(new ClassifyProcessParam(p.zoom, 0, 0, p.x0, p.y0));

            string date = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "-" + DateTime.Now.Minute;
            string path = @"export/" + date + "/" + p.zoom;

            System.IO.Directory.CreateDirectory(path);

            CreateQueue(p, ref queue, queue2);

            ImageGrid g = null;
            dispatcher.Invoke(() =>
            {
                imageGridIdCount++;
                g = new ImageGrid(this, "BatchProcess_" + imageGridIdCount, "BatchProcess_" + imageGridIdCount, p.x1 - p.x0 + 1, p.y1 - p.y0 + 1);

                TabItem newTab = new TabItem
                {
                    Background = Transparent,
                    Foreground = White,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                };

                newTab.Visibility = Visibility.Visible;

                newTab.Name = g.id;

                Grid grid = new Grid
                {
                    Background = DarkBackground,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };

                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());

                grid.Children.Add(g.grid);

                Grid.SetColumn(g.grid, 0);
                Grid.SetRow(g.grid, 0);

                newTab.Content = grid;

                ImageTab.Items.Add(newTab);
                ImageTab.SelectedItem = newTab;
            });

            int classifier = -1;
            dispatcher.Invoke(() => classifier = MainWindow.Instance.ClassifierSelection.SelectedIndex);

            BatchClassification(sender as BackgroundWorker, g, queue, classifier, path);

            dispatcher.Invoke(() =>
            {
                ImageHelper.DeleteImage(this, g.id);

                foreach (TabItem tb in ImageTab.Items)
                {
                    if (tb.Name == g.id)
                    {
                        images.Remove((tb.Content as Grid).Children[0] as Image);

                        if (ImageTab.SelectedItem as TabItem == tb) ImageTab.SelectedItem = ImageTab.Items[ImageTab.Items.Count - 1];
                        ImageTab.Items.Remove(tb);

                        break;
                    }
                }

                GC.Collect();

                UpdateClassTree();
            });

            (sender as BackgroundWorker).ReportProgress(100, "Batch Process completed in " + TimeToString(DateTime.Now - time));
        }

        private void BatchProcess_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            worker.DoWork += BatchProcess;
            worker.ProgressChanged += ProgressChanged;

            int level = int.Parse(zoomlevel.Text);

            string[] x_ = x_coord.Text.Split('-');
            string[] y_ = y_coord.Text.Split('-');

            int x0 = int.Parse(x_[0]);
            int x1 = x_.Length == 2 ? int.Parse(x_[1]) : x0;
            int y0 = int.Parse(y_[0]);
            int y1 = y_.Length == 2 ? int.Parse(y_[1]) : y0;

            BatchProcessParam p = new BatchProcessParam(level, x0, x1, y0, y1);

            worker.RunWorkerAsync(p);
        }
    }
}