using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceLogCollection))]
	class GlbDeviceLogCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceLogCollection>
	{
		protected override GlbDeviceLogCollection GetCollectionToTest()
		{
			return new GlbDeviceLogCollection(Factory, new ZQuery());
		}
	}
}
