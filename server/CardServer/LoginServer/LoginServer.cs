﻿using System;
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
        private LoginServer()
        {
            LoginServerHandlers Handlers = new LoginServerHandlers();
            Handlers.AutoLoad();
            this.Server = new BaseServer<LoginClient>(2000,1024*2, Handlers);
            this.ServerList = new ServerInfoManager();
        }
        public void Run()
        {
            if (_running)
                throw new Exception("服务器正在运行...");
            //标题栏
            CliUtil.WriteHeader("Login Server "+DateTime.Now.ToString(), ConsoleColor.Magenta);
            CliUtil.LoadingTitle();
            //配置文件
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

            //开启服务器
            this.Server.Start(this.Conf.Login.Port);

            CliUtil.RunningTitle();
            _running = true;

            //GM操作
            var commands = new LoginConsoleCommands();
            commands.Wait();
        }
    }
}
