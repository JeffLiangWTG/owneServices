using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	internal class CAIRateEntryCollectionTest : AIRRateEntryCollectionTest
	{
		protected override string Category => RatingConstants.RateCategory.CAI;
	}

	[TestedType(typeof(CAIRateEntryCollection))]
	internal class CAIRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CAIRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
