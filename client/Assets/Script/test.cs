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
    }


    void Client_ConnectOkEvent()
    {
        Debug.Log("ConnectOk");
        ClientLogin model_login = new ClientLogin();
        model_login.UserName = "kkk";
        model_login.Password = "54321";
        NetHelp.Send<ClientLogin>(Op.Client.Login, model_login, Connection.Client.socket);
    }
}
