using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// variuos Helper classes
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("74F845DD-4F78-43C9-B4B2-A1E5172824A1")]
    public class MiscTools : IMiscTools
    {
        public MiscTools() { }
        public static bool IsRecognisedImageFile(string fileName)
        {
            string targetExtension = System.IO.Path.GetExtension(fileName);
            if (String.IsNullOrEmpty(targetExtension))
                return false;
            else
                targetExtension = "*" + targetExtension.ToLowerInvariant();

            List<string> recognisedImageExtensions = new List<string>();

            foreach (System.Drawing.Imaging.ImageCodecInfo imageCodec in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
                recognisedImageExtensions.AddRange(imageCodec.FilenameExtension.ToLowerInvariant().Split(";".ToCharArray()));

            //bool targetFound = false;
            foreach (string extension in recognisedImageExtensions)
            {
                if (extension.Equals(targetExtension))
                {
                    return true;
                }
            }
            return false;
        }
        public bool ConvertFileToASCII(string fileName)
        {
            if (!File.Exists(fileName))
                return false;

            bool result = false;

            StreamReader sr = new StreamReader(fileName);

            byte[] content = sr.CurrentEncoding.GetBytes(sr.ReadToEnd());

            sr.Close();

            string asciicontent = Encoding.Default.GetString(content);

            File.Delete(fileName);

            File.AppendAllText(fileName, asciicontent, Encoding.Default);

            return result;
        }

        public string GetTextFromXMLDOMNode(ref object node, int maxLength)
        {
            string shortText = ((MSXML2.IXMLDOMNode)node).text;
            if (shortText.Length > maxLength)
            {
                shortText = shortText.Substring(0, maxLength);
            }
            return shortText;
        }
    }
}
