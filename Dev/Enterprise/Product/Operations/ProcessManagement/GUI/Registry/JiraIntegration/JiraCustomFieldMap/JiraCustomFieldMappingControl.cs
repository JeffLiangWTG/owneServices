using Enterprise.Registry.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class JiraCustomFieldMappingControl : RegistryZUserControl
	{
		public JiraCustomFieldMappingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			IssueTypesMappingGrid.ReadOnly = readOnly;
		}
	}
}
