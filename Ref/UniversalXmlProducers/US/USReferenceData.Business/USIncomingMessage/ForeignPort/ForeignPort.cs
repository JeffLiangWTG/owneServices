using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor
{
	public class ForeignPort
	{
		[Name("Port")]
		public string Code { get; set; }

		[Name("Description")]
		public string Description { get; set; }

		[Name("Type")]
		public string Type { get; set; }
	}
}
