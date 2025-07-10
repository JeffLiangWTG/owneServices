namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceGroupHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceGroupHeaderLookupsTest
	{
		public override void TestMessageTypes()
		{
			var parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(typeof(Common.US.USJobMessageTypeList), parent.Lookups.MessageTypes.GetType());
		}
	}
}
