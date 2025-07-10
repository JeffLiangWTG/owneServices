using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.Module.Testing.DispatchConsignment
{
	sealed class WhsItemDispatchLoadListFilterControlTest : WhsTransitFilterControlTest<WhsItemDispatchLoadListFilterControl>
	{
		public void TestCreditorColumn()
			=> AssertColumn("Creditor", $"{nameof(WhsItemDispatchLoadList.Creditor)}+{nameof(WhsItemDispatchLoadList.Creditor.E2_CompanyName)}");

		public void TestLoadListIDColumn()
			=> AssertColumn("Load List ID", nameof(WhsItemDispatchLoadList.WDL_JobID));

		public void TestReferenceNumberColumn()
			=> AssertColumn("Reference Number", nameof(WhsItemDispatchLoadList.WDL_ReferenceNumber));

		protected override WhsItemDispatchLoadListFilterControl GetFilterControl()
			=> new (new WhsItemDispatchLoadListCollection(Factory), new WhsItemDispatchLoadListFilterBusinessObject());
	}
}
