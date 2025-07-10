using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvoiceHeaderRefsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceTypeList()
		{
			var invRefs = Factory.New<JobComInvoiceHeaderRefs>();
			AssertEquals(Factory.GetCachedValue<InvoiceHeaderRefsTypeList>(), invRefs.Lookups.ReferenceTypeList);
		}
	}
}
