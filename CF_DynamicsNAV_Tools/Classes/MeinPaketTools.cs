using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CF_DynamicsNAV_Tools
{
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
}
