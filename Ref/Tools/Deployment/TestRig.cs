using System.Collections.Generic;
using CargoWise.RefDbRepo.Deployment.TestRigConfiguration;
using CargoWise.RefDbRepo.Tools.Common;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Deployment
{
	public class TestRig
	{
		readonly TestedShelfDeploymentOptions _options;

		public TestRig(TestedShelfDeploymentOptions options)
		{
			_options = options;
		}

		public List<(string paramName, object paramValue)> GetTestRigParameters()
		{
			var parameters = new List<(string paramName, object paramValue)>();
			//regular parameters
			parameters.Add(("xmlProducersDestination", $@"RefDataRepoTestRigs\{_options.UpdateServiceDomain}\UniversalXMLProducers\"));
			parameters.Add(("stagingDestination", $@"RefDataRepoTestRigs\{_options.UpdateServiceDomain}\StagingDeployTest\"));
			parameters.Add(("stagingServer", new[] { _options.WebServer }));
			parameters.Add(("dbWriterUsername", "RefDbRepoAdmin"));
			parameters.Add(("dbReaderUsername", "RefDbRepoAdmin"));
			parameters.Add(("dbWriterPassword", DATCrypto.Decrypt(Passwords.TEST_WriterPassword)));
			parameters.Add(("dbReaderPassword", DATCrypto.Decrypt(Passwords.TEST_ReaderPassword)));
			parameters.Add(("dbWriterPasswordJson", DATCrypto.Decrypt(Passwords.TEST_WriterPassword)));
			parameters.Add(("dbReaderPasswordJson", DATCrypto.Decrypt(Passwords.TEST_ReaderPassword)));
			parameters.Add(("refDbRepoSafeDbName", _options.SafeDbName));
			parameters.Add(("refDbRepoStagingDbName", _options.StagingDbName));
			parameters.Add(("refDbRepoDbServer", _options.SqlServer));
			parameters.Add(("deliveryServiceUrl", _options.DeliveryServiceDomain));
			parameters.Add(("updateServiceUrl", _options.UpdateServiceDomain));
			parameters.Add(("deliveryServiceDeployUsername", _options.DeployUsername));
			parameters.Add(("deliveryServiceDeployPassword", _options.DeployPassword));
			parameters.Add(("updateServiceDeployUsername", _options.DeployUsername));
			parameters.Add(("updateServiceDeployPassword", _options.DeployPassword));
			parameters.Add(("stagingDeployUsername", _options.DeployUsername));
			parameters.Add(("stagingDeployPassword", _options.DeployPassword));
			parameters.Add(("deployQuartz", false));
			parameters.Add(("forceSafeDbUprade", string.IsNullOrEmpty(_options.RefDbRepoSafeRestoreFromBackup)));
			parameters.Add(("forceStagingDbUprade", string.IsNullOrEmpty(_options.RefDbRepoStagingRestoreFromBackup)));
			return parameters;
		}

		string GetConnectionStringToMaster(string sqlServer = "")
		{
			return $"Data Source={(!string.IsNullOrEmpty(sqlServer) ? sqlServer : _options.SqlServer)};Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
		}

		public void DropDatabases()
		{
			DropDatabase(_options.SafeDbName);
			DropDatabase($"{_options.SafeDbName}Template");
			DropDatabase(_options.StagingDbName);
			DropDatabase($"{_options.StagingDbName}Template");
		}

		void DropDatabase(string dbName)
		{
			var availableDbServers = TestedShelfDeploymentOptions.Defaults.SQLDataServers.Split(';');
			foreach (var sqlServer in availableDbServers)
			{
				var connString = GetConnectionStringToMaster(sqlServer);
				DBHelper.DropDatabase(connString, dbName);
			}
		}

		public void CreateDbAndRestoreFromBackupIfNeeded()
		{
			CreateAndRestoreDb(_options.RefDbRepoSafeRestoreFromBackup, _options.SafeDbName);
			CreateAndRestoreDb(_options.RefDbRepoStagingRestoreFromBackup, _options.StagingDbName);

			if (!string.IsNullOrEmpty(_options.RefDbRepoStagingRestoreFromBackup))
			{
				DeleteSpecificDataFromRestoredDb(_options.StagingDbName);
			}
		}

		void DeleteSpecificDataFromRestoredDb(string dbName)
		{
			var connString = GetConnectionStringToMaster();
			connString = connString.Replace("master", dbName);

			using (var conn = new SqlConnection(connString))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					//deletes quartz triggers and application attribute overrides.
					cmd.CommandText = @"
DELETE FROM QRTZ_CRON_TRIGGERS;
DELETE FROM QRTZ_SIMPLE_TRIGGERS;
DELETE FROM QRTZ_FIRED_TRIGGERS;
DELETE FROM QRTZ_BLOB_TRIGGERS;
DELETE FROM QRTZ_PAUSED_TRIGGER_GRPS;
DELETE FROM QRTZ_SIMPROP_TRIGGERS;
DELETE FROM QRTZ_TRIGGERS;

DELETE FROM RefApplicationAttribute;";
					cmd.ExecuteNonQuery();
				}
			}
		}

		void CreateAndRestoreDb(string backupPath, string dbName)
		{
			var connString = GetConnectionStringToMaster();
			if (string.IsNullOrEmpty(backupPath))
			{
				DBHelper.DropAndCreateNewDatabase(connString, dbName, null);
				return;
			}
			var databaseInfo = new DatabaseInfo(dbName, backupPath);
			using (var conn = new SqlConnection(connString))
			{
				conn.Open();
				DBHelper.DropAndCreateNewDatabase(connString, dbName,
					() => RestoreManager.GetRestoreScript(new BackupFileInfo().GetBackupFileInfoArray(conn, databaseInfo), databaseInfo));
			}
		}
	}
}
