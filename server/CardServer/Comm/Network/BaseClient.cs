using System;
using System.Net.Sockets;
using Comm.Util;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ProtoBuf;

namespace Comm.Network
{
    /// <summary>
    /// 客户端基类
    /// </summary>
    public class BaseClient<TModel>
    {
        public TModel t;
        private int bufferSize;
        public SocketAsyncEventArgs asyn;
        public Socket socket { get; set; }
        public List<byte> AllDatas;
        public ClientState State { get; set; }
        public BaseClient()
        {

        }
        public void Init(SocketAsyncEventArgs _asyn,int _bufferSize, CallBack<BaseClient<TModel>, int, byte[]> _Handle, CallBack<BaseClient<TModel>> _CloseSocket)
        {
            bufferSize = _bufferSize;
            asyn = _asyn;
            asyn.Completed += Asyn_Completed;
            Handle = _Handle;
            CloseSocket = _CloseSocket;
            AllDatas = new List<byte>();
        }
        private void Asyn_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    if(ConnectOk!=null)
                    {
                        ConnectOk();
                        ConnectOk = null;
                    }
                    asyn.RemoteEndPoint = null;
                    this.BeginRecv();
                    break;
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive();
                    break;
                case SocketAsyncOperation.Send:
                    this.ProcessSend();
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        #region 连接
        public CallBack ConnectOk;
        public void Connect(IPEndPoint ipEndPoint, CallBack _ConnectOk)
        {
            ConnectOk = _ConnectOk;
            asyn.RemoteEndPoint = ipEndPoint;
            if (!socket.ConnectAsync(asyn))
            {
                asyn.RemoteEndPoint = null;
                this.BeginRecv();
            }
        }
        #endregion


        #region 接受数据
        public CallBack<BaseClient<TModel>, int, byte[]> Handle;
        public void BeginRecv()
        {
            if (!socket.ReceiveAsync(asyn))
            {
                this.ProcessReceive();
            }
        }
        /// <summary>
        ///接收完成时处理函数
        /// </summary>
        /// <param name="e">与接收完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessReceive()
        {
            // 检查远程主机是否关闭连接
            if (asyn.BytesTransferred > 0)
            {
                if (asyn.SocketError == SocketError.Success)
                {
                    //判断所有需接收的数据是否已经完成
                    if (socket.Available == 0)
                    {
                        //获取接收到的数据
                        //byte[] ByteArray = new byte[asyn.BytesTransferred];
                        //Array.Copy(asyn.Buffer, 0, ByteArray, 0, ByteArray.Length);

                        //拷贝到缓存队列
                        for (int i = 0; i < asyn.BytesTransferred; i++)
                        {
                            AllDatas.Add(asyn.Buffer[i]);
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
                                    Handle(this, command, msgBytes);
                                }
                                else { break; }
                            }
                            else { break; }
                        } while (true);
                    }
                    else if (!socket.ReceiveAsync(asyn))    //为接收下一段数据，投递接收请求，这个函数有可能同步完成，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                    {
                        // 同步接收时处理接收完成事件
                        this.ProcessReceive();
                    }
                }
                else
                {
                    this.ProcessError();
                }
            }
            else
            {
                CloseSocket(this);
            }
        }
        #endregion

        #region 发送
        public void  Send<T>(int type, T t)
        {
            //============生成数据============
            byte[] msg;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<T>(ms, t);
                msg = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(msg, 0, msg.Length);
            }
            byte[] type_value = IntToBytes(type);
            byte[] Length_value = IntToBytes(msg.Length + type_value.Length);
            //消息体结构：消息体长度+消息体
            byte[] data = new byte[Length_value.Length + type_value.Length + msg.Length];
            Length_value.CopyTo(data, 0);
            type_value.CopyTo(data, 4);
            msg.CopyTo(data, 8);
            //============发送数据============
            asyn.SetBuffer(asyn.Offset, data.Length);
            Array.Copy(data, 0, asyn.Buffer, 0, data.Length);
            asyn.SetBuffer(asyn.Offset, data.Length);
            if (!socket.SendAsync(asyn))//投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
            {
                // 同步发送时处理发送完成事件
                this.ProcessSend();
            }
        }

        /// <summary>
        /// 发送完成时处理函数
        /// </summary>
        /// <param name="e">与发送完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessSend()
        {
            if (asyn.SocketError == SocketError.Success)
            {
                //接收时根据接收的字节数收缩了缓冲区的大小，因此投递接收请求时，恢复缓冲区大小
                asyn.SetBuffer(0, bufferSize);
                if (!socket.ReceiveAsync(asyn))     //投递接收请求
                {
                    // 同步接收时处理接收完成事件
                    this.ProcessReceive();
                }
            }
            else
            {
                this.ProcessError();
            }
        }
        #endregion

        #region 错误
        public CallBack<BaseClient<TModel>> CloseSocket;
        /// <summary>
        /// 处理socket错误
        /// </summary>
        /// <param name="e"></param>
        private void ProcessError()
        {
            IPEndPoint localEp = socket.LocalEndPoint as IPEndPoint;
            CloseSocket(this);
            Console.WriteLine(String.Format("套接字错误 {0}, IP {1}, 操作 {2}。", (Int32)asyn.SocketError, localEp, asyn.LastOperation));
        }
        #endregion

        public static byte[] IntToBytes(int num)
        {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = (byte)(num >> (24 - i * 8));
            }
            return bytes;
        }
    }

    /// <summary>
    /// 客户端状态
    /// </summary>
	public enum ClientState { BeingChecked, LoggingIn, LoggedIn, Dead }
}
