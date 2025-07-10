using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightDomainContextServiceTest : TestCaseWithFactory
	{
		public void TestSetDomain()
		{
			AssertEquals("set domain context on null factory", false, ((BusinessObjectFactory)null).SetFreightDomainContext(FreightDomainContext.Forwarding));
			AssertEquals("set domain context when domain has not yet been set", true, Factory.SetFreightDomainContext(FreightDomainContext.Forwarding));
			AssertEquals("disallow set domain context after domain has been set", false, Factory.SetFreightDomainContext(FreightDomainContext.CFS));
			AssertEquals("domain context has not been changed", FreightDomainContext.Forwarding, Factory.GetFreightDomainContext());
		}

		public void TestGetDomain()
		{
			AssertEquals("get domain context on null factory", FreightDomainContext.Unspecified, ((BusinessObjectFactory)null).GetFreightDomainContext());
			AssertEquals("get domain context when domain not yet been set", FreightDomainContext.Unspecified, Factory.GetFreightDomainContext());

			Factory.SetFreightDomainContext(FreightDomainContext.Forwarding);
			AssertEquals("get domain context after domain has been set", FreightDomainContext.Forwarding, Factory.GetFreightDomainContext());
		}
	}
}
