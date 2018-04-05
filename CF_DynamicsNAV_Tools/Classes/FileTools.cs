using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace CF_DynamicsNAV_Tools
{
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
