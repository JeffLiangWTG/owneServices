using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AdditionalLineLinkEntryLineCollection))]
	sealed class AdditionalLineLinkEntryLineCollectionTest : ActiveBusinessObjectCollectionTestCase<AdditionalLineLinkEntryLineCollection>
	{
		public void TestAdditionalInvoiceLines()
		{
			BaseJobComInvoiceLine invoiceLine1 = Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoiceLine1.InvoiceHeader.JobComInvoiceLines.AddNew();

			AdditionalLineLinkEntryLineCollection coll = new AdditionalLineLinkEntryLineCollection(EntryLine);
			AdditionalInvoiceLineEntryLineLink link = coll.AddNew();
			link.BU_JI = invoiceLine1.PK;

			AssertEquals(link.BU_CL, EntryLine.PK);

			List<BaseJobComInvoiceLine> invoiceLines = new List<BaseJobComInvoiceLine>(coll.AdditionalInvoiceLines);
			AssertEquals("invoiceLine1", true, invoiceLines.Contains(invoiceLine1));
			AssertEquals("invoiceLine2", false, invoiceLines.Contains(invoiceLine2));
		}

		protected override AdditionalLineLinkEntryLineCollection GetCollectionToTest()
		{
			return new AdditionalLineLinkEntryLineCollection(EntryLine);
		}

		BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<BaseJobDeclaration>();
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
					fEntryLine = entry.MergedLines.AddNew();
				}
				return fEntryLine;
			}
		}
		CusEntryLine fEntryLine;
	}
}
