using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class AirlineLogoControl
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
            this.fallbackPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.carrierCodeLabel = new Enterprise.ZArchitecture.ZLabel();
            this.zPictureBox2 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fallbackPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zPictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel);
            // 
            // fallbackPictureBox
            // 
            this.fallbackPictureBox.Image = global::Enterprise.Rating.GUI.Properties.Resources.LogoFallback;
            this.fallbackPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.fallbackPictureBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.fallbackPictureBox.Name = "fallbackPictureBox";
            this.fallbackPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 42, true);
            this.fallbackPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.fallbackPictureBox.TabIndex = 0;
            this.fallbackPictureBox.TabStop = false;
            // 
            // carrierCodeLabel
            // 
            this.carrierCodeLabel.BackColor = System.Drawing.Color.Transparent;
            this.BindingSource.SetBindingMember(this.carrierCodeLabel, "CarrierCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel)(null)).CarrierCode)));
            this.carrierCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.carrierCodeLabel.ForeColor = System.Drawing.Color.White;
            this.carrierCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.carrierCodeLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.carrierCodeLabel.Name = "carrierCodeLabel";
            this.carrierCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 42, true);
            this.carrierCodeLabel.TabIndex = 1;
            this.carrierCodeLabel.Text = "ZZ";
            this.carrierCodeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // zPictureBox2
            // 
            this.zPictureBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zPictureBox2.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.zPictureBox2.Name = "zPictureBox2";
            this.zPictureBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 42, true);
            this.zPictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.zPictureBox2.TabIndex = 2;
            this.zPictureBox2.TabStop = false;
            // 
            // AirlineLogoControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.carrierCodeLabel);
            this.Controls.Add(this.fallbackPictureBox);
            this.Controls.Add(this.zPictureBox2);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.Name = "AirlineLogoControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 42, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fallbackPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zPictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPictureBox fallbackPictureBox;
		private ZArchitecture.ZLabel carrierCodeLabel;
		private ZArchitecture.GUI.ZPictureBox zPictureBox2;
	}
}
