using System;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;

namespace CargoWise.eServices.USCustoms.OutboundProcessingService
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : System.Configuration.Install.Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
			this.OutboundProcessingServiceInstaller.ServiceName = GetConfigurationValue("ServiceName");
		}

		string GetConfigurationValue(string key)
		{
			Assembly service = Assembly.GetAssembly(typeof(ProjectInstaller));
			var config = ConfigurationManager.OpenExeConfiguration(service.Location);
			if (config.AppSettings.Settings[key] != null)
			{
				return config.AppSettings.Settings[key].Value;
			}
			else
			{
				throw new IndexOutOfRangeException
					("Settings collection does not contain the requested key: " + key);
			}
		}
	}
}
