using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefCusTariffService : ReferenceDataServiceBase<Models.RefCusTariff>
	{
		protected override string TableCode => "ZZ1";

		static RefCusTariffService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusTariffType, Models.RefCusTariffType>();
				cfg.CreateMap<RefCusTariffView, Models.RefCusTariff>();
				cfg.CreateMap<RefCusRate, Models.RefCusRate>();
				cfg.CreateMap<RefCusTariffUOM, Models.RefCusTariffUOM>();
				cfg.CreateMap<RefCusTariffAttribute, Models.RefCusTariffAttribute>();
				cfg.CreateMap<RefCusRateUOM, Models.RefCusRateUOM>();
				cfg.CreateMap<RefCusTariffRelationship, Models.RefCusTariffRelationship>();
				cfg.CreateMap<RefCusRateType, Models.RefCusRateType>();
				cfg.CreateMap<RefCusTariffNationalCode, Models.RefCusTariffNationalCode>();
				cfg.CreateMap<RefCusRateCode, Models.RefCusRateCode>();
				cfg.CreateMap<RefCusPreference, Models.RefCusPreference>();
				cfg.CreateMap<RefCusVATApplicability, Models.RefCusVATApplicability>();
				cfg.CreateMap<RefCusTariffLanguage, Models.RefCusTariffLanguage>();
				cfg.CreateMap<RefCusTradeGroup, Models.RefCusTradeGroup>();
				cfg.CreateMap<RefCusTariffAdditionalCode, Models.RefCusTariffAdditionalCode>();
				cfg.CreateMap<RefCusTariffAdditionalCodeLanguage, Models.RefCusTariffAdditionalCodeLanguage>();
				cfg.CreateMap<RefCusTariffBRCharacteristic, Models.RefCusTariffBRCharacteristic>();
				cfg.CreateMap<RefCusTariffBRCharacteristicAttribute, Models.RefCusTariffBRCharacteristicAttribute>();
				cfg.CreateMap<RefCusTariffBRCharacteristicValue, Models.RefCusTariffBRCharacteristicValue>();
			});
		}

		public RefCusTariffService(IReadOnlyReferenceDataRepository refDbRepo, RefCusApplicabilityService cusApplicabilityService, RefCusConditionService conditionService, RefCusTariffBRCharacteristicService refCusTariffBRCharacteristicService) : base(refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
			this.cusApplicabilityService = cusApplicabilityService;
			this.conditionService = conditionService;
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

		readonly RefCusApplicabilityService cusApplicabilityService;
		readonly RefCusConditionService conditionService;
		readonly RefCusTariffBRCharacteristicService refCusTariffBRCharacteristicService;

#pragma warning disable CA1502
		IEnumerable<Models.RefCusTariff> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short dataSetId)
		{
			var mapper = Config.CreateMapper();
			Guid? startPK = null;
			Guid? endPK = null;
			var tariffTypes = RefDbRepo.Get<RefCusTariffType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZZI_PK, x => mapper.Map<Models.RefCusTariffType>(x));
			var preferences = RefDbRepo.Get<RefCusPreference>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZZS_PK, x => mapper.Map<Models.RefCusPreference>(x));
			var rateTypes = RefDbRepo.Get<RefCusRateType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZZR_PK, x => mapper.Map<Models.RefCusRateType>(x));
			var rateCodes = RefDbRepo.Get<RefCusRateCode>().ToArray().Where(x => rateTypes.ContainsKey(x.ZY1_ZZR_RateType)).ToDictionary(x => x.ZY1_PK, x =>
			{
				var model = mapper.Map<Models.RefCusRateCode>(x);
				model.RefCusRateType = rateTypes[x.ZY1_ZZR_RateType];
				return model;
			});
			var tradeGroups = RefDbRepo.Get<RefCusTradeGroup>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZZA_PK, x => mapper.Map<Models.RefCusTradeGroup>(x));

			while (true)
			{
				var rangeQuery = RefDbRepo.Get<RefCusTariffView>().FilterView(lowerTimestamp, upperTimestamp, checkpointPK, dataSetId).Select(x => x.RVC_ParentPK);
				if (startPK.HasValue)
				{
					rangeQuery = rangeQuery.Where(x => x.CompareTo(startPK.Value) > 0);
				}
				var ranges = rangeQuery.OrderBy(x => x).Take(chunkSize.Value).ToArray();
				if (ranges.Length == 0)
				{
					break;
				}
				endPK = ranges[ranges.Length - 1];

				var rateUOMsChunk = RefDbRepo.Get<RefCusRateUOM>().GetChunk(GetVersionControls(dataSetId), x => x.ZXG_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				var ratesChunk = RefDbRepo.Get<RefCusRate>().GetChunk(GetVersionControls(dataSetId), x => x.ZZ2_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				var tariffAttributesChunk = RefDbRepo.Get<RefCusTariffAttribute>().GetChunk(GetVersionControls(dataSetId), x => x.ZZ3_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				var uomsChunk = RefDbRepo.Get<RefCusTariffUOM>().GetChunk(GetVersionControls(dataSetId), x => x.ZZ8_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				var tariffRelationshipChunk = RefDbRepo.Get<RefCusTariffRelationship>().GetChunk(GetVersionControls(dataSetId), x => x.ZZH_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				var nationalCodesChunk = RefDbRepo.Get<RefCusTariffNationalCode>().GetChunk(GetVersionControls(dataSetId), x => x.ZZW_ZZ1_Tariff, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				var bRCharacteristicChunk = refCusTariffBRCharacteristicService.GetRefCusTariffBRCharacteristicChunk("ZZ1", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, startPK, endPK);
				var bRCharacteristicAttributeChunk = refCusTariffBRCharacteristicService.GetRefCusTariffBRCharacteristicAttributeChunk("ZZ1", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, startPK, endPK);
				var bRCharacteristicValueChunk = refCusTariffBRCharacteristicService.GetRefCusTariffBRCharacteristicValueChunk("ZZ1", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, startPK, endPK);
				var additionalCodeLanguagesChunk = RefDbRepo.Get<RefCusTariffAdditionalCodeLanguage>().GetChunk(GetVersionControls(dataSetId), x => x.ZY4_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				var additionalCodesChunk = RefDbRepo.Get<RefCusTariffAdditionalCode>().GetChunk(GetVersionControls(dataSetId), x => x.ZY2_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				var conditionsChunk = conditionService.GetConditionChunk("ZZ1", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, startPK, endPK);
				var conditionValuesChunk = conditionService.GetConditionValueChunk("ZZ1", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, startPK, endPK);
				var conditionLanguagesChunk = conditionService.GetConditionLanguageChunk("ZZ1", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, startPK, endPK);
				var applicabilitiesChunk = cusApplicabilityService.GetCusApplicabilityChunk("ZZ1", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, startPK, endPK);
				var vatApplicabilitiesChunk = RefDbRepo.Get<RefCusVATApplicability>().GetChunk(GetVersionControls(dataSetId), x => x.ZX5_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				var excludedTradesChunk = cusApplicabilityService.GetExcludedTradeGroupChunk("ZZ1", lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, startPK, endPK);
				var languageChunk = RefDbRepo.Get<RefCusTariffLanguage>().GetChunk(GetVersionControls(dataSetId), x => x.ZX7_ZZ1_Tariff, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
				// always retrieve tariffs the last
				var tariffsChunk = RefDbRepo.Get<RefCusTariffView>().GetChunk(lowerTimestamp, upperTimestamp, checkpointPK, dataSetId, startPK, endPK);

				startPK = ranges[ranges.Length - 1];
				var tariffIdx = 0;
				var rateIdx = 0;
				var rateUomIdx = 0;
				var tariffAttributeIdx = 0;
				var uomIdx = 0;
				var relationshipIdx = 0;
				var nationalCodeIdx = 0;
				var conditionIdx = 0;
				var conditionValueIdx = 0;
				var conditionLanguageIdx = 0;
				var applicabilityIdx = 0;
				var excludedTradeIdx = 0;
				var vatApplicabilityIdx = 0;
				var languageIdx = 0;
				var additionalCodeLanguageIdx = 0;
				var additionalCodeIdx = 0;
				var characteristicIdx = 0;
				var characteristicAttributeIdx = 0;
				var characteristicValueIdx = 0;

				while (tariffIdx < tariffsChunk.Length)
				{
					var tariff = tariffsChunk[tariffIdx];
					var dataSetPK = tariff.RVC_ParentPK;
					var result = mapper.Map<Models.RefCusTariff>(tariff);
					result.Deleted = tariff.RVC_Deleted;
					Models.RefCusTariffType tariffType = null;
					tariffTypes.TryGetValue(tariff.ZZ1_ZZI_TariffType, out tariffType);
					if (tariffType != null)
					{
						result.RefCusTariffType = tariffType;
						if (!result.Deleted)
						{
							var tariffAttributes = tariffAttributesChunk.GetSetData(x => x.ZZ3_DataSetPK, dataSetPK, ref tariffAttributeIdx).GroupBy(x => x.ZZ3_ZZ1_Tariff ?? x.ZZ3_ZZW_TariffNationalCode, x =>
							{
								return mapper.Map<Models.RefCusTariffAttribute>(x);
							});
							result.RefCusTariffAttributes = tariffAttributes.Where(x => x.Key == tariff.RVC_ParentPK).SelectMany(x => x).NotNull().ToArray();
							var uoms = uomsChunk.GetSetData(x => x.ZZ8_DataSetPK, dataSetPK, ref uomIdx).GroupBy(x => x.ZZ8_ZZ1_Tariff ?? x.ZZ8_ZZW_TariffNationalCode, x =>
							{
								Models.RefCusTariffUOM uomModel = null;
								if ((!x.ZZ8_ZZA_TradeGroup.HasValue || tradeGroups.ContainsKey(x.ZZ8_ZZA_TradeGroup.Value))
								&& (!x.ZZ8_ZZA_SecondTradeGroup.HasValue || tradeGroups.ContainsKey(x.ZZ8_ZZA_SecondTradeGroup.Value)))
								{
									uomModel = mapper.Map<Models.RefCusTariffUOM>(x);
									uomModel.RefCusTradeGroup = x.ZZ8_ZZA_TradeGroup.HasValue ? tradeGroups[x.ZZ8_ZZA_TradeGroup.Value] : null;
									uomModel.RefCusTradeGroup1 = x.ZZ8_ZZA_SecondTradeGroup.HasValue ? tradeGroups[x.ZZ8_ZZA_SecondTradeGroup.Value] : null;
								}
								return uomModel;
							});
							result.RefCusTariffUOMs = uoms.Where(x => x.Key == tariff.RVC_ParentPK).SelectMany(x => x).NotNull().ToArray();
							var applicabilities = cusApplicabilityService.GetSetData(applicabilitiesChunk, excludedTradesChunk, dataSetPK, ref applicabilityIdx, ref excludedTradeIdx);
							result.RefCusTariffRelationships = tariffRelationshipChunk.GetSetData(x => x.ZZH_DataSetPK, dataSetPK, ref relationshipIdx).Select(x =>
							{
								var relationshipModel = mapper.Map<Models.RefCusTariffRelationship>(x);
								Models.RefCusTariffType relationshipType = null;
								tariffTypes.TryGetValue(x.ZZH_ZZI_TariffType, out relationshipType);
								relationshipModel.RefCusTariffType = relationshipType;
								relationshipModel.RefCusApplicabilities = applicabilities.Where(a => a.Key == x.ZZH_PK).SelectMany(a => a).ToArray();
								return relationshipModel.RefCusTariffType != null ? relationshipModel : null;
							}).NotNull().ToArray();
							var rateUOMs = rateUOMsChunk.GetSetData(x => x.ZXG_DataSetPK, dataSetPK, ref rateUomIdx);
							var rates = ratesChunk.GetSetData(x => x.ZZ2_DataSetPK, dataSetPK, ref rateIdx).GroupBy(x => x.ZZ2_ZZ1_Tariff ?? x.ZZ2_ZZW_TariffNationalCode, x =>
							{
								Models.RefCusRate rateModel = null;
								if ((!x.ZZ2_ZZS_Preference.HasValue || preferences.ContainsKey(x.ZZ2_ZZS_Preference.Value)) &&
									(!x.ZZ2_ZY1_RateCode.HasValue || rateCodes.ContainsKey(x.ZZ2_ZY1_RateCode.Value)))
								{
									rateModel = mapper.Map<Models.RefCusRate>(x);
									rateModel.RefCusRateCode = x.ZZ2_ZY1_RateCode.HasValue ? rateCodes[x.ZZ2_ZY1_RateCode.Value] : null;
									rateModel.RefCusPreference = x.ZZ2_ZZS_Preference.HasValue ? preferences[x.ZZ2_ZZS_Preference.Value] : null;
									rateModel.RefCusRateUOMs = mapper.MapCollection<Models.RefCusRateUOM, RefCusRateUOM>(rateUOMs.Where(a => a.ZXG_ZZ2_Rate == x.ZZ2_PK)).ToArray();
									rateModel.RefCusApplicabilities = applicabilities.Where(a => a.Key == x.ZZ2_PK).SelectMany(a => a).ToArray();
								}
								return rateModel;
							});
							result.RefCusRates = rates.Where(x => x.Key == tariff.RVC_ParentPK).SelectMany(x => x).NotNull().ToArray();
							var vat = vatApplicabilitiesChunk.GetSetData(x => x.ZX5_DataSetPK, dataSetPK, ref vatApplicabilityIdx).GroupBy(x => x.ZX5_ZZ1_Tariff ?? x.ZX5_ZZW_TariffNationalCode, x =>
							{
								Models.RefCusVATApplicability vatModel = null;
								if (!x.ZX5_ZZA_TradeGroup.HasValue || tradeGroups.ContainsKey(x.ZX5_ZZA_TradeGroup.Value))
								{
									vatModel = mapper.Map<Models.RefCusVATApplicability>(x);
									vatModel.RefCusTradeGroup = x.ZX5_ZZA_TradeGroup.HasValue ? tradeGroups[x.ZX5_ZZA_TradeGroup.Value] : null;
									vatModel.ZX5_DataSetId = dataSetId;
								}
								return vatModel;
							});

							var additionalCodeLanguages = additionalCodeLanguagesChunk.GetSetData(x => x.ZY4_DataSetPK, dataSetPK, ref additionalCodeLanguageIdx);
							var additionalCodes = additionalCodesChunk.GetSetData(x => x.ZY2_DataSetPK, dataSetPK, ref additionalCodeIdx).GroupBy(x => x.ZY2_ZZ1_Tariff ?? x.ZY2_ZZW_NationalCode, x =>
								{
									var addCode = mapper.Map<Models.RefCusTariffAdditionalCode>(x);
									addCode.RefCusTariffAdditionalCodeLanguages = additionalCodeLanguages.Where(l => l.ZY4_ZY2_TariffAdditionalCode == x.ZY2_PK).Select(o => mapper.Map<Models.RefCusTariffAdditionalCodeLanguage>(o)).ToArray();
									addCode.RefCusApplicabilities = applicabilities.Where(a => a.Key == x.ZY2_PK).SelectMany(a => a).ToArray();
									return addCode;
								});

							result.RefCusTariffNationalCodes = nationalCodesChunk.GetSetData(x => x.ZZW_ZZ1_Tariff, dataSetPK, ref nationalCodeIdx).Select(x =>
								{
									var nationalCodeModel = mapper.Map<Models.RefCusTariffNationalCode>(x);
									nationalCodeModel.RefCusRates = rates.Where(r => r.Key == x.ZZW_PK).SelectMany(r => r).NotNull().ToArray();
									nationalCodeModel.RefCusTariffAttributes = tariffAttributes.Where(r => r.Key == x.ZZW_PK).SelectMany(r => r).NotNull().ToArray();
									nationalCodeModel.RefCusVATApplicabilities = vat.Where(v => v.Key == x.ZZW_PK).SelectMany(v => v).NotNull().ToArray();
									nationalCodeModel.RefCusTariffUOMs = uoms.Where(u => u.Key == x.ZZW_PK).SelectMany(v => v).NotNull().ToArray();
									nationalCodeModel.RefCusTariffAdditionalCodes = additionalCodes.Where(u => u.Key == x.ZZW_PK).SelectMany(v => v).NotNull().ToArray();
									return nationalCodeModel;
								}).ToArray();
							result.RefCusVATApplicabilities = vat.Where(v => v.Key == tariff.ZZ1_PK).SelectMany(v => v).ToArray();
							result.RefCusConditions = conditionService.GetSetData(conditionsChunk, conditionValuesChunk, conditionLanguagesChunk, applicabilities, preferences, dataSetPK, ref conditionValueIdx, ref conditionLanguageIdx, ref conditionIdx);
							result.RefCusTariffLanguages = languageChunk.GetSetData(x => x.ZX7_ZZ1_Tariff, dataSetPK, ref languageIdx)
								.Select(x => mapper.Map<Models.RefCusTariffLanguage>(x)).ToArray();
							result.RefCusTariffAdditionalCodes = additionalCodes.Where(u => u.Key == tariff.ZZ1_PK).SelectMany(v => v).NotNull().ToArray();

							result.RefCusTariffBRCharacteristics = refCusTariffBRCharacteristicService.GetSetData(bRCharacteristicChunk, bRCharacteristicValueChunk, bRCharacteristicAttributeChunk, dataSetPK, ref characteristicValueIdx, ref characteristicAttributeIdx, ref characteristicIdx);
						}
						tariffIdx++;
						if (tariffIdx % chunkSize == 0)
						{
							result.Checkpoint = CheckpointHelper.Create(tariff.ZZ1_PK).ToString();
						}

						yield return result;
					}
				}
			}
		}
#pragma warning restore CA1502

		public override IEnumerable<Models.RefCusTariff> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}
	}
}
