using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class ProductFilterReValidatorTest : WhsTestCaseWithFactory
	{
		#region TestRevalidateProductFilter

		public void TestRevalidateProductFilter()
		{
			var clientFilter = new ModuleGuidFilter("Client", ModuleIDs.Organisation, WhsDocketSchema.WD_OH_Client, new WarehouseClientCollectionWithSecurityCheck(Factory));
			var productFilter = new ModuleGuidFilter("Product", ModuleIDs.Organisation, WhsDocketLineSchema.WE_OP, new WhsOrgSupplierPartCollection(Factory, null, null, false));
			AssertExceptionThrown("Since Client ModuleGuidFilters are null, it should throw ArgumentNullException.", typeof(ArgumentNullException), () => ProductFilterReValidator.RevalidateProductFilter(null, productFilter, Factory));
			AssertExceptionThrown("Since Product ModuleGuidFilters are null, it should throw ArgumentNullException.", typeof(ArgumentNullException), () => ProductFilterReValidator.RevalidateProductFilter(clientFilter, null, Factory));
			AssertExceptionThrown("Since both ModuleGuidFilters are null, it should throw ArgumentNullException.", typeof(ArgumentNullException), () => ProductFilterReValidator.RevalidateProductFilter(null, null, Factory));
		}

		#endregion
	}
}
