using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class TradeGroupCountryCodes
	{
		[Name("Trade Group Code")]
		public string TradeGroupCode { get; set; }

		[Name("Trade Group Description")]
		public string TradeGroupDescription { get; set; }

		[Name("Country Code")]
		public string CountryCode { get; set; }

		[Name("Country Name")]
		public string CountryName { get; set; }

		[Name("Origin Control")]
		public string OriginControl { get; set; }

		[Name("Exit Country Control")]
		public string ExitCountryControl { get; set; }
	}
}
