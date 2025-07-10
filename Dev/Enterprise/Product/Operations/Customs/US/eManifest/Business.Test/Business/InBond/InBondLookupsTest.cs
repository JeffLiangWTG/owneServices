using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class InBondLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var lookups = Factory.New<Trip>().Shipments.AddNew().InBond.Lookups;
			AssertType<InbondTypes>("InBondTypes", lookups.InBondTypes);
			AssertType<ZZRefCusCodeListCombinedCollection>("ScheduleDPortCodes", lookups.ScheduleDPortCodes);
			AssertType<ZZRefCusCodeListCombinedCollection>("ScheduleKPortCodes", lookups.ScheduleKPortCodes);
			AssertType<USCarrierCombinedCollection>("SCACCarrierCodes", lookups.SCACCarrierCodes);
		}
	}
}
