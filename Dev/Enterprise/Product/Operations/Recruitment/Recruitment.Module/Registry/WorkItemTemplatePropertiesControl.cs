using Enterprise.Registry.GUI;

namespace Enterprise.Recruitment.Registry
{
	public partial class WorkItemTemplatePropertiesControl : RegistryZUserControl
	{
		public WorkItemTemplatePropertiesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			WorkItemTemplatePropertiesGrid.ReadOnly = readOnly;
		}
	}
}
