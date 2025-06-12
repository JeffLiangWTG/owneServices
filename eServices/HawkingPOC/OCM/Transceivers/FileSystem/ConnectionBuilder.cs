namespace OcmPoc.Transceivers.FileSystem
{
	class ConnectionBuilder : BaseConnectionBuilder<FileSystemConfiguration>
	{ 
		FileSystemConnection connection;

		public ConnectionBuilder()
			: base(new FileSystemConfiguration())
		{
		}

		protected virtual void Build()
		{
			if (connection == null)
			{
				connection = new FileSystemConnection(Configuration);
			}
		}

		public override IReceivingConnection BuildReceivingConnection()
		{
			Build();
			return connection;
		}

		public override ISendingConnection BuildSendingConnection()
		{
			Build();
			return connection;
		}
	}
}
