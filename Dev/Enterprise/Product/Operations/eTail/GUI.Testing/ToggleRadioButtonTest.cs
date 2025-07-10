
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	class ToggleRadioButtonTest : TestCase
	{
		public void TestToggleRadioButton_OnClick()
		{
			var btn = new ToggleRadioButton();
			btn.AutoCheck = true;

			Assert("Precondition - ToggleRadioButton should be unchecked", !btn.Checked);

			btn.PerformClick();

			Assert("ToggleRadioButton should be checked.", btn.Checked);

			btn.PerformClick();

			Assert("ToggleRadioButton should be unchecked.", !btn.Checked);
		}
	}
}
