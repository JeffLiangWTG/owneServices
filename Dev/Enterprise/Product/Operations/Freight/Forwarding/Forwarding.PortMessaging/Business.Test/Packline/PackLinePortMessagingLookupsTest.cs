using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	sealed class PackLinePortMessagingLookupsTest : BusinessObjectLookupsTestCase
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

		JobPackLinePortMessagingLookups Lookups
		{
			get { return lookups ?? (lookups = new JobPackLinePortMessagingLookups(Factory.New<PackLinePortMessaging>())); }
		}
		JobPackLinePortMessagingLookups lookups;

		#endregion
	}
}
