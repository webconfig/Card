using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using google.protobuf;

public class test : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 150, 100, 50), "连接"))
        {
            Connection.Client.ConnectOkEvent += Client_ConnectOkEvent;
            Connection.Client.ConnectAsync("192.168.2.100", 11000);
        }
        if (GUI.Button(new Rect(200, 150, 100, 50), "创建房间"))
        {
            Client_CreateRomme();
        }
        if (GUI.Button(new Rect(300, 150, 100, 50), "查询房间"))
        {
            Client_QueryRomme();
        }
    }


    void Client_ConnectOkEvent()
    {
        Debug.Log("ConnectOk");
        ClientLogin model_login = new ClientLogin();
        model_login.UserName = "kkk";
        model_login.Password = "54321";
        NetHelp.Send<ClientLogin>(Op.Client.Login, model_login, Connection.Client.socket);
    }

    void Client_CreateRomme()
    {
        CreateGameRequest model_create_room = new CreateGameRequest();
        model_create_room.UserName = "kkk";
        NetHelp.Send<CreateGameRequest>(Op.Client.CreateRoom, model_create_room, Connection.Client.socket);
    }

    void Client_QueryRomme()
    {
        QueryRoomRequest model_query_room = new QueryRoomRequest();
        model_query_room.RoomId = "kkk";
        NetHelp.Send<QueryRoomRequest>(Op.Client.QueryRoom, model_query_room, Connection.Client.socket);
    }
}
