using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AccTransactionHeaderValidationTest : BusinessObjectValidationTestCase
	{
		#region Data Refresh Bus Update Validation Tests

		#region AH_Ledger and AH_TransactionType

		public void TestAH_LedgerAndAHTransactionTypeBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var paLedger = new ZString(LedgerTypes.TransactionsPendingAllocation);
			var apLedger = new ZString(LedgerTypes.AccountsPayable);
			var arLedger = new ZString(LedgerTypes.AccountsReceivable);
			var paType = new ZString(TransactionTypes.InvoicePendingAllocation);
			var apType = new ZString(TransactionTypes.Invoice);
			var arType = new ZString(TransactionTypes.CreditNote);
			AssertLedgerAndTransactionTypeBeingChangedByDataRefreshBus((paLedger, paType), (apLedger, apType), (arLedger, arType));
		}

		#endregion

		#endregion

		#region AH_FullyPaidDate

		public virtual void TestAH_FullyPaidDateBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var date1 = ZDateTime.Today;
			var date2 = ZDateTime.Today.AddDays(1);
			var date3 = ZDateTime.Today.AddDays(2);
			AssertPropertyBeingChangedByDataRefreshBus(AccTransactionHeaderSchema.Constants.AH_FullyPaidDate, date1, date2, date3);
		}

		#endregion

		#region AH_OutstandingAmount

		static ZBool ShouldTestAH_OutstandingAmountForThisHeaderType(AccTransactionHeader header)
		{
			var isMiscellaneousTransaction = header.AH_TransactionType == TransactionTypes.Discount || header.AH_TransactionType == TransactionTypes.ExchangeDifference || header.AH_TransactionType == TransactionTypes.Overpayment;
			var isAROrAPTransaction = header.AH_Ledger == LedgerTypes.AccountsPayable || header.AH_Ledger == LedgerTypes.AccountsReceivable;
			return isAROrAPTransaction && !isMiscellaneousTransaction;
		}

		public void TestAH_OutstandingAmountBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegistryIsSetToAnyChange()
		{
			var subscriberFactory = new BusinessObjectFactory();
			var publisherFactory = new BusinessObjectFactory();

			var subscriberTransactionHeader = (AccTransactionHeader)subscriberFactory.NewWithValidTestData(HeaderType);
			if (ShouldTestAH_OutstandingAmountForThisHeaderType(subscriberTransactionHeader))
			{
				subscriberTransactionHeader.AH_TransactionNum = "0001";
				SetupTransactionForOutstandingAmountTest(subscriberTransactionHeader);
				subscriberFactory.Save();

				var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
				glHeader.AG_AccountNum = "999.888.88";
				glHeader.AG_AccountType = "BSH";

				var bankAccount = Factory.New<AccBankAccount>();
				bankAccount.AB_Code = "TSTBNK";
				bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
				bankAccount.AB_AG = glHeader.PK;
				Factory.Save();

				var publisherTransactionHeader = (AccTransactionHeader)publisherFactory.Load(HeaderType, subscriberTransactionHeader.PK);
				CreateDiscountAndPartPayTransaction(publisherTransactionHeader, bankAccount.PK, 10m, "M01");
				Assert(!subscriberTransactionHeader.HasChanges);
				publisherFactory.Save();

				subscriberTransactionHeader.RunPreSaveValidation();
				var expectedErrorMessage = "This record was modified by this user during another operation. Please cancel your changes and reload the form.";
				AssertNoRowError(subscriberTransactionHeader, expectedErrorMessage);
				Assert(!subscriberTransactionHeader.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));

				CreateDiscountAndPartPayTransaction(publisherTransactionHeader, bankAccount.PK, 20m, "M02");

				subscriberTransactionHeader.AH_Desc = "Test";

				Assert(subscriberTransactionHeader.HasChanges);
				publisherFactory.Save();
				subscriberTransactionHeader.RunPreSaveValidation();

				AssertHasRowError(subscriberTransactionHeader, expectedErrorMessage);

				Assert(subscriberTransactionHeader.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
			}
			else
			{
				Assert("Only applicable for AR and AP transactions.", true);
			}
		}

		protected virtual void SetupTransactionForOutstandingAmountTest(AccTransactionHeader header)
		{
			header.AH_InvoiceAmount = header.AH_OSTotal = header.AH_OutstandingAmount = 100m;
		}

		void CreateDiscountAndPartPayTransaction(AccTransactionHeader invoice, ZGuid bankAccountPK, ZDecimal amount, ZString matchGroupNumber)
		{
			var sign = Math.Sign(invoice.AH_OutstandingAmount);
			var factory = invoice.Factory;
			var discount = factory.New<AccTransactionHeader>();
			discount.AH_GB = GlbBranch.CurrentBranch.PK;
			discount.AH_GE = GlbDepartment.CurrentDepartment.PK;
			discount.AH_TransactionNum = discount.PK.ToString().Replace("-", "").Substring(0, 5);
			discount.AH_Ledger = invoice.AH_Ledger;
			discount.AH_TransactionType = TransactionTypes.Discount;
			discount.AH_PostDate = discount.AH_DueDate = discount.AH_InvoiceDate = ZDateTime.Today;
			discount.AH_OH = invoice.AH_OH;
			discount.AH_AB = bankAccountPK;
			discount.AH_InvoiceAmount = -amount * sign;
			discount.AH_OutstandingAmount = 0m;

			var matchLink1 = factory.New<AccTransactionMatchLink>();
			matchLink1.AP_AH = discount.PK;
			matchLink1.AP_Amount = -amount * sign;
			matchLink1.AP_MatchDate = ZDateTime.Today;
			matchLink1.AP_MatchGroupNum = matchGroupNumber;

			invoice.AH_OutstandingAmount -= amount * sign;

			var matchLink2 = factory.New<AccTransactionMatchLink>();
			matchLink2.AP_AH = invoice.PK;
			matchLink2.AP_Amount = amount * sign;
			matchLink2.AP_MatchDate = ZDateTime.Today;
			matchLink2.AP_MatchGroupNum = matchGroupNumber;

			var matchLinkGroup = new MatchLinkGroupForTest(factory);
			matchLinkGroup.Add(matchLink1);
			matchLinkGroup.Add(matchLink2);
		}

		class MatchLinkGroupForTest : BusinessObjectCollection<AccTransactionMatchLink>, ISupportCriticalValidation
		{
			public MatchLinkGroupForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ICriticalValidation CriticalValidation => new DummyCriticalValidation();

			public void SetConflictWithCriticalFieldsBusinessContext()
			{
				throw new NotImplementedException();
			}
		}

		class DummyCriticalValidation : ICriticalValidation
		{
			void ICriticalValidation.RegisterOnSavingCheck()
			{
			}

			void ICriticalValidation.RunOnSavingCheck()
			{
			}

			void ICriticalValidation.RunDeletedObjectOnSavingCheck()
			{
			}

			public void RunAfterSavingCheck()
			{
			}
		}

		#endregion

		#region AH_IsCancelled

		public virtual void TestAH_IsCancelledBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var isCancelled1 = new ZBool(true);
			var isCancelled2 = new ZBool(false);
			var isCancelled3 = new ZBool(true);
			AssertAH_IsCancelledBeingChangedByDataRefreshBus();
		}

		protected void AssertAH_IsCancelledBeingChangedByDataRefreshBus()
		{
			var expectedErrorMessage = "This record was modified by this user during another operation. Please cancel your changes and reload the form.";
			var subscriberFactory = new BusinessObjectFactory();
			var publisherFactory = new BusinessObjectFactory();

			var originalTransaction1 = (AccTransactionHeader)Factory.NewWithValidTestData(HeaderType);
			if (originalTransaction1.AH_Ledger != LedgerTypes.TransactionsPendingAllocation)
			{
				originalTransaction1.AH_TransactionNum = "0001";
				originalTransaction1.AH_Desc = "OriginalTransaction1";
				CreateMatchGroupForMiscellaneousTransaction(originalTransaction1, "M01");
				var originalTransaction2 = (AccTransactionHeader)Factory.NewWithValidTestData(HeaderType);
				originalTransaction2.AH_TransactionNum = "0002";
				originalTransaction2.AH_Desc = "OriginalTransaction2";
				CreateMatchGroupForMiscellaneousTransaction(originalTransaction2, "M02");
				Factory.Save();

				var subscriberTransactionHeader1 = (AccTransactionHeader)subscriberFactory.Load(HeaderType, originalTransaction1.PK);
				var publisherTransactionHeader1 = (AccTransactionHeader)publisherFactory.Load(HeaderType, originalTransaction1.PK);
				GenerateReverseTransactionForTest(publisherTransactionHeader1, "M01");
				Assert(!subscriberTransactionHeader1.HasChanges);
				publisherFactory.Save();
				subscriberTransactionHeader1.RunPreSaveValidation();
				AssertNoRowError(subscriberTransactionHeader1, expectedErrorMessage);
				Assert(!subscriberTransactionHeader1.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
			}
			else
			{
				Assert("Unallocated transactions are deleted, not cancelled.", true);
			}
		}

		protected virtual void GenerateReverseTransactionForTest(AccTransactionHeader header, ZString matchGroupNumber)
		{
			header.AH_IsCancelled = true;
		}

		protected virtual void CreateMatchGroupForMiscellaneousTransaction(AccTransactionHeader header, ZString matchGroupNumber) { }

		#endregion

		#region AH_AH_InvoiceStatement

		public virtual void TestAH_AH_InvoiceStatementBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			DeleteExistingHeader();
			var invoiceBatch1 = CreateInvoiceBatch("InvoiceBatch1");
			var invoiceBatch2 = CreateInvoiceBatch("InvoiceBatch2");
			var invoiceBatch3 = CreateInvoiceBatch("InvoiceBatch3");
			Factory.Save();
			AssertPropertyBeingChangedByDataRefreshBus(AccTransactionHeaderSchema.Constants.AH_AH_InvoiceStatement, invoiceBatch1.PK, invoiceBatch2.PK, invoiceBatch3.PK);
		}

		void DeleteExistingHeader()
		{
			var exisitingHeader = Factory.LoadTop1<AccTransactionHeader>(new ZQuery());
			if (exisitingHeader != null)
			{
				if (exisitingHeader.AH_Ledger == LedgerTypes.CashBook && exisitingHeader.AH_TransactionType == TransactionTypes.Transfer)
				{
					exisitingHeader.Factory.Save();
				}
				exisitingHeader.Delete();
			}
		}

		AccTransactionHeader CreateInvoiceBatch(ZString transactionNumber)
		{
			var invoiceBatch = Factory.New<AccTransactionHeader>();
			invoiceBatch.AH_TransactionNum = transactionNumber;
			invoiceBatch.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceBatch.AH_TransactionType = TransactionTypes.InvoiceBatch;
			invoiceBatch.AH_InvoiceDate = ZDateTime.Today;
			invoiceBatch.AH_GB = GlbBranch.CurrentBranch.PK;
			invoiceBatch.AH_GE = GlbDepartment.CurrentDepartment.PK;
			return invoiceBatch;
		}

		#endregion

		#region AH_ReceiptBatchNo

		public virtual void TestAH_ReceiptBatchNoBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var batchNo1 = new ZString("1111");
			var batchNo2 = new ZString("2222");
			var batchNo3 = new ZString("3333");
			AssertPropertyBeingChangedByDataRefreshBus(AccTransactionHeaderSchema.Constants.AH_ReceiptBatchNo, batchNo1, batchNo2, batchNo3);
		}

		#endregion

		protected void AssertPropertyBeingChangedByDataRefreshBus(string propertyName, IZType value1, IZType value2, IZType value3)
		{
			var subscriberFactory = new BusinessObjectFactory();
			var publisherFactory = new BusinessObjectFactory();

			var subscriberTransactionHeader = (AccTransactionHeader)subscriberFactory.NewWithValidTestData(HeaderType);
			var bankAccountPK = ZGuid.Empty;
			SetupLedgerAndTransactionTypeForAH_TransactionNumTesting(propertyName, subscriberTransactionHeader);
			subscriberTransactionHeader[propertyName] = value1;
			subscriberFactory.Save();

			var publisherTransactionHeader = (AccTransactionHeader)publisherFactory.Load(HeaderType, subscriberTransactionHeader.PK);
			publisherTransactionHeader[propertyName] = value2;

			Assert(!subscriberTransactionHeader.HasChanges);
			publisherFactory.Save();

			subscriberTransactionHeader.RunPreSaveValidation();
			var expectedErrorMessage = "This record was modified by this user during another operation. Please cancel your changes and reload the form.";
			AssertNoRowError(subscriberTransactionHeader, expectedErrorMessage);
			Assert(!subscriberTransactionHeader.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));

			publisherTransactionHeader[propertyName] = value3;
			subscriberTransactionHeader.AH_Desc = "Test";

			Assert(subscriberTransactionHeader.HasChanges);
			publisherFactory.Save();
			subscriberTransactionHeader.RunPreSaveValidation();
			
			AssertHasRowError(subscriberTransactionHeader, expectedErrorMessage);
			Assert(subscriberTransactionHeader.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
		}

		void AssertLedgerAndTransactionTypeBeingChangedByDataRefreshBus((IZType ledger, IZType type) valuesPA, (IZType ledger, IZType type) valuesAP, (IZType ledger, IZType type) valuesAR)
		{
			var expectedErrorMessage = "This record was modified by this user during another operation. Please cancel your changes and reload the form.";
			var subscriberFactory1 = new BusinessObjectFactory();
			var subscriberFactory2 = new BusinessObjectFactory();
			var publisherFactory = new BusinessObjectFactory();

			var subscriberTransactionHeader1 = subscriberFactory1.NewWithValidTestData<AccTransactionHeader>();
			subscriberTransactionHeader1["AH_Ledger"] = valuesPA.ledger;
			subscriberTransactionHeader1["AH_TransactionType"] = valuesPA.type;
			subscriberFactory1.Save();

			var subscriberTransactionHeader2 = subscriberFactory2.Load<AccTransactionHeader>(subscriberTransactionHeader1.PK);

			var publisherTransactionHeader = publisherFactory.Load<AccTransactionHeader>(subscriberTransactionHeader1.PK);
			publisherTransactionHeader["AH_Ledger"] = valuesAP.ledger;
			publisherTransactionHeader["AH_TransactionType"] = valuesAP.type;

			subscriberTransactionHeader2.AH_Desc = "Test";

			Assert(!subscriberTransactionHeader1.HasChanges);
			Assert(subscriberTransactionHeader2.HasChanges);
			publisherFactory.Save();

			subscriberTransactionHeader1.RunPreSaveValidation();
			subscriberTransactionHeader2.RunPreSaveValidation();

			Assert("Post-coindition: SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged on transaction header", subscriberTransactionHeader1.HasContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged));
			AssertHasRowError(subscriberTransactionHeader1, expectedErrorMessage);

			Assert("Post-coindition: SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged on transaction header", subscriberTransactionHeader1.HasContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged));
			AssertHasRowError(subscriberTransactionHeader2, expectedErrorMessage);
		}

		void SetupLedgerAndTransactionTypeForAH_TransactionNumTesting(string propertyName, AccTransactionHeader subscriberTransactionHeader)
		{
			if (propertyName == AccTransactionHeaderSchema.AH_TransactionNum.Name && subscriberTransactionHeader.AH_Ledger != LedgerTypes.TransactionsPendingAllocation)
			{
				subscriberTransactionHeader.AH_Ledger = LedgerTypes.IncompleteTransactions;
				switch (subscriberTransactionHeader.AH_TransactionType)
				{
					case TransactionTypes.Invoice:
						subscriberTransactionHeader.AH_TransactionType = TransactionTypes.IncompleteInvoice;
						break;
					case TransactionTypes.CreditNote:
						subscriberTransactionHeader.AH_TransactionType = TransactionTypes.IncompleteCreditNote;
						break;
					case TransactionTypes.AdjustmentNote:
						subscriberTransactionHeader.AH_TransactionType = TransactionTypes.IncompleteAdjustmentNote;
						break;
				}
			}
		}

		protected abstract Type HeaderType { get; }

		public void TestLedgerAndTransactionTypeHasChangedByDataRefreshBusError()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.IncompleteTransactions;
			transaction.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var transactionInAnotherFactory = anotherFactory.Load<AccTransactionHeader>(transaction.PK);
			transactionInAnotherFactory.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionInAnotherFactory.AH_TransactionType = TransactionTypes.Invoice; //currently there is not case when ledger can be changed without transaction type.
			anotherFactory.Save();

			transaction.RunPreSaveValidation();
			AssertHasRowError(transaction, "This record was modified by this user during another operation. Please cancel your changes and reload the form.");
		}

		public void TestValidationNotificationsRemovedBeforeValidateAll()
		{
			AccTransactionHeader result = Factory.NewWithValidTestData<AccTransactionHeader>();

			result.Validation.ValidateAll();
			AssertNoRowErrors(result);

			string expectedError = "ExpectedError";
			result.AddRowError(expectedError);
			AssertHasRowError(result, expectedError);

			result.Validation.ValidateAll();
			AssertHasRowError("Row errors should be cleared unconditionally. Each row error should be cleared in place where it's added.", result, expectedError);
		}

		public void TestCheckAH_RX_NKTransactionCurrency()
		{
			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			header.AH_RX_NKTransactionCurrency = "XXX";
			AssertHasErrors("Invalid Currency", header.AH_RX_NKTransactionCurrencyInfo);

			header.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrors("Valid Currency", header.AH_RX_NKTransactionCurrencyInfo);
		}

		public void TestCheckAH_ComplianceDocumentDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				AccTransactionHeader header = Factory.New<AccTransactionHeader>();
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_InvoiceDate = new ZDateTime(2016, 08, 14);
				header.AH_ComplianceDocumentDate = new ZDate(2016, 08, 13);
				AssertHasError(header.AH_ComplianceDocumentDateInfo, "Compliance doc date must be on or after the invoice date.");
				header.AH_ComplianceDocumentDate = new ZDate(2016, 08, 14);
				AssertNoErrors(header.AH_ComplianceDocumentDateInfo);
				header.AH_ComplianceDocumentDate = new ZDate(2016, 08, 15);
				AssertNoErrors(header.AH_ComplianceDocumentDateInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
			{
				AccTransactionHeader header = Factory.New<AccTransactionHeader>();
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_InvoiceDate = new ZDateTime(2016, 08, 14);
				header.AH_ComplianceDocumentDate = new ZDate(2016, 08, 13);
				AssertNoError(header.AH_ComplianceDocumentDateInfo, "Compliance doc date must be on or after the invoice date.");
				header.AH_ComplianceDocumentDate = new ZDate(2016, 08, 14);
				AssertNoErrors(header.AH_ComplianceDocumentDateInfo);
				header.AH_ComplianceDocumentDate = new ZDate(2016, 08, 15);
				AssertNoErrors(header.AH_ComplianceDocumentDateInfo);
			}
		}

		#region TestCheckAH_ComplianceSubType

		public virtual void TestCheckAH_ComplianceSubType()
		{
			AssertCheckAH_ComplianceSubType(LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			AssertCheckAH_ComplianceSubType(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote);
			AssertCheckAH_ComplianceSubType(LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote);

			AssertCheckAH_ComplianceSubType(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			AssertCheckAH_ComplianceSubType(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			AssertCheckAH_ComplianceSubType(LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote);

			AssertCheckAH_ComplianceSubType(LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.Invoice);
			AssertCheckAH_ComplianceSubType(LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.CreditNote);
			AssertCheckAH_ComplianceSubType(LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.AdjustmentNote);

			AssertCheckAH_ComplianceSubType(LedgerTypes.IncompleteTransactions, TransactionTypes.IncompleteInvoice);
			AssertCheckAH_ComplianceSubType(LedgerTypes.IncompleteTransactions, TransactionTypes.IncompleteCreditNote);
			AssertCheckAH_ComplianceSubType(LedgerTypes.IncompleteTransactions, TransactionTypes.IncompleteAdjustmentNote);
		}

		void AssertCheckAH_ComplianceSubType(ZString ledgerType, ZString transactionType)
		{
			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			header.AH_Ledger = ledgerType;
			header.AH_TransactionType = transactionType;

			if (header.AH_Ledger == LedgerTypes.AccountsPayable
				|| header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions
				|| header.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Peru))
				{
					header.AH_ComplianceSubType = "TXI";
					AssertNoErrors("Valid subtype", header.AH_ComplianceSubTypeInfo);
					header.AH_ComplianceSubType = "XXX";
					AssertHasError(header.AH_ComplianceSubTypeInfo, "Enter a valid Compliance Sub Type.");
					header.AH_ComplianceSubType = "";
					AssertNoErrors("subtype is optional", header.AH_ComplianceSubTypeInfo);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Afghanistan))
				{
					header.AH_ComplianceSubType = "XXX";
					AssertNoErrors("expect no error as country is not PERU", header.AH_ComplianceSubTypeInfo);
					header.AH_ComplianceSubType = "";
					AssertNoErrors("expect no error as country is not PERU", header.AH_ComplianceSubTypeInfo);
				}
			}
			else if (header.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Peru))
				{
					header.AH_ComplianceSubType = "TXI";
					AssertNoErrors("Valid subtype", header.AH_ComplianceSubTypeInfo);
					header.AH_ComplianceSubType = "XXX";
					AssertHasError(header.AH_ComplianceSubTypeInfo, "Enter a valid Compliance Sub Type.");
					header.AH_ComplianceSubType = "";
					AssertNoErrors("expect no error as AH_TransactionReference is empty", header.AH_ComplianceSubTypeInfo);
					header.AH_TransactionReference = "123456";
					header.AH_ComplianceSubType = "TXI";
					header.AH_ComplianceSubType = "";
					AssertNoErrors("expect no error", header.AH_ComplianceSubTypeInfo);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Afghanistan))
				{
					header.AH_ComplianceSubType = "XXX";
					AssertNoErrors("expect no error as country is not PERU", header.AH_ComplianceSubTypeInfo);
					header.AH_ComplianceSubType = "";
					AssertNoErrors("expect no error as country is not PERU", header.AH_ComplianceSubTypeInfo);
					header.AH_TransactionReference = "123456";
					header.AH_ComplianceSubType = "TXI";
					header.AH_ComplianceSubType = "";
					AssertNoErrors("expect no error", header.AH_ComplianceSubTypeInfo);
				}
			}
		}

		public void TestValidateWarningAndErrorComplianceSubType()
		{
			var country = Constants.CountryCodes.AlandIslands;
			TestMockObjectCreator.SetupSupportComplianceSubType(country);

			AccTransactionHeader header;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				AssertEquals("Precondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				AssertWarnAndError(LedgerTypes.AccountsReceivable);
				AssertWarnAndError(LedgerTypes.AccountsPayable);

				AssertAH_Ledger(LedgerTypes.UnapprovedPayableTransactions);
				AssertAH_Ledger(LedgerTypes.IncompleteTransactions);
			}

			void AssertWarnAndError(string ledger)
			{
				setupTest(ledger);
				((INeedRow)header).Row.AcceptChanges();
				AssertIsDataseAndHasChanges(true, false, false);
				header.Validation.ValidateAH_ComplianceSubType();
				AssertHasWarnings(header.AH_ComplianceSubTypeInfo);

				setupTest(ledger);
				AssertIsDataseAndHasChanges(false, false, false);
				header.Validation.ValidateAH_ComplianceSubType();
				AssertHasErrors(header.AH_ComplianceSubTypeInfo);

				setupTest(ledger);
				((INeedRow)header).Row.AcceptChanges();
				header.AH_ComplianceSubType = "XX1";
				AssertIsDataseAndHasChanges(true, false, true);
				header.Validation.ValidateAH_ComplianceSubType();
				AssertHasErrors(header.AH_ComplianceSubTypeInfo);

				setupTest(ledger);
				((INeedRow)header).Row.AcceptChanges();
				header.AH_Ledger = ledger == LedgerTypes.AccountsReceivable ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
				AssertIsDataseAndHasChanges(true, true, false);
				header.Validation.ValidateAH_ComplianceSubType();
				AssertHasErrors(header.AH_ComplianceSubTypeInfo);
			}

			void AssertAH_Ledger(string ledger)
			{
				setupTest(ledger);
				((INeedRow)header).Row.AcceptChanges();
				AssertIsDataseAndHasChanges(true, false, false);
				header.Validation.ValidateAH_ComplianceSubType();
				AssertHasErrors(header.AH_ComplianceSubTypeInfo);
			}

			void setupTest(ZString ledger)
			{
				header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_TransactionType = TransactionTypes.Invoice;
				header.AH_ComplianceSubType = "XXX";
				header.AH_Ledger = ledger;
			}

			void AssertIsDataseAndHasChanges(bool isInDatabase, bool hasChangesLedgerInfo, bool hasChangesComplianceSubTyp)
			{
				AssertEquals("Precondition: IsInDatabase", isInDatabase, header.IsInDatabase);
				AssertEquals("Precondition: AH_LedgerInfo.HasChanges", hasChangesLedgerInfo, header.AH_LedgerInfo.HasChanges);
				AssertEquals("Precondition: AH_ComplianceSubTypeInfo.HasChanges", hasChangesComplianceSubTyp, header.AH_ComplianceSubTypeInfo.HasChanges);
			}
		}

		#endregion

		public void TestCheckAH_OA_InvoiceAddressOverride()
		{
			var org = Factory.New<OrgHeader>();
			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			var addressForAnotherOrg = Factory.New<OrgAddress>();
			addressForAnotherOrg.OA_OH = ZGuid.NewZGuid();

			var transaction = Factory.New<AccTransactionHeader>();
			transaction.AH_OH = org.PK;

			transaction.AH_OA_InvoiceAddressOverride = address.PK;
			AssertNoErrors(transaction.AH_OA_InvoiceAddressOverrideInfo);

			transaction.AH_OA_InvoiceAddressOverride = addressForAnotherOrg.PK;
			AssertHasError(transaction.AH_OA_InvoiceAddressOverrideInfo, "The Address Override must belong to the Account.");

			transaction.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			AssertNoErrors(transaction.AH_OA_InvoiceAddressOverrideInfo);
		}

		public void TestCheckAH_OC_InvoiceContactOverride()
		{
			var org = Factory.New<OrgHeader>();
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			var contactForAnotherOrg = Factory.New<OrgContact>();
			contactForAnotherOrg.OC_OH = ZGuid.NewZGuid();

			var transaction = Factory.New<AccTransactionHeader>();
			transaction.AH_OH = org.PK;

			transaction.AH_OC_InvoiceContactOverride = contact.PK;
			AssertNoErrors(transaction.AH_OC_InvoiceContactOverrideInfo);

			transaction.AH_OC_InvoiceContactOverride = contactForAnotherOrg.PK;
			AssertHasError(transaction.AH_OC_InvoiceContactOverrideInfo, "The Contact Override must belong to the Account.");

			transaction.AH_OC_InvoiceContactOverride = ZGuid.Empty;
			AssertNoErrors(transaction.AH_OC_InvoiceContactOverrideInfo);
		}

		public void TestBranchDepartmentCombinationValidation_AccTransactionHeaderValidation()
		{
			var bizObj = Factory.NewWithValidTestData<AccTransactionHeader>();

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { bizObj.AH_GB = branch; bizObj.AH_GE = department; }, bizObj.AH_GEInfo);
		}
	}
}
