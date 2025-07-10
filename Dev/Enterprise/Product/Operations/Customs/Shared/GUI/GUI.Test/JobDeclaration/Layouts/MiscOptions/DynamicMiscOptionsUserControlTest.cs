using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class DynamicMiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestMiscOptionsLayout()
		{
			using (var control = new DynamicMiscOptionsUserControl())
			{
				control.SetMiscOptionsLayout(new CommonMiscOptionsLayouts());
				var dynamicPanel = control.DynamicMiscOptionsPanel;
				AssertNoExceptionThrown(() => { dynamicPanel.PerformLayout(); });
			}
		}
	}
}
