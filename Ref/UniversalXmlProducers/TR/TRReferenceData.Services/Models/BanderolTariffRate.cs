using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class BanderolTariffRate
	{
		[Name("TariffCode")]
		public string TariffCode { get; set; }

		[Name("Description")]
		public string Description { get; set; }

		[Name("RateFormula")]
		public string RateFormula { get; set; }

		[Name("UOM")]
		public string UOM { get; set; }

		[Name("Currency")]
		public string Currency { get; set; }
	}
}
