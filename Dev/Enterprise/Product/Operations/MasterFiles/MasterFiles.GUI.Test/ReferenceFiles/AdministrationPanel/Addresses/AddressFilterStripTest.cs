using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var addressFilterStrip = new AddressFilterStripForTest())
			{
				var moduleFilter1 = new AddressSourceAndJobNumberModuleFilter("TEST");
				var controls = addressFilterStrip.GetCurrentFilterControlsForTest(moduleFilter1);
				AssertEquals(1, controls.Length);
				Assert(controls[0] is AddressSourceAndJobNumberFilterControl);
				controls[0].Dispose();
			}
		}
	}
}
