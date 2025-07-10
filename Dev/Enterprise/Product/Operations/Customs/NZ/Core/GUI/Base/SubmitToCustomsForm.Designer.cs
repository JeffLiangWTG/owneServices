
namespace Enterprise.Customs.NZ.GUI.Base
{
	partial class SubmitToCustomsForm
	{
		protected Enterprise.ZArchitecture.GUI.ZButton CancelBtn;
		public Enterprise.ZArchitecture.GUI.ZButton OkButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.RemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectDefaultRemarksButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemarksGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.RemarksGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 218, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 25, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(196);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(197);
			// 
			// RemarksTextBox
			// 
			this.RemarksTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RemarksTextBox.BindTo = "EnteredRemarks";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.NZ.Business.MessageBuilders.MessageManagerForClearance)(null)).EnteredRemarksInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.MessageBuilders.MessageManagerForClearance)(null)).EnteredRemarks)));
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.RemarksTextBox.Multiline = true;
			this.RemarksTextBox.Name = "RemarksTextBox";
			this.RemarksTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 95, true);
			this.RemarksTextBox.TabIndex = 0;
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 189, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.CancelBtn.TabIndex = 3;
			this.CancelBtn.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("90b647dc-e326-4347-ad7a-a054584e02ac", "Cancel");
			this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 189, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.OkButton.TabIndex = 2;
			this.OkButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("d7d22e7f-3db6-49c2-9c2e-23cef8071e1d", "OK");
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// SelectDefaultRemarksButton
			// 
			this.SelectDefaultRemarksButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectDefaultRemarksButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 120, true);
			this.SelectDefaultRemarksButton.Name = "SelectDefaultRemarksButton";
			this.SelectDefaultRemarksButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 23, true);
			this.SelectDefaultRemarksButton.TabIndex = 1;
			this.SelectDefaultRemarksButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("edd71207-3c9c-420e-b800-891ed09acdf1", "Select Default Remarks");
			this.SelectDefaultRemarksButton.Click += new System.EventHandler(this.SelectDefaultRemarksButton_Click);
			// 
			// RemarksGroupBox
			// 
			this.RemarksGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RemarksGroupBox.Controls.Add(this.RemarksTextBox);
			this.RemarksGroupBox.Controls.Add(this.SelectDefaultRemarksButton);
			this.RemarksGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 34, true);
			this.RemarksGroupBox.Name = "RemarksGroupBox";
			this.RemarksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 149, true);
			this.RemarksGroupBox.TabIndex = 1;
			this.RemarksGroupBox.TabStop = false;
			this.RemarksGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("3eaccb84-f939-4b36-b135-24b1434f0cde", "Remarks (Sent to Customs)");
			// 
			// MessageTypeLabel
			// 
			this.MessageTypeLabel.IsFontBold = true;
			this.MessageTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.MessageTypeLabel.Name = "MessageTypeLabel";
			this.MessageTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 22, true);
			this.MessageTypeLabel.TabIndex = 0;
			this.MessageTypeLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("18d4699b-0970-4745-adbc-75e4497b50b5", "Ready to Send Original Message");
			// 
			// SubmitToCustomsForm
			// 
			
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 243, true);
			this.Controls.Add(this.MessageTypeLabel);
			this.Controls.Add(this.RemarksGroupBox);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.OkButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceType = typeof(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManagerForClearance);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.MessageBuilders.MessageManagerForClearance";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 193, true);
			this.Name = "SubmitToCustomsForm";
			this.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("7063ee79-3a6c-4cc5-a3c3-95afd196c432", "Send Message To Customs");
			this.CaptionRenderingEnabled = true;
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.RemarksGroupBox, 0);
			this.Controls.SetChildIndex(this.MessageTypeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.RemarksGroupBox.ResumeLayout(false);
			this.RemarksGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		public Enterprise.ZArchitecture.GUI.ZButton SelectDefaultRemarksButton;
		protected internal Enterprise.ZArchitecture.ZTextBox RemarksTextBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox RemarksGroupBox;
		protected Enterprise.ZArchitecture.ZLabel MessageTypeLabel;
	}
}
