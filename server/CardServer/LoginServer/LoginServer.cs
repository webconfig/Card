using System;
using Comm;
using Comm.Network;
using LoginServer.Network.Handlers;
using Comm.Util;
using System.Collections.Generic;

namespace LoginServer
{
    public class LoginServer : ServerMain
    {
        public static readonly LoginServer Instance = new LoginServer();
        private bool _running = false;

        public BaseServer<LoginClient> Server { get; set; }
        public LoginConf Conf { get; private set; }
        public ServerInfoManager ServerList { get; private set; }
        public List<LoginClient> ChannelClients { get; private set; }
        private LoginServer()
        {
            this.Server = new BaseServer<LoginClient>(2000,1024*5);
            this.Server.Handlers = new LoginServerHandlers();
            this.Server.Handlers.AutoLoad();
            this.ServerList = new ServerInfoManager();
            this.ChannelClients = new List<LoginClient>();
        }
        public void Run()
        {
            if (_running)
                throw new Exception("服务器正在运行...");

            CliUtil.WriteHeader("Login Server", ConsoleColor.Magenta);
            CliUtil.LoadingTitle();
            //// Conf
            this.LoadConf(this.Conf = new LoginConf());

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

            // Start
            this.Server.Start(this.Conf.Login.Port);

            CliUtil.RunningTitle();
            _running = true;

            // Commands
            var commands = new LoginConsoleCommands();
            commands.Wait();
        }
    }
}
