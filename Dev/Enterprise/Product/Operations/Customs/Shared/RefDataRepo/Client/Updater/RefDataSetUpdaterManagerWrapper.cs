using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.Integration;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class RefDataSetUpdaterManagerWrapper : SharedDataSetUpdaterWrapper, Customs.Shared.IRefDataSetUpdaterWrapper
	{
		readonly IServerProxyHelper _serverProxyHelper;

		public RefDataSetUpdaterManagerWrapper()
		{
			_serverProxyHelper = new ServerProxyHelper(logger, RefDataRepoRegistry.Instance);
		}

		public override IEnumerable<Customs.Shared.ISharedDataSetUpdater> GetAllDataSetUpdater()
		{
			if (!preConditionCheck)
			{
				return null;
			}

			InitializeDataSetUpdaterMapperIfRequired();
			return dataSetUpdaterMapper.GetDataSets();
		}

		public override bool Update(string tableName, string dataSet)
		{
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(dataSet, nameof(dataSet));

			if (!preConditionCheck)
			{
				return false;
			}

			InitializeDataSetUpdaterMapperIfRequired();
			savedDataSetCount = 0;

			var dataSetUpdater = dataSetUpdaterMapper.GetDataSetUpdater(tableName);
			var dataSetUpdaterManager = dataSetUpdaterMapper.GetDataSetUpdaterManager(tableName);
			var dataSetVersion = dataSetUpdaterMapper.GetDataSetVersion(tableName, dataSet);

			var result = dataSetUpdaterManager.Update(dataSetVersion, dataSetUpdater);
			savedDataSetCount = dataSetUpdaterManager.SavedDataSetCount;

			return result;
		}

		public override bool UpdateAll()
		{
			throw new NotImplementedException("UpdateAll Method should not be called from RefDataSetUpdateManagerWrapper");
		}

		public override void Dispose()
		{
			dataSetUpdaterMapper = null;
			base.Dispose();
		}

		protected virtual bool preConditionCheck => DbInitializationCheck() && _serverProxyHelper.CanInitiliseServerProxy();

		#region Helper Methods

		void InitializeDataSetUpdaterMapperIfRequired()
		{
			if (dataSetUpdaterMapper != null)
			{
				return;
			}

			dataSetUpdaterMapper = new DataSetUpdaterMapper(_serverProxyHelper.GetServerProxy(), logger, RefDataRepoRegistry.Instance);
		}

		#endregion

		protected IDataSetUpdaterMapper dataSetUpdaterMapper;
	}
}
