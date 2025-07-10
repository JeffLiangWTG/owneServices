using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class CustomsOfficesUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			TestHelper.AssertControlExists(control, "CustomsOfficeFindBox", "JE_CustomsOffice");
			TestHelper.AssertControlExists(control, "DischargeOfficeFindBox", "DischargeOffice");
			TestHelper.AssertControlExists(control, "DischargePlaceTextBox", "DischargePlace");
			TestHelper.AssertControlExists(control, "EntryCustomsFindBox", "EntryOffice");
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CustomsOfficesUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		CustomsOfficesUserControl control;
	}
}
