using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	sealed class SqlAlwaysOnSecondaryServerWorkerTest : TestCase
	{
		public void TestAddDbToGivenSecondaryServer()
		{
			var agName = "AGName";
			var dbName = Db.DatabaseName + "_SD049";
			var tempDirForTesting = Path.Combine(Temp.TempPath, "BackupDir");
			Env.Registry.BackupDirectoryPath = tempDirForTesting;

			try
			{
				PrepareForTesting(dbName, tempDirForTesting);

				var sqlAuto = new SqlAlwaysOnSecondaryServerWorkerForTesting(agName, dbName);
				sqlAuto.primaryDbFileDir = tempDirForTesting;
				sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);

				var expectedCommandList = new List<string>()
				{
					$"RESTORE DATABASE [{dbName}] FROM DISK = N'{tempDirForTesting}\\{dbName}.bak' WITH NORECOVERY, MOVE '{dbName}' TO '{tempDirForTesting}\\{dbName}.mdf', MOVE '{dbName}_log' TO '{tempDirForTesting}\\{dbName}_log.ldf'",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\{dbName}_20190102030407.trn' WITH NORECOVERY",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\{dbName}_20190102030408.trn' WITH NORECOVERY",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\Restored\{dbName}_20190102030408.trn' WITH NORECOVERY",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\{dbName}_20190102030409.trn' WITH NORECOVERY",
					$"ALTER DATABASE [{dbName}] SET HADR AVAILABILITY GROUP = [{agName}]"
				};

				AssertResutls("First run", expectedCommandList, sqlAuto.commandList);

				sqlAuto.commandList = new List<string>();
				sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);

				expectedCommandList = new List<string>()
				{
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\{dbName}_20190102030407.trn' WITH NORECOVERY",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\{dbName}_20190102030408.trn' WITH NORECOVERY",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\Restored\{dbName}_20190102030408.trn' WITH NORECOVERY",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\{dbName}_20190102030409.trn' WITH NORECOVERY",
					$"ALTER DATABASE [{dbName}] SET HADR AVAILABILITY GROUP = [{agName}]"
				};

				AssertResutls("Second run", expectedCommandList, sqlAuto.commandList);
			}
			finally
			{
				DropTestDatabase(dbName);

				if (Directory.Exists(tempDirForTesting))
				{
					Directory.Delete(tempDirForTesting, true);
				}
			}
		}

		public void TestDatabaseAlreadyJoinedDoesNotSendNotification()
		{
			var agName = "AGName";
			var sqlAuto = new SqlAlwaysOnSecondaryServerWorkerWithDatabaseAlreadyJoined(agName, Db.DatabaseName);
			sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);
			AssertEquals("Number of exceptions", 0, sqlAuto.SecondaryServerExceptions.ToList<KeyValuePair<string, Exception>>().Count);
		}

		public void TestAddDbToGivenSecondaryServerWhenDatabaseAlreadyJoinedTheAvailabilityGroup()
		{
			// Arrange
			var alwaysOnGroupName = "AlwaysOnGroup1";
			var alwaysOnDatabaseName = "TestDatabase1";
			var sqlAlwaysOnSecondaryServerWorkerMock = new Mock<SqlAlwaysOnSecondaryServerWorker>(alwaysOnGroupName, alwaysOnDatabaseName);
			var alwaysOonReplicaworker = sqlAlwaysOnSecondaryServerWorkerMock.Object;

			var sqlErrorNumber = 3104;
			var sqlErrorMessage = $"RESTORE cannot operate on database '{alwaysOnDatabaseName}' because it is configured for database mirroring or has joined an availability group. If you intend to restore the database, use ALTER DATABASE to remove mirroring or to remove the database from its availability group.";
			var sqlException = SqlExceptionBuilder.CreateSqlException(sqlErrorNumber, sqlErrorMessage);

			_ = sqlAlwaysOnSecondaryServerWorkerMock
				.Setup(x => x.AvailabilityGroupExists(It.IsAny<AdminConnection>()))
				.Returns(true);
			_ = sqlAlwaysOnSecondaryServerWorkerMock
				.Protected()
				.Setup("RestoreTransactionLogBackupsForDatabase", ItExpr.IsAny<AdminConnection>(), ItExpr.IsAny<AdminConnection>())
				.Throws(sqlException);

			// Act
			using (AdoTestUtils.CreateDbDropExistingDisposable(alwaysOnDatabaseName))
			{
				alwaysOonReplicaworker.AddDbToGivenSecondaryServer(Db.ServerName);
			}

			// Assert
			AssertEquals("SQL Error 3104 exception should not be reported as an error", 0, alwaysOonReplicaworker.SecondaryServerExceptions.ToList<KeyValuePair<string, Exception>>().Count);
			sqlAlwaysOnSecondaryServerWorkerMock.VerifyAll();
		}

		public void TestAddDbToGivenSecondaryServerOnSecondThread()
		{
			var thread = new Thread(TestAddDbToGivenSecondaryServer);
			thread.Start();
			thread.Join();
		}

		void AssertResutls(string message, List<string> expectedCommandList, List<string> actualCommandList)
		{
			AssertEquals($"{message}: number of SQL Commands:", expectedCommandList.Count, actualCommandList.Count);
			for (var i = 0; i < expectedCommandList.Count; i++)
			{
				AssertEquals($"{message}, SQL Command #{i + 1}", expectedCommandList[i], actualCommandList[i]);
			}
		}

		void DropTestDatabase(string dbName)
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				if (connection.DatabaseExists(dbName))
				{
					connection.ExecuteNonQuery($@"
EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{dbName}'
DROP DATABASE [{dbName}]
");
				}
			}
		}

		void PrepareForTesting(string dbName, string backupDir)
		{
			if (!Directory.Exists(backupDir))
			{
				Directory.CreateDirectory(backupDir);
			}

			if (!Directory.Exists(backupDir + @"\Restored"))
			{
				Directory.CreateDirectory(backupDir + @"\Restored");
			}

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				if (connection.DatabaseExists(dbName))
				{
					connection.ExecuteNonQuery($"DROP DATABASE [{dbName}]");
				}
				var sql = $@"
EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{dbName}'

CREATE DATABASE [{dbName}];
ALTER DATABASE [{dbName}] SET RECOVERY FULL

BACKUP DATABASE [{dbName}] TO DISK = N'{backupDir}\{dbName}.bak'
BACKUP LOG [{dbName}] TO DISK = N'{backupDir}\{dbName}_20190102030405.trn'
BACKUP LOG [{dbName}] TO DISK = N'{backupDir}\{dbName}_20190102030406.trn'

BACKUP DATABASE [{dbName}] TO DISK = N'{backupDir}\\{dbName}.bak' WITH INIT
BACKUP LOG [{dbName}] TO DISK = N'{backupDir}\{dbName}_20190102030407.trn'
BACKUP LOG [{dbName}] TO DISK = N'{backupDir}\{dbName}_20190102030408.trn'
BACKUP LOG [{dbName}] TO DISK = N'{backupDir}\{dbName}_20190102030409.trn'
BACKUP LOG [{dbName}] TO DISK = N'{backupDir}\{dbName}_20190102030410.trn'
DROP DATABASE [{dbName}]
UPDATE msdb.dbo.backupmediafamily set physical_device_name = 'BAD_FILE_PATH' 
FROM msdb.dbo.backupset bck INNER JOIN
	 msdb.dbo.backupmediafamily msf ON bck.media_set_id = msf.media_set_id
WHERE physical_device_name =N'{backupDir}\{dbName}_20190102030410.trn'
";
				connection.ExecuteNonQuery(sql);
				File.Move(Path.Combine(backupDir, dbName + "_20190102030408.trn"), Path.Combine(backupDir, "Restored", dbName + "_20190102030408.trn"));
			}
		}

		public void TestGracePeriodOfErrorsOnAON()
		{
			var agName = "AGName";
			var dbName = Db.DatabaseName + "_SD049";
			var tempDirForTesting = Path.Combine(Temp.TempPath, "BackupDir");
			Env.Registry.BackupDirectoryPath = tempDirForTesting;

			try
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					if (connection.DatabaseExists(dbName))
					{
						connection.ExecuteNonQuery($"DROP DATABASE [{dbName}]");
					}
					connection.ExecuteNonQuery($"CREATE DATABASE [{dbName}]");
				}

				var sqlAuto = new SqlAlwaysOnSecondaryServerWorkerForTesting(agName, dbName);
				sqlAuto.JoinDatabaseToSecondaryReplicaOverride = () => throw SqlExceptionBuilder.CreateSqlException(123, "Error");
				sqlAuto.PrimaryServerDatabaseCreationDateOverride = true;
				sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);

				var expectedCommandList = new List<string>()
				{
					// nothing should be run.
				};

				AssertResutls($"First run:{string.Join("\r\n", sqlAuto.commandList)}", expectedCommandList, sqlAuto.commandList);
				AssertEquals(0, sqlAuto.SecondaryServerExceptions.Count());

				sqlAuto.commandList = new List<string>();
				sqlAuto.PrimaryServerDatabaseCreationDateOverride = false;
				sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);

				AssertResutls("Second run", expectedCommandList, sqlAuto.commandList);
				AssertEquals(1, sqlAuto.SecondaryServerExceptions.Count());
			}
			finally
			{
				DropTestDatabase(dbName);

				if (Directory.Exists(tempDirForTesting))
				{
					Directory.Delete(tempDirForTesting, true);
				}
			}
		}

		public void TestLogFilesThatDoNotExistAreIgnored()
		{
			var agName = "AGName";
			var dbName = Db.DatabaseName + "_SD049";
			var tempDirForTesting = Path.Combine(Temp.TempPath, "BackupDir");
			Env.Registry.BackupDirectoryPath = tempDirForTesting;
			try
			{
				PrepareForTesting(dbName, tempDirForTesting);

				var sqlAuto = new SqlAlwaysOnSecondaryServerWorkerForTestingThrowingOnLogRestore(agName, dbName);
				sqlAuto.primaryDbFileDir = tempDirForTesting;
				sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);

				var exceptionList = sqlAuto.SecondaryServerExceptions.ToList<KeyValuePair<string, Exception>>();
				AssertEquals($"There should be a single sequence exception:\r\n{string.Join("\r\n", exceptionList.Select(i => $"{i.Key} - {i.Value.Message}"))}", 1, exceptionList.Count);
				var match = Regex.IsMatch(exceptionList.First().Value.Message, $@"The log in this backup set begins at LSN \d+, which is too recent to apply to the database. An earlier log backup that includes LSN \d+ can be restored.
RESTORE LOG is terminating abnormally.");
				AssertEquals(true, match);

				var expectedCommandList = new List<string>()
				{
					$"RESTORE DATABASE [{dbName}] FROM DISK = N'{tempDirForTesting}\\{dbName}.bak' WITH NORECOVERY, MOVE '{dbName}' TO '{tempDirForTesting}\\{dbName}.mdf', MOVE '{dbName}_log' TO '{tempDirForTesting}\\{dbName}_log.ldf'",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\{dbName}_20190102030407.trn' WITH NORECOVERY",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\Restored\{dbName}_20190102030407.trn' WITH NORECOVERY",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\{dbName}_20190102030408.trn' WITH NORECOVERY",
					$@"RESTORE LOG [{dbName}] FROM DISK = N'{tempDirForTesting}\Restored\{dbName}_20190102030408.trn' WITH NORECOVERY",
				};
				AssertResutls("We should only have one restore log", expectedCommandList, sqlAuto.commandList);
			}
			finally
			{
				DropTestDatabase(dbName);

				if (Directory.Exists(tempDirForTesting))
				{
					Directory.Delete(tempDirForTesting, true);
				}
			}
		}

		public void TestAddDbToGivenSecondaryServerWithMsdbLocked()
		{
			var agName = "AGName";
			var dbName = Db.DatabaseName + "_SD049";
			var tempDirForTesting = Path.Combine(Temp.TempPath, "BackupDir");
			Env.Registry.BackupDirectoryPath = tempDirForTesting;
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					PrepareForTesting(dbName, tempDirForTesting);
					CheckAndLockDownMsdbAccess(connection);

					var sqlAuto = new SqlAlwaysOnSecondaryServerWorkerForTesting(agName, dbName);
					sqlAuto.primaryDbFileDir = tempDirForTesting;
					sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);
					AssertEquals(0, sqlAuto.SecondaryServerExceptions.Count());
				}
				finally
				{
					DropTestDatabase(dbName);

					if (Directory.Exists(tempDirForTesting))
					{
						Directory.Delete(tempDirForTesting, true);
					}

					foreach (var login in connection.Logins.Select(login => login.LoginName))
					{
						SqlSecurityUtils.DbUser.Drop(connection, Db.SqlMsdb, login);
					}

					SqlSecurityUtils.DbRole.Drop(connection, Db.SqlMsdb, DbRoleTypes.Constants.CwMsdbAccessDeniedRole);
				}
			}
		}

		void CheckAndLockDownMsdbAccess(AdminConnection connection)
		{
			var loginsToCheck = new DbUserManager().GetStaffDbLogins(mainDbConnection: connection);
			loginsToCheck.AddRange(connection.Logins.Select(login => login.LoginName));

			DbSecurity.LockDownMsdbAccessOnServer(connection, Db.ServerName, loginsToCheck);
		}

		public void TestRestoreDatabaseFromFullBackup_SamePaths()
		{
			var agName = "AGName";
			var dbName = Db.DatabaseName + "_RefDb_Cmr_AU_Test";
			var dbFileDir = Path.Combine(Temp.TempPath, "DbDir");
			var dbBackupDir = Path.Combine(Temp.TempPath, "DbBak");
			Env.Registry.BackupDirectoryPath = dbBackupDir;

			try
			{
				PrepareForRestoringDb(dbName, dbFileDir, dbBackupDir);

				var sqlAuto = new SqlAlwaysOnSecondaryServerWorkerForTesting(agName, dbName);
				sqlAuto.primaryDbFileDir = dbFileDir;
				sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);

				var expectedMoveclause = $", MOVE '{dbName}' TO '{dbFileDir + $@"\{dbName}.mdf"}', MOVE '{dbName}_log' TO '{dbFileDir + $@"\{dbName}_log.ldf"}'";
				var expectedCommandList = new List<string>()
				{
					$"RESTORE DATABASE [{dbName}] FROM DISK = N'{dbBackupDir}\\{dbName}.bak' WITH NORECOVERY{expectedMoveclause}",
					$"ALTER DATABASE [{dbName}] SET HADR AVAILABILITY GROUP = [{agName}]"
				};

				AssertResutls("There Should Have A Move Clause", expectedCommandList, sqlAuto.commandList);
			}
			finally
			{
				DropTestDatabase(dbName);
				DeleteDirectories(new string[] { dbFileDir, dbBackupDir });
			}
		}

		public void TestRestoreDatabaseFromFullBackup_DifferentPaths()
		{
			var agName = "AGName";
			var dbName = Db.DatabaseName + "_RefDb_Cmr_AU_Test";
			var dbFileDir = Path.Combine(Temp.TempPath, "DbDir");
			var dbBackupDir = Path.Combine(Temp.TempPath, "DbBak");
			var primaryDbFileDir = Path.Combine(Temp.TempPath, "DifferentDbDir");
			Env.Registry.BackupDirectoryPath = dbBackupDir;

			try
			{
				PrepareForRestoringDb(dbName, dbFileDir, dbBackupDir, primaryDbFileDir);

				var sqlAuto = new SqlAlwaysOnSecondaryServerWorkerForTesting(agName, dbName);
				sqlAuto.primaryDbFileDir = primaryDbFileDir;
				sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);

				var expectedMoveclause = $", MOVE '{dbName}' TO '{primaryDbFileDir + $@"\{dbName}.mdf"}', MOVE '{dbName}_log' TO '{primaryDbFileDir + $@"\{dbName}_log.ldf"}'";
				var expectedCommandList = new List<string>()
				{
					$"RESTORE DATABASE [{dbName}] FROM DISK = N'{dbBackupDir}\\{dbName}.bak' WITH NORECOVERY{expectedMoveclause}",
					$"ALTER DATABASE [{dbName}] SET HADR AVAILABILITY GROUP = [{agName}]"
				};

				AssertResutls("There Should Have A Move Clause", expectedCommandList, sqlAuto.commandList);
			}
			finally
			{
				DropTestDatabase(dbName);
				DeleteDirectories(new string[] { dbFileDir, dbBackupDir, primaryDbFileDir });
			}
		}

		public void TestRestoreDatabaseFromFullBackup_MultipleDbFiles()
		{
			var agName = "AGName";
			var dbName = Db.DatabaseName + "_RefDb_Cmr_AU_Multiple_Test";
			var dbFileDir = Path.Combine(Temp.TempPath, "DbDir");
			var dbBackupDir = Path.Combine(Temp.TempPath, "DbBak");
			var multipleDbFileDir = dbFileDir + "2";
			Env.Registry.BackupDirectoryPath = dbBackupDir;

			try
			{
				PrepareForRestoringDb(dbName, dbFileDir, dbBackupDir, string.Empty, true);

				var sqlAuto = new SqlAlwaysOnSecondaryServerWorkerForTesting(agName, dbName);
				sqlAuto.primaryDbFileDir = dbFileDir;
				sqlAuto.IsMultipleDb = true;
				sqlAuto.AddDbToGivenSecondaryServer(Db.ServerName);

				var expectedMoveclause = new StringBuilder();
				expectedMoveclause.Append($", MOVE '{dbName}' TO '{dbFileDir + $@"\{dbName}.mdf"}'");
				expectedMoveclause.Append($", MOVE '{dbName}2' TO '{multipleDbFileDir + $@"\{dbName}2.ndf"}'");
				expectedMoveclause.Append($", MOVE '{dbName}_log' TO '{dbFileDir + $@"\{dbName}_log.ldf"}'");
				expectedMoveclause.Append($", MOVE '{dbName}2_log' TO '{multipleDbFileDir + $@"\{dbName}2_log.ldf"}'");

				var expectedCommandList = new List<string>()
				{
					$"RESTORE DATABASE [{dbName}] FROM DISK = N'{dbBackupDir}\\{dbName}.bak' WITH NORECOVERY{expectedMoveclause.ToString()}",
					$"ALTER DATABASE [{dbName}] SET HADR AVAILABILITY GROUP = [{agName}]"
				};

				AssertResutls("There Should Have A Move Clause For Multiple Files", expectedCommandList, sqlAuto.commandList);
			}
			finally
			{
				DropTestDatabase(dbName);
				DeleteDirectories(new string[] { dbFileDir, dbBackupDir, multipleDbFileDir });
			}
		}

		void PrepareForRestoringDb(string dbName, string dbDir, string backupFileDir, string primaryDbDir = "", bool multipleDbFiles = false)
		{
			if (!Directory.Exists(dbDir))
			{
				Directory.CreateDirectory(dbDir);
			}

			if (!Directory.Exists(backupFileDir))
			{
				Directory.CreateDirectory(backupFileDir);
			}

			if (!string.IsNullOrEmpty(primaryDbDir) && !Directory.Exists(primaryDbDir))
			{
				Directory.CreateDirectory(primaryDbDir);
			}

			var multipleDbDir = string.Empty;
			if (multipleDbFiles)
			{
				multipleDbDir = dbDir + "2";
				if (!Directory.Exists(multipleDbDir))
				{
					Directory.CreateDirectory(multipleDbDir);
				}
			}

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				if (connection.DatabaseExists(dbName))
				{
					connection.ExecuteNonQuery($"DROP DATABASE [{dbName}]");
				}
				var createDbSql = !multipleDbFiles
					? $@"CREATE DATABASE [{dbName}] ON (NAME = {dbName}, FILENAME = '{dbDir}\{dbName}.mdf') LOG ON (NAME = {dbName}_log, FILENAME = '{dbDir}\{dbName}_log.ldf');"
					: $@"CREATE DATABASE [{dbName}]
ON (NAME = {dbName}, FILENAME = '{dbDir}\{dbName}.mdf'),(NAME = {dbName}2, FILENAME = '{multipleDbDir}\{dbName}2.ndf')
LOG ON (NAME = {dbName}_log, FILENAME = '{dbDir}\{dbName}_log.ldf'),(NAME = {dbName}2_log, FILENAME = '{multipleDbDir}\{dbName}2_log.ldf');";

				var sql = $@"
EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{dbName}'

{createDbSql}
ALTER DATABASE [{dbName}] SET RECOVERY FULL
BACKUP DATABASE [{dbName}] TO DISK = N'{backupFileDir}\{dbName}.bak'
DROP DATABASE [{dbName}]
";
				connection.ExecuteNonQuery(sql);
			}
		}

		void DeleteDirectories(IEnumerable<string> directories)
		{
			foreach (var dir in directories)
			{
				if (Directory.Exists(dir))
				{
					Directory.Delete(dir, true);
				}
			}
		}

		class SqlAlwaysOnSecondaryServerWorkerForTesting : SqlAlwaysOnSecondaryServerWorker
		{
			internal List<string> commandList = new List<string>();
			readonly string newDbName;
			internal string primaryDbFileDir { get; set; }
			internal bool IsMultipleDb { get; set; }

			public SqlAlwaysOnSecondaryServerWorkerForTesting(string availabilityGroup, string newDbName) : base(availabilityGroup, newDbName)
			{
				this.newDbName = newDbName;
			}

			public override bool AvailabilityGroupExists(AdminConnection secondaryServerConnection)
			{
				return true;
			}

			protected internal override bool IsHostedWithCargowise()
			{
				return true;
			}

			protected override bool IsDatabaseCreatedWithinGracePeriod() => PrimaryServerDatabaseCreationDateOverride;

			public bool PrimaryServerDatabaseCreationDateOverride { get; set; }

			public override void ExecuteNonQuery(AdminConnection connection, string sqlText)
			{
				this.commandList.Add(sqlText);
				if ((sqlText.StartsWith($@"RESTORE DATABASE [{newDbName}]", StringComparison.OrdinalIgnoreCase)) ||
					(sqlText.StartsWith($@"RESTORE LOG [{newDbName}]", StringComparison.OrdinalIgnoreCase)))
				{
					base.ExecuteNonQuery(connection, sqlText);
				}
			}

			public Action JoinDatabaseToSecondaryReplicaOverride { get; set; }

			protected override void JoinDatabaseToSecondaryReplica(AdminConnection connection)
			{
				if (JoinDatabaseToSecondaryReplicaOverride != null)
				{
					JoinDatabaseToSecondaryReplicaOverride();
				}
				else
				{
					base.JoinDatabaseToSecondaryReplica(connection);
				}
			}

			protected internal override string[] GetDBFiles(string dbName, AdminConnection connection)
			{
				var dbFiles = new List<string>
				{
				primaryDbFileDir + $@"\{newDbName}.mdf",
				primaryDbFileDir + $@"\{newDbName}_log.ldf"
				};

				if (IsMultipleDb)
				{
					var multipleDbDir = primaryDbFileDir + "2";
					dbFiles.Add(multipleDbDir + $@"\{newDbName}2.ndf");
					dbFiles.Add(multipleDbDir + $@"\{newDbName}2_log.ldf");
				}
				return dbFiles.ToArray();
			}
		}

		sealed class SqlAlwaysOnSecondaryServerWorkerForTestingThrowingOnLogRestore : SqlAlwaysOnSecondaryServerWorkerForTesting
		{
			public SqlAlwaysOnSecondaryServerWorkerForTestingThrowingOnLogRestore(string availabilityGroup, string newDbName) : base(availabilityGroup, newDbName)
			{
			}

			public override void ExecuteNonQuery(AdminConnection connection, string sqlText)
			{
				if (sqlText.StartsWith($@"RESTORE LOG", StringComparison.OrdinalIgnoreCase) && sqlText.Contains("20190102030407"))
				{
					this.commandList.Add(sqlText);
					throw SqlExceptionBuilder.CreateSqlException(3038, "message");
				}
				base.ExecuteNonQuery(connection, sqlText);
			}
		}

		sealed class SqlAlwaysOnSecondaryServerWorkerWithDatabaseAlreadyJoined : SqlAlwaysOnSecondaryServerWorker
		{
			public SqlAlwaysOnSecondaryServerWorkerWithDatabaseAlreadyJoined(string availabilityGroup, string databaseName) : base(availabilityGroup, databaseName)
			{
			}

			protected override bool IsDatabaseCreatedWithinGracePeriod() => false;

			public override bool AvailabilityGroupExists(AdminConnection secondaryServerConnection)
			{
				return true;
			}

			protected override void JoinDatabaseToSecondaryReplica(AdminConnection connection)
			{
				var errMessage = "Cannot join database '%.*ls' to availability group '%.*ls'. The database has already joined the availability group.";
				var error = SqlExceptionBuilder.CreateSqlError(41145, byte.MaxValue, byte.MinValue, Db.ServerName, errMessage, "@@NoProceedure", 0);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				throw SqlExceptionBuilder.CreateSqlException(errors);
			}
		}
	}
}
