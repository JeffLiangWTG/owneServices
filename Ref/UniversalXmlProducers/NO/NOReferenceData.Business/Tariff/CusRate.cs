using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tollsats;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff;

public static class CusRate
{
	public static RefCusRate[] GetRefCusRate(
		vareListe customsDutyRates,
		vareListe rawMaterialTariffs,
		AvgiftListe importFees,
		AvgiftListe exportFees,
		LandgruppeListe countries,
		string tariffId,
		List<string> allUomForTariff,
		RefCusCondition[] refCusCondition)
	{
		return GetRefCusRate_Tollsatser(customsDutyRates, countries, tariffId, allUomForTariff, refCusCondition)
			.Union(GetRefCusRate_Avgifter(importFees, exportFees, tariffId, allUomForTariff, refCusCondition))
			.Union(GetRefCusRate_RawMaterialCustomsRates(rawMaterialTariffs, countries, tariffId, allUomForTariff, refCusCondition))
			.ToArray();
	}

	static IEnumerable<RefCusRate> GetRefCusRate_Tollsatser(vareListe customsDutyRates, LandgruppeListe countries, string tariffId, List<string> allUomForTariff, RefCusCondition[] refCusCondition)
	{
		var allRates = from goods in customsDutyRates?.Goods ?? Array.Empty<AvtaleSats>()
			where goods.id == tariffId
			from goodsRates in goods.Rates
			from rate in goodsRates.Rate
			select new
			{
				goods.Unit,
				goods.UnitDescription,
				goodsRates.CountryGroup,
				rate.RateValue,
				rate.RateUnit,
				rate.DateStart,
				rate.DateEnd
			};
		var allRatesForTariff = allRates.ToArray();
		foreach (var dutyRate in allRatesForTariff)
		{
			var preference = GetPreferenceCodeForTradeGroup(dutyRate.CountryGroup, countries);
			var hasMultipleRatesForSameTariffRateAndTradeGroup = allRatesForTariff.Any(x => x.CountryGroup == dutyRate.CountryGroup & x.RateUnit != dutyRate.RateUnit);
			var rateCode = hasMultipleRatesForSameTariffRateAndTradeGroup && dutyRate.RateUnit == "P" ? Constants.RateTypes.DutyInPercent : Constants.RateTypes.Duty;

			var rate = ConvertRefCusRateDuty(dutyRate.Unit, dutyRate.UnitDescription, dutyRate.CountryGroup, dutyRate.RateValue, dutyRate.RateUnit, preference, rateCode, Constants.Types.Duty, dutyRate.DateStart, dutyRate.DateEnd);
			if (rate != null)
			{
				if (rate.ZZ2_RateFormula != "0")
				{
					rate.RefCusRateUOMs = GenerateRefCusRateUomRecords(allUomForTariff, refCusCondition, dutyRate.RateUnit, rate.ZZ2_ZY1_NKRateCode);
				}

				yield return rate;
			}
		}
	}

	static IEnumerable<RefCusRate> GetRefCusRate_Avgifter(AvgiftListe importFees, AvgiftListe exportFees, string tariffId, List<string> allUomForTariff, RefCusCondition[] refCusCondition)
	{
		var avgiftsListe = new Dictionary<string, AvgiftListe>()
		{
			{ Constants.Types.ExciseImport, importFees },
			{ Constants.Types.ExciseExport, exportFees }
		};
		foreach (var (feeType, avgift) in avgiftsListe)
		{
			var fees = (from goods in avgift?.Goods ?? Array.Empty<Vare>()
				where goods.id == tariffId
				from dutyRates in goods.DutyRates
				from dutyTypes in dutyRates.DutyTypes
				where dutyTypes.DutyType != Constants.Types.VAT
				select new
				{
					avgGrupper = (from dutyGroup in dutyTypes.DutyGroups
						where !dutyGroup.Rate.IsNullOrEmpty()
						orderby dutyGroup.DutyGroup
						select new
						{
							feeCode = string.Concat(dutyTypes.DutyType, dutyGroup.DutyGroup),
							dutyGroup.DateStart,
							dutyGroup.DateEnd,
							dutyGroup.Rate,
							dutyGroup.Unit,
							dutyGroup.RateIsGivenInFractionOfNOK,
							dutyGroup.DutyGroupDescription
						})
				});

			var ratesToAdd = fees
				.Distinct()
				.SelectMany(grp => grp.avgGrupper.Distinct())
				.Select(details =>
				{
					var result = ConvertRefCusRateExcise(details.DateStart, details.DateEnd, details.Rate, details.RateIsGivenInFractionOfNOK, details.Unit, details.feeCode, feeType, details.DutyGroupDescription);

					if (result.ZZ2_RateFormula != "0")
					{
						result.RefCusRateUOMs = GenerateRefCusRateUomRecords(allUomForTariff, refCusCondition, details.Unit, result.ZZ2_ZY1_NKRateCode);
					}

					return result;
				});

			foreach (var rateToAdd in ratesToAdd)
			{
				yield return rateToAdd;
			}
		}
	}

