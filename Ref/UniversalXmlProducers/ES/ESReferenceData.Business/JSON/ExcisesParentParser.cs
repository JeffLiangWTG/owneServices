using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public abstract class ExcisesParentParser(IDateTimeProvider dateTimeProvider) : CommonParser(dateTimeProvider)
{
	protected abstract string applicabilityType { get; }
	protected abstract string dataSource { get; }
	protected List<RefCusMapUOMSchema.MapCodes> UOMMapCodesList { set; get; }

	public string ConvertToXMLFile(string uomJson, string measuresJsonContent, string measuresCodesJsonContent, string exciseJsonContent, string outPutFileWithPath)
	{
		ErrorBuilder.Clear();

		var uomsObject = JsonHelper.GetJsonItem<RefCusMapUOMSchema>(new System.IO.StringReader(uomJson), ErrorBuilder);
		UOMMapCodesList = [.. uomsObject.value.Where(x => !string.IsNullOrEmpty(x.ZZM_CW1orCommercialValue) && !string.IsNullOrEmpty(x.ZZM_CustomsValue))];

		if (UOMMapCodesList.Count > 0)
		{
			var records = GetRecordsToExport(JsonHelper.ToJsonl(measuresJsonContent), JsonHelper.ToJsonl(measuresCodesJsonContent), exciseJsonContent);
			if (records.Count > 0)
			{
				Helper.ExportToXMLFile(dataSource, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, records, UpdateType.Full);
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to locate {applicabilityType} records");
			}
		}
		else
		{
			ErrorBuilder.AppendLine("Schema load failed. Unable to load UOMMapCodeList.");
		}
		return ErrorBuilder.ToString();
	}

	List<RefDataRepoModelEntityType> GetRecordsToExport(string measuresJsonContent, string measuresCodesJsonContent, string exciseJsonContent)
	{
		var records = new List<RefDataRepoModelEntityType>();
		if (!measuresJsonContent.IsNullOrEmpty())
		{
			var measures = JsonHelper.GetListItemsFromJsonl<MeasuresSchema>(measuresJsonContent, ErrorBuilder, handleDates: true, x => x.measure_type.StartsWith(measureCode, StringComparison.InvariantCulture));
			var measuresCodes = JsonHelper.GetListItemsFromJsonl<MeasuresCodesSchema>(measuresCodesJsonContent, ErrorBuilder);
			var pvpResults = JsonHelper.GetJsonItem<ExciseBaseTypeSchema>(new System.IO.StringReader(exciseJsonContent), ErrorBuilder)?.pvp?.ToArray() ?? [];
			var continueProcessing = true;

			if (measuresCodes.Count == 0)
			{
				ErrorBuilder.AppendLine("Error in Measures Codes processing. Details: item count is 0.");
				continueProcessing = false;
			}
			if (pvpResults.Length == 0)
			{
				ErrorBuilder.AppendLine("Error in Excise Base Type processing. Details: Required properties are missing from object: pvp.");
				continueProcessing = false;
			}
			if (continueProcessing && measures != null)
			{
				ProcessAndGetMeasures(records, measures, measuresCodes, pvpResults);
			}
		}
		else
		{
			ErrorBuilder.AppendLine("Error in Measures processing. Json content is empty.");
		}
		return records;
	}

	Dictionary<string, string> GetCodesDescriptions(Dictionary<string, List<MeasuresSchema>> codeMeasuresDictionary, List<MeasuresCodesSchema> descriptions)
	{
		var result = new Dictionary<string, string>();
		var filter = new HashSet<string>();
		foreach (var listMeasures in codeMeasuresDictionary)
		{
			var measuresSchema = listMeasures.Value.First();
			filter.Add(measuresSchema.codadd);
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
					if (CheckCodeDescriptionAreValid(code_id, description))
					{
						result.Add(code_id, description);
					}
				}
			}
			catch (ArgumentException e)
			{
				ErrorBuilder.AppendLine(e.Message.Replace("\r\n", ""));
			}
		}
		return result;
	}

	void ProcessAndGetMeasures(List<RefDataRepoModelEntityType> records, List<MeasuresSchema> measures, List<MeasuresCodesSchema> measuresCodes, string[] exciseResults)
	{
		var codeMeasuresDictionary = measures.GroupBy(x => x.codadd).ToDictionary(x => x.Key, x => x.ToList());
		var codesDescriptionsDictionary = GetCodesDescriptions(codeMeasuresDictionary, measuresCodes);
		if (codeMeasuresDictionary.Count > 0)
		{
			foreach (var codeMeasures in codeMeasuresDictionary)
			{
				var tariffCode = codeMeasures.Key;
				codesDescriptionsDictionary.TryGetValue(tariffCode, out string description);
				if (CheckCodeDescriptionAreValid(tariffCode, description ?? string.Empty))
				{
					var refTariff = ProcessMeasures(tariffCode, codeMeasures.Value, description, exciseResults);
					if (refTariff != null)
					{
						records.Add(refTariff);
					}
				}
			}
		}
	}

	RefCusTariff ProcessMeasures(string tariffCode, List<MeasuresSchema> measures, string description, string[] exciseResults)
	{
		var measure = measures.First();
		var duty = measure.duty;
		var rateFormula = FormulaFormatter.ParseJsonFormula(duty, UOMMapCodesList, !exciseResults.Contains(tariffCode));
		RefCusTariff result = null;
		if (!string.IsNullOrEmpty(rateFormula))
		{
			if (tariffCode.Equals(FluorinatedGasesCode, StringComparison.Ordinal))
			{
				rateFormula = RateFormulaFor1CF;
			}

			result = new RefCusTariff()
			{
				ZZ1_TariffCode = tariffCode,
				ZZ1_Description = description,
				RefCusTariffUOMs = GetRefCusTariffUOMs(duty),
				RefCusRates = new RefCusRate[] { new RefCusRate() { ZZ2_ZY1_NKRateCode = tariffCode, ZZ2_RateFormula = rateFormula } },
				RefCusTariffRelationships = GetTariffRelationships(measures)
			};
		}
		else
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import Tariff as invalid Rate Formula, format has changed {duty}");
		}
		return result;
	}

	const string FluorinatedGasesCode = "1CF";

	const string RateFormulaFor1CF = "IF(PCA=0, 100*[GF], MIN(0.015*PCA*[GF], 100*[GF]))";

	RefCusTariffUOM[] GetRefCusTariffUOMs(string duty)
	{
		var tariffUOMs = new List<RefCusTariffUOM>();

		if (duty.Any(x => char.IsLetter(x)))
		{
			var uom = FormulaFormatter.GetUOM(duty.Substring(Math.Max(0, duty.Length - 2)), UOMMapCodesList);
			if (!string.IsNullOrEmpty(uom))
			{
				tariffUOMs.Add(new RefCusTariffUOM() { ZZ8_UOM = uom });
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {applicabilityType} RefCusTariffUOM as invalid 'duty'. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Duty: {duty}");
			}
		}

		return [.. tariffUOMs];
	}

	RefCusTariffRelationship[] GetTariffRelationships(List<MeasuresSchema> measures)
	{
		var result = new List<RefCusTariffRelationship>();
		foreach (var measure in measures)
		{
			var tariff = measure.goods_id;

			if (CheckTariffDataIsValid(tariff))
			{
				result.Add(new RefCusTariffRelationship() { ZZH_TariffCode = tariff });
			}
		}
		return [.. result];
	}

	protected abstract string measureCode { get; }

	protected override XmlWriterConfiguration XMLWriterConfiguration
	{
		get
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codeListConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, applicabilityType);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_StartDate, false, Constants.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_EndDate, false, Constants.MaximumDateTime);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_Description, false);
			codeListConfiguration.IncludeColumn(x => x.RefCusTariffUOMs, false);
			codeListConfiguration.IncludeColumn(x => x.RefCusRates, false);
			codeListConfiguration.IncludeColumn(x => x.RefCusTariffRelationships, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			var codelistUOMConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			codelistUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_Type, true, "CU1");
			codelistUOMConfiguration.IncludeColumn(x => x.ZZ8_UOM, false);
			codelistUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistUOMConfiguration);

			var codelistRateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_EndDate, false, Constants.MaximumDateTime);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_StartDate, false, Constants.MinimumDateTime);
			codelistRateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			codelistRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, "EXC");
			writerConfiguration.IncludeEntityTypeConfiguration(codelistRateConfiguration);

			var codelistRelationshipConfiguration = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			codelistRelationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_NKTariffType, true, "IMP");
			codelistRelationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);//Change this to constant as AIEM
			codelistRelationshipConfiguration.IncludeColumn(x => x.ZZH_TariffCode, true);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistRelationshipConfiguration);

			return writerConfiguration;
		}
	}

	protected bool CheckTariffDataIsValid(string code)
	{
		var result = true;
		if (string.IsNullOrEmpty(code) || code.Length != 10)
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {applicabilityType} Relationship as missing or invalid 'tariff'. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Tariff: {code}");
			result = false;
		}
		return result;
	}

	protected bool CheckCodeDescriptionAreValid(string code, string description)
	{
		var result = true;
		if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(description))
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {applicabilityType} RateCode as missing or invalid 'code' or 'description'");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {description}");
			result = false;
		}
		return result;
	}
}
