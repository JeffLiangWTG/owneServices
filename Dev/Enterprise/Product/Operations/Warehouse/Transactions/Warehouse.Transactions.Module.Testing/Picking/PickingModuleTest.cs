using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickingModule))]
	internal class PickingModuleTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (PickingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsPicking, module.ID);
			}
		}

		#endregion

		#region TestSupportsWorkflow

		public void TestSupportsWorkflow()
		{
			using (var module = new PickingModule())
			{
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (PickingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (PickingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsPicking, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestNewMenuItems

		public void TestNewMenuItems()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = new PickingModule())
			{
				var newMenu = module.FormActionMenu.FindByText("&New");
				AssertEquals("There should be 2 New menu items.", 2, newMenu.MenuItems.Count);
				AssertEquals("New Pick For Available Inventory", newMenu.MenuItems[0].Text);
				AssertEquals("New Pick For Held Inventory", newMenu.MenuItems[1].Text);
			}
		}

		public void TestNewMenuItems_HeldGoodsForOrdersRegistryDisabled()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new PickingModule())
			{
				var newMenu = module.FormActionMenu.FindByText("&New");
				AssertEquals("There shouldn't be any New menu items.", 0, newMenu.MenuItems.Count);
			}
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = new PickingModule())
			{
				Assert("Delete option should not be available", !module.AllowDelete);
			}
		}

		#endregion

		#region TestPrintingDocument_ChangesStatusAndSave

		public void TestPrintingDocument_ChangesStatusAndSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition", 10m, pick.GetAllPickLines().Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition", PickStatus.Codes.Created, pick.WP_PickStatus);

			using (var pickModule = new PickingModule())
			using (var form = (ZForm)pickModule.ShowPopup())
			{
				form.Show();
				pickModule.PerformSearch_ForTest();

				var grid = pickModule.DisplayGrid;
				grid.SelectAllElements();

				EventHandler handler = null;
				handler = (o, e) =>
				{
					using (var documentsMenuItem = grid.ContextMenu.MenuItems.FindByText("Documents"))
					{
						documentsMenuItem.OnPopup(EventArgs.Empty);

						var pickingSlipMenuItem = documentsMenuItem.MenuItems.FindByText("Picking Slip");
						pickingSlipMenuItem.PerformClick();

						grid.ContextMenu.Popup -= handler;
						grid.ContextMenu.Dispose();
					}
				};

				grid.ContextMenu.Popup += handler;
				grid.ContextMenu.Show(grid, grid.Location);
			}

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Pick status should have been changed and saved to DB.", PickStatus.Codes.PickSlip, pickInOtherFactory.WP_PickStatus);
		}

		#endregion

		#region TestIOperationalActionSupportable

		public void TestIOperationalActionSupportable()
		{
			using (var pickingModule = new PickingModule())
			{
				AssertNotNull(pickingModule.Plugins.GetPlugin(ControllerIDs.OperationalActions));
				AssertEquals(typeof(PickOperationalActionsSupporter), ((IOperationalActionSupportable)pickingModule).OperationalActionSupporter.GetType());
			}
		}

		#endregion

		#region Implementation

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsPicking;
		}

		#region Helper

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));

		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
