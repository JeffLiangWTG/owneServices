using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(OneOffQuoteStatisticsWrapper))]
	public class OneOffQuoteStatisticsWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var oneOffShipment = Factory.New<RateOneOffShipment>();
			return new OneOffQuoteStatisticsWrapper(oneOffShipment);
		}
	}
}
