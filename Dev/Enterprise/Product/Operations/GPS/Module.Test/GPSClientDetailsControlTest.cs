using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.GPS.Module.Testing
{
	sealed class GPSClientDetailsControlTest : ZTabPageControlTest
	{
		[TestDate(2018, 2, 2, 1, 0, 0)]
		public void TestGPSClientDetailsControlTest_Clear()
		{
			var vehicle = Factory.New<RefEquipment>();
			var gpsSupporter = new GPSSupporter(vehicle);

			using (var form = new ZForm())
			{
				var control = new GPSSupporterDetailsControl();
				form.Controls.Add(control);
				control.SetDataBinding(gpsSupporter, "");
				form.Show();

				gpsSupporter.ActivityFilterProvider.ActivityFilterDateFrom = ZDateTime.BrettsBirthday;
				gpsSupporter.ActivityFilterProvider.ActivityFilterDateTo = ZDateTime.BrettsBirthday;
				AssertEquals("Precondition: ActivityFilterDateFrom", ZDateTime.BrettsBirthday, gpsSupporter.ActivityFilterProvider.ActivityFilterDateFrom);
				AssertEquals("Precondition: ActivityFilterDateTo", ZDateTime.BrettsBirthday, gpsSupporter.ActivityFilterProvider.ActivityFilterDateTo);

				control.ClearButton.PerformClick();
				AssertEquals("Start date correct", ZDateTime.Today, gpsSupporter.ActivityFilterProvider.ActivityFilterDateFrom);
				AssertEquals("End date corrrect", ZDateTime.Today.AddDays(1), gpsSupporter.ActivityFilterProvider.ActivityFilterDateTo);
			}
		}
	}
}
