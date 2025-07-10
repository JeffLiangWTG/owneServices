using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OpportunityStageProgressControl : ZUserControl
	{
		public OpportunityStageProgressControl()
		{
			InitializeComponent();

			ProgressGrid.Sort = OrgOpportunityStageProgressSchema.Constants.OSP_DateStarted;
		}
	}
}
