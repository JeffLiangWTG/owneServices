using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	[TestedType(typeof(ProfileNotesDetailForm))]
	public class ProfileNotesDetailFormTest : ZFormBasherTest
	{
		public void TestProfileNoteContentRichTextBox()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var contentRichTextBox = form.Controls.Find("ProfileNoteContentRichTextBox", true).Single() as ZRichTextBox;
#if !WINZOR
				AssertContains("Content", contentRichTextBox.Rtf);
#endif
				AssertEquals(true, contentRichTextBox.ReadOnly);
				AssertEquals(false, contentRichTextBox.IsToolBarVisible);
			}
		}

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			return new ProfileNotesDetailForm("Content");
		}

		#endregion
	}
}
