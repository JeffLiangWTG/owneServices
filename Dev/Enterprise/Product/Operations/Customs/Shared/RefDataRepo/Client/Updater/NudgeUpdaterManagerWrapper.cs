using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using static Enterprise.Integration.Customs.Shared;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class NudgeUpdaterManagerWrapper : SharedDataSetUpdaterWrapper, INudgeUpdaterManagerWrapper
	{
		public NudgeUpdaterManagerWrapper()
		{
		}

		public override IEnumerable<ISharedDataSetUpdater> GetAllDataSetUpdater()
		{
			throw new NotImplementedException("GetAllDataSetUpdater Method should not be called from NudgeUpdaterManagerWrapper");
		}

		public override bool Update(string tableName, string dataSet)
		{
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(dataSet, nameof(dataSet));

			if (RefDataSetUpdaterWrapper.GetAllDataSetUpdater().Any(x => DataSetUpdaterHelper.GetTableName(x.Name) == tableName))
			{
				return RefDataSetUpdaterWrapper.Update(tableName, dataSet);
			}
			else if (SRDbDataSetUpdaterWrapper.GetAllDataSetUpdater().Any(x => DataSetUpdaterHelper.GetTableName(x.Name) == tableName))
			{
				return SRDbDataSetUpdaterWrapper.Update(tableName, dataSet);
			}
			throw new InvalidOperationException($"DataSet {dataSet} could not be found in RDU and REF mappings");
		}

		public override bool UpdateAll()
		{
			throw new NotImplementedException("UpdateAll Method should not be called from NudgeUpdaterManagerWrapper");
		}

		public IRefDataSetUpdaterWrapper RefDataSetUpdaterWrapper => refDataSetUpdaterWrapper ?? (refDataSetUpdaterWrapper = ObjectFactory.Get<IRefDataSetUpdaterWrapper>());
		protected IRefDataSetUpdaterWrapper refDataSetUpdaterWrapper;

		public ISRDbDataSetUpdaterWrapper SRDbDataSetUpdaterWrapper => sRDbDataSetUpdaterWrapper ?? (sRDbDataSetUpdaterWrapper = ObjectFactory.Get<ISRDbDataSetUpdaterWrapper>());
		protected ISRDbDataSetUpdaterWrapper sRDbDataSetUpdaterWrapper;
	}
}
