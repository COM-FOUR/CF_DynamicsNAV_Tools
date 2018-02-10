using System;
using System.Windows.Input;
using System.ComponentModel;
using System.IO;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// DataModel for picture selection
    /// </summary>
    public class ImageEntry : INotifyPropertyChanged
    {
        string id = "";
        string grouping = "";
        string fileName = "";
        string fileNameInclPath = "";
        bool selected;
        string backGroundColor;

        ICommand toggleCommand;

        public string ID { get { return id; } set { id = value; NotifyPropertyChanged("id"); } }
        public string Grouping { get { return grouping; } set { grouping = value; NotifyPropertyChanged("Grouping"); } }
        public string FileName { get { return fileName; } set { fileName = value; NotifyPropertyChanged("FileName"); } }
        public string FileNameInclPath { get { return fileNameInclPath; } set { fileNameInclPath = value; NotifyPropertyChanged("FileNameInclPath"); } }
        public bool Selected { get { return selected; } set { selected = value; NotifyPropertyChanged("Selected"); } }
        public string BackGroundColor { get { return backGroundColor; } set { backGroundColor = value; NotifyPropertyChanged("BackGroundColor"); } }
        public ICommand ToggleCommand
        {
            get
            {
                if (toggleCommand == null)
                {
                    toggleCommand = new RelayCommand(param => this.ToggleSelected(),
                        param => this.fileName != "");
                }
                return toggleCommand;
            }
        }

        public ImageEntry(string filename)
            : this(new FileInfo(filename)) { }
        public ImageEntry(FileInfo fi)
        {
            fileName = fi.Name;
            fileNameInclPath = fi.FullName;
            selected = false;
            backGroundColor = "LightGray";
        }

        public ImageEntry(string id, string group, string uri, bool selection)
        {
            ID = id;
            Grouping = group;

            Uri u = new Uri(uri);

            FileInfo fi = new FileInfo(u.LocalPath);
            fileName = fi.Name;
            fileNameInclPath = u.AbsoluteUri;
            ToggleSelected(selection);
        }

        void ToggleSelected()
        {
            ToggleSelected(!selected);
        }
        public void ToggleSelected(bool selection)
        {
            if (selection)
            {
                Selected = true;
                BackGroundColor = "LightGreen";
            }
            else
            {
                Selected = false;
                BackGroundColor = "LightGray";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
