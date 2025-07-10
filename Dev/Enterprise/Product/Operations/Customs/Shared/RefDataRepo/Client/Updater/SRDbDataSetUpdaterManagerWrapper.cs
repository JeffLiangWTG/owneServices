using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.Integration;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class SRDbDataSetUpdaterManagerWrapper : SharedDataSetUpdaterWrapper, Customs.Shared.ISRDbDataSetUpdaterWrapper
	{
		readonly IServerProxyHelper _serverProxyHelper;
#if DEBUG
		public
#endif
		readonly DbConnection dbConnection;

		public SRDbDataSetUpdaterManagerWrapper()
		{
			_serverProxyHelper = new ServerProxyHelper(logger, RemoteDatabaseRegistry.Instance);
			dbConnection = Db.NewAdminConnection(InitialDatabase);
		}

		public override IEnumerable<Customs.Shared.ISharedDataSetUpdater> GetAllDataSetUpdater()
		{
			if (!preConditionCheck)
			{
				return null;
			}

			InitializeSRDbDataSetUpdaterMapperIfRequired();
			return sRDbMapper.GetDataSets();
		}

		public override bool Update(string tableName, string dataSet)
		{
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(dataSet, nameof(dataSet));

			if (!preConditionCheck)
			{
				return false;
			}

			InitializeSRDbDataSetUpdaterMapperIfRequired();
			savedDataSetCount = 0;

			var dataSetUpdater = sRDbMapper.GetSRDbDataSetUpdater(tableName);
			var dataSetUpdaterManager = sRDbMapper.GetDataSetUpdaterManager(tableName);
			var dataSetVersion = sRDbMapper.GetDataSetVersion(tableName, dataSet);

			var result = dataSetUpdaterManager.Update(dataSetVersion, dataSetUpdater);
			savedDataSetCount = dataSetUpdaterManager.SavedDataSetCount;

			return result;
		}

		public override bool UpdateAll()
		{
			throw new NotImplementedException("UpdateAll Method should not be called from SRDbDataSetUpdaterManagerWrapper");
		}

		public override void Dispose()
		{
			sRDbMapper = null;
			dbConnection.Dispose();
			base.Dispose();
		}

		public bool IsSchemaUpgradeSuccessful()
		{
			if (!preConditionCheck)
			{
				return false;
			}
			var sRDbUpgrader = new RefDataBaseUpgrader(logger, _serverProxyHelper.GetServerProxy(), new DBUpgradeHelper(((IDbConnectionInternals)dbConnection).ADOConnection));
			return sRDbUpgrader.DoUpgrade(((IDbConnectionInternals)dbConnection).ADOTransaction);
		}

		protected virtual bool preConditionCheck => _serverProxyHelper.CanInitiliseServerProxy();

		#region Helper Methods

		void InitializeSRDbDataSetUpdaterMapperIfRequired()
		{
			var adoConnection = ((IDbConnectionInternals)dbConnection).ADOConnection;
			if (adoConnection.State != System.Data.ConnectionState.Open)
			{
				adoConnection.Open();
			}
			if (adoConnection.Database != InitialDatabase)
			{
				adoConnection.ChangeDatabase(InitialDatabase);
			}

			if (sRDbMapper != null)
			{
				return;
			}

			sRDbMapper = new SRDbDataSetUpdaterMapper(_serverProxyHelper.GetServerProxy(), logger, RemoteDatabaseRegistry.Instance, adoConnection);
		}

		#endregion

		protected IDataSetUpdaterMapper sRDbMapper;
		protected override string InitialDatabase => RefDbTableNameResolver.SingleRefDatabaseName;
	}
}
