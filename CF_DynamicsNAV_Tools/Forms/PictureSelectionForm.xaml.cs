using System;
using System.Collections;
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
using System.ComponentModel;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// Interaktionslogik für PictureSelectionForm.xaml
    /// </summary>
    public partial class PictureSelectionForm : Window, INotifyPropertyChanged
    {
        int pictureSize = 100;
        public string FormTitle { get; set; }
        public int PictureSize { get { return pictureSize; } set { pictureSize = value; NotifyPropertyChanged("PictureSize"); } }

        public List<ObservableCollection<ImageEntry>> Pictures { get; set; }

        public PictureSelectionForm()
        {
            InitializeComponent();
        }

        public void InitPictures(SortedList<string, ImageEntry> pics)
        {
            List < ObservableCollection < ImageEntry >> pictures = new List<ObservableCollection<ImageEntry>>();

            List<ImageEntry> templist = new List<ImageEntry>();
            string lastgroup = "";

            foreach (ImageEntry item in pics.Values)
            {
                if (lastgroup!="" & lastgroup!=item.Grouping)
                {
                    pictures.Add(new ObservableCollection<ImageEntry>(templist));
                    templist.Clear();
                }
                templist.Add(item);
                lastgroup = item.Grouping;
            }
            if (templist.Count!=0)
            {
                pictures.Add(new ObservableCollection<ImageEntry>(templist));
            }

            Pictures = pictures;
        }
        public SortedList<string, ImageEntry> GetSelectedFiles()
        {
            SortedList<string, ImageEntry> result = new SortedList<string, ImageEntry>();
            foreach (var lists in Pictures)
            {
                foreach (var item in lists)
                {
                    result.Add(item.ID, item);
                }
            }
            
            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
