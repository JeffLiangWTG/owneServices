using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(AdjustmentModule))]
	class AdjustmentModuleTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (AdjustmentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsAdjustment, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (AdjustmentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (AdjustmentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsAdjustment, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestControllersDefinedForAllCountriesModuleDefinedOn

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		#endregion

		#region TestNewMenuItems

		public void TestNewMenuItems()
		{
			using (var module = new AdjustmentModule())
			{
				var newMenu = module.FormActionMenu.FindByText("&New");
				AssertEquals("There should be 4 menu items.", 4, newMenu.MenuItems.Count);
				AssertEquals("New Adjustment", newMenu.MenuItems[0].Text);
				AssertEquals("New Ownership Adjustment", newMenu.MenuItems[1].Text);
				AssertEquals("New Internal Warehouse Adjustment", newMenu.MenuItems[2].Text);
				AssertEquals("New Customs Amendment Adjustment", newMenu.MenuItems[3].Text);
			}
		}

		#endregion

		#region TestIOperationalActionSupportable

		public void TestIOperationalActionSupportable()
		{
			using (var adjustmentModule = new AdjustmentModule())
			{
				AssertNotNull(adjustmentModule.Plugins.GetPlugin(ControllerIDs.OperationalActions));
				AssertEquals(typeof(AdjustmentOperationalActionSupporter), ((IOperationalActionSupportable)adjustmentModule).OperationalActionSupporter.GetType());
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsAdjustment;
		}

		#endregion
	}
}
