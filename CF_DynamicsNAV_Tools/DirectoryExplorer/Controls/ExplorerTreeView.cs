using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace ExplorerTreeView.Controls
{
    /// <summary>
    /// This control displays a file system tree.
    /// </summary>
    public class ExplorerTreeView : TreeView
    {
        #region Dependency properties
        /// <summary>
        /// The <see cref="SelectedPath" /> dependency property's name.
        /// </summary>
        public const string SelectedPathPropertyName = "SelectedPath";

        /// <summary>
        /// Gets or sets the value of the <see cref="SelectedPath" />
        /// property. This is a dependency property.
        /// </summary>
        public String SelectedPath
        {
            get
            {
                return (String)GetValue(SelectedPathProperty);
            }
            set
            {
                SetValue(SelectedPathProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="SelectedPath" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(
            SelectedPathPropertyName,
            typeof(String),
            typeof(ExplorerTreeView));

        /// <summary>
        /// The <see cref="InitPath" /> dependency property's name.
        /// </summary>
        public const string InitPathPropertyName = "InitPath";

        /// <summary>
        /// Gets or sets the value of the <see cref="InitPath" />
        /// property. This is a dependency property.
        /// </summary>
        public String[] InitPath
        {
            get
            {
                return (String[])GetValue(InitPathProperty);
            }
            set
            {
                SetValue(InitPathProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="InitPath" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty InitPathProperty = DependencyProperty.Register(
            InitPathPropertyName,
            typeof(String[]),
            typeof(ExplorerTreeView));

        /// <summary>
        /// The <see cref="UnloadItemsOnCollapse" /> dependency property's name.
        /// </summary>
        public const string UnloadItemsOnCollapsePropertyName = "UnloadItemsOnCollapse";

        /// <summary>
        /// Gets or sets the value of the <see cref="UnloadItemsOnCollapse" />
        /// property. This is a dependency property.
        /// </summary>
        public bool UnloadItemsOnCollapse
        {
            get
            {
                return (bool)GetValue(UnloadItemsOnCollapseProperty);
            }
            set
            {
                SetValue(UnloadItemsOnCollapseProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="UnloadItemsOnCollapse" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty UnloadItemsOnCollapseProperty = DependencyProperty.Register(
            UnloadItemsOnCollapsePropertyName,
            typeof(bool),
            typeof(ExplorerTreeView),
            new UIPropertyMetadata(true));
        #endregion

        /// <summary>
        /// This event is raised if error occurs while creating file system tree.
        /// </summary>
        public event EventHandler<ExplorerErrorEventArgs> ExplorerError;

        /// <summary>
        /// Invocator for <see cref="ExplorerError"/> event.
        /// </summary>
        /// <param name="e"></param>
        private void InvokeExplorerError(ExplorerErrorEventArgs e)
        {
            EventHandler<ExplorerErrorEventArgs> handler = ExplorerError;
            if (handler != null) handler(this, e);
        }

        public ExplorerTreeView()
        {
            Loaded += (s, e) => InitExplorer();
            
            SelectedItemChanged += OnSelectedItemChanged;

            AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnItemExpanded));
            AddHandler(TreeViewItem.CollapsedEvent, new RoutedEventHandler(OnItemCollapsed));
        }

        /// <summary>
        /// This method is invoked when user selects a node.
        /// It causes <see cref="SelectedPath"/> to update its value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            SelectedPath = GetSelectedPath();
        }

        /// <summary>
        /// Occurs when tree node is expanded.
        /// Reloads node sub-folders, if required.
        /// May raise <see cref="ExplorerError"/> on some IO exceptions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemExpanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)e.OriginalSource;

            if (UnloadItemsOnCollapse || !HasSubFolders(item))
            {
                item.Items.Clear();
                DirectoryInfo dir;
                if (item.Tag is DriveInfo)
                {
                    var drive = (DriveInfo)item.Tag;
                    dir = drive.RootDirectory;
                }
                else
                {
                    dir = (DirectoryInfo)item.Tag;
                }

                try
                {
                    foreach (var subDir in dir.GetDirectories().OrderBy(f => f.Name))
                    {
                        item.Items.Add(GenerateDirectoryNode(subDir));
                    }
                }
                catch (Exception ex)
                {
                    InvokeExplorerError(new ExplorerErrorEventArgs(ex));
                }
            }
        }

        /// <summary>
        /// Occurs when tree node is collapsed.
        /// Unloads node sub-folders, if <see cref="UnloadItemsOnCollapse"/> is set to True.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemCollapsed(object sender, RoutedEventArgs e)
        {
            if (UnloadItemsOnCollapse)
            {
                var item = (TreeViewItem)e.OriginalSource;
                item.Items.Clear();
                item.Items.Add("*");
            }
        }

        /// <summary>
        /// Checks whether specified <see cref="TreeViewItem"/> has any real sub-folder nodes.
        /// </summary>
        /// <param name="item">Node to check.</param>
        /// <returns></returns>
        private static bool HasSubFolders(TreeViewItem item)
        {
            if (item.Items.Count == 0)
            {
                return false;
            }

            var firstItem = item.Items[0] as TreeViewItem;
            return firstItem != null;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == SelectedPathProperty)
            {
                var newValue = (String)e.NewValue;
                if (IsSelectionUpdateRequired(newValue))
                {
                    SetSelectedPath(newValue);
                }
            }
        }

        /// <summary>
        /// Compares old <see cref="SelectedPath"/> value with the specified one 
        /// and desides whether tree view selection has to be updated or not, if you apply this new value.
        /// </summary>
        /// <param name="newPath">New <see cref="SelectedPath"/> value.</param>
        /// <returns></returns>
        private bool IsSelectionUpdateRequired(String newPath)
        {
            if (String.IsNullOrEmpty(newPath))
            {
                return true;
            }

            var selectedPath = GetSelectedPath();
            if (String.IsNullOrEmpty(selectedPath))
            {
                return true;
            }

            return !Path.GetFullPath(newPath).Equals(Path.GetFullPath(selectedPath));
        }

        /// <summary>
        /// Generates <see cref="TreeViewItem"/> for directory info.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private static TreeViewItem GenerateDirectoryNode(DirectoryInfo directory)
        {
            var item = new TreeViewItem
            {
                Tag = directory,
                Header = directory.Name
            };
            item.Items.Add("*");

            return item;
        }

        /// <summary>
        /// Generates <see cref="TreeViewItem"/> for drive info.
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        private static TreeViewItem GenerateDriveNode(DriveInfo drive)
        {
            var item = new TreeViewItem
            {
                Tag = drive,
                Header = drive.ToString()
            };

            item.Items.Add("*");
            return item;
        }

        /// <summary>
        /// Populates tree with initial drive nodes. 
        /// </summary>
        public void InitExplorer()
        {
            Items.Clear();
            
            string dtpath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (Directory.Exists(dtpath))
                Items.Add(GenerateDirectoryNode(new DirectoryInfo(dtpath)));

            if (InitPath != null && InitPath.Length != 0)
            {
                foreach (var item in InitPath)
                {
                    if (Directory.Exists(item))
                        Items.Add(GenerateDirectoryNode(new DirectoryInfo(item)));
                }
            }

            foreach (var drive in DriveInfo.GetDrives())
            {
                Items.Add(GenerateDriveNode(drive));
            }
        }

        /// <summary>
        /// Updates tree view selection, so that it corresponds to specified <see cref="SelectedPath"/> value.
        /// </summary>
        /// <param name="value"></param>
        public void SetSelectedPath(String value)
        {
            //For now, there are some issues with correct item (un)selection,
            //cause item that was previously selected, becomes selected again after this code, that selects a new item.
            //It may be related somehow to TreeViewItem.IsSelected behavior.

            //Re-creating items on each SelectedPath = XXX expression (or binding evaluations) 
            //POTENTIALLY may cause lags at some cases (when getting DriveInfo of DirectoryInfo is slow, e.g. if you are trying to access network drives/folders).
            //NOTE: selecting items through UI doesn't invoke this method.
            //So when you have explorer tree and address bar in your application and user mainly iteracts with the tree,
            //while specifying full folder path is not a common task, there should be no problems at all.

            //If it is not possible to avoid this issue, some sort of dictionary, mapping or backing storage can be used
            //to store information about loaded DriveInfo or FolderInfo, so that you will not have to iterate through the entire tree to select an item.
            InitExplorer();

            if (String.IsNullOrEmpty(value))
            {
                return;
            }

            var split =
                Path.GetFullPath(value).Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries);
            var drive = new DriveInfo(split[0]);

            Action<TreeViewItem, int> expand = null;
            expand = (item, index) =>
            {
                if (index > split.Length)
                {
                    return;
                }

                if (index == split.Length)
                {
                    item.IsSelected = true;
                    return;
                }

                if (!item.IsExpanded)
                {
                    item.IsExpanded = true;
                }

                var name = split[index].ToLower();

                foreach (TreeViewItem folderItem in item.Items)
                {
                    var directoryInfo = (DirectoryInfo)folderItem.Tag;
                    if (directoryInfo.Name.ToLower() == name)
                    {
                        // ReSharper disable PossibleNullReferenceException
                        // ReSharper disable AccessToModifiedClosure
                        expand(folderItem, index + 1);
                        // ReSharper restore AccessToModifiedClosure
                        // ReSharper restore PossibleNullReferenceException
                        break;
                    }
                }
            };

            foreach (TreeViewItem driveItem in Items)
            {
                if (driveItem.Tag.GetType() == typeof(DriveInfo))
                {
                    var name = ((DriveInfo)driveItem.Tag).Name.ToLower();
                    if (name == drive.Name.ToLower())
                    {
                        expand(driveItem, 1);
                        break;
                    }    
                }
                
            }
        }

        /// <summary>
        /// Returns full path of the selected node.
        /// </summary>
        /// <returns></returns>
        private String GetSelectedPath()
        {
            var item = (TreeViewItem)SelectedItem;
            if (item == null)
            {
                return null;
            }

            if (item.Tag is DriveInfo)
            {
                return ((DriveInfo)item.Tag).RootDirectory.FullName;
            }

            if (item.Tag is DirectoryInfo)
            {
                return ((DirectoryInfo)item.Tag).FullName;
            }

            return null;
        }
    }
}