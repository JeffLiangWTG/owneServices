using Microsoft.Build.Framework;
using Microsoft.Build.Locator;
using Microsoft.Build.Utilities;

namespace XT.IntegrationTesting.Tasks
{
	public class Build : Task
	{
		public override bool Execute()
		{
			var success = false;
			Log.LogMessage(MessageImportance.High, "Importing xT configuration and preparing interface directory.");
			try
			{
				if (MSBuildLocator.CanRegister) MSBuildLocator.RegisterDefaults();
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
