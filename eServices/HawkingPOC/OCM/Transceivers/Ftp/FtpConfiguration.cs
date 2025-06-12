namespace OcmPoc.Transceivers.Ftp
{
	public class FtpConfiguration : ConnectionConfigBase
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string SendDirectory { get; set; } = "/transfers/send";
		public string ReceiveDirectory { get; set; } = "/transfers/receive";
	}
}
