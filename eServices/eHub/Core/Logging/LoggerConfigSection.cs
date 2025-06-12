using System.Configuration;

namespace CargoWise.eHub.Core.Logging
{
	public class LoggerConfigSection : ConfigurationSection
	{
		[ConfigurationProperty("loggers", IsRequired = true)]
		public LoggerConfigCollection Loggers
		{
			get
			{
				return (LoggerConfigCollection)this["loggers"];
			}
			set
			{
				this["loggers"] = value;
			}
		}

		[ConfigurationProperty("logDir", IsRequired = true)]
		public string LogDir
		{
			get
			{
				return (string)this["logDir"];
			}
			set
			{
				this["logDir"] = value;
			}
		}

		[ConfigurationProperty("defaultLevel", DefaultValue = "INFO")]
		public string DefaultLevel
		{
			get
			{
				return (string)this["defaultLevel"];
			}
			set
			{
				this["defaultLevel"] = value;
			}
		}

		[ConfigurationProperty("defaultMaxSizeRollBackups", DefaultValue = 10)]
		public int DefaultMaxSizeRollBackups
		{
			get
			{
				return (int)this["defaultMaxSizeRollBackups"];
			}
			set
			{
				this["defaultMaxSizeRollBackups"] = value;
			}
		}

		[ConfigurationProperty("defaultMaximumFileSize", DefaultValue = "2MB"), RegexStringValidator(@"^\d+(KB|MB|GB)$")]
		public string DefaultMaximumFileSize
		{
			get
			{
				return (string)this["defaultMaximumFileSize"];
			}
			set
			{
				this["defaultMaximumFileSize"] = value;
			}
		}
	}
}
