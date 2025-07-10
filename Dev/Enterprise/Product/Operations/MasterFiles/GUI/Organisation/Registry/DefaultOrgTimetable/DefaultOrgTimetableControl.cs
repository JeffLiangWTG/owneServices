using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DefaultOrgTimetableControl : RegistryZUserControl
	{
		public DefaultOrgTimetableControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CountryGrid.ReadOnly = readOnly;
			TimetableGrid.ReadOnly = readOnly;
		}
	}
}
