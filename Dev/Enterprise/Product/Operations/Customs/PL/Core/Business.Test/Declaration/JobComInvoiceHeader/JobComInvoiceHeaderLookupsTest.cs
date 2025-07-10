namespace Enterprise.Customs.PL.Business.Declaration.Testing;

public class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
{
	public void TestInvoice()
	{
		JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
		AssertEquals(parent.Lookups.Invoice, parent);
	}

	public void TestTranCircumstancesList() => AssertEquals("A00PL, B00PL, C00PL, D00PL, J00PL, K00PL", Factory.New<JobComInvoiceHeader>().Lookups.TranCircumstancesList.CodesAsString);
}
