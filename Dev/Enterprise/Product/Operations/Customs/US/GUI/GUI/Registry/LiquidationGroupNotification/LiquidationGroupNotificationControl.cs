using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class LiquidationGroupNotificationControl : RegistryZUserControl
	{
		public LiquidationGroupNotificationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.SendGroupGuidFindBox.ReadOnly = readOnly;
			this.SendModeDropEdit.ReadOnly = readOnly;
			this.SuppressNoChangeLiquidationCheckBox.ReadOnly = readOnly;
		}
	}
}
