using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	sealed class GenerateRMAByOrdersLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestWarehouses

		public void TestWarehouses()
		{
			AssertType(ObjectFactory.GetType<IWhsWarehouseCollection>(), GetNewLookups().Warehouses);
		}

		public void TestWarehouses_WarehouseCollectionType()
		{
			AssertEquals(WarehouseCollectionType.ProductWarehouse, GetNewLookups().Warehouses.WarehouseCollectionType);
		}

		#endregion

		#region Implementation

		GenerateRMAByOrdersActionLookups GetNewLookups()
		{
			return new GenerateRMAByOrdersActionLookups(new GenerateRMAByOrdersActionMethodApplicator(Factory));
		}

		#endregion
	}
}
