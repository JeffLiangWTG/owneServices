using System;
using System.IO;
using System.Net.Sockets;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public abstract class MailClient : IDisposable
	{
		readonly IMailClientConfiguration configuration;
		TcpClient client;
		Stream mailStream;
		bool disposed = false;

		protected MailClient(IMailClientConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			this.configuration = configuration;
		}

		Stream GetMailStream()
		{
			client = new TcpClient(configuration.Server, configuration.Port) { SendTimeout = 1000, ReceiveTimeout = 1000 };
			return client.GetStream();
		}

		public string Read()
		{
			return Reader.ReadLine();
		}

		public void Write(string text)
		{
			Writer.WriteLine(text);
		}

		protected virtual StreamReader Reader
		{
			get
			{
				if (reader == null)
				{
					if (mailStream == null) mailStream = GetMailStream();
					reader = new StreamReader(mailStream);
				}

				return reader;
			}
		}
		StreamReader reader;

		protected virtual StreamWriter Writer
		{
			get
			{
				if (writer == null)
				{
					if (mailStream == null) mailStream = GetMailStream();
					writer = new StreamWriter(mailStream) { AutoFlush = true };
				}

				return writer;
			}
		}

		StreamWriter writer;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (client != null)
					{
						client.Close();
						client = null;
						mailStream = null;
						writer = null;
						reader = null;
					}
				}

				this.disposed = true;
			}
		}

		~MailClient()
		{
			Dispose(false);
		}
	}
}
