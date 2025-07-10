using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Models
{
	public class PortData
	{
		public string Code { get; set; }
		public string Description { get; set; }
		public string AdditionalInfo { get; set; }

		public ICDSPortSource Source { get; set; }
	}
}
