using System;
using CargoWise.eHub.Products.JPCustoms.Client;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	public class TestPop3MailClientConfiguration : IPop3MailClientConfiguration
	{
		readonly ILog logger;

		public TestPop3MailClientConfiguration(ILog logger)
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
				return 110;
			}
		}

		public ILog Logger
		{
			get
			{
				return logger;
			}
		}

		public string UserName
		{
			get
			{
				return "recipient1";
			}
		}

		public string Password
		{
			get
			{
				return "123";
			}
		}

		public int HeaderLineNumberToSkip
		{
			get
			{
				return headerLineNumberToSkip;
			}
			set
			{
				headerLineNumberToSkip = value;
			}
		}

		int headerLineNumberToSkip = 9;
	}
}
