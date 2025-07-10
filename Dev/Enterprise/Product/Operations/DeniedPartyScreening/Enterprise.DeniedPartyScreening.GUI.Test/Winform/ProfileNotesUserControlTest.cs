using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class ProfileNotesUserControlTest : TestCaseWithFactory
	{
		public void TestProfileNotesContentPanelExpandCollapseWithContent()
		{
			var winModel = new ProfileNotesWinModel(new ProfileNotesModel("Profile Note Content"));

			using (var form = new ZForm())
			using (var userControl = new ProfileNotesUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var expander = userControl.FindSingle<ZLinkLabel>("ProfileNotesExpander");
				var tableLayoutPanel = userControl.FindSingleOrDefault<KTableLayoutPanel>("ProfileNotesTableLayoutPanel");

				AssertNotNull("TabLayout Should display", tableLayoutPanel);
				AssertNotNull("Expander Should display", expander);

				AssertNotNull("HeaderPanel display", userControl.FindSingleOrDefault<ZPanel>("ProfileNotesHeaderPanel"));
				AssertEquals("No ContentPanel display", 0F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("No ContentPanel display", SizeType.Absolute, tableLayoutPanel.RowStyles[1].SizeType);

				expander.PerformClick_ForTest();

				AssertEquals("ContentPanel Expanded", 100F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("ContentPanel Expanded", SizeType.Percent, tableLayoutPanel.RowStyles[1].SizeType);

				expander.PerformClick_ForTest();

				AssertEquals("ContentPanel Collapsed", 0F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("ContentPanel Collapsed", SizeType.Absolute, tableLayoutPanel.RowStyles[1].SizeType);
			}
		}

		public void TestProfileNotesContentPanelExpandCollapseWhenNoContent()
		{
			var winModel = new ProfileNotesWinModel(new ProfileNotesModel(string.Empty));

			using (var form = new ZForm())
			using (var userControl = new ProfileNotesUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var expander = userControl.FindSingle<ZLinkLabel>("ProfileNotesExpander");
				var tableLayoutPanel = userControl.FindSingleOrDefault<KTableLayoutPanel>("ProfileNotesTableLayoutPanel");

				AssertNotNull("TabLayout Should display", tableLayoutPanel);
				AssertNotNull("HeaderPanel Should display", expander);

				AssertNotNull("HeaderPanel display", userControl.FindSingleOrDefault<ZPanel>("ProfileNotesHeaderPanel"));
				AssertEquals("No ContentPanel display", 0F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("No ContentPanel display", SizeType.Absolute, tableLayoutPanel.RowStyles[1].SizeType);

				expander.PerformClick_ForTest();

				AssertEquals("ContentPanel should not expand", 0F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("ContentPanel should not expand", SizeType.Absolute, tableLayoutPanel.RowStyles[1].SizeType);
			}
		}

		public void TestProfileNotesContent()
		{
			var profileNoteContent = "Profile Note Content";
			var winModel = new ProfileNotesWinModel(new ProfileNotesModel(profileNoteContent));

			using (var form = new ZForm())
			using (var userControl = new ProfileNotesUserControlForTest())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				AssertContains(profileNoteContent, userControl.GetProfileNoteContentRichTextBoxContent());
			}
		}

		public void TestProfileNotesLink()
		{
			var winModel = new ProfileNotesWinModel(new ProfileNotesModel("Profile Note Content"));

			using (var form = new ZForm())
			using (var userControl = new ProfileNotesUserControlForTest())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				AssertEquals("Open in New Window", userControl.GetOpenProfileLinkLabelText());
			}
		}

		public void TestProfileNotesOpenNewWindow()
		{
			var winModel = new ProfileNotesWinModel(new ProfileNotesModel("Profile Note Content"));

			using (var form = new ZForm())
			using (var userControl = new ProfileNotesUserControlForTest())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNull(lastShownForm);
				}

				userControl.OpenProfileLinkLabel_LinkClickedForTest();

				using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull(lastShownForm);
					AssertEquals(typeof(ProfileNotesDetailForm), lastShownForm.GetType());
#if !WINZOR
					AssertContains("RichTextBox Rtf is correct", "Profile Note Content", ((ProfileNotesDetailForm)lastShownForm).ProfileNoteContentRichTextBox.Rtf);
#else
					AssertContains("RichTextBox Rtf is correct", "Profile Note Content", ((ProfileNotesDetailForm)lastShownForm).ProfileNoteContentRichTextBox.Html);
#endif
				}
			}
		}

		class ProfileNotesUserControlForTest : ProfileNotesUserControl
		{
			public string GetProfileNoteContentRichTextBoxContent()
			{
#if !WINZOR
				return ProfileNoteContentRichTextBox.Rtf;
#else
				return ProfileNoteContentRichTextBox.Html;
#endif
			}

			public string GetOpenProfileLinkLabelText()
			{
				return OpenProfileLinkLabel.Text;
			}

			public void OpenProfileLinkLabel_LinkClickedForTest()
			{
				OpenProfileLinkLabel_LinkClicked(null, null);
			}
		}
	}
}
