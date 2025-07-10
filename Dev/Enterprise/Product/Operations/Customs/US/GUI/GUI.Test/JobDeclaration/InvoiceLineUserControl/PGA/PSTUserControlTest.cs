using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class PSTUserControlTest : TestCaseWithFactory
	{
		public void TestNoExceptionOnView()
		{
			using (var control = new PSTUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => control.ViewEditButton.PerformClick());
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(PSTUserControl.NotificationMessage));
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var pesticide = invoiceLine.PSTLines.AddNew();
				control.SetDataBinding(pesticide, "");
				control.PSTGrid.DataSource = invoiceLine.PSTLines;
				control.ViewEditButton.PerformClick();
				AssertEquals(typeof(PSTEditForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}
	}

	sealed class PSTGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<PSTUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.PSTLines;

		protected override ZGrid GetGrid(PSTUserControl control) => control.PSTGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((PesticideCollection)collection).AddNew();
	}
}
