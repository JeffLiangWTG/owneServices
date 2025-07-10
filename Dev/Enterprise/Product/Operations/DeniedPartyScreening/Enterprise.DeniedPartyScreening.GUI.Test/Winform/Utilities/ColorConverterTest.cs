using System.Drawing;
using Enterprise.DeniedPartyScreening.Business;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class ColorConverterTest : TestCase
	{
		public void TestConverter()
		{
			CombineAssertions("Corresponding color of High, Middle, Low ScoreGrade", () =>
			{
				AssertEquals(Color.FromArgb(255, 209, 25, 25), ColorConverter.MapScoreGrade(ScoreGrades.High));
				AssertEquals(Color.FromArgb(255, 226, 105, 0), ColorConverter.MapScoreGrade(ScoreGrades.Medium));
				AssertEquals(Color.Black, ColorConverter.MapScoreGrade(ScoreGrades.Low));
			});
		}
	}
}
