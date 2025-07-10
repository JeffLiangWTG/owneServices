using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCapabilityGroupPivotCollection))]
	public class GlbCapabilityGroupPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbCapabilityGroupPivotCollection>
	{
		protected override GlbCapabilityGroupPivotCollection GetCollectionToTest()
		{
			return new GlbCapabilityGroupPivotCollection(Factory);
		}
	}
}
