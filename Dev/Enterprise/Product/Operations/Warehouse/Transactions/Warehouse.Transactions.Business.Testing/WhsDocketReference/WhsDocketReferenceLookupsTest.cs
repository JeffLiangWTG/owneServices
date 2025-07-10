namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsDocketReferenceLookupsTest : WhsBusinessObjectLookupsTestCase
	{
		public void TestRefereceTypes()
		{
			WhsDocketReference @ref = Factory.New<WhsDocketReference>();
			AssertNotNull(@ref.Lookups.ReferenceTypes);
		}
	}
}
