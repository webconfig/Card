using System.Reflection;
namespace Comm.Network
{
    /// <summary>
    /// All Op codes
    /// </summary>
    public static class Op
    {
        public static class Client
        {
            public const int Login = 0x10000001;
            public const int CreateRoom = 0x20000001;
            public const int QueryRoom = 0x20000002;
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
