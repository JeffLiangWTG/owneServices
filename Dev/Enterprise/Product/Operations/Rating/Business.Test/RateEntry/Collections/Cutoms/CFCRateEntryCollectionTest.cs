using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal class CFCRateEntryCollectionTest : FCLRateEntryCollectionTest
	{
		protected override string Category => RatingConstants.RateCategory.CFC;

		protected override string Mode => RateMode.SEA;
	}

	[TestedType(typeof(CFCRateEntryCollection))]
	internal class CFCRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CFCRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
