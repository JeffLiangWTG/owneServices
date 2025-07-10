using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class DefaultFilerContactInformationControl : RegistryZUserControl
	{
		public DefaultFilerContactInformationControl()
		{
			InitializeComponent();
		}

		public new DefaultFilerContactInformation CurrentDataItem
		{
			get => (DefaultFilerContactInformation)base.CurrentDataItem;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ContactNameTextBox.ReadOnly = readOnly;
			ContactPhoneTextBox.ReadOnly = readOnly;
		}
	}
}
