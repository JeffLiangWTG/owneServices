using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceTyreReportCollection))]
	class GlbDeviceTyreReportCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceTyreReportCollection>
	{
		protected override GlbDeviceTyreReportCollection GetCollectionToTest()
		{
			return new GlbDeviceTyreReportCollection(Factory, new ZQuery());
		}
	}
}
