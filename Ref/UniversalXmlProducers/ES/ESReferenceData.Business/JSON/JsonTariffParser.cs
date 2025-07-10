using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public abstract class JsonTariffParser(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : CommonParser(dateTimeProvider)
{
	protected string RefDbServiceURI { get; } = refDbServiceURI;

	protected abstract string applicabilityType { get; }
	protected abstract string dataSource { get; }

	public string ConvertToXMLFile(string measuresJsonContent, string footnotesJsonContent, string outPutFileWithPath, string taxOrFeeJsonContent = null)
	{
		ErrorBuilder.Clear();

		var records = GetRecordsToExport(JsonHelper.ToJsonl(measuresJsonContent), JsonHelper.ToJsonl(footnotesJsonContent), taxOrFeeJsonContent);
		if (records.Count > 0)
		{
			Helper.ExportToXMLFile(dataSource, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, records, UpdateType.Full);
		}
		else
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to locate {applicabilityType} records");
		}
		return ErrorBuilder.ToString();
	}

	protected List<RefDataRepoModelEntityType> GetRecordsToExport(string measuresJsonContent, string footnotesJsonContent, string taxOrFeeJsonContent)
	{
		var records = new List<RefDataRepoModelEntityType>();
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
			return records;
		}

		var measures = JsonHelper.GetListItemsFromJsonl<MeasuresSchema>(measuresJsonContent, ErrorBuilder, handleDates: true, x => x.measure_type.StartsWith(measureTypeCode, StringComparison.InvariantCulture));
		var footnoteDescriptions = JsonHelper.GetListItemsFromJsonl<FootnoteSchema>(footnotesJsonContent, ErrorBuilder);

		if (measures.Count == 0 || footnoteDescriptions.Count == 0)
		{
			ErrorBuilder.AppendLine("Unable to parse any record for Measures or Footnotes.");
			return records;
		}

		var footnoteDescriptionsDictionary = new Dictionary<string, string>();

		static string GetDictionaryDescription(IEnumerable<FootnoteSchema.Description> descriptions)
			=> descriptions.FirstOrDefault(x => string.Equals(x.lang, "es", StringComparison.OrdinalIgnoreCase))?.text ?? descriptions.FirstOrDefault(x => string.Equals(x.lang, "en", StringComparison.OrdinalIgnoreCase))?.text ?? string.Empty;

		foreach (var footnotes in footnoteDescriptions)
		{
			var footnote_id = footnotes.footnote_id;
			if (!footnoteDescriptionsDictionary.ContainsKey(footnote_id))
			{
				footnoteDescriptionsDictionary.Add(footnotes.footnote_id, GetDictionaryDescription(footnotes.descriptions));
			}
		}

		ProcessAndGetMeasures(records, measures, footnoteDescriptionsDictionary, taxOrFeeJsonContent);

		return records;
	}

	protected abstract string measureTypeCode { get; }

	protected abstract void ProcessAndGetMeasures(List<RefDataRepoModelEntityType> records, List<MeasuresSchema> measures, Dictionary<string, string> footnoteDescriptionsDictionary, string taxOrFeeJsonContent);
}
