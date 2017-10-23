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
            
            tb_lines.Focus();
        }
        public void SetLines(string[] newlines)
        {
            tb_lines.Text = String.Join(System.Environment.NewLine, newlines);
        }
        public void SetAdditionalLines(string[] newlines)
        {
            additionalLines = string.Join(" ", newlines);
        }
        public string[] GetLines()
        {
            return tb_lines.Text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
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
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            tb_lines.Text = tb_lines.Text.Insert(tb_lines.SelectionStart, "&bull; ");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            int start = tb_lines.SelectionStart;
            tb_lines.Text = tb_lines.Text.Insert(start + tb_lines.SelectionLength, "</b>");
            tb_lines.Text = tb_lines.Text.Insert(start, "<b>");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            tb_lines.Text = tb_lines.Text + Environment.NewLine + Environment.NewLine + CF_DynamicsNAV_Tools.Properties.Resources.BrandNote;
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
            }
            else
            {
                wb_preview.NavigateToString("<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head><body>" + ((TextBox)sender).Text + "</body>");
            }

            remainingTextLength = 2000 - ((TextBox)sender).Text.Length - additionalLines.Length;
            tbremainingTextLength.Text = remainingTextLength.ToString();
        }
    }
}
