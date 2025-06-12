using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.Internal.API.tasks
{
	public class CreateSnapshot : Task
	{
		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.High, "Creating database snapshot.");
			try
			{
				SharedObjects.snapshot = new Snapshot(ConnectionString, DatabaseName, SnapshotName);
				SharedObjects.hasSnapshitBeenCreated = true;
			}
			finally
			{
				if (SharedObjects.hasSnapshitBeenCreated)
				{
					Log.LogMessage(MessageImportance.High, "\tDatabase snapshot was created.");
					Log.LogMessage(MessageImportance.High, $"\t\tDatabase Name: {SharedObjects.snapshot.DatabaseName}.");
					Log.LogMessage(MessageImportance.High, $"\t\tSnapshot Name: {SharedObjects.snapshot.SnapshotName}");
				}
				else
				{
					Log.LogMessage(MessageImportance.High, "\tCould not create snapshot.");
					Log.LogMessage(MessageImportance.High, $"\t\tDatabase Name: {DatabaseName}.");
					Log.LogMessage(MessageImportance.High, $"\t\tSnapshot Name: {SnapshotName}");
				}
			}
			return SharedObjects.hasSnapshitBeenCreated;
		}

		[Required]
		public string ConnectionString { get; set; }
		public string DatabaseName { get; set; }
		public string SnapshotName { get; set; }
	}
}
