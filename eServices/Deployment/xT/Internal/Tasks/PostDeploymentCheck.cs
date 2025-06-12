using System;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.Internal.API.tasks
{
	public class PostDeploymentCheck : Task
	{
		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.High, "Starting Post Deployment Check.");

			var report = new StringBuilder();
			try
			{
				SharedObjects.xtProxy.PostDeploymentCheck(SharedObjects.ExternalRelations, report);
				if (report.Length > 0)
				{
					Log.LogMessage(MessageImportance.High, report.ToString());
				}
				else
				{
					SharedObjects.isPostDeploymentCheckPassed = true;
					Log.LogMessage(MessageImportance.High, "Post Deployment Check passed.");
				}
			}
			finally
			{
			}
			return SharedObjects.isPostDeploymentCheckPassed;
		}
	}
}
