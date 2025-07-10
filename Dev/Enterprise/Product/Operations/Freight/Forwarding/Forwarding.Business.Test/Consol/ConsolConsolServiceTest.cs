using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolConsolServiceTest : TestCaseWithFactory
	{
		public void TestGetInstance()
		{
			ConsolDomainService service = ConsolDomainService.GetInstance(Factory);
			ConsolDomainService service2 = ConsolDomainService.GetInstance(Factory);
			AssertEquals("Instances should be cached", service, service2);
		}
	}
}
