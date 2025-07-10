namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	using System.Windows.Forms;
	using CargoWise.Windows.UI;
	using Enterprise.Environment;
	using Enterprise.Warehouse.Transactions.GUI.Testing;
	using Enterprise.Warehouse.Transactions.Invoicing.Testing;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.Testing;
	using NUnit.Framework;

	[TestedType(typeof(InvoicingModule))]
	public class InvoicingModuleBasherTest : ZModuleBasherTest
	{
		#region TestToolBarButtons

		public void TestToolBarButtons()
		{
			using (InvoicingModule module = (InvoicingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuItem item = null;
				foreach (MenuItem actionItem in module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems)
				{
					if (actionItem.Text == "Invoice All")
					{
						item = actionItem;
						break;
					}
				}
				AssertNotNull("Invoice All menu item exists", item);
				item.PerformClick();
				AssertNotNull(module.GetLastMultiClientForm());
				module.GetLastMultiClientForm().Dispose();
			}
		}

		#endregion

		#region TestMultiClientFormLoadInvoicesOnShowForm

		public void TestMultiClientFormLoadInvoicesOnShowForm()
		{
			var client = Helper.CreateClient("CLIENT1");
			client.OH_IsDebtor = true;

			var whs = Helper.CreateWarehouse("W1", "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client, "P1");
			Helper.CreateProductClientRelationShip(client, part);

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 10m);
			Factory.Save();

			using (InvoicingModule module = (InvoicingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuItem item = null;
				foreach (MenuItem actionItem in module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems)
				{
					if (actionItem.Text == "Invoice All")
					{
						item = actionItem;
						break;
					}
				}
				AssertNotNull("Invoice All menu item exists", item);
				item.PerformClick();
				var form = module.GetLastMultiClientForm();
				var grid = GUITestHelper.FindControl<ZGrid>(form.Controls, "zGrid1");
				var rows = grid.List;
				AssertEquals("Invoices should be loaded.", 1, rows.Count);
				form.Dispose();
			}
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			using (InvoicingModule module = (InvoicingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsInvoicing, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (InvoicingModule module = (InvoicingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (InvoicingModule module = (InvoicingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsInvoicing, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint_Message

		public void TestSecurityCheckPoint_Message()
		{
			Env.Security.WhsInvoicingNew.IsAllowed = false;

			using (var module = new TestInvoicingModule())
			{
				const string expected =
@"Error You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Warehouse -> Periodic Billing -> New
";

				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
				{
					module.ShowNewForm();
					AssertMultilineASCIIEquals("Should have shown the security dialog", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				});
			}
		}

		class TestInvoicingModule : InvoicingModule
		{
			public new IZForm ShowNewForm()
			{
				return base.ShowNewForm();
			}
		}

		#endregion

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = new InvoicingModule())
			{
				Assert("Delete option should be available", module.AllowDelete);
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
			return ModuleIDs.WhsInvoicing;
		}
		protected WhsTestHelperFunctionsInvoice Helper => helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory));
		WhsTestHelperFunctionsInvoice helper;

		#endregion
	}
}
