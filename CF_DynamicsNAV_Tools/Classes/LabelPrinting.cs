using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// Tools for Printing Labels for integration into DynamicsNAV 
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("BAF80517-EB2F-4642-AAD8-68AB3C44D548")]
    public class LabelPrinting : ILabelPrinting
    {
        private string LabelContent = "";
        private string LabelFormat = "";
        private string PrinterName = "";
        private string ZPLPrinterName = "";
        private string LastErrorMessage = "";
        private int XOffset = 0;
        private int YOffset = 0;
        private Queue<Label> ZPLLabelQueue;
        private Queue<Label> ImageLabelQueue;
        private string[] zplLines;

        public LabelPrinting() { }

        public void Init(string labelFormat, string printerName, string zplprinterName, int xOffset, int yOffset)
        {
            LabelFormat = labelFormat;
            PrinterName = printerName;
            ZPLPrinterName = zplprinterName;
            XOffset = xOffset;
            YOffset = yOffset;
            ZPLLabelQueue = new Queue<Label>();
            ImageLabelQueue = new Queue<Label>();
        }

        #region structs/classes
        class Label
        {
            public string LabelId = "";
            public string LabelFormat = "";
            public string LabelContent = "";
            public string AdditionalContent = "";
            public bool isBase64 = false;

            public Label(string id, string format, string content) : this(id, format, content, "",false){}
            public Label(string id, string format, string content, string addContent, bool base64)
            {
                LabelId = id;
                LabelFormat = format;
                LabelContent = content;
                AdditionalContent = addContent;
                isBase64 = base64;
            }
        }
        struct PointText
        {
            public string Origin;
            public int RotationAngle;
            public Point Point;
            public string Text;

            public PointText(string origin, int angle, int x, int y, string text)
            {
                Origin = origin;
                RotationAngle = angle;
                Point = new Point(x, y);
                Text = text;
            }
        }
        #endregion

        #region common methods
        #region com-exposed common methods
        public void AddToLabelContent(string textPart)
        {
            LabelContent += textPart;
        }
        public void ClearLabelContent()
        {
            LabelContent = "";
        }
        public void EnqueueBase64String(string labelFormat)
        {
            EnqueueBase64String2(labelFormat, "");
        }
        public void EnqueueBase64String2(string labelFormat, string addContent)
        {
            EnqueueBase64String3("", labelFormat, addContent);
        }
        public void EnqueueBase64String3(string id, string labelFormat, string addContent)
        {
            if (labelFormat == "")
            {
                labelFormat = LabelFormat;
            }

            Label label = new Label(id, LabelFormat, LabelContent, addContent, true);
            EnqueueLabel(labelFormat, label);

            ClearLabelContent();
        }
        public void EnqueueString(string id, string labelFormat)
        {
            if (labelFormat == "")
            {
                labelFormat = LabelFormat;
            }

            Label label = new Label(id, LabelFormat, LabelContent, "", false);

            EnqueueLabel(labelFormat, label);

            ClearLabelContent();
        }
        public string GetLastErrorMessage()
        {
            return LastErrorMessage;
        }
        public bool PrintLabel(bool directPrinting)
        {
            bool result = false;

            MessageBox.Show("Funktion Veraltet");

            return result;
        }
        public bool PrintQueue()
        {
            bool result = false;

            if (ImageLabelQueue.Count > 0)
            {
                result = PrintImageLabels();
            }
            if (ZPLLabelQueue.Count > 0)
            {
                result = PrintZPLLabels();
            }

            return result;
        }
        public bool ExportQueue(string exportPath)
        {
            bool result = false;

            if (ImageLabelQueue.Count > 0)
            {
                result = ExportImageLabels(exportPath);
            }
            if (ZPLLabelQueue.Count > 0)
            {
                result = ExportZPLLabels(exportPath);
            }

            return result;
        }
        public bool PrintLabelFromFile(string fileName)
        {
            bool result = false;

            try
            {
                byte[] bytes = File.ReadAllBytes(fileName);
                FileInfo fi = new FileInfo(fileName);
                
                switch (fi.Extension)
                {
                    case ".png": break;
                    case ".txt":
                    case ".zpl": result = PrintZPLBytes(bytes); break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
            }

            return result;
        }
        #endregion

        private void EnqueueLabel(string labelFormat, Label label)
        {
            switch (labelFormat)
            {
                case "ZPL":
                case "ZPL203":
                case "APPLICATION/ZPL": ZPLLabelQueue.Enqueue(label); break;
                case "PNG":
                case "IMAGE/PNG": ImageLabelQueue.Enqueue(label); break;
                default: ImageLabelQueue.Enqueue(label); break;
            }
        }
        #endregion
        
        #region Image Methods
        #region com-exposed Image Methods
        public bool PrintImageLabelToFile(string fileName)
        {
            bool result = false;

            Label label = ImageLabelQueue.Dequeue();
            result = ImageLabelToFile(fileName, label);
            
            return result;
        }
        #endregion

        #region ImageLabel-Handling
        private bool PrintImageLabels()
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
        private void Pd_PrintPage(object sender, PrintPageEventArgs args)
        {
            try
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

                    if (imageTexts.Count > 0)
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
                    if (pageTexts.Count > 0)
                    {
                        foreach (var item in pageTexts)
                        {
                            switch (item.Origin)
                            {
                                case "UL":
                                    args.Graphics.TranslateTransform(0, 0);
                                    break;
                                case "UR":
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
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
            }

        }
        private bool ImageLabelToFile(string fileName, Label label)
        {
            bool result = false;

            try
            {
                Image image = Base64StringToImage(label.LabelContent);
                image.Save(fileName);

                result = true;
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
            }

            return result;
        }
        private bool ExportImageLabels(string exportPath)
        {
            bool result = false;

            DirectoryInfo di = new DirectoryInfo(exportPath);
            if (di.Exists)
            {
                result = true;

                while (ImageLabelQueue.Count > 0)
                {
                    Label label = ImageLabelQueue.Dequeue();
                    if (!ImageLabelToFile(String.Format("{0}\\{1}.img", di.FullName, label.LabelId), label))
                    {
                        result = false;
                    }
                }
            }

            return result;
        }
        private List<PointText> GetAddTexts(string text)
        {
            List<PointText> result = new List<PointText>();

            if (text == "")
            {
                return result;
            }

            string[] subtexts = text.Split(new char[] { '|' });
            foreach (var item in subtexts)
            {
                string[] parts = item.Split(new char[] { ',' });
                if (parts.Length == 5)
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
        #endregion
        #endregion

        #region ZPL Methods
        #region com-exposed ZPL Methods
        public bool PrintZPLCodeToFile(string fileName)
        {
            bool result = false;
            
            Label label = ZPLLabelQueue.Dequeue();
            result = ZPLCodeToFile(fileName, label);

            return result;
        }
        public int GetZPLLines()
        {
            int result = 0;

            Label label = ZPLLabelQueue.Dequeue();
            byte[] bytes = DecodeBase64String(label.LabelContent);
            string lines = Encoding.ASCII.GetString(bytes);

            zplLines = lines.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            result = zplLines.Length;

            return result;
        }
        public string GetZPLLine(int key, ref string addStr)
        {
            string result = "";
            addStr = "";

            string line = zplLines[key];

            if (line.Length > 1024)
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
        #endregion

        #region ZPLLabel-Handling
        private bool PrintZPLLabels()
        {
            bool result = false;

            try
            {
                byte[] zplbytes = new byte[0];

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryWriter bw = new BinaryWriter(ms);
                    do
                    {
                        Label label = ZPLLabelQueue.Dequeue();

                        byte[] bytes = ProcessZPLLabel(label);

                        bw.Write(bytes);

                    } while (ZPLLabelQueue.Count > 0);

                    zplbytes = ms.ToArray();
                }

                result = PrintZPLBytes(zplbytes);
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
            }

            return result;
        }
        private bool ExportZPLLabels(string exportPath)
        {
            bool result = false;

            DirectoryInfo di = new DirectoryInfo(exportPath);
            if (di.Exists & ZPLLabelQueue.Count > 0)
            {
                result = true;
                
                do
                {
                    Label label = ZPLLabelQueue.Dequeue();
                    if (!ZPLCodeToFile(String.Format("{0}\\{1}.zpl", di.FullName, label.LabelId), label))
                    {
                        result = false;
                    }
                } while (ZPLLabelQueue.Count > 0);
            }

            return result;
        }
        private byte[] ProcessZPLLabel(Label label)
        {
            byte[] bytes = null;

            if (label.isBase64)
            {
                bytes = DecodeBase64String(label.LabelContent);

                string text = Encoding.ASCII.GetString(bytes);
                string[] textarray = text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                textarray[textarray.Length - 1] = label.AdditionalContent.Replace("|", "\n") + Environment.NewLine + "^XZ\n";
                text = string.Join("\n", textarray);
                bytes = Encoding.ASCII.GetBytes(text);
            }
            else
            {
                bytes = Encoding.UTF8.GetBytes(label.LabelContent);
            }

            return bytes;
        }
        private bool PrintZPLBytes(byte[] bytes)
        {
            bool result = false;

            try
            {
                Com.SharpZebra.Printing.PrinterSettings settings = new Com.SharpZebra.Printing.PrinterSettings();
                if (ZPLPrinterName != "")
                {
                    settings.PrinterName = this.ZPLPrinterName;
                }
                else
                {
                    settings.PrinterName = this.PrinterName;
                }

                Com.SharpZebra.Printing.SpoolPrinter spoolPrinter = new Com.SharpZebra.Printing.SpoolPrinter(settings);
                spoolPrinter.Print(bytes);

                result = true;
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
            }

            return result;
        }
        private bool ZPLCodeToFile(string fileName, Label label)
        {
            bool result = false;
            
            try
            {
                byte[] bytes = ProcessZPLLabel(label);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                File.WriteAllBytes(fileName, bytes);
                result = true;
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
            }

            return result;
        }
        #endregion
        #endregion

        #region base64 handling
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
            catch (Exception e)
            {

                LastErrorMessage = e.Message;
            }

            return result;
        }
        #endregion
    }
}
