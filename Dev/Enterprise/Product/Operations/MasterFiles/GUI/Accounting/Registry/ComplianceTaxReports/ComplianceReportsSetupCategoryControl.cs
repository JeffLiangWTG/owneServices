using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ComplianceReportsSetupCategoryControl : RegistryZUserControl
	{
		public ComplianceReportsSetupCategoryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ReportCategoryGrid.ReadOnly = ReportTypeGrid.ReadOnly = readOnly;
		}
	}
}
