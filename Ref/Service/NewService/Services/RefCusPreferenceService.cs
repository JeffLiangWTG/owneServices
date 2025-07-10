using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefCusPreferenceService : ReferenceDataServiceBase<Models.RefCusPreference>
	{
		protected override string TableCode => "ZZS";

		static RefCusPreferenceService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusPreference, Models.RefCusPreference>();
				cfg.CreateMap<RefCusPreferenceLanguage, Models.RefCusPreferenceLanguage>();
			});
		}

		readonly RefCusConditionService conditionService;
		readonly RefCusApplicabilityService applicabilityService;

		public RefCusPreferenceService(IReadOnlyReferenceDataRepository refDbRepo, RefCusConditionService conditionService, RefCusApplicabilityService applicabilityService) : base(refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));

			this.conditionService = conditionService;
			this.applicabilityService = applicabilityService;
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

		public override IEnumerable<Models.RefCusPreference> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}

		IEnumerable<Models.RefCusPreference> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short dataSetId)
		{
			var mapper = Config.CreateMapper();

			var conditionsChunk = conditionService.GetConditionChunk("ZZS", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var conditionValuesChunk = conditionService.GetConditionValueChunk("ZZS", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var conditionLanguagesChunk = conditionService.GetConditionLanguageChunk("ZZS", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var applicabilitiesChunk = applicabilityService.GetCusApplicabilityChunk("ZZS", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var excludedTradesChunk = applicabilityService.GetExcludedTradeGroupChunk("ZZS", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, null, null);
			var versionsChunk = GetVersionControls(dataSetId).Filter(lowerTimestamp, upperTimestamp, checkpointPK).OrderBy(x => x.RVC_ParentPK).ToArray();
			var languageChunk = RefDbRepo.Get<RefCusPreferenceLanguage>().GetChunk(GetVersionControls(dataSetId), x => x.ZX9_ZZS_Preference, lowerTimestamp, upperTimestamp, checkpointPK);

			var preferences = RefDbRepo.Get<RefCusPreference>().GetChunk(GetVersionControls(dataSetId), x => x.ZZS_PK, lowerTimestamp, upperTimestamp, checkpointPK);

			var preferencesIdx = 0;
			var conditionIdx = 0;
			var conditionValueIdx = 0;
			var conditionLanguageIdx = 0;
			var applicabilityIdx = 0;
			var excludedTradeIdx = 0;
			var versionIdx = 0;
			var languageIdx = 0;

			foreach (var preference in preferences)
			{
				var dataSetPK = preference.ZZS_PK;
				var result = mapper.Map<Models.RefCusPreference>(preference);
				var versions = versionsChunk.GetSetData(x => x.RVC_ParentPK, dataSetPK, ref versionIdx, RefDbRepo.IsDbProvider);
				result.Deleted = versions.FirstOrDefault().RVC_Deleted;
				if (!result.Deleted)
				{
					var applicabilities = applicabilityService.GetSetData(applicabilitiesChunk, excludedTradesChunk, dataSetPK, ref applicabilityIdx, ref excludedTradeIdx);
					result.RefCusPreferenceLanguages = languageChunk.GetSetData(x => x.ZX9_ZZS_Preference, dataSetPK, ref languageIdx)
						.Select(x => mapper.Map<Models.RefCusPreferenceLanguage>(x)).ToArray();
					result.RefCusConditions = conditionService.GetSetData(conditionsChunk, conditionValuesChunk, conditionLanguagesChunk, applicabilities, null, dataSetPK, ref conditionValueIdx, ref conditionLanguageIdx, ref conditionIdx);
				}
				result.WriteCheckpoint(dataSetPK, ref preferencesIdx, chunkSize);
				yield return result;
			}
		}
	}
}
