using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.GUI.Testing
{
	[AsycudaCustomsCountries(Core.Constants.CountryCodes.Namibia)]
	public class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestSplitContainer1_SplitterDistance()
		{
			using (var form = new ZForm())
			using (var userControl = new BaseEntryInstructionDetailsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					var splitContainer1 = userControl.FindSingle<KSplitContainer>("splitContainer1");
					AssertEquals("SplitterDistance", 80, splitContainer1.SplitterDistance);
					AssertEquals("Panel1MinSize", 80, splitContainer1.Panel1MinSize);
				});
			}
		}

		public void TestAssessmentDateIncludesTime()
		{
			using (var form = new ZForm())
			using (var userControl = new BaseEntryInstructionDetailsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					var assessmentDateControl = userControl.FindSingle<ZDateEdit>("AssessmentDateEdit");
					AssertEquals("AssessmentDateEdit should have start date with time", ZDateTimePickerFormat.Long, assessmentDateControl.DateTimeFormat);
					var assessmentDateGridStyle = userControl.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle("CEI_DateForDuty") as ZDateEditColumnStyleInfo;
					AssertEquals("EntryInstructionsGrid CEI_ValuationDate should have start date with time", ZDateTimePickerFormat.Long, assessmentDateGridStyle.DateTimeFormat);
				});
			}
		}

		public void TestBondedWarehouseMenuItems()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				Helper.UniversalTariffHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ", Core.Constants.CountryCodes.Namibia);
				Helper.UniversalTariffHelper.CreateCusCodeList(Core.Constants.CountryCodes.Namibia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", new ZDateTime(2020, 01, 01), new ZDateTime(2079, 01, 01));
				Helper.SetTariffAndSave(Helper.Part, "KG");
				Helper.SetTariffAndSave(Helper.Part2, "KG");
				var inwardEntry = Helper.GetNewEntryHeader("IMP", "B00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m, Helper.InwardCusProcedure.ZZ6_Group, Core.Constants.CurrencyCodes.Namibia);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				inwardDeclaration.DoMerge();
				Factory.Save();
				Enterprise.Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishShipmentForWHSInward(inwardEntry, false);
				Enterprise.Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishAcceptEventForWHSInwardAndSaveIfNeeded(inwardEntry, true);
				var bondedEntryKey1 = "ENT3243-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_OP = Helper.Part.PK;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				invoiceLine1.JI_CustomsUnitQty = "KG";
				invoiceLine1.JI_Procedure = Helper.InwardCusProcedure.ZZ6_ProcedureCode + Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;

				using (var form = new BaseJobDeclarationForm(declaration))
				{
					form.Show();
					var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
					customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
					var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<BaseEntryInstructionDetailsUserControl>().First();
					var grid = instrUserControl.EntryInstructionsGrid;
					grid.Select(0);

					grid.ContextMenu.ShowPopupMenu();
					var bondedWarehouseMenuItem = grid.ContextMenu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", false, bondedWarehouseMenuItem.Visible);

					declaration.JE_MessageType = "EXW";
					instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<BaseEntryInstructionDetailsUserControl>().First();
					grid = instrUserControl.EntryInstructionsGrid;
					grid.Select(0);
					MenuItem synchronizeWithBondedWarehouseMenuItem = null;
					MenuItem inventoriesSelectionFromBondedWarehouseMenuItem = null;
					CombineAssertions("Inventory Management Menu items are visible", () =>
					{
						grid.ContextMenu.ShowPopupMenu();
						AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);
						AssertEquals("bondedWarehouseMenuItem.MenuItems.Count", 2, bondedWarehouseMenuItem.MenuItems.Count);
						synchronizeWithBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems[0];
						AssertMenuItem(synchronizeWithBondedWarehouseMenuItem, "&Synchronize with Inventory", true);
						inventoriesSelectionFromBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems[1];
						AssertMenuItem(inventoriesSelectionFromBondedWarehouseMenuItem, "S&elect Inventory", true);
					});

					CombineAssertions("Inventory Selection", () =>
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						inventoriesSelectionFromBondedWarehouseMenuItem.PerformClick();
						AssertEquals(typeof(InventorySelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
						AssertEquals("Selected Tab", customsBrokerageUserControl.InvoiceLinesTabPage, customsBrokerageUserControl.MainTabControl.SelectedTab);
					});

					CombineAssertions("Synchronize with Inventory", () =>
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.DeclarationTabPage;
						synchronizeWithBondedWarehouseMenuItem.PerformClick();
						Helper.AssertInvoiceLine(invoiceLine1, entryInstruction.PK, 50m, "NO", 50m, "NO", 500m, "KG", 0, "", 5000m, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode);
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Selected Tab", customsBrokerageUserControl.DeclarationTabPage, customsBrokerageUserControl.MainTabControl.SelectedTab);

						invoice.JobComInvoiceLines.RemoveAndDeleteAll();
						synchronizeWithBondedWarehouseMenuItem.PerformClick();
						AssertEquals("At least one invoice line is required for this entry instruction in order for Synchronization to work.", UnitTestUserNotification.Instance.LastMessage.Text);

						invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
						invoiceLine1.JI_CEI = entryInstruction.PK;
						invoiceLine1.JI_Procedure = Helper.OutwardCusProcedure.ZZ6_ProcedureCode + Helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						synchronizeWithBondedWarehouseMenuItem.PerformClick();
						AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Countable Quantity is required.", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		public void TestDetailsUserControlVisibility_Default()
		{
			var mock = new Mock<BaseEntryInstructionDetailsUserControl>();
			mock.CallBase = true;

			using (var form = new ZForm())
			using (var userControl = mock.Object)
			{
				form.Controls.Add(userControl);
				form.Show();

				var detailsPanel = (ZPanel)userControl.Controls.Find("DetailsPanel", true)[0];
				var detailsGroupBox = detailsPanel.Controls.Find("DetailsGroupBox", true)[0];
				var detailsUserControl = detailsPanel.Controls.Find("DetailsUserControl", true)[0];

				CombineAssertions(() =>
				{
					AssertEquals("By default the detailsGroupBox should be visible", true, detailsGroupBox.Visible);
					AssertEquals("By default the DetailsUserControl should be hidden", false, detailsUserControl.Visible);
				});
			}
		}

		public void TestDetailsUserControlVisibility_Layout()
		{
			var mock = new Mock<BaseEntryInstructionDetailsUserControl>();
			mock.CallBase = true;
			mock.Protected().Setup<Type>("GetDetailsUserControlType").Returns(typeof(LayoutEntryInstructionDetailBasicUserControl));

			using (var form = new ZForm())
			using (var userControl = mock.Object)
			{
				form.Controls.Add(userControl);
				form.Show();

				var detailsPanel = (ZPanel)userControl.Controls.Find("DetailsPanel", true)[0];
				var detailsGroupBox = detailsPanel.Controls.Find("DetailsGroupBox", true)[0];
				var detailsUserControl = detailsPanel.Controls.Find("DetailsUserControl", true)[0];

				CombineAssertions(() =>
				{
					AssertEquals("When a layout is used the detailsGroupBox should be hidden", false, detailsGroupBox.Visible);
					AssertEquals("When a layout is used the DetailsUserControl should be visible", true, detailsUserControl.Visible);
				});
			}
		}

		static void AssertMenuItem(MenuItem menuItem, string text, bool visible)
		{
			AssertEquals("menuItem.Text", text, menuItem.Text);
			AssertEquals("menuItem.Visible", visible, menuItem.Visible);
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;
	}
}
