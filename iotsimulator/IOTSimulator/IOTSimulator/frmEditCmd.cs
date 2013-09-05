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
    public partial class frmEditCmd : Form, IResponse
    {
        List<CommandResponse> resList = new List<CommandResponse>();
        frmMain callbackForm = null;
        Command command = null;
        public frmEditCmd()
        {
            InitializeComponent();
        }
        public frmEditCmd(frmMain _callbackForm, Command _cmd)
            : this()
        {
            this.callbackForm = _callbackForm;
            this.command = _cmd;

            this.txtCmd.Text = command.cmd;
            this.txtName.Text = command.name;

            foreach (CommandResponse cr in command.responseList)
            {
                this.lbRes.Items.Add(cr.name);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            //string name = this.txtName.Text;
            //string cmd = this.txtCmd.Text;
            //if (name == string.Empty || cmd == string.Empty)
            //{
            //    MessageBox.Show("内容填写不完善");
            //    return;
            //}
            //Command newCmd = new Command(name, cmd);
            //newCmd.responseList.AddRange(resList);
            ////newCmd.addResponse(new CommandResponse("light is on", "on"));
            ////newCmd.addResponse(new CommandResponse("light is off", "off"));
            //if (this.callbackForm != null)
            //{
            //    bool r = callbackForm.addNewCmd(newCmd);
            //    if (!r)
            //    {
            //        MessageBox.Show("添加时出现错误！");
            //        return;
            //    }
            //    else
            //    {
            //        this.Close();
            //    }
            //}
        }
        public bool addResponse(CommandResponse _res)
        {
            if (command.addResponse(_res))
            {
                this.lbRes.Items.Add(_res.name);
                this.callbackForm.refreshResponseList();
                return true;
            }
            else
            {
                return false;
            }
            //if (command.responseList.Exists(
            //    (r) =>
            //    {
            //        return _res.name == r.name || _res.cmd == r.cmd;
            //    }))
            //{
            //    return false;
            //}
            //else
            //{
            //    command.responseList.Add(_res);
            //    this.lbRes.Items.Add(_res.name);
            //    return true;
            //}
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
                command.removeResponse(name);
            }
            this.callbackForm.refreshResponseList();
        }
    }
}
