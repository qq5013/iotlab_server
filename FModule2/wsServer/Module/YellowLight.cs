using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ModuleCommand;
using Server;
using nsUHF;
using System.Collections.Generic;
using System.Net;
using wsServer;
using Fleck;


namespace ModuleService
{
    public class YellowLightService : WebSocketService
    {
        public static string recently_broadcast = string.Empty;
        command last_command = null;
        public static command last_effective_command = null;
        public YellowLightService(WebSocketServiceManager _manager, IWebSocketConnection socket)
        {
            services.register_service("yellow_light", this);
            this.ID = socket.ConnectionInfo.Id.ToString();
            this._manager = _manager;
            this._websocket = socket;
            this._context = socket.ConnectionInfo;
        }
        public override void OnOpen()
        {
            if (last_effective_command != null)
            {
                recently_broadcast = JsonConvert.SerializeObject(last_effective_command);
                this.Send(recently_broadcast);
            }
            //if (recently_broadcast != string.Empty)
            //{
            //    //this.Broadcast(recently_broadcast); 

            //}
        }
        public override void OnMessage(string msg)
        {
            Debug.WriteLine(string.Format("YellowLight OnMessage => {0}", msg));
            try
            {
                command cmd = (command)JsonConvert.DeserializeObject(msg, typeof(command));
                Debug.WriteLine(cmd.print_string());
                last_command = cmd;
                switch (cmd.Name)
                {
                    case "open":
                        �򿪵�(Program.getRemoteIPEndPoint());
                        Debug.WriteLine(string.Format("{0} ��ͼ�򿪻Ƶ�", cmd.Commander));
                        break;
                    case "close":
                        �رյ�(Program.getRemoteIPEndPoint());
                        Debug.WriteLine(string.Format("{0} �رջƵ�", cmd.Commander));
                        break;
                }
                ����״̬(Program.getRemoteIPEndPoint());
            }
            catch
            {
                Debug.WriteLine("parse error!");
            }
        }
        public override void OnClose()
        {
            //base.OnClose(e);
        }

        void �رյ�(IPEndPoint ipEndPoint)
        {
            DeviceCommandManager.executeCommand(enumDeviceCommand.�رջƵ�, ipEndPoint);

        }
        void �򿪵�(IPEndPoint ipEndPoint)
        {
            DeviceCommandManager.executeCommand(enumDeviceCommand.�򿪻Ƶ�, ipEndPoint);

        }
        void ����״̬(IPEndPoint ipEndPoint)
        {
            DeviceCommandManager.setCommandCallback(enumDeviceCommand.��ѯ�Ƶ�״̬,
               (data) =>
               {
                   Debug.WriteLine("�Ƶ�״̬ => " + data);
                   IDeviceCommand idc = DeviceCommandManager.getDeivceCommand(enumDeviceCommand.��ѯ�Ƶ�״̬);
                   if (null != idc)
                   {
                       LightState ls = idc.parseResponse(data);
                       if (null != ls && last_command != null)
                       {
                           bool temp = (last_command.Name == "open") ? true : false;
                           if (ls.State == temp)//�����ڵ�״̬�Ͳ���Ŀ��״̬��ͬ��ͬΪ�ػ��߿�
                           {
                               switch (last_command.Name)
                               {
                                   case "open":
                                       Debug.WriteLine(string.Format("{0} �򿪻Ƶ�", last_command.Commander));
                                       command ncOpen = new command("open", string.Format("{0}���˵�", last_command.Commander));
                                       last_effective_command = ncOpen;
                                       recently_broadcast = JsonConvert.SerializeObject(ncOpen);
                                       this.Broadcast(recently_broadcast);
                                       break;
                                   case "close":
                                       Debug.WriteLine(string.Format("{0} �رջƵ�", last_command.Commander));
                                       command ncClose = new command("close", string.Format("{0}�ر��˵�", last_command.Commander));
                                       last_effective_command = ncClose;
                                       recently_broadcast = JsonConvert.SerializeObject(ncClose);
                                       this.Broadcast(recently_broadcast);
                                       break;
                               }
                           }
                           else//˵���豸û����Ӧ��֪ͨ�ͻ���
                           {
                               switch (ls.State)
                               {
                                   case true:
                                       command ncOpen = new command("open", "����ʧ��");
                                       recently_broadcast = JsonConvert.SerializeObject(ncOpen);
                                       this.Send(recently_broadcast);//ֻ֪ͨ����
                                       Debug.WriteLine("�رջƵƲ���ʧ��");
                                       break;
                                   case false:
                                       command ncClose = new command("close", "����ʧ��");
                                       recently_broadcast = JsonConvert.SerializeObject(ncClose);
                                       this.Send(recently_broadcast);
                                       Debug.WriteLine("�򿪻ƵƲ���ʧ��");
                                       break;
                               }
                           }
                       }
                   }
               });
            DeviceCommandManager.executeCommand(enumDeviceCommand.��ѯ�Ƶ�״̬, ipEndPoint, 1000);
        }
    }
}
