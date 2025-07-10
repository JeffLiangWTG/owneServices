using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	sealed class GroupInvoiceUserControlTest : TestCaseWithFactory
	{
		public void TestChangeGSTApplicableText()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceGroupingTabPage;

				GroupInvoiceUserControl userControl = (GroupInvoiceUserControl)form.CustomsBrokerageUserControl.InvoiceGroupUserControl;
				AssertEquals("Charges grid GST Applicable column text", USCustomsSupplierHeaderUserControl.IsCIFComponent, userControl.GroupChargeGrid.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
			}
		}

		public void TestAIIFields()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceGroupingTabPage;

				GroupInvoiceUserControl userControl = (GroupInvoiceUserControl)form.CustomsBrokerageUserControl.InvoiceGroupUserControl;
				AssertNotNull("Charges grid Exchange Rate text", userControl.GroupChargeGrid.GetColumnStyle(GroupInvoiceCharge.Schema.J7_ExchangeRate));
				AssertNotNull("Charges grid Fixed Rate text", userControl.GroupChargeGrid.GetColumnStyle(GroupInvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable));
				AssertNotNull("Charges grid Charge Description text", userControl.GroupChargeGrid.GetColumnStyle(GroupInvoiceCharge.Schema.J7_ChargeDescription));
			}
		}
	}
}
