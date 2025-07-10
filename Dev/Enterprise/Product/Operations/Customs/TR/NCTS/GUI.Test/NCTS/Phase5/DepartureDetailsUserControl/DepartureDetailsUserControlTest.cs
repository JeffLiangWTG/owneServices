using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class DepartureDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestStampDutyStatusDropEdit()
		{
			var stampDutyStatusDropEdit = control.StampDutyStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", stampDutyStatusDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.StampDutyStatus), stampDutyStatusDropEdit.GetBindingMember());
			});
		}

		public void TestStampDutyCalcEdit()
		{
			var stampDutyCalcEdit = control.StampDutyCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", stampDutyCalcEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.StampDuty), stampDutyCalcEdit.GetBindingMember());
			});
		}

		public void TestRegistrationDateEdit()
		{
			var registrationDateEdit = control.RegistrationDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", registrationDateEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.RegistrationDate), registrationDateEdit.GetBindingMember());
			});
		}

		public void TestGoodsShippingLocationAndGIKUserControl()
		{
			AssertType<GoodsShippingLocationAndGIKUserControl>("Type", control.GoodsShippingLocationAndGIKUserControl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new DepartureDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		DepartureDetailsUserControl control;
	}
}
