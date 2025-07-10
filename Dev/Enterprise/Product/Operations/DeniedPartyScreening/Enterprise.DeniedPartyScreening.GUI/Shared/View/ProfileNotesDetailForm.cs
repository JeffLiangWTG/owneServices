using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class ProfileNotesDetailForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes 1 string, this constructor is just for the designer", true)]
		public ProfileNotesDetailForm()
		{
		}

		public ProfileNotesDetailForm(string content)
		{
#if !WINZOR
			this.ProfileNoteContentRichTextBox.Rtf = content;
#else
			this.ProfileNoteContentRichTextBox.Html = content;
#endif
		}

		public override string FormCaption
		{
			get { return Res.GetString("fed59ab4-5ae4-43eb-8836-70c1c5c3cc61", "Profile Notes Detail"); }
		}
	}
}
