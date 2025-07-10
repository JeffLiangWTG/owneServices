namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class ProfileNotesDetailForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.ProfileNoteContentRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProfileNoteContentRichTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 236, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 24, true);
			// 
			// PopupContentRichTextBox
			// 
			this.ProfileNoteContentRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProfileNoteContentRichTextBox.IsToolBarVisible = false;
			this.ProfileNoteContentRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 14, true);
			this.ProfileNoteContentRichTextBox.MaxLength = 2147483647;
			this.ProfileNoteContentRichTextBox.Name = "ProfileNoteContentRichTextBox";
			this.ProfileNoteContentRichTextBox.ParentZForm = this;
			this.ProfileNoteContentRichTextBox.ReadOnly = true;
			this.ProfileNoteContentRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 209, true);
			this.ProfileNoteContentRichTextBox.TabIndex = 1;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProfileNoteContentRichTextBox, false);
			// 
			// ProfileNotesDetailForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 650, true);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 260, true);
			this.Controls.Add(this.ProfileNoteContentRichTextBox);
			this.Name = "ProfileNotesDetailForm";
			this.Text = "Form1";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ProfileNoteContentRichTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProfileNoteContentRichTextBox.ResumeLayout(true);
			this.ProfileNoteContentRichTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZRichTextBox ProfileNoteContentRichTextBox;
	}
}
