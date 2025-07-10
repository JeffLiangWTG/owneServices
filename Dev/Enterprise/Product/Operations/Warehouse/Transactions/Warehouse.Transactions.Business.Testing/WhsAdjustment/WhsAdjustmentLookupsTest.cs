using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAdjustmentLookupsTest : WhsDocketLookupsTest<WhsAdjustment>
	{
		#region TestSubTypes

		protected override void TestSubTypesCore()
		{
			var lookups = (WhsAdjustmentLookups)GetNewBusinessObject().Lookups;
			AssertEquals(4, lookups.SubTypes.Count);
			AssertEquals(true, lookups.SubTypes.ContainsCode(AdjustmentType.Codes.Customs));
			AssertEquals(true, lookups.SubTypes.ContainsCode(AdjustmentType.Codes.Adjustment));
			AssertEquals(true, lookups.SubTypes.ContainsCode(AdjustmentType.Codes.OwnershipAdjustment));
			AssertEquals(true, lookups.SubTypes.ContainsCode(AdjustmentType.Codes.InternalWarehouseAdjustment));
		}

		#endregion
	}
}
