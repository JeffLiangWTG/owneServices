using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class TextWithValidation
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
            this.pnlMain = new CargoWise.Windows.UI.KFlowLayoutPanel();
            this.lblText = new Enterprise.ZArchitecture.ZLabel();
            this.pbValidationImage = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbValidationImage)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.AutoSize = true;
            this.pnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlMain.Controls.Add(this.lblText);
            this.pnlMain.Controls.Add(this.pbValidationImage);
            this.pnlMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlMain.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 15, true);
            this.pnlMain.TabIndex = 0;
            // 
            // lblText
            // 
            this.lblText.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblText.AutoSize = true;
            this.lblText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblText.IsFontBold = true;
            this.lblText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
            this.lblText.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.lblText.Name = "lblText";
            this.lblText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
            this.lblText.TabIndex = 0;
            this.lblText.Text = "Text";
            // 
            // pbValidationImage
            // 
            this.pbValidationImage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pbValidationImage.Image = global::Enterprise.Rating.GUI.Properties.Resources.WarningDrawing;
            this.pbValidationImage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 0, true);
            this.pbValidationImage.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pbValidationImage.Name = "pbValidationImage";
            this.pbValidationImage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 15, true);
            this.pbValidationImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbValidationImage.TabIndex = 1;
            this.pbValidationImage.TabStop = false;
            this.pbValidationImage.Visible = false;
            // 
            // TextWithValidation
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlMain);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.Name = "TextWithValidation";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 15, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbValidationImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel lblText;
		private ZArchitecture.GUI.ZPictureBox pbValidationImage;
		private KFlowLayoutPanel pnlMain;
	}
}
