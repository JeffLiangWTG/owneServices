using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceCombinationReportCollection))]
	class GlbDeviceCombinationReportCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceCombinationReportCollection>
	{
		protected override GlbDeviceCombinationReportCollection GetCollectionToTest()
		{
			return new GlbDeviceCombinationReportCollection(Factory, new ZQuery());
		}
	}
}
