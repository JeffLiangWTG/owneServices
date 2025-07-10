using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class GlbAccreditationHighestLevelByProgramFilterControl : ZUserControl
	{
		public GlbAccreditationHighestLevelByProgramFilterControl()
		{
			InitializeComponent();
		}

		protected GlbAccreditationHighestLevelByProgramFilter Filter => (GlbAccreditationHighestLevelByProgramFilter)CurrentDataItem;
	}
}
