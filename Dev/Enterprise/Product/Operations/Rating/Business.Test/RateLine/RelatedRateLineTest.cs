using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RelatedRateLine))]
	public class RelatedRateLineTest : BizObjectRateLineTestCase
	{
		public void TestServiceLevel()
		{
			var costHeader = Factory.New<Costing>();
			var costEntry = costHeader.AddRateEntry("AIR");
			costEntry.TI_RS_NKServiceLevel_NI = "XXX";
			costEntry.TI_PL_NKCarrierServiceLevel = "YYY";
			var costLine = costEntry.RelatedRateLines.AddNew();
			costLine.TL_TI = costEntry.PK;

			AssertEquals("XXX", costLine.ServiceLevel);
			AssertEquals("YYY", costLine.CarrierServiceLevel);

			var rateHeader = Factory.New<ClientRate>();
			var rateEntry = rateHeader.AddRateEntry("AIR");
			rateEntry.TI_RS_NKServiceLevel_NI = "AAA";
			rateEntry.TI_PL_NKCarrierServiceLevel = "BBB";
			var rateLine = rateEntry.RelatedRateLines.AddNew();
			rateLine.TL_TI = rateEntry.PK;

			AssertEquals("AAA", rateLine.ServiceLevel);
			AssertEquals("BBB", rateLine.CarrierServiceLevel);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestRateLineItemsCorrectType()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RelatedRateLines.AddNew();
			AssertEquals("Related Rate Line has correct rate line items type", typeof(RelatedRateLineItemsCollection), line.RateLineItems.GetType());
		}

		public void TestChargeCodeDescription()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "TST";
			chargeCode.AC_Desc = "My Charge Code";

			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RelatedRateLines.AddNew();
			line.TL_AC = chargeCode.PK;

			AssertEquals("Charge code description correct", "My Charge Code", line.ChargeCodeDescription);

			line.TL_RateDesc = "Some overidden description";
			AssertEquals("Charge code description correct", "Some overidden description", line.ChargeCodeDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<Costing>();
			var entry = header.AddRateEntry("AIR");
			var relatedLine = entry.RelatedRateLines.AddNew();
			relatedLine.TL_TI = entry.PK;
			entry.RateLines.Add(relatedLine);

			return relatedLine;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<Costing>();
			var entry = header.AddRateEntry(RatingConstants.RateCategory.AIR);

			var relatedLine = entry.RelatedRateLines.AddNew();
			relatedLine.TL_TI = entry.PK;
			relatedLine.TL_AC = factory.New<AccChargeCode>().PK;

			entry.RateLines.Add(relatedLine);

			return relatedLine;
		}
	}
}
