using google.protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class LoginServerHandlers : PacketHandlerManager
{
    [PacketHandler(Op.Client.Login)]
    public void ClientLogin(Client client, byte[] datas)
    {
        ClientResult model_login;
        NetHelp.RecvData<ClientResult>(datas, out model_login);
        UnityEngine.Debug.Log("登陆返回结果：" + model_login.Result);
    }
}
