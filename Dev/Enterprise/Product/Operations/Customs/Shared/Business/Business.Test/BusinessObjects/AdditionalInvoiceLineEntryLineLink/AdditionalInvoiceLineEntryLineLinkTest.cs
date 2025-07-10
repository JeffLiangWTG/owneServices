using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AdditionalInvoiceLineEntryLineLink))]
	sealed class AdditionalInvoiceLineEntryLineLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestInvoiceLineEntryLine()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			AdditionalInvoiceLineEntryLineLink link = Factory.New<AdditionalInvoiceLineEntryLineLink>();
			link.BU_JI = invoiceLine.PK;
			link.BU_CL = entryLine.PK;
			AssertEquals(entryLine, link.EntryLine);
			AssertEquals(invoiceLine, link.InvoiceLine);
		}

		public void TestErrorReportedWhenEntryLineIsLinkedToIrrelevantInvoiceLine()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.MergedLines.AddNew();

			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			var additionalLinks = invoiceLine2.AdditionalEntryLineLinks;
			try
			{
				additionalLinks.AddLinkIfNoneExists(entryLine1);
				AssertContains("An error should be thrown", "Entry line is linked to irrelevant invoice line.", ExceptionReporterTestListener.Instance[0].Message);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BaseJobDeclaration declaration = factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			AdditionalInvoiceLineEntryLineLink link = factory.New<AdditionalInvoiceLineEntryLineLink>();
			link.BU_JI = invoiceLine.PK;
			link.BU_CL = entryLine.PK;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			return link;
		}
	}
}
