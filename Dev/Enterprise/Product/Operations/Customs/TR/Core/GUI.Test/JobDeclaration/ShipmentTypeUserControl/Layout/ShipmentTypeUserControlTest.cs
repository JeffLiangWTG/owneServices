using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class ShipmentTypeUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestEntrySubStyleDropEdit()
		{
			AssertType<ZDropEdit>(control.EntrySubStyleDropEdit);
		}

		public void TestEntryDateForDutyDateEdit()
		{
			AssertType<ZDateEdit>(control.EntryDateForDutyDateEdit);
		}

		public void TestBankCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.BankCodeFindBox);
		}

		public void TestDutyPaymentTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.DutyPaymentTypeDropEdit);
		}

		public void TestInspectionClerkTextBox()
		{
			AssertType<ZTextBox>(control.InspectionClerkTextBox);
		}

		public void TestGoodsAtCustomsAreaCheckBox()
		{
			AssertType<ZCheckBox>(control.GoodsAtCustomsAreaCheckBox);
		}

		public void TestOverTimePaymentCompletedCheckBox()
		{
			AssertType<ZCheckBox>(control.OverTimePaymentCompletedCheckBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentTypeUserControl();
		}
		ShipmentTypeUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
