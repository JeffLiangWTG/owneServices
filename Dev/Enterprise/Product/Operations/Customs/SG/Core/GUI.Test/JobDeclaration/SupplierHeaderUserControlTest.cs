using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class SupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestChargeColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				form.CustomsBrokerageUserControl.SupplierHeaderUserControl.ChargesTabControl.SelectedTab = form.CustomsBrokerageUserControl.SupplierHeaderUserControl.ApportionedTabPage;
				//apportioned charge grid
				var apportionedGrid = ((SGSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl).ApportionedChargesGrid;
				var dutiableColumn = apportionedGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsDutiable];
				AssertNull("J7_IsDutiable column removed", dutiableColumn);
				var gSTApplicableColumn = apportionedGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable];
				AssertNull("J7_IsGSTApplicable column removed", gSTApplicableColumn);
				//charge grid
				var chargeGrid = ((SGSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl).InvoiceChargesGrid;
				dutiableColumn = chargeGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsDutiable];
				AssertNull("J7_IsDutiable column removed", dutiableColumn);
				gSTApplicableColumn = chargeGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable];
				AssertNull("J7_IsGSTApplicable column removed", gSTApplicableColumn);
				var percentageColumn = chargeGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_Percentage];
				AssertEquals("%", percentageColumn.ColumnStyle.HeaderText);
				//group charge grid
				var groupChargeGrid = ((SGSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl).BaseGroupChargesGrid;
				dutiableColumn = groupChargeGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsDutiable];
				AssertNull("J7_IsDutiable column removed", dutiableColumn);
				gSTApplicableColumn = groupChargeGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable];
				AssertNull("J7_IsGSTApplicable column removed", gSTApplicableColumn);
				percentageColumn = groupChargeGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_Percentage];
				AssertEquals("%", percentageColumn.ColumnStyle.HeaderText);
			}
		}

		public void TestJZ_ValuationDateOverrideDateEdit()
		{
			AssertType<ZDateEdit>(control.JZ_ValuationDateOverrideDateEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new SGSupplierHeaderUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		SGSupplierHeaderUserControl control;
	}
}
