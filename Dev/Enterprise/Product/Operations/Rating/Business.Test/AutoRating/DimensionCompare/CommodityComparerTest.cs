using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class CommodityComparerTest : TestCaseWithFactory
	{
		public void TestGetSimilarity()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "");
			entry1.TI_RH_NKCommodityCode = ZString.Empty;
			var entry2 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "");
			entry2.TI_RH_NKCommodityCode = "GEN";
			var entry3 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "");
			entry3.TI_RH_NKCommodityCode = "HAZ";

			var criteria = new TestRatingCriteria();

			var measuresMock = new Mock<IDistinctPartRateDimensions>();
			var measures = measuresMock.Object;

			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasCommodity).Returns(true);
			var partList = partListMock.Object;

			var partEmpty = CreatePart(string.Empty);
			var partGEN = CreatePart("GEN");
			var partHAZ = CreatePart("HAZ");
			var partREF = CreatePart("REF");

			var lineProvider = new FastLineProvider(criteria);
			var line1 = lineProvider.GetOrCreate(entry1.RateLines[0]);
			var line2 = lineProvider.GetOrCreate(entry2.RateLines[0]);
			var line3 = lineProvider.GetOrCreate(entry3.RateLines[0]);

			IDimensionComparer comparer = new CommodityComparer();

			AssertEquals(Similarity.Lowest, comparer.GetSimilarity(line1, measures, partList, partEmpty, null));
			AssertEquals(Similarity.Lowest, comparer.GetSimilarity(line1, measures, partList, partGEN, null));
			AssertEquals(Similarity.Lowest, comparer.GetSimilarity(line1, measures, partList, partHAZ, null));
			AssertEquals(Similarity.Lowest, comparer.GetSimilarity(line1, measures, partList, partREF, null));

			AssertEquals(Similarity.Generic, comparer.GetSimilarity(line2, measures, partList, partEmpty, null));
			AssertEquals(Similarity.Exact, comparer.GetSimilarity(line2, measures, partList, partGEN, null));
			AssertEquals(Similarity.None, comparer.GetSimilarity(line2, measures, partList, partHAZ, null));
			AssertEquals(Similarity.None, comparer.GetSimilarity(line2, measures, partList, partREF, null));

			AssertEquals(Similarity.None, comparer.GetSimilarity(line3, measures, partList, partEmpty, null));
			AssertEquals(Similarity.None, comparer.GetSimilarity(line3, measures, partList, partGEN, null));
			AssertEquals(Similarity.Exact, comparer.GetSimilarity(line3, measures, partList, partHAZ, null));
			AssertEquals(Similarity.None, comparer.GetSimilarity(line3, measures, partList, partREF, null));
		}

		static IRateablePart CreatePart(string commodity)
		{
			var partMock = new Mock<IRateablePart>();
			partMock.Setup(x => x.CommodityCode).Returns(commodity);
			return partMock.Object;
		}

		protected TestHelper Helper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;
	}
}
