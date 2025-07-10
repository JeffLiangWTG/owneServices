using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class PalletizedComparerTest : TestCaseWithFactory
	{
		public void TestGetSimilarity()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine1 = rateEntry.RateLines.AddNew();
			var rateLine2 = rateEntry.RateLines.AddNew();
			var rateLine3 = rateEntry.RateLines.AddNew();

			rateLine1.TL_WeightVolume = RatingConstants.Units.SV;
			rateLine2.TL_WeightVolume = RatingConstants.Units.CN;
			rateLine3.TL_WeightVolume = RatingConstants.Units.CN;
			rateLine3.TL_IsOnPallets = true;

			var criteria = new TestRatingCriteria();
			var lineProvider = new FastLineProvider(criteria);
			var line1 = lineProvider.GetOrCreate(rateLine1);
			var line2 = lineProvider.GetOrCreate(rateLine2);
			var line3 = lineProvider.GetOrCreate(rateLine3);

			var measuresMock = new Mock<IDistinctPartRateDimensions>();
			var measures = measuresMock.Object;

			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasPalletized).Returns(true);
			var partList = partListMock.Object;

			var partPalletized = CreatePart(true);
			var partNotPalletized = CreatePart(false);

			var comparer = new PalletizedComparer();

			AssertEquals("line unit not CN vs part palletized", Similarity.Exact, comparer.GetSimilarity(line1, measures, partList, partPalletized));
			AssertEquals("line unit not CN vs part not palletized", Similarity.Exact, comparer.GetSimilarity(line1, measures, partList, partNotPalletized));

			AssertEquals("line CN and not palletized vs part palletized", Similarity.None, comparer.GetSimilarity(line2, measures, partList, partPalletized));
			AssertEquals("line CN and not palletized vs part not palletized", Similarity.Exact, comparer.GetSimilarity(line2, measures, partList, partNotPalletized));

			AssertEquals("line CN and palletized vs part palletized", Similarity.Exact, comparer.GetSimilarity(line3, measures, partList, partPalletized));
			AssertEquals("line CN and palletized vs part not palletized", Similarity.None, comparer.GetSimilarity(line3, measures, partList, partNotPalletized));
		}

		static IRateablePart CreatePart(bool isPalletized)
		{
			var partMock = new Mock<IRateablePart>();
			partMock.Setup(x => x.IsOnPallets).Returns(isPalletized);
			return partMock.Object;
		}

		protected TestHelper Helper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;
	}
}
