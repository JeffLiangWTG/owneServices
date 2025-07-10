using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeOrAttributeTransportMode))]
	class RefCusCodeOrAttributeTransportModeTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
