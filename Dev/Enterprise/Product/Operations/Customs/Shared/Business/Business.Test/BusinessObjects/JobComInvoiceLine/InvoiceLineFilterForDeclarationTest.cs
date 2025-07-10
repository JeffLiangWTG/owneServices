using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceLineFilterForDeclarationTest : TestCaseWithFactory
	{
		public void TestGetFilter_CombineClusterKeys()
		{
			var declaration = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			var invoice = declaration.Invoices.AddNew();
			var invoice1 = declaration.Invoices.AddNew();

			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice2 = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			declaration2.Invoices.Add(invoice2);
			invoice2.AttachToAdditionalDeclaration(declaration);
			invoice2.GroupHeader.AttachToAdditionalDeclaration(declaration);

			Factory.Save();

			AssertEquals($"(JI_ClusterKey = 1 and (JI_JZ in (CONVERT('', 'System.Guid'), CONVERT('', 'System.Guid')))) or JI_JZ = CONVERT('{invoice2.PK}', 'System.Guid')",
				InvoiceLineFilterForDeclaration.GetFilter(declaration).LiteralTextADO.Replace(invoice.PK.ToString(), "").Replace(invoice1.PK.ToString(), ""));
		}

		public void TestLoadRightLines()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			testDec.Invoices.AddNew();
			testDec.FilteredInvoiceLines.AddNew();
			testDec.FilteredInvoiceLines.AddNew();
			testDec.FilteredInvoiceLines.AddNew();

			BaseJobDeclaration testDec2 = Factory.New<BaseJobDeclaration>();
			testDec2.Invoices.AddNew();
			testDec2.FilteredInvoiceLines.AddNew();

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			BaseJobComInvoiceLine[] invoiceLines = factory2.Load<BaseJobComInvoiceLine>(InvoiceLineFilterForDeclaration.GetFilter(testDec));
			AssertEquals(3, invoiceLines.Length);

			invoiceLines = factory2.Load<BaseJobComInvoiceLine>(InvoiceLineFilterForDeclaration.GetFilter(testDec2));
			AssertEquals(1, invoiceLines.Length);
		}

		public void TestLoadRightLinesWithAdditionalInvoices()
		{
			BaseJobDeclaration declaration = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			declaration.FilteredInvoiceLines.AddNew();

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice2 = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			declaration2.Invoices.Add(invoice2);
			declaration2.FilteredInvoiceLines.AddNew();
			invoice2.AttachToAdditionalDeclaration(declaration);
			invoice2.GroupHeader.AttachToAdditionalDeclaration(declaration);

			BaseJobComInvoiceLine[] invoiceLines = Factory.Load<BaseJobComInvoiceLine>(InvoiceLineFilterForDeclaration.GetFilter(declaration));
			AssertEquals(2, invoiceLines.Length);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			invoiceLines = factory2.Load<BaseJobComInvoiceLine>(InvoiceLineFilterForDeclaration.GetFilter(declaration));
			AssertEquals(2, invoiceLines.Length);
		}
	}
}
