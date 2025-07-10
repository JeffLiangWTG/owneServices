using System;
using System.Data;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public class UpgradeManager
	{
		public UpgradeManager(
			IDbConnection adminConnection,
			string sourceDbName,
			TransformationTasks preUpgrade,
			TransformationTasks postUpgrade,
			int targetVersion,
			ISchemaUpgrade schemaUpgrade,
			ISchemaVersionManager versionMgr,
			IDbCreator dbCreator,
			IDbLockout dbLockout,
			bool forceUpgrade,
			ErrorReportingClientWrapper errorReportWrapper = null)
		{
			Argument.Argument.NotNull(adminConnection, nameof(adminConnection));
			Argument.Argument.NotNull(preUpgrade, nameof(preUpgrade));
			Argument.Argument.NotNull(postUpgrade, nameof(postUpgrade));
			Argument.Argument.NotNullOrEmpty(sourceDbName, nameof(sourceDbName));
			Argument.Argument.NotNull(schemaUpgrade, nameof(schemaUpgrade));
			Argument.Argument.NotNull(versionMgr, nameof(versionMgr));
			Argument.Argument.NotNull(dbCreator, nameof(dbCreator));
			Argument.Argument.NotNull(dbLockout, nameof(dbLockout));

			this.adminConnection = adminConnection;
			this.preUpgrade = preUpgrade;
			this.postUpgrade = postUpgrade;
			this.targetVersion = targetVersion;
			this.schemaUpgrade = schemaUpgrade;
			this.versionMgr = versionMgr;
			this.dbCreator = dbCreator;
			this.sourceDbName = sourceDbName;
			this.dbLockout = dbLockout;
			this.forceUpgrade = forceUpgrade;
			errorReportingClientWrapper = errorReportWrapper ?? new ErrorReportingClientWrapper();
		}

		public void Run()
		{
			Console.WriteLine($"Starting {sourceDbName} database upgrade.");
			var currentVersion = versionMgr.GetVersion(null);
			Console.WriteLine($"Force Upgrade is set to: {forceUpgrade}");
			if (!forceUpgrade && currentVersion >= targetVersion)
			{
				Console.WriteLine($"Current version {currentVersion} is equal or higher than the target version {targetVersion}, nothing to upgrade.");
				return;
			}

			var templateDbName = sourceDbName + "Template";
			try
			{
				using (dbLockout.Acquire())
				{
					dbCreator.DropDatabase(templateDbName);
					schemaUpgrade.CloneSchema(sourceDbName, templateDbName, dbCreator);
					Console.WriteLine($"Switching connection to database {templateDbName}");
					adminConnection.ChangeDatabase(templateDbName);
					using (var trans = adminConnection.BeginTransaction())
					{
						if (!templateDbName.Equals(adminConnection.Database, StringComparison.OrdinalIgnoreCase))
						{
							throw new NotSupportedException("Cannot connect to template database");
						}
						Console.WriteLine($"Executing pre-upgrade transformations on {templateDbName}. Current version: {currentVersion}, Target version: {targetVersion}");
						preUpgrade.Run(currentVersion, targetVersion, trans);
						trans.Commit();
					}
					Console.WriteLine($"Getting difference between database {templateDbName} and database {sourceDbName}");
					var sqlDiff = schemaUpgrade.GetDiffSql(templateDbName);
					Console.WriteLine($"Switching connection to database {sourceDbName}");
					adminConnection.ChangeDatabase(sourceDbName);
					var isReadCommitedSnapshotOnResult = adminConnection.ExecuteScalar($"SELECT is_read_committed_snapshot_on FROM sys.databases WHERE name = '{sourceDbName}'");
					var isReadCommitedSnapshotOn = (bool)isReadCommitedSnapshotOnResult;
					if (!isReadCommitedSnapshotOn)
					{
						Console.WriteLine($"Please enable READ_COMMITTED_SNAPSHOT on {sourceDbName}");
					}
					using (var trans = adminConnection.BeginTransaction())
					{
						currentVersion = versionMgr.GetVersion(trans);
						if (forceUpgrade || currentVersion < targetVersion)
						{
							Console.WriteLine($"Executing pre-upgrade transformations on {sourceDbName}. Current version: {currentVersion}, Target version: {targetVersion}");
							preUpgrade.Run(currentVersion, targetVersion, trans);
							Console.WriteLine($"Upgrading database {sourceDbName} based on the difference collected earlier");
							dbCreator.ExcuteDbScript(sourceDbName, sqlDiff, (SqlTransaction)trans);
							Console.WriteLine($"Executing post-upgrade transformations on {sourceDbName}. Current version: {currentVersion}, Target version: {targetVersion}");
							postUpgrade.Run(currentVersion, targetVersion, trans);
							versionMgr.UpdateVersion(targetVersion, trans);
							trans.Commit();
						}
					}
				}
			}
			catch (Exception ex)
			{
				errorReportingClientWrapper.PostCrashReport(ex, null, null);
				throw;
			}
			finally
			{
				dbCreator.DropDatabase(templateDbName);
				Console.WriteLine($"Finished {sourceDbName} database upgrade.");
			}
		}

		readonly IDbConnection adminConnection;
		readonly int targetVersion;
		readonly string sourceDbName;
		readonly ISchemaVersionManager versionMgr;
		readonly TransformationTasks preUpgrade;
		readonly TransformationTasks postUpgrade;
		readonly ISchemaUpgrade schemaUpgrade;
		readonly IDbCreator dbCreator;
		readonly IDbLockout dbLockout;
		readonly bool forceUpgrade;
		readonly ErrorReportingClientWrapper errorReportingClientWrapper;
	}
}
