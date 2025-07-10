using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	partial class CostBreakdownItemControl
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
            this.costAndDescriptionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.costStringLabel = new Enterprise.ZArchitecture.ZLabel();
            this.chargeCodeDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.costAndDescriptionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.CostBreakdownChargeViewModel);
            // 
            // costAndDescriptionPanel
            // 
            this.costAndDescriptionPanel.AutoSize = true;
            this.costAndDescriptionPanel.Controls.Add(this.costStringLabel);
            this.costAndDescriptionPanel.Controls.Add(this.chargeCodeDescriptionLabel);
            this.costAndDescriptionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.costAndDescriptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.costAndDescriptionPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.costAndDescriptionPanel.Name = "costAndDescriptionPanel";
            this.costAndDescriptionPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 0, true);
            this.costAndDescriptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 19, true);
            this.costAndDescriptionPanel.TabIndex = 0;
            this.costAndDescriptionPanel.SizeChanged += new System.EventHandler(this.costAndDescriptionPanel_SizeChanged);
            // 
            // costStringLabel
            // 
            this.costStringLabel.AutoSize = true;
            this.BindingSource.SetBindingMember(this.costStringLabel, "CostString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.CostBreakdownChargeViewModel)(null)).CostString)));
            this.costStringLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.costStringLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.costStringLabel.ForeColor = System.Drawing.Color.DimGray;
            this.costStringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 3, true);
            this.costStringLabel.Name = "costStringLabel";
            this.costStringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
            this.costStringLabel.TabIndex = 1;
            this.costStringLabel.Text = "CostString";
            this.costStringLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.costStringLabel.UseMnemonic = false;
            this.costStringLabel.SizeChanged += new System.EventHandler(this.costStringLabel_SizeChanged);
            // 
            // chargeCodeDescriptionLabel
            // 
            this.chargeCodeDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chargeCodeDescriptionLabel.AutoSize = true;
            this.chargeCodeDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.chargeCodeDescriptionLabel.ForeColor = System.Drawing.Color.DimGray;
            this.chargeCodeDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
            this.chargeCodeDescriptionLabel.Name = "chargeCodeDescriptionLabel";
            this.chargeCodeDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 13, true);
            this.chargeCodeDescriptionLabel.TabIndex = 0;
            this.chargeCodeDescriptionLabel.Text = "ChargeCodeDescription";
            this.chargeCodeDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chargeCodeDescriptionLabel.UseMnemonic = false;
            // 
            // CostBreakdownItemControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.costAndDescriptionPanel);
            this.Name = "CostBreakdownItemControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 19, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.costAndDescriptionPanel.ResumeLayout(false);
            this.costAndDescriptionPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel costAndDescriptionPanel;
		private ZArchitecture.ZLabel chargeCodeDescriptionLabel;
		private ZArchitecture.ZLabel costStringLabel;
	}
}
