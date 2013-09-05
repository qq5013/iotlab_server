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
    public partial class frmNewResponse : Form
    {
        IResponse callbackForm = null;
        public frmNewResponse()
        {
            InitializeComponent();
        }
        public frmNewResponse(IResponse callbackForm)
            : this()
        {
            this.callbackForm = callbackForm;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = this.txtName.Text;
            string cmd = this.txtCmd.Text;
            if (name == string.Empty || cmd == string.Empty)
            {
                MessageBox.Show("内容填写不完善");
                return;
            }
            CommandResponse res = new CommandResponse(name, cmd);
            if (callbackForm != null)
            {
                if (callbackForm.addResponse(res))
                {
                    this.Close();
                }
                else
                {
                    MessageBox.Show("添加出现错误！");
                    return;
                }
            }
        }

    }
}
