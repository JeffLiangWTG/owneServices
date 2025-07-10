using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceIgnitionCollection))]
	class GlbDeviceIgnitionCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceIgnitionCollection>
	{
		protected override GlbDeviceIgnitionCollection GetCollectionToTest()
		{
			return new GlbDeviceIgnitionCollection(Factory, new ZQuery());
		}
	}
}
