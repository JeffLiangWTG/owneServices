using System;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public interface IJPCustomsPullClient
	{
		void Connect();
		int MessageCount { get; }
		string GetNextMessage();
		void Delete();
		void Quit();
		void Disconnect();
	}

	public class JPCustomsPullClient : IJPCustomsPullClient
	{
		readonly IPop3MailClientConfiguration configuration;
		Pop3MailClient client;

		public JPCustomsPullClient(IPop3MailClientConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			this.configuration = configuration;
		}

		public void Connect()
		{
			client = GetNewPop3MailClient();
			client.Connect();
		}

		public int MessageCount
		{
			get
			{
				return client.MessageCount;
			}
		}

		public string GetNextMessage()
		{
			return client.GetNextMessage();
		}

		public void Delete()
		{
			client.Delete();
		}

		public void Disconnect()
		{
			client.Dispose();
		}

		protected virtual Pop3MailClient GetNewPop3MailClient()
		{
			return new Pop3MailClient(configuration);
		}

		public void Quit()
		{
			client.Diconnect();
		}
	}
}
