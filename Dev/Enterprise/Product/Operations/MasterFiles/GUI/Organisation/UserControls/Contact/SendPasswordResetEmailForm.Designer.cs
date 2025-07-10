using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class SendPasswordResetEmailForm
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
		new void InitializeComponent()
		{
			this.displayMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.useCurrentEmailCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.fromEmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 126, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 24, true);
			// 
			// displayMessageLabel
			// 
			this.displayMessageLabel.AutoSize = true;
			this.displayMessageLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9ba287b6-58e6-4eb1-8776-af5665d9088d", "Send an email to the contact with the password instruction?");
			this.displayMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.displayMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.displayMessageLabel.Name = "displayMessageLabel";
			this.displayMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 14, true);
			this.displayMessageLabel.TabIndex = 1;
			// 
			// useCurrentEmailCheckBox
			// 
			this.useCurrentEmailCheckBox.AutoSize = true;
			this.useCurrentEmailCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("61ba1501-44a8-4bc7-970f-267b87454506", "Use Current User\'s Name and Title");
			this.useCurrentEmailCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useCurrentEmailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 59, true);
			this.useCurrentEmailCheckBox.Name = "useCurrentEmailCheckBox";
			this.useCurrentEmailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.useCurrentEmailCheckBox.TabIndex = 2;
			this.useCurrentEmailCheckBox.UseVisualStyleBackColor = true;
			this.useCurrentEmailCheckBox.CheckedChanged += new System.EventHandler(this.UseCurrentEmailCheckBox_CheckedChanged);
			// 
			// fromEmailLabel
			// 
			this.fromEmailLabel.AutoSize = true;
			this.fromEmailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.fromEmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 34, true);
			this.fromEmailLabel.Name = "fromEmailLabel";
			this.fromEmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 14, true);
			this.fromEmailLabel.TabIndex = 3;
			this.fromEmailLabel.Text = "-";
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.IsCaptionOverridden = true;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 98, true);
			this.okButton.Name = "okButton";
			this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 4;
			this.okButton.Text = Enterprise.MasterFiles.GUI.Res.GetString("75454AF0-9D66-4FB1-9F5A-99C73AE72C5E", "OK");
			this.okButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = true;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 98, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = Enterprise.MasterFiles.GUI.Res.GetString("52558BC5-1E81-4C20-BE14-5C59F1932A62", "Cancel");
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// SendPasswordResetEmailForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("316be9cf-3cf9-485d-9caf-2606e625ab32", "Send Password Email");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 150, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.fromEmailLabel);
			this.Controls.Add(this.useCurrentEmailCheckBox);
			this.Controls.Add(this.displayMessageLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "SendPasswordResetEmailForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.displayMessageLabel, 0);
			this.Controls.SetChildIndex(this.useCurrentEmailCheckBox, 0);
			this.Controls.SetChildIndex(this.fromEmailLabel, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel displayMessageLabel;
		public ZArchitecture.GUI.ZCheckBox useCurrentEmailCheckBox;
		public ZArchitecture.ZLabel fromEmailLabel;
		private ZArchitecture.GUI.ZButton okButton;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}
