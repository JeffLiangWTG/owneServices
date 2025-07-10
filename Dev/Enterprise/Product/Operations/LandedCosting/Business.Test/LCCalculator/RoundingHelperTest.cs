using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class RoundingHelperTest : TestCase
	{
		public void TestRound()
		{
			AssertEquals("Round up", 0.245m, new RoundingHelper().Round(0.2445m, 3));
			AssertEquals("Round down", 0.244m, new RoundingHelper().Round(0.2444m, 3));

			AssertEquals("Round up", "0.245", new RoundingHelper().RoundAndToString(0.2445m, 3));
			AssertEquals("Round down", "0.244", new RoundingHelper().RoundAndToString(0.2444m, 3));
		}
	}
}
