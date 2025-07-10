using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<NHTSAUserControl>))]
	sealed class NHTSAUserControlTest : ZPGAFormBasherAbstractTest<NHTSAUserControl>
	{
		public void TestNoExceptionOnView()
		{
			using (var control = new NHTSAUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => control.ViewEditButton.PerformClick());
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(NHTSAUserControl.NotificationMessage));
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var nhtsa = invoiceLine.NHTSALines.AddNew();
				control.SetDataBinding(nhtsa, "");
				control.NHTSAHeaderGrid.DataSource = invoiceLine.NHTSALines;
				control.ViewEditButton.PerformClick();
				AssertEquals(typeof(NHTSAEditForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		protected override string BindMember => "FilteredInvoiceLines.NHTSALines";

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			var result = invoiceLine.NHTSALines.AddNew();
			result.NHTSADetails.AddNew();
			return result;
		}
	}

	sealed class NHTSAGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<NHTSAUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.NHTSALines;

		protected override ZGrid GetGrid(NHTSAUserControl control) => control.NHTSAHeaderGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((NHTSAHeaderCollection)collection).AddNew();
	}
}
