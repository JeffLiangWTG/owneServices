using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class PickupUserControlTest : TestCase
	{
		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new PickupUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("PickupDetailsUserControl", typeof(PickupDetailsUserControl), control.FindSingleOrDefault<ZDynamicControlCreationUserControl>("PickupDetailsUserControl").UserControlType);
					AssertEquals("PickupPartiesUserControl", typeof(PickupPartiesUserControl), control.FindSingleOrDefault<ZDynamicControlCreationUserControl>("PickupPartiesUserControl").UserControlType);
				});
			}
		}
	}
}
