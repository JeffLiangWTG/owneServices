using CargoWise.Types;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	struct SailingReference
	{
		public ZString VesselName { get; set; }
		public ZString VoyageNumber { get; set; }
		public ZString PortOfLoading { get; set; }
		public ZString PortOfDischarge { get; set; }
		public ZString LloydsNumber { get; set; }
	}
}
