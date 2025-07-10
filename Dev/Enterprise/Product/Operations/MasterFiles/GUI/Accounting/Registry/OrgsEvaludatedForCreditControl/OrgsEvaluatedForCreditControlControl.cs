using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgsEvaluatedForCreditControlControl : RegistryZUserControl
	{
		public OrgsEvaluatedForCreditControlControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			OrgsEvaluatedForCreditControlGrid.ReadOnly = readOnly;
		}
	}
}
