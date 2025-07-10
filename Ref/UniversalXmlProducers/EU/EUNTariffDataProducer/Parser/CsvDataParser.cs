using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.VisualBasic.FileIO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public abstract class CsvDataParser
	{
		public enum CsvHeaderMap
		{
			TariffHeader = 0,
			AdditionalCode = 1,
			OrderNumber = 2,
			StartDate = 3,
			EndDate = 4,
			ReductionIndicator = 5,
			LegalBase = 6,
			Rate = 7,
			TradeGroup = 8,
			MeasureTypeId = 9,
			RecordType = 10
		}

		public abstract IExcelDataRecord ParseRecord(string[] splitData);

		protected ICollection<IExcelDataRecord> ParseRecords(string filePath)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));

			var rawRecords = new List<IExcelDataRecord>();

			using (var csvParser = new TextFieldParser(filePath, Encoding.UTF8))
			{
				csvParser.SetDelimiters(new string[] { "," });
				csvParser.HasFieldsEnclosedInQuotes = true;

				while (!csvParser.EndOfData)
				{
					if (csvParser.LineNumber == 1)
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
