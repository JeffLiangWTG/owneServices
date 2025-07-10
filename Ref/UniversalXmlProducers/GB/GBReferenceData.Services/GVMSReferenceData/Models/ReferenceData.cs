using System.Collections.Generic;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models
{
	public class ReferenceData
	{
		public List<Carrier> Carriers { get; set; }
		public List<Port> Ports { get; set; }
		public List<Route> Routes { get; set; }
		public List<RuleFailure> RuleFailures { get; set; }
		public List<InspectionLocation> InspectionLocations { get; set; }
		public List<InspectionType> InspectionTypes { get; set; }
		public string ErrorCollector { get; set; }
	}
}
