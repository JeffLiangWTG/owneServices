namespace Enterprise.Customs.GUI
{
	partial class AmendmentReasonForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected Enterprise.ZArchitecture.ZLabel ReasonLabel;
		protected Enterprise.ZArchitecture.ZTextBox ReasonTextTextBox;
		protected new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		protected internal Enterprise.ZArchitecture.GUI.ZButton OKButton;

		#region Auto-Generated

		protected new void InitializeComponent()
		{
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReasonTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 144, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.CancelButton.TabIndex = 4;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("39DAD9C2-C2D5-40FA-AAA6-9F7946C5DA4B", "Cancel");
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 144, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0BA53352-D696-43AD-895C-D7C081C9F17C", "OK");
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ReasonLabel
			// 
			this.ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 8, true);
			this.ReasonLabel.Name = "ReasonLabel";
			this.ReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 23, true);
			this.ReasonLabel.TabIndex = 4;
			this.ReasonLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F50561DA-8CB1-4817-A04B-B2106ADE515F", "Please enter a reason for the amendment or withdrawal:");
			// 
			// ReasonTextTextBox
			// 
			this.ReasonTextTextBox.BindTo = "ReasonText";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.AmendmentWithdrawalReason)(null)).ReasonTextInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.AmendmentWithdrawalReason)(null)).ReasonText)));
			this.ReasonTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReasonTextTextBox, false);
			this.ReasonTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 32, true);
			this.ReasonTextTextBox.Multiline = true;
			this.ReasonTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ReasonTextTextBox.Name = "ReasonTextTextBox";
			this.ReasonTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 104, true);
			this.ReasonTextTextBox.TabIndex = 2;
			// 
			// AmendmentReasonForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 200, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReasonTextTextBox);
			this.Controls.Add(this.ReasonLabel);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "Enterprise.Customs.Business.AmendmentWithdrawalReason";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 232, true);
			this.Name = "AmendmentReasonForm";
			this.Text = "Amendment or Withdrawal";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.ReasonLabel, 0);
			this.Controls.SetChildIndex(this.ReasonTextTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
