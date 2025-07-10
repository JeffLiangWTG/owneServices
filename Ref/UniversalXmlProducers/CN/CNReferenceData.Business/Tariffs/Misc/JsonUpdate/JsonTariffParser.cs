using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.CNReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class JsonTariffParser
	{
		public JsonTariffParser(HSData[] input)
		{
			HSDatas = input;
			AdditionalElementHelper = new AdditionalElementHelper();
			Logger = GlobalOption.Instance.Log;
		}
		readonly ILog Logger;

		HSData[] HSDatas { get; }
		public AdditionalElementHelper AdditionalElementHelper { get; }

		public IEnumerable<RefCusTariff> GetTariffList()
		{
			var result = new List<RefCusTariff>();

			FixHS_CodeByDIGIT_MARK();

			foreach (var groupedTariffs in HSDatas.Where(x => x != null && x.DIGIT_MARK == 10).GroupBy(x => x.HS_CODE))
			{
				var tariffData = groupedTariffs.GetEffectiveData(Logger);

				if (string.IsNullOrEmpty(tariffData.DESCRIPTION_CN))
				{
					Logger.Error($"Tariff {tariffData.GetDebugInfo()} has no description.");
				}
				else
				{
					Logger.Info($"Populating XML for tariff {tariffData.GetDebugInfo()} ...");
					result.AddRange(CreateRefCusTariffs(tariffData));
				}
			}
			return result;
		}

		void FixHS_CodeByDIGIT_MARK()
		{
			foreach (var tariffData in HSDatas)
			{
				if (tariffData.DIGIT_MARK > 0)
				{
					if (tariffData.HS_CODE.Length != tariffData.DIGIT_MARK && tariffData.HS_BOOK.Length == tariffData.DIGIT_MARK)
					{
						Logger.Debug($"Tariff {tariffData.HS_BOOK}. Use HS_BOOK {tariffData.HS_BOOK} instead of HS_CODE {tariffData.HS_CODE}");

						tariffData.HS_CODE = tariffData.HS_BOOK;
					}

					if (tariffData.HS_CODE.Length != tariffData.DIGIT_MARK)
					{
						Logger.Error($"Tariff {tariffData.HS_CODE} seems have wrong DIGIT_MARK {tariffData.DIGIT_MARK}.");
					}
				}
			}
		}

		IEnumerable<RefCusTariff> CreateRefCusTariffs(HSData tariffData)
		{
			var tariffElmt = CreateTariff(tariffData);

			if (!tariffData.IsDeleted())
			{
				AddRates(tariffElmt, tariffData);

				AddConditions(tariffElmt, tariffData);
				AddRequirments(tariffElmt, tariffData);

				if (tariffData.IsAdded())
				{
					var tariffAttrbutesFromCSV = new List<RefCusTariffAttribute>();
					foreach (var attribute in TariffAttributeRepository.Instance.GetByTariffCode(tariffData.HS_CODE))
					{
						var refCusTariffAttribute = new RefCusTariffAttribute
						{
							ZZ3_Name = attribute.AttributeName,
							ZZ3_Value = attribute.AttributeValue
						};
						tariffAttrbutesFromCSV.Add(refCusTariffAttribute);
					}
					AppendTariffAttribute(tariffElmt, tariffAttrbutesFromCSV);
				}

				var ysData = tariffData.YS_LIST?.GetModifiedDataList().GetEffectiveData(Logger);
				if (ysData != null)
				{
					AddAdditionalInfoElment(tariffElmt, 1, "品名");

					foreach (var (index, description) in AdditionalElementHelper.Parse(tariffData.HS_CODE, ysData?.YS_CN, Logger))
					{
						AddAdditionalInfoElment(tariffElmt, index, description);
					}
				}

				AddRefCusTariffUOM(tariffElmt, tariffData);
			}

			yield return tariffElmt;

			if (tariffData.CIQ_LIST != null)
			{
				foreach (var groupedCiqData in tariffData.CIQ_LIST.Where(x => x != null).GroupBy(x => x.CIQ_CODE))
				{
					var effectiveCiqData = groupedCiqData.GetModifiedDataList().GetEffectiveData(Logger);
					if (effectiveCiqData != null)
					{
						yield return CreateCIQTariffElmt(tariffData, effectiveCiqData);
					}
				}
			}
		}

		static RefCusTariff CreateCIQTariffElmt(HSData tariffData, CIQData ciqData)
		{
			var ciqTariffElmt = CreateTariffElmt(tariffData, ciqData);
			var relationship = new RefCusTariffRelationship
			{
				ZZH_TariffCode = tariffData.HS_CODE
			};
			ciqTariffElmt.RefCusTariffRelationships = new[] { relationship };
			return ciqTariffElmt;
		}

		void AddRatesWithProvisionalRate(RefCusTariff tariffElmt, string preference, string tradeGroupCode, IEnumerable<IRateData> rateList, IEnumerable<IRateData> provisionalRateList)
		{
			if (rateList?.GetModifiedDataList().Count() > 0 || provisionalRateList?.GetModifiedDataList().Count() > 0)
			{
				var effectiveData = ResultDataHelper.GetEffectiveData(provisionalRateList?.GetEffectiveData(Logger), rateList?.GetEffectiveData(Logger));
				AddRateElement(tariffElmt, preference, tradeGroupCode, effectiveData);
			}
		}

		void AddRates(RefCusTariff tariffElmt, HSData tariffData)
		{
			AddRatesWithProvisionalRate(tariffElmt, "MFN", "MFN", tariffData.MFN_LIST, tariffData.PR_LIST);

			var genData = tariffData.GEN_LIST?.GetModifiedDataList().GetEffectiveData(Logger);
			if (genData != null)
			{
				AddRateElement(tariffElmt, "NORMAL", "STANDARD", genData);
			}

			if (tariffData.PT_LIST?.Count > 0)
			{
				var ratesForTradeGroups = new Dictionary<string, List<RateWithDateRange>>();
				var modifiedPTDataList = tariffData.PT_LIST.GetModifiedDataList().ToArray();

				foreach (var tradeGroup in TradeGroupHelper.Instance.Keys)
				{
					ratesForTradeGroups.Add(tradeGroup, CombineRates(modifiedPTDataList, data => data.GetRateByTradeGroup(tradeGroup)));
				}

				foreach (var tradeGroup in ratesForTradeGroups.Keys)
				{
					var rateList = ratesForTradeGroups[tradeGroup];

					if (modifiedPTDataList.Length == 1 || modifiedPTDataList.Select(x => x.VPT_OPT).Distinct().Count() == 1 || rateList.Select(x => x.Rate).Distinct().Count() > 1)
					{
						var rateWithDateRange = rateList.LastOrDefault(r => r.Rate.Length > 0);
						if (rateWithDateRange != null)
						{
							var ldcRates = ratesForTradeGroups.Where(x => x.Key.StartsWith("LDC", StringComparison.Ordinal)
								&& x.Value.Any(r => r.Rate.Length > 0 && r.StartDate <= rateWithDateRange.EndDate && r.EndDate > rateWithDateRange.StartDate)).Select(x => x.Key).ToList();

							AddRateElement(tariffElmt, tradeGroup.StartsWith("LDC", StringComparison.Ordinal) ? "LDC" : "FTA", tradeGroup, rateWithDateRange.Rate, rateWithDateRange.StartDate, rateWithDateRange.EndDate.GetEndDate(), ldcRates);
						}
					}
				}
			}

			AddRatesWithProvisionalRate(tariffElmt, "EXC", "STANDARD", tariffData.CAD_LIST, tariffData.ISD_LIST);
			AddRatesWithProvisionalRate(tariffElmt, "EXP", "STANDARD", tariffData.EXPT_LIST, tariffData.EXPP_LIST);

			if (tariffData.AU_LIST?.Count > 0)
			{
				var auData = tariffData.AU_LIST?.GetModifiedDataList().GetEffectiveData(Logger);
				if (auData != null)
				{
					AddRateElement(tariffElmt, "ADL", "US", auData);
				}
			}
		}

		static List<RateWithDateRange> CombineRates<T>(IEnumerable<T> modifiedDataList, Func<T, string> getRateFunc) where T : IJsonData
		{
			var rateList = new List<RateWithDateRange>();

			foreach (var data in modifiedDataList)
			{
				var rate = getRateFunc(data) ?? string.Empty;
				var endDate = data.ValidTo == DateTime.MinValue ? DateTime.MaxValue : data.ValidTo;

				var sameRate = rateList.FirstOrDefault(x => x.Rate == rate && x.EndDate == data.ValidFrom);
				if (sameRate != null)
				{
					sameRate.EndDate = endDate;
				}
				else
				{
					rateList.Add(new RateWithDateRange() { StartDate = data.ValidFrom, EndDate = endDate, Rate = rate });
				}
			}

			return rateList;
		}

		class RateWithDateRange
		{
			public string Rate { get; set; }
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
		}

		RefCusTariff CreateTariff(HSData tariffData)
		{
			RefCusTariffLanguage[] refCusTariffLanguages = null;
			var englishDescription = tariffData.DESCRIPTION_EN;
			if (!string.IsNullOrEmpty(englishDescription))
			{
				refCusTariffLanguages = new[]
				{
					new RefCusTariffLanguage
					{
						ZX7_ZX6_NKLanguage = "EN",
						ZX7_Description = englishDescription
					}
				};
			}

			var tariffElmt = new RefCusTariff
			{
				ZZ1_TariffCode = tariffData.HS_CODE,
				ZZ1_Description = tariffData.DESCRIPTION_CN,
				ZZ1_StartDate = tariffData.GetStartDate(),
				ZZ1_EndDate = tariffData.GetEndDate().OrMaxSmallDateTime(),
				RefCusTariffLanguages = refCusTariffLanguages
			};

			var vat = tariffData.VAT_LIST?.GetEffectiveData(Logger)?.VAT;
			if (string.IsNullOrEmpty(vat))
			{
				var parentTariff = HSDatas.FirstOrDefault(x => x.DIGIT_MARK == 8 && x.HS_CODE == tariffData.HS_CODE.Substring(0, 8));
				if (parentTariff != null)
				{
					vat = parentTariff.VAT_LIST?.GetEffectiveData(Logger, parentTariff.IsDeleted())?.VAT;
					Logger.Info($"Tariff {tariffData.HS_CODE} has no VAT Data, use VAT Data of {parentTariff.HS_CODE}");
				}
				if (string.IsNullOrEmpty(vat))
				{
					Logger.Error($"Tariff {tariffData.HS_CODE} has no effective VAT Data");
				}
			}

			if (!string.IsNullOrEmpty(vat))
			{
				var vatCode = TaxOrFeeHelper.Instance.GetCode(int.Parse(vat, CultureInfo.InvariantCulture));
				if (vatCode == null)
				{
					Logger.Error($"Tariff {tariffData.HS_CODE} has invalid VAT Data {vat}");
				}
				else
				{
					tariffElmt.ZZ1_ZZF_NKTaxOrFeeCode = vatCode;
				}
			}
			tariffElmt.ZZ1_ZZI_NKTariffType = "HSN";
			return tariffElmt;
		}

		static RefCusTariff CreateTariffElmt(HSData tariffData, CIQData data)
		{
			RefCusTariffLanguage[] refCusTariffLanguages = null;

			var englishDescription = data.CIQ_EXTEND_EN.FallbackTo(data.CIQ_DESCRIPTION_EN);
			if (!string.IsNullOrEmpty(englishDescription))
			{
				refCusTariffLanguages = new[]
				{
					new RefCusTariffLanguage
					{
						ZX7_ZX6_NKLanguage = "EN",
						ZX7_Description = englishDescription
					}
				};
			}

			var tariffElmt = new RefCusTariff
			{
				ZZ1_TariffCode = data.CIQ_CODE,
				ZZ1_Description = data.CIQ_EXTEND_CN.FallbackTo(data.CIQ_DESCRIPTION_CN),
				ZZ1_StartDate = data.GetStartDate(),
				ZZ1_EndDate = data.GetEndDate().MinOrMaxSmallDateTime(tariffData.GetEndDate()),
				ZZ1_ZZF_NKTaxOrFeeCode = "",
				ZZ1_ZZI_NKTariffType = "CIQ",
				RefCusTariffLanguages = refCusTariffLanguages
			};
			return tariffElmt;
		}

		void AddAdditionalInfoElment(RefCusTariff tariffElmt, int index, string description)
		{
			var attributeEmlt = new RefCusTariffAttribute
			{
				ZZ3_Name = "AdditionalInfo" + index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
				ZZ3_Value = AdditionalElementHelper.GetAdditionElementCode(description)
			};
			AppendTariffAttribute(tariffElmt, new[] { attributeEmlt });
		}

		static void AppendTariffAttribute(RefCusTariff tariffElmt, IEnumerable<RefCusTariffAttribute> attributes)
		{
			if (tariffElmt.RefCusTariffAttributes == null)
			{
				tariffElmt.RefCusTariffAttributes = attributes.ToArray();
			}
			else
			{
				tariffElmt.RefCusTariffAttributes = tariffElmt.RefCusTariffAttributes.Union(attributes).ToArray();
			}
		}

		void AddRefCusTariffUOM(RefCusTariff tariffElmt, HSData tariffData)
		{
			if (tariffData.IsAdded())
			{
				var uoms = new List<RefCusTariffUOM>();

				var cu1Emlt = new RefCusTariffUOM
				{
					ZZ8_Type = "CU1"
				};
				if (string.IsNullOrEmpty(tariffData.UNIT1_CN))
				{
					Logger.Error($"Tariff {tariffData.HS_CODE} has no UNIT1 Data");
				}
				else
				{
					var cu1Code = UnitOfMeasurementHelper.Instance.GetValueOrDefault(tariffData.UNIT1_CN);
					if (cu1Code == null)
					{
						var index = tariffData.UNIT1_CN.IndexOf('/');
						if (index > -1)
						{
							cu1Code = UnitOfMeasurementHelper.Instance.GetValueOrDefault(tariffData.UNIT1_CN.Remove(index));
						}
					}
					if (cu1Code == null)
					{
						Logger.Error($"Tariff {tariffData.HS_CODE} has invalid UNIT1 Data {tariffData.UNIT1_CN}");
					}
					else
					{
						cu1Emlt.ZZ8_UOM = cu1Code;
					}
					uoms.Add(cu1Emlt);
				}

				if (!string.IsNullOrEmpty(tariffData.UNIT2_CN))
				{
					var cu2Emlt = new RefCusTariffUOM
					{
						ZZ8_Type = "CU2"
					};

					var cu2Code = UnitOfMeasurementHelper.Instance.GetValueOrDefault(tariffData.UNIT2_CN);
					if (cu2Code == null)
					{
						Logger.Error($"Tariff {tariffData.HS_CODE} has invalid UNIT2 Data {tariffData.UNIT2_CN}");
					}
					else
					{
						cu2Emlt.ZZ8_UOM = cu2Code;
					}
					uoms.Add(cu2Emlt);
				}

				tariffElmt.RefCusTariffUOMs = uoms.ToArray();
			}
		}

		void AddRequirments(RefCusTariff tariffElmt, HSData tariffData)
		{
			var requirements = new List<RefCusTariffAttribute>();

			var scData = tariffData.SC_LIST?.GetModifiedDataList().GetEffectiveData(Logger);
			if (scData != null)
			{
				var cusReqs = CustomsCondition.Extract(scData.SC_CODE).ToArray();

				foreach (var reqs in cusReqs.GroupBy(x => x.RequirementName).Where(x => !string.IsNullOrEmpty(x.Key)))
				{
					var attributeEmlt = new RefCusTariffAttribute
					{
						ZZ3_Name = reqs.Key,
						ZZ3_Value = new string(reqs.Select(x => x.CustomsCode).ToArray())
					};
					requirements.Add(attributeEmlt);
				}
			}

			var sjData = tariffData.SJ_LIST?.GetModifiedDataList().GetEffectiveData(Logger);
			if (sjData != null)
			{
				var ciqReqs = CIQRequirement.Extract(sjData.SJ_CODE).ToArray();
				foreach (var reqs in ciqReqs.GroupBy(x => x.RequirementName).Where(x => !string.IsNullOrEmpty(x.Key)))
				{
					var attributeEmlt = new RefCusTariffAttribute
					{
						ZZ3_Name = reqs.Key,
						ZZ3_Value = new string(reqs.Select(x => x.Code).ToArray())
					};
					requirements.Add(attributeEmlt);
				}
			}

			AppendTariffAttribute(tariffElmt, requirements);
		}

		static void AddCondition(RefCusTariff tariffElmt, CustomsCondition condition, IJsonData data)
		{
			var conditionElmt = new RefCusCondition
			{
				ZX1_Comment = $"{condition.CustomsCode}.{condition.Description}",
				ZX1_ConditionValueTrueMeansStop = condition.IsProhibit,
				ZX1_IsExport = condition.IsExport,
				ZX1_IsImport = condition.IsImport,
				ZX1_StartDate = data.GetStartDate(),
				ZX1_EndDate = data.GetEndDate().MinOrMaxSmallDateTime(tariffElmt.ZZ1_EndDate),
				ZX1_ZX2_NKConditionType = condition.IsProhibit ? (condition.IsImport ? "IMPPH" : "EXPPH") : "CNDOC"
			};

			AppendConditions(tariffElmt, new[] { conditionElmt });

			if (!condition.IsProhibit)
			{
				conditionElmt.RefCusConditionValues = new[] { CreateConditionValueElmt(condition) };
			}
		}

		static void AppendConditions(RefCusTariff tariffElmt, IEnumerable<RefCusCondition> conditions)
		{
			if (tariffElmt.RefCusConditions == null)
			{
				tariffElmt.RefCusConditions = conditions.ToArray();
			}
			else
			{
				tariffElmt.RefCusConditions = tariffElmt.RefCusConditions.Union(conditions).ToArray();
			}
		}

		void AddConditions(RefCusTariff tariffElmt, HSData tariffData)
		{
			var scData = tariffData.SC_LIST?.GetModifiedDataList().GetEffectiveData(Logger, false);
			if (scData != null)
			{
				var validCodes = getValidConditionCodes(scData);

				var validImportLicense = new string(validCodes.Where(c => CustomsCondition.IsImportPermit(c)).OrderBy(c => c).ToArray());
				var validExportLicense = new string(validCodes.Where(c => CustomsCondition.IsExportPermit(c)).OrderBy(c => c).ToArray());
				var validNonLicenseCodes = validCodes.Where(c => !CustomsCondition.IsImportPermit(c) && !CustomsCondition.IsExportPermit(c));

				var deletedSCData = tariffData.SC_LIST?.Where(x => x != null).FirstOrDefault(x => x.IsDeleted());
				if (deletedSCData != null && deletedSCData != scData)
				{
					var deletedCodes = getValidConditionCodes(deletedSCData);

					var deletedImportLicense = new string(deletedCodes.Where(c => CustomsCondition.IsImportPermit(c)).OrderBy(c => c).ToArray());
					var deletedExportLicense = new string(deletedCodes.Where(c => CustomsCondition.IsExportPermit(c)).OrderBy(c => c).ToArray());
					var deletedNonLicenseCodes = deletedCodes.Where(c => !CustomsCondition.IsImportPermit(c) && !CustomsCondition.IsExportPermit(c));

					AddConditions(tariffElmt, deletedSCData, deletedNonLicenseCodes.Where(c => !validNonLicenseCodes.Contains(c)));

					if (validImportLicense != deletedImportLicense)
					{
						AddImportLicense(tariffElmt, deletedSCData, deletedImportLicense);
					}
					if (validExportLicense != deletedExportLicense)
					{
						AddExportLicense(tariffElmt, deletedSCData, deletedExportLicense);
					}
				}

				AddConditions(tariffElmt, scData, validCodes);
				AddImportLicense(tariffElmt, scData, validImportLicense);
				AddExportLicense(tariffElmt, scData, validExportLicense);
			}
		}

		static RefCusConditionValue CreateConditionValueElmt(CustomsCondition condition)
		{
			var conditionValueElmt = new RefCusConditionValue
			{
				ZX3_Value = condition.Code,
				ZX3_ZX4_NKValueType = "DOC"
			};
			return conditionValueElmt;
		}

		static IEnumerable<char> getValidConditionCodes(SCData scData) => scData?.SC_CODE.Where(c => c != '/' && c != '.' && c != 'A');

		static readonly DateTime newLicenseCommentEffectiveDate = new DateTime(2022, 3, 17);

		static void AddConditions(RefCusTariff tariffElmt, SCData scData, IEnumerable<char> codes)
		{
			var conditions = codes.Select(code => CustomsConditionList.Get(code)).Where(con => con != null);
			var refConditions = new List<RefCusCondition>();

			foreach (var condition in conditions.Where(c => !c.IsLicense))
			{
				AddCondition(tariffElmt, condition, scData);
			}

			AppendConditions(tariffElmt, refConditions);
		}

		static void AddImportLicense(RefCusTariff tariffElmt, SCData scData, string codes)
		{
			AddLicense(tariffElmt, scData, codes, true);
		}

		static void AddExportLicense(RefCusTariff tariffElmt, SCData scData, string codes)
		{
			AddLicense(tariffElmt, scData, codes, false);
		}

		static void AddLicense(RefCusTariff tariffElmt, SCData scData, string codes, bool isImport)
		{
			if (!string.IsNullOrEmpty(codes))
			{
				var startDate = scData.GetStartDate();
				var conditionElmt = new RefCusCondition();
				var directionText = isImport ? "进口" : "出口";
				conditionElmt.ZX1_Comment = startDate < newLicenseCommentEffectiveDate ? $"相关{directionText}许可证" : $"相关{directionText}许可证({codes})";
				conditionElmt.ZX1_ConditionValueTrueMeansStop = false;
				conditionElmt.ZX1_IsExport = !isImport;
				conditionElmt.ZX1_IsImport = isImport;
				conditionElmt.ZX1_StartDate = startDate;
				conditionElmt.ZX1_EndDate = scData.GetEndDate().MinOrMaxSmallDateTime(tariffElmt.ZZ1_EndDate).Date;
				conditionElmt.ZX1_ZX2_NKConditionType = "CNDOC";
				conditionElmt.RefCusConditionValues = codes.Select(c => CreateConditionValueElmt(CustomsConditionList.Get(c))).ToArray();

				AppendConditions(tariffElmt, new[] { conditionElmt });
			}
		}

		static bool IsDuty(string preference) => preference != "EXP" && preference != "EXC";

		void AddRateElement(RefCusTariff tariffElmt, string preference, string tradeGroupCode, IRateData data, List<string> ldcRates = null)
		{
			AddRateElement(tariffElmt, preference, tradeGroupCode, data.Rate, data.GetStartDate(), data.GetEndDate(), ldcRates);
		}

		void AddRateElement(RefCusTariff tariffElmt, string preference, string tradeGroupCode, string rate, DateTime startDate, DateTime endDate, List<string> ldcRates = null)
		{
			if (!string.IsNullOrEmpty(rate) && startDate < endDate)
			{
				var rateElmt = new RefCusRate();

				rate = RateFormulaParser.RemoveNewLines(rate);
				rateElmt.ZZ2_RateFormula = RateFormulaParser.GetRateFormula(tariffElmt.ZZ1_TariffCode, preference, rate, Logger);
				rateElmt.ZZ2_RateFormulaDerivedFrom = rate;
				rateElmt.ZZ2_StartDate = startDate;
				rateElmt.ZZ2_EndDate = tariffElmt.ZZ1_EndDate;
				rateElmt.ZZ2_ZY1_NKRateCode = IsDuty(preference) ? (preference == "ADL" ? "ADL" : "DTY") : preference;
				rateElmt.ZZ2_ZY1_ZZR_NKRateType = IsDuty(preference) ? "DTY" : preference;
				var preferenceValue = IsDuty(preference) ? (preference == "ADL" ? "" : preference) : "";
				rateElmt.ZZ2_ZZS_NKPreference = preferenceValue;
				rateElmt.ZZ2_ZZS_ZZZ_NKDataGrouping = string.IsNullOrEmpty(preferenceValue) ? "" : "CN";
				AppendRefCusRate(tariffElmt, new[] { rateElmt });

				var rateAppEmlt = new RefCusApplicability
				{
					ZZT_AdditionalCode = TradeGroupHelper.Instance.GetAdditionalCode(tradeGroupCode),
					ZZT_StartDate = startDate,
					ZZT_EndDate = endDate.OrMaxSmallDateTime(),
					ZZT_ZZA_NKTradeGroup = tradeGroupCode
				};

				if (TradeGroupHelper.Instance.GetExcludeTradeGroups(tradeGroupCode) is string[] excludeTradeGroup)
				{
					foreach (var exclude in excludeTradeGroup)
					{
						AddExcludeElementIfNotSpecific(rateAppEmlt, exclude, ldcRates);
					}
				}

				rateElmt.RefCusApplicabilities = new[] { rateAppEmlt };
			}
		}

		static void AppendRefCusRate(RefCusTariff tariffElmt, IEnumerable<RefCusRate> rates)
		{
			if (tariffElmt.RefCusRates == null)
			{
				tariffElmt.RefCusRates = rates.ToArray();
			}
			else
			{
				tariffElmt.RefCusRates = tariffElmt.RefCusRates.Union(rates).ToArray();
			}
		}

		static void AddExcludeElementIfNotSpecific(RefCusApplicability rateAppEmlt, string tradeGroup, List<string> ldcRates)
		{
			if (ldcRates?.Contains(tradeGroup) ?? false)
			{
				var excludeElmt = new RefCusExcludedTradeGroup
				{
					ZZC_ZZA_NKTradeGroup = tradeGroup
				};
				rateAppEmlt.RefCusExcludedTradeGroups = new[] { excludeElmt };
			}
		}
	}
}
