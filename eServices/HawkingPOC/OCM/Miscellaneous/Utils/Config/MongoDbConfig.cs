namespace OcmPoc.Utils.Config
{
	public class MongoDbConfig
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string AuthDb { get; set; }
		public string DatabaseName { get; set; }
		public string MessagesCollection { get; set; }
	}
}
