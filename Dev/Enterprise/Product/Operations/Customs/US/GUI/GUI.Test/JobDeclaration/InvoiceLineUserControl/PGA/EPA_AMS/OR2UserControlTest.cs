using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class OR2UserControlTest : TestCaseWithFactory
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
				AssertEquals("userControl.AMSTabPage.TabVisible", false, userControl.AMSTabPage.TabVisible);
				AssertNull(userControl.amsUserControl);
				invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.AMSTabPage.TabVisible", true, userControl.AMSTabPage.TabVisible);
				AssertNull(userControl.amsUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.AMSTabPage;
				AssertEquals("userControl.AMSTabPage.TabVisible", true, userControl.AMSTabPage.TabVisible);
				AssertNotNull(userControl.amsUserControl);
				var amsLine = invoiceLine.AMSLines.AddNew();
				amsLine.US_Program = AMSProgramList.Codes.MO1;
				var or2UserControl = userControl.amsUserControl.FindSingle<OR2UserControl>("or2");
				AssertEquals("OR2UserControl.Visible", false, or2UserControl.Visible);
				amsLine.US_Program = AMSProgramList.Codes.OR2;
				AssertEquals("OR2UserControl.Visible", true, or2UserControl.Visible);
			}
		}

		public void TestUS_CertTypeColumn()
		{
			using (var control = new OR2UserControl())
			{
				var grid = (ZGrid)control.Controls[0].Controls.Find("CertificatesGrid", true)[0];
				var column = grid.GetColumnStyle("US_CertType");
				AssertEquals(typeof(ZDropEditColumnStyleInfo), column.GetType());
				AssertEquals("LPCO Type", ((ZDropEditColumnStyleInfo)column).CaptionResourceString.Caption);
			}
		}
	}
}
