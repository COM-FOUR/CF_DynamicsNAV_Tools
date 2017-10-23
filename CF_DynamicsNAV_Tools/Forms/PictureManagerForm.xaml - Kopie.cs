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
        private System.Windows.Point _startPoint;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragIsOutOfScope = false;
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
            foreach (var item in fileslistbox.Items)
            //foreach (var item in listbox.Items)
            {
                ImageEntry ie = (ImageEntry)item;

                if (ie.Selected)
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

        #region Drag&Drop filesBox

        private void fileslistbox_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }
       
        private void fileslistbox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                //MessageBox.Show(e.Data.GetData("myFormat").GetType().ToString());
                ImageEntry name = e.Data.GetData("myFormat") as ImageEntry;
                ListViewItem listViewItem = FindAncestorOrSelf<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    ImageEntry nameToReplace = (ImageEntry)fileslistbox.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    int removedIdx = (fileslistbox.ItemsSource as ObservableCollection<ImageEntry>).IndexOf(name);
                    int targetIdx = (fileslistbox.ItemsSource as ObservableCollection<ImageEntry>).IndexOf(nameToReplace);

                    (fileslistbox.ItemsSource as ObservableCollection<ImageEntry>).Move(removedIdx, targetIdx);
                }
                else
                {
                    (fileslistbox.ItemsSource as ObservableCollection<ImageEntry>).Remove(name);
                    (fileslistbox.ItemsSource as ObservableCollection<ImageEntry>).Add(name);
                }
            }
        }

        private void fileslistbox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    filesBeginDrag(e);
                }
            }
        }

        private void fileslistbox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void filesBeginDrag(MouseEventArgs e)
        {
            //ListView listView = this.listbox;
            //ListViewItem listViewItem =
            //    FindAncestorOrSelf<ListViewItem>((DependencyObject)e.OriginalSource);

            ListView listView = this.fileslistbox;
            ListViewItem listViewItem =
                FindAncestorOrSelf<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            // get the data for the ListViewItem
            ImageEntry name = (ImageEntry)fileslistbox.ItemContainerGenerator.ItemFromContainer(listViewItem);

            //setup the drag adorner.
            filesInitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            fileslistbox.PreviewDragOver += filesListViewDragOver;
            fileslistbox.DragLeave += filesListViewDragLeave;
            fileslistbox.DragEnter += filesListViewDragEnter;

            DataObject data = new DataObject("myFormat", name);
            DragDropEffects de = DragDrop.DoDragDrop(this.fileslistbox, data, DragDropEffects.Move);

            //cleanup
            fileslistbox.PreviewDragOver -= filesListViewDragOver;
            fileslistbox.DragLeave -= filesListViewDragLeave;
            fileslistbox.DragEnter -= filesListViewDragEnter;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
            }
        }

        private void filesListViewDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void filesListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                ImageEntry name = e.Data.GetData("myFormat") as ImageEntry;
                ListViewItem listViewItem = FindAncestorOrSelf<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    ImageEntry nameToReplace = (ImageEntry)fileslistbox.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    int index = fileslistbox.Items.IndexOf(nameToReplace);

                    if (index >= 0)
                    {
                        fileslistbox.Items.Remove(name);
                        fileslistbox.Items.Insert(index, name);
                    }
                }
                else
                {
                    fileslistbox.Items.Remove(name);
                    fileslistbox.Items.Add(name);
                }
            }
        }

        private void filesInitialiseAdorner(ListBoxItem listViewItem)
        {
            VisualBrush brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner((UIElement)listViewItem, listViewItem.RenderSize, brush);
            _adorner.Opacity = 0.5;
            _layer = AdornerLayer.GetAdornerLayer(fileslistbox as Visual);
            _layer.Add(_adorner);
        }

        private void ListViewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (this._dragIsOutOfScope)
            {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
        }

        private void filesListViewDragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == fileslistbox)
            {
                System.Windows.Point point = e.GetPosition(fileslistbox);
                Rect rect = VisualTreeHelper.GetContentBounds(fileslistbox);
               
                //Check if within range of list view.
                if (!rect.Contains(point))
                {
                    this._dragIsOutOfScope = true;
                    e.Handled = true;
                }
            }
        }

        void filesListViewDragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = args.GetPosition(fileslistbox).X;
                _adorner.OffsetTop = args.GetPosition(fileslistbox).Y - _startPoint.Y;
            }
        }
        private void fileslistbox_DragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == fileslistbox)
            {
                System.Windows.Point point = e.GetPosition(fileslistbox);
                Rect rect = VisualTreeHelper.GetContentBounds(fileslistbox);


                //Check if within range of list view.
                if (!rect.Contains(point))
                {
                    this._dragIsOutOfScope = true;
                    e.Handled = true;
                }
            }
        }

        #endregion
        
        #region Drag&Drop selectionBox
        private void selectionlistbox_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void selectionlistbox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                //MessageBox.Show(e.Data.GetData("myFormat").GetType().ToString());
                ImageEntry name = e.Data.GetData("myFormat") as ImageEntry;
                ListViewItem listViewItem = FindAncestorOrSelf<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    ImageEntry nameToReplace = (ImageEntry)selectionlistbox.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    int removedIdx = (selectionlistbox.ItemsSource as ObservableCollection<ImageEntry>).IndexOf(name);
                    int targetIdx = (selectionlistbox.ItemsSource as ObservableCollection<ImageEntry>).IndexOf(nameToReplace);

                    (selectionlistbox.ItemsSource as ObservableCollection<ImageEntry>).Move(removedIdx, targetIdx);
                }
                else
                {
                    (selectionlistbox.ItemsSource as ObservableCollection<ImageEntry>).Remove(name);
                    (selectionlistbox.ItemsSource as ObservableCollection<ImageEntry>).Add(name);
                }
            }
        }

        private void selectionlistbox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    selectionBeginDrag(e);
                }
            }
        }

        private void selectionlistbox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void selectionBeginDrag(MouseEventArgs e)
        {
            //ListView listView = this.listbox;
            //ListViewItem listViewItem =
            //    FindAncestorOrSelf<ListViewItem>((DependencyObject)e.OriginalSource);

            ListView listView = this.selectionlistbox;
            ListViewItem listViewItem =
                FindAncestorOrSelf<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            // get the data for the ListViewItem
            ImageEntry name = (ImageEntry)selectionlistbox.ItemContainerGenerator.ItemFromContainer(listViewItem);

            //setup the drag adorner.
            selectionInitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            fileslistbox.PreviewDragOver += selectionListViewDragOver;
            fileslistbox.DragLeave += selectionListViewDragLeave;
            fileslistbox.DragEnter += selectionListViewDragEnter;

            DataObject data = new DataObject("myFormat", name);
            DragDropEffects de = DragDrop.DoDragDrop(this.fileslistbox, data, DragDropEffects.Move);

            //cleanup
            fileslistbox.PreviewDragOver -= selectionListViewDragOver;
            fileslistbox.DragLeave -= selectionListViewDragLeave;
            fileslistbox.DragEnter -= selectionListViewDragEnter;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
            }
        }

        private void selectionListViewDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void selectionListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                ImageEntry name = e.Data.GetData("myFormat") as ImageEntry;
                ListViewItem listViewItem = FindAncestorOrSelf<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    ImageEntry nameToReplace = (ImageEntry)selectionlistbox.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    int index = selectionlistbox.Items.IndexOf(nameToReplace);

                    if (index >= 0)
                    {
                        selectionlistbox.Items.Remove(name);
                        selectionlistbox.Items.Insert(index, name);
                    }
                }
                else
                {
                    selectionlistbox.Items.Remove(name);
                    selectionlistbox.Items.Add(name);
                }
            }
        }

        private void selectionInitialiseAdorner(ListBoxItem listViewItem)
        {
            VisualBrush brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner((UIElement)listViewItem, listViewItem.RenderSize, brush);
            _adorner.Opacity = 0.5;
            _layer = AdornerLayer.GetAdornerLayer(selectionlistbox as Visual);
            _layer.Add(_adorner);
        }

        private void selectionListViewDragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == selectionlistbox)
            {
                System.Windows.Point point = e.GetPosition(selectionlistbox);
                Rect rect = VisualTreeHelper.GetContentBounds(selectionlistbox);

                //Check if within range of list view.
                if (!rect.Contains(point))
                {
                    this._dragIsOutOfScope = true;
                    e.Handled = true;
                }
            }
        }

        void selectionListViewDragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = args.GetPosition(selectionlistbox).X;
                _adorner.OffsetTop = args.GetPosition(selectionlistbox).Y - _startPoint.Y;
            }
        }
        private void selectionlistbox_DragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == selectionlistbox)
            {
                System.Windows.Point point = e.GetPosition(selectionlistbox);
                Rect rect = VisualTreeHelper.GetContentBounds(selectionlistbox);


                //Check if within range of list view.
                if (!rect.Contains(point))
                {
                    this._dragIsOutOfScope = true;
                    e.Handled = true;
                }
            }
        }
        #endregion

        public static T FindAncestorOrSelf<T>(DependencyObject dependencyObject)
                where T : class
        {
            DependencyObject target = dependencyObject;
            do
            {
                target = VisualTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));
            return target as T;
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