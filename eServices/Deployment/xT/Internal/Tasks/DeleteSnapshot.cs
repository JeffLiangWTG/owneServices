using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.Internal.API.tasks
{
	public class DeleteSnapshot : Task
	{
		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.High, "Deleting database snapshot.");
			if (SharedObjects.hasSnapshitBeenCreated && !SharedObjects.hasSnapshitBeenDeleted)
			{
				try
				{
					SharedObjects.snapshot.Delete();
					SharedObjects.hasSnapshitBeenDeleted = true;
				}
				finally
				{
					if (SharedObjects.hasSnapshitBeenDeleted)
					{
						Log.LogMessage(MessageImportance.High, "\tDatabase snapshot was deleted.");
						Log.LogMessage(MessageImportance.High, $"\t\tDatabase Name: {SharedObjects.snapshot.DatabaseName}");
						Log.LogMessage(MessageImportance.High, $"\t\tSnapshot Name: {SharedObjects.snapshot.SnapshotName}");
					}
					else
					{
						Log.LogMessage(MessageImportance.High, "\tCould not delete snapshot.");
						Log.LogMessage(MessageImportance.High, "\t\tHIGHLY IMPORTANT NOTICE:");
						Log.LogMessage(MessageImportance.High, "\t\tSNAPSHOT COULD NOT BE DELETED. PLEASE DELETE SNAPSHOT MANUALLY AS SOON AS POSSIBLE.");
						Log.LogMessage(MessageImportance.High, $"\t\tDatabase Name: {SharedObjects.snapshot.DatabaseName}");
						Log.LogMessage(MessageImportance.High, $"\t\tSnapshot Name: {SharedObjects.snapshot.SnapshotName}");
					}
				}
			}
			else
			{
				Log.LogMessage(MessageImportance.High, "\tSnapshot has either been deleted previously or not been created.");
			}
			return SharedObjects.hasSnapshitBeenDeleted;
		}
	}
}
