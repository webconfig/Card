using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Comm.Util;

namespace Comm.Network
{
	/// <summary>
    /// 服务器基类
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
	public class BaseServer<TClient> where TClient : BaseClient, new()
	{
		private Socket _socket;
		public List<TClient> Clients { get; set; }
        public PacketHandlerManager<TClient> Handlers { get; set; }
        /// <summary>
        /// 客户端连接
        /// </summary>
        public event ClientConnectionEventHandler ClientConnected;

		/// <summary>
        /// 客户端断开连接
        /// </summary>
		public event ClientConnectionEventHandler ClientDisconnected;

        public BaseServer()
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.NoDelay = true;
			this.Clients = new List<TClient>();
		}

        #region 启动
        public void Start(int port)
		{
			this.Start(new IPEndPoint(IPAddress.Any, port));
		}
		public void Start(string host, int port)
		{
			this.Start(new IPEndPoint(IPAddress.Parse(host), port));
		}
		private void Start(IPEndPoint endPoint)
		{
			try
			{
				_socket.Bind(endPoint);
				_socket.Listen(10);

                //_socket.BeginAccept(this.OnAccept, _socket);
                AcceptAsync();

                Log.Status("Server ready, listening on {0}.", _socket.LocalEndPoint);
			}
			catch (Exception ex)
			{
                if (this._socket != null)
                {
                    this._socket.Close();
                }
                Log.Exception(ex, "Unable to set up socket; perhaps you're already running a server?");
				//CliUtil.Exit(1);
			}
		}
        #endregion

        #region 接受连接
        private void AcceptAsync()
        {
            try
            {
                if (this._socket != null)
                {
                    SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(this.AcceptAsyncCompleted);
                    this._socket.AcceptAsync(e);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex,"AcceptAsync is error!");
            }
        }
        private void AcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {

            var client = new TClient();
            client.Socket = e.AcceptSocket;
            try
            {
                client.Socket = e.AcceptSocket;
                client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, this.OnReceive, client);
                this.AddClient(client);
                Log.Info("客户端建立连接->{0}", client.Address);
                this.OnClientConnected(client);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "While accepting connection.");
            }
            finally
            {
                if (sock != null)
                {
                    try
                    {
                        sock.Close();
                    }
                    catch
                    {
                    }
                }
                e.Dispose();
                this.AcceptAsync();
            }

        }
        #endregion

        /// <summary>
        /// 关闭
        /// </summary>
        public void Stop()
		{
			try
			{
				_socket.Shutdown(SocketShutdown.Both);
				_socket.Close();
			}
			catch
			{ }
		}

		///// <summary>
		///// 建立一个新的连接
		///// </summary>
		///// <param name="result"></param>
		//private void OnAccept(IAsyncResult result)
		//{
		//	var client = new TClient();

		//	try
		//	{
		//		client.Socket = (result.AsyncState as Socket).EndAccept(result);
		//		client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, this.OnReceive, client);
		//		this.AddClient(client);
		//		Log.Info("客户端建立连接->{0}", client.Address);
		//		this.OnClientConnected(client);
		//	}
		//	catch (ObjectDisposedException)
		//	{
		//	}
		//	catch (Exception ex)
		//	{
		//		Log.Exception(ex, "While accepting connection.");
		//	}
		//	finally
		//	{
		//		_socket.BeginAccept(this.OnAccept, _socket);
		//	}
		//}

		///// <summary>
		///// Starts receiving for client.
		///// </summary>
		///// <param name="client"></param>
		//public void AddReceivingClient(TClient client)
		//{
		//	client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, this.OnReceive, client);
		//}

		/// <summary>
        /// 接受客户端数据
        /// </summary>
        /// <param name="result"></param>
		protected void OnReceive(IAsyncResult result)
		{
			var client = result.AsyncState as TClient;
			try
			{
				int bytesReceived = client.Socket.EndReceive(result);
				if (bytesReceived == 0)
				{
					Log.Info("连接被关闭->{0}", client.Address);
					this.KillAndRemoveClient(client);
					this.OnClientDisconnected(client);
					return;
				}
                //拷贝到缓存队列
                for (int i = 0; i < bytesReceived; i++)
                {
                    client.AllDatas.Add(client.Buffer[i]);
                }
                //===解析数据===
                int len = 0, command=0;
                do
                {
                    if (client.AllDatas.Count > 7)//最小的包应该有8个字节
                    {
                        NetHelp.BytesToInt(client.AllDatas, 0, ref len);//读取消息体的长度
                        len += 4;
                        //读取消息体内容
                        if (len <= client.AllDatas.Count)
                        {
                            NetHelp.BytesToInt(client.AllDatas, 4, ref command);//操作命令
                            byte[] msgBytes = new byte[len - 8];
                            client.AllDatas.CopyTo(8, msgBytes, 0, msgBytes.Length);
                            client.AllDatas.RemoveRange(0, len);
                            HandleBuffer(client, command, msgBytes);
                        }
                        else { break; }
                    }
                    else { break; }
                } while (true);
                //客户端自己关闭
                if (client.State == ClientState.Dead)
				{
					Log.Info("Killed connection from '{0}'.", client.Address);
					this.RemoveClient(client);
					this.OnClientDisconnected(client);
					return;
				}
				client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, this.OnReceive, client);
			}
			catch (SocketException)
			{
				Log.Info("Connection lost from '{0}'.", client.Address);
				this.KillAndRemoveClient(client);
				this.OnClientDisconnected(client);
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "While receiving data from '{0}'.", client.Address);
				this.KillAndRemoveClient(client);
				this.OnClientDisconnected(client);
			}
		}

        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="client"></param>
        protected void AddClient(TClient client)
        {
            lock (this.Clients)
            {
                this.Clients.Add(client);
                //Log.Status("Connected clients: {0}", _clients.Count);
            }
        }

        /// <summary>
        /// 关闭并且移除客户端
        /// </summary>
        /// <param name="client"></param>
        protected void KillAndRemoveClient(TClient client)
		{
			client.Kill();
			this.RemoveClient(client);
		}

		/// <summary>
        /// 删除客户端
        /// </summary>
        /// <param name="client"></param>
		protected void RemoveClient(TClient client)
		{
			lock (this.Clients)
			{
				this.Clients.Remove(client);
				//Log.Status("Connected clients: {0}", _clients.Count);
			}
		}

        /// <summary>
        /// 处理包数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
		private void HandleBuffer(TClient client,int command, byte[] buffer)
        {
            try
            {
                this.Handlers.Handle(client, command, buffer);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "There has been a problem while handling '{0:X4}', '{1}'.", command, Op.GetName(command));
            }
        }

		protected virtual void OnClientConnected(TClient client)
		{
			if (this.ClientConnected != null)
				this.ClientConnected(client);
		}

		protected virtual void OnClientDisconnected(TClient client)
		{
			if (this.ClientDisconnected != null)
				this.ClientDisconnected(client);
		}

		public delegate void ClientConnectionEventHandler(TClient client);
	}
}
