using CargoWise.RefDbRepo.Staging.Schedulers.Common;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers
{
	public static class ServerConfiguration
	{
		const string PrefixServerConfiguration = "quartz.server";
		const string KeyServiceName = PrefixServerConfiguration + ".serviceName";
		const string KeyServiceDisplayName = PrefixServerConfiguration + ".serviceDisplayName";
		const string KeyServiceDescription = PrefixServerConfiguration + ".serviceDescription";
		const string KeyServerImplementationType = PrefixServerConfiguration + ".type";

		const string DefaultServiceName = "QuartzServer";
		const string DefaultServiceDisplayName = "Quartz Server";
		const string DefaultServiceDescription = "Quartz Job Scheduling Server";
		static readonly string DefaultServerImplementationType = typeof(QuartzServer).AssemblyQualifiedName;


		public static string ServiceName => GetConfigurationOrDefault(KeyServiceName, DefaultServiceName);

		public static string ServiceDisplayName => GetConfigurationOrDefault(KeyServiceDisplayName, DefaultServiceDisplayName);

		public static string ServiceDescription => GetConfigurationOrDefault(KeyServiceDescription, DefaultServiceDescription);

		public static string ServerImplementationType => GetConfigurationOrDefault(KeyServerImplementationType, DefaultServerImplementationType);

		static string GetConfigurationOrDefault(string configurationKey, string defaultValue)
		{
			string retValue = null;
			if (ConfigurationProvider.QuartzProps != null)
			{
				retValue = ConfigurationProvider.QuartzProps[configurationKey];
			}
			if (retValue == null || retValue.Trim().Length == 0)
			{
				retValue = defaultValue;
			}
			return retValue;
		}
	}
}
