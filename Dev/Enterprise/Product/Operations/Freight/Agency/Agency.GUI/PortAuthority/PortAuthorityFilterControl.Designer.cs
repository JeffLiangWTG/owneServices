namespace Enterprise.Freight.Agency.GUI
{
	partial class PortAuthorityFilterControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.principalDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.thirdPartyPanel = new CargoWise.Windows.UI.KPanel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.sendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.progressPanel = new CargoWise.Windows.UI.KPanel();
			this.stateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.issuesGrid = new Enterprise.ZArchitecture.ZGrid();
			portBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			messageTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			directionBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			versionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			emailAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			senderIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			recipientIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			deliverToThirdPartyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.topPanel.SuspendLayout();
			this.thirdPartyPanel.SuspendLayout();
			this.progressPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.issuesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.PortAuthority);
			// 
			// portBoundDropEdit
			// 
			portBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(portBoundDropEdit, "Port");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).Port)));
			portBoundDropEdit.CaptionResourceString = null;
			portBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			portBoundDropEdit.Name = "portBoundDropEdit";
			portBoundDropEdit.PreBoundMaxLength = 5;
			portBoundDropEdit.ShowDescriptionBox = false;
			portBoundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			portBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			portBoundDropEdit.TabIndex = 0;
			//
			// principalDropEdit
			//
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).PrincipalPK)));
			this.BindingSource.SetBindingMember(principalDropEdit, "PrincipalPK");
			principalDropEdit.Name = "Principal";
			principalDropEdit.ShowDescriptionBox = false;
			this.principalDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("582a0857-928b-4c5f-b1b5-cfacb31072e8", "Principal");
			principalDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			principalDropEdit.TabIndex = 1;
			// 
			// messageTypeBoundDropEdit
			// 
			messageTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(messageTypeBoundDropEdit, "MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).MessageType)));
			messageTypeBoundDropEdit.CaptionResourceString = null;
			messageTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 80, true);
			messageTypeBoundDropEdit.Name = "messageTypeBoundDropEdit";
			messageTypeBoundDropEdit.PreBoundMaxLength = 3;
			messageTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			messageTypeBoundDropEdit.TabIndex = 3;
			// 
			// directionBoundDropEdit
			// 
			directionBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(directionBoundDropEdit, "Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).Direction)));
			directionBoundDropEdit.CaptionResourceString = null;
			directionBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 56, true);
			directionBoundDropEdit.Name = "directionBoundDropEdit";
			directionBoundDropEdit.PreBoundMaxLength = 8;
			directionBoundDropEdit.ShowDescriptionBox = false;
			directionBoundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			directionBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			directionBoundDropEdit.TabIndex = 2;
			// 
			// versionDropEdit
			// 
			versionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(versionDropEdit, "Version");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).Version)));
			versionDropEdit.CaptionResourceString = null;
			versionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
			versionDropEdit.Name = "versionDropEdit";
			versionDropEdit.PreBoundMaxLength = 3;
			versionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			versionDropEdit.TabIndex = 0;
			// 
			// emailAddressTextBox
			// 
			this.BindingSource.SetBindingMember(emailAddressTextBox, "EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).EmailAddress)));
			emailAddressTextBox.CaptionResourceString = null;
			emailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 72, true);
			emailAddressTextBox.Name = "emailAddressTextBox";
			emailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			emailAddressTextBox.TabIndex = 3;
			// 
			// senderIdTextBox
			// 
			this.BindingSource.SetBindingMember(senderIdTextBox, "SenderId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).SenderId)));
			senderIdTextBox.CaptionResourceString = null;
			senderIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 24, true);
			senderIdTextBox.Name = "senderIdTextBox";
			senderIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			senderIdTextBox.TabIndex = 1;
			// 
			// recipientIdTextBox
			// 
			this.BindingSource.SetBindingMember(recipientIdTextBox, "RecipientId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).RecipientId)));
			recipientIdTextBox.CaptionResourceString = null;
			recipientIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 48, true);
			recipientIdTextBox.Name = "recipientIdTextBox";
			recipientIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			recipientIdTextBox.TabIndex = 2;
			// 
			// deliverToThirdPartyCheckBox
			// 
			deliverToThirdPartyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(deliverToThirdPartyCheckBox, "DeliverTo3rdParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).DeliverTo3rdParty)));
			deliverToThirdPartyCheckBox.CaptionResourceString = null;
			deliverToThirdPartyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			deliverToThirdPartyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 104, true);
			deliverToThirdPartyCheckBox.Name = "deliverToThirdPartyCheckBox";
			deliverToThirdPartyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 17, true);
			deliverToThirdPartyCheckBox.TabIndex = 4;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.thirdPartyPanel);
			this.topPanel.Controls.Add(this.cancelButton);
			this.topPanel.Controls.Add(deliverToThirdPartyCheckBox);
			this.topPanel.Controls.Add(this.sendButton);
			this.topPanel.Controls.Add(portBoundDropEdit);
			this.topPanel.Controls.Add(messageTypeBoundDropEdit);
			this.topPanel.Controls.Add(directionBoundDropEdit);
			this.topPanel.Controls.Add(principalDropEdit);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(16, 0, 16, 3, true);
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 224, true);
			this.topPanel.TabIndex = 0;
			// 
			// thirdPartyPanel
			// 
			this.thirdPartyPanel.Controls.Add(versionDropEdit);
			this.thirdPartyPanel.Controls.Add(emailAddressTextBox);
			this.thirdPartyPanel.Controls.Add(senderIdTextBox);
			this.thirdPartyPanel.Controls.Add(recipientIdTextBox);
			this.thirdPartyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.thirdPartyPanel.Name = "thirdPartyPanel";
			this.thirdPartyPanel.Visible = false;
			this.thirdPartyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 120, true);
			this.thirdPartyPanel.TabIndex = 4;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthorityFilterControl|5ed6173e-899c-48cd-8bf3-f748d1ab89b6", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 32, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 6;
			// 
			// sendButton
			// 
			this.sendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sendButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthorityFilterControl|66628f6a-5e47-444f-a4a6-f821e4f1e15c", "Send");
			this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 8, true);
			this.sendButton.Name = "sendButton";
			this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.sendButton.TabIndex = 5;
			// 
			// progressPanel
			// 
			this.progressPanel.Controls.Add(this.stateLabel);
			this.progressPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.progressPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
			this.progressPanel.Name = "progressPanel";
			this.progressPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(16, 2, 16, 2, true);
			this.progressPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 28, true);
			this.progressPanel.TabIndex = 1;
			// 
			// stateLabel
			// 
			this.stateLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.stateLabel.CaptionResourceString = null;
			this.stateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.stateLabel, false);
			this.stateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 2, true);
			this.stateLabel.Name = "stateLabel";
			this.stateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 24, true);
			this.stateLabel.TabIndex = 0;
			this.stateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// issuesGrid
			// 
			this.issuesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.issuesGrid, "Issues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).Issues)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortMessageIssue)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).Issues)).SyncRoot)).Text)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortMessageIssue)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthority)(null)).Issues)).SyncRoot)).Detail)));
			this.issuesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthorityFilterDialog|12785027-65a7-477a-8b48-ed3d2b2c9130", "Errors");
			zTextBoxColumnStyleInfo1.ColumnName = "Text";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(390);
			zMultiLineTextBoxColumnInfo1.Caption = null;
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = null;
			zMultiLineTextBoxColumnInfo1.ColumnName = "Detail";
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthorityFilterControl|Detail", "Detail");
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			this.issuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.issuesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.issuesGrid.CopySelectedRowsAllowed = true;
			this.issuesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.issuesGrid.GridId = "79923e71-7dee-4dc4-be17-88b58c01e407";
			this.issuesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.issuesGrid.LayoutKey = "issuesGrid";
			this.issuesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 228, true);
			this.issuesGrid.Name = "issuesGrid";
			this.issuesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.issuesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 80, true);
			this.issuesGrid.TabIndex = 2;
			this.issuesGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.senderNotificationsGrid_KeyDown);
			this.issuesGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Grid_MouseDown);
			// 
			// PortAuthorityFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.issuesGrid);
			this.Controls.Add(this.progressPanel);
			this.Controls.Add(this.topPanel);
			this.Name = "PortAuthorityFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 308, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.thirdPartyPanel.ResumeLayout(false);
			this.thirdPartyPanel.PerformLayout();
			this.progressPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.issuesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		CargoWise.Windows.UI.KPanel progressPanel;
		Enterprise.ZArchitecture.ZLabel stateLabel;
		Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		Enterprise.ZArchitecture.GUI.ZButton sendButton;
		Enterprise.ZArchitecture.ZGrid issuesGrid;
		CargoWise.Windows.UI.KPanel thirdPartyPanel;
		Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		Enterprise.ZArchitecture.GUI.ZDropEdit portBoundDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit messageTypeBoundDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit directionBoundDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit versionDropEdit;
		Enterprise.ZArchitecture.ZTextBox emailAddressTextBox;
		Enterprise.ZArchitecture.ZTextBox senderIdTextBox;
		Enterprise.ZArchitecture.ZTextBox recipientIdTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox deliverToThirdPartyCheckBox;
		Enterprise.ZArchitecture.GUI.ZGuidDropEdit principalDropEdit;
	}
}
