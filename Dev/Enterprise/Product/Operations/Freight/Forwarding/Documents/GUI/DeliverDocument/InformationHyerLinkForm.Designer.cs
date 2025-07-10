namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	partial class InformationHyerLinkForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InformationHyerLinkForm));
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HyperlinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.SuspendLayout();
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 26, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 37, true);
			this.MessageLabel.TabIndex = 0;
			this.MessageLabel.Text = this.MessageBody;
			// 
			// HyperlinkLabel
			// 
			this.HyperlinkLabel.AutoSize = true;
			this.HyperlinkLabel.IsFontBold = false;
			this.HyperlinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 80, true);
			this.HyperlinkLabel.Name = "HyperlinkLabel";
			this.HyperlinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 13, true);
			this.HyperlinkLabel.TabIndex = 1;
			this.HyperlinkLabel.Text = this.MessageLink;
			this.HyperlinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HyperlinkLabelClick);
			//// 
			//// OKButton
			//// 
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("AlertHyperLink|a30bf2e7-7401-40ec-a5c9-984a2a5e0bfe", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 120, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 24, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// InformationHyerLinkForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 175, true);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "InformationHyerLinkForm";
			this.Text = "";
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.HyperlinkLabel);
			this.Controls.Add(this.OKButton);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.HyperlinkLabel, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZArchitecture.GUI.ZButton OKButton;
		protected Enterprise.ZArchitecture.ZLabel MessageLabel;
		protected Enterprise.ZArchitecture.GUI.ZLinkLabel HyperlinkLabel;
	}
}
