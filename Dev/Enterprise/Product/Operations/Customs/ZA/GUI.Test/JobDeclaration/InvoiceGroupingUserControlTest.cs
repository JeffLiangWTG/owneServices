using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class InvoiceGroupingUserControlTest : TestCaseWithFactory
	{
		public void TestRemoveGSTApplicableFromGroupCharge()
		{
			using (InvoiceGroupingUserControl testUserControl = new InvoiceGroupingUserControl())
			{
				testUserControl.SetDataBinding(testDec, "");
				ZGridColumn theColumn = testUserControl.GroupChargeGridForUnitTest.Columns[InvoiceCharge.Schema.J7_IsGSTApplicable];
				AssertNull("GST applicable is not relevant for ZA", theColumn);
			}
		}

		JobDeclaration testDec;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
		}
	}
}
