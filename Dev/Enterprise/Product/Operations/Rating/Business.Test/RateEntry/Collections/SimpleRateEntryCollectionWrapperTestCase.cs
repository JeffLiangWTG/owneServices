using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(PossibleMatchesWrapper))]
	public class SimpleRateEntryCollectionWrapperTestCase : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			// Hard-coded as Cost now, but it doesn't matter Cost or Revenue in this NonPersistentBusinessObjectTestCase.
			return new PossibleMatchesWrapper(CostSell.Cost);
		}
	}

	[TestedType(typeof(SimpleRateEntryCollection))]
	public class SimpleRateEntryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SimpleRateEntryCollection(Factory);
		}
	}
}
