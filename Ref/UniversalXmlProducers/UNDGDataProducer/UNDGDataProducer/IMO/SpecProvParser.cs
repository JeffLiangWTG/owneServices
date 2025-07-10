using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class SpecProvParser : CsvDataParser<SpecProvRecord>
	{
		public SpecProvParser()
		{
			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(SpecProvRecord.SPNo), 0 },
				{ nameof(SpecProvRecord.SPText), 1 }
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override SpecProvRecord ParseRecord(string[] rawDataRow)
		{
			return new SpecProvRecord()
			{
				SPNo = rawDataRow[HeaderMap[nameof(SpecProvRecord.SPNo)]],
				SPText = rawDataRow[HeaderMap[nameof(SpecProvRecord.SPText)]]
			};
		}

		public IEnumerable<SpecProvRecord> Parse(string filePath)
		{
			return ParseRecords(filePath, Constants.Encodings.IMOZipFile, true);
		}

		public static UNDGCommonData GetCommonData(SpecProvRecord specProvRecord)
		{
			return new UNDGCommonData
			{
				DC_Descriptor = specProvRecord.SPText,
				DC_Index = specProvRecord.SPNo,
				DC_Language = "EN",
				DC_Type = "SPP"
			};
		}
	}
}
