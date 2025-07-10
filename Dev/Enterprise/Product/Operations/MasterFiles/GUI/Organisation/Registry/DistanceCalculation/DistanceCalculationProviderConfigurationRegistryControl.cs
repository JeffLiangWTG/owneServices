using System.Windows.Forms;
using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class DistanceCalculationProviderConfigurationRegistryControl : RegistryZUserControl
	{
		public DistanceCalculationProviderConfigurationRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			foreach (Control control in this.Controls)
			{
				control.Enabled = !ReadOnly;
			}
		}
	}
}
