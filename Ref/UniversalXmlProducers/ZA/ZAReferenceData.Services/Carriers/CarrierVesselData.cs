using System.Collections.Generic;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers
{
	public class CarrierVesselData : BaseData<CarrierData>
	{
		public List<VesselData> Vessels { get; set; }
	}
}
