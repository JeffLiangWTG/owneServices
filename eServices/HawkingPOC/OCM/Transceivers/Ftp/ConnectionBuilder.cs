using FluentFTP;

namespace OcmPoc.Transceivers.Ftp
{
	class ConnectionBuilder : BaseConnectionBuilder<FtpConfiguration>
	{
		FtpConnection connection;

		public ConnectionBuilder()
			: base(new FtpConfiguration())
		{
		}

		protected virtual void Build()
		{
			if (connection == null)
			{
				connection = new FtpConnection(
					new FtpClient(Configuration.Host, Configuration.Port, Configuration.UserName, Configuration.Password),
					Configuration);
			}
		}

		public override IReceivingConnection BuildReceivingConnection()
		{
			//Build();
			return new FtpConnection(
					new FtpClient(Configuration.Host, Configuration.Port, Configuration.UserName, Configuration.Password),
					Configuration);
		}

		public override ISendingConnection BuildSendingConnection()
		{
			//Build();
			return new FtpConnection(
					new FtpClient(Configuration.Host, Configuration.Port, Configuration.UserName, Configuration.Password),
					Configuration);
		}
	}
}
