using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;

namespace Enterprise.GPS.Module.Testing
{
	sealed class GPSSupporterPlugInTest : TestCaseWithFactory
	{
		public void TestName()
		{
			using (var pi = new GPSSupporterPlugIn(Factory.New<RefEquipment>()))
			{
				AssertEquals("GPS", pi.Name);
			}
		}

		public void TestGetControl()
		{
			using (var pi = new GPSSupporterPlugIn(Factory.New<RefEquipment>()))
			using (var ctr = pi.UserControl)
			{
				AssertType<GPSSupporterDetailsControl>(ctr);
			}
		}

		public void TestGetTabPage()
		{
			using (var pi = new GPSSupporterPlugIn(Factory.New<RefEquipment>()))
			using (var tp = pi.TabPage)
			{
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(600), tp.MinimumAutoSizedHeight);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(800), tp.MinimumAutoSizedWidth);
			}
		}
	}
}
