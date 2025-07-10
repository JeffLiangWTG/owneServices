using System.Collections.Generic;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public abstract class CsvDataParser<T>
	{
		public abstract IDictionary<string, int> HeaderMap { get; }
		public abstract T ParseRecord(string[] rawDataRow);

		protected IEnumerable<T> ParseRecords(string filePath, Encoding encoding, bool hasHeader = false)
		{
			var rawRecords = new List<T>();

			using (var csvParser = new TextFieldParser(filePath, encoding))
			{
				csvParser.SetDelimiters(new string[] { "," });
				csvParser.HasFieldsEnclosedInQuotes = true;

				while (!csvParser.EndOfData)
				{
					if (hasHeader && csvParser.LineNumber == 1)
					{
						csvParser.ReadFields();
						continue;
					}
					var recordSplit = csvParser.ReadFields();
					rawRecords.Add(ParseRecord(recordSplit));
				}
			}
			return rawRecords;
		}
	}
}
