using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SPINormalInvoiceLineTest : TestCaseWithFactory
	{
		public void TestImportTariff()
		{
			var invoiceLine = GetInvoiceLine();
			invoiceLine.US_SupTariff = "9802009000";
			AssertNotNull("PreCondition", invoiceLine.ImportSupTariff);
			invoiceLine.JI_Tariff = "3902205000";
			AssertNotNull("PreCondition", invoiceLine.ImportTariff);
			var spiLine = new SPINormalInvoiceLine(invoiceLine);
			AssertEquals(invoiceLine.ImportTariff, spiLine.ImportTariff);
		}

		public void TestSecondaryTariffLines()
		{
			var declaration = CreateWatchDeclaration();
			var invoiceLine1 = declaration.InvoiceLines[0];
			Assert("PreCondition", invoiceLine1.IsParentLine);
			var spiLine = SPILine.New(invoiceLine1);
			AssertNull("No ParentLine", spiLine.ParentTariffLine);
			AssertEquals("SecondaryTariffLines", 3, spiLine.SecondaryTariffLines.Count());
			var invoiceLine2 = declaration.InvoiceLines[1];
			var spiLine2 = SPILine.New(invoiceLine2);
			AssertEquals("ParentTariffLine", typeof(SPINormalInvoiceLine), spiLine2.ParentTariffLine.GetType());
			AssertEquals("invoiceLine2 is a secondary line", false, spiLine2.SecondaryTariffLines.Any());
		}

		JobComInvoiceLine GetInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			return declaration.InvoiceLines.AddNew();
		}

		JobDeclaration CreateWatchDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "REPAIRED WATCH";
				invoiceHeader.JZ_InvoiceAmount = 9426m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
				invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
				invoiceHeader.JZ_IncoTerm = "FOB";
				var invoiceLine1 = declaration.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;
				var childLine1 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine1.JI_Tariff = "9102111020";
				childLine1.JI_InvoiceQuantity = 1000m;
				childLine1.JI_InvoiceUQ = "NO";
				childLine1.JI_CustomsQuantity = 1000m;
				childLine1.JI_CustomsUnitQty = "NO";
				childLine1.JI_LinePrice = 1609m;
				var childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.JI_Tariff = "9102111030";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1345m;
				var childLine3 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine3.JI_Tariff = "9102111040";
				childLine3.JI_InvoiceQuantity = 1000m;
				childLine3.JI_InvoiceUQ = "NO";
				childLine3.JI_CustomsQuantity = 1000m;
				childLine3.JI_CustomsUnitQty = "NO";
				childLine3.JI_LinePrice = 0m;
			}

			return declaration;
		}
	}
}
