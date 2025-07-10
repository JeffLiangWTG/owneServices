using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class QuotaParser(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : CommonParser(dateTimeProvider)
{
	string RefDbServiceURI { get; } = refDbServiceURI;

	List<RefCusMapUOMSchema.MapCodes> UOMMapCodesList { set; get; }

	public string ConvertToXMLFile(string uomJson, string measuresJsonContent, string footnotesJsonContent, string outPutFileWithPath)
	{
		ErrorBuilder.Clear();

		var uomsObject = JsonHelper.GetJsonItem<RefCusMapUOMSchema>(new System.IO.StringReader(uomJson), ErrorBuilder);
		UOMMapCodesList = uomsObject != null ? [.. uomsObject.value.Where(x => !string.IsNullOrEmpty(x.ZZM_CW1orCommercialValue) && !string.IsNullOrEmpty(x.ZZM_CustomsValue))] : [];

		if (UOMMapCodesList.Count > 0)
		{
			var result = GetRecordsToExport(JsonHelper.ToJsonl(measuresJsonContent), JsonHelper.ToJsonl(footnotesJsonContent));
			if (result.Count > 0)
			{
				Helper.ExportToXMLFile(Constants.DataSources.Quota, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, result);
			}
			else
			{
				ErrorBuilder.AppendLine("Unable to locate Quota records.");
			}
		}
		else
		{
			ErrorBuilder.AppendLine("Unable to load UOMMap records.");
		}

		return ErrorBuilder.ToString();
	}

	List<RefDataRepoModelEntityType> GetRecordsToExport(string measuresJsonContent, string footnotesJsonContent)
	{
		var resultQuota = new List<RefDataRepoModelEntityType>();
		var continueProcessing = true;

		if (string.IsNullOrEmpty(footnotesJsonContent))
		{
			continueProcessing = false;
			ErrorBuilder.AppendLine("Measures process schema. Json is null or empty.");
		}
		if (string.IsNullOrEmpty(footnotesJsonContent))
		{
			continueProcessing = false;
			ErrorBuilder.AppendLine("Footnotes process schema. Json is null or empty.");
		}
		if (!continueProcessing)
		{
			return resultQuota;
		}
		var measures = JsonHelper.GetListItemsFromJsonl<MeasuresSchema>(measuresJsonContent, ErrorBuilder, handleDates: true, x => x.measure_type.StartsWith(Constants.Quota.MeasureTypeCode, StringComparison.InvariantCulture));
		var footnoteDescriptions = JsonHelper.GetListItemsFromJsonl<FootnoteSchema>(footnotesJsonContent, ErrorBuilder);

		if (measures.Count == 0 ||  footnoteDescriptions.Count == 0)
		{
			ErrorBuilder.AppendLine("Unable to parse any record for Measures or Footnotes.");
			return resultQuota;
		}

		var footnoteDescriptionsDictionary = new Dictionary<string, string>();

		static string GetDictionaryDescription(IEnumerable<FootnoteSchema.Description> descriptions)
			=> descriptions.FirstOrDefault(x => string.Equals(x.lang, "en", StringComparison.OrdinalIgnoreCase))?.text ?? descriptions.FirstOrDefault(x => string.Equals(x.lang, "es", StringComparison.OrdinalIgnoreCase))?.text ?? string.Empty;

		foreach (var footnotes in footnoteDescriptions)
		{
			var footnote_id = footnotes.footnote_id;
			if (!footnoteDescriptionsDictionary.ContainsKey(footnote_id))
			{
				footnoteDescriptionsDictionary.Add(footnotes.footnote_id, GetDictionaryDescription(footnotes.descriptions));
			}
		}
		var tariffsProcessed = new HashSet<string>();

		foreach (var measure in measures)
		{
			var tariff = measure.goods_id;
			var duty = measure.duty;
			if (CheckTariffDataIsValid(tariff, duty) && CheckTariffIsNotDuplicated(tariffsProcessed, tariff))
			{
				var applicabilities = NewRefCusApplicability(measure);
				var rates = NewRefCusRate(measure, applicabilities);

				var footnote = measure.footnotes?.FirstOrDefault() ?? string.Empty;
				var conditions = NewRefCusCondition(measure, GetDescription(footnote, footnoteDescriptionsDictionary), applicabilities);

				var code = tariff[..4];
				var tariffCodesJsonContent = Services.DownloadJson.Download(GetQueryUrlForTariffCode(RefDbServiceURI, code, false, false).AbsoluteUri);
				var tariffCodesObject = JsonHelper.GetJsonItem<RefCusTariffCodeSchema>(new System.IO.StringReader(tariffCodesJsonContent), ErrorBuilder);
				var tariffCodesToExport = TariffHelper.GetTariffCodesArray(tariffCodesObject, ErrorBuilder);

				foreach (var tariffCode in tariffCodesToExport)
				{
					var dataGrouping = GetDataGroupingWhenESTariff(RefDbServiceURI, tariffCode);

					resultQuota.Add(new RefCusTariff()
					{
						ZZ1_TariffCode = tariffCode,
						ZZ1_ZZZ_NKDataGrouping = dataGrouping,
						RefCusRates = rates,
						RefCusConditions = conditions
					});
				}
			}
		}

		return resultQuota;
	}

	bool CheckTariffIsNotDuplicated(HashSet<string> tariffsProcessed, string tariff)
	{
		if (tariffsProcessed.Add(tariff))
		{
			return true;
		}
		else
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Cannot process duplicated info for the code: {tariff}");
			return false;
		}
	}

	static RefCusApplicability[] NewRefCusApplicability(MeasuresSchema m)
		=> [new(){ ZZT_StartDate = m.start_date, ZZT_ZZA_NKTradeGroup = m.geographical_area, ZZT_AdditionalCode = m.codadd, ZZT_OrderNumber = m.order_number_id }];

	RefCusRate[] NewRefCusRate(MeasuresSchema m, RefCusApplicability[] applicabilities)
		=> [new() { ZZ2_StartDate = m.start_date, ZZ2_RateFormula = FormulaFormatter.ParseJsonFormula(m.duty, UOMMapCodesList), RefCusApplicabilities = applicabilities }];

	static RefCusCondition[] NewRefCusCondition(MeasuresSchema m, string description, RefCusApplicability[] applicabilities)
		=> [new() { ZX1_StartDate = m.start_date, ZX1_Comment = description, RefCusApplicabilities = applicabilities }];

	string GetDescription(string footnote, Dictionary<string, string> footnotes)
	{
		string result = string.Empty;
		if (footnotes.TryGetValue(footnote, out var description))
		{
			result = description.Replace("<p>", "").Replace("<b>", "").Replace("</b>", "");
		}
		else
		{
			ErrorBuilder.AppendLine("Unable to import Quota as footnote code not found in the footnotes json. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Footnote: {footnote}");
		}
		if (result.Length > 500)
		{
			result = result[..500];
		}
		return result;
	}

	bool CheckTariffDataIsValid(string code, string duty)
	{
		var result = true;
		var dutyNumber = duty.Replace("%", string.Empty);
		if (string.IsNullOrEmpty(code) || code.Length != 10 || string.IsNullOrEmpty(duty) || !double.TryParse(dutyNumber, out double _))
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import Quota as missing or invalid 'tariff' or 'duty'. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Tariff: {code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Duty: {duty}");
			result = false;
		}
		return result;
	}

	protected override XmlWriterConfiguration XMLWriterConfiguration
	{
		get
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refCusTariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffType.Import);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			refCusTariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusRates, false);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusConditions, false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffConfiguration);

			var refCusRateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_EndDate, false, Constants.MaximumDateTime);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_NKRateCode, true, Constants.RateCode.CustomDutiesOnIndustrialProducts);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, Constants.RateType.Duty);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_NKPreference, false, Constants.Preference.GoodsCoveredByQuota);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			refCusRateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusRateConfiguration);

			var refCusApplicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_OrderNumber, true);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, false);
			refCusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, false, Constants.EuropeanUnion);
			refCusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_EndDate, false, Constants.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusApplicabilityConfiguration);

			var refCusConditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			refCusConditionConfiguration.IncludeColumn(x => x.ZX1_StartDate, false);
			refCusConditionConfiguration.IncludeColumn(x => x.ZX1_Comment, false);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_NKConditionType, true, Constants.ConditionType.QuotaSuspensions);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, false, Constants.CountryCode);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_EndDate, false, Constants.MaximumDateTime);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_Source, false, Constants.CountryCode);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, false, Constants.CountryCode);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsImport, false, true);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsExport, false, false);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ConditionValueTrueMeansStop, false, false);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_LogicalANDWithinGroup, false, 0);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZS_NKPreference, false, Constants.Preference.GoodsCoveredByQuota);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZS_ZZZ_NKDataGrouping, false, Constants.CountryCode);
			refCusConditionConfiguration.IncludeColumn(x => x.RefCusApplicabilities, false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionConfiguration);

			return writerConfiguration;
		}
	}
}
