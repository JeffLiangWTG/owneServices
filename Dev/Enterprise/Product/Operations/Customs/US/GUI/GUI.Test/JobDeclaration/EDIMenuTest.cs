using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Business.WarehouseExtensions.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;
using MessageSendingNotificationCollection = Enterprise.Customs.Business.MessageSendingNotificationCollection;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.GUI
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestNoBIRDMenusWhenDrawback()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Factory.Save();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(false, menu.bIRDExportMenu.Visible);
			}
		}

		public void TestNoExceptionOnShipmentBrokerageMenu()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (BrokeragePlugIn plugin = new BrokeragePlugIn(shipment))
			{
				plugin.Enabled = true;
				EDIMenu menu = (EDIMenu)plugin.TopLevelMenu;
				AssertNoExceptionThrown(() => menu.OnPopup(EventArgs.Empty));
			}
		}

		public void TestAddAuditResetToOriginalClicked()
		{
			using (Env.SetTemporaryUserContext(User.SupportUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				declaration.US_EnableENS = true;
				declaration.US_EnableCRL = true;
				declaration.Logs.AddNew(AutoEvents.CustomsCleared);
				var ensEntry = declaration.CustomsEntryHeaders.AddNew();
				ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
				var seEntry = declaration.CustomsEntryHeaders.AddNew();
				seEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
				seEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				declaration.Factory.Save();
				using (var formForTest = new FormForTestMoq(declaration))
				{
					var mockMenu = formForTest.MockMenu;
					var menu = formForTest.MenuForTest;
					menu.Declaration = declaration;
					menu.sendEntrySummaryQueryMenu.PerformClick();
					AssertEquals("Shoud have both ENS and SE EntryHeaders", true, declaration.CustomsEntryHeaders.Count > 0);
					using (EDIMenu eDImenu = new EDIMenu())
					{
						eDImenu.Declaration = declaration;
						AssertEquals("Should be visible for IMP", true, eDImenu.resetToOriginalMenu.Visible);
						var resetMenu = eDImenu.MenuItems.FindByText(EDIMenu.Constants.ResetToOriginal);
						resetMenu.PerformClick();
						AssertEquals("JE_MessageStatus should be blank", string.Empty, declaration.JE_MessageStatus);
						AssertEquals("CusEntryHeader.CH_Status should be blank", string.Empty, declaration.CustomsEntryHeaders[0].CH_Status);
						AssertEquals("CusEntryHeader.CH_Status should be blank", string.Empty, declaration.CustomsEntryHeaders[1].CH_Status);
					}
				}

				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				declaration.Factory.Save();
				AssertEquals("CargoReleaseType should be ACS", CargoReleaseTypeList.Codes.ACS, declaration.US_CargoReleaseType);
				AssertNoErrors(declaration);
			}
		}

		public void TestAuditMenuClicked()
		{
			using (Env.SetTemporaryUserContext(User.SupportUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using (EDIMenu eDImenu = new EDIMenu())
				{
					var declaration = Factory.New<JobDeclaration>();
					Factory.Save();
					eDImenu.Declaration = declaration;
					var auditMenu = (WriteToLogMenuItem)eDImenu.MenuItems.FindByText("Audit Customs Declaration");
					auditMenu.PerformClick();
					AssertEquals(128, WriteToLogFormInvoker.LoggerRefMaxLengthForTesting);
					var moreAuditsMenu = eDImenu.MenuItems.FindByText("More Audits");
					var spiAuditMenu = (WriteToLogMenuItem)moreAuditsMenu.MenuItems.FindByText("Audit SPI");
					spiAuditMenu.PerformClick();
					AssertEquals(128, WriteToLogFormInvoker.LoggerRefMaxLengthForTesting);
					var fdaAuditMenu = (WriteToLogMenuItem)moreAuditsMenu.MenuItems.FindByText("Audit FDA");
					fdaAuditMenu.PerformClick();
					AssertEquals(128, WriteToLogFormInvoker.LoggerRefMaxLengthForTesting);
					var censusWarningAuditMenu = (WriteToLogMenuItem)moreAuditsMenu.MenuItems.FindByText("Audit FDA");
					censusWarningAuditMenu.PerformClick();
					AssertEquals(128, WriteToLogFormInvoker.LoggerRefMaxLengthForTesting);
				}
			}
		}

		public void TestImportInvoicesMenuItem_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_ConsolACE = true;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var importInvoicestMenu = form.Menu.MenuItems.FindByText("Brokerage").MenuItems.FindByText(EDIMenu.Constants.ImportBulkDeclarations);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				importInvoicestMenu.PerformClick();
				var popupForm = (EmbeddedModulePopup)ZFormModaliser.GetActiveChildFormForParentForm(form);
				AssertEquals("OK button strategy", typeof(JobDeclarationBulkImportPopupOKButtonStrategy), popupForm.EmbeddedModulePopupOKButtonStrategy.GetType());
			}
		}

		public void TestSendOriginalValidationForBondedWarehouse()
		{
			SetupBondedWarehouseEnvironment(true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.WarehouseDocAddress.Delete();
			declaration.Invoices.DeleteAll();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.US_WHSEntryLineNo = ZShort.Zero;
			invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
			declaration.DoMerge();
			Factory.Save();
			using (var form = new FormForTestMoq(declaration))
			{
				var mockMenu = form.MockMenu;
				var menu = form.MenuForTest;
				mockMenu
					.Protected()
					.Setup<bool>("ShowImportMessageSendingActionForm", ItExpr.IsAny<ImportMessageSendingActionCollection>())
					.Returns((ImportMessageSendingActionCollection actions) =>
					{
						actions.IsCancelled = false;
						actions[0].US_SendMessage = true;
						return true;
					});
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendOriginalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, true, true, true, true, false, false, false);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
				invoiceLine.JI_PartNo = Part.OP_PartNum;
				invoiceLine.JI_InvoiceQuantity = 1m;
				invoiceLine.JI_InvoiceUQ = "NO";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendOriginalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, false, true, false, false);
				invoiceLine.JI_BondedWhsQuantity = 1m;
				invoiceLine.JI_InvoiceUQ = "";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendOriginalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, true, false, false, false);
				invoiceLine.JI_InvoiceUQ = "NO";
				invoiceLine.JI_InvoiceQuantity = 0m;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendOriginalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, true, false, false, false);
				mockMenu
					.Protected()
					.Verify<bool>("ShowImportMessageSendingActionForm", Times.Exactly(4), ItExpr.IsAny<ImportMessageSendingActionCollection>());
			}

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.Invoices.DeleteAll();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.US_WHSEntryLineNo = ZShort.Zero;
			invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
			AssertEquals(true, declaration.IsExBondAutomationEnabled);
			declaration.DoMerge();
			Factory.Save();
			using (var form = new FormForTestMoq(declaration))
			{
				var mockMenu = form.MockMenu;
				var menu = form.MenuForTest;
				mockMenu
					.Protected()
					.Setup<bool>("ShowImportMessageSendingActionForm", ItExpr.IsAny<ImportMessageSendingActionCollection>())
					.Returns((ImportMessageSendingActionCollection actions) =>
					{
						actions.IsCancelled = false;
						actions[0].US_SendMessage = true;
						return true;
					});
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendOriginalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, true, true, true, false, true, true);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
				declaration.US_WHSEntryFilerCode = "XJ5";
				declaration.US_WHSEntryNumber = "ENT32434";
				invoiceLine.JI_PartNo = Part.OP_PartNum;
				invoiceLine.JI_InvoiceQuantity = 1m;
				invoiceLine.US_WHSEntryLineNo = 1;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendOriginalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, false, false, false, false);
				mockMenu
					.Protected()
					.Verify<bool>("ShowImportMessageSendingActionForm", Times.Exactly(2), ItExpr.IsAny<ImportMessageSendingActionCollection>());
			}
		}

		public void TestSendAmendmentValidationForBondedWarehouse()
		{
			SetupBondedWarehouseEnvironment(true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.WarehouseDocAddress.Delete();
			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.US_WHSEntryLineNo = ZShort.Zero;
			invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			using (var form = new FormForTestMoq(declaration))
			{
				var mockMenu = form.MockMenu;
				var menu = form.MenuForTest;
				mockMenu
					.Protected()
					.Setup<bool>("ShowImportMessageSendingActionForm", ItExpr.IsAny<ImportMessageSendingActionCollection>())
					.Returns((ImportMessageSendingActionCollection actions) =>
					{
						actions.IsCancelled = false;
						actions[0].US_SendMessage = true;
						return true;
					});
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendAmendmentMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, true, true, true, true, false, false, false);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
				invoiceLine.JI_PartNo = Part.OP_PartNum;
				invoiceLine.JI_InvoiceQuantity = 1m;
				invoiceLine.JI_InvoiceUQ = "NO";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendAmendmentMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, false, true, false, false);
				invoiceLine.JI_BondedWhsQuantity = 1m;
				invoiceLine.JI_InvoiceUQ = "";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendAmendmentMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, true, false, false, false);
				invoiceLine.JI_InvoiceUQ = "NO";
				invoiceLine.JI_InvoiceQuantity = 0m;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendAmendmentMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, true, false, false, false);
				mockMenu
					.Protected()
					.Verify<bool>("ShowImportMessageSendingActionForm", Times.Exactly(4), ItExpr.IsAny<ImportMessageSendingActionCollection>());
			}

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.Invoices.DeleteAll();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.US_WHSEntryLineNo = ZShort.Zero;
			invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
			AssertEquals(true, declaration.IsExBondAutomationEnabled);
			declaration.DoMerge();
			Factory.Save();
			using (var form = new FormForTestMoq(declaration))
			{
				var mockMenu = form.MockMenu;
				var menu = form.MenuForTest;
				mockMenu
					.Protected()
					.Setup<bool>("ShowImportMessageSendingActionForm", ItExpr.IsAny<ImportMessageSendingActionCollection>())
					.Returns((ImportMessageSendingActionCollection actions) =>
					{
						actions.IsCancelled = false;
						actions[0].US_SendMessage = true;
						return true;
					});
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendAmendmentMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, true, true, true, false, true, true);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
				declaration.US_WHSEntryFilerCode = "XJ5";
				declaration.US_WHSEntryNumber = "ENT32434";
				invoiceLine.JI_PartNo = Part.OP_PartNum;
				invoiceLine.JI_InvoiceQuantity = 1m;
				invoiceLine.US_WHSEntryLineNo = 1;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendAmendmentMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, false, false, false, false);
				mockMenu
					.Protected()
					.Verify<bool>("ShowImportMessageSendingActionForm", Times.Exactly(2), ItExpr.IsAny<ImportMessageSendingActionCollection>());
			}
		}

		public void TestSendDeletionValidationForBondedWarehouse()
		{
			SetupBondedWarehouseEnvironment(true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.WarehouseDocAddress.Delete();
			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.US_WHSEntryLineNo = ZShort.Zero;
			invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			using (var form = new FormForTestMoq(declaration))
			{
				var mockMenu = form.MockMenu;
				var menu = form.MenuForTest;
				mockMenu
					.Protected()
					.Setup<bool>("ShowImportMessageSendingActionForm", ItExpr.IsAny<ImportMessageSendingActionCollection>())
					.Returns((ImportMessageSendingActionCollection actions) =>
					{
						actions.IsCancelled = false;
						actions[0].US_SendMessage = true;
						return true;
					});
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendWithdrawalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, true, true, false, false, false, false, false);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendWithdrawalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, false, false, false, false);
				mockMenu
					.Protected()
					.Verify<bool>("ShowImportMessageSendingActionForm", Times.Exactly(2), ItExpr.IsAny<ImportMessageSendingActionCollection>());
			}

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.Invoices.DeleteAll();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.US_WHSEntryLineNo = ZShort.Zero;
			invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
			AssertEquals(true, declaration.IsExBondAutomationEnabled);
			declaration.DoMerge();
			Factory.Save();
			using (var form = new FormForTestMoq(declaration))
			{
				var mockMenu = form.MockMenu;
				var menu = form.MenuForTest;
				mockMenu
					.Protected()
					.Setup<bool>("ShowImportMessageSendingActionForm", ItExpr.IsAny<ImportMessageSendingActionCollection>())
					.Returns((ImportMessageSendingActionCollection actions) =>
					{
						actions.IsCancelled = false;
						actions[0].US_SendMessage = true;
						return true;
					});
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendWithdrawalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, true, false, false, false, false, false);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.sendWithdrawalMenu.PerformClick();
				AssertBondedWarehouseRequiredFieldsMessage(UnitTestUserNotification.Instance.LastMessage.Text, false, false, false, false, false, false, false);
				mockMenu
					.Protected()
					.Verify<bool>("ShowImportMessageSendingActionForm", Times.Exactly(2), ItExpr.IsAny<ImportMessageSendingActionCollection>());
			}
		}

		public void TestSettingDefaultImporterForBulkImportMenu()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var claimant = Factory.New<OrgHeader>();
			drawback.JE_OH_Importer = claimant.PK;
			using (var form = new JobDeclarationForm(drawback))
			{
				form.Show();
				var bulkImportMenu = form.Menu.MenuItems.FindByText("Brokerage").MenuItems.FindByText("Data").MenuItems.FindByText(EDIMenu.Constants.DrawbackBulkImportEntryLines);
				bulkImportMenu.PerformClick();
				var popupForm = (EmbeddedModulePopup)ZFormModaliser.GetActiveChildFormForParentForm(form);
				var module = popupForm.Module_ForTest;
				var importerFilter = (ModuleGuidFilter)module.FilterBusinessObject.AlwaysVisibleModuleFilters.FirstOrDefault(x => x.Description == "Importer");
				AssertEquals(claimant.PK, importerFilter.DefaultProperty);
			}
		}

		public void TestBulkImportMenuItemForDrawback()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			using (var form = new JobDeclarationForm(drawback))
			{
				AssertNotNull(form.Menu.MenuItems.FindByText("Brokerage").MenuItems.FindByText("Data").MenuItems.FindByText(EDIMenu.Constants.DrawbackBulkImportEntryLines));
			}
		}

		public void TestAESMessageMenuItemVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(true, menu.aESMessagingMenuItem.Visible);
				AssertEquals("FILE AES Message", menu.aESMessagingMenuItem.Text);
			}
		}

		public void TestMenuItemVisibilityWhenImportDeclarationIsInterface()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var performApportionment = menu.MenuItems.FindByText("Perform Apportionment");
				var submit = menu.MenuItems.FindByText("Submit");
				menu.RefreshMenu();
				AssertEquals(false, submit.Visible);
				AssertEquals(true, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals(true, performApportionment.Visible);
				AssertEquals(true, menu.otherMenuItems.Visible);
				AssertEquals(true, menu.queryMenuItems.Visible);
				AssertEquals(true, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseWithdrawalMenu.Visible);
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();
				AssertEquals(true, submit.Visible);
				AssertEquals(false, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals(false, performApportionment.Visible);
				AssertEquals(false, menu.otherMenuItems.Visible);
				AssertEquals(false, menu.queryMenuItems.Visible);
				AssertEquals(false, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseWithdrawalMenu.Visible);
			}
		}

		public void TestMenuItemVisibilityWhenFTZDeclarationIsInterface()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var fdaMenuItem = menu.MenuItems.FindByText("FDA");
				var submit = menu.MenuItems.FindByText("Submit");
				var performApportionment = menu.MenuItems.FindByText("Perform Apportionment");
				menu.RefreshMenu();
				AssertEquals(false, submit.Visible);
				AssertEquals(true, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals(true, performApportionment.Visible);
				AssertEquals(true, menu.fTZMenus.Where(x => x.Text != EDIMenu.Constants.SendPTTArrival && x.Text != EDIMenu.Constants.SendPTTUnArrival).All(x => x.Visible));
				AssertEquals(true, fdaMenuItem.Visible);
				AssertEquals(true, menu.queryMenuItems.Visible);
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();
				AssertEquals(true, submit.Visible);
				AssertEquals(false, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals(false, performApportionment.Visible);
				AssertEquals(false, menu.fTZMenus.Any(x => x.Visible));
				AssertEquals(false, fdaMenuItem.Visible);
				AssertEquals(false, menu.queryMenuItems.Visible);
			}
		}

		public void TestMenuItemVisibilityWhenExportDeclarationIsInterface()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var performApportionment = menu.MenuItems.FindByText("Perform Apportionment");
				var submit = menu.MenuItems.FindByText("Submit");
				menu.RefreshMenu();
				AssertEquals(false, submit.Visible);
				AssertEquals(true, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals(true, performApportionment.Visible);
				AssertEquals(true, menu.aESMessagingMenuItem.Visible);
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();
				AssertEquals(true, submit.Visible);
				AssertEquals(false, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals(false, performApportionment.Visible);
				AssertEquals(false, menu.aESMessagingMenuItem.Visible);
			}
		}

		public void TestMenuItemVisibilityIfMiscellaneousOrImportByExternalBroker()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(false, menu.sendOriginalMenu.Visible);
				AssertEquals(false, menu.sendAmendmentMenu.Visible);
				AssertEquals(false, menu.sendWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendFDACorrectionMessagesMenu.Visible);
				AssertEquals(false, menu.sendFDACorrectionMessagesMenu.Visible);
				AssertEquals(false, menu.otherMenuItems.Visible);
				AssertEquals(true, menu.sendBillOfLadingUpdateMenu.Visible);
				AssertEquals(false, menu.sendFTZCargoManifestStatusQueryMenu.Visible);
				AssertEquals(false, menu.importInvoicesMenuItem.Visible);
				declaration.US_ConsolACE = true;
				menu.RefreshMenu();
				AssertEquals(true, menu.importInvoicesMenuItem.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(false, menu.sendOriginalMenu.Visible);
				AssertEquals(false, menu.sendAmendmentMenu.Visible);
				AssertEquals(false, menu.sendWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendFDACorrectionMessagesMenu.Visible);
				AssertEquals(false, menu.otherMenuItems.Visible);
				AssertEquals(true, menu.sendBillOfLadingUpdateMenu.Visible);
				AssertEquals(false, menu.importInvoicesMenuItem.Visible);
				declaration.US_ConsolACE = true;
				menu.RefreshMenu();
				AssertEquals(true, menu.importInvoicesMenuItem.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(true, menu.sendOriginalMenu.Visible);
				AssertEquals(true, menu.sendAmendmentMenu.Visible);
				AssertEquals(true, menu.sendWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseWithdrawalMenu.Visible);
				AssertEquals(true, menu.sendFDACorrectionMessagesMenu.Visible);
				AssertEquals(true, menu.otherMenuItems.Visible);
				AssertEquals(true, menu.sendBillOfLadingUpdateMenu.Visible);
				AssertEquals(false, menu.sendFTZCargoManifestStatusQueryMenu.Visible);
				AssertEquals(false, menu.importInvoicesMenuItem.Visible);
				declaration.US_ConsolACE = true;
				menu.RefreshMenu();
				AssertEquals(true, menu.importInvoicesMenuItem.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(false, menu.sendOriginalMenu.Visible);
				AssertEquals(false, menu.sendAmendmentMenu.Visible);
				AssertEquals(false, menu.sendWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendFDACorrectionMessagesMenu.Visible);
				AssertEquals(false, menu.otherMenuItems.Visible);
				AssertEquals(false, menu.sendBillOfLadingUpdateMenu.Visible);
				AssertEquals(false, menu.sendFTZCargoManifestStatusQueryMenu.Visible);
				AssertEquals(false, menu.importInvoicesMenuItem.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(false, menu.sendOriginalMenu.Visible);
				AssertEquals(false, menu.sendAmendmentMenu.Visible);
				AssertEquals(false, menu.sendWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendFDACorrectionMessagesMenu.Visible);
				AssertEquals(true, menu.sendFTZCargoManifestStatusQueryMenu.Visible);
				AssertEquals(false, menu.otherMenuItems.Visible);
				var fdaMenuItem = menu.MenuItems.FindByText("FDA");
				AssertEquals(true, fdaMenuItem.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(false, menu.sendOriginalMenu.Visible);
				AssertEquals(false, menu.sendAmendmentMenu.Visible);
				AssertEquals(false, menu.sendWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendFDACorrectionMessagesMenu.Visible);
				AssertEquals(true, menu.otherMenuItems.Visible);
				AssertEquals(false, menu.sendBillOfLadingUpdateMenu.Visible);
				AssertEquals(false, menu.sendFTZCargoManifestStatusQueryMenu.Visible);
			}

			declaration.US_EnableCRL = false;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(false, menu.sendOriginalMenu.Visible);
				AssertEquals(false, menu.sendAmendmentMenu.Visible);
				AssertEquals(false, menu.sendWithdrawalMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(true, menu.sendCargoReleaseWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendFDACorrectionMessagesMenu.Visible);
				AssertEquals(true, menu.otherMenuItems.Visible);
				AssertEquals(false, menu.sendBillOfLadingUpdateMenu.Visible);
				AssertEquals(false, menu.sendFTZCargoManifestStatusQueryMenu.Visible);
			}

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(false, menu.sendOriginalMenu.Visible);
				AssertEquals(false, menu.sendAmendmentMenu.Visible);
				AssertEquals(false, menu.sendWithdrawalMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryOriginalMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryAmendmentMenu.Visible);
				AssertEquals(true, menu.sendEntrySummaryWithdrawalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseOriginalMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseAmendmentMenu.Visible);
				AssertEquals(false, menu.sendCargoReleaseWithdrawalMenu.Visible);
				AssertEquals(true, menu.sendFDACorrectionMessagesMenu.Visible);
				AssertEquals(true, menu.otherMenuItems.Visible);
				AssertEquals("For ACE jobs certified vis ACS user should have an ability to send legacy bill update message", true, menu.sendBillOfLadingUpdateMenu.Visible);
				AssertEquals(false, menu.sendFTZCargoManifestStatusQueryMenu.Visible);
			}
		}

		public void TestQueryMenuForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(true, menu.queryADDCVDMenu.Visible);
				AssertEquals(true, menu.queryADDCVDByTariffMenu.Visible);
				AssertEquals(false, menu.cargoManifestStatusQueryMenu.Visible);
				AssertEquals(false, menu.sendEntrySummaryQueryMenu.Visible);
				AssertEquals(false, menu.queryQuotaMenu.Visible);
				AssertEquals(false, menu.censusWarningQueryMessageMenu.Visible);
			}
		}

		public void TestPPTArrivalMenuItemVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				menu.RefreshMenu();
				AssertEquals(true, menu.sendPTTArrivalMenuItem.Visible);
				AssertEquals(true, menu.sendPTTUnArrivalMenuItem.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				menu.RefreshMenu();
				AssertEquals(true, menu.sendPTTArrivalMenuItem.Visible);
				AssertEquals(true, menu.sendPTTUnArrivalMenuItem.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				menu.RefreshMenu();
				AssertEquals(false, menu.sendPTTArrivalMenuItem.Visible);
				AssertEquals(false, menu.sendPTTUnArrivalMenuItem.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				menu.RefreshMenu();
				AssertEquals(false, menu.sendPTTArrivalMenuItem.Visible);
				AssertEquals(false, menu.sendPTTUnArrivalMenuItem.Visible);
			}
		}

		public void TestAESSecurityRight()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Env.Security.ExportMessaging.IsAllowed = false;
			using (var form = new ZForm())
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(true, menu.aESMessagingMenuItem.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.aESMessagingMenuItem.PerformClick();
				AssertEquals(Env.Security.ExportMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestImportMessagingSecurityRight()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Env.Security.ImportMessaging.IsAllowed = false;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(true, menu.sendOriginalMenu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.sendOriginalMenu.PerformClick();
				AssertEquals(Env.Security.ImportMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, menu.sendAmendmentMenu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.sendAmendmentMenu.PerformClick();
				AssertEquals(Env.Security.ImportMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, menu.sendWithdrawalMenu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.sendWithdrawalMenu.PerformClick();
				AssertEquals(Env.Security.ImportMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				foreach (var ftzMenu in menu.fTZMenus)
				{
					Assert(!ftzMenu.Visible);
				}
			}
		}

		public void TestShowDiscardedAndInactiveMessages_Clicked()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals(false, menu.MenuItems.FindByText(EDIMenu.Constants.ShowDiscardedAndInactiveMessages).Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var message = Factory.New<MQEDIMessage>();
				declaration.Messages.Add(message);
				message.EM_Status = MQEDIMessage.Status.Discarded;
				var invoice = declaration.Invoices.AddNew();
				message = Factory.New<MQEDIMessage>();
				invoice.Messages.Add(message);
				message = Factory.New<MQEDIMessage>();
				invoice.Messages.Add(message);
				menu.RefreshMenu();
				AssertEquals(true, menu.MenuItems.FindByText(EDIMenu.Constants.ShowDiscardedAndInactiveMessages).Visible);
				var theMenu = menu.MenuItems.FindByText(EDIMenu.Constants.ShowDiscardedAndInactiveMessages);
				theMenu.PerformClick();
				Assert(theMenu.Checked);
				AssertEquals(1, declaration.InBondRelatedRecords.Count);
				AssertEquals(1, declaration.InBondRelatedRecords[0].MessagesToShow.Count);
				theMenu.PerformClick();
				Assert(!theMenu.Checked);
				AssertEquals(1, declaration.InBondRelatedRecords.Count);
				AssertEquals(2, declaration.InBondRelatedRecords[0].MessagesToShow.Count);
			}
		}

		public void TestContinueWithNotifications()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var menu = new EDIMenu())
			{
				declaration.RunPreSaveValidation();
				menu.Declaration = declaration;
				var messageSendingNotificationCollection = new MessageSendingNotificationCollection();
				GlbSecurity se = Factory.New<GlbSecurity>();
				se.GU_SecurityRight = "AllMessageErrors";
				se.GU_GG = Core.Constants.Groups.AllPK;
				se.GU_SecurityItemIsAllowed = true;
				Factory.Save();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Assert(menu.ContinueWithNotifications(messageSendingNotificationCollection));
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				Assert(menu.ContinueWithNotifications(messageSendingNotificationCollection));
			}
		}

		public void TestSendCWOWhenDeclarationIsNotAcceptedYet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			using (var formForTest = new FormForTestMoq(declaration))
			{
				EDIMenu menu = formForTest.MenuForTest;
				declaration.RunPreSaveValidation();
				menu.Declaration = declaration;
				var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				var coll = new EntryCensusWarningOverrideCollection(entry);
				var cwo = coll.AddNew();
				cwo.EntryLinePK = entry.MergedLines[0].PK;
				cwo.ConditionCode = "27J";
				cwo.OverrideCode = "11";
				GlbSecurity se = Factory.New<GlbSecurity>();
				se.GU_SecurityRight = "AllMessageErrors";
				se.GU_GG = Core.Constants.Groups.AllPK;
				se.GU_SecurityItemIsAllowed = true;
				Factory.Save();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.PerformRequiredCWOAction(CWOForm.Action.Send, entry, coll);
				AssertEquals("No CWO messages should have been sent", 0, entry.Messages.Count);
			}
		}

		public void TestQueryADDCVD_Click()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfOrigin = "FR";
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = USCTariff.CottonFeeApplicable;
			line.US_ADDCaseNo = "1";
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				EDIMenu menu = formForTest.MenuForTest;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.queryADDCVDMenu.PerformClick();
				AssertEquals(QueryADDCVDMessageSender.NotSupportedByCBP, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestGenerateEntriesUseMutex()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98191124";
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			Factory.Save();
			var decEntryNumberRefreshCount = 0;
			declaration.DecEntryNumberInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				decEntryNumberRefreshCount++;
			};
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decInFactory2 = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals(true, decInFactory2.LockImportEntryNumberAllocationMutex);
			using (var importEntryNumberAllocationMutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "ENS" + declaration.PK.ToString()))
			{
				AssertEquals(true, importEntryNumberAllocationMutex.IsLocked);
				AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
				using (var formForTest = new FormForTestMoq(declaration))
				{
					var menu = formForTest.MenuForTest;
					menu.Declaration = declaration;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.GenerateEntriesMenuItem.PerformClick();
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals(MessageSenderController.CannotMergeImportEntryNumberAllocationInProgress(declaration.GetImportEntryNumberAllocationMutexLockInfo()), UnitTestUserNotification.Instance.LastMessage.Text);
					decInFactory2.ImportEntryNumber = "ENT3234";
					factory2.Save();
					AssertEquals(true, decInFactory2.LockImportEntryNumberAllocationMutex);
					AssertEquals(true, importEntryNumberAllocationMutex.IsLocked);
					AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
					AssertEquals(0, decEntryNumberRefreshCount);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.GenerateEntriesMenuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, decEntryNumberRefreshCount);
					AssertEquals("Should have been refreshed", "ENT3234", declaration.ImportEntryNumber);
					AssertEquals(true, decInFactory2.LockImportEntryNumberAllocationMutex);
					AssertEquals(true, importEntryNumberAllocationMutex.IsLocked);
					AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
					decInFactory2.UnlockImportEntryNumberAllocationMutex();
					AssertEquals(false, importEntryNumberAllocationMutex.IsLocked);
					AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
					AssertEquals(true, declaration.LockImportEntryNumberAllocationMutex);
					AssertEquals(true, importEntryNumberAllocationMutex.IsLocked);
					AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
				}

				AssertEquals("Lock should be released on disposal of menu", false, importEntryNumberAllocationMutex.IsLocked);
			}
		}

		public void TestQueryADDCVDByTariff_Click()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98191124";
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				EDIMenu menu = formForTest.MenuForTest;
				menu.Declaration = declaration;
				menu.queryADDCVDByTariffMenu.PerformClick();
				AssertEquals(typeof(ImportMessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestCopyPreviousFDALine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				MenuItem fdaMenuItem = menu.MenuItems.FindByText("FDA");
				AssertEquals(true, fdaMenuItem.Visible);
				AssertEquals(true, fdaMenuItem.MenuItems.FindByText(EDIMenu.Constants.CopyPreviousFDALine).Visible);
				AssertEquals(false, declaration.CopyLastFDADetailsToNewLine);
				menu.copyPreviousFDALineMenu.PerformClick();
				AssertEquals(true, declaration.CopyLastFDADetailsToNewLine);
			}
		}

		public void TestSendPGACorrectionForAddedPGALine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "SV9";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.ImportEntryNumber = "71002057";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";
			declaration.JE_OH_Importer = importer.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "2921429011";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				Mock<EDIMenu> mockMenu = formForTest.MockMenu;
				EDIMenu menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				AssertEquals(true, menu.pGACorrectionMessageMenu.Visible);
				mockMenu
					.Protected()
					.Setup<bool>("ShowPGACorrectionSendingForm", ItExpr.IsAny<PGACorrectionMessageSendingAction>())
					.Returns((PGACorrectionMessageSendingAction action) =>
					{
						action.US_SendMessage = true;
						return true;
					});
				menu.pGACorrectionMessageMenu.PerformClick();
				AssertEquals("There is no Entry available for sending PGA Correction message to Customs and no new PGA lines available for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				var builder = new ACEEntrySummaryMessageBuilderForTesting(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add);
				var outgoingMessage = builder.PopulateMessage();
				declaration.FormalEntry.Messages.Add(outgoingMessage);
				var incomingMessage = Factory.New<MQEDIMessage>();
				incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				incomingMessage.EM_Status = EDIMessage.Status.Received;
				incomingMessage.EM_MessageNum = "HYEDUSCMT_148588";
				declaration.FormalEntry.Messages.Add(incomingMessage);
				declaration.FormalEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
				Factory.Save();
				outgoingMessage.EM_MessageNum = "HYEDUSCMT_148588";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.pGACorrectionMessageMenu.PerformClick();
				AssertEquals("There are no PGA lines available for sending PGA Correction. No messages generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
				var fdaLine = invoiceLine.ACE_FDALines.AddNew();
				fdaLine.US_ProgramCode = "FOO";
				fdaLine.US_LineNo = 1;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.pGACorrectionMessageMenu.PerformClick();
				AssertEquals("PGA Correction Message Sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				var correctionMessages = declaration.FormalEntry.Messages.OfType<MQEDIMessage>().Where(x => x.EM_MessageSubType == EM_MessageSubTypeList.Codes.PGADataCorrection);
				AssertEquals("There should be 1 PGA Correction message generated", 1, correctionMessages.Count());
			}
		}

		public void TestSendStandalonePriorNoticeVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableSPN = true;
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				MenuItem fdaMenuItem = menu.MenuItems.FindByText("FDA");
				AssertEquals(true, fdaMenuItem.Visible);
				AssertEquals(true, fdaMenuItem.MenuItems.FindByText(EDIMenu.Constants.SendStandalonePriorNotice).Visible);
				declaration.US_EnableSPN = false;
				menu.RefreshMenu();
				fdaMenuItem = menu.MenuItems.FindByText("FDA");
				AssertEquals(false, fdaMenuItem.Visible);
				AssertEquals(false, fdaMenuItem.MenuItems.FindByText(EDIMenu.Constants.SendStandalonePriorNotice).Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				menu.RefreshMenu();
				fdaMenuItem = menu.MenuItems.FindByText("FDA");
				AssertEquals(false, fdaMenuItem.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				declaration.US_EnableSPN = true;
				menu.RefreshMenu();
				fdaMenuItem = menu.MenuItems.FindByText("FDA");
				AssertEquals(true, fdaMenuItem.Visible);
			}
		}

		public void TestSendRequestToExtendOrClosureTIB()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			CusEntryHeader crlEntry = declaration.ActiveEntryHeaders[1];
			declaration.Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				Mock<EDIMenu> mockMenu = formForTest.MockMenu;
				EDIMenu menu = formForTest.MenuForTest;
				mockMenu
					.Protected()
					.Setup<bool>("ShowImportMessageSendingActionForm", ItExpr.IsAny<ImportMessageSendingActionCollection>())
					.Returns((ImportMessageSendingActionCollection actions) =>
					{
						return true;
					});
				menu.requestToExtendTIBMenu.PerformClick();
				AssertEquals("US_EntryType != 23 is warned", true, HasThisWarned(RequestToExtensionOrClosureTIBMessageSender.JobIsNotTemporaryImportationBond));
				declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.requestToExtendTIBMenu.PerformClick();
				AssertEquals("now US_EntryType == 23", false, HasThisWarned(RequestToExtensionOrClosureTIBMessageSender.JobIsNotTemporaryImportationBond));
				AssertEquals("For ens entry, there should be 1 query message", 1, ensEntry.Messages.Count);
				AssertEquals("But not for CRL entry", 0, crlEntry.Messages.Count);
				AssertEquals("the message sub type", EM_MessageSubTypeList.Codes.TemporaryImportationBondRequestToExtend, ensEntry.Messages[0].EM_MessageSubType);
				ensEntry.Messages.RemoveAndDeleteAllFromTest();
				declaration.Factory.Save();
				menu.RefreshMenu();
				AssertEquals("Closure TIB menu is visible", true, menu.requestToClosureTIBMenu.Visible);
				menu.requestToClosureTIBMenu.PerformClick();
				AssertEquals("For ens entry, there should be 1 query message", 1, ensEntry.Messages.Count);
				mockMenu.VerifyAll();
			}
		}

		[ExpectNoExceptions]
		public void TestMergeInvoiceLines()
		{
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			Customs.Business.SendsMessagesToCustomsShutterUpperer initiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			mockDeclaration.Object.MessageInitiator = initiator;
			mockDeclaration
				.Protected()
				.Setup<bool>("DoMergeCore", initiator)
				.Returns(true);
			mockDeclaration.Object.Invoices.AddNew().JobComInvoiceLines.AddNew();
			using (ZForm form = new ZForm(mockDeclaration.Object))
			{
				using (var menu = new EDIMenuForTest())
				{
					menu.Declaration = mockDeclaration.Object;
					form.Menu.MenuItems.Add(menu);
					menu.SetupTopLevelMenuInternal();
					MenuItem foundMenuItem = menu.MenuItems.FindByText("MERGE (Generate Entries)");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					foundMenuItem.PerformClick();
					mockDeclaration.Verify();
				}
			}
			mockDeclaration.VerifyAll();
		}

		public void TestRefreshMenu()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				menu.RefreshMenu();
				AssertEquals("AES invisible", false, menu.aESMessagingMenuItem.Visible);
				AssertEquals("Drawback invisible", false, menu.drawbackSummaryMenuItem.Visible);
				AssertEquals("Drawback Delete invisible", false, menu.drawbackSummaryDeleteMenuItem.Visible);
				AssertEquals("Generate entries is visible", true, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals("InbondMenus visible", true, menu.importCommonMenus[0].Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				menu.RefreshMenu();
				AssertEquals(false, declaration.IsInBond);
				AssertEquals("AES visible", true, menu.aESMessagingMenuItem.Visible);
				AssertEquals("Drawback invisible", false, menu.drawbackSummaryMenuItem.Visible);
				AssertEquals("Drawback Delete invisible", false, menu.drawbackSummaryDeleteMenuItem.Visible);
				AssertEquals("Generate entries is visible", true, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals("InbondMenus invisible", false, menu.importCommonMenus[0].Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				menu.RefreshMenu();
				AssertEquals(false, declaration.IsInBond);
				AssertEquals("AES invisible", false, menu.aESMessagingMenuItem.Visible);
				AssertEquals("Drawback visible", true, menu.drawbackSummaryMenuItem.Visible);
				AssertEquals("Drawback Delete visible", true, menu.drawbackSummaryDeleteMenuItem.Visible);
				AssertEquals("Drawback Replacement invisible", false, menu.drawbackSummaryReplaceMenuItem.Visible);
				AssertEquals("Generate entries invisible", false, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals("InbondMenus invisible", false, menu.importCommonMenus[0].Visible);
				declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
				menu.RefreshMenu();
				AssertEquals("Drawback invisible", false, menu.drawbackSummaryMenuItem.Visible);
				AssertEquals("Drawback Delete invisible", false, menu.drawbackSummaryDeleteMenuItem.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				menu.RefreshMenu();
				AssertEquals("Drawback Summary visible", true, menu.drawbackSummaryMenuItem.Visible);
				AssertEquals("Drawback Replacement visible", true, menu.drawbackSummaryReplaceMenuItem.Visible);
				AssertEquals("Drawback Delete invisible", false, menu.drawbackSummaryDeleteMenuItem.Visible);
			}
		}

		public void TestSendEntrySummaryQueryForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.Declaration = declaration;
				menu.sendEntrySummaryQueryMenu.PerformClick();
				AssertEquals("There should be 1 Entry Summary Query message", 1, ensEntry.Messages.Count);
				AssertEquals("the message sub type", EM_MessageSubTypeList.Codes.EntrySummaryQuery, ensEntry.Messages[0].EM_MessageSubType);
			}
		}

		public void TestSendEntrySummaryQueryWhenACENotLodged()
		{
			var declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "01";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.Declaration = declaration;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				menu.sendEntrySummaryQueryMenu.PerformClick();
				AssertContains("The entry has not been lodged at Customs. Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestStatementDeleteAdd_Click()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.Factory.Save();
			var mockMenu = new Mock<EDIMenu> { CallBase = true };
			mockMenu.Object.Declaration = declaration;
			using (EDIMenu eDIMenu = mockMenu.Object)
			{
				eDIMenu.statementDeleteAddMessageMenu.PerformClick();
				AssertEquals(typeof(StatementSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		[TestDate(2015, 10, 31)]
		public void TestQueryMenuNotVisibleForExternalBroker()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (var eDIMenu = new EDIMenu())
			{
				eDIMenu.Declaration = declaration;
				eDIMenu.RefreshMenu();
				Assert(eDIMenu.queryMenuItems.MenuItems.Count > 0);
				foreach (MenuItem menu in eDIMenu.queryMenuItems.MenuItems)
				{
					//"All menu items should be visible under Query MenuItems except for 'Send Entry Summary Query message' and 'Census Warning Query'", 
					//ACE Cargo Manifest Query should not be visible if not ACE
					AssertEquals(menu != eDIMenu.sendEntrySummaryQueryMenu && menu != eDIMenu.censusWarningQueryMessageMenu, menu.Visible);
				}
			}
		}

		public void TestExportToBIRD7501When7501IsNotEnabled()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = false;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				AssertEquals("PreCondition", true, menu.exportToBIRD7501Menu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.exportToBIRD7501Menu.PerformClick();
				Assert(HasThisWarned("This Declaration does not have 7501 enabled."));
			}
		}

		public void TestSendEntryQueryFile()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "";
			Assert("PreCondition", declaration.HasMessageErrors());
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			SetUpExternalBroker(declaration);
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				AssertEquals("PreCondition", true, menu.exportToBIRDEntryQueryMenu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to saving
				AssertEquals("PreCondition", true, declaration.HasChanges);
				menu.exportToBIRDEntryQueryMenu.PerformClick();
				AssertEquals("Still HasChanges", true, declaration.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.exportToBIRDEntryQueryMenu.PerformClick();
				MQEDIMessage message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDEntrySummaryQuery, message.EM_MessageType);
				AssertEquals(Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.BIRDEntrySummaryQueryInput, message.EM_MessageSubType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDEntrySummaryQueryInput, message.EM_MessageSubType);
			}
		}

		public void TestSendLiquidationFile()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "";
			Assert("PreCondition", declaration.HasMessageErrors());
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			SetUpExternalBroker(declaration);
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("PreCondition", true, menu.exportToBIRDLiquidationMenu.Visible);
				menu.exportToBIRDLiquidationMenu.PerformClick();
				AssertEquals(string.Format(EDIMenu.NoMessageToExportAsBIRD, "Liquidation Notices"), UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to saving
				AssertEquals("PreCondition", true, declaration.HasChanges);
				menu.exportToBIRDLiquidationMenu.PerformClick();
				AssertEquals("Still HasChanges", true, declaration.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				CusLiquidation liquidation = declaration.Liquidations.AddNew();
				MQEDIMessage message = Factory.New<MQEDIMessage>();
				message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
				message.EM_LinkedObject = liquidation;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.exportToBIRDLiquidationMenu.PerformClick();
				AssertNotEquals(string.Format(EDIMenu.NoMessageToExportAsBIRD, "Liquidation Notices"), UnitTestUserNotification.Instance.LastMessage.Text);
				message = (MQEDIMessage)declaration.Messages[0];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.BIRDLiquidationNotice, message.EM_MessageSubType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDLiquidationNotice, message.EM_MessageSubType);
			}
		}

		public void TestSendEntrySummaryQueryResponseFile()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "";
			Assert("PreCondition", declaration.HasMessageErrors());
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			SetUpExternalBroker(declaration);
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("PreCondition", true, menu.exportToBIRDEntrySummaryQueryResponseMenu.Visible);
				menu.exportToBIRDEntrySummaryQueryResponseMenu.PerformClick();
				AssertEquals(string.Format(EDIMenu.NoMessageToExportAsBIRD, "Entry Summary Query response"), UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to saving
				AssertEquals("PreCondition", true, declaration.HasChanges);
				menu.exportToBIRDEntrySummaryQueryResponseMenu.PerformClick();
				AssertEquals("Still HasChanges", true, declaration.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				MQEDIMessage message = Factory.New<MQEDIMessage>();
				message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse;
				message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
				declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.exportToBIRDEntrySummaryQueryResponseMenu.PerformClick();
				AssertNotEquals(string.Format(EDIMenu.NoMessageToExportAsBIRD, "Entry Summary Query response"), UnitTestUserNotification.Instance.LastMessage.Text);
				message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[1];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.BIRDEntrySummaryQueryOutput, message.EM_MessageSubType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDEntrySummaryQueryOutput, message.EM_MessageSubType);
			}
		}

		public void TestSendCargoReleaseProcessingFile()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "";
			Assert("PreCondition", declaration.HasMessageErrors());
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			SetUpExternalBroker(declaration);
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("PreCondition", true, menu.exportToBIRDCargoReleaseProcessingResultMenu.Visible);
				menu.exportToBIRDCargoReleaseProcessingResultMenu.PerformClick();
				AssertEquals(string.Format(EDIMenu.NoMessageToExportAsBIRD, "Cargo Release Processing Results"), UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to saving
				MQEDIMessage message = Factory.New<MQEDIMessage>();
				declaration.Messages.Add(message);
				message.EM_LinkedObject = declaration;
				message.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
				message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
				AssertEquals("PreCondition", true, declaration.HasChanges);
				menu.exportToBIRDCargoReleaseProcessingResultMenu.PerformClick();
				AssertEquals("Still HasChanges", true, declaration.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.exportToBIRDCargoReleaseProcessingResultMenu.PerformClick();
				AssertNotEquals(string.Format(EDIMenu.NoMessageToExportAsBIRD, "Cargo Release Processing Results"), UnitTestUserNotification.Instance.LastMessage.Text);
				message = (MQEDIMessage)declaration.Messages[1];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.BIRDStatusRecords, message.EM_MessageSubType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDStatusRecords, message.EM_MessageSubType);
			}
		}

		public void TestExportToBIRD7501()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = "ACS";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "";
			Assert("PreCondition", declaration.HasMessageErrors());
			SetUpExternalBroker(declaration);
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				AssertEquals("PreCondition", true, menu.exportToBIRD7501Menu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to saving
				AssertEquals("PreCondition", true, declaration.HasChanges);
				menu.exportToBIRD7501Menu.PerformClick();
				AssertEquals("Still HasChanges", true, declaration.HasChanges);
				AssertEquals("Still RequireMerge", true, declaration.MergeManager.RequiresMerge);
				AssertNull("No further action taken by system", ZFormModaliser.LastCommonDialogShownDialogForTest);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to continuing with notifications
				menu.exportToBIRD7501Menu.PerformClick();
				AssertEquals("Changes saved", false, declaration.HasChanges);
				AssertEquals("Should have merged", false, declaration.MergeManager.RequiresMerge);
				AssertNull("No further action taken by system", ZFormModaliser.LastCommonDialogShownDialogForTest);
				declaration.US_EntryType = "~~";
				Assert("PreCondition", declaration.HasMessageErrors() && declaration.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to continuing with notifications
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.exportToBIRD7501Menu.PerformClick();
				AssertEquals("Saved", false, declaration.HasChanges);
				AssertEquals("Should have merged", false, declaration.MergeManager.RequiresMerge);
				AssertEquals("One message should have been created", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
				var message = declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.BIRDEntrySummary, message.EM_MessageSubType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDEntrySummary, message.EM_MessageSubType);
			}
		}

		public void TestExportToBIRD3461When3461IsNotEnabled()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableCRL = false;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				AssertEquals("PreCondition", true, menu.exportToBIRD3461Menu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.exportToBIRD3461Menu.PerformClick();
				Assert(HasThisWarned("This Declaration does not have 3461 enabled."));
			}
		}

		public void TestExportToBIRD3461()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "";
			Assert("PreCondition", declaration.HasMessageErrors());
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			SetUpExternalBroker(declaration);
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				AssertEquals("PreCondition", true, menu.exportToBIRD3461Menu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to saving
				AssertEquals("PreCondition", true, declaration.HasChanges);
				menu.exportToBIRD3461Menu.PerformClick();
				AssertEquals("Still HasChanges", true, declaration.HasChanges);
				AssertNull("No further action taken by system", ZFormModaliser.LastCommonDialogShownDialogForTest);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to continuing with notifications
				menu.exportToBIRD3461Menu.PerformClick();
				AssertEquals("Changes saved", false, declaration.HasChanges);
				AssertEquals("No further action is taken", 0, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
				declaration.US_EntryType = "~~";
				Assert("PreCondition", declaration.HasMessageErrors() && declaration.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to continuing with notifications
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.exportToBIRD3461Menu.PerformClick();
				AssertEquals("Saved", false, declaration.HasChanges);
				AssertEquals("One message should have been created", 1, declaration.ActiveEntryHeaders.CargoReleaseEntry.Messages.Count);
				var message = declaration.ActiveEntryHeaders.CargoReleaseEntry.Messages[0];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDCargoRelease, message.EM_MessageSubType);
			}
		}

		public void TestExportToBIRDACECargoRelease()
		{
			var declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			SetUpExternalBroker(declaration);
			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				AssertEquals("PreCondition", true, menu.exportToBIRD3461Menu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to continuing with notifications
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				AssertNoExceptionThrown(delegate
				{
					menu.exportToBIRD3461Menu.PerformClick();
				});
				AssertEquals("One message should have been created", 1, declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Count);
				var message = declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDCargoRelease, message.EM_MessageSubType);
			}
		}

		public void TestExportAcknowledgeOfReceiptOf7501()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			SetUpExternalBroker(declaration);
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				AssertEquals("PreCondition", true, menu.exportToBIRDENMenu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to saving
				AssertEquals("PreCondition", true, declaration.HasChanges);
				menu.exportToBIRDENMenu.PerformClick();
				AssertEquals("Still HasChanges", true, declaration.HasChanges);
				AssertEquals(0, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.exportToBIRDENMenu.PerformClick();
				AssertEquals("Saved", false, declaration.HasChanges);
				AssertEquals("One message should have been created", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
				var message = declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDStatusRecords, message.EM_MessageSubType);
			}
		}

		public void TestExportSignificantDates()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			SetUpExternalBroker(declaration);
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				AssertEquals("PreCondition", true, menu.exportToBIRDDTMenu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //No to saving
				AssertEquals("PreCondition", true, declaration.HasChanges);
				menu.exportToBIRDDTMenu.PerformClick();
				AssertEquals("Still HasChanges", true, declaration.HasChanges);
				AssertEquals(0, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.exportToBIRDDTMenu.PerformClick();
				AssertEquals("Saved", false, declaration.HasChanges);
				AssertEquals("One message should have been created", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
				var message = declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDStatusRecords, message.EM_MessageSubType);
			}
		}

		public void TestExportSignificantDatesForACECargoRelease()
		{
			var declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			SetUpExternalBroker(declaration);
			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				AssertEquals("PreCondition", true, menu.exportToBIRDDTMenu.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes to saving
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				AssertNoExceptionThrown(delegate
				{
					menu.exportToBIRDDTMenu.PerformClick();
				});
				AssertEquals("One message should have been created", 1, declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Count);
				var message = declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(EM_MessageSubTypeList.Codes.BIRDStatusRecords, message.EM_MessageSubType);
			}
		}

		public void TestAddAuditMenu()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("PreCondition", declaration.HasMessageErrors());
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				AssertEquals("Should be visible for IMP", true, menu.moreAuditsMenu.Visible);
				AssertNotNull("Contains 'Audit SPI'", menu.moreAuditsMenu.MenuItems.FindByText("Audit SPI"));
				AssertNotNull("Contains 'Audit FDA'", menu.moreAuditsMenu.MenuItems.FindByText("Audit FDA"));
				var cwoAuditMenu = menu.moreAuditsMenu.MenuItems.FindByText("Audit Census Warning");
				AssertNotNull("Contains 'Audit CW'", cwoAuditMenu);
				Assert("Not visible though", !cwoAuditMenu.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				menu.RefreshMenu();
				AssertEquals("Should be not visible for EXP", false, menu.moreAuditsMenu.Visible);
			}
		}

		public void TestSendDrawbackSummaryMessage()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_DRWFilingMethod = DrawbackMethodOfFilingList.Codes.ABI;
			declaration.JE_EntryStatus = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal;
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				Mock<EDIMenu> mockMenu = formForTest.MockMenu;
				EDIMenu menu = formForTest.MenuForTest;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.drawbackSummaryMenuItem.PerformClick();
				Assert("Has already been added", UnitTestUserNotification.Instance.LastMessage.Text.Contains("the Drawback Summary has already been added"));
				AssertEquals("No message generated", 0, declaration.Messages.Count);
			}

			declaration.JE_EntryStatus = "";
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryOriginal;
			Factory.Save();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.drawbackSummaryMenuItem.PerformClick();
				Assert("Waiting for response", UnitTestUserNotification.Instance.LastMessage.Text.Contains("This Drawback is waiting for Customs response"));
				AssertEquals("No message generated", 0, declaration.Messages.Count);
			}

			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.drawbackSummaryMenuItem.PerformClick();
				Assert("Validation errors", UnitTestUserNotification.Instance.LastMessage.Text.Contains("the following message error"));
				AssertEquals("No message generated", 0, declaration.Messages.Count);
			}

			declaration.JE_MessageStatus = "";
			Factory.Save();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.drawbackSummaryMenuItem.PerformClick();
				Assert("Validation errors", UnitTestUserNotification.Instance.LastMessage.Text.Contains("the following message error"));
				AssertEquals("No message generated", 0, declaration.Messages.Count);
			}

			using (var formForTest = new FormForTestMoq(declaration))
			{
				Mock<EDIMenu> mockMenu = formForTest.MockMenu;
				EDIMenu menu = formForTest.MenuForTest;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.drawbackSummaryMenuItem.PerformClick();
				Assert("Message sent", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Drawback Summary Add Message sent"));
				AssertEquals("One message should have been generated", 1, declaration.Messages.Count);
			}
		}

		public void TestSendDrawbackSummaryDeleteMessage()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_DRWFilingMethod = DrawbackMethodOfFilingList.Codes.ABI;
			Factory.Save();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.drawbackSummaryDeleteMenuItem.PerformClick();
				Assert("Has already been added", UnitTestUserNotification.Instance.LastMessage.Text.Contains("the Drawback Summary has not been added yet"));
				AssertEquals("No message generated", 0, declaration.Messages.Count);
			}

			declaration.JE_EntryStatus = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal;
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryDelete;
			Factory.Save();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.drawbackSummaryDeleteMenuItem.PerformClick();
				Assert("Waiting for response", UnitTestUserNotification.Instance.LastMessage.Text.Contains("This Drawback is waiting for Customs response"));
				AssertEquals("No message generated", 0, declaration.Messages.Count);
			}

			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.drawbackSummaryDeleteMenuItem.PerformClick();
				Assert("Validation errors", UnitTestUserNotification.Instance.LastMessage.Text.Contains("the following message error"));
				AssertEquals("No message generated", 0, declaration.Messages.Count);
			}

			declaration.JE_MessageStatus = "";
			Factory.Save();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.drawbackSummaryDeleteMenuItem.PerformClick();
				Assert("Validation errors", UnitTestUserNotification.Instance.LastMessage.Text.Contains("the following message error"));
				AssertEquals("No message generated", 0, declaration.Messages.Count);
			}

			using (var formForTest = new FormForTestMoq(declaration))
			{
				Mock<EDIMenu> mockMenu = formForTest.MockMenu;
				EDIMenu menu = formForTest.MenuForTest;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.drawbackSummaryDeleteMenuItem.PerformClick();
				Assert("Message sent", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Drawback Summary Delete Message sent"));
				AssertEquals("One message should have been generated", 1, declaration.Messages.Count);
			}
		}

		public void TestSendCensusWarningQuery()
		{
			var declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "01";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				AssertEquals("PreCondition: CensusWarningQueryMessageMenu is visible", true, menu.censusWarningQueryMessageMenu.Visible);
				menu.censusWarningQueryMessageMenu.PerformClick();
				AssertContains(EDIMenu.NoFormalEntries, UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				menu.censusWarningQueryMessageMenu.PerformClick();
				AssertContains(EDIMenu.EntryNotAccepted, UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
				menu.censusWarningQueryMessageMenu.PerformClick();
				AssertEquals("One message should have been generated", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
			}
		}

		[ExpectNoExceptions]
		public void TestWhenDeclarationIsNull_Issue00215082()
		{
			using (EDIMenu menu = new EDIMenu())
			{
				menu.RefreshMenu();
			}
		}

		public void TestFTZForAutomationDisabled()
		{
			SetupBondedWarehouseEnvironment(true);
			var declaration = GetNewDeclaration(Factory, "BFTZ02132", "", "", "ENT324", 100m, JobMessageTypeList.Codes.FTZ);
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			warehouse.WW_WarehouseType = "FTZ";
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				foreach (var ftzMenu in menu.fTZMenus)
				{
					if (ftzMenu.Text != EDIMenu.Constants.SendPTTArrival && ftzMenu.Text != EDIMenu.Constants.SendPTTUnArrival)
					{
						Assert(ftzMenu.Visible);
					}
				}

				var customsEntryKey = declaration.FTZAdmissionNumberFormatted.ToString();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var item = menu.fTZMenus.FindByText(EDIMenu.Constants.SendFTZOriginalMessages);
				item.PerformClick();
				AssertEquals("FTZ Add Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, declaration.WarehouseTransactionStatus);
				var exportLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.Events.DataExportCode);
				AssertEquals(0, declaration.Logs.Find(exportLogQuery).Length);
				declaration.WarehouseTransactionStatus = ZString.Empty;
				declaration.AdmissionStatus = ZString.Empty;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				item.PerformClick();
				AssertEquals("FTZ Add Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals(1, declaration.Logs.Find(exportLogQuery).Length);
				AssertNotNull(declaration.GetLastHoldUniversalShipmentFromNote());
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				declaration.InvoiceLines[0].JI_InvoiceQuantity = 80m;
				declaration.AdmissionStatus = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd;
				Factory.Save();
				declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				item = menu.fTZMenus.FindByText(EDIMenu.Constants.SendFTZWithdrawalMessages);
				item.PerformClick();
				AssertEquals("FTZ Delete Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, declaration.WarehouseTransactionStatus);
				AssertEquals(1, declaration.Logs.Find(exportLogQuery).Length);
				AssertNotNull(declaration.GetLastHoldUniversalShipmentFromNote());
			}
		}

		public void TestSendFTZAmendmentMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.FTZZoneID = "1111111";
			declaration.FTZControlNumber = "ABC22222";
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				var item = menu.fTZMenus.FindByText(EDIMenu.Constants.SendFTZAmendmentMessages);

				item.PerformClick();
				AssertType<FTZMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSendFTZAdmissionMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.FTZZoneID = "1111111";
			declaration.FTZControlNumber = "ABC22222";
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				foreach (var ftzMenu in menu.fTZMenus)
				{
					if (ftzMenu.Text != EDIMenu.Constants.SendPTTArrival && ftzMenu.Text != EDIMenu.Constants.SendPTTUnArrival)
					{
						Assert(ftzMenu.Visible);
					}
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var item = menu.fTZMenus.FindByText(EDIMenu.Constants.SendFTZOriginalMessages);
				item.PerformClick();
				AssertEquals("FTZ Add Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message generated", 1, declaration.Messages.Count);
			}
		}

		public void TestSendFZApplicationMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var item = menu.fTZMenus.FindByText(EDIMenu.Constants.SendPTTMessages);
				item.PerformClick();
				AssertEquals("FTZ Permit To Transfer Message sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message generated", 1, declaration.Messages.Count);
			}
		}

		public void TestCancelPermitToTransferMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var item = menu.fTZMenus.FindByText(EDIMenu.Constants.CancelPTTMessage);
				item.PerformClick();
				AssertEquals("FTZ Cancel Permit To Transfer Message sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message generated", 1, declaration.Messages.Count);
			}
		}

		public void TestSendPermitToTransferArrival()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var item = menu.fTZMenus.FindByText(EDIMenu.Constants.SendPTTArrival);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
				{
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
					menu.RefreshMenu();
					AssertEquals("Visibility", true, item.Visible);

					declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
					menu.RefreshMenu();
					AssertEquals("Visibility", true, item.Visible);

					declaration.JE_TransportMode = TransportTypeList.Codes.Air;
					menu.RefreshMenu();
					AssertEquals("Visibility", false, item.Visible);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
				{
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
					menu.RefreshMenu();
					AssertEquals("Visibility", false, item.Visible);

					declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
					menu.RefreshMenu();
					AssertEquals("Visibility", false, item.Visible);

					declaration.JE_TransportMode = TransportTypeList.Codes.Air;
					menu.RefreshMenu();
					AssertEquals("Visibility", false, item.Visible);
				}

				Factory.Save();

				item.PerformClick();
				AssertEquals("FTZ Permit To Transfer Arrival Message sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message generated", 1, declaration.Messages.Count);
			}
		}

		public void TestSendPermitToTransferUnArrival()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				var mockMenu = formForTest.MockMenu;
				var menu = formForTest.MenuForTest;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var item = menu.fTZMenus.FindByText(EDIMenu.Constants.SendPTTUnArrival);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
				{
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
					menu.RefreshMenu();
					AssertEquals("Visibility", true, item.Visible);

					declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
					menu.RefreshMenu();
					AssertEquals("Visibility", true, item.Visible);

					declaration.JE_TransportMode = TransportTypeList.Codes.Air;
					menu.RefreshMenu();
					AssertEquals("Visibility", false, item.Visible);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
				{
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
					menu.RefreshMenu();
					AssertEquals("Visibility", false, item.Visible);

					declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
					menu.RefreshMenu();
					AssertEquals("Visibility", false, item.Visible);

					declaration.JE_TransportMode = TransportTypeList.Codes.Air;
					menu.RefreshMenu();
					AssertEquals("Visibility", false, item.Visible);
				}

				Factory.Save();

				item.PerformClick();
				AssertEquals("FTZ Send Permit To Transfer Un-Arrival Message sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message generated", 1, declaration.Messages.Count);
			}
		}

		public void TestSendUnconcurrenceMessageMenuVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (var form = new ZForm(declaration))
			{
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.RefreshMenu();
					var unconcurMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.SendPostAdmissionCorrection);
					AssertEquals("Send Post Admission Correction Message is visible", true, unconcurMenuItem.Visible);
				}
			}
		}

		public void TestBIRDExportMenuVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (ZForm form = new ZForm(declaration))
			using (EDIMenu menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.RefreshMenu();
				menu.Declaration = declaration;
				AssertEquals("PreCondition: BIRDExportMenu is visible for ACS jobs", true, menu.bIRDExportMenu.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				menu.RefreshMenu();
				AssertEquals("PreCondition: BIRDExportMenu is hide for ACE jobs", false, menu.bIRDExportMenu.Visible);
			}
		}

		public void TestSendPGADataCorrectionMenuItem()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				Mock<EDIMenu> mockMenu = formForTest.MockMenu;
				EDIMenu menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				AssertEquals(true, menu.pGACorrectionMessageMenu.Visible);
				mockMenu
					.Protected()
					.Setup<bool>("ShowImportMessageSendingActionForm", ItExpr.IsAny<ImportMessageSendingActionCollection>())
					.Returns(true);
				menu.pGACorrectionMessageMenu.PerformClick();
				AssertEquals("There is no Entry available for sending PGA Correction message to Customs and no new PGA lines available for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				declaration.Factory.Save();
				menu.pGACorrectionMessageMenu.PerformClick();
				AssertEquals("There are no PGA lines available for sending PGA Correction. No messages generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.US_DDTCLicenseNo = "S6123122";
				declaration.Factory.Save();
				menu.pGACorrectionMessageMenu.PerformClick();
				AssertEquals("PGA Line ID, i.e. entry line numbers and/or tariffs have changed and as such, you should send an ACE Cargo release replacement instead of a PGA Correction message to update PGA details. Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRequestTariffUpdates_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				EDIMenu menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				AssertEquals(false, menu.requestTariffUpdateMenu.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			declaration.Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				EDIMenu menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				AssertEquals(true, menu.requestTariffUpdateMenu.Visible);
				menu.requestTariffUpdateMenu.PerformClick();
				AssertEquals("Tariff information has been requested from ABI, once the request has been processed by Customs, select 'Refresh Tariff Details' from the Brokerage menu to update the tariff details in this job.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode = ZString.Empty;
			using (var formForTest = new FormForTestMoq(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				EDIMenu menu = formForTest.MenuForTest;
				menu.requestTariffUpdateMenu.PerformClick();
				AssertEquals("No ABI filer code found. The ABI tariff query can only be used when an active ABI profile is found. No Query was sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRefreshNotificationDispositionActionsMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var message = header.Messages.AddNew(typeof(EDIMessage));
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;

			using (var form = new FormForTestMoq(declaration))
			{
				var menu = form.MenuForTest;
				var menuItem = menu.refreshNotificationDispositionActions;
				Assert(!menuItem.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				menu.RefreshMenu();
				Assert(menuItem.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				menu.RefreshMenu();
				Assert(menuItem.Visible);
				menuItem.PerformClick();
				AssertEquals("Declaration's notification disposition actions have been updated.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(declaration.JE_AddInfo, "ENSAction=Incomplete");
			}
		}

		public void TestSendAESMessageAgainstExportEntryFilerID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var formForTest = new FormForTestMoq(declaration))
			{
				Mock<EDIMenu> mockMenu = formForTest.MockMenu;
				EDIMenu menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				menu.aESMessagingMenuItem.PerformClick();
				AssertEquals("Entry Filer ID has not been set for this branch. Please set it in the Registry > Customs > United States of America > Export > AES > Entry Filer ID.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "123456789";
			filer.EntryFilerIDType = "D";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			using (var formForTest = new FormForTestMoq(declaration))
			{
				Mock<EDIMenu> mockMenu = formForTest.MockMenu;
				EDIMenu menu = formForTest.MenuForTest;
				menu.RefreshMenu();
				menu.aESMessagingMenuItem.PerformClick();
				AssertNotEquals("Entry Filer ID has not been set for this branch. Please set it in the Registry > Customs > United States of America > Export > AES > Entry Filer ID.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2018, 10, 8)]
		public void TestResetDutyCalculationDateDate_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2018, 10, 13);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				var resetDutyCalculationMenuItem = menu.MenuItems.FindByText(EDIMenu.Constants.ResetDutyCalculationDate);
				AssertEquals(false, resetDutyCalculationMenuItem.Visible);
				declaration.DoMerge();
				menu.RefreshMenu();
				AssertEquals(false, resetDutyCalculationMenuItem.Visible);
				var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				ensEntry.US_DutyCalcDate = new ZDateTime(2018, 10, 12);
				menu.RefreshMenu();
				AssertEquals(true, resetDutyCalculationMenuItem.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				resetDutyCalculationMenuItem.PerformClick();
				AssertEquals(new ZDateTime(2018, 10, 13), ensEntry.US_DutyCalcDate);
				var rstLog1 = declaration.Logs.MostRecentLogByEventTime(ZArchitecture.Business.Events.ResetEntryMessageItemFunction);
				AssertEquals("DUTY CALC DATE CHANGED 12-OCT-18 TO 13-OCT-18", rstLog1.SL_Reference);
				AssertEquals("Duty Calculation Date has been changed from (12-OCT-18) to (13-OCT-18).", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				resetDutyCalculationMenuItem.PerformClick();
				AssertEquals(new ZDateTime(2018, 10, 13), ensEntry.US_DutyCalcDate);
				AssertNull(declaration.Logs.MostRecentLogByEventTime(ZArchitecture.Business.Events.ResetEntryMessageItemFunction, new ZQuery(StmALogSchema.PK, SQLComparisonOperator.NotEqual, rstLog1.PK)));
				AssertEquals("No change to Duty Calculation Date (13-OCT-18) has been found.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.US_EstimatedEntryDate = new ZDateTime(2018, 10, 15);
				resetDutyCalculationMenuItem.PerformClick();
				AssertEquals(new ZDateTime(2018, 10, 15), ensEntry.US_DutyCalcDate);
				var rstLog2 = declaration.Logs.MostRecentLogByEventTime(ZArchitecture.Business.Events.ResetEntryMessageItemFunction, new ZQuery(StmALogSchema.PK, SQLComparisonOperator.NotEqual, rstLog1.PK));
				AssertEquals("DUTY CALC DATE CHANGED 13-OCT-18 TO 15-OCT-18", rstLog2.SL_Reference);
				AssertEquals("Duty Calculation Date has been changed from (13-OCT-18) to (15-OCT-18).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFTZCargoManifestStatusQuery()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Weight = 9000m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_InvoiceQuantity = 10000m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsQuantity = 70m;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MWB12321";
			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillNum = "HWB23423";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			using (var formForTest = new FormForTestMoq(declaration))
			{
				var menu = formForTest.MenuForTest;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var item = menu.fTZMenus.FindByText(EDIMenu.Constants.FTZCargoManifestStatusQuery);
				var showQueryForm = false;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					showQueryForm = form is CargoManifestStatusQueryActionForm;
				});
				declaration.JE_VoyageFlightNo = "1206";
				Assert(declaration.HasChanges);
				item.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Not saved", declaration.HasChanges);
				Assert("Not show CargoManifestStatusQueryActionForm", !showQueryForm);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				item.PerformClick();
				Assert("Saved", !declaration.HasChanges);
				Assert("Show CargoManifestStatusQueryActionForm", showQueryForm);
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		public void TestSynchronizeWithOrdersMenuItem_Click()
		{
			OrgHeader warehouseImporter = Factory.NewWithValidTestData<OrgHeader>();
			warehouseImporter.CompanyData.OB_IMUsedBondedWhs = true;
			warehouseImporter.OH_IsWarehouseClient = true;
			OrgHeader warehouse = Factory.NewWithValidTestData<OrgHeader>();
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_OH_Importer = warehouseImporter.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_PresentationDate = new ZDateTime(2017, 10, 14);
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "40000007";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 14);
			Factory.Save();
			var permit = declaration.FindRelatedPermits()[0];
			var lineTransaction1 = permit.CusPermitLineTransactions.AddNew();
			lineTransaction1.CPL_TransactionStatus = Customs.Business.PermitTransactionStatusList.Codes.Pending;
			var lineTransaction2 = permit.CusPermitLineTransactions.AddNew();
			declaration.US_EnableENS = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			using (var formForTest = new FormForTestMoq(declaration))
			{
				Mock<EDIMenu> mockMenu = formForTest.MockMenu;
				MenuItem synchronizeWithOrdersMenuItem = formForTest.MenuForTest.SynchronizeWithOrdersMenuItemInternal;
				synchronizeWithOrdersMenuItem.PerformClick();
				AssertEquals("Cannot synchronize. Please check that all orders are finalized.", UnitTestUserNotification.Instance.LastMessage.Text);
				lineTransaction1.CPL_TransactionStatus = ZString.Empty;
				synchronizeWithOrdersMenuItem.PerformClick();
				AssertEquals("System will replace existing invoice data with the data from Orders. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void SetupBondedWarehouseEnvironment(bool isVirtualWarehouse)
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
				currentStaff.GS_EmailAddress = "test@test.com.au";
				var inwardDeclaration = GetNewDeclaration(Factory, "B00002131", EntryTypeList.Codes.Warehouse, "XJ5", "ENT111", 1m);
				Factory.Save();
				// Force creation of warehouse
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				inwardDeclaration.PublishCancelEventForWHSInwardAndSaveIfNeeded();
				Warehouse.MainAddress.SetWarehouseType(isVirtualWarehouse);
				Factory.Save();
			}
		}

		void AssertBondedWarehouseRequiredFieldsMessage(ZString message, bool hasBondedWarehouseAddress, bool hasImporterDocumentAddress, bool hasProduct, bool hasInvoiceQty, bool hasWHSPackageQty, bool hasWHSEntryLineNo, bool hasWHSEntryNumber)
		{
			if (hasBondedWarehouseAddress)
			{
				AssertContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", message);
			}
			else
			{
				AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", message);
			}

			if (hasImporterDocumentAddress)
			{
				AssertContains("Importer Documentary Address is required for Inventory Management integration.", message);
			}
			else
			{
				AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", message);
			}

			if (hasProduct)
			{
				AssertContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", message);
			}
			else
			{
				AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", message);
			}

			if (hasInvoiceQty)
			{
				AssertContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", message);
			}
			else
			{
				AssertNotContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", message);
			}

			if (hasWHSPackageQty)
			{
				AssertContains("An Invoice Line marked for Inventory Management must have a Package Quantity specified; not all Invoice Lines marked for Inventory Management have a Package Quantity specified.", message);
			}
			else
			{
				AssertNotContains("An Invoice Line marked for Inventory Management must have a Package Quantity specified; not all Invoice Lines marked for Inventory Management have a Package Quantity specified.", message);
			}

			if (hasWHSEntryLineNo)
			{
				AssertContains("An Invoice Line marked for Inventory Management must have a Entry Line Number specified; not all Invoice Lines marked for Inventory Management have an Entry Line Number specified.", message);
			}
			else
			{
				AssertNotContains("An Invoice Line marked for Inventory Management must have a Entry Line Number specified; not all Invoice Lines marked for Inventory Management have an Entry Line Number specified.", message);
			}

			if (hasWHSEntryNumber)
			{
				AssertContains("Warehouse Entry Filer Code and Entry Number are required for Inventory Management integration.", message);
			}
			else
			{
				AssertNotContains("Warehouse Entry Filer Code and Entry Number are required for Inventory Management integration.", message);
			}
		}

		bool HasThisWarned(string message)
		{
			foreach (UnitTestUserNotification.PreviousMessage previousMessage in UnitTestUserNotification.Instance.PreviousMessages)
			{
				if (previousMessage.Text != null && previousMessage.Text.IndexOf(message) >= 0)
				{
					return true;
				}
			}

			return false;
		}

		void SetUpExternalBroker(JobDeclaration declaration)
		{
			OrgHeader externalBroker = Factory.LoadTop1<OrgHeader>(new ZQuery());
			EDICommunicationsMode mode = externalBroker.EDICommunicationsModes.AddNew();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			mode.EK_Destination = "test@ema.com";
			mode.EK_ServerAddressSubject = "EmailAsAttchMode";
			mode.EK_Module = EDICommunicationsMode.Modules.US_BIRD;
			declaration.JE_OH_ExternalBroker = externalBroker.PK;
		}

		JobDeclaration GetNewDeclaration(BusinessObjectFactory factory, ZString declarationReference, ZString entryType, ZString entryFilerCode, ZString entryNumber, ZDecimal quantity, ZString? messageType = null, bool isVirtualWarehouse = true)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType.HasValue ? messageType.Value.ToString() : JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			if (warehouse == null)
			{
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				warehouse = (IWhsWarehouse)helper.CreateWarehouse(Warehouse.MainAddress.OA_Address1, "WHS", "BOND");
				warehouse.WW_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = isVirtualWarehouse;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				warehouse.WW_AutoPrintPackingSlip = false;
			}

			var isFTZAdmission = declaration.IsFTZAdmission;
			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			SetupQuantity(quantity, invoice, invoiceLine);
			if (isFTZAdmission)
			{
				declaration.FTZZoneID = "1530100";
				declaration.FTZYear = "15";
				declaration.FTZControlNumber = entryNumber;
			}
			else
			{
				declaration.US_EnableENS = true;
				declaration.US_EntryType = entryType;
				if (declaration.IsExWarehouseEntryType)
				{
					declaration.US_WHSEntryFilerCode = entryFilerCode;
					declaration.US_WHSEntryNumber = entryNumber;
					invoiceLine.US_WHSEntryLineNo = 1;
				}
				else if (declaration.IsENSFormalImportAndConsumptionFTZ)
				{
					invoiceLine.US_WHSEntryNumber = entryFilerCode + "-" + entryNumber;
					invoiceLine.US_WHSEntryLineNo = 1;
				}
				else
				{
					declaration.US_EntryFilerCode = entryFilerCode;
					declaration.ImportEntryNumber = entryNumber;
				}
			}

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			return declaration;
		}

		void SetupQuantity(ZDecimal quantity, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine)
		{
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			invoiceLine.JI_BondedWhsQuantity = quantity;
		}

		OrgHeader importer;
		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_Code = "IMP";
					importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
					importer.MiscServ.OM_IMPartAttrib1Type = "NON";
					importer.CompanyData.OB_IMUsedBondedWhs = true;
					importer.OH_IsWarehouseClient = true;
				}

				return importer;
			}
		}

		OrgHeader warehouse;
		OrgHeader Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.New<OrgHeader>();
					warehouse.OH_Code = "W1";
					warehouse.OH_RL_NKClosestPort = "USCHI";
					warehouse.MainAddress.LocalControlledPremisesID = "23423";
				}

				return warehouse;
			}
		}

		Business.OrgSupplierPart part;
		Business.OrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<Business.OrgSupplierPart>();
					part.OP_PartNum = "~~1";
					part.OP_StockKeepingUnit = "NO";
					part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
					CusClassPartPivot importPivot = part.PivotsForBinding.AddNew();
					importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
					importPivot.CI_CC = Classification.PK;
				}

				return part;
			}
		}

		CusClassification classification;
		CusClassification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<CusClassification>();
					classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
					classification.CC_LookupCode = "~~1L";
					classification.CC_TariffNum = "4901.10.00 01";
				}

				return classification;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
		}

		sealed class EDIMenuForTest : EDIMenu
		{
			internal EDIMenuForTest() : base()
			{
			}

			internal void SetupTopLevelMenuInternal() => SetupTopLevelMenu();
		}

		sealed class FormForTestMoq : JobDeclarationForm
		{
			public FormForTestMoq(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override IEDIMenu GetNewTopLevelMenuCore() => MenuForTest;

			readonly Mock<EDIMenu> mockMenu = new Mock<EDIMenu> { CallBase = true };
			internal Mock<EDIMenu> MockMenu => mockMenu;

			internal EDIMenu MenuForTest => MockMenu.Object;
		}
	}
}
