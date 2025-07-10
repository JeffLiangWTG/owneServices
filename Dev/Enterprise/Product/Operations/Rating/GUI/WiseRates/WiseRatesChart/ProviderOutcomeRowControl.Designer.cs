using System.Drawing;

namespace Enterprise.Rating.GUI
{
	partial class ProviderOutcomeRowControl
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
            this.lblProviderName = new Enterprise.ZArchitecture.ZLabel();
            this.lblRatesSeconds = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // labelRectangle
            // 
            this.labelRectangle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelRectangle.BackColor = System.Drawing.Color.Red;
            this.labelRectangle.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("labelRectangle", " ");
            this.labelRectangle.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.labelRectangle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 7, true);
            this.labelRectangle.Name = "labelRectangle";
            this.labelRectangle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 11, true);
            this.labelRectangle.TabIndex = 0;
            // 
            // lblProviderName
            // 
            this.lblProviderName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProviderName.AutoSize = true;
            this.lblProviderName.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("lblProviderName", "WiseTech Global");
            this.lblProviderName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblProviderName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(41, 7, true);
            this.lblProviderName.Name = "lblProviderName";
            this.lblProviderName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.lblProviderName.TabIndex = 1;
            // 
            // lblRatesSeconds
            // 
            this.lblRatesSeconds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRatesSeconds.AutoSize = true;
            this.lblRatesSeconds.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("lblRatesSeconds", "66 rates (980.65 secs)");
            this.lblRatesSeconds.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblRatesSeconds.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 7, true);
            this.lblRatesSeconds.Name = "lblRatesSeconds";
            this.lblRatesSeconds.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.lblRatesSeconds.TabIndex = 2;
            // 
            // ProviderOutcomeRowControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.lblRatesSeconds);
            this.Controls.Add(this.lblProviderName);
            this.Controls.Add(this.labelRectangle);
            this.Name = "ProviderOutcomeRowControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 28, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel labelRectangle;
		private Enterprise.ZArchitecture.ZLabel lblProviderName;
		private Enterprise.ZArchitecture.ZLabel lblRatesSeconds;
	}
}
