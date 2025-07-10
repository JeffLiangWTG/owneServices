using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ODSAndTSCAControlTest : TestCaseWithFactory
	{
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
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Deleted;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals(true, userControl.ODSTabPage.TabVisible);
				AssertNull(userControl.odsAndTSCAControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.ODSTabPage;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
				userControl.odsAndTSCAControl.UpdateButton.PerformClick();
				AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, invoiceLine.US_TSCATrackingStatus);
				AssertEquals(PGATrackingStatusList.Codes.Deleted, invoiceLine.US_ODSTrackingStatus);
				AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.UpdateStatusChangeWarning));
				declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
				invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.odsAndTSCAControl.UpdateButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.PGAReleasedUpdateStatusChangeWarning));
				invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Deleted;
				invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Added;
				declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.odsAndTSCAControl.UpdateButton.PerformClick();
				AssertEquals(PGATrackingStatusList.Codes.Deleted, invoiceLine.US_TSCATrackingStatus);
				AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, invoiceLine.US_ODSTrackingStatus);
				AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
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
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals(true, userControl.ODSTabPage.TabVisible);
				AssertNull(userControl.odsAndTSCAControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.ODSTabPage;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
				userControl.odsAndTSCAControl.DeleteButton.PerformClick();
				AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, invoiceLine.US_TSCATrackingStatus);
				AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, invoiceLine.US_ODSTrackingStatus);
				AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.DeleteStatusChangeWarning));
				invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
				declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.odsAndTSCAControl.DeleteButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.PGAReleasedDeleteStatusChangeWarning));
				invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
				invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Added;
				declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
				declaration.US_PGAReplaceUpdateNeeded = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.odsAndTSCAControl.DeleteButton.PerformClick();
				AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, invoiceLine.US_TSCATrackingStatus);
				AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, invoiceLine.US_ODSTrackingStatus);
				AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
			}
		}
	}
}
