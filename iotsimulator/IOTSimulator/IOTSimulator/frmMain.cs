using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IOTSimulator
{
    public partial class frmMain : Form
    {

        #region 成员
        Socket serverSocket = null; //The main server socket
        Command currentCommand = null;
        byte[] byteData = new byte[1024];
        string resCommandName = string.Empty;
        #endregion

        public frmMain()
        {
            InitializeComponent();

            this.lbCmds.SelectedIndexChanged += lbCmds_SelectedIndexChanged;
            this.lbResList.SelectedIndexChanged +=
                (sender, e) =>
                {
                    resCommandName = (string)lbResList.SelectedItem;
                };
            this.Shown += frmMain_Shown;
            CommandList.importCommand();
        }

        public void testResponse()
        {
            string inputCmd = "A5030000000000";
            Debug.WriteLine("input => " + inputCmd);
            string resCmd = getResponse(inputCmd);
            Debug.WriteLine("response => " + resCmd);

            string CheckLight1State = "5A0180";
            Debug.WriteLine("CheckLight1State => " + CheckLight1State);
            resCmd = getResponse(CheckLight1State);
            Debug.WriteLine("response => " + resCmd);

        }

        /// <summary>
        /// 如果指定的返回项，则根据指定返回；否则查找所有命令，返回第一个返回项
        /// </summary>
        /// <param name="inputCmd"></param>
        /// <returns></returns>
        string getResponse(string _inputCmd)
        {
            Func<string, string> func = (inputCmd) =>
            {
                Command cmd = CommandList.getCommand(inputCmd);
                if (cmd == null) return null;

                if (currentCommand != null && cmd.name == currentCommand.name)
                {
                    string resName = (string)this.lbResList.SelectedItem;
                    if (resName != null)
                    {
                        string resCmd = cmd.getCommandResponse(resName);
                        return resCmd;
                    }
                }
                else
                {
                    if (cmd.responseList.Count > 0)
                    {
                        return cmd.responseList[0].cmd;
                    }
                }
                return null;
            };
            return this.Invoke(func, _inputCmd) as string;
        }

        #region 事件处理
        void frmMain_Shown(object sender, EventArgs e)
        {
            foreach (Command cmd in CommandList.List)
            {
                this.lbCmds.Items.Add(cmd.name);
            }
        }

        void lbCmds_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbCmds.SelectedIndex >= 0)
            {
                this.btnDelete.Enabled = true;
            }
            else
            {
                this.btnDelete.Enabled = false;
            }
            string cmdName = (string)this.lbCmds.SelectedItem;

            Command cmd = CommandList.List.Find((item) =>
            {
                return item.name == cmdName;
            });
            if (cmd != null)
            {
                currentCommand = cmd;

                this.txtCmdName.Text = cmd.name;
                this.txtCmd.Text = cmd.cmd;

                this.refreshResponseList();
            }

        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            frmAddCmd frm = new frmAddCmd(this);
            frm.ShowDialog();
        }

        private void btnDetail_Click(object sender, EventArgs e)
        {
            if (null != currentCommand)
            {
                CommandResponse cr = currentCommand.responseList.Find(
                    (_cr) =>
                    {
                        return _cr.name == resCommandName;
                    });
                if (cr != null)
                {
                    frmResponseDetail frm = new frmResponseDetail(cr.name, cr.cmd);
                    frm.ShowDialog();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string cmdName = currentCommand.name;
            if (CommandList.deleteCommand(currentCommand))
            {
                this.lbCmds.Items.Remove(cmdName);
                if (this.lbCmds.Items.Count > 0)
                {
                    this.lbCmds.SelectedIndex = 0;
                }
                else
                {
                    currentCommand = null;
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            frmEditCmd frm = new frmEditCmd(this, currentCommand);
            frm.ShowDialog();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            CommandList.exportCommand();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.testResponse();
        }

        #endregion
        #region 公共函数
        public void refreshResponseList()
        {
            this.lbResList.Items.Clear();

            foreach (CommandResponse cr in currentCommand.responseList)
            {
                this.lbResList.Items.Add(cr.name);
            }
            if (this.lbResList.Items.Count > 0)
            {
                this.lbResList.SelectedIndex = 0;
                this.btnDetail.Enabled = true;
            }
            else
            {
                this.btnDetail.Enabled = false;
            }
        }
        public bool addNewCmd(Command _newCommand)
        {
            if (CommandList.addCommand(_newCommand))
            {
                this.lbCmds.Items.Add(_newCommand.name);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion


        public void initial_udp_server(int port)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork,
                            SocketType.Dgram, ProtocolType.Udp);
                IPAddress ip = IPAddress.Parse(GetLocalIP4());
                IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
                //Bind this address to the server
                serverSocket.Bind(ipEndPoint);
                //防止客户端强行中断造成的异常
                long IOC_IN = 0x80000000;
                long IOC_VENDOR = 0x18000000;
                long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                byte[] optionInValue = { Convert.ToByte(false) };
                byte[] optionOutValue = new byte[4];
                serverSocket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);

                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                //The epSender identifies the incoming clients
                EndPoint epSender = (EndPoint)ipeSender;

                //Start receiving data
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length,
                    SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void OnReceive(IAsyncResult ar)
        {

            try
            {
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                serverSocket.EndReceiveFrom(ar, ref epSender);

                //IPAddress ip = ((IPEndPoint)epSender).Address;
                //int port = ((IPEndPoint)epSender).Port;

                string strReceived = Encoding.UTF8.GetString(byteData);

                Array.Clear(byteData, 0, byteData.Length);
                int i = strReceived.IndexOf("\0");
                if (i > 0)
                {
                    string data = strReceived.Substring(0, i);
                    Debug.WriteLine(" Data => SP: " + data);
                    this.addLog("=>   " + data);
                    //todo here should deal with the received string
                    string res = getResponse(data);
                    this.addLog("<=   " + res);

                    if (res != null)
                    {
                        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        byte[] _byteData = Encoding.UTF8.GetBytes(res);

                        clientSocket.BeginSendTo(_byteData, 0,
                                        _byteData.Length, SocketFlags.None,
                                        epSender, null, null);
                    }
                }
                //Start listening to the message send by the user
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender,
                    new AsyncCallback(OnReceive), epSender);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    string.Format("UDPServer.OnReceive  -> error = {0}"
                    , ex.Message));
            }
        }
        public string GetLocalIP4()
        {
            IPAddress ipAddress = null;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
            {
                ipAddress = ipHostInfo.AddressList[i];
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    break;
                }
                else
                {
                    ipAddress = null;
                }
            }
            if (null == ipAddress)
            {
                return null;
            }
            return ipAddress.ToString();
        }

        void addLog(string _data)
        {
            Action<string> func = (data) =>
            {
                this.txtLog.Text = data + "\r\n" + this.txtLog.Text;
            };
            this.Invoke(func, _data);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            this.btnRun.Enabled = false;
            this.btnRun.Text = "运行中...";
            initial_udp_server(19200);
        }

    }
}
