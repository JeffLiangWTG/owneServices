using System.Drawing;
using CargoWise.Tools.DuplicateDetector.Standard.Common;

namespace Enterprise.MasterData.GUI
{
	public static class ColorHelper
	{
		public static Color GetConfidenceRatingColor(ConfidenceRating rating)
		{
			switch (rating)
			{
				case ConfidenceRating.Exact:
				case ConfidenceRating.High:
					return Color.Green;
				case ConfidenceRating.Medium:
					return Color.Orange;
				case ConfidenceRating.Low:
				case ConfidenceRating.None:
					return Color.Red;
				default:
					return Color.Empty;
			}
		}

		public static Color SelectedItemColor => Color.FromArgb(255, 125, 197, 232);
		public static Color HighlightedItemColor => Color.FromArgb(255, 229, 243, 251);
	}
}
