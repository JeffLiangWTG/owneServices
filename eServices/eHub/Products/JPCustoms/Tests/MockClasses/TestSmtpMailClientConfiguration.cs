using System;
using CargoWise.eHub.Products.JPCustoms.Client;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	public class TestSmtpMailClientConfiguration : ISmtpMailClientConfiguration
	{
		readonly ILog logger;

		public TestSmtpMailClientConfiguration(ILog logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
		}

		public string Server
		{
			get
			{
				return "127.0.0.1";
			}
		}

		public int Port
		{
			get
			{
				return 25;
			}
		}

		public ILog Logger
		{
			get
			{
				return logger;
			}
		}

		public string EmailFrom
		{
			get
			{
				return "sender@test.com";
			}
		}

		public string EmailTo
		{
			get
			{
				return "recipient@test.com";
			}
		}

		public string DomainName
		{
			get
			{
				return "localhost";
			}
		}
	}
}
