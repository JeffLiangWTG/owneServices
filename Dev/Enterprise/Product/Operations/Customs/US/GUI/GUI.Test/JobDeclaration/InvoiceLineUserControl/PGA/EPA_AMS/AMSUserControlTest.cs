using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<AMSUserControl>))]
	sealed class AMSUserControlTest : ZPGAFormBasherAbstractTest<AMSUserControl>
	{
		public void TestViewEditButtonVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.AMSTabPage;
				var amsLine = invoiceLine.AMSLines.AddNew();
				amsLine.US_Program = AMSProgramList.Codes.EG1;
				var viewEditButton = invoiceLineUserControl.amsUserControl.FindSingle<ZButton>("ViewEditButton");
				AssertEquals("viewEditButton.Visible", false, viewEditButton.Visible);
				amsLine.US_Program = AMSProgramList.Codes.OR1;
				AssertEquals("viewEditButton.Visible", true, viewEditButton.Visible);
				amsLine.US_Program = AMSProgramList.Codes.OR2;
				AssertEquals("viewEditButton.Visible", false, viewEditButton.Visible);
			}
		}

		public void TestAMSGridHasIntendedUsedCodeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.AMSTabPage;
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.AMSTabPage.TabVisible", true, invoiceLineUserControl.AMSTabPage.TabVisible);
				AssertNotNull(invoiceLineUserControl.amsUserControl);
				var amsLine = invoiceLine.AMSLines.AddNew();
				amsLine.US_Program = AMSProgramList.Codes.MO2;
				var amsGrid = invoiceLineUserControl.amsUserControl.AMSGrid;
				var column = amsGrid.GetColumnStyle(AMS.Schema.US_IntendedUseDescription);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), column.GetType());
			}
		}

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._025000;
			amsLine.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			return amsLine;
		}

		protected override string BindMember => "FilteredInvoiceLines.AMSLines";
	}

	sealed class AMSGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<AMSUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.AMSLines;

		protected override ZGrid GetGrid(AMSUserControl control) => control.AMSGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((AMSCollection)collection).AddNew();
	}
}
