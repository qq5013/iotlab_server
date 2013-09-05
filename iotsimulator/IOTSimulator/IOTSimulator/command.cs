using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTSimulator
{
    public class CommandList
    {
        public static List<Command> List = new List<Command>();

        public static Command getCommand(string strCmd)
        {
            Command cmd = List.Find(
                (c) =>
                {
                    return c.cmd == strCmd;
                });
            return cmd;
        }
        public static bool addCommand(Command _newCommand)
        {
            if (List.Exists(
                (res) =>
                {
                    return res.name == _newCommand.name || res.cmd == _newCommand.cmd;
                }))
            {
                return false;
            }
            List.Add(_newCommand);
            return true;
        }
        public static bool deleteCommand(Command _cmd)
        {
            return List.Remove(_cmd);
        }
        public static string getCommandJson(List<Command> list)
        {
            return JsonConvert.SerializeObject(list);
        }
        public static void importCommand()
        {
            try
            {
                string strReadFilePath3 = @"./db.txt";
                StreamReader srReadFile3 = new StreamReader(strReadFilePath3);
                string Command = srReadFile3.ReadToEnd();
                srReadFile3.Close();
                List = (List<Command>)JsonConvert.DeserializeObject<List<Command>>(Command);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public static void exportCommand()
        {
            try
            {
                string data = CommandList.getCommandJson(CommandList.List);
                string strReadFilePath1 = @"./db.txt";
                StreamWriter srWriteFile1 = new StreamWriter(strReadFilePath1);
                srWriteFile1.Write(data);
                srWriteFile1.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }

    /// <summary>
    /// 设备接收的命令
    /// </summary>
    public class Command
    {
        public string name;
        public string cmd;
        public List<CommandResponse> responseList = new List<CommandResponse>();
        public Command() { }
        public Command(string _name, string _cmd)
        {
            this.name = _name;
            this.cmd = _cmd;
        }
        public string getCommandResponse(string resName)
        {
            CommandResponse cr = this.responseList.Find(
                (res) =>
                {
                    return res.name == resName;
                });
            if (cr != null)
            {
                return cr.cmd;
            }
            return null;
        }

        public bool addResponse(CommandResponse _res)
        {
            if (this.responseList.Exists(
                (res) =>
                {
                    return res.name == _res.name || res.cmd == _res.cmd;
                }))
            {
                return false;
            }
            this.responseList.Add(_res);
            return true;
        }
        public bool removeResponse(string _name)
        {
            Predicate<CommandResponse> test =
                (res) =>
                {
                    return res.name == _name;
                };

            CommandResponse cr = this.responseList.Find(test);

            if (cr != null)
            {
                return this.responseList.Remove(cr);
            }
            return false;
        }
        public void setResponseDefault(string _name)
        {
            Predicate<CommandResponse> test =
                (res) =>
                {
                    return res.name == _name;
                };

            CommandResponse cr = this.responseList.Find(test);

            if (cr != null)
            {
                this.responseList.Remove(cr);
                this.responseList.Insert(0, cr);
            }
        }
    }

    public class CommandResponse
    {
        public string name;
        public string cmd;
        public CommandResponse() { }
        public CommandResponse(string _name, string _cmd)
        {
            this.name = _name;
            this.cmd = _cmd;
        }
    }
}
