using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class REAParser(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : CommonParser(dateTimeProvider)
{
	string RefDbServiceURI { get; } = refDbServiceURI;

	List<RefCusMapUOMSchema.MapCodes> UOMMapCodesList { set; get; }

	public string ConvertToXMLFile(string uomJson, string measuresJsonContent, string measuresCodesJsonContent, string outPutFileWithPath, string outPutFileWithPathCodeList)
	{
		ErrorBuilder.Clear();

		var uomsObject = JsonHelper.GetJsonItem<RefCusMapUOMSchema>(new System.IO.StringReader(uomJson), ErrorBuilder);
		UOMMapCodesList = uomsObject != null ? [.. uomsObject.value.Where(x => !string.IsNullOrEmpty(x.ZZM_CW1orCommercialValue) && !string.IsNullOrEmpty(x.ZZM_CustomsValue))] : [];

		if (UOMMapCodesList.Count > 0)
		{
			var (resultREA, resultCodeList) = GetRecordsToExport(JsonHelper.ToJsonl(measuresJsonContent), JsonHelper.ToJsonl(measuresCodesJsonContent));
			if (resultREA.Count > 0 && resultCodeList.Count > 0)
			{
				Helper.ExportToXMLFile(Constants.DataSources.REA_Rates, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, resultREA);
				Helper.ExportToXMLFile(Constants.DataSources.REA_Codes, outPutFileWithPathCodeList, XMLWriterConfigurationCodeList, DateTimeProvider.CurrentLocalDate, resultCodeList);
			}
			else
			{
				ErrorBuilder.AppendLine("Unable to locate any records for REA Codes.");
			}
		}
		else
		{
			ErrorBuilder.AppendLine("Unable to load UOMMap records.");
		}

		return ErrorBuilder.ToString();
	}

	(List<RefCusTariff> refCusTariffs, List<RefCusCodeList> refCusCodeLists) GetRecordsToExport(string measuresJsonContent, string measuresCodesJsonContent)
	{
		var resultREA = new List<RefCusTariff>();
		var resultCodeList = new List<RefCusCodeList>();

		var measures = JsonHelper.GetListItemsFromJsonl<MeasuresSchema>(measuresJsonContent, ErrorBuilder, handleDates: true, x =>
		{
			var type = x.measure_type;
			var comparisonType = StringComparison.Ordinal;
			return type.Equals(Constants.REA.REACodeForTransformation, comparisonType) || type.Equals(Constants.REA.REACodeForDirectComsuption, comparisonType);
		});
		var measuresCodes = JsonHelper.GetListItemsFromJsonl<MeasuresCodesSchema>(measuresCodesJsonContent, ErrorBuilder);
		var continueProcessing = true;
		if (measures.Count == 0)
		{
			ErrorBuilder.Append("Error in Measures processing. Details: Item count is 0.");
			continueProcessing = false;
		}
		if (measuresCodes.Count == 0)
		{
			ErrorBuilder.Append("Error in Measures Codes processing. Details: Item count is 0.");
			continueProcessing = false;
		}
		if (continueProcessing)
		{
			var recordsREA = GetRecordsFromMeasures(measures, measuresCodes);
			if (recordsREA.Count > 0)
			{
				foreach (var record in recordsREA)
				{
					var tariff = record.TariffCode;
					var rates = record.REACodes;
					if (CheckTariffDataIsValid(tariff, rates))
					{
						var tariffCodesToExport = GetTariffCodesFromParentTariff(tariff);
						var refCusRatesToApply = GetRates(rates);

						foreach (var tariffCode in tariffCodesToExport)
						{
							var dataGrouping = GetDataGroupingWhenESTariff(RefDbServiceURI, tariffCode);

							resultREA.Add(new RefCusTariff
							{
								ZZ1_TariffCode = tariffCode,
								ZZ1_ZZZ_NKDataGrouping = dataGrouping,
								RefCusRates = refCusRatesToApply
							});
						}
						resultCodeList.AddRange(rates.Select(r => new RefCusCodeList { ZZD_Code = r.Code, ZZD_Description = r.Description }));
					}
				}
			}
		}

		return (resultREA, resultCodeList);
	}

	string[] GetTariffCodesFromParentTariff(string tariff)
	{
		var parentTariff = tariff[..^2];

		var tariffCodesJsonContent = Services.DownloadJson.Download(GetQueryUrlForTariffCode(RefDbServiceURI, parentTariff, false, false).AbsoluteUri);
		var tariffCodeSchema = JsonHelper.GetJsonItem<RefCusTariffCodeSchema>(new System.IO.StringReader(tariffCodesJsonContent), ErrorBuilder);

		return TariffHelper.GetTariffCodesArray(tariffCodeSchema, ErrorBuilder);
	}

	List<REATariff> GetRecordsFromMeasures(List<MeasuresSchema> measures, List<MeasuresCodesSchema> measuresCodes)
	{
		var result = new List<REATariff>();
		var codeMeasuresDictionary = measures.GroupBy(x => x.goods_id).ToDictionary(x => x.Key, x => x.ToList());
		var codesDescriptionsDictionary = GetCodesDescriptions(codeMeasuresDictionary, measuresCodes);

		foreach (var tariffAndReaCodes in codeMeasuresDictionary)
		{
			var tariff = new REATariff(tariffAndReaCodes.Key);
			var codeAidsDictionary = new Dictionary<string, Dictionary<string, string>>();
			foreach (var reaCode in tariffAndReaCodes.Value)
			{
				var code = reaCode.codadd;
				var aidType = reaCode.measure_type;

				var formula = GetFormula(reaCode.conditions);
				if (!codeAidsDictionary.TryGetValue(code, out var value))
				{
					value = [];
					codeAidsDictionary.Add(code, value);
				}

				value.Add(aidType, formula);
			}

			foreach (var codeAndAids in codeAidsDictionary)
			{
				var code = codeAndAids.Key;
				var aidAmountForDirectConsumption = string.Empty;
				var aidAmountForTransformation = string.Empty;
				codeAndAids.Value.TryGetValue(Constants.REA.REACodeForDirectComsuption, out aidAmountForDirectConsumption);
				codeAndAids.Value.TryGetValue(Constants.REA.REACodeForTransformation, out aidAmountForTransformation);
				var description = string.Empty;
				codesDescriptionsDictionary.TryGetValue(code, out description);
				if (CheckRateDataIsValid(aidAmountForDirectConsumption, aidAmountForTransformation, code, description))
				{
					var rea = new REACodes(code, aidAmountForDirectConsumption, aidAmountForTransformation, description);
					tariff.REACodes.Add(rea);
				}
			}
			result.Add(tariff);
		}
		return result;
	}

	static string GetFormula(string conditions)
	{
		static int Get3rdIndex(string s, char t)
		{
			int count = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == t)
				{
					count++;
					if (count == 3)
					{
						return i;
					}
				}
			}
			return -1;
		}
		var startIndex = Get3rdIndex(conditions, LeftBoundaryExpressionSeparator);
		var endIndex = conditions.IndexOf(RightBoundaryExpressionSeparator, StringComparison.InvariantCulture);
		var result = string.Empty;
		if (startIndex != -1 && endIndex != -1)
		{
			startIndex += 1;
			result = conditions[startIndex..endIndex];
		}

		return result;
	}
	const string RightBoundaryExpressionSeparator = "; C (07)";
	const char LeftBoundaryExpressionSeparator = ':';

	Dictionary<string, string> GetCodesDescriptions(Dictionary<string, List<MeasuresSchema>> codeMeasuresDictionary, List<MeasuresCodesSchema> descriptions)
	{
		var result = new Dictionary<string, string>();
		var filter = new HashSet<string>();
		foreach (var listMeasures in codeMeasuresDictionary)
		{
			foreach (var measuresSchema in listMeasures.Value)
			{
				filter.Add(measuresSchema.codadd);
			}
		}

		foreach (var measuresCodesSchema in descriptions)
		{
			try
			{
				var code_id = measuresCodesSchema.additional_code_id;
				if (filter.Contains(code_id) && !result.ContainsKey(code_id))
				{
					var description = measuresCodesSchema.descriptions.FirstOrDefault()?.text ?? string.Empty;
					if (description.Length > 500)
					{
						description = description[..500];
					}
					result.Add(code_id, description);
				}
			}
			catch (ArgumentException e)
			{
				ErrorBuilder.AppendLine(e.Message.Replace("\r\n", ""));
			}
		}
		return result;
	}

	RefCusRate[] GetRates(List<REACodes> rates)
	{
		var refCusRates = new List<RefCusRate>();
		foreach (var rate in rates)
		{
			var rateCode = rate.Code;
			AddRefCusRate(refCusRates, GetCW1Formula(rate.AidAmountForDirectConsumption), REAConstants.DirectConsumption, rateCode);
			AddRefCusRate(refCusRates, GetCW1Formula(rate.AidAmountForTransformation), REAConstants.Transformation, rateCode);
		}
		return refCusRates.ToArray();
	}

	void AddRefCusRate(List<RefCusRate> cusRates, string formula, string rateCode, string additionalCode)
	{
		var date = DateTimeProvider.CurrentLocalDate;
		cusRates.Add(new RefCusRate()
		{
			ZZ2_RateFormula = formula,
			ZZ2_StartDate = date,
			ZZ2_ZY1_NKRateCode = rateCode,
			ZZ2_ZY1_ZZR_NKRateType = REAConstants.REA,
			RefCusApplicabilities = [new RefCusApplicability() { ZZT_AdditionalCode = additionalCode, ZZT_StartDate = date }]
		});
	}

	string GetCW1Formula(string jsonFormula) => FormulaFormatter.ParseJsonFormula(jsonFormula, UOMMapCodesList);

	bool CheckTariffDataIsValid(string code, List<REACodes> reaCodes)
	{
		var result = true;
		if (string.IsNullOrEmpty(code) || code.Length != TariffCodeMaxLength || reaCodes.Count == 0)
		{
			ErrorBuilder.AppendLine("Unable to import REA Codes as missing or invalid 'tariff' or missing REA Codes for the current tariff code. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Tariff: {code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"REA codes: {reaCodes.Count}");
			result = false;
		}
		return result;
	}
	const int TariffCodeMaxLength = 10;

	bool CheckRateDataIsValid(string aidAmountDirect, string aidAmountTransformation, string rateCode, string description)
	{
		var result = true;
		var aidDirect = aidAmountDirect ?? string.Empty;
		var aidTransformation = aidAmountTransformation ?? string.Empty;

		var indexD = aidDirect.IndexOf(FormulaFormatter.FormulaKeys.Specific, StringComparison.InvariantCulture);
		var indexT = aidTransformation.IndexOf(FormulaFormatter.FormulaKeys.Specific, StringComparison.InvariantCulture);
		if (indexD > 0 && indexT > 0)
		{
			aidDirect = aidDirect.Substring(0, indexD);
			aidTransformation = aidTransformation[..indexT];
		}

		if (!float.TryParse(aidDirect, out float x) || !float.TryParse(aidTransformation, out float x2) || string.IsNullOrEmpty(rateCode) || string.IsNullOrEmpty(description))
		{
			ErrorBuilder.AppendLine("Unable to import REA Codes as missing or invalid 'Aid amount' or missing 'REA Code' or 'Description'. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Aid amount for Direct Consumption: {aidAmountDirect}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Aid amount for Transformation: {aidAmountTransformation}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"REA code: {rateCode}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {description}");
			result = false;
		}
		return result;
	}

	protected override XmlWriterConfiguration XMLWriterConfiguration
	{
		get
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, "IMP");
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			codeListConfiguration.IncludeColumn(x => x.RefCusRates, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			var codelistRateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			codelistRateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			codelistRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, Constants.MaximumDateTime);
			codelistRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			codelistRateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
			codelistRateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			codelistRateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistRateConfiguration);

			var codelistApplicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			codelistApplicabilityConfiguration.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			codelistApplicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, Constants.MaximumDateTime);
			codelistApplicabilityConfiguration.IncludeColumn(x => x.ZZT_OrderNumber, true);
			codelistApplicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistApplicabilityConfiguration);

			return writerConfiguration;
		}
	}

	protected static XmlWriterConfiguration XMLWriterConfigurationCodeList
	{
		get
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, REAConstants.REA);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			return writerConfiguration;
		}
	}
}
