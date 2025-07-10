using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class Measure : IMeasure
	{
		public Measure(string sid, string measureType, string measureClass, string tradeGroup, List<string> excludedTradeGroups, string applicationTerritory, string taxCode, string taxCodeDescription
			, string regulation, DateTime startDate, DateTime endDate, string quotaNumber, string supplementaryCode, string supplementaryCodeDescription, string nomenclature, string direction
			, List<string> renvois, List<string> preferences, List<Component> components, List<Condition> conditions)
		{
			this.Sid = sid;
			this.MeasureType = measureType;
			this.TradeGroup = tradeGroup;
			this.ExcludedTradeGroups = excludedTradeGroups;
			this.ApplicationTerritory = applicationTerritory;
			this.TaxCode = taxCode;
			this.TaxCodeDescription = taxCodeDescription;
			this.Regulation = regulation;
			this.StartDate = startDate;
			this.EndDate = endDate;
			this.QuotaNumber = quotaNumber;
			this.SupplementaryCode = supplementaryCode;
			this.SupplementaryCodeDescription = supplementaryCodeDescription;
			this.Nomenclature = nomenclature;
			this.Direction = direction;
			this.Renvois = renvois;
			this.Preferences = preferences;
			this.Components = components;
			this.Conditions = conditions;
			this.MeasureClass = measureClass;
		}

		#region VAT Applicability

		public RefCusVATApplicability[] GetVatApplicabilities()
		{
			var result = new List<RefCusVATApplicability>();

			if (UniversalDataHelper.CheckDatesAreValid(StartDate, EndDate))
			{
				var vatFRTaxCode = TaxCode;
				var vatStartDate = StartDate;
				var vatEndDate = EndDate;

				var vatRateType = GetVatRateType();
				string region1, region2;
				MeasureHelper.SplitRegionWhereNecessary(ApplicationTerritory, out region1, out region2);

				var applicabilityForRegion1 = CreateRefCusVATApplicability(vatRateType, vatFRTaxCode, vatStartDate, vatEndDate, SupplementaryCode, SupplementaryCodeDescription, region1);
				result.Add(applicabilityForRegion1);

				if (!string.IsNullOrEmpty(region2))
				{
					var applicabilityForRegion2 = CreateRefCusVATApplicability(vatRateType, vatFRTaxCode, vatStartDate, vatEndDate, SupplementaryCode, SupplementaryCodeDescription, region2);
					result.Add(applicabilityForRegion2);
				}
			}

			return result.ToArray();
		}

		RefCusVATApplicability CreateRefCusVATApplicability(string vatRateType, string vatFRTaxCode, DateTime vatStartDate, DateTime vatEndDate, string vatCANA, string vatCANADescription, string region)
		{
			return new RefCusVATApplicability()
			{
				ZX5_ZZF_NKTaxOrFeeCode = vatRateType, //FR Specific : here we make reference to the type of VAT : Super reduced, standard, petroleum etc... using 3 codes char
				ZX5_StartDate = vatStartDate,
				ZX5_EndDate = vatEndDate,
				ZX5_AdditionalCode = vatCANA,
				ZX5_Description = vatCANADescription.Length > 500 ? SupplementaryCodeDescription.Substring(0, 500) : SupplementaryCodeDescription,
				ZX5_VATCategory = vatFRTaxCode, //FR Specific : here we store the FR tax code : A445, A305 etc...
				ZX5_ZZA_NKTradeGroup = region
			};
		}

		public string GetVatRateType()
		{
			string result;

			var measureRate = Components?.FirstOrDefault(c => c.Amount.HasValue)?.Amount ?? 0;
			if (measureRate == 0)
			{
				foreach (var condition in Conditions)
				{
					measureRate = condition.Components?.FirstOrDefault(c => c.Amount.HasValue)?.Amount ?? 0;
					if (measureRate != 0)
					{
						break;
					}
				}
			}

			if (measureRate == ApplicationConfig.Instance.StandardVatRate)
			{
				result = standardVatRateCode;
			}
			else if (measureRate == ApplicationConfig.Instance.PetroleumVatRate)
			{
				result = petroleumVatRateCode;
			}
			else if (measureRate == ApplicationConfig.Instance.HalfVatRate)
			{
				result = halfVatRateCode;
			}
			else if (measureRate == ApplicationConfig.Instance.DOMStandardVatRate)
			{
				result = domStandardVatRateCode;
			}
			else if (measureRate == ApplicationConfig.Instance.ReducedVatRate)
			{
				result = reducedVatRateCode;
			}
			else if (measureRate == ApplicationConfig.Instance.SuperReducedVatRate)
			{
				result = superReducedVatRateCode;
			}
			else if (measureRate == ApplicationConfig.Instance.DOMLiveStockVatRate)
			{
				result = domLiveStockVatRateCode;
			}
			else if (measureRate == ApplicationConfig.Instance.DOMPressVatRate)
			{
				result = domPressVatRateCode;
			}
			else if (measureRate == ApplicationConfig.Instance.CorsicaSuperReducedVatRate)
			{
				result = corsicaSuperReducedVatRateCode;
			}
			else
			{
				result = freeVatRateCode;
			}
			return result;
		}

		#endregion

		#region VATConditions

		public RefCusCondition[] GetConditionsForVat()
		{
			var result = new List<RefCusCondition>();

			if (UniversalDataHelper.CheckDatesAreValid(StartDate, EndDate))
			{
				string region1, region2;
				MeasureHelper.SplitRegionWhereNecessary(ApplicationTerritory, out region1, out region2);

				var mandatoryConditions = Conditions.Where(c => string.IsNullOrEmpty(c.ActionCode) && !string.IsNullOrEmpty(c.DocumentCode)).OrderBy(x => x.SequenceNumber).ToList();
				if (mandatoryConditions.Any())
				{
					result.AddRange(CreateConditionsForVat(region1, region2, mandatoryConditions));
				}

				var optionalConditionsForSuspension = Conditions.Where(c => c.ActionCode == "01").OrderBy(x => x.SequenceNumber).ToList();
				if (optionalConditionsForSuspension.Any())
				{
					result.AddRange(CreateConditionsForVat(region1, region2, optionalConditionsForSuspension));
				}
			}

			return result.ToArray();
		}

		List<RefCusCondition> CreateConditionsForVat(string region1, string region2, List<Condition> conditions)
		{
			var result = new List<RefCusCondition>();

			var conditionType = GetConditionType(conditions.First());
			var comment = GetConditionComment(conditions.First());
			var values = GetConditionValues(conditions);
			result.Add(new RefCusCondition()
			{
				ZX1_ZX2_NKConditionType = conditionType,
				ZX1_ZX2_ZZZ_NKDataGrouping = france,
				ZX1_StartDate = StartDate,
				ZX1_IsImport = Direction == "I",
				ZX1_IsExport = Direction == "E",
				ZX1_EndDate = EndDate.MidnightToEndOfDay(),
				ZX1_Comment = comment,
				RefCusApplicabilities = GetConditionApplicabilitiesForVat(region1),
				RefCusConditionValues = values
			});

			if (!string.IsNullOrEmpty(region2))
			{
				result.Add(new RefCusCondition()
				{
					ZX1_ZX2_NKConditionType = conditionType,
					ZX1_ZX2_ZZZ_NKDataGrouping = france,
					ZX1_StartDate = StartDate,
					ZX1_EndDate = EndDate.MidnightToEndOfDay(),
					ZX1_IsImport = Direction == "I",
					ZX1_IsExport = Direction == "E",
					ZX1_Comment = comment,
					RefCusApplicabilities = GetConditionApplicabilitiesForVat(region2),
					RefCusConditionValues = values
				});
			}

			return result;
		}

		RefCusApplicability[] GetConditionApplicabilitiesForVat(string region)
		{
			var result = new List<RefCusApplicability>();
			result.Add(new RefCusApplicability()
			{
				ZZT_AdditionalCode = SupplementaryCode,
				ZZT_StartDate = StartDate,
				ZZT_EndDate = EndDate.MidnightToEndOfDay(),
				ZZT_ZZA_NKTradeGroup = TradeGroup,
				ZZT_ZZA_NKSecondTradeGroup = region,
			});

			return result.ToArray();
		}

		#endregion

		#region Excise Conditions

		public RefCusCondition[] GetConditionsForExcise()
		{
			var result = new List<RefCusCondition>();

			if (UniversalDataHelper.CheckDatesAreValid(StartDate, EndDate))
			{

				string region1, region2;
				MeasureHelper.SplitRegionWhereNecessary(TradeGroup, out region1, out region2);

				var mandatoryConditions = Conditions.Where(c => string.IsNullOrEmpty(c.ActionCode) && !string.IsNullOrEmpty(c.DocumentCode)).OrderBy(x => x.SequenceNumber).ToList();
				if (mandatoryConditions.Any())
				{
					result.AddRange(CreateConditionForExcise(region1, region2, mandatoryConditions));
				}

				var optionalConditionsForSuspension = Conditions.Where(c => c.ActionCode == "01").OrderBy(x => x.SequenceNumber).ToList();
				if (optionalConditionsForSuspension.Any())
				{
					result.AddRange(CreateConditionForExcise(region1, region2, optionalConditionsForSuspension));
				}
			}

			return result.ToArray();
		}

		List<RefCusCondition> CreateConditionForExcise(string region1, string region2, List<Condition> conditions)
		{
			var result = new List<RefCusCondition>();

			var conditionType = GetConditionType(conditions.First());
			var comment = GetConditionComment(conditions.First());
			var values = GetConditionValues(conditions);
			result.Add(new RefCusCondition()
			{
				ZX1_ZX2_NKConditionType = conditionType,
				ZX1_ZX2_ZZZ_NKDataGrouping = france,
				ZX1_StartDate = StartDate,
				ZX1_IsImport = Direction == "I",
				ZX1_IsExport = Direction == "E",
				ZX1_EndDate = EndDate.MidnightToEndOfDay(),
				ZX1_Comment = comment,
				RefCusApplicabilities = GetConditionApplicabilities(region1),
				RefCusConditionValues = values
			});

			if (!string.IsNullOrEmpty(region2))
			{
				result.Add(new RefCusCondition()
				{
					ZX1_ZX2_NKConditionType = conditionType,
					ZX1_ZX2_ZZZ_NKDataGrouping = france,
					ZX1_StartDate = StartDate,
					ZX1_EndDate = EndDate.MidnightToEndOfDay(),
					ZX1_IsImport = Direction == "I",
					ZX1_IsExport = Direction == "E",
					ZX1_Comment = comment,
					RefCusApplicabilities = GetConditionApplicabilities(region2),
					RefCusConditionValues = values
				});
			}

			return result;
		}

		#endregion

		#region Prohibition Conditions

		public RefCusCondition[] GetConditionsForProhibition()
		{
			var result = new List<RefCusCondition>();

			if (UniversalDataHelper.CheckDatesAreValid(StartDate, EndDate))
			{

				var conditions = Conditions.Where(x => string.IsNullOrEmpty(x.ActionCode)).OrderBy(x => x.SequenceNumber).ToList();

				result.Add(CreateConditionForProhibitionAndGrantingOfSea(TradeGroup, conditions));
			}

			return result.ToArray();
		}

		RefCusCondition CreateConditionForProhibitionAndGrantingOfSea(string tradeGroup, List<Condition> conditions)
		{
			var conditionType = conditions.Count > 0 ? GetConditionType(conditions.First()) : MeasureType;
			var comment = conditions.Count > 0 ? GetConditionComment(conditions.First()) : commentForNoDocument;
			var values = conditions.Count > 0 ? GetConditionValues(conditions) : null;
			var result = new RefCusCondition()
			{
				ZX1_ZX2_NKConditionType = conditionType,
				ZX1_ZX2_ZZZ_NKDataGrouping = france,
				ZX1_StartDate = StartDate,
				ZX1_EndDate = EndDate.MidnightToEndOfDay(),
				ZX1_IsImport = Direction == "I",
				ZX1_IsExport = Direction == "E",
				ZX1_Comment = comment,
				RefCusApplicabilities = GetConditionApplicabilities(tradeGroup),
				RefCusConditionValues = values
			};

			return result;
		}

		#endregion

		#region ExportRate Conditions

		public RefCusCondition[] GetConditionsForExportRate()
		{
			var result = new List<RefCusCondition>();

			if (UniversalDataHelper.CheckDatesAreValid(StartDate, EndDate))
			{

				var conditions = Conditions.Where(x => x.IsRateFormula).ToList();

				if (conditions.Count == 1)
				{
					result.Add(CreateConditionForProhibitionAndGrantingOfSea(TradeGroup, conditions));
				}
				else
				{
					conditions = Conditions.Where(x => string.IsNullOrEmpty(x.ActionCode)).OrderBy(x => x.SequenceNumber).ToList();

					result.Add(CreateConditionForProhibitionAndGrantingOfSea(TradeGroup, conditions));
				}
			}

			return result.ToArray();
		}

		#endregion

		#region Granting of Sea Conditions

		public RefCusCondition[] GetConditionsForGrantingOfSea()
		{
			var result = new List<RefCusCondition>();

			if (UniversalDataHelper.CheckDatesAreValid(StartDate, EndDate))
			{

				var conditions = Conditions.OrderBy(x => x.SequenceNumber).ToList();

				result.Add(CreateConditionForProhibitionAndGrantingOfSea(TradeGroup, conditions));
			}

			return result.ToArray();
		}

		#endregion

		#region Conditions methods

		string GetConditionType(Condition condition)
		{
			string result;

			if (IsVAT)
			{
				result = vatConditionType;
			}
			else if (IsImportProhibition || IsExportProhibition || IsExportRate || IsGrantingOfSea)
			{
				result = MeasureType;
			}
			else
			{
				if (string.IsNullOrEmpty(condition.ActionCode) && !string.IsNullOrEmpty(condition.DocumentCode))
				{
					result = mandatoryConditionType;
				}
				else
				{
					result = MeasureType;
				}
			}

			return result;
		}

		string GetConditionComment(Condition condition)
		{
			var result = string.Empty;

			if (!IsVAT)
			{
				if (IsImportProhibition || IsExportProhibition)
				{
					result = GetConditionValues(Conditions).Length == 1 ? commentForOneMandatoryDocument : commentForOneMandatoryAmongManyDocuments;
				}
				else
				{
					if (string.IsNullOrEmpty(condition.ActionCode) && !string.IsNullOrEmpty(condition.DocumentCode))
					{
						result = commentForAllMandatoryDocument;
					}
					else if (condition.ActionCode == "01")
					{
						if (condition.Components.Any(x => x.Code != "DS"))
						{
							result = GetConditionValues(Conditions).Length == 1 ? commentForOneMandatoryDocument : commentForOneMandatoryAmongManyDocuments;
						}
						else
						{
							result = commentForTaxSuspensiveDocuments;
						}
					}
				}
			}
			return result;
		}

		RefCusApplicability[] GetConditionApplicabilities(string region)
		{
			var result = new List<RefCusApplicability>();

			var applicability = CreateApplicability(region);
			result.Add(applicability);

			return result.ToArray();
		}

		static RefCusConditionValue[] GetConditionValues(List<Condition> conditions)
		{
			var result = new List<RefCusConditionValue>();

			var useSmartLogicalOrWithinGroup = conditions.Any(x => x.Code == "B") && conditions.Any(x => x.Code == "C"); //Case when 2 sets of condition should articlate with AND logic instruction
			if (useSmartLogicalOrWithinGroup)
			{
				conditions = conditions.OrderBy(x => x.Code).ThenBy(x => x.SequenceNumber).ToList();
			}

			foreach (var condition in conditions)
			{
				if (!string.IsNullOrEmpty(condition.DocumentCode))
				{
					result.Add(new RefCusConditionValue()
					{
						ZX3_ZX4_NKValueType = condition.DocumentType,
						ZX3_Value = condition.DocumentCode,
						ZX3_LogicalORWithinGroup = (byte)(useSmartLogicalOrWithinGroup ? (condition.Code == "B" ? 1 : 2) : 0)
					});
				}
				else
				{
					result.Add(new RefCusConditionValue()
					{
						ZX3_ZX4_NKValueType = Condition.informative,
						ZX3_Value = $"Sinon le montant à percevoir est égal à {MeasureHelper.BuildFormulaFromComponents(condition.Components, true)}",
					});
				}
			}
			return result.ToArray();
		}

		#endregion

		#region Rates

		public RefCusRate[] GetRate()
		{
			var result = new List<RefCusRate>();

			if (UniversalDataHelper.CheckDatesAreValid(StartDate, EndDate))
			{
				var rateType = RateTypeHelper.GetRateType(TaxCode);

				if (string.IsNullOrEmpty(rateType))
				{
					throw new MissingInfoException($"No rate type found for tax code {TaxCode} when parsing measure {Sid} of tariff {Nomenclature}.");
				}

				var rateFormula = MeasureHelper.GenerateFormula(this, false);
				if (MeasureHelper.IsLookingIncorrect(rateFormula))
				{
					throw new FormatException($"Formula {rateFormula} for measure {Sid} of tariff {Nomenclature} doesn't look correct. Please investigate.");
				}

				result.Add(new RefCusRate()
				{
					ZZ2_ZY1_NKRateCode = TaxCode,
					ZZ2_ZY1_ZZR_NKRateType = rateType,
					ZZ2_RateFormula = rateFormula,
					ZZ2_RateFormulaDerivedFrom = MeasureHelper.GenerateFormula(this, true),
					ZZ2_EndDate = EndDate,
					ZZ2_StartDate = StartDate,
					RefCusRateUOMs = GetRateUoms(),
					RefCusApplicabilities = GetRateApplicabilities(),
				});
			}

			return result.ToArray();
		}

		RefCusRateUOM[] GetRateUoms()
		{
			RefCusRateUOM[] result;

			var component = Components.FirstOrDefault(c => !string.IsNullOrEmpty(c.MeasurementCode));

			if (component != null)
			{
				result = CreateRateUOM(component);
			}
			else
			{
				var conditionComponent = Conditions?.FirstOrDefault(x => x.Components.Any(c => !string.IsNullOrEmpty(c.MeasurementCode)))?.Components?.FirstOrDefault(c => !string.IsNullOrEmpty(c.MeasurementCode)) ?? null;

				if (conditionComponent != null)
				{
					result = CreateRateUOM(conditionComponent);
				}
				else
				{
					result = null;
				}
			}

			return result;
		}

		static RefCusRateUOM[] CreateRateUOM(Component component)
		{
			var result = new List<RefCusRateUOM>();

			var uomCode = component.MeasurementCode + component.Qualifier;
			result.Add(new RefCusRateUOM()
			{
				ZXG_UOM = uomCode
			});

			return result.ToArray();
		}

		RefCusApplicability[] GetRateApplicabilities()
		{
			var result = new List<RefCusApplicability>();

			var applicability = CreateApplicability(TradeGroup);
			result.Add(applicability);

			return result.ToArray();
		}

		RefCusTariffUOM CreateUOM(Component component)
		{
			var uomCode = component.MeasurementCode + component.Qualifier; 
			return new RefCusTariffUOM
			{
				ZZ8_Type = uomType,
				ZZ8_UOM = uomCode,
				ZZ8_ZZA_NKTradeGroup = TradeGroup,
				ZZ8_ZZA_NKSecondTradeGroup = ApplicationTerritory,
				ZZ8_ZZA_ZZZ_NKSecondDataGrouping = string.IsNullOrEmpty(ApplicationTerritory) ? string.Empty : france,
				ZZ8_StartDate = StartDate,
				ZZ8_EndDate = EndDate,
			};
		}

		public RefCusTariffUOM GetUOM()
		{
			RefCusTariffUOM result = null;

			var component = Components.FirstOrDefault(c => !string.IsNullOrEmpty(c.MeasurementCode));
			if (component != null)
			{
				result = CreateUOM(component);
			}
			return result;
		}

		#endregion

		#region Statistical additional codes

		public RefCusTariffAdditionalCode[] GetStatisticalAdditionalCodes()
		{
			var result = new List<RefCusTariffAdditionalCode>();

			if (UniversalDataHelper.CheckDatesAreValid(StartDate, EndDate))
			{
				result.Add(new RefCusTariffAdditionalCode()
				{
					ZY2_AdditionalCode = SupplementaryCode,
					ZY2_Description = SupplementaryCodeDescription,
					ZY2_ZY3_NKCategory = MeasureType,
					RefCusApplicabilities = GetStatisticalAdditionalCodesApplicabilities(),
				});
			}

			return result.ToArray();
		}

		RefCusApplicability[] GetStatisticalAdditionalCodesApplicabilities()
		{
			var result = new List<RefCusApplicability>();

			var applicability = StatisticalAdditionalCodesApplicabilities(TradeGroup);
			result.Add(applicability);

			return result.ToArray();
		}

		RefCusApplicability StatisticalAdditionalCodesApplicabilities(string tradeGroup)
		{
			return new RefCusApplicability()
			{
				ZZT_StartDate = StartDate,
				ZZT_EndDate = EndDate.MidnightToEndOfDay(),
				ZZT_ZZA_NKTradeGroup = tradeGroup,
				ZZT_ZZA_NKSecondTradeGroup = ApplicationTerritory,
				ZZT_ZZA_ZZZ_NKSecondDataGrouping = string.IsNullOrEmpty(ApplicationTerritory) ? string.Empty : france,
				RefCusExcludedTradeGroups = GetExcludedTradeGroups(ExcludedTradeGroups),
			};
		}

		#endregion

		#region Other methods

		RefCusApplicability CreateApplicability(string tradeGroup)
		{
			return new RefCusApplicability()
			{
				ZZT_AdditionalCode = SupplementaryCode,
				ZZT_StartDate = StartDate,
				ZZT_EndDate = EndDate.MidnightToEndOfDay(),
				ZZT_ZZA_NKTradeGroup = tradeGroup,
				ZZT_ZZA_NKSecondTradeGroup = ApplicationTerritory,
				ZZT_ZZA_ZZZ_NKSecondDataGrouping = string.IsNullOrEmpty(ApplicationTerritory) ? string.Empty : france,
				RefCusExcludedTradeGroups = GetExcludedTradeGroups(ExcludedTradeGroups),
			};
		}

		static RefCusExcludedTradeGroup[] GetExcludedTradeGroups(List<string> excludedTradeGroups)
		{
			var result = new List<RefCusExcludedTradeGroup>();
			foreach (var excludedTradeGroup in excludedTradeGroups)
			{
				result.Add(new RefCusExcludedTradeGroup()
				{
					ZZC_ZZA_NKTradeGroup = excludedTradeGroup
				});
			}

			return result.ToArray();
		}

		#endregion

		#region Calulated Properties

		public bool IsRedevance => redevanceAndFeesMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsVAT => vatMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsDevelopment => developmentMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsPrecalculated => precalculatedMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsExcise => exciseMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsImportProhibition => importProhibitionMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsExportProhibition => exportProhibitionMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsExportRate => exportRateMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsGrantingOfSea => grantingOfSeaMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsImportSupported => IsRedevance || IsDevelopment || IsPrecalculated || IsVAT || IsExcise || IsImportProhibition || IsGrantingOfSea || IsStatistical || IsImportUOM;
		public bool IsExportSupported => IsExportProhibition || IsExportRate || IsExportStatistical || IsExportUOM;
		public bool IsStatistical => statisticalMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsExportStatistical => exportStatisticalMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsImportUOM => importUomMeasureTypeList.Contains(MeasureType.Trim());
		public bool IsExportUOM => exportUomMeasureTypeList.Contains(MeasureType.Trim());

		public string Formula => formula ?? (Components.Count > 0 ? formula = MeasureHelper.BuildFormulaFromComponents(Components, false) : string.Empty);
		string formula;

		public string PlainTextFormula => plainTextFormula ?? (Components.Count > 0 ? plainTextFormula = MeasureHelper.BuildFormulaFromComponents(Components, true) : string.Empty);
		string plainTextFormula;


		#endregion

		#region Fields

		public string Sid { get; set; }
		public string MeasureType { get; set; }
		public string MeasureClass { get; set; }
		public string TradeGroup { get; set; }
		public List<string> ExcludedTradeGroups { get; set; }
		public string ApplicationTerritory { get; set; }
		public string TaxCode { get; set; }
		public string TaxCodeDescription { get; set; }
		public string Regulation { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string QuotaNumber { get; set; }
		public string SupplementaryCode { get; set; }
		public string SupplementaryCodeDescription { get; set; }
		public string Nomenclature { get; set; }
		public string Direction { get; set; }
		public List<string> Renvois { get; set; }
		public List<string> Preferences { get; set; }
		public List<Component> Components { get; set; }
		public List<Condition> Conditions { get; set; }

		#endregion

		#region Constants

		public const string standardVatRateCode = "STD";
		public const string petroleumVatRateCode = "PET";
		public const string halfVatRateCode = "RED";
		public const string domStandardVatRateCode = "DST";
		public const string reducedVatRateCode = "RDE";
		public const string superReducedVatRateCode = "SRR";
		public const string domLiveStockVatRateCode = "DAN";
		public const string domPressVatRateCode = "DPR";
		public const string corsicaSuperReducedVatRateCode = "CSR";
		public const string freeVatRateCode = "NIL";

		public const string vatRateType = "VAT";
		public const string vatConditionType = "VAT";
		public const string mandatoryConditionType = "MAN";
		public const string france = "FR";
		public const string uomType = "CU2";

		const string commentForAllMandatoryDocument = "Tous les documents ou dispositions tarifaires particulières suivants sont présents";
		const string commentForTaxSuspensiveDocuments = "Si présentation de l'un des documents ou dispositions tarifaires particulières suivants alors les droits sont suspendus";
		const string commentForOneMandatoryDocument = "Le document ou disposition tarifaire particulière suivant doit être présent";
		const string commentForOneMandatoryAmongManyDocuments = "L'un des documents ou dispositions tarifaires particulières suivants doit être présent";
		const string commentForNoDocument = "Aucun document requis";

		#endregion

		#region Lists

		readonly string[] redevanceAndFeesMeasureTypeList = new string[] { "RCP", "RCR", "RSD", "RVT", "TIM" };
		readonly string[] vatMeasureTypeList = new string[] { "TVA", "TVB" };
		readonly string[] developmentMeasureTypeList = new string[] { "ROC", "TBO", "TCG", "TDA", "TDB", "TDC", "TDF", "TDH", "TPC", "TPP" };
		readonly string[] precalculatedMeasureTypeList = new string[] { "RCA", "RMA", "RPH" };
		readonly string[] exciseMeasureTypeList = new string[] { "AMC", "BNA", "CBE", "CBS", "CMP", "CSS", "PMX", "SOU", "TIC", "TIP", "TMC", "TSC" };
		readonly string[] importProhibitionMeasureTypeList = new string[] { "AAN", "ADI", "ALP", "AMB", "AQI", "ARD", "BIO", "CIA", "COVID", "CWI", "CZO", "DIS", "LAC", "PBP", "PCA", "PCB", "PCC", "PCD", "PCE", "PCF", "PCG", "PCI", "PCJ", "PCK", "PCL", "PCM", "PCN", "PDM", "PHY", "PPH", "PPHUMI", "PRI", "STI" };
		readonly string[] exportProhibitionMeasureTypeList = new string[] { "ADE", "AEX", "AQE", "BDU", "CEA", "COV", "CWE", "PAC", "PCR", "PCS", "PCV", "PCW", "PCX", "PCY", "PCZ", "PPHUME", "PRE", "STE", "TEX" };
		readonly string[] exportRateMeasureTypeList = new string[] { "TMP", "SOU", "CMP" };
		readonly string[] grantingOfSeaMeasureTypeList = new string[] { "OEA", "OEB", "ORA", "ORB" };
		readonly string[] statisticalMeasureTypeList = new string[] { "SEP", "SIP"};
		readonly string[] exportStatisticalMeasureTypeList = new string[] { "SEP" };
		readonly string[] importUomMeasureTypeList = new string[] { "USI", "USU" };
		readonly string[] exportUomMeasureTypeList = new string[] { "USU" };

		#endregion
	}
}
