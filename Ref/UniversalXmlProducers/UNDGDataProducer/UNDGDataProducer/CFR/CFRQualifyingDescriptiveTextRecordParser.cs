using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class CFRQualifyingDescriptiveTextRecordParser : CsvDataParser<CFRQualifyingDescriptiveTextRecord>
	{
		public CFRQualifyingDescriptiveTextRecordParser()
		{
			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(CFRQualifyingDescriptiveTextRecord.Unno), 0 },
				{ nameof(CFRQualifyingDescriptiveTextRecord.Variant), 1 },
				{ nameof(CFRQualifyingDescriptiveTextRecord.QualifyingDescriptiveText), 3 }
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override CFRQualifyingDescriptiveTextRecord ParseRecord(string[] rawDataRow)
		{
			return new CFRQualifyingDescriptiveTextRecord()
			{
				Unno = ParserHelper.ParseUNNOAndVariant(rawDataRow[HeaderMap[nameof(CFRQualifyingDescriptiveTextRecord.Unno)]]),
				QualifyingDescriptiveText = rawDataRow[HeaderMap[nameof(CFRQualifyingDescriptiveTextRecord.QualifyingDescriptiveText)]],
				Variant = rawDataRow[HeaderMap[nameof(CFRQualifyingDescriptiveTextRecord.Variant)]]
			};
		}

		public IEnumerable<CFRQualifyingDescriptiveTextRecord> Parse(string filePath)
		{
			return ParseRecords(filePath, Constants.Encodings.CFRZipFile, true);
		}
	}
}
