using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class RateTypeComparerTest : RatingTestCase
	{
		public void TestCompare_RateLineFromCompanyTariffLevelOverrideHasHigherPriorityThanRateLineFromClientRate()
		{
			var autoRatingMock = new Moq.Mock<IAutoRating>();
			var mockTariffLevelProvider = autoRatingMock.As<IAutoRatingCompanyTariffLevelProvider>();
			mockTariffLevelProvider.Setup(a => a.TariffLevel).Returns(1);
			var ratingCriteria = new TestRatingCriteria(autoRatingMock.Object);
			var line1 = CreateClientRateFastLine(ratingCriteria);
			var line2 = CreateCompanyTariffFastLine(ratingCriteria);
			var comparer = CreateComparer(ratingCriteria);

			AssertEquals("RateLine from Company Tariff Level Override has higher priority than rateline from Client Rate", -1, comparer.Compare(line1, line2));
			AssertEquals("RateLine from Company Tariff Level Override has higher priority than rateline from Client Rate", 1, comparer.Compare(line2, line1));
		}

		RateTypeComparer CreateComparer(RatingCriteria criteria, string origin = "AUSYD", string destination = "CNSHA", string rateOrigin = "AUSYD", string rateDestination = "CNSHA")
		{
			if (!string.IsNullOrWhiteSpace(rateOrigin))
			{
				criteria.RateOrigin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, rateOrigin);
			}
			if (!string.IsNullOrWhiteSpace(rateDestination))
			{
				criteria.RateDestination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, rateDestination);
			}
			return new RateTypeComparer(criteria);
		}

		FastLine CreateClientRateFastLine(RatingCriteria criteria, string rateCategory = "AIR", string origin = "AUSYD", string destination = "CNSHA")
		{
			return criteria.Cache.GetOrCreateFastLine(CreateRateLineFromClientRate(rateCategory, origin, destination));
		}

		FastLine CreateCompanyTariffFastLine(RatingCriteria criteria, string rateCategory = "AIR", string origin = "AUSYD", string destination = "CNSHA")
		{
			return criteria.Cache.GetOrCreateFastLine(CreateRateLineFromCompanyTariff(rateCategory, origin, destination));
		}

		RateLine CreateRateLineFromClientRate(string rateCategory, string origin, string destination)
		{
			return clientRate.AddRateEntryWithFlatRateLine(rateCategory, "LCL", origin, destination, "FRT", 50).RateLines[0];
		}

		RateLine CreateRateLineFromCompanyTariff(string rateCategory, string origin, string destination)
		{
			return companyTariff.AddRateEntryWithFlatRateLine(rateCategory, "LCL", origin, destination, "FRT", 50).RateLines[0];
		}

		protected override void SetUp()
		{
			base.SetUp();
			clientRate = Helper.NewClientRate(NewClient);
			companyTariff = Helper.NewCompanyTariff();
		}

		ClientRate clientRate;
		CompanyTariff companyTariff;
	}
}
