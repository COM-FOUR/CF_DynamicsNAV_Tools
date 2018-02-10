using System;
using System.Runtime.InteropServices;

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
        string GetTemporaryThumbnail(string filepath, string filename, int pixelsize);
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
    public interface ILabelPrinting
    {
        void Init(string labelFormat, string printerName, string zplprinterName, int xOffset, int yOffset);
        void AddToLabelContent(string textPart);
        void ClearBase64String();
        string GetLastErrorMessage();
        bool PrintLabel(bool directPrinting);
        bool PrintQueue();
        void EnqueueBase64String(string labelFormat);
        bool PrintZPLCodeToFile(string fileName);
        int GetZPLLines();
        string GetZPLLine(int key, ref string addStr);
        bool PrintImageLabelToFile(string fileName);
        void EnqueueBase64String2(string labelFormat, string addContent);
        void EnqueueBase64String3(string id, string labelFormat, string addContent);
        void EnqueueString(string id, string labelFormat);
        bool ExportQueue(string exportPath);
        bool PrintLabelFromFile(string fileName);
    }
    #endregion
}
