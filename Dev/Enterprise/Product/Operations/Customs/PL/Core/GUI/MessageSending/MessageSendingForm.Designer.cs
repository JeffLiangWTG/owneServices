namespace Enterprise.Customs.PL.GUI
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
			this.AdditionalDataGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ValidationErrorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ValidationErrorsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalWarningsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalWarningsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContinueToSendCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FallbackSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AdditionalDetailsUserControl = new Enterprise.Customs.PL.GUI.AdditionalDetailsUserControl();
			this.AdditionalDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ValidationErrorsGroupBox.SuspendLayout();
			this.AdditionalWarningsGroupBox.SuspendLayout();
			this.AdditionalDetailsUserControl.SuspendLayout();
			this.AdditionalDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(649, 823, true);
			this.SendButton.TabIndex = 8;
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(742, 823, true);
			this.CancelButton2.TabIndex = 9;
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 106, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 89, true);
			this.MessageSendingObjectsGrid.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 849, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent);
			// 
			// AdditionalDataGroupBox
			// 
			this.AdditionalDataGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalDataGroupBox, false);
			this.AdditionalDataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 119, true);
			this.AdditionalDataGroupBox.Name = "AdditionalDataGroupBox";
			this.AdditionalDataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 226, true);
			this.AdditionalDataGroupBox.TabIndex = 2;
			this.AdditionalDataGroupBox.TabStop = false;
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ValidationErrorsGroupBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("PLMessageSendingForm|ValidationErrorsGroupBox", "Validation Errors");
			this.ValidationErrorsGroupBox.Controls.Add(this.ValidationErrorsTextBox);
			this.ValidationErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 350, true);
			this.ValidationErrorsGroupBox.Name = "ValidationErrorsGroupBox";
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 131, true);
			this.ValidationErrorsGroupBox.TabIndex = 3;
			this.ValidationErrorsGroupBox.TabStop = false;
			// 
			// ValidationErrorsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ValidationErrorsTextBox, "BizObjValidationMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).BizObjValidationMessageErrors)));
			this.ValidationErrorsTextBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("PLMessageSendingForm|ValidationErrorsTextBox", "Validation Errors");
			this.ValidationErrorsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ValidationErrorsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationErrorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ValidationErrorsTextBox.Multiline = true;
			this.ValidationErrorsTextBox.Name = "ValidationErrorsTextBox";
			this.ValidationErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ValidationErrorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 114, true);
			this.ValidationErrorsTextBox.TabIndex = 0;
			// 
			// AdditionalWarningsGroupBox
			// 
			this.AdditionalWarningsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalWarningsGroupBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("PLMessageSendingForm|AdditionalWarningsGroupBox", "Additional Warnings");
			this.AdditionalWarningsGroupBox.Controls.Add(this.AdditionalWarningsTextBox);
			this.AdditionalWarningsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 483, true);
			this.AdditionalWarningsGroupBox.Name = "AdditionalWarningsGroupBox";
			this.AdditionalWarningsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 130, true);
			this.AdditionalWarningsGroupBox.TabIndex = 4;
			this.AdditionalWarningsGroupBox.TabStop = false;
			// 
			// AdditionalWarningsTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalWarningsTextBox, "AdditionalWarnings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).AdditionalWarnings)));
			this.AdditionalWarningsTextBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("PLMessageSendingForm|AdditionalWarningsTextBox", "Additional Warnings");
			this.AdditionalWarningsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalWarningsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalWarningsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.AdditionalWarningsTextBox.Multiline = true;
			this.AdditionalWarningsTextBox.Name = "AdditionalWarningsTextBox";
			this.AdditionalWarningsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.AdditionalWarningsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 113, true);
			this.AdditionalWarningsTextBox.TabIndex = 0;
			// 
			// ContinueToSendCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ContinueToSendCheckBox, "AllowSendWithError");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).AllowSendWithError)));
			this.ContinueToSendCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 775, true);
			this.ContinueToSendCheckBox.Name = "ContinueToSendCheckBox";
			this.ContinueToSendCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 22, true);
			this.ContinueToSendCheckBox.TabIndex = 7;
			this.ContinueToSendCheckBox.UseVisualStyleBackColor = true;
			// 
			// FallbackSystemCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FallbackSystemCheckBox, "FallbackSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).FallbackSystem)));
			this.FallbackSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 802, true);
			this.FallbackSystemCheckBox.Name = "FallbackSystemCheckBox";
			this.FallbackSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 22, true);
			this.FallbackSystemCheckBox.TabIndex = 8;
			this.FallbackSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// AdditionalDetailsUserControl
			// 
			this.AdditionalDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalDetailsUserControl, "SendingObjectsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.PL.Business.BaseMessageSendingObject)(((Enterprise.Customs.PL.Business.BaseMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)))));
			this.AdditionalDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.AdditionalDetailsUserControl.Name = "AdditionalDetailsUserControl";
			this.AdditionalDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 134, true);
			this.AdditionalDetailsUserControl.TabIndex = 0;
			// 
			// AdditionalDetailsGroupBox
			// 
			this.AdditionalDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalDetailsGroupBox.Controls.Add(this.AdditionalDetailsUserControl);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalDetailsGroupBox, false);
			this.AdditionalDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 618, true);
			this.AdditionalDetailsGroupBox.Name = "AdditionalDetailsGroupBox";
			this.AdditionalDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 151, true);
			this.AdditionalDetailsGroupBox.TabIndex = 6;
			this.AdditionalDetailsGroupBox.TabStop = false;
			// 
			// MessageSendingForm
			//
			this.AutoScroll = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 872, true);
			this.Controls.Add(this.AdditionalDetailsGroupBox);
			this.Controls.Add(this.AdditionalDataGroupBox);
			this.Controls.Add(this.FallbackSystemCheckBox);
			this.Controls.Add(this.ContinueToSendCheckBox);
			this.Controls.Add(this.AdditionalWarningsGroupBox);
			this.Controls.Add(this.ValidationErrorsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 475, true);
			this.Name = "MessageSendingForm";
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.ValidationErrorsGroupBox, 0);
			this.Controls.SetChildIndex(this.AdditionalWarningsGroupBox, 0);
			this.Controls.SetChildIndex(this.ContinueToSendCheckBox, 0);
			this.Controls.SetChildIndex(this.FallbackSystemCheckBox, 0);
			this.Controls.SetChildIndex(this.AdditionalDataGroupBox, 0);
			this.Controls.SetChildIndex(this.AdditionalDetailsGroupBox, 0);
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
			this.AdditionalDetailsUserControl.ResumeLayout(true);
			this.AdditionalDetailsUserControl.PerformLayout();
			this.AdditionalDetailsGroupBox.ResumeLayout(false);
			this.AdditionalDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ValidationErrorsGroupBox;
		internal ZArchitecture.ZTextBox ValidationErrorsTextBox;
		internal ZArchitecture.GUI.ZGroupBox AdditionalWarningsGroupBox;
		internal ZArchitecture.ZTextBox AdditionalWarningsTextBox;
		internal ZArchitecture.GUI.ZCheckBox ContinueToSendCheckBox;
		internal ZArchitecture.GUI.ZCheckBox FallbackSystemCheckBox;
		protected internal ZArchitecture.GUI.ZGroupBox AdditionalDataGroupBox;
		internal ZArchitecture.GUI.ZGroupBox AdditionalDetailsGroupBox;
		internal AdditionalDetailsUserControl AdditionalDetailsUserControl;
	}
}
