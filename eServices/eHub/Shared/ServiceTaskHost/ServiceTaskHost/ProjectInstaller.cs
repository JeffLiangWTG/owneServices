using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace CargoWise.eHub.Shared.ServiceTaskHost
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : Installer
	{
		public ProjectInstaller()
		{
			var serviceTaskHostProjectInstaller = new ServiceProcessInstaller
			{
				Password = null,
				Username = null
			};

			Installers.Add(serviceTaskHostProjectInstaller);
			AddServiceInstallers();
		}

		void AddServiceInstallers()
		{
			var serviceTaskDirectory = ServiceTaskHelper.GetServiceTaskDirectory();

			var configFiles = serviceTaskDirectory.GetFiles("*.config", SearchOption.AllDirectories);
			if (configFiles.Length != 1)
				throw new ConfigurationErrorsException(string.Format("Should be only one config file in {0}", serviceTaskDirectory));
			var configMap = new ExeConfigurationFileMap {ExeConfigFilename = configFiles[0].FullName};
			var configuration = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
			var serviceTaskName = configuration.AppSettings.Settings["ServiceTaskName"].Value;
			if (string.IsNullOrWhiteSpace(serviceTaskName))
				throw new ConfigurationErrorsException(string.Format("Config file in {0} doesn't have ServiceTaskName appSetting", serviceTaskDirectory.Name));

			var serviceTaskHostInstaller = new ServiceInstaller
			{
				DelayedAutoStart = true,
				Description = serviceTaskName,
				DisplayName = serviceTaskName,
				ServiceName = serviceTaskName,
				StartType = ServiceStartMode.Automatic
			};

			Installers.Add(serviceTaskHostInstaller);
		}
	}
}