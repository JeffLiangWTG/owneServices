using Enterprise.Customs.Business.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderDocumentSupporter))]
	sealed class JobComInvoiceHeaderDocumentSupporterTest : JobComInvoiceHeaderDocumentSupportTest
	{
		public void TestGenericCommercialInvoiceProvider()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetDocumentSupportableBusinessObject();
			var providers = invoiceHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobComInvoiceHeaderDocumentSupporter.GenericCommercialInvoice), null);
			AssertEquals("Enterprise.Customs.TW.Business.CommercialInvoiceWrapper", providers[0].GetType().FullName);

			var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader2.InvoiceLines.AddNew();
			var providers2 = invoiceHeader2.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobComInvoiceHeaderDocumentSupporter.GenericCommercialInvoice), null);
			AssertEquals("Enterprise.Customs.TW.Business.SingleCommercialInvoiceWrapper", providers2[0].GetType().FullName);
		}

		public void TestGetBODocDataProviders()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetDocumentSupportableBusinessObject();
			var providers = invoiceHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobComInvoiceHeaderDocumentSupporter.InvoicePackingWeightListDocument), null);
			AssertEquals("Provider for InvoicePackingWeightListDocument", 1, providers.Length);
		}

		public void TestInvoicePackingWeightListIsSupported()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetDocumentSupportableBusinessObject();
			AssertEquals(true, invoiceHeader.DocumentSupporter.IsDataContextSupported(new DataContextValue(JobComInvoiceHeaderDocumentSupporter.InvoicePackingWeightListDocument)));
		}

		public void TestGenericCommercialInvoiceIsSupported()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetDocumentSupportableBusinessObject();
			AssertEquals(true, invoiceHeader.DocumentSupporter.IsDataContextSupported(new DataContextValue(JobComInvoiceHeaderDocumentSupporter.GenericCommercialInvoice)));
		}

		public void TestCusPackingListIsSupported()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetDocumentSupportableBusinessObject();
			AssertEquals(true, invoiceHeader.DocumentSupporter.IsDataContextSupported(new DataContextValue(JobComInvoiceHeaderDocumentSupporter.CusPackingList)));
		}

		public void TestCusPackingListGetBODocDataProviders()
		{
			CombineAssertions(() =>
			{
				var invoiceHeader = (JobComInvoiceHeader)GetDocumentSupportableBusinessObject();
				var providers = invoiceHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobComInvoiceHeaderDocumentSupporter.CusPackingList), null);
				AssertEquals("Provider for CusPackingListDocument without CusPackingList BO", 0, providers.Length);

				invoiceHeader.CreateCusPackingList(Factory);
				providers = invoiceHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobComInvoiceHeaderDocumentSupporter.CusPackingList), null);
				AssertEquals("Provider for CusPackingListDocument with CusPackingList BO", 1, providers.Length);
			});
		}

		public override void TestGetDocBusinessObjects()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetDocumentSupportableBusinessObject();
			DocumentWrapper[] result = invoiceHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.JobComInvoiceHeader, null);
			AssertNotNull(result[0]);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			return invoiceHeader;
		}
	}
}
