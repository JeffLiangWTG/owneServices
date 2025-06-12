using System.Configuration;

namespace CargoWise.eHub.Core.Logging
{
	public class LoggerConfigItem : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name
		{
			get
			{
				return (string)this["name"];
			}
			set
			{
				this["name"] = value;
			}
		}

		[ConfigurationProperty("level", DefaultValue = "INFO")]
		public string Level
		{
			get
			{
				return (string)this["level"];
			}
			set
			{
				this["level"] = value;
			}
		}

		[ConfigurationProperty("maxSizeRollBackups", DefaultValue = 10)]
		public int MaxSizeRollBackups
		{
			get
			{
				return (int)this["maxSizeRollBackups"];
			}
			set
			{
				this["maxSizeRollBackups"] = value;
			}
		}

		[ConfigurationProperty("maximumFileSize", DefaultValue = "2MB"), RegexStringValidator(@"^\d+(KB|MB|GB)$")]
		public string MaximumFileSize
		{
			get
			{
				return (string)this["maximumFileSize"];
			}
			set
			{
				this["maximumFileSize"] = value;
			}
		}
	}
}
