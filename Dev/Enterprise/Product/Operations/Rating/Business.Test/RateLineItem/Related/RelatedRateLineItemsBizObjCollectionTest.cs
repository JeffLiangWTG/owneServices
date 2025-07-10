using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RelatedRateLineItemsCollection))]
	public class RelatedRateLineItemsBizObjCollectionTest : BizObjectCollectionAddDeleteTestCase
	{
		protected override BusinessObjectCollection GetCollection()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			var rateLine = rateEntry.RelatedRateLines.AddNew();
			return rateLine.RateLineItems;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}
	}
}
