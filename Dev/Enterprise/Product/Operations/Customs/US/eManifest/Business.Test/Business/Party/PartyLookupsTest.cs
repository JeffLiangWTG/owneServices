using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class PartyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			var lookups = shipment.Parties.AddNew().Lookups;
			AssertType<PartyTypes>("PartyTypes", lookups.PartyTypes);
			AssertEquals("Consignee should not be included to the other parties types", false, lookups.PartyTypes.ContainsCode(PartyTypes.Codes.Consignee));
			AssertEquals("Shipper should not be included to the other parties types", false, lookups.PartyTypes.ContainsCode(PartyTypes.Codes.Shipper));
			AssertType<OrgHeaderCollection>("Organizations", lookups.Organizations);
		}
	}
}
