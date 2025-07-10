using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseCustomsDeliveryUserControlTest : TestCase
	{
		public void TestDtbBookingTabPlugIn()
		{
			using (var form = new ZForm())
			using (var control = new BaseCustomsDeliveryUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertNotNull(control.FindSingle<ZTabControl>("DeliveryTabControl").PlugIns.GetPlugIn(ControllerIDs.DtbBookingTabPlugIn));
			}
		}

		public void TestDeliveryUserControl()
		{
			using (var form = new ZForm())
			using (var control = new BaseCustomsDeliveryUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(typeof(DeliveryUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("DeliveryUserControl").UserControlType);
			}
		}
	}
}
