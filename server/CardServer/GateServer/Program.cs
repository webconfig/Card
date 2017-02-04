using System;
using Comm.Util;
namespace GateServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GateServer.Instance.Run();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "An exception occured while starting the server.");
                CliUtil.Exit(1);
            }
        }
    }
}
