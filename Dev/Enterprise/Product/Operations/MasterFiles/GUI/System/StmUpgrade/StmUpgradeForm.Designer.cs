namespace Enterprise.MasterFiles.GUI
{
	public partial class StmUpgradeForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.StmUpgradeTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.UpgradesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UpgradesTabGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportFromFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShowGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeletedOrObsoleteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NotAppliedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AppliedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReadyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SaveToDiskButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UpgradeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GridLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NoticeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AvailableUpgradesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpgradesTabNotDisplayedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CMRReferenceFilesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CMRReferenceFilesTabPageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WebGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UpdateFromWebFullButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IndustryTestingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UpdateFromWebMainTestingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FileGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UpdateFromFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			this.BrowseButtonForData = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DataFilePathTextBox = new CargoWise.Windows.UI.KTextBox();
			this.CMRUpgradesTabNotDisplayedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EventsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StmUpgradeTabControl.SuspendLayout();
			this.UpgradesTabPage.SuspendLayout();
			this.UpgradesTabGroupBox.SuspendLayout();
			this.ShowGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AvailableUpgradesGrid)).BeginInit();
			this.AvailableUpgradesGrid.SuspendLayout();
			this.CMRReferenceFilesTabPage.SuspendLayout();
			this.CMRReferenceFilesTabPageGroupBox.SuspendLayout();
			this.WebGroupBox.SuspendLayout();
			this.ProductionGroupBox.SuspendLayout();
			this.IndustryTestingGroupBox.SuspendLayout();
			this.FileGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 365, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(637);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(639);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.StmUpgradeCollectionContainer);
			// 
			// StmUpgradeTabControl
			// 
			this.StmUpgradeTabControl.Controls.Add(this.UpgradesTabPage);
			this.StmUpgradeTabControl.Controls.Add(this.CMRReferenceFilesTabPage);
			this.StmUpgradeTabControl.Controls.Add(this.EventsTabPage);
			this.StmUpgradeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StmUpgradeTabControl.Name = "StmUpgradeTabControl";
			this.StmUpgradeTabControl.SelectedIndex = 0;
			this.StmUpgradeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 362, true);
			this.StmUpgradeTabControl.TabIndex = 0;
			// 
			// UpgradesTabPage
			// 
			this.UpgradesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|2fe9d470-bc9d-48a1-92af-229ad7756bd5", "Upgrades");
			this.UpgradesTabPage.Controls.Add(this.UpgradesTabGroupBox);
			this.UpgradesTabPage.Controls.Add(this.UpgradesTabNotDisplayedLabel);
			this.UpgradesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.UpgradesTabPage.Name = "UpgradesTabPage";
			this.UpgradesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 341, true);
			this.UpgradesTabPage.TabIndex = 0;
			// 
			// UpgradesTabGroupBox
			// 
			this.UpgradesTabGroupBox.Controls.Add(this.ImportFromFileButton);
			this.UpgradesTabGroupBox.Controls.Add(this.ShowGroupBox);
			this.UpgradesTabGroupBox.Controls.Add(this.SaveToDiskButton);
			this.UpgradesTabGroupBox.Controls.Add(this.RedLabel);
			this.UpgradesTabGroupBox.Controls.Add(this.DeleteButton);
			this.UpgradesTabGroupBox.Controls.Add(this.InfoLabel);
			this.UpgradesTabGroupBox.Controls.Add(this.UpgradeButton);
			this.UpgradesTabGroupBox.Controls.Add(this.CloseButton);
			this.UpgradesTabGroupBox.Controls.Add(this.GridLabel);
			this.UpgradesTabGroupBox.Controls.Add(this.NoticeLabel);
			this.UpgradesTabGroupBox.Controls.Add(this.AvailableUpgradesGrid);
			this.UpgradesTabGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UpgradesTabGroupBox, false);
			this.UpgradesTabGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UpgradesTabGroupBox.Name = "UpgradesTabGroupBox";
			this.UpgradesTabGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 341, true);
			this.UpgradesTabGroupBox.TabIndex = 1;
			this.UpgradesTabGroupBox.TabStop = false;
			// 
			// ImportFromFileButton
			// 
			this.ImportFromFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportFromFileButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|fd75a3fa-940c-4b23-b585-5cc3584bcd3a", "Import From File");
			this.ImportFromFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 315, true);
			this.ImportFromFileButton.Name = "ImportFromFileButton";
			this.ImportFromFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 22, true);
			this.ImportFromFileButton.TabIndex = 18;
			this.ImportFromFileButton.Click += new System.EventHandler(this.ImportFromFileButton_Click);
			// 
			// ShowGroupBox
			// 
			this.ShowGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|89f19d18-67c2-43e9-85f3-c615af221f77", "Show");
			this.ShowGroupBox.Controls.Add(this.DeletedOrObsoleteCheckBox);
			this.ShowGroupBox.Controls.Add(this.NotAppliedCheckBox);
			this.ShowGroupBox.Controls.Add(this.AppliedCheckBox);
			this.ShowGroupBox.Controls.Add(this.ReadyCheckBox);
			this.ShowGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 5, true);
			this.ShowGroupBox.Name = "ShowGroupBox";
			this.ShowGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 45, true);
			this.ShowGroupBox.TabIndex = 11;
			this.ShowGroupBox.TabStop = false;
			// 
			// DeletedOrObsoleteCheckBox
			// 
			this.DeletedOrObsoleteCheckBox.AutoSize = true;
			this.DeletedOrObsoleteCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|97efb15d-f979-4454-ac89-2da6ee75ab3b", "Deleted or Obsolete");
			this.DeletedOrObsoleteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeletedOrObsoleteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 19, true);
			this.DeletedOrObsoleteCheckBox.Name = "DeletedOrObsoleteCheckBox";
			this.DeletedOrObsoleteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.DeletedOrObsoleteCheckBox.TabIndex = 3;
			this.DeletedOrObsoleteCheckBox.CheckedChanged += new System.EventHandler(this.DeletedOrObsoleteCheckBox_CheckedChanged);
			// 
			// NotAppliedCheckBox
			// 
			this.NotAppliedCheckBox.AutoSize = true;
			this.NotAppliedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|58e855f9-1043-493f-99ee-c84b0664eec4", "Not Applied");
			this.NotAppliedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NotAppliedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 19, true);
			this.NotAppliedCheckBox.Name = "NotAppliedCheckBox";
			this.NotAppliedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.NotAppliedCheckBox.TabIndex = 2;
			this.NotAppliedCheckBox.CheckedChanged += new System.EventHandler(this.NotAppliedCheckBox_CheckedChanged);
			// 
			// AppliedCheckBox
			// 
			this.AppliedCheckBox.AutoSize = true;
			this.AppliedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|8ee6c346-56e0-400b-93d7-d66812f8d7cc", "Applied");
			this.AppliedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AppliedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 19, true);
			this.AppliedCheckBox.Name = "AppliedCheckBox";
			this.AppliedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.AppliedCheckBox.TabIndex = 1;
			this.AppliedCheckBox.CheckedChanged += new System.EventHandler(this.AppliedCheckBox_CheckedChanged);
			// 
			// ReadyCheckBox
			// 
			this.ReadyCheckBox.AutoSize = true;
			this.ReadyCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.ReadyCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|c76b583b-6b88-44bd-a66d-a4f739e93a31", "Ready");
			this.ReadyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReadyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ReadyCheckBox.Name = "ReadyCheckBox";
			this.ReadyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ReadyCheckBox.TabIndex = 0;
			this.ReadyCheckBox.UseVisualStyleBackColor = false;
			this.ReadyCheckBox.CheckedChanged += new System.EventHandler(this.ReadyCheckBox_CheckedChanged);
			// 
			// SaveToDiskButton
			// 
			this.SaveToDiskButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveToDiskButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|36d6ddcd-386b-4072-9173-b3cdb0cc4bc0", "Save To Disk");
			this.SaveToDiskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 315, true);
			this.SaveToDiskButton.Name = "SaveToDiskButton";
			this.SaveToDiskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 22, true);
			this.SaveToDiskButton.TabIndex = 19;
			this.SaveToDiskButton.Click += new System.EventHandler(this.SaveToDiskButton_Click);
			// 
			// RedLabel
			// 
			this.RedLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.RedLabel.BackColor = System.Drawing.Color.Crimson;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RedLabel, false);
			this.RedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 317, true);
			this.RedLabel.Name = "RedLabel";
			this.RedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.RedLabel.TabIndex = 17;
			// 
			// DeleteButton
			// 
			this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeleteButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|d674ee4c-5a33-4397-b57b-9b9c24ddb86b", "Delete");
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 315, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.DeleteButton.TabIndex = 15;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// InfoLabel
			// 
			this.InfoLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.InfoLabel.AutoSize = true;
			this.InfoLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|10d01f8f-6782-4289-83b1-014ffa36254c", "The current version is highlighted in");
			this.InfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 317, true);
			this.InfoLabel.Name = "InfoLabel";
			this.InfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 13, true);
			this.InfoLabel.TabIndex = 16;
			// 
			// UpgradeButton
			// 
			this.UpgradeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpgradeButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|6b4336d1-5962-4248-ac37-1fc6a846b1df", "Upgrade");
			this.UpgradeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 315, true);
			this.UpgradeButton.Name = "UpgradeButton";
			this.UpgradeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 22, true);
			this.UpgradeButton.TabIndex = 20;
			this.UpgradeButton.Click += new System.EventHandler(this.UpgradeButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|73a2fca8-6cb8-4344-862e-b87db39402cc", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(799, 315, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CloseButton.TabIndex = 21;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// GridLabel
			// 
			this.GridLabel.AutoSize = true;
			this.GridLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|e86990fb-7a21-4512-8d70-3ec2bf309117", "Select A Version To Upgrade or Downgrade");
			this.GridLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 62, true);
			this.GridLabel.Name = "GridLabel";
			this.GridLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 13, true);
			this.GridLabel.TabIndex = 12;
			// 
			// NoticeLabel
			// 
			this.NoticeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.NoticeLabel.AutoSize = true;
			this.NoticeLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|c278ff44-9b1e-400c-87e5-86d426f75cd7", "Upgrades are cumulative and contain all changes from previous versions");
			this.NoticeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 62, true);
			this.NoticeLabel.Name = "NoticeLabel";
			this.NoticeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 13, true);
			this.NoticeLabel.TabIndex = 13;
			// 
			// AvailableUpgradesGrid
			// 
			this.AvailableUpgradesGrid.AllowNavigation = false;
			this.AvailableUpgradesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AvailableUpgradesGrid, "FullUpgradesView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.StmUpgradeCollectionContainer)(null)).FullUpgradesView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.StmUpgrade)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.StmUpgradeCollectionContainer)(null)).FullUpgradesView)).SyncRoot)).SZ_ExeVersionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.StmUpgrade)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.StmUpgradeCollectionContainer)(null)).FullUpgradesView)).SyncRoot)).FullVersionString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.StmUpgrade)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.StmUpgradeCollectionContainer)(null)).FullUpgradesView)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.StmUpgrade)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.StmUpgradeCollectionContainer)(null)).FullUpgradesView)).SyncRoot)).SZ_StatusTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.StmUpgrade)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.StmUpgradeCollectionContainer)(null)).FullUpgradesView)).SyncRoot)).SZ_StatusComment)));
			this.AvailableUpgradesGrid.CaptionText = "Select a Version Below To Upgrade/Downgrade";
			this.AvailableUpgradesGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "SZ_ExeVersionDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|5fceaf0a-59fd-4426-873c-b2bbad4342a4", "Version");
			zTextBoxColumnStyleInfo1.ColumnName = "FullVersionString";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|d30e03c8-7e33-4cc7-98f5-35e50801190d", "Status");
			zTextBoxColumnStyleInfo2.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "SZ_StatusTime";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.ColumnName = "SZ_StatusComment";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.AvailableUpgradesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.AvailableUpgradesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AvailableUpgradesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AvailableUpgradesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.AvailableUpgradesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AvailableUpgradesGrid.GridId = "e676eb13-ccb6-4358-b445-e578ad8aaef5";
			this.AvailableUpgradesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AvailableUpgradesGrid.IsWholeRowSelectedOnClick = true;
			this.AvailableUpgradesGrid.LayoutKey = "AvailableUpgradesGrid";
			this.AvailableUpgradesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 80, true);
			this.AvailableUpgradesGrid.Name = "AvailableUpgradesGrid";
			this.AvailableUpgradesGrid.ReadOnly = true;
			this.AvailableUpgradesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 229, true);
			this.AvailableUpgradesGrid.TabIndex = 14;
			this.AvailableUpgradesGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.AvailableUpgradesGrid_ColourDeciding);
			// 
			// UpgradesTabNotDisplayedLabel
			// 
			this.UpgradesTabNotDisplayedLabel.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.UpgradesTabNotDisplayedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 111, true);
			this.UpgradesTabNotDisplayedLabel.Name = "UpgradesTabNotDisplayedLabel";
			this.UpgradesTabNotDisplayedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 113, true);
			this.UpgradesTabNotDisplayedLabel.TabIndex = 0;
			// 
			// CMRReferenceFilesTabPage
			// 
			this.CMRReferenceFilesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|3a456cca-263c-4d3c-a8b6-52917c8d010a", "CMR Reference Files Updates");
			this.CMRReferenceFilesTabPage.Controls.Add(this.CMRReferenceFilesTabPageGroupBox);
			this.CMRReferenceFilesTabPage.Controls.Add(this.CMRUpgradesTabNotDisplayedLabel);
			this.CMRReferenceFilesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CMRReferenceFilesTabPage.Name = "CMRReferenceFilesTabPage";
			this.CMRReferenceFilesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 341, true);
			this.CMRReferenceFilesTabPage.TabIndex = 3;
			// 
			// CMRReferenceFilesTabPageGroupBox
			// 
			this.CMRReferenceFilesTabPageGroupBox.Controls.Add(this.WebGroupBox);
			this.CMRReferenceFilesTabPageGroupBox.Controls.Add(this.FileGroupBox);
			this.CMRReferenceFilesTabPageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CMRReferenceFilesTabPageGroupBox, false);
			this.CMRReferenceFilesTabPageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CMRReferenceFilesTabPageGroupBox.Name = "CMRReferenceFilesTabPageGroupBox";
			this.CMRReferenceFilesTabPageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(878, 338, true);
			this.CMRReferenceFilesTabPageGroupBox.TabIndex = 0;
			this.CMRReferenceFilesTabPageGroupBox.TabStop = false;
			// 
			// WebGroupBox
			// 
			this.WebGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|d9408c17-a9ae-4665-be82-8b5154cce038", "Manual Update From Web");
			this.WebGroupBox.Controls.Add(this.ProductionGroupBox);
			this.WebGroupBox.Controls.Add(this.IndustryTestingGroupBox);
			this.WebGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.WebGroupBox.Name = "WebGroupBox";
			this.WebGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 81, true);
			this.WebGroupBox.TabIndex = 3;
			this.WebGroupBox.TabStop = false;
			// 
			// ProductionGroupBox
			// 
			this.ProductionGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|f9f0a468-0f15-4688-b384-31020e5950f3", "Production");
			this.ProductionGroupBox.Controls.Add(this.UpdateFromWebFullButton);
			this.ProductionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ProductionGroupBox.Name = "ProductionGroupBox";
			this.ProductionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 45, true);
			this.ProductionGroupBox.TabIndex = 0;
			this.ProductionGroupBox.TabStop = false;
			// 
			// UpdateFromWebFullButton
			// 
			this.UpdateFromWebFullButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|e4dedca0-8c28-4112-b4e4-a67f92121452", "Full Update");
			this.UpdateFromWebFullButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.UpdateFromWebFullButton.Name = "UpdateFromWebFullButton";
			this.UpdateFromWebFullButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 21, true);
			this.UpdateFromWebFullButton.TabIndex = 0;
			this.UpdateFromWebFullButton.Click += new System.EventHandler(this.UpdateFromWebFullButton_Click);
			// 
			// IndustryTestingGroupBox
			// 
			this.IndustryTestingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|7a8441d7-5783-49ed-98a5-26868a29d60c", "Industry Testing");
			this.IndustryTestingGroupBox.Controls.Add(this.UpdateFromWebMainTestingButton);
			this.IndustryTestingGroupBox.ForeColor = System.Drawing.Color.Red;
			this.IndustryTestingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 19, true);
			this.IndustryTestingGroupBox.Name = "IndustryTestingGroupBox";
			this.IndustryTestingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 45, true);
			this.IndustryTestingGroupBox.TabIndex = 1;
			this.IndustryTestingGroupBox.TabStop = false;
			// 
			// UpdateFromWebMainTestingButton
			// 
			this.UpdateFromWebMainTestingButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|1b83d587-aba2-401a-b838-857ea972153e", "Full Update");
			this.UpdateFromWebMainTestingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.UpdateFromWebMainTestingButton.Name = "UpdateFromWebMainTestingButton";
			this.UpdateFromWebMainTestingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 21, true);
			this.UpdateFromWebMainTestingButton.TabIndex = 0;
			this.UpdateFromWebMainTestingButton.Click += new System.EventHandler(this.UpdateFromWebMainTestingButton_Click);
			// 
			// FileGroupBox
			// 
			this.FileGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|eba721ee-965d-45d1-8247-b01822d1cf74", "Manual Update From File");
			this.FileGroupBox.Controls.Add(this.UpdateFromFileButton);
			this.FileGroupBox.Controls.Add(this.label1);
			this.FileGroupBox.Controls.Add(this.BrowseButtonForData);
			this.FileGroupBox.Controls.Add(this.DataFilePathTextBox);
			this.FileGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 106, true);
			this.FileGroupBox.Name = "FileGroupBox";
			this.FileGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 101, true);
			this.FileGroupBox.TabIndex = 2;
			this.FileGroupBox.TabStop = false;
			// 
			// UpdateFromFileButton
			// 
			this.UpdateFromFileButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|79f5716e-a544-4e9c-a98b-73711a6454a9", "Update From File");
			this.UpdateFromFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 68, true);
			this.UpdateFromFileButton.Name = "UpdateFromFileButton";
			this.UpdateFromFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 21, true);
			this.UpdateFromFileButton.TabIndex = 3;
			this.UpdateFromFileButton.Click += new System.EventHandler(this.UpdateFromFileButton_Click);
			// 
			// label1
			// 
			this.label1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|bf4fd901-6f7c-489a-a67e-2aa768dc0b85", "Location of Update File");
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 15, true);
			this.label1.TabIndex = 0;
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// BrowseButtonForData
			// 
			this.BrowseButtonForData.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|74f1fd31-bb70-4bce-9ef1-f5fbfef9879c", "Browse...");
			this.BrowseButtonForData.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 39, true);
			this.BrowseButtonForData.Name = "BrowseButtonForData";
			this.BrowseButtonForData.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.BrowseButtonForData.TabIndex = 2;
			this.BrowseButtonForData.Click += new System.EventHandler(this.BrowseButtonForData_Click);
			// 
			// DataFilePathTextBox
			// 
			this.DataFilePathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 39, true);
			this.DataFilePathTextBox.Name = "DataFilePathTextBox";
			this.DataFilePathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 18, true);
			this.DataFilePathTextBox.TabIndex = 1;
			// 
			// CMRUpgradesTabNotDisplayedLabel
			// 
			this.CMRUpgradesTabNotDisplayedLabel.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.CMRUpgradesTabNotDisplayedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 111, true);
			this.CMRUpgradesTabNotDisplayedLabel.Name = "CMRUpgradesTabNotDisplayedLabel";
			this.CMRUpgradesTabNotDisplayedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 113, true);
			this.CMRUpgradesTabNotDisplayedLabel.TabIndex = 7;
			// 
			// EventsTabPage
			// 
			this.EventsTabPage.ExcludeFromBindingOnSave = true;
			this.EventsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.EventsTabPage.Name = "EventsTabPage";
			this.EventsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.EventsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 341, true);
			this.EventsTabPage.TabIndex = 2;
			// 
			// StmUpgradeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StmUpgradeForm|b70aed1a-1431-403e-a71b-3311efcb99af", "Browse Available Versions For Upgrade or Downgrade");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 389, true);
			this.Controls.Add(this.StmUpgradeTabControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.StmUpgradeCollectionContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 386, true);
			this.Name = "StmUpgradeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Click += new System.EventHandler(this.ImportFromFileButton_Click);
			this.Controls.SetChildIndex(this.StmUpgradeTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StmUpgradeTabControl.ResumeLayout(false);
			this.StmUpgradeTabControl.PerformLayout();
			this.UpgradesTabPage.ResumeLayout(false);
			this.UpgradesTabPage.PerformLayout();
			this.UpgradesTabGroupBox.ResumeLayout(false);
			this.UpgradesTabGroupBox.PerformLayout();
			this.ShowGroupBox.ResumeLayout(false);
			this.ShowGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AvailableUpgradesGrid)).EndInit();
			this.AvailableUpgradesGrid.ResumeLayout(false);
			this.AvailableUpgradesGrid.PerformLayout();
			this.CMRReferenceFilesTabPage.ResumeLayout(false);
			this.CMRReferenceFilesTabPage.PerformLayout();
			this.CMRReferenceFilesTabPageGroupBox.ResumeLayout(false);
			this.CMRReferenceFilesTabPageGroupBox.PerformLayout();
			this.WebGroupBox.ResumeLayout(false);
			this.WebGroupBox.PerformLayout();
			this.ProductionGroupBox.ResumeLayout(false);
			this.ProductionGroupBox.PerformLayout();
			this.IndustryTestingGroupBox.ResumeLayout(false);
			this.IndustryTestingGroupBox.PerformLayout();
			this.FileGroupBox.ResumeLayout(false);
			this.FileGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl StmUpgradeTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage UpgradesTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage EventsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage CMRReferenceFilesTabPage;
		protected Enterprise.ZArchitecture.ZLabel UpgradesTabNotDisplayedLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox UpgradesTabGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZButton ImportFromFileButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ShowGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox DeletedOrObsoleteCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox NotAppliedCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox AppliedCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox ReadyCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZButton SaveToDiskButton;
		private Enterprise.ZArchitecture.ZLabel RedLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton DeleteButton;
		private Enterprise.ZArchitecture.ZLabel InfoLabel;
		internal Enterprise.ZArchitecture.GUI.ZButton UpgradeButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.ZLabel GridLabel;
		private Enterprise.ZArchitecture.ZLabel NoticeLabel;
		protected Enterprise.ZArchitecture.ZGrid AvailableUpgradesGrid;
		protected Enterprise.ZArchitecture.ZLabel CMRUpgradesTabNotDisplayedLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox CMRReferenceFilesTabPageGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox WebGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ProductionGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZButton UpdateFromWebFullButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox IndustryTestingGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZButton UpdateFromWebMainTestingButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox FileGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZButton UpdateFromFileButton;
		private Enterprise.ZArchitecture.ZLabel label1;
		private Enterprise.ZArchitecture.GUI.ZButton BrowseButtonForData;
		protected internal CargoWise.Windows.UI.KTextBox DataFilePathTextBox;
	}
}
