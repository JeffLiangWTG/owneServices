using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefCusNomenclatureGroupService : ReferenceDataServiceBase<Models.RefCusNomenclatureGroup>
	{
		protected override string TableCode => "ZZ5";

		static RefCusNomenclatureGroupService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusNomenclatureGroup, Models.RefCusNomenclatureGroup>();
				cfg.CreateMap<RefCusNomenclatureGroupNote, Models.RefCusNomenclatureGroupNote>();
				cfg.CreateMap<RefCusNomenclatureLanguage, Models.RefCusNomenclatureLanguage>();
				cfg.CreateMap<RefCusPreference, Models.RefCusPreference>();
			});
		}

		public RefCusNomenclatureGroupService(IReadOnlyReferenceDataRepository refDbRepo, RefCusConditionService conditionService, RefCusApplicabilityService applicabilityService, RefCusTariffBRCharacteristicService refCusTariffBRCharacteristicService) : base(refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));

			this.conditionService = conditionService;
			this.applicabilityService = applicabilityService;
			this.refCusTariffBRCharacteristicService = refCusTariffBRCharacteristicService;
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

		readonly RefCusConditionService conditionService;
		readonly RefCusApplicabilityService applicabilityService;
		readonly RefCusTariffBRCharacteristicService refCusTariffBRCharacteristicService;

		IEnumerable<Models.RefCusNomenclatureGroup> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short dataSetId)
		{
			var mapper = Config.CreateMapper();

			var preferences = RefDbRepo.Get<RefCusPreference>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZZS_PK, x => mapper.Map<Models.RefCusPreference>(x));
			var notesChunk = RefDbRepo.Get<RefCusNomenclatureGroupNote>().GetChunk(GetVersionControls(dataSetId), x => x.ZZL_ZZ5_NomenclatureGroup,
				lowerTimestamp, upperTimestamp, checkpointPK);
			var languageChunk = RefDbRepo.Get<RefCusNomenclatureLanguage>().GetChunk(GetVersionControls(dataSetId), x => x.ZX8_ZZ5_NomenclatureGroup,
				lowerTimestamp, upperTimestamp, checkpointPK);
			var conditionsChunk = conditionService.GetConditionChunk("ZZ5", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var conditionValuesChunk = conditionService.GetConditionValueChunk("ZZ5", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var conditionLanguagesChunk = conditionService.GetConditionLanguageChunk("ZZ5", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var bRCharacteristicChunk = refCusTariffBRCharacteristicService.GetRefCusTariffBRCharacteristicChunk("ZZ5", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var bRCharacteristicAttributeChunk = refCusTariffBRCharacteristicService.GetRefCusTariffBRCharacteristicAttributeChunk("ZZ5", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var bRCharacteristicValueChunk = refCusTariffBRCharacteristicService.GetRefCusTariffBRCharacteristicValueChunk("ZZ5", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var applicabilitiesChunk = applicabilityService.GetCusApplicabilityChunk("ZZ5", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var excludedTradesChunk = applicabilityService.GetExcludedTradeGroupChunk("ZZ5", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var versionsChunk = GetVersionControls(dataSetId).Filter(lowerTimestamp, upperTimestamp, checkpointPK).OrderBy(x => x.RVC_ParentPK).ToArray();
			// always retrieve nomenclatures the last
			var nomenclatures = RefDbRepo.Get<RefCusNomenclatureGroup>().GetChunk(GetVersionControls(dataSetId), x => x.ZZ5_PK, lowerTimestamp, upperTimestamp, checkpointPK);

			var nomenclatureIdx = 0;
			var noteIdx = 0;
			var conditionIdx = 0;
			var conditionValueIdx = 0;
			var conditionLanguageIdx = 0;
			var applicabilityIdx = 0;
			var excludedTradeIdx = 0;
			var versionIdx = 0;
			var languageIdx = 0;
			var characteristicIdx = 0;
			var characteristicAttributeIdx = 0;
			var characteristicValueIdx = 0;

			while (nomenclatureIdx < nomenclatures.Length)
			{
				var nomenclature = nomenclatures[nomenclatureIdx];
				var dataSetPK = nomenclature.ZZ5_PK;
				var result = mapper.Map<Models.RefCusNomenclatureGroup>(nomenclature);
				var versions = versionsChunk.GetSetData(x => x.RVC_ParentPK, dataSetPK, ref versionIdx, RefDbRepo.IsDbProvider);
				result.Deleted = versions.FirstOrDefault().RVC_Deleted;
				if (!result.Deleted)
				{
					result.RefCusNomenclatureGroupNotes = notesChunk.GetSetData(x => x.ZZL_ZZ5_NomenclatureGroup, dataSetPK, ref noteIdx)
						.Select(x => mapper.Map<Models.RefCusNomenclatureGroupNote>(x)).ToArray();
					result.RefCusNomenclatureLanguages = languageChunk.GetSetData(x => x.ZX8_ZZ5_NomenclatureGroup, dataSetPK, ref languageIdx)
						.Select(x => mapper.Map<Models.RefCusNomenclatureLanguage>(x)).ToArray();
					var applicabilities = applicabilityService.GetSetData(applicabilitiesChunk, excludedTradesChunk, dataSetPK, ref applicabilityIdx, ref excludedTradeIdx);
					result.RefCusConditions = conditionService.GetSetData(conditionsChunk, conditionValuesChunk, conditionLanguagesChunk, applicabilities, preferences, dataSetPK, ref conditionValueIdx, ref conditionLanguageIdx, ref conditionIdx);
					result.RefCusTariffBRCharacteristics = refCusTariffBRCharacteristicService.GetSetData(bRCharacteristicChunk, bRCharacteristicValueChunk, bRCharacteristicAttributeChunk, dataSetPK, ref characteristicValueIdx, ref characteristicAttributeIdx, ref characteristicIdx);
				}
				nomenclatureIdx++;
				if (nomenclatureIdx % chunkSize == 0)
				{
					result.Checkpoint = CheckpointHelper.Create(dataSetPK).ToString();
				}
				yield return result;
			}
		}

		public override IEnumerable<Models.RefCusNomenclatureGroup> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}
	}
}
