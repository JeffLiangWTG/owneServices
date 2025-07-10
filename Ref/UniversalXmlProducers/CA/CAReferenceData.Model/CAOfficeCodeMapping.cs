using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.CAReferenceData.Model
{
	public class CAOfficeCodeMapping
	{
		[Name("CA OFFICE CODE")]
		public string OfficeCode { get; set; }

		[Name("US Port of Exit")]
		public string USPortOfExit { get; set; }
	}
}
