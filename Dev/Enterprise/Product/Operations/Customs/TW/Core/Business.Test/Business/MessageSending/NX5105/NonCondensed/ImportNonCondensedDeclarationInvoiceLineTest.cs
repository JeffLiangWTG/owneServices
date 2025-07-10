using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ImportNonCondensedDeclarationInvoiceLineTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCommodity_InvoiceLine()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader1 = Factory.New<JobComInvoiceHeader>();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_CL = entryLine.PK;
			IInvoiceLine invoiceLine = new ImportNonCondensedDeclarationInvoiceLine(entryLine, invoiceLine1);
			entryHeader.CH_DeclarationIncoterm = "FOB";
			entryLine.RefreshInvoiceLines();
			NUnit.Framework.Assert.That(invoiceLine.ChargesTypeCode, NUnit.Framework.Is.EqualTo("FOB").Using(CustomComparers.TypeComparison), "Commodity.InvoiceLine.ChargesTypeCode should be");
			entryHeader.CH_DeclarationIncoterm = "CIF";
			entryLine.RefreshInvoiceLines();
			NUnit.Framework.Assert.That(invoiceLine.ChargesTypeCode, NUnit.Framework.Is.EqualTo("CIF").Using(CustomComparers.TypeComparison), "Commodity.InvoiceLine.ChargesTypeCode should be");
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
			NUnit.Framework.Assert.That(invoiceLine.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "Commodity.InvoiceLine.CurrencyTypeCode should be");
		}
	}
}
