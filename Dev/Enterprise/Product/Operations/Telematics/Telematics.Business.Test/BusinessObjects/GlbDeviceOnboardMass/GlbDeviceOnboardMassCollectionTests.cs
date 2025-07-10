using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceOnboardMassCollection))]
	class GlbDeviceOnboardMassCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceOnboardMassCollection>
	{
		protected override GlbDeviceOnboardMassCollection GetCollectionToTest()
		{
			return new GlbDeviceOnboardMassCollection(Factory, new ZQuery());
		}
	}
}
