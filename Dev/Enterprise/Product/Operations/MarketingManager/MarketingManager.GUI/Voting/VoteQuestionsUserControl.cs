namespace Enterprise.MarketingManager.GUI
{
	public partial class VoteQuestionsUserControl : QuestionsUserControl
	{
		public VoteQuestionsUserControl()
		{
			InitializeComponent();
			VisibilityRelationshipProvider.SetDependency(this.MaxVotesCalcEdit, this.MinVotesCalcEdit);
			VisibilityRelationshipProvider.SetDependency(this.ShouldRankCheckBox, this.MinVotesCalcEdit);
			this.QuestionsPerPageCalcEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteQuestionsUserControl|e7e78de1-c992-4d4e-a3c0-e8fc560f83b9", "No. of Items per page");
		}

		public bool ShowVoteSettingsControls
		{
			get { return MinVotesCalcEdit.Visible; }
			set { MinVotesCalcEdit.Visible = value; }
		}
	}
}
