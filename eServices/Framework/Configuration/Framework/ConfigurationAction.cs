using System.Linq;
using eServices.Configuration.Schemas;

namespace eServices.Configuration.Framework
{
	public enum ConfigurationAction
	{
		Insert,
		Update,
		Delete,
		Skip
	}

	public class ConfigurationActionHelper
	{
		public static ConfigurationAction GetConfigurationAction(Credential[] credentials, bool dbRecordExist)
		{
			if ((credentials == null || !credentials.Any()) && dbRecordExist)
				return ConfigurationAction.Delete;

			if (credentials != null && credentials.Any())
			{
				if (!dbRecordExist)
				{
					return ConfigurationAction.Insert;
				}

				return ConfigurationAction.Update;
			}

			return ConfigurationAction.Skip;
		}
	}
}
