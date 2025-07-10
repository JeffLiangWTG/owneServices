using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobComInvoiceHeaderTestForDocumentBaseOnlyWrapper : TestCaseWithFactory
	{
		public void TestEffectiveValuationDate()
		{
			ZDateTime exportDate = new ZDateTime(2004, 12, 12);
			invoice.JobDeclaration.JE_ExportDate = exportDate;
			AssertEquals("Export Date", exportDate, invoice.EffectiveValuationDate);

			ZDateTime valuationDate = new ZDateTime(2004, 12, 15);
			invoice.JZ_ValuationDateOverride = valuationDate;
			AssertEquals("Valuation Date", valuationDate, invoice.EffectiveValuationDate);

			testDec.JE_MessageType = testDec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			invoice.JZ_InvoiceDate = ZDateTime.Empty;
			invoice.JZ_ValuationDateOverride = valuationDate;
			AssertEquals("Valuation Date", valuationDate, invoice.EffectiveValuationDate);

			ZDateTime invoiceDate = new ZDateTime(2005, 01, 01);
			invoice.JZ_InvoiceDate = invoiceDate;
			AssertEquals("Valuation Date", invoiceDate, invoice.EffectiveValuationDate);
		}

		BaseJobDeclaration testDec;
		BaseJobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = BaseJobDeclaration.New(Factory);
			invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
		}
	}
}
