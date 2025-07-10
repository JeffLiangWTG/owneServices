using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public partial class RefCarrierCodeService
	{
		static MapperConfiguration configUpToV33;
		static void SetupMapperConfigurationUpToV33()
		{
			if (configUpToV33 == null)
			{
				configUpToV33 = new MapperConfiguration(cfg =>
				{
					cfg.CreateMap<RefCarrierCode, Models.RefCarrierCode>();
					cfg.CreateMap<RefCarrierCodeAttribute, Models.RefCarrierCodeAttribute>();
					cfg.CreateMap<RefVesselZZ, Models.RefVesselZZ>();
					cfg.CreateMap<RefCarrierVesselPivot, Models.RefCarrierVesselPivot>();
				});
			}
		}

		public IEnumerable<Models.RefCarrierCode> GetDataUpToV33(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short dataSetId)
		{
			SetupMapperConfigurationUpToV33();
			return GetDataCoreUpToV33(lowerTimestamp, upperTimestamp, checkpointPK, chunkSize, dataSetId);
		}

		IEnumerable<Models.RefCarrierCode> GetDataCoreUpToV33(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short datasetId)
		{
			var mapper = configUpToV33.CreateMapper();
			var vesselListForVersion33 = GetVesselsUpToV33().ToArray();

			var dataSets = from carrier in RefDbRepo.Get<RefCarrierCode>()
						   join version in GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpointPK) on carrier.ZZ4_PK equals version.RVC_ParentPK

						   join attr in RefDbRepo.Get<RefCarrierCodeAttribute>() on carrier.ZZ4_PK equals attr.ZZG_ZZ4_CarrierCode into attrG
						   from attr in attrG.DefaultIfEmpty()

						   join pivot in RefDbRepo.Get<RefCarrierVesselPivot>() on carrier.ZZ4_PK equals pivot.ZZQ_ZZ4 into pivotG
						   from pivot in pivotG.DefaultIfEmpty()

						   select new { carrier, version.RVC_Deleted, attr, pivot };

			// GroupBy keeps the order of the elements in source that produced the first key.
			// https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?view=net-8.0
			var dataSetGroups = dataSets.OrderBy(x => x.carrier.ZZ4_PK).GroupBy(x => x.carrier.ZZ4_PK);

			int idx = 0;
			foreach (var dataSetG in dataSetGroups)
			{
				var dataSet = dataSetG.FirstOrDefault().carrier;

				var result = mapper.Map<Models.RefCarrierCode>(dataSet);
				result.Deleted = dataSetG.FirstOrDefault().RVC_Deleted;

				if (!result.Deleted)
				{
					result.RefCarrierCodeAttributes = dataSetG.Select(x => x.attr).NotNull().DistinctByKey(x => x.ZZG_PK).Select(x => mapper.Map<Models.RefCarrierCodeAttribute>(x)).ToArray();
					result.RefCarrierVesselPivots = dataSetG.Select(x => x.pivot).NotNull().DistinctByKey(x => x.ZZQ_PK).Select(p =>
					{
						var pivot = mapper.Map<Models.RefCarrierVesselPivot>(p);
						pivot.RefVesselZZ = mapper.Map<Models.RefVesselZZ>(vesselListForVersion33.FirstOrDefault(x => x.ZZO_PK == p.ZZQ_ZZO));
						return pivot.RefVesselZZ != null ? pivot : null;
					}).NotNull().ToArray();
				}
				result.WriteCheckpoint(dataSetG.Key, ref idx, chunkSize);
				yield return result;
			}
		}

		IEnumerable<RefVesselZZ> GetVesselsUpToV33()
		{
			var vesselZZDataSetId = GetDataSetId(nameof(RefVesselZZ));
			var vessels = from vessel in RefDbRepo.Get<RefVesselZZ>()
						  join version in GetVersionControls(vesselZZDataSetId) on vessel.ZZO_PK equals version.RVC_ParentPK
						  join vessel2 in RefDbRepo.Get<RefVesselZZ>() on new { vessel.ZZO_Code, vessel.ZZO_ZZZ_NKDataGrouping } equals new { vessel2.ZZO_Code, vessel2.ZZO_ZZZ_NKDataGrouping }
						  orderby vessel.ZZO_PK ascending, vessel2.ZZO_PK descending
						  select new { vessel, version.RVC_Deleted, vessel2PK = vessel2.ZZO_PK };

			// GroupBy keeps the order of the elements in source that produced the first key.
			// https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?view=net-8.0
			var dataSetGroups = vessels.GroupBy(x => x.vessel.ZZO_PK);

			foreach (var dataSetG in dataSetGroups)
			{
				var dataSet = dataSetG.FirstOrDefault();
				if (dataSet.vessel.ZZO_PK == dataSet.vessel2PK && !dataSet.RVC_Deleted)
				{
					yield return dataSet.vessel;
				}
			}
		}
	}
}
