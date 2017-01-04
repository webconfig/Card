using System;
using System.Net.Sockets;
using Comm.Util;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Comm.Network
{
    /// <summary>
    /// 客户端基类
    /// </summary>
    public class BaseClient
    {
        public SocketAsyncEventArgs asyn;
        public Socket socket { get; set; }
        public List<byte> AllDatas;
        public ClientState State { get; set; }
        public BaseClient()
        {

        }
        public void Init(SocketAsyncEventArgs _asyn)
        {
            asyn = _asyn;
            asyn.Completed += Asyn_Completed;
            AllDatas = new List<byte>();
        }
        private void Asyn_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
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

        #region 接受数据
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
                                    //server.Handlers.Handle(this, command, msgBytes);
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
                //server.CloseClientSocket(this);
            }
        }
        #endregion

        #region 发送
        public void Send()
        {
            //设置发送数据（自定义数据）
            //byte[] bt = new byte[10] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };
            //e.SetBuffer(e.Offset, bt.Length);
            //Array.Copy(bt, 0, e.Buffer, 0, bt.Length);
            //e.SetBuffer(e.Offset, bt.Length);

            ////设置发送的数据（原样返回）
            //Array.Copy(e.Buffer, 0, e.Buffer, e.BytesTransferred, e.BytesTransferred);
            //e.SetBuffer(e.Offset, e.BytesTransferred);

            if (!socket.SendAsync(asyn))        //投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
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
                //asyn.SetBuffer(0, server.bufferSize);
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
        /// <summary>
        /// 处理socket错误
        /// </summary>
        /// <param name="e"></param>
        private void ProcessError()
        {
            IPEndPoint localEp = socket.LocalEndPoint as IPEndPoint;
            //server.CloseClientSocket(this);
            Console.WriteLine(String.Format("套接字错误 {0}, IP {1}, 操作 {2}。", (Int32)asyn.SocketError, localEp, asyn.LastOperation));
        }
        #endregion
    }
    /// <summary>
    /// 客户端状态
    /// </summary>
	public enum ClientState { BeingChecked, LoggingIn, LoggedIn, Dead }
}
