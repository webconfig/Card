using Comm.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoginServer.Network.Handlers
{
    public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
    {
        [PacketHandler(Op.Login)]
        public void Login(LoginClient client, byte[] datas)
        {

        }
    }
}
