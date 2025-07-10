using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCarrierCombinedLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportList()
		{
			var carrier1 = Factory.New<USCarrierCombined>();
			var carrier2 = Factory.New<USCarrierCombined>();
			var list = carrier1.Lookups.TransportList;
			AssertEquals(list, carrier2.Lookups.TransportList);
			AssertEquals(4, list.Count);
			AssertEquals("Vessel", list.GetDescriptionFromCode(TransportModeCodes.Codes.VesselNonContainer));
			AssertEquals("Rail", list.GetDescriptionFromCode(TransportModeCodes.Codes.RailNonContainer));
			AssertEquals("Truck", list.GetDescriptionFromCode(TransportModeCodes.Codes.TruckNonContainer));
			AssertEquals("Air", list.GetDescriptionFromCode(TransportModeCodes.Codes.AirNonContainer));
		}
	}
}
