using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class GroupInvoiceUserControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGroupInvoiceUserControl()
		{
			var control = new GroupInvoiceUserControl();
			control.Dispose();
		}

		public void TestChargeColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceGroupingTabPage;
				var grid = ((GroupInvoiceUserControl)form.CustomsBrokerageUserControl.InvoiceGroupUserControl).GroupChargeGrid;
				var dutiableColumn = grid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsDutiable];
				AssertNull("J7_IsDutiable column removed", dutiableColumn);
				var gSTApplicableColumn = grid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable];
				AssertNull("J7_GSTApplicable column removed", gSTApplicableColumn);
			}
		}
	}
}
