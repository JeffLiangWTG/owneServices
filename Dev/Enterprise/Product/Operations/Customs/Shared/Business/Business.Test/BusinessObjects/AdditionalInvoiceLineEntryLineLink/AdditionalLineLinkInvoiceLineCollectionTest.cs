using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AdditionalLineLinkInvoiceLineCollection))]
	sealed class AdditionalLineLinkInvoiceLineCollectionTest : ActiveBusinessObjectCollectionTestCase<AdditionalLineLinkInvoiceLineCollection>
	{
		public void TestLinksBetweenAdditionalDeclarationsCanLoadUsingClusterKey()
		{
			var mockAdditionalDeclaration = Factory.NewMoq<JobDeclarationSupportAdditionalInvoices>();
			mockAdditionalDeclaration.Setup(m => m.AllowEntryLinesToBeLinkedToAnotherJob).Returns(true);
			var additionalDeclaration = mockAdditionalDeclaration.Object;
			var entry = additionalDeclaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var primaryDeclaration = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_JE = primaryDeclaration.PK;
			invoice.AttachToAdditionalDeclaration(additionalDeclaration);
			AssertCollectionContains(invoice.InvoiceLines, ((IBusiness)additionalDeclaration).Children);
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			_ = factory.Load<JobDeclarationSupportAdditionalInvoices>(additionalDeclaration.PK);
			primaryDeclaration = factory.Load<BaseJobDeclaration>(primaryDeclaration.PK);
			_ = factory.Load<JobComInvoiceHeaderSupportAdditionalDeclarations>(invoice.PK);
			AssertEquals(1, primaryDeclaration.InvoiceLines[0].AdditionalEntryLineLinks.Count);
		}

		public void TestDeleteLinkIfExistsForCH_MessageType()
		{
			CusEntryHeader entry1 = Declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = "AAA";
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();

			CusEntryHeader entry2 = Declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = "BBB";
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			AdditionalInvoiceLineEntryLineLink link = InvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			AdditionalInvoiceLineEntryLineLink link2 = InvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);

			AssertEquals("There are two links", 2, InvoiceLine.AdditionalEntryLineLinks.Count);

			InvoiceLine.AdditionalEntryLineLinks.DeleteLinkIfExistsFor("BBB");
			AssertEquals("Link is not deleted", false, link.IsDeleted);
			AssertEquals("Link2 is deleted", true, link2.IsDeleted);
		}

		public void TestAddLinkIfNoneExists()
		{
			CusEntryHeader entry1 = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();

			CusEntryHeader entry2 = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			AssertEquals(0, InvoiceLine.AdditionalEntryLineLinks.Count);
			AdditionalInvoiceLineEntryLineLink link = InvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			AssertEquals(1, InvoiceLine.AdditionalEntryLineLinks.Count);
			AssertEquals(true, entryLine1.AdditionalInvoiceLineLinks.Contains(link));

			AdditionalInvoiceLineEntryLineLink link2 = InvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			AssertEquals(1, InvoiceLine.AdditionalEntryLineLinks.Count);
			AssertEquals(true, entryLine1.AdditionalInvoiceLineLinks.Contains(link2));

			AssertEquals(link, InvoiceLine.AdditionalEntryLineLinks.GetPivotFor(entryLine1));
			AssertNull(InvoiceLine.AdditionalEntryLineLinks.GetPivotFor(entryLine2));
		}

		public void TestAdditionalInvoiceLines()
		{
			CusEntryHeader entry1 = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();

			CusEntryHeader entry2 = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			AdditionalLineLinkInvoiceLineCollection coll = new AdditionalLineLinkInvoiceLineCollection(InvoiceLine);
			AdditionalInvoiceLineEntryLineLink link = coll.AddNew();
			link.BU_CL = entryLine1.PK;

			AssertEquals(InvoiceLine.PK, link.BU_JI);

			List<CusEntryLine> entryLines = new List<CusEntryLine>(coll.AdditionalEntryLines);
			AssertEquals("entryLine1", true, entryLines.Contains(entryLine1));
			AssertEquals("entryLine2", false, entryLines.Contains(entryLine2));

			AssertEquals("ContainsPivotFor", true, coll.ContainsPivotFor(entryLine1));
			AssertEquals("ContainsPivotFor", false, coll.ContainsPivotFor(entryLine2));
		}

		public void TestDeleteLinkIfExistsFor()
		{
			CusEntryHeader entry1 = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();

			CusEntryHeader entry2 = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			AdditionalLineLinkInvoiceLineCollection coll = new AdditionalLineLinkInvoiceLineCollection(InvoiceLine);
			AdditionalInvoiceLineEntryLineLink link = coll.AddNew();
			link.BU_CL = entryLine1.PK;

			coll.DeleteLinkIfExistsFor(entryLine1);
			AssertEquals(true, link.IsDeleted);
			AssertEquals(0, coll.Count);
		}

		public void TestGetEntryLineFor()
		{
			CusEntryHeader entry1 = Declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = "AAA";
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();

			CusEntryHeader entry2 = Declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = "BBB";
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			AdditionalLineLinkInvoiceLineCollection coll = new AdditionalLineLinkInvoiceLineCollection(InvoiceLine);
			AdditionalInvoiceLineEntryLineLink link = coll.AddNew();
			link.BU_CL = entryLine1.PK;

			AdditionalInvoiceLineEntryLineLink link2 = coll.AddNew();
			link2.BU_CL = entryLine2.PK;

			AssertEquals(entryLine1, coll.GetEntryLineFor("AAA").ElementAt(0));
			AssertEquals(entryLine2, coll.GetEntryLineFor("BBB").ElementAt(0));
		}

		public void TestGetEntryLineFor_NullReferenceException()
		{
			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = "AAA";
			var entryLine1 = entry1.MergedLines.AddNew();

			var coll = new AdditionalLineLinkInvoiceLineCollection(InvoiceLine);
			coll.AddNew();

			AssertNoExceptionThrown(() => coll.GetEntryLineFor("AAA").Count());
		}

		protected override AdditionalLineLinkInvoiceLineCollection GetCollectionToTest()
		{
			return new AdditionalLineLinkInvoiceLineCollection(InvoiceLine);
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

		BaseJobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					BaseJobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
					fInvoiceLine = Declaration.InvoiceLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		BaseJobComInvoiceLine fInvoiceLine;
	}
}
