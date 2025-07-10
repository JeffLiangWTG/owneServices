namespace Enterprise.Rating.GUI
{
	public partial class WizardPageStart
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.imageBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.titleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.descriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imageBox)).BeginInit();
			this.SuspendLayout();
			// 
			// imageBox
			// 
			this.imageBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.imageBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.imageBox.Name = "imageBox";
			this.imageBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 270, true);
			this.imageBox.TabIndex = 0;
			this.imageBox.TabStop = false;
			// 
			// titleLabel
			// 
			this.titleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.titleLabel.IsFontBold = true;
			this.titleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 56, true);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 23, true);
			this.titleLabel.TabIndex = 1;
			// 
			// descriptionLabel
			// 
			this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.descriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 96, true);
			this.descriptionLabel.Name = "descriptionLabel";
			this.descriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 158, true);
			this.descriptionLabel.TabIndex = 2;
			this.descriptionLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// WizardPageStart
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.descriptionLabel);
			this.Controls.Add(this.titleLabel);
			this.Controls.Add(this.imageBox);
			this.Name = "WizardPageStart";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 275, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imageBox)).EndInit();
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.GUI.ZPictureBox imageBox;
		private Enterprise.ZArchitecture.ZLabel titleLabel;
		private Enterprise.ZArchitecture.ZLabel descriptionLabel;
	}
}
