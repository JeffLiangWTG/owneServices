using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateOneOffContainersCollection))]
	public class RateOneOffContainersCollectionTest : RateOneOffContainersCollectionTest<RateOneOffContainers>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testQuote = Factory.New<Quote>();
			testQuote.TH_OneTimeQuote = true;
			return new RateOneOffContainersCollection(testQuote.CurrentOneOffQuote, false);
		}
	}

	public abstract class RateOneOffContainersCollectionTest<T> : BusinessObjectCollectionTestCase
		where T : RateOneOffContainers
	{
	}
}
