using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseVASOrderAssemblyDataTest : WhsTestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			var assemblyData = new WarehouseVASOrderAssemblyData();
			AssertEquals(typeof(WhsVASOrder), assemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			var assemblyData = new WarehouseVASOrderAssemblyData();
			AssertNotNull(assemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(typeof(WhsVASOrderCollection), assemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var assemblyData = new WarehouseVASOrderAssemblyData();
			AssertEquals(ModuleIDs.WhsVASOrder, assemblyData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			var assemblyData = new WarehouseVASOrderAssemblyData();
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, assemblyData.ReferenceType);
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			var assemblyData = new WarehouseVASOrderAssemblyData();
			AssertEquals(true, assemblyData.IsAllowedForUnallocatedeDocs);
		}

		#endregion
	}
}