	static IEnumerable<RefCusRate> GetRefCusRate_RawMaterialCustomsRates(vareListe rawMaterialRates, LandgruppeListe countries, string tariffId, List<string> allUomForTariff, RefCusCondition[] refCusCondition)
	{
		foreach (var rawMaterialRate in from goods in rawMaterialRates?.Goods ?? Array.Empty<AvtaleSats>()
		         where goods.id == tariffId
		         from goodsrates in goods.Rates
		         from rate in goodsrates.Rate
		         select new
		         {
			         goods.Unit,
			         goods.UnitDescription,
			         goodsrates.CountryGroup,
			         rate.RateValue,
			         rate.RateUnit,
			         rate.DateStart,
			         rate.DateEnd
		         }
		        )
		{
			var preference = GetPreferenceCodeForTradeGroup(rawMaterialRate.CountryGroup, countries);

				var rate = ConvertRefCusRateDuty(rawMaterialRate.Unit, rawMaterialRate.UnitDescription, rawMaterialRate.CountryGroup, rawMaterialRate.RateValue, rawMaterialRate.RateUnit, preference, Constants.RateTypes.RawMaterialDutiesRate, Constants.Types.Duty, rawMaterialRate.DateStart, rawMaterialRate.DateEnd);
			if (rate != null)
			{
				if (rate.ZZ2_RateFormula != "0")
				{
					rate.RefCusRateUOMs = GenerateRefCusRateUomRecords(allUomForTariff, refCusCondition, rawMaterialRate.RateUnit, rate.ZZ2_ZY1_NKRateCode);
				}

				yield return rate;
			}
		}
	}

	public static RefCusRateUOM[] GenerateRefCusRateUomRecords(List<string> allUomForTariff, RefCusCondition[] refCusCondition, string unit, string rateCode)
	{
		const string regexGetUOMValueBetweenSquareBrackets = @"(?<=\[).+?(?=\])";
		var refCusRateUOM = new List<RefCusRateUOM>();

		var rateUom = convertRateUnitToUom(unit);
		if (!allUomForTariff.Contains(rateUom))
		{
			refCusRateUOM.Add(new RefCusRateUOM() { ZXG_UOM = rateUom });
		}

		var conditionValues = refCusCondition
			.Where(x => x.ZX1_ZX2_NKConditionType == rateCode)
			.SelectMany(x => x.RefCusConditionValues);

		foreach (var value in conditionValues)
		{
			foreach (var match in Regex.Matches(value.ZX3_Value, regexGetUOMValueBetweenSquareBrackets))
			{
				var potentialNewUomCode = match.ToString();
					if (refCusRateUOM.All(x => x.ZXG_UOM != potentialNewUomCode))
				{
					refCusRateUOM.Add(new RefCusRateUOM() { ZXG_UOM = potentialNewUomCode });
				}
			}
		}

		return refCusRateUOM.ToArray();
	}

