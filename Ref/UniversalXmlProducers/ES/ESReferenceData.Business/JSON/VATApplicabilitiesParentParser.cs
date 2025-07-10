using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public abstract class VATApplicabilitiesParentParser(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : JsonTariffParser(dateTimeProvider, refDbServiceURI)
{
	protected List<RefCusTaxOrFeeSchema.TaxOrFeeCodes> taxOrFeeCodesList { set; get; }

	protected override void ProcessAndGetMeasures(List<RefDataRepoModelEntityType> records, List<MeasuresSchema> measures, Dictionary<string, string> footnoteDescriptionsDictionary, string taxOrFeeJsonContent)
	{
		if (taxOrFeeJsonContent != null)
		{
			var taxOrFeeObject = JsonHelper.GetJsonItem<RefCusTaxOrFeeSchema>(new System.IO.StringReader(taxOrFeeJsonContent), ErrorBuilder);
			taxOrFeeCodesList = [.. taxOrFeeObject.value.ToList().Where(x => !string.IsNullOrEmpty(x.ZZF_Code) && x.ZZF_Value != -1)];
		}

		var keyMeasureDictionary = measures.GroupBy(x => x.goods_id).ToDictionary(x => x.Key, x => x.ToList());
		if (keyMeasureDictionary.Count > 0)
		{
			foreach (var measure in keyMeasureDictionary)
			{
				var refTariff = ProcessMeasuresForTariff(measure.Key, measure.Value, footnoteDescriptionsDictionary);
				if (refTariff != null)
				{
					records.Add(refTariff);
				}
			}
		}
	}

	protected RefCusTariff ProcessMeasuresForTariff(string tariffCode, List<MeasuresSchema> measuresSchema, Dictionary<string, string> footnotes)
	{
		if (CheckTariffDataIsValid(tariffCode))
		{
			var dataGrouping = GetDataGroupingWhenESTariff(RefDbServiceURI, tariffCode);

			return new RefCusTariff()
			{
				ZZ1_TariffCode = tariffCode,
				ZZ1_ZZZ_NKDataGrouping = dataGrouping,
				RefCusVATApplicabilities = GetVATApplicabilities(measuresSchema, footnotes),
			};
		}
		return null;
	}

	protected RefCusVATApplicability[] GetVATApplicabilities(List<MeasuresSchema> measuresSchema, Dictionary<string, string> footnotes)
	{
		var applicabilities = new List<RefCusVATApplicability>();
		foreach (var applicability in measuresSchema)
		{
			var duty = applicability.duty;

			if (IsValidDutyToProcess(duty))
			{
				var endDate = applicability.end_date ?? Constants.MaximumDateTime;
				var footnote = applicability.footnotes?.FirstOrDefault() ?? string.Empty;
				var description = string.Empty;
				if (!string.IsNullOrEmpty(footnote))
				{
					description = GetDescription(footnote, duty, footnotes);
				}

				var taxOrFeeCode = GetTaxOrFeeCode(duty);

				if (description != null && !string.IsNullOrEmpty(taxOrFeeCode))
				{
					applicabilities.Add(new RefCusVATApplicability()
					{
						ZX5_StartDate = applicability.start_date,
						ZX5_EndDate = endDate,
						ZX5_ZZF_NKTaxOrFeeCode = taxOrFeeCode,
						ZX5_Description = description
					});
				}
			}
		}
		return applicabilities.ToArray();
	}
	protected abstract bool IsValidDutyToProcess(string duty);

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
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {applicabilityType} Applicability as footnote code not found in the footnotes json. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Footnote: {footnote}");
		}
		if (result != null && result.Length > 0)
		{
			if (result[0] == '-')
			{
				result = result.Substring(1).Trim();
			}
			if (result.Length > 500)
			{
				result = result.Substring(0, 500);
			}
		}
		return result;
	}

	string GetTaxOrFeeCode(string duty)
	{
		string result = null;

		if (taxOrFeeCodesList != null)
		{
			var numberPercentage = double.Parse(duty.Replace("%", ""), CultureInfo.InvariantCulture);
			var value = numberPercentage / 100;

			var taxOrFeeResult = taxOrFeeCodesList.FirstOrDefault(x => x.ZZF_Value == value);
			if (taxOrFeeResult != null)
			{
				result = taxOrFeeResult.ZZF_Code;
			}
		}

		if (result == null)
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {applicabilityType} Applicability as invalid 'duty'. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Duty: {duty}");
		}

		return result;
	}

	protected bool CheckTariffDataIsValid(string code)
	{
		var result = true;
		if (string.IsNullOrEmpty(code) || code.Length != 10)
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {applicabilityType} Applicabilities as missing or invalid 'tariff'. Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Tariff: {code}");
			result = false;
		}
		return result;
	}

	protected abstract bool CheckDutyIsValid(string duty);

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
			codeListConfiguration.IncludeColumn(x => x.RefCusVATApplicabilities, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			var codelistRateConfiguration = new EntityTypeConfiguration<RefCusVATApplicability>(true);
			codelistRateConfiguration.IncludeColumn(x => x.ZX5_EndDate, false);
			codelistRateConfiguration.IncludeColumn(x => x.ZX5_StartDate, false);
			codelistRateConfiguration.IncludeColumn(x => x.ZX5_Description, false);
			codelistRateConfiguration.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, true);
			codelistRateConfiguration.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistRateConfiguration);

			return writerConfiguration;
		}
	}
}
