namespace Enterprise.TransportCommon.Integration
{
	// tested in TransportRegistryTest
	public struct ServerUsernamePassword
	{
		public ServerUsernamePassword(string userName, string userNameWithoutServer, string password, string serverName)
		{
			UserName = userName;
			UserNameWithoutServer = userNameWithoutServer;
			Password = password;
			ServerName = serverName;
		}

		public string UserName { get; }
		public string UserNameWithoutServer { get; }
		public string Password { get; }
		public string ServerName { get; }
	}
}
