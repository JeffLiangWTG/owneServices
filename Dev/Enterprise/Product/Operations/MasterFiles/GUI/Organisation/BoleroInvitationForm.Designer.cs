namespace Enterprise.MasterFiles.GUI
{
	public partial class BoleroInvitationForm
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
		protected override void InitializeComponent()
		{
			this.contactDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.contactNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.contactSearchEdit = new Enterprise.ZArchitecture.GUI.ZGuidSearchEdit();
			this.recipientEmailAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.recipientEmailAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.yourDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.yourNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.yourEmailAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.yourMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.yourNameTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.yourEmailAddressTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.yourMessageTextBox = new ZArchitecture.ZTextBox();
			this.sendInviteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.contactSearchEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 388, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.BoleroInvitationDetails);
			// 
			// contactDetailsLabel
			// 
			this.contactDetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.contactDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 9, true);
			this.contactDetailsLabel.Name = "contactDetailsLabel";
			this.contactDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 22, true);
			this.contactDetailsLabel.TabIndex = 0;
			this.contactDetailsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|06DAAB0E-6BE7-4FB3-8670-935FAA92BE45", "Please select Recipient (invitee) contact details");
			this.contactDetailsLabel.UseMnemonic = false;
			// 
			// contactNameLabel
			// 
			this.contactNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.contactNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(38, 34, true);
			this.contactNameLabel.Name = "contactNameLabel";
			this.contactNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 28, true);
			this.contactNameLabel.TabIndex = 1;
			this.contactNameLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|3E7C17C0-56F1-4F6D-9252-194830D63FE7", "Contact name");
			this.contactNameLabel.UseMnemonic = false;
			// 
			// contactSearchEdit
			// 
			this.contactSearchEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contactSearchEdit, "SelectedContactPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.BoleroInvitationDetails)(null)).SelectedContactPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.BoleroInvitationDetails)(null)).ActiveContacts)));
			this.contactSearchEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 40, true);
			this.contactSearchEdit.Name = "contactSearchEdit";
			this.contactSearchEdit.ShowDescriptionBox = false;
			this.contactSearchEdit.CodeBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.contactSearchEdit.CodeBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.contactSearchEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 16, true);
			this.contactSearchEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|FD9D6254-C609-49A0-A52C-A014B145A757", "Contact Name");
			this.contactSearchEdit.TabIndex = 2;
			// 
			// recipientEmailAddressLabel
			// 
			this.recipientEmailAddressLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.recipientEmailAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 70, true);
			this.recipientEmailAddressLabel.Name = "recipientEmailAddressLabel";
			this.recipientEmailAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 28, true);
			this.recipientEmailAddressLabel.TabIndex = 3;
			this.recipientEmailAddressLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|3A113F27-45A2-4999-8623-83ED11841CA2", "Email address");
			this.recipientEmailAddressLabel.UseMnemonic = false;
			// 
			// recipientEmailAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.recipientEmailAddressTextBox, "SelectedContactEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.BoleroInvitationDetails)(null)).SelectedContactEmail)));
			this.recipientEmailAddressTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|6298E0CE-C7BF-4312-AB33-9F0D2B00ABA5", "Contact Email");
			this.recipientEmailAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.recipientEmailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 76, true);
			this.recipientEmailAddressTextBox.Name = "recipientEmailAddressTextBox";
			this.recipientEmailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 16, true);
			this.recipientEmailAddressTextBox.TabIndex = 4;
			// 
			// yourDetailsLabel
			// 
			this.yourDetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.yourDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 124, true);
			this.yourDetailsLabel.Name = "yourDetailsLabel";
			this.yourDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 21, true);
			this.yourDetailsLabel.TabIndex = 5;
			this.yourDetailsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|4FCE82ED-4DD8-4FD4-B514-B7A249AD7EE0", "Your details (which will be shared with the invitee)");
			this.yourDetailsLabel.UseMnemonic = false;
			// 
			// yourNameLabel
			// 
			this.yourNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.yourNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 151, true);
			this.yourNameLabel.Name = "yourNameLabel";
			this.yourNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 24, true);
			this.yourNameLabel.TabIndex = 6;
			this.yourNameLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|C8AA0244-42AE-4432-B6F5-EB42B9A1DB98", "Name");
			this.yourNameLabel.UseMnemonic = false;
			// 
			// yourEmailAddressLabel
			// 
			this.yourEmailAddressLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.yourEmailAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 179, true);
			this.yourEmailAddressLabel.Name = "yourEmailAddressLabel";
			this.yourEmailAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 28, true);
			this.yourEmailAddressLabel.TabIndex = 7;
			this.yourEmailAddressLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|4540E81D-AF73-4164-8479-65BDD60DAD02", "Email address");
			this.yourEmailAddressLabel.UseMnemonic = false;
			// 
			// yourMessageLabel
			// 
			this.yourMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.yourMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 213, true);
			this.yourMessageLabel.Name = "yourMessageLabel";
			this.yourMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 28, true);
			this.yourMessageLabel.TabIndex = 8;
			this.yourMessageLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|7207FB53-11EB-4AD3-A136-F6D453263D8E", "Your message");
			this.yourMessageLabel.UseMnemonic = false;
			// 
			// yourNameTextLabel
			// 
			this.BindingSource.SetBindingMember(this.yourNameTextLabel, "YourName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.BoleroInvitationDetails)(null)).YourName)));
			this.yourNameTextLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|D774ECCA-6D79-43E8-AC01-9372412600AC", "Your Name");
			this.yourNameTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 155, true);
			this.yourNameTextLabel.Name = "yourNameTextLabel";
			this.yourNameTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 16, true);
			this.yourNameTextLabel.TabIndex = 9;
			// 
			// yourEmailAddressTextLabel
			//
			this.BindingSource.SetBindingMember(this.yourEmailAddressTextLabel, "YourEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.BoleroInvitationDetails)(null)).YourEmail)));
			this.yourEmailAddressTextLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|4B1FDF56-CBDE-492E-95DD-6A59A4DC7686", "Your Email");
			this.yourEmailAddressTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 185, true);
			this.yourEmailAddressTextLabel.Name = "yourEmailAddressTextLabel";
			this.yourEmailAddressTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 16, true);
			this.yourEmailAddressTextLabel.TabIndex = 10;
			// 
			// yourMessageTextBox
			//
			this.BindingSource.SetBindingMember(this.yourMessageTextBox, "YourMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.BoleroInvitationDetails)(null)).YourMessage)));
			this.yourMessageTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|63ECA4C3-F799-4403-A625-882AB0E1E4E6", "Your Message");
			this.yourMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 222, true);
			this.yourMessageTextBox.Name = "yourMessageTextBox";
			this.yourMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 111, true);
			this.yourMessageTextBox.Multiline = true;
			this.yourMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.yourMessageTextBox.TabIndex = 11;
			// 
			// sendInviteButton
			//
			this.sendInviteButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BoleroInvitationForm|5A1E6741-BF0F-4C63-8165-EB37B6F769AD", "Send invite");
			this.sendInviteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 353, true);
			this.sendInviteButton.Name = "sendInviteButton";
			this.sendInviteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
			this.sendInviteButton.TabIndex = 12;
			this.sendInviteButton.UseVisualStyleBackColor = true;
			this.sendInviteButton.Click += new System.EventHandler(this.SendInviteButton_Click);
			// 
			// BoleroInvitationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 412, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.sendInviteButton);
			this.Controls.Add(this.yourMessageTextBox);
			this.Controls.Add(this.yourEmailAddressTextLabel);
			this.Controls.Add(this.yourNameTextLabel);
			this.Controls.Add(this.yourMessageLabel);
			this.Controls.Add(this.yourEmailAddressLabel);
			this.Controls.Add(this.yourNameLabel);
			this.Controls.Add(this.yourDetailsLabel);
			this.Controls.Add(this.recipientEmailAddressTextBox);
			this.Controls.Add(this.recipientEmailAddressLabel);
			this.Controls.Add(this.contactSearchEdit);
			this.Controls.Add(this.contactNameLabel);
			this.Controls.Add(this.contactDetailsLabel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "BoleroInvitationForm";
			this.Controls.SetChildIndex(this.contactDetailsLabel, 0);
			this.Controls.SetChildIndex(this.contactNameLabel, 0);
			this.Controls.SetChildIndex(this.contactSearchEdit, 0);
			this.Controls.SetChildIndex(this.recipientEmailAddressLabel, 0);
			this.Controls.SetChildIndex(this.recipientEmailAddressTextBox, 0);
			this.Controls.SetChildIndex(this.yourDetailsLabel, 0);
			this.Controls.SetChildIndex(this.yourNameLabel, 0);
			this.Controls.SetChildIndex(this.yourEmailAddressLabel, 0);
			this.Controls.SetChildIndex(this.yourMessageLabel, 0);
			this.Controls.SetChildIndex(this.yourNameTextLabel, 0);
			this.Controls.SetChildIndex(this.yourEmailAddressTextLabel, 0);
			this.Controls.SetChildIndex(this.yourMessageTextBox, 0);
			this.Controls.SetChildIndex(this.sendInviteButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.contactSearchEdit.ResumeLayout(true);
			this.contactSearchEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel contactDetailsLabel;
		private ZArchitecture.ZLabel contactNameLabel;
		private ZArchitecture.GUI.ZGuidSearchEdit contactSearchEdit;
		private ZArchitecture.ZLabel recipientEmailAddressLabel;
		private ZArchitecture.ZTextBox recipientEmailAddressTextBox;
		private ZArchitecture.ZLabel yourDetailsLabel;
		private ZArchitecture.ZLabel yourNameLabel;
		private ZArchitecture.ZLabel yourEmailAddressLabel;
		private ZArchitecture.ZLabel yourMessageLabel;
		private ZArchitecture.ZLabel yourNameTextLabel;
		private ZArchitecture.ZLabel yourEmailAddressTextLabel;
		private ZArchitecture.ZTextBox yourMessageTextBox;
		private ZArchitecture.GUI.ZButton sendInviteButton;
	}
}
