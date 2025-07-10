using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseForAttachGUI
	{
		public void TestAmendmentSnapshotManagementManuItemsCreator()
		{
			using var ediMenu = new EDIMenuForTest();
			var amendmentSnapshotManagementMenuItemsCreator = typeof(EDIMenu).GetField("amendmentSnapshotManagementMenuItemsCreator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ediMenu);
			AssertNull(amendmentSnapshotManagementMenuItemsCreator);
		}

		public void TestImportNCTSLinesMenuItem()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);

			using (var ediMenu = new EDIMenu())
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclarationWithEntryInstructions>();
				ediMenu.Declaration = declaration;
				var importNCTSLinesMenuItem = ediMenu.MenuItems.FindByText("Import NCTS lines");
				AssertNull(importNCTSLinesMenuItem);

				using (var form = new ZForm())
				{
					form.Menu.MenuItems.Add(ediMenu);
					form.Show();

					var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
					entryInstruction1.CEI_Style = "A";
					entryInstruction1.CEI_Description = "Description 1";
					entryInstruction1.CEI_DisplaySequence = 2;

					var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
					entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
					entryHeader1.EntryNumber = "entry1";

					ediMenu.RefreshMenu();
					importNCTSLinesMenuItem = ediMenu.MenuItems.FindByText("Import NCTS lines");
					AssertNotNull(importNCTSLinesMenuItem);
					AssertEquals(0, importNCTSLinesMenuItem.MenuItems.Count);

					var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
					entryInstruction2.CEI_Style = "B";
					entryInstruction2.CEI_Description = "Description 2";
					entryInstruction2.CEI_DisplaySequence = 1;

					var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
					entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
					entryHeader2.EntryNumber = "entry2";

					ediMenu.RefreshMenu();
					importNCTSLinesMenuItem = ediMenu.MenuItems.FindByText("Import NCTS lines");
					AssertNotNull(importNCTSLinesMenuItem);
					AssertEquals(2, importNCTSLinesMenuItem.MenuItems.Count);
					AssertEquals("1_B_Description 2_entry2", importNCTSLinesMenuItem.MenuItems[0].Text);
					AssertEquals("2_A_Description 1_entry1", importNCTSLinesMenuItem.MenuItems[1].Text);
					AssertEquals(entryInstruction2.PK, importNCTSLinesMenuItem.MenuItems[0].Tag);
					AssertEquals(entryInstruction1.PK, importNCTSLinesMenuItem.MenuItems[1].Tag);

					importNCTSLinesMenuItem.MenuItems[1].PerformClick();
					var lastFormShown = ZFormModaliser.LastFormShownForTest;
					AssertType<EmbeddedModulePopup>("LastFormShown type", lastFormShown);
					AssertEquals("NCTS Transit Movements", lastFormShown.Text);

					declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
					var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
					var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
					entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;

					ediMenu.RefreshMenu();
					AssertEquals(0, importNCTSLinesMenuItem.MenuItems.Count);

					importNCTSLinesMenuItem.PerformClick();
					lastFormShown = ZFormModaliser.LastFormShownForTest;
					AssertType<EmbeddedModulePopup>("LastFormShown type", lastFormShown);
					AssertEquals("NCTS Transit Movements", lastFormShown.Text);
				}
			}
		}

		public void TestLockOrUnlockCustomsFileMenuItems_Visible()
		{
			var collection = new DeclarationLockConfigCollection(null, Factory);

			var config = collection.AddNew();
			config.DeclarationType = "FRM";

			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Declaration;

			AssertMenuItemVisible(AutoEvents.LockForEdit, "lockCustomsFileMenuItem", collection, false);
			AssertMenuItemVisible(AutoEvents.UnlockForEdit, "unlockCustomsFileMenuItem", collection, false);

			AssertMenuItemVisible(AutoEvents.UnlockForEdit, "lockCustomsFileMenuItem", collection, true);
			AssertMenuItemVisible(AutoEvents.LockForEdit, "unlockCustomsFileMenuItem", collection, true);

			config.DeclarationType = "IMP";

			AssertMenuItemVisible(AutoEvents.UnlockForEdit, "lockCustomsFileMenuItem", collection, false);
			AssertMenuItemVisible(AutoEvents.LockForEdit, "unlockCustomsFileMenuItem", collection, false);
		}

		public void TestLockOrUnlockCustomsFileMenuItems_Click()
		{
			var collection = new DeclarationLockConfigCollection(null, Factory);

			var config = collection.AddNew();
			config.DeclarationType = "FRM";

			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Declaration;

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var declaration = CreateBaseJobDeclarationWithInvoiceLines("DP1");

				((ICustomsFileParent)declaration).DeclarationTypeInfo.SetValueFromString("FRM");
				declaration.Logs.RemoveAndDeleteAll();
				declaration.Factory.Save();

				using (var form = new BaseJobDeclarationFormForTest(declaration))
				{
					form.Show();
					Application.DoEvents();

					var menu = form.EDIMenu;
					menu.Declaration = declaration;

					var lockCustomsFileMenuItem = menu.MenuItems.FindByName("lockCustomsFileMenuItem");
					var unlockCustomsFileMenuItem = menu.MenuItems.FindByName("unlockCustomsFileMenuItem");

					lockCustomsFileMenuItem.PerformClick();

					var log = declaration.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.LockForEditCode).First();
					Assert("Should contains the active LCK event.", !log.IsCancelled);

					declaration.Factory.Save();

					unlockCustomsFileMenuItem.PerformClick();

					log = declaration.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.UnlockForEditCode).First();
					Assert("Should contains the actived UCK event.", !log.IsCancelled);
				}
			}
		}

		public void TestImportOrderLinesClick()
		{
			using (var menu = new EDIMenu())
			{
				var dec = Factory.New<BaseJobDeclaration>();
				menu.Declaration = dec;

				var menuItem = menu.MenuItems.FindByText("Data").MenuItems.FindByText("Import Order Lines");
				menuItem.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

				var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

				Order order = Factory.New<Order>();
				order.BuyerPK = org.PK;
				order.SupplierPK = org.PK;
				order.JD_OrderNumber = "1";
				Factory.Save();

				dec.AttachedOrders.Add(order);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dec.ReadOnly = true;
				menuItem.PerformClick();
				AssertEquals("The Declaration is Read-only, this function is not available", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dec.ReadOnly = false;
				dec.HasChanges = true;
				menuItem.PerformClick();
				Assert("Has to save before Importing Order Lines", UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestCreateProductFilesMenuItemClick()
		{
			using (var menu = new EDIMenu())
			{
				var dec = Factory.New<BaseJobDeclaration>();
				menu.Declaration = dec;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var menuItem = menu.MenuItems.FindByText("Create Product Files");
				menuItem.PerformClick();
				AssertEquals("There are no new products that can be saved in this declaration.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				BaseCusClassification classification = Factory.New<BaseCusClassification>();
				classification.CC_LookupCode = "NEWCLASS";
				classification.CC_IsActive = true;
				classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
				Factory.Save();

				dec.Invoices.AddNew();
				BaseJobComInvoiceLine line = dec.Invoices.AddNew().InvoiceLines.AddNew();
				line.JI_PartNo = "123";
				line.JI_CC = classification.PK;
				line.JI_Description = "123";
				line.JI_InvoiceUQ = "KG";
				dec.InvoiceLines.Add(line);
				menuItem.PerformClick();
				AssertEquals("Please save declaration before saving products.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save();
				menuItem.PerformClick();
				AssertEquals("No client means nothing can be saved", "There are no new products that can be saved in this declaration.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateProductFilesMenuItemClickWithoutSecurityRight()
		{
			using (var menu = new EDIMenu())
			{
				var dec = Factory.New<BaseJobDeclaration>();
				menu.Declaration = dec;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.CustomsSupplierPartModifyCustoms.IsAllowed = false;

				var menuItem = menu.MenuItems.FindByText("Create Product Files");
				menuItem.PerformClick();
				AssertContains(Env.Security.CustomsSupplierPartModifyCustoms.DisplayText, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRefreshInvoiceLineProducts()
		{
			using (var menu = new EDIMenu())
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var partPK = SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC").PK;
				var invLine = CreateInvoiceLine(Factory, dec, "PARTNUM", Importer.PK, Supplier.PK);

				menu.Declaration = dec;

				var menuItem = menu.MenuItems.FindByText("Refresh Product Data");

				ChangePartDescription(partPK, "NEWDESC");

				menuItem.PerformClick();
				AssertEquals("Line Description", "NEWDESC", invLine.JI_Description);
			}
		}

		public void TestImportInvoicesClick()
		{
			using (var menu = new EDIMenu())
			{
				var dec = Factory.New<BaseJobDeclaration>();
				menu.Declaration = dec;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dec.ReadOnly = true;
				menu.MenuItems.FindByText("Data").MenuItems.FindByText("Import Invoices").PerformClick();
				AssertEquals("The Declaration is Read-only, this function is not available", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestApportionmentMenuWhenDirty()
		{
			using (var menu = new EDIMenu())
			{
				var dec = BaseJobDeclaration.New(Factory);
				menu.Declaration = dec;

				dec.ApportionmentDirty = true;

				menu.MenuItems.FindByText("Perform Apportionment").PerformClick();
				AssertEquals("Apportionment done and clean", false, dec.ApportionmentDirty);
			}
		}

		public void TestApportionmentMenuWhenClean()
		{
			using (var menu = new EDIMenu())
			{
				var dec = BaseJobDeclaration.New(Factory);
				menu.Declaration = dec;

				dec.ApportionmentDirty = false;

				menu.MenuItems.FindByText("Perform Apportionment").PerformClick();
				AssertEquals("Apportionment is up to date", false, dec.ApportionmentDirty);
			}
		}

		public void TestAddAttachSupportIfAppropriate()
		{
			using (EDIMenu testMenu = new EDIMenu())
			{
				testMenu.Declaration = GetNewJobDeclaration();
				AssertEquals("Commerical Invoices", true, MenuItemsContains(testMenu.MenuItems, "Commercial &Invoices"));
				var commercialInvoicesMenu = testMenu.MenuItems.FindByText("Commercial Invoices");
				AssertEquals("Attach option presents", true, MenuItemsContains(commercialInvoicesMenu.MenuItems, "Attach Commercial Invoices"));
				AssertEquals("Copy option presents", true, MenuItemsContains(commercialInvoicesMenu.MenuItems, "Copy Commercial Invoices"));
			}
		}

		public void TestMenuItems()
		{
			using (var testMenu = new EDIMenu())
			{
				testMenu.Declaration = GetNewMockDeclaration();
				AssertEquals("&Brokerage", testMenu.Text);

				var menuItemsText = new ZStringBuilder();
				testMenu.MenuItems.Cast<MenuItem>().ForEach(x => menuItemsText.Append(x.Text));
				AssertEquals(@"Submit
Commercial &Invoices
Auto Apportion &Weight
Allocate Remaining Weight
Inventory Management
&Copy Previous Invoice Line
Create Product Files
Refresh Product Data
Data
Generate Entries (&Merge)
Perform Apportionment
Expand ALL Lines by their Bills Of Materials
Collapse Bills Of Materials for ALL Lines (Remove Expanded Lines)
Send to Global Manifest
Create Packing List
-
Audit Customs Declaration", menuItemsText.ToStringWithNewLineBetweenAppends());

				AssertEquals("Submit", testMenu.MenuItems[0].Text);
				var commercialInvoicesItem = testMenu.MenuItems[1];
				AssertEquals("Commercial &Invoices", commercialInvoicesItem.Text);
				AssertEquals("&Attach Commercial Invoices", commercialInvoicesItem.MenuItems[0].Text);
				AssertEquals("&Copy Commercial Invoices", commercialInvoicesItem.MenuItems[1].Text);

				AssertEquals("Auto Apportion &Weight", testMenu.MenuItems[2].Text);

				var allocateRemainingWeightMenuItem = testMenu.MenuItems[3];
				AssertEquals("Allocate Remaining Weight", allocateRemainingWeightMenuItem.Text);
				AssertEquals("Allocate Remaining Weight by Price", allocateRemainingWeightMenuItem.MenuItems[0].Text);
				AssertEquals("Allocate Remaining Weight by Quantity", allocateRemainingWeightMenuItem.MenuItems[1].Text);

				AssertEquals("Inventory Management", testMenu.MenuItems[4].Text);
				AssertEquals("&Copy Previous Invoice Line", testMenu.MenuItems[5].Text);
				AssertEquals("Create Product Files", testMenu.MenuItems[6].Text);
				AssertEquals("Refresh Product Data", testMenu.MenuItems[7].Text);
				var dataMenuItem = testMenu.MenuItems[8];
				AssertEquals("Data", dataMenuItem.Text);
				AssertEquals("Data sub menu items", "Import Invoices", dataMenuItem.MenuItems[0].Text);
				AssertEquals("Data sub menu items", "Import Order Lines", dataMenuItem.MenuItems[1].Text);
				AssertEquals("Data sub menu items", "Export Declaration to XML", dataMenuItem.MenuItems[2].Text);
				AssertEquals("Generate Entries (&Merge)", testMenu.MenuItems[9].Text);
				AssertEquals("Perform Apportionment", testMenu.MenuItems[10].Text);
			}
		}

		public void TestBondedWarehouseMenuItemsForMultiEntry()
		{
			using (var menu = new EDIMenu())
			{
				var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
				var declarationMockProtected = declarationMock.Protected();
				declarationMockProtected.Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
				declarationMockProtected.Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
				declarationMockProtected.Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(false);
				declarationMockProtected.Setup<bool>("ShouldUpdateOutwardLinesWithInventoryDetailsCore").Returns(false);
				declarationMockProtected.Setup<bool>("IsInventorySelectionEnabledCore").Returns(false);
				declarationMock.Setup(x => x.HasLineGoingIntoAnAutomatedBondedWarehouse).Returns(true);

				var declaration = declarationMock.Object;
				var helperMock = new Mock<BondedWarehousingHelper>(declaration) { CallBase = true };
				helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>()).Returns(true);
				helperMock.Protected().Setup<bool>("HasBondedWarehouseEntryDetailsCore",
					ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(),
					ItExpr.IsAny<bool>()).Returns(true);

				declarationMockProtected.Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var helper = new WhsDataTestHelper(Factory);
				var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction1.CEI_Description = "DESC 1";
				entryInstruction1.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
				helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;
				var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction2.CEI_Description = "DESC 2";
				entryInstruction2.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;

				var entry1Mock = Factory.NewMoq<CusEntryHeader>();
				entry1Mock.Protected().Setup<bool>("GetHasLinesForInwardBondedWarehousing").Returns(false);
				entry1Mock.Protected().Setup<bool>("IsInwardBondedWarehousingEnabledCore").Returns(false);
				entry1Mock.Protected().Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(false);
				var entry1 = entry1Mock.Object;
				declaration.CustomsEntryHeaders.Add(entry1);
				entry1.CH_CEI_Instruction = entryInstruction1.PK;
				entry1.CH_BGMReference = "BGM1223";
				entry1.EntryNumber = "ENT1231";
				var entry1Line = entry1.MergedLines.AddNew();
				var entry2Mock = Factory.NewMoq<CusEntryHeader>();
				entry2Mock.Protected().Setup<bool>("GetHasLinesForInwardBondedWarehousing").Returns(false);
				entry2Mock.Protected().Setup<bool>("IsInwardBondedWarehousingEnabledCore").Returns(false);
				entry2Mock.Protected().Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(false);
				var entry2 = entry2Mock.Object;
				declaration.CustomsEntryHeaders.Add(entry2);
				entry2.CH_CEI_Instruction = entryInstruction2.PK;
				entry2.CH_BGMReference = "BGM5865";
				entry2.EntryNumber = "ENT65644";
				var entry2Line = entry2.MergedLines.AddNew();

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1Mock = Factory.NewMoq<BaseJobComInvoiceLine>();
				invoiceLine1Mock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(declaration.SupportsBondedWarehousing);
				var invoiceLine1 = invoiceLine1Mock.Object;
				invoiceLine1.JI_JZ = invoice.PK;
				declaration.InvoiceLines.Add(invoiceLine1);
				invoiceLine1.JI_CEI = entryInstruction1.PK;
				invoiceLine1.JI_CL = entry1Line.PK;
				invoiceLine1.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				var invoiceLine2Mock = Factory.NewMoq<BaseJobComInvoiceLine>();
				invoiceLine2Mock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(declaration.SupportsBondedWarehousing);
				var invoiceLine2 = invoiceLine2Mock.Object;
				invoiceLine2.JI_JZ = invoice.PK;
				declaration.InvoiceLines.Add(invoiceLine2);
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine2.JI_CL = entry2Line.PK;
				invoiceLine2.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;

				menu.Declaration = declaration;
				CombineAssertions(() =>
				{
					menu.RefreshMenu();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", false, bondedWarehouseMenuItem.Visible);

					entry2Mock.Protected().Setup<bool>("GetHasLinesForInwardBondedWarehousing").Returns(true);
					entry2Mock.Protected().Setup<bool>("IsInwardBondedWarehousingEnabledCore").Returns(true);
					entryInstruction2.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
					entryInstruction2.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
					invoiceLine2.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
					menu.RefreshMenu();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);

					AssertEquals("bondedWarehouseMenuItem.MenuItems.Count", 10, bondedWarehouseMenuItem.MenuItems.Count);
					var entryMenuItem1 = bondedWarehouseMenuItem.MenuItems[0];
					AssertMenuItem(entryMenuItem1, entry2.EntryHeaderDescriptiveMenuItemText, true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[1], "S&elect Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[2], "&Synchronize with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[3], "Update Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[4], "Cancel Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[5], "&Finalize Stock with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[6], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[7], "&Disable Integration", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[8], "Synchronize With Orders", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[9], "Select &Order Lines", false);

					AssertEquals("entryMenuItem1.Visible", true, entryMenuItem1.Visible);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					entry2.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					entry2.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", false);
					declarationMock.Setup(x => x.HasLineGoingIntoAnAutomatedBondedWarehouse).Returns(false);
					declarationMockProtected.Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(true);
					entry2Mock.Protected().Setup<bool>("IsInwardBondedWarehousingEnabledCore").Returns(false);
					entry2Mock.Protected().Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(true);
					entry1Mock.Protected().Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(true);
					entryInstruction1.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
					entryInstruction1.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
					invoiceLine1.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
					entryInstruction2.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
					invoiceLine2.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
					entry2.CH_WarehouseTransactionStatus = ZString.Empty;
					menu.RefreshMenu();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);

					AssertEquals("bondedWarehouseMenuItem.MenuItems.Count", 11, bondedWarehouseMenuItem.MenuItems.Count);
					entryMenuItem1 = bondedWarehouseMenuItem.MenuItems[0];
					AssertMenuItem(entryMenuItem1, entry1.EntryHeaderDescriptiveMenuItemText, true);
					var entryMenuItem2 = bondedWarehouseMenuItem.MenuItems[1];
					AssertMenuItem(entryMenuItem2, entry2.EntryHeaderDescriptiveMenuItemText, true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[2], "S&elect Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[3], "&Synchronize with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[4], "Update Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[5], "Cancel Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[6], "&Finalize Stock with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[7], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[8], "&Disable Integration", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[9], "Synchronize With Orders", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[10], "Select &Order Lines", false);

					AssertEquals("entryMenuItem1.Visible", true, entryMenuItem1.Visible);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					AssertEquals("entryMenuItem2.Visible", true, entryMenuItem2.Visible);
					entryMenuItem2.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem2.MenuItems.Count", 6, entryMenuItem2.MenuItems.Count);
					AssertMenuItem(entryMenuItem2.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem2.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem2.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					entry1.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated;
					AssertEquals("entryMenuItem1.Visible", true, entryMenuItem1.Visible);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", true);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					entry1.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
					AssertEquals("entryMenuItem1.Visible", true, entryMenuItem1.Visible);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", true);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					AssertEquals("entryMenuItem2.Visible", true, entryMenuItem2.Visible);
					entryMenuItem2.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem2.MenuItems.Count", 6, entryMenuItem2.MenuItems.Count);
					AssertMenuItem(entryMenuItem2.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem2.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem2.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					declarationMockProtected.Setup<bool>("ShouldUpdateOutwardLinesWithInventoryDetailsCore").Returns(true);
					declarationMockProtected.Setup<bool>("IsInventorySelectionEnabledCore").Returns(true);
					Factory.InvalidateCachedProperties();
					menu.RefreshMenu();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					AssertEquals("bondedWarehouseMenuItem.MenuItems.Count", 11, bondedWarehouseMenuItem.MenuItems.Count);
					entryMenuItem1 = bondedWarehouseMenuItem.MenuItems[0];
					AssertMenuItem(entryMenuItem1, entry1.EntryHeaderDescriptiveMenuItemText, true);
					entryMenuItem2 = bondedWarehouseMenuItem.MenuItems[1];
					AssertMenuItem(entryMenuItem2, entry2.EntryHeaderDescriptiveMenuItemText, true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[2], "S&elect Inventory", true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[3], "&Synchronize with Inventory", true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[4], "Update Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[5], "Cancel Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[6], "&Finalize Stock with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[7], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[8], "&Disable Integration", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[9], "Synchronize With Orders", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[10], "Select &Order Lines", false);
				});
			}
		}

		public void TestSubmitToCustomsWare()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Ireland, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			using (CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestCustomsWareCompany"))
			{
				BaseJobDeclaration dec = GetNewMockDeclaration();
				dec.JE_TransportMode = "AIR";
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;

				using (var form = new BaseJobDeclarationFormForTest(dec))
				{
					var menu = form.EDIMenu;
					menu.Declaration = dec;
					var menuItem = menu.MenuItems.FindByText("Submit");
					menu.RefreshMenu();
					AssertEquals(true, menuItem.Visible);
					AssertEquals(false, menu.MenuItems.FindByText("Generate Entries (&Merge)").Visible);
					AssertEquals(false, menu.MenuItems.FindByText("Perform Apportionment").Visible);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					AssertEquals(true, menu.Declaration.HasChanges);
					menuItem.PerformClick();
					AssertEquals("Declaration Not Submitted.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					AssertEquals("pre-condition", false, menu.Declaration.IsInDatabase);
					menuItem.PerformClick();
					AssertEquals(true, menu.Declaration.IsInDatabase);
					AssertEquals("called", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var menu = new EDIMenuForTest())
			{
				menu.Declaration = GetNewMockDeclaration();
				menu.RefreshMenu();
				var menuItem = menu.MenuItems.FindByText("Submit");
				menuItem.PerformSelect();
				AssertEquals(false, menuItem.Visible);
			}
		}

		public void TestSendToGlobalManifestMenuItem()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = "INP";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1m;
				var log = declaration.Logs.AddNew(Events.TransferFromManifestToCustoms, "MK322423");

				using (var form = new BaseJobDeclarationFormForTest(declaration))
				{
					var menu = form.EDIMenu;
					menu.Declaration = declaration;
					var sendToGlobalManifestMenuItem = menu.MenuItems.FindByText("Send to Global Manifest");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					sendToGlobalManifestMenuItem.PerformClick();
					AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, declaration.HasChanges);
					AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.TransferFromCustomsToManifest));
					AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.DataExport));

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					sendToGlobalManifestMenuItem.PerformClick();
					AssertEquals(false, declaration.HasChanges);
					declaration.Logs.GetAllLogs().Reload(true);
					var dexLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
					AssertNotNull(dexLog);
					AssertEquals("Error - Match couldn't be found for AsycudaManifest with Key MK322423", UnitTestUserNotification.Instance.LastMessage.Text);

					var header = Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
					header.AMA_JobReference = "MK322423";
					Factory.Save();

					sendToGlobalManifestMenuItem.PerformClick();
					var dexLog2 = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
					AssertNotNull(dexLog2);
					AssertNotEquals(dexLog, dexLog2);
					AssertEquals("Data sent to Global Manifest 'MK322423'.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotNull(((BusinessObject)header).GetLogs().MostRecentLogByEventTime(Events.DataImport));
				}
			}
		}

		public void TestPreSaveDeclarationShouldEnsureBothDeclarationAndShipmentAreSaved()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = CreateBaseJobDeclarationWithInvoiceLines("DP1");
			declaration.JE_JS = shipment.PK;

			using (var form = new BaseJobDeclarationFormForTest(declaration))
			{
				var menu = form.EDIMenu;
				menu.Declaration = declaration;
				menu.RefreshMenu();
				((EDIMenuForTest)menu).SetUniversalCustomsMessagingRecipientID("TESTCUSTOMS");

				declaration.HasChanges = true;
				shipment.HasChanges = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((EDIMenuForTest)menu).PreSaveDeclaration_Exposed(declaration);
				AssertEquals("PreSaveDeclaration should not pass as both declaration and shipment have changed.", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.HasChanges = false;
				shipment.HasChanges = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((EDIMenuForTest)menu).PreSaveDeclaration_Exposed(declaration);
				AssertEquals("PreSaveDeclaration should not pass as shipment have changed.", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.HasChanges = false;
				shipment.HasChanges = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((EDIMenuForTest)menu).PreSaveDeclaration_Exposed(declaration);
				AssertNullOrEmpty("PreSaveDeclaration should pass as neither declaration nor shipment have changed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEnableMenuItemsAfterRefresh()
		{
			var declaration = CreateBaseJobDeclarationWithInvoiceLines("DP");

			try
			{
				Env.Security.LockOrUnlockFileForEdit.IsAllowed = false;
				using (var form = new BaseJobDeclarationFormForTest(declaration))
				{
					var menu = form.EDIMenu;

					form.DisplayMode = ODisplayMode.Edit;
					menu.ShowPopupMenu();
					var alwaysDisabledMenuItem = menu.MenuItems.FindByText("AlwaysDisabledMenuItem");
					Assert("If a menu item is set as disabled, it should keep disabled even the form is editable.", !alwaysDisabledMenuItem.Enabled);
					Assert("All the menu items should be enabled except the AlwaysDisabledMenuItem.", menu.MenuItems.Cast<MenuItem>().Except(alwaysDisabledMenuItem).All(x => x.Enabled));

					form.DisplayMode = ODisplayMode.ReadOnly;
					menu.ShowPopupMenu();
					Assert("All menu items should be disabled.", menu.MenuItems.Cast<MenuItem>().All(x => !x.Enabled));
				}
			}
			finally
			{
				Env.Security.LockOrUnlockFileForEdit.ClearOverriddenSecurityValue();
			}
		}

		public void TestPackingListMenuItemShouldBeCreatedByDefault()
		{
			using (var testMenu = new EDIMenu())
			{
				testMenu.Declaration = GetNewJobDeclaration();
				AssertEquals("&Brokerage", testMenu.Text);
				AssertNotNull(testMenu.MenuItems.FindByText("Create Packing List"));
			}
		}

		public void TestPackingListMenuItemCaption()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			using (var testMenu = new EDIMenuForTest())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertNotNull(testMenu.MenuItems.FindByText("Create Packing List"));
				AssertNull(testMenu.MenuItems.FindByText("Edit Packing List"));
			}

			declaration.LoadOrCreateCusPackingList(Factory);
			using (var testMenu = new EDIMenuForTest())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertNotNull(testMenu.MenuItems.FindByText("Create Packing List"));
				AssertNull(testMenu.MenuItems.FindByText("Edit Packing List"));
			}

			Factory.Save();
			using (var testMenu = new EDIMenuForTest())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertNull(testMenu.MenuItems.FindByText("Create Packing List"));
				AssertNotNull(testMenu.MenuItems.FindByText("Edit Packing List"));
			}
		}

		public void TestUnlockDoMergeMutex()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new BaseJobDeclarationFormForTest(declaration))
			{
				Assert(!declaration.DoMergeMutex.IsLocked);
				declaration.LockDoMergeMutex();
				Assert(declaration.DoMergeMutex.IsLocked);
			}
			Assert(!declaration.DoMergeMutex.IsLocked);
		}

		public void TestCreatePackingListOnlyAfterDeclarationSaved()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "SEA";
			var query = new ZQuery(CusPackingListSchema.CUL_JE, declaration.PK);
			var packingList = Factory.LoadTop1<CusPackingList>(query);
			AssertNull(packingList);
			using (var testMenu = new EDIMenuForTest())
			{
				testMenu.Declaration = declaration;
				var createPackingListItem = testMenu.MenuItems.FindByText("Create Packing List");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				createPackingListItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(packingList);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createPackingListItem.PerformClick();
				var openedForm = (testMenu.LastController.LastShownForm as ZForm);
				var entity = openedForm.BusinessEntity as CusPackingList;
				entity.PackageJob.Packages.First().KP_MarksAndNumbers = "marks and numbers";
				openedForm.FireSaveButton();
				packingList = Factory.LoadTop1<CusPackingList>(query);
				AssertNotNull(packingList);
				(testMenu.LastController.LastShownForm as ZForm).Close();
			}
		}

		public void TestCreatePackingListAddsDefaultPackage()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			using (var testMenu = new EDIMenuForTest())
			{
				testMenu.Declaration = declaration;
				var createPackingListItem = testMenu.MenuItems.FindByText("Create Packing List");
				createPackingListItem.PerformClick();
				var openedForm = (testMenu.LastController.LastShownForm as ZForm);
				var entity = openedForm.BusinessEntity as CusPackingList;
				AssertEquals(1, entity.PackageJob.Packages.Count);
				(testMenu.LastController.LastShownForm as ZForm).Close();
			}
		}

		public void TestCreatePackingListMenuItemClick_HasNoSecurity()
		{
			CombineAssertions(() =>
			{
				AssertCreatePackingListMenuItemClick_InCertainSecurity(true, true);
				AssertCreatePackingListMenuItemClick_InCertainSecurity(true, false);
				AssertCreatePackingListMenuItemClick_InCertainSecurity(false, true);
				AssertCreatePackingListMenuItemClick_InCertainSecurity(false, false);
			});
		}

		public void TestBondedWarehouseShortCutMenuItems()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var quantity = 100m;
				var helper = new WhsDataTestHelper(Factory);
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;

				var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
				declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
				var declaration = declarationMock.Object;

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = helper.Importer.PK;
				declaration.JE_DeclarationReference = "B0000123";
				declaration.JE_CustomsOffice = "BFN";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

				var invoice = declaration.Invoices.AddNew();

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_Style = "01";
				entryInstruction1.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = "02";
				entryInstruction2.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;

				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction1.PK;
				invoiceLine1.JI_PartNo = helper.Part.OP_PartNum;
				invoiceLine1.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				invoiceLine1.JI_InvoiceQuantity = quantity;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_BondedWhsQuantity = quantity;
				invoiceLine1.JI_BondedWhsUnitQty = "NO";
				invoiceLine1.JI_CustomsUnitQty = "KG";
				invoiceLine1.JI_CustomsQuantity = quantity * 10m;
				invoiceLine1.JI_LinePrice = quantity * 100m;

				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine2.JI_PartNo = helper.Part.OP_PartNum;
				invoiceLine2.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				invoiceLine2.JI_InvoiceQuantity = quantity;
				invoiceLine2.JI_InvoiceUQ = "NO";
				invoiceLine2.JI_BondedWhsQuantity = quantity;
				invoiceLine2.JI_BondedWhsUnitQty = "NO";
				invoiceLine2.JI_CustomsUnitQty = "KG";
				invoiceLine2.JI_CustomsQuantity = quantity * 10m;
				invoiceLine2.JI_LinePrice = quantity * 100m;

				declaration.DoMerge();

				AssertEquals(2, declaration.ActiveEntryHeaders.Count);
				declaration.ActiveEntryHeaders[0].EntryNumber = "ENT3243";
				declaration.ActiveEntryHeaders[1].EntryNumber = "ENT3245";

				using (var menu = new EDIMenu())
				{
					menu.Declaration = declaration;
					menu.RefreshMenu();
					CombineAssertions("Test for EDI menu that does not support shortCut menus", () =>
					{
						var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
						bondedWarehouseMenuItem.ShowPopupMenu();
						var index = 0;
						AssertMultilineASCIIEquals("Visible Menus", @"0 01 - ENT3243=True
1 02 - ENT3245=True
2 S&elect Inventory=False
3 &Synchronize with Inventory=False
4 Update Inventory=False
5 Cancel Inventory=False
6 &Finalize Stock with Inventory=False
7 &Cancel Inventory Stock Release=False
8 &Disable Integration=False
9 Synchronize With Orders=False
10 Select &Order Lines=False", string.Join(System.Environment.NewLine, bondedWarehouseMenuItem.MenuItems.OfType<MenuItem>().Select(x => $"{index++} {x.Text}={x.Visible}")));

						var entry1MenuItem = bondedWarehouseMenuItem.MenuItems[0];
						AssertEquals("Entry1 menu", entry1MenuItem.Text, "01 - ENT3243");
						AssertEquals("Entry1 sub menu", entry1MenuItem.MenuItems[0].Text, "&Update Inventory");

						var entry2MenuItem = bondedWarehouseMenuItem.MenuItems[1];
						AssertEquals("Entry2 menu", entry2MenuItem.Text, "02 - ENT3245");
						AssertEquals("Entry2 sub menu", entry2MenuItem.MenuItems[0].Text, "&Update Inventory");

						invoiceLine1.JI_Procedure = ZString.Empty;
						invoiceLine2.JI_Procedure = ZString.Empty;

						bondedWarehouseMenuItem.ShowPopupMenu();
						index = 0;
						AssertMultilineASCIIEquals("Visible Menus", @"0 S&elect Inventory=False
1 &Synchronize with Inventory=False
2 Update Inventory=False
3 Cancel Inventory=False
4 &Finalize Stock with Inventory=False
5 &Cancel Inventory Stock Release=False
6 &Disable Integration=False
7 Synchronize With Orders=False
8 Select &Order Lines=False", string.Join(System.Environment.NewLine, bondedWarehouseMenuItem.MenuItems.OfType<MenuItem>().Select(x => $"{index++} {x.Text}={x.Visible}")));
						invoiceLine1.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
						invoiceLine2.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
					});
				}

				entryInstruction1.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;
				entryInstruction2.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;
				var mockMenu = new Mock<EDIMenu>();
				mockMenu.Protected().Setup<bool>("SupportShortCutBondedWarehouseMenus").Returns(true);
				using (var menu = mockMenu.Object)
				{
					menu.Declaration = declaration;
					menu.RefreshMenu();
					CombineAssertions("Test for EDI menu that supports shortCut menus", () =>
					{
						var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
						bondedWarehouseMenuItem.ShowPopupMenu();
						var index = 0;
						AssertMultilineASCIIEquals("Visible Menus", @"0 &Update Bonded Warehouse - 01 - ENT3243=True
1 &Update Bonded Warehouse - 02 - ENT3245=True
2 S&elect Inventory=False
3 &Synchronize with Inventory=False
4 Update Inventory=False
5 Cancel Inventory=False
6 &Finalize Stock with Inventory=False
7 &Cancel Inventory Stock Release=False
8 &Disable Integration=False
9 Synchronize With Orders=False
10 Select &Order Lines=False
11 01 - ENT3243=True
12 02 - ENT3245=True", string.Join(System.Environment.NewLine, bondedWarehouseMenuItem.MenuItems.OfType<MenuItem>().Select(x => $"{index++} {x.Text}={x.Visible}")));

						AssertEquals("Entry1 shortCut menu appears at the top", bondedWarehouseMenuItem.MenuItems[0].Text, "&Update Bonded Warehouse - 01 - ENT3243");
						AssertEquals("Entry2 shortCut menu appears behind Entry1 shortCut", bondedWarehouseMenuItem.MenuItems[1].Text, "&Update Bonded Warehouse - 02 - ENT3245");

						var entry1MenuItem = bondedWarehouseMenuItem.MenuItems[11];
						AssertEquals("Entry1 menu appears at the bottom", entry1MenuItem.Text, "01 - ENT3243");
						AssertEquals("Entry1 sub menu", entry1MenuItem.MenuItems[0].Text, "&Update Inventory");

						var entry2MenuItem = bondedWarehouseMenuItem.MenuItems[12];
						AssertEquals("Entry2 menu", entry2MenuItem.Text, "02 - ENT3245");
						AssertEquals("Entry2 sub menu", entry2MenuItem.MenuItems[0].Text, "&Update Inventory");

						invoiceLine1.JI_Procedure = ZString.Empty;
						invoiceLine2.JI_Procedure = ZString.Empty;

						bondedWarehouseMenuItem.ShowPopupMenu();
						index = 0;
						AssertMultilineASCIIEquals("Visible Menus", @"0 S&elect Inventory=False
1 &Synchronize with Inventory=False
2 Update Inventory=False
3 Cancel Inventory=False
4 &Finalize Stock with Inventory=False
5 &Cancel Inventory Stock Release=False
6 &Disable Integration=False
7 Synchronize With Orders=False
8 Select &Order Lines=False", string.Join(System.Environment.NewLine, bondedWarehouseMenuItem.MenuItems.OfType<MenuItem>().Select(x => $"{index++} {x.Text}={x.Visible}")));
					});
				}
			}
		}

		public void TestCopyCommercialInvoiceMenuItem()
		{
			using (Env.SetTemporaryUserContext(User.SupportUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using (EDIMenu ediMenu = new EDIMenu())
				{
					AssertEquals(false, ediMenu.copyCommercialInvoiceMenuItem.Visible);
					var declaration = BaseJobDeclaration.New(Factory);
					AssertEquals(true, declaration.EnableCopyCommercialInvoice);
					Factory.Save();

					ediMenu.Declaration = declaration;
					AssertEquals(true, ediMenu.copyCommercialInvoiceMenuItem.Visible);
				}
			}
		}

		public void TestCommercialInvoiceMenuItem()
		{
			using (Env.SetTemporaryUserContext(User.SupportUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using (EDIMenu ediMenu = new EDIMenu())
				{
					var commercialInvoiceMenuItem = ediMenu.commercialInvoiceMenuItem;
					AssertEquals(true, ediMenu.MenuItems.Contains(commercialInvoiceMenuItem));
					AssertEquals(false, commercialInvoiceMenuItem.Visible);

					var declaration = BaseJobDeclaration.New(Factory);
					AssertEquals(true, declaration.EnableCommercialInvoiceMenuItem);
					AssertEquals(false, commercialInvoiceMenuItem.Visible);
					Factory.Save();

					ediMenu.Declaration = declaration;
					AssertEquals(true, ediMenu.MenuItems.Contains(ediMenu.commercialInvoiceMenuItem));
					AssertEquals(true, commercialInvoiceMenuItem.Visible);
				}
			}
		}

		public void TestAuditMenuClicked()
		{
			using (Env.SetTemporaryUserContext(User.SupportUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using (EDIMenu eDImenu = new EDIMenu())
				{
					var declaration = BaseJobDeclaration.New(Factory);
					Factory.Save();

					eDImenu.Declaration = declaration;
					var auditMenu = (WriteToLogMenuItem)eDImenu.MenuItems.FindByText("Audit Customs Declaration");
					auditMenu.PerformClick();
					AssertEquals(128, WriteToLogFormInvoker.LoggerRefMaxLengthForTesting);
				}
			}
		}

		public void TestUpdateBondedWarehouseMenuItemVisible()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			using (EDIMenu menu = new EDIMenu())
			using (ZForm form = new ZForm())
			{
				menu.Declaration = BaseJobDeclaration.New(Factory);
				menu.Declaration.SetSupportsBondedWarehousingForTesting(false);

				form.Menu.MenuItems.Add(menu);
				form.Show();

				menu.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				menu.bondedWarehouseMenuItem.ShowPopupMenu();
				AssertEquals("Invisible", false, menu.updateBondedWarehouseMenuItem.Visible);

				menu.Declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
				menu.bondedWarehouseMenuItem.ShowPopupMenu();
				AssertEquals("Visible", false, menu.updateBondedWarehouseMenuItem.Visible);

				menu.Declaration.SetSupportsBondedWarehousingForTesting(true);
				menu.bondedWarehouseMenuItem.ShowPopupMenu();
				AssertEquals("Visible", true, menu.updateBondedWarehouseMenuItem.Visible);

				menu.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				menu.bondedWarehouseMenuItem.ShowPopupMenu();
				AssertEquals("Invisible", false, menu.updateBondedWarehouseMenuItem.Visible);

				menu.Declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
				menu.bondedWarehouseMenuItem.ShowPopupMenu();
				AssertEquals("Visible", true, menu.updateBondedWarehouseMenuItem.Visible);
			}
		}

		public void TestDisableBondedWarehouseIntegrationMenuItemVisible()
		{
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = BaseJobDeclaration.New(Factory);
				menu.Declaration.SetSupportsBondedWarehousingForTesting(true);
				menu.bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals("Disable Integration is visible", true, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
				var entryHeader1 = menu.Declaration.CustomsEntryHeaders.AddNew();
				menu.bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals("Disable Integration is visible", true, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
				entryHeader1.CH_HasManualWhsUpdate = true;
				AssertEquals("Prerequisite: CusEntryHeader HasManualWhsUpdate", true, entryHeader1.CH_HasManualWhsUpdate);
				menu.bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals("Disable Integration is invisible", false, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
			}
		}

		public void TestDataTransferImplReturnsBase()
		{
			using (EDIMenu menu = new EDIMenu())
			{
				AssertEquals("Menu.DataTransferImpl is DataTransferImplementation", typeof(DataTransferImpl), menu.DataTransferImpl.GetType());
			}
		}

		public void TestQueueforConsolidationMenuItemVisibility()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				AssertNull("Consolidated Entry is not created in base EDIMenu.  Needs to be explicitly implemented.", consolidatedEntryMenuItem);
			}

			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				menu.Declaration = declaration;
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				AssertNull("Consolidated Entry is not created when registry disabled (default)", consolidatedEntryMenuItem);
			}

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				menu.Declaration = declaration;
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				var dequeueFromConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("dequeueFromConsolidationMenuItem");
				AssertEquals("ConsolidatedEntry visible", true, consolidatedEntryMenuItem.Visible);
				AssertEquals("QueueForConsolidation visible", true, queueForConsolidationMenuItem.Visible);
				AssertEquals("DequeueForConsolidation visible", true, dequeueFromConsolidationMenuItem.Visible);
			}
		}

		void AssertMenuItemVisible(Event eventType, string menuName, DeclarationLockConfigCollection configures, bool isVisible)
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			((ICustomsFileParent)declaration).DeclarationTypeInfo.SetValueFromString("FRM");

			var log = declaration.Logs.AddNew(eventType, ZDateTimeOffset.Now);
			log.IsCancelled = false;

			declaration.Factory.Save();

			var originalValue = Env.Security.LockOrUnlockFileForEdit.IsAllowed;
			var securityAction = new DisposableAction(() => { Env.Security.LockOrUnlockFileForEdit.IsAllowed = true; }, () => { Env.Security.LockOrUnlockFileForEdit.IsAllowed = originalValue; });

			using (securityAction)
			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configures))
			using (var form = new BaseJobDeclarationFormForTest(declaration))
			{
				form.Show();
				Application.DoEvents();

				var menu = form.EDIMenu;
				menu.Declaration = declaration;

				var menuItem = menu.MenuItems.FindByName(menuName);
				AssertEquals(isVisible, menuItem != null && menuItem.Visible);
			}
		}

		void AssertMenuItem(MenuItem menuItem, string text, bool visible)
		{
			AssertEquals("menuItem.Text", text, menuItem.Text);
			AssertEquals("menuItem.Visible", visible, menuItem.Visible);
		}

		void AssertCreatePackingListMenuItemClick_InCertainSecurity(bool editIsAllowed, bool viewIsAllowed)
		{
			var editIsAllowedOrigin = Env.Security.CustomsPackingListEdit.IsAllowed;
			var viewIsAllowedOrigin = Env.Security.CustomsPackingListView.IsAllowed;
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var mutex = new ZGlobalMutex(MutexIDs.CusPackingListMutex, declaration.PK.ToString());

			using (new DisposableAction(() =>
			{
				Env.Security.CustomsPackingListEdit.IsAllowed = editIsAllowed;
				Env.Security.CustomsPackingListView.IsAllowed = viewIsAllowed;
			}, () =>
			{
				Env.Security.CustomsPackingListEdit.IsAllowed = editIsAllowedOrigin;
				Env.Security.CustomsPackingListView.IsAllowed = viewIsAllowedOrigin;
			}))
			{
				using (var form = new BaseJobDeclarationFormForTest(declaration))
				{
					form.Show();
					var testMenu = form.EDIMenu;
					testMenu.Declaration = declaration;
					testMenu.RefreshMenu();
					if (editIsAllowed)
					{
						AssertNoExceptionThrown($"PackingList isn't in database, CustomsPackingListEdit.IsAllowed = {editIsAllowed} and CustomsPackingListView.IsAllowed = {viewIsAllowed}", () =>
						{
							testMenu.MenuItems.FindByText("Create Packing List").PerformClick();
						});

						var openedForm = (testMenu.LastController.LastShownForm as ZForm);

						AssertType<PackingListForm>("PackingList Form popped up", openedForm);
						Assert("Should lock for creating new Packing List", mutex.IsLocked);

						openedForm.Close();
						Assert("Should unlock after Packing List form closed", !mutex.IsLocked);
					}
					else
					{
						AssertExceptionThrown<SecurityAccessDeniedException>($"PackingList isn't in database, CustomsPackingListEdit.IsAllowed = {editIsAllowed} and CustomsPackingListView.IsAllowed = {viewIsAllowed}", () =>
						{
							try
							{
								testMenu.MenuItems.FindByText("Create Packing List").PerformClick();
							}
							catch (RethrownByExceptionHandlerException ex)
							{
								throw ex.InnerException;
							}
						});

						AssertNull("PackingList Form does not popped up", testMenu.LastController);
						Assert("Should not unlock after exception thrown", !mutex.IsLocked);
					}
				}
				declaration.LoadOrCreateCusPackingList(Factory);
				Factory.Save();
				using (var form = new BaseJobDeclarationFormForTest(declaration))
				{
					form.Show();
					var testMenu = form.EDIMenu;
					testMenu.Declaration = declaration;
					testMenu.RefreshMenu();
					AssertNoExceptionThrown($"PackingList is in database, CustomsPackingListEdit.IsAllowed = {editIsAllowed} and CustomsPackingListView.IsAllowed = {viewIsAllowed}", () =>
					{
						testMenu.MenuItems.FindByText("Edit Packing List").PerformClick();
					});

					var openedForm = (testMenu.LastController.LastShownForm as ZForm);
					if (editIsAllowed || viewIsAllowed)
					{
						AssertType<PackingListForm>("PackingList Form popped up", openedForm);
						openedForm.Close();
					}
					else
					{
						AssertNull("PackingList Form does not popped up", openedForm);
					}
				}
			}
		}

		BaseJobDeclaration CreateBaseJobDeclarationWithInvoiceLines(ZString depCode)
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_CEI = instruction.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = depCode;
			department.GE_Warehouse = true;
			department.GE_CustomsBrokerage = true;

			var job = new JobHeader.Loader(declaration).TryCreate();
			job.JH_GE = department.PK;

			return declaration;
		}

		#region Product Refresh

		OrgHeader Importer
		{
			get { return importer ?? (importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS")); }
		}
		OrgHeader importer;

		OrgHeader Supplier
		{
			get { return supplier ?? (supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABABEU")); }
		}
		OrgHeader supplier;

		BaseJobComInvoiceLine CreateInvoiceLine(BusinessObjectFactory factory, BaseJobDeclaration declaration, ZString partNum, ZGuid importerPK, params ZGuid[] supplierPKs)
		{
			declaration.JE_OH_Importer = importerPK;

			BaseJobComInvoiceLine result = null;
			var firstHeader = true;
			foreach (ZGuid supplierPK in supplierPKs)
			{
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				if (firstHeader)
				{
					result = invoiceHeader.JobComInvoiceLines.AddNew();
				}

				invoiceHeader.JZ_OH_Supplier = supplierPK;
				firstHeader = false;
			}
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(factory, result.PartSyncManagerActiveDeciderPK);
			result.JI_PartNo = partNum;
			return result;
		}

		Business.OrgSupplierPart SaveNewPart(BusinessObjectFactory factory, ZString partNum, OrgHeader importer, OrgHeader supplier, ZString description)
		{
			var newPart = factory.New<Business.OrgSupplierPart>();
			if (importer != null)
			{
				newPart.RelatedOrganisations.AddOwner(importer);
			}
			if (supplier != null)
			{
				newPart.RelatedOrganisations.AddSupplier(supplier);
			}
			newPart.OP_PartNum = partNum;
			newPart.OP_Desc = description;
			factory.Save();
			return newPart;
		}

		void ChangePartDescription(ZGuid partPK, ZString newDescription)
		{
			var factory = new BusinessObjectFactory();
			var part = factory.Load<Business.OrgSupplierPart>(partPK);
			part.OP_Desc = newDescription;
			factory.Save();
		}

		#endregion
	}

	sealed class BaseJobDeclarationFormForTest : BaseJobDeclarationForm
	{
		public BaseJobDeclarationFormForTest()
			: base()
		{
		}

		public BaseJobDeclarationFormForTest(BaseJobDeclaration declaration)
			: base(declaration)
		{
		}

		public EDIMenu EDIMenu => (EDIMenu)TopLevelMenu;

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenuForTest();

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				TopLevelMenu.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	sealed class EDIMenuForTest : EDIMenu
	{
		protected override ZString Submit() => "called";

		ZString universalCustomsMessagingRecipientID;

		public void SetUniversalCustomsMessagingRecipientID(ZString value) => universalCustomsMessagingRecipientID = value;

		public bool PreSaveDeclaration_Exposed(BaseJobDeclaration declaration) => PreSaveDeclaration(declaration);

		protected override JobDeclarationUniversalMessagingHelper GetJobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent wrapper)
		{
			var result = new JobDeclarationUniversalMessagingHelperForTest(wrapper);
			result.SetUniversalCustomsMessagingRecipientID(universalCustomsMessagingRecipientID);
			return result;
		}

		ZMenuItem alwaysDisabledMenuItem;

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			alwaysDisabledMenuItem = new ZMenuItem("AlwaysDisabledMenuItem");
			MenuItems.Add(alwaysDisabledMenuItem);
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			alwaysDisabledMenuItem.Enabled = false;
		}

		protected override bool DisplayCreatePackingListMenuOption => true;
	}

	sealed class JobDeclarationUniversalMessagingHelperForTest : JobDeclarationUniversalMessagingHelper
	{
		ZString universalCustomsMessagingRecipientID;

		public void SetUniversalCustomsMessagingRecipientID(ZString value)
		{
			universalCustomsMessagingRecipientID = value;
		}

		protected override ZString UniversalCustomsMessagingRecipientID => universalCustomsMessagingRecipientID;

		public JobDeclarationUniversalMessagingHelperForTest(IJobDeclarationMessageSendingObjectParent messageSendingObject) : base(messageSendingObject)
		{
		}
	}
}
