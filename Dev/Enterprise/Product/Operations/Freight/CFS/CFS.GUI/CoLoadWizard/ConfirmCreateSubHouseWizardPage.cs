using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ConfirmCreateSubHouseWizardPage : WizardPage
	{
		public ConfirmCreateSubHouseWizardPage()
		{
			InitializeComponent();

			InformationLabel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("C1E3CA4F-C785-4e44-89F3-AEBEE0A21168", "Please review the information below for the sub house shipment that will be added to the Co-Loads for Shipment.  If any information is incorrect, please press back to fix.  To add this shipment, press Next");
#if DEBUG
			TypeDescriptor.AddAttributes(InformationLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public override Business.CoLoadWizardSteps WizardPageStep
		{
			get
			{
				return Enterprise.Freight.CFS.Business.CoLoadWizardSteps.AddColoadShipment;
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

