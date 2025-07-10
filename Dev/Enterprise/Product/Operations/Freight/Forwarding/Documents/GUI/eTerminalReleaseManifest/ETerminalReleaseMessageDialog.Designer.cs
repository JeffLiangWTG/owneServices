namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	partial class ETerminalReleaseMessageDialog
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.ConsolGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.messageConsolsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LogGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProgressLog = new Enterprise.Freight.Forwarding.Documents.GUI.ProgressLog();
			this.tableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.statusPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.sendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.messageDetailsGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.loadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.ConsolGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.messageConsolsGrid)).BeginInit();
			this.messageConsolsGrid.SuspendLayout();
			this.LogGroupBox.SuspendLayout();
			this.ProgressLog.SuspendLayout();
			this.tableLayoutPanel.SuspendLayout();
			this.statusPanel.SuspendLayout();
			this.panel.SuspendLayout();
			this.messageDetailsGroup.SuspendLayout();
			this.loadPortCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 750, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1087, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessage);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 80, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ConsolGroupBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.LogGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1083, 687, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(311);
			this.splitContainer1.TabIndex = 6;
			// 
			// ConsolGroupBox
			// 
			this.ConsolGroupBox.AutoSize = true;
			this.ConsolGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("e1f1b853-0db8-4257-a318-d7a950ba33db", "Consols");
			this.ConsolGroupBox.Controls.Add(this.messageConsolsGrid);
			this.ConsolGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolGroupBox.Name = "ConsolGroupBox";
			this.ConsolGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1083, 311, true);
			this.ConsolGroupBox.TabIndex = 5;
			this.ConsolGroupBox.TabStop = false;
			// 
			// messageConsolsGrid
			// 
			this.messageConsolsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.messageConsolsGrid, "MessageConsols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessage)(null)).MessageConsols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessageConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessage)(null)).MessageConsols)).SyncRoot)).Consol.JK_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessageConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessage)(null)).MessageConsols)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessageConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessage)(null)).MessageConsols)).SyncRoot)).Send)));
			this.messageConsolsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("739fe6d7-6ce0-4fa8-9543-22bb8f27b969", "Consol");
			zTextBoxColumnStyleInfo1.ColumnName = "Consol+JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("c303b740-cae9-472e-b261-102bab7c7327", "Status");
			zTextBoxColumnStyleInfo2.ColumnName = "Status";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("6e0a7720-f4c9-4c28-9419-2d086780f002", "Select");
			zCheckBoxColumnStyleInfo1.ColumnName = "Send";
			zCheckBoxColumnStyleInfo1.IsCustomColumn = false;
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.messageConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.messageConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.messageConsolsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.messageConsolsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageConsolsGrid.GridId = "40f1b0f2-901e-416a-9747-2608ef5b5b16";
			this.messageConsolsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.messageConsolsGrid.LayoutKey = "messageConsolsGrid";
			this.messageConsolsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.messageConsolsGrid.Name = "messageConsolsGrid";
			this.messageConsolsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.messageConsolsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 294, true);
			this.messageConsolsGrid.TabIndex = 4;
			this.messageConsolsGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MessagingConsolsDoubleClick);
			// 
			// LogGroupBox
			// 
			this.LogGroupBox.AutoSize = true;
			this.LogGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("572d6893-783f-40b4-a9f0-6b2e64937f3d", "Sending Log");
			this.LogGroupBox.Controls.Add(this.ProgressLog);
			this.LogGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LogGroupBox.Name = "LogGroupBox";
			this.LogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1083, 374, true);
			this.LogGroupBox.TabIndex = 6;
			this.LogGroupBox.TabStop = false;
			// 
			// ProgressLog
			// 
			this.ProgressLog.AllowDrop = true;
			this.ProgressLog.CaptionRenderingEnabled = true;
			this.ProgressLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressLog.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ProgressLog.Name = "ProgressLog";
			this.ProgressLog.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProgressLog.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 357, true);
			this.ProgressLog.TabIndex = 5;
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.ColumnCount = 2;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel.Controls.Add(this.statusPanel, 0, 2);
			this.tableLayoutPanel.Controls.Add(this.panel, 1, 1);
			this.tableLayoutPanel.Controls.Add(this.messageDetailsGroup, 0, 1);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 4;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.tableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1087, 774, true);
			this.tableLayoutPanel.TabIndex = 1;
			// 
			// statusPanel
			// 
			this.tableLayoutPanel.SetColumnSpan(this.statusPanel, 2);
			this.statusPanel.Controls.Add(this.splitContainer1);
			this.statusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 65, true);
			this.statusPanel.Name = "statusPanel";
			this.statusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1083, 687, true);
			this.statusPanel.TabIndex = 0;
			// 
			// panel
			// 
			this.panel.AutoSize = true;
			this.panel.Controls.Add(this.cancelButton);
			this.panel.Controls.Add(this.sendButton);
			this.panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1005, 2, true);
			this.panel.Name = "panel";
			this.panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 53, true);
			this.panel.TabIndex = 1;
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("CNTerminalReleaseMessageDialog|2cea3cd6-e677-49a2-bb49-0281b2c8b238", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			// 
			// sendButton
			// 
			this.sendButton.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("CNTerminalReleaseMessageDialog|379519ee-b5ef-4450-8dbb-f6ad5470fae9", "Send");
			this.sendButton.IsCaptionOverridden = false;
			this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.sendButton.Name = "sendButton";
			this.sendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.sendButton.TabIndex = 2;
			this.sendButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.sendButton.ToolTipCaption = null;
			this.sendButton.Click += new System.EventHandler(this.SendButtonClicked);
			// 
			// messageDetailsGroup
			// 
			this.messageDetailsGroup.AutoSize = true;
			this.messageDetailsGroup.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("CNTerminalReleaseMessageDialog|19d7d8af-b97e-4944-b6d7-aa4fa74b1d3b", "Message Details");
			this.messageDetailsGroup.Controls.Add(this.loadPortCodeFindBox);
			this.messageDetailsGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.messageDetailsGroup.Name = "messageDetailsGroup";
			this.messageDetailsGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 59, true);
			this.messageDetailsGroup.TabIndex = 1;
			this.messageDetailsGroup.TabStop = false;
			// 
			// loadPortCodeFindBox
			// 
			this.loadPortCodeFindBox.AllowDrop = true;
			this.loadPortCodeFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.loadPortCodeFindBox, "MessageConsols.LoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessageConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessage)(null)).MessageConsols)).SyncRoot)).LoadPort)));
			this.loadPortCodeFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("78437a34-7f6b-4b09-bf26-ca2b2318771b", "Load Port");
			this.loadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 25, true);
			this.loadPortCodeFindBox.Name = "loadPortCodeFindBox";
			this.loadPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.loadPortCodeFindBox.ParentType = null;
			this.loadPortCodeFindBox.PreBoundMaxLength = 5;
			this.loadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.loadPortCodeFindBox.TabIndex = 0;
			// 
			// ETerminalReleaseMessageDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("99f6fe82-1507-49c2-8810-8ae5b96e9e83", "Terminal Release Messaging");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1087, 774, true);
			this.Controls.Add(this.tableLayoutPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN.ETerminalReleaseMessage);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 400, true);
			this.Name = "ETerminalReleaseMessageDialog";
			this.Controls.SetChildIndex(this.tableLayoutPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.ConsolGroupBox.ResumeLayout(false);
			this.ConsolGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.messageConsolsGrid)).EndInit();
			this.messageConsolsGrid.ResumeLayout(false);
			this.messageConsolsGrid.PerformLayout();
			this.LogGroupBox.ResumeLayout(false);
			this.LogGroupBox.PerformLayout();
			this.ProgressLog.ResumeLayout(true);
			this.ProgressLog.PerformLayout();
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.statusPanel.ResumeLayout(false);
			this.statusPanel.PerformLayout();
			this.panel.ResumeLayout(false);
			this.panel.PerformLayout();
			this.messageDetailsGroup.ResumeLayout(false);
			this.messageDetailsGroup.PerformLayout();
			this.loadPortCodeFindBox.ResumeLayout(true);
			this.loadPortCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel;
		ZArchitecture.ZGrid messageConsolsGrid;
		ZArchitecture.GUI.ZPanel panel;
		ZArchitecture.GUI.ZButton cancelButton;
		protected ZArchitecture.GUI.ZButton sendButton;
		ZArchitecture.GUI.ZGroupBox messageDetailsGroup;
		ZArchitecture.GUI.ZPanel statusPanel;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox loadPortCodeFindBox;
		ProgressLog ProgressLog;
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.GUI.ZGroupBox ConsolGroupBox;
		private ZArchitecture.GUI.ZGroupBox LogGroupBox;
	}
}
