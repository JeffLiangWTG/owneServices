using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using ContractModel = CargoWise.RefDbRepo.Common.Contract_0_9;
using SchemaModel = CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefUNLOCOService : ReferenceDataServiceBase<ContractModel.RefUNLOCO>
	{
		protected override string TableCode => "RL";

		static RefUNLOCOService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<SchemaModel.RefCountry, ContractModel.RefCountry>();
				cfg.CreateMap<SchemaModel.RefLocoMap, ContractModel.RefLocoMap>();
				cfg.CreateMap<SchemaModel.RefCountryStates, ContractModel.RefCountryStates>();
				cfg.CreateMap<SchemaModel.RefTimeZone, ContractModel.RefTimeZone>();
				cfg.CreateMap<SchemaModel.RefTimeZoneSet, ContractModel.RefTimeZoneSet>();
				cfg.CreateMap<SchemaModel.RefUNLOCO, ContractModel.RefUNLOCO>().ForMember(x => x.RL_GeoLocation, o => o.MapFrom(s => s.RL_GeoLocation.AsText()));
				cfg.CreateMap<SchemaModel.RefUNLOCOUtcOffset, ContractModel.RefUNLOCOUtcOffset>();
				cfg.CreateMap<SchemaModel.RefUNLOCORelatedPort, ContractModel.RefUNLOCORelatedPort>();
			});
		}

		public RefUNLOCOService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
		}

		static MapperConfiguration config;
		static MapperConfiguration Config
		{
			get { return config; }
			set
			{
				if (config != null)
					throw new InvalidProgramException("Mapper configuration should not be set second time");
				else
					config = value;
			}
		}

		public override IEnumerable<ContractModel.RefUNLOCO> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}

		IEnumerable<ContractModel.RefUNLOCO> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var mapper = Config.CreateMapper();

			var countryStates = RefDbRepo.Get<SchemaModel.RefCountryStates>().WhereNotDeleted(RefDbRepo)
				.ToDictionary(x => x.RW_PK, x => mapper.Map<ContractModel.RefCountryStates>(x));

			var timeZoneSets = RefDbRepo.Get<SchemaModel.RefTimeZoneSet>().WhereNotDeleted(RefDbRepo)
				.ToDictionary(x => x.R3_PK, x => mapper.Map<ContractModel.RefTimeZoneSet>(x));

			var dataSets = from refUnloco in RefDbRepo.Get<SchemaModel.RefUNLOCO>()
						   join version in GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpoint) on refUnloco.RL_PK equals version.RVC_ParentPK

						   join locoMap in RefDbRepo.Get<SchemaModel.RefLocoMap>() on refUnloco.RL_Code equals locoMap.RY_RL_NKLocoPort into locoMapG
						   from locoMap in locoMapG.DefaultIfEmpty()

						   join utcOffset in RefDbRepo.Get<SchemaModel.RefUNLOCOUtcOffset>() on refUnloco.RL_Code equals utcOffset.RLO_RL_NKCode into utcOffsetG
						   from utcOffset in utcOffsetG.DefaultIfEmpty()

						   join relatedPort in RefDbRepo.Get<SchemaModel.RefUNLOCORelatedPort>() on refUnloco.RL_Code equals relatedPort.RLR_RL_NKRelatedPort into relatedPortG
						   from relatedPort in relatedPortG.DefaultIfEmpty()

						   select new { refUnloco, version.RVC_Deleted, locoMap, utcOffset, relatedPort };

			// GroupBy keeps the order of the elements in source that produced the first key.
			// https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?view=net-8.0
			var dataSetGroups = dataSets.OrderBy(x => x.refUnloco.RL_PK).GroupBy(x => x.refUnloco.RL_PK);

			var idx = 0;
			foreach (var dataSetG in dataSetGroups)
			{
				var dataSet = dataSetG.FirstOrDefault().refUnloco;

				var result = mapper.Map<ContractModel.RefUNLOCO>(dataSet);
				result.Deleted = dataSetG.FirstOrDefault().RVC_Deleted;

				if (dataSet.RL_RW.HasValue)
				{
					ContractModel.RefCountryStates countryState = null;
					countryStates.TryGetValue(dataSet.RL_RW.Value, out countryState);
					result.RefCountryStates = countryState;
				}

				if (dataSet.RL_R3.HasValue)
				{
					ContractModel.RefTimeZoneSet timeZoneSet = null;
					timeZoneSets.TryGetValue(dataSet.RL_R3.Value, out timeZoneSet);
					result.RefTimeZoneSet = timeZoneSet;
				}

				if (!result.Deleted)
				{
					result.RefLocoMaps = dataSetG.Select(x => x.locoMap).Where(x => x != null).DistinctByKey(x => x.RY_PK).Select(x => mapper.Map<ContractModel.RefLocoMap>(x)).ToArray();
					result.RefUNLOCOUtcOffsets = dataSetG.Select(x => x.utcOffset).Where(x => x != null).DistinctByKey(x => x.RLO_PK).Select(x => mapper.Map<ContractModel.RefUNLOCOUtcOffset>(x)).ToArray();
					result.RefUNLOCORelatedPorts = dataSetG.Select(x => x.relatedPort).Where(x => x != null).DistinctByKey(x => x.RLR_PK).Select(x => mapper.Map<ContractModel.RefUNLOCORelatedPort>(x)).ToArray();
				}

				result.WriteCheckpoint(dataSetG.Key, ref idx, chunkSize);
				yield return result;
			}
		}
	}
}
