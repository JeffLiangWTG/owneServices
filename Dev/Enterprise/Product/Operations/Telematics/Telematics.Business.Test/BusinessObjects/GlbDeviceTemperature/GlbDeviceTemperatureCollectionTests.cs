using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceTemperatureCollection))]
	class GlbDeviceTemperatureCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceTemperatureCollection>
	{
		protected override GlbDeviceTemperatureCollection GetCollectionToTest()
		{
			return new GlbDeviceTemperatureCollection(Factory, new ZQuery());
		}
	}
}
