using Enterprise.Registry.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class CusBrokerageBoxNumberRegistryItemUserControl : RegistryZUserControl
	{
		public CusBrokerageBoxNumberRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			MainGrid.ReadOnly = readOnly;
		}
	}
}
