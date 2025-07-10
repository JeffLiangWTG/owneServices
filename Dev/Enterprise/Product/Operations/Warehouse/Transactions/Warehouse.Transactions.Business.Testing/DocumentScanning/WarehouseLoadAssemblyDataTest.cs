using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseLoadAssemblyDataTest : WhsTestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			var assemblyData = new WarehouseLoadAssemblyData();
			AssertEquals(typeof(WhsLoad), assemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			var assemblyData = new WarehouseLoadAssemblyData();
			AssertNotNull(assemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(typeof(WhsLoadCollection), assemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var assemblyData = new WarehouseLoadAssemblyData();
			AssertEquals(ModuleIDs.WhsLoad, assemblyData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			var assemblyData = new WarehouseLoadAssemblyData();
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, assemblyData.ReferenceType);
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			var assemblyData = new WarehouseLoadAssemblyData();
			AssertEquals(true, assemblyData.IsAllowedForUnallocatedeDocs);
		}

		#endregion
	}
}
