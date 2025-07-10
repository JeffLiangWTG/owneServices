using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceTyreAlertCollection))]
	class GlbDeviceTyreAlertCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceTyreAlertCollection>
	{
		protected override GlbDeviceTyreAlertCollection GetCollectionToTest()
		{
			return new GlbDeviceTyreAlertCollection(Factory, new ZQuery());
		}
	}
}
