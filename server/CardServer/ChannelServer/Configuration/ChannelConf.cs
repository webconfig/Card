using Comm.Util;
using System;

namespace ChannelServer.Configuration
{
    public sealed class ChannelConf : BaseConf
    {

        /// <summary>
        /// channel.conf
        /// </summary>
        public ChannelConfFile Channel { get; private set; }

        public ChannelConf()
        {
            this.Channel = new ChannelConfFile();
        }

        public override void Load()
        {
            this.Channel.Load();
        }
    }
    public class ChannelConfFile : ConfFile
    {
        public string LoginHost { get; protected set; }
        public int LoginPort { get; protected set; }

        public string ChannelServer { get; protected set; }
        public string ChannelName { get; protected set; }
        public string ChannelHost { get; protected set; }
        public int ChannelPort { get; protected set; }
        public int MaxUsers { get; protected set; }

        public void Load()
        {
            this.Require("channel.conf");

            this.LoginHost = this.GetString("login_host", "127.0.0.1");
            this.LoginPort = this.GetInt("login_port", 11000);

            this.ChannelServer = this.GetString("channel_server", "Aura");
            this.ChannelName = this.GetString("channel_name", "Ch1");
            this.ChannelHost = this.GetString("channel_host", "127.0.0.1");
            this.ChannelPort = this.GetInt("channel_port", 11020);
            this.MaxUsers = Math.Max(1, this.GetInt("max_users", 20));
        }
    }
}
