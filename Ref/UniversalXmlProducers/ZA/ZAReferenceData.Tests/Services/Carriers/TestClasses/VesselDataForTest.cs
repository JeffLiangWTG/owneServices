using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestClasses
{
	internal class VesselDataForTest : VesselData
	{
		public VesselDataForTest(string radioCallSign, string vesselName, CarrierData carrier) : base(radioCallSign, vesselName, carrier)
		{
		}

		public string NormalizeNameExposed(string name) => NormalizeName(name);
	}
}
