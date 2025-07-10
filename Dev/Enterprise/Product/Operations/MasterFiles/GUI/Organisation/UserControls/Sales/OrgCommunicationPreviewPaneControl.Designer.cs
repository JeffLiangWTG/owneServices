namespace Enterprise.MasterFiles.GUI
{
	partial class OrgCommunicationPreviewPaneControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.CommunicationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CommunicationDetailsControl = new Enterprise.MasterFiles.GUI.CommunicationDetailsUserControl();
			this.NotesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CallNoteRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.FollowUpNotesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FollowupNoteRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.DiscussedTradeLanesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TradeProfileLookupCheckedListBox = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CustomFieldsControl = new Enterprise.MasterFiles.GUI.CustomFieldsCommunicationUserControl();
			this.RelatedActivityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CommunicationRelatedActivityControl = new Enterprise.MasterFiles.GUI.OrgCommunicationPreviewRelatedActivityControl();
			this.ToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommunicationTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.FollowUpNotesTabPage.SuspendLayout();
			this.DiscussedTradeLanesTabPage.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.RelatedActivityTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CommunicationTabControl
			// 
			this.CommunicationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CommunicationTabControl.Controls.Add(this.DetailsTabPage);
			this.CommunicationTabControl.Controls.Add(this.NotesTabPage);
			this.CommunicationTabControl.Controls.Add(this.FollowUpNotesTabPage);
			this.CommunicationTabControl.Controls.Add(this.DiscussedTradeLanesTabPage);
			this.CommunicationTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.CommunicationTabControl.Controls.Add(this.RelatedActivityTabPage);
			this.CommunicationTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CommunicationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 330, true);
			this.CommunicationTabControl.Name = "CommunicationTabControl";
			this.CommunicationTabControl.SelectedIndex = 0;
			this.CommunicationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 320, true);
			this.CommunicationTabControl.TabIndex = 2;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c6b915b5-e8a3-4407-93d8-a979f5285455", "Details");
			this.DetailsTabPage.Controls.Add(this.CommunicationDetailsControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// CommunicationDetailsControl
			// 
			this.CommunicationDetailsControl.AllowDrop = true;
			this.CommunicationDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommunicationDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CommunicationDetailsControl.Name = "CommunicationDetailsControl";
			this.CommunicationDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(997, 287, true);
			this.CommunicationDetailsControl.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4B2EC626-DC95-4F20-8608-7B9EA8C6852E", "Internal Notes");
			this.NotesTabPage.Controls.Add(this.CallNoteRichTextBox);
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.NotesTabPage.TabIndex = 1;
			this.NotesTabPage.UseVisualStyleBackColor = true;
			// 
			// CallNoteRichTextBox
			// 
			this.CallNoteRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CallNoteRichTextBox, false);
			this.CallNoteRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CallNoteRichTextBox.MaxLength = 10000000;
			this.CallNoteRichTextBox.Name = "CallNoteRichTextBox";
			this.CallNoteRichTextBox.ReadOnly = true;
			this.CallNoteRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.CallNoteRichTextBox.TabIndex = 0;
			// 
			// FollowUpNotesTabPage
			// 
			this.FollowUpNotesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3844226e-a3f3-4f7a-b522-eb3997b488ef", "Follow Up Notes");
			this.FollowUpNotesTabPage.Controls.Add(this.FollowupNoteRichTextBox);
			this.FollowUpNotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FollowUpNotesTabPage.Name = "FollowUpNotesTabPage";
			this.FollowUpNotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.FollowUpNotesTabPage.TabIndex = 2;
			this.FollowUpNotesTabPage.UseVisualStyleBackColor = true;
			// 
			// FollowupNoteRichTextBox
			// 
			this.FollowupNoteRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FollowupNoteRichTextBox, false);
			this.FollowupNoteRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FollowupNoteRichTextBox.MaxLength = 10000000;
			this.FollowupNoteRichTextBox.Name = "FollowupNoteRichTextBox";
			this.FollowupNoteRichTextBox.ReadOnly = true;
			this.FollowupNoteRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.FollowupNoteRichTextBox.TabIndex = 0;
			// 
			// DiscussedTradeLanesTabPage
			// 
			this.DiscussedTradeLanesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0a4329fc-30dc-45eb-87d1-edf33afb5965", "Related Value Analysis");
			this.DiscussedTradeLanesTabPage.Controls.Add(this.TradeProfileLookupCheckedListBox);
			this.DiscussedTradeLanesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DiscussedTradeLanesTabPage.Name = "DiscussedTradeLanesTabPage";
			this.DiscussedTradeLanesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.DiscussedTradeLanesTabPage.TabIndex = 3;
			this.DiscussedTradeLanesTabPage.UseVisualStyleBackColor = true;
			// 
			// TradeProfileLookupCheckedListBox
			// 
			this.TradeProfileLookupCheckedListBox.BindingItems = null;
			this.TradeProfileLookupCheckedListBox.CheckOnClick = true;
			this.TradeProfileLookupCheckedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TradeProfileLookupCheckedListBox.Enabled = false;
			this.TradeProfileLookupCheckedListBox.FormattingEnabled = true;
			this.TradeProfileLookupCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TradeProfileLookupCheckedListBox.Name = "TradeProfileLookupCheckedListBox";
			this.TradeProfileLookupCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.TradeProfileLookupCheckedListBox.TabIndex = 0;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2be41bf7-8d51-4266-96b8-54af3954caeb", "Custom Fields");
			this.CustomFieldsTabPage.Controls.Add(this.CustomFieldsControl);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 7, 3, 3, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.CustomFieldsTabPage.TabIndex = 4;
			// 
			// CustomFieldsControl
			// 
			this.CustomFieldsControl.AllowDrop = true;
			this.CustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.CustomFieldsControl.Name = "CustomFieldsControl";
			this.CustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(997, 283, true);
			this.CustomFieldsControl.TabIndex = 0;
			// 
			// RelatedActivityTabPage
			// 
			this.RelatedActivityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9af1f12d-6091-434d-8164-9515f4de3a69", "Related Activity");
			this.RelatedActivityTabPage.Controls.Add(this.CommunicationRelatedActivityControl);
			this.RelatedActivityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedActivityTabPage.Name = "RelatedActivityTabPage";
			this.RelatedActivityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.RelatedActivityTabPage.TabIndex = 5;
			// 
			// RelatedActivityGrid
			// 
			this.CommunicationRelatedActivityControl.AllowDrop = true;
			this.CommunicationRelatedActivityControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommunicationRelatedActivityControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommunicationRelatedActivityControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CommunicationRelatedActivityControl.Name = "RelatedActivityGrid";
			this.CommunicationRelatedActivityControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 293, true);
			this.CommunicationRelatedActivityControl.TabIndex = 0;
			// 
			// ToolStrip
			// 
			this.ToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 25, true);
			this.ToolStrip.TabIndex = 0;
			// 
			// FilterPanel
			// 
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 305, true);
			this.FilterPanel.TabIndex = 1;
			// 
			// OrgCommunicationPreviewPaneControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FilterPanel);
			this.Controls.Add(this.ToolStrip);
			this.Controls.Add(this.CommunicationTabControl);
			this.Name = "OrgCommunicationPreviewPaneControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 650, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommunicationTabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.NotesTabPage.ResumeLayout(false);
			this.FollowUpNotesTabPage.ResumeLayout(false);
			this.DiscussedTradeLanesTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.RelatedActivityTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl CommunicationTabControl;
		private ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private CommunicationDetailsUserControl CommunicationDetailsControl;
		private ZArchitecture.GUI.ZTabPage NotesTabPage;
		private ZArchitecture.GUI.ZRichTextBox CallNoteRichTextBox;
		private ZArchitecture.GUI.ZTabPage FollowUpNotesTabPage;
		private ZArchitecture.GUI.ZRichTextBox FollowupNoteRichTextBox;
		private ZArchitecture.GUI.ZTabPage DiscussedTradeLanesTabPage;
		internal ZArchitecture.GUI.ZCheckedListBox TradeProfileLookupCheckedListBox;
		private ZArchitecture.GUI.ZToolStrip ToolStrip;
		private ZArchitecture.GUI.ZPanel FilterPanel;
		private ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		private CustomFieldsCommunicationUserControl CustomFieldsControl;
		private ZArchitecture.GUI.ZTabPage RelatedActivityTabPage;
		private OrgCommunicationPreviewRelatedActivityControl CommunicationRelatedActivityControl;

	}
}
