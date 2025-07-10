using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class PotentialMatchListItemUserControl : ZUserControl
	{
		public PotentialMatchListItemUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is PotentialMatchWinModel potentialMatchWinModel)
			{
				base.SetDataBinding(dataSource, dataMember);
				ProfileIconPicture.Image = potentialMatchWinModel.EntityTypeIcon.ToBitmap();
				WarningIcon.Image = potentialMatchWinModel.WarningIcon.ToBitmap();
				ProfileName.Text = potentialMatchWinModel.ProfileName;
				ScoreGradeText.Text = potentialMatchWinModel.ScoreGradeText;
				ScoreGradeText.ForeColor = ColorConverter.MapScoreGrade(potentialMatchWinModel.ScoreGrade);
			}
		}
	}
}
