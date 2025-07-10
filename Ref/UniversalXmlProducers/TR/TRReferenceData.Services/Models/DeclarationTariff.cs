using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class DeclarationTariff
	{
		[Name("TariffCode")]
		public string TariffCode { get; set; }

		[Name("AdditionalCode")]
		public string AdditionalCode { get; set; }

		[Name("Description")]
		public string Description { get; set; }

		[Name("Percent")]
		public string Percent { get; set; }

		[Name("Formula")]
		public string Formula { get; set; }
	}
}
