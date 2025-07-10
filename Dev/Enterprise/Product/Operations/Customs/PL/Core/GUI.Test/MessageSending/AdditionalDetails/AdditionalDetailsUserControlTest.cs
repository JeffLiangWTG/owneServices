using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class AdditionalDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new AdditionalDetailsUserControl())
		{
			AssertEquals(typeof(BaseMessageSendingObject), control.BindingSource.DataSourceType);
		}
	}

	public void TestDynamicAdditionalDetailsPanel()
	{
		using (var control = new AdditionalDetailsUserControl())
		{
			control.Show();
			var dynamicAdditionalDetailsPanel = control.FindSingle<DynamicLayoutPanel>("DynamicAdditionalDetailsPanel");

			CombineAssertions(() =>
			{
				AssertNotNull("DynamicAdditionalDetailsPanel", dynamicAdditionalDetailsPanel);
				AssertEquals("Should be visible", true, dynamicAdditionalDetailsPanel.Visible);
			});
		}
	}
}
