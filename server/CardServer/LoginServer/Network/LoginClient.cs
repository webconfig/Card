using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    public class LoginClient :Comm.Network.BaseClient
    {
        public string Ident { get; set; }
        //public Account Account { get; set; }

        public override void CleanUp()
        {
            //if (this.Account != null)
            //    LoginServer.Instance.Database.SetAccountLoggedIn(this.Account.Name, false);
        }
    }
}
