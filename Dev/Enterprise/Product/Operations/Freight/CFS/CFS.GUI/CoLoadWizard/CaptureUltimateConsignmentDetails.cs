using System.Windows.Forms;

namespace Enterprise.Freight.CFS.GUI
{
	/// <summary>
	/// Summary description for UserControl1.
	/// </summary>
	public partial class CaptureUltimateConsignmentDetails : WizardPage
	{
		readonly System.ComponentModel.Container components;

		public CaptureUltimateConsignmentDetails()
		{
			InitializeComponent();
		}

		public override Business.CoLoadWizardSteps WizardPageStep
		{
			get
			{
				return Enterprise.Freight.CFS.Business.CoLoadWizardSteps.UltimateDetails;
			}
		}

		public override Control DefaultFocusedControl
		{
			get
			{
				return ShipmentHouseBillTextBox;
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

