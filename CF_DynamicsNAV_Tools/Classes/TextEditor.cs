using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace CF_DynamicsNAV_Tools
{
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

            if (startpos <= 0)
            {
                startpos = tempString.IndexOf("BeiCOM-FOUR");
            }

            if (startpos <= 0)
            {
                return false;
            }
            else
            {
                tempString = tempString.Remove(startpos);
                tempString = tempString += newBrandText;
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
}