	public static RefCusRate ConvertRefCusRateDuty(string unit, string unitDescription, string countryGroup, string ratevalue, string rateUnit, string preference, string rateCode, string rateType, string startDate, string endDate)
	{
		RefCusRate result = null;

		var (startDateOk, startDateDtm) = startDate.TryParseDateTime();
		var (endDateOk, endDateDtm) = endDate.TryParseEndDateTime();
		var (formulaOk, formula, uom) = CreateFormula(rateUnit, ratevalue, rateCode);

		if (startDateOk && endDateOk && startDateDtm <= endDateDtm && !string.IsNullOrEmpty(countryGroup) && formulaOk)
		{
			result = new RefCusRate
			{
				ZZ2_EndDate = endDateDtm,
				ZZ2_RateFormula = formula,
				ZZ2_StartDate = startDateDtm,
				ZZ2_SelectorFormula = rateUnit,
				ZZ2_ZZS_ZZZ_NKDataGrouping = Constants.CountryCodes.Norway,
				ZZ2_ZY1_NKRateCode = rateCode,
				ZZ2_ZY1_ZZR_NKRateType = rateType,
				RefCusApplicabilities = new[]
				{
					new RefCusApplicability()
					{
						ZZT_EndDate = endDateDtm,
						ZZT_StartDate = startDateDtm,
						ZZT_ZZA_NKTradeGroup = countryGroup,
						ZZT_AdditionalCode = rateType != Constants.Types.Duty ? rateCode : string.Empty
					}
				}
			};

			if (!string.IsNullOrEmpty(preference))
			{
				result.ZZ2_ZZS_NKPreference = preference;
			}

			if (!string.IsNullOrEmpty(uom))
			{
				result.RefCusRateUOMs = new[] { new RefCusRateUOM() { ZXG_UOM = uom } };
			}
		}
		else
		{
			TariffParser.ErrorBuilder.AppendLine("Unable to parse Rate code due to empty code, empty description or invalid Dates.");
			TariffParser.ErrorBuilder.AppendLine("DETAILS:");
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Unit code: {0}", unit).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Description: {0}", unitDescription).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Value: {0}", ratevalue).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Unit: {0}", rateUnit).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Countrygroup: {0}", countryGroup).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Start Date: {0}", startDate).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "End Date: {0}", endDate).AppendLine();
		}

		return result;
	}

	public static RefCusRate ConvertRefCusRateExcise(string startDate, string endDate, string rate, string rateIsGivenAsPercentageOfNOKString, string unit, string feeCode, string feeType, string feeDescription)
	{
		bool.TryParse(rateIsGivenAsPercentageOfNOKString, out var rateIsGivenAsPercentageOfNOK);

		var (startDateOk, startDateDtm) = startDate.TryParseDateTime();
		var (endDateOk, endDateDtm) = endDate.TryParseEndDateTime();

		var (formulaOk, formula, _) = CreateFormula(unit, rate, feeCode, rateIsGivenAsPercentageOfNOK ? 100 : 1);
		var derivedFrom = rateIsGivenAsPercentageOfNOK ? Constants.RateGivenInFractionsOfKroner : string.Empty;

		if (startDateOk && endDateOk && startDateDtm <= endDateDtm && formulaOk && !string.IsNullOrEmpty(rate) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(feeCode) && !string.IsNullOrEmpty(feeType))
		{
			return new RefCusRate
			{
				ZZ2_EndDate = endDateDtm,
				ZZ2_RateFormula = formula,
				ZZ2_StartDate = startDateDtm,
				ZZ2_SelectorFormula = unit,
				ZZ2_ZY1_NKRateCode = feeCode,
				ZZ2_ZY1_ZZR_NKRateType = feeType,
				ZZ2_RateFormulaDerivedFrom = derivedFrom,
				RefCusApplicabilities = new[]
				{
					new RefCusApplicability()
					{
						ZZT_EndDate = endDateDtm,
						ZZT_StartDate = startDateDtm,
						ZZT_AdditionalCode = feeCode
					}
				}
			};
		}

		TariffParser.ErrorBuilder.AppendLine("Unable to parse Rate code due to empty code, empty description or invalid Dates.");
		TariffParser.ErrorBuilder.AppendLine("DETAILS:");
		TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Fee code: {0}", feeCode).AppendLine();
		TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Value: {0}", rate).AppendLine();
		TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Unit: {0}", unit).AppendLine();
		TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Fee Type: {0}", feeType).AppendLine();
		TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Fee Type Description: {0}", feeDescription).AppendLine();
		TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Start Date: {0}", startDate).AppendLine();
		TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "End Date: {0}", endDate).AppendLine();
		return null;
	}

	static string convertRateUnitToUom(string unit)
	{
		return unit switch
		{
			"G" => UomCodes.Gram,
			"K" => UomCodes.Kilogram,
			"L" => UomCodes.Liter,
			"M" => UomCodes.Kubikkmeter,
			"N" => UomCodes.MilliLiter,
			"P" => UomCodes.ValueForDuty,
			"S" => UomCodes.AntallEnheter,
			_ => unit
		};
	}

	public static (bool Valid, string Formula, string UOM) CreateFormula(string rateUnit, string rateValue, string rateCode, int divideValueBy = 1)
	{
		var formula = string.Empty;
		var uom = string.Empty;
		var value = decimal.Zero;

		if (rateValue == "0,00")
		{
			formula = "0";
		}
		else
		{
			var numberFormatInfo = new NumberFormatInfo { NumberDecimalSeparator = "," };
			var success = decimal.TryParse(rateValue, numberFormatInfo, out value);
			if (!success || divideValueBy == 0)
			{
				return (false, formula, uom);
			}
			value /= divideValueBy;

			switch (rateUnit)
			{
				case "G":
					formula = $"{value.FormatDecimalValue()} * [{UomCodes.Gram}]";
					uom = UomCodes.Gram;
					break;

				case "K":
					formula = $"{value.FormatDecimalValue()} * [{UomCodes.Kilogram}]";
					uom = UomCodes.Kilogram;
					break;

				case "L":
					formula = $"{value.FormatDecimalValue()} * [{UomCodes.Liter}]";
					uom = UomCodes.Liter;
					break;

				case "M":
					formula = $"{value.FormatDecimalValue()} * [{UomCodes.Kubikkmeter}]";
					uom = UomCodes.Kubikkmeter;
					break;

				case "N":
					formula = $"{value.FormatDecimalValue()} * [{UomCodes.MilliLiter}]";
					uom = UomCodes.MilliLiter;
					break;

				case "P":
					formula = $"{FormatDecimalValue(decimal.Parse(rateValue, numberFormatInfo) / 100)} * {UomCodes.ValueForDuty}";
					break;

				case "S":
					formula = $"{value.FormatDecimalValue()} * [{UomCodes.AntallEnheter}]";
					uom = UomCodes.AntallEnheter;
					break;

				case "":
					formula = $"{value.FormatDecimalValue()}";
					break;
			}
			switch (rateCode)
			{
				case Constants.RateTypes.BV515:
				case Constants.RateTypes.BV516:
				case Constants.RateTypes.BV517:
				case Constants.RateTypes.BV610:
				case Constants.RateTypes.BV620:
				case Constants.RateTypes.BV630:
				case Constants.RateTypes.BV640:
				case Constants.RateTypes.BV650:
				case Constants.RateTypes.OL720:
				case Constants.RateTypes.OL730:
					formula = $"{formula} * [{UomCodes.AlcoholVolumePercentage}]";
					break;
			}
		}

		return (!string.IsNullOrEmpty(formula), formula, uom);
	}

	public static string GetPreferenceCodeForTradeGroup(string tradeGroup, LandgruppeListe land)
	{
		return land.CountryGroup.Where(x => x.CountryGroupCode == tradeGroup).Select(x => x.PreferenceCode).First();
	}

	public static string FormatDecimalValue(this decimal value)
	{
		var format = value % 1 == 0 ? "F0" : string.Empty;
		return value.ToString(format, CultureInfo.InvariantCulture);
	}
}
