using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public abstract class BizObjectRateLineItemTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			var rateLine = rateEntry.RateLines.AddNew();

			var collection = new RateLineItemsCollection(rateLine);
			return collection.AddNew();
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name.Equals(RateLineItem.Schema.TM_TL))
			{
				using (info.BizObj.MarkAsInDeletion())
				{
					base.TestBizObjectField(info);
				}
			}
			else
			{
				base.TestBizObjectField(info);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			var rateLine = rateEntry.RateLines.AddNew();
			var rateLineItem = rateLine.RateLineItems.AddNew();

			return rateLineItem;
		}
	}
}
