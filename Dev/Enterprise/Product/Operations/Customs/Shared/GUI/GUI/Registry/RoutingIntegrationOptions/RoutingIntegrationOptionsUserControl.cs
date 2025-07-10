using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DataRegistry.GUI
{
	public partial class RoutingIntegrationOptionsUserControl : RegistryZUserControl
	{
		public RoutingIntegrationOptionsUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			this.AlwaysLinkRadioButton.ReadOnly = readOnly;
			this.ConditionalLinkRadioButton.ReadOnly = readOnly;
			this.NeverLinkRadioButton.ReadOnly = readOnly;
		}
	}
}
