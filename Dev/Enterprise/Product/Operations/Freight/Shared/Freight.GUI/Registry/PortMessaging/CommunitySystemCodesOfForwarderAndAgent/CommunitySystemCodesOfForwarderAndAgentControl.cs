using Enterprise.Registry.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class CommunitySystemCodesOfForwarderAndAgentControl : RegistryZUserControl
	{
		public CommunitySystemCodesOfForwarderAndAgentControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CommunitySystemCodesOfForwarderAndAgentGrid.ReadOnly = readOnly;
		}
	}
}
