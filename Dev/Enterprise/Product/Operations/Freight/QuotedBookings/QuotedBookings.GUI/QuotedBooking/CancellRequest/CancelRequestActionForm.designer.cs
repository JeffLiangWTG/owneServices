
namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class CancelRequestActionForm
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
			this.zButtonAccept = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonReject = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zTextBoxRejectionReason = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabelMessage = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelRejectionReason = new Enterprise.ZArchitecture.ZLabel();
			this.zButtonSubmitRejectionReason = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 142, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 24, true);
			// 
			// zButtonAccept
			// 
			this.zButtonAccept.IsCaptionOverridden = true;
			this.zButtonAccept.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 60, true);
			this.zButtonAccept.Name = "zButtonAccept";
			this.zButtonAccept.CaptionResourceString = Res.GetData("6365E878-D222-46A1-AF27-80EF4AEE157A", "Accept");
			this.zButtonAccept.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonAccept.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonAccept.TabIndex = 1;
			this.zButtonAccept.Text = Res.GetString("6365E878-D222-46A1-AF27-80EF4AEE157A", "Accept");
			this.zButtonAccept.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButtonAccept.ToolTipCaption = null;
			this.zButtonAccept.UseVisualStyleBackColor = true;
			this.zButtonAccept.Click += new System.EventHandler(this.zButtonAccept_Click);
			// 
			// zButtonCancel
			// 
			this.zButtonCancel.IsCaptionOverridden = true;
			this.zButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 60, true);
			this.zButtonCancel.Name = "zButtonCancel";
			this.zButtonCancel.CaptionResourceString = Res.GetData("2AF86447-0900-425C-995B-A0B2DFD27BEE", "Cancel");
			this.zButtonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonCancel.TabIndex = 3;
			this.zButtonCancel.Text = Res.GetString("2AF86447-0900-425C-995B-A0B2DFD27BEE", "Cancel");
			this.zButtonCancel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButtonCancel.ToolTipCaption = null;
			this.zButtonCancel.UseVisualStyleBackColor = true;
			this.zButtonCancel.Click += new System.EventHandler(this.zButtonCancel_Click);
			// 
			// zButtonReject
			// 
			this.zButtonReject.IsCaptionOverridden = true;
			this.zButtonReject.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 60, true);
			this.zButtonReject.Name = "zButtonReject";
			this.zButtonReject.CaptionResourceString = Res.GetData("967BB85B-0715-477C-9797-CCD01AB01971", "Reject");
			this.zButtonReject.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonReject.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonReject.TabIndex = 2;
			this.zButtonReject.Text = Res.GetString("967BB85B-0715-477C-9797-CCD01AB01971", "Reject");
			this.zButtonReject.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButtonReject.ToolTipCaption = null;
			this.zButtonReject.UseVisualStyleBackColor = true;
			this.zButtonReject.Click += new System.EventHandler(this.zButtonReject_Click);
			// 
			// zTextBoxRejectionReason
			// 
			this.zTextBoxRejectionReason.CaptionResourceString = null;
			this.zTextBoxRejectionReason.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBoxRejectionReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 112, true);
			this.zTextBoxRejectionReason.Name = "zTextBoxRejectionReason";
			this.zTextBoxRejectionReason.ShouldEscapeAllSpecialCharacters = false;
			this.zTextBoxRejectionReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
			this.zTextBoxRejectionReason.TabIndex = 4;
			this.zTextBoxRejectionReason.Visible = false;
			// 
			// zLabelMessage
			// 
			this.zLabelMessage.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.zLabelMessage.Name = "zLabelMessage";
			this.zLabelMessage.CaptionResourceString = Res.GetData("D6CA010D-3B97-4D1F-8740-3794FF5ACAAF", "A Booking Request Withdrawal/Cancellation message was received for this booking. Do you accept the withdrawal/cancellation, reject the withdrawal/cancellation or do you want to cancel this dialog and review the booking before taking an action?");
			this.zLabelMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 44, true);
			this.zLabelMessage.TabIndex = 0;
			this.zLabelMessage.Text = Res.GetString("D6CA010D-3B97-4D1F-8740-3794FF5ACAAF", "A Booking Request Withdrawal/Cancellation message was received for this booking. Do you accept the withdrawal/cancellation, reject the withdrawal/cancellation or do you want to cancel this dialog and review the booking before taking an action?");
			// 
			// zLabelRejectionReason
			// 
			this.zLabelRejectionReason.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelRejectionReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 86, true);
			this.zLabelRejectionReason.Name = "zLabelRejectionReason";
			this.zLabelRejectionReason.CaptionResourceString = Res.GetData("65086984-721F-4182-B1A2-AD67566BBB73", "Rejection Reason:");
			this.zLabelRejectionReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabelRejectionReason.TabIndex = 0;
			this.zLabelRejectionReason.Text = Res.GetString("65086984-721F-4182-B1A2-AD67566BBB73", "Rejection Reason:");
			this.zLabelRejectionReason.Visible = false;
			// 
			// zButtonSubmitRejectionReason
			// 
			this.zButtonSubmitRejectionReason.IsCaptionOverridden = true;
			this.zButtonSubmitRejectionReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 112, true);
			this.zButtonSubmitRejectionReason.Name = "zButtonSubmitRejectionReason";
			this.zButtonSubmitRejectionReason.CaptionResourceString = Res.GetData("301A4F0E-E0A0-4F3C-AEE4-30B55D468865", "Submit");
			this.zButtonSubmitRejectionReason.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonSubmitRejectionReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonSubmitRejectionReason.TabIndex = 5;
			this.zButtonSubmitRejectionReason.Text = Res.GetString("301A4F0E-E0A0-4F3C-AEE4-30B55D468865", "Submit");
			this.zButtonSubmitRejectionReason.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButtonSubmitRejectionReason.ToolTipCaption = null;
			this.zButtonSubmitRejectionReason.UseVisualStyleBackColor = true;
			this.zButtonSubmitRejectionReason.Visible = false;
			this.zButtonSubmitRejectionReason.Click += new System.EventHandler(this.zButtonSubmitRejectionReason_Click);
			// 
			// CancelRequestActionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 166, true);
			this.Controls.Add(this.zButtonSubmitRejectionReason);
			this.Controls.Add(this.zLabelRejectionReason);
			this.Controls.Add(this.zLabelMessage);
			this.Controls.Add(this.zTextBoxRejectionReason);
			this.Controls.Add(this.zButtonReject);
			this.Controls.Add(this.zButtonCancel);
			this.Controls.Add(this.zButtonAccept);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CancelRequestActionForm";
			this.CaptionResourceString = Res.GetData("25C3CAF9-D554-4B5A-B741-A441C2E10588", "Booking Withdrawal/Cancellation");
			this.ShowIcon = false;
			this.Text = Res.GetString("25C3CAF9-D554-4B5A-B741-A441C2E10588", "Booking Withdrawal/Cancellation");
			this.Controls.SetChildIndex(this.zButtonAccept, 0);
			this.Controls.SetChildIndex(this.zButtonCancel, 0);
			this.Controls.SetChildIndex(this.zButtonReject, 0);
			this.Controls.SetChildIndex(this.zTextBoxRejectionReason, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabelMessage, 0);
			this.Controls.SetChildIndex(this.zLabelRejectionReason, 0);
			this.Controls.SetChildIndex(this.zButtonSubmitRejectionReason, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZButton zButtonAccept;
		private ZArchitecture.GUI.ZButton zButtonCancel;
		private ZArchitecture.GUI.ZButton zButtonReject;
		private ZArchitecture.ZTextBox zTextBoxRejectionReason;
		private ZArchitecture.ZLabel zLabelMessage;
		private ZArchitecture.ZLabel zLabelRejectionReason;
		private ZArchitecture.GUI.ZButton zButtonSubmitRejectionReason;
	}
}
