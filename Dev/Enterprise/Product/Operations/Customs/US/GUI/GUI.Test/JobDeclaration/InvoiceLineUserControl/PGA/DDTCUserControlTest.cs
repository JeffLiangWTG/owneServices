using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class DDTCUserControlTest : TestCaseWithFactory
	{
		public void TestVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull(userControl);
				AssertEquals(true, userControl.Visible);
				AssertNull(userControl.ddtcUserControl);
				AssertEquals(false, userControl.DDTCTabPage.TabVisible);
				invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
				AssertEquals(true, userControl.DDTCTabPage.TabVisible);
				AssertNull(userControl.ddtcUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.DDTCTabPage;
				AssertNotNull(userControl.ddtcUserControl);
			}
		}

		public void TestUpdatePGALine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.Added;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals(true, userControl.DDTCTabPage.TabVisible);
				AssertNull(userControl.ddtcUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.DDTCTabPage;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
				userControl.ddtcUserControl.UpdateButton.PerformClick();
				AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, invoiceLine.US_DDTCTrackingStatus);
				AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.UpdateStatusChangeWarning));
				invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.Added;
				declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.ddtcUserControl.UpdateButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.PGAReleasedUpdateStatusChangeWarning));
			}
		}

		public void TestDeletePGALine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.Added;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals(true, userControl.DDTCTabPage.TabVisible);
				AssertNull(userControl.ddtcUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.DDTCTabPage;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
				userControl.ddtcUserControl.DeleteButton.PerformClick();
				AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, invoiceLine.US_DDTCTrackingStatus);
				AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.DeleteStatusChangeWarning));
				invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.Added;
				declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.ddtcUserControl.DeleteButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.PGAReleasedDeleteStatusChangeWarning));
			}
		}
	}
}
