using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefCusApplicabilityService
	{
		static RefCusApplicabilityService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusApplicability, Models.RefCusApplicability>();
				cfg.CreateMap<RefCusTradeGroup, Models.RefCusTradeGroup>();
				cfg.CreateMap<RefCusExcludedTradeGroup, Models.RefCusExcludedTradeGroup>();
			});
		}

		public RefCusApplicabilityService(IReadOnlyReferenceDataRepository refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
			mapper = Config.CreateMapper();
			this.RefDbRepo = refDbRepo;
		}

		static MapperConfiguration config;
		static MapperConfiguration Config
		{
			get { return config; }
			set
			{
				if (config != null) throw new InvalidProgramException("Mapper configuration should not be set second time");
				else config = value;
			}
		}

		readonly IMapper mapper;
		readonly IReadOnlyReferenceDataRepository RefDbRepo;

		Dictionary<Guid, Models.RefCusTradeGroup> TradeGroups
		{
			get { return fTradeGroups ?? (fTradeGroups = RefDbRepo.Get<RefCusTradeGroup>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZZA_PK, x => mapper.Map<Models.RefCusTradeGroup>(x))); }
		}
		Dictionary<Guid, Models.RefCusTradeGroup> fTradeGroups;

		public RefCusExcludedTradeGroup[] GetExcludedTradeGroupChunk(string parentCode, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, short dataSetId, Guid? startPK, Guid? endPK)
		{
			Argument.NotNullOrEmpty(parentCode, nameof(parentCode));

			return RefDbRepo.Get<RefCusExcludedTradeGroup>().Where(x => x.ZZC_DataSetCode == parentCode)
				.GetChunk(RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_DataSetId == dataSetId && x.RVC_IsPublished), x => x.ZZC_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
		}

		public RefCusApplicability[] GetCusApplicabilityChunk(string parentCode, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, short dataSetId, Guid? startPK, Guid? endPK)
		{
			Argument.NotNullOrEmpty(parentCode, nameof(parentCode));

			return RefDbRepo.Get<RefCusApplicability>().Where(x => x.ZZT_DataSetCode == parentCode)
				.GetChunk(RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_DataSetId == dataSetId && x.RVC_IsPublished), x => x.ZZT_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
		}

		public IEnumerable<IGrouping<Guid?, Models.RefCusApplicability>> GetSetData(RefCusApplicability[] applicabilityChunk, RefCusExcludedTradeGroup[] excludedTradeGroupChunk,
			Guid dataSetPK, ref int applicabilityIdx, ref int excludedTradeIdx)
		{
			Argument.NotNull(applicabilityChunk, nameof(applicabilityChunk));
			Argument.NotNull(excludedTradeGroupChunk, nameof(excludedTradeGroupChunk));
			Argument.GreaterThanOrEqual(applicabilityIdx, 0, nameof(applicabilityIdx));
			Argument.GreaterThanOrEqual(excludedTradeIdx, 0, nameof(excludedTradeIdx));

			var excludedTrades = excludedTradeGroupChunk.GetSetData(x => x.ZZC_DataSetPK, dataSetPK, ref excludedTradeIdx).Where(x => TradeGroups.ContainsKey(x.ZZC_ZZA_TradeGroup));
			return applicabilityChunk.GetSetData(x => x.ZZT_DataSetPK, dataSetPK, ref applicabilityIdx).Where(x => !x.ZZT_ZZA_TradeGroup.HasValue || TradeGroups.ContainsKey(x.ZZT_ZZA_TradeGroup.Value))
				.Where(x => !x.ZZT_ZZA_SecondTradeGroup.HasValue || TradeGroups.ContainsKey(x.ZZT_ZZA_SecondTradeGroup.Value))
				.GroupBy(x => x.ZZT_ZX1_Conditions ?? x.ZZT_ZZ2_Rate ?? x.ZZT_ZY2_AdditionalCode ?? x.ZZT_ZZH_TariffRelationship, x =>
			{
				var model = mapper.Map<Models.RefCusApplicability>(x);
				model.RefCusTradeGroup = x.ZZT_ZZA_TradeGroup.HasValue ? TradeGroups[x.ZZT_ZZA_TradeGroup.Value] : null;
				model.RefCusTradeGroup1 = x.ZZT_ZZA_SecondTradeGroup.HasValue ? TradeGroups[x.ZZT_ZZA_SecondTradeGroup.Value] : null;
				model.RefCusExcludedTradeGroups = excludedTrades.Where(t => t.ZZC_ZZT_Applicability == x.ZZT_PK).Select(t =>
				{
					var tradeM = mapper.Map<Models.RefCusExcludedTradeGroup>(t);
					tradeM.RefCusTradeGroup = TradeGroups[t.ZZC_ZZA_TradeGroup];
					return tradeM;
				}).ToArray();
				return model;
			}).ToArray();
		}
	}
}
