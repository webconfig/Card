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
using System.Threading.Tasks;
using System.Net;

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
        private ChannelServer()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            ChannelServerHandlers Handlers = new ChannelServerHandlers();
            Handlers.AutoLoad();
            this.Server = new BaseServer<ChannelClient>(2000, 1024 * 2, Handlers);


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

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
        }

        public void Run()
        {
            if (_running)
                throw new Exception("Server is already running.");
            //标题栏
            CliUtil.WriteHeader("Channel Server" + DateTime.Now.ToString(), ConsoleColor.DarkGreen);
            CliUtil.LoadingTitle();
            //配置文件
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

            //服务器开启
            this.Server.Start(this.Conf.Channel.ChannelPort);

            //连接登陆服务器
            this.ConnectToLogin(true);
            //this.StartStatusUpdateTimer();

            CliUtil.RunningTitle();
            _running = true;

            //GM操作
            this.ConsoleCommands.Wait();
        }

        #region 连接登陆服务器
        /// <summary>
        /// 连接登陆服务器
        /// </summary>
        public BaseClient<LoginClientModel> LoginClient { get; private set; }
        /// <summary>
        /// 重连登陆服务器间歇时间
        /// </summary>
        private const int LoginTryTime = 10 * 1000;
        private const int LoginClientBuffSize = 1024 * 2;
        /// <summary>
        /// 连接登陆服务器
        /// </summary>
        /// <param name="firstTime"></param>
        public void ConnectToLogin(bool firstTime)
        {
            if (this.LoginClient != null && this.LoginClient.State == ClientState.LoggedIn)
                throw new Exception("已经连接到登陆服务器.");

            Log.WriteLine();

            if (firstTime)
                Log.Info("开始连接登陆服务器 {0}:{1}...", ChannelServer.Instance.Conf.Channel.LoginHost, ChannelServer.Instance.Conf.Channel.LoginPort);
            else
            {
                Log.Info("{0} 秒后重新连接登陆服务器.", LoginTryTime / 1000);
                Thread.Sleep(LoginTryTime);
            }

            try
            {
                if (this.LoginClient == null)
                {
                    SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
                    socketAsyncEventArgs.SetBuffer(new Byte[LoginClientBuffSize], 0, LoginClientBuffSize);
                    LoginClientHandlers lch = new LoginClientHandlers();
                    lch.AutoLoad();
                    this.LoginClient = new BaseClient<LoginClientModel>();
                    this.LoginClient.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.LoginClient.Init(socketAsyncEventArgs, LoginClientBuffSize, lch.Handle, LoginClientClose);
                }
                this.LoginClient.Connect(new System.Net.IPEndPoint(IPAddress.Parse(ChannelServer.Instance.Conf.Channel.LoginHost), ChannelServer.Instance.Conf.Channel.LoginPort), ConnOk);
            }
            catch (Exception ex)
            {
                Log.Error("开启服务器错误. ({0})", ex.Message);
                Log.Info("{0} 秒后重新连接登陆服务器.", LoginTryTime / 1000);
                Thread.Sleep(LoginTryTime);
            }
        }
        public void ConnOk()
        {
            this.LoginClient.State = ClientState.LoggingIn;
            //发送注册包
            Register res_model = new Register();
            res_model.Password = "12345";
            LoginClient.Send<Register>(Op.Internal.ChannelToLogin, res_model);
            Log.Info("成功连接登陆服务器:'{0}'", ChannelServer.Instance.Conf.Channel.LoginHost);
            Log.WriteLine();
        }

        public void HandleLoginMsg(BaseClient<LoginClientModel> client, int command, byte[] bytes)
        {

        }
        public void LoginClientClose(BaseClient<LoginClientModel> client)
        {
            try
            {
                client.socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
                // Throw if client has closed, so it is not necessary to catch.
            }
            finally
            {
                client.socket.Close();
            }
        }
        #endregion
    }

}
