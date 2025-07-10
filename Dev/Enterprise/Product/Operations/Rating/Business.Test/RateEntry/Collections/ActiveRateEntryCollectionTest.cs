using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(ActiveRateEntryCollection))]
	public class ActiveRateEntryCollectionTest : ActiveBusinessObjectCollectionTestCase<ActiveRateEntryCollection>
	{
		protected override ActiveRateEntryCollection GetCollectionToTest()
		{
			var costing = Factory.New<Costing>();
			return new ActiveRateEntryCollection(costing);
		}
	}

	[TestedType(typeof(ActiveQuoteEntryCollection))]
	public class ActiveQuoteEntryCollectionTest : ActiveBusinessObjectCollectionTestCase<ActiveQuoteEntryCollection>
	{
		protected override ActiveQuoteEntryCollection GetCollectionToTest()
		{
			var quote = Factory.New<Quote>();
			return new ActiveQuoteEntryCollection(quote);
		}
	}
}
