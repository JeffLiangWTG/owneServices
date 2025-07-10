using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class AddInfoJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestValuationMethodList()
	{
		var valuationMethodList = lookups.ValuationMethodList;
		CombineAssertions(() =>
		{
			AssertEquals("Codes", "E, F, G, H, I, J, K", valuationMethodList.CodesAsString);
			AssertSame("Cached", Factory.GetCachedValue<ValuationMethodList>(), valuationMethodList);
		});
	}

	public void TestTransportMethodOfPaymentList()
	{
		var transportMethodOfPaymentList = lookups.TransportMethodOfPaymentList;
		CombineAssertions(() =>
		{
			AssertEquals("Codes", "A, B, C, D, H, Y, Z", transportMethodOfPaymentList.CodesAsString);
			AssertSame("Cached", Factory.GetCachedValue<ExportTransportMethodOfPaymentList>(), transportMethodOfPaymentList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var addInfoJobComInvoiceHeader = new AddInfoJobComInvoiceHeader(Factory.New<JobComInvoiceHeader>().JZ_AddInfoInfo);
		lookups = new AddInfoJobComInvoiceHeaderLookups(addInfoJobComInvoiceHeader);
	}
	AddInfoJobComInvoiceHeaderLookups lookups;
}
