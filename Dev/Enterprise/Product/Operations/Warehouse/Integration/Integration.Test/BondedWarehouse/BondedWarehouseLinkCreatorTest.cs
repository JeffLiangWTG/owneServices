using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Integration.BondedWarehouse.Testing
{
	public class BondedWarehouseLinkCreatorTest : TestCaseWithFactory
	{
		public void TestGetNewBondedWarehouseLink()
		{
			AssertNotNull(new BondedWarehouseLinkCreator().GetNewBondedWarehouseLink(Factory));
		}
	}
}
