using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test
{
	[TestedType(typeof(RateOneOffCarrierCollection))]
	public class RateOneOffCarrierCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testQuote = Factory.New<Quote>();
			testQuote.TH_OneTimeQuote = true;
			return new RateOneOffCarrierCollection(testQuote.CurrentOneOffQuote);
		}
	}
}
