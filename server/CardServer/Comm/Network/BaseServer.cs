using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Comm.Util;
using Comm.Network.Iocp;

namespace Comm.Network
{
	/// <summary>
    /// 服务器基类
    /// </summary>
	public class BaseServer<TClient> where TClient : BaseClient,new()
    {
        /// <summary>
        /// 监听Socket，用于接受客户端的连接请求
        /// </summary>
        private Socket listenSocket;

        /// <summary>
        /// 用于服务器执行的互斥同步对象
        /// </summary>
        private static Mutex mutex = new Mutex();

        /// <summary>
        /// 用于每个I/O Socket操作的缓冲区大小
        /// </summary>
        public Int32 bufferSize;

        /// <summary>
        /// 服务器上连接的客户端总数
        /// </summary>
        private Int32 numConnectedSockets;

        /// <summary>
        /// 服务器能接受的最大连接数量
        /// </summary>
        private Int32 numConnections;

        /// <summary>
        /// 完成端口上进行投递所用的IoContext对象池
        /// </summary>
        private IoContextPool<TClient> ioContextPool;

        //=================================
        public PacketHandlerManager<TClient> Handlers { get; set; }
        public List<TClient> Clients=new List<TClient>();
        //==================================

        /// <summary>
        /// 构造函数，建立一个未初始化的服务器实例
        /// </summary>
        /// <param name="numConnections">服务器的最大连接数据</param>
        /// <param name="bufferSize"></param>
        public BaseServer(Int32 numConnections, Int32 bufferSize)
        {
            this.numConnectedSockets = 0;
            this.numConnections = numConnections;
            this.bufferSize = bufferSize;

            this.ioContextPool = new IoContextPool<TClient>(numConnections);

            // 为IoContextPool预分配SocketAsyncEventArgs对象
            for (Int32 i = 0; i < this.numConnections; i++)
            {
                SocketAsyncEventArgs ioContext = new SocketAsyncEventArgs();
                ioContext.SetBuffer(new Byte[this.bufferSize], 0, this.bufferSize);
                TClient client = new TClient();
                client.Init(ioContext);
                this.ioContextPool.Add(client);
            }
        }

        #region 开始结束
        /// <summary>
        /// 启动服务，开始监听
        /// </summary>
        /// <param name="port">Port where the server will listen for connection requests.</param>
        public void Start(Int32 port)
        {
            // 获得主机相关信息
            IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;
            IPEndPoint localEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);

            // 创建监听socket
            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.ReceiveBufferSize = this.bufferSize;
            this.listenSocket.SendBufferSize = this.bufferSize;

            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                // 配置监听socket为 dual-mode (IPv4 & IPv6) 
                // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,
                this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                this.listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
            }
            else
            {
                this.listenSocket.Bind(localEndPoint);
            }

            // 开始监听
            this.listenSocket.Listen(this.numConnections);

            // 在监听Socket上投递一个接受请求。
            this.StartAccept(null);

            // Blocks the current thread to receive incoming messages.
            mutex.WaitOne();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            this.listenSocket.Close();
            mutex.ReleaseMutex();
        }
        #endregion

        #region 接受连接
        /// <summary>
        /// 从客户端开始接受一个连接操作
        /// </summary>
        /// <param name="acceptEventArg">The context object to use when issuing 
        /// the accept operation on the server's listening socket.</param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
            {
                // 重用前进行对象清理
                acceptEventArg.AcceptSocket = null;
            }

            if (!this.listenSocket.AcceptAsync(acceptEventArg))
            {
                this.ProcessAccept(acceptEventArg);
            }
        }
        /// <summary>
        /// accept 操作完成时回调函数
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }
        /// <summary>
        /// 有客户端连接上
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket s = e.AcceptSocket;
            if (s.Connected)
            {
                try
                {
                    TClient client = this.ioContextPool.Pop();
                    if (client != null)
                    {
                        Interlocked.Increment(ref this.numConnectedSockets);
                        Console.WriteLine(String.Format("客户 {0} 连入, 共有 {1} 个连接。", s.RemoteEndPoint.ToString(), this.numConnectedSockets));
                        client.socket = s;
                        Clients.Add(client);
                        client.BeginRecv();//开始接受数据
                    }
                    else//已经达到最大客户连接数量，在这接受连接，发送“连接已经达到最大数”，然后断开连接
                    {
                        s.Send(Encoding.Default.GetBytes("连接已经达到最大数!"));
                        string outStr = String.Format("连接已满，拒绝 {0} 的连接。", s.RemoteEndPoint);
                        //mainForm.Invoke(mainForm.setlistboxcallback, outStr);
                        s.Close();
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(String.Format("接收客户 {0} 数据出错, 异常信息： {1} 。", s.RemoteEndPoint, ex.ToString()));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("异常：" + ex.ToString());
                }
                // 投递下一个接受请求
                this.StartAccept(e);
            }
        }
        #endregion

        #region 关闭客户端连接
        public void CloseClientSocket(TClient client)
        {
            Interlocked.Decrement(ref this.numConnectedSockets);
            this.ioContextPool.Push(client); // SocketAsyncEventArg 对象被释放，压入可重用队列。
            Console.WriteLine(String.Format("客户 {0} 断开, 共有 {1} 个连接。", client.socket.RemoteEndPoint.ToString(), this.numConnectedSockets));
            try
            {
                client.socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
                // Throw if client has closed, so it is not necessary to catch.
            }
            finally
            {
                client.socket.Close();
            }
        }
        #endregion
	}
}
