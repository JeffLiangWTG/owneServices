
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	partial class DeliverDocumentPopupForm
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
		private new void InitializeComponent()
		{
            this.SendMessageButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.DeliverDocumentButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ShowLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 151, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 24, true);
			// 
			// SendMessageButton
			// 
			this.SendMessageButton.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("DeliverDocumentPopup|A2D9CC0B-5A22-4A2B-B148-E5FDB5D2535C", "Send Message");
            this.SendMessageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 103, true);
            this.SendMessageButton.Name = "SendMessageButton";
            this.SendMessageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 24, true);
            this.SendMessageButton.TabIndex = 0;
            this.SendMessageButton.ToolTipCaption = null;
            this.SendMessageButton.UseVisualStyleBackColor = true;
            this.SendMessageButton.Click += new System.EventHandler(this.SendMessageButton_Click);
            // 
            // DeliverDocumentButton
            // 
            this.DeliverDocumentButton.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("DeliverDocumentPopup|B5B60189-9C78-452B-9993-C1E77A36F4AB", "Deliver Document");
            this.DeliverDocumentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 104, true);
            this.DeliverDocumentButton.Name = "DeliverDocumentButton";
            this.DeliverDocumentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
            this.DeliverDocumentButton.TabIndex = 1;
            this.DeliverDocumentButton.ToolTipCaption = null;
            this.DeliverDocumentButton.UseVisualStyleBackColor = true;
            this.DeliverDocumentButton.Click += new System.EventHandler(this.DeliverDocumentButton_Click);

			// 
			// ShowLabel
			// 
			this.ShowLabel.AutoSize = true;
            this.ShowLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.ShowLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 26, true);
            this.ShowLabel.Name = "ShowLabel";
            this.ShowLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 12, true);
            this.ShowLabel.TabIndex = 3;

			// 
			// DeliverDocumentPopup
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 175, true);
            this.Controls.Add(this.ShowLabel);
            this.Controls.Add(this.DeliverDocumentButton);
            this.Controls.Add(this.SendMessageButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DeliverDocumentPopup";
            this.Text = "Selecting Deliver Document option";
            this.Controls.SetChildIndex(this.SendMessageButton, 0);
            this.Controls.SetChildIndex(this.DeliverDocumentButton, 0);
            this.Controls.SetChildIndex(this.ShowLabel, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZButton SendMessageButton;
		protected ZArchitecture.GUI.ZButton DeliverDocumentButton;
		protected Enterprise.ZArchitecture.ZLabel ShowLabel;
	}
}
