using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class ChargeGroupSettingControl : RegistryZUserControl
	{
		public ChargeGroupSettingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ChargeGroupSetupGrid.ReadOnly = readOnly;
			ChargeGroupGrid.ReadOnly = true;
		}
	}
}
