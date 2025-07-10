using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(DummyRateLineItem))]
	public class DummyRateLineItemBizObjTest : BizObjectRateLineItemTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<RatingHeader>();
			var rateEntry = header.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			var lineItem = Factory.New<DummyRateLineItem>();

			lineItem.TM_TL = rateLine.PK;
			rateLine.RateLineItems.Add(lineItem);

			return lineItem;
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<RatingHeader>();
			header.TH_RateType = RatingConstants.RatingHeaderTypes.Costing;

			var rateEntry = header.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = factory.New<AccChargeCode>().PK;

			var lineItem = factory.New<DummyRateLineItem>();
			lineItem.TM_TL = rateLine.PK;

			rateLine.RateLineItems.Add(lineItem);

			return lineItem;
		}
	}
}
