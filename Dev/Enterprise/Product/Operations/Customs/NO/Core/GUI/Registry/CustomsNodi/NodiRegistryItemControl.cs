using Enterprise.Registry.GUI;

namespace Enterprise.Customs.NO.Registry.GUI
{
	public partial class NodiRegistryItemControl : RegistryZUserControl
	{
		public NodiRegistryItemControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			NodiDetailsGrid.ReadOnly = readOnly;
		}
	}
}
