using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class OR1UserControlTest : TestCaseWithFactory
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
				var or1UserControl = userControl.amsUserControl.FindSingle<OR1UserControl>("or1");
				AssertEquals("OR1UserControl.Visible", false, or1UserControl.Visible);
				amsLine.US_Program = AMSProgramList.Codes.OR1;
				AssertEquals("OR1UserControl.Visible", true, or1UserControl.Visible);
			}
		}

		public void TestUS_ProductLabelColumn()
		{
			using (var control = new OR1UserControl())
			{
				var grid = (ZGrid)control.Controls[0].Controls.Find("OR1Grid", true)[0];
				var column = grid.GetColumnStyle("US_ProductLabel");
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), column.GetType());
				AssertEquals(107, column.Width);
			}
		}
	}
}
