using Enterprise.Registry.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class IssueTypeMappingControl : RegistryZUserControl
	{
		public IssueTypeMappingControl()
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
