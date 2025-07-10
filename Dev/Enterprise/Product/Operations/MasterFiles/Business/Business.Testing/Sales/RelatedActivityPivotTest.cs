using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RelatedActivityPivot))]
	public class RelatedActivityPivotTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This business object is saved and deleted by ViewRelatedActivityPivot triggers", true);
		}
	}
}
