using System.Drawing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DeniedPartyScreening.GUI
{
	internal static class ColorConverter
	{
		public static Color MapScreeningStatus(string screeningStatus)
		{
			Color color = Color.Black;

			if (screeningStatus == ScreeningStatusesList.Descriptions.Matched.GetUnresolvedString())
			{
				color = Color.FromArgb(255, 220, 88, 89);
			}
			else if (screeningStatus == ScreeningStatusesList.Descriptions.Unknown.GetUnresolvedString())
			{
				color = Color.FromArgb(255, 199, 203, 28);
			}
			else if (screeningStatus == ScreeningStatusesList.Descriptions.Clear.GetUnresolvedString())
			{
				color = Color.FromArgb(255, 97, 161, 71);
			}
			else if (screeningStatus == ScreeningStatusesList.Descriptions.PermanentClear.GetUnresolvedString())
			{
				color = Color.LightSkyBlue;
			}

			return color;
		}

		public static Color MapScoreGrade(ScoreGrades scoreGrade)
		{
			Color color = Color.Black;

			if (scoreGrade == ScoreGrades.High)
			{
				color = Color.FromArgb(255, 209, 25, 25);
			}
			else if (scoreGrade == ScoreGrades.Medium)
			{
				color = Color.FromArgb(255, 226, 105, 0);
			}

			return color;
		}

		public static Color MapExpanderDescriptionTextColor(bool isExpanderEnabled) => isExpanderEnabled ? Color.FromArgb(255, 55, 63, 80) : Color.FromArgb(255, 109, 109, 109);
	}
}
