using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.REST.Deployment.Tasks
{
	public class Deploy : Task
	{
		public override bool Execute()
		{
			var success = false;
			Log.LogMessage(MessageImportance.High, "Importing xT configuration");
			try
			{
				var deployManager = Shared.CreateDeployManager(WorkingDirectory, InterfaceVersion);
				deployManager.Deploy().GetAwaiter().GetResult();
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
	}
}
