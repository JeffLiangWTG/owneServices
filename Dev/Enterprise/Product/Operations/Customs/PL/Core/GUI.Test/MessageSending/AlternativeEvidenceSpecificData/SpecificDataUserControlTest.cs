using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class SpecificDataUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using var control = new SpecificDataUserControl();
		AssertEquals(typeof(BaseMessageSendingObjectParent), control.BindingSource.DataSourceType);
	}

	public void TestDynamicSpecificDataPanel()
	{
		using var control = new SpecificDataUserControl();
		{
			control.Show();
			var dynamicSpecificDataPanel = control.FindSingle<DynamicLayoutPanel>("DynamicSpecificDataPanel");

			CombineAssertions(() =>
			{
				AssertNotNull("DynamicSpecificDataPanel", dynamicSpecificDataPanel);
				AssertEquals("Should be visible", true, dynamicSpecificDataPanel.Visible);
			});
		}
	}

	public void TestGetPropertyDescriptors() => CombineAssertions(() =>
	{
		var expectedProperty = SpecificDataUserControl.GetPropertyDescriptors();
		AssertEquals("only one property", 1, expectedProperty.Length);

		var descriptors = expectedProperty.Select(x => x.Name);
		AssertCollectionContains("[IsVisibleForBinding] should be in SpecificDataUserControl's property descriptors", "IsVisibleForBinding", descriptors);
	});
}
