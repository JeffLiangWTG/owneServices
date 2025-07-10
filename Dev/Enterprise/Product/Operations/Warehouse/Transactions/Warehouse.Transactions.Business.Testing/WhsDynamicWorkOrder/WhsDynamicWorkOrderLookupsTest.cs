using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDynamicWorkOrderLookupsTest : WhsComponentOrderLookupsTest<WhsDynamicWorkOrder>
	{
		protected override void TestSubTypesCore()
		{
			AssertContainsExactElementsInAnyOrder(
				new DynamicWorkOrderType(),
				GetNewBusinessObject().Lookups.SubTypes);
		}
	}
}
