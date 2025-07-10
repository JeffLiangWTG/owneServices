using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class ProfileNotesUserControl : ZUserControl
	{
		public ProfileNotesUserControl()
		{
			InitializeComponent();
		}

		ProfileNotesWinModel ProfileNotesWinModel { get; set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is ProfileNotesWinModel profileNotesWinModel)
			{
				ProfileNotesWinModel = profileNotesWinModel;

				base.SetDataBinding(ProfileNotesWinModel, dataMember);
				var tableExpanderHelper = new TableExpanderHelper(ProfileNotesTableLayoutPanel, ProfileNotesExpander, ProfileNotesHeaderPanel, null, collapsed => { profileNotesWinModel.IsExpanderExpanded = !collapsed; });

				tableExpanderHelper.InitBehaviour(!ProfileNotesWinModel.IsExpanderExpanded, ProfileNotesWinModel.IsExpanderEnabled);
#if !WINZOR
				ProfileNoteContentRichTextBox.Rtf = ProfileNotesWinModel.Content;
#else
				ProfileNoteContentRichTextBox.Html = ProfileNotesWinModel.Content;
#endif
				OpenProfileLinkLabel.Text = ProfileNotesWinModel.Link;
			}
		}

		protected override void Dispose(bool disposing)
		{
			ProfileNotesContentPanel.Dispose();
			base.Dispose(disposing);
		}

		protected void OpenProfileLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ProfileNotesWinModel?.OpenProfileNotesInNewWindow();
		}

		public class DpsRichTextBox : ZRichTextBox
		{
			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);

				RichEdit.BackColor = System.Drawing.Color.White;
				RichEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
				RichEdit.BackColorChanged += RichEdit_BackColorChanged;
			}

			void RichEdit_BackColorChanged(object sender, EventArgs e)
			{
				if (RichEdit.BackColor != System.Drawing.Color.White)
				{
					RichEdit.BackColor = System.Drawing.Color.White;
				}
			}
		}
	}
}
