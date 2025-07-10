namespace Enterprise.Warehouse.Environment.Business.US.Testing
{
	public class WhsRowLookupsTestCase : Business.Testing.WhsRowLookupsTestCase
	{
		#region TestApprovedKnownStatuses

		public override void TestApprovedKnownStatuses()
		{
			AssertEquals(true, Lookups.ApprovedKnownStatuses.ContainsCode(CodeLists.US.TSAStatus.Codes.Unknown));
			AssertEquals(true, Lookups.ApprovedKnownStatuses.ContainsCode(CodeLists.US.TSAStatus.Codes.Known));
		}

		#endregion

		#region Implementation

		WhsRowLookups Lookups
		{
			get { return lookups ?? (lookups = new WhsRowLookups(Row)); }
		}

		WhsRowLookups lookups;

		#endregion
	}
}
