using System.Configuration;
using System.IO;
using System.Reflection;

namespace CargoWise.eHub.Shared.ServiceTaskHost
{
	static class ServiceTaskHelper
	{
		public static DirectoryInfo GetServiceTaskDirectory()
		{
			const string serviceTaskFolder = "ServiceTasks";
			var serviceHostDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var serviceTasksDirectory = Path.Combine(serviceHostDirectory, serviceTaskFolder);
			if (!Directory.Exists(serviceTasksDirectory)) throw new DirectoryNotFoundException(serviceTasksDirectory);
			var serviceTaskDirectories = Directory.GetDirectories(serviceTasksDirectory);
			if (serviceTaskDirectories.Length == 0)
				throw new ConfigurationErrorsException(string.Format(
					"At least one service task folder should exists in '{0}' folder.", serviceTaskFolder));
			if (serviceTaskDirectories.Length > 1)
				throw new ConfigurationErrorsException(
					string.Format(
						"Current version of ServiceTaskHost support only one service task. Please remove other from '{0}' folder.",
						serviceTaskFolder));

			return new DirectoryInfo(serviceTaskDirectories[0]);
		}
	}
}
