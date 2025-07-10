using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestOverrideCustomsOfficeDropEdit()
		{
			using (var form = new ZForm())
			using (var userControl = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var customsOfficeOverrideDropEdit = userControl.FindSingle<ZDropEdit>("CustomsOfficeOverrideDropEdit");
				AssertNotNull("CustomsOfficeOverrideDropEdit exist", customsOfficeOverrideDropEdit);
			}
		}

		public void TestAssessmentDateIncludesTime()
		{
			using (var form = new ZForm())
			using (var userControl = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					var assessmentDateControl = userControl.FindSingle<ZDateEdit>("AssessmentDateEdit");
					AssertEquals("AssessmentDateEdit should have start date with time", ZDateTimePickerFormat.Long, assessmentDateControl.DateTimeFormat);
					var assessmentDateGridStyle = userControl.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle("CEI_DateForDuty") as ZDateEditColumnStyleInfo;
					AssertEquals("EntryInstructionsGrid CEI_DateForDuty should have start date with time", ZDateTimePickerFormat.Long, assessmentDateGridStyle.DateTimeFormat);
				});
			}
		}

		public void TestRCCPermitGrid()
		{
			PermitGridTest("RCCPermitsGrid");
		}

		public void TestDutyRebatePermitGrid()
		{
			PermitGridTest("DutyRebateGrid");
		}

		void PermitGridTest(ZString permitGrid)
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
				{
					userControl.JobDeclaration = declaration;
					form.Controls.Add(userControl);
					form.Show();
					userControl.MainTabControl.SelectedTab = userControl.EntryInstructionDetailsTabPage;
					AssertEquals("EntryInstructionDetailsUserControl is contructed", 1, userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().Count());
					var permitsGrid = userControl.EntryInstructionDetailsTabPage.Find(x => x.Name == permitGrid).FirstOrDefault() as ZGrid;
					AssertNotNull(permitGrid + " is constructed", permitsGrid);
					AssertNotNull("Permit Number", permitsGrid.GetColumnStyle("CY_Code"));
					AssertNotNull("Expiry Date", permitsGrid.GetColumnStyle("ExpiryDate"));
					AssertNotNull("Remaining Value", permitsGrid.GetColumnStyle("RemainingValue"));
					AssertNotNull("Usage Sequance", permitsGrid.GetColumnStyle("CY_Order"));
					AssertNotNull("Permit Type", permitsGrid.GetColumnStyle("PermitType"));
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
					Assert(permitGrid + " not Visible", !permitsGrid.Visible);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
					Assert(permitGrid + " not Visible", !permitsGrid.Visible);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					Assert(permitGrid + " not Visible", !permitsGrid.Visible);
					var importer = Factory.NewWithValidTestData<OrgHeader>();
					declaration.JE_OH_Importer = importer.PK;
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
					Assert(permitGrid + " is Visible", permitsGrid.Visible);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
					Assert(permitGrid + " is Visible", permitsGrid.Visible);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					Assert(permitGrid + " not Visible", !permitsGrid.Visible);
				}
			}
		}

		public void TestEntryInstructionsGridColumnVisibilityOrCaption()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
				{
					userControl.JobDeclaration = declaration;
					form.Controls.Add(userControl);
					form.Show();
					userControl.MainTabControl.SelectedTab = userControl.EntryInstructionDetailsTabPage;
					AssertEquals("EntryInstructionDetailsUserControl is constructed", 1, userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().Count());
					var instrUserControl = userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().First();
					var grid = instrUserControl.EntryInstructionsGrid;
					CombineAssertions("TestEntryInstructionsGridColumnVisibilityOrCaption - ImportByExternalBroker & Export", () =>
					{
						declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
						var columnStyle = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_PreviousMRN);
						AssertEquals("WHS MRN", columnStyle.Caption);
						Assert("CEI_PreviousMRN should always be visible on the CEI Grid", columnStyle.IsVisible);
						Assert("CEI_PreviousMRN should always be available on the CEI Grid", !columnStyle.IsUnavailable);
						columnStyle = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_MRNToBeReplaced);
						Assert("CEI_MRNToBeReplaced should always be visible on the CEI Grid", columnStyle.IsVisible);
						Assert("CEI_MRNToBeReplaced should always be available on the CEI Grid", !columnStyle.IsUnavailable);
						columnStyle = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_ExchangeRateDate);
						Assert("CEI_ExchangeRateDate should be visible on the CEI Grid", columnStyle.IsVisible);
						Assert("CEI_ExchangeRateDate should be unavailable on the CEI Grid", columnStyle.IsUnavailable);
						declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
						columnStyle = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_PreviousMRN);
						AssertEquals("Previous MRN", columnStyle.Caption);
						Assert("CEI_PreviousMRN should always be visible on the CEI Grid", columnStyle.IsVisible);
						Assert("CEI_PreviousMRN should always be available on the CEI Grid", !columnStyle.IsUnavailable);
						columnStyle = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_MRNToBeReplaced);
						Assert("CEI_MRNToBeReplaced should always be visible on the CEI Grid", columnStyle.IsVisible);
						Assert("CEI_MRNToBeReplaced should always be available on the CEI Grid", !columnStyle.IsUnavailable);
						columnStyle = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_ExchangeRateDate);
						Assert("CEI_ExchangeRateDate should be visible on the CEI Grid", columnStyle.IsVisible);
						Assert("CEI_ExchangeRateDate should be available on the CEI Grid", !columnStyle.IsUnavailable);
					});
				}
			}
		}

		public void TestVisibilityOfCEI_ExchangeRateDate()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
				{
					userControl.JobDeclaration = declaration;
					form.Controls.Add(userControl);
					form.Show();
					userControl.MainTabControl.SelectedTab = userControl.EntryInstructionDetailsTabPage;
					AssertEquals("EntryInstructionDetailsUserControl is constructed", 1, userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().Count());
					var instrUserControl = userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().First();
					var exchangeRateDateControl = instrUserControl.Controls.Find("ExchangeRateDateZDateEdit", true).First();
					CombineAssertions("TestEntryInstructionsGridColumnVisibilityOrCaption - ImportByExternalBroker & Export", () =>
					{
						declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
						Assert("CEI_ExchangeRateDate should be invisible", !exchangeRateDateControl.Visible);
						declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
						Assert("CEI_ExchangeRateDate should be visible", exchangeRateDateControl.Visible);
					});
				}
			}
		}

		public void TestUCRDetailsControl()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				using (var userControl = new CustomsBrokerageUserControl())
				{
					userControl.JobDeclaration = declaration;
					form.Controls.Add(userControl);
					form.Show();
					userControl.MainTabControl.SelectedTab = userControl.EntryInstructionDetailsTabPage;
					AssertEquals("EntryInstructionDetailsUserControl is constructed", 1, userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().Count());
					var entryInstructionDetailsUserControl = userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().First();
					var refTypeDropEdit = entryInstructionDetailsUserControl.Controls.Find("RefTypeDropEdit", true).First() as ZDropEdit;
					Assert("Should be visible after the effective date", refTypeDropEdit.Visible);
					var ucrGroupBox = entryInstructionDetailsUserControl.Controls.Find("UCRGroupBox", true).First();
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
					Assert(ucrGroupBox.Visible);

					var ucrOverrideCheckBox = entryInstructionDetailsUserControl.Controls.Find("UCROverrideCheckBox", true).First() as ZCheckBox;
					var refNo = entryInstructionDetailsUserControl.Controls.Find("OrderNumberZTextBox", true).First() as ZTextBox;
					var scopeDropEdit = entryInstructionDetailsUserControl.Controls.Find("ScopeDropEdit", true).First() as ZDropEdit;
					var entityTypeDropEdit = entryInstructionDetailsUserControl.Controls.Find("EntityTypeDropEdit", true).First() as ZDropEdit;
					Assert(ucrOverrideCheckBox.Checked);
					Assert(!refTypeDropEdit.Enabled);
					Assert(!refNo.Enabled);
					Assert(!scopeDropEdit.Enabled);
					Assert(!entityTypeDropEdit.Enabled);

					Assert(entryInstructionDetailsUserControl.Controls.Find("BankCodeDropEdit", true).First().Enabled);
					Assert(entryInstructionDetailsUserControl.Controls.Find("CreditTermDropEdit", true).First().Enabled);
					Assert(entryInstructionDetailsUserControl.Controls.Find("TransactionInfoConvertToLocalCurrencyControl", true).First().Enabled);

					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
					Assert(ucrGroupBox.Visible);
					Assert(ucrOverrideCheckBox.Checked);
					Assert(!refNo.Enabled);
					Assert(!refTypeDropEdit.Enabled);
					Assert(!scopeDropEdit.Enabled);
					Assert(!entityTypeDropEdit.Enabled);

					Assert(entryInstructionDetailsUserControl.Controls.Find("BankCodeDropEdit", true).First().Enabled);
					Assert(entryInstructionDetailsUserControl.Controls.Find("CreditTermDropEdit", true).First().Enabled);
					Assert(entryInstructionDetailsUserControl.Controls.Find("TransactionInfoConvertToLocalCurrencyControl", true).First().Enabled);

					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					Assert(ucrGroupBox.Visible);
					Assert(!ucrOverrideCheckBox.Checked);
					Assert(refNo.Enabled);
					Assert(refTypeDropEdit.Enabled);
					Assert(scopeDropEdit.Enabled);
					Assert(entityTypeDropEdit.Enabled);

					Assert(entryInstructionDetailsUserControl.Controls.Find("BankCodeDropEdit", true).First().Enabled);
					Assert(entryInstructionDetailsUserControl.Controls.Find("CreditTermDropEdit", true).First().Enabled);
					Assert(entryInstructionDetailsUserControl.Controls.Find("TransactionInfoConvertToLocalCurrencyControl", true).First().Enabled);

					declaration.JE_RL_NKFinalDestination = "ZAAAA";
					Assert(ucrGroupBox.Visible);
					declaration.JE_RL_NKFinalDestination = "BWAAA";
					Assert(ucrGroupBox.Visible);
				}
			}
		}

		public void TestToAndFromWarehouseCodeShown()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var inwardEntry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			var inwardDeclaration = inwardEntry.Declaration;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
			OrgHeader header = OrgHeader.New(Factory);
			OrgAddress warehouseAddress1 = header.Addresses.AddNew();
			OrgCusCode warehouseCustomsCode1 = warehouseAddress1.Header.CustomsCodes.AddNew();
			OrgAddress warehouseAddress2 = header.Addresses.AddNew();
			OrgCusCode warehouseCustomsCode2 = warehouseAddress2.Header.CustomsCodes.AddNew();
			warehouseCustomsCode1.OK_CustomsRegNo = "DBNOS 02546";
			warehouseCustomsCode1.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			warehouseCustomsCode1.OK_OA_PremisesAddress = warehouseAddress1.PK;
			warehouseCustomsCode2.OK_CustomsRegNo = "JHBOS 35583";
			warehouseCustomsCode2.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			warehouseCustomsCode2.OK_OA_PremisesAddress = warehouseAddress2.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
			entryInstruction.CEI_Description = "OUT DESC";
			entryInstruction.CEI_OA_Warehouse = warehouseAddress1.PK;
			entryInstruction.CEI_OA_Warehouse2 = warehouseAddress2.PK;
			AssertEquals(warehouseCustomsCode1.OK_CustomsRegNo, entryInstruction.FromWarehouseCode);
			AssertEquals(warehouseCustomsCode2.OK_CustomsRegNo, entryInstruction.ToWarehouseCode);
		}

		public void TestBondedWarehouseMenuItems()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				helper.SetTariffAndSave(helper.Part, "KG");
				helper.SetTariffAndSave(helper.Part2, "KG");
				var inwardEntry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_CEI = inwardEntry.CH_CEI_Instruction;
				inwardInvoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_InvoiceQuantity = 400m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_BondedWhsQuantity = 200m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.JI_CustomsQuantity = 20m;
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_LinePrice = 5000m;
				inwardInvoiceLine2.JI_EngineNumber = "EGN12";
				inwardInvoiceLine2.JI_PrimaryPreference = "EU";
				inwardInvoiceLine2.JI_ROOCert = "ROO32342";
				inwardInvoiceLine2.JI_VIN = "VIN4353";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 5000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				var bondedEntryKey1 = "ENT3243-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_PartNo = helper.Part.OP_PartNum;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
				invoiceLine2.JI_BondedWhsQuantity = 100m;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
					var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().First();
					var grid = instrUserControl.EntryInstructionsGrid;
					grid.Select(0);
					grid.ContextMenu.ShowPopupMenu();
					var bondedWarehouseMenuItem = grid.ContextMenu.MenuItems.FindByText("Inventory Management’");
					AssertEquals("bondedWarehouseMenuItem.Visible", false, bondedWarehouseMenuItem.Visible);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
					MenuItem synchronizeWithBondedWarehouseMenuItem = null;
					MenuItem inventoriesSelectionFromBondedWarehouseMenuItem = null;
					CombineAssertions("Inventory Management’ Menu items are visible", () =>
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
						AssertEquals(typeof(Customs.GUI.InventorySelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
						AssertEquals("Selected Tab", customsBrokerageUserControl.InvoiceLinesTabPage, customsBrokerageUserControl.MainTabControl.SelectedTab);
					});
					CombineAssertions("Synchronize with Inventory", () =>
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.DeclarationTabPage;
						synchronizeWithBondedWarehouseMenuItem.PerformClick();
						AssertInvoiceLine(invoiceLine1, entryInstruction.PK, 50m, "NO", 50m, "NO", 500m, "KG", 5000m, "", "", "");
						AssertInvoiceLine(invoiceLine2, entryInstruction.PK, 100m, "BX", 100m, "BX", 10m, "KG", 2500m, "EGN12", "VIN4353", "ROO32342");
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Selected Tab", customsBrokerageUserControl.DeclarationTabPage, customsBrokerageUserControl.MainTabControl.SelectedTab);
						invoice.JobComInvoiceLines.RemoveAndDeleteAll();
						synchronizeWithBondedWarehouseMenuItem.PerformClick();
						AssertEquals("At least one invoice line is required for this entry instruction in order for Synchronization to work.", UnitTestUserNotification.Instance.LastMessage.Text);
						invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						synchronizeWithBondedWarehouseMenuItem.PerformClick();
						AssertEquals("At least one Invoice Line with valid Previous Entry Details or VIN or Part and Countable Quantity is required.", UnitTestUserNotification.Instance.LastMessage.Text);
						invoiceLine1.JI_PartNo = helper.Part.OP_PartNum;
						invoiceLine1.JI_BondedWhsQuantity = 50m;
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						synchronizeWithBondedWarehouseMenuItem.PerformClick();
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertInvoiceLine(invoiceLine1, entryInstruction.PK, 50m, "NO", 50m, "NO", 500m, "KG", 5000m, "", "", "");
					});
					Assert("Customize Columns Menu Item is present", grid.ContextMenu.MenuItems.Cast<ZMenuItem>().Any(itm => itm.Text == "&Customize Columns"));
				}
			}
		}

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, ZGuid entryInstructionPK, ZDecimal invoiceQty, ZString invoiceUQ, ZDecimal countableQty, ZString countableUQ, ZDecimal customsQty, ZString customsUQ, ZDecimal linePrice, ZString engineNumber, ZString vin, ZString rooCert)
		{
			AssertEquals("invoiceLine.JI_CEI", entryInstructionPK, invoiceLine.JI_CEI);
			AssertEquals("invoiceLine.JI_InvoiceQuantity", invoiceQty, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("invoiceLine.JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
			AssertEquals("invoiceLine.JI_BondedWhsQuantity", countableQty, invoiceLine.JI_BondedWhsQuantity);
			AssertEquals("invoiceLine.JI_BondedWhsUnitQty", countableUQ, invoiceLine.JI_BondedWhsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsUnitQty", customsUQ, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsQuantity", customsQty, invoiceLine.JI_CustomsQuantity);
			AssertEquals("invoiceLine.JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
			AssertEquals("invoiceLine.JI_EngineNumber", engineNumber, invoiceLine.JI_EngineNumber);
			AssertEquals("invoiceLine.JI_VIN", vin, invoiceLine.JI_VIN);
			AssertEquals("invoiceLine.JI_ROOCert", rooCert, invoiceLine.JI_ROOCert);
		}

		void AssertMenuItem(MenuItem menuItem, string text, bool visible)
		{
			AssertEquals("menuItem.Text", text, menuItem.Text);
			AssertEquals("menuItem.Visible", visible, menuItem.Visible);
		}

		public void TestCaseNumberGridColumns()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
				{
					userControl.JobDeclaration = declaration;
					form.Controls.Add(userControl);
					form.Show();
					userControl.MainTabControl.SelectedTab = userControl.EntryInstructionDetailsTabPage;
					AssertEquals("EntryInstructionDetailsUserControl is contructed", 1, userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().Count());
					var instrUserControl = userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().First();
					var grid = instrUserControl.Controls.Find("CaseNumberGrid", true)[0] as ZGrid;
					var columnStyle = grid.GetColumnStyle("Document_Status");
					Assert("Document Status IsUnavailable", !columnStyle.IsUnavailable);
					AssertEquals("Document Status", columnStyle.CaptionResourceString.Caption);
					columnStyle = grid.GetColumnStyle(CaseNumber.Schema.Description);
					Assert("Description IsUnavailable", !columnStyle.IsUnavailable);
					AssertEquals("Status Description", columnStyle.CaptionResourceString.Caption);
					columnStyle = grid.GetColumnStyle(CaseNumber.Schema.CY_Data);
					Assert("Case Number IsUnavailable", !columnStyle.IsUnavailable);
					AssertEquals("Case Number", columnStyle.CaptionResourceString.Caption);
					columnStyle = grid.GetColumnStyle(CaseNumber.Schema.CY_Date);
					Assert("Date Closed IsUnavailable", !columnStyle.IsUnavailable);
					AssertEquals("Date Closed", columnStyle.CaptionResourceString.Caption);
					columnStyle = grid.GetColumnStyle(CaseNumber.Schema.CY_Code);
					Assert("Case Type IsUnavailable", !columnStyle.IsUnavailable);
					AssertEquals("Case Type", columnStyle.CaptionResourceString.Caption);
					AssertEquals((grid.Parent as ZGroupBox).Text, "Case Management");
				}
			}
		}

		public void TestProvisionalPaymentsGridColumns()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
				{
					userControl.JobDeclaration = declaration;
					form.Controls.Add(userControl);
					form.Show();
					userControl.MainTabControl.SelectedTab = userControl.EntryInstructionDetailsTabPage;
					AssertEquals("EntryInstructionDetailsUserControl is contructed", 1, userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().Count());
					var instrUserControl = userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().First();
					var grid = instrUserControl.Controls.Find("ProvisionalPaymentsGrid", true)[0] as ZGrid;
					var columnStyle = grid.GetColumnStyle(CusEntryPayInfo.Schema.C9_IncomingPayResponseNo);
					Assert("Line No. IsVisible", columnStyle.IsVisible);
				}
			}
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
	}
}
