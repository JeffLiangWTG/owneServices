using CargoWiseOne.ResourceStrings;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	partial class AlertHyperLinkForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlertHyperLinkForm));
            this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ShowLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.HyperlinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.ShowLabel2 = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 151, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 24, true);
            // 
            // OKButton
            // 
            this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("AlertHyperLink|a30bf2e7-7401-40ec-a5c9-984a2a5e0bfe", "OK");
            this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 103, true);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 24, true);
            this.OKButton.TabIndex = 0;
            this.OKButton.ToolTipCaption = null;
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("AlertHyperLink|bd1deadb-496c-4265-8863-74f065a1a221", "Cancel");
            this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 104, true);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
            this.CancelBtn.TabIndex = 1;
            this.CancelBtn.ToolTipCaption = null;
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ShowLabel1
            // 
            this.ShowLabel1.AutoSize = true;
            this.ShowLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.ShowLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 26, true);
            this.ShowLabel1.Name = "ShowLabel1";
            this.ShowLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 37, true);
            this.ShowLabel1.TabIndex = 2;
            this.ShowLabel1.Text = resources.GetString("ShowLabel1.Text");
            // 
            // HyperlinkLabel
            // 
            this.HyperlinkLabel.AutoSize = true;
            this.HyperlinkLabel.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("AlertHyperLinkForm|b4415f93-2b56-4582-b829-fc0642a459b3", "View Details");
            this.HyperlinkLabel.IsFontBold = false;
            this.HyperlinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 67, true);
            this.HyperlinkLabel.Name = "HyperlinkLabel";
            this.HyperlinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 13, true);
            this.HyperlinkLabel.TabIndex = 3;
            this.HyperlinkLabel.Text = "https://wisetechacademy.com/search?quickstart=c3c83405-4c62-4988-9c24-7dbbe85bf9c" +
    "5";
            this.HyperlinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HyperlinkLabelClick);
            // 
            // ShowLabel2
            // 
            this.ShowLabel2.AutoSize = true;
            this.ShowLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.ShowLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 80, true);
            this.ShowLabel2.Name = "ShowLabel2";
            this.ShowLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 12, true);
            this.ShowLabel2.TabIndex = 4;
            this.ShowLabel2.Text = "Click OK to continue with document delivery.";
            // 
            // AlertHyperLinkForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 175, true);
            this.Controls.Add(this.ShowLabel1);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.HyperlinkLabel);
            this.Controls.Add(this.ShowLabel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "AlertHyperLinkForm";
            this.Text = "Confirmation";
            this.Controls.SetChildIndex(this.ShowLabel2, 0);
            this.Controls.SetChildIndex(this.HyperlinkLabel, 0);
            this.Controls.SetChildIndex(this.OKButton, 0);
            this.Controls.SetChildIndex(this.CancelBtn, 0);
            this.Controls.SetChildIndex(this.ShowLabel1, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZButton OKButton;
		protected ZArchitecture.GUI.ZButton CancelBtn;
		protected Enterprise.ZArchitecture.ZLabel ShowLabel1;
		protected Enterprise.ZArchitecture.GUI.ZLinkLabel HyperlinkLabel;
		protected Enterprise.ZArchitecture.ZLabel ShowLabel2;
	}
}
