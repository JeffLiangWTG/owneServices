using System;
using System.Configuration;
using CargoWise.eHub.Products.JPCustoms.Client;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.Gateway
{
	public class SmtpMailClientConfiguration : ISmtpMailClientConfiguration
	{
		readonly ILog logger;

		public SmtpMailClientConfiguration(ILog logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
		}

		public string Server
		{
			get
			{
				return ConfigurationManager.AppSettings["SmtpServerName"];
			}
		}

		public int Port
		{
			get
			{
				return Convert.ToInt32(ConfigurationManager.AppSettings["SmtpServerPort"]);
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
				return ConfigurationManager.AppSettings["EmailFrom"];
			}
		}

		public string EmailTo
		{
			get
			{
				return ConfigurationManager.AppSettings["EmailTo"];
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