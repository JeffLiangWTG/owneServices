using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Service.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.US.Business.CusEntryHeader;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;
using PermitTransactionTypeList = Enterprise.Customs.US.Business.PermitTransactionTypeList;
using UniversalReferenceConstants = Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(ImportMessageSendingActionForm))]
	sealed class ImportMessageSendingActionFormTest : ZFormBasherTest
	{
		public void TestACEPaymentMadePanelVisibility()
		{
			//test Suppress Payment Info box will be show when the reference is CER(CS00757643) and WRW/WRC and CEO, WAW or WAC
			// ACE Declaration
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_PSC = false;
			declaration.US_EntryFilerCode = "XXX";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearEntrySummaryReplace;
			Factory.Save();
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", form.ACEPaymentMadePanel.Visible);
			}

			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", form.ACEPaymentMadePanel.Visible);
			}

			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings;
			Factory.Save();
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", form.ACEPaymentMadePanel.Visible);
			}

			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", form.ACEPaymentMadePanel.Visible);
			}

			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings;
			Factory.Save();
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", form.ACEPaymentMadePanel.Visible);
			}

			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", form.ACEPaymentMadePanel.Visible);
			}
		}

		public void TestPanelsMovedToSeparateTabs()
		{
			//to enable billing job ready for posting
			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			options.PreApprovalBillingJob = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);
			// ACE Declaration
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(!form.ACSEntrySummaryActionTabPage.TabVisible);
				Assert(!form.ACECargoReleaseTabPage.TabVisible);
				Assert(!form.ACSCargoReleaseTabPage.TabVisible);
				Assert(!form.MiscTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("ACETopPanel", form.ACETopPanel.Visible);
				Assert("Send Checkbox", form.ACE_US_SendMessageCheckBox.Visible);
				Assert("EntrySummaryPanel should be visible", form.EntrySummaryPanel.Visible);
				Assert("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", form.ACEPaymentMadePanel.Visible);
				Assert("BillingReadyPanel should be visible due to registry setting", form.BillingReadyPanel.Visible);
				Assert("ACESignOriginalAmendmentPanel should be visible", form.ACESignOriginalAmendmentPanel.Visible);
				Assert("CertifyCargoReleasePanel should be visible", form.CertifyCargoReleasePanel.Visible);
			}

			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.PSTLines.AddNew();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.VehicleLines.AddNew();
			invoiceLine.FDAs.AddNew();
			invoiceLine.FCCs.AddNew();
			invoiceLine.LaceyActLines.AddNew();
			invoiceLine.FSISLines.AddNew();
			invoiceLine.NMFSLines.AddNew();
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DDTCLicenseNo = "111;";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = "+";
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				var seAction = (EntryHeaderMessageSendingAction)form.EntriesGrid.ListManager.List[0];
				seAction.US_SendMessage = true;
				seAction.US_CertifyCargoRelease = true;
			}

			// ACE Declaration update
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement)))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("ACETopPanel", form.ACETopPanel.Visible);
				Assert("Send Checkbox", form.ACE_US_SendMessageCheckBox.Visible);
				Assert("EntrySummaryPanel should be visible", form.EntrySummaryPanel.Visible);
				Assert("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", form.ACEPaymentMadePanel.Visible);
				Assert("BillingReadyPanel should be visible due to registry setting", form.BillingReadyPanel.Visible);
				Assert("ACESignOriginalAmendmentPanel should be visible", form.ACESignOriginalAmendmentPanel.Visible);
				Assert("CertifyCargoReleasePanel should be visible", form.CertifyCargoReleasePanel.Visible);
				Assert("ACEntrySummarySE13Data", form.ACEEntrySummarySE13DataPanel.Visible);
			}

			// ACE Declaration delete
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion)))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("ACETopPanel", form.ACETopPanel.Visible);
				Assert("Send Checkbox", form.ACE_US_SendMessageCheckBox.Visible);
			}

			// ACE Cargo Release
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.US_VNEInd = "D";
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DDTCLicenseNo = "111;";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = "+";
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.EntriesGrid.ListManager.Position = 1;
				Assert(form.ACECargoReleaseTabPage.TabVisible);
				Assert(!form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(!form.ACSEntrySummaryActionTabPage.TabVisible);
				Assert(!form.ACSCargoReleaseTabPage.TabVisible);
				Assert(!form.MiscTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACECargoReleaseTabPage;
				Assert("ACECargoTopPanel", form.ACECargoTopPanel.Visible);
				Assert("Send Checkbox", form.ACECargo_US_SendMessageCheckBox.Visible);
				Assert("SEUpdateActionPanel should not be visible", !form.SEUpdateActionPanel.Visible);
				Assert("SEDeleteActionPanel should not be visible", !form.SEDeleteActionPanel.Visible);
			}

			// ACE Cargo Release Update
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement)))
			{
				form.Show();
				form.EntriesGrid.ListManager.Position = 1;
				Assert(form.ACECargoReleaseTabPage.TabVisible);
				Assert("ACECargoTopPanel", form.ACECargoTopPanel.Visible);
				Assert("Send Checkbox", form.ACECargo_US_SendMessageCheckBox.Visible);
				Assert("SEUpdateActionPanel should be visible", form.SEUpdateActionPanel.Visible);
			}

			// ACE Cargo Release Delete
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion)))
			{
				form.Show();
				form.EntriesGrid.ListManager.Position = 1;
				Assert(form.ACECargoReleaseTabPage.TabVisible);
				Assert("ACECargoTopPanel", form.ACECargoTopPanel.Visible);
				Assert("Send Checkbox", form.ACECargo_US_SendMessageCheckBox.Visible);
				Assert("SEDeleteActionPanel should be visible", form.SEDeleteActionPanel.Visible);
			}

			// ACS Declaration
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				Assert(form.ACSEntrySummaryActionTabPage.TabVisible);
				Assert(!form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(!form.ACECargoReleaseTabPage.TabVisible);
				Assert(!form.ACSCargoReleaseTabPage.TabVisible);
				Assert(!form.MiscTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACSEntrySummaryActionTabPage;
				Assert("ACSTopPanel", form.ACSTopPanel.Visible);
				Assert("Send Checkbox", form.ACS_US_SendMessageCheckBox.Visible);
				Assert("BillingReadyPanel should be visible due to registry setting", form.ACSBillingReadyPanel.Visible);
				Assert("CertifyCargoReleasePanel should be visible", form.ACSCertifyCargoReleasePanel.Visible);
			}

			// ACS Declaration Update
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement)))
			{
				form.Show();
				Assert(form.ACSEntrySummaryActionTabPage.TabVisible);
				Assert("ACSTopPanel", form.ACSTopPanel.Visible);
				Assert("Send Checkbox", form.ACS_US_SendMessageCheckBox.Visible);
				Assert("BillingReadyPanel should be visible due to registry setting", form.ACSBillingReadyPanel.Visible);
				Assert("CertifyCargoReleasePanel should be visible", form.ACSCertifyCargoReleasePanel.Visible);
			}

			// ACS Declaration Delete
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion)))
			{
				form.Show();
				Assert(form.ACSEntrySummaryActionTabPage.TabVisible);
				Assert("ACSTopPanel", form.ACSTopPanel.Visible);
				Assert("Send Checkbox", form.ACS_US_SendMessageCheckBox.Visible);
			}

			// ACS Cargo Release
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "XXA";
			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			ErrorReporter.Clear();
			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.EntriesGrid.ListManager.Position = 1;
				Assert(form.ACSCargoReleaseTabPage.TabVisible);
				Assert(!form.ACSEntrySummaryActionTabPage.TabVisible);
				Assert(!form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(!form.ACECargoReleaseTabPage.TabVisible);
				Assert(!form.MiscTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACSCargoReleaseTabPage;
				Assert("ACSCargoTopPanel", form.ACSCargoTopPanel.Visible);
				Assert("Send Checkbox", form.US_SendMessageCheckBox_ACSCargo.Visible);
				Assert("CertifyCargoReleasePanel should be visible", form.ACSCargoCertifyCargoReleasePanel.Visible);
			}

			// ACS Cargo Release Update 
			declaration.ActiveEntryHeaders.CargoReleaseEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement)))
			{
				form.Show();
				Assert("ACSCargoTopPanel", form.ACSCargoTopPanel.Visible);
				Assert("Send Checkbox", form.US_SendMessageCheckBox_ACSCargo.Visible);
				Assert("CertifyCargoReleasePanel should be visible", form.ACSCargoCertifyCargoReleasePanel.Visible);
			}

			// ACS Cargo Release Delete 
			using (var form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion)))
			{
				form.Show();
				form.EntriesGrid.ListManager.Position = 1;
				Assert("ACSCargoTopPanel", form.ACSCargoTopPanel.Visible);
				Assert("Send Checkbox", form.US_SendMessageCheckBox_ACSCargo.Visible);
			}
		}

		public void TestRemoveColumns()
		{
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.ExtendTIB))
			{
				form.Show();
				AssertNull("US_CertifyCargoRelease should be hidden", form.EntriesGrid.Columns[ImportMessageSendingAction.Schema.US_CertifyCargoRelease]);
			}

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (var form = CreateNewForm(ImportMessageSendingMessageType.Deletion))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("EntrySummaryPanel", form.EntrySummaryPanel.Visible);
				AssertEquals("ACESignOriginalAmendmentPanel should be hidden for deletion", false, form.ACESignOriginalAmendmentPanel.Visible);
				AssertEquals("ACEPaymentMadePanel should be hidden for deletion", false, form.ACEPaymentMadePanel.Visible);
			}

			using (var form = CreateNewForm(ImportMessageSendingMessageType.Original))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("EntrySummaryPanel", form.EntrySummaryPanel.Visible);
				Assert("SEUpdateActionPanel", !form.SEUpdateActionPanel.Visible);
				AssertEquals("ACESignOriginalAmendmentPanel should be visible for Original", true, form.ACESignOriginalAmendmentPanel.Visible);
				AssertEquals("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", true, form.ACEPaymentMadePanel.Visible);
			}

			Declaration.US_EnableCRL = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Declaration.US_EntryFilerCode = "XJ5";
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			using (var form = CreateNewForm(ImportMessageSendingMessageType.Replacement))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("EntrySummaryPanel", form.EntrySummaryPanel.Visible);
				AssertEquals("ACESignOriginalAmendmentPanel", true, form.ACESignOriginalAmendmentPanel.Visible);
				var action1 = (EntryHeaderMessageSendingAction)form.EntriesGrid.ListManager.List[0];
				AssertEquals("SEUpdateActionPanel", action1.IsACECargoRelease, form.SEUpdateActionPanel.Visible);
				var action2 = (EntryHeaderMessageSendingAction)form.EntriesGrid.ListManager.List[1];
				form.EntriesGrid.ListManager.Position = 1;
				AssertEquals("SEUpdateActionPanel", action2.IsACECargoRelease, form.SEUpdateActionPanel.Visible);
				Assert(form.SEDISIndicatorForUpdateCheckBox.Visible);
				Assert(!form.DISIDRefNoForUpdateDropEdit.Visible);
				action2.US_SE_DISIndicator = true;
				Assert(form.DISIDRefNoForUpdateDropEdit.Visible);
			}

			using (var form = CreateNewForm(ImportMessageSendingMessageType.Deletion))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("EntrySummaryPanel", form.EntrySummaryPanel.Visible);
				Assert("SEUpdateActionPanel", !form.SEUpdateActionPanel.Visible);
			}

			Declaration.US_PSC = true;
			using (var form = CreateNewForm(ImportMessageSendingMessageType.Original))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert("EntrySummaryPanel", form.EntrySummaryPanel.Visible);
				AssertEquals("ACESignOriginalAmendmentPanel should be visible for Original", true, form.ACESignOriginalAmendmentPanel.Visible);
				AssertEquals("ACEPaymentMadePanel should be visible for Original, because Entry has been lodged", false, form.ACEPaymentMadePanel.Visible);
			}

			Declaration.US_PSC = false;
			using (var form = CreateNewForm(ImportMessageSendingMessageType.Replacement))
			{
				form.Show();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				Assert("EntrySummaryPanel", form.EntrySummaryPanel.Visible);
				AssertEquals("ACESignOriginalAmendmentPanel should be visible for Amendment", true, form.ACESignOriginalAmendmentPanel.Visible);
				AssertEquals("ACEPaymentMadePanel should be visible for Amendment", true, form.ACEPaymentMadePanel.Visible);
			}

			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			options.PreApprovalBillingJob = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.StandAlonePriorNotice))
			{
				form.Show();
				AssertNull("US_CertifyCargoRelease should be hidden", form.EntriesGrid.Columns[ImportMessageSendingAction.Schema.US_CertifyCargoRelease]);
			}

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.Replacement))
			{
				form.Show();
				AssertNull("US_CertifyCargoRelease should not be visible for ExWarehouse", form.EntriesGrid.Columns[ImportMessageSendingAction.Schema.US_CertifyCargoRelease]);
			}
		}

		public void TestControlVisibiiityForACECargoReleaseOptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement, (x) => x.IsACECargoRelease);
			using (var form = new ImportMessageSendingActionForm(actions))
			{
				form.Show();
				Assert("SEUpdateActionPanel", form.SEUpdateActionPanel.Visible);
			}
		}

		public void TestControlVisibiiityForACECargoReleaseOptionsDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion, (x) => x.IsACECargoRelease);
			using (var form = new ImportMessageSendingActionForm(actions))
			{
				form.Show();
				Assert("SEDeleteActionPanel", form.SEDeleteActionPanel.Visible);
				Assert("EntrySummaryPanel", !form.EntrySummaryPanel.Visible);
			}
		}

		public void TestSelectAllMenu()
		{
			CusEntryHeader ensEntry = Declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.Original))
			{
				AssertEquals("More than 1 action", 2, form.actions.Count);
				ZFormModaliser.ShowDialogWithoutDispose(form);
				MenuItem tickSendAllMenuItem = form.EntriesGrid.ContextMenu.MenuItems.FindByText("Tick 'Send' for All");
				AssertNotNull("Precondition: Tick 'Send' for All", tickSendAllMenuItem);
				tickSendAllMenuItem.PerformClick();
				foreach (ImportMessageSendingAction action in form.actions)
				{
					AssertEquals(true, action.US_SendMessage);
				}

				MenuItem untickSendAllMenuItem = form.EntriesGrid.ContextMenu.MenuItems.FindByText("Untick 'Send' for All");
				AssertNotNull("Precondition: Untick 'Send' for All", untickSendAllMenuItem);
				untickSendAllMenuItem.PerformClick();
				foreach (ImportMessageSendingAction action in form.actions)
				{
					AssertEquals(false, action.US_SendMessage);
				}
			}
		}

		public void TestClickSendButton()
		{
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.Original))
			{
				form.actions[0].US_SendMessage = false;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				AssertEquals(ImportMessageSendingActionForm.YouHaveNotSelectedAnythingToSendMessagesFor, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsCancelled", true, form.actions.IsCancelled);
				form.CancelButton_Click(form.CancelButton, EventArgs.Empty);
				AssertEquals("IsCancelled", true, form.actions.IsCancelled);
			}
		}

		public void TestClickSendButtonWarnsAboutMessageErrors_ACE()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.DispositionCodes.AddNew();
			Entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(ImportMessageSendingActionCollection))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				ImportMessageSendingActionCollection[0].US_SendMessage = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				ImportMessageSendingActionCollection[0].US_CertifyCargoRelease = true;
				AssertHasMessageErrors(ImportMessageSendingActionCollection[0].US_CertifyCargoReleaseInfo);
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				AssertEquals("IsCancelled", true, ImportMessageSendingActionCollection.IsCancelled);
				AssertEquals("Message error is warned", ImportMessageSendingActionForm.ThereIsANotification, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				AssertEquals("IsCancelled", false, ImportMessageSendingActionCollection.IsCancelled);
				AssertEquals("Message error is warned", ImportMessageSendingActionForm.ThereIsANotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickSendButtonWarnsAboutMessageErrors_ACS()
		{
			Declaration.DispositionCodes.AddNew();
			Entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(ImportMessageSendingActionCollection))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				ImportMessageSendingActionCollection[0].US_SendMessage = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				ImportMessageSendingActionCollection[0].US_CertifyCargoRelease = true;
				AssertHasMessageErrors(ImportMessageSendingActionCollection[0].US_CertifyCargoReleaseInfo);
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				AssertEquals("IsCancelled", true, ImportMessageSendingActionCollection.IsCancelled);
				AssertEquals("Message error is warned", "There are errors that need to be corrected before this message can be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				AssertEquals("IsCancelled", true, ImportMessageSendingActionCollection.IsCancelled);
				AssertEquals("Message error is warned", "There are errors that need to be corrected before this message can be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickSendButtonWithMessageErrorsWithNoSecurityRights()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_ConsolACE = true;
			using (var form = new ImportMessageSendingActionForm(ImportMessageSendingActionCollection))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var action = ImportMessageSendingActionCollection[0];
				action.US_SendMessage = true;
				action.US_CertifyCargoRelease = true;
				action.US_SE_ContactName = "AMY XIANG";
				action.US_SE_ContactPhone = "123456";
				AssertHasMessageErrors(action.US_CertifyCargoReleaseInfo);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				AssertEquals("Message error and no security rights is warned", Enterprise.Customs.Business.MessageSendingValidation.MessageErrorsExistWithNoSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Declaration.US_ConsolACE = false;
				action.US_AcknowledgeAndSign = true;
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				AssertNull("No Notifications", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
		}

		public void TestJobReadyForPostingCheckBoxVisibility()
		{
			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			options.PreApprovalBillingJob = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.ExtendTIB))
			{
				form.Show();
				Assert(!form.BillingJobReadyForPostingCheckBox.Visible);
			}

			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.Original))
			{
				form.Show();
				AssertEquals(true, form.ACSEntrySummaryActionTabPage.TabVisible);
				form.MessageDetailsTabControl.SelectedTab = form.ACSEntrySummaryActionTabPage;
				AssertEquals(true, form.ACSBillingReadyPanel.Visible);
				AssertEquals(true, form.ACSBillingJobReadyForPostingCheckBox.Visible);
				Assert(form.ACSBillingJobReadyForPostingCheckBox.Visible);
			}

			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.EntrySummaryQuery))
			{
				form.Show();
				Assert(!form.BillingJobReadyForPostingCheckBox.Visible);
			}
		}

		public void TestDeletionMessageOnPermitWithOrders()
		{
			// - create FTZ declaration
			Declaration.JE_OH_Importer = WarehouseImporter.PK;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			Declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			Declaration.US_EnableCRL = true;
			Declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Declaration.US_PresentationDate = new ZDateTime(2017, 10, 14);
			Declaration.US_EntryFilerCode = "SV9";
			Declaration.ImportEntryNumber = "40000007";
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			// this line creates permit
			Declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 14);
			Factory.Save();
			// - check that permit exists and not closed
			List<CusPermitHeader> permits = Declaration.FindRelatedPermits();
			AssertEquals(1, permits.Count);
			CusPermitHeader permit = permits[0];
			AssertEquals(false, permit.CPH_IsClosed);
			// - check that 'Deletion' message is allowed 
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.Deletion))
			{
				form.Show();
				var cargoReleaseAction = form.actions.Cast<ImportMessageSendingAction>().Single(a => a.IsACECargoRelease);
				cargoReleaseAction.US_SendMessage = true;
				form.actions.RunPreSaveValidation();
				AssertNoErrors(cargoReleaseAction.US_SendMessageInfo);
			}

			// - create transaction in permit
			var lineTransaction = permit.CusPermitLineTransactions.AddNew();
			lineTransaction.CPL_Reference = "TEST";
			lineTransaction.CPL_TranQty = 1;
			lineTransaction.CPL_TranValue = 1;
			lineTransaction.CPL_TransactionDate = ZDateTime.Now;
			lineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			lineTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			lineTransaction.CPL_AppId = PermitTransactionAppIdList.Codes.WarehouseOrder;
			lineTransaction.CPL_Comment = ZString.Empty;
			lineTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			Factory.Save();
			// - show form
			// - check that 'Deletion' message is not allowed 
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.Deletion))
			{
				form.Show();
				var cargoReleaseAction = form.actions.Cast<ImportMessageSendingAction>().Single(a => a.IsACECargoRelease);
				cargoReleaseAction.US_SendMessage = true;
				form.actions.RunPreSaveValidation();
				AssertHasError(cargoReleaseAction.US_SendMessageInfo, "This weekly estimate has warehouse orders against it and cannot be deleted.");
			}

			IPermitService permitService = ObjectFactory.Get<IPermitService>();
			// - relinquish transaction in permit
			AssertEquals(SuccessOrFailure.Success, permitService.RelinquishPermitTransactions(new[] { PermitTransactionDetailForTesting.NewWithTestData(Factory, lineTransaction.CPL_Reference) }));
			// - check that 'Deletion' message is allowed 
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.Deletion))
			{
				form.Show();
				var cargoReleaseAction = form.actions.Cast<ImportMessageSendingAction>().Single(a => a.IsACECargoRelease);
				cargoReleaseAction.US_SendMessage = true;
				form.actions.RunPreSaveValidation();
				AssertNoErrors(cargoReleaseAction.US_SendMessageInfo);
			}
		}

		public void TestReplacementMessageOnPermitWithOrders()
		{
			// - create FTZ declaration
			Declaration.JE_OH_Importer = WarehouseImporter.PK;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			Declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			Declaration.US_EnableCRL = true;
			Declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Declaration.US_PresentationDate = new ZDateTime(2017, 10, 14);
			Declaration.US_EntryFilerCode = "SV9";
			Declaration.ImportEntryNumber = "40000007";
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			// this line creates permit
			Declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 14);
			Factory.Save();
			// - check that permit exists and not closed
			List<CusPermitHeader> permits = Declaration.FindRelatedPermits();
			AssertEquals(1, permits.Count);
			CusPermitHeader permit = permits[0];
			AssertEquals(false, permit.CPH_IsClosed);
			// - check that 'Update / Replace' message is not allowed 
			using (ImportMessageSendingActionForm form = CreateNewForm(ImportMessageSendingMessageType.Replacement))
			{
				form.Show();
				var cargoReleaseAction = form.actions.Cast<ImportMessageSendingAction>().Single(a => a.IsACECargoRelease);
				cargoReleaseAction.US_SendMessage = true;
				form.actions.RunPreSaveValidation();
				AssertHasError(cargoReleaseAction.US_SendMessageInfo, "Weekly Estimates cannot be updated or replaced. You must file a supplemental weekly estimate instead.");
			}
		}

		public void TestInitialiseIsCancelled()
		{
			Entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(ImportMessageSendingActionCollection))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				AssertEquals("Coll.IsCancelled", true, ImportMessageSendingActionCollection.IsCancelled);
			}
		}

		public void TestClickLineCountColumn()
		{
			var reasonQuery = new ZQuery();
			reasonQuery.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			int existingReasonCount = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), reasonQuery);
			var explanationQuery = new ZQuery();
			explanationQuery.AddToFilter(CusAddInfoSchema.B7_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			int existingExplanationCount = Factory.GetDatabaseCount(typeof(PSCExplanationCusAddInfo), explanationQuery);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_PSC = true;
			var actions = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			var entry1 = Declaration.ActiveEntryHeaders.AddNew();
			var entry2 = Declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine = entry2.MergedLines.AddNew();
			var action1 = new EntryHeaderMessageSendingAction(entry1, ImportMessageStatusList.MessageType.EntrySummary, actions);
			var action2 = new EntryHeaderMessageSendingAction(entry2, ImportMessageStatusList.MessageType.EntrySummary, actions);
			action1.US_SendMessage = true;
			action2.US_SendMessage = true;
			action1.US_PSCExplanation = "Explanations 1";
			action2.US_PSCExplanation = "Explanations 2";
			var pscCode1 = action1.PSCReasonCodes.AddNew();
			pscCode1.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			pscCode1.Reason1 = PSCHeaderReasonList.Codes.H01;
			var pscCode2 = action2.PSCReasonCodes.AddNew();
			pscCode2.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			pscCode2.Reason1 = PSCHeaderReasonList.Codes.H02;
			pscCode2.LineNumber = entryLine.CL_LineNumberFormatted;
			actions.Add(action1);
			actions.Add(action2);
			actions.CopyPSCReasonsAndExplanation();
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(actions))
			{
				form.Show();
				Application.DoEvents();
				form.MessageDetailsTabControl.SelectedIndex = 3;
				Application.DoEvents();
				AssertEquals(new DataGridCell(0, 0), form.PSCReasonCodesGrid.CurrentCell);
				AssertNotEquals(0, form.PSCReasonCodesGrid.Columns.Count);
				//AssertNoExceptionThrown("Should not throw an exception", () => form.PSCReasonCodesGrid.BeginEdit(form.PSCReasonCodesGrid.Columns["LineNumber"].ColumnStyle, 0));
				AssertNoExceptionThrown("Should not throw an exception", () => form.PSCReasonCodesGrid.CurrentCell = new DataGridCell(0, 1));
			}
		}

		public void TestCertifyCBMAPanelVisibility()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			Declaration.US_EntryFilerCode = "XJ5";
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine01 = invoice.InvoiceLines.AddNew();
			var invoiceLine02 = invoice.InvoiceLines.AddNew();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				form.ChangeACEEntrySummaryTabControlsVisibility();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(!form.CertifyCBMAPanel.Visible);
			}

			invoiceLine01.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			Factory.Save();
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				form.ChangeACEEntrySummaryTabControlsVisibility();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(form.CertifyCBMAPanel.Visible);
			}

			invoiceLine01.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			Factory.Save();
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				form.ChangeACEEntrySummaryTabControlsVisibility();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(!form.CertifyCBMAPanel.Visible);
			}

			invoiceLine02.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			Factory.Save();
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				form.ChangeACEEntrySummaryTabControlsVisibility();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(form.CertifyCBMAPanel.Visible);
			}
		}

		public void TestCertifyNonRussianPanelVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "2003900010", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.NonRUCertificationRequired, tariff);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "2003900011", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			Declaration.US_EntryFilerCode = "XJ5";
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine01 = invoice.InvoiceLines.AddNew();
			invoiceLine01.JI_Tariff = "2003900011";
			invoiceLine01.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			invoiceLine01.Validation.ValidateJI_Tariff();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				form.ChangeACEEntrySummaryTabControlsVisibility();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(!form.DISStatementPanel.Visible);
			}

			invoiceLine01.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine01.Validation.ValidateJI_Tariff();
			Factory.Save();
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				form.ChangeACEEntrySummaryTabControlsVisibility();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(!form.DISStatementPanel.Visible);
			}

			var invoiceLine02 = invoice.InvoiceLines.AddNew();
			invoiceLine02.JI_Tariff = "2003900010";
			invoiceLine02.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			invoiceLine02.Validation.ValidateJI_Tariff();
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				form.ChangeACEEntrySummaryTabControlsVisibility();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(!form.DISStatementPanel.Visible);
			}

			invoiceLine02.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine02.Validation.ValidateJI_Tariff();
			using (ImportMessageSendingActionForm form = new ImportMessageSendingActionForm(new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original)))
			{
				form.Show();
				form.MessageDetailsTabControl.SelectedTab = form.ACEEntrySummaryActionTabPage;
				form.ChangeACEEntrySummaryTabControlsVisibility();
				Assert(form.ACEEntrySummaryActionTabPage.TabVisible);
				Assert(form.DISStatementPanel.Visible);
			}
		}

		protected override Form GetFormToBashCore() => new ImportMessageSendingActionForm(ImportMessageSendingActionCollection);

		protected override IEnumerable<Form> FormsToBash
		{
			get
			{
				yield return GetFormToBash();
			}
		}

		ImportMessageSendingActionForm CreateNewForm(ImportMessageSendingMessageType messageType)
		{
			var collection = new ImportMessageSendingActionCollection(Declaration, messageType);
			return new ImportMessageSendingActionForm(collection);
		}

		CusEntryHeader entry;
		CusEntryHeader Entry => entry ?? (entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry);

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					Declaration.US_EntryFilerCode = "XJ5";
					Declaration.US_EnableENS = true;
					var invoice = Declaration.Invoices.AddNew();
					invoice.JobComInvoiceLines.AddNew();
					Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
					Declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
				}

				return declaration;
			}
		}

		OrgHeader warehouseImporter;
		OrgHeader WarehouseImporter
		{
			get
			{
				if (warehouseImporter == null)
				{
					warehouseImporter = Factory.New<OrgHeader>();
					warehouseImporter.OH_Code = "IMP";
					warehouseImporter.MiscServ.OM_IMPartAttrib1Name = "VIN1";
					warehouseImporter.MiscServ.OM_IMPartAttrib1Type = "NON";
					warehouseImporter.CompanyData.OB_IMUsedBondedWhs = true;
					warehouseImporter.OH_IsWarehouseClient = true;
				}

				return warehouseImporter;
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
					warehouse.OH_RL_NKClosestPort = "USLAX";
					warehouse.OH_FullName = "WAREHOUSE ORG";
					warehouse.MainAddress.OA_Address1 = "ADDRESS 1";
					warehouse.MainAddress.LocalControlledPremisesID = "23423";
				}

				return warehouse;
			}
		}

		ImportMessageSendingActionCollection importMessageSendingActionCollection;
		ImportMessageSendingActionCollection ImportMessageSendingActionCollection
			=> importMessageSendingActionCollection ?? (importMessageSendingActionCollection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original));
	}
}
