using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.REST.Deployment.Tasks
{
	public class AddRepository : Task
	{
		public override bool Execute()
		{
			var success = false;
			Log.LogMessage(MessageImportance.High, $"Adding Repository: {Repository}.");
			try
			{
				Shared.ConfigurationManager.Profile.Repositories.Add(Repository);
				success = true;
			}
			finally
			{
				Shared.LogResult(success, Log);
			}
			return true;
		}
		public string Repository { get; set; }
	}
}
