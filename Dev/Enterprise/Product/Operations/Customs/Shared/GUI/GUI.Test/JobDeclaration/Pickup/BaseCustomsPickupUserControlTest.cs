using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseCustomsPickupUserControlTest : TestCase
	{
		public void TestDtbBookingTabPlugIn()
		{
			using (var form = new ZForm())
			using (var control = new BaseCustomsPickupUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertNotNull(control.FindSingle<ZTabControl>("PickTabControl").PlugIns.GetPlugIn(ControllerIDs.DtbBookingTabPlugIn));
			}
		}

		public void TestPickupUserControl()
		{
			using (var form = new ZForm())
			using (var control = new BaseCustomsPickupUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(typeof(PickupUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("PickupUserControl").UserControlType);
			}
		}
	}
}
