using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusIntegrationServiceTest : TestCaseWithFactory
	{
		public void TestSendTimeout()
		{
			CustomsDataRegistry.Instance.WebServiceTimeoutInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			var service = new CusIntegrationService();
#if NETFRAMEWORK
			AssertEquals("SendTimeout", 100d, service.HttpBinding.SendTimeout.TotalSeconds);
			AssertEquals("SendTimeout", 100d, service.HttpsBinding.SendTimeout.TotalSeconds);
			AssertEquals("SendTimeout", 100d, service.CustomBinding.SendTimeout.TotalSeconds);
#elif NETCOREAPP
			AssertEquals("SendTimeout", 100d, service.HttpBinding.Timeout.TotalSeconds);
			AssertEquals("SendTimeout", 100d, service.HttpsBinding.Timeout.TotalSeconds);
			AssertEquals("SendTimeout", 100d, service.CustomBinding.Timeout.TotalSeconds);
#endif
		}
	}
}
