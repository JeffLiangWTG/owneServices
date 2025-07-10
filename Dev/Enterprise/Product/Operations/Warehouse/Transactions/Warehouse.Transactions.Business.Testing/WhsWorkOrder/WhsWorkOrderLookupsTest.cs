namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWorkOrderLookupsTest : WhsComponentOrderLookupsTest<WhsWorkOrder>
	{
		protected override void TestSubTypesCore()
		{
			AssertEquals(true, GetNewBusinessObject().Lookups.SubTypes.ContainsCode(CodeLists.WorkOrderType.Codes.Assemble));
		}
	}
}
