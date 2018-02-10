using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// external Window for Selection of Pictures
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("6651762B-8364-49A4-BE7E-9EA79FDAF80B")]
    public class PictureSelectionManager : IPictureSelectionManager
    {
        public PictureSelectionManager()
        {
            formwidth = 600;
            formheight = 570;
        }
        string formTitle { get; set; }
        bool formVisible { get; set; }
        bool formCanceled { get; set; }
        int picSize { get; set; }
        double formwidth { get; set; }
        double formheight { get; set; }

        SortedList<string, ImageEntry> pictures = new SortedList<string, ImageEntry>();

        public void Title(string title)
        {
            formTitle = title;
        }
        public void PictureSize(int newsize)
        {
            picSize = newsize;
        }

        public void FormSize(double width, double height)
        {
            formwidth = width;
            formheight = height;
        }

        public void Show()
        {
            PictureSelectionForm psf = new PictureSelectionForm();

            psf.FormTitle = formTitle;
            formVisible = true;

            psf.InitPictures(pictures);

            if (picSize != 0)
                psf.PictureSize = picSize;
            if (formwidth != 0)
                psf.Width = formwidth;
            if (formheight != 0)
                psf.Height = formheight;

            psf.ShowDialog();

            if (psf.DialogResult.HasValue && psf.DialogResult.Value)
            {
                pictures = psf.GetSelectedFiles();

                picSize = psf.PictureSize;
                formheight = psf.Height;
                formwidth = psf.Width;

                formCanceled = false;
            }
            else
            {
                formCanceled = true;
            }

            formVisible = false;

        }
        public bool Visible()
        {
            return formVisible;
        }
        public bool Cancel()
        {
            return formCanceled;
        }
        public void AddPicture(string id, string group, string newpic, bool selected)
        {
            pictures.Add(id, new ImageEntry(id, group, newpic, selected));
        }
        public bool GetPictureSelected(string id)
        {
            bool result = false;
            if (pictures.ContainsKey(id))
            {
                result = pictures[id].Selected;
            }

            return result;
        }

        public double GetFormSize(int which)
        {
            double result = 0;
            switch (which)
            {
                case 1: result = formwidth; break;
                case 2: result = formheight; break;
            }
            return result; ;
        }

        public int GetPictureSize()
        {
            return picSize;
        }
    }
}
