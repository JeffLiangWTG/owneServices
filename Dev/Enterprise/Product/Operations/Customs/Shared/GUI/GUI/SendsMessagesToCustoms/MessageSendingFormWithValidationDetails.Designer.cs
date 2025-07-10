namespace Enterprise.Customs.GUI
{
	partial class MessageSendingFormWithValidationDetails
	{

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>

		new void InitializeComponent()
		{
			this.ValidationErrorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ValidationErrorsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SendWithValidationErrorsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendWithAdditionalWarningCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.WarningSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PreviewMessageCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ValidationErrorsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).BeginInit();
			this.WarningSplitContainer.Panel1.SuspendLayout();
			this.WarningSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 433, true);
			this.SendButton.TabIndex = 5;
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 433, true);
			this.CancelButton2.TabIndex = 6;
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.messageSendingObjectsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 151, true);
			this.messageSendingObjectsGroupBox.TabIndex = 0;
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 108, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 464, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.IJobDeclarationMessageSendingObjectParent);
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("b9bd25f2-a20d-45e3-bfe7-7aa92c2949ee", "Validation Errors");
			this.ValidationErrorsGroupBox.Controls.Add(this.ValidationErrorsTextBox);
			this.ValidationErrorsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationErrorsGroupBox.Name = "ValidationErrorsGroupBox";
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 106, true);
			this.ValidationErrorsGroupBox.TabIndex = 1;
			this.ValidationErrorsGroupBox.TabStop = false;
			// 
			// ValidationErrorsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ValidationErrorsTextBox, "BizObjValidationMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.IJobDeclarationMessageSendingObjectParent)(null)).BizObjValidationMessageErrors)));
			this.ValidationErrorsTextBox.CaptionResourceString = null;
			this.ValidationErrorsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ValidationErrorsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValidationErrorsTextBox, false);
			this.ValidationErrorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 40, true);
			this.ValidationErrorsTextBox.Multiline = true;
			this.ValidationErrorsTextBox.Name = "ValidationErrorsTextBox";
			this.ValidationErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ValidationErrorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 63, true);
			this.ValidationErrorsTextBox.TabIndex = 0;
			// 
			// SendWithValidationErrorsCheckBox
			// 
			this.SendWithValidationErrorsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SendWithValidationErrorsCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("aa083da3-8f87-4911-91b8-503c131ef9f7", "Continue to send even though the selected message(s) contains validation errors?");
			this.SendWithValidationErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 387, true);
			this.SendWithValidationErrorsCheckBox.Name = "SendWithValidationErrorsCheckBox";
			this.SendWithValidationErrorsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 24, true);
			this.SendWithValidationErrorsCheckBox.TabIndex = 3;
			// 
			// SendWithAdditionalWarningCheckBox
			// 
			this.SendWithAdditionalWarningCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SendWithAdditionalWarningCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ac25081b-c5f5-498b-93bd-863a55ed8222", "Continue to send even though the selected message(s) has additional warnings?");
			this.SendWithAdditionalWarningCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 407, true);
			this.SendWithAdditionalWarningCheckBox.Name = "SendWithAdditionalWarningCheckBox";
			this.SendWithAdditionalWarningCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 24, true);
			this.SendWithAdditionalWarningCheckBox.TabIndex = 4;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 381, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(74);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.WarningSplitContainer);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(137);
			this.SplitContainer.TabIndex = 0;
			// 
			// WarningSplitContainer
			// 
			this.WarningSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WarningSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WarningSplitContainer.Name = "WarningSplitContainer";
			this.WarningSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// WarningSplitContainer.Panel1
			// 
			this.WarningSplitContainer.Panel1.Controls.Add(this.ValidationErrorsGroupBox);
			this.WarningSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 240, true);
			this.WarningSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(74);
			this.WarningSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(74);
			this.WarningSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(106);
			this.WarningSplitContainer.TabIndex = 0;
			// 
			// PreviewMessageCheckBox
			// 
			this.PreviewMessageCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PreviewMessageCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("27FA8BD4-8815-463B-815E-CDA53453BF95", "Preview Message?");
			this.PreviewMessageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 427, true);
			this.PreviewMessageCheckBox.Name = "PreviewMessageCheckBox";
			this.PreviewMessageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 24, true);
			this.PreviewMessageCheckBox.TabIndex = 5;
			this.PreviewMessageCheckBox.Visible = false;
			// 
			// MessageSendingFormWithValidationDetails
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 487, true);
			this.Controls.Add(this.PreviewMessageCheckBox);
			this.Controls.Add(this.SendWithAdditionalWarningCheckBox);
			this.Controls.Add(this.SendWithValidationErrorsCheckBox);
			this.Controls.Add(this.SplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.Business.IJobDeclarationMessageSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 510, true);
			this.Name = "MessageSendingFormWithValidationDetails";
			this.Controls.SetChildIndex(this.SplitContainer, 0);
			this.Controls.SetChildIndex(this.SendWithValidationErrorsCheckBox, 0);
			this.Controls.SetChildIndex(this.SendWithAdditionalWarningCheckBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.PreviewMessageCheckBox, 0);
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
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.WarningSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).EndInit();
			this.WarningSplitContainer.ResumeLayout(false);
			this.WarningSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected Enterprise.ZArchitecture.GUI.ZGroupBox ValidationErrorsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox ValidationErrorsTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox SendWithValidationErrorsCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox SendWithAdditionalWarningCheckBox;
		public ZArchitecture.GUI.ZCheckBox PreviewMessageCheckBox;
		protected CargoWise.Windows.UI.KSplitContainer SplitContainer;
		protected CargoWise.Windows.UI.KSplitContainer WarningSplitContainer;

		#endregion
	}
}
