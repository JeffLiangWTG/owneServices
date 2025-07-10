using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class PropertyRecordParser : CsvDataParser<PropertyRecord>
	{
		public PropertyRecordParser()
		{
			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(PropertyRecord.UNNO), 0 },
				{ nameof(PropertyRecord.Variant), 1 },
				{ nameof(PropertyRecord.PropObs), 2 }
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override PropertyRecord ParseRecord(string[] rawDataRow)
		{
			return new PropertyRecord()
			{
				UNNO = ParserHelper.ParseUNNO(rawDataRow[HeaderMap[nameof(PropertyRecord.UNNO)]]),
				Variant = rawDataRow[HeaderMap[nameof(PropertyRecord.Variant)]],
				PropObs = rawDataRow[HeaderMap[nameof(PropertyRecord.PropObs)]]
			};
		}

		public IEnumerable<PropertyRecord> Parse(string filePath)
		{
			return ParseRecords(filePath, Constants.Encodings.IMOZipFile, true);
		}
	}
}
