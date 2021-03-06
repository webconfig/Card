﻿using Comm.Network;
using google.protobuf;
using Comm.Util;
using System.Net.Sockets;

namespace LoginServer.Network.Handlers
{
    public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
    {
        [PacketHandler(Op.Client.Login)]
        public void ClientLogin(BaseClient<LoginClient> client, byte[] datas)
        {
            ClientLogin model_login;
            NetHelp.RecvData<ClientLogin>(datas, out model_login);
            Log.Debug("用户名：{0},密码：{1}", model_login.UserName, model_login.Password);

            ClientResult result = new ClientResult();
            result.Result = true;
            client.Send<ClientResult>(Op.Client.Login, result);
            
        }
    }
}
