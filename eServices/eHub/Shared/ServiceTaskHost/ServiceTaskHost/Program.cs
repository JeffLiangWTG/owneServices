using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;
using CargoWise.eHub.Shared.ServiceTaskHost.Core;
using CargoWise.eHub.Shared.ServiceTaskHost.Integration;
using Common.Logging;
using log4net.Appender;

namespace CargoWise.eHub.Shared.ServiceTaskHost
{
	internal static class Program
	{
		static void Main()
		{
			try
			{
				var runner = GetServiceTaskRunners();
				var host = (ServiceBase)new ServiceTaskHost(runner);

				if (!Environment.UserInteractive)
				{
					ServiceBase.Run(host);
				}
				else
				{
					RunServiceInteractively(runner);
				}
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry("CargoWise.eHub.Shared.ServiceTaskHost", ex.ToString(), EventLogEntryType.Error);
			}
		}

		static IServiceTaskRunner GetServiceTaskRunners()
		{
			var serviceTaskDirectory = ServiceTaskHelper.GetServiceTaskDirectory();


			var configFiles = serviceTaskDirectory.GetFiles("*.config", SearchOption.AllDirectories);

			if (configFiles.Length != 1)
				throw new ConfigurationErrorsException(string.Format("Should be only one config file in {0}", serviceTaskDirectory));

			var configMap = new ExeConfigurationFileMap {ExeConfigFilename = configFiles[0].FullName};
			var configuration = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
			var serviceTaskTypeList = new List<Type>();

			var files = serviceTaskDirectory.GetFiles("*.dll", SearchOption.AllDirectories);

			foreach (var file in files)
			{
				var assemblyName = AssemblyName.GetAssemblyName(file.FullName);
				if (
					AppDomain.CurrentDomain.GetAssemblies()
						.Any(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), assemblyName))) continue;

				foreach (
					var serviceTaskType in
						Assembly.LoadFrom(file.FullName).GetTypes().Where(t => t.GetInterfaces().Contains(typeof (IServiceTask))))
				{
					serviceTaskTypeList.Add(serviceTaskType);
				}
			}

			if (serviceTaskTypeList.Count == 0)
				throw new EntryPointNotFoundException(string.Format("Class which implement {0} not found for all dlls in {1}",
					typeof (IServiceTask), serviceTaskDirectory));
			if (serviceTaskTypeList.Count != 1)
				throw new EntryPointNotFoundException(
					string.Format("ServiceTaskHost implemnentation require only one class in {1} implement {0}", typeof (IServiceTask),
						serviceTaskDirectory));

			var serviceTask = (IServiceTask) Activator.CreateInstance(serviceTaskTypeList.First());

			string serviceTaskName = configuration.AppSettings.Settings["ServiceTaskName"].Value;
			if (string.IsNullOrWhiteSpace(serviceTaskName)) throw new ConfigurationErrorsException(string.Format("Config file for {0} doesn't have ServiceTaskName appSetting", serviceTask.GetType()));

			log4net.GlobalContext.Properties["ServiceTaskName"] = serviceTaskName;
			ILog logger = LogManager.GetLogger(serviceTaskName);
			return new ServiceTaskRunner(serviceTask, logger, configuration, serviceTaskName);
		}	



		static void RunServiceInteractively(IServiceTaskRunner service)
		{
			service.Start();
			MessageBox.Show("Close this dialog to stop running services", "Stop service", MessageBoxButtons.OK);
			service.Stop();
		}
	}
}