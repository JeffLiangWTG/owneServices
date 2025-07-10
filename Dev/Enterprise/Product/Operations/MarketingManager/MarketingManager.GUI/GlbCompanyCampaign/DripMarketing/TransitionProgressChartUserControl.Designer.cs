
namespace Enterprise.MarketingManager.GUI
{
	partial class TransitionProgressChartUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.transitionedLegendRectangle = new Enterprise.ZArchitecture.ZLabel();
            this.transitionedLegendText = new Enterprise.ZArchitecture.ZLabel();
            this.notTransitionedLegendRectangle = new Enterprise.ZArchitecture.ZLabel();
            this.notTransitionedLegendText = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
            // transitionedLegendRectangle
            // 
            this.transitionedLegendRectangle.AutoSize = true;
            this.transitionedLegendRectangle.BackColor = System.Drawing.Color.FromArgb(241, 241, 241);
            this.transitionedLegendRectangle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.transitionedLegendRectangle.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("transitionedLegendRectangle", " ");
            this.transitionedLegendRectangle.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.transitionedLegendRectangle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 15, true);
            this.transitionedLegendRectangle.Name = "transitionedLegendRectangle";
            this.transitionedLegendRectangle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 15, true);
            this.transitionedLegendRectangle.TabIndex = 1;
            // 
            // transitionedLegendText
            // 
            this.transitionedLegendText.AutoSize = true;
            this.transitionedLegendText.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("transitionedLegendText", "Transitioned");
            this.transitionedLegendText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.transitionedLegendText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 15, true);
            this.transitionedLegendText.Name = "transitionedLegendText";
            this.transitionedLegendText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 13, true);
            this.transitionedLegendText.TabIndex = 2;
            // 
            // notTransitionedLegendRectangle
            // 
            this.notTransitionedLegendRectangle.AutoSize = true;
            this.notTransitionedLegendRectangle.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.notTransitionedLegendRectangle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.notTransitionedLegendRectangle.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("notTransitionedLegendRectangle", " ");
            this.notTransitionedLegendRectangle.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.notTransitionedLegendRectangle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 15, true);
            this.notTransitionedLegendRectangle.Name = "notTransitionedLegendRectangle";
            this.notTransitionedLegendRectangle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 15, true);
            this.notTransitionedLegendRectangle.TabIndex = 1;
            // 
            // notTransitionedLegendText
            // 
            this.notTransitionedLegendText.AutoSize = true;
            this.notTransitionedLegendText.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("notTransitionedLegendText", "Not Transitioned");
            this.notTransitionedLegendText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.notTransitionedLegendText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 15, true);
            this.notTransitionedLegendText.Name = "notTransitionedLegendText";
            this.notTransitionedLegendText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
            this.notTransitionedLegendText.TabIndex = 2;
            // 
            // TransitionProgressChartUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.transitionedLegendRectangle);
            this.Controls.Add(this.transitionedLegendText);
            this.Controls.Add(this.notTransitionedLegendRectangle);
            this.Controls.Add(this.notTransitionedLegendText);
            this.Name = "TransitionProgressChartUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 170, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel transitionedLegendRectangle;
		private Enterprise.ZArchitecture.ZLabel transitionedLegendText;
		private Enterprise.ZArchitecture.ZLabel notTransitionedLegendRectangle;
		private Enterprise.ZArchitecture.ZLabel notTransitionedLegendText;
	}
}
