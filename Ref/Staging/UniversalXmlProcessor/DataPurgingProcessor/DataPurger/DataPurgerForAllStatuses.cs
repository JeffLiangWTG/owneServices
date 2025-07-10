using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	public class DataPurgerForAllStatuses : AbstractDataPurger
	{
		public DataPurgerForAllStatuses(IStagingRepository stagingRepo) : base(stagingRepo)
		{
		}

		protected override IQueryable<DataProcessingInformation> GetDataProcessingInformation(SourceData sourceData)
		{
			return StagingRepo.Get<DataProcessingInformation>().Where(x => x.DPI_SourceId == sourceData.SDA_PK);
		}

		protected override string BuildInsertBatchDataProcessingInformationSql(Guid sourceid, Type rootType)
		{
			return $@"INSERT #{rootType.Name}Temp (PK)
SELECT TOP {BatchSize} {nameof(DataProcessingInformation.DPI_ParentPk)}
FROM  {nameof(DataProcessingInformation)}
WHERE {nameof(DataProcessingInformation.DPI_SourceId)} = '{sourceid}'";
		}

		protected override void SetSourceDataStatus(SourceData sourceData)
		{
			sourceData.SDA_Status = StatusProvider.GetPURStatus();
		}

		protected override IEnumerable<SourceData> FetchSourceData()
		{
			var purgeDataMonthsAgo = DateTime.Now.AddMonths(-Application.PurgeDataMonthsAgo);
			return StagingRepo.Get<SourceData>()
				.Where(x => !string.IsNullOrEmpty(x.SDA_SubSource) &&
					x.SDA_CreatedTime < purgeDataMonthsAgo &&
					StatusProvider.GetSourceDataFinalStatuses().Contains(x.SDA_Status));
		}
	}
}
