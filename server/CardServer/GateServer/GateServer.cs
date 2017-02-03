using System;
using Comm;
using Comm.Network;
using Comm.Util;

namespace GateServer
{
    public class GateServer : ServerMain
    {
        public static readonly GateServer Instance = new GateServer();
        private bool _running = false;
        public BaseServer<ConnClient> Server { get; set; }
        public GateConf Conf { get; private set; }
        private GateServer()
        {
            //LoginServerHandlers Handlers = new LoginServerHandlers();
            //Handlers.AutoLoad();
            //this.Server = new BaseServer<ConnClient>(2000, 1024 * 2, Handlers);
        }
        public void Run()
        {
            if (_running)
                throw new Exception("服务器正在运行...");
            //标题栏
            CliUtil.WriteHeader("Login Server " + DateTime.Now.ToString(), ConsoleColor.Magenta);
            CliUtil.LoadingTitle();
            //配置文件
            this.LoadConf(this.Conf = new GateConf());

            //// Database
            //this.InitDatabase(this.Database = new LoginDb(), this.Conf);

            //// Check if there are any updates
            //this.CheckDatabaseUpdates();

            //// Data
            //this.LoadData(DataLoad.LoginServer, false);

            //// Localization
            //this.LoadLocalization(this.Conf);

            //// Web API
            //this.LoadWebApi();

            //// Scripts
            //this.LoadScripts();

            //开启服务器
            this.Server.Start(this.Conf.Gate.Port);

            CliUtil.RunningTitle();
            _running = true;

            ////GM操作
            //var commands = new LoginConsoleCommands();
            //commands.Wait();
        }
    }
}
