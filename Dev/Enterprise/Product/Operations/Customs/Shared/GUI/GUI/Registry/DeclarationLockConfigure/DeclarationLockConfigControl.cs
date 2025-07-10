using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DataRegistry.GUI
{
	public sealed partial class DeclarationLockConfigControl : RegistryZUserControl
	{
		public DeclarationLockConfigControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			LockConfigGrid.ReadOnly = readOnly;
			TabLockInfoGrid.ReadOnly = readOnly;
			EventLockInfoGrid.ReadOnly = readOnly;
		}
	}
}
