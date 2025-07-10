using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<ACEFDAUserControl>))]
	sealed class ACEFDAUserControlTest : ZPGAFormBasherAbstractTest<ACEFDAUserControl>
	{
		public void TestNoExceptionOnView()
		{
			using (var control = new ACEFDAUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => control.ViewEditButton.PerformClick());
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(ACEFDAUserControl.NotificationMessage));
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var fda = invoiceLine.ACE_FDALines.AddNew();
				control.SetDataBinding(fda, "");
				control.FDAGrid.DataSource = invoiceLine.ACE_FDALines;
				control.ViewEditButton.PerformClick();
				AssertEquals(typeof(ACEFDAPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestFDAGridColumnNames()
		{
			using (var control = new ACEFDAUserControl())
			{
				var columnStyle = control.FDAGrid.GetColumnStyle("US_ProdCountry");
				AssertEquals("US_ProdCountry caption", "Prod. Ctry/Rgn.", columnStyle.CaptionResourceString.Caption);
				columnStyle = control.FDAGrid.GetColumnStyle("US_RefusedCountry");
				AssertEquals("US_RefusedCountry caption", "Refused Ctry/Rgn.", columnStyle.CaptionResourceString.Caption);
				columnStyle = control.FDAGrid.GetColumnStyle("US_SourceCountry");
				AssertEquals("US_SourceCountry caption", "Source Ctry/Rgn.", columnStyle.CaptionResourceString.Caption);
			}
		}

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			fdaLine.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BLO;
			return fdaLine;
		}

		protected override string BindMember
		{
			get
			{
				return "FilteredInvoiceLines.ACE_FDALines";
			}
		}
	}

	sealed class ACEFDAGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<ACEFDAUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.ACE_FDALines;

		protected override ZGrid GetGrid(ACEFDAUserControl control) => control.FDAGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((ACEFDACollection)collection).AddNew();

		protected override bool UpdateFDA => true;

		protected override ZString UpdateFDACode => "FDA";
	}
}
