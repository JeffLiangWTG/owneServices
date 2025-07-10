using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Packing.Business;

namespace Enterprise.Packing.ServiceTasks
{
	public class DeletePackingFountainForFinalizedPackingJobsProcessingManager : IDeletePackingFountainForFinalizedPackingJobsProcessingManager
	{
		public void DeletePackingFountainsForFinalizedPackingJobs(ILogger serviceLogger, int batchSize, CancellationToken cancellationToken)
		{
			var lastPackingFountainsDeleteTimeUtc = PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.Value;

			var totalRowsDeleted = 0;
			DateTime? lastMaxRunUtc;
			var lastRunUtc = lastPackingFountainsDeleteTimeUtc;

			do
			{
				cancellationToken.ThrowIfCancellationRequested();

				(var rowsDeleted, lastMaxRunUtc) = DeleteNumberFountains(batchSize, lastRunUtc);
				totalRowsDeleted += rowsDeleted;

				if (lastMaxRunUtc.HasValue)
				{
					PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lastMaxRunUtc.Value);
					lastRunUtc = lastMaxRunUtc.Value;
				}
			}
			while (lastMaxRunUtc != null);

			if (totalRowsDeleted > 0)
			{
				serviceLogger.Information(Res.GetString("e593c8f5-2c56-463b-b544-370ca1709837", "Successfully deleted {0} Packing Fountain(s).", totalRowsDeleted));
			}
			else
			{
				serviceLogger.Information(Res.GetString("19c5c7ea-3901-4f51-9b63-a8b63f5c8dfd", "No Packing Fountains were deleted."));
			}
		}

		(int fountainsDeleted, DateTime? lastMaxRunUtc) DeleteNumberFountains(int batchSize, DateTime lastRunUtc)
		{
			var fountainsDeleted = 0;
			DateTime? lastMaxRun = null;

			Db.Connection.ExecuteReader(
				GetDeleteQuery(batchSize),
				(cmd) =>
				{
					cmd.AddParameter("@lastRunUtc", SqlDbType.DateTime, lastRunUtc);
				},
				(reader) =>
				{
					fountainsDeleted = reader.GetInt32(0);
					lastMaxRun = reader.IsDBNull(1) ? null : reader.GetDateTime(1);
				}
			);

			return (fountainsDeleted, lastMaxRun);
		}

		static string GetDeleteQuery(int batchSize) => @$"
BEGIN TRY
	DROP TABLE IF EXISTS #NumberFountainsToDelete
	CREATE TABLE #NumberFountainsToDelete
	(
		FountainOwner UNIQUEIDENTIFIER,
		FountainName VARCHAR(256) COLLATE database_default,
		FountainSequence SMALLINT,
		CompletedDateTime SMALLDATETIME NOT NULL,
		INDEX NameOwnerSequenceIndex CLUSTERED(FountainName, FountainOwner, FountainSequence),
		INDEX CompletedDateTimeIndex NONCLUSTERED(CompletedDateTime)
	)

	INSERT INTO #NumberFountainsToDelete
	SELECT TOP {batchSize} WITH TIES
		SN_Owner,
		SN_Name,
		SN_Sequence,
		FinalizedJob.CompletedDateTime
	FROM
		dbo.PkgPackageJob
		CROSS APPLY
		(
			SELECT
				WP_FinalizedDateUtc AS CompletedDateTime
			FROM
				dbo.WhsDocket
				JOIN dbo.WhsPick ON WD_WP = WP_PK
			WHERE
				WD_DocketType = 'ORD'
				AND WD_PK = KJ_ParentID
				AND WP_FinalizedDateUtc IS NOT NULL
				AND WP_FinalizedDateUtc > @lastRunUtc
		) FinalizedJob
		LEFT JOIN dbo.StmNums WITH (INDEX(NR_RX__SN_Owner)) ON SN_Owner = KJ_PK AND SN_Name LIKE 'GeneratorFountain-PKGID%'
	ORDER BY FinalizedJob.CompletedDateTime

	DELETE dbo.StmNums
	FROM
		#NumberFountainsToDelete
	WHERE
		SN_Name = FountainName
		AND SN_Owner = FountainOwner
		AND SN_Sequence =  FountainSequence

	SELECT @@ROWCOUNT, (SELECT MAX(CompletedDateTime) FROM #NumberFountainsToDelete)

	DROP TABLE #NumberFountainsToDelete
END TRY
BEGIN CATCH
	THROW
END CATCH
";
	}
}
