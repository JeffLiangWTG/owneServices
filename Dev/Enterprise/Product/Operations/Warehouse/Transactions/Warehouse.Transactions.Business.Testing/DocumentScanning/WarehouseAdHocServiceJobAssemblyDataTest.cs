using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseAdHocServiceJobAssemblyDataTest : WhsTestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			var assemblyData = new WarehouseAdHocServiceJobAssemblyData();
			AssertEquals(typeof(WhsAdHocServiceJob), assemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			var assemblyData = new WarehouseAdHocServiceJobAssemblyData();
			AssertNotNull(assemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(typeof(WhsAdHocServiceJobCollection),
				assemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var assemblyData = new WarehouseAdHocServiceJobAssemblyData();
			AssertEquals(ModuleIDs.WhsAdHocServiceJob, assemblyData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			var assemblyData = new WarehouseAdHocServiceJobAssemblyData();
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, assemblyData.ReferenceType);
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			var assemblyData = new WarehouseAdHocServiceJobAssemblyData();
			AssertEquals(true, assemblyData.IsAllowedForUnallocatedeDocs);
		}

		#endregion
	}
}
