using System.Linq;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(VASOrderModule))]
	public class VASOrderModuleTest : ZModuleBasherTest
	{
		#region TestController

		public void TestController()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var newMenuItem = ((ZFilterGridModule)module).FormActionMenu.Single(m => m.Text == "&New");
				newMenuItem.PerformClick();

				var controller = ((IFilterModuleInternalsForTesting)module).LastController;
				AssertEquals(typeof(VASOrderController), controller.GetType());
				controller.LastShownForm.Dispose();
			}
		}

		#endregion

		#region TestFilterBusinessObject

		public void TestFilterBusinessObject()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(VASOrderFilterBusinessObject), ((IFilterModuleInternalsForTesting)module).FilterBusinessObject.GetType());
			}
		}

		#endregion

		#region TestGridCollection

		public void TestGridCollection()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(WhsVASOrderCollection), ((IFilterModuleInternalsForTesting)module).GridCollection.GetType());
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsVASOrder, module.ID);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsVASOrder, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestSupportsWorkflow

		public void TestSupportsWorkflow()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsVASOrder;
		}

		#endregion
	}
}
