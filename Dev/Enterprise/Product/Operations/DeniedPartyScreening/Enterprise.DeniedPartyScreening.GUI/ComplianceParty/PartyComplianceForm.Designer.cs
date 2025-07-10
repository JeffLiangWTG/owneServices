namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class PartyComplianceForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ForceRescreenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProcessedWrappersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ScreenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.UnProcessedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UnprocessedWrappersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProcessedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.MessagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageDetailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProcessedWrappersGrid)).BeginInit();
			this.ProcessedWrappersGrid.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.UnProcessedGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnprocessedWrappersGrid)).BeginInit();
			this.UnprocessedWrappersGrid.SuspendLayout();
			this.ProcessedGroupBox.SuspendLayout();
			this.FlowLayoutPanel.SuspendLayout();
			this.MessagePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 342, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("1bb95a8f-8b06-4afe-8815-c0be3b8751cc", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(714, 2, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ForceRescreenButton
			// 
			this.ForceRescreenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ForceRescreenButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("30469aba-65d2-4cb6-9577-32741db52789", "Force Re-screen All");
			this.ForceRescreenButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ForceRescreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 2, true);
			this.ForceRescreenButton.Name = "ForceRescreenButton";
			this.ForceRescreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.ForceRescreenButton.TabIndex = 2;
			this.ForceRescreenButton.ToolTipCaption = null;
			this.ForceRescreenButton.UseVisualStyleBackColor = true;
			this.ForceRescreenButton.Click += new System.EventHandler(this.ForceRescreenButton_Click);
			// 
			// ProcessedWrappersGrid
			// 
			this.ProcessedWrappersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProcessedWrappersGrid, "ProcessedParties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).ProcessedParties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).ProcessedParties)).SyncRoot)).ScreeningStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).ProcessedParties)).SyncRoot)).ScreeningStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).ProcessedParties)).SyncRoot)).OrgCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).ProcessedParties)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).ProcessedParties)).SyncRoot)).ParentsDescription)));
			this.ProcessedWrappersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("fe9f9277-ef80-4ef8-9155-74f10aec321f", "Status Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ScreeningStatus";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("345fee17-c8ab-40b5-aa40-e373f82f114e", "Screening Status");
			zTextBoxColumnStyleInfo2.ColumnName = "ScreeningStatusDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("1aad930a-a17a-46d1-8f2d-e764556095d7", "Party Code");
			zTextBoxColumnStyleInfo3.ColumnName = "OrgCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("dc81bc5d-675b-4f3a-aebc-23078ee9daf0", "Party Name");
			zTextBoxColumnStyleInfo4.ColumnName = "Code";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("b27780cf-e774-4bbb-aca8-d5059bd9316e", "Description");
			zMultiLineTextBoxColumnInfo1.ColumnName = "ParentsDescription";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.ProcessedWrappersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProcessedWrappersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProcessedWrappersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProcessedWrappersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ProcessedWrappersGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ProcessedWrappersGrid.DisableImportDataMenuItem = true;
			this.ProcessedWrappersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcessedWrappersGrid.GridId = "72c6b7ce-62de-4eec-8526-4856bad609f8";
			this.ProcessedWrappersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProcessedWrappersGrid.LayoutKey = "ProcessedWrappersGrid";
			this.ProcessedWrappersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ProcessedWrappersGrid.Name = "ProcessedWrappersGrid";
			this.ProcessedWrappersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 116, true);
			this.ProcessedWrappersGrid.TabIndex = 0;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ScreenButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("79ebd2e4-e424-4481-b2e3-1741c0551228", "Screen Unprocessed");
			this.ScreenButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 2, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.ScreenButton.TabIndex = 1;
			this.ScreenButton.ToolTipCaption = null;
			this.ScreenButton.UseVisualStyleBackColor = true;
			this.ScreenButton.Click += new System.EventHandler(this.ScreenButton_Click);
			// 
			// MainPanel
			// 
			this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MainPanel.ColumnCount = 1;
			this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainPanel.Controls.Add(this.UnProcessedGroupBox, 0, 1);
			this.MainPanel.Controls.Add(this.ProcessedGroupBox, 0, 0);
			this.MainPanel.Controls.Add(this.FlowLayoutPanel, 0, 2);
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 41, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.RowCount = 3;
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 305, true);
			this.MainPanel.TabIndex = 6;
			// 
			// UnProcessedGroupBox
			// 
			this.UnProcessedGroupBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("5ee96c4a-5520-481e-9b56-1b119b474245", "Unprocessed");
			this.UnProcessedGroupBox.Controls.Add(this.UnprocessedWrappersGrid);
			this.UnProcessedGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnProcessedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 140, true);
			this.UnProcessedGroupBox.Name = "UnProcessedGroupBox";
			this.UnProcessedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 133, true);
			this.UnProcessedGroupBox.TabIndex = 8;
			this.UnProcessedGroupBox.TabStop = false;
			// 
			// UnprocessedWrappersGrid
			// 
			this.UnprocessedWrappersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnprocessedWrappersGrid, "UnProcessedParties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).UnprocessedParties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).UnprocessedParties)).SyncRoot)).ScreeningStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).UnprocessedParties)).SyncRoot)).ScreeningStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).UnprocessedParties)).SyncRoot)).OrgCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).UnprocessedParties)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection)(null)).UnprocessedParties)).SyncRoot)).ParentsDescription)));
			this.UnprocessedWrappersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("fe9f9277-ef80-4ef8-9155-74f10aec321f", "Status Code");
			zTextBoxColumnStyleInfo5.ColumnName = "ScreeningStatus";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("345fee17-c8ab-40b5-aa40-e373f82f114e", "Screening Status");
			zTextBoxColumnStyleInfo6.ColumnName = "ScreeningStatusDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("2e894f15-9c9e-4199-88c8-d272554bc4d6", "Party Code");
			zTextBoxColumnStyleInfo7.ColumnName = "OrgCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("dc81bc5d-675b-4f3a-aebc-23078ee9daf0", "Party Name");
			zTextBoxColumnStyleInfo8.ColumnName = "Code";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("b27780cf-e774-4bbb-aca8-d5059bd9316e", "Description");
			zMultiLineTextBoxColumnInfo2.ColumnName = "ParentsDescription";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.UnprocessedWrappersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.UnprocessedWrappersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.UnprocessedWrappersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.UnprocessedWrappersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.UnprocessedWrappersGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.UnprocessedWrappersGrid.DisableImportDataMenuItem = true;
			this.UnprocessedWrappersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnprocessedWrappersGrid.GridId = "72c6b7ce-63de-4eec-8526-4856bad609f1";
			this.UnprocessedWrappersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnprocessedWrappersGrid.LayoutKey = "ProcessedWrappersGrid";
			this.UnprocessedWrappersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.UnprocessedWrappersGrid.Name = "UnprocessedWrappersGrid";
			this.UnprocessedWrappersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 116, true);
			this.UnprocessedWrappersGrid.TabIndex = 0;
			// 
			// ProcessedGroupBox
			// 
			this.ProcessedGroupBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("588a5839-6e0a-413c-9d26-362541f83968", "Processed");
			this.ProcessedGroupBox.Controls.Add(this.ProcessedWrappersGrid);
			this.ProcessedGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcessedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ProcessedGroupBox.Name = "ProcessedGroupBox";
			this.ProcessedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 133, true);
			this.ProcessedGroupBox.TabIndex = 7;
			this.ProcessedGroupBox.TabStop = false;
			// 
			// FlowLayoutPanel
			// 
			this.FlowLayoutPanel.Controls.Add(this.CloseButton);
			this.FlowLayoutPanel.Controls.Add(this.ForceRescreenButton);
			this.FlowLayoutPanel.Controls.Add(this.ScreenButton);
			this.FlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.FlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
			this.FlowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.FlowLayoutPanel.Name = "FlowLayoutPanel";
			this.FlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 30, true);
			this.FlowLayoutPanel.TabIndex = 9;
			// 
			// MessagePanel
			// 
			this.MessagePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MessagePanel.AutoScroll = true;
			this.MessagePanel.BackColor = System.Drawing.SystemColors.Window;
			this.MessagePanel.Controls.Add(this.MessageDetailLabel);
			this.MessagePanel.Controls.Add(this.MessageLabel);
			this.MessagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagePanel.Name = "MessagePanel";
			this.MessagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 40, true);
			this.MessagePanel.TabIndex = 7;
			// 
			// MessageDetailLabel
			// 
			this.MessageDetailLabel.AutoSize = true;
			this.MessageDetailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageDetailLabel.ForeColor = System.Drawing.Color.Red;
			this.MessageDetailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 13, true);
			this.MessageDetailLabel.Name = "MessageDetailLabel";
			this.MessageDetailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.MessageDetailLabel.TabIndex = 1;
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("e55ecd36-e7e0-4865-9290-0905fb44d8df", "The following records have been skipped because they are permanently clear");
			this.MessageLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.MessageLabel.ForeColor = System.Drawing.Color.Red;
			this.MessageLabel.IsFontBold = true;
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 14, true);
			this.MessageLabel.TabIndex = 0;
			// 
			// PartyComplianceForm
			// 
			this.AcceptButton = this.CloseButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("fe9d6d90-c83a-4ba3-a7af-dc54edd6126c", "Party Compliance");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 366, true);
			this.Controls.Add(this.MessagePanel);
			this.Controls.Add(this.MainPanel);
			this.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.PartyComplianceWrapperFilteredCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 404, true);
			this.Name = "PartyComplianceForm";
			this.Text = "PartyComplianceForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MessagePanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProcessedWrappersGrid)).EndInit();
			this.ProcessedWrappersGrid.ResumeLayout(false);
			this.ProcessedWrappersGrid.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.UnProcessedGroupBox.ResumeLayout(false);
			this.UnProcessedGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnprocessedWrappersGrid)).EndInit();
			this.UnprocessedWrappersGrid.ResumeLayout(false);
			this.UnprocessedWrappersGrid.PerformLayout();
			this.ProcessedGroupBox.ResumeLayout(false);
			this.ProcessedGroupBox.PerformLayout();
			this.FlowLayoutPanel.ResumeLayout(false);
			this.FlowLayoutPanel.PerformLayout();
			this.MessagePanel.ResumeLayout(false);
			this.MessagePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ForceRescreenButton;
		internal Enterprise.ZArchitecture.ZGrid ProcessedWrappersGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton ScreenButton;
		private CargoWise.Windows.UI.KTableLayoutPanel MainPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox UnProcessedGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ProcessedGroupBox;
		private CargoWise.Windows.UI.KFlowLayoutPanel FlowLayoutPanel;
		internal ZArchitecture.ZGrid UnprocessedWrappersGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel MessagePanel;
		private Enterprise.ZArchitecture.ZLabel MessageLabel;
		private Enterprise.ZArchitecture.ZLabel MessageDetailLabel;
	}
}
