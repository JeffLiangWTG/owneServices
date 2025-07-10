using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DataRegistry.GUI
{
	public partial class AutoBillingGroupNotificationUserControl : RegistryZUserControl
	{
		public AutoBillingGroupNotificationUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			this.SendGroupGuidFindBox.ReadOnly = readOnly;
			this.SuspendUnpostARNotificationCheckBox.ReadOnly = readOnly;
		}
	}
}
