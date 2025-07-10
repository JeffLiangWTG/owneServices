namespace Enterprise.Freight.Agency.GUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CancelRequestActionForm));
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
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 146, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 24, true);
            // 
            // zButtonAccept
            // 
            this.zButtonAccept.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("4f2e04da-f037-4ce0-a42e-b14a2223a97f", "Accept");
            this.zButtonAccept.IsCaptionOverridden = true;
            this.zButtonAccept.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 60, true);
            this.zButtonAccept.Name = "zButtonAccept";
            this.zButtonAccept.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.zButtonAccept.TabIndex = 1;
            this.zButtonAccept.Text = Enterprise.Freight.Agency.GUI.Res.GetString("4f2e04da-f037-4ce0-a42e-b14a2223a97f", "Accept");
            this.zButtonAccept.ToolTipCaption = null;
            this.zButtonAccept.UseVisualStyleBackColor = true;
            this.zButtonAccept.Click += new System.EventHandler(this.zButtonAccept_Click);
            // 
            // zButtonCancel
            // 
            this.zButtonCancel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("f1013999-e9b9-4fed-b7fe-4dbb32452ac6", "Cancel");
            this.zButtonCancel.IsCaptionOverridden = true;
            this.zButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 60, true);
            this.zButtonCancel.Name = "zButtonCancel";
            this.zButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.zButtonCancel.TabIndex = 3;
            this.zButtonCancel.Text = Enterprise.Freight.Agency.GUI.Res.GetString("f1013999-e9b9-4fed-b7fe-4dbb32452ac6", "Cancel");
            this.zButtonCancel.ToolTipCaption = null;
            this.zButtonCancel.UseVisualStyleBackColor = true;
            this.zButtonCancel.Click += new System.EventHandler(this.zButtonCancel_Click);
            // 
            // zButtonReject
            // 
            this.zButtonReject.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("cb0098c9-9fd0-438d-839b-ba626ccd50f0", "Reject");
            this.zButtonReject.IsCaptionOverridden = true;
            this.zButtonReject.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 60, true);
            this.zButtonReject.Name = "zButtonReject";
            this.zButtonReject.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.zButtonReject.TabIndex = 2;
            this.zButtonReject.Text = Enterprise.Freight.Agency.GUI.Res.GetString("cb0098c9-9fd0-438d-839b-ba626ccd50f0", "Reject");
            this.zButtonReject.ToolTipCaption = null;
            this.zButtonReject.UseVisualStyleBackColor = true;
            this.zButtonReject.Click += new System.EventHandler(this.zButtonReject_Click);
            // 
            // zTextBoxRejectionReason
            // 
            this.zTextBoxRejectionReason.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("zTextBoxRejectionReason.CaptionResourceString")));
            this.zTextBoxRejectionReason.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.zTextBoxRejectionReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 112, true);
            this.zTextBoxRejectionReason.Name = "zTextBoxRejectionReason";
            this.zTextBoxRejectionReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 18, true);
            this.zTextBoxRejectionReason.TabIndex = 4;
            this.zTextBoxRejectionReason.Visible = false;
            // 
            // zLabelMessage
            // 
            this.zLabelMessage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ed6a7455-1f24-4ce4-9da2-848f8abd897d", "A Booking Request Withdrawal/Cancellation message was received for this booking. Do you accept the withdrawal/cancellation, reject the withdrawal/cancellation or do you want to cancel this dialog and review the booking before taking an action?");
            this.zLabelMessage.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabelMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
            this.zLabelMessage.Name = "zLabelMessage";
            this.zLabelMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 44, true);
            this.zLabelMessage.TabIndex = 0;
            this.zLabelMessage.Text = Enterprise.Freight.Agency.GUI.Res.GetString("ed6a7455-1f24-4ce4-9da2-848f8abd897d", "A Booking Request Withdrawal/Cancellation message was received for this booking. Do you accept the withdrawal/cancellation, reject the withdrawal/cancellation or do you want to cancel this dialog and review the booking before taking an action?");
            // 
            // zLabelRejectionReason
            // 
            this.zLabelRejectionReason.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("86bdc7e5-33f4-4cc0-b316-ad62e991ebf1", "Rejection Reason:");
            this.zLabelRejectionReason.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabelRejectionReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 86, true);
            this.zLabelRejectionReason.Name = "zLabelRejectionReason";
            this.zLabelRejectionReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
            this.zLabelRejectionReason.TabIndex = 0;
            this.zLabelRejectionReason.Text = Enterprise.Freight.Agency.GUI.Res.GetString("86bdc7e5-33f4-4cc0-b316-ad62e991ebf1", "Rejection Reason:");
            this.zLabelRejectionReason.Visible = false;
            // 
            // zButtonSubmitRejectionReason
            // 
            this.zButtonSubmitRejectionReason.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("22a8ab1a-707c-45cf-8688-540a51a9181c", "Submit");
            this.zButtonSubmitRejectionReason.IsCaptionOverridden = true;
            this.zButtonSubmitRejectionReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 112, true);
            this.zButtonSubmitRejectionReason.Name = "zButtonSubmitRejectionReason";
            this.zButtonSubmitRejectionReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.zButtonSubmitRejectionReason.TabIndex = 5;
            this.zButtonSubmitRejectionReason.Text = Enterprise.Freight.Agency.GUI.Res.GetString("22a8ab1a-707c-45cf-8688-540a51a9181c", "Submit");
            this.zButtonSubmitRejectionReason.ToolTipCaption = null;
            this.zButtonSubmitRejectionReason.UseVisualStyleBackColor = true;
            this.zButtonSubmitRejectionReason.Visible = false;
            this.zButtonSubmitRejectionReason.Click += new System.EventHandler(this.zButtonSubmitRejectionReason_Click);
            // 
            // CancelRequestActionForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("8fe739aa-24da-4f0e-9a29-d1ef1928c322", "Booking Withdrawal/Cancellation");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 170, true);
            this.Controls.Add(this.zButtonSubmitRejectionReason);
            this.Controls.Add(this.zLabelRejectionReason);
            this.Controls.Add(this.zLabelMessage);
            this.Controls.Add(this.zTextBoxRejectionReason);
            this.Controls.Add(this.zButtonReject);
            this.Controls.Add(this.zButtonCancel);
            this.Controls.Add(this.zButtonAccept);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "CancelRequestActionForm";
            this.ShowIcon = false;
            this.Text = Enterprise.Freight.Agency.GUI.Res.GetString("8fe739aa-24da-4f0e-9a29-d1ef1928c322", "Booking Withdrawal/Cancellation");
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
