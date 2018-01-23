using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Drawing.Printing;
using System.Web;
using Com.SharpZebra;

namespace CF_DynamicsNAV_Tools
{
    #region Interfaces

    /// <summary>
    /// Tools for Amazon-Integration into Dynamics NAV
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("FA77BFAD-3875-4A02-B9B2-B1BF7A5AE4AD")]
    public interface IAmazonTools
    {
        /// <summary>
        /// sorting string by BigEndian, for calculating AmazonMWS signature
        /// </summary>
        /// <param name="inStr">string to be sorted</param>
        /// <returns>sorted string</returns>
        string GetByteOrder(string InStr);

        /// <summary>
        /// calculating the hash value for AmazonMWS signature
        /// </summary>
        /// <param name="key">secretkey for building the hash</param>
        /// <param name="text">string for the hash to be calculated</param>
        /// <returns>hash string</returns>
        string GetAmazonSHA256Hash(string key, string text);
        string GetUTCTimeString(string dtnow);
        string URLEncode(string toEncode);

        string GetRequestURL();
        void SetRequestURL(string intext);
        void AddToRequestURL(string intext);

        string GetSignature();
        void SetSignature(string intext);
        void AddRightToSignature(string intext);
        void AddLeftToSignature(string intext);

        void InitSignature();
        void ReplaceInSignature(string fromtext, string totext);
        void SortSignature();
        void SignatureToHash(string key);

        string RequestPart(int entryno);
        int CreateRequestParts();

        void SetXMLHTTP(MSXML2.ServerXMLHTTP60 xml);
        MSXML2.ServerXMLHTTP60 GetXMLHTTP();
        
        bool DoOpen(string method, string url, bool async, string user, string password);
        bool DoSend(string body);
        void ShowXMLHTTPResponseText(MSXML2.ServerXMLHTTP60 xml);
    }

    /// <summary>
    /// Tools for MeinPaket-Integration into Dynamics NAV
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("04422467-0FBA-4A1D-B26E-8EA2C7ABEAC6")]
    public interface IMeinPaketTools
    {
        void AppendText(string text);
        bool ShowDialog();
        int SplitTextToParts(int partlength);
        string GetSelectedHTMLText(int ptr);
    }

    /// <summary>
    /// variuos Helper classes
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("D7D5A3A2-65B6-471D-AB95-832619F79639")]
    public interface IMiscTools
    {
        bool ConvertFileToASCII(string fileName);

        string GetTextFromXMLDOMNode(ref object node, int maxLength);
    }

    /// <summary>
    /// external TextEditor for Integration in Dynamics NAV
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("D6016BFC-69B8-4174-B497-8D9500710489")]
    public interface ITextEditor
    {
        void Title(string title);
        void Language(int language);
        void MaxLineLen(int linelength);
        void AddLine(string newline);
        string GetLine(int lineno);
        //void Show();
        bool Visible();
        bool Cancel();
        int Lines();
        void ShowWPF();
        void SetDownloadURL(string url);
        bool UpdateBrandText();
        void ResetInitLines();
        void AddAdditionalLine(string newline);
    }

    /// <summary>
    /// external Window for Selection of Pictures in Filesystem
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("9B2C7EFC-CD33-4BB1-A884-A49CEC1388BB")]
    public interface IPictureUploadManager
    {
        void Title(string title);
        void Show();
        bool Visible();
        bool Cancel();
        int Pictures();
        void AddFavoriteFolder(string path);
        //void SetWorkingDirectory(string path);
        void AddPicture(string newpic);
        string GetPicture(int picno);
        string GetLastSelectedFolder();
        int Favorites();
        string GetFavorite(int favno);
    }

    /// <summary>
    /// external Window for Selection of Pictures
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("450BD7C0-4775-4666-AAB0-2E71D27DBA4C")]
    public interface IPictureSelectionManager
    {
        void Title(string title);
        void Show();
        bool Visible();
        bool Cancel();
        //int Pictures();
        //void AddInitDirectory(string path);
        //void SetWorkingDirectory(string path);
        void AddPicture(string id, string group, string newpic, bool selected);
        bool GetPictureSelected(string id);

        void PictureSize(int newsize);
        void FormSize(double width, double height);

        double GetFormSize(int which);
        int GetPictureSize();
    }

    /// <summary>
    /// Tools for Up/Downloads 
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("177D0362-CBE7-43B4-960C-AA55CC83F905")]
    public interface IWebTools
    {
        string LastErrorMessage();
        void SetCredentials(string username, string password);
        bool DownloadFile(string url, string filename, int timeout);
        bool UploadFile(string filename, string url);
        bool PostDownloadFile(string url, string filename);
        bool CheckImageForBlackBorder(string filename);
    }

    /// <summary>
    /// Tools for interacting with the Filesystem
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("33574C0A-BDC7-4111-AB72-37ABEC2BDCB5")]
    public interface IFileTools
    {
        void ConvertFile2DefaultEncoding(string fileName);
        string GetTemporaryThumbnail(string filepath, string filename,int pixelsize);
        bool DirectoryExists(string folderpath);
        bool CreateDirectory(string inpath);
        string GetFileName(string inpath);
        string GetDirectory(string inpath);
    }

    /// <summary>
    /// (deprecated)Tools for retrieving Amazon related Fees, (deprecated)
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("35A61A12-58E9-4253-A916-788BC6FB48D9")]
    public interface IFBAFees
    {
        void Initialize(string language, string marketPlaceId, string currency);
        int GetFees(string asin, double price);
        double GetFee(string feename);
        string ErrorMessage();
        bool GetDimensions(string asin, ref double length, ref double width, ref double height, ref double weight);
    }

    /// <summary>
    /// Tools for Printing Labels for integration into DynamicsNAV 
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("506399AC-E2C1-48B0-A967-341AFF971BBD")]
    public interface iLabelPrinting
    {
        void Init(string labelFormat, string printerName, string zplprinterName, int xOffset, int yOffset);
        void AddToBase64String(string textPart);
        void ClearBase64String();
        string GetLastErrorMessage();
        bool PrintLabel(bool directPrinting);
        bool PrintQueue();
        void EnqueueBase64String(string labelFormat);
        bool PrintZPLCodeToFile(string fileName);
        int GetZPLLines();
        string GetZPLLine(int key, ref string addStr);
        bool ImageLabelToFile(string fileName);
        void EnqueueBase64String2(string labelFormat, string addContent);
        void SetMargins(int left, int right, int top, int bottom);
    }
    #endregion

    #region Classes
    /// <summary>
    /// Tools for Amazon-Integration into Dynamics NAV
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("907E2820-81F4-45E0-B151-8311453ED37D")]
    public class AmazonTools : IAmazonTools
    {
        string requestURL;
        string signature;

        string[] requestparts;
        MSXML2.ServerXMLHTTP60 xmlhttp;

        public AmazonTools()
        {
            requestURL = "";
            signature = "";
            //requestparts = new string[1];
        }

        /// <summary>
        /// sorting string by BigEndian, for calculating AmazonMWS signature
        /// </summary>
        /// <param name="inStr">string to be sorted</param>
        /// <returns>sorted string</returns>
        public string GetByteOrder(string inStr)
        {

            ParamComparer pc = new ParamComparer();
            SortedDictionary<string, string> slist = new SortedDictionary<string, string>(pc);

            while (inStr.IndexOf("&", 0) >= 0)
            {
                int i = inStr.IndexOf("&", 0);

                slist.Add(inStr.Substring(0, i), "");
                inStr = inStr.Remove(0, i + 1);
            }

            slist.Add(inStr, "");

            string result = "";

            foreach (KeyValuePair<string, string> item in slist)
            {
                if (result == "")
                {
                    result = (item.Key);
                }
                else
                {
                    result += "&" + (item.Key);
                }
            }

            return result;
        }
        /// <summary>
        /// calculating the hash value for AmazonMWS signature
        /// </summary>
        /// <param name="key">secretkey for building the hash</param>
        /// <param name="text">string for the hash to be calculated</param>
        /// <returns>hash string</returns>
        public string GetAmazonSHA256Hash(string key, string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] keybytes = Encoding.ASCII.GetBytes(key);
            byte[] message = Encoding.ASCII.GetBytes(text);

            HMACSHA256 hashstring = new HMACSHA256(keybytes);
            string result = "";

            hashValue = hashstring.ComputeHash(message);

            result = UTF8toASCII(System.Convert.ToBase64String(hashValue));
            //result = System.Convert.ToBase64String(hashValue);

            return result;
        }
        class ParamComparer : IComparer<string>
        {
            public int Compare(string p1, string p2)
            {
                //Amazon Special Kacke-->

                bool specialkacke = false;

                if (p1.Contains("SellerSkus.member.") & p2.Contains("SellerSkus.member."))
                {
                    p1 = p1.Remove(0, 18);
                    p2 = p2.Remove(0, 18);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }

                //Amazon Special Kacke die ZWEITE-->
                if (p1.Contains("SellerSKUList.SellerSKU.") & p2.Contains("SellerSKUList.SellerSKU."))
                {
                    p1 = p1.Remove(0, 24);
                    p2 = p2.Remove(0, 24);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }
                //<--
                //Amazon Special Kacke die DRITTE-->
                if (p1.Contains("ASINList.ASIN.") & p2.Contains("ASINList.ASIN."))
                {
                    p1 = p1.Remove(0, 14);
                    p2 = p2.Remove(0, 14);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }
                //<--
                //Amazon Special Kacke die VIERTE-->
                if (p1.Contains("IdList.Id.") & p2.Contains("IdList.Id."))
                {
                    p1 = p1.Remove(0, 10);
                    p2 = p2.Remove(0, 10);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }
                //<--
                
                //Amazon Special Kacke die FÜNFTE-->
                if (p1.Contains("SellerSKUList.Id.") & p2.Contains("SellerSKUList.Id."))
                {
                    p1 = p1.Remove(0, 17);
                    p2 = p2.Remove(0, 17);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }
                //<--
                //Amazon Special Kacke die SECHSTE-->
                //if (p1.Contains("InboundShipmentItems.member.") & p2.Contains("InboundShipmentItems.member."))
                //{
                //    p1 = p1.Remove(0, 28);
                //    p2 = p2.Remove(0, 28);

                //    if (p1[0] == p2[0])
                //    {
                //        specialkacke = true;
                //    }
                //}
                //<--
                //Amazon Special Kacke die siebente-->
                //if (p1.Contains("InboundShipmentPlanRequestItems.member.") & p2.Contains("InboundShipmentPlanRequestItems.member."))
                //{
                //    p1 = p1.Remove(0, 39);
                //    p2 = p2.Remove(0, 39);

                //    if (p1[0] == p2[0])
                //    {
                //        specialkacke = true;
                //    }
                //}
                //<--
                if (specialkacke)
                {
                    int int1 = int.Parse(p1.Substring(0, p1.IndexOf("=")));
                    int int2 = int.Parse(p2.Substring(0, p2.IndexOf("=")));

                    Console.WriteLine("{0} vs. {1}", int1, int2);

                    return int1 == int2 ? 0 : (Math.Max(int1, int2) == int1 ? 1 : -1);
                }
                else
                {
                    return string.CompareOrdinal(p1, p2);
                }

                //return string.CompareOrdinal(p1, p2);
            }
        }

        static string UTF8toASCII(string text)
        {
            System.Text.Encoding utf8 = System.Text.Encoding.UTF8;
            Byte[] encodedBytes = utf8.GetBytes(text);
            Byte[] convertedBytes =
                    Encoding.Convert(Encoding.UTF8, Encoding.ASCII, encodedBytes);
            System.Text.Encoding ascii = System.Text.Encoding.ASCII;

            return ascii.GetString(convertedBytes);
        }

        public string GetUTCTimeString(string dtnow)
        {
            string result = DateTime.UtcNow.ToString();
            DateTime utcnow = new DateTime();
            if (DateTime.TryParse(dtnow, out utcnow))
            {
                result = utcnow.ToUniversalTime().ToString();
            }

            return result;
        }
        public string URLEncode(string inStr)
        {
            string outStr = "";

            outStr = System.Web.HttpUtility.UrlEncode(inStr);

            return outStr;
        }

        public string GetRequestURL()
        {
            return requestURL;
        }
        public void SetRequestURL(string intext)
        {
            requestURL = intext;
        }
        public void AddToRequestURL(string intext)
        {
            requestURL += intext;
        }
        public string GetSignature()
        {
            return signature;
        }
        public void SetSignature(string intext)
        {
            signature = intext;
        }
        public void AddRightToSignature(string intext)
        {
            signature += intext;
        }
        public void AddLeftToSignature(string intext)
        {
            signature = intext + signature;
        }

        public void InitSignature()
        {
            signature = requestURL.Remove(0, requestURL.IndexOf("?") + 1);
        }
        public void ReplaceInSignature(string fromtext, string totext)
        {
            signature = signature.Replace(fromtext, totext);
        }

        public void SortSignature()
        {
            signature = GetByteOrder(signature);
        }
        public void SignatureToHash(string key)
        {
            signature = GetAmazonSHA256Hash(key, signature);
        }
        public string RequestPart(int entryno)
        {
            string result = "";

            if (entryno <= requestparts.Length)
                result = requestparts[entryno - 1];

            return result;
        }

        public int CreateRequestParts()
        {
            string result = requestURL + signature;

            List<string> parts = new List<string>();

            while (result != "")
            {
                if (result.Length > 1000)
                {
                    string part = result.Substring(0, 1000);
                    parts.Add(part);
                    result = result.Remove(0, 1000);
                }
                else
                {
                    parts.Add(result);
                    result = "";
                }
            }

            requestparts = parts.ToArray();

            requestURL = requestURL + signature;
            return requestparts.Length;
        }

        public void SetXMLHTTP(MSXML2.ServerXMLHTTP60 xml)
        {
            xmlhttp = xml;
        }
        public MSXML2.ServerXMLHTTP60 GetXMLHTTP()
        {
            return xmlhttp;
        }

        public bool DoOpen(string method,string url,bool async,string user,string password)
        {
            bool result = false;

            if (url=="")
            {
                url = requestURL;
            }

            try
            {
                xmlhttp.open(method, url, async, user, password);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            
            return result;
        }
        public bool DoSend(string body)
        {
            bool result = false;

            try
            {
                xmlhttp.send(body);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public void ShowXMLHTTPResponseText(MSXML2.ServerXMLHTTP60 xml)
        {
            MessageBox.Show(xml.responseText);
        }
    }

    /// <summary>
    /// Tools for MeinPaket-Integration into Dynamics NAV
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("BAE37CD7-051D-4BDF-B40B-D812B6639B0E")]
    public class MeinPaketTools : IMeinPaketTools
    {
        public MeinPaketTools() { }

        string internalText = "";
        string[] internalTextArray;

        public void AppendText(string text)
        {
            internalText += text;
        }
        public bool ShowDialog()
        {
            bool result = false;



            SelectionForm sf = new SelectionForm(internalText);

            if (sf.ShowDialog() == DialogResult.OK)
            {
                result = true;
                internalText = sf.SelectedText;
            }

            return result;
        }
        public int SplitTextToParts(int partlength)
        {
            int result = 0;

            double partcount = Math.Ceiling((double)internalText.Length / (double)partlength);

            string temptext = internalText;

            string[] temparray = new string[(int)partcount];

            for (int i = 0; i < partcount; i++)
            {
                if (temptext.Length > partlength)
                {
                    temparray[i] = temptext.Substring(0, partlength);
                    temptext = temptext.Remove(0, partlength);
                }
                else
                {
                    temparray[i] = temptext;
                }

            }

            internalTextArray = temparray;

            result = (int)partcount;

            return result;
        }
        public string GetSelectedHTMLText(int ptr)
        {
            string result = "";

            if (internalTextArray == null)
            {
                result = "";
            }
            else
            {
                result = internalTextArray[ptr - 1];
            }

            return result.Replace(Environment.NewLine, "");
        }
    }

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
      
        public string GetTextFromXMLDOMNode(ref object node,int maxLength)
        {
            string shortText = ((MSXML2.IXMLDOMNode)node).text;
            if (shortText.Length>maxLength)
            {
                shortText = shortText.Substring(0, maxLength);
            }
            return shortText;
        }
    }

    /// <summary>
    /// external TextEditor for Integration in Dynamics NAV
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("3FF037F5-306B-4282-A0C7-D331E557579B")]
    public class TextEditor : ITextEditor
    {
        public TextEditor() { }
        string FormTitle { get; set; }
        int LanguageID { get; set; }
        bool FormVisible { get; set; }
        bool FormCanceled { get; set; }
        int FormMaxLineLength { get; set; }
        string[] FormLines { get; set; }
        string DownloadURL { get; set; }
        List<string> InitLines = new List<string>();
        List<string> AdditionalLines = new List<string>();

        public void Title(string title)
        {
            FormTitle = title;
        }
        public void Language(int language)
        {
            LanguageID = language;
        }
        public void MaxLineLen(int linelength)
        {
            FormMaxLineLength = linelength;
        }
        public void AddLine(string newline)
        {
            InitLines.Add(newline);
        }
        public string GetLine(int lineno)
        {
            string result = "";
            if (FormLines.Length >= lineno)
            {
                result = FormLines[lineno - 1];
            }

            return result;
        }
        public void ShowWPF()
        {
            TextEntryFormWPF tef = new TextEntryFormWPF();

            tef.SetCaption(FormTitle);
            tef.SetAdditionalLines(AdditionalLines.ToArray());
            tef.SetLines(InitLines.ToArray());
            FormVisible = true;
            tef.ShowDialog();

            if (tef.DialogResult.HasValue && tef.DialogResult.Value)
            {
                FormLines = tef.GetLines();

                FormLines = TrimStringArrayToMaxLineLength(FormLines, FormMaxLineLength, new char[] { (char)32 });

                FormCanceled = false;
            }
            else
            {
                FormCanceled = true;
            }

            FormVisible = false;
        }
        public bool Visible()
        {
            return FormVisible;
        }
        public bool Cancel()
        {
            return FormCanceled;
        }
        public int Lines()
        {
            return FormLines.Length;
        }
        public void SetDownloadURL(string url)
        {
            DownloadURL = url;
        }
        public bool UpdateBrandText()
        {
            bool result = false;
            string newBrandText = "Bei COM-FOUR&reg; handelt es sich um eine Marke die beim Amt der Europ&auml;ischen Union für geistiges Eigentum als Unionsmarke Nr. EU011758811 und Nr. EU014441703  eingetragen ist. Gem&auml;&szlig; &sect; 14 Abs. 2 MarkenG ist es Dritten untersagt, diese Marke im gesch&auml;ftlichen Verkehr zu verwenden.";
            
            string tempString = "";

            foreach (string line in InitLines)
            {
                tempString += line;
            }

            string oldstring = tempString;

            int startpos = tempString.IndexOf("Bei COM-FOUR");

            if (startpos<=0)
            {
                startpos = tempString.IndexOf("BeiCOM-FOUR");
            }

            if (startpos<=0)
            {
                return false;
            }
            else
            {
                tempString = tempString.Remove(startpos);
                tempString = tempString+=newBrandText;
            }

            if (tempString != oldstring)
            {
                result = true;
                string[] temparray = new string[1];
                temparray[0] = tempString;

                FormLines = TrimStringArrayToMaxLineLength(temparray, FormMaxLineLength, new char[] { (char)32 });
            }

            return result;
        }
        public void ResetInitLines() 
        {
            InitLines = new List<string>();
        }

        public void AddAdditionalLine(string newline)
        {
            AdditionalLines.Add(newline);
        }
        internal static string[] TrimStringArrayToMaxLineLength(string[] fromarray, int maxlinelength, char[] separators)
        {
            List<string> result = new List<string>();

            foreach (string item in fromarray)
            {
                if (item.Length <= maxlinelength)
                {
                    result.Add(item.Trim(separators));
                }
                else
                {
                    string[] parts = item.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    string newitem = "";

                    foreach (string part in parts)
                    {
                        if (part.Length > maxlinelength)
                        {
                            string temppart = part;
                            do
                            {
                                if (temppart.Length > maxlinelength)
                                {
                                    result.Add(temppart.Substring(0, maxlinelength).Trim(separators));
                                    temppart = temppart.Remove(0, maxlinelength);
                                }
                                else
                                {
                                    result.Add(temppart.Trim(separators));
                                    temppart = "";
                                }
                            } while (temppart != "");
                        }
                        else
                        {
                            if (newitem.Length + part.Length + 1 > maxlinelength)
                            {
                                result.Add(newitem.Trim(separators));
                                newitem = part;
                            }
                            else
                            {
                                newitem = newitem + " " + part;
                            }
                        }
                    }

                    if (newitem != "")
                    {
                        result.Add(newitem.Trim(separators));
                    }
                }
            }

            return result.ToArray();
        }
    }

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
            if (path!=null & path!="")
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

                result = (upperleft.R<=val && upperleft.G<=val && upperleft.B<=val) &&
                         (upperright.R<=val && upperright.G<=val && upperright.B<=val) &&
                         (lowerleft.R<=val && lowerleft.G<=val && lowerleft.B<=val) &&
                         (lowerright.R<=val && lowerright.G<=val && lowerright.B<=val);
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
            string result="";

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
                        bmp.Save(tofilename,System.Drawing.Imaging.ImageFormat.Bmp);
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
            string result="";

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

    /// <summary>
    /// (deprecated)Tools for retrieving Amazon related Fees, (deprecated)
    /// </summary>
    [System.Obsolete("Method of retrieving FBAFees is deprecated", false)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("F2EE235B-3E92-4636-80B6-F82D7FD77B41")]
    public class FBAFees : IFBAFees
    {
        private MiscClasses.AFNFees fees;

        public FBAFees() { }

        public void Initialize(string language, string marketPlaceId, string currency)
        {
            fees = new MiscClasses.AFNFees(language, marketPlaceId, currency);
        }

        public int GetFees(string asin, double price)
        {
            return fees.GetFees(asin, price);
        }

        public double GetFee(string feename)
        {
            double result = 0;

            if (fees.Fees.ContainsKey(feename))
                result = fees.Fees[feename];

            return result;
        }

        public bool GetDimensions(string asin, ref double length, ref double width, ref double height, ref double weight)
        {
            return fees.GetDimensions(asin, ref length, ref width, ref height, ref weight);
        }

        public string ErrorMessage()
        {
            string result = "";

            if (fees.ErrorOccurred)
            {
                result = fees.ErrorMessage;
            }

            return result;
        }

    }

    /// <summary>
    /// Tools for Printing Labels for integration into DynamicsNAV 
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("BAF80517-EB2F-4642-AAD8-68AB3C44D548")]
    public class LabelPrinting : iLabelPrinting
    {
        private string Base64String="";
        private string LabelFormat = "";
        private string PrinterName = "";
        private string ZPLPrinterName = "";
        private string LastErrorMessage = "";
        private int XOffset = 0;
        private int YOffset = 0;
        private Queue<Label> ZPLLabelQueue;
        private Queue<Label> ImageLabelQueue;
        private string[] zplLines;
        private Margins margins;
        private bool marginSet = false;

        public LabelPrinting() { }

        public void Init(string labelFormat, string printerName, string zplprinterName, int xOffset,int yOffset)
        {
            LabelFormat = labelFormat;
            PrinterName = printerName;
            ZPLPrinterName = zplprinterName;
            XOffset = xOffset;
            YOffset = yOffset;
            ZPLLabelQueue = new Queue<Label>();
            ImageLabelQueue = new Queue<Label>();

        }

        struct Label
        {
            public string LabelFormat;
            public string LabelContent;
            public string AdditionalContent;

            public Label(string format,string content,string addContent)
            {
                LabelFormat = format;
                LabelContent = content;
                AdditionalContent = addContent;
            }
        }
        struct PointText
        {
            public string Origin;
            public int RotationAngle;
            public Point Point;
            public string Text;

            public PointText(string origin ,int angle,int x, int y, string text)
            {
                Origin = origin;
                RotationAngle = angle;
                Point = new Point(x, y);
                Text = text;
            }
        }
        struct Margins
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;

            public Margins(int left,int right, int top, int bottom)
            {
                Left = left;
                Right = right;
                Top = top;
                Bottom = bottom;
            }
        }

        public void AddToBase64String(string textPart)
        {
            Base64String += textPart;
        }

        public void ClearBase64String()
        {
            Base64String = "";
        }
        public void EnqueueBase64String(string labelFormat)
        {
            EnqueueBase64String2(labelFormat, "");
        }
        public void EnqueueBase64String2(string labelFormat,string addContent)
        {
            if (labelFormat=="")
            {
                labelFormat = LabelFormat;
            }

            Label label = new Label(LabelFormat, Base64String, addContent);

            switch (labelFormat)
            {
                case "ZPL":
                case "ZPL203":
                case "APPLICATION/ZPL": ZPLLabelQueue.Enqueue(label); break;
                case "PNG":
                case "IMAGE/PNG": ImageLabelQueue.Enqueue(label); break;
                default: ImageLabelQueue.Enqueue(label); break;
            }
            //
            Base64String = "";
        }

        public string GetLastErrorMessage()
        {
            return LastErrorMessage;
        }
        public bool PrintLabel(bool directPrinting)
        {
            bool result = false;

            MessageBox.Show("Funktion Veraltet");
            //LastErrorMessage = "";

            //if (Base64String =="")
            //{
            //    return false;
            //}

            //try
            //{
            //    LabelQueue.Enqueue(new Label(LabelFormat,Base64String));

            //    ClearBase64String();

            //    if (directPrinting)
            //    {
            //        PrintQueue();
            //    }

            //    result = true;
            //}
            //catch (Exception e)
            //{
            //    LastErrorMessage = e.Message;
            //}

            return result;
        }

        public bool PrintQueue()
        {
            bool result = false;
            
            if (ImageLabelQueue.Count>0)
            {
                result = PrintImageFileLabels();
            }
            if (ZPLLabelQueue.Count > 0)
            {
                result = PrintZPLFileLabels();
            }

            return result;
        }

        private bool PrintImageFileLabels()
        {
            bool result = false;

            try
            {
                PrintDocument pd = new PrintDocument();
                if (PrinterName != "")
                {
                    pd.PrinterSettings.PrinterName = PrinterName;
                }

                pd.DefaultPageSettings.Landscape = false;

                if (marginSet)
                {
                    var pagemargins = pd.PrinterSettings.DefaultPageSettings.Margins;
                    pagemargins.Left = margins.Left;
                    pagemargins.Right = margins.Right;
                    pagemargins.Top = margins.Top;
                    pagemargins.Bottom = margins.Bottom;
                }

                pd.PrintPage += Pd_PrintPage;

                pd.Print();

                result = true;
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
            }
            
            return result;
        }
        private bool PrintZPLFileLabels()
        {
            bool result = false;

            try
            {
                Com.SharpZebra.Printing.PrinterSettings settings = new Com.SharpZebra.Printing.PrinterSettings();
                if (ZPLPrinterName!="")
                {
                    settings.PrinterName = this.ZPLPrinterName;
                }
                else
                {
                    settings.PrinterName = this.PrinterName;
                }
                
                //settings.PrinterPort = 9100;
                
                Com.SharpZebra.Printing.SpoolPrinter spoolPrinter = new Com.SharpZebra.Printing.SpoolPrinter(settings);

                //Com.SharpZebra.Printing.NetworkPrinter netPrinter = new Com.SharpZebra.Printing.NetworkPrinter(settings);

                byte[] zplbytes = new byte[0];

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryWriter bw = new BinaryWriter(ms);

                    do
                    {
                        Label label = ZPLLabelQueue.Dequeue();
                        byte[] bytes = DecodeBase64String(label.LabelContent);
                        bw.Write(bytes);

                    } while (ZPLLabelQueue.Count > 0);

                    zplbytes = ms.ToArray();
                }

                spoolPrinter.Print(zplbytes);

                result = true;
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
            }
            
            return result;
        }
        public bool PrintZPLCodeToFile(string fileName)
        {
            bool result = false;

            try
            {
                Label label = ZPLLabelQueue.Dequeue();
                byte[] bytes = DecodeBase64String(label.LabelContent);
                File.AppendAllText(fileName, Encoding.ASCII.GetString(bytes));
                result = true;
            }
            catch (Exception e)
            {

                LastErrorMessage = e.Message;
            }

            return result;
        }
        public int GetZPLLines()
        {
            int result = 0;

            Label label = ZPLLabelQueue.Dequeue();
            byte[] bytes = DecodeBase64String(label.LabelContent);
            string lines = Encoding.ASCII.GetString(bytes);

            zplLines = lines.Split(new string[] {"\r\n","\r","\n"},StringSplitOptions.RemoveEmptyEntries);
            result = zplLines.Length;

            return result;
        }
        public string GetZPLLine(int key,ref string addStr)
        {
            string result = "";
            addStr = "";

            string line = zplLines[key];

            if (line.Length>1024)
            {
                result = line.Substring(0, 1024);
                addStr = line.Substring(1024);
            }
            else
            {
                result = line;
            }


            return result; ;
        }
        public bool ImageLabelToFile(string fileName)
        {
            bool result = false;
            try
            {
                Label label = ImageLabelQueue.Dequeue();
                Image image = Base64StringToImage(label.LabelContent);
                
                image.Save(fileName);
                result = true;
            }
            catch (Exception e )
            {
                LastErrorMessage = e.Message;
            }

            return result;
        }
        public void SetMargins(int left, int right, int top, int bottom)
        {
            margins = new Margins(left,right,top,bottom);
            marginSet = true;
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs args)
        {
            Rectangle m = args.MarginBounds;
            Rectangle r = args.PageBounds;

            m.X = m.X + XOffset;
            m.Y = m.Y + YOffset;
            
            if (ImageLabelQueue.Count > 0)
            {
                Label label = ImageLabelQueue.Dequeue();
                Image image = Base64StringToImage(label.LabelContent);
                
                List<PointText> texts = GetAddTexts(label.AdditionalContent);
                List<PointText> imageTexts = texts.Where(f => f.Origin == "I").ToList<PointText>();

                if (imageTexts.Count>0)
                {
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        foreach (var item in imageTexts)
                        {
                            g.DrawString(item.Text, new Font("Arial", 8), new SolidBrush(Color.Black), item.Point);
                        }
                    }
                }

                if ((double)image.Width / (double)image.Height > (double)m.Width / (double)m.Height) // image is wider
                {
                    m.Height = (int)((double)image.Height / (double)image.Width * (double)m.Width);
                }
                else
                {
                    m.Width = (int)((double)image.Width / (double)image.Height * (double)m.Height);
                }

                args.Graphics.DrawImage(image, m);
                //args.Graphics.DrawImage(image, r);

                List<PointText> pageTexts = texts.Where(f => f.Origin != "I").ToList<PointText>();
                if (pageTexts.Count>0)
                {
                    foreach (var item in pageTexts)
                    {
                        switch (item.Origin)
                        {
                            case "UL":
                                args.Graphics.TranslateTransform(0, 0);
                                break;
                            case  "UR" :
                                args.Graphics.TranslateTransform(r.Width, 0);
                                break;
                            case "LL":
                                args.Graphics.TranslateTransform(0, r.Height);
                                break;
                            case "LR":
                                args.Graphics.TranslateTransform(r.Width, r.Height);
                                break;
                            default:
                                args.Graphics.TranslateTransform(0, 0);
                                break;
                        }
                        
                        args.Graphics.RotateTransform(item.RotationAngle);
                        args.Graphics.DrawString(item.Text, new Font("Arial", 8), new SolidBrush(Color.Black), item.Point);
                        args.Graphics.ResetTransform();
                    }
                }
                
                

                args.HasMorePages = (ImageLabelQueue.Count > 0);
                image.Dispose();
            }
            else
            {
                args.Cancel = true;
            }
        }
        private Image Base64StringToImage(string base64String)
        {
            Image label = null;

            LastErrorMessage = "";

            if (base64String == "")
            {
                return label;
            }

            try
            {
                byte[] bytes = DecodeBase64String(base64String);
                MemoryStream ms = new MemoryStream(bytes);

                label = Image.FromStream(ms);
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
            }

            return label;
        }
        private byte[] DecodeBase64String(string base64String)
        {
            byte[] result = null;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64String);

                Stream b64stream = new MemoryStream(bytes);

                GZipStream zipstream = new GZipStream(b64stream, CompressionMode.Decompress);

                const int size = 4096;
                byte[] buffer = new byte[size];

                MemoryStream memory = new MemoryStream();

                int count = 0;
                do
                {
                    count = zipstream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                }
                while (count > 0);

                result = memory.ToArray();
            }
            catch (Exception e )
            {

                LastErrorMessage = e.Message;
            }

            return result;
        }
        private List<PointText> GetAddTexts(string text)
        {
            List<PointText> result = new List<PointText>();

            if (text=="")
            {
                return result;
            }

            string[] subtexts = text.Split(new char[] { '|' });
            foreach (var item in subtexts)
            {
                string[] parts = item.Split(new char[] { ',' });
                if (parts.Length==5)
                {
                    string origin = parts[0];
                    int angle = int.Parse(parts[1]);
                    int x = int.Parse(parts[2]);
                    int y = int.Parse(parts[3]);
                    result.Add(new PointText(origin, angle, x, y, parts[4]));
                }
            }

            return result;
        }
    }
    #endregion

    #region misc classes
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
    /// <summary>
    /// implementation of ICommand, for binding purposes
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion // Constructors

        #region ICommand Members


        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members
    }

    #endregion
}

