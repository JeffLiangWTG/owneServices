using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class AddInfoJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentCodeList()
		{
			var list = lookups.InvoicePaymentCodeList;
			CombineAssertions("InvoicePaymentCodeList", () =>
			{
				AssertEquals("CodeAsString", "1, 2, 3, 4, 6, 7, 11, 12, 13, 14, 15, 16, 17", list.CodesAsString);
				AssertSame(Factory.GetCachedValue<InvoicePaymentCodeList>(), list);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var addInfo = new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
			lookups = new AddInfoJobComInvoiceHeaderLookups(addInfo);
		}
		AddInfoJobComInvoiceHeaderLookups lookups;
	}
}
