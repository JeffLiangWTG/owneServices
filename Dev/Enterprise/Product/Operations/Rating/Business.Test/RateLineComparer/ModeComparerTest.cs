using Enterprise.Core;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.Rating.Business.Test
{
	public class ModeComparerTest : RatingTestCase
	{
		public void TestOBCAndUNAHasHigherPriorityThanCOUMode()
		{
			AssertEquals("Mode OBC is more specific than COU", true, RatingConstants.RateMode.IsFirstModeMoreSpecific(Constants.RateMode.OBC, Constants.RateMode.COU, RatingConstants.RateCategory.ORG));
			AssertEquals("Mode UNA is more specific than COU", true, RatingConstants.RateMode.IsFirstModeMoreSpecific(Constants.RateMode.UNA, Constants.RateMode.COU, RatingConstants.RateCategory.ORG));
			AssertEquals("Mode UNA is more specific than ALL", true, RatingConstants.RateMode.IsFirstModeMoreSpecific(Constants.RateMode.UNA, Constants.RateMode.ALL, RatingConstants.RateCategory.ORG));
			AssertEquals("Mode OBC is more specific than ALL", true, RatingConstants.RateMode.IsFirstModeMoreSpecific(Constants.RateMode.OBC, Constants.RateMode.ALL, RatingConstants.RateCategory.ORG));
		}
	}
}
