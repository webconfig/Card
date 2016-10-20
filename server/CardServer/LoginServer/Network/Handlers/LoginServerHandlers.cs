using Comm.Network;
using google.protobuf;
using Comm.Util;
namespace LoginServer.Network.Handlers
{
    public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
    {
        [PacketHandler(Op.Client.Login)]
        public void ClientLogin(LoginClient client, byte[] datas)
        {
            ClientLogin model_login;
            NetHelp.RecvData<ClientLogin>(datas, out model_login);
            Log.WriteLine(LogLevel.Debug, "用户名：{0},密码：{1}", model_login.UserName, model_login.Password);

            ClientResult result = new ClientResult();
            result.Result = true;
            NetHelp.Send<ClientResult>(Op.Client.Login, result, client.Socket);
        }
    }
}
