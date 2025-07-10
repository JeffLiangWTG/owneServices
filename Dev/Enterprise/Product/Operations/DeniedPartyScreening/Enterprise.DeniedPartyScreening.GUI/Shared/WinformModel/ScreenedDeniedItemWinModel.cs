using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Business;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class ScreenedDeniedItemWinModel
	{
		public ScreenedDeniedItemWinModel(string deniedParty)
		{
			Argument.NotNullOrEmpty(deniedParty, nameof(deniedParty));

			ScreenedParty = string.Empty;
			DeniedParty = deniedParty;
			DisplayScore = string.Empty;
			ScoreGrade = ScoreGrades.Low;
		}

		public ScreenedDeniedItemWinModel(string screenedParty, string deniedParty, string displayScore, ScoreGrades scoreGrade, int score = 0)
		{
			Argument.NotNullOrEmpty(screenedParty, nameof(screenedParty));
			Argument.NotNullOrEmpty(deniedParty, nameof(deniedParty));

			DisplayScore = displayScore;
			ScreenedParty = screenedParty;
			DeniedParty = deniedParty;
			ScoreGrade = scoreGrade;
			Score = score;
		}

		public string ScreenedParty { get; }

		public string DeniedParty { get; }

		public string DisplayScore { get; }

		public bool BottomLineVisibility { get; set; } = true;

		public ScoreGrades ScoreGrade { get; }

		public int Score { get; }
	}
}
