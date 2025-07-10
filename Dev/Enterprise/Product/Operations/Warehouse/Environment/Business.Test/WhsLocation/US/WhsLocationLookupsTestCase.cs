using Enterprise.Warehouse.Environment.CodeLists.US;

namespace Enterprise.Warehouse.Environment.Business.US.Testing
{
	class WhsLocationLookupsTestCase : Business.Testing.WhsLocationViewLookupsTestCase
	{
		public override void TestApprovedKnownStatuses()
		{
			AssertEquals(true, Lookups.ApprovedKnownStatuses.ContainsCode(TSAStatus.Codes.Unknown));
			AssertEquals(true, Lookups.ApprovedKnownStatuses.ContainsCode(TSAStatus.Codes.Known));
		}

		protected override void TestApprovedKnownStatuses_CachedCore()
		{
			AssertNotNull(Lookups.ApprovedKnownStatuses);
			AssertNotNull("Cache should exist on factory", Factory.GetCachedValue<TSAStatus>($"WhsLocationLookups|ApprovedKnownStatuses", () => null));
		}

		#region Implementation

		protected override WhsLocationViewLookups GetNewLookups()
		{
			return new WhsLocationLookups(Factory.New<WhsLocation>());
		}

		#endregion
	}
}
