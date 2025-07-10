using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateTransportZonesCollection))]
	public class RateTransportZonesCollectionTest : ActiveBusinessObjectCollectionTestCase<RateTransportZonesCollection>
	{
		protected override RateTransportZonesCollection GetCollectionToTest()
		{
			return new RateTransportZonesCollection(Factory.New<RateTransportProvider>());
		}
	}
}
