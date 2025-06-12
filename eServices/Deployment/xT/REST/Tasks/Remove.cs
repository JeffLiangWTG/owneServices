using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.REST.Deployment.Tasks
{
	public class Remove : Task
	{
		public override bool Execute()
		{
			var success = false;
			Log.LogMessage(MessageImportance.High, "Removing xT interface and interface directory.");
			try
			{
				var deployManager = Shared.CreateDeployManager(WorkingDirectory, InterfaceVersion);
				deployManager.Remove().GetAwaiter().GetResult();
				RemovedInterfaceName = Shared.ConfigurationManager.Interface.InterfaceName; 
				RemovedInterfaceVersion = Shared.ConfigurationManager.Interface.InterfaceVersion; 
				success = true;
			}
			finally
			{
				Shared.LogResult(success, Log);
			}
			return true;
		}
		public string WorkingDirectory { get; set; }
		public string InterfaceVersion { get; set; }
		[Output]
		public string RemovedInterfaceName { get; private set; }
		[Output]
		public string RemovedInterfaceVersion { get; private set; }
	}
}