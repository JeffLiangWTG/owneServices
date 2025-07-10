using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public partial class RefVesselZZService
	{
		public IEnumerable<Models.RefVesselZZ> GetDataUpToV33(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short dataSetId)
		{
			return GetDataCoreUpToV33(lowerTimestamp, upperTimestamp, checkpointPK, chunkSize, dataSetId);
		}

		IEnumerable<Models.RefVesselZZ> GetDataCoreUpToV33(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short datasetId)
		{
			var mapper = Config.CreateMapper();

			var dataSets = from vessel in RefDbRepo.Get<RefVesselZZ>()
						   join version in GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpointPK) on vessel.ZZO_PK equals version.RVC_ParentPK
						   join vessel2 in RefDbRepo.Get<RefVesselZZ>() on new { vessel.ZZO_Code, vessel.ZZO_ZZZ_NKDataGrouping } equals new { vessel2.ZZO_Code, vessel2.ZZO_ZZZ_NKDataGrouping }
						   orderby vessel.ZZO_PK ascending, vessel2.ZZO_PK descending
						   select new { vessel, version, vessel2PK = vessel2.ZZO_PK };

			int idx = 0;
			Guid lastGuid = Guid.Empty;
			foreach (var dataSet in dataSets)
			{
				if (lastGuid == Guid.Empty || lastGuid != dataSet.vessel.ZZO_PK)
				{
					lastGuid = dataSet.vessel.ZZO_PK;
					if (dataSet.vessel.ZZO_PK == dataSet.vessel2PK)
					{
						var result = mapper.Map<Models.RefVesselZZ>(dataSet.vessel);
						result.Deleted = dataSet.version.RVC_Deleted;
						result.WriteCheckpoint(dataSet.vessel.ZZO_PK, ref idx, chunkSize);
						yield return result;
					}
				}
			}
		}
	}
}
