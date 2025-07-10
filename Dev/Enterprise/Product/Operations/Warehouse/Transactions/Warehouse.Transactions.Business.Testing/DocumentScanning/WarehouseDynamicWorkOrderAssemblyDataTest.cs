using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseDynamicWorkOrderAssemblyDataTest : WhsTestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			var assemblyData = new WarehouseDynamicWorkOrderAssemblyData();
			AssertEquals(typeof(WhsDynamicWorkOrder), assemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			var assemblyData = new WarehouseDynamicWorkOrderAssemblyData();
			AssertNotNull(assemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(typeof(WhsDynamicWorkOrderCollection), assemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var assemblyData = new WarehouseDynamicWorkOrderAssemblyData();
			AssertEquals(ModuleIDs.WhsDynamicWorkOrder, assemblyData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			var assemblyData = new WarehouseDynamicWorkOrderAssemblyData();
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, assemblyData.ReferenceType);
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			var assemblyData = new WarehouseDynamicWorkOrderAssemblyData();
			AssertEquals(true, assemblyData.IsAllowedForUnallocatedeDocs);
		}

		#endregion
	}
}
