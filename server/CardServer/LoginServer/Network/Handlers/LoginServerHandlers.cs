using Comm.Network;
using google.protobuf;

namespace LoginServer.Network.Handlers
{
    public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
    {
        [PacketHandler(Op.Internal.ChannelToLogin)]
        public void ChannelToLogin(LoginClient client, byte[] datas)
        {
            Register reg_model;
            NetHelp.RecvData<Register>(datas, out reg_model);
            Comm.Util.Log.WriteLine(Comm.Util.LogLevel.Debug, "Channel pwd：{0}", reg_model.Password);

            RegisterResult result = new RegisterResult();
            result.Success = true;
            NetHelp.Send<RegisterResult>(Op.Internal.LoginResult, result, client.Socket);
        }
    }
}
