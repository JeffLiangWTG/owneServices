using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceOdometerCollection))]
	class GlbDeviceOdometerCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceOdometerCollection>
	{
		protected override GlbDeviceOdometerCollection GetCollectionToTest()
		{
			return new GlbDeviceOdometerCollection(Factory, new ZQuery());
		}
	}
}
