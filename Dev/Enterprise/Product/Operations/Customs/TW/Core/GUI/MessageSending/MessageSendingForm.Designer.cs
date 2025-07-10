namespace Enterprise.Customs.TW.GUI
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
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 379, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 379, true);
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 66, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 49, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 496, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclarationMessageSendingObjectParent);
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ValidationErrorsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("ef32af14-eb6b-4b5e-a232-a94614e89e8a", "Validation Errors");
			this.ValidationErrorsGroupBox.Controls.Add(this.ValidationErrorsTextBox);
			this.ValidationErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 82, true);
			this.ValidationErrorsGroupBox.Name = "ValidationErrorsGroupBox";
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 131, true);
			this.ValidationErrorsGroupBox.TabIndex = 4;
			this.ValidationErrorsGroupBox.TabStop = false;
			// 
			// ValidationErrorsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ValidationErrorsTextBox, "BizObjValidationMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclarationMessageSendingObjectParent)(null)).BizObjValidationMessageErrors)));
			this.ValidationErrorsTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("26d565ea-7bfb-461a-8fa1-615807572ca0", "Validation Errors");
			this.ValidationErrorsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ValidationErrorsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationErrorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ValidationErrorsTextBox.Multiline = true;
			this.ValidationErrorsTextBox.Name = "ValidationErrorsTextBox";
			this.ValidationErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ValidationErrorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 114, true);
			this.ValidationErrorsTextBox.TabIndex = 0;
			// 
			// AdditionalWarningsGroupBox
			// 
			this.AdditionalWarningsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalWarningsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e14001ad-2226-42a3-bbd3-13077797d65d", "Additional Warnings");
			this.AdditionalWarningsGroupBox.Controls.Add(this.AdditionalWarningsTextBox);
			this.AdditionalWarningsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 217, true);
			this.AdditionalWarningsGroupBox.Name = "AdditionalWarningsGroupBox";
			this.AdditionalWarningsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 130, true);
			this.AdditionalWarningsGroupBox.TabIndex = 5;
			this.AdditionalWarningsGroupBox.TabStop = false;
			// 
			// AdditionalWarningsTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalWarningsTextBox, "AdditionalWarnings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclarationMessageSendingObjectParent)(null)).AdditionalWarnings)));
			this.AdditionalWarningsTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e0f9329c-71d1-43d7-b8bb-8b31c198cf26", "Additional Warnings");
			this.AdditionalWarningsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalWarningsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalWarningsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AdditionalWarningsTextBox.Multiline = true;
			this.AdditionalWarningsTextBox.Name = "AdditionalWarningsTextBox";
			this.AdditionalWarningsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.AdditionalWarningsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 113, true);
			this.AdditionalWarningsTextBox.TabIndex = 0;
			// 
			// ContinueToSendCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ContinueToSendCheckBox, "AllowSendWithError");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobDeclarationMessageSendingObjectParent)(null)).AllowSendWithError)));
			this.ContinueToSendCheckBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("9d31e9db-f80b-4faf-9e68-3beb3e335d13", "Continue to send even though the selected message(s) contains validation errors?");
			this.ContinueToSendCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ContinueToSendCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 359, true);
			this.ContinueToSendCheckBox.Name = "ContinueToSendCheckBox";
			this.ContinueToSendCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 22, true);
			this.ContinueToSendCheckBox.TabIndex = 6;
			this.ContinueToSendCheckBox.UseVisualStyleBackColor = true;
			// 
			// MessageSendingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 400, true);
			this.Controls.Add(this.ContinueToSendCheckBox);
			this.Controls.Add(this.AdditionalWarningsGroupBox);
			this.Controls.Add(this.ValidationErrorsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclarationMessageSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 475, true);
			this.Name = "MessageSendingForm";
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.ValidationErrorsGroupBox, 0);
			this.Controls.SetChildIndex(this.AdditionalWarningsGroupBox, 0);
			this.Controls.SetChildIndex(this.ContinueToSendCheckBox, 0);
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
	}
}
