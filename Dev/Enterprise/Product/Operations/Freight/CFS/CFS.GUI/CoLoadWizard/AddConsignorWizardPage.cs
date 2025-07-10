using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Windows.UI;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class AddConsignorWizardPage : WizardPage
	{
		public AddConsignorWizardPage()
		{
			InitializeComponent();
		}

		public override CoLoadWizardSteps WizardPageStep
		{
			get
			{
				return CoLoadWizardSteps.AddConsignor;
			}
		}

		#region Implemetnation

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void StartWizardPictureBox_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
#if !WINZOR

			using (var brush = new LinearGradientBrush(StartWizardPictureBox.Bounds, SystemColors.ActiveCaption, SystemColors.ControlLight, 90.0f, false))
			{
				e.Graphics.FillRectangle(brush, StartWizardPictureBox.Bounds);
				e.Graphics.DrawRectangle(SystemPens.WindowText, StartWizardPictureBox.Bounds);

				var point = new Point(ControlDpiScalingHelper.ScaleToCurrentDpiX(8), StartWizardPictureBox.Bottom - (CoLoadWizardForm.ConsignorImage.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(10)));
				e.Graphics.DrawImage(CoLoadWizardForm.ConsignorImage, point);
			}

#endif
		}

		#endregion
	}
}

