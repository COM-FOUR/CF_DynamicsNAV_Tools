using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// Tools for Up/Downloads 
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("121F018F-F173-41AA-BD75-8BEBB289C34D")]
    public class WebTools : IWebTools
    {
        internal string lastErrorMessage = "";
        internal NetworkCredential credentials = null;
        public WebTools() { }
        public string LastErrorMessage()
        {
            return lastErrorMessage;
        }
        public void SetCredentials(string username, string password)
        {
            credentials = new NetworkCredential(username, password);
        }
        public bool DownloadFile(string url, string filename, int timeout)
        {
            bool result = false;

            lastErrorMessage = "";

            WebClientTimeOut wc = new WebClientTimeOut();

            if (credentials != null)
                wc.Credentials = credentials;

            wc.TimeOut = timeout;
            try
            {
                wc.DownloadFile(url, filename);
                result = true;
            }
            catch (Exception e)
            {
                lastErrorMessage = e.Message;
            }

            return result;
        }

        public bool PostDownloadFile(string url, string filename)
        {
            bool result = false;

            try
            {
                WebClient wc = new WebClient();

                if (credentials != null)
                    wc.Credentials = credentials;

                string content = wc.UploadString(url, "POST", "");
                if (File.Exists(filename))
                    File.Delete(filename);

                File.AppendAllText(filename, content, System.Text.Encoding.Default);

                result = true;
            }
            catch (Exception e)
            {
                lastErrorMessage = e.Message;
            }

            return result;
        }

        public bool UploadFile(string filename, string url)
        {
            bool result = false;
            lastErrorMessage = "";

            WebClientTimeOut wc = new WebClientTimeOut();
            wc.TimeOut = 360000;

            if (credentials != null)
                wc.Credentials = credentials;
            try
            {
                wc.UploadFile(url, filename);
                result = true;
            }
            catch (Exception e)
            {
                lastErrorMessage = e.Message;
            }

            return result;
        }

        public bool CheckImageForBlackBorder(string filename)
        {
            bool result = false;

            int val = 75;

            lastErrorMessage = "";

            try
            {
                Bitmap img = new Bitmap(filename);
                System.Drawing.Color upperleft = img.GetPixel(0, 0);
                System.Drawing.Color upperright = img.GetPixel(img.Width - 1, 0);
                System.Drawing.Color lowerleft = img.GetPixel(0, img.Height - 1);
                System.Drawing.Color lowerright = img.GetPixel(img.Width - 1, img.Height - 1);

                result = (upperleft.R <= val && upperleft.G <= val && upperleft.B <= val) &&
                         (upperright.R <= val && upperright.G <= val && upperright.B <= val) &&
                         (lowerleft.R <= val && lowerleft.G <= val && lowerleft.B <= val) &&
                         (lowerright.R <= val && lowerright.G <= val && lowerright.B <= val);
                //result = upperleft.Name == "ff000000" & upperright.Name == "ff000000" & lowerleft.Name == "ff000000" & lowerright.Name == "ff000000";

                img.Dispose();
            }
            catch (Exception ex)
            {
                lastErrorMessage = ex.Message;
            }

            return result;
        }
    }

    /// <summary>
    /// Tools for interacting with the Filesystem
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("6B2DC1E9-9CD9-4DA5-9B95-736ABE2298AC")]
    public class FileTools : IFileTools
    {
        public void ConvertFile2DefaultEncoding(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
            string content = sr.ReadToEnd();
            sr.Close();

            File.Delete(fileName);

            StreamWriter sw = new StreamWriter(fileName, false, Encoding.Default);
            sw.WriteLine(content);
            sw.Close();
        }
        public string GetTemporaryThumbnail(string filepath, string filename, int pixelsize)
        {
            string result = "";

            if (pixelsize < 50)
                pixelsize = 50;

            string tofilename = string.Format("{0}\\{1}.bmp", Path.GetTempPath(), filename);

            if (File.Exists(tofilename))
            {
                result = tofilename;
            }
            else
            {
                try
                {
                    if (File.Exists(filepath))
                    {
                        Image bmp = new Bitmap(filepath).GetThumbnailImage(pixelsize, pixelsize, () => false, IntPtr.Zero);
                        bmp.Save(tofilename, System.Drawing.Imaging.ImageFormat.Bmp);
                        result = tofilename;
                    }
                }
                catch { }
            }

            return result;
        }

        public bool DirectoryExists(string folderpath)
        {
            return Directory.Exists(folderpath);
        }

        public bool CreateDirectory(string inpath)
        {
            bool result = false;

            try
            {
                Uri tempuri = new Uri(inpath);
                if (tempuri.IsFile)
                {
                    FileInfo fi = new FileInfo(tempuri.LocalPath);

                    if (!fi.Directory.Exists)
                        fi.Directory.Create();
                }
                else
                {
                    if (!Directory.Exists(inpath))
                        Directory.CreateDirectory(inpath);
                }

                result = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return result;
        }

        public string GetFileName(string inpath)
        {
            string result = "";

            try
            {
                Uri tempuri = new Uri(inpath);
                if (tempuri.IsFile)
                {
                    result = Path.GetFileName(tempuri.LocalPath);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return result;
        }
        public string GetDirectory(string inpath)
        {
            string result = "";

            try
            {
                Uri tempuri = new Uri(inpath);
                if (tempuri.IsFile)
                {
                    result = Path.GetDirectoryName(tempuri.LocalPath);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return result;
        }
    }
}
