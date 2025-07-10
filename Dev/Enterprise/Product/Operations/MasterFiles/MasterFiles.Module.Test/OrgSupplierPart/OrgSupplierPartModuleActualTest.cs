using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgSupplierPartModuleActualTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestBulkRelationshipMenuItemRequiresSecurity()
		{
			using (OrgSupplierPartModuleForTesting module = new OrgSupplierPartModuleForTesting())
			{
				//no permission
				Env.Security.CustomsSupplierPartModify.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.BulkRelationshipsChangeMenuItem_Click();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(Env.Security.CustomsSupplierPartModify.ErrorMessageForNotAllowed));

				//with permission
				Env.Security.CustomsSupplierPartModify.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.BulkRelationshipsChangeMenuItem_Click();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBulkDeactivation()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG1");
			var whs = helper.CreateWarehouse("1", "A");
			var whs2 = helper.CreateWarehouse("2", "B");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "Test 1");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "Test 2");
			part2.OP_IsActive = false;
			var part3 = (OrgSupplierPart)helper.CreateProduct(client, "Test 3");
			var part4 = (OrgSupplierPart)helper.CreateProduct(client, "Test 4");
			var part5 = (OrgSupplierPart)helper.CreateProduct(client, "Product");
			var part6 = (OrgSupplierPart)helper.CreateProduct(client, "Test 6");
			var part7 = (OrgSupplierPart)helper.CreateProduct(client, "Test 7");
			Factory.Save();

			helper.CreateStock(whs.PK, client, part6.PK, 10m);

			var receive1 = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receive1, part7.PK, 10m, "A");
			helper.CreateAsnLine(receive1, part7.PK, 10m);
			Factory.Save();

			using (var module = new OrgSupplierPartModuleForTesting())
			{
				var txtFilter = (ModuleTextFilter)module.FilterBusinessObject["Product Code"];
				txtFilter.IsActive = true;
				txtFilter.Property = "Test";
				txtFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				module.BulkDeactivationMenuItem_Click();
				AssertEquals("3 Products Deactivated\r\n\r\n2 Products cannot be deactivated, because they have current stock on hand quantities in the warehouse module or there are ASN Lines on un-finalized receives referencing the product. \r\nYou must remove these quantities first, either using a Warehouse Release or Warehouse Adjustment and finalize/cancel any Warehouse Receives with ASN Lines referencing the product. \r\nPrint a Warehouse Stock on Hand report or use the Warehouse Inventory module to find these stock quantities and use the Warehouse Receive module to find any un-finalized receives.", UnitTestUserNotification.Instance.LastMessage.Text);

				part1.Reload();
				AssertEquals(false, part1.OP_IsActive);
				part2.Reload();
				AssertEquals(false, part2.OP_IsActive);
				part3.Reload();
				AssertEquals(false, part3.OP_IsActive);
				part4.Reload();
				AssertEquals(false, part4.OP_IsActive);
				AssertEquals("Not deactivated because doesn't match filter", true, part5.OP_IsActive);
				AssertEquals("Not deactivated because has stock", true, part6.OP_IsActive);
				AssertEquals("Not deactivated because has unfinalised Receive is referencing the product.", true, part7.OP_IsActive);
			}
		}

		#region TestSupportsWorkflow

		public void TestSupportsWorkflow()
		{
			using (var module = new OrgSupplierPartModuleForTesting())
			{
				AssertEquals("Should support Worflow", true, module.SupportsWorkflow);
			}
		}

		#endregion

		public void TestBulkDeactivation_DisplaysSummaryWhenNoEligibleProductsDeActivated()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var part = CreatePartWithWhsStock(helper, "2");
			Factory.Save();

			using (OrgSupplierPartModuleForTesting module = new OrgSupplierPartModuleForTesting())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				module.BulkDeactivationMenuItem_Click();
				AssertEquals("0 Products Deactivated\r\n\r\n1 Products cannot be deactivated, because they have current stock on hand quantities in the warehouse module or there are ASN Lines on un-finalized receives referencing the product. \r\nYou must remove these quantities first, either using a Warehouse Release or Warehouse Adjustment and finalize/cancel any Warehouse Receives with ASN Lines referencing the product. \r\nPrint a Warehouse Stock on Hand report or use the Warehouse Inventory module to find these stock quantities and use the Warehouse Receive module to find any un-finalized receives.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Not deactivated because has stock", true, part.OP_IsActive);
			}
		}

		OrgSupplierPart CreatePartWithWhsStock(IWhsTransactionTestHelper helper, string whsName)
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "Test with stock on hand";
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			relation.OU_OP = part.PK;
			var whsPK = helper.CreateWarehouse(whsName, "A").PK;

			Factory.Save(); // Make sure client, product and warehouse are saved to db before creating dockets.

			// add some stock on hand
			helper.CreateStock(whsPK, org.PK, part.PK, 10m);

			return part;
		}

		public void TestImportFromCSVMenuItemRequiresSecurity()
		{
			using (OrgSupplierPartModuleForTesting module = new OrgSupplierPartModuleForTesting())
			{
				_ = module.ContextMenuExposed;
				//no permission
				module.ImportSecurityCheckpoint.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var handler1 = ((IFilterModuleInternalsForTesting)module).ImportMenuItems["Import From CSV"];
				handler1.Invoke(null, EventArgs.Empty);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(module.ImportSecurityCheckpoint.ErrorMessageForNotAllowed));
				var handler2 = ((IFilterModuleInternalsForTesting)module).ImportMenuItems["Import Last Cost From CSV"];
				handler2.Invoke(null, EventArgs.Empty);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(module.ImportSecurityCheckpoint.ErrorMessageForNotAllowed));

				//with permission
				module.ImportSecurityCheckpoint.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				handler1.Invoke(null, EventArgs.Empty);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				ZForm form = ZApplication.GetOpenForms().OfType<ImportProductsFromCSVForm>().Single();
				form.Dispose();
				handler2.Invoke(null, EventArgs.Empty);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				form = ZApplication.GetOpenForms().OfType<ImportLastCostFromCSVForm>().Single();
				form.Dispose();
			}
		}

		[RequiresSTA]
		public void TestPerformSearchWhenDeactivationProcessComplete()
		{
			IWhsTransactionTestHelper helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			for (int i = 0; i < 500; i++)
			{
				if (i == 200 || i == 250)
				{
					OrgSupplierPart part = CreatePartWithWhsStock(helper, "1");
				}
				else
				{
					OrgSupplierPart part1 = Factory.New<OrgSupplierPart>();
					part1.OP_PartNum = "Test " + i.ToString();
				}
			}

			Factory.Save();

			using (ZForm form = new ZForm())
			using (OrgSupplierPartModuleForTesting module = new OrgSupplierPartModuleForTesting())
			{
				OrgSupplierPartFilterStripControl filter = (OrgSupplierPartFilterStripControl)module.EmbeddedControl;
				filter.FirePerformSearch();
				AssertEquals(500, ((BusinessObjectCollection)module.GridCollection).Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				module.BulkDeactivationMenuItem_Click();
				AssertEquals(2, ((BusinessObjectCollection)module.GridCollection).Count);
			}
		}

		public void TestActivate_NotAllProductsActivated()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			part1.OP_IsActive = false;
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TCE";
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			part2.OP_IsActive = false;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "TCE";
			var part3 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT3");
			part3.OP_IsActive = false;
			var barcode3 = part3.PartBarcodes.AddNew();
			barcode3.PH_Barcode = "TestCodeX";
			barcode3.PH_F3_NKPackType = "TCE";
			Factory.Save();

			using (var module = new OrgSupplierPartModuleForTesting())
			{
				var filter = (ModuleTextFilter)module.FilterBusinessObject["Active Status"];
				filter.Property = "Inactive";
				filter.IsActive = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.SetDelegateToCallOnFormClosing((x) =>
				{
					var bulkDeactivationForm = (BulkDeactivationForm)x;
					var activateRadioButton = bulkDeactivationForm.GetControl<ZRadioButton>("ARadioButton");
					activateRadioButton.PerformClick();

					var continueButton = bulkDeactivationForm.GetControl<ZButton>("ContinueBtn");
					continueButton.PerformClick();
				});
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertNoExceptionThrown("No exceptions are thrown", module.BulkDeactivationMenuItem_Click);
				AssertEquals("2 Products Activated\r\n\r\n1 Products cannot be activated, because they either have a duplicate product code or barcode on other products with the same owner.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class OrgSupplierPartModuleForTesting : OrgSupplierPartModule
		{
			public void BulkRelationshipsChangeMenuItem_Click()
			{
				base.BulkRelationshipsChangeMenuItem_Click(null, EventArgs.Empty);
			}

			public void BulkDeactivationMenuItem_Click()
			{
				base.BulkDeactivationMenuItem_Click(null, EventArgs.Empty);
			}

			public new SecurityCheckpoint ImportSecurityCheckpoint => base.ImportSecurityCheckpoint;

			public MenuItem[] ContextMenuExposed { get { return ContextMenu; } }
		}
	}
}
