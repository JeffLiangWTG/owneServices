using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	public partial class WizardFinishedWizardPage : WizardPage
	{
		private ZArchitecture.ZLabel InformationLabel;
		private ZPictureBox StartWizardPictureBox;
		private Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InformationLabel = new ZArchitecture.ZLabel();
			this.StartWizardPictureBox = new ZPictureBox();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.StartWizardPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// InformationLabel
			// 
			this.InformationLabel.Location = ControlDpiScalingHelper.NewScaledPoint(184, 88, true);
			this.InformationLabel.Name = "InformationLabel";
			this.InformationLabel.Size = ControlDpiScalingHelper.NewScaledSize(216, 88, true);
			this.InformationLabel.TabIndex = 0;
			this.InformationLabel.TextAlign = ContentAlignment.TopLeft;
			// 
			// StartWizardPictureBox
			// 
			this.StartWizardPictureBox.Location = ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.StartWizardPictureBox.Name = "StartWizardPictureBox";
			this.StartWizardPictureBox.Size = ControlDpiScalingHelper.NewScaledSize(152, 304, true);
			this.StartWizardPictureBox.TabIndex = 6;
			this.StartWizardPictureBox.TabStop = false;
			this.StartWizardPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.StartWizardPictureBox_Paint);
			// 
			// WizardFinishedWizardPage
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StartWizardPictureBox);
			this.Controls.Add(this.InformationLabel);
			this.Name = "WizardFinishedWizardPage";
			this.Size = ControlDpiScalingHelper.NewScaledSize(424, 360, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			((ISupportInitialize)(this.StartWizardPictureBox)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
