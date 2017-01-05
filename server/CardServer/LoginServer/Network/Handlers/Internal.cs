using Comm.Network;
using Comm.Util;
using google.protobuf;
using LoginServer;
using LoginServer.Database;
using System;

namespace LoginServer.Network.Handlers
{
    public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
    {
        [PacketHandler(Op.Internal.ChannelToLogin)]
        public void ChannelToLogin(BaseClient<LoginClient> client, byte[] datas)
        {
            //Register reg_model;
            //NetHelp.RecvData<Register>(datas, out reg_model);
            //Comm.Util.Log.WriteLine(Comm.Util.LogLevel.Debug, "Channel pwd：{0}", reg_model.Password);

            //client.State = ClientState.LoggedIn;
            //lock (LoginServer.Instance.ChannelClients)
            //    LoginServer.Instance.ChannelClients.Add(client);



            //RegisterResult result = new RegisterResult();
            //result.Success = true;
            //NetHelp.Send<RegisterResult>(Op.Internal.LoginResult, result, client.socket);
        }
        [PacketHandler(Op.Internal.ChannelStatus)]
        public void Internal_ChannelStatus(BaseClient<LoginClient> client, byte[] datas)
        {
            //if (client.State != ClientState.LoggedIn)
            //    return;

            //ChannelData info_model;
            //NetHelp.RecvData<ChannelData>(datas, out info_model);
            //var server =LoginServer.Instance.ServerList.Add(info_model.ChannelServer);

            //ChannelInfo channel;
            //server.Channels.TryGetValue(info_model.ChannelName, out channel);
            //if (channel == null)
            //{
            //    channel = new ChannelInfo(info_model.ChannelName, info_model.ChannelServer, info_model.ChannelHost, info_model.ChannelPort);
            //    server.Channels.Add(info_model.ChannelName, channel);

            //    Log.Info("New channel registered: {0}", channel.FullName);
            //}

            //// A way to identify the channel of this client
            //if (client.Account == null)
            //{
            //    client.Account = new Account();
            //    client.Account.Name = channel.FullName;
            //}

            //if ((int)channel.State != info_model.state)
            //{
            //    Log.Status("Channel '{0}' is now in '{1}' mode.", channel.FullName, info_model.state);
            //}

            //channel.Host = info_model.ChannelHost;
            //channel.Port = info_model.ChannelPort;
            //channel.Users = info_model.cur;
            //channel.MaxUsers = info_model.max;
            //channel.LastUpdate = DateTime.Now;
            //channel.State =(ChannelState)info_model.state;

            ////给所有的玩家广播
            ////Send.ChannelStatus(LoginServer.Instance.ServerList.List);
            ////Send.Internal_ChannelStatus(LoginServer.Instance.ServerList.List);
        }
    }
}
