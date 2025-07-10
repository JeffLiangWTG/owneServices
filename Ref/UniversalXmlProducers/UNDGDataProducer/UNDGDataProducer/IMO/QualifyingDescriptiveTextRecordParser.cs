using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class QualifyingDescriptiveTextRecordParser : CsvDataParser<QualifyingDescriptiveTextRecord>
	{
		public QualifyingDescriptiveTextRecordParser()
		{
			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(QualifyingDescriptiveTextRecord.UnId), 0 },
				{ nameof(QualifyingDescriptiveTextRecord.QualifyingDescriptiveText), 1 }
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override QualifyingDescriptiveTextRecord ParseRecord(string[] rawDataRow)
		{
			return new QualifyingDescriptiveTextRecord()
			{
				UnId = ParserHelper.ParseUNNOAndVariant(rawDataRow[HeaderMap[nameof(QualifyingDescriptiveTextRecord.UnId)]]),
				QualifyingDescriptiveText = rawDataRow[HeaderMap[nameof(QualifyingDescriptiveTextRecord.QualifyingDescriptiveText)]]
			};
		}

		public IEnumerable<QualifyingDescriptiveTextRecord> Parse(string filePath)
		{
			return ParseRecords(filePath, Constants.Encodings.IMOZipFile, true);
		}
	}
}
