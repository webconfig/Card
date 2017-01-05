using Comm.Network;
using google.protobuf;
using Comm.Util;
namespace ChannelServer.Network.Handlers
{
    public partial class LoginClientHandlers : PacketHandlerManager<LoginClientModel>
    {
        /// <summary>
        /// 接受登陆服务器返回结果
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(Op.Internal.LoginResult)]
        public void Internal_ServerIdentifyR(BaseClient<LoginClientModel> client, byte[] datas)
        {
            RegisterResult result;
            NetHelp.RecvData<RegisterResult>(datas, out result);
            Log.WriteLine(LogLevel.Debug, "注册返回结果:{0}", result.Success);
            if (result.Success)
            {
                //client.State = ClientState.LoggedIn;
                ////发送服务器信息到登陆服务器
                //ChannelData info_model = new ChannelData();
                //info_model.ChannelServer = ChannelServer.Instance.Conf.Channel.ChannelServer;
                //info_model.ChannelName = ChannelServer.Instance.Conf.Channel.ChannelName;
                //info_model.ChannelHost = ChannelServer.Instance.Conf.Channel.ChannelHost;
                //info_model.ChannelPort = ChannelServer.Instance.Conf.Channel.ChannelPort;
                //info_model.cur = 0;
                //info_model.max = 10;
                //info_model.state = (int)ChannelState.Normal;
                //NetHelp.Send<ChannelData>(Op.Internal.ChannelStatus, info_model, client.Socket);
            }
            else
            {
                Log.Error("Server identification failed, check the password.");
                return;
            }
        }
    }
}
