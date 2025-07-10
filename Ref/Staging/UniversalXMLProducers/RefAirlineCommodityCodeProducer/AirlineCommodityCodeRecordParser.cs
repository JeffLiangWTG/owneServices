using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.VisualBasic.FileIO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProducer
{
	public class AirlineCommodityCodeRecordParser
	{
		readonly IDictionary<string, int> _headerMap = new Dictionary<string, int>
		{
			{ nameof(AirlineCommodityCodeRecord.Code), 0 },
			{ nameof(AirlineCommodityCodeRecord.Description), 1 }
		};

		public IEnumerable<RefAirlineCommodityCode> Parse(string filePath)
		{
			var records = new List<AirlineCommodityCodeRecord>();

			using (var csvParser = new TextFieldParser(filePath, Encoding.UTF8))
			{
				csvParser.SetDelimiters(",");
				csvParser.HasFieldsEnclosedInQuotes = true;

				while (!csvParser.EndOfData)
				{
					var recordSplit = csvParser.ReadFields();

					var parsedRecord = ParseRecord(recordSplit);

					if (!string.IsNullOrWhiteSpace(parsedRecord.Code))
					{
						records.Add(parsedRecord);
					}
				}
			}

			return records.Where(c => !string.IsNullOrEmpty(c.Code)).Select(ConvertAirlineCommodityCodeRecordToRefAirlineCommodityCode);
		}

		RefAirlineCommodityCode ConvertAirlineCommodityCodeRecordToRefAirlineCommodityCode(AirlineCommodityCodeRecord record)
		{
			return new RefAirlineCommodityCode
			{
				RAC_Code = record.Code,
				RAC_Description = record.Description
			};
		}

		AirlineCommodityCodeRecord ParseRecord(string[] recordRow)
		{
			return new AirlineCommodityCodeRecord
			{
				Code = recordRow[_headerMap[nameof(AirlineCommodityCodeRecord.Code)]],
				Description = recordRow[_headerMap[nameof(AirlineCommodityCodeRecord.Description)]]
			};
		}
	}
}
