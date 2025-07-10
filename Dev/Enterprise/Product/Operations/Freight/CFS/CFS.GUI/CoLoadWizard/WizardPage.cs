using System.Windows.Forms;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class WizardPage : ZUserControl
	{
		public WizardPage()
		{
			InitializeComponent();
		}

		public virtual CoLoadWizardSteps WizardPageStep
		{
			get { return CoLoadWizardSteps.NotSet; }
		}

		public virtual Control DefaultFocusedControl
		{
			get { return null; }
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

