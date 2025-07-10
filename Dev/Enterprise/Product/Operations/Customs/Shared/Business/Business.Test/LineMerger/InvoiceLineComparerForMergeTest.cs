using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceLineComparerForMergeTest : TestCaseWithFactory
	{
		public void TestSortWhenNothingIsMerged()
		{
			//to make invoice line2 come first
			declaration.InvoiceLines.Remove(invoiceLine1);
			declaration.InvoiceLines.Add(invoiceLine1);
			AssertEquals("PreCondition:InvoiceLine2 should come first", invoiceLine2, declaration.InvoiceLines[0]);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge();

			AssertEquals("InvoiceLine1 should be linked to an entry line numbered 1", (short)1, invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("InvoiceLine2 should be linked to an entry line numbered 2", (short)2, invoiceLine2.CusEntryLine.CL_LineNumber);
		}

		public void TestSortWhenOneIsMerged()
		{
			AssertEquals("PreCondition:InvoiceLine1 should come first at this point", invoiceLine1, declaration.InvoiceLines[0]);

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			invoiceLine2.JI_CL = entryLine.PK;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge();

			AssertEquals(entryLine, invoiceLine1.CusEntryLine);
			AssertEquals(entryLine, invoiceLine2.CusEntryLine);
		}

		public void TestSortWhenBothAreMergedOneIsLodged()
		{
			invoice1.JZ_ValuationDateOverride = ZDateTime.Today;
			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;

			invoice2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			invoiceLine2.JI_CL = entryLine2.PK;
			entry2.EntryNumber = "1";
			AssertEquals("HasBeenLodged", true, entry2.HasBeenLodgedAtCustoms);

			invoice1.JZ_ValuationDateOverride = ZDateTime.Empty;
			invoice2.JZ_ValuationDateOverride = ZDateTime.Empty;
			declaration.DoMerge();

			AssertEquals("InvoiceLine1 should be linked to an entry2 as it has been lodged and should be kept", entry2, invoiceLine1.CusEntryLine.Header);
			AssertEquals("InvoiceLine2 should be linked to an entry line2", entryLine2, invoiceLine2.CusEntryLine);
		}

		public void TestSortWhenBothAreLodgedOneIsDeactivated()
		{
			invoice1.JZ_ValuationDateOverride = ZDateTime.Today;
			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "2";
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;

			invoice2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			invoiceLine2.JI_CL = entryLine2.PK;
			entry2.EntryNumber = "1";

			AssertEquals("HasBeenLodged", true, entry1.HasBeenLodgedAtCustoms);
			AssertEquals("HasBeenLodged", true, entry2.HasBeenLodgedAtCustoms);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var mockEntry1 = factory2.LoadMoq<CusEntryHeader>(entry1.PK);
			mockEntry1.Setup(m => m.IsActive).Returns(false);
			mockEntry1.Setup(m => m.HasBeenWithdrawn).Returns(true);

			var mockEntry2 = factory2.LoadMoq<CusEntryHeader>(entry2.PK);
			mockEntry2.Setup(m => m.IsActive).Returns(true);
			mockEntry2.Setup(m => m.HasBeenWithdrawn).Returns(false);

			BaseJobDeclaration decLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);

			decLoaded.Invoices[0].JZ_ValuationDateOverride = ZDateTime.Empty;
			decLoaded.Invoices[1].JZ_ValuationDateOverride = ZDateTime.Empty;//should be merged into one entry which is not deactivated

			decLoaded.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			AssertEquals("Both should be linked to the active/lodged entry", mockEntry2.Object, decLoaded.Invoices[0].JobComInvoiceLines[0].CusEntryLine.Header);
			AssertEquals("Both should be linked to the active/lodged entry", mockEntry2.Object, decLoaded.Invoices[1].JobComInvoiceLines[0].CusEntryLine.Header);
		}

		public void TestSortWhenBothAreMergedLodgedAndOneChanges()
		{
			declaration.DoMerge();

			AssertEquals("One entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.CustomsEntryHeaders[0].EntryNumber = "1";
			AssertEquals("HasBeenLodged", true, declaration.CustomsEntryHeaders[0].HasBeenLodgedAtCustoms);

			Factory.Save();

			ZGuid existingEntryLine = invoiceLine1.JI_CL;

			invoiceLine1.JI_Tariff = "2";
			invoiceLine2.JI_Description = "Changed, but not part of merge key";
			declaration.DoMerge();

			//AU AQIS needs this requirement
			AssertNotEquals("invoice line with key changes should be assigned to a new entry line", existingEntryLine, invoiceLine1.JI_CL);
			AssertEquals("invoice line with key changes should be assigned to a new entry line", existingEntryLine, invoiceLine2.JI_CL);
		}

		public void TestSortWhenBothAreActiveLodgedAndOneHasBeenWithdrawn()
		{
			invoice1.JZ_ValuationDateOverride = ZDateTime.Today;
			var mockEntry1 = Factory.NewMoq<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(mockEntry1.Object);
			mockEntry1.Object.EntryNumber = "2";
			CusEntryLine entryLine1 = mockEntry1.Object.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;
			mockEntry1.Setup(m => m.HasBeenWithdrawn).Returns(true);

			invoice2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
			var mockEntry2 = Factory.NewMoq<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(mockEntry2.Object);
			CusEntryLine entryLine2 = mockEntry2.Object.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			invoiceLine2.JI_CL = entryLine2.PK;
			mockEntry2.Object.EntryNumber = "1";
			mockEntry2.Setup(m => m.HasBeenWithdrawn).Returns(false);

			//started with two entries and one of them is withdrawn
			//both invoices should be attached to an entry2;

			invoice1.JZ_ValuationDateOverride = ZDateTime.Empty;
			invoice2.JZ_ValuationDateOverride = ZDateTime.Empty;

			declaration.DoMerge();
			AssertEquals("should link to a non-withdrawn entry", mockEntry2.Object, invoiceLine1.CusEntryLine.Header);
			AssertEquals("should link to a non-withdrawn entry", mockEntry2.Object, invoiceLine2.CusEntryLine.Header);
		}

		BaseJobDeclaration declaration;
		BaseJobComInvoiceHeader invoice1;
		BaseJobComInvoiceLine invoiceLine1;

		BaseJobComInvoiceHeader invoice2;
		BaseJobComInvoiceLine invoiceLine2;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);

			invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
		}
	}
}
