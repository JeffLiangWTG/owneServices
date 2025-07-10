using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceGroupHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			JobComInvoiceGroupHeader groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Lookups.Invoice, groupHeader);
		}

		public override void TestMessageTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = Factory.New<JobComInvoiceGroupHeader>();
			var list = invoice.Lookups.MessageTypes;
			AssertType<TWJobMessageTypeList>(list);
			AssertEquals(declaration.Lookups.MessageTypeList.CodesAsString, list.CodesAsString);
		}

		public override void TestMessageTypesContainsASN()
		{
			Assert("Only support EXP and IMP", true);
		}
	}
}
