namespace Enterprise.Rating.GUI
{
	partial class RateChooserFilterStripControl
	{
		Enterprise.ZArchitecture.GUI.ZLinkLabel RatesSearchErrorsOrWarningsLink;

		void InitializeComponent()
		{
			this.RatesSearchErrorsOrWarningsLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.RawDataLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilterStripsPanel
			// 
			this.FilterStripsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 41, true);
			// 
			// RatesSearchErrorsOrWarningsLink
			// 
			this.RatesSearchErrorsOrWarningsLink.AutoSize = true;
			this.RatesSearchErrorsOrWarningsLink.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("32606236-e373-420c-8d71-6b7dbc6d09a2", "Rates Search Errors/Warnings");
			this.RatesSearchErrorsOrWarningsLink.IsFontBold = false;
			this.RatesSearchErrorsOrWarningsLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 11, true);
			this.RatesSearchErrorsOrWarningsLink.Name = "RatesSearchErrorsOrWarningsLink";
			this.RatesSearchErrorsOrWarningsLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 13, true);
			this.RatesSearchErrorsOrWarningsLink.TabIndex = 7;
			this.RatesSearchErrorsOrWarningsLink.Visible = false;
			this.RatesSearchErrorsOrWarningsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RatesSearchErrorsOrWarningsLink_LinkClicked);
			// 
			// RawDataLinkLabel
			// 
			this.RawDataLinkLabel.AutoSize = true;
			this.RawDataLinkLabel.IsFontBold = false;
			this.RawDataLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 28, true);
			this.RawDataLinkLabel.Name = "RawDataLinkLabel";
			this.RawDataLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 13, true);
			this.RawDataLinkLabel.TabIndex = 8;
			this.RawDataLinkLabel.Visible = false;
			this.RawDataLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RawDataLinkLabel_LinkClicked);
			// 
			// RateChooserFilterStripControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RawDataLinkLabel);
			this.Controls.Add(this.RatesSearchErrorsOrWarningsLink);
			this.Name = "RateChooserFilterStripControl";
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.RatesSearchErrorsOrWarningsLink, 0);
			this.Controls.SetChildIndex(this.RawDataLinkLabel, 0);
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZLinkLabel RawDataLinkLabel;
	}
}
