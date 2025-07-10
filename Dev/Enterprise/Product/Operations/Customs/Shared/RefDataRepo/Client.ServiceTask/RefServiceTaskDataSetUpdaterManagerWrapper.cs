using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.Integration;

namespace CargoWise.RefDataRepo.Ent.Client.ServiceTask
{
	public class RefServiceTaskDataSetUpdaterManagerWrapper : SharedDataSetUpdaterWrapper, Customs.Shared.IRefDataSetUpdaterWrapper
	{
		readonly IServerProxyHelper _serverProxyHelper;

		public RefServiceTaskDataSetUpdaterManagerWrapper(Enterprise.Integration.ILogger logger, Action<RefVersionControlManager> onInitializeDb)
		{
			base.logger = new ServiceLoggerWrapper(logger);
			base.onInitializeDb = onInitializeDb;
			_serverProxyHelper = new ServerProxyHelper(base.logger, RefDataRepoRegistry.Instance);
		}

		public override IEnumerable<Customs.Shared.ISharedDataSetUpdater> GetAllDataSetUpdater()
		{
			throw new NotImplementedException("GetAllDataSetUpdater Method should not be called from RefServiceTaskDataSetUpdaterManagerWrapper");
		}

		public override bool Update(string tableName, string dataSet)
		{
			throw new NotImplementedException("Update(string, string) Method should not be called from RefServiceTaskDataSetUpdaterManagerWrapper");
		}

		public override bool UpdateAll()
		{
			if (!_serverProxyHelper.CanInitiliseServerProxy())
			{
				return false;
			}

			var result = true;
			savedDataSetCount = 0;

			using (var mainConnection = Db.NewAdminConnection())
			{
				var mainDbDataSetUpdaterManager = SetupManager(((IDbConnectionInternals)mainConnection).ADOConnection, new MainDbUpdaterRegistration(), RefDataRepoRegistry.Instance);
				result = mainDbDataSetUpdaterManager.UpdateAll() && result;
				savedDataSetCount += mainDbDataSetUpdaterManager.SavedDataSetCount;
			}

			return result;
		}

		#region Helper Methods

		protected virtual IDataSetUpdaterManager SetupManager(IDbConnection dbConnection, IUpdaterRegistration updaterRegistration, IClientConfiguration config)
		{
			Argument.NotNull(dbConnection, nameof(dbConnection));
			Argument.NotNull(updaterRegistration, nameof(updaterRegistration));

			var dbHelper = new DBHelper(dbConnection);

			var dataSetUpdaters = GetDataSetUpdatersWithLogger(updaterRegistration, dbHelper);
			var dependencyProvider = new UpdaterDependencyProvider<IDataSetUpdater>(dataSetUpdaters, dbHelper.GetReferencedForeignKeys().ToArray());
			dependencyProvider.Initialize();

			return new DataSetUpdaterManager<IDataSetUpdater>(logger, dbConnection, dependencyProvider, config, new ErrorReportingClientWrapper(false));
		}

		protected virtual IDataSetUpdater[] GetDataSetUpdatersWithLogger(IUpdaterRegistration updaterRegistration, IDBHelper dbHelper)
		{
			var dataSetUpdaters = updaterRegistration.Get(_serverProxyHelper.GetServerProxy(), dbHelper);
			dataSetUpdaters.ForEach(x => x.Logger = logger);
			return dataSetUpdaters;
		}

		#endregion
	}
}
