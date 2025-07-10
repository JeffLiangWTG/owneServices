using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	internal class CDSRateEntryCollectionTest : DSTRateEntryCollectionTest
	{
		protected override string Category => RatingConstants.RateCategory.CDS;
	}

	[TestedType(typeof(CDSRateEntryCollection))]
	internal class CDSRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CDSRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
