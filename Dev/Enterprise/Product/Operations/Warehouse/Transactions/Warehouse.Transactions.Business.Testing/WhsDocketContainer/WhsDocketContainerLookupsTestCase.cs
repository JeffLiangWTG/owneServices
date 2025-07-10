using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsDocketContainerLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		public void TestRefContainerList()
		{
			AssertNotNull(Lookups.RefContainers);
			Assert("Is it loaded", Lookups.RefContainers.Count > 0);
			AssertEquals("List type", typeof(RefContainerCollection), Lookups.RefContainers.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			WhsDocketContainer container = Factory.New<WhsDocketContainer>();
			Lookups = new WhsDocketContainerLookups(container);
		}

		WhsDocketContainerLookups Lookups;
	}
}
