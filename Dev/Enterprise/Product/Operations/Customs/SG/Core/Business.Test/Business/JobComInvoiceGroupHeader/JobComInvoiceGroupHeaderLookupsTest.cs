namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceGroupHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			JobComInvoiceGroupHeader groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Lookups.Invoice, groupHeader);
		}

		public override void TestMessageTypes()
		{
			JobComInvoiceHeader header = Factory.New<JobComInvoiceHeader>();
			AssertEquals(typeof(MessageTypeCodeList), header.Lookups.MessageTypes.GetType());
		}
	}
}
