using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	partial class GlbCompanyCampaignForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.macrosToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
            this.macrosToolStripButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
            this.SendingOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.BatchCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.OrLabel = new Enterprise.ZArchitecture.ZLabel();
            this.FilterForToAllContactsCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.IgnoreDuplicatesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.TouchesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.continueSendPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.postingToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
            this.sendEmailToSelectedButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
            this.TouchSetupTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.buttonMasterCampaign = new Enterprise.ZArchitecture.GUI.ZButton();
            this.buttonDefibrillator = new Enterprise.ZArchitecture.GUI.ZButton();
            this.spacingLabel = new Enterprise.ZArchitecture.ZLabel();
            this.CampaignNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CompanyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ContentDesignerTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.SendingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.BudgetTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zCodeFindBox2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CostPerUnitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.UnitsSentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
            this.CampaignDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.SenderUnlocoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.UseCampaignCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.PermanentlySaveAgainstRecipientCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.EmailSenderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.StaffAssignmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SenderPoolButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.EmailSendersNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SenderEmailAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SenderEmailAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ReplyToTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.UseLastEmailSenderAddressCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ReplyToCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.eDocsAttachmentLabel = new Enterprise.ZArchitecture.ZLabel();
            this.EmailSubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AttachmentsGrid = new Enterprise.ZArchitecture.ZGrid();
            this.SendScheduleTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.SendScheduleControl = new Enterprise.MarketingManager.GUI.SendScheduleUserControl();
            this.TrackingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.CampaignTrackingControl = new Enterprise.MarketingManager.GUI.CampaignTrackingControl();
            this.CampaignDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CampaignTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.MediaCategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CampaignManagerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.MediaTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ActualStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ActualCompletionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
            this.SalesRelationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.salesRelationControl = new Enterprise.MarketingManager.GUI.CampaignSalesRelationControl();
            this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CampaignCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
            this.TrackingSummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ClickStatControl = new Enterprise.MarketingManager.GUI.CampaignClickStatSummaryUserControl();
            this.TrackingStatusGroupBox = new Enterprise.MarketingManager.GUI.CampaignTrackingStatusGroupBox();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SendingOptionsGroupBox.SuspendLayout();
            this.continueSendPanel.SuspendLayout();
            this.TouchSetupTabPage.SuspendLayout();
            this.BudgetTabPage.SuspendLayout();
            this.zCodeFindBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
            this.zGrid1.SuspendLayout();
            this.CampaignDocumentGroupBox.SuspendLayout();
            this.SenderUnlocoCodeFindBox.SuspendLayout();
            this.EmailSenderDropEdit.SuspendLayout();
            this.StaffAssignmentDropEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).BeginInit();
            this.AttachmentsGrid.SuspendLayout();
            this.SendScheduleTabPage.SuspendLayout();
            this.SendScheduleControl.SuspendLayout();
            this.TrackingTabPage.SuspendLayout();
            this.CampaignTrackingControl.SuspendLayout();
            this.CampaignDetailsGroupBox.SuspendLayout();
            this.CampaignTypeDropEdit.SuspendLayout();
            this.MediaCategoryDropEdit.SuspendLayout();
            this.CampaignManagerCodeFindBox.SuspendLayout();
            this.zCodeFindBox1.SuspendLayout();
            this.MediaTypeDropEdit.SuspendLayout();
            this.StatusDropEdit.SuspendLayout();
            this.zDateEdit2.SuspendLayout();
            this.ActualStartDateEdit.SuspendLayout();
            this.ActualCompletionDateEdit.SuspendLayout();
            this.zDateEdit1.SuspendLayout();
            this.WorkflowTabPage.SuspendLayout();
            this.SalesRelationTabPage.SuspendLayout();
            this.salesRelationControl.SuspendLayout();
            this.StatusGroupBox.SuspendLayout();
            this.CustomFieldsGroupBox.SuspendLayout();
            this.CampaignCustomFieldsControl.SuspendLayout();
            this.TrackingSummaryGroupBox.SuspendLayout();
            this.ClickStatControl.SuspendLayout();
			this.SenderEmailAddressDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.TouchesTabPage);
            this.MainTabControl.Controls.Add(this.TouchSetupTabPage);
            this.MainTabControl.Controls.Add(this.ContentDesignerTabPage);
            this.MainTabControl.Controls.Add(this.SendingTabPage);
            this.MainTabControl.Controls.Add(this.TrackingTabPage);
            this.MainTabControl.Controls.Add(this.SendScheduleTabPage);
            this.MainTabControl.Controls.Add(this.BudgetTabPage);
            this.MainTabControl.Controls.Add(this.SalesRelationTabPage);
            this.MainTabControl.Controls.Add(this.WorkflowTabPage);
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1424, 673, true);
            this.MainTabControl.TabIndex = 0;
            this.MainTabControl.SelectedIndexChanged += new System.EventHandler(this.MainTabControl_SelectedIndexChanged);
            this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.SalesRelationTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.BudgetTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.SendScheduleTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.TrackingTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.SendingTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.ContentDesignerTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.TouchSetupTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.TouchesTabPage, 0);
            // 
            // MainTabPage
            // 
            this.MainTabPage.AutoScroll = true;
            this.MainTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1176, 0, true);
            this.MainTabPage.Controls.Add(this.CustomFieldsGroupBox);
            this.MainTabPage.Controls.Add(this.StatusGroupBox);
            this.MainTabPage.Controls.Add(this.CampaignNameTextBox);
            this.MainTabPage.Controls.Add(this.CompanyNameTextBox);
            this.MainTabPage.Controls.Add(this.CampaignDetailsGroupBox);
            this.MainTabPage.Controls.Add(this.CampaignDocumentGroupBox);
            this.MainTabPage.Controls.Add(this.TrackingSummaryGroupBox);
            this.MainTabPage.Controls.Add(this.TrackingStatusGroupBox);
            this.MainTabPage.Controls.Add(this.SendingOptionsGroupBox);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1424, 673, true);
            // 
            // PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
            // 
            this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.spacingLabel);
            this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.continueSendPanel);
            this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1044, 0, true);
            this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 32, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1424, 24, true);
            this.MainStatusBar.TabIndex = 0;
            // 
            // MessageStatusBarPanel
            // 
            this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(913);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
            // 
            // macrosToolStrip
            // 
            this.macrosToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.macrosToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.macrosToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.macrosToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.macrosToolStripButton});
            this.macrosToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 71, true);
            this.macrosToolStrip.Name = "macrosToolStrip";
            this.macrosToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 25, true);
            this.macrosToolStrip.TabIndex = 21;
            // 
            // macrosToolStripButton
            // 
            this.macrosToolStripButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|8171a896-d699-47c0-86bb-b0cd338ba382", "Macros");
            this.macrosToolStripButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.HTMLModeImage;
            this.macrosToolStripButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.macrosToolStripButton.Name = "macrosToolStripButton";
            this.macrosToolStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 22, true);
            this.macrosToolStripButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.macrosToolStripButton.Click += new System.EventHandler(this.DocumentFieldHelpButton_Click);
            // 
            // SendingOptionsGroupBox
            // 
            this.SendingOptionsGroupBox.Controls.Add(this.BatchCountCalcEdit);
            this.SendingOptionsGroupBox.Controls.Add(this.OrLabel);
            this.SendingOptionsGroupBox.Controls.Add(this.FilterForToAllContactsCheckbox);
            this.SendingOptionsGroupBox.Controls.Add(this.IgnoreDuplicatesCheckBox);
            this.SendingOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 220, true);
            this.SendingOptionsGroupBox.Name = "SendingOptionsGroupBox";
            this.SendingOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 50, true);
            this.SendingOptionsGroupBox.TabIndex = 28;
            this.SendingOptionsGroupBox.TabStop = false;
			// 
			// BatchCountCalcEdit
			//
			this.BatchCountCalcEdit.AllowNegative = false;
            this.BatchCountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BatchCountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.BatchCountCalcEdit, "BatchCountForDisplay");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BatchCountForDisplay)));
            this.BatchCountCalcEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|d5e10ffe-b023-41e9-b618-af28471e032c", "Batch Count", "The number of campaigns to send in each batch");
            this.BatchCountCalcEdit.DecimalPlaces = 0;
            this.BatchCountCalcEdit.Decimals = 0;
            this.BatchCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 18, true);
            this.BatchCountCalcEdit.Name = "BatchCountCalcEdit";
            this.BatchCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
            this.BatchCountCalcEdit.TabIndex = 22;
            this.BatchCountCalcEdit.Text = "0";
            this.BatchCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // OrLabel
            // 
            this.OrLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OrLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9b4b6322-9d3b-476e-8745-187dbb60fe07", "OR");
            this.OrLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.OrLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 15, true);
            this.OrLabel.Name = "OrLabel";
            this.OrLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 24, true);
            this.OrLabel.TabIndex = 19;
            // 
            // FilterForToAllContactsCheckbox
            // 
            this.FilterForToAllContactsCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.FilterForToAllContactsCheckbox, "SendToAll");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).SendToAll)));
            this.FilterForToAllContactsCheckbox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|bf37c935-ebfc-4d9d-be3a-625ac953a3e1", "Filter for all Contacts");
            this.FilterForToAllContactsCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 19, true);
            this.FilterForToAllContactsCheckbox.Name = "FilterForToAllContactsCheckbox";
            this.FilterForToAllContactsCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 17, true);
            this.FilterForToAllContactsCheckbox.TabIndex = 23;
            // 
            // IgnoreDuplicatesCheckBox
            // 
            this.IgnoreDuplicatesCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IgnoreDuplicatesCheckBox, "G0_DeDuplicateContacts");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_DeDuplicateContacts)));
            this.IgnoreDuplicatesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 19, true);
            this.IgnoreDuplicatesCheckBox.Name = "IgnoreDuplicatesCheckBox";
            this.IgnoreDuplicatesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.IgnoreDuplicatesCheckBox.TabIndex = 24;
            // 
            // TouchesTabPage
            // 
            this.TouchesTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|C0A4D1C1-D82D-459E-90DF-A1F13AEA4E1F", "Touch Summary");
            this.TouchesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.TouchesTabPage.Name = "TouchesTabPage";
            this.TouchesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.TouchesTabPage.TabIndex = 4;
            // 
            // continueSendPanel
            // 
            this.continueSendPanel.Controls.Add(this.postingToolStrip);
            this.continueSendPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.continueSendPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 0, true);
            this.continueSendPanel.Name = "continueSendPanel";
            this.continueSendPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 32, true);
            this.continueSendPanel.TabIndex = 1;
            // 
            // postingToolStrip
            // 
            this.postingToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.postingToolStrip.Dock = System.Windows.Forms.DockStyle.Right;
            this.postingToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.postingToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendEmailToSelectedButton});
            this.postingToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 0, true);
            this.postingToolStrip.Name = "postingToolStrip";
            this.postingToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 32, true);
            this.postingToolStrip.TabIndex = 0;
            // 
            // sendEmailToSelectedButton
            // 
            this.sendEmailToSelectedButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4CF6C92D-7A30-4EC0-A287-3287B05F0ADE", "Continue Send to Selected");
            this.sendEmailToSelectedButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.SendEmailImage;
            this.sendEmailToSelectedButton.Name = "sendEmailToSelectedButton";
            this.sendEmailToSelectedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
            this.sendEmailToSelectedButton.Click += new System.EventHandler(this.SendEmailToSelectedButton_Click);
            // 
            // TouchSetupTabPage
            // 
            this.TouchSetupTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("94b56307-0560-4af8-bea7-8bd2eb5ca9a1", "Setup Touch");
            this.TouchSetupTabPage.Controls.Add(this.buttonMasterCampaign);
            this.TouchSetupTabPage.Controls.Add(this.buttonDefibrillator);
            this.TouchSetupTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.TouchSetupTabPage.Name = "TouchSetupTabPage";
            this.TouchSetupTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.TouchSetupTabPage.TabIndex = 5;
            // 
            // buttonMasterCampaign
            // 
            this.buttonMasterCampaign.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2C3E9F73-194A-4BAF-98E9-DA4C68F15792", "Open Master Campaign");
            this.buttonMasterCampaign.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 10, true);
            this.buttonMasterCampaign.Name = "buttonMasterCampaign";
            this.buttonMasterCampaign.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 25, true);
            this.buttonMasterCampaign.TabIndex = 0;
            this.buttonMasterCampaign.ToolTipCaption = null;
            this.buttonMasterCampaign.Click += ButtonMasterCampaignOnClick;
			// 
			// buttonDefibrillator
			// 
			this.buttonDefibrillator.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("AEC70B53-C662-4580-A68E-A1D7040691B1", "Force Transitions");
            this.buttonDefibrillator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 10, true);
            this.buttonDefibrillator.Name = "buttonDefibrillator";
            this.buttonDefibrillator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 25, true);
            this.buttonDefibrillator.TabIndex = 0;
            this.buttonDefibrillator.ToolTipCaption = null;
            this.buttonDefibrillator.Click += ButtonDefibrillatorOnClick;
			// 
			// spacingLabel
			// 
			this.spacingLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.spacingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.spacingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 0, true);
            this.spacingLabel.Name = "spacingLabel";
            this.spacingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 32, true);
            this.spacingLabel.TabIndex = 0;
            // 
            // CampaignNameTextBox
            // 
            this.CampaignNameTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CampaignNameTextBox, "G0_CampaignName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_CampaignName)));
            this.CampaignNameTextBox.CaptionResourceString = null;
            this.CampaignNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CampaignNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 12, true);
            this.CampaignNameTextBox.Name = "CampaignNameTextBox";
            this.CampaignNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 20, true);
            this.CampaignNameTextBox.TabIndex = 0;
            // 
            // CompanyNameTextBox
            // 
            this.CompanyNameTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CompanyNameTextBox, "Company.GC_Name");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Company.GC_Name)));
            this.CompanyNameTextBox.CaptionResourceString = null;
            this.CompanyNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CompanyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 12, true);
            this.CompanyNameTextBox.Name = "CompanyNameTextBox";
            this.CompanyNameTextBox.ReadOnly = true;
            this.CompanyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
            this.CompanyNameTextBox.TabIndex = 1;
            // 
            // ContentDesignerTabPage
            // 
            this.ContentDesignerTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|c072542d-cc63-431f-a394-87ec6913dd05", "Email Content");
            this.ContentDesignerTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.ContentDesignerTabPage.Name = "ContentDesignerTabPage";
            this.ContentDesignerTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.ContentDesignerTabPage.TabIndex = 3;
            // 
            // SendingTabPage
            // 
            this.SendingTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|8504def7-fb2c-4c50-98c0-a6fca7ea3406", "Filter");
            this.SendingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.SendingTabPage.Name = "SendingTabPage";
            this.SendingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.SendingTabPage.TabIndex = 4;
            // 
            // BudgetTabPage
            // 
            this.BudgetTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|6c6d3961-93c8-42f4-8720-61eb5509aca7", "Budget");
            this.BudgetTabPage.Controls.Add(this.zCalcEdit1);
            this.BudgetTabPage.Controls.Add(this.zCodeFindBox2);
            this.BudgetTabPage.Controls.Add(this.CostPerUnitCalcEdit);
            this.BudgetTabPage.Controls.Add(this.UnitsSentCalcEdit);
            this.BudgetTabPage.Controls.Add(this.zGrid1);
            this.BudgetTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.BudgetTabPage.Name = "BudgetTabPage";
            this.BudgetTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.BudgetTabPage.TabIndex = 5;
            // 
            // zCalcEdit1
            // 
            this.zCalcEdit1.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.zCalcEdit1, "TotalCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TotalCost)));
            this.zCalcEdit1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|a155b394-94bf-4cb8-bd1b-9eb2a895235f", "Total Cost");
            this.zCalcEdit1.DecimalPlaces = 2;
            this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 336, true);
            this.zCalcEdit1.Name = "zCalcEdit1";
            this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.zCalcEdit1.TabIndex = 36;
            this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // zCodeFindBox2
            // 
            this.zCodeFindBox2.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBox2, "G0_RX_NKCampaignCurrency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_RX_NKCampaignCurrency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Lookups.CampaignCurrencies)));
            this.zCodeFindBox2.BindToList = "Lookups+CampaignCurrencies";
            this.zCodeFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
            this.zCodeFindBox2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.zCodeFindBox2.Name = "zCodeFindBox2";
            this.zCodeFindBox2.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBox2.ParentType = null;
            this.zCodeFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
            this.zCodeFindBox2.TabIndex = 32;
            // 
            // CostPerUnitCalcEdit
            // 
            this.CostPerUnitCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CostPerUnitCalcEdit, "CostPerUnit");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CostPerUnit)));
            this.CostPerUnitCalcEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|a990acbf-140c-4f3b-93a7-6e8b7bb01ac2", "Avg. Cost per Unit", "Total cost divided by Campaign units sent");
            this.CostPerUnitCalcEdit.DecimalPlaces = 2;
            this.CostPerUnitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 360, true);
            this.CostPerUnitCalcEdit.Name = "CostPerUnitCalcEdit";
            this.CostPerUnitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.CostPerUnitCalcEdit.TabIndex = 37;
            this.CostPerUnitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // UnitsSentCalcEdit
            // 
            this.UnitsSentCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UnitsSentCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.UnitsSentCalcEdit, "CampaignUnitsSent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignUnitsSent)));
            this.UnitsSentCalcEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|ddd4062f-0d41-4333-ba44-a68bf66ae630", "Units Sent");
            this.UnitsSentCalcEdit.DecimalPlaces = 0;
            this.UnitsSentCalcEdit.Decimals = 0;
            this.UnitsSentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1327, 8, true);
            this.UnitsSentCalcEdit.Name = "UnitsSentCalcEdit";
            this.UnitsSentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
            this.UnitsSentCalcEdit.TabIndex = 33;
            this.UnitsSentCalcEdit.Text = "0";
            this.UnitsSentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // zGrid1
            // 
            this.zGrid1.AllowNavigation = false;
            this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.zGrid1, "BudgetItems");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).G9_AC)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).Lookups.ChargeCodes)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).G9_ChargeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).G9_PerUnitAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).G9_FlatAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).G9_RX_NKCurrency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).Lookups.Currencies)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).G9_ExchangeRate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).LocalPerUnitAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).BudgetItems)).SyncRoot)).LocalFlatAmount)));
            this.zGrid1.CaptionVisible = false;
            zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.ChargeCodes";
            zGuidFindBoxColumnStyleInfo1.ColumnName = "G9_AC";
            zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCode;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zTextBoxColumnStyleInfo1.ColumnName = "G9_ChargeDescription";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "G9_PerUnitAmount";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "G9_FlatAmount";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.Currencies";
            zCodeFindBoxColumnStyleInfo1.ColumnName = "G9_RX_NKCurrency";
            zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "G9_ExchangeRate";
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|76ec608f-51e8-4ac0-bcf9-b36e67410579", "Local Per Unit");
            zCalcEditColumnStyleInfo4.ColumnName = "LocalPerUnitAmount";
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|557b305d-7174-49a4-a13b-5b923bfd0073", "Local Flat");
            zCalcEditColumnStyleInfo5.ColumnName = "LocalFlatAmount";
            zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
            this.zGrid1.GridId = "eabdc4aa-4a2c-415d-b209-a5da5d5d4868";
            this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.zGrid1.LayoutKey = "zGrid1";
            this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 32, true);
            this.zGrid1.Name = "zGrid1";
            this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 288, true);
            this.zGrid1.TabIndex = 34;
            // 
            // CampaignDocumentGroupBox
            // 
            this.CampaignDocumentGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CampaignDocumentGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|cc017453-f581-4c14-a781-e924a217b7bf", "Campaign Sending Options");
            this.CampaignDocumentGroupBox.Controls.Add(this.SenderUnlocoCodeFindBox);
            this.CampaignDocumentGroupBox.Controls.Add(this.UseCampaignCheckBox);
            this.CampaignDocumentGroupBox.Controls.Add(this.PermanentlySaveAgainstRecipientCheckBox);
            this.CampaignDocumentGroupBox.Controls.Add(this.EmailSenderDropEdit);
            this.CampaignDocumentGroupBox.Controls.Add(this.StaffAssignmentDropEdit);
            this.CampaignDocumentGroupBox.Controls.Add(this.SenderPoolButton);
            this.CampaignDocumentGroupBox.Controls.Add(this.EmailSendersNameTextBox);
			this.CampaignDocumentGroupBox.Controls.Add(this.SenderEmailAddressDropEdit);
			this.CampaignDocumentGroupBox.Controls.Add(this.SenderEmailAddressTextBox);
            this.CampaignDocumentGroupBox.Controls.Add(this.ReplyToTextBox);
            this.CampaignDocumentGroupBox.Controls.Add(this.UseLastEmailSenderAddressCheckBox);
            this.CampaignDocumentGroupBox.Controls.Add(this.ReplyToCheckBox);
            this.CampaignDocumentGroupBox.Controls.Add(this.eDocsAttachmentLabel);
            this.CampaignDocumentGroupBox.Controls.Add(this.EmailSubjectTextBox);
            this.CampaignDocumentGroupBox.Controls.Add(this.macrosToolStrip);
            this.CampaignDocumentGroupBox.Controls.Add(this.AttachmentsGrid);
            this.CampaignDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 40, true);
            this.CampaignDocumentGroupBox.Name = "CampaignDocumentGroupBox";
            this.CampaignDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1989, 175, true);
            this.CampaignDocumentGroupBox.TabIndex = 2;
            this.CampaignDocumentGroupBox.TabStop = false;
            // 
            // SenderUnlocoCodeFindBox
            // 
            this.SenderUnlocoCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SenderUnlocoCodeFindBox, "G0_RL_NKEmailSenderUNLOCO");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_RL_NKEmailSenderUNLOCO)));
            this.SenderUnlocoCodeFindBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6fd9675e-5049-4f7d-bf35-185807e72c18", "Sender Location");
            this.SenderUnlocoCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(590, 47, true);
            this.SenderUnlocoCodeFindBox.Name = "SenderUnlocoCodeFindBox";
            this.SenderUnlocoCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.SenderUnlocoCodeFindBox.ParentType = null;
            this.SenderUnlocoCodeFindBox.PreBoundMaxLength = 5;
            this.SenderUnlocoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 20, true);
            this.SenderUnlocoCodeFindBox.TabIndex = 19;
            // 
            // UseCampaignCheckBox
            // 
            this.BindingSource.SetBindingMember(this.UseCampaignCheckBox, "UseCampaignName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).UseCampaignName)));
            this.UseCampaignCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5f86f7f8-4190-42ed-8b43-eee120a91ed6", "Use Campaign Name");
            this.UseCampaignCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 71, true);
            this.UseCampaignCheckBox.Name = "UseCampaignCheckBox";
            this.UseCampaignCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
            this.UseCampaignCheckBox.TabIndex = 22;
            // 
            // PermanentlySaveAgainstRecipientCheckBox
            // 
            this.BindingSource.SetBindingMember(this.PermanentlySaveAgainstRecipientCheckBox, "G0_StoreEmailInEDocs");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_StoreEmailInEDocs)));
            this.PermanentlySaveAgainstRecipientCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("{F129E1E9-D3F1-4C07-9594-C03F39A91069}", "Permanently Save Against Recipient");
            this.PermanentlySaveAgainstRecipientCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(710, 71, true);
            this.PermanentlySaveAgainstRecipientCheckBox.Name = "PermanentlySaveAgainstRecipientCheckBox";
            this.PermanentlySaveAgainstRecipientCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.PermanentlySaveAgainstRecipientCheckBox.TabIndex = 23;
            // 
            // EmailSenderDropEdit
            // 
            this.EmailSenderDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EmailSenderDropEdit, "G0_EmailSenderOption");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_EmailSenderOption)));
            this.EmailSenderDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("47f4b1d8-98ec-448d-8f66-c88fe7926017", "Email Sender");
            this.EmailSenderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 19, true);
            this.EmailSenderDropEdit.Name = "EmailSenderDropEdit";
            this.EmailSenderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.EmailSenderDropEdit.TabIndex = 12;
            // 
            // StaffAssignmentDropEdit
            // 
            this.StaffAssignmentDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StaffAssignmentDropEdit, "G0_EmailSenderRole");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_EmailSenderRole)));
            this.StaffAssignmentDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3b35df66-faf1-4fa9-a86a-169b8d3d1e16", "Staff Assignment");
            this.StaffAssignmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 19, true);
            this.StaffAssignmentDropEdit.Name = "StaffAssignmentDropEdit";
            this.StaffAssignmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
            this.StaffAssignmentDropEdit.TabIndex = 15;
            this.StaffAssignmentDropEdit.Visible = false;
            // 
            // SenderPoolButton
            // 
            this.SenderPoolButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("849ef026-a74e-4298-8e36-caf398eda8cf", "...", "", "Edit Staff Pool of Senders");
            this.SenderPoolButton.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.SenderPoolButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 19, true);
            this.SenderPoolButton.Name = "SenderPoolButton";
            this.SenderPoolButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
            this.SenderPoolButton.TabIndex = 16;
            this.SenderPoolButton.TabStop = false;
            this.SenderPoolButton.ToolTipCaption = null;
            this.SenderPoolButton.UseVisualStyleBackColor = true;
            this.SenderPoolButton.Click += new System.EventHandler(this.SenderPoolButton_Click);
            // 
            // EmailSendersNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.EmailSendersNameTextBox, "SenderName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).SenderName)));
            this.EmailSendersNameTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("42b4744a-eade-4790-bd70-ed4d1c8c1877", "From");
            this.EmailSendersNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.EmailSendersNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 19, true);
            this.EmailSendersNameTextBox.Name = "EmailSendersNameTextBox";
            this.EmailSendersNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
            this.EmailSendersNameTextBox.TabIndex = 13;
			// 
			// SenderEmailAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.SenderEmailAddressTextBox, "SenderEmailAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).SenderEmailAddress)));
            this.SenderEmailAddressTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7a30518d-5ef1-41ca-a54b-36961088e057", "Email");
            this.SenderEmailAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SenderEmailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(617, 19, true);
            this.SenderEmailAddressTextBox.Name = "SenderEmailAddressTextBox";
            this.SenderEmailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 20, true);
            this.SenderEmailAddressTextBox.TabIndex = 14;
			// 
			// SenderEmailAddressDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SenderEmailAddressDropEdit, "CoordinatorEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CoordinatorEmailAddress)));
			this.SenderEmailAddressDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7a30518d-5ef1-41ca-a54b-36961088e057", "Email");
			this.SenderEmailAddressDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SenderEmailAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(617, 19, true);
			this.SenderEmailAddressDropEdit.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 20, true);
			this.SenderEmailAddressDropEdit.Name = "SenderEmailAddressDropEdit";
			this.SenderEmailAddressDropEdit.ShouldResizeByMaxLength = false;
			this.SenderEmailAddressDropEdit.PreBoundMaxLength = 35;
			this.SenderEmailAddressDropEdit.TabIndex = 14;
			this.SenderEmailAddressDropEdit.ShowDescriptionBox = false;
			this.SenderEmailAddressDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			// 
			// ReplyToTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReplyToTextBox, "G0_ReplyToEmail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_ReplyToEmail)));
            this.ReplyToTextBox.CaptionResourceString = null;
            this.ReplyToTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReplyToTextBox, false);
            this.ReplyToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 47, true);
            this.ReplyToTextBox.Name = "ReplyToTextBox";
            this.ReplyToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
            this.ReplyToTextBox.TabIndex = 18;
            // 
            // UseLastEmailSenderAddressCheckBox
            // 
            this.UseLastEmailSenderAddressCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.UseLastEmailSenderAddressCheckBox, "G0_UseLastEmailSenderAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_UseLastEmailSenderAddress)));
            this.UseLastEmailSenderAddressCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f2fa2218-5ba9-41c5-bb75-9a114f643a1e", "Attempt Last Email Sender First", "Attempt to use last email sender first, else use current campaign sender settings");
            this.UseLastEmailSenderAddressCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(650, 0, true);
            this.UseLastEmailSenderAddressCheckBox.Name = "UseLastEmailSenderAddressCheckBox";
            this.UseLastEmailSenderAddressCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.UseLastEmailSenderAddressCheckBox.TabIndex = 20;
            // 
            // ReplyToCheckBox
            // 
            this.ReplyToCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.ReplyToCheckBox, "UseEmailSenderAddressAsReplyTo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).UseEmailSenderAddressAsReplyTo)));
            this.ReplyToCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d0deccd9-94b9-4a6d-82fa-0570a6c6a7bc", "Reply To Email Sender");
            this.ReplyToCheckBox.Checked = true;
            this.ReplyToCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ReplyToCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 48, true);
            this.ReplyToCheckBox.Name = "ReplyToCheckBox";
            this.ReplyToCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.ReplyToCheckBox.TabIndex = 17;
            // 
            // eDocsAttachmentLabel
            // 
            this.eDocsAttachmentLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("49df4485-1302-4a02-85ac-93571c81da1d", "eDocs Attachments");
            this.eDocsAttachmentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.eDocsAttachmentLabel.IsFontBold = true;
            this.eDocsAttachmentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 95, true);
            this.eDocsAttachmentLabel.Name = "eDocsAttachmentLabel";
            this.eDocsAttachmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 24, true);
            this.eDocsAttachmentLabel.TabIndex = 27;
            // 
            // EmailSubjectTextBox
            // 
            this.EmailSubjectTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.EmailSubjectTextBox, "G0_EmailSubject");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_EmailSubject)));
            this.EmailSubjectTextBox.CaptionResourceString = null;
            this.EmailSubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.EmailSubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 73, true);
            this.EmailSubjectTextBox.Name = "EmailSubjectTextBox";
            this.EmailSubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
            this.EmailSubjectTextBox.TabIndex = 20;
            // 
            // AttachmentsGrid
            // 
            this.AttachmentsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AttachmentsGrid, "CampaignAttachments");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignAttachments)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignAttachmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignAttachments)).SyncRoot)).Selected)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignAttachmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignAttachments)).SyncRoot)).Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignAttachmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignAttachments)).SyncRoot)).FileName)));
            this.AttachmentsGrid.CaptionVisible = false;
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|2bd7c991-d77e-4e4b-b485-83a98774b656", "Selected");
            zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|d8d7528b-cd98-4513-a387-dc0bb83eeb6c", "Description");
            zTextBoxColumnStyleInfo2.ColumnName = "Description";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|809E8807-BFA7-4EB4-8EA9-FCD155E29483", "File Name");
            zTextBoxColumnStyleInfo3.ColumnName = "FileName";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
            this.AttachmentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.AttachmentsGrid.GridId = "525ba11d-5e04-4475-8e1b-49dd39eae52d";
            this.AttachmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AttachmentsGrid.LayoutKey = "AttachmentsGrid";
            this.AttachmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 101, true);
            this.AttachmentsGrid.Name = "AttachmentsGrid";
            this.AttachmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 64, true);
            this.AttachmentsGrid.TabIndex = 25;
            // 
            // SendScheduleTabPage
            // 
            this.SendScheduleTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|c71dae59-92c6-48bd-912b-b39ccaeeeffc", "Send Schedule");
            this.SendScheduleTabPage.Controls.Add(this.SendScheduleControl);
            this.SendScheduleTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.SendScheduleTabPage.Name = "SendScheduleTabPage";
            this.SendScheduleTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.SendScheduleTabPage.TabIndex = 5;
            // 
            // SendScheduleControl
            // 
            this.SendScheduleControl.AllowDrop = true;
            this.SendScheduleControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SendScheduleControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SendScheduleControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 103, true);
            this.SendScheduleControl.Name = "SendScheduleControl";
            this.SendScheduleControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.SendScheduleControl.TabIndex = 3;
            // 
            // TrackingTabPage
            // 
            this.TrackingTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|76226f8e-0788-49c1-8638-71ae17b05c23", "Tracking");
            this.TrackingTabPage.Controls.Add(this.CampaignTrackingControl);
            this.TrackingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.TrackingTabPage.Name = "TrackingTabPage";
            this.TrackingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.TrackingTabPage.TabIndex = 6;
            // 
            // CampaignTrackingControl
            // 
            this.CampaignTrackingControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CampaignTrackingControl, ".");
            this.CampaignTrackingControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CampaignTrackingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CampaignTrackingControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 103, true);
            this.CampaignTrackingControl.Name = "CampaignTrackingControl";
            this.CampaignTrackingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.CampaignTrackingControl.TabIndex = 3;
            // 
            // CampaignDetailsGroupBox
            // 
            this.CampaignDetailsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|34a73da8-f898-4ead-b0d5-c064d77e4e30", "Details");
            this.CampaignDetailsGroupBox.Controls.Add(this.CampaignTypeDropEdit);
            this.CampaignDetailsGroupBox.Controls.Add(this.MediaCategoryDropEdit);
            this.CampaignDetailsGroupBox.Controls.Add(this.CampaignManagerCodeFindBox);
            this.CampaignDetailsGroupBox.Controls.Add(this.zCodeFindBox1);
            this.CampaignDetailsGroupBox.Controls.Add(this.MediaTypeDropEdit);
            this.CampaignDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 39, true);
            this.CampaignDetailsGroupBox.Name = "CampaignDetailsGroupBox";
            this.CampaignDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 153, true);
            this.CampaignDetailsGroupBox.TabIndex = 0;
            this.CampaignDetailsGroupBox.TabStop = false;
            // 
            // CampaignTypeDropEdit
            // 
            this.CampaignTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CampaignTypeDropEdit, "G0_BroadcastVoteSurveyExam");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_BroadcastVoteSurveyExam)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Lookups.CampaignTypeList)));
            this.CampaignTypeDropEdit.BindToList = "Lookups+CampaignTypeList";
            this.CampaignTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 17, true);
            this.CampaignTypeDropEdit.Name = "CampaignTypeDropEdit";
            this.CampaignTypeDropEdit.PreBoundMaxLength = 3;
            this.CampaignTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
            this.CampaignTypeDropEdit.TabIndex = 1;
            // 
            // MediaCategoryDropEdit
            // 
            this.MediaCategoryDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MediaCategoryDropEdit, "G0_Category");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_Category)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).MediaCategoryDescription)));
            this.MediaCategoryDropEdit.BindToForDescription = "MediaCategoryDescription";
            this.MediaCategoryDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("aae24b55-4ad4-4ab2-964d-604549330c00", "Media Category");
            this.MediaCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 43, true);
            this.MediaCategoryDropEdit.Name = "MediaCategoryDropEdit";
            this.MediaCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
            this.MediaCategoryDropEdit.TabIndex = 3;
            // 
            // CampaignManagerCodeFindBox
            // 
            this.CampaignManagerCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CampaignManagerCodeFindBox, "G0_GS_NKCampaignManager");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_GS_NKCampaignManager)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Lookups.CampaignManagers)));
            this.CampaignManagerCodeFindBox.BindToList = "Lookups+CampaignManagers";
            this.CampaignManagerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 95, true);
            this.CampaignManagerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
            this.CampaignManagerCodeFindBox.Name = "CampaignManagerCodeFindBox";
            this.CampaignManagerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CampaignManagerCodeFindBox.ParentType = null;
            this.CampaignManagerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
            this.CampaignManagerCodeFindBox.TabIndex = 5;
            // 
            // zCodeFindBox1
            // 
            this.zCodeFindBox1.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBox1, "G0_GS_NKCampaignCoordinator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_GS_NKCampaignCoordinator)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Lookups.CampaignCoordinators)));
            this.zCodeFindBox1.BindToList = "Lookups+CampaignCoordinators";
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCodeFindBox1, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
            this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 121, true);
            this.zCodeFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
            this.zCodeFindBox1.Name = "zCodeFindBox1";
            this.zCodeFindBox1.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBox1.ParentType = null;
            this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
            this.zCodeFindBox1.TabIndex = 7;
            // 
            // MediaTypeDropEdit
            // 
            this.MediaTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MediaTypeDropEdit, "G0_Type");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_Type)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).MediaTypeDescription)));
            this.MediaTypeDropEdit.BindToForDescription = "MediaTypeDescription";
            this.MediaTypeDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c2903504-242d-4c78-8ab1-b617c616b6a3", "Media Type");
            this.MediaTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 69, true);
            this.MediaTypeDropEdit.Name = "MediaTypeDropEdit";
            this.MediaTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
            this.MediaTypeDropEdit.TabIndex = 4;
            // 
            // StatusDropEdit
            // 
            this.StatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StatusDropEdit, "G0_Stage");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_Stage)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Lookups.CampaignStages)));
            this.StatusDropEdit.BindToList = "Lookups+CampaignStages";
            this.StatusDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3bf83fcf-55e5-4076-a9dd-9c04248a4853", "Campaign Stage");
            this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 19, true);
            this.StatusDropEdit.Name = "StatusDropEdit";
            this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
            this.StatusDropEdit.TabIndex = 9;
            // 
            // zDateEdit2
            // 
            this.zDateEdit2.AllowDrop = true;
            this.zDateEdit2.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEdit2, "G0_EstimatedStartedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_EstimatedStartedDate)));
            this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 49, true);
            this.zDateEdit2.Name = "zDateEdit2";
            this.zDateEdit2.TabIndex = 10;
            // 
            // ActualStartDateEdit
            // 
            this.ActualStartDateEdit.AllowDrop = true;
            this.ActualStartDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ActualStartDateEdit, "G0_ActualStartedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_ActualStartedDate)));
            this.ActualStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 76, true);
            this.ActualStartDateEdit.Name = "ActualStartDateEdit";
            this.ActualStartDateEdit.TabIndex = 11;
            // 
            // ActualCompletionDateEdit
            // 
            this.ActualCompletionDateEdit.AllowDrop = true;
            this.ActualCompletionDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ActualCompletionDateEdit, "G0_ActualCompletedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_ActualCompletedDate)));
            this.ActualCompletionDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|d7ce1eb2-1cc9-4d25-9524-0d6b93bf1c52", "Actual Completion Date");
            this.ActualCompletionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 76, true);
            this.ActualCompletionDateEdit.Name = "ActualCompletionDateEdit";
            this.ActualCompletionDateEdit.TabIndex = 13;
            // 
            // zDateEdit1
            // 
            this.zDateEdit1.AllowDrop = true;
            this.zDateEdit1.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEdit1, "G0_EstimatedCompletedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_EstimatedCompletedDate)));
            this.zDateEdit1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|cd69159a-cb41-46fe-a59c-bdcba737d724", "Estimated Completion Date");
            this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 49, true);
            this.zDateEdit1.Name = "zDateEdit1";
            this.zDateEdit1.TabIndex = 12;
            // 
            // WorkflowTabPage
            // 
            this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.WorkflowTabPage.Name = "WorkflowTabPage";
            this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
            this.WorkflowTabPage.TabIndex = 10;
            // 
            // SalesRelationTabPage
            // 
            this.SalesRelationTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignForm|400351bf-062f-493c-b64c-f1e5c5f74de1", "Sales Relations");
            this.SalesRelationTabPage.Controls.Add(this.salesRelationControl);
            this.SalesRelationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.SalesRelationTabPage.Name = "SalesRelationTabPage";
            this.SalesRelationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.SalesRelationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1416, 646, true);
            this.SalesRelationTabPage.TabIndex = 11;
            // 
            // salesRelationControl
            // 
            this.salesRelationControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.salesRelationControl, "SalesRelationModel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.SalesRelationModel)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).SalesRelationModel)));
            this.salesRelationControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.salesRelationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.salesRelationControl.Name = "salesRelationControl";
            this.salesRelationControl.NameOfATreeElement = Enterprise.MarketingManager.GUI.Res.GetData("75bf3c0c-98a9-461c-9cfa-adc6dc67bf5c", "Relatable Activity");
            this.salesRelationControl.NameOfTreeElementsPlural = Enterprise.MarketingManager.GUI.Res.GetData("e85aadc8-5b4f-4f33-86c4-180b090e06d6", "Relatable Activities");
            this.salesRelationControl.ShowNewButton = false;
            this.salesRelationControl.ShowPopupButton = false;
            this.salesRelationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1410, 640, true);
            this.salesRelationControl.TabIndex = 0;
            // 
            // StatusGroupBox
            // 
            this.StatusGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9d502df2-d9fd-4ab8-af79-428e70aa0710", "Status");
            this.StatusGroupBox.Controls.Add(this.StatusDropEdit);
            this.StatusGroupBox.Controls.Add(this.zDateEdit2);
            this.StatusGroupBox.Controls.Add(this.ActualStartDateEdit);
            this.StatusGroupBox.Controls.Add(this.zDateEdit1);
            this.StatusGroupBox.Controls.Add(this.ActualCompletionDateEdit);
            this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 198, true);
            this.StatusGroupBox.Name = "StatusGroupBox";
            this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 112, true);
            this.StatusGroupBox.TabIndex = 1;
            this.StatusGroupBox.TabStop = false;
            // 
            // CustomFieldsGroupBox
            // 
            this.CustomFieldsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomFieldsGroupBox.AutoSize = true;
            this.CustomFieldsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d511c107-e3ad-4ebb-9b43-d36f7f3ac541", "Custom Fields");
            this.CustomFieldsGroupBox.Controls.Add(this.CampaignCustomFieldsControl);
            this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 316, true);
            this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
            this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 320, true);
            this.CustomFieldsGroupBox.TabIndex = 5;
            this.CustomFieldsGroupBox.TabStop = false;
            // 
            // CampaignCustomFieldsControl
            // 
            this.CampaignCustomFieldsControl.AllowDrop = true;
            this.CampaignCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CampaignCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.CampaignCustomFieldsControl.Name = "CampaignCustomFieldsControl";
            this.CampaignCustomFieldsControl.NothingSetupMessageLabelText = "To make use of this tab, please setup Transport Booking Instruction custom fields" +
    " in Workflow Manager.";
            this.CampaignCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 485, true);
            this.CampaignCustomFieldsControl.TabIndex = 6;
            // 
            // TrackingSummaryGroupBox
            // 
            this.TrackingSummaryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TrackingSummaryGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3b7ae107-00b6-4e80-9e3c-53138c66cf9f", "Link Activity");
            this.TrackingSummaryGroupBox.Controls.Add(this.ClickStatControl);
            this.TrackingSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 436, true);
            this.TrackingSummaryGroupBox.Name = "TrackingSummaryGroupBox";
            this.TrackingSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 200, true);
            this.TrackingSummaryGroupBox.TabIndex = 7;
            this.TrackingSummaryGroupBox.TabStop = false;
            // 
            // ClickStatControl
            // 
            this.ClickStatControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ClickStatControl, "StatModel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.Business.ClickStatModel)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).StatModel)));
            this.ClickStatControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClickStatControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.ClickStatControl.Name = "ClickStatControl";
            this.ClickStatControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1983, 367, true);
            this.ClickStatControl.TabIndex = 0;
            // 
            // TrackingStatusGroupBox
            // 
            this.TrackingStatusGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TrackingStatusGroupBox, false);
            this.TrackingStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 284, true);
            this.TrackingStatusGroupBox.Name = "TrackingStatusGroupBox";
            this.TrackingStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1989, 150, true);
            this.TrackingStatusGroupBox.TabIndex = 6;
            this.TrackingStatusGroupBox.TabStop = false;
            // 
            // GlbCompanyCampaignForm
            // 
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1424, 729, true);
            this.DataSourceAssemblyName = "Enterprise.MarketingManager.Business";
            this.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
            this.DataSourceTypeName = "Enterprise.MarketingManager.Business.GlbCompanyCampaign";
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1440, 768, true);
            this.Name = "GlbCompanyCampaignForm";
            this.ShouldSerializeTabPageMethods = false;
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainTabPage.ResumeLayout(false);
            this.MainTabPage.PerformLayout();
            this.NotesTabPage.ResumeLayout(false);
            this.NotesTabPage.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.ResumeLayout(false);
            this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SendingOptionsGroupBox.ResumeLayout(false);
            this.SendingOptionsGroupBox.PerformLayout();
            this.continueSendPanel.ResumeLayout(false);
            this.continueSendPanel.PerformLayout();
            this.TouchSetupTabPage.ResumeLayout(false);
            this.TouchSetupTabPage.PerformLayout();
            this.BudgetTabPage.ResumeLayout(false);
            this.BudgetTabPage.PerformLayout();
            this.zCodeFindBox2.ResumeLayout(true);
            this.zCodeFindBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
            this.zGrid1.ResumeLayout(false);
            this.zGrid1.PerformLayout();
            this.CampaignDocumentGroupBox.ResumeLayout(false);
            this.CampaignDocumentGroupBox.PerformLayout();
            this.SenderUnlocoCodeFindBox.ResumeLayout(true);
            this.SenderUnlocoCodeFindBox.PerformLayout();
            this.EmailSenderDropEdit.ResumeLayout(true);
            this.EmailSenderDropEdit.PerformLayout();
            this.StaffAssignmentDropEdit.ResumeLayout(true);
            this.StaffAssignmentDropEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).EndInit();
            this.AttachmentsGrid.ResumeLayout(false);
            this.AttachmentsGrid.PerformLayout();
            this.SendScheduleTabPage.ResumeLayout(false);
            this.SendScheduleTabPage.PerformLayout();
            this.SendScheduleControl.ResumeLayout(true);
            this.SendScheduleControl.PerformLayout();
            this.TrackingTabPage.ResumeLayout(false);
            this.TrackingTabPage.PerformLayout();
            this.CampaignTrackingControl.ResumeLayout(true);
            this.CampaignTrackingControl.PerformLayout();
            this.CampaignDetailsGroupBox.ResumeLayout(false);
            this.CampaignDetailsGroupBox.PerformLayout();
            this.CampaignTypeDropEdit.ResumeLayout(true);
            this.CampaignTypeDropEdit.PerformLayout();
            this.MediaCategoryDropEdit.ResumeLayout(true);
            this.MediaCategoryDropEdit.PerformLayout();
            this.CampaignManagerCodeFindBox.ResumeLayout(true);
            this.CampaignManagerCodeFindBox.PerformLayout();
            this.zCodeFindBox1.ResumeLayout(true);
            this.zCodeFindBox1.PerformLayout();
            this.MediaTypeDropEdit.ResumeLayout(true);
            this.MediaTypeDropEdit.PerformLayout();
            this.StatusDropEdit.ResumeLayout(true);
            this.StatusDropEdit.PerformLayout();
            this.zDateEdit2.ResumeLayout(true);
            this.zDateEdit2.PerformLayout();
            this.ActualStartDateEdit.ResumeLayout(true);
            this.ActualStartDateEdit.PerformLayout();
            this.ActualCompletionDateEdit.ResumeLayout(true);
            this.ActualCompletionDateEdit.PerformLayout();
            this.zDateEdit1.ResumeLayout(true);
            this.zDateEdit1.PerformLayout();
            this.WorkflowTabPage.ResumeLayout(false);
            this.WorkflowTabPage.PerformLayout();
            this.SalesRelationTabPage.ResumeLayout(false);
            this.SalesRelationTabPage.PerformLayout();
            this.salesRelationControl.ResumeLayout(true);
            this.salesRelationControl.PerformLayout();
            this.StatusGroupBox.ResumeLayout(false);
            this.StatusGroupBox.PerformLayout();
            this.CustomFieldsGroupBox.ResumeLayout(false);
            this.CustomFieldsGroupBox.PerformLayout();
            this.CampaignCustomFieldsControl.ResumeLayout(true);
            this.CampaignCustomFieldsControl.PerformLayout();
            this.TrackingSummaryGroupBox.ResumeLayout(false);
            this.TrackingSummaryGroupBox.PerformLayout();
            this.ClickStatControl.ResumeLayout(true);
            this.ClickStatControl.PerformLayout();
			this.SenderEmailAddressDropEdit.ResumeLayout(true);
			this.SenderEmailAddressDropEdit.PerformLayout();
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZGroupBox StatusGroupBox;
		internal ZLabel OrLabel;
		private ZLabel eDocsAttachmentLabel;
		internal ZCheckBox UseCampaignCheckBox;
		internal ZCheckBox PermanentlySaveAgainstRecipientCheckBox;
		private ZDropEdit EmailSenderDropEdit;
		internal ZDropEdit StaffAssignmentDropEdit;
		private ZGrid zGrid1;
		internal ZGrid AttachmentsGrid;
		private ZCodeFindBox zCodeFindBox1;
		private ZCodeFindBox CampaignManagerCodeFindBox;
		private ZDropEdit StatusDropEdit;
		private ZDropEdit MediaCategoryDropEdit;
		private ZDropEdit MediaTypeDropEdit;
		internal ZDropEdit SenderEmailAddressDropEdit;
		private ZDateEdit zDateEdit2;
		private ZDateEdit ActualStartDateEdit;
		private ZDateEdit ActualCompletionDateEdit;
		private ZDateEdit zDateEdit1;
		private ZCheckBox IgnoreDuplicatesCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit1;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox2;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox FilterForToAllContactsCheckbox;
		internal Enterprise.ZArchitecture.ZTextBox CampaignNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox CompanyNameTextBox;
		internal Enterprise.ZArchitecture.GUI.ZTabPage ContentDesignerTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage SendingTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage BudgetTabPage;
		private Enterprise.ZArchitecture.ZCalcEdit UnitsSentCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit CostPerUnitCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox CampaignDocumentGroupBox;
		internal Enterprise.ZArchitecture.ZCalcEdit BatchCountCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZTabPage SendScheduleTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage TrackingTabPage;
		internal Enterprise.ZArchitecture.ZTextBox EmailSubjectTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CampaignDetailsGroupBox;
		internal Enterprise.ZArchitecture.ZTextBox EmailSendersNameTextBox;
		internal Enterprise.ZArchitecture.ZTextBox SenderEmailAddressTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ReplyToTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox ReplyToCheckBox;
		internal ZDropEdit CampaignTypeDropEdit;
		private ZWorkflowTabPage WorkflowTabPage;
		private ZTabPage SalesRelationTabPage;
		internal SendScheduleUserControl SendScheduleControl;
		internal CampaignTrackingControl CampaignTrackingControl;
		private ZGroupBox CustomFieldsGroupBox;
		private ProcessTemplateCustomFieldsControl CampaignCustomFieldsControl;
		private CampaignSalesRelationControl salesRelationControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox TrackingSummaryGroupBox;
		private CampaignTrackingStatusGroupBox TrackingStatusGroupBox;
		private CampaignClickStatSummaryUserControl ClickStatControl;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox SendingOptionsGroupBox;
		private ZToolStrip macrosToolStrip;
		private ZToolStripButton macrosToolStripButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SenderPoolButton;
		internal ZTabPage TouchesTabPage;
		internal ZTabPage TouchSetupTabPage;
		private ZButton buttonMasterCampaign;
		private ZButton buttonDefibrillator;
		internal ZCodeFindBox SenderUnlocoCodeFindBox;
		internal ZCheckBox UseLastEmailSenderAddressCheckBox;
		protected internal ZArchitecture.GUI.ZToolStripButton sendEmailToSelectedButton;
		protected ZLabel spacingLabel;
		private ZArchitecture.GUI.ZToolStrip postingToolStrip;
		private ZPanel continueSendPanel;
	}
}
