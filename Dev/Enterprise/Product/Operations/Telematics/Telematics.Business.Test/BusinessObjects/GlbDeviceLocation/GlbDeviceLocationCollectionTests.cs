using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceLocationCollection))]
	class GlbDeviceLocationCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceLocationCollection>
	{
		protected override GlbDeviceLocationCollection GetCollectionToTest()
		{
			return new GlbDeviceLocationCollection(Factory, new ZQuery());
		}
	}
}

