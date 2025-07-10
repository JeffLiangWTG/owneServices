using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.ExportTariffs.MasterData;
using CargoWise.RefDbRepo.CHReferenceData.Business.TradeGroups.CountryCodes;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	public class ImportTariffsParser : EdecTariffsParser
	{
		readonly DownloadResult masterDataDownload;
		readonly DownloadResult tariffStructureDownload;
		readonly DownloadResult keyStructureDownload;
		readonly DownloadResult permitInformationDownload;
		readonly DownloadResult nonCustomsLawInformationDownload;
		readonly DownloadResult customsFacilitiesDownload;
		readonly DownloadResult countryCodesDownload;

		public ImportTariffsParser(DownloadResult masterDataDownload, DownloadResult tariffStructureDownload, DownloadResult keyStructureDownload, DownloadResult permitInformationDownload, DownloadResult nonCustomsLawInformationDownload, DownloadResult customsFacilitiesDownload, DownloadResult countryCodesDownload, DateTime actualDate) : base(actualDate)
		{
			this.masterDataDownload = masterDataDownload;
			this.tariffStructureDownload = tariffStructureDownload;
			this.keyStructureDownload = keyStructureDownload;
			this.permitInformationDownload = permitInformationDownload;
			this.nonCustomsLawInformationDownload = nonCustomsLawInformationDownload;
			this.customsFacilitiesDownload = customsFacilitiesDownload;
			this.countryCodesDownload = countryCodesDownload;
		}

		protected override string TariffType => "IMP";

		internal AdditionalCodeList AdditionalCodeList => additionalCodeList ?? (additionalCodeList = new AdditionalCodeList());
		AdditionalCodeList additionalCodeList;

		public override void ConvertToRefXML(string outputFile, Func<string, bool> tariffCodeFilter = null)
		{
			tariffCodeFilter = tariffCodeFilter ?? (x => true);

			var writerConfiguration = GetXmlWriterConfiguration();

			var tariffStructure = new TariffStructure().Load(tariffStructureDownload);
			var keyStructure = new KeyStructureImport(keyStructureDownload, false);
			var keyStructureForVLS = new KeyStructureImport(keyStructureDownload, true);
			var permitInformation = new PermitInformation(permitInformationDownload);
			var nonCustomsLawInformation = new NonCustomsLawInformation(nonCustomsLawInformationDownload);
			var customsFacilities = new CustomsFacilities(customsFacilitiesDownload);

			var inputDoc = Helper.DeserializeXML<tariffMasterData>(masterDataDownload.Content);
			var tariffList = new List<RefCusTariff>();

			foreach (var tariff in from commodityCode in inputDoc.commodityCodes
								   where commodityCode.validForImport && tariffCodeFilter(commodityCode.value)
								   from customsFavourCode in (from r in commodityCode.rate select r.customsFavourCode).Distinct()
								   from statisticalCode in commodityCode.statisticalCode
								   where statisticalCode.validForImport
								   select new { commodityCode, customsFavourCode, statisticalCode })
			{
				var statisticalCode = tariff.statisticalCode.value.ToString("000", CultureInfo.InvariantCulture);
				var tariffCode = tariff.commodityCode.value.Replace(".", "") + tariff.customsFavourCode.ToString("000", CultureInfo.InvariantCulture) + statisticalCode;
				var validFrom = Helper.Max(tariff.commodityCode.validFrom, tariff.statisticalCode.validFrom);
				var validTo = Helper.Min(tariff.commodityCode.validTo, tariff.statisticalCode.validTo);

				var description = GetDescription(tariffStructure, keyStructure, keyStructureForVLS, customsFacilities, tariff.commodityCode.value, tariff.customsFavourCode, tariff.statisticalCode.value);

				var tariffUOMList = new List<RefCusTariffUOM>();
				GetDefaultUOMs(tariffUOMList);
				var uom = GetAdditionalQuantityUOM(tariffUOMList, tariff.statisticalCode);

				var languages = GetLanguages(description);
				if (!languages.Any())
				{
					continue;
				}

				var refCusConditionList = new List<RefCusCondition>();
				GetScaleWeightCode(refCusConditionList, tariff.statisticalCode, uom);
				GetMeanValueLimits(refCusConditionList, tariff.statisticalCode, uom);
				GetPermitConditions(refCusConditionList, tariff.statisticalCode, permitInformation, tariff.commodityCode.value);
				GetNonCustomsLawConditions(refCusConditionList, tariff.statisticalCode, nonCustomsLawInformation, tariff.commodityCode.value);

				var refCusVatApplicabilityList = new List<RefCusVATApplicability>();
				GetVATCodes(refCusVatApplicabilityList, tariff.commodityCode);

				var refCusRatesList = new List<RefCusRate>();
				GetRates(refCusRatesList, tariff.commodityCode, tariff.customsFavourCode);

				var refCusTariffAttributesList = new List<RefCusTariffAttribute>();
				GetTareSupplements(refCusTariffAttributesList, tariff.commodityCode);
				GetCustomsFavourHintCode(refCusTariffAttributesList, tariff.commodityCode);
				GetStorageType(refCusTariffAttributesList, tariff.statisticalCode);
				GetQuantityCode(refCusTariffAttributesList, tariff.statisticalCode, 1);
				GetProcessingCode(refCusTariffAttributesList, tariff.statisticalCode);
				GetPermitOptionalAttribute(refCusTariffAttributesList, tariff.statisticalCode);
				GetNonCustomsLawOptionalAttribute(refCusTariffAttributesList, tariff.statisticalCode);

				tariffList.Add(new RefCusTariff()
				{
					ZZ1_TariffCode = tariffCode,
					ZZ1_StartDate = Helper.Truncate(validFrom),
					ZZ1_EndDate = Helper.EndOfDay(Helper.Truncate(validTo)),
					ZZ1_CompositeKeyOnZZ5 = description.SortKey,
					ZZ1_Description = languages.First().ZX7_Description,
					RefCusTariffLanguages = languages.Skip(1).ToArray(),
					RefCusTariffUOMs = tariffUOMList.ToArray(),
					RefCusConditions = refCusConditionList.ToArray(),
					RefCusVATApplicabilities = refCusVatApplicabilityList.ToArray(),
					RefCusRates = refCusRatesList.ToArray(),
					RefCusTariffAttributes = refCusTariffAttributesList.ToArray(),
				});
			}

			tariffList.Add(GetPlaceholderTariff());

			var published = inputDoc.created.Date;
			Helper.ExportToXMLFile(DataSource, outputFile, writerConfiguration, published, tariffList);

			var additionalCodesOutputFile = outputFile.Insert(outputFile.LastIndexOf('.'), $"-{AdditionalCodeList.CodeType}");
			AdditionalCodeList.WriteXml(additionalCodesOutputFile, published);
		}

		protected override RefCusTariff GetPlaceholderTariff()
		{
			var placeholderTariff = base.GetPlaceholderTariff();
			placeholderTariff.ZZ1_TariffCode = "99999999000000";
			placeholderTariff.RefCusRates = new[]
			{
				new RefCusRate()
				{
					ZZ2_StartDate = new DateTime(2000, 1, 1, 0, 0, 0),
					ZZ2_EndDate = new DateTime(2079, 6, 6, 23, 59, 0),
					ZZ2_RateFormula = "0",
					ZZ2_ZZS_NKPreference = "NT",
					ZZ2_ZZS_ZZZ_NKDataGrouping = "CH",
					RefCusApplicabilities = new []
					{
						new RefCusApplicability() { ZZT_ZZA_NKTradeGroup = AllCountriesGroup.ToString(CultureInfo.InvariantCulture), ZZT_StartDate = new DateTime(2000, 1, 1, 0, 0, 0), ZZT_EndDate = new DateTime(2079, 6, 6, 23, 59, 0) }
					},
				}
			};

			return placeholderTariff;
		}

		protected override string[] WriterConfigurationProperties => new[]
		{
			nameof(RefCusTariffLanguage),
			nameof(RefCusTariffUOM),
			nameof(RefCusCondition),
			nameof(RefCusConditionLanguage),
			nameof(RefCusConditionValue),
			nameof(RefCusVATApplicability),
			nameof(RefCusRate),
			nameof(RefCusApplicability),
			nameof(RefCusTariffAttribute),
			nameof(RefCusExcludedTradeGroup),
			nameof(RefCusTariff.ZZ1_CompositeKeyOnZZ5),
			nameof(RefCusRate.ZZ2_ZZS_NKPreference),
			nameof(RefCusRate.ZZ2_ZZS_ZZZ_NKDataGrouping),
		};

		protected override string RateTypeConstantValue => "DTY";

		protected override string RateCodeConstantValue => "DTY";

		protected static string GetRateFormula(sbyte assessmentCode, string rateValue)
		{
			if (rateValue == "0")
			{
				return "0";
			}
			else
			{
				var tempRate = decimal.Parse(rateValue, CultureInfo.InvariantCulture) / 100;

				switch (assessmentCode)
				{
					case 1:
						return FormattableString.Invariant($"{tempRate} * [KGMG]");
					case 2:
						return $"{rateValue.PadLeftMissingZero()} * [NAR]";
					case 3:
						return $"{rateValue.PadLeftMissingZero()} * [NAR]";
					case 4:
						return $"{rateValue.PadLeftMissingZero()} * [MTR]";
					case 5:
						return $"{rateValue.PadLeftMissingZero()} * [LTR]";
					case 6:
						return $"{rateValue.PadLeftMissingZero()} * [MWH]";
					case 7:
						return FormattableString.Invariant($"{tempRate} * [KGM]");
					default:
						return "0";
				}
			}
		}

		const string DataSource = "CH Tariff IMP";

		static void GetVATCodes(List<RefCusVATApplicability> refCusVatApplicabilityList, tariffMasterDataCommodityCode commodityCode)
		{
			if (commodityCode.VATCode?.Length >= 1)
			{
				foreach (var vatCode in commodityCode.VATCode)
				{
					refCusVatApplicabilityList.Add(new RefCusVATApplicability()
					{
						ZX5_ZZF_NKTaxOrFeeCode = vatCode.value.ToString(CultureInfo.InvariantCulture),
						ZX5_StartDate = Helper.Truncate(vatCode.validFrom),
						ZX5_EndDate = Helper.EndOfDay(Helper.Truncate(vatCode.validTo))
					});
				}
			}
		}

		void GetRates(List<RefCusRate> refCusRatesList, tariffMasterDataCommodityCode commodityCode, sbyte customsFavourCode)
		{
			if (commodityCode.rate.Length > 0)
			{
				tariffMasterDataCommodityCodeRate previousRate = null;
				var tradeGroupsHavingPRRate = commodityCode.rate.Where(x => x.countryGrpNrSpecified && IsRateTypePR(x)).Select(x => x.countryGrpNr).ToArray();
				foreach (var item in splitOverlappingValidityRanges(commodityCode.rate.Where(x => x.customsFavourCode == customsFavourCode)).Select(rate => (rate, refCusRate: CreateRefCusRate(rate, tradeGroupsHavingPRRate))).OrderBy(x => x.refCusRate.ZZ2_ZZS_NKPreference).ThenBy(x => x.refCusRate.RefCusApplicabilities.First().ZZT_ZZA_NKTradeGroup).ThenBy(x => x.refCusRate.ZZ2_StartDate))
				{
					if (refCusRatesList.Any() && item.refCusRate.ZZ2_ZZS_NKPreference == refCusRatesList.Last().ZZ2_ZZS_NKPreference && item.refCusRate.RefCusApplicabilities.First().ZZT_ZZA_NKTradeGroup == refCusRatesList.Last().RefCusApplicabilities.First().ZZT_ZZA_NKTradeGroup && item.refCusRate.ZZ2_StartDate == refCusRatesList.Last().ZZ2_StartDate)
					{
						if (refCusRatesList.Last().RefCusApplicabilities.First().ZZT_AdditionalCode == null)
						{
							refCusRatesList.Last().RefCusApplicabilities.First().ZZT_AdditionalCode = GetDescriptionHashCode(previousRate);
						}
						item.refCusRate.RefCusApplicabilities.First().ZZT_AdditionalCode = GetDescriptionHashCode(item.rate);
					}

					refCusRatesList.Add(item.refCusRate);
					previousRate = item.rate;
				}
			}

			IEnumerable<tariffMasterDataCommodityCodeRate> splitOverlappingValidityRanges(IEnumerable<tariffMasterDataCommodityCodeRate> rates)
			{
				tariffMasterDataCommodityCodeRate previousRate = null;
				foreach (var rate in splitOverlappingValidFrom(rates).OrderBy(r => r.type).ThenBy(r => r.countryGrpNr).ThenBy(r => r.validFrom).ThenBy(r => r.validTo))
				{
					if (previousRate != null)
					{
						if (rate.type == previousRate.type && rate.countryGrpNr == previousRate.countryGrpNr)
						{
							if (rate.validFrom == previousRate.validFrom && rate.validTo > previousRate.validTo)
							{
								var newRate = cloneRate(rate);
								newRate.validFrom = previousRate.validTo.AddDays(1);
								rate.validTo = previousRate.validTo;
								yield return newRate;
							}
						}
					}
					yield return previousRate = rate;
				}
			}

			IEnumerable<tariffMasterDataCommodityCodeRate> splitOverlappingValidFrom(IEnumerable<tariffMasterDataCommodityCodeRate> rates)
			{
				tariffMasterDataCommodityCodeRate previousRate = null;
				foreach (var rate in rates.OrderBy(r => r.type).ThenBy(r => r.countryGrpNr).ThenByDescending(r => r.validFrom))
				{
					if (previousRate != null)
					{
						if (rate.type == previousRate.type && rate.countryGrpNr == previousRate.countryGrpNr)
						{
							if (rate.validFrom < previousRate.validFrom && rate.validTo > previousRate.validFrom)
							{
								var newRate = cloneRate(rate);
								newRate.validTo = previousRate.validFrom.AddDays(-1);
								rate.validFrom = previousRate.validFrom;
								yield return newRate;
							}
						}
					}
					yield return previousRate = rate;
				}
			}

			tariffMasterDataCommodityCodeRate cloneRate(tariffMasterDataCommodityCodeRate rate)
			{
				return new tariffMasterDataCommodityCodeRate()
				{
					value = rate.value,
					type = rate.type,
					assessmentCode = rate.assessmentCode,
					customsFavourCode = rate.customsFavourCode,
					countryGrpNr = rate.countryGrpNr,
					countryGrpNrSpecified = rate.countryGrpNrSpecified,
					validFrom = rate.validFrom,
					validTo = rate.validTo,
					text = rate.text
				};
			}
		}

		RefCusRate CreateRefCusRate(tariffMasterDataCommodityCodeRate rate, IEnumerable<int> tradeGroupsHavingPRRate)
		{
			bool isRateTypePR = IsRateTypePR(rate);

			List<RefCusExcludedTradeGroup> excludedTradeGroupList = null;
			if (isRateTypePR)
			{
				foreach (var tradeGroup in tradeGroupsHavingPRRate.Where(x => x > rate.countryGrpNr))
				{
					if (GetTradeGroupCountries(rate.countryGrpNr).Intersect(GetTradeGroupCountries(tradeGroup)).Any())
					{
						if (excludedTradeGroupList == null)
						{
							excludedTradeGroupList = new List<RefCusExcludedTradeGroup>();
						}

						var formattedTradeGroup = tradeGroup.ToString(CultureInfo.InvariantCulture);
						if (!excludedTradeGroupList.Exists(x => x.ZZC_ZZA_NKTradeGroup == formattedTradeGroup))
						{
							excludedTradeGroupList.Add(new RefCusExcludedTradeGroup() { ZZC_ZZA_NKTradeGroup = formattedTradeGroup });
						}
					}
				}
			}

			return new RefCusRate()
			{
				ZZ2_StartDate = Helper.Truncate(rate.validFrom),
				ZZ2_EndDate = Helper.EndOfDay(Helper.Truncate(rate.validTo)),
				ZZ2_RateFormula = GetRateFormula(rate.assessmentCode, rate.value),
				ZZ2_ZZS_NKPreference = rate.type,
				ZZ2_ZZS_ZZZ_NKDataGrouping = (rate.type?.Length >= 1) ? "CH" : "",
				RefCusApplicabilities = new[] { new RefCusApplicability()
						{
							ZZT_StartDate = Helper.Truncate(rate.validFrom),
							ZZT_EndDate = Helper.EndOfDay(Helper.Truncate(rate.validTo)),
							ZZT_ZZA_NKTradeGroup = (isRateTypePR ? rate.countryGrpNr :  AllCountriesGroup).ToString(CultureInfo.InvariantCulture),
							RefCusExcludedTradeGroups = excludedTradeGroupList?.ToArray(),
						}
					}
			};
		}

		string GetDescriptionHashCode(tariffMasterDataCommodityCodeRate rate)
		{
			string hashCode;

			var textDE = rate.text?.Where(t => t.language == "D").FirstOrDefault();
			if (!string.IsNullOrEmpty(textDE?.value))
			{
				var textEN = rate.text?.Where(t => t.language == "E").FirstOrDefault();
				var textFR = rate.text?.Where(t => t.language == "F").FirstOrDefault();
				var textIT = rate.text?.Where(t => t.language == "I").FirstOrDefault();
				hashCode = AdditionalCodeList.AddOrGetDescriptionHashCode(textDE.value, textEN?.value, textFR?.value, textIT?.value, textDE.validFrom, textDE.validTo);
			}
			else
			{
				hashCode = AdditionalCodeList.AddOrGetDescriptionHashCode(rate.value, null, null, null, rate.validFrom, rate.validTo);
			}

			return hashCode;
		}

		static void GetTareSupplements(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCode commodityCode)
		{
			if (commodityCode?.tareSupplement?.Length >= 1)
			{
				foreach (var tareSupplement in commodityCode.tareSupplement)
				{
					refCusTariffAttributesList.Add(new RefCusTariffAttribute()
					{
						ZZ3_Name = "tareSupplement",
						ZZ3_Value = tareSupplement.value
					});
				}
			}
		}

		static void GetCustomsFavourHintCode(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCode commodityCode)
		{
			if (commodityCode?.customsFavourHintCode?.Length >= 1)
			{
				var value = (from customsFavourHintCode in commodityCode.customsFavourHintCode
							 orderby customsFavourHintCode.validTo
							 select customsFavourHintCode.value)
									.Last();

				refCusTariffAttributesList.Add(new RefCusTariffAttribute()
				{
					ZZ3_Name = "customsFavourHintCode",
					ZZ3_Value = value.ToString(CultureInfo.InvariantCulture)
				});
			}
		}

		static void GetStorageType(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode)
		{
			if (statisticalCode != null)
			{
				refCusTariffAttributesList.Add(new RefCusTariffAttribute()
				{
					ZZ3_Name = "storageType",
					ZZ3_Value = statisticalCode.storageType == "J" ? "Y" : statisticalCode.storageType
				});
			}
		}

		static void GetProcessingCode(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode)
		{
			if (statisticalCode?.processingCode?.Length >= 1)
			{
				var value = (from processingCode in statisticalCode.processingCode
							 orderby processingCode.validTo
							 select processingCode.value)
									.Last();

				refCusTariffAttributesList.Add(new RefCusTariffAttribute()
				{
					ZZ3_Name = "processingCode",
					ZZ3_Value = value.ToString(CultureInfo.InvariantCulture)
				});
			}
		}

		Dictionary<int, string[]> tradeGroupCountries;

		IEnumerable<string> GetTradeGroupCountries(int countryGroupNr)
		{
			if (tradeGroupCountries == null)
			{
				tradeGroupCountries = new Dictionary<int, string[]>();
				var countryGroupsDoc = Helper.DeserializeXML<countryCodes>(countryCodesDownload.Content);
				foreach (var countryGroup in countryGroupsDoc.countryGroups.Where(x => x.grpUser == "Tarif"))
				{
					tradeGroupCountries.Add(countryGroup.grpNr, countryGroupsDoc.countries.Where(x => x.countryGroupAssignment != null && x.countryGroupAssignment.Any(y => y.grpNr == countryGroup.grpNr)).Select(x => x.isoCode).ToArray());
				}
			}
			return tradeGroupCountries.TryGetValue(countryGroupNr, out string[] countries) ? countries : Array.Empty<string>();
		}

		static bool IsRateTypePR(tariffMasterDataCommodityCodeRate rate) => rate.type == "PR";
	}
}
