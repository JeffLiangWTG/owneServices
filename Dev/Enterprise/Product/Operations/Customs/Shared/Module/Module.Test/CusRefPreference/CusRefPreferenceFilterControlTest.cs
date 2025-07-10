using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.Module.Testing
{
	sealed class CusRefPreferenceFilterControlTest : ZFilterStripControlTest
	{
		public void TestReferenceNoTextBoxLocation()
		{
			using (var control = new ReferenceFilterControl())
			{
				AssertEquals("This lines the control up with the other filter strips", control.RefNoTextBox.Location, ControlDpiScalingHelper.NewScaledPoint(308, 1, true));
			}
		}
	}
}
