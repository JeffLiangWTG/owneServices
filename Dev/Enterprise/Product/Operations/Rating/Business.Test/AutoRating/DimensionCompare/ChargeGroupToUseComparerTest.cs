using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class ChargeGroupToUseComparerTest : TestCaseWithFactory
	{
		public void TestGetSimilarity()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "");
			var rateLine = entry.RateLines[0];
			rateLine.TL_AC = Helper.ChargeCodes["8179"].PK;
			rateLine.ChargeCode.AC_ChargeGroup = "WOU";

			var criteria = new TestRatingCriteria();
			var lineProvider = new FastLineProvider(criteria);
			var line = lineProvider.GetOrCreate(rateLine);

			var measuresMock = new Mock<IDistinctPartRateDimensions>();
			var measures = measuresMock.Object;

			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasChargeGroupToUse).Returns(true);
			var partList = partListMock.Object;

			var comparer = new ChargeGroupToUseComparer();

			AssertEquals(Similarity.Exact, comparer.GetSimilarity(line, measures, partList, CreatePart(""), null));
			AssertEquals(Similarity.Generic, comparer.GetSimilarity(line, measures, partList, CreatePart("WOU"), null));
			AssertEquals(Similarity.None, comparer.GetSimilarity(line, measures, partList, CreatePart("XXX"), null));
		}

		static IRateablePart CreatePart(string chargeGroup)
		{
			var partMock = new Mock<IRateablePart>();
			partMock.Setup(x => x.ChargeGroupToUse).Returns(chargeGroup);
			return partMock.Object;
		}

		protected TestHelper Helper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;
	}
}
