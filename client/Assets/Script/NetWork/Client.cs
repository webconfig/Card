using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;

public  class Client
{
    private const int BufferSize = 1024 * 5;
    private Queue<Packet> queue;
    public Socket socket;
    private byte[] buffer;
    private List<byte> AllDatas;
    public ConnectionState State { get; private set; }
    public event CallBack ConnectOkEvent;
    public Client()
    {
        this.queue = new Queue<Packet>();
        this.buffer = new byte[BufferSize];
        AllDatas = new List<byte>();
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

    //private void HandShake()
    //{
    //    // Get seed
    //    var length = this.socket.Receive(this.buffer);
    //    if (length != 4)
    //        throw new Exception("Invalid seed length.");

    //    //var seed = BitConverter.ToInt32(this.buffer, 0);

    //    // Last 4 byte is the checksum
    //    this.socket.Send(new byte[] { 0x88, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04 });

    //    // Get handshake response
    //    length = this.socket.Receive(this.buffer);
    //    if (length != 7 || this.buffer[0] != 0x88 || this.buffer[6] != 0x07) // 88 07 00 00 00 00 07
    //        throw new Exception("Invalid handshake response");

    //    this.BeginReceive();
    //}

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
                        HandleBuffer(this, command, msgBytes);
                    }
                    else { break; }
                }
                else { break; }
            } while (true);

            this.BeginReceive();
        }
        catch (ObjectDisposedException)
        {
            
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

    /// <summary>
    /// 处理包数据
    /// </summary>
    /// <param name="client"></param>
    /// <param name="buffer"></param>
    private void HandleBuffer(Client client, int command, byte[] buffer)
    {
        var packet = new Packet(command, buffer);

        lock (this.queue)
            this.queue.Enqueue(packet);
    }
    #endregion

    public List<Packet> GetPacketsFromQueue()
    {
        var result = new List<Packet>();

        lock (this.queue)
        {
            result.AddRange(this.queue);
            this.queue.Clear();
        }

        return result;
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

