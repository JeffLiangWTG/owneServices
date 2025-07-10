namespace Enterprise.MarketingManager.GUI
{
	partial class TrackingStatusChartRowUserControl
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
		private void InitializeComponent()
		{
			this.labelRectangle = new Enterprise.ZArchitecture.ZLabel();
			this.labelItemText = new Enterprise.ZArchitecture.ZLabel();
			this.labelEmailsCount = new Enterprise.ZArchitecture.ZLabel();
			this.labelClientsCount = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// labelRectangle
			// 
			this.labelRectangle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.labelRectangle.BackColor = System.Drawing.Color.Red;
			this.labelRectangle.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("trackingStatusChartRowlabelRectangle", " ");
			this.labelRectangle.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.labelRectangle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 7, true);
			this.labelRectangle.Name = "labelRectangle";
			this.labelRectangle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 11, true);
			this.labelRectangle.TabIndex = 0;
			// 
			// labelItemText
			// 
			this.labelItemText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.labelItemText.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelItemText", "Opp Queued");
			this.labelItemText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.labelItemText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(41, 7, true);
			this.labelItemText.Name = "labelItemText";
			this.labelItemText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 13, true);
			this.labelItemText.TabIndex = 1;
			this.labelItemText.Text = "TEST";
			// 
			// labelEmailsCount
			// 
			this.labelEmailsCount.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.labelEmailsCount.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelEmailsCount", "0");
			this.labelEmailsCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.labelEmailsCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 7, true);
			this.labelEmailsCount.Name = "labelEmailsCount";
			this.labelEmailsCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.labelEmailsCount.TabIndex = 2;
			this.labelEmailsCount.Text = "0";
			// 
			// labelClientsCount
			// 
			this.labelClientsCount.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.labelClientsCount.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelClientsCount", "0");
			this.labelClientsCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.labelClientsCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 7, true);
			this.labelClientsCount.Name = "labelClientsCount";
			this.labelClientsCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.labelClientsCount.TabIndex = 3;
			this.labelClientsCount.Text = "0";
			// 
			// TrackingStatusChartRowUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.labelRectangle);
			this.Controls.Add(this.labelItemText);
			this.Controls.Add(this.labelEmailsCount);
			this.Controls.Add(this.labelClientsCount);
			this.Name = "TrackingStatusChartRowUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 28, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZLabel labelRectangle;
		private Enterprise.ZArchitecture.ZLabel labelItemText;
		private Enterprise.ZArchitecture.ZLabel labelEmailsCount;
		private Enterprise.ZArchitecture.ZLabel labelClientsCount;

		#endregion
	}
}
