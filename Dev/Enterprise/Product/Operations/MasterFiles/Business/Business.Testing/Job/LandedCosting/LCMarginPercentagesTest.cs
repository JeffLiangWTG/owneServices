using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LCMarginPercentagesTest : TestCase
	{
		public void TestHasValues()
		{
			LCMarginPercentages test = new LCMarginPercentages();
			AssertEquals("HasValues", false, test.HasValues);

			test.LCMarginPercentage1 = 0m;
			AssertEquals("HasValues", false, test.HasValues);

			test.LCMarginPercentage1 = 10m;
			AssertEquals("HasValues", true, test.HasValues);

			test.LCMarginPercentage2 = 10m;
			test.LCMarginPercentage3 = 10m;
			AssertEquals("HasValues", true, test.HasValues);
		}
	}
}
