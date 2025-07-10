using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	sealed class ShipmentPortMessagingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryTypeList()
		{
			AssertType(typeof(EntryTypeList), Lookups.EntryTypeList);
		}

		public void TestExemptionReasonList()
		{
			AssertType(typeof(ExemptionReasonList), Lookups.ExemptionReasonList);
		}

		public void TestAnnex30ATypeList()
		{
			AssertType(typeof(Annex30ATypeList), Lookups.Annex30ATypeList);
		}

		#region Implementation

		JobShipmentPortMessagingLookups Lookups
		{
			get { return lookups ?? (lookups = new JobShipmentPortMessagingLookups(Factory.New<ShipmentPortMessaging>())); }
		}
		JobShipmentPortMessagingLookups lookups;

		#endregion
	}
}
