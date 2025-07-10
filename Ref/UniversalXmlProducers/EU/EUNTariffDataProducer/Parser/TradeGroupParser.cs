using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class TradeGroupParser : ExcelDataParser, ITradeGroupParser
	{
		const string IdxCountryGroup = "Country group";
		const string IdxStartDate = "Start date";
		const string IdxCountryGroupDescription = "Country group descr.";
		const string IdxMemberCountry = "Member country";
		const string IdxMemberCountryDescription = "Description";
		const string IdxMemberStartDate = "Mbship start date";
		const string IdxMemberEndDate = "Mbship end date";

		public TradeGroupParser()
		{
			HeaderMap = new Dictionary<string, int>
			{
				{ IdxCountryGroup, 0 },
				{ IdxStartDate, 1 },
				{ IdxCountryGroupDescription, 4 },
				{ IdxMemberCountry, 5 },
				{ IdxMemberCountryDescription, 7 },
				{ IdxMemberStartDate, 8 },
				{ IdxMemberEndDate, 9 }
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public IEnumerable<IRawTradeGroupRecord> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath);
			return parsedRecords.Cast<IRawTradeGroupRecord>();
		}

		public override IExcelDataRecord ParseRecord(IRow rawDataRow)
		{
			var countryGroup = rawDataRow.GetStringValue(HeaderMap[IdxCountryGroup]);
			var startDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[IdxStartDate]));
			var countryGroupDescription = rawDataRow.GetStringValue(HeaderMap[IdxCountryGroupDescription]);
			var memberCountry = rawDataRow.GetStringValue(HeaderMap[IdxMemberCountry]);
			var memberCountryDescription = rawDataRow.GetStringValue(HeaderMap[IdxMemberCountryDescription]);
			var memberStartDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[IdxMemberStartDate]));
			var memberEndDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[IdxMemberEndDate]), isStartDate: false);

			return new RawTradeGroupRecord(countryGroup, countryGroupDescription, memberCountry, memberCountryDescription, startDate, memberStartDate, memberEndDate);
		}
	}
}
