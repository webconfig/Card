using Comm.Util;
using System;
using System.IO;

namespace Comm
{
    public abstract class ServerMain
    {

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="conf"></param>
        public void LoadConf(BaseConf conf)
        {
            Log.Info("读取配置...");

            try
            {
                conf.Load();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "读取错误. ({0})", ex.Message);
                CliUtil.Exit(1);
            }
        }
    }
}
