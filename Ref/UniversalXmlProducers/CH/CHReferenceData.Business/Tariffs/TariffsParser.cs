using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	public abstract class TariffsParser
	{
		protected abstract string TariffType { get; }

		protected virtual string RelationshipTariffType => null;

		protected abstract string[] WriterConfigurationProperties { get; }

		protected abstract string RateTypeConstantValue { get; }

		protected abstract string RateCodeConstantValue { get; }

		protected TariffsParser(DateTime actualDate)
		{
			ActualDate = actualDate;
		}
		protected DateTime ActualDate { get; }

		public abstract void ConvertToRefXML(string outputFile, Func<string, bool> tariffCodeFilter = null);

		internal XmlWriterConfiguration GetXmlWriterConfiguration() => GetXmlWriterConfiguration(WriterConfigurationProperties, TariffType, RelationshipTariffType, RateTypeConstantValue, RateCodeConstantValue);

		internal static XmlWriterConfiguration GetXmlWriterConfiguration(string[] writerConfigurationProperties, string tariffType, string relationshipTariffType, string rateTypeConstantValue, string rateCodeConstantValue)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, tariffType);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "CH");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "CH");
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description, false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate, false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_EndDate, false);
			if (writerConfigurationProperties.Contains(nameof(RefCusTariff.ZZ1_CompositeKeyOnZZ5)))
			{
				tariffConfiguration.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, false);
			}
			if (writerConfigurationProperties.Contains(nameof(RefCusTariffUOM)))
			{
				tariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs);
			}
			if (writerConfigurationProperties.Contains(nameof(RefCusTariffLanguage)))
			{
				tariffConfiguration.IncludeColumn(x => x.RefCusTariffLanguages);
			}
			if (writerConfigurationProperties.Contains(nameof(RefCusCondition)))
			{
				tariffConfiguration.IncludeColumn(x => x.RefCusConditions);
			}
			if (writerConfigurationProperties.Contains(nameof(RefCusVATApplicability)))
			{
				tariffConfiguration.IncludeColumn(x => x.RefCusVATApplicabilities);
			}
			if (writerConfigurationProperties.Contains(nameof(RefCusRate)))
			{
				tariffConfiguration.IncludeColumn(x => x.RefCusRates);
			}
			if (writerConfigurationProperties.Contains(nameof(RefCusTariffAttribute)))
			{
				tariffConfiguration.IncludeColumn(x => x.RefCusTariffAttributes);
			}
			if (writerConfigurationProperties.Contains(nameof(RefCusTariffRelationship)))
			{
				tariffConfiguration.IncludeColumn(x => x.RefCusTariffRelationships);
			}

			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);

			if (writerConfigurationProperties.Contains(nameof(RefCusTariffLanguage)))
			{
				var languageConfiguration = new EntityTypeConfiguration<RefCusTariffLanguage>(true);
				languageConfiguration.IncludeColumn(x => x.ZX7_ZX6_NKLanguage, true);
				languageConfiguration.IncludeColumn(x => x.ZX7_Description, false);
				writerConfiguration.IncludeEntityTypeConfiguration(languageConfiguration);
			}

			if (writerConfigurationProperties.Contains(nameof(RefCusTariffUOM)))
			{
				var tariffUomConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
				tariffUomConfiguration.IncludeColumn(x => x.ZZ8_Type, true);
				tariffUomConfiguration.IncludeColumn(x => x.ZZ8_UOM, false);
				tariffUomConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, false, "CH");
				writerConfiguration.IncludeEntityTypeConfiguration(tariffUomConfiguration);
			}

			ConfigureRefCusCondition(writerConfiguration, writerConfigurationProperties, tariffType);

			if (writerConfigurationProperties.Contains(nameof(RefCusConditionLanguage)))
			{
				var refCusConditionLanguageConfiguration = new EntityTypeConfiguration<RefCusConditionLanguage>(true);
				refCusConditionLanguageConfiguration.IncludeColumn(x => x.ZXJ_ZX6_NKLanguage, true);
				refCusConditionLanguageConfiguration.IncludeColumn(x => x.ZXJ_Comment, false);
				refCusConditionLanguageConfiguration.IncludeColumnWithConstantValue(x => x.ZXJ_Source, false, "CH Tares");
				writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionLanguageConfiguration);
			}

			if (writerConfigurationProperties.Contains(nameof(RefCusConditionValue)))
			{
				var refCusConditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
				refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);
				refCusConditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, true, "CH");
				refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, false);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionValueConfiguration);
			}

			if (writerConfigurationProperties.Contains(nameof(RefCusVATApplicability)))
			{
				var vatApplicabilityConfiguration = new EntityTypeConfiguration<RefCusVATApplicability>(true);
				vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, true);
				vatApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, true, "CH");
				vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_StartDate, false);
				vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_EndDate, false);
				writerConfiguration.IncludeEntityTypeConfiguration(vatApplicabilityConfiguration);
			}

			ConfigureRefCusRate(writerConfiguration, writerConfigurationProperties, rateTypeConstantValue, rateCodeConstantValue);


			if (writerConfigurationProperties.Contains(nameof(RefCusApplicability)))
			{
				var refCusApplicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
				refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
				refCusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, "CH");
				refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
				refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_EndDate, false);
				refCusApplicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_AdditionalCode, true, string.Empty);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusApplicabilityConfiguration);
				if (writerConfigurationProperties.Contains(nameof(RefCusExcludedTradeGroup)))
				{
					refCusApplicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups);
				}
			}

			if (writerConfigurationProperties.Contains(nameof(RefCusTariffAttribute)))
			{
				var refCusTariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
				refCusTariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Name, true);
				refCusTariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Value, true);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffAttributeConfiguration);
			}

			if (writerConfigurationProperties.Contains(nameof(RefCusExcludedTradeGroup)))
			{
				var refCusExcludedTradeGroupConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
				refCusExcludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
				refCusExcludedTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, "CH");
				writerConfiguration.IncludeEntityTypeConfiguration(refCusExcludedTradeGroupConfiguration);
			}

			if (writerConfigurationProperties.Contains(nameof(RefCusTariffRelationship)))
			{
				var refCusTariffRelationshipConfiguration = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
				refCusTariffRelationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, "CH");
				refCusTariffRelationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_NKTariffType, true, relationshipTariffType);
				refCusTariffRelationshipConfiguration.IncludeColumn(x => x.ZZH_TariffCode, true);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffRelationshipConfiguration);
			}

			return writerConfiguration;
		}

		private static void ConfigureRefCusCondition(XmlWriterConfiguration writerConfiguration, string[] writerConfigurationProperties, string tariffType)
		{
			if (writerConfigurationProperties.Contains(nameof(RefCusCondition)))
			{
				var refCusConditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
				refCusConditionConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true);
				refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, true, "CH");
				refCusConditionConfiguration.IncludeColumn(x => x.ZX1_StartDate, false);
				refCusConditionConfiguration.IncludeColumn(x => x.ZX1_EndDate, false);
				refCusConditionConfiguration.IncludeColumn(x => x.ZX1_Comment, false);
				refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_Source, false, "CH Tares");
				refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsImport, false, tariffType == "IMP" ? "1" : "0");
				refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsExport, false, tariffType == "EXP" ? "1" : "0");
				refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, "CH");
				if (writerConfigurationProperties.Contains(nameof(RefCusApplicability)))
				{
					refCusConditionConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
				}
				if (writerConfigurationProperties.Contains(nameof(RefCusConditionLanguage)))
				{
					refCusConditionConfiguration.IncludeColumn(x => x.RefCusConditionLanguages);
				}
				if (writerConfigurationProperties.Contains(nameof(RefCusConditionValue)))
				{
					refCusConditionConfiguration.IncludeColumn(x => x.RefCusConditionValues);
				}
				writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionConfiguration);
			}
		}

		private static void ConfigureRefCusRate(XmlWriterConfiguration writerConfiguration, string[] writerConfigurationProperties, string rateTypeConstantValue, string rateCodeConstantValue)
		{
			if (writerConfigurationProperties.Contains(nameof(RefCusRate)))
			{
				var refCusRateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
				refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, "CH");
				refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, rateTypeConstantValue);
				refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, "CH");
				if (rateCodeConstantValue == null)
				{
					refCusRateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
				}
				else
				{
					refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_NKRateCode, true, rateCodeConstantValue);
				}
				refCusRateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
				refCusRateConfiguration.IncludeColumn(x => x.ZZ2_EndDate, false);
				refCusRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
				if (writerConfigurationProperties.Contains(nameof(RefCusRate.ZZ2_ZZS_NKPreference)))
				{
					refCusRateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
				}
				if (writerConfigurationProperties.Contains(nameof(RefCusRate.ZZ2_ZZS_ZZZ_NKDataGrouping)))
				{
					refCusRateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true);
				}
				if (writerConfigurationProperties.Contains(nameof(RefCusApplicability)))
				{
					refCusRateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
				}
				writerConfiguration.IncludeEntityTypeConfiguration(refCusRateConfiguration);
			}
		}

		internal static Description GetDescription(TariffStructure tariffDescriptions, KeyStructure keyDescriptions, KeyStructure keyDescriptionsVLS, CustomsFacilities customsFacilities, string commodityCode, short customsFavourCode, short statisticalCode)
		{
			return GetDescription(tariffDescriptions, keyDescriptions, customsFacilities, keyDescriptionsVLS, commodityCode, customsFavourCode, statisticalCode);
		}

		internal static Description GetDescription(TariffStructure tariffDescriptions, KeyStructure keyDescriptions, KeyStructure keyVLSDescriptions, string commodityCode, short statisticalCode)
		{
			return GetDescription(tariffDescriptions, keyDescriptions, null, keyVLSDescriptions, commodityCode, 0, statisticalCode);
		}

		internal static Description GetDescription(TariffStructure tariffDescriptions, KeyStructure keyDescriptions, CustomsFacilities customsFacilities, KeyStructure keyVLSDescriptions, string commodityCode, short customsFavourCode, short statisticalCode)
		{
			if (!tariffDescriptions.TryGetValue(commodityCode, out var description))
			{
				description = new Description() { TextD = "", TextF = "", TextI = "", TextE = "", SortKey = "" };
			}

			if (customsFavourCode > 0 && customsFacilities != null)
			{
				if (customsFacilities.TryGetValue(commodityCode, customsFavourCode, out var customsFacilityDescription))
				{
					description.TextD = appendIfNotEmpty(description.TextD, customsFacilityDescription.TextD);
					description.TextF = appendIfNotEmpty(description.TextF, customsFacilityDescription.TextF);
					description.TextI = appendIfNotEmpty(description.TextI, customsFacilityDescription.TextI);
					description.TextE = appendIfNotEmpty(description.TextE, customsFacilityDescription.TextE);
				}
			}

			if (statisticalCode > 0)
			{
				var statisticalCodeFormatted = statisticalCode.ToString("000", CultureInfo.InvariantCulture);
				if (keyDescriptions.TryGetValue(commodityCode + KeyStructure.KeySeparator + statisticalCodeFormatted, out var keyDescription))
				{
					if (keyVLSDescriptions != null)
					{
						for (var vlsKey = keyDescription.VKey; !string.IsNullOrEmpty(vlsKey);)
						{
							if (!keyVLSDescriptions.TryGetValue(vlsKey, out var keyVLSDescription))
							{
								break;
							}
							keyDescription.TextD = keyVLSDescription.TextD += " " + keyDescription.TextD;
							keyDescription.TextF = keyVLSDescription.TextF += " " + keyDescription.TextF;
							keyDescription.TextI = keyVLSDescription.TextI += " " + keyDescription.TextI;
							keyDescription.TextE = keyVLSDescription.TextE += " " + keyDescription.TextE;
							vlsKey = keyVLSDescription.VKey;
						}
					}

					description.TextD += " " + keyDescription.TextD;
					description.TextF += " " + keyDescription.TextF;
					description.TextI += " " + keyDescription.TextI;
					description.TextE += " " + keyDescription.TextE;
					description.SortKey = description.SortKey + "." + statisticalCodeFormatted;
				}
			}

			return description;

			string appendIfNotEmpty(string text, string additionalText)
			{
				if (additionalText.Any())
				{
					text = text + " " + additionalText;
				}
				return text;
			}
		}

		protected static List<RefCusTariffLanguage> GetLanguages(Description description)
		{
			var languages = new List<RefCusTariffLanguage>();
			AddLanguageIfNotEmpty(languages, "DE", description.TextD);
			AddLanguageIfNotEmpty(languages, "FR", description.TextF);
			AddLanguageIfNotEmpty(languages, "IT", description.TextI);
			AddLanguageIfNotEmpty(languages, "EN", description.TextE);
			return languages;
		}

		protected static void AddLanguageIfNotEmpty(List<RefCusTariffLanguage> languages, string languageCode, string description)
		{
			if (!string.IsNullOrEmpty(description))
			{
				languages.Add(new RefCusTariffLanguage()
				{
					ZX7_ZX6_NKLanguage = languageCode,
					ZX7_Description = Helper.Truncate(description, 2000)
				});
			}
		}

		internal static void GetRefCusConditionOptionalValue(List<RefCusConditionValue> valueList, bool isOptional)
		{
			if (isOptional)
			{
				valueList.Add(new RefCusConditionValue()
				{
					ZX3_ZX4_NKValueType = "INF",
					ZX3_Value = "Optional"
				});
			}
		}

		protected static void GetPermitToleranceValue(List<RefCusConditionValue> valueList, int toleranceCode)
		{
			string value = null;

			switch (toleranceCode)
			{
				case 2:
					value = "[KGMG] <= 2.5";
					break;
				case 3:
					value = "[KGMG] <= 20";
					break;
			}

			if (value != null)
			{
				valueList.Add(new RefCusConditionValue() { ZX3_ZX4_NKValueType = "FRM", ZX3_Value = value });
			}
		}

		protected const string ConditionTypeWGTC1 = "WGTC1";
		protected const string ConditionDefaultLanguageCommentWGTC1 = "Staffelgewichtsprüfung 1 - Nettogewicht";
		protected static RefCusConditionLanguage[] ConditionLanguagesWGTC1 { get; } = new[] {
			new RefCusConditionLanguage() { ZXJ_ZX6_NKLanguage = "FR", ZXJ_Comment = "Contrôle du poids1 – masse nette" },
			new RefCusConditionLanguage() { ZXJ_ZX6_NKLanguage = "IT", ZXJ_Comment = "Prova del peso 1– massa netta" },
			new RefCusConditionLanguage() { ZXJ_ZX6_NKLanguage = "EN", ZXJ_Comment = "Weight check 1 - net mass" },
		};

		protected const string ConditionTypeWGTC2 = "WGTC2";
		protected const string ConditionDefaultLanguageCommentWGTC2 = "Staffelgewichtsprüfung 2 – Eigenmasse/Zusatzmenge";
		protected static RefCusConditionLanguage[] ConditionLanguagesWGTC2 { get; } = new[] {
			new RefCusConditionLanguage() { ZXJ_ZX6_NKLanguage = "FR", ZXJ_Comment = "Contrôle du poids 2 – masse nette/quantité supplémentaire" },
			new RefCusConditionLanguage() { ZXJ_ZX6_NKLanguage = "IT", ZXJ_Comment = "Prova del peso 2 – massa netta/quantità supplementare" },
			new RefCusConditionLanguage() { ZXJ_ZX6_NKLanguage = "EN", ZXJ_Comment = "Weight check2 - net mass/additional quantity" },
		};

		protected static void GetDefaultUOMs(List<RefCusTariffUOM> tariffUOMList)
		{
			tariffUOMList.Add(new RefCusTariffUOM() { ZZ8_Type = "CU1", ZZ8_UOM = "KGMG" });
			tariffUOMList.Add(new RefCusTariffUOM() { ZZ8_Type = "CU2", ZZ8_UOM = "KGM" });
		}

		internal static string MapSensibleGoodsUOM(string commodityCode)
		{
			if (commodityCode != null && commodityCode.Length >= 7)
			{
				switch (commodityCode.Substring(0, 7))
				{
					case "0207.12":
					case "0207.14":
					case "1701.12":
					case "1701.13":
					case "1701.14":
					case "1701.91":
					case "1701.99":
					case "2403.11":
					case "2403.19":
						return "KGM";
					case "2208.20":
					case "2208.30":
					case "2208.40":
					case "2208.50":
					case "2208.60":
					case "2208.70":
					case "2208.90":
						return "LPA";
					case "2402.20":
						return "MIL";
				}
			}

			return null;
		}

		internal static string MapSensibleGoodsCode(string commodityCode)
		{
			if (commodityCode != null && commodityCode.Length >= 7)
			{
				switch (commodityCode.Substring(0, 7))
				{
					case "0207.12":
					case "0207.14":
					case "1701.12":
					case "1701.13":
					case "1701.14":
					case "1701.91":
					case "1701.99":
					case "2208.20":
					case "2208.30":
					case "2208.40":
					case "2208.50":
					case "2208.60":
					case "2208.70":
					case "2402.20":
					case "2403.11":
					case "2403.19":
						return "0";
					case "2208.90":
						return "1";
				}
			}

			return null;
		}

		protected virtual RefCusTariff GetPlaceholderTariff()
		{
			return new RefCusTariff()
			{
				ZZ1_StartDate = new DateTime(1999, 7, 1, 0, 0, 0),
				ZZ1_EndDate = new DateTime(2079, 6, 6, 23, 59, 0),
				ZZ1_CompositeKeyOnZZ5 = "21.99.99.99.99",
				ZZ1_Description = "Warenmuster und Warenproben – Sendungen in kleinen Mengen und von unbedeutendem Wert",
				RefCusTariffLanguages = new[]
				{
					new RefCusTariffLanguage() { ZX7_ZX6_NKLanguage = "EN", ZX7_Description = "Commercial samples and specimens – Consignments in small quantities and of insignificant value" },
					new RefCusTariffLanguage() { ZX7_ZX6_NKLanguage = "FR", ZX7_Description = "Echantillons et spécimens de marchandises – Envoies en petites quantités et d’une valeur insignifiantes" },
					new RefCusTariffLanguage() { ZX7_ZX6_NKLanguage = "IT", ZX7_Description = "Campioni e saggi di merci – Invii di quantità e valore esigui" },
				},
				RefCusTariffUOMs = new[]
				{
					new RefCusTariffUOM() { ZZ8_Type = "CU1", ZZ8_UOM = "KGMG"},
					new RefCusTariffUOM() { ZZ8_Type = "CU2", ZZ8_UOM = "KGM"},
				},
			};
		}

		protected static void GetSensibleGoodsUOM(List<RefCusTariffUOM> tariffUOMList, string commodityCode)
		{
			var sensibleGoodsUOM = MapSensibleGoodsUOM(commodityCode);
			if (sensibleGoodsUOM != null)
			{
				tariffUOMList.Add(new RefCusTariffUOM() { ZZ8_Type = "CU4", ZZ8_UOM = sensibleGoodsUOM });
			}
		}

		protected static void GetScaleWeightCode(List<RefCusCondition> refCusConditionList, int scaleWeightCode, string lowerScaleWeight, string upperScaleWeight, DateTime validFrom, DateTime validTo, string uom)
		{
			if (scaleWeightCode == 1 || scaleWeightCode == 2)
			{
				var value = new RefCusConditionValue();
				var condition = new RefCusCondition()
				{
					ZX1_ZX2_ZZZ_NKDataGrouping = "CH",
					ZX1_StartDate = Helper.Truncate(validFrom),
					ZX1_EndDate = Helper.EndOfDay(Helper.Truncate(validTo)),
					RefCusConditionValues = new[] { value },
				};

				if (scaleWeightCode == 1)
				{
					condition.ZX1_ZX2_NKConditionType = ConditionTypeWGTC1;
					condition.ZX1_Comment = ConditionDefaultLanguageCommentWGTC1;
					condition.RefCusConditionLanguages = ConditionLanguagesWGTC1;
					value.ZX3_ZX4_NKValueType = "FRM";
					value.ZX3_Value = $"[KGM] >= {lowerScaleWeight.PadLeftMissingZero()}";
				}
				else
				{
					condition.ZX1_ZX2_NKConditionType = ConditionTypeWGTC2;
					condition.ZX1_Comment = ConditionDefaultLanguageCommentWGTC2;
					condition.RefCusConditionLanguages = ConditionLanguagesWGTC2;
					value.ZX3_ZX4_NKValueType = "FRM";
					value.ZX3_Value = $"[KGM]/[{uom}] <= {upperScaleWeight.PadLeftMissingZero()} & [KGM]/[{uom}] >= {lowerScaleWeight.PadLeftMissingZero()}";
				}

				refCusConditionList.Add(condition);
			}
		}

		private protected void GetPermitComment(RefCusCondition condition, bool optional, int toleranceCode, int permitAuthority, PermitInformation permitInformation, string commodityCodeValue, short statisticalCodeValue, sbyte grpObligation = 0)
		{
			string[] commentPart1;

			if (optional)
			{
				commentPart1 = new[] { "Vorlage einer Bewilligung OPTIONAL", "Présentation d’un permis OPTIONNEL", "Presentazione di un permesso OPZIONALE", "Presentation of a permit OPTIONAL" };
			}
			else
			{
				switch (toleranceCode)
				{
					case 1:
						commentPart1 = new[] { "Vorlage einer Bewilligung", "Présentation d'un permis", "Presentazione di un permesso", "Presentation of a permit" };
						break;
					case 2:
					case 3:
						commentPart1 = new[] { "Vorlage einer Bewilligung bei Überschreitung des Toleranzgewichtes", "Présentation d'un permis en cas de dépassement du poids de tolérance", "Presentazione di un permesso quando la tolleranza del peso è superata", "Presentation of a permit when the weight tolerance is exceeded" };
						break;
					default:
						commentPart1 = null;
						break;
				}
			}

			var languageCodes = new string[] { "DE", "FR", "IT", "EN" };
			var languageList = new List<RefCusConditionLanguage>();

			permitInformation.TryGetValue(PermitInformation.CreateKey(commodityCodeValue, statisticalCodeValue, permitAuthority, toleranceCode), out var description);
			string[] remarks = new string[] { description.TextD, description.TextF, description.TextI, description.TextE };

			for (var languageIndex = 0; languageIndex < languageCodes.Length; languageIndex++)
			{
				var comment = new StringBuilder();

				if (!string.IsNullOrEmpty(remarks[languageIndex]))
				{
					comment.Append(remarks[languageIndex]);
				}

				if (commentPart1 != null)
				{
					if (comment.Length > 0)
					{
						comment.Insert(0, " (").Append(')');
					}
					comment.Insert(0, commentPart1[languageIndex]);
				}

				AppendPermitGrpObligationComment(languageCodes[languageIndex], comment, grpObligation);

				if (languageIndex == 0)
				{
					condition.ZX1_Comment = comment.ToString();
				}
				else
				{
					if (comment.Length > 0)
					{
						languageList.Add(new RefCusConditionLanguage()
						{
							ZXJ_ZX6_NKLanguage = languageCodes[languageIndex],
							ZXJ_Comment = comment.ToString(),
						});
					}
				}
			}

			condition.RefCusConditionLanguages = languageList.ToArray();
		}

		protected virtual void AppendPermitGrpObligationComment(string language, StringBuilder comment, sbyte grpObligation)
		{
		}

		internal static void GetNonCustomsLawComment(RefCusCondition condition, int code, bool optional, NonCustomsLawInformation nonCustomsLawInformation, string commodityCodeValue, short statisticalCodeValue)
		{
			string[] commentPart1;

			if (optional)
			{
				commentPart1 = new[] { "Kontrolle von Nichtzollrechtlichen Erlassen OPTIONAL", "Contrôle des actes législatifs autres que douaniers OPTIONNEL", "Controllo delle leggi non doganali OPZIONALE", "Non-customs laws check OPTIONAL" };
			}
			else
			{
				commentPart1 = new[] { "Kontrolle von Nichtzollrechtlichen Erlassen", "Contrôle des actes législatifs autres que douaniers", "Controllo delle leggi non doganali", "Non-customs laws check" };
			}

			var languageCodes = new string[] { "DE", "FR", "IT", "EN" };
			var languageList = new List<RefCusConditionLanguage>();

			nonCustomsLawInformation.TryGetValue(NonCustomsLawInformation.CreateKey(commodityCodeValue, statisticalCodeValue, code), out var description);
			string[] remarks = new string[] { description.TextD, description.TextF, description.TextI, description.TextE };

			for (var languageIndex = 0; languageIndex < languageCodes.Length; languageIndex++)
			{
				var comment = new StringBuilder();

				comment.Append(commentPart1[languageIndex]);

				if (!string.IsNullOrEmpty(remarks[languageIndex]))
				{
					comment.Append(" (" + remarks[languageIndex] + ")");
				}

				if (languageIndex == 0)
				{
					condition.ZX1_Comment = comment.ToString();
				}
				else
				{
					if (comment.Length > 0)
					{
						languageList.Add(new RefCusConditionLanguage()
						{
							ZXJ_ZX6_NKLanguage = languageCodes[languageIndex],
							ZXJ_Comment = comment.ToString(),
						});
					}
				}
			}
			condition.RefCusConditionLanguages = languageList.ToArray();
		}


		protected static void GetSensibleGoodsCode(List<RefCusTariffAttribute> refCusTariffAttributesList, string commodityCodeValue)
		{
			var sensibleGoodsCode = MapSensibleGoodsCode(commodityCodeValue);
			if (sensibleGoodsCode != null)
			{
				refCusTariffAttributesList.Add(new RefCusTariffAttribute() { ZZ3_Name = "sensibleGoodsCode", ZZ3_Value = sensibleGoodsCode });
			}
		}

		protected const int AllCountriesGroup = 200000;
	}
}
