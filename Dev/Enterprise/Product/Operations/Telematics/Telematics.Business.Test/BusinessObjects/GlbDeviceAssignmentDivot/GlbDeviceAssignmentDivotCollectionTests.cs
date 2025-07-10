using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceAssignmentDivotCollection))]
	class GlbDeviceAssignmentDivotCollectionTests : ActiveBusinessObjectCollectionTestCase<GlbDeviceAssignmentDivotCollection>
	{
		protected override GlbDeviceAssignmentDivotCollection GetCollectionToTest()
		{
			return new GlbDeviceAssignmentDivotCollection(Factory, new ZQuery());
		}
	}
}

