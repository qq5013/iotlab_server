using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IOTSimulator
{
    public partial class frmResponseDetail : Form
    {
        public frmResponseDetail()
        {
            InitializeComponent();
        }
        public frmResponseDetail(string name, string cmd)
            : this()
        {
            this.txtName.Text = name;
            this.txtCmd.Text = cmd;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
