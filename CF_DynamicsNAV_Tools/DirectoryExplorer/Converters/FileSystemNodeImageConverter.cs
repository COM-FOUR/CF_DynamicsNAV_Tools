using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ExplorerTreeView.Converters
{
    public class FileSystemNodeImageConverter: IValueConverter
    {
        public ImageSource DriveImage
        {
            get; set;
        }

        public ImageSource DirectoryImage
        {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeViewItem)
            {
                var node = value as TreeViewItem;
                if (node.Tag is DriveInfo)
                {
                    return DriveImage;
                }

                if (node.Tag is DirectoryInfo)
                {
                    return DirectoryImage;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
