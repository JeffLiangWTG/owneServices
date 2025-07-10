using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AssignWhsPutawayGroupLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestWhsPutawayGroups

		public void TestWhsPutawayGroups()
		{
			AssertType(ObjectFactory.GetType<IWhsPutawayGroupCollection>(), GetNewLookups().WhsPutawayGroups);
		}

		#endregion

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

		AssignWhsPutawayGroupLookups GetNewLookups()
		{
			return new AssignWhsPutawayGroupLookups(new AssignWhsPutawayGroupMethodApplicator("test", Factory));
		}

		#endregion
	}
}
