using System.Collections.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business.Testing
{
	public class AutoRateInfoComparerTest : TestCaseWithFactory
	{
		public void TestComparer()
		{
			var autoRateInfo1 = new AutoRateInfo(Factory);
			var autoRateInfo2 = new AutoRateInfo(Factory);

			AssertEquals("Cannot compare infos without lines", 0, comparer.Compare(autoRateInfo1, autoRateInfo2));

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var rateEntry = clientRate.AddRateEntry("DST", "AIR", "", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.TL_RX_NKCurrency = "AUD";

			var calculator = CalculatorFactory.GetCalculator(rateLine);

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			ratingCriteria.AutoRatedFor = new Collection<IBusiness> { rateEntry };
			// Is a container count with no containers really needed for this test?
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetQuantity(MeasureType.ContainerCount, 0, string.Empty);
			ratingCriteria.RateableMeasures = measures;
			ratingCriteria.ChargeCodeGroups = new ChargeCodeGroupCollection();

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			var calculationResult1 = CalculationResult.CreateForTest(rateLine, 0m, 0m, 0m, calculatorParameters.Criteria);

			var autoRateInfo3 = new AutoRateInfo(calculationResult1, calculatorParameters, Factory);
			var autoRateInfo4 = new AutoRateInfo(calculationResult1, calculatorParameters, Factory);

			AssertEquals("Should still be equal as they come from the same source", 0, comparer.Compare(autoRateInfo3, autoRateInfo4));

			rateLine.TL_RX_NKCurrency = "USD";
			var calculationResult2 = CalculationResult.CreateForTest(rateLine, 0m, 0m, 0m, calculatorParameters.Criteria);
			var autoRateInfo5 = new AutoRateInfo(calculationResult2, calculatorParameters, Factory);

			AssertNotEquals("Should differentiate on currency", 0, comparer.Compare(autoRateInfo3, autoRateInfo5));
			AssertEquals(-1, comparer.Compare(autoRateInfo4, autoRateInfo5));
		}

		public void TestComparer_InclusiveCharges()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var rateEntry = clientRate.AddRateEntry("DST", "AIR", "", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("BAF");
			var rateLine2 = rateEntry.AddRateLine("BAF", FreightInclusiveCalculator.Code);

			var criteria = new RatingCriteria(null, Factory);
			criteria.ValuesCanBeSet = true;
			criteria.AutoRatedFor = new Collection<IBusiness> { rateEntry };
			// Is a container count with no containers really needed for this test?
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetQuantity(MeasureType.ContainerCount, 0, string.Empty);
			criteria.RateableMeasures = measures;
			criteria.ChargeCodeGroups = new ChargeCodeGroupCollection();

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			var calculationResult1 = CalculationResult.CreateForTest(rateLine, 0m, 0m, 0m, criteria);
			var calculationResult2 = CalculationResult.CreateForTest(rateLine2, 0m, 0m, 0m, criteria);

			var autoRateInfo1 = new AutoRateInfo(calculationResult1, calculatorParameters, Factory);
			var autoRateInfo2 = new AutoRateInfo(calculationResult2, calculatorParameters, Factory);

			AssertEquals("Non inclusive should be preferred to inclusive", -1, comparer.Compare(autoRateInfo1, autoRateInfo2));
		}

		#region Implementation

		readonly AutoRateInfoComparer comparer = new AutoRateInfoComparer();

		#endregion
	}
}
