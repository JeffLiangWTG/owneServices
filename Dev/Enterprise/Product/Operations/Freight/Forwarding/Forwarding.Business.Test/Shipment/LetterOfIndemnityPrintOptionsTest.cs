using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class LetterOfIndemnityPrintOptionsTest : TestCaseWithFactory
	{
		public void TestInstance()
		{
			LetterOfIndemnityOptions options = new LetterOfIndemnityOptions();

			AssertEquals(false, options.ChangeMarksAndNumbers);
			AssertEquals(null, options.NewMarksAndNumbers);

			AssertEquals(false, options.ChangeGoodsDescription);
			AssertEquals(null, options.NewGoodsDescription);

			AssertEquals(false, options.ChangeWeight);
			AssertEquals(0m, options.NewWeight);
			AssertEquals(null, options.NewWeightUnit);

			AssertEquals(false, options.ChangeVolume);
			AssertEquals(0m, options.NewVolume);
			AssertEquals(null, options.NewVolumeUnit);
		}
	}
}
