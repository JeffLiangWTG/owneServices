using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS.Testing
{
	sealed class TradeTransactionsValueProviderTest : TestCaseWithFactory
	{
		public void TestDetailsWhenEntrySummaryExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "12345678";
			declaration.JE_DeclarationReference = "0123456789";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;

			AssertEquals(1, transactions.Count());
			var transaction = transactions.ElementAt(0);
			AssertEquals(TradeTransactionType.EntrySummary, transaction.Type);
			AssertEquals("XJ5", transaction.FilerOrSCAC);
			AssertEquals("12345678", transaction.Number);
			Assert(transaction.AdditionalNumbers.IsNullOrEmpty());
			AssertEquals("123456789", transaction.ReferenceNumber);

			declaration.US_BRDRefNo = "9876543210";
			transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;
			transaction = transactions.ElementAt(0);
			AssertEquals("876543210", transaction.ReferenceNumber);
		}

		public void TestDetailsWhenEntryExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "12345678";
			declaration.JE_DeclarationReference = "0123456789";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			var transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;

			AssertEquals(1, transactions.Count());
			var transaction = transactions.ElementAt(0);
			AssertEquals(TradeTransactionType.Entry, transaction.Type);
			AssertEquals("XJ5", transaction.FilerOrSCAC);
			AssertEquals("12345678", transaction.Number);
			Assert(transaction.AdditionalNumbers.IsNullOrEmpty());
			AssertEquals("123456789", transaction.ReferenceNumber);

			declaration.US_BRDRefNo = "9876543210";
			transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;
			transaction = transactions.ElementAt(0);
			AssertEquals("876543210", transaction.ReferenceNumber);
		}

		public void TestDetailsWhenBillsExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "12345678";

			declaration.JE_MasterBill = "M432089";
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_HouseBill = "H890342";
			declaration.JE_HouseBillIssuerSCAC = "APLU";
			declaration.ActiveEntryHeaders.AddNew();

			var subhouseBill1 = declaration.Bills.AddNew();
			subhouseBill1.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subhouseBill1.CU_BillNum = "1";
			subhouseBill1.CU_CU_ParentBill = declaration.PrimaryHouseBill.PK;

			var subhouseBill2 = declaration.Bills.AddNew();
			subhouseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subhouseBill2.CU_BillNum = "2";
			subhouseBill2.CU_CU_ParentBill = declaration.PrimaryHouseBill.PK;

			var transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;
			AssertEquals(1, transactions.Count());
			var transaction = transactions.ElementAt(0);
			AssertEquals(TradeTransactionType.Bill, transaction.Type);
			AssertEquals("APLU", transaction.FilerOrSCAC);
			AssertEquals("M432089", transaction.Number);
			AssertEquals(1, transaction.AdditionalNumbers.Count());
			AssertEquals("APLUH890342", transaction.AdditionalNumbers.ElementAt(0));
		}

		public void TestDetailsWhenMultipleHouseBillsExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "12345678";

			declaration.JE_MasterBill = "M432089";
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_HouseBill = "H890342";
			declaration.JE_HouseBillIssuerSCAC = "APLU";
			declaration.ActiveEntryHeaders.AddNew();

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_CU_ParentBill = declaration.PrimaryMasterBill.PK;
			houseBill2.CU_BillNum = "H4389342";
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";

			var transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;
			AssertEquals(2, transactions.Count());
			var transaction = transactions.ElementAt(0);
			AssertEquals(TradeTransactionType.Bill, transaction.Type);
			AssertEquals("APLU", transaction.FilerOrSCAC);
			AssertEquals("M432089", transaction.Number);
			AssertEquals(1, transaction.AdditionalNumbers.Count());
			AssertEquals("APLUH890342", transaction.AdditionalNumbers.ElementAt(0));

			transaction = transactions.ElementAt(1);
			AssertEquals(TradeTransactionType.Bill, transaction.Type);
			AssertEquals("APLU", transaction.FilerOrSCAC);
			AssertEquals("M432089", transaction.Number);
			AssertEquals(1, transaction.AdditionalNumbers.Count());
			AssertEquals("APLUH4389342", transaction.AdditionalNumbers.ElementAt(0));
		}

		public void TestDetailsWhenNoCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "12345678";
			declaration.JE_DeclarationReference = "987654321";

			var transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;
			AssertEquals(1, transactions.Count());
			var transaction = transactions.ElementAt(0);
			AssertEquals(TradeTransactionType.EntrySummary, transaction.Type);
			AssertEquals("XJ5", transaction.FilerOrSCAC);
			AssertEquals("12345678", transaction.Number);
			Assert(transaction.AdditionalNumbers.IsNullOrEmpty());
			AssertEquals("987654321", transaction.ReferenceNumber);

			declaration.US_EnableENS = false;
			transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;
			AssertEquals(1, transactions.Count());
			transaction = transactions.ElementAt(0);
			AssertEquals(TradeTransactionType.Entry, transaction.Type);
			AssertEquals("XJ5", transaction.FilerOrSCAC);
			AssertEquals("12345678", transaction.Number);
			Assert(transaction.AdditionalNumbers.IsNullOrEmpty());
			AssertEquals("987654321", transaction.ReferenceNumber);
		}

		public void TestDetailsForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B001";
			declaration.US_EntryFilerCode = "SV9";
			var entryHeader1 = declaration.EntryHeadersWithOptionalDeactivated.AddNew();
			entryHeader1.CH_BGMReference = "B001";
			entryHeader1.EntryNumber = "00001";
			entryHeader1.US_XTN = "00001XTN";

			var entryHeader2 = declaration.EntryHeadersWithOptionalDeactivated.AddNew();
			entryHeader2.CH_BGMReference = "B002";
			entryHeader2.EntryNumber = "00002";
			entryHeader2.US_XTN = "00002XTN";

			var transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;
			AssertEquals(2, transactions.Count());
			var transaction1 = transactions.ElementAt(0);
			AssertEquals(TradeTransactionType.Export, transaction1.Type);
			AssertEquals("B001", transaction1.ShipmentNo);
			AssertEquals("00001", transaction1.Number);
			AssertEquals("00001XTN", transaction1.XTN);

			var transaction2 = transactions.ElementAt(1);
			AssertEquals(TradeTransactionType.Export, transaction2.Type);
			AssertEquals("B002", transaction2.ShipmentNo);
			AssertEquals("00002", transaction2.Number);
			AssertEquals("00002XTN", transaction2.XTN);
		}

		public void TestDetailsForFTZAdmission()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.FTZAdmissionNumber = "1530001|18|0001234";
			declaration.JE_DeclarationReference = "987654321";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var transactions = new TradeTransactionsValueProvider(declaration).TradeTransactions;
			AssertEquals(1, transactions.Count());
			var transaction = transactions.ElementAt(0);
			AssertEquals(TradeTransactionType.FTZAdmission, transaction.Type);
			AssertEquals(ZString.Empty, transaction.FilerOrSCAC);
			AssertEquals("1530001|18|0001234", transaction.Number);
			Assert(transaction.AdditionalNumbers.IsNullOrEmpty());
			AssertEquals("987654321", transaction.ReferenceNumber);
		}
	}
}
