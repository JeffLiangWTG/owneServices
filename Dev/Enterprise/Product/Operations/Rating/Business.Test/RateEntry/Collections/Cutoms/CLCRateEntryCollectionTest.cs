using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal class CLCRateEntryCollectionTest : LCLRateEntryCollectionTest
	{
		protected override string Category => RatingConstants.RateCategory.CLC;

		protected override string Mode => RateMode.LCL;
	}

	[TestedType(typeof(CLCRateEntryCollection))]
	public class CLCRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CLCRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
