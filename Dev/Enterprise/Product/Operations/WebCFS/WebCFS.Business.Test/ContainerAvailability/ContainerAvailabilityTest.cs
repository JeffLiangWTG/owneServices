using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Business
{
	[TestedType(typeof(ContainerAvailability))]
	public class ContainerAvailabilityTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Not required for business objects based on views", true);
		}
	}
}
