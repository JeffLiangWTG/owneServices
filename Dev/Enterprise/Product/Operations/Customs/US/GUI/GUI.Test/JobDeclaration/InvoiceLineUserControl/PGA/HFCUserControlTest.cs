using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class HFCUserControlTest : TestCaseWithFactory
	{
		public void TestNoExceptionOnView()
		{
			using (var control = new HFCUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => control.ViewEditButton.PerformClick());
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(HFCUserControl.NotificationMessage));
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var pesticide = invoiceLine.USHFCHeaders.AddNew();
				control.SetDataBinding(pesticide, "");
				control.HFCGrid.DataSource = invoiceLine.USHFCHeaders;
				control.ViewEditButton.PerformClick();
				AssertEquals(typeof(HFCForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}
	}

	sealed class HFCGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<HFCUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.USHFCHeaders;

		protected override ZGrid GetGrid(HFCUserControl control) => control.HFCGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((USHFCHeaderCollection)collection).AddNew();
	}
}
