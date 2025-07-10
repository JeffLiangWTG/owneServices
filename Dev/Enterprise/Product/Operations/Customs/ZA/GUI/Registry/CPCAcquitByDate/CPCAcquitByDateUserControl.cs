using Enterprise.Registry.GUI;

namespace Enterprise.Customs.ZA.DataRegistry.GUI
{
	public partial class CPCAcquitByDateUserControl : RegistryZUserControl
	{
		public CPCAcquitByDateUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			QuantityEdit.ReadOnly = readOnly;
			UnitEdit.ReadOnly = readOnly;
		}
	}
}
