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
    public partial class frmAddCmd : Form, IResponse
    {
        List<CommandResponse> resList = new List<CommandResponse>();
        frmMain callbackForm = null;
        public frmAddCmd()
        {
            InitializeComponent();
        }
        public frmAddCmd(frmMain _callbackForm)
            : this()
        {
            this.callbackForm = _callbackForm;
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
            Command newCmd = new Command(name, cmd);
            newCmd.responseList.AddRange(resList);
            //newCmd.addResponse(new CommandResponse("light is on", "on"));
            //newCmd.addResponse(new CommandResponse("light is off", "off"));
            if (this.callbackForm != null)
            {
                bool r = callbackForm.addNewCmd(newCmd);
                if (!r)
                {
                    MessageBox.Show("添加时出现错误！");
                    return;
                }
                else
                {
                    this.Close();
                }
            }
        }
        public bool addResponse(CommandResponse _res)
        {
            if (resList.Exists(
                (r) =>
                {
                    return _res.name == r.name || _res.cmd == r.cmd;
                }))
            {
                return false;
            }
            else
            {
                resList.Add(_res);
                this.lbRes.Items.Add(_res.name);
                return true;
            }
        }
        private void btnNewRes_Click(object sender, EventArgs e)
        {
            frmNewResponse frm = new frmNewResponse(this);
            frm.ShowDialog();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string name = (string)this.lbRes.SelectedItem;
            if (null != name)
            {
                this.resList.RemoveAll(
                    (res) =>
                    {
                        return res.name == name;
                    });
                this.lbRes.Items.Remove(name);
            }
        }
    }
}
