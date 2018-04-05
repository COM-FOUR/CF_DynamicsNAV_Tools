using System;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Drawing;

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
}
