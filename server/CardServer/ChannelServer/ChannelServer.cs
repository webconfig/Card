using ChannelServer.Configuration;
using ChannelServer.Network;
using Comm;
using Comm.Network;
using Comm.Util;
using System;
using System.Net.Sockets;
using System.Threading;
using google.protobuf;
using ChannelServer.Commands;
using ChannelServer.Network.Handlers;

namespace ChannelServer
{
    public class ChannelServer : ServerMain
    {
        public static readonly ChannelServer Instance = new ChannelServer();
        /// <summary>
        /// 服务器
        /// </summary>
        public BaseServer<ChannelClient> Server { get; protected set; }
        /// <summary>
        /// 配置
        /// </summary>
		public ChannelConf Conf { get; private set; }
        private bool _running;

        public ChannelConsoleCommands ConsoleCommands { get; private set; }

        //public PokerManager Poker;

        private ChannelServer()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            this.Server = new BaseServer<ChannelClient>();
            this.Server.Handlers = new ChannelServerHandlers();
            this.Server.Handlers.AutoLoad();
            this.Server.ClientDisconnected += this.OnClientDisconnected;

            //this.ServerList = new ServerInfoManager();

            //this.CommandProcessor = new GmCommandManager();
            this.ConsoleCommands = new ChannelConsoleCommands();

            //this.ScriptManager = new ScriptManager();
            //this.SkillManager = new SkillManager();
            //this.Events = new EventManager();
            //this.Weather = new WeatherManager();
            //this.PartyManager = new PartyManager();
            //this.GuildManager = new GuildManager();

            //this.Timer = new Timer(new TimerCallback(ShutdownTimerDone));
        }

        public void Run()
        {
            if (_running)
                throw new Exception("Server is already running.");

            CliUtil.WriteHeader("Channel Server", ConsoleColor.DarkGreen);
            CliUtil.LoadingTitle();

            // Conf
            this.LoadConf(this.Conf = new ChannelConf());

            //// Database
            //this.InitDatabase(this.Database = new ChannelDb(), this.Conf);

            //// Data
            //this.LoadData(DataLoad.ChannelServer, false);

            // Localization
            //this.LoadLocalization(this.Conf);

            //// World
            //this.InitializeWorld();

            //// Skills
            //this.LoadSkills();

            //// Scripts
            //this.LoadScripts();

            //// Weather
            //this.Weather.Initialize();

            //// Autoban
            //if (this.Conf.Autoban.Enabled)
            //    this.Events.SecurityViolation += (e) => Autoban.Incident(e.Client, e.Level, e.Report, e.StackReport);

            // Start
            this.Server.Start(this.Conf.Channel.ChannelPort);

            // Inter
            this.ConnectToLogin(true);
            //this.StartStatusUpdateTimer();

            CliUtil.RunningTitle();
            _running = true;

            // Commands
            this.ConsoleCommands.Wait();
        }

        #region 连接登陆服务器
        /// <summary>
        /// 连接登陆服务器
        /// </summary>
        public ChannelClient LoginServer { get; private set; }
        /// <summary>
        /// 重连登陆服务器间歇时间
        /// </summary>
        private const int LoginTryTime = 10 * 1000;
        /// <summary>
        /// 连接登陆服务器
        /// </summary>
        /// <param name="firstTime"></param>
        public void ConnectToLogin(bool firstTime)
        {
            if (this.LoginServer != null && this.LoginServer.State == ClientState.LoggedIn)
                throw new Exception("已经连接到登陆服务器.");

            Log.WriteLine();

            if (firstTime)
                Log.Info("开始连接登陆服务器 {0}:{1}...", ChannelServer.Instance.Conf.Channel.LoginHost, ChannelServer.Instance.Conf.Channel.LoginPort);
            else
            {
                Log.Info("{0} 秒后重新连接登陆服务器.", LoginTryTime / 1000);
                Thread.Sleep(LoginTryTime);
            }

            var success = false;
            while (!success)
            {
                try
                {
                    if (this.LoginServer != null && this.LoginServer.State != ClientState.Dead)
                        this.LoginServer.Kill();

                    this.LoginServer = new ChannelClient();
                    this.LoginServer.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.LoginServer.Socket.Connect(ChannelServer.Instance.Conf.Channel.LoginHost, ChannelServer.Instance.Conf.Channel.LoginPort);
                    this.Server.AddReceivingClient(this.LoginServer);
                    this.LoginServer.State = ClientState.LoggingIn;
                    success = true;
                    //发送注册包
                    Register res_model = new Register();
                    res_model.Password = "12345";
                    NetHelp.Send<Register>(Op.Internal.ChannelToLogin, res_model, this.LoginServer.Socket);
                }
                catch (Exception ex)
                {
                    Log.Error("开启服务器错误. ({0})", ex.Message);
                    Log.Info("{0} 秒后重新连接登陆服务器.", LoginTryTime / 1000);
                    Thread.Sleep(LoginTryTime);
                }
            }

            Log.Info("成功连接登陆服务器:'{0}'", this.LoginServer.Address);
            Log.WriteLine();
        }
        #endregion

        /// <summary>
        /// 处理异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Error("Oh no! Ferghus escaped his memory block and infected the rest of the server!");
                Log.Error("Aura has encountered an unexpected and unrecoverable error. We're going to try to save as much as we can.");
            }
            catch { }
            try
            {
                this.Server.Stop();
            }
            catch { }
            try
            {
                // save the world
            }
            catch { }
            try
            {
                Log.Exception((Exception)e.ExceptionObject);
                Log.Status("Closing server.");
            }
            catch { }

            CliUtil.Exit(1, false);
        }

        
        private void OnClientDisconnected(ChannelClient client)
        {
            if (client == this.LoginServer)
                this.ConnectToLogin(false);
        }
    }
}
