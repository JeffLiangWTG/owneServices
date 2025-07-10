namespace Enterprise.MasterFiles.GUI
{
	partial class CommunicationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DetailsUserControl = new Enterprise.MasterFiles.GUI.CommunicationDetailsUserControl();
			this.NotesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.CallNotesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NotesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.InternalNotesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CallNotesRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.ClientVisibleNoteGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClientVisibleNoteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FollowupNotesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FollowupNotesRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.TradeLanesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TradeLanesCheckedListBox = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CommunicationCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.bottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.relatedActivityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RelatedActivitiesGrid = new Enterprise.MasterFiles.GUI.RelatedActivityButtonGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsUserControl.SuspendLayout();
			this.NotesTabControl.SuspendLayout();
			this.CallNotesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NotesSplitContainer)).BeginInit();
			this.NotesSplitContainer.Panel1.SuspendLayout();
			this.NotesSplitContainer.Panel2.SuspendLayout();
			this.NotesSplitContainer.SuspendLayout();
			this.InternalNotesGroupBox.SuspendLayout();
			this.CallNotesRichTextBox.SuspendLayout();
			this.ClientVisibleNoteGroupBox.SuspendLayout();
			this.FollowupNotesTabPage.SuspendLayout();
			this.FollowupNotesRichTextBox.SuspendLayout();
			this.TradeLanesTabPage.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.CommunicationCustomFieldsControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).BeginInit();
			this.bottomSplitContainer.Panel1.SuspendLayout();
			this.bottomSplitContainer.Panel2.SuspendLayout();
			this.bottomSplitContainer.SuspendLayout();
			this.relatedActivityGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedActivitiesGrid.InnerGrid)).BeginInit();
			this.RelatedActivitiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 644, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.bottomSplitContainer);
			this.MainTabPage.Controls.Add(this.DetailsUserControl);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1345, 623, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1345, 623, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1345, 623, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 644, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSalesCall);
			// 
			// DetailsUserControl
			// 
			this.DetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsUserControl, ".");
			this.DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsUserControl.Name = "DetailsUserControl";
			this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1345, 291, true);
			this.DetailsUserControl.TabIndex = 0;
			// 
			// NotesTabControl
			// 
			this.NotesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.NotesTabControl.Controls.Add(this.CallNotesTabPage);
			this.NotesTabControl.Controls.Add(this.FollowupNotesTabPage);
			this.NotesTabControl.Controls.Add(this.TradeLanesTabPage);
			this.NotesTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.NotesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NotesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NotesTabControl.Name = "NotesTabControl";
			this.NotesTabControl.SelectedIndex = 0;
			this.NotesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 332, true);
			this.NotesTabControl.TabIndex = 1;
			// 
			// CallNotesTabPage
			// 
			this.CallNotesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b0711583-9c50-4171-9414-13f4473a618d", "Notes");
			this.CallNotesTabPage.Controls.Add(this.NotesSplitContainer);
			this.CallNotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CallNotesTabPage.Name = "CallNotesTabPage";
			this.CallNotesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CallNotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 311, true);
			this.CallNotesTabPage.TabIndex = 0;
			this.CallNotesTabPage.UseVisualStyleBackColor = true;
			// 
			// NotesSplitContainer
			// 
			this.NotesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NotesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.NotesSplitContainer.Name = "NotesSplitContainer";
			this.NotesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// NotesSplitContainer.Panel1
			// 
			this.NotesSplitContainer.Panel1.Controls.Add(this.InternalNotesGroupBox);
			// 
			// NotesSplitContainer.Panel2
			// 
			this.NotesSplitContainer.Panel2.Controls.Add(this.ClientVisibleNoteGroupBox);
			this.NotesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 305, true);
			this.NotesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(167);
			this.NotesSplitContainer.SplitterWidth = 1;
			this.NotesSplitContainer.TabIndex = 3;
			// 
			// InternalNotesGroupBox
			// 
			this.InternalNotesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0f4c2c47-a5b4-4865-b550-f81c17f034f0", "Internal");
			this.InternalNotesGroupBox.Controls.Add(this.CallNotesRichTextBox);
			this.InternalNotesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InternalNotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InternalNotesGroupBox.Name = "InternalNotesGroupBox";
			this.InternalNotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 167, true);
			this.InternalNotesGroupBox.TabIndex = 1;
			this.InternalNotesGroupBox.TabStop = false;
			// 
			// CallNotesRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.CallNotesRichTextBox, "OQ_SalesCallNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_SalesCallNotes)));
			this.CallNotesRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CallNotesRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CallNotesRichTextBox.MaxLength = 10000000;
			this.CallNotesRichTextBox.Name = "CallNotesRichTextBox";
			this.CallNotesRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 150, true);
			this.CallNotesRichTextBox.TabIndex = 0;
			// 
			// ClientVisibleNoteGroupBox
			// 
			this.ClientVisibleNoteGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0bea2239-3eb4-4f6d-af38-4d10d1ddd0de", "Client Visible Note");
			this.ClientVisibleNoteGroupBox.Controls.Add(this.ClientVisibleNoteTextBox);
			this.ClientVisibleNoteGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientVisibleNoteGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientVisibleNoteGroupBox.Name = "ClientVisibleNoteGroupBox";
			this.ClientVisibleNoteGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 138, true);
			this.ClientVisibleNoteGroupBox.TabIndex = 2;
			this.ClientVisibleNoteGroupBox.TabStop = false;
			// 
			// ClientVisibleNoteTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientVisibleNoteTextBox, "ClientVisibleNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).ClientVisibleNote)));
			this.ClientVisibleNoteTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientVisibleNoteTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClientVisibleNoteTextBox, false);
			this.ClientVisibleNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ClientVisibleNoteTextBox.Multiline = true;
			this.ClientVisibleNoteTextBox.Name = "ClientVisibleNoteTextBox";
			this.ClientVisibleNoteTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ClientVisibleNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 121, true);
			this.ClientVisibleNoteTextBox.TabIndex = 1;
			// 
			// FollowupNotesTabPage
			// 
			this.FollowupNotesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b28b77c9-ecf8-47e6-8cce-8a3bf79146ac", "Follow Up Notes");
			this.FollowupNotesTabPage.Controls.Add(this.FollowupNotesRichTextBox);
			this.FollowupNotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.FollowupNotesTabPage.Name = "FollowupNotesTabPage";
			this.FollowupNotesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FollowupNotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 311, true);
			this.FollowupNotesTabPage.TabIndex = 1;
			this.FollowupNotesTabPage.UseVisualStyleBackColor = true;
			// 
			// FollowupNotesRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.FollowupNotesRichTextBox, "OQ_FollowupNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_FollowupNotes)));
			this.FollowupNotesRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FollowupNotesRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FollowupNotesRichTextBox.MaxLength = 10000000;
			this.FollowupNotesRichTextBox.Name = "FollowupNotesRichTextBox";
			this.FollowupNotesRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 305, true);
			this.FollowupNotesRichTextBox.TabIndex = 0;
			// 
			// TradeLanesTabPage
			// 
			this.TradeLanesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2800c7eb-14e1-49b0-81c8-f764efc37120", "Related Value Analysis");
			this.TradeLanesTabPage.Controls.Add(this.TradeLanesCheckedListBox);
			this.TradeLanesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.TradeLanesTabPage.Name = "TradeLanesTabPage";
			this.TradeLanesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TradeLanesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 311, true);
			this.TradeLanesTabPage.TabIndex = 2;
			this.TradeLanesTabPage.UseVisualStyleBackColor = true;
			// 
			// TradeLanesCheckedListBox
			// 
			this.TradeLanesCheckedListBox.BindingItems = null;
			this.BindingSource.SetBindingMember(this.TradeLanesCheckedListBox, "TradeProfileDescriptionList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZBoolDescriptionPairList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).TradeProfileDescriptionList)));
			this.TradeLanesCheckedListBox.CheckOnClick = true;
			this.TradeLanesCheckedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TradeLanesCheckedListBox.FormattingEnabled = true;
			this.TradeLanesCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TradeLanesCheckedListBox.Name = "TradeLanesCheckedListBox";
			this.TradeLanesCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 305, true);
			this.TradeLanesCheckedListBox.TabIndex = 0;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f12cb45a-e82c-4b23-adbc-bc5779f20302", "Custom Fields");
			this.CustomFieldsTabPage.Controls.Add(this.CommunicationCustomFieldsControl);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 7, 3, 3, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 311, true);
			this.CustomFieldsTabPage.TabIndex = 3;
			// 
			// CommunicationCustomFieldsControl
			// 
			this.CommunicationCustomFieldsControl.AllowDrop = true;
			this.CommunicationCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommunicationCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.CommunicationCustomFieldsControl.Name = "CommunicationCustomFieldsControl";
			this.CommunicationCustomFieldsControl.NothingSetupMessageLabelText = "To make use of this tab, please setup Transport Booking Instruction custom fields" +
    " in Workflow Manager.";
			this.CommunicationCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 301, true);
			this.CommunicationCustomFieldsControl.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 554, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// bottomSplitContainer
			// 
			this.bottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomSplitContainer.IsSplitterFixed = true;
			this.bottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 291, true);
			this.bottomSplitContainer.Name = "bottomSplitContainer";
			// 
			// bottomSplitContainer.Panel1
			// 
			this.bottomSplitContainer.Panel1.Controls.Add(this.NotesTabControl);
			// 
			// bottomSplitContainer.Panel2
			// 
			this.bottomSplitContainer.Panel2.Controls.Add(this.relatedActivityGroupBox);
			this.bottomSplitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 9, 3, true);
			this.bottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1345, 332, true);
			this.bottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(848);
			this.bottomSplitContainer.TabIndex = 1;
			// 
			// relatedActivityGroupBox
			// 
			this.relatedActivityGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("02f42fa3-7ab5-4959-84fa-e96ee519d0cc", "Related Activity");
			this.relatedActivityGroupBox.Controls.Add(this.RelatedActivitiesGrid);
			this.relatedActivityGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.relatedActivityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.relatedActivityGroupBox.Name = "relatedActivityGroupBox";
			this.relatedActivityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 329, true);
			this.relatedActivityGroupBox.TabIndex = 0;
			this.relatedActivityGroupBox.TabStop = false;
			// 
			// RelatedActivitiesGrid
			// 
			this.RelatedActivitiesGrid.AllowDrop = true;
			this.RelatedActivitiesGrid.AttachButtonText = null;
			this.BindingSource.SetBindingMember(this.RelatedActivitiesGrid, "RelatedActivityLinkCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).RelatedActivityLinkCollection)));
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7448e161-4197-4b13-b84c-b9085eb0ace5", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "ToActivityTypeForBinding";
			zDropEditColumnStyleInfo1.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f15e8ff9-abd3-4efd-ad51-ec4aad5fd397", "ID");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ToActivityIDForBinding";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bec7a97b-fd84-487a-a0c8-018421125dab", "Information");
			zTextBoxColumnStyleInfo1.ColumnName = "ToActivityForBinding+Summary";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("410ac086-6150-41b6-b289-acef3b746438", "Created Time");
			zDateEditColumnStyleInfo1.ColumnName = "ToActivityForBindingLocalAuditDetails+SystemCreateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e34eea72-0ffb-4d74-be24-a45cbbb8ab72", "Creating User");
			zTextBoxColumnStyleInfo2.ColumnName = "ToActivityForBindingLocalAuditDetails+SystemCreateUser";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6f09a0c0-88db-4d53-abc8-f034c3f88c05", "Last Edit Time");
			zDateEditColumnStyleInfo2.ColumnName = "ToActivityForBindingLocalAuditDetails+SystemLastEditTime";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("efab984c-7716-4ce1-8b29-d87c4d8e6e6a", "Last Edit User");
			zTextBoxColumnStyleInfo3.ColumnName = "ToActivityForBindingLocalAuditDetails+SystemLastEditUser";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.RelatedActivitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RelatedActivitiesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RelatedActivitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedActivitiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RelatedActivitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedActivitiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RelatedActivitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RelatedActivitiesGrid.DetachButtonText = null;
			this.RelatedActivitiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedActivitiesGrid.GridId = "e49e2bbf-751c-44ce-984d-c5b094085086";
			// 
			// 
			// 
			this.RelatedActivitiesGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedActivitiesGrid.InnerGrid.LayoutKey = "Grid";
			this.RelatedActivitiesGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.RelatedActivitiesGrid.InnerGrid.Name = "Grid";
			this.RelatedActivitiesGrid.InnerGrid.TabIndex = 0;
			this.RelatedActivitiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.RelatedActivitiesGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.RelatedActivitiesGrid.Name = "RelatedActivitiesGrid";
			this.RelatedActivitiesGrid.NewButtonText = null;
			this.RelatedActivitiesGrid.ReadOnly = false;
			this.RelatedActivitiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 312, true);
			this.RelatedActivitiesGrid.TabIndex = 0;
			// 
			// CommunicationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 700, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSalesCall);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1250, 700, true);
			this.Name = "CommunicationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "CommunicationForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsUserControl.ResumeLayout(true);
			this.DetailsUserControl.PerformLayout();
			this.NotesTabControl.ResumeLayout(false);
			this.NotesTabControl.PerformLayout();
			this.CallNotesTabPage.ResumeLayout(false);
			this.CallNotesTabPage.PerformLayout();
			this.NotesSplitContainer.Panel1.ResumeLayout(false);
			this.NotesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.NotesSplitContainer)).EndInit();
			this.NotesSplitContainer.ResumeLayout(false);
			this.NotesSplitContainer.PerformLayout();
			this.InternalNotesGroupBox.ResumeLayout(false);
			this.InternalNotesGroupBox.PerformLayout();
			this.CallNotesRichTextBox.ResumeLayout(true);
			this.CallNotesRichTextBox.PerformLayout();
			this.ClientVisibleNoteGroupBox.ResumeLayout(false);
			this.ClientVisibleNoteGroupBox.PerformLayout();
			this.FollowupNotesTabPage.ResumeLayout(false);
			this.FollowupNotesTabPage.PerformLayout();
			this.FollowupNotesRichTextBox.ResumeLayout(true);
			this.FollowupNotesRichTextBox.PerformLayout();
			this.TradeLanesTabPage.ResumeLayout(false);
			this.TradeLanesTabPage.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.CommunicationCustomFieldsControl.ResumeLayout(true);
			this.CommunicationCustomFieldsControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.bottomSplitContainer.Panel1.ResumeLayout(false);
			this.bottomSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).EndInit();
			this.bottomSplitContainer.ResumeLayout(false);
			this.bottomSplitContainer.PerformLayout();
			this.relatedActivityGroupBox.ResumeLayout(false);
			this.relatedActivityGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedActivitiesGrid.InnerGrid)).EndInit();
			this.RelatedActivitiesGrid.ResumeLayout(true);
			this.RelatedActivitiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CommunicationDetailsUserControl DetailsUserControl;
		private ZArchitecture.GUI.ZTabControl NotesTabControl;
		private ZArchitecture.GUI.ZTabPage CallNotesTabPage;
		private ZArchitecture.GUI.ZTabPage FollowupNotesTabPage;
		private ZArchitecture.GUI.ZRichTextBox FollowupNotesRichTextBox;
		private ZArchitecture.GUI.ZRichTextBox CallNotesRichTextBox;
		private ZArchitecture.GUI.ZTabPage TradeLanesTabPage;
		internal ZArchitecture.GUI.ZCheckedListBox TradeLanesCheckedListBox;
		private ZWorkflowTabPage WorkflowTabPage;
		private ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl CommunicationCustomFieldsControl;
		protected CargoWise.Windows.UI.KSplitContainer bottomSplitContainer;
		private ZArchitecture.GUI.ZGroupBox relatedActivityGroupBox;
		protected Enterprise.MasterFiles.GUI.RelatedActivityButtonGrid RelatedActivitiesGrid;
		private ZArchitecture.GUI.ZGroupBox ClientVisibleNoteGroupBox;
		private ZArchitecture.ZTextBox ClientVisibleNoteTextBox;
		private ZArchitecture.GUI.ZGroupBox InternalNotesGroupBox;
		protected CargoWise.Windows.UI.KSplitContainer NotesSplitContainer;
	}
}
