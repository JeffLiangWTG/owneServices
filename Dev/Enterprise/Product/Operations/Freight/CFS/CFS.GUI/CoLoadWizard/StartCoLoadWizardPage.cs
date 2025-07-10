using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Windows.UI;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI
{
	/// <summary>
	/// Summary description for StartCoLoadWizardPage.
	/// </summary>
	public partial class StartCoLoadWizardPage : WizardPage
	{
		public StartCoLoadWizardPage()
		{
			InitializeComponent();

			WelcomeLabel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("3241D926-2816-457f-9A36-96195207C8AA", "Welcome to the Co-Load Shipments Wizard.  This wizard will assist you in entering the ultimate consignee details for declaring to customs");
			DirectionsLabel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("9E5F42E6-8AE0-4443-9A28-E7D714BCA70F", "To start this wizard you need the ultimate consignee detail from the forwarder.  When you have the details ready, click Next to begin the wizard.  If you are waiting for the details to be faxed across, you can cancel this wizard and restart it from the Sea Cargo Menu.");
		}

		public override CoLoadWizardSteps WizardPageStep
		{
			get
			{
				return CoLoadWizardSteps.WelcomeStep;
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

			using (var brush = new LinearGradientBrush(StartWizardPictureBox.Bounds, SystemColors.ActiveCaption, SystemColors.ControlLightLight, 90.0f, false))
			{
				e.Graphics.FillRectangle(brush, StartWizardPictureBox.Bounds);
			}

			var frameBorder = StartWizardPictureBox.Bounds;
			frameBorder.Inflate(ControlDpiScalingHelper.NewScaledSize(-1, -1));
			e.Graphics.DrawRectangle(SystemPens.WindowFrame, frameBorder);

			var wizardStartPic = CoLoadWizardForm.CoLoadWizardStartImage;

			var point = new Point(StartWizardPictureBox.Left + ControlDpiScalingHelper.ScaleToCurrentDpiX(4), StartWizardPictureBox.Bottom - (wizardStartPic.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(15)));
			e.Graphics.DrawImage(wizardStartPic, point);

#endif
		}

		#endregion
	}
}

