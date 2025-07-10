using CargoWise.Application;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefSysConfigType))]
	class RefSysConfigTypeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIRefSysConfigType()
		{
			AssertEquals(typeof(RefSysConfigType), ObjectFactory.GetType<Integration.Customs.Shared.IRefSysConfigType>());
		}
	}
}
