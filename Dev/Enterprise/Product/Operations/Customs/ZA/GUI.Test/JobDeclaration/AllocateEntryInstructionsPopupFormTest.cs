using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(AllocateEntryInstructionsPopupForm))]
	class AllocateEntryInstructionsPopupFormTest : ZFormBasherTest
	{
		public void TestAllocateEntryInstructionsConfirmation()
		{
			using (var dialog = new AllocateEntryInstructionsPopupForm(true))
			{
				dialog.Show();
				CombineAssertions("These controls should be visible when there are existing links to entry instructions", () =>
				{
					AssertEquals("CaptionLabel", true, dialog.AlreadyLinkedConfirmationCaptionLabel.Visible);
					AssertEquals("OverwriteRadioButton", true, dialog.OverwriteRadioButton.Visible);
					AssertEquals("IgnoreRadioButton", true, dialog.IgnoreRadioButton.Visible);
				});
				dialog.OverwriteRadioButton.PerformClick();
				AssertEquals(true, dialog.AllowOverwrite);
				dialog.IgnoreRadioButton.PerformClick();
				AssertEquals(false, dialog.AllowOverwrite);
			}

			using (var dialog = new AllocateEntryInstructionsPopupForm(false))
			{
				dialog.Show();
				CombineAssertions("These controls should not be visible when there are no existing links to entry instructions", () =>
				{
					AssertEquals("CaptionLabel", false, dialog.AlreadyLinkedConfirmationCaptionLabel.Visible);
					AssertEquals("OverwriteRadioButton", false, dialog.OverwriteRadioButton.Visible);
					AssertEquals("IgnoreRadioButton", false, dialog.IgnoreRadioButton.Visible);
				});
				AssertEquals("OK Button should always be enabled when there are no options to choose!", true, dialog.OKButton.Enabled);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new AllocateEntryInstructionsPopupForm(true);
		}
	}
}
