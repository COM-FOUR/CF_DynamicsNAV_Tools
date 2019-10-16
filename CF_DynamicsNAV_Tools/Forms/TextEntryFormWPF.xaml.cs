using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// Interaktionslogik für TextEntryFormWPF.xaml
    /// </summary>
    public partial class TextEntryFormWPF : Window
    {
        string DownloadURL = "";
        string additionalLines;
        int remainingTextLength = 0;

        //static List<string> tags = new List<string>();
        //static List<char> specials = new List<char>();
        //static string text;
        //List<Tag> m_tags = new List<Tag>();

        public TextEntryFormWPF()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DownloadURL != "")
            {
                DownloadWebSiteText(DownloadURL);
            }

            
            ////TODO
            //string[] specialWords = { "string", "char", "null", "b", @"/b", "br", @"&bull;" };
            //tags = new List<string>(specialWords);
            //// We also want to know all possible delimiters so adding this stuff.     
            //char[] chrs = {
            //  '.',
            //    //')',
            //    //'(',
            //    //'[',
            //    //']',
            //    '>',
            //    '<',
            //    //':',
            //    //';',
            //    '\n',
            //    '\t',
            //    '\r'
            //};
            //specials = new List<char>(chrs);

            tb_lines.Focus();
            //rtb_lines.Focus();
            //rtb_lines_TextChanged(rtb_lines, null);
        }
        public void SetLines(string[] newlines)
        {
            tb_lines.Text = String.Join(System.Environment.NewLine, newlines);

            //rtb_lines.Document.Blocks.Clear();
            //rtb_lines.Document.Blocks.Add(new Paragraph(new Run(String.Join(System.Environment.NewLine, newlines))));
            //rtb_lines.Refresh();

        }
        public void SetAdditionalLines(string[] newlines)
        {
            additionalLines = string.Join(" ", newlines);
        }
        public string[] GetLines()
        {
            return tb_lines.Text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);

            //TextRange tr = new TextRange(rtb_lines.Document.ContentStart, rtb_lines.Document.ContentEnd);

            //return tr.Text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
        }
        public void SetCaption(string newcaption)
        {
            this.Title = newcaption;
        }
        public void SetDownloadURL(string url)
        {
            if (url != "" & url != null)
            {
                DownloadURL = url;
            }
        }
        void DownloadWebSiteText(string url)
        {
            WebBrowser wb = new WebBrowser();
            //wb.ScriptErrorsSuppressed = true;
            //wb.DocumentCompleted += wb_DocumentCompleted;
            wb.Navigated += wb_Navigated;
            //wb.Url = new Uri(url);
            wb.Navigate(url);
        }

        void wb_Navigated(object sender, NavigationEventArgs e)
        {
            //tb_lines.Text = ((WebBrowser)sender).DocumentText.Split(System.Environment.NewLine.ToCharArray());
            tb_lines.Text = ((WebBrowser)sender).Document.ToString();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Return || e.Key == Key.Enter) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            tb_lines.Text = tb_lines.Text.Insert(tb_lines.SelectionStart,"<br>");
            
            //rtb_lines.Selection.Text = "<br> ";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            tb_lines.Text = tb_lines.Text.Insert(tb_lines.SelectionStart, "&bull; ");
            
            //rtb_lines.Selection.Text = "&bull; ";
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            int start = tb_lines.SelectionStart;
            tb_lines.Text = tb_lines.Text.Insert(start + tb_lines.SelectionLength, "</b>");
            tb_lines.Text = tb_lines.Text.Insert(start, "<b>");

            //rtb_lines.Selection.Text = String.Format("<b>{0}</b>", rtb_lines.Selection.Text);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo(Environment.SystemDirectory+"\\charmap.exe");
            Process.Start(psi);
        }

        private void tb_lines_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (additionalLines!=null && additionalLines.Length>0)
            {
                wb_preview.NavigateToString("<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head><body>" + ((TextBox)sender).Text +
                        "<span style='color:red'>" + additionalLines + " </div></body>");

                //NEU 
                //rtb_lines.Document.Blocks.Clear();
                //rtb_lines.Document.Blocks.Add(new Paragraph(new Run(((TextBox)sender).Text)));   
            }
            else
            {
                wb_preview.NavigateToString("<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head><body>" + ((TextBox)sender).Text + "</body>");
            }

            remainingTextLength = 2000 - ((TextBox)sender).Text.Length - additionalLines.Length;
            tbremainingTextLength.Text = remainingTextLength.ToString();
        }
        
        //public static bool IsKnownTag(string tag)
        //{
        //    return tags.Exists(delegate (string s) { return s.ToLower().Equals(tag.ToLower()); });
        //}
        //private static bool GetSpecials(char i)
        //{
        //    foreach (var item in specials)
        //    {
        //        if (item.Equals(i))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        //// Wow. Great. Now I should separate words, that equals to my tags. For this propose we'll create new internal structure named Tag. This will help us to save words and its' positions.
        //new struct Tag
        //{
        //    public TextPointer StartPosition;
        //    public TextPointer EndPosition;
        //    public string Word;
        //}


        //internal void CheckWordsInRun(Run theRun) //do not hightlight keywords in this method
        //{
        //    //How, let's go through our text and save all tags we have to save.               
        //    int sIndex = 0;
        //    int eIndex = 0;

        //    for (int i = 0; i < text.Length; i++)
        //    {
        //        if (Char.IsWhiteSpace(text[i]) | GetSpecials(text[i]))
        //        {
        //            if (i > 0 && !(Char.IsWhiteSpace(text[i - 1]) | GetSpecials(text[i - 1])))
        //            {
        //                eIndex = i - 1;
        //                string word = text.Substring(sIndex, eIndex - sIndex + 1);
        //                if (IsKnownTag(word))
        //                {
        //                    Tag t = new Tag();

        //                    if (GetSpecials(text[sIndex-1]) & GetSpecials(text[eIndex + 1]))
        //                    {
        //                        t.StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex-1, LogicalDirection.Forward);
        //                        t.EndPosition = theRun.ContentStart.GetPositionAtOffset(eIndex + 2, LogicalDirection.Backward);
        //                    }
        //                    else
        //                    {
        //                        t.StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex, LogicalDirection.Forward);
        //                        t.EndPosition = theRun.ContentStart.GetPositionAtOffset(eIndex + 1, LogicalDirection.Backward);
        //                    }
                            
        //                    t.Word = word;
        //                    m_tags.Add(t);
        //                }
        //            }
        //            sIndex = i + 1;
        //        }
        //    }
        //    //How this works. But wait. If the word is last word in my text I'll never hightlight it, due I'm looking for separators. Let's add some fix for this case
        //    string lastWord = text.Substring(sIndex, text.Length - sIndex);
        //    if (IsKnownTag(lastWord))
        //    {
        //        Tag t = new Tag();
        //        t.StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex, LogicalDirection.Forward);
        //        t.EndPosition = theRun.ContentStart.GetPositionAtOffset(text.Length, LogicalDirection.Backward); //fix 1
        //        t.Word = lastWord;
        //        m_tags.Add(t);
        //    }
        //}

        //private void rtb_lines_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    RichTextBox txtStatus = (RichTextBox)sender;

        //    if (txtStatus.Document == null)
        //        return;
        //    txtStatus.TextChanged -= rtb_lines_TextChanged;

        //    m_tags.Clear();

        //    //first clear all the formats
        //    TextRange documentRange = new TextRange(txtStatus.Document.ContentStart, txtStatus.Document.ContentEnd);
        //    documentRange.ClearAllProperties();
        //    //text = documentRange.Text; //fix 2

        //    //Now let's create navigator to go though the text, find all the keywords but do not hightlight
        //    TextPointer navigator = txtStatus.Document.ContentStart;
        //    while (navigator.CompareTo(txtStatus.Document.ContentEnd) < 0)
        //    {
        //        TextPointerContext context = navigator.GetPointerContext(LogicalDirection.Backward);
        //        if (context == TextPointerContext.ElementStart && navigator.Parent is Run)
        //        {
        //            text = ((Run)navigator.Parent).Text; //fix 2
        //            if (text != "")
        //                CheckWordsInRun((Run)navigator.Parent);
        //        }
        //        navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
        //    }

        //    //only after all keywords are found, then we highlight them
        //    for (int i = 0; i < m_tags.Count; i++)
        //    {
        //        try
        //        {
        //            TextRange range = new TextRange(m_tags[i].StartPosition, m_tags[i].EndPosition);
        //            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Blue));
        //            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        //        }
        //        catch { }
        //    }
        //    txtStatus.TextChanged += rtb_lines_TextChanged;

        //    TextRange tr = new TextRange(txtStatus.Document.ContentStart, txtStatus.Document.ContentEnd);

        //    if (additionalLines != null && additionalLines.Length > 0)
        //    {
        //        wb_preview.NavigateToString("<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head><body>" + tr.Text +
        //                "<span style='color:red'>" + additionalLines + " </div></body>");
        //    }
        //    else
        //    {
        //        wb_preview.NavigateToString("<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head><body>" + tr.Text + "</body>");
        //    }

        //    remainingTextLength = 2000 - tr.Text.Length - additionalLines.Length;
        //    tbremainingTextLength.Text = remainingTextLength.ToString();
        //}
    }
}
