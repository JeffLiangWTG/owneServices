using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Rating;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(QuoteCollection))]
	public class QuoteCollectionTest : RatingHeaderCollectionBaseTest
	{
		public void TestQuoteCollectionImplementsIQuoteCollection()
		{
			var collection = new QuoteCollection(Factory);
			Assert("QuoteCollection should implement IQuoteCollection", collection is IQuoteCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new QuoteCollection(Factory);
		}
	}

	[TestedType(typeof(SimpleOneOffQuoteCollectionWrapper))]
	public class SimpleOneOffQuoteCollectionWrapperTestCase : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SimpleOneOffQuoteCollectionWrapper(new QuoteCollection(Factory));
		}
	}
}
