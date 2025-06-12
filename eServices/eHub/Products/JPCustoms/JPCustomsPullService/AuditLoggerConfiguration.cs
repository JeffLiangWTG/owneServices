using System;
using System.Configuration;
using System.IO;
using CargoWise.eHub.Products.JPCustoms.Common;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.PullService
{
	public class AuditLoggerConfiguration : IAuditLoggerConfiguration
	{
		readonly IDateTimeProvider dateTimeProvider;

		public AuditLoggerConfiguration(IDateTimeProvider dateTimeProvider)
		{
			if (dateTimeProvider == null) throw new ArgumentNullException("dateTimeProvider");
			this.dateTimeProvider = dateTimeProvider;
		}

		public IDateTimeProvider DateTimeProvider
		{
			get { return dateTimeProvider; }
		}

		public System.IO.DirectoryInfo AuditLogFolder
		{
			get
			{
				return new DirectoryInfo(ConfigurationManager.AppSettings["AuditLogFolder"]);
			}
		}

		public string FileNamePattern
		{
			get
			{
				return ConfigurationManager.AppSettings["FileNamePattern"];
			}
		}

		public ILog Logger { get; private set; }

		public TimeSpan PullInterval
		{
			get
			{
				return new TimeSpan(0, Convert.ToInt32(ConfigurationManager.AppSettings["PullIntervalInMinutes"]), 0);
			}
		}
	}
}