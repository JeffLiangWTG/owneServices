namespace Enterprise.Freight.Agency.GUI
{
	partial class NZPortMessageDialog
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NZPortMessageDialog));
			this.tableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.statusPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.issuesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.logTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.panel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.sendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.messageDetailsGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.principalBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.ctoFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.portBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.messageTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.directionBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.warningText = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.statusPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.issuesGrid)).BeginInit();
			this.issuesGrid.SuspendLayout();
			this.panel1.SuspendLayout();
			this.messageDetailsGroup.SuspendLayout();
			this.principalBoundDropEdit.SuspendLayout();
			this.ctoFindBox.SuspendLayout();
			this.portBoundDropEdit.SuspendLayout();
			this.messageTypeBoundDropEdit.SuspendLayout();
			this.directionBoundDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 520, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.NZPortMessage);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.statusPanel, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.messageDetailsGroup, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.warningText, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.tableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 544, true);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// statusPanel
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.statusPanel, 2);
			this.statusPanel.Controls.Add(this.issuesGrid);
			this.statusPanel.Controls.Add(this.logTextBox);
			this.statusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 191, true);
			this.statusPanel.Name = "statusPanel";
			this.statusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 330, true);
			this.statusPanel.TabIndex = 6;
			// 
			// issuesGrid
			// 
			this.issuesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.issuesGrid, "Issues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.NZPortMessage)(null)).Issues)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortMessageIssue)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.NZPortMessage)(null)).Issues)).SyncRoot)).Text)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortMessageIssue)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.NZPortMessage)(null)).Issues)).SyncRoot)).Detail)));
			this.issuesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortMessageDialog|12785027-65a7-477a-8b48-ed3d2b2c9130", "Errors");
			zTextBoxColumnStyleInfo1.ColumnName = "Text";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(390);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthorityFilterControl|Detail", "Detail");
			zMultiLineTextBoxColumnInfo1.ColumnName = "Detail";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.issuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.issuesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.issuesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.issuesGrid.GridId = "79923e71-7dee-4dc4-be17-88b58c01e407";
			this.issuesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.issuesGrid.LayoutKey = "issuesGrid";
			this.issuesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.issuesGrid.Name = "issuesGrid";
			this.issuesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.issuesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 330, true);
			this.issuesGrid.TabIndex = 3;
			// 
			// logTextBox
			// 
			this.logTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("logTextBox.CaptionResourceString")));
			this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.logTextBox.Multiline = true;
			this.logTextBox.Name = "logTextBox";
			this.logTextBox.ReadOnly = true;
			this.logTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 330, true);
			this.logTextBox.TabIndex = 4;
			this.logTextBox.Visible = false;
			// 
			// panel1
			// 
			this.panel1.AutoSize = true;
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Controls.Add(this.sendButton);
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 35, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 54, true);
			this.panel1.TabIndex = 4;
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortMessageForm|5ed6173e-899c-48cd-8bf3-f748d1ab89b6", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 9;
			this.cancelButton.ToolTipCaption = null;
			// 
			// sendButton
			// 
			this.sendButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortMessageForm|66628f6a-5e47-444f-a4a6-f821e4f1e15c", "Send");
			this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.sendButton.Name = "sendButton";
			this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.sendButton.TabIndex = 8;
			this.sendButton.ToolTipCaption = null;
			this.sendButton.Click += new System.EventHandler(this.SendButtonClicked);
			// 
			// messageDetailsGroup
			// 
			this.messageDetailsGroup.AutoSize = true;
			this.messageDetailsGroup.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortMessageForm|88ef1f34-afe6-11e4-a025-902b34dc814a", "Message details");
			this.messageDetailsGroup.Controls.Add(this.principalBoundDropEdit);
			this.messageDetailsGroup.Controls.Add(this.ctoFindBox);
			this.messageDetailsGroup.Controls.Add(this.portBoundDropEdit);
			this.messageDetailsGroup.Controls.Add(this.messageTypeBoundDropEdit);
			this.messageDetailsGroup.Controls.Add(this.directionBoundDropEdit);
			this.messageDetailsGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 35, true);
			this.messageDetailsGroup.Name = "messageDetailsGroup";
			this.messageDetailsGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 151, true);
			this.messageDetailsGroup.TabIndex = 5;
			this.messageDetailsGroup.TabStop = false;
			// 
			// principalBoundDropEdit
			// 
			this.principalBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.principalBoundDropEdit, "PrincipalPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.NZPortMessage)(null)).PrincipalPK)));
			this.principalBoundDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("0ec60d62-129f-4965-9284-7aada119534e", "Principal");
			this.principalBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 42, true);
			this.principalBoundDropEdit.Name = "principalBoundDropEdit";
			this.principalBoundDropEdit.PreBoundMaxLength = 12;
			this.principalBoundDropEdit.ShowDescriptionBox = false;
			this.principalBoundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.principalBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 18, true);
			this.principalBoundDropEdit.TabIndex = 4;
			// 
			// ctoFindBox
			// 
			this.ctoFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ctoFindBox, "CTO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.NZPortMessage)(null)).CTO)));
			this.ctoFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 117, true);
			this.ctoFindBox.Name = "ctoFindBox";
			this.ctoFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ctoFindBox.ParentType = null;
			this.ctoFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.ctoFindBox.TabIndex = 7;
			// 
			// portBoundDropEdit
			// 
			this.portBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.portBoundDropEdit, "Port");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.NZPortMessage)(null)).Port)));
			this.portBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 19, true);
			this.portBoundDropEdit.Name = "portBoundDropEdit";
			this.portBoundDropEdit.PreBoundMaxLength = 5;
			this.portBoundDropEdit.ShowDescriptionBox = false;
			this.portBoundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.portBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 18, true);
			this.portBoundDropEdit.TabIndex = 3;
			// 
			// messageTypeBoundDropEdit
			// 
			this.messageTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messageTypeBoundDropEdit, "MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.NZPortMessage)(null)).MessageType)));
			this.messageTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 90, true);
			this.messageTypeBoundDropEdit.Name = "messageTypeBoundDropEdit";
			this.messageTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.messageTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 18, true);
			this.messageTypeBoundDropEdit.TabIndex = 6;
			// 
			// directionBoundDropEdit
			// 
			this.directionBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.directionBoundDropEdit, "Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.NZPortMessage)(null)).Direction)));
			this.directionBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 65, true);
			this.directionBoundDropEdit.Name = "directionBoundDropEdit";
			this.directionBoundDropEdit.PreBoundMaxLength = 8;
			this.directionBoundDropEdit.ShowDescriptionBox = false;
			this.directionBoundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.directionBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 18, true);
			this.directionBoundDropEdit.TabIndex = 5;
			// 
			// warningText
			// 
			this.warningText.AutoSize = true;
			this.warningText.Dock = System.Windows.Forms.DockStyle.Top;
			this.warningText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.warningText.ForeColor = System.Drawing.Color.Red;
			this.warningText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.warningText.Name = "warningText";
			this.warningText.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 10, true);
			this.warningText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 33, true);
			this.warningText.TabIndex = 7;
			this.warningText.Text = "Some warning ";
			this.warningText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// NZPortMessageDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("67a15146-b75e-4292-8437-7e8fa86505da", "Load and Discharge Manifest Message");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 544, true);
			this.Controls.Add(this.tableLayoutPanel1);
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.NZPortMessage);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 400, true);
			this.Name = "NZPortMessageDialog";
			this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.statusPanel.ResumeLayout(false);
			this.statusPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.issuesGrid)).EndInit();
			this.issuesGrid.ResumeLayout(false);
			this.issuesGrid.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.messageDetailsGroup.ResumeLayout(false);
			this.messageDetailsGroup.PerformLayout();
			this.principalBoundDropEdit.ResumeLayout(true);
			this.principalBoundDropEdit.PerformLayout();
			this.ctoFindBox.ResumeLayout(true);
			this.ctoFindBox.PerformLayout();
			this.portBoundDropEdit.ResumeLayout(true);
			this.portBoundDropEdit.PerformLayout();
			this.messageTypeBoundDropEdit.ResumeLayout(true);
			this.messageTypeBoundDropEdit.PerformLayout();
			this.directionBoundDropEdit.ResumeLayout(true);
			this.directionBoundDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel1;
		ZArchitecture.ZGrid issuesGrid;
		ZArchitecture.GUI.ZPanel panel1;
		ZArchitecture.GUI.ZButton cancelButton;
		protected ZArchitecture.GUI.ZButton sendButton;
		ZArchitecture.GUI.ZGroupBox messageDetailsGroup;
		Enterprise.ZArchitecture.GUI.ZDropEdit portBoundDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit messageTypeBoundDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit directionBoundDropEdit;
		ZArchitecture.GUI.ZPanel statusPanel;
		ZArchitecture.ZLabel warningText;
		ZArchitecture.GUI.ZGuidFindBox ctoFindBox;
		ZArchitecture.ZTextBox logTextBox;
		private ZArchitecture.GUI.ZGuidDropEdit principalBoundDropEdit;
	}
}
