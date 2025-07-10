using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class WizardFinishedWizardPage : WizardPage
	{
		public WizardFinishedWizardPage()
		{
			InitializeComponent();

			InformationLabel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("8888BC20-D6D6-4fcf-9FC7-32876A35AF60", "The co-load master shipment now has the sub house bill details entered.  Press finish to close this wizard and insert the shipments into the containers load list.");
#if DEBUG
			TypeDescriptor.AddAttributes(InformationLabel, new SuppressFormsLocalizedTestAttribute());
#endif

		}

		public override Business.CoLoadWizardSteps WizardPageStep
		{
			get
			{
				return Enterprise.Freight.CFS.Business.CoLoadWizardSteps.FinishWizard;
			}
		}

		#region Implementation
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

			LinearGradientBrush bKBrush = new LinearGradientBrush(StartWizardPictureBox.Bounds, SystemColors.ActiveCaption, SystemColors.ControlLight, 90.0f, false);
			e.Graphics.FillRectangle(bKBrush, StartWizardPictureBox.Bounds);
			e.Graphics.DrawRectangle(SystemPens.WindowText, StartWizardPictureBox.Bounds);
			e.Graphics.DrawImage(CoLoadWizardForm.FinishWizardImage, 14, (StartWizardPictureBox.Bottom / 2) - (CoLoadWizardForm.FinishWizardImage.Height / 2));

#endif
		}
		#endregion
	}
}
