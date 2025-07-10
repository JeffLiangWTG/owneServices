using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EntryManagerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAfterCreateOrGetEntryLineCalled()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			declaration.InvoiceLines[0].JI_CL = entryLine.PK;

			var manager = new Mock<MergeManager>(declaration) { CallBase = true };
			var merger = new Mock<LineMerger>(declaration) { CallBase = true };
			var strategy = new Mock<EntryCreationStrategy>(declaration) { CallBase = true };

			manager.Protected().Setup<LineMerger>("GetNewLineMergerCore").Returns(merger.Object);
			merger.Protected().Setup<EntryCreationStrategy[]>("GetEntryCreationStrategies").Returns(new[] { strategy.Object });

			manager.Object.Execute();
			strategy.Verify();
		}

		public void TestWhenAnEntryIsWithdrawnAndRelodged()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsCustomsHeaderAmendmentATotalReplacement).Returns(false);
			mockDeclaration.Setup(m => m.IsCustomsLineAmendmentATotalReplacement).Returns(false);

			var declaration = mockDeclaration.Object;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			invoice1.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "3";
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2";

			declaration.DoMerge();

			AssertEquals("two entries", 2, declaration.CustomsEntryHeaders.Count);

			invoiceLine1.CusEntryLine.Header.EntryNumber = "1";
			invoiceLine3.CusEntryLine.Header.EntryNumber = "1";
			invoiceLine1.CusEntryLine.Header.CH_HighestLineNumber = 1;
			invoiceLine3.CusEntryLine.Header.CH_HighestLineNumber = 1;
			AssertEquals("hasbeenlodged", true, invoiceLine1.CusEntryLine.Header.HasBeenLodgedAtCustoms);
			AssertEquals("hasbeenlodged", true, invoiceLine3.CusEntryLine.Header.HasBeenLodgedAtCustoms);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var entryWithdrawn = factory2.LoadMoq<CusEntryHeader>(invoiceLine1.CusEntryLine.Header.PK);
			entryWithdrawn.Setup(m => m.HasBeenWithdrawn).Returns(true);
			entryWithdrawn.Setup(m => m.IsActive).Returns(false);

			var invoice1Loaded = factory2.Load<BaseJobComInvoiceHeader>(invoice1.PK);
			var invoice2Loaded = factory2.Load<BaseJobComInvoiceHeader>(invoice2.PK);
			invoice1Loaded.JZ_ValuationDateOverride = ZDateTime.Empty;
			invoice2Loaded.JZ_ValuationDateOverride = ZDateTime.Empty;

			ZGuid entryToBeKept = invoiceLine3.CusEntryLine.Header.PK;
			invoice2Loaded.JobDeclaration.MessageInitiator = declaration.MessageInitiator;
			invoice2Loaded.JobDeclaration.DoMerge();

			AssertEquals("two entry", 2, invoice2Loaded.JobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("one active entry", 1, invoice2Loaded.JobDeclaration.ActiveEntryHeaders.Count);
			AssertEquals(true, invoice2Loaded.JobDeclaration.ActiveEntryHeaders.Contains(entryToBeKept));
		}

		public void TestRemergeAfterTariffChanges()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsCustomsHeaderAmendmentATotalReplacement).Returns(false);
			mockDeclaration.Setup(m => m.IsCustomsLineAmendmentATotalReplacement).Returns(false);

			var declaration = mockDeclaration.Object;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";

			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";

			declaration.DoMerge();

			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			invoiceLine2.JI_Tariff = "2";
			declaration.DoMerge();
			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("two lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals(invoiceLine1.JI_CL, invoiceLine3.JI_CL);
			AssertNotEquals(invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestRemergeWhenHeaderKeyChanges()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsCustomsHeaderAmendmentATotalReplacement").Returns(false);
			mockDeclaration.Protected().Setup<bool>("IsCustomsLineAmendmentATotalReplacement").Returns(false);

			var declaration = mockDeclaration.Object;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-10);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice2.JZ_ValuationDateOverride = ZDateTime.Today;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "3";
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2";

			declaration.DoMerge();

			AssertEquals("two entries", 2, declaration.CustomsEntryHeaders.Count);

			invoiceLine2.CusEntryLine.Header.EntryNumber = "1";
			invoiceLine2.CusEntryLine.Header.CH_HighestLineNumber = 1;
			AssertEquals("hasbeenlodged", true, invoiceLine2.CusEntryLine.Header.HasBeenLodgedAtCustoms);

			Factory.Save();

			var entryToBeDiscarded = invoiceLine1.CusEntryLine.Header;
			var entryToBeKept = invoiceLine2.CusEntryLine.Header;
			var entryLineToBeKept = entryToBeKept.MergedLines[0];

			invoice2.JZ_ValuationDateOverride = declaration.JE_ExportDate;
			declaration.DoMerge();

			AssertEquals("One entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals("a lodged entry should be kept", true, declaration.CustomsEntryHeaders.Contains(entryToBeKept));
			AssertEquals("an extra entry is discarded", true, entryToBeDiscarded.IsDeleted);

			AssertEquals("invoice line1 is linked to entryLineToBeKept", entryLineToBeKept, invoiceLine1.CusEntryLine);
			AssertEquals("entryLineToBeKept's entry number", (short)1, entryLineToBeKept.CL_LineNumber);
		}

		public void TestUsersReassignedInvoiceLineNosAndUnmergedInvoiceLinesComeFirst()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsCustomsHeaderAmendmentATotalReplacement").Returns(false);
			mockDeclaration.Protected().Setup<bool>("IsCustomsLineAmendmentATotalReplacement").Returns(false);

			var declaration = mockDeclaration.Object;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3";

			declaration.DoMerge();

			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("three lines", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			var invoiceLine4 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "4";
			declaration.DoMerge();

			invoiceLine4.JI_LineNo = 1;

			AssertEquals((short)1, invoiceLine4.JI_LineNo);

			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("four lines", 4, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals("Fourth entry line is linked to an invoice line which was unmerged, but came first", invoiceLine4, declaration.CustomsEntryHeaders[0].MergedLines[3].InvoiceLines[0]);
		}

		public void TestDiscardUnusedEntriesWhenMergeHappensTwice()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsCustomsHeaderAmendmentATotalReplacement").Returns(false);
			mockDeclaration.Protected().Setup<bool>("IsCustomsLineAmendmentATotalReplacement").Returns(false);

			var declaration = mockDeclaration.Object;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice1 = declaration.Invoices.AddNew();
			var yesterday = ZDateTime.Today.AddDays(-1);
			invoice1.JZ_ValuationDateOverride = ZDateTime.Today;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationDateOverride = yesterday;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";

			declaration.DoMerge();

			AssertEquals("two entries", 2, declaration.CustomsEntryHeaders.Count);

			declaration.CustomsEntryHeaders[0].CH_HighestLineNumber = 1;
			declaration.CustomsEntryHeaders[1].CH_HighestLineNumber = 1;
			declaration.Factory.Save();

			invoice2.Delete();
			declaration.DoMerge();

			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_ValuationDateOverride = yesterday;
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";

			declaration.DoMerge();
			AssertEquals("two entries", 2, declaration.CustomsEntryHeaders.Count);
		}
	}
}
