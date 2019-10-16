using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// external Window for Selection of Pictures in Filesystem
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("1DA231FC-87AA-430E-9795-6D224882A392")]
    public class PictureUploadManager : IPictureUploadManager
    {
        public PictureUploadManager() { }
        string formTitle { get; set; }
        bool formVisible { get; set; }
        bool formCanceled { get; set; }
        string lastSelectedFolder { get; set; }

        List<string> pictures = new List<string>();
        List<string> favorites = new List<string>();
        //string workingDirectoryPath { get; set; }

        public void Title(string title)
        {
            formTitle = title;
        }
        public void Show()
        {

            PictureManagerForm pmf = new PictureManagerForm();

            pmf.FormTitle = formTitle;
            formVisible = true;

            if (favorites != null & favorites.Count != 0)
            {
                pmf.InitFolder = favorites.ToArray();
            }
            pmf.ShowDialog();

            if (pmf.DialogResult.HasValue && pmf.DialogResult.Value)
            {
                pictures = pmf.GetSelectedFiles();
                lastSelectedFolder = pmf.LastSelectedPath;
                favorites = pmf.favorites;

                formCanceled = false;
            }
            else
            {
                formCanceled = true;
            }

            formVisible = false;

            pmf = new PictureManagerForm();
        }
        public bool Visible()
        {
            return formVisible;
        }
        public bool Cancel()
        {
            return formCanceled;
        }
        public int Pictures()
        {
            return pictures.Count;
        }
        public void AddFavoriteFolder(string path)
        {
            if (path != null & path != "")
                favorites.Add(path);
        }
        //public void SetWorkingDirectory(string path)
        //{
        //    workingDirectoryPath = path;
        //}
        public void AddPicture(string newpic)
        {
            pictures.Add(newpic);
        }
        public string GetPicture(int picno)
        {
            string result = "";
            if (pictures.Count >= picno)
            {
                result = pictures[picno - 1];
            }

            return result;
        }

        public string GetLastSelectedFolder()
        {
            if (lastSelectedFolder == null)
                lastSelectedFolder = "";

            return lastSelectedFolder;
        }
        public int Favorites()
        {
            return favorites.Count;
        }
        public string GetFavorite(int favno)
        {
            string result = "";
            if (favorites.Count >= favno)
            {
                result = favorites[favno - 1];
            }

            return result;
        }
    }
}
