using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class CusClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using var userControl = new CusClassificationUserControl();
			CombineAssertions(() =>
			{
				var groupBox = userControl.AssertContainsControl<ZGroupBox>("BaseClassificationGroupBox");
				groupBox.AssertContainsControl<ZDateEdit>("LastAuditDateEdit");
				groupBox.AssertContainsControl<ZCodeFindBox>("AuditStaffCodeFindBox");
			});
		}
	}
}
