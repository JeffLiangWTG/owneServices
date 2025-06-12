using System;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;

namespace CargoWise.eServices.USCustoms.InboundService
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : System.Configuration.Install.Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
			this.InboundServiceInstaller.ServiceName = GetConfigurationValue("ServiceName");
		}

		string GetConfigurationValue(string key)
		{
			var service = Assembly.GetAssembly(typeof(ProjectInstaller));
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
