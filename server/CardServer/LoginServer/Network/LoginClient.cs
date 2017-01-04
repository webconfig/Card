using System.Net.Sockets;
using Comm.Network;
using LoginServer.Database;

namespace LoginServer
{
    public class LoginClient :Comm.Network.BaseClient
    {
        public LoginClient() : base()
        {
        }

        public string Ident { get; set; }
        public Account Account { get; set; }
    }
}
