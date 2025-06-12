using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.IntegrationTesting.Tasks
{
	public class LoadProfiles : Task
	{
		public override bool Execute()
		{
			var success = false;
			Log.LogMessage(MessageImportance.High, "Loading profiles.");
			try
			{
				Shared.LoadAllProfiles(DefaultProfileFile, ProfileFile);
				Shared.ConfigurationManager.Profile.InterfaceVersion = null;
				InterfaceName = Shared.ConfigurationManager.Profile.InterfaceName;
				WorkingDirectory = Shared.ConfigurationManager.Profile.WorkingDirectory;
				ApiUrl = Shared.ConfigurationManager.Profile.ApiUrl;
				success = true;
			}
			finally
			{
				Shared.LogResult(success, Log);
			}
			return true;
		}

		[Required]
		public string DefaultProfileFile { get; set; }
		public string ProfileFile { get; set; }
		[Output]
		public string InterfaceName { get; private set; }
		[Output]
		public string WorkingDirectory { get; private set; }
		[Output]
		public string ApiUrl { get; private set; }
	}
}
