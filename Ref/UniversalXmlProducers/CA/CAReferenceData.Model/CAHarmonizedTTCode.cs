using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.CAReferenceData.Model
{
	public class CAHarmonizedTTCode
	{
		[Name("TT code")]
		public string Code { get; set; }

		[Name("Tariff Treatment Category")]
		public string Description { get; set; }

		[Name("Abbreviation")]
		public string Abbreviation { get; set; }
	}
}
