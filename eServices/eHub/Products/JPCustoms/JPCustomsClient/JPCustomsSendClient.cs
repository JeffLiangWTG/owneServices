using System;
using System.IO;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public interface IJPCustomsSendClient
	{
        void Send(Stream message);
        bool Ping();
	}

	public class JPCustomsSendClient : IJPCustomsSendClient
	{
		readonly ISmtpMailClientConfiguration configuration;

		public JPCustomsSendClient(ISmtpMailClientConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			this.configuration = configuration;
		}

		public void Send(Stream message)
		{
			using (var client = GetSmtpMailClient())
			{
				client.Send(message);
			}
		}

	    public bool Ping()
	    {
	        using (var client = GetSmtpMailClient())
	        {
	            return client.Ping();
	        }
	    }

		protected virtual SmtpMailClient GetSmtpMailClient()
		{
			return new SmtpMailClient(configuration);
		}
	}
}
