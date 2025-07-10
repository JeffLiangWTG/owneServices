using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class ESTariffRatesParser(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : CommonParser(dateTimeProvider)
{
	string RefDbServiceURI { get; } = refDbServiceURI;

	public (string errors, string logs) ConvertToXMLFile(TextReader measuresJson, string outPutFileWithPath)
	{
		var comparison = StringComparison.Ordinal;
		var measuresList = JsonHelper.GetListItemsFromJsonl<TOMeasuresSchema>(measuresJson, ErrorBuilder, handleDates: false, x => string.Equals(x.scope, Constants.CountryCode, comparison) && string.Equals(x.measure_type_id, "ES103", comparison));

		if (measuresList.Count > 0)
		{
			var records = GetRecords(measuresList);

			if (records.Count > 0)
			{
				Helper.ExportToXMLFile(Constants.DataSources.ES_Rates, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, records, UpdateType.Full);
			}
			else
			{
				ErrorBuilder.AppendLine("Unable to load any records from measures data. Details:");
				ErrorBuilder.AppendLine("Number of measure records: " + measuresList.Count);
			}
		}
		else
		{
			ErrorBuilder.AppendLine("Measures load failed. Details:");
			ErrorBuilder.AppendLine("Number of records: " + measuresList.Count);
		}

		return (ErrorBuilder.ToString(), LogBuilder.ToString());
	}

	List<RefCusTariff> GetRecords(List<TOMeasuresSchema> measuresList)
	{
		var tariffAndRates = new Dictionary<string, List<RefCusRate>>();

		foreach (var measure in measuresList)
		{
			var tariffCode = measure.goods_id;

			if (!tariffAndRates.TryGetValue(tariffCode, out var currentRateList))
			{
				currentRateList = new List<RefCusRate>();
				tariffAndRates.Add(tariffCode, currentRateList);
			}

			var startDate = measure.start_date ?? Constants.MinimumDateTime;
			var endDate = measure.end_date?.MidnightToEndOfDay() ?? Constants.MaximumDateTime;
			var newRate = new RefCusRate()
			{
				ZZ2_StartDate = startDate,
				ZZ2_EndDate = endDate,
				ZZ2_RateFormula = GetFormula(measure.components.FirstOrDefault()),
				ZZ2_ZY1_NKRateCode = measure.tax_codes.FirstOrDefault(x => string.Equals(x.scope, Constants.CountryCode, StringComparison.Ordinal))?.tax_code_id,
				RefCusApplicabilities =
				new [] {
					new RefCusApplicability
					{
						ZZT_StartDate = startDate,
						ZZT_EndDate = endDate,
						ZZT_ZZA_NKTradeGroup = measure.geographical_area_id,
					}
				}
			};

			currentRateList.Add(newRate);
		}

		var result = new List<RefCusTariff>();


		foreach (var pair in tariffAndRates)
		{
			var tariffCode = pair.Key;
			var dataGrouping = GetDataGroupingWhenESTariff(RefDbServiceURI, tariffCode);

			result.Add(new RefCusTariff()
			{
				ZZ1_TariffCode = tariffCode,
				ZZ1_ZZZ_NKDataGrouping = dataGrouping,
				RefCusRates = pair.Value.ToArray()
			});
		}

		return result;
	}

	static string GetFormula(TOMeasuresSchema.Component component)
	{
		if (component != null && component.duty_expression_id == PercentageFormula)
		{
			var amount = component.duty_amount ?? 0;
			return amount == 0 ? "0" : $"VFD * {ConvertToPercentage(amount)}";
		}

		return string.Empty;
	}

	static string ConvertToPercentage(decimal amount) => (amount / 100).ToString("0.#####", CultureInfo.InvariantCulture);

	const string PercentageFormula = "01";

	protected override XmlWriterConfiguration XMLWriterConfiguration
	{
		get
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusTariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refCusTariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffType.Import);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusRates, false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffConfiguration);

			var refCusRateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_EndDate, false);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, Constants.RateType.Duty);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_NKPreference, false, Constants.Preference.ErgaOmnesThirdCountryDutyRates);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			refCusRateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusRateConfiguration);

			var refCusApplicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_EndDate, false);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			refCusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, false, Constants.EuropeanUnion);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusApplicabilityConfiguration);

			return writerConfiguration;
		}
	}
}
