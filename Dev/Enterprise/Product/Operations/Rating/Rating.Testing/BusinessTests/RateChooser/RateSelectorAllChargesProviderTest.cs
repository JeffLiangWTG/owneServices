using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.Testing;
using Moq;

namespace Enterprise.Rating.Business.Test;

class RateSelectorAllChargesProviderTest : RatingTestCase
{
	public void TestGetAllCharges_GetsNonCarrierCharges_WhenNoCarrierOnCriteria()
	{
		// Arrange
		InsertClientChargeCodesForAutoRaterTests(Factory);

		var carrier = Factory.NewWithValidTestData<OrgHeader>();

		var creditor = Factory.NewWithValidTestData<OrgHeader>();
		var costing = Helper.NewCosting(creditor);
		var costingRateEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD");
		costingRateEntry.RateLines.RemoveAndDeleteAll();
		costingRateEntry.AddFlatCharge(TestFRT.AC_Code, 10);

		Factory.Save();

		var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 10m, 10m, creditor);
		criteria.Creditors = Creditors.New();
		criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(creditor, ["Test"]));
		criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(carrier, ["Test"]));

		var carrierChargeInfoCollection = new AutoRateInfoCollection(Factory) {
			new (Factory)
			{
				ProviderPK = carrier.PK,
				ChargeCode = TestBAF,
				IsFromRatesService = true,
				IsInclusiveCalculator = false,
				IsSubjectTo = false,
				HasExplicitZeroAmount = false,
				CalculationDescription = "No subject-to fallback happened"
			}
		};

		using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
		using (_Rating.StartCost())
		{
			// Act
			var allChargesProvider = new RateSelectorAllChargesProvider(carrierChargeInfoCollection, criteria, new DummyLogger(), true, carrier);
			var allChargesCollection = allChargesProvider.GetAllCharges();

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("selected carrier costing was matched", 1, allChargesCollection.Count(x => x.ProviderPK == carrier.PK));
				AssertEquals("other org costing was matched ", 1, allChargesCollection.Count(x => x.ProviderPK == creditor.PK));
				AssertEquals("total", 2, allChargesCollection.Count);
			});
		}
	}
}
