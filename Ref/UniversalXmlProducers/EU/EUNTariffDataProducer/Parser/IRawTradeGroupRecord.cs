using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawTradeGroupRecord : IExcelDataRecord
	{
		string CountryGroup { get; }
		string CountryGroupDescription { get; }
		string MemberCountry { get; }
		string MemberCountryDescription { get; }
		DateTime MemberStartDate { get; }
		DateTime MemberEndDate { get; }
	}
}
