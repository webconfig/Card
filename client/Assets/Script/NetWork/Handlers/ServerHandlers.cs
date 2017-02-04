using google.protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class LoginServerHandlers : PacketHandlerManager
{
    [PacketHandler(Op.Client.Login)]
    public void LoginResult(Client client, byte[] datas)
    {
        ClientResult model_login;
        NetHelp.RecvData<ClientResult>(datas, out model_login);
        UnityEngine.Debug.Log("登陆返回结果：" + model_login.Result);
    }

    [PacketHandler(Op.Client.CreateRoom)]
    public void CreateRoomResult(Client client, byte[] datas)
    {
        CreateGameResult model_login;
        NetHelp.RecvData<CreateGameResult>(datas, out model_login);
        UnityEngine.Debug.Log("创建房间结果：" + model_login.RoomId);
    }

    [PacketHandler(Op.Client.QueryRoom)]
    public void QueryRoom(Client client, byte[] datas)
    {
        QueryRoomResult model_query_result;
        NetHelp.RecvData<QueryRoomResult>(datas, out model_query_result);
        for (int i = 0; i < model_query_result.result.Count; i++)
        {
            UnityEngine.Debug.Log(string.Format("房间ID:{0},房间名:{1}", model_query_result.result[i].RoomId, model_query_result.result[i].RoomName));
        }
        
    }
}
