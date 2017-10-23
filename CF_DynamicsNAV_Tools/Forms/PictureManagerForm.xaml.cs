using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing;
using System.IO;
using ExplorerTreeView.Controls;
using System.ComponentModel;
using CF_DynamicsNAV_Tools.MiscClasses;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// Interaktionslogik für PictureManagerForm.xaml
    /// </summary>
    public partial class PictureManagerForm : Window, INotifyPropertyChanged
    {
        private System.Windows.Point startPoint;
        //private DragAdorner _adorner;
        //private AdornerLayer _layer;
        //private bool _dragIsOutOfScope = false;
        string selectedPath;
        string lastSelectedPath;
        public List<string> favorites = new List<string>();
        string heartColor = "Black";

        int pictureSize = 100;

        public int PictureSize { get { return pictureSize; } set { pictureSize = value; NotifyPropertyChanged("PictureSize"); } }
        public string HeartColor { get { return heartColor; } set { heartColor = value; NotifyPropertyChanged("HeartColor"); } }

        public string SelectedPathVar
        {
            get { return selectedPath; }
            set
            {
                selectedPath = value;
                if (selectedPath.Substring(0, 2) == "CF")
                {
                    string temppath = @"Y:\Bilderarchiv_Schomann_Roch\COM_FOUR Artikel\" + selectedPath;
                    if (Directory.Exists(temppath))
                    {
                        selectedPath = temppath;
                    }
                }
                NotifyPropertyChanged("SelectedPathVar");
                UpdateHeartColor();
            }
        }
        public string LastSelectedPath { get { return lastSelectedPath; } set { lastSelectedPath = value; } }
        public string FormTitle { get; set; }

        public string[] InitFolder { get; set; }

        public PictureManagerForm()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            if (InitFolder != null && InitFolder.Length != 0)
            {
                explorer.InitPath = InitFolder;
                Extensions.Refresh(explorer);

                foreach (var item in InitFolder)
                {
                    favorites.Add(item.ToUpper());
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        private void explorer_ExplorerError(object sender, ExplorerErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
        public List<string> GetSelectedFiles()
        {
            List<string> result = new List<string>();

            if (explorer.SelectedPath == "")
                return result;

            //DirectoryInfo dirinfo = new DirectoryInfo(explorer.SelectedPath);
            foreach (var item in selectionlistbox.Items)
            //foreach (var item in listbox.Items)
            {
                ImageEntry ie = (ImageEntry)item;

                 //if (ie.Selected)
                    result.Add(ie.FileNameInclPath);
            }

            return result;
        }
        private void explorer_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (explorer.SelectedPath == null | explorer.SelectedPath == "")
            {
                return;
            }

            //listbox.Items.Clear();
            //List<FileInfo> files = new List<FileInfo>();
            List<ImageEntry> files = new List<ImageEntry>();
            DirectoryInfo dirinfo = new DirectoryInfo(explorer.SelectedPath);

            if (dirinfo == null)
            {
                return;
            }

            FileInfo[] fieinfos = dirinfo.GetFiles();

            if (fieinfos != null)
            {
                foreach (var item in fieinfos)
                {
                    if (MiscTools.IsRecognisedImageFile(item.FullName))
                    {
                        //files.Add(item);
                        files.Add(new ImageEntry(item));
                    }
                }
            }
            fileslistbox.ItemsSource = new ObservableCollection<ImageEntry>(files);
        }

        void UpdateHeartColor()
        {
            if (selectedPath != "" & selectedPath != null)
                lastSelectedPath = selectedPath;

            if (favorites.Contains(selectedPath.ToUpper()))
                heartColor = "Red";
            else
                heartColor = "Gray";

            NotifyPropertyChanged("HeartColor");
        }

       

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (selectedPath == "" | selectedPath == null)
            {
                return;
            }

            if (favorites.Contains(selectedPath.ToUpper()))
            {
                favorites.Remove(selectedPath.ToUpper());
            }
            else
            {
                favorites.Add(selectedPath.ToUpper());
            }
            UpdateHeartColor();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            explorer.SetSelectedPath(SelectedPathVar);
            NotifyPropertyChanged("SelectedPathVar");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void fileslistbox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void fileslistbox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            System.Windows.Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                )
            {
                // Get the dragged ListViewItem
                ListView listView = sender as ListView;
                ListViewItem listViewItem =
                    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    // Find the data behind the ListViewItem
                    ImageEntry ie = (ImageEntry)listView.ItemContainerGenerator.
                        ItemFromContainer(listViewItem);

                    // Initialize the drag & drop operation
                    DataObject dragData = new DataObject("myFormat", ie);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy);
                }
            } 
        }

        private void selectionlistbox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                ImageEntry ie = e.Data.GetData("myFormat") as ImageEntry;
                ListView listView = sender as ListView;
                if (!listView.Items.Contains(ie))
                {
                    ie.ToggleSelected(true);
                    listView.Items.Add(ie); 
                }
            }
        }

        private void selectionlistbox_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem =
                    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem!=null)
            {
                (listViewItem.Content as ImageEntry).ToggleSelected(false);
                this.selectionlistbox.Items.Remove(listViewItem.Content);
                
            }
        }
    }
    static class Extensions
    {
        private static Action EmptyDelegate = delegate() { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

}