using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.REST.Deployment.Tasks
{
	public class Build : Task
	{
		public override bool Execute()
		{
			var success = false;
			Log.LogMessage(MessageImportance.High, $"Building interface: {Shared.ConfigurationManager.Profile.InterfaceName}.");
			try
			{
				var buildManager = Shared.CreateBuildManager(WorkingDirectory, BuildEngine);
				buildManager.BuildInterface().GetAwaiter().GetResult();
				success = true;
			}
			finally
			{
				Shared.LogResult(success, Log);
			}
			return true;
		}
		public string WorkingDirectory { get; set; }
	}
}
