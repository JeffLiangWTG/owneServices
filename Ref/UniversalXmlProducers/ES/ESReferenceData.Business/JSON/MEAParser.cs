using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class MEAParser(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : CommonParser(dateTimeProvider)
{
	protected string RefDbServiceURI { get; } = refDbServiceURI;

	protected List<RefCusMapUOMSchema.MapCodes> UOMMapCodesList { set; get; }

	public string ConvertToXMLFile(string uomJson, string measuresJsonContent, string outPutFileWithPath)
	{
		ErrorBuilder.Clear();

		var uomsObject = JsonHelper.GetJsonItem<RefCusMapUOMSchema>(new System.IO.StringReader(uomJson), ErrorBuilder);
		UOMMapCodesList = uomsObject != null ? [.. uomsObject.value.Where(x => !string.IsNullOrEmpty(x.ZZM_CW1orCommercialValue) && !string.IsNullOrEmpty(x.ZZM_CustomsValue))] : [];

		if (UOMMapCodesList.Count > 0)
		{
			var result = GetRecordsToExport(JsonHelper.ToJsonl(measuresJsonContent));
			if (result.Count > 0)
			{
				Helper.ExportToXMLFile(Constants.DataSources.MEA, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, result);
			}
			else
			{
				ErrorBuilder.AppendLine("Unable to locate any records for MEA Codes.");
			}
		}
		else
		{
			ErrorBuilder.AppendLine("Unable to load UOMMap records.");
		}

		return ErrorBuilder.ToString();
	}

	List<RefDataRepoModelEntityType> GetRecordsToExport(string measuresJsonContent)
	{
		var resultMEA = new List<RefDataRepoModelEntityType>();

		var measures = JsonHelper.GetListItemsFromJsonl<MeasuresSchema>(measuresJsonContent, ErrorBuilder, handleDates: true, x => x.measure_type.Equals(Constants.MEA.MeasureTypeCode, StringComparison.Ordinal));

		if (measures.Count == 0)
		{
			ErrorBuilder.AppendLine("Error in Measures processing. Details: Item count is 0.");
			return resultMEA;
		}

		var conditionDescriptionsList = GetConditionDescriptionList();

		var tariffsProcessed = new HashSet<string>();

		foreach (var measure in measures)
		{
			var tariff = measure.goods_id;
			var duty = measure.duty;
			if (CheckTariffDataIsValid(tariff, duty) && CheckTariffIsNotDuplicated(tariffsProcessed, tariff))
			{
				var startDate = measure.start_date;
				var applicabilities = new RefCusApplicability[]
				{
					new()
					{
						ZZT_StartDate = startDate,
						ZZT_ZZA_NKTradeGroup = measure.geographical_area,
						ZZT_AdditionalCode = measure.codadd,
						ZZT_OrderNumber = measure.order_number_id
					}
				};
				var rate = new RefCusRate() { ZZ2_StartDate = startDate, ZZ2_RateFormula = FormulaFormatter.ParseJsonFormula(duty, UOMMapCodesList), RefCusApplicabilities = applicabilities };

				var conditionsToApply = GetConditions(measure, conditionDescriptionsList, applicabilities);

				var code = TariffHelper.RemoveLastPairOfZeros(tariff);
				var tariffCodesJsonContent = Services.DownloadJson.Download(GetQueryUrlForTariffCode(RefDbServiceURI, code, false, false).AbsoluteUri);
				var tariffCodeSchema = JsonHelper.GetJsonItem<RefCusTariffCodeSchema>(new System.IO.StringReader(tariffCodesJsonContent), ErrorBuilder);

				var tariffCodesToExport = TariffHelper.GetTariffCodesArray(tariffCodeSchema, ErrorBuilder);

				foreach (var tariffCode in tariffCodesToExport)
				{
					var dataGrouping = GetDataGroupingWhenESTariff(RefDbServiceURI, tariffCode);

					resultMEA.Add(new RefCusTariff()
					{
						ZZ1_TariffCode = tariffCode,
						ZZ1_ZZZ_NKDataGrouping = dataGrouping,
						RefCusRates = [rate],
						RefCusConditions = conditionsToApply
					});
				}
			}
		}

		return resultMEA;
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

	static Dictionary<string, string> GetConditionDescriptionList()
	{
		var conditionDescriptionsList = new Dictionary<string, string>();
		using (var listStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.ESReferenceData.Business.JSON.MeasureConditionsList.xml"))
		using (var reader = XmlReader.Create(listStream))
		{
			var xmlDocument = XDocument.Load(reader);
			foreach (XElement item in xmlDocument.Descendants("item"))
			{
				conditionDescriptionsList.Add(item.Element("code").Value, item.Element("description").Value);
			}
		}
		return conditionDescriptionsList;
	}

	RefCusCondition[] GetConditions(MeasuresSchema measure, Dictionary<string, string> conditionDescriptionsList, RefCusApplicability[] applicabilities)
	{
		List<RefCusCondition> conditions = [];
		var conditionsToProcess = measure.conditions.Replace("Cond:", string.Empty).Split(';');

		foreach (var conditionString in conditionsToProcess)
		{
			if (conditionString.Contains("cert:"))
			{
				var conditionID = conditionString.Trim()[0].ToString();
				var startIndex = conditionString.IndexOf(CertificateCondition, StringComparison.InvariantCulture) + 5;
				var lastIndex = conditionString.IndexOf(RightBoundaryExpressionSeparator);
				if (lastIndex > 0)
				{
					var certificateCode = conditionString[startIndex..lastIndex].Trim();

					if (conditionDescriptionsList.TryGetValue(conditionID, out string description))
					{
						conditions.Add(new RefCusCondition()
						{
							ZX1_StartDate = measure.start_date,
							ZX1_Comment = description,
							RefCusConditionValues = [new() { ZX3_Value = certificateCode }],
							RefCusApplicabilities = applicabilities
						});
					}
					else
					{
						ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import MEA 'condition', description not found. Details:");
						ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Tariff: {measure.goods_id}");
						ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Condition: {conditionString}");
						ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Condition ID: {conditionID}");
					}
				}
				else
				{
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import MEA 'condition' as invalid format. Details:");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Tariff: {measure.goods_id}");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Condition: {conditionString}");
				}
			}
		}

		return [.. conditions];
	}
	const string CertificateCondition = "cert:";
	const char RightBoundaryExpressionSeparator = '(';

	bool CheckTariffDataIsValid(string code, string duty)
	{
		var result = true;
		var dutyNumber = duty.Replace("%", string.Empty);
		if (string.IsNullOrEmpty(code) || code.Length != 10 || string.IsNullOrEmpty(duty) || !double.TryParse(dutyNumber, out double _))
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import MEA as missing or invalid 'tariff' or 'duty'. Details:");
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
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_NKPreference, false, Constants.Preference.GoodsCoveredByMEA);
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
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_NKConditionType, true, Constants.ConditionType.MEASuspensions);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, false, Constants.CountryCode);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_EndDate, false, Constants.MaximumDateTime);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_Source, false, Constants.CountryCode);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, false, Constants.CountryCode);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsImport, false, true);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsExport, false, false);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ConditionValueTrueMeansStop, false, false);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_LogicalANDWithinGroup, false, 0);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZS_NKPreference, false, Constants.Preference.GoodsCoveredByMEA);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZS_ZZZ_NKDataGrouping, false, Constants.CountryCode);
			refCusConditionConfiguration.IncludeColumn(x => x.RefCusApplicabilities, false);
			refCusConditionConfiguration.IncludeColumn(x => x.RefCusConditionValues, false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionConfiguration);

			var refCusConditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, false);
			refCusConditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_NKValueType, false, Constants.ConditionValueType.SupportingDocuments);
			refCusConditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			refCusConditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_LogicalORWithinGroup, false, 0);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionValueConfiguration);

			return writerConfiguration;
		}
	}
}
