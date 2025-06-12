using System;
using System.Configuration;
using CargoWise.eHub.Products.JPCustoms.Client;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.PullService
{
	public class Pop3MailClientConfiguration : IPop3MailClientConfiguration
	{
		readonly ILog logger;

		public Pop3MailClientConfiguration(ILog logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
		}

		public string Server
		{
			get
			{
				return ConfigurationManager.AppSettings["Server"];
			}
		}

		public int Port
		{
			get
			{
				return Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
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
				return ConfigurationManager.AppSettings["UserName"];
			}
		}

		public string Password
		{
			get
			{
				return ConfigurationManager.AppSettings["Password"];
			}
		}

		public int HeaderLineNumberToSkip
		{
			get
			{
				return Convert.ToInt32(ConfigurationManager.AppSettings["HeaderLineNumberToSkip"]);
			}
		}
	}
}
