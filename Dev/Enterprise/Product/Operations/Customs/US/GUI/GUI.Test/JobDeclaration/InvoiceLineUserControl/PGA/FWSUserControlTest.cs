using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<FWSUserControl>))]
	sealed class FWSUserControlTest : ZPGAFormBasherAbstractTest<FWSUserControl>
	{
		public void TestNoExceptionOnView()
		{
			using (var control = new FWSUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => control.ViewEditButton.PerformClick());
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(FWSUserControl.NotificationMessage));
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var fws = invoiceLine.FWSHeaders.AddNew();
				control.SetDataBinding(fws, "");
				control.FWSHeaderGrid.DataSource = invoiceLine.FWSHeaders;
				control.ViewEditButton.PerformClick();
				AssertEquals(typeof(FWSEditForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

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
				AssertEquals("userControl.FWSTabPage.TabVisible", false, userControl.FWSTabPage.TabVisible);
				AssertNull(userControl.fwsUserControl);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.FWSTabPage.TabVisible", true, userControl.FWSTabPage.TabVisible);
				AssertNull(userControl.fwsUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.FWSTabPage;
				AssertEquals("userControl.FWSTabPage.TabVisible", true, userControl.FWSTabPage.TabVisible);
				AssertNotNull(userControl.fwsUserControl);
			}
		}

		protected override BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fwsAVSHeader = invoiceLine.FWSHeaders.AddNew();
			return fwsAVSHeader;
		}

		protected override string BindMember => "FilteredInvoiceLines.FWSHeaders";
	}

	sealed class FWSGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<FWSUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.FWSHeaders;

		protected override ZGrid GetGrid(FWSUserControl control) => control.FWSHeaderGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((FWSHeaderCollection)collection).AddNew();

		protected override bool UpdateFDA => true;

		protected override ZString UpdateFDACode => "FWS";
	}
}
