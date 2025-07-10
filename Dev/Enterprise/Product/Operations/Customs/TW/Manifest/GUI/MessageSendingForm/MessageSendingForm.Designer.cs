namespace Enterprise.Customs.TW.Manifest.GUI
{
	partial class MessageSendingForm
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
            this.ValidationErrorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ValidationErrorsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AdditionalWarningsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AdditionalWarningsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ContinueToSendCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.IsFinalManifestCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.messageSendingObjectsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
            this.MessageSendingObjectsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ValidationErrorsGroupBox.SuspendLayout();
            this.AdditionalWarningsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // SendButton
            // 
            this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 443, true);
            this.SendButton.TabIndex = 10;
            // 
            // CancelButton2
            // 
            this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 443, true);
            this.CancelButton2.TabIndex = 11;
            // 
            // messageSendingObjectsGroupBox
            // 
            this.messageSendingObjectsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 10, true);
            this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 130, true);
            // 
            // MessageSendingObjectsGrid
            // 
            this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(756, 113, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 477, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Manifest.Business.MessageSendingObjectParent);
            // 
            // ValidationErrorsGroupBox
            // 
            this.ValidationErrorsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ValidationErrorsGroupBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("6952e02e-17e2-4a71-acc5-79a2558c7ea8", "Validation Errors");
            this.ValidationErrorsGroupBox.Controls.Add(this.ValidationErrorsTextBox);
            this.ValidationErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 146, true);
            this.ValidationErrorsGroupBox.Name = "ValidationErrorsGroupBox";
            this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 131, true);
            this.ValidationErrorsGroupBox.TabIndex = 5;
            this.ValidationErrorsGroupBox.TabStop = false;
            // 
            // ValidationErrorsTextBox
            // 
            this.BindingSource.SetBindingMember(this.ValidationErrorsTextBox, "BizObjValidationMessageErrors");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.MessageSendingObjectParent)(null)).BizObjValidationMessageErrors)));
            this.ValidationErrorsTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("7c7e386b-9a5a-443a-89d5-8b7bd8f56c2d", "Validation Errors");
            this.ValidationErrorsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ValidationErrorsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ValidationErrorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.ValidationErrorsTextBox.Multiline = true;
            this.ValidationErrorsTextBox.Name = "ValidationErrorsTextBox";
            this.ValidationErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ValidationErrorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(756, 114, true);
            this.ValidationErrorsTextBox.TabIndex = 0;
            // 
            // AdditionalWarningsGroupBox
            // 
            this.AdditionalWarningsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AdditionalWarningsGroupBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("228cfe30-d78c-4115-bfd5-f6d754b1fdd6", "Additional Warnings");
            this.AdditionalWarningsGroupBox.Controls.Add(this.AdditionalWarningsTextBox);
            this.AdditionalWarningsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 283, true);
            this.AdditionalWarningsGroupBox.Name = "AdditionalWarningsGroupBox";
            this.AdditionalWarningsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 131, true);
            this.AdditionalWarningsGroupBox.TabIndex = 6;
            this.AdditionalWarningsGroupBox.TabStop = false;
            // 
            // AdditionalWarningsTextBox
            // 
            this.BindingSource.SetBindingMember(this.AdditionalWarningsTextBox, "AdditionalWarnings");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.MessageSendingObjectParent)(null)).AdditionalWarnings)));
            this.AdditionalWarningsTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("f325f86d-961f-4e4d-b038-99a7785240e7", "Additional Warnings");
            this.AdditionalWarningsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.AdditionalWarningsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalWarningsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.AdditionalWarningsTextBox.Multiline = true;
            this.AdditionalWarningsTextBox.Name = "AdditionalWarningsTextBox";
            this.AdditionalWarningsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.AdditionalWarningsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(756, 114, true);
            this.AdditionalWarningsTextBox.TabIndex = 0;
            // 
            // ContinueToSendCheckBox
            // 
            this.ContinueToSendCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BindingSource.SetBindingMember(this.ContinueToSendCheckBox, "AllowSendWithError");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Manifest.Business.MessageSendingObjectParent)(null)).AllowSendWithError)));
            this.ContinueToSendCheckBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("fb91bb5f-6461-4878-85a7-15327433e936", "Continue to send even though the selected message(s) contains validation errors? " +
        "");
            this.ContinueToSendCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 443, true);
            this.ContinueToSendCheckBox.Name = "ContinueToSendCheckBox";
            this.ContinueToSendCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 22, true);
            this.ContinueToSendCheckBox.TabIndex = 9;
            this.ContinueToSendCheckBox.UseVisualStyleBackColor = true;
            // 
            // IsFinalManifestCheckBox
            // 
            this.IsFinalManifestCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BindingSource.SetBindingMember(this.IsFinalManifestCheckBox, "IsFinalManifest");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Manifest.Business.MessageSendingObjectParent)(null)).IsFinalManifest)));
            this.IsFinalManifestCheckBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("098b8f5c-b082-4372-98b5-a28a472e2b94", "Final Manifest?", "By checking this checkbox, you confirm that this is the final manifest for the ma" +
        "ster bill number, and all manifests under the master bill number have been submi" +
        "tted to customs.");
            this.IsFinalManifestCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 417, true);
            this.IsFinalManifestCheckBox.Name = "IsFinalManifestCheckBox";
            this.IsFinalManifestCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 22, true);
            this.IsFinalManifestCheckBox.TabIndex = 8;
            this.IsFinalManifestCheckBox.UseVisualStyleBackColor = true;
            // 
            // MessageSendingForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 499, true);
            this.Controls.Add(this.IsFinalManifestCheckBox);
            this.Controls.Add(this.ContinueToSendCheckBox);
            this.Controls.Add(this.AdditionalWarningsGroupBox);
            this.Controls.Add(this.ValidationErrorsGroupBox);
            this.DataSourceType = typeof(Enterprise.Customs.TW.Manifest.Business.MessageSendingObjectParent);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 537, true);
            this.Name = "MessageSendingForm";
            this.Text = "SendingMessageForm";
            this.Controls.SetChildIndex(this.ValidationErrorsGroupBox, 0);
            this.Controls.SetChildIndex(this.AdditionalWarningsGroupBox, 0);
            this.Controls.SetChildIndex(this.ContinueToSendCheckBox, 0);
            this.Controls.SetChildIndex(this.SendButton, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.CancelButton2, 0);
            this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
            this.Controls.SetChildIndex(this.IsFinalManifestCheckBox, 0);
            this.messageSendingObjectsGroupBox.ResumeLayout(false);
            this.messageSendingObjectsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
            this.MessageSendingObjectsGrid.ResumeLayout(false);
            this.MessageSendingObjectsGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ValidationErrorsGroupBox.ResumeLayout(false);
            this.ValidationErrorsGroupBox.PerformLayout();
            this.AdditionalWarningsGroupBox.ResumeLayout(false);
            this.AdditionalWarningsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox ValidationErrorsGroupBox;
		protected ZArchitecture.ZTextBox ValidationErrorsTextBox;
		protected ZArchitecture.GUI.ZGroupBox AdditionalWarningsGroupBox;
		protected ZArchitecture.ZTextBox AdditionalWarningsTextBox;
		protected ZArchitecture.GUI.ZCheckBox ContinueToSendCheckBox;
		protected ZArchitecture.GUI.ZCheckBox IsFinalManifestCheckBox;
	}
}
