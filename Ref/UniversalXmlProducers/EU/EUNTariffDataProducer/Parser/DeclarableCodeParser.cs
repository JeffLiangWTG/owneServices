using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class DeclarableCodeParser : ExcelDataParser, IDeclarableCodeParser
	{
		const string idxTariffHeader = "Goods code";
		const string idxStartDate = "Start date";
		const string idxDeclarableStartDate = "Decl. start date";
		const string idxIsLeaf = "IS_LEAF";
		const string idxEndDate = "End Date";

		public DeclarableCodeParser()
		{
			HeaderMap = new Dictionary<string, int>
			{
				{ idxTariffHeader, 0 },
				{ idxStartDate, 1 },
				{ idxDeclarableStartDate, 2 },
				{ idxIsLeaf, 3 },
				{ idxEndDate, 4 },
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override IExcelDataRecord ParseRecord(IRow rawDataRow)
		{
			var tariffHeader = rawDataRow.GetStringValue(HeaderMap[idxTariffHeader]);
			if (string.IsNullOrEmpty(tariffHeader))
			{
				return null;
			}

			var startDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxStartDate]));
			var declarableStartDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxDeclarableStartDate]));
			var isleaf = rawDataRow.GetStringValue(HeaderMap[idxIsLeaf]);

			var endDateValue = rawDataRow.GetStringValue(HeaderMap[idxEndDate]);
			var endDate = !string.IsNullOrEmpty(rawDataRow.GetStringValue(HeaderMap[idxEndDate]))
				? DateTime.ParseExact(endDateValue, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)
				: new DateTime(2079, 06, 06, 23, 59, 00);
			;

			return new RawDeclarableCodeRecord(tariffHeader, startDate, declarableStartDate, isleaf, endDate);
		}

		public IEnumerable<IRawDeclarableCodeRecord> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath);
			return parsedRecords.Cast<IRawDeclarableCodeRecord>();
		}
	}
}
