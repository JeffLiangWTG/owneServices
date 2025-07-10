using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class SqlAlwaysOnSecondaryServerWorker
	{
		public SqlAlwaysOnSecondaryServerWorker(string availabilityGroupName, string newDbName)
		{
			this.availabilityGroupName = availabilityGroupName;
			this.newDbName = newDbName;
		}

		#region Multithreaded

		public void AddDbToGivenSecondaryServer(string secondaryServer)
		{
			using (var primaryConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				AddDbToGivenSecondaryServerCore(primaryConnection, secondaryServer);
			}
		}

		public void AddDbToGivenSecondaryServer(AdminConnection primaryConnection, string secondaryServer)
		{
			using (((ICurrentDbControl)primaryConnection).UseDatabase(Db.SqlMasterDb))
			{
				AddDbToGivenSecondaryServerCore(primaryConnection, secondaryServer);
			}
		}

		void AddDbToGivenSecondaryServerCore(AdminConnection primaryConnection, string secondaryServer)
		{
			try
			{
				using (var secondaryServerConnection = Db.NewAdminConnection(secondaryServer, Db.SqlMasterDb))
				{
					if (AvailabilityGroupExists(secondaryServerConnection))
					{
						if (!secondaryServerConnection.DatabaseExists(newDbName))
						{
							RestoreDatabaseFromFullBackup(secondaryServerConnection);
						}

						if (secondaryServerConnection.DatabaseExists(newDbName)
							&& !AlwaysOn.IsDbPartOfAlwaysOn(secondaryServerConnection, newDbName))
						{
							RestoreTransactionLogBackupsForDatabase(primaryConnection, secondaryServerConnection);
						}

						if (!AlwaysOn.IsDbPartOfAlwaysOn(secondaryServerConnection, newDbName))
						{
							JoinDatabaseToSecondaryReplica(secondaryServerConnection);
						}
					}
				}
			}
			catch (SqlException ex)
			{
				var exceptionType = new DbErrorMatch(ex).ExceptionType;
				if (exceptionType != DbErrorType.DatabaseAlreadyJoinedToAvailabilityGroup
					&& exceptionType != DbErrorType.CannotOperateOnDatabaseWhenAlreadyConfiguredForMirroringOrJoinedToAvailabilityGroup
					&& !IsDatabaseCreatedWithinGracePeriod())
				{
					secServerExceptions[secondaryServer] = ex;
				}
			}
		}

		protected virtual bool IsDatabaseCreatedWithinGracePeriod()
		{
			var sql = $@"SELECT CASE WHEN GETDATE() < DATEADD(MINUTE, 30, create_date) THEN 1 ELSE 0 END FROM sys.databases where name = @dbName";
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (var command = connection.Command(sql))
			{
				command.AddParameter("@dbName", SqlDbType.VarChar, 128, newDbName);
				return (int)command.ExecuteScalar() == 1;
			}
		}

		public virtual bool AvailabilityGroupExists(AdminConnection secondaryServerConnection)
		{
			var sqlScript = "SELECT CONVERT(bit, CASE WHEN EXISTS (SELECT NULL FROM sys.availability_groups WHERE name = @agName) THEN 1 ELSE 0 END);";
			using (var cmd = secondaryServerConnection.Command(sqlScript))
			{
				cmd.AddParameter("@agName", SqlDbType.NVarChar, 128, availabilityGroupName);

				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		protected virtual void JoinDatabaseToSecondaryReplica(AdminConnection connection)
		{
			var sqlText = $"ALTER DATABASE [{newDbName}] SET HADR AVAILABILITY GROUP = [{availabilityGroupName}]";
			ExecuteNonQuery(connection, sqlText);
		}

		void RestoreDatabaseFromFullBackup(AdminConnection connection)
		{
			string moveClause = GetSecondaryDatabaseRestoreMoveClause(connection, newDbName);
			string sqlText = string.Format(CultureInfo.InvariantCulture, "RESTORE DATABASE [{0}] FROM DISK = N'{1}' WITH NORECOVERY{2}", newDbName, backupFilePath, moveClause);
			ExecuteNonQuery(connection, sqlText);
		}

		public virtual void ExecuteNonQuery(AdminConnection connection, string sqlText)
		{
			connection.ExecuteNonQuery(sqlText);
		}

		protected virtual void RestoreTransactionLogBackupsForDatabase(AdminConnection primaryConnection, AdminConnection secondaryServerConnection)
		{
			var lastLSN = GetLastFullBackupLastLSN(secondaryServerConnection);

			foreach (var trnFile in GetListOfLogBackupFilesToRestore(primaryConnection, lastLSN))
			{
				try
				{
					if (trnFile.LastIndexOf('\\') < 0)
					{
						continue;
					}
					RestoreTransactionLogBackup(secondaryServerConnection, trnFile);
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CannotOpenBackupDeviceOrInvalidDeviceName)
				{
					if (IsHostedWithCargowise())
					{
						try
						{
							RestoreTransactionLogBackupFromRestoredFolder(secondaryServerConnection, trnFile);
						}
						catch (SqlException ex2) when (new DbErrorMatch(ex2).ExceptionType == DbErrorType.CannotOpenBackupDeviceOrInvalidDeviceName)
						{
						}
					}
				}
			}
		}

		protected internal virtual bool IsHostedWithCargowise() => EnvProxy.IsHostedWithCargowise;

		void RestoreTransactionLogBackup(AdminConnection secondaryServerConnection, string trnFile)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "RESTORE LOG [{0}] FROM DISK = N'{1}' WITH NORECOVERY", newDbName, trnFile);
			try
			{
				ExecuteNonQuery(secondaryServerConnection, sqlText);
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LogBackupIsTooEarlyToApplyToTheDatabase)
			{
			}
		}

		void RestoreTransactionLogBackupFromRestoredFolder(AdminConnection secondaryServerConnection, string trnFile)
		{
			var trnFileinRestoredFolder = trnFile.Insert(trnFile.LastIndexOf('\\'), @"\Restored");
			RestoreTransactionLogBackup(secondaryServerConnection, trnFileinRestoredFolder);
		}

		IEnumerable<string> GetListOfLogBackupFilesToRestore(AdminConnection connection, decimal lastLSN)
		{
			var listOfFiles = new List<string>();

			var sqlText = @"
SELECT msf.physical_device_name
FROM msdb.dbo.backupset bck INNER JOIN
	 msdb.dbo.backupmediafamily msf ON bck.media_set_id = msf.media_set_id
WHERE TYPE = 'L'
  AND database_name = @dbName
  AND bck.last_lsn >= @lastLSN
ORDER BY bck.backup_set_id
";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, newDbName);
				cmd.AddParameter("@lastLSN", SqlDbType.Decimal, lastLSN);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						listOfFiles.Add(reader["physical_device_name"].ToString());
					}
				}
			}

			return listOfFiles;
		}

		decimal GetLastFullBackupLastLSN(AdminConnection secondaryServerConnection)
		{
			var sqlText = @"SELECT TOP 1 last_lsn
 FROM msdb.dbo.restorehistory rh
JOIN msdb.dbo.backupset bs ON rh.backup_set_id = bs.backup_set_id
WHERE restore_type = 'D'
AND destination_database_name = @dbName
ORDER BY rh.backup_set_id DESC";
			var value = secondaryServerConnection.ExecuteScalar(sqlText, cmd => cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, newDbName));
			return value == null ? 0 : (decimal)value;
		}

		string GetSecondaryDatabaseRestoreMoveClause(DbConnection secondaryServerConnection, string dbName)
		{
			using (var connToPrimary = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var dbFiles = GetDBFiles(dbName, connToPrimary);
				return (dbFiles.Length == 0)
					? ""
					: GetRestoreMoveClause(secondaryServerConnection, dbFiles);
			}
		}

		protected internal virtual string[] GetDBFiles(string dbName, AdminConnection connection) => connection.GetDbFiles(dbName);

		string GetRestoreMoveClause(DbConnection secondaryServerConnection, IEnumerable<string> dbFiles)
		{
			var moveClause = new StringBuilder();
			var sqlText = "RESTORE FILELISTONLY FROM DISK = @backupPath";

			var dbFilePaths = new Dictionary<string, string>();
			foreach (var file in dbFiles)
			{
				dbFilePaths.Add(Path.GetFileName(file), Path.GetDirectoryName(file));
			}

			using (var cmd = secondaryServerConnection.Command(sqlText))
			{
				cmd.AddParameter("@backupPath", SqlDbType.NVarChar, backupFilePath);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var logicalName = reader["LogicalName"].ToString();
						var physicalFilePath = reader["PhysicalName"].ToString();
						var physicalFileName = Path.GetFileName(physicalFilePath);

						dbFilePaths.TryGetValue(physicalFileName, out string dbFilePath);
						if (!string.IsNullOrEmpty(dbFilePath))
						{
							var targetFilePath = Path.Combine(dbFilePath, physicalFileName);
							moveClause.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)", MOVE '{0}' TO '{1}'", logicalName, targetFilePath));
						}
					}
				}
			}

			return moveClause.ToString();
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
		public IEnumerable<KeyValuePair<string, Exception>> SecondaryServerExceptions
		{
			get
			{
				foreach (var secondaryServer in secServerExceptions.Keys)
				{
					yield return new KeyValuePair<string, Exception>(secondaryServer, secServerExceptions[secondaryServer]);
				}
			}
		}

		readonly ConcurrentDictionary<string, Exception> secServerExceptions = new ConcurrentDictionary<string, Exception>();
		internal readonly string availabilityGroupName;
		readonly string newDbName;
		string backupFilePath => Path.Combine(Env.Registry.BackupDirectoryPath, newDbName + ".bak");
	}
}
