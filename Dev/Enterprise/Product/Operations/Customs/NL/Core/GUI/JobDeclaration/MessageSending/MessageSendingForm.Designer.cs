
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
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
			this.StatementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatementTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValidationErrorsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).BeginInit();
			this.WarningSplitContainer.Panel1.SuspendLayout();
			this.WarningSplitContainer.Panel2.SuspendLayout();
			this.WarningSplitContainer.SuspendLayout();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatementGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 97, true);
			// 
			// SendWithValidationErrorsCheckBox
			// 
			this.SendWithValidationErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 378, true);
			// 
			// SendWithAdditionalWarningCheckBox
			// 
			this.SendWithAdditionalWarningCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 398, true);
			// 
			// PreviewMessageCheckBox
			// 
			this.PreviewMessageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 418, true);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 380, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(78);
			// 
			// WarningSplitContainer
			// 
			// 
			// WarningSplitContainer.Panel2
			// 
			this.WarningSplitContainer.Panel2.Controls.Add(this.StatementGroupBox);
			this.WarningSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 298, true);
			this.WarningSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(197);
			this.WarningSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(97);
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(601, 413, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 413, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 450, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent);
			// 
			// StatementGroupBox
			// 
			this.StatementGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("424E4ABE-BDA6-4775-90AC-E2F6588E0AC7", "Reason for Invalidation");
			this.StatementGroupBox.Controls.Add(this.StatementTextBox);
			this.StatementGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatementGroupBox.Name = "StatementGroupBox";
			this.StatementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 197, true);
			this.StatementGroupBox.TabIndex = 1;
			this.StatementGroupBox.TabStop = false;
			// 
			// StatementTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatementTextBox, "SendingObjectsCollection.ReasonForInvalidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ReasonForInvalidation)));
			this.StatementTextBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("EB5407C6-B2B8-4364-8577-5D87EF22AE36", "Reason for invalidation");
			this.StatementTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatementTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatementTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.StatementTextBox.Multiline = true;
			this.StatementTextBox.Name = "StatementTextBox";
			this.StatementTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.StatementTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 181, true);
			this.StatementTextBox.TabIndex = 0;
			// 
			// MessageSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 473, true);
			this.DataSourceType = typeof(Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent);
			this.Name = "MessageSendingForm";
			this.Text = "MessageSendingForm";
			this.ValidationErrorsGroupBox.ResumeLayout(false);
			this.ValidationErrorsGroupBox.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.WarningSplitContainer.Panel1.ResumeLayout(false);
			this.WarningSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).EndInit();
			this.WarningSplitContainer.ResumeLayout(false);
			this.WarningSplitContainer.PerformLayout();
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatementGroupBox.ResumeLayout(false);
			this.StatementGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected Enterprise.ZArchitecture.ZTextBox StatementTextBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox StatementGroupBox;
		#endregion
	}
}
