using System.Reflection;
namespace Comm.Network
{
    /// <summary>
    /// All Op codes
    /// </summary>
    public static class Op
    {
        // Login Server
        // ------------------------------------------------------------------
        public const int Login = 0x0001;

        public static class Internal
        {
            public const int ChannelToLogin = 0x00000001;
            public const int LoginResult = 0x00000002;
        }


        /// <summary>
        /// Returns name of op code, if it's defined.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static string GetName(int op)
        {
            // Login/Channel
            foreach (var field in typeof(Op).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if ((int)field.GetValue(null) == op)
                    return field.Name;
            }

            //// Msgr
            //foreach (var field in typeof(Op.Msgr).GetFields(BindingFlags.Public | BindingFlags.Static))
            //{
            //    if ((int)field.GetValue(null) == op)
            //        return "Msgr." + field.Name;
            //}

            return "?";
        }
    }
}
