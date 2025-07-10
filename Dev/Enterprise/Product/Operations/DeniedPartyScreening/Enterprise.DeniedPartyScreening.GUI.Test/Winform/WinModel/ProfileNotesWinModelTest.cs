using Enterprise.DeniedPartyScreening.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class ProfileNotesWinModelTest : TestCase
	{
		public void TestConstructor()
		{
			var model = new ProfileNotesWinModel(new ProfileNotesModel("Content"));
			CombineAssertions(() =>
			{
				AssertEquals("Content", model.Content);
				AssertEquals("Open in New Window", model.Link);
				AssertEquals("Profile Notes", model.ExpanderTitle);
				AssertEquals(string.Empty, model.ExpanderDescription);
				AssertEquals(true, model.IsExpanderEnabled);
			});

			model = new ProfileNotesWinModel(new ProfileNotesModel(" "));
			CombineAssertions(() =>
			{
				AssertEquals(false, model.IsExpanderEnabled);
			});
		}

		public void TestLinkClickCommand()
		{
			var model = new ProfileNotesWinModel(new ProfileNotesModel("Profile Note Content"));
			model.OpenProfileNotesInNewWindow();

			using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
			{
				AssertNotNull(lastShownForm);
				AssertEquals(lastShownForm.GetType(), typeof(ProfileNotesDetailForm));
#if !WINZOR
				AssertContains("RichTextBox Rtf is correct", "Profile Note Content", ((ProfileNotesDetailForm)lastShownForm).ProfileNoteContentRichTextBox.Rtf);
#else
				AssertContains("RichTextBox HTML is correct", "Profile Note Content", ((ProfileNotesDetailForm)lastShownForm).ProfileNoteContentRichTextBox.Html);
#endif
			}
		}
	}
}
