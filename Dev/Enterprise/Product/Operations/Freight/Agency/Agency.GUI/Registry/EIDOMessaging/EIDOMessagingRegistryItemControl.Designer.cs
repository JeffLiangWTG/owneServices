namespace Enterprise.Freight.Agency.GUI
{
	partial class EIDOMessagingRegistryItemControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			passwordTextBox = new Enterprise.ZArchitecture.ZGrid();
			emailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			sendTestMessagesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			topPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(passwordTextBox)).BeginInit();
			topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.EIDOMessagingHeader);
			// 
			// passwordTextBox
			// 
			passwordTextBox.AllowNavigation = false;
			this.BindingSource.SetBindingMember(passwordTextBox, "Identities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.EIDOMessagingHeader)(null)).Identities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.EIDOMessagingIdentity)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.EIDOMessagingHeader)(null)).Identities)).SyncRoot)).PrincipalPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.EIDOMessagingIdentity)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.EIDOMessagingHeader)(null)).Identities)).SyncRoot)).SenderID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.EIDOMessagingIdentity)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.EIDOMessagingHeader)(null)).Identities)).SyncRoot)).RecipientID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.EIDOMessagingIdentity)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.EIDOMessagingHeader)(null)).Identities)).SyncRoot)).Password)));
			passwordTextBox.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PrincipalPK";
			zTextBoxColumnStyleInfo1.ColumnName = "SenderID";
			zTextBoxColumnStyleInfo2.ColumnName = "RecipientID";
			zTextBoxColumnStyleInfo3.ColumnName = "Password";
			passwordTextBox.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			passwordTextBox.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			passwordTextBox.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			passwordTextBox.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			passwordTextBox.GridId = "432636a2-5fd5-4a21-81c6-6041e54af57c";
			passwordTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			passwordTextBox.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			passwordTextBox.LayoutKey = "passwordTextBox";
			passwordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 64, true);
			passwordTextBox.Name = "passwordTextBox";
			passwordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 96, true);
			passwordTextBox.TabIndex = 3;
			// 
			// emailTextBox
			// 
			this.BindingSource.SetBindingMember(emailTextBox, "Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.EIDOMessagingHeader)(null)).Email)));
			emailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			emailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 32, true);
			emailTextBox.Name = "emailTextBox";
			emailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			emailTextBox.TabIndex = 1;
			// 
			// sendTestMessagesCheckBox
			// 
			sendTestMessagesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(sendTestMessagesCheckBox, "Testing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.EIDOMessagingHeader)(null)).Testing)));
			sendTestMessagesCheckBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("EIDOMessagingRegistryItemControl|e2581925-d338-40b3-8bae-f8d0aa99238b", "Send Test Messages");
			sendTestMessagesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			sendTestMessagesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			sendTestMessagesCheckBox.Name = "sendTestMessagesCheckBox";
			sendTestMessagesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 17, true);
			sendTestMessagesCheckBox.TabIndex = 0;
			sendTestMessagesCheckBox.UseVisualStyleBackColor = true;
			// 
			// topPanel
			// 
			topPanel.Controls.Add(sendTestMessagesCheckBox);
			topPanel.Controls.Add(emailTextBox);
			topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			topPanel.Name = "topPanel";
			topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 61, true);
			topPanel.TabIndex = 4;
			// 
			// EIDOMessagingRegistryItemControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(passwordTextBox);
			this.Controls.Add(topPanel);
			this.Name = "EIDOMessagingRegistryItemControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 163, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(passwordTextBox)).EndInit();
			topPanel.ResumeLayout(false);
			topPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZGrid passwordTextBox;
		Enterprise.ZArchitecture.ZTextBox emailTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox sendTestMessagesCheckBox;
		CargoWise.Windows.UI.KPanel topPanel;
	}
}
