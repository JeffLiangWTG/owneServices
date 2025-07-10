using Enterprise.Rating.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class ReferenceIntegrationTests : BaseRatingIntegrationTest
	{
		public void TestReferenceRevenueRating()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "KRSEL");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 1200m;
			rateLine1.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 1.5m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 1100m;

			var shipment = CreateForwardingShipment(TransportModes.Air, NewClient.PK, Consignee.PK, "AUSYD", "KRSEL", 500m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 1200m,
					ChargeCode = "FRT",
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 1100m,
					ChargeCode = "BAF",
				}
			};

			AutorateAndAssert(expected, shipment, NewClient);
		}
	}
}
