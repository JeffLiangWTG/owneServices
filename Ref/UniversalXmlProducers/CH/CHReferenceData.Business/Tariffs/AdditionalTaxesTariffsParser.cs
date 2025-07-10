using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.ExportTariffs.MasterData;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	public class AdditionalTaxesTariffsParser : TariffsParser
	{
		readonly DownloadResult masterDataDownload;
		readonly DownloadResult keyStructureAdditionalTaxesDownload;

		public AdditionalTaxesTariffsParser(DownloadResult masterDataDownload, DownloadResult keyStructureAdditionalTaxesDownload, DateTime actualDate) : base(actualDate)
		{
			this.masterDataDownload = masterDataDownload;
			this.keyStructureAdditionalTaxesDownload = keyStructureAdditionalTaxesDownload;
		}

		public override void ConvertToRefXML(string outputFile, Func<string, bool> tariffCodeFilter = null)
		{
			tariffCodeFilter = tariffCodeFilter ?? (t => true);

			var writerConfiguration = GetXmlWriterConfiguration();

			var inputDoc = Helper.DeserializeXML<tariffMasterData>(masterDataDownload.Content);
			var additionalTaxTariffs = new AdditionalTaxTariffs(keyStructureAdditionalTaxesDownload);

			var tariffList = new List<RefCusTariff>();

			AddOptionalGroupsPseudoTariffs(additionalTaxTariffs);

			string lastTariffCode = null;

			foreach (var type in inputDoc.additionalTaxes.type)
			{
				foreach (var key in type.key.OrderByDescending(k => k.validFrom))
				{
					if (additionalTaxTariffs.TryGetValue(type.value, key.value, out AdditionalTaxTariff additionalTaxTariff))
					{
						var tariffCode = type.value + "-" + key.value;
						if (!tariffCodeFilter(tariffCode))
						{
							continue;
						}

						if (tariffCode == lastTariffCode && key.validTo.AddDays(1) == tariffList.Last().ZZ1_StartDate)
						{
							tariffList.Last().ZZ1_StartDate = key.validFrom.Truncate();
						}
						else
						{
							tariffList.Add(GetTariff(tariffCode, type, key, additionalTaxTariff));
						}
						lastTariffCode = tariffCode;
					}
				}
				{
					if (additionalTaxTariffs.TryGetValue(type.value, "000", out AdditionalTaxTariff additionalTaxTariff))
					{
						var tariffCode = type.value + "-000";
						if (!tariffCodeFilter(tariffCode))
						{
							continue;
						}

						tariffList.Add(GetTariff(tariffCode, type, null, additionalTaxTariff));
					}
				}
			}

			var published = inputDoc.created.Date;
			Helper.ExportToXMLFile(DataSource, outputFile, writerConfiguration, published, tariffList);
		}

		static void AddOptionalGroupsPseudoTariffs(AdditionalTaxTariffs additionalTariffs)
		{
			Dictionary<string, AdditionalTaxTariff> pseudoTariffs = new Dictionary<string, AdditionalTaxTariff>();

			foreach (var additionalTariffKeyValuePair in additionalTariffs)
			{
				var type = additionalTariffKeyValuePair.Key.Item1;
				var additionalTariff = additionalTariffKeyValuePair.Value;
				foreach (var relationship in additionalTariff.relationships.Values.Where(r => r.isOptional && r.groupControl == 0))
				{
					if (!pseudoTariffs.TryGetValue(type, out var pseudoTariff))
					{
						pseudoTariff = new AdditionalTaxTariff();
						pseudoTariffs.Add(type, pseudoTariff);
					}
					if (!pseudoTariff.relationships.TryGetValue((relationship.commodityCode, relationship.statisticalCode), out var existingRelationship))
					{
						var newRelationship = new AdditionalTaxRelationship(relationship)
						{
							tradeGroup = AllCountriesGroup
						};
						pseudoTariff.relationships.Add((relationship.commodityCode, relationship.statisticalCode), newRelationship);
					}
					else
					{
						existingRelationship.excludedTradeGroups.Remove(relationship.tradeGroup);
						if (relationship.excludedTradeGroups.Any())
						{
							existingRelationship.excludedTradeGroups.RemoveAll(excludedTradeGroup => !relationship.excludedTradeGroups.Contains(excludedTradeGroup));
						}
					}
				}
			}

			foreach (var pseudoTariffKeyValuePair in pseudoTariffs)
			{
				pseudoTariffKeyValuePair.Value.hasExclusions = pseudoTariffKeyValuePair.Value.relationships.Any(r => r.Value.excludedTradeGroups.Any());
				additionalTariffs.Add((pseudoTariffKeyValuePair.Key, "000"), pseudoTariffKeyValuePair.Value);
			}
		}

		static RefCusTariff GetTariff(string tariffCode, tariffMasterDataAdditionalTaxesType type, tariffMasterDataAdditionalTaxesTypeKey key, AdditionalTaxTariff additionalTaxTariff)
		{
			var refCusTariff = new RefCusTariff()
			{
				ZZ1_TariffCode = tariffCode,
				ZZ1_StartDate = key?.validFrom.Truncate() ?? Helper.MinimumDateTime,
				ZZ1_EndDate = key?.validTo.Truncate().EndOfDay() ?? Helper.MaximumDateTime,
				ZZ1_Description = GetDescription(type.meaningDe, key != null ? key.meaningDe : GetOptionalKeyDescription("DE")),
				RefCusTariffLanguages = new RefCusTariffLanguage[]
				{
								GetLanguage("FR", type.meaningFr, key != null ? key.meaningFr : GetOptionalKeyDescription("FR")),
								GetLanguage("IT", type.meaningIt, key != null ? key.meaningIt : GetOptionalKeyDescription("IT")),
								GetLanguage("EN", type.meaningEn, key != null ? key.meaningEn : GetOptionalKeyDescription("EN")),
				},
				RefCusTariffRelationships = additionalTaxTariff.hasExclusions ? null : GetRelationsships(additionalTaxTariff),
				RefCusTariffAttributes = GetAttributes(additionalTaxTariff)
			};

			var uom = GetUOM(additionalTaxTariff) ?? GetTobaccoUOM(type.value, key?.value);
			if (uom != null)
			{
				refCusTariff.RefCusTariffUOMs = new[] { new RefCusTariffUOM { ZZ8_Type = "CU1", ZZ8_UOM = uom } };
			}

			var rateFormula = GetRateFormula(additionalTaxTariff);
			if (rateFormula != null)
			{
				var rate = new RefCusRate
				{
					ZZ2_ZY1_NKRateCode = type.Value,
					ZZ2_RateFormula = rateFormula,
					ZZ2_StartDate = refCusTariff.ZZ1_StartDate,
					ZZ2_EndDate = refCusTariff.ZZ1_EndDate,
				};

				if (additionalTaxTariff.hasExclusions)
				{
					rate.RefCusApplicabilities = GetApplicabilitiesWithExclusions(refCusTariff, additionalTaxTariff).ToArray();
				}
				else
				{
					rate.RefCusApplicabilities = new[] { GetApplicabilityWithoutExclusions(refCusTariff, additionalTaxTariff) };
				}

				refCusTariff.RefCusRates = new[] { rate };
			}

			return refCusTariff;
		}

		static string GetDescription(string typeMeaning, string keyMeaning)
		{
			return typeMeaning + " - " + keyMeaning;
		}

		static RefCusTariffLanguage GetLanguage(string language, string typeMeaning, string keyMeaning)
		{
			return new RefCusTariffLanguage() { ZX7_ZX6_NKLanguage = language, ZX7_Description = GetDescription(typeMeaning, keyMeaning) };
		}

		static string GetOptionalKeyDescription(string language)
		{
			switch (language)
			{
				case "DE":
					return "OPTIONAL";
				case "FR":
					return "OPTIONNEL";
				case "IT":
					return "OPZIONALE";
				case "EN":
					return "OPTIONAL";
				default:
					return string.Empty;
			}
		}

		static string GetRelatedTariffCode(AdditionalTaxRelationship relationship)
		{
			return relationship.commodityCode.Remove(4, 1) + relationship.statisticalCode.ToString("000", CultureInfo.InvariantCulture);
		}

		static string GetUOM(AdditionalTaxTariff additionalTaxTariff)
		{
			switch (additionalTaxTariff.assessmentCode)
			{
				case 11:
				case 28:
				case 206:
					return "KGMG";
				case 12:
				case 201:
					return "NAR";
				case 13:
				case 24:
				case 227:
					return "KGM";
				case 17:
				case 221:
					return "HLT";
				case 22:
					return "NAR";
				case 23:
				case 211:
					return "LTR";
				case 26:
				case 220:
					return "KGMV";
				case 27:
				case 222:
					return "LPA";
				default:
					return null;
			}
		}

		static string GetTobaccoUOM(string type, string key)
		{
			if (type == "450")
			{
				switch (key)
				{
					case "001":
					case "201":
						return "KGM";
					case "002":
					case "202":
						return "NAR";
					case "003":
					case "203":
						return "ML";
				}
			}
			return null;
		}

		static string GetRateFormula(AdditionalTaxTariff additionalTaxTariff)
		{
			if (additionalTaxTariff.rate == 0)
				return "0";

			switch (additionalTaxTariff.assessmentCode)
			{
				case 11:
					return FormattableString.Invariant($"MIN(MAX({additionalTaxTariff.rateMin}, {additionalTaxTariff.rate / 100} * [KGMG]), {additionalTaxTariff.rateMax})");
				case 12:
					return FormattableString.Invariant($"{additionalTaxTariff.rate} * [NAR]");
				case 13:
					return FormattableString.Invariant($"{additionalTaxTariff.rate} * [KGM]");
				case 17:
					return FormattableString.Invariant($"{additionalTaxTariff.rate} * [HLT]");
				case 22:
					return FormattableString.Invariant($"{additionalTaxTariff.rate / 1000} * [NAR]");
				case 23:
					return FormattableString.Invariant($"{additionalTaxTariff.rate / 1000} * [LTR]");
				case 24:
					return FormattableString.Invariant($"{additionalTaxTariff.rate / 1000} * [KGM]");
				case 25:
					return FormattableString.Invariant($"{additionalTaxTariff.rate / 100} * (VFD + DTY)");
				case 26:
					return FormattableString.Invariant($"{additionalTaxTariff.rate} * [KGMV]");
				case 27:
					return FormattableString.Invariant($"{additionalTaxTariff.rate} * [LPA]");
				case 28:
					return FormattableString.Invariant($"{additionalTaxTariff.rate} * [KGMG]");
				case 29:
					return FormattableString.Invariant($"{additionalTaxTariff.rate}");
				case 201:
					return FormattableString.Invariant($"{additionalTaxTariff.rate / additionalTaxTariff.factor} * [NAR]");
				case 206:
					return FormattableString.Invariant($"MIN(MAX({additionalTaxTariff.rateMin}, {additionalTaxTariff.rate / additionalTaxTariff.factor} * [KGMG]), {additionalTaxTariff.rateMax})");
				case 211:
					return FormattableString.Invariant($"{additionalTaxTariff.rate / additionalTaxTariff.factor} * [LTR]");
				case 220:
					return FormattableString.Invariant($"{additionalTaxTariff.rate} * [KGMV]");
				case 221:
					return FormattableString.Invariant($"{additionalTaxTariff.rate} * [HLT]");
				case 222:
					return FormattableString.Invariant($"{additionalTaxTariff.rate} * [LPA]");
				case 224:
					return FormattableString.Invariant($"{additionalTaxTariff.rate / 100} * (VFD + DTY)");
				case 226:
					return FormattableString.Invariant($"{additionalTaxTariff.rate}");
				case 227:
					return FormattableString.Invariant($"{additionalTaxTariff.rate / additionalTaxTariff.factor} * [KGM]");
				case 0:
					return string.Empty;
				default:
					return null;
			}
		}

		static int GetEdecAssessmentCode(AdditionalTaxTariff additionalTaxTariff)
		{
			switch (additionalTaxTariff.assessmentCode)
			{
				case 201:
					switch (additionalTaxTariff.factor)
					{
						case 1:
							return 12;
						case 1000:
							return 22;
					}
					break;
				case 206:
					switch (additionalTaxTariff.factor)
					{
						case 1:
							return 28;
						case 100:
							return 11;
					}
					break;
				case 211:
					return 23;
				case 220:
					return 26;
				case 221:
					return 17;
				case 222:
					return 27;
				case 224:
					return 25;
				case 226:
					return 29;
				case 227:
					switch (additionalTaxTariff.factor)
					{
						case 1:
							return 13;
						case 1000:
							return 24;
					}
					break;
				default:
					return additionalTaxTariff.assessmentCode;
			}
			return 0;
		}

		static RefCusApplicability GetApplicabilityWithoutExclusions(RefCusTariff refCusTariff, AdditionalTaxTariff tariff)
		{
			return new RefCusApplicability()
			{
				ZZT_ZZA_NKTradeGroup = tariff.relationships.First().Value.tradeGroup.ToString(CultureInfo.InvariantCulture),
				ZZT_StartDate = refCusTariff.ZZ1_StartDate,
				ZZT_EndDate = refCusTariff.ZZ1_EndDate,
			};
		}

		static IEnumerable<RefCusApplicability> GetApplicabilitiesWithExclusions(RefCusTariff refCusTariff, AdditionalTaxTariff tariff)
		{
			foreach (var relationship in tariff.relationships.Values)
			{
				yield return new RefCusApplicability()
				{
					ZZT_AdditionalCode = GetRelatedTariffCode(relationship),
					ZZT_ZZA_NKTradeGroup = relationship.tradeGroup.ToString(CultureInfo.InvariantCulture),
					ZZT_StartDate = refCusTariff.ZZ1_StartDate,
					ZZT_EndDate = refCusTariff.ZZ1_EndDate,
					RefCusExcludedTradeGroups = relationship.excludedTradeGroups.Select(x => new RefCusExcludedTradeGroup() { ZZC_ZZA_NKTradeGroup = x.ToString(CultureInfo.InvariantCulture) }).ToArray()
				};
			}
		}

		static RefCusTariffRelationship[] GetRelationsships(AdditionalTaxTariff additionalTaxTariff)
		{
			return (from relationship in additionalTaxTariff.relationships.Values
					select new RefCusTariffRelationship()
					{
						ZZH_TariffCode = GetRelatedTariffCode(relationship)
					}).ToArray();
		}

		static RefCusTariffAttribute[] GetAttributes(AdditionalTaxTariff additionalTaxTariff)
		{
			return
			[
					new RefCusTariffAttribute()
					{
						ZZ3_Name = "assessmentCode",
						ZZ3_Value = GetEdecAssessmentCode(additionalTaxTariff).ToString(CultureInfo.InvariantCulture),
					}
			];
		}

		protected override string TariffType => "ADT";

		protected override string RelationshipTariffType => "IMP";

		const string DataSource = "CH Tariff ADT";

		protected override string[] WriterConfigurationProperties => new string[]
		{
			nameof(RefCusTariffLanguage),
			nameof(RefCusTariffUOM),
			nameof(RefCusRate),
			nameof(RefCusApplicability),
			nameof(RefCusExcludedTradeGroup),
			nameof(RefCusTariffRelationship),
			nameof(RefCusTariffAttribute),
		};

		protected override string RateTypeConstantValue => "ADT";
		protected override string RateCodeConstantValue => null;
	}
}
