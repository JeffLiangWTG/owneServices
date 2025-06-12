using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.Internal.API.tasks
{
	public class RestoreSnapshot : Task
	{
		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.High, "Restoring database snapshot.");
			if (SharedObjects.hasSnapshitBeenCreated && !SharedObjects.hasSnapshitBeenDeleted)
			{
				try
				{
					SharedObjects.snapshot.Restore();
					SharedObjects.hasSnapshitBeenRestored = true;
				}
				finally
				{
					if (SharedObjects.hasSnapshitBeenRestored)
					{
						Log.LogMessage(MessageImportance.High, "\tDatabase snapshot was restored.");
						Log.LogMessage(MessageImportance.High, $"\t\tDatabase Name: {SharedObjects.snapshot.DatabaseName}.");
						Log.LogMessage(MessageImportance.High, $"\t\tSnapshot Name: {SharedObjects.snapshot.SnapshotName}");
					}
					else
					{
						Log.LogMessage(MessageImportance.High, "\tCould not restore snapshot.");
						Log.LogMessage(MessageImportance.High, "\t\tHIGHLY IMPORTANT NOTICE:");
						Log.LogMessage(MessageImportance.High, "\t\tDATABASE HAS BEEN LEFT IN A CORRUPTED STATE. PLEASE USE THE LATEST BACKUP FILE TO RESTORE DATABASE AS SOON AS POSSIBLE.");
					}
				}
			}
			else
			{
				Log.LogMessage(MessageImportance.High, "\tSnapshot has either been deleted or not been created.");
			}
			return SharedObjects.hasSnapshitBeenRestored;
		}
	}
}
