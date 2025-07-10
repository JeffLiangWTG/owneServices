using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.Integration;

namespace CargoWise.RefDataRepo.Ent.Client.RemoteDbUpgradeServiceTask
{
	public class RemoteDbServiceTaskDataSetUpdaterManagerWrapper : SharedDataSetUpdaterWrapper, Customs.Shared.ISRDbDataSetUpdaterWrapper
	{
		readonly IServerProxyHelper _serverProxyHelper;
		readonly IEnumerable<string> serverList;

		public RemoteDbServiceTaskDataSetUpdaterManagerWrapper(Enterprise.Integration.ILogger logger, Action<RefVersionControlManager> onInitializeDb, IEnumerable<string> serverList)
		{
			base.logger = new ServiceLoggerWrapper(logger);
			base.onInitializeDb = onInitializeDb;
			_serverProxyHelper = new ServerProxyHelper(base.logger, RemoteDatabaseRegistry.Instance);
			this.serverList = serverList;
		}

		public override IEnumerable<Customs.Shared.ISharedDataSetUpdater> GetAllDataSetUpdater()
		{
			throw new NotImplementedException("GetAllDataSetUpdater Method should not be called from RemoteDbServiceTaskDataSetUpdaterManagerWrapper");
		}

		public override bool Update(string tableName, string dataSet)
		{
			throw new NotImplementedException("Update(string, string) Method should not be called from RemoteDbServiceTaskDataSetUpdaterManagerWrapper");
		}

		protected virtual IDataSetUpdaterManager SetupManager(IDbConnection dbConnection, ISRDbUpdaterRegistration updaterRegistration, IClientConfiguration config)
		{
			Argument.NotNull(dbConnection, nameof(dbConnection));
			Argument.NotNull(updaterRegistration, nameof(updaterRegistration));

			IDBHelper dbHelper = new DBHelper(dbConnection);
			var errorReportingWrapper = new ErrorReportingClientWrapper(true);

			var dataSetUpdaters = updaterRegistration.Get(_serverProxyHelper.GetServerProxy(), dbHelper, errorReportingWrapper, logger);

			if (dataSetUpdaters == null || dataSetUpdaters.Length == 0)
			{
				return null;
			}

			return new DataSetUpdaterManager<ISRDbDataSetUpdater>(logger, dbConnection, new SRDbUpdaterDependencyProvider(dataSetUpdaters), config, errorReportingWrapper);
		}

		public override bool UpdateAll()
		{
			if (!_serverProxyHelper.CanInitiliseServerProxy())
			{
				return false;
			}

			var result = true;
			savedDataSetCount = 0;

			foreach (var server in serverList)
			{
#if DEBUG
				if (server == Db.ServerName)
				{
					using (var mainConnection = Db.NewAdminConnection())
					{
						IDBHelper dbHelper = new DBHelper(((IDbConnectionInternals)mainConnection).ADOConnection);
						if (!dbHelper.IsRefDatabaseSynonymAllPointToSRDb())
						{
							throw new ApplicationException("RefDatabase Synonym not set correctly, please go to Help -> Database Administration -> Restore Single Reference Database Synonyms, to restore synonym for SRDb");
						}
					}
				}
#endif
				using (var connection = Db.NewAdminConnection(server, InitialDatabase))
				{
					var rDUDataSetUpdaterManager = SetupManager(((IDbConnectionInternals)connection).ADOConnection, new SRDbUpdaterRegistration(), RemoteDatabaseRegistry.Instance);
					if (rDUDataSetUpdaterManager == null)
					{
						result = false;
					}
					else
					{
						logger.WriteLine($"Start data update on server {server}.");
						result = rDUDataSetUpdaterManager.UpdateAll() & result;
						savedDataSetCount += rDUDataSetUpdaterManager.SavedDataSetCount;
					}
				}
			}

			return result;
		}

		public bool IsSchemaUpgradeSuccessful()
		{
			throw new NotImplementedException("IsSchemaUpgradeSuccessful() Method should not be called from RemoteDbServiceTaskDataSetUpdaterManagerWrapper");
		}

		protected override string InitialDatabase => RefDbTableNameResolver.SingleRefDatabaseName;
	}
}
