using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator
{
	public class EUNWebsiteData
	{
		public string TariffCode;
		public string TariffDescription;
		public List<RateData> Rates = new List<RateData>();
		public Exception ex;

		public void ConvertToRefCusTariff(XmlWriter writer)
		{
			if (Rates.Count(x => x.EunMapping.HasAdditionalRule) != 0)
				ApplyRateDataRules();

			foreach (var tarifftype in Rates.GroupBy(x => x.EunMapping.TariffType))
			{
				var tariff = new RefCusTariff()
				{
					ZZ1_TariffCode = TariffCode,
					ZZ1_ZZI_NKTariffType = tarifftype.First().EunMapping.TariffType,
					ZZ1_StartDate = Rates.Min(x => x.StartDate),
					ZZ1_EndDate = Rates.Max(x => x.EndDate),
					ZZ1_Description = TariffDescription
				};

				var rates = new List<RefCusRate>();
				var refCusConditions = new List<RefCusCondition>();
				string errorDescription = null;

				foreach (var rate in tarifftype)
				{
					var excludedTradeGroups = new List<RefCusExcludedTradeGroup>();
					foreach (var excludedCountry in rate.ExcludedCountries)
					{
						var excludedTradeGroup = new RefCusExcludedTradeGroup()
						{
							ZZC_ZZA_NKTradeGroup = excludedCountry
						};
						excludedTradeGroups.Add(excludedTradeGroup);
					}

					var refCusApplicabilities = new List<RefCusApplicability>();
					var applicability = new RefCusApplicability()
					{
						ZZT_StartDate = rate.StartDate,
						ZZT_EndDate = rate.EndDate,
						ZZT_ZZA_NKTradeGroup = rate.GeographicalArea,
						ZZT_OrderNumber = rate.OrderNumber ?? "",
						RefCusExcludedTradeGroups = excludedTradeGroups.ToArray()
					};
					refCusApplicabilities.Add(applicability);

					if (rate.Conditions.Count != 0)
					{
						foreach (var condition in rate.Conditions)
						{
							var refCusApplicabilityForCondition = new RefCusApplicability()
							{
								ZZT_StartDate = rate.StartDate,
								ZZT_EndDate = rate.EndDate,
								ZZT_ZZA_NKTradeGroup = rate.GeographicalArea,
								ZZT_OrderNumber = rate.OrderNumber ?? ""
							};

							var conditionValue = new RefCusConditionValue()
							{
								ZX3_ZX4_NKValueType = condition.Columns[0].Substring(0, 1),
								ZX3_Value = condition.Certificate
							};

							var existingCondition = refCusConditions?.FirstOrDefault(x => x.ZX1_ZX2_NKConditionType == rate.EunMapping.MeasureType
							&& x.ZX1_Comment == (condition.Columns?[1] + " " + condition.Certificate).Trim()
							&& x.ZX1_StartDate == rate.StartDate
							&& x.ZX1_EndDate == rate.EndDate);

							if (existingCondition != null)
							{
								var applicabilities = existingCondition.RefCusApplicabilities;
								Array.Resize(ref applicabilities, applicabilities.Length + 1);
								applicabilities[applicabilities.Length - 1] = refCusApplicabilityForCondition;
								existingCondition.RefCusApplicabilities = applicabilities;
							}
							else
							{
								var refCusCondition = new RefCusCondition()
								{
									ZX1_StartDate = rate.StartDate,
									ZX1_EndDate = rate.EndDate,
									ZX1_ConditionValueTrueMeansStop = false,
									ZX1_Comment = (condition.Columns?[1] + " " + condition.Certificate).Trim(),
									ZX1_ZX2_NKConditionType = rate.EunMapping.MeasureType,
									ZX1_IsExport = rate.EunMapping?.TariffType == "EXP" ? true : false,
									ZX1_IsImport = rate.EunMapping?.TariffType == "IMP" ? true : false,
									RefCusApplicabilities = new[] { refCusApplicabilityForCondition },
									RefCusConditionValues = new[] { conditionValue }
								};
								refCusConditions.Add(refCusCondition);
							}
						}
					}

					string standaridisedRateFormula = TransformWebFormulaToSpreadsheetStandard.GetSpreadsheetStandardFormula(rate.RateFormula);
					foreach (var rateFormula in FormulaExtractor.GetFormula(standaridisedRateFormula))
					{
						foreach (var preference in rate.EunMapping?.Preference)
						{
							var existingRate = rates?.FirstOrDefault(x => x.ZZ2_ZZS_NKPreference == preference
							&& x.ZZ2_ZY1_NKRateCode == rateFormula.Item2
							&& x.ZZ2_RateFormula == rateFormula.Item1
							&& x.ZZ2_StartDate == rate.StartDate
							&& x.ZZ2_EndDate == rate.EndDate);

							if (existingRate != null)
							{
								var applicabilities = existingRate.RefCusApplicabilities;
								Array.Resize(ref applicabilities, applicabilities.Length + 1);
								applicabilities[applicabilities.Length - 1] = applicability;
								existingRate.RefCusApplicabilities = applicabilities;
							}
							else
							{

								var refCusRate = new RefCusRate()
								{
									ZZ2_StartDate = rate.StartDate,
									ZZ2_EndDate = rate.EndDate,
									ZZ2_RateFormula = rateFormula.Item1,
									ZZ2_ZY1_NKRateCode = rateFormula.Item2,
									ZZ2_ZZS_NKPreference = preference,
									RefCusApplicabilities = refCusApplicabilities.ToArray()
								};
								if (standaridisedRateFormula == refCusRate.ZZ2_RateFormula)
								{
									var rateError = "Unable to transform rate from string : " + standaridisedRateFormula;
									if (!string.IsNullOrEmpty(errorDescription))
										errorDescription += rateError;
									errorDescription += Environment.NewLine + rateError;
								}
								rates.Add(refCusRate);
							}
						}
					}
				}
				tariff.RefCusRates = rates.ToArray();
				tariff.RefCusConditions = refCusConditions.ToArray();

				if (string.IsNullOrEmpty(errorDescription))
				{
					writer.PopulateData(tariff);
				}
				else
				{
					throw new NotSupportedException($"Error processing EUN tariff from website, {errorDescription}.");
				}

			}
		}

		private void ApplyRateDataRules()
		{
			foreach (var item in Rates.Where(x => x.EunMapping.MeasureType == "143" && new[] { "2004", "2020", "2027" }.Contains(x.GeographicalArea)))
			{
				item.EunMapping.Preference = new[] { "220", "225" };
			}
		}
	}

	public class RateData
	{
		public string GeographicalArea;
		public string TariffTypeDescription;
		public DateTime StartDate;
		public DateTime EndDate;
		public string RateFormula;
		public string OrderNumber;
		public EUNMapping EunMapping;
		public List<string> ExcludedCountries = new List<string>();
		public List<RateCondition> Conditions = new List<RateCondition>();
	}

	public class RateCondition
	{
		public string Certificate;
		public List<string> Columns = new List<string>();
	}
}
