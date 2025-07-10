using Enterprise.Registry.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class ProjectCategoryMappingControl : RegistryZUserControl
	{
		public ProjectCategoryMappingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			ProjectCategoriesMappingGrid.ReadOnly = readOnly;
		}
	}
}
