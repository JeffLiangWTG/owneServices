using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	public class DataPurgerForPRSStatus : AbstractDataPurger
	{
		public DataPurgerForPRSStatus(IStagingRepository stagingRepo) : base(stagingRepo)
		{
		}

		protected override IQueryable<DataProcessingInformation> GetDataProcessingInformation(SourceData sourceData)
		{
			return StagingRepo.Get<DataProcessingInformation>().Where(x => StatusProvider.GetDataProcessingInformationProcessedStatuses().Contains(x.DPI_Status)
				&& x.DPI_SourceId == sourceData.SDA_PK);
		}

		protected override string BuildInsertBatchDataProcessingInformationSql(Guid sourceid, Type rootType)
		{
			return $@"INSERT #{rootType.Name}Temp (PK)
SELECT TOP {BatchSize} {nameof(DataProcessingInformation.DPI_ParentPk)}
FROM  {nameof(DataProcessingInformation)}
WHERE {nameof(DataProcessingInformation.DPI_SourceId)} = '{sourceid}' AND {nameof(DataProcessingInformation.DPI_Status)} = '{StatusProvider.GetPRSStatus()}'";
		}

		protected override void SetSourceDataStatus(SourceData sourceData)
		{
			sourceData.SDA_Status = sourceData.IsSourceDataMergedStatus() ? StatusProvider.GetFINStatus() : StatusProvider.GetFIEStatus();
		}

		protected override IEnumerable<SourceData> FetchSourceData()
		{
			var subSourceGroups = StagingRepo.Get<SourceData>()
				.Where(x => !string.IsNullOrEmpty(x.SDA_SubSource) &&
					StatusProvider.GetSourceDataToBeFinalisedStatuses().Contains(x.SDA_Status))
				.GroupBy(x => x.SDA_SubSource);
			foreach (var subSourceGroup in subSourceGroups)
			{
				var sourceGroup = subSourceGroup.OrderByDescending(x => x.SDA_CreatedTime).Skip(1);
				foreach (var gr in sourceGroup)
				{
					yield return gr;
				}
			}
		}
	}
}
