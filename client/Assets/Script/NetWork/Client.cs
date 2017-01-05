using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;

public  class Client
{
    private const int BufferSize = 1024 * 5;
    private Dictionary<int, Queue<Packet>> datas;
    public object datas_obj;
    public Socket socket;
    private byte[] buffer;
    private List<byte> AllDatas;
    public ConnectionState State { get; private set; }
    public event CallBack ConnectOkEvent;
    public PacketHandlerManager Handlers { get; set; }
    public Client()
    {
        this.datas = new Dictionary<int, Queue<Packet>>();
        this.buffer = new byte[BufferSize];
        AllDatas = new List<byte>();
        Handlers = new LoginServerHandlers();
        Handlers.AutoLoad();
    }

    #region 连接

    public bool Connect(string host, int port)
    {
        if (this.State == ConnectionState.Connected)
            this.Disconnect();

        this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.State = ConnectionState.Connecting;

        var success = false;

        try
        {
            this.socket.Connect(host, port);
            //this.HandShake();
            this.BeginReceive();
            success = true;
        }
        catch (SocketException ex)
        {
            Debug.LogException(ex);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        if (!success)
            this.State = ConnectionState.Disconnected;
        else
            this.State = ConnectionState.Connected;

        return success;
    }

    public void ConnectAsync(string host, int port)
    {
        if (this.State == ConnectionState.Connected)
            this.Disconnect();
        this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.buffer = new byte[BufferSize];
        this.State = ConnectionState.Connecting;
        this.socket.BeginConnect(host, port, this.OnConnect, null);
    }

    private void OnConnect(IAsyncResult result)
    {
        var success = false;

        try
        {
            this.socket.EndConnect(result);
            //this.HandShake();
            this.BeginReceive();
            success = true;
            if(ConnectOkEvent!=null)
            {
                ConnectOkEvent();
                ConnectOkEvent = null;
            }
        }
        catch (SocketException ex)
        {
            Debug.LogException(ex);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        if (!success)
            this.State = ConnectionState.Disconnected;
        else
            this.State = ConnectionState.Connected;
    }
    public void Disconnect()
    {
        if (this.socket == null)
            return;

        try
        {
            this.socket.Disconnect(false);
        }
        catch { }

        try
        {
            this.socket.Shutdown(SocketShutdown.Both);
        }
        catch { }

        try
        {
            this.socket.Close();
        }
        catch { }

        this.State = ConnectionState.Disconnected;
    }
    #endregion

    #region 接收数据
    private void BeginReceive()
    {
        this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, null);
    }

    private void OnReceive(IAsyncResult result)
    {
        try
        {
            int bytesReceived = this.socket.EndReceive(result);
            if (bytesReceived == 0)
            {
                this.State = ConnectionState.Disconnected;
                return;
            }
            //拷贝到缓存队列
            for (int i = 0; i < bytesReceived; i++)
            {
                AllDatas.Add(buffer[i]);
            }
            //===解析数据===
            int len = 0, command = 0;
            do
            {
                if (AllDatas.Count > 7)//最小的包应该有8个字节
                {
                    NetHelp.BytesToInt(AllDatas, 0, ref len);//读取消息体的长度
                    len += 4;
                    //读取消息体内容
                    if (len <= AllDatas.Count)
                    {
                        NetHelp.BytesToInt(AllDatas, 4, ref command);//操作命令
                        byte[] msgBytes = new byte[len - 8];
                        AllDatas.CopyTo(8, msgBytes, 0, msgBytes.Length);
                        AllDatas.RemoveRange(0, len);
                        Handlers.Handle(this, command, msgBytes);
                    }
                    else { break; }
                }
                else { break; }
            } while (true);

            this.BeginReceive();
        }
        catch (SocketException ex)
        {
            Debug.LogException(ex);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion

    public List<Packet> GetPacketsFromQueue(int entity_id)
    {
        lock (this.datas_obj)
        {
            if (datas.ContainsKey(entity_id))
            {
                var result = new List<Packet>();
                result.AddRange(datas[entity_id]);
                datas[entity_id].Clear();
                return result;
            }
            return null;
        }
    }

    public string GetLocalIp()
    {
        if (socket == null)
            return "?";

        return ((IPEndPoint)socket.LocalEndPoint).Address.ToString();
    }
}

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
}

