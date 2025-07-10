namespace Enterprise.Customs.TR.GUI.GlbStaff
{
	partial class TRStaffCredentialsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateInfButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ChooseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zSerialNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.ChipsetListDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SignatureTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BilgePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BilgeCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TRCustomsCertificateDefiningLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.ChipsetListDropEdit.SuspendLayout();
			this.SignatureTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.TRGlbStaffWrapper);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.CertificateInfButton);
			this.zGroupBox1.Controls.Add(this.ChooseButton);
			this.zGroupBox1.Controls.Add(this.zSerialNumber);
			this.zGroupBox1.Controls.Add(this.ChipsetListDropEdit);
			this.zGroupBox1.Controls.Add(this.SignatureTypeDropEdit);
			this.zGroupBox1.Controls.Add(this.BilgePasswordTextBox);
			this.zGroupBox1.Controls.Add(this.BilgeCodeTextBox);
			this.zGroupBox1.Controls.Add(this.StatusReasonTextBox);
			this.zGroupBox1.Controls.Add(this.PasswordStatusTextBox);
			this.zGroupBox1.Controls.Add(this.TRCustomsCertificateDefiningLinkLabel);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 26, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 314, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = Enterprise.Customs.TR.GUI.Res.GetString("8F570C80-D677-4007-96CF-6DE3F778CA1F", "Broker");
			// 
			// CertificateInfButton
			// 
			this.CertificateInfButton.IsCaptionOverridden = true;
			this.CertificateInfButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 181, true);
			this.CertificateInfButton.Name = "CertificateInfButton";
			this.CertificateInfButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CertificateInfButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.CertificateInfButton.TabIndex = 11;
			this.CertificateInfButton.Text = Enterprise.Customs.TR.GUI.Res.GetString("46C3CD68-2340-4D39-B043-347BDE17F116", "Certificate Informations");
			this.CertificateInfButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CertificateInfButton.ToolTipCaption = null;
			this.CertificateInfButton.UseVisualStyleBackColor = true;
			this.CertificateInfButton.Click += new System.EventHandler(this.CertificateInfButton_Click);
			// 
			// ChooseButton
			// 
			this.ChooseButton.IsCaptionOverridden = true;
			this.ChooseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 134, true);
			this.ChooseButton.Name = "ChooseButton";
			this.ChooseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ChooseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.ChooseButton.TabIndex = 10;
			this.ChooseButton.Text = Enterprise.Customs.TR.GUI.Res.GetString("81E5A5C2-D300-4E46-98F1-A869F9D99D67", "Choose Certificate");
			this.ChooseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ChooseButton.ToolTipCaption = null;
			this.ChooseButton.UseVisualStyleBackColor = true;
			this.ChooseButton.Click += new System.EventHandler(this.ChooseButton_Click);
			// 
			// zSerialNumber
			// 
			this.BindingSource.SetBindingMember(this.zSerialNumber, "TRBPassword.GP_CertificateSerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.TRGlbStaffWrapper)(null)).TRBPassword.GP_CertificateSerialNumber)));
			this.zSerialNumber.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("5435905f-2301-4c63-8118-de8d6df1aad2", "Serial Number");
			this.zSerialNumber.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zSerialNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 160, true);
			this.zSerialNumber.Name = "zSerialNumber";
			this.zSerialNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.zSerialNumber.TabIndex = 5;
			// 
			// ChipsetListDropEdit
			// 
			this.ChipsetListDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChipsetListDropEdit, "TRBPassword.TR_Chipset");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.TRGlbStaffWrapper)(null)).TRBPassword.TR_Chipset)));
			this.ChipsetListDropEdit.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("47644399-d4c8-4d68-a1ae-65851aa7c7a6", "Chip-set");
			this.ChipsetListDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 113, true);
			this.ChipsetListDropEdit.Name = "ChipsetListDropEdit";
			this.ChipsetListDropEdit.ShouldResizeByMaxLength = true;
			this.ChipsetListDropEdit.ShowDescriptionBox = false;
			this.ChipsetListDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ChipsetListDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.ChipsetListDropEdit.TabIndex = 3;
			// 
			// SignatureTypeDropEdit
			// 
			this.SignatureTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SignatureTypeDropEdit, "TRBPassword.GP_CertificateAuthority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.TRGlbStaffWrapper)(null)).TRBPassword.GP_CertificateAuthority)));
			this.SignatureTypeDropEdit.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("b938c659-fb8d-4be2-b8e1-8ab0448ebb01", "Signature Type");
			this.SignatureTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 86, true);
			this.SignatureTypeDropEdit.Name = "SignatureTypeDropEdit";
			this.SignatureTypeDropEdit.ShouldResizeByMaxLength = true;
			this.SignatureTypeDropEdit.ShowDescriptionBox = false;
			this.SignatureTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.SignatureTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.SignatureTypeDropEdit.TabIndex = 2;
			// 
			// BilgePasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.BilgePasswordTextBox, "TRBPassword.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.TRGlbStaffWrapper)(null)).TRBPassword.CurrentDecryptedPassword)));
			this.BilgePasswordTextBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("9bdb78b8-24f4-4095-b8d8-8a795c62c540", "Bilge Password");
			this.BilgePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 59, true);
			this.BilgePasswordTextBox.Name = "BilgePasswordTextBox";
			this.BilgePasswordTextBox.PasswordChar = '*';
			this.BilgePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.BilgePasswordTextBox.TabIndex = 1;
			// 
			// BilgeCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.BilgeCodeTextBox, "TRBPassword.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.TRGlbStaffWrapper)(null)).TRBPassword.GP_UserID)));
			this.BilgeCodeTextBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("83840952-a49e-40fc-b2ff-3e90d88f3509", "Bilge Code");
			this.BilgeCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 32, true);
			this.BilgeCodeTextBox.Name = "BilgeCodeTextBox";
			this.BilgeCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.BilgeCodeTextBox.TabIndex = 0;
			// 
			// StatusReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusReasonTextBox, "TRBPassword.GP_StatusReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.TRGlbStaffWrapper)(null)).TRBPassword.GP_StatusReason)));
			this.StatusReasonTextBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("B6E5E88E-7C1B-442A-BD95-4A4754EBF317", "Status Reason");
			this.StatusReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatusReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 234, true);
			this.StatusReasonTextBox.Multiline = true;
			this.StatusReasonTextBox.Name = "StatusReasonTextBox";
			this.StatusReasonTextBox.ReadOnly = true;
			this.StatusReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.StatusReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.StatusReasonTextBox.TabIndex = 9;
			this.StatusReasonTextBox.Visible = false;
			// 
			// PasswordStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordStatusTextBox, "TRBPassword.GP_PasswordStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.TRGlbStaffWrapper)(null)).TRBPassword.GP_PasswordStatusDescription)));
			this.PasswordStatusTextBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("57c3fa13-444a-4cf2-ae9b-86c21d2af42a", "Password Status");
			this.PasswordStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 213, true);
			this.PasswordStatusTextBox.Name = "PasswordStatusTextBox";
			this.PasswordStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.PasswordStatusTextBox.TabIndex = 8;
			// 
			// TRCustomsCertificateDefiningLinkLabel
			// 
			this.TRCustomsCertificateDefiningLinkLabel.AutoSize = true;
			this.TRCustomsCertificateDefiningLinkLabel.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("E79DD3B8-B1BB-4790-89FC-BB307C70EA17", "TR Customs Certificate Defining");
			this.TRCustomsCertificateDefiningLinkLabel.IsFontBold = false;
			this.TRCustomsCertificateDefiningLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 254, true);
			this.TRCustomsCertificateDefiningLinkLabel.Name = "TRCustomsCertificateDefiningLinkLabel";
			this.TRCustomsCertificateDefiningLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 14, true);
			this.TRCustomsCertificateDefiningLinkLabel.TabIndex = 9;
			this.TRCustomsCertificateDefiningLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TRCustomsCertificateDefiningLinkLabel_LinkClicked);
			// 
			// TRStaffCredentialsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "TRStaffCredentialsUserControl";
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ChipsetListDropEdit.ResumeLayout(true);
			this.ChipsetListDropEdit.PerformLayout();
			this.SignatureTypeDropEdit.ResumeLayout(true);
			this.SignatureTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

			#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZDropEdit SignatureTypeDropEdit;
		private ZArchitecture.ZTextBox BilgePasswordTextBox;
		private ZArchitecture.ZTextBox BilgeCodeTextBox;
		private ZArchitecture.GUI.ZDropEdit ChipsetListDropEdit;
		private ZArchitecture.ZTextBox zSerialNumber;
		private ZArchitecture.ZTextBox PasswordStatusTextBox;
		private ZArchitecture.ZTextBox StatusReasonTextBox;	
		private ZArchitecture.GUI.ZLinkLabel TRCustomsCertificateDefiningLinkLabel;
		private ZArchitecture.GUI.ZButton CertificateInfButton;
		private ZArchitecture.GUI.ZButton ChooseButton;
	}
}
