using System;
using System.Net.Sockets;
using Comm.Util;
using System.Collections.Generic;

namespace Comm.Network
{
	/// <summary>
	/// 客户端基类
	/// </summary>
	public abstract class BaseClient
	{
		private const int BufferDefaultSize = 4096;

		public Socket Socket { get; set; }
		public byte[] Buffer { get; set; }
        public List<byte> AllDatas;
        public ClientState State { get; set; }
		private string _address;
		public string Address
		{
			get
			{
				if (_address == null)
				{
					try
					{
						_address = this.Socket.RemoteEndPoint.ToString();
					}
					catch
					{
						_address = "?";
					}
				}

				return _address;
			}
		}

		protected BaseClient()
		{
			this.Buffer = new byte[BufferDefaultSize];
            AllDatas = new List<byte>();
        }

        #region 发送数据
        public virtual void Send(byte[] buffer)
		{
			if (this.State == ClientState.Dead)
				return;

			//this.EncodeBuffer(buffer);

			//Log.Debug("out: " + BitConverter.ToString(buffer));

			try
			{
				this.Socket.Send(buffer);
			}
			catch (Exception ex)
			{
				Log.Error("Unable to send packet to '{0}'. ({1})", this.Address, ex.Message);
			}
		}
		//public virtual void Send(Packet packet)
		//{
		//	// Don't log internal packets
		//	//if (packet.Op < Op.Internal.ServerIdentify)
		//	//    Log.Debug("S: " + packet);

		//	this.Send(this.BuildPacket(packet));
		//}
        #endregion

		/// <summary>
		/// 关闭客户端连接
		/// </summary>
		public virtual void Kill()
		{
			if (this.State != ClientState.Dead)
			{
				try { this.Socket.Shutdown(SocketShutdown.Both); }
				catch { }

				try { this.Socket.Close(); }
				catch { }
				this.CleanUp();

				this.State = ClientState.Dead;
			}
			else
			{
				Log.Warning("Client got killed multiple times." + Environment.NewLine + Environment.StackTrace);
			}
		}
		public virtual void CleanUp()
		{

		}
	}
    /// <summary>
    /// 客户端状态
    /// </summary>
	public enum ClientState { BeingChecked, LoggingIn, LoggedIn, Dead }
}
