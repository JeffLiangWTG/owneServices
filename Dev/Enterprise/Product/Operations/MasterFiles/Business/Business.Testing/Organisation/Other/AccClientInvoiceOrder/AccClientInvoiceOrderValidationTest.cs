using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccClientInvoiceOrderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAI_PrintOrder()
		{
			AccClientInvoiceOrder invoiceOrder = Factory.New<AccClientInvoiceOrder>();
			invoiceOrder.Lookups.ChargeCodes.Load();
			invoiceOrder.AI_AC = invoiceOrder.Lookups.ChargeCodes[0].PK;
			invoiceOrder.AI_PrintOrder = 0;
			AssertNoErrors("Print Order between 0 and 999, should not have errors", invoiceOrder.AI_PrintOrderInfo);
			invoiceOrder.AI_PrintOrder = -1;
			AssertHasErrors("Print Order should be between 0 and 999", invoiceOrder.AI_PrintOrderInfo);
			invoiceOrder.AI_PrintOrder = 999;
			AssertNoErrors("Print Order between 0 and 999, should not have errors", invoiceOrder.AI_PrintOrderInfo);
		}
	}
}
