using Enterprise.MarketingManager.GUI;

namespace Enterprise.Recruiter.GUI
{
	public partial class ScaledTestQuestionDetailsUserControl : SurveyQuestionDetailsUserControl
	{
		public ScaledTestQuestionDetailsUserControl()
		{
			InitializeComponent();
			this.VisibilityRelationshipProvider.SetDependency(this.MaxCalcEdit, this.MinCalcEdit);
		}
	}
}
