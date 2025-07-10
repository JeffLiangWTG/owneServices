using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateTariffDiscountCollection))]
	public class RateTariffDiscountBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RateTariffDiscountCollection(Factory.New<CompanyTariff>());
		}

		public void TestLoadAndCreateNewTypes()
		{
			var tariff = Factory.New<CompanyTariff>();
			Factory.Save();

			var collection = new RateTariffDiscountCollection(tariff, RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.DST);
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertNotNull(collection.Find(RatingConstants.RateCategory.ORG));
			AssertNotNull(collection.Find(RatingConstants.RateCategory.DST));
			collection.SetDiscount(RatingConstants.RateCategory.ORG, 10m);

			AssertEquals(10m, collection.GetDiscount(RatingConstants.RateCategory.ORG));
			AssertEquals(10m, collection.GetDiscount(RatingConstants.RateCategory.ORG, "TTT"));

			collection.SetServiceLevel(RatingConstants.RateCategory.ORG, "TTT");
			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var filter = new ZQuery(RateTariffDiscountSchema.TD_TH, tariff.PK);
			filter.AddToFilter(RateTariffDiscountSchema.TD_TariffType, RatingConstants.RateCategory.DST);
			var dstDiscount = factory2.LoadTop1<RateTariffDiscount>(filter);
			AssertNotNull(dstDiscount);
			Assert(!dstDiscount.IsDeleted);

			var tariffInOtherFactory = factory2.Load<CompanyTariff>(tariff.PK);
			collection = new RateTariffDiscountCollection(tariffInOtherFactory, RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR);
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertNotNull(collection.Find(RatingConstants.RateCategory.ORG));
			AssertNotNull(collection.Find(RatingConstants.RateCategory.AIR));
			AssertEquals(10m, collection.GetDiscount(RatingConstants.RateCategory.ORG));
			AssertEquals(10m, collection.GetDiscount(RatingConstants.RateCategory.ORG, "TTT"));
			AssertEquals(0m, collection.GetDiscount(RatingConstants.RateCategory.ORG, "TTA"));
			AssertEquals("TTT", collection.GetServiceLevel(RatingConstants.RateCategory.ORG));
			Assert(dstDiscount.IsDeleted);
		}
	}
}
