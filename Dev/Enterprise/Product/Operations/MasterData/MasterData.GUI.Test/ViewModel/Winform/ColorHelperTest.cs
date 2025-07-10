using System.Drawing;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	public class ColorHelperTest : TestCase
	{
		public void TestColor()
		{
			CombineAssertions(() =>
			{
				AssertEquals(Color.FromArgb(255, 125, 197, 232), ColorHelper.SelectedItemColor);
				AssertEquals(Color.FromArgb(255, 229, 243, 251), ColorHelper.HighlightedItemColor);

				AssertEquals(Color.Green, ColorHelper.GetConfidenceRatingColor(ConfidenceRating.Exact));
				AssertEquals(Color.Green, ColorHelper.GetConfidenceRatingColor(ConfidenceRating.High));
				AssertEquals(Color.Orange, ColorHelper.GetConfidenceRatingColor(ConfidenceRating.Medium));
				AssertEquals(Color.Red, ColorHelper.GetConfidenceRatingColor(ConfidenceRating.Low));
				AssertEquals(Color.Red, ColorHelper.GetConfidenceRatingColor(ConfidenceRating.None));
				AssertEquals(Color.Empty, ColorHelper.GetConfidenceRatingColor(ConfidenceRating.Undefined));
			});
		}
	}
}
