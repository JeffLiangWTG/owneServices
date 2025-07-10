using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class InvoiceLoaderTest : TestCaseWithFactory
	{
		public void TestDontLoadInvoicesWhereUniqueRefIsBlank()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			AccTransactionHeader transHeader1 = Factory.New<AccTransactionHeader>();
			transHeader1.AH_TransactionNum = "00001001";
			transHeader1.AH_OH = org.PK;
			transHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader1.AH_TransactionType = TransactionTypes.Invoice;
			transHeader1.AH_ConsolidatedInvoiceRef = "11111";

			AccTransactionHeader transHeader2 = Factory.New<AccTransactionHeader>();
			transHeader2.AH_TransactionNum = "00001006";
			transHeader2.AH_OH = org.PK;
			transHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader2.AH_TransactionType = TransactionTypes.Invoice;
			transHeader2.AH_ConsolidatedInvoiceRef = @"11111/A";

			AccTransactionHeader transHeader3 = Factory.New<AccTransactionHeader>();
			transHeader3.AH_TransactionNum = "00001009";
			transHeader3.AH_OH = org.PK;
			transHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader3.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader3.AH_TransactionType = TransactionTypes.Invoice;
			transHeader3.AH_ConsolidatedInvoiceRef = @"111111";

			AccTransactionHeaderCollection collection = new InvoiceLoader(Factory).GetInvoicesForUniqueRef("");

			AssertEquals("Collection should be empty", 0, collection.Count);

			collection = new InvoiceLoader(Factory).GetInvoicesForUniqueRef(new ZGuid[] { org.PK }, "");

			AssertEquals("Collection should still be empty", 0, collection.Count);

			collection = new InvoiceLoader(Factory).GetInvoicesForUniqueRef("11111");

			AssertEquals("Collection should be empty", 2, collection.Count);
		}

		public void TestGetInvoicesForOrgAndUniqueRef()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			AccTransactionHeader transHeader1 = Factory.New<AccTransactionHeader>();
			transHeader1.AH_TransactionNum = "00001001";
			transHeader1.AH_OH = org.PK;
			transHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader1.AH_TransactionType = TransactionTypes.Invoice;
			transHeader1.AH_ConsolidatedInvoiceRef = "11111";

			AccTransactionHeader transHeader2 = Factory.New<AccTransactionHeader>();
			transHeader2.AH_TransactionNum = "00001006";
			transHeader2.AH_OH = org.PK;
			transHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader2.AH_TransactionType = TransactionTypes.Invoice;
			transHeader2.AH_ConsolidatedInvoiceRef = @"11111/A";

			AccTransactionHeader transHeader3 = Factory.New<AccTransactionHeader>();
			transHeader3.AH_TransactionNum = "00001010";
			transHeader3.AH_OH = org.PK;
			transHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader3.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader3.AH_TransactionType = TransactionTypes.Invoice;
			transHeader3.AH_ConsolidatedInvoiceRef = "22222";

			AccTransactionHeader transHeader4 = Factory.New<AccTransactionHeader>();
			transHeader4.AH_TransactionNum = "00001050";
			transHeader4.AH_OH = org.PK;
			transHeader4.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader4.AH_Ledger = LedgerTypes.AccountsPayable; // doesn't belong in the results
			transHeader4.AH_TransactionType = TransactionTypes.Invoice;
			transHeader4.AH_ConsolidatedInvoiceRef = "33333";

			InvoiceLoader loader = new InvoiceLoader(Factory);

			AccTransactionHeaderCollection invoicesLoaded = loader.GetInvoicesForUniqueRef(new ZGuid[] { org.PK }, "11111");

			AssertEquals("Invoices Loaded", 2, invoicesLoaded.Count);
			AssertEquals("Contains first invoice", true, invoicesLoaded.Contains(transHeader1.PK));
			AssertEquals("Contains second invoice", true, invoicesLoaded.Contains(transHeader2.PK));
			AssertEquals("Doesn't contain third invoice", false, invoicesLoaded.Contains(transHeader3.PK));
			AssertEquals("Doesn't contain fourth invoice", false, invoicesLoaded.Contains(transHeader4.PK));
		}

		public void TestGetInvoicesForUniqueRef()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();

			AccTransactionHeader transHeader1 = Factory.New<AccTransactionHeader>();
			transHeader1.AH_TransactionNum = "00001001";
			transHeader1.AH_OH = org1.PK;
			transHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader1.AH_TransactionType = TransactionTypes.Invoice;
			transHeader1.AH_ConsolidatedInvoiceRef = "C11111";

			AccTransactionHeader transHeader2 = Factory.New<AccTransactionHeader>();
			transHeader2.AH_TransactionNum = "00001006";
			transHeader2.AH_OH = org2.PK;
			transHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader2.AH_TransactionType = TransactionTypes.Invoice;
			transHeader2.AH_ConsolidatedInvoiceRef = @"C11111/A";

			AccTransactionHeader transHeader3 = Factory.New<AccTransactionHeader>();
			transHeader3.AH_TransactionNum = "00001010";
			transHeader3.AH_OH = org1.PK;
			transHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader3.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader3.AH_TransactionType = TransactionTypes.Invoice;
			transHeader3.AH_ConsolidatedInvoiceRef = "C22222";

			InvoiceLoader loader = new InvoiceLoader(Factory);

			AccTransactionHeaderCollection invoicesLoaded = loader.GetInvoicesForUniqueRef("C11111");

			AssertEquals("Invoices Loaded", 2, invoicesLoaded.Count);
			AssertEquals("Contains first invoice", true, invoicesLoaded.Contains(transHeader1.PK));
			AssertEquals("Contains second invoice", true, invoicesLoaded.Contains(transHeader2.PK));
			AssertEquals("Doesn't contain third invoice", false, invoicesLoaded.Contains(transHeader3.PK));
		}

		public void TestLoadsReversedInvoicesByDefault()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			GlbBranch branch = Factory.New<GlbBranch>();

			AccTransactionHeader transHeader1 = Factory.New<AccTransactionHeader>();
			transHeader1.AH_TransactionNum = "00001001";
			transHeader1.AH_OH = org.PK;
			transHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader1.AH_TransactionType = TransactionTypes.Invoice;
			transHeader1.AH_ConsolidatedInvoiceRef = "11111";

			AccTransactionHeader transHeader2 = Factory.New<AccTransactionHeader>();
			transHeader2.AH_TransactionNum = "00001006";
			transHeader2.AH_OH = org.PK;
			transHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader2.AH_TransactionType = TransactionTypes.Invoice;
			transHeader2.AH_IsCancelled = ZBool.True;
			transHeader2.AH_ConsolidatedInvoiceRef = @"11111/A";

			AccTransactionHeader transHeader3 = Factory.New<AccTransactionHeader>();
			transHeader3.AH_TransactionNum = "00001010";
			transHeader3.AH_OH = org.PK;
			transHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader3.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader3.AH_TransactionType = TransactionTypes.Invoice;
			transHeader3.AH_ConsolidatedInvoiceRef = "22222";

			AccTransactionHeader transHeader4 = Factory.New<AccTransactionHeader>();
			transHeader4.AH_TransactionNum = "00001050";
			transHeader4.AH_OH = org.PK;
			transHeader4.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader4.AH_Ledger = LedgerTypes.AccountsPayable; // doesn't belong in the results
			transHeader4.AH_TransactionType = TransactionTypes.Invoice;
			transHeader4.AH_ConsolidatedInvoiceRef = "33333";

			InvoiceLoader loader = new InvoiceLoader(Factory);

			AccTransactionHeaderCollection invoicesLoaded = loader.GetInvoicesForUniqueRef(new ZGuid[] { org.PK }, "11111");

			AssertEquals("IncludeReversedTransactions true", true, loader.IncludeReversedTransaction);
			AssertEquals("Invoices Loaded", 2, invoicesLoaded.Count);
			AssertEquals("Contains first invoice", true, invoicesLoaded.Contains(transHeader1.PK));
			AssertEquals("Contains second invoice", true, invoicesLoaded.Contains(transHeader2.PK));
			AssertEquals("Doesn't contain third invoice", false, invoicesLoaded.Contains(transHeader3.PK));
			AssertEquals("Doesn't contain fourth invoice", false, invoicesLoaded.Contains(transHeader4.PK));
		}

		public void TestGetInvoicesForOrgAndUniqueRefNotReversed()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			GlbBranch branch = Factory.New<GlbBranch>();

			AccTransactionHeader transHeader1 = Factory.New<AccTransactionHeader>();
			transHeader1.AH_TransactionNum = "00001001";
			transHeader1.AH_OH = org.PK;
			transHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader1.AH_TransactionType = TransactionTypes.Invoice;
			transHeader1.AH_ConsolidatedInvoiceRef = "11111";

			AccTransactionHeader transHeader2 = Factory.New<AccTransactionHeader>();
			transHeader2.AH_TransactionNum = "00001006";
			transHeader2.AH_OH = org.PK;
			transHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader2.AH_TransactionType = TransactionTypes.Invoice;
			transHeader2.AH_IsCancelled = ZBool.True;
			transHeader2.AH_ConsolidatedInvoiceRef = @"11111/A";

			AccTransactionHeader transHeader3 = Factory.New<AccTransactionHeader>();
			transHeader3.AH_TransactionNum = "00001010";
			transHeader3.AH_OH = org.PK;
			transHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader3.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader3.AH_TransactionType = TransactionTypes.Invoice;
			transHeader3.AH_ConsolidatedInvoiceRef = "22222";

			AccTransactionHeader transHeader4 = Factory.New<AccTransactionHeader>();
			transHeader4.AH_TransactionNum = "00001050";
			transHeader4.AH_OH = org.PK;
			transHeader4.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader4.AH_Ledger = LedgerTypes.AccountsPayable; // doesn't belong in the results
			transHeader4.AH_TransactionType = TransactionTypes.Invoice;
			transHeader4.AH_ConsolidatedInvoiceRef = "33333";

			InvoiceLoader loader = new InvoiceLoader(Factory);
			loader.IncludeReversedTransaction = false;

			AccTransactionHeaderCollection invoicesLoaded = loader.GetInvoicesForUniqueRef(new ZGuid[] { org.PK }, "11111");

			AssertEquals("IncludeReversedTransactions false", false, loader.IncludeReversedTransaction);
			AssertEquals("Invoices Loaded", 1, invoicesLoaded.Count);
			AssertEquals("Contains first invoice", true, invoicesLoaded.Contains(transHeader1.PK));
			AssertEquals("Doesn't second invoice", false, invoicesLoaded.Contains(transHeader2.PK));
			AssertEquals("Doesn't contain third invoice", false, invoicesLoaded.Contains(transHeader3.PK));
			AssertEquals("Doesn't contain fourth invoice", false, invoicesLoaded.Contains(transHeader4.PK));
		}

		public void TestGetInvoicesForCurrentCompany()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			GlbBranch otherBranch = Factory.New<GlbBranch>();
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherBranch.GB_GC = otherCompany.PK;

			AccTransactionHeader transHeader1 = Factory.New<AccTransactionHeader>();
			transHeader1.AH_TransactionNum = "00001001";
			transHeader1.AH_OH = org.PK;
			transHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader1.AH_TransactionType = TransactionTypes.Invoice;
			transHeader1.AH_ConsolidatedInvoiceRef = "11111";

			AccTransactionHeader transHeader2 = Factory.New<AccTransactionHeader>();
			transHeader2.AH_TransactionNum = "00001006";
			transHeader2.AH_OH = org.PK;
			transHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader2.AH_TransactionType = TransactionTypes.Invoice;
			transHeader2.AH_ConsolidatedInvoiceRef = @"11111/A";

			AccTransactionHeader transHeader3 = Factory.New<AccTransactionHeader>();
			transHeader3.AH_TransactionNum = "00001010";
			transHeader3.AH_OH = org.PK;
			transHeader3.AH_GB = otherBranch.PK;
			transHeader3.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader3.AH_TransactionType = TransactionTypes.Invoice;
			transHeader3.AH_ConsolidatedInvoiceRef = @"11111/B";

			AccTransactionHeader transHeader4 = Factory.New<AccTransactionHeader>();
			transHeader4.AH_TransactionNum = "00001050";
			transHeader4.AH_OH = org.PK;
			transHeader4.AH_GB = otherBranch.PK;
			transHeader4.AH_Ledger = LedgerTypes.AccountsReceivable; // doesn't belong in the results
			transHeader4.AH_TransactionType = TransactionTypes.Invoice;
			transHeader4.AH_ConsolidatedInvoiceRef = @"11111/C";

			InvoiceLoader loader = new InvoiceLoader(Factory);

			AccTransactionHeaderCollection invoicesLoaded = loader.GetInvoicesForUniqueRef(new ZGuid[] { org.PK }, "11111");

			AssertEquals("LimitToCurrentCompany true", true, loader.LimitToCurrentCompany);
			AssertEquals("Invoices Loaded", 2, invoicesLoaded.Count);
			AssertEquals("Contains first invoice", true, invoicesLoaded.Contains(transHeader1.PK));
			AssertEquals("Contains second invoice", true, invoicesLoaded.Contains(transHeader2.PK));
			AssertEquals("Doesn't contain third invoice", false, invoicesLoaded.Contains(transHeader3.PK));
			AssertEquals("Doesn't contain fourth invoice", false, invoicesLoaded.Contains(transHeader4.PK));
		}

		public void TestGetInvoicesForAllCompanies()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			GlbBranch otherBranch = Factory.New<GlbBranch>();
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherBranch.GB_GC = otherCompany.PK;

			AccTransactionHeader transHeader1 = Factory.New<AccTransactionHeader>();
			transHeader1.AH_TransactionNum = "00001001";
			transHeader1.AH_OH = org.PK;
			transHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader1.AH_TransactionType = TransactionTypes.Invoice;
			transHeader1.AH_ConsolidatedInvoiceRef = "11111";

			AccTransactionHeader transHeader2 = Factory.New<AccTransactionHeader>();
			transHeader2.AH_TransactionNum = "00001006";
			transHeader2.AH_OH = org.PK;
			transHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			transHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader2.AH_TransactionType = TransactionTypes.Invoice;
			transHeader2.AH_ConsolidatedInvoiceRef = @"11111/A";

			AccTransactionHeader transHeader3 = Factory.New<AccTransactionHeader>();
			transHeader3.AH_TransactionNum = "00001010";
			transHeader3.AH_OH = org.PK;
			transHeader3.AH_GB = otherBranch.PK;
			transHeader3.AH_Ledger = LedgerTypes.AccountsReceivable;
			transHeader3.AH_TransactionType = TransactionTypes.Invoice;
			transHeader3.AH_ConsolidatedInvoiceRef = @"11111/B";

			AccTransactionHeader transHeader4 = Factory.New<AccTransactionHeader>();
			transHeader4.AH_TransactionNum = "00001050";
			transHeader4.AH_OH = org.PK;
			transHeader4.AH_GB = otherBranch.PK;
			transHeader4.AH_Ledger = LedgerTypes.AccountsReceivable; // doesn't belong in the results
			transHeader4.AH_TransactionType = TransactionTypes.Invoice;
			transHeader4.AH_ConsolidatedInvoiceRef = @"11111/C";

			InvoiceLoader loader = new InvoiceLoader(Factory);
			loader.LimitToCurrentCompany = false;

			AccTransactionHeaderCollection invoicesLoaded = loader.GetInvoicesForUniqueRef(new ZGuid[] { org.PK }, "11111");

			AssertEquals("LimitToCurrentCompany true", false, loader.LimitToCurrentCompany);
			AssertEquals("Invoices Loaded", 4, invoicesLoaded.Count);
			AssertEquals("Contains first invoice", true, invoicesLoaded.Contains(transHeader1.PK));
			AssertEquals("Contains second invoice", true, invoicesLoaded.Contains(transHeader2.PK));
			AssertEquals("Contains third invoice", true, invoicesLoaded.Contains(transHeader3.PK));
			AssertEquals("Contains fourth invoice", true, invoicesLoaded.Contains(transHeader4.PK));
		}

		public void TestGetOutstandingARAPTransactions()
		{
			OrgHeader aALSHI = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			OrgHeader aBIGAS = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			OrgHeader aBIMOT = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIMOT");

			InitializeInvoices(TransactionTypes.AdjustmentNote, LedgerTypes.AccountsPayable, aALSHI, true, 100, false);
			InitializeInvoices(TransactionTypes.AdjustmentNote, LedgerTypes.AccountsPayable, aBIGAS, false, 110, true);
			InitializeInvoices(TransactionTypes.AdjustmentNote, LedgerTypes.AccountsReceivable, aALSHI, false, 120, false);
			InitializeInvoices(TransactionTypes.AdjustmentNote, LedgerTypes.AccountsReceivable, aBIMOT, false, 130, true);

			InitializeInvoices(TransactionTypes.CreditNote, LedgerTypes.AccountsPayable, aALSHI, true, 200, true);
			InitializeInvoices(TransactionTypes.CreditNote, LedgerTypes.AccountsPayable, aALSHI, false, 210, true);
			InitializeInvoices(TransactionTypes.CreditNote, LedgerTypes.AccountsReceivable, aBIGAS, false, 220, false);
			InitializeInvoices(TransactionTypes.CreditNote, LedgerTypes.AccountsReceivable, aBIMOT, false, 230, true);

			InitializeInvoices(TransactionTypes.Invoice, LedgerTypes.AccountsPayable, aBIMOT, true, 300, false);
			InitializeInvoices(TransactionTypes.Invoice, LedgerTypes.AccountsPayable, aBIGAS, false, 310, true);
			InitializeInvoices(TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, aALSHI, false, 320, true);
			InitializeInvoices(TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, aALSHI, false, 330, true);

			InitializeInvoices(TransactionTypes.IncompleteInvoice, LedgerTypes.IncompleteTransactions, aALSHI, false, 333, true);

			InitializeInvoices(TransactionTypes.Journal, LedgerTypes.AccountsPayable, aBIGAS, false, 900M, true);
			InitializeInvoices(TransactionTypes.Journal, LedgerTypes.AccountsReceivable, aALSHI, false, 901M, false);

			InitializeInvoices(TransactionTypes.Contra, LedgerTypes.AccountsPayable, aBIGAS, false, 910M, true);
			InitializeInvoices(TransactionTypes.Contra, LedgerTypes.AccountsReceivable, aALSHI, false, 911M, false);

			InitializeInvoices(TransactionTypes.Transfer, LedgerTypes.AccountsPayable, aBIGAS, false, 920M, true);
			InitializeInvoices(TransactionTypes.Transfer, LedgerTypes.AccountsReceivable, aALSHI, false, 921M, false);

			InitializeInvoices(TransactionTypes.Receipt, LedgerTypes.AccountsPayable, aBIGAS, false, 930M, true);
			InitializeInvoices(TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, aALSHI, false, 931M, false);

			InitializeInvoices(TransactionTypes.Payment, LedgerTypes.AccountsPayable, aBIGAS, false, 940M, true);
			InitializeInvoices(TransactionTypes.Payment, LedgerTypes.AccountsReceivable, aALSHI, false, 941M, false);

			Factory.Save();

			InvoiceLoader loader = new InvoiceLoader(Factory);
			AccTransactionHeaderCollection outstandingARAPTransactions = loader.GetOutstandingARAPTransactions(
				TransactionTypes.Invoice,
				TransactionTypes.CreditNote,
				TransactionTypes.AdjustmentNote,
				TransactionTypes.Journal,
				TransactionTypes.Contra,
				TransactionTypes.Transfer,
				TransactionTypes.Receipt,
				TransactionTypes.Payment
				);
			AssertEquals("It should be loaded 12 transactions", 12, outstandingARAPTransactions.Count);

			outstandingARAPTransactions = loader.GetOutstandingARAPTransactions(TransactionTypes.Invoice);
			AssertEquals("It should be loaded 3 Invoices", 3, outstandingARAPTransactions.Count);

			outstandingARAPTransactions = loader.GetOutstandingARAPTransactions(TransactionTypes.AdjustmentNote);
			AssertEquals("It should be loaded 2 AdjustmentNotes", 2, outstandingARAPTransactions.Count);

			outstandingARAPTransactions = loader.GetOutstandingARAPTransactions(TransactionTypes.CreditNote);
			AssertEquals("It should be loaded 2 CreditNotes", 2, outstandingARAPTransactions.Count);

			outstandingARAPTransactions = loader.GetOutstandingARAPTransactions(TransactionTypes.Journal);
			AssertEquals("It should be loaded 1 Journal", 1, outstandingARAPTransactions.Count);

			outstandingARAPTransactions = loader.GetOutstandingARAPTransactions(TransactionTypes.Contra);
			AssertEquals("It should be loaded 1 Contra", 1, outstandingARAPTransactions.Count);

			outstandingARAPTransactions = loader.GetOutstandingARAPTransactions(TransactionTypes.Transfer);
			AssertEquals("It should be loaded 1 Transfer", 1, outstandingARAPTransactions.Count);

			outstandingARAPTransactions = loader.GetOutstandingARAPTransactions(TransactionTypes.Receipt);
			AssertEquals("It should be loaded 1 Receipt", 1, outstandingARAPTransactions.Count);

			outstandingARAPTransactions = loader.GetOutstandingARAPTransactions(TransactionTypes.Payment);
			AssertEquals("It should be loaded 1 Payment", 1, outstandingARAPTransactions.Count);
		}

		public void TestGetWrappersForARInvoice()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			const string invoiceReference = "1";
			InitializeInvoices(TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, org1, false, 100M, true, invoiceReference);
			InitializeInvoices(TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, org2, false, 200M, true, invoiceReference);
			InitializeInvoices(TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, org1, false, 300M, false, invoiceReference);
			InitializeInvoices(TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, org1, false, 400M, true, invoiceReference);

			Factory.Save();

			var loader = new InvoiceLoader(Factory);

			//doc builder
			var wrappers = loader.GetWrappersForARInvoice(new ZGuid[] { org1.PK }, invoiceReference, true);
			AssertEquals(2, wrappers.Length);
			AssertEquals(500m,
				((AccTransactionHeader)wrappers[0].WrappedObject).AH_InvoiceAmount
				+ ((AccTransactionHeader)wrappers[1].WrappedObject).AH_InvoiceAmount);

			wrappers = loader.GetWrappersForARInvoice(new ZGuid[] { org2.PK }, invoiceReference, true);
			AssertEquals(1, wrappers.Length);
			AssertEquals(200m, ((AccTransactionHeader)wrappers[0].WrappedObject).AH_InvoiceAmount);

			wrappers = loader.GetWrappersForARInvoice(new ZGuid[] { org1.PK, org2.PK }, invoiceReference, true);
			AssertEquals(3, wrappers.Length);
			AssertEquals(700m,
				((AccTransactionHeader)wrappers[0].WrappedObject).AH_InvoiceAmount
				+ ((AccTransactionHeader)wrappers[1].WrappedObject).AH_InvoiceAmount
				+ ((AccTransactionHeader)wrappers[2].WrappedObject).AH_InvoiceAmount);

			//ar invoice
			wrappers = loader.GetWrappersForARInvoice(new ZGuid[] { org1.PK }, invoiceReference, false);
			AssertEquals(2, wrappers.Length);
			AssertEquals(500m,
				((AccTransactionHeader)wrappers[0].WrappedObject).AH_InvoiceAmount
				+ ((AccTransactionHeader)wrappers[1].WrappedObject).AH_InvoiceAmount);

			wrappers = loader.GetWrappersForARInvoice(new ZGuid[] { org2.PK }, invoiceReference, false);
			AssertEquals(1, wrappers.Length);
			AssertEquals(200m, ((AccTransactionHeader)wrappers[0].WrappedObject).AH_InvoiceAmount);

			wrappers = loader.GetWrappersForARInvoice(new ZGuid[] { org1.PK, org2.PK }, invoiceReference, false);
			AssertEquals(3, wrappers.Length);
			AssertEquals(700m,
				((AccTransactionHeader)wrappers[0].WrappedObject).AH_InvoiceAmount
				+ ((AccTransactionHeader)wrappers[1].WrappedObject).AH_InvoiceAmount
				+ ((AccTransactionHeader)wrappers[2].WrappedObject).AH_InvoiceAmount);
		}

		#region Implementation

		void InitializeInvoices(ZString transactionType, ZString ledger, OrgHeader organization, bool isFullyPaid, Decimal oSTotal, bool isItCurrentBranch, ZString referenceNumber)
		{
			var invoice = InitializeInvoices(transactionType, ledger, organization, isFullyPaid, oSTotal, isItCurrentBranch);
			invoice.AH_ConsolidatedInvoiceRef = referenceNumber;
		}

		AccTransactionHeader InitializeInvoices(ZString transactionType, ZString ledger, OrgHeader organization, bool isFullyPaid, Decimal oSTotal, bool isItCurrentBranch)
		{
			AccTransactionHeader testInvoicingBase = Factory.NewWithValidTestData<AccTransactionHeader>();
			testInvoicingBase.AH_TransactionType = transactionType;
			testInvoicingBase.AH_Ledger = ledger;
			testInvoicingBase.AH_OH = organization.PK;
			if (!isItCurrentBranch)
			{
				testInvoicingBase.AH_GB = NonCurrentCompanyBranch.PK;
			}
			if (isFullyPaid)
			{
				testInvoicingBase.AH_FullyPaidDate = ZDateTime.Now.AddDays(-4);
			}
			testInvoicingBase.AH_OSTotal = oSTotal;
			testInvoicingBase.AH_OutstandingAmount = testInvoicingBase.AH_OSTotal;
			testInvoicingBase.AH_InvoiceAmount = testInvoicingBase.AH_OutstandingAmount;

			return testInvoicingBase;
		}

		GlbBranch NonCurrentCompanyBranch
		{
			get { return nonCurrentBranch ?? (nonCurrentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK))); }
		}
		GlbBranch nonCurrentBranch;

		#endregion
	}
}
