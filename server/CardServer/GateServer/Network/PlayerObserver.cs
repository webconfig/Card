using CardGrainInterfaces;
using Comm.Network;
using Comm.Util;

namespace GateServer.Network
{
    public class PlayerObserver : IPlayerObserver
    {
        public BaseClient<ClientData> _client;

        public void Msg(byte[] datas)
        {
            Log.Debug("收到信息");
            _client.Send(Op.Client.LockSetp, datas);
        }
    }
}
