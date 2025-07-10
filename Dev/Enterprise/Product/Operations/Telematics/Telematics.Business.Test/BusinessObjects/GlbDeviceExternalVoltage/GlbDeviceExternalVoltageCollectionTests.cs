using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceExternalVoltageCollection))]
	class GlbDeviceExternalVoltageCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceExternalVoltageCollection>
	{
		protected override GlbDeviceExternalVoltageCollection GetCollectionToTest()
		{
			return new GlbDeviceExternalVoltageCollection(Factory, new ZQuery());
		}
	}
}
