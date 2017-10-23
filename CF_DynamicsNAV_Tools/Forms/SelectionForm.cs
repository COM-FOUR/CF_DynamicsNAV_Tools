using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;

namespace CF_DynamicsNAV_Tools
{
    public partial class SelectionForm : Form
    {
        public string SelectedText { get; set; }

        public SelectionForm()
        {
            InitializeComponent();
        }
        public SelectionForm(string inittext)
        {
            InitializeComponent();

            //wb_output.DocumentText = inittext;
            //tb_input.Text = wb_output.DocumentText;

            tb_input.Text = inittext.Replace(">", ">" + Environment.NewLine);
        }

        private void tb_input_TextChanged(object sender, EventArgs e)
        {
            wb_output.DocumentText = tb_input.Text;
        }

        private void tb_input_MouseCaptureChanged(object sender, EventArgs e)
        {
            wb_output.DocumentText = tb_input.SelectedText;
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            SelectedText = tb_input.SelectedText;
        }
    }
}
