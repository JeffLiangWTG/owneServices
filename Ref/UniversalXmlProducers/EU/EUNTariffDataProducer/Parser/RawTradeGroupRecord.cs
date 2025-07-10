using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RawTradeGroupRecord : IRawTradeGroupRecord
	{
		public string TariffHeader { get; }
		public DateTime StartDate { get; }
		public string CountryGroup { get; }
		public string CountryGroupDescription { get; }
		public string MemberCountry { get; }
		public string MemberCountryDescription { get; }
		public DateTime MemberStartDate { get; }
		public DateTime MemberEndDate { get; }

		public RawTradeGroupRecord(string countryGroup, string countryGroupDescription, string memberCountry, string memberCountryDescription, DateTime startDate, DateTime memberStartDate, DateTime memberEndDate)
		{
			CountryGroup = countryGroup;
			CountryGroupDescription = countryGroupDescription;
			MemberCountry = memberCountry;
			MemberCountryDescription = memberCountryDescription;
			StartDate = startDate;
			MemberStartDate = memberStartDate;
			MemberEndDate = memberEndDate;
		}
	}
}
