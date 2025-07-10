using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCityPCodePivot))]
	sealed class RefCityPCodePivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDeafultValues()
		{
			Assert(!Factory.New<RefCityPCodePivot>().R0_IsSystem);
		}
	}
}
