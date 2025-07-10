using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.ServiceTasks
{
	public class ProcessingManager
	{
		public ProcessingManager(ILogger logger)
		{
			Logger = logger;
		}

		readonly ILogger Logger;

		public void PurgeOldJobOrphanScans()
		{
			var utcNow = ZDateTime.UtcNow;
			var dateBeforeWhichToDeleteOrphanScans = utcNow.AddDays(-PackingRegistry.Instance.AnonymousPackagePurgeTime.Value);
			string sql = string.Format(Culture.Invariant, @"
				DELETE dbo.JobOrphanScan
				WHERE JOS_EventTimeUtc < '{0}'
				SELECT @@ROWCOUNT", dateBeforeWhichToDeleteOrphanScans.ToISO8601String()); // This is Sql Text

			int? rowsDeleted = null;

			try
			{
				rowsDeleted = (int)Db.Connection.ExecuteScalar(sql); // No need to load the rows into memory
			}
			catch (SqlException exception)
			{
				Logger.Error(exception.Message);
				Logger.Information(Res.GetString("cbe2b523-367e-4ce2-ad0e-19f06945980a", "Purging of Anonymous Packages failed."));
			}

			if (rowsDeleted.HasValue)
			{
				if (rowsDeleted.Value > 0)
				{
					Logger.Information(Res.GetString("0ee926ac-2689-4532-804d-a9e960ac2ffc", "Successfully purged {0} Anonymous Packages.", rowsDeleted));
				}
				else
				{
					Logger.Information(Res.GetString("51905f27-d71c-4813-a896-54bbc8867a0f", "Did not find any Anonymous Packages to Purge."));
				}
			}
		}
	}
}
