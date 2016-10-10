﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Comm.Util.Files
{
	/// <summary>
	/// Represents database.conf
	/// </summary>
	public class DatabaseConfFile : ConfFile
	{
		public string Host { get; protected set; }
		public int Port { get; protected set; }
		public string User { get; protected set; }
		public string Pass { get; protected set; }
		public string Db { get; protected set; }
		

		public void Load()
		{
			this.Require("system/conf/database.conf");

			this.Host = this.GetString("host", "127.0.0.1");
			this.Port = this.GetInt("port", 3306);
			this.User = this.GetString("user", "root");
			this.Pass = this.GetString("pass", "");
			this.Db = this.GetString("database", "aura");
		}
	}
}
