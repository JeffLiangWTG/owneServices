using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.GUI.Testing;

sealed class ValidationToolTabPageTest : TestCaseWithFactory
{
	public void TestValidationToolTabPageUserControl()
	{
		using var tab = new ValidationToolTabPage();
		AssertEquals(DockStyle.Fill, tab.ValidationToolTabPageUserControl.Dock);
		AssertEquals(true, tab.ValidationToolTabPageUserControl.CaptionRenderingEnabled);
	}
}
