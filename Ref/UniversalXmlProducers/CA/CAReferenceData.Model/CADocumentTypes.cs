using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.CAReferenceData.Model
{
	public class CADocumentTypes
	{
		[Name("ID Code")]
		public string GovAgencyIDCode { get; set; }

		[Name("Code")]
		public string Code { get; set; }

		[Name("Description")]
		public string Desc { get; set; }

		[Name("Is Default Ref Num")]
		public string IsDefaultRefNum { get; set; }
	}
}
