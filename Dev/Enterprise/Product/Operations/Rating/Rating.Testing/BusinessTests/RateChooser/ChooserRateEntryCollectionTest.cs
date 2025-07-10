using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test
{
	[TestedType(typeof(ChooserRateEntryCollection))]
	public class ChooserRateEntryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ChooserRateEntryCollection>
	{
		protected override ChooserRateEntryCollection GetCollectionToTest()
		{
			return new ChooserRateEntryCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var helper = new TestHelper(Factory);
			var rate = helper.NewCosting(helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "FRT", 10m);
			return new ChooserRateEntry(Factory, rateEntry, null, calculatedResult: null);
		}
	}
}
