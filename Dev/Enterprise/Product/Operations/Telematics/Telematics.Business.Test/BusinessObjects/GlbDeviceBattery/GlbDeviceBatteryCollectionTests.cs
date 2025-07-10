using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceBatteryCollection))]
	class GlbDeviceBatteryCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceBatteryCollection>
	{
		protected override GlbDeviceBatteryCollection GetCollectionToTest()
		{
			return new GlbDeviceBatteryCollection(Factory, new ZQuery());
		}
	}
}
