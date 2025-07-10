using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefCusConditionService
	{
		static RefCusConditionService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusCondition, Models.RefCusCondition>();
				cfg.CreateMap<RefCusConditionValue, Models.RefCusConditionValue>();
				cfg.CreateMap<RefCusConditionLanguage, Models.RefCusConditionLanguage>();
				cfg.CreateMap<RefCusConditionValueType, Models.RefCusConditionValueType>();
				cfg.CreateMap<RefCusConditionType, Models.RefCusConditionType>();
				cfg.CreateMap<RefCusPreference, Models.RefCusPreference>();
			});
		}

		public RefCusConditionService(IReadOnlyReferenceDataRepository refDbRepo)
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

		Dictionary<Guid, Models.RefCusConditionValueType> ConditionValueTypes
		{
			get { return fConditionValueTypes ?? (fConditionValueTypes = RefDbRepo.Get<RefCusConditionValueType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZX4_PK, x => mapper.Map<Models.RefCusConditionValueType>(x))); }
		}
		Dictionary<Guid, Models.RefCusConditionValueType> fConditionValueTypes;

		Dictionary<Guid, Models.RefCusConditionType> ConditionTypes
		{
			get { return fConditionTypes ?? (fConditionTypes = RefDbRepo.Get<RefCusConditionType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZX2_PK, x => mapper.Map<Models.RefCusConditionType>(x))); }
		}
		Dictionary<Guid, Models.RefCusConditionType> fConditionTypes;

		public RefCusCondition[] GetConditionChunk(string parentCode, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, short dataSetId, Guid? startPK, Guid? endPK)
		{
			return RefDbRepo.Get<RefCusCondition>().Where(x => x.ZX1_DataSetCode == parentCode)
				.GetChunk(RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_DataSetId == dataSetId && x.RVC_IsPublished), x => x.ZX1_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
		}

		public RefCusConditionValue[] GetConditionValueChunk(string parentCode, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, short dataSetId, Guid? startPK, Guid? endPK)
		{
			return RefDbRepo.Get<RefCusConditionValue>().Where(x => x.ZX3_DataSetCode == parentCode)
				.GetChunk(RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_DataSetId == dataSetId && x.RVC_IsPublished), x => x.ZX3_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
		}

		public RefCusConditionLanguage[] GetConditionLanguageChunk(string parentCode, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, short dataSetId, Guid? startPK, Guid? endPK)
		{
			return RefDbRepo.Get<RefCusConditionLanguage>().Where(x => x.ZXJ_DataSetCode == parentCode)
				.GetChunk(RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_DataSetId == dataSetId && x.RVC_IsPublished), x => x.ZXJ_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
		}

		public Models.RefCusCondition[] GetSetData(RefCusCondition[] conditionChunk, RefCusConditionValue[] conditionValueChunk, RefCusConditionLanguage[] conditionLanguageChunk,
			IEnumerable<IGrouping<Guid?, Models.RefCusApplicability>> applicabilityChunk, Dictionary<Guid, Models.RefCusPreference> preferenceChunk
			, Guid dataSetPK, ref int conditionValueIdx, ref int conditionLanguageIdx, ref int conditionIdx)
		{
			Argument.NotNull(conditionChunk, nameof(conditionChunk));
			Argument.NotNull(conditionValueChunk, nameof(conditionValueChunk));
			Argument.NotNull(conditionLanguageChunk, nameof(conditionLanguageChunk));
			Argument.NotNull(applicabilityChunk, nameof(applicabilityChunk));
			Argument.GreaterThanOrEqual(conditionValueIdx, 0, nameof(conditionValueIdx));
			Argument.GreaterThanOrEqual(conditionIdx, 0, nameof(conditionIdx));
			Argument.GreaterThanOrEqual(conditionLanguageIdx, 0, nameof(conditionLanguageIdx));

			var conditionValues = conditionValueChunk.GetSetData(x => x.ZX3_DataSetPK, dataSetPK, ref conditionValueIdx);
			var conditionLanguages = conditionLanguageChunk.GetSetData(x => x.ZXJ_DataSetPK, dataSetPK, ref conditionLanguageIdx);

			return conditionChunk.GetSetData(x => x.ZX1_DataSetPK, dataSetPK, ref conditionIdx).Where(x => ConditionTypes.ContainsKey(x.ZX1_ZX2_ConditionType)).Select(x =>
			{
				var cm = mapper.Map<Models.RefCusCondition>(x);
				cm.RefCusConditionType = ConditionTypes[x.ZX1_ZX2_ConditionType];
				cm.RefCusApplicabilities = applicabilityChunk.Where(a => a.Key == x.ZX1_PK).SelectMany(a => a).ToArray();
				cm.RefCusConditionValues = conditionValues.Where(v => ConditionValueTypes.ContainsKey(v.ZX3_ZX4_ValueType) && v.ZX3_ZX1_Condition == x.ZX1_PK).Select(v =>
				{
					var vm = mapper.Map<Models.RefCusConditionValue>(v);
					vm.RefCusConditionValueType = ConditionValueTypes[v.ZX3_ZX4_ValueType];
					return vm;
				}).ToArray();
				cm.RefCusConditionLanguages = conditionLanguages.Where(l => l.ZXJ_ZX1_Condition == x.ZX1_PK).Select(l =>
				{
					var vm = mapper.Map<Models.RefCusConditionLanguage>(l);
					return vm;
				}).ToArray();
				if (preferenceChunk != null && (!x.ZX1_ZZS_Preference.HasValue || preferenceChunk.ContainsKey(x.ZX1_ZZS_Preference.Value)))
				{
					cm.RefCusPreference = x.ZX1_ZZS_Preference.HasValue ? preferenceChunk[x.ZX1_ZZS_Preference.Value] : null;
				}
				return cm;
			}).ToArray();
		}
	}
}
