using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ProductModule))]
	public class ProductModuleTest : ZModuleBasherTest
	{
		public void TestControllerID()
		{
			using (var module = new ProductModule())
			{
				var newMenuItem = module.FormActionMenu.Single(m => m.Text == "&New");
				newMenuItem.PerformClick();

				var controller = ((IFilterModuleInternalsForTesting)module).LastController;
				try
				{
					AssertEquals(ControllerIDs.WhsConfigProduct, controller.ID);
				}
				finally
				{
					controller.LastShownForm.Dispose();
				}
			}
		}

		public void TestShowNewForm_SetsPartNum()
		{
			using (var module = new ProductModule())
			{
				var filterBizO = module.FilterBusinessObject;
				((ModuleTextFilter)filterBizO[OrgSupplierPartFilterStripBusinessObject.Descriptions.ProductCode]).Property = "TEST123";

				var newMenuItem = module.FormActionMenu.Single(m => m.Text == "&New");
				newMenuItem.PerformClick();

				var controller = ((IFilterModuleInternalsForTesting)module).LastController;
				try
				{
					AssertEquals("TEST123", ((OrgSupplierPart)controller.LastShownForm.BusinessEntityForPersistingForm).OP_PartNum);
				}
				finally
				{
					controller.LastShownForm.Dispose();
				}
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsConfigProduct;
		}
	}
}
