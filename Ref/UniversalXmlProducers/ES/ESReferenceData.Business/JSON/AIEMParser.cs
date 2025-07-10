using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class AIEMParser(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : JsonTariffParser(dateTimeProvider, refDbServiceURI)
{
	protected override string applicabilityType => "AIEM";

	protected override string dataSource => Constants.DataSources.AIEM;

	protected override void ProcessAndGetMeasures(List<RefDataRepoModelEntityType> records, List<MeasuresSchema> measures, Dictionary<string, string> footnoteDescriptionsDictionary, string taxOrFeeJsonContent)
	{
		if (measures.Count > 0)
		{
			foreach (var measure in measures)
			{
				var refTariff = ProcessMeasuresForKey(measure, footnoteDescriptionsDictionary);
				if (refTariff != null)
				{
					records.Add(refTariff);
				}
			}
		}
	}

	protected override string measureTypeCode => MeasuresConstants.AIEM.AIEMCode;

	protected RefCusTariff ProcessMeasuresForKey(MeasuresSchema measuresSchema, Dictionary<string, string> footnotes)
	{
		var duty = measuresSchema.duty;
		var result = GetFormattedDutyAndRateFormula(duty);
		var formattedDuty = result.formattedDuty;
		if (!string.IsNullOrEmpty(formattedDuty))
		{
			var startDate = measuresSchema.start_date;
			var endDate = measuresSchema.end_date ?? Constants.MaximumDateTime;
			var footnote = measuresSchema.footnotes?.FirstOrDefault() ?? string.Empty;

			var description = string.Empty;
			if (!string.IsNullOrEmpty(footnote))
			{
				description = GetDescription(footnote, duty, footnotes);
			}
			var tariff = measuresSchema.goods_id;
			if (CheckTariffDataIsValid(tariff))
			{
				var rateFormula = result.rateFormula;

				return new RefCusTariff()
				{
					ZZ1_TariffCode = tariff + "_" + formattedDuty,
					ZZ1_Description = string.IsNullOrWhiteSpace(description) ? "Specific rate. No exemptions." : description,
					ZZ1_StartDate = startDate,
					ZZ1_EndDate = endDate,
					RefCusTariffUOMs = GetRefCusTariffUOMs(formattedDuty, rateFormula),
					RefCusRates = [new() { ZZ2_StartDate = startDate, ZZ2_EndDate = endDate, ZZ2_RateFormula = rateFormula }],
					RefCusTariffRelationships = [new() { ZZH_TariffCode = tariff }]
				};
			}
		}
		return null;
	}

	RefCusTariffUOM[] GetRefCusTariffUOMs(string duty, string rateFormula)
	{
		var tariffUOMs = new List<RefCusTariffUOM>();
		var aiemNumber = int.Parse(duty.Remove(0, 4), CultureInfo.InvariantCulture);
		if (aiemNumber > 4)
		{
			var startIndex = rateFormula.IndexOf('[') + 1;
			var endIndex = rateFormula.IndexOf(']');
			if (startIndex != -1 && endIndex != -1)
			{
				var uom = rateFormula.Substring(startIndex, endIndex - startIndex);
				tariffUOMs.Add(new RefCusTariffUOM()
				{
					ZZ8_Type = "CU1",
					ZZ8_UOM = uom
				});
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import TariffUOM as invalid rate formula format. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Rate Formula: {rateFormula}");
			}
		}
		return [.. tariffUOMs];
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

	protected (string formattedDuty, string rateFormula) GetFormattedDutyAndRateFormula(string duty)
	{
		string resultDuty = null;
		string resultRateFormula = null;
		switch (duty)
		{
			case "5 %":
				resultDuty = MeasuresConstants.AIEM.AIEM01;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM01;
				break;
			case "10 %":
				resultDuty = MeasuresConstants.AIEM.AIEM02;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM02;
				break;
			case "15 %":
				resultDuty = MeasuresConstants.AIEM.AIEM03;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM03;
				break;
			case "25 %":
				resultDuty = MeasuresConstants.AIEM.AIEM04;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM04;
				break;
			case "7 EUR KL":
				resultDuty = MeasuresConstants.AIEM.AIEM05;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM05;
				break;
			case "7.5 EUR KL":
				resultDuty = MeasuresConstants.AIEM.AIEM06;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM06;
				break;
			case "8.5 EUR KL":
				resultDuty = MeasuresConstants.AIEM.AIEM07;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM07;
				break;
			case "6.5 EUR KL":
				resultDuty = MeasuresConstants.AIEM.AIEM08;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM08;
				break;
			case "4 EUR TN":
				resultDuty = MeasuresConstants.AIEM.AIEM09;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM09;
				break;
			case "12 EUR TN":
				resultDuty = MeasuresConstants.AIEM.AIEM10;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM10;
				break;
			case "15 % MIN 18 EUR MI":
				resultDuty = MeasuresConstants.AIEM.AIEM11;
				resultRateFormula = MeasuresConstants.AIEM.RateFormulas.AIEM11;
				break;
		}
		if (resultDuty == null)
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {applicabilityType} Relationship as invalid 'duty'. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Duty: {duty}");
		}
		return (resultDuty, resultRateFormula);
	}

	protected string GetDescription(string footnote, string duty, Dictionary<string, string> footnotes)
	{
		string result = null;
		if (footnotes.TryGetValue(footnote, out var descriptions))
		{
			var formattedDuty = duty.Replace(" ", ""); // 10 % -> 10%
			var dutyPosition = descriptions.IndexOf("<b>" + formattedDuty, StringComparison.InvariantCulture);
			if (dutyPosition != -1)
			{
				var startPosition = descriptions.IndexOf("</b>", dutyPosition, StringComparison.InvariantCulture) + 4;

				var nextDuty = descriptions.IndexOf("<b>", startPosition, StringComparison.InvariantCulture);
				var length = (nextDuty == -1) ? (descriptions.Length - startPosition) : (nextDuty - startPosition);

				result = descriptions.Substring(startPosition, length).Replace("<p>", "").Trim();
			}
			else
			{
				result = descriptions.Replace("<p>", "").Replace("<b>", "").Replace("</b>", "");
			}
		}
		else
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {applicabilityType} Relationship as footnote code not found in the footnotes json. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Footnote: {footnote}");
		}
		if (result != null && result.Length > 0)
		{
			if (result[0] == '-')
			{
				result = result[1..].Trim();
			}
			if (result.Length > 500)
			{
				result = result[..500];
			}
		}
		return result;
	}

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
			codeListConfiguration.IncludeColumn(x => x.ZZ1_StartDate, false);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_EndDate, false);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_Description, false);
			codeListConfiguration.IncludeColumn(x => x.RefCusTariffUOMs, false);
			codeListConfiguration.IncludeColumn(x => x.RefCusRates, false);
			codeListConfiguration.IncludeColumn(x => x.RefCusTariffRelationships, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			var codelistUOMConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			codelistUOMConfiguration.IncludeColumn(x => x.ZZ8_Type, true);
			codelistUOMConfiguration.IncludeColumn(x => x.ZZ8_UOM, false);
			codelistUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistUOMConfiguration);

			var codelistRateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			codelistRateConfiguration.IncludeColumn(x => x.ZZ2_EndDate, false);
			codelistRateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_NKRateCode, true, MeasuresConstants.AIEM.AIEMRateCode);
			codelistRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, "MSC");
			writerConfiguration.IncludeEntityTypeConfiguration(codelistRateConfiguration);

			var codelistRelationshipConfiguration = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			codelistRelationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_NKTariffType, true, "IMP");
			codelistRelationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			codelistRelationshipConfiguration.IncludeColumn(x => x.ZZH_TariffCode, true);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistRelationshipConfiguration);

			return writerConfiguration;
		}
	}
}
