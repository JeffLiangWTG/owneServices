using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class ContainerOwnershipComparerTest : TestCaseWithFactory
	{
		public void TestGetSimilarity()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");

			var lineNA = rateEntry.RateLines.AddNew();
			lineNA.TL_WeightVolume = RatingConstants.Units.SV;

			var lineBlank = rateEntry.RateLines.AddNew();
			lineBlank.TL_WeightVolume = RatingConstants.Units.CN;

			var lineCAR = rateEntry.RateLines.AddNew();
			lineCAR.TL_WeightVolume = RatingConstants.Units.CN;
			lineCAR.TL_ContainerOwnership = Core.Constants.ContainerOwnership.Codes.CarrierOwned;

			var lineSHP = rateEntry.RateLines.AddNew();
			lineSHP.TL_WeightVolume = RatingConstants.Units.CN;
			lineSHP.TL_ContainerOwnership = Core.Constants.ContainerOwnership.Codes.ShipperOwned;

			var comparer = new ContainerOwnershipComparer();

			var criteria = new TestRatingCriteria();

			var measuresMock = new Mock<IDistinctPartRateDimensions>();
			var measures = measuresMock.Object;

			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasContainerOwnership).Returns(true);
			var partList = partListMock.Object;
			var carrierOwnedMock = new Mock<IRateablePart>();
			var shipperOwnedMock = new Mock<IRateablePart>();
			carrierOwnedMock.Setup(x => x.ContainerOwnership).Returns(Core.Constants.ContainerOwnership.Codes.CarrierOwned);
			shipperOwnedMock.Setup(x => x.ContainerOwnership).Returns(Core.Constants.ContainerOwnership.Codes.ShipperOwned);
			var carrierOwned = carrierOwnedMock.Object;
			var shipperOwned = shipperOwnedMock.Object;
			var lineProvider = new FastLineProvider(criteria);
			var fastLineNA = lineProvider.GetOrCreate(lineNA);
			var fastLineBlank = lineProvider.GetOrCreate(lineBlank);
			var fastLineCAR = lineProvider.GetOrCreate(lineCAR);
			var fastLineSHP = lineProvider.GetOrCreate(lineSHP);

			CombineAssertions(() =>
			{
				AssertEquals("Equally as similar as neither option is really applicable", Similarity.Lowest, comparer.GetSimilarity(fastLineNA, measures, partList, carrierOwned, null));
				AssertEquals("Equally as similar as neither option is really applicable", Similarity.Lowest, comparer.GetSimilarity(fastLineNA, measures, partList, shipperOwned, null));

				AssertEquals("Equally as similar, it's up to the criteria matching to filter", Similarity.Lowest, comparer.GetSimilarity(fastLineBlank, measures, partList, carrierOwned, null));
				AssertEquals("Equally as similar, it's up to the criteria matching to filter", Similarity.Lowest, comparer.GetSimilarity(fastLineBlank, measures, partList, shipperOwned, null));

				AssertEquals(Similarity.Exact, comparer.GetSimilarity(fastLineCAR, measures, partList, carrierOwned, null));
				AssertEquals(Similarity.None, comparer.GetSimilarity(fastLineCAR, measures, partList, shipperOwned, null));

				AssertEquals(Similarity.None, comparer.GetSimilarity(fastLineSHP, measures, partList, carrierOwned, null));
				AssertEquals(Similarity.Exact, comparer.GetSimilarity(fastLineSHP, measures, partList, shipperOwned, null));
			});
		}
	}
}
