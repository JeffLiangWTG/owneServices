using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public static class VatExciseParser
	{
		static List<string> importMeasures = new List<string>()
		{
			"BBT",
			"RBT",
			"BAO",
			"BAS",
			"BCE",
			"BRC",
			"BVH"
		};

		static Dictionary<string, string> dutyCodes = new Dictionary<string, string>
		{
			["BAO"] = "100",
			["BAS"] = "200",
			["BCE"] = "300",
			["BRC"] = "400",
			["BVH"] = "500"
		};

		static Dictionary<string, string> rateTypes = new Dictionary<string, string>
		{
			["BAO"] = "EXC",
			["BAS"] = "EXC",
			["BCE"] = "MSC",
			["BRC"] = "MSC",
			["BVH"] = "LEV"
		};

		static Dictionary<string, string> vatCodes = new Dictionary<string, string>
		{
			["6"] = "BRR",
			["21"] = "BSR",
			["0"] = "BEZ"
		};

#pragma warning disable CA1502
		public static List<RefCusTariff> ReadXlsIntoResults(XlsFile xlsFile, string sheetName, string type)
		{
			var allExcelDatas = new List<RefCusTariff>();
			var sheetIndex = 1;
			var startingRow = 2;
			var lastRow = int.MaxValue;

			var findSheet = 1;
			while (findSheet <= xlsFile.SheetCount)
			{
				if (sheetName != null && xlsFile.GetSheetName(findSheet) == sheetName)
				{
					sheetIndex = findSheet;
					break;
				}

				findSheet++;
			}

			if (sheetIndex == -1)
			{
				return allExcelDatas;
			}

			xlsFile.SetSheetSelected(sheetIndex, true);
			xlsFile.ActiveSheet = sheetIndex;

			var rowCount = xlsFile.GetRowCount(sheetIndex);

			var rowGrouping = new Dictionary<string, RefCusTariff>();
			var uniqueRows = new Dictionary<string, RefCusTariff>();
			var tariffHeaders = new List<string>();
			for (var rowId = startingRow; rowId <= rowCount && rowId <= lastRow; rowId++)
			{
				RefCusTariff excelData = null;
				var nomenclature = xlsFile.GetCellValue(rowId, 1)?.ToString();
				var cadd = xlsFile.GetCellValue(rowId, 2)?.ToString();
				var measure = xlsFile.GetCellValue(rowId, 3)?.ToString();
				var tradinggroup = xlsFile.GetCellValue(rowId, 4)?.ToString();
				var startDateOA = xlsFile.GetCellValue(rowId, 5)?.ToString();
				var endDateOA = xlsFile.GetCellValue(rowId, 6)?.ToString();
				var vat = xlsFile.GetCellValue(rowId, 8)?.ToString();
				var measurement = "";
				var vatCode = "";
				var dutyExp = "";
				var dutyCode = "";
				var datagroup = "EUN";
				var groupingKey = "";
				var uniqueKey = "";

				if (string.IsNullOrEmpty(measure) || !importMeasures.Contains(measure))
				{
					continue;
				}

				if (endDateOA == null)
				{
					endDateOA = Constants.Common.MaximumDateTime.ToString(CultureInfo.InvariantCulture);
				}
				groupingKey = string.Concat(nomenclature);
				if (dutyCodes.TryGetValue(measure, out dutyCode))
				{
					groupingKey = string.Concat(nomenclature, cadd, measure, tradinggroup, startDateOA, endDateOA);
					uniqueKey = string.Concat(nomenclature, cadd, measure, tradinggroup, startDateOA, endDateOA, vat);
				}
				else if (!string.IsNullOrEmpty(vat) && vatCodes.TryGetValue(vat, out vatCode))
				{
					uniqueKey = string.Concat(tradinggroup, nomenclature, startDateOA, endDateOA, vat);
				}

				if (string.IsNullOrEmpty(uniqueKey))
				{
					continue;
				}

				if (uniqueRows.ContainsKey(uniqueKey))
				{
					continue;
				}
				dutyExp = xlsFile.GetCellValue(rowId, 7)?.ToString();
				measurement = xlsFile.GetCellValue(rowId, 9)?.ToString();
				vat = vat.Replace(",", ".");
				RefCusVATApplicability[] applicabilities = null;
				RefCusRate[] rates = null;

				var tariffCode = nomenclature.Trim();

				if (rowGrouping.ContainsKey(groupingKey))
				{
					rowGrouping.TryGetValue(groupingKey, out excelData);
					applicabilities = excelData.RefCusVATApplicabilities;
					rates = excelData.RefCusRates;
				}
				else
				{
					excelData = new RefCusTariff
					{
						ZZ1_ZZI_NKTariffType = type,
						ZZ1_TariffCode = tariffCode,
						ZZ1_ZZZ_NKDataGrouping = datagroup
					};
				}
				if (string.IsNullOrEmpty(dutyCode) && !string.IsNullOrEmpty(vatCode))
				{
					// import tariff
					excelData.RefCusVATApplicabilities = addCusVATApplicabilityToArray(applicabilities, startDateOA, endDateOA, vatCode, cadd);
				}
				else
				{
					var rateType = "";
					var rateFormula = vat;
					if (!string.IsNullOrEmpty(measurement))
					{
						rateFormula = string.Concat(rateFormula, " * ", "[" + measurement + "]");
					}

					rateTypes.TryGetValue(measure, out rateType);
					// excise codes
					excelData.RefCusRates = addCusRatesToArray(rates, startDateOA, endDateOA, dutyCode, cadd, rateType, dutyExp, rateFormula);
				}

				tariffHeaders.Add(excelData.ZZ1_TariffCode);
				if (!rowGrouping.ContainsKey(groupingKey))
				{
					rowGrouping.Add(groupingKey, excelData);
				}
				uniqueRows.Add(uniqueKey, excelData);
				allExcelDatas.Add(excelData);
			}

			return allExcelDatas;
		}
#pragma warning restore CA1502

		internal class PreferenceMapperNoChangesImplementation : IPreferenceMapper
		{
			public IEnumerable<RefCusCondition> SetPreferenceOnConditions(IEnumerable<RefCusCondition> originalConditions)
			{
				return originalConditions;
			}

			public IEnumerable<RefCusRate> SetPreferenceOnRates(IEnumerable<RefCusRate> originalRates)
			{
				return originalRates;
			}
		}

		static RefCusVATApplicability[] addCusVATApplicabilityToArray(RefCusVATApplicability[] applicabilities, string startDateOA, string endDateOA, string vatCode, string cadd)
		{
			var endDate = Constants.Common.MaximumDateTime;
			DateTime startDate;
			if (!string.IsNullOrEmpty(endDateOA) &&
				DateTime.TryParseExact(endDateOA, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate)
					)
			{
				if (endDate.CompareTo(DateTime.Now) < 0)
				{
					return applicabilities;
				}
			}
			if (applicabilities == null)
			{
				applicabilities = new RefCusVATApplicability[1];
			}
			else
			{
				Array.Resize(ref applicabilities, applicabilities.Length + 1);
			}
			var applicability = new RefCusVATApplicability()
			{
				ZX5_ZZF_NKTaxOrFeeCode = vatCode,
				ZX5_AdditionalCode = cadd
			};

			if (!string.IsNullOrEmpty(startDateOA) &&
				DateTime.TryParseExact(startDateOA, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate)
				)
			{
				applicability.ZX5_StartDate = startDate;
			}
			if (!string.IsNullOrEmpty(endDateOA))
			{
				applicability.ZX5_EndDate = endDate;
			}
			applicabilities[applicabilities.Length - 1] = applicability;
			return applicabilities;
		}

		static RefCusRate[] addCusRatesToArray(RefCusRate[] rates, string startDateOA, string endDateOA, string dutyCode, string cadd, string rateType, string dutyExp, string rateFormula)
		{
			var startDate = Constants.ZZRefCusCondition.MinimumDateTime;
			var endDate = Constants.Common.MaximumDateTime;
			if (!string.IsNullOrEmpty(endDateOA) &&
				DateTime.TryParseExact(endDateOA, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate)
					)
			{
				if (endDate.CompareTo(DateTime.Now) < 0)
				{
					return rates;
				}
			}
			if (!string.IsNullOrEmpty(startDateOA))
			{
				_ = DateTime.TryParse(startDateOA, out startDate);
			}
			if (rates == null)
			{
				rates = new RefCusRate[1];
			}
			else
			{
				foreach (var rt in rates)
				{
					var groupingKey = string.Concat(cadd, startDate.Date, dutyCode, rateType);
					var rateKey = string.Concat(rt.RefCusApplicabilities[0].ZZT_AdditionalCode, rt.ZZ2_StartDate.Date, rt.ZZ2_ZY1_NKRateCode, rt.ZZ2_ZY1_ZZR_NKRateType);

					if (groupingKey == rateKey && rt.ZZ2_RateFormula != rateFormula)
					{
						rt.ZZ2_RateFormula = string.Concat(rt.ZZ2_RateFormula, " + ", rateFormula);
						return rates;
					}
				}
				Array.Resize(ref rates, rates.Length + 1);
			}
			var rate = new RefCusRate()
			{
				ZZ2_ZY1_ZZR_NKRateType = rateType,
				ZZ2_RateFormula = rateFormula,
				ZZ2_ZY1_NKRateCode = dutyCode
			};

			if (!string.IsNullOrEmpty(startDateOA))
			{
				rate.ZZ2_StartDate = startDate;
			}
			if (!string.IsNullOrEmpty(endDateOA))
			{
				rate.ZZ2_EndDate = endDate;
			}
			var applicability = new RefCusApplicability()
			{
				ZZT_AdditionalCode = cadd
			};
			rate.RefCusApplicabilities = new RefCusApplicability[1];
			rate.RefCusApplicabilities[0] = applicability;

			rates[rates.Length - 1] = rate;
			return rates;
		}
	}
}
