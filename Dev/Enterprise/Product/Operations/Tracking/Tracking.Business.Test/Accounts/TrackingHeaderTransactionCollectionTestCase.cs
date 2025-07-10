using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingTransactionHeaderCollection))]
	sealed class TrackingHeaderTransactionCollectionTestCase : InvoicingBaseCollectionTest
	{
		#region TestCollectionOnlyReturnsInvoicesAndCreditNotes

		public void TestCollectionOnlyReturnsInvoicesAndCreditNotes()
		{
			//ClearTable(AccTransactionHeader.Schema.TableName);

			BusinessObjectFactory savingFactory = new BusinessObjectFactory();

			AccTransactionHeader trans1 = GetNewTransaction(savingFactory);
			trans1.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionHeader trans2 = GetNewTransaction(savingFactory);
			trans2.AH_TransactionType = TransactionTypes.CreditNote;

			AccTransactionHeader trans3 = GetNewTransaction(savingFactory);
			trans3.AH_TransactionType = TransactionTypes.AdjustmentNote;

			AccTransactionHeader trans4 = GetNewTransaction(savingFactory);
			trans4.AH_TransactionType = TransactionTypes.Discount;

			AccTransactionHeader trans5 = GetNewTransaction(savingFactory);
			trans5.AH_TransactionType = TransactionTypes.Overpayment;

			AccTransactionHeader trans6 = GetNewTransaction(savingFactory);
			trans6.AH_TransactionType = TransactionTypes.Payment;

			AccTransactionHeader trans7 = GetNewTransaction(savingFactory);
			trans7.AH_TransactionType = TransactionTypes.Receipt;

			savingFactory.Save();

			TestCollection.Load();

			AssertEquals("Expected 2 transactions in collection", 2, TestCollection.Count);
			Assert("Transaction 1 should be in the collection", TestCollection.Contains(trans1.PK));
			Assert("Transaction 2 should be in the collection", TestCollection.Contains(trans2.PK));
		}

		public void TestCollectionReturnsInvoicesAndCreditNotesAsCorrectTypes()
		{
			BusinessObjectFactory savingFactory = new BusinessObjectFactory();

			AccTransactionHeader trans1 = GetNewTransaction(savingFactory);
			trans1.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionHeader trans2 = GetNewTransaction(savingFactory);
			trans2.AH_TransactionType = TransactionTypes.CreditNote;

			savingFactory.Save();

			TestCollection.Load();

			AssertEquals("Expected 2 transactions in collection", 2, TestCollection.Count);
			Assert("Transaction 1 should be in the collection", TestCollection.Contains(trans1.PK));
			Assert("Expected Transaction 1 to be an ARInvoice", TestCollection.FindByPK(trans1.PK).GetType().Equals(typeof(ARInvoice)));
			Assert("Transaction 2 should be in the collection", TestCollection.Contains(trans2.PK));
			Assert("Expected Transaction 2 to be an ARCreditNote", TestCollection.FindByPK(trans2.PK).GetType().Equals(typeof(ARCreditNote)));
		}

		#endregion TestCollectionOnlyReturnsInvoicesAndCreditNotes

		#region TestCancelledTransactions

		public void TestCollectionDoesNotReturnCancelledTransactions()
		{
			ARInvoice trans1 = GetNewInvoice();
			ARCreditNote trans2 = GetNewCreditNote();

			ARInvoice trans3 = GetNewInvoice();
			trans3.AH_IsCancelled = ZBool.True;

			ARCreditNote trans4 = GetNewCreditNote();
			trans4.AH_IsCancelled = ZBool.True;

			TestCollection.Load();

			AssertEquals("Expected 2 transactions in collection", 2, TestCollection.Count);
			Assert("Transaction 1 should be in the collection", TestCollection.Contains(trans1));
			Assert("Transaction 2 should be in the collection", TestCollection.Contains(trans2));
		}

		#endregion TestCancelledTransactions

		#region Implementation

		ARCreditNote GetNewCreditNote()
		{
			ARCreditNote creditNote = Factory.New<ARCreditNote>();
			creditNote.AH_OH = TestOrg.PK;
			return creditNote;
		}

		ARInvoice GetNewInvoice()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = TestOrg.PK;
			return invoice;
		}

		AccTransactionHeader GetNewTransaction(BusinessObjectFactory factory)
		{
			AccTransactionHeader transaction = factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			return transaction;
		}

		new TrackingTransactionHeaderCollection TestCollection
		{
			get { return (TrackingTransactionHeaderCollection)Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TrackingTransactionHeaderCollection(WebFactory);
		}

		BusinessObjectFactory WebFactory
		{
			get { return Factory; }
		}

		OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>();
				}
				return fTestOrg;
			}
		}
		OrgHeader fTestOrg;

		#endregion Implementation
	}
}
