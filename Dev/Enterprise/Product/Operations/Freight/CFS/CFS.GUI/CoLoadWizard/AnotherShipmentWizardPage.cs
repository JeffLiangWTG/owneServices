using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class AnotherShipmentWizardPage : WizardPage
	{
		public AnotherShipmentWizardPage()
		{
			InitializeComponent();

			BottomInfoLabel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("1D9A7599-0E13-4e5c-BDB8-1ED58DB25ED4", "To add another shipment to the co-load master, tick the checkbox below and press next.  If the last shipment has been added, make sure the checkbox is unticked and press next.");

#if DEBUG
			TypeDescriptor.AddAttributes(BottomInfoLabel, new SuppressFormsLocalizedTestAttribute());
#endif

		}

		public override Business.CoLoadWizardSteps WizardPageStep
		{
			get
			{
				return Enterprise.Freight.CFS.Business.CoLoadWizardSteps.AnotherShipment;
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
		#endregion
	}
}

