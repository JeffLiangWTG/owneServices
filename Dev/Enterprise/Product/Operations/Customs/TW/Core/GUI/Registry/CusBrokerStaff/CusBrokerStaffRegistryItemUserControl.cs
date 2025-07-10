using Enterprise.Registry.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class CusBrokerStaffRegistryItemUserControl : RegistryZUserControl
	{
		public CusBrokerStaffRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			BrokerStaffCodeFindBox.ReadOnly = readOnly;
		}
	}
}
