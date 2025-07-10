using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	internal class CORRateEntryCollectionTest : ORGRateEntryCollectionTest
	{
		protected override string Category => RatingConstants.RateCategory.COR;
	}

	[TestedType(typeof(CORRateEntryCollection))]
	internal class CORRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CORRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
