using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.Testing.AccTransactionMatchLinkCriticalValidationTest;
namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccTransactionHeaderCriticalValidationTest : CriticalValidationTest<AccTransactionHeader>
	{
		#region Validate Outstanding Amount

		public void TestCheckOutstandingAmountIsValid_ForOpeningReceipt()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();
			var transaction = creator.CreateTransaction(Factory, LedgerTypes.CashBook, TransactionTypes.OpeningReceipt);

			transaction.AH_OutstandingAmount = 101m;
			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OutstandingAmount", true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage,
					"Ledger = CB, Transaction Type = ORC"));
		}

		public void TestCheckOutstandingAmountIsValid_ForOpeningPayment()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();
			var transaction = creator.CreateTransaction(Factory, LedgerTypes.CashBook, TransactionTypes.OpeningPayment);

			transaction.AH_OutstandingAmount = 101m;
			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OutstandingAmount", true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage,
					"Ledger = CB, Transaction Type = OPY"));
		}

		public void TestCheckOutstandingAmountIsValid_WhenAH_OutstandingAmountInfoHasChanges()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();
			var transaction = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			AssertNoExceptionThrown(() => Factory.Save());

			transaction.AH_OutstandingAmount = 110m;
			AssertEquals("Precondition", true, transaction.IsInDatabase);
			AssertEquals("Precondition", true, transaction.AH_OutstandingAmountInfo.HasChanges);
			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OutstandingAmount", true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage));
		}

		public void TestCheckOutstandingAmountIsValid_IncludeMatchLinks()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();

			var arINV = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			var matchLinkForARINV = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLinkForARINV.AP_MatchGroupNum = "M001";
			matchLinkForARINV.AP_AH = arINV.PK;
			matchLinkForARINV.AP_Amount = 100m;
			arINV.AH_OutstandingAmount = 0m;

			var arCRD = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			var matchLinkForARCRD = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLinkForARCRD.AP_MatchGroupNum = "M001";
			matchLinkForARCRD.AP_AH = arCRD.PK;
			matchLinkForARCRD.AP_Amount = -100m;
			arCRD.AH_OutstandingAmount = 0m;

			var group = new MatchLinkGroupForTest(Factory);
			group.Add(matchLinkForARINV);
			group.Add(matchLinkForARCRD);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Precondition", true, arINV.IsInDatabase);
			AssertEquals("Precondition", true, arCRD.IsInDatabase);

			matchLinkForARINV.AP_Amount = 99m;
			AssertEquals("Precondition", false, arINV.AH_OutstandingAmountInfo.HasChanges);
			AssertEquals("Precondition", true, matchLinkForARINV.HasChanges);

			AssertOnSavingCheck(arINV, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OutstandingAmount", true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage));

			arCRD.AH_OutstandingAmount = 1m;
			AssertEquals("Precondition", true, arCRD.AH_OutstandingAmountInfo.HasChanges);
			AssertEquals("Precondition", false, matchLinkForARCRD.HasChanges);

			AssertOnSavingCheck(arCRD, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OutstandingAmount", true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage));
		}

		#endregion

		#region Validate OS Outstanding Amount

		public void TestCheckOSOutstandingAmountIsValid_ForOpeningReceipt()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			{
				var creator = ObjectFactory.Get<ITransactionCreator>();
				var transaction = creator.CreateTransaction(Factory, LedgerTypes.CashBook, TransactionTypes.OpeningReceipt);

				transaction.MakeOSOutstandingAmountApplicable(101m);
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OSOutstandingAmount", true,
						CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
						CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage,
						"Ledger = CB, Transaction Type = ORC"));
			}
		}

		public void TestCheckOSOutstandingAmountIsValid_ForOpeningPayment()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			{
				var creator = ObjectFactory.Get<ITransactionCreator>();
				var transaction = creator.CreateTransaction(Factory, LedgerTypes.CashBook, TransactionTypes.OpeningPayment);

				transaction.MakeOSOutstandingAmountApplicable(101m);
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OSOutstandingAmount", true,
						CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
						CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage,
						"Ledger = CB, Transaction Type = OPY"));
			}
		}

		public void TestCheckOSOutstandingAmountIsValid_WhenAH_OSOutstandingAmountInfoHasChanges()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			{
				var creator = ObjectFactory.Get<ITransactionCreator>();
				var transaction = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);

				transaction.MakeOSOutstandingAmountApplicable(100m);
				AssertNoExceptionThrown(() => Factory.Save());

				transaction.AH_OSOutstandingAmount = 110m;
				AssertEquals("Precondition", true, transaction.IsInDatabase);
				AssertEquals("Precondition", true, transaction.AH_OSOutstandingAmountInfo.HasChanges);

				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OSOutstandingAmount", true,
						CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
						CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage));
			}
		}

		public void TestCheckOSOutstandingAmountIsValid_MatchLinkIsInDatabase()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			{
				var creator = ObjectFactory.Get<ITransactionCreator>();

				var arINV = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
				var matchLinkForARINV = Factory.NewWithValidTestData<AccTransactionMatchLink>();
				matchLinkForARINV.AP_MatchGroupNum = "M001";
				matchLinkForARINV.AP_AH = arINV.PK;
				matchLinkForARINV.AP_Amount = matchLinkForARINV.AP_OSAmount = 100m;
				arINV.AH_OutstandingAmount = 0m;
				arINV.MakeOSOutstandingAmountApplicable(0m);

				var arCRD = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
				var matchLinkForARCRD = Factory.NewWithValidTestData<AccTransactionMatchLink>();
				matchLinkForARCRD.AP_MatchGroupNum = "M001";
				matchLinkForARCRD.AP_AH = arCRD.PK;
				matchLinkForARCRD.AP_Amount = matchLinkForARCRD.AP_OSAmount = -100m;
				arCRD.AH_OutstandingAmount = 0m;
				arCRD.MakeOSOutstandingAmountApplicable(0m);

				var group = new MatchLinkGroupForTest(Factory);
				group.Add(matchLinkForARINV);
				group.Add(matchLinkForARCRD);
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Precondition", true, arINV.IsInDatabase);
				AssertEquals("Precondition", true, arCRD.IsInDatabase);

				matchLinkForARINV.AP_OSAmount = 99m;
				AssertEquals("Precondition", false, arINV.AH_OSOutstandingAmountInfo.HasChanges);
				AssertEquals("Precondition", true, matchLinkForARINV.HasChanges);
				AssertOnSavingCheck(arINV, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OSOutstandingAmount", true,
						CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
						CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage));

				arCRD.AH_OSOutstandingAmount = 1m;
				AssertEquals("Precondition", true, arCRD.AH_OSOutstandingAmountInfo.HasChanges);
				AssertEquals("Precondition", false, matchLinkForARCRD.HasChanges);
				AssertOnSavingCheck(arCRD, new TestCaseDefinition_ForSeparateTestsMethods("Incorrect AH_OSOutstandingAmount", true,
						CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
						CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage));
			}
		}

		public void TestCheckOSOutstandingAmountIsValid_WithoutMatchLinks()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			{
				var creator = ObjectFactory.Get<ITransactionCreator>();
				var transaction = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);

				var matchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, transaction.PK);
				var matchLinks = Factory.Load<AccTransactionMatchLink>(matchLinkQuery);
				AssertEquals("Precondition", false, transaction.IsInDatabase);
				AssertEquals("Precondition", false, matchLinks.Any());
				AssertEquals("Precondition", 100m, transaction.AH_OSTotal);

				transaction.MakeOSOutstandingAmountApplicable(100m);
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("No error"));

				transaction.AH_OSOutstandingAmount = 99m;
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("AH_OSOutstandingAmount should NOT be 99m", true,
						CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
						CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage,
						"OS Outstanding Amount is incorrect: os outstanding amount = 99, os total amount = 100, sum ap os amount = 0"));

				transaction.AH_OSOutstandingAmount = -100m;
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("AH_OSOutstandingAmount should NOT be -2m", true,
						CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
						CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage,
						"OS Outstanding Amount is incorrect: os outstanding amount = -100, os total amount = 100"));
			}
		}

		public void TestCheckOSOutstandingAmountIsValid_PartialMatch()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			{
				var creator = ObjectFactory.Get<ITransactionCreator>();
				var transaction = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
				var matchLink1 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
				var matchLink2 = Factory.NewWithValidTestData<AccTransactionMatchLink>();

				matchLink1.AP_AH = transaction.PK;
				matchLink1.AP_OSAmount = 40m;

				matchLink2.AP_AH = transaction.PK;
				matchLink2.AP_OSAmount = 50m;

				var matchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, transaction.PK);
				var matchLinks = Factory.Load<AccTransactionMatchLink>(matchLinkQuery);
				AssertEquals("Precondition", false, transaction.IsInDatabase);
				AssertEquals("Precondition", true, matchLinks.Any());
				AssertEquals("Precondition", 90m, matchLinks.Sum(x => x.AP_OSAmount));
				AssertEquals("Precondition", 100m, transaction.AH_OSTotal);

				transaction.MakeOSOutstandingAmountApplicable(10m);
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("No error"));

				transaction.AH_OSOutstandingAmount = 11m;
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("AH_OSOutstandingAmount should NOT be 1m", true,
						CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
						CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage,
						"OS Outstanding Amount is incorrect: os outstanding amount = 11, os total amount = 100, sum ap os amount = 90",
						"Related Match Links:",
						"Match Link: Group Number =",
						"Match Link: Group Number ="));
			}
		}

		public void TestCheckOSOutstandingAmountIsValid_FullyMatch()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			{
				var creator = ObjectFactory.Get<ITransactionCreator>();
				var transaction = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
				var matchLink1 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
				var matchLink2 = Factory.NewWithValidTestData<AccTransactionMatchLink>();

				matchLink1.AP_AH = transaction.PK;
				matchLink1.AP_OSAmount = 40m;

				matchLink2.AP_AH = transaction.PK;
				matchLink2.AP_OSAmount = 60m;

				var matchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, transaction.PK);
				var matchLinks = Factory.Load<AccTransactionMatchLink>(matchLinkQuery);
				AssertEquals("Precondition", false, transaction.IsInDatabase);
				AssertEquals("Precondition", true, matchLinks.Any());
				AssertEquals("Precondition", 100m, matchLinks.Sum(x => x.AP_OSAmount));
				AssertEquals("Precondition", 100m, transaction.AH_OSTotal);

				transaction.MakeOSOutstandingAmountApplicable(0m);
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("No error"));

				transaction.AH_OSOutstandingAmount = 0.1m;
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("AH_OSOutstandingAmount should NOT be 1m", true,
						CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
						CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage,
						"OS Outstanding Amount is incorrect: os outstanding amount = 0.1, os total amount = 100, sum ap os amount = 100",
						"Related Match Links:",
						"Match Link: Group Number =",
						"Match Link: Group Number ="));
			}
		}

		#endregion

		#region Data Refresh Bus Update Tests

		public void TestAccTransactionHeaderIsSavedSuccessfullyWhileHavingSkipDataRefreshBusUpdateBusinessContexts()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			Assert(!header.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("AccTransactionHeaderWithoutSkippingDataRefreshBusUpdateContexts");
			AssertAfterSavingCheck(header, testCase);

			header = Factory.NewWithValidTestData<AccTransactionHeader>();
			using (new DisposableAction(() => header.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange), () => header.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange)))
			{
				AddCriticalValidationErrorInfo(header);
				Assert(header.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("ChargeWithSkipDataRefreshBusUpdateDueToAnyChangeContext", true,
					CriticalValidationErrorType.TransactionHeaderSkippedDataRefreshBusUpdateButWasSavedSuccessfully_2,
					CriticalValidationMessageTemplate.TransactionHeaderSkippedDataRefreshBusUpdateButWasSavedSuccessfully,
					"Transaction header was modified by this user during another operation.",
					"Transaction header skipped data refresh bus update, but it was saved successfully.",
					$"Header: PK = {header.PK}",
					"Business Contexts = BizObj Level : (SkipDataRefreshBusUpdateDueToAnyChange)",
					"Subscriber:",
					"Publisher: ",
					"StackTrace:");
				AssertAfterSavingCheck(header, testCase);
			}

			void AddCriticalValidationErrorInfo(BusinessObject bizO)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(bizO.Factory).AddInfoWhenAllowed(bizO.PK, CriticalValidationInfoCollectorServiceKeyType.DataRefreshBusUpdateSkipped,
							() =>
							{
								var message = new ZStringBuilder();
								message.AppendLine("Subscriber: ");
								message.AppendLine("Publisher: ");
								message.AppendLine("StackTrace:");
								message.AppendLine(System.Environment.StackTrace);
								return message.ToString();
							},
							CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			}
		}

		public void TestAccTransactionHeaderIsDeletedAndHasSkippedDataRefreshBusUpdateBusinessContextsAndFactoryIsSavedSuccessfully()
		{
			var header1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			Factory.Save();

			header1.Delete();
			Assert(!header1.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("AccTransactionHeaderWithoutSkippingDataRefreshBusUpdateContexts");
			AssertAfterSavingCheck(header1, testCase);

			var header2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			Factory.Save();
			using (new DisposableAction(() => header2.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange), () => header2.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange)))
			{
				header2.Delete();
				Assert(header2.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("AccTransactionHeaderWithSkipDataRefreshBusUpdateDueToAnyChangeContext", true,
					CriticalValidationErrorType.TransactionHeaderSkippedDataRefreshBusUpdateButWasSavedSuccessfully_2,
					CriticalValidationMessageTemplate.TransactionHeaderSkippedDataRefreshBusUpdateButWasSavedSuccessfully,
					"Transaction header was modified by this user during another operation.",
					"Transaction header skipped data refresh bus update, but it was saved successfully.",
					"Header original values:",
					$"PK = {header2.PK}");
				AssertAfterSavingCheck(header2, testCase);
			}
		}

		public void TestAccTransactionHeaderHasSkippedDataRefreshBusUpdateAndFactorySaveThrowConcurrencyError_SkipDataRefreshBusUpdateRegsitryIsAnyChange()
		{
			AssertCriticalErrorNotReportedWhenFactorySaveThrowConcurrencyError(false);
		}

		public void TestAccTransactionHeaderIsDeletedAndHasSkippedDataRefreshBusUpdateBusinessContextsAndFactorySaveThrowConcurrencyError_SkipDataRefreshBusUpdateRegsitryIsAnyChange()
		{
			AssertCriticalErrorNotReportedWhenFactorySaveThrowConcurrencyError(true);
		}

		void AssertCriticalErrorNotReportedWhenFactorySaveThrowConcurrencyError(bool isDeletedHeader)
		{
			var subscriberFactory = new BusinessObjectFactory();
			var subscriberHeader = subscriberFactory.NewWithValidTestData<AccTransactionHeader>();
			subscriberFactory.Save();

			var publisherFactory = new BusinessObjectFactory();
			var publisherHeader = publisherFactory.Load<AccTransactionHeader>(subscriberHeader.PK);

			subscriberHeader.AH_ReceiptBatchNo = "TestReceiptBatch1";
			publisherHeader.AH_ReceiptBatchNo = "TestReceiptBatch2";

			publisherFactory.Save();
			subscriberHeader.RunPreSaveValidation();
			Assert(subscriberHeader.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));

			if (isDeletedHeader)
			{
				subscriberHeader.Delete();
			}

			try
			{
				subscriberFactory.Save();
			}
			catch (OnSavingCriticalCheckException)
			{
				Fail("Did not expect a critical check exception as factory should throw concurrency error");
			}
			catch (ZSaveConcurrencyException ex)
			{
					var rowState = isDeletedHeader ? "Deleted" : "Modified";
					var expectedErrorMessage = FormattableString.Invariant($@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccTransactionHeader
PK: {subscriberHeader.PK}
RowState: {rowState}"); 
					AssertContains(expectedErrorMessage, ex.Message);
			}
		}

		#endregion

		public void TestCheckClearedDateIsEmptyForReceiptsWithNoBatchNumber()
		{
			var clearedDateTestCases = new ClearedDateTestCase[]
			{
				new ClearedDateTestCase() { SetBatchNo = true, SetClearedDate = true, SetIsCancelled = true, ShouldFail =  false },
				new ClearedDateTestCase() { SetBatchNo = true, SetClearedDate = true, SetIsCancelled = false, ShouldFail =  false },
				new ClearedDateTestCase() { SetBatchNo = true, SetClearedDate = false, SetIsCancelled = true, ShouldFail =  false },
				new ClearedDateTestCase() { SetBatchNo = true, SetClearedDate = false, SetIsCancelled = false, ShouldFail =  false },
				new ClearedDateTestCase() { SetBatchNo = false, SetClearedDate = true, SetIsCancelled = true, ShouldFail =  false },
				new ClearedDateTestCase() { SetBatchNo = false, SetClearedDate = true, SetIsCancelled = false, ShouldFail =  true },
				new ClearedDateTestCase() { SetBatchNo = false, SetClearedDate = false, SetIsCancelled = true, ShouldFail =  false },
				new ClearedDateTestCase() { SetBatchNo = false, SetClearedDate = false, SetIsCancelled = false, ShouldFail =  false }
			};

			foreach (var ledgerType in ledgerTypes)
			{
				foreach (var clearedDateTestCase in clearedDateTestCases)
				{
					var transactionNum = NewTransactionNum;
					var headerPK = new Guid();
					var accTransactionHeader = GetNewParentForClearedDateCheck_NotInDB(Factory, ledgerType, clearedDateTestCase, transactionNum, headerPK);

					var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("CheckClearedDateIsEmptyForReceiptsWithNoBatchNumber, Ledger = {0}, SetBatchNo = {1}, SetClearedDate = {2}, SetIsCancelled = {3}", ledgerType, clearedDateTestCase.SetBatchNo, clearedDateTestCase.SetClearedDate, clearedDateTestCase.SetIsCancelled),
							clearedDateTestCase.ShouldFail,
							clearedDateTestCase.ShouldFail ? CriticalValidationErrorType.ClearedReceiptWithoutBatchNumber_3 : CriticalValidationErrorType.NoError,
							"Cleared receipt without a batch number.");

					AssertOnSavingCheck(accTransactionHeader, testCase);
				}
			}
		}

		public void TestCheckClearedDateIsEmptyForReceiptsWithNoBatchNumber_ClearedReceiptLinkedToNonClearedDepositBatchReported()
		{
			var transactionNum = NewTransactionNum;
			var headerPK = new Guid();
			var ledgerType = LedgerTypes.AccountsReceivable;
			var clearedDateTestCase = new ClearedDateTestCase() { SetBatchNo = false, SetClearedDate = true, SetIsCancelled = false, ShouldFail = true };
			var accTransactionHeader = GetNewParentForClearedDateCheck_NotInDB(Factory, ledgerType, clearedDateTestCase, transactionNum, headerPK);

			var propertyInfo = "Properties:";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("CheckClearedDateIsEmptyForReceiptsWithNoBatchNumber, Ledger = {0}, SetBatchNo = {1}, SetClearedDate = {2}, SetIsCancelled = {3}", ledgerType, clearedDateTestCase.SetBatchNo, clearedDateTestCase.SetClearedDate, clearedDateTestCase.SetIsCancelled),
					true,
					CriticalValidationErrorType.ClearedReceiptWithoutBatchNumber_3,
					"Cleared receipt without a batch number.",
					@"
ClearedReceiptLinkedToNonClearedDepositBatch: ",
					propertyInfo);

			AssertOnSavingCheck(accTransactionHeader, testCase);
		}

		public void TestCheckClearedDateIsEmptyForReceiptsWithNoBatchNumber_ClearedReceiptLinkedToNonClearedDepositBatchReported_CollectedInfoIsAlwaysReported()
		{
			var transactionNum = NewTransactionNum;
			var headerPK = new Guid();
			var clearedDateTestCase = new ClearedDateTestCase() { SetBatchNo = false, SetClearedDate = true, SetIsCancelled = false, ShouldFail = true };
			var ledgerType = LedgerTypes.AccountsReceivable;
			var accTransactionHeader = GetNewParentForClearedDateCheck_NotInDB(Factory, ledgerType, clearedDateTestCase, transactionNum, headerPK);

			Factory.ServiceContainer.RemoveService<CriticalValidationInfoCollectorService>();

			var infoCollectorService = CriticalValidationInfoCollectorService.GetService(Factory);
			AssertNull($"Precondition: {nameof(infoCollectorService)}", infoCollectorService);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("CheckClearedDateIsEmptyForReceiptsWithNoBatchNumber_CollectedInfoIsAlwaysReported, Ledger = {0}, SetBatchNo = {1}, SetClearedDate = {2}, SetIsCancelled = {3}", ledgerType, clearedDateTestCase.SetBatchNo, clearedDateTestCase.SetClearedDate, clearedDateTestCase.SetIsCancelled),
			true,
			CriticalValidationErrorType.ClearedReceiptWithoutBatchNumber_3,
			"Cleared receipt without a batch number.",
			@"
ClearedReceiptLinkedToNonClearedDepositBatch: There was no attempt to collect any data.");

			AssertOnSavingCheck(accTransactionHeader, testCase);
		}

		public void TestCheckClearedDateIsEmptyForReceiptsWithNoBatchNumber_InvalidTransactionInDBWithoutPropertyChanges()
		{
			foreach (var ledgerType in ledgerTypes)
			{
				var transactionNum = NewTransactionNum;
				var headerPK = Guid.NewGuid();
				var accTransactionHeader = GetNewParentForClearedDateCheck_InDB(Factory, ledgerType, transactionNum, string.Empty, headerPK);
				Assert(!accTransactionHeader.HasChanges);

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("CheckClearedDateIsEmptyForReceiptsWithNoBatchNumber, Ledger = {0}, invalid transaction in DB, no fields have changes", ledgerType));
				AssertOnSavingCheck(accTransactionHeader, testCase);
			}
		}

		public void TestCheckClearedDateIsEmptyForReceiptsWithNoBatchNumber_InvalidTransactionInDBWithPropertyChanges()
		{
			foreach (var ledgerType in ledgerTypes)
			{
				string[] fieldsToHaveChanges = { "AH_Ledger", "AH_TransactionType", "AH_IsCancelled", "AH_ReceiptBatchNo", "AH_DateClearedInCashbook" };

				foreach (string field in fieldsToHaveChanges)
				{
					var transactionNum = NewTransactionNum;
					var headerPK = Guid.NewGuid();
					var accTransactionHeader = GetNewParentForClearedDateCheck_InDB(Factory, ledgerType, transactionNum, field, headerPK);

					var info = accTransactionHeader.FindPropertyInfo(field);
					IZType newValue = null;
					switch (field)
					{
						case "AH_Ledger":
							newValue = new ZString(ledgerType);
							break;
						case "AH_TransactionType":
							newValue = new ZString("REC");
							break;
						case "AH_IsCancelled":
							newValue = ZBool.False;
							break;
						case "AH_ReceiptBatchNo":
							newValue = ZString.Empty;
							break;
						case "AH_DateClearedInCashbook":
							newValue = ZDateTime.Today;
							break;
					}
					info.Value = newValue;
					Assert(info.HasChanges);

					var propertyInfo = "Properties:";
					var changesPropertyInfo = "Fields with changes: ";

					var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("CheckClearedDateIsEmptyForReceiptsWithNoBatchNumber, Ledger = {0}, invalid transaction in DB, {1} has changes", ledgerType, field),
					true,
					CriticalValidationErrorType.ClearedReceiptWithoutBatchNumber_3,
					"Cleared receipt without a batch number.",
					@"
ClearedReceiptLinkedToNonClearedDepositBatch: ",
					propertyInfo,
					changesPropertyInfo);

					AssertOnSavingCheck(accTransactionHeader, testCase);
				}
			}
		}

		public void TestCheckInvoiceDate_IInvoiceDateValidation()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;

			var validationMock = new Mock<IInvoiceDateValidation>();
			var countryFactoryMock = new Mock<IAccountingCountryFactory>();
			countryFactoryMock.As<IInstanceProvider<IInvoiceDateValidation>>().Setup(x => x.Get()).Returns(validationMock.Object);
			var factoryMock = new Mock<IGlobalAccountingCountryFactory>();
			factoryMock.Setup(c => c.GetCountryFactory(It.IsAny<ZString>())).Returns(countryFactoryMock.Object);

			using (ObjectFactory.Substitute(factoryMock.Object))
			{
				validationMock.Setup(x => x.ValidateInvoiceDate(It.IsAny<AccTransactionHeader>())).Returns((ResourceString)null);
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("DO NOT report error: InvalidInvoiceDate"));

				validationMock.Reset();
				validationMock.Setup(x => x.ValidateInvoiceDate(It.IsAny<AccTransactionHeader>())).Returns(ResString.GetMultilingualString("Test", "Dummy Error"));
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Report error: InvalidInvoiceDate", true, CriticalValidationErrorType.InvalidInvoiceDate, "Dummy Error"));
			}
		}

		public void TestCreateCreditNoteWhenNotAllowedInRegistryConfiguration()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull("Company not null", company);
			AssertNotEquals("Company should not be same as current company", GlbCompany.CurrentCompany, company);

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.CreditNote;

			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Disallow posting AR credit note.",
					true,
					CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
					CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));

			header.AH_GC = company.PK;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting AR credit note."));

			header.Delete();

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			foreach (var combination in ledgerAndTransactionTypeCombinationsForAPCreditNotesNotAllowedToPost)
			{
				header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_Ledger = combination.Item1;
				header.AH_TransactionType = combination.Item2;

				AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
						"Disallow posting AP credit note.",
						true,
						CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
						CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));

				header.AH_GC = company.PK;
				AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting AP credit note."));
			}
		}

		public void TestCreateCreditNoteWhenNotAllowedInRegistryConfiguration_AlreadyInDatabase()
		{
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Disallow posting AR credit note.",
					true,
					CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
					CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Factory.Save();
			Assert(header.IsInDatabase);
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			header.AH_Desc = "Desc";
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting AR credit note."));

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			foreach (var combination in ledgerAndTransactionTypeCombinationsForAPCreditNotesNotAllowedToPost)
			{
				AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_Ledger = combination.Item1;
				header.AH_TransactionType = combination.Item2;
				AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
						"Disallow posting AP credit note.",
						true,
						CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
						CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));
				AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				Factory.Save();
				Assert(header.IsInDatabase);
				AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				header.AH_Desc = "Desc";
				AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting AP credit note."));
			}
		}

		public void TestCreateCreditNoteWhenNotAllowedInRegistryConfiguration_IsCancelled()
		{
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			header.AH_IsCancelled = false;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Disallow posting AR credit note.",
					true,
					CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
					CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));
			header.AH_IsCancelled = true;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting AR credit note."));

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			foreach (var combination in ledgerAndTransactionTypeCombinationsForAPCreditNotesNotAllowedToPost)
			{
				header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_Ledger = combination.Item1;
				header.AH_TransactionType = combination.Item2;
				header.AH_IsCancelled = false;
				AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
						"Disallow posting AP credit note.",
						true,
						CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
						CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));
				header.AH_IsCancelled = true;
				AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting AP credit note."));
			}
		}

		readonly Tuple<ZString, ZString>[] ledgerAndTransactionTypeCombinationsForAPCreditNotesNotAllowedToPost = new[]
		{
			new Tuple<ZString, ZString>(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote),
			new Tuple<ZString, ZString>(LedgerTypes.TransactionsPendingAllocation, TransactionTypes.CreditNotePendingAllocation),
			new Tuple<ZString, ZString>(LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UACreditNote),
			new Tuple<ZString, ZString>(LedgerTypes.IncompleteTransactions, TransactionTypes.IncompleteCreditNote),
		};

		public void TestCreateReversalCreditNoteWhenNotAllowedInRegistryConfiguration()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull("Company not null", company);
			AssertNotEquals("Company should not be same as current company", GlbCompany.CurrentCompany, company);

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			header.AH_IsCancelled = true;

			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Disallow posting reversal AR credit note.",
					true,
					CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
					CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));

			header.AH_GC = company.PK;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting reversal AR credit note."));

			header.Delete();

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			header.AH_IsCancelled = true;

			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Disallow posting reversal AP credit note.",
					true,
					CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
					CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));

			header.AH_GC = company.PK;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting reversal AP credit note."));
		}

		public void TestCreateReversalCreditNoteWhenNotAllowedInRegistryConfiguration_AlreadyInDatabase()
		{
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			header.AH_IsCancelled = true;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Disallow posting AR credit note.",
					true,
					CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
					CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			SuspendCriticalValidationAttribute.IsActive = true;
			Factory.Save();
			SuspendCriticalValidationAttribute.IsActive = false;
			Assert(header.IsInDatabase);
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			header.AH_Desc = "Desc";
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting reversal AR credit note."));

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			header.AH_IsCancelled = true;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Disallow posting AP credit note.",
					true,
					CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
					CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			SuspendCriticalValidationAttribute.IsActive = true;
			Factory.Save();
			SuspendCriticalValidationAttribute.IsActive = false;
			Assert(header.IsInDatabase);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			header.AH_Desc = "Desc";
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting reversal AP credit note."));
		}

		public void TestCreateReversalCreditNoteWhenNotAllowedInRegistryConfiguration_IsNotCancelled()
		{
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			header.AH_IsCancelled = true;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Disallow posting AR credit note.",
					true,
					CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
					CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));
			header.AH_IsCancelled = false;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting reversal AR credit note."));

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			header.AH_IsCancelled = true;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Disallow posting AP credit note.",
					true,
					CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration,
					CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage));
			header.AH_IsCancelled = false;
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods("Allow posting reversal AP credit note."));
		}

		public void TestTransactionExchangeRateIsNotGreaterThanZero()
		{
			ObjectFactory.Get<IAccounting>().Registry.SetInvoicePostingExchangeRateOptionAP(GlbCompany.CurrentCompany.PK.ToGuid(), "TOD");
			ObjectFactory.Get<IAccounting>().Registry.SetInvoicePostingExchangeRateOptionAR(GlbCompany.CurrentCompany.PK.ToGuid(), "EIT");
			AssertEquals("TOD", ObjectFactory.Get<IAccounting>().Registry.GetInvoicePostingExchangeRateOptionAP(GlbCompany.CurrentCompany.PK.ToGuid(), true));
			AssertEquals("TOD", ObjectFactory.Get<IAccounting>().Registry.GetInvoicePostingExchangeRateOptionAP(GlbCompany.CurrentCompany.PK.ToGuid(), false));
			AssertEquals("EIT", ObjectFactory.Get<IAccounting>().Registry.GetInvoicePostingExchangeRateOptionAR(GlbCompany.CurrentCompany.PK.ToGuid(), true));
			AssertEquals("EIT", ObjectFactory.Get<IAccounting>().Registry.GetInvoicePostingExchangeRateOptionAR(GlbCompany.CurrentCompany.PK.ToGuid(), false));

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_ExchangeRate = 0m;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_AH = header.PK;
			line.AL_GC = header.AH_GC;
			line.AL_JH = job.PK;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_GC = GlbCompany.CurrentCompany.PK;
			charge.JR_AL_ARLine = line.PK;
			line.AL_AC = charge.JR_AC;
			charge.SetChargeValuesFromLinkedARLineForTests();

			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Exchange Rate Is Not Greater Than Zero.",
					true,
					CriticalValidationErrorType.TransactionHeaderExchangeRateShouldBeGreaterThanZero_10,
					CriticalValidationMessageTemplate.TransactionHeaderExchangeRateIsNotGreaterThanZeroErrorMessage,
					$"Header: PK = {header.PK}",
					"Posted Lines: ",
					$"Line: PK = {line.PK}",
					"Related Job Charges: ",
					$"Charge: PK = {charge.PK}",
					"AP Invoice Posting Exchange Rate Option: TOD"));

			var helper = ObjectFactory.Get<IAccTransactionHeaderCriticalValidationHelper>();
			helper.SetIsReversalOfOriginalTransaction_ForTestOnly(header);
			AssertOnSavingCheck(header, new TestCaseDefinition_ForSeparateTestsMethods(
					"Exchange Rate Is Not Greater Than Zero.",
					false,
					CriticalValidationErrorType.TransactionHeaderExchangeRateShouldBeGreaterThanZero_10,
					CriticalValidationMessageTemplate.TransactionHeaderExchangeRateIsNotGreaterThanZeroErrorMessage));
		}

		public void TestTransactionExchangeRateIsNotGreaterThanZeroWithStackTrace()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_AH = header.PK;
			line.AL_GC = header.AH_GC;
			line.AL_JH = job.PK;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_GC = GlbCompany.CurrentCompany.PK;
			charge.JR_AL_ARLine = line.PK;
			line.AL_AC = charge.JR_AC;
			charge.SetChargeValuesFromLinkedARLineForTests();

			Factory.SetContext(BusinessContext.CreateTransactionsBeforePostingForFactoryLevel);

			charge.JR_OSSellExRate = 0M;
			header.AH_ExchangeRate = 0M;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Zero ExchangeRate",
				true,
				CriticalValidationErrorType.TransactionHeaderExchangeRateShouldBeGreaterThanZero_10,
				CriticalValidationMessageTemplate.TransactionHeaderExchangeRateIsNotGreaterThanZeroErrorMessage,
				"JobChargeOSSellExRateChangeWhenPostingReceivableCharges: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.",
				"EvaluateTransactionHeaderWithZeroExchangeRate: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(header, testCase);
		}

		public void TestTransactionHeaderAmountExceedMaximumAllowedAmount()
		{
			var maximumAllowedHeaderAmount = 101M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = maximumAllowedHeaderAmount;
			registryValue.MaximumAllowedLineAmount = 10000M;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_IsCancelled = false;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_AH = header.PK;
			line.AL_GC = header.AH_GC;

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				Assert("Precondition: AH_IsCancelled is false", !header.AH_IsCancelled);
				Assert("Precondition: AH_InvoiceAmount is less than registry setting.", Math.Abs(header.AH_InvoiceAmount) < maximumAllowedHeaderAmount);
				Assert("Precondition: AH_GSTAmount is less than registry setting.", Math.Abs(header.AH_GSTAmount) < maximumAllowedHeaderAmount);

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Header amount is not greater than registry setting.");
				AssertOnSavingCheck(header, testCase);

				header.AH_InvoiceAmount = 102M;
				Assert("Precondition: AH_InvoiceAmount is greater than registry setting.", Math.Abs(header.AH_InvoiceAmount) > maximumAllowedHeaderAmount);

				var expectedMessage = $"The transaction header amount exceed the maximum allowed amount {registryValue.MaximumAllowedHeaderAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals())} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("Header amount is greater than registry setting.", true, CriticalValidationErrorType.TransactionHeaderAmountExceedMaximumAllowedAmount, expectedMessage);
				AssertOnSavingCheck(header, testCase);

				header.AH_InvoiceAmount = 100M;
				header.AH_GSTAmount = 102M;
				Assert("Precondition: AH_IsCancelled is false", !header.AH_IsCancelled);
				Assert("Precondition: AH_InvoiceAmount is less than registry setting.", Math.Abs(header.AH_InvoiceAmount) < maximumAllowedHeaderAmount);
				Assert("Precondition: AH_GSTAmount is greater than registry setting.", Math.Abs(header.AH_GSTAmount) > maximumAllowedHeaderAmount);
				AssertOnSavingCheck(header, testCase);
			}
		}

		public void TestTransactionHeaderAmountExceedMaximumAllowedAmount_InDB()
		{
			var maximumAllowedHeaderAmount = 101M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = maximumAllowedHeaderAmount;
			registryValue.MaximumAllowedLineAmount = 10000M;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;
			var expectedMessage = $"The transaction header amount exceed the maximum allowed amount {registryValue.MaximumAllowedHeaderAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals())} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var newFactory1 = new BusinessObjectFactory();
				var header1 = newFactory1.NewWithValidTestData<AccTransactionHeader>();

				newFactory1.Save();

				Assert("Precondition: IsInDatabase is true", header1.IsInDatabase);
				Assert("Precondition: AH_IsCancelled is false", !header1.AH_IsCancelled);
				Assert("Precondition: AH_InvoiceAmountInfo HasChanges is false", !header1.AH_InvoiceAmountInfo.HasChanges);
				Assert("Precondition: AH_GSTAmountInfo HasChanges is false", !header1.AH_GSTAmountInfo.HasChanges);
				var testCase1 = new TestCaseDefinition_ForSeparateTestsMethods("Header is in DB, AH_InvoiceAmountInfo and AH_GSTAmountInfo have no changes");
				AssertOnSavingCheck(header1, testCase1);

				header1.AH_InvoiceAmount = 102M;
				Assert("Precondition: AH_InvoiceAmountInfo HasChanges is true", header1.AH_InvoiceAmountInfo.HasChanges);
				Assert("Precondition: AH_InvoiceAmount is greater than registry setting.", Math.Abs(header1.AH_InvoiceAmount) > maximumAllowedHeaderAmount);

				testCase1 = new TestCaseDefinition_ForSeparateTestsMethods("Header AH_InvoiceAmount is greater than registry setting.", true, CriticalValidationErrorType.TransactionHeaderAmountExceedMaximumAllowedAmount, expectedMessage);
				AssertOnSavingCheck(header1, testCase1);
			}
		}

		public void TestTransactionInvoiceAmountNotEditedAfterPosting()
		{
			var allLedgerTypes = new LedgerTypesList().GetAllCodes();
			var ledgerTransactionTypeCompatibilityMatrix = new AccTransactionHeaderCompatibilityMatrixTestHelper().LedgerTransactionTypesCompatibilityMatrix;
			foreach (var ledgerType in allLedgerTypes)
			{
				var transactionType = ledgerTransactionTypeCompatibilityMatrix[ledgerType].FirstOrDefault();
				if (ledgerTypesAllowedToChangeAmountAfterBeingSaved.Contains(ledgerType) || ledgerTypesNotAllowedToChangeAmountAfterBeingSaved.Contains(ledgerType))
				{
					var isAmountChangeAllowedByLedgerType = ledgerTypesAllowedToChangeAmountAfterBeingSaved.Contains(ledgerType);
					var transaction = GetTransactionSavedInDb(Factory, ledgerType, transactionType);
					Assert(transaction.IsInDatabase);
					transaction.AH_InvoiceAmount = 2m;
					transaction.AH_OutstandingAmount = 3m;
					var testcase = isAmountChangeAllowedByLedgerType ?
						new TestCaseDefinition_ForSeparateTestsMethods("Transaction is in DB and ledger type allow invoice amount to be changed") :
						new TestCaseDefinition_ForSeparateTestsMethods("Transaction is in DB and ledger type do not allow invoice amount to be changed", true,
						CriticalValidationErrorType.TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved_7,
						CriticalValidationMessageTemplate.TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved);
					AssertOnSavingCheck(transaction, testcase);

					if (isAmountChangeAllowedByLedgerType)
					{
						transaction.AH_Ledger = LedgerTypes.AccountsPayable;
						transaction.AH_TransactionType = TransactionTypes.Invoice;
						testcase = new TestCaseDefinition_ForSeparateTestsMethods("Saving Unallocated, Unapproved Or Incomplete Invoices as AP Invoice with invoice amount changed");
						AssertOnSavingCheck(transaction, testcase);

						Factory.Save();
					}
				}
				else
				{
					Fail($"You have added a new ledger type: {ledgerType}. Please add this to either ledgerTypesAllowedToChangeAmountAfterBeingSaved or ledgerTypesNotAllowedToChangeAmountAfterBeingSaved");
				}
			}
		}

		public void TestTransactionOSAmountNotEditedAfterPosting()
		{
			var allLedgerTypes = new LedgerTypesList().GetAllCodes();
			var ledgerTransactionTypeCompatibilityMatrix = new AccTransactionHeaderCompatibilityMatrixTestHelper().LedgerTransactionTypesCompatibilityMatrix;
			foreach (var ledgerType in allLedgerTypes)
			{
				var transactionType = ledgerTransactionTypeCompatibilityMatrix[ledgerType].FirstOrDefault();
				if (ledgerTypesAllowedToChangeAmountAfterBeingSaved.Contains(ledgerType) || ledgerTypesNotAllowedToChangeAmountAfterBeingSaved.Contains(ledgerType))
				{
					var isAmountChangeAllowedByLedgerType = ledgerTypesAllowedToChangeAmountAfterBeingSaved.Contains(ledgerType);
					var transaction = GetTransactionSavedInDb(Factory, ledgerType, transactionType);
					Assert(transaction.IsInDatabase);
					transaction.AH_OSTotal = 3m;
					var testcase = isAmountChangeAllowedByLedgerType ?
						new TestCaseDefinition_ForSeparateTestsMethods("Transaction is in DB and ledger type allow OS amount to be changed") :
						new TestCaseDefinition_ForSeparateTestsMethods("Transaction is in DB and ledger type do not allow OS amount to be changed", true,
						CriticalValidationErrorType.TransactionHeaderOSAmountWasModifiedAfterBeingSaved_2,
						CriticalValidationMessageTemplate.TransactionHeaderOSAmountWasModifiedAfterBeingSaved);
					AssertOnSavingCheck(transaction, testcase);

					if (isAmountChangeAllowedByLedgerType)
					{
						transaction.AH_Ledger = LedgerTypes.AccountsPayable;
						transaction.AH_TransactionType = TransactionTypes.Invoice;
						testcase = new TestCaseDefinition_ForSeparateTestsMethods("Saving Unallocated, Unapproved Or Incomplete Invoices as AP Invoice with OS amount changed");
						AssertOnSavingCheck(transaction, testcase);
					}
				}
				else
				{
					Fail($"You have added a new ledger type: {ledgerType}. Please add this to either ledgerTypesAllowedToChangeAmountAfterBeingSaved or ledgerTypesNotAllowedToChangeAmountAfterBeingSaved");
				}
			}
		}

		public void TestTransactionCreationRestrictionPolicy()
		{
			var helper = ObjectFactory.Get<ITransactionCreationRestrictionHelper>();
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			transaction.AH_OH = org.PK;

			List<string> ledgers = new List<string> { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable };
			List<string> transactionTypes = new List<string> { TransactionTypes.Invoice, TransactionTypes.AdjustmentNote, TransactionTypes.CreditNote };

			transaction.Header.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.Invoice;
			transaction.Header.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.Invoice;
			foreach (var ledger in ledgers)
			{
				foreach (var transactionType in transactionTypes)
				{
					transaction.AH_TransactionNum = "Test123";
					transaction.AH_Ledger = ledger;
					transaction.AH_TransactionType = transactionType;

					var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CreateTransactionRestriction", true, CriticalValidationErrorType.CreateTransactionRestriction,
					string.Format(@"You cannot create transaction '{0}' because organization '{1}' has an {2} transaction creation restriction policy set to {3}.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.",
						transaction.AH_TransactionNum, transaction.Header.OH_Code, transaction.AH_Ledger, Core.Constants.TransactionCreationRestriction.Invoice),
					"You cannot create transaction");
					AssertOnSavingCheck(transaction, testCase);
				}
			}

			transaction.Header.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.None;
			transaction.Header.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.None;
			foreach (var ledger in ledgers)
			{
				foreach (var transactionType in transactionTypes)
				{
					transaction.AH_TransactionNum = "Test123";
					transaction.AH_OH = org.PK;
					transaction.AH_Ledger = ledger;
					transaction.AH_TransactionType = transactionType;

					var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format(@"You cannot create transaction '{0}' because organization '{1}' has an {2} transaction creation restriction policy set to {3}.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.",
							transaction.AH_TransactionNum, transaction.Header.OH_Code, transaction.AH_Ledger, Core.Constants.TransactionCreationRestriction.Invoice));
					AssertOnSavingCheck(transaction, testCase);
				}
			}
		}

		public void TestTransactionHeaderBankAccountWasModifiedAfterBeingSaved()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderBankAccountWasModifiedAfterBeingSaved);

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionType = TransactionTypes.Receipt;
			var oldBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			transaction.AH_AB = oldBankAccount.PK;
			Factory.Save();

			var newBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			transaction.AH_AB = newBankAccount.PK;
			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Bank Account is changed.", true, CriticalValidationErrorType.TransactionHeaderBankAccountWasModifiedAfterBeingSaved_2,
				"Transaction header bank account was modified after being saved.",
				$"Old Bank Account: {oldBankAccount.PK}, New Bank Account: {newBankAccount.PK}, Transaction Header: {transaction.PK}"));
		}

		public void TestLedgerHasChangedByDataRefreshBusError()
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
			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Ledger is changed.", true, CriticalValidationErrorType.TransactionHeaderWasCriticallyChangedByDataRefreshBus_2,
				"Transaction was critically changed by this user during another operation. Please close this screen as this operation is not valid any more.",
				"Standard validation must catch this case and doesn't not allow to save.",
				$"Header: PK = {transaction.PK}",
				"Business Contexts = BizObj Level : (SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged)"));
		}

		public void TestTransactionTypeHasChangedByDataRefreshBusError()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			transaction.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var transactionInAnotherFactory = anotherFactory.Load<AccTransactionHeader>(transaction.PK);
			transactionInAnotherFactory.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;
			anotherFactory.Save();
			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Transaction type is changed.", true, CriticalValidationErrorType.TransactionHeaderWasCriticallyChangedByDataRefreshBus_2,
				"Transaction was critically changed by this user during another operation. Please close this screen as this operation is not valid any more.",
				"Standard validation must catch this case and doesn't not allow to save.",
				$"Header: PK = {transaction.PK}",
				"Business Contexts = BizObj Level : (SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged)"));
		}

		[TestDate(2009, 11, 10)]
		public void TestInvalidChangePostDateAfterTransactionPosted()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_PostDate = new ZDateTime(2009, 11, 10);
			Factory.Save();

			AssertEquals(new ZDateTime(2009, 11, 10), transaction.AH_PostDate);

			transaction.AH_PostDate = new ZDateTime(2009, 11, 20);

			var expectedMessagePart1 = string.Format(CultureInfo.InvariantCulture,
				"Header: PK = {0}, Ledger = AP, Transaction Type = INV, Invoice Date = {1}, Post Date = {2}",
				transaction.PK.ToString(), transaction.AH_InvoiceDate.ToAUString(), transaction.AH_PostDate.ToAUString());

			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods(
					"Post date changed after post.",
					true,
					CriticalValidationErrorType.TransactionHeaderPostDateChangesAfterPosted_3,
					CriticalValidationMessageTemplate.TransactionHeaderPostDateChangesAfterPostedMessage,
					expectedMessagePart1,
					"changes: AH_PostDate"));
		}

		[TestDate(2009, 11, 10)]
		public void TestValidChangePostDateAfterTransactionPosted()
		{
			var noteJournal = Factory.NewWithValidTestData<AccTransactionHeader>();
			noteJournal.AH_Ledger = LedgerTypes.General;
			noteJournal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			noteJournal.AH_PostDate = new ZDateTime(2009, 11, 10);
			Factory.Save();

			AssertEquals(new ZDateTime(2009, 11, 10), noteJournal.AH_PostDate);

			noteJournal.AH_PostDate = new ZDateTime(2009, 11, 20);

			AssertOnSavingCheck(noteJournal, new TestCaseDefinition_ForSeparateTestsMethods("Can change post date of Note Journal."));

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			transaction.AH_TransactionType = TransactionTypes.UAInvoice;
			transaction.AH_PostDate = new ZDateTime(2009, 11, 10);
			Factory.Save();

			AssertEquals(new ZDateTime(2009, 11, 10), transaction.AH_PostDate);

			transaction.AH_PostDate = new ZDateTime(2009, 11, 20);

			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Can change post date of UA transactions."));

			var transaction2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction2.AH_Ledger = LedgerTypes.General;
			transaction2.AH_TransactionType = TransactionTypes.GLAutoJournal;
			transaction2.AH_PostDate = new ZDateTime(2009, 11, 10);
			transaction2.AH_DueDate = new ZDateTime(2009, 11, 10);
			Factory.Save();

			AssertEquals(new ZDateTime(2009, 11, 10), transaction2.AH_PostDate);

			transaction2.AH_PostDate = new ZDateTime(2009, 11, 20);

			AssertOnSavingCheck(transaction2, new TestCaseDefinition_ForSeparateTestsMethods("Can change post date of Auto Journal transactions."));

			transaction2.AH_PostDate = ZDateTime.Empty;

			var expectedMessagePart1 = string.Format(CultureInfo.InvariantCulture,
				"Header: PK = {0}, Ledger = GL, Transaction Type = AJL, Invoice Date = {1}, Post Date = {2}",
				transaction2.PK.ToString(), transaction2.AH_InvoiceDate.ToAUString(), transaction2.AH_PostDate.ToAUString());

			AssertOnSavingCheck(transaction2, new TestCaseDefinition_ForSeparateTestsMethods(
					"However the post date of the auto Journal should be valid.",
					true,
					CriticalValidationErrorType.TransactionHeaderPostDateChangesAfterPosted_3,
					CriticalValidationMessageTemplate.TransactionHeaderPostDateChangesAfterPostedMessage,
					expectedMessagePart1,
					"changes: AH_PostDate"));
		}

		public void TestAPInvoiceShouldHaveLines()
		{
			var transaction = Factory.New<AccTransactionHeader>();
			transaction.AH_TransactionNum = "00001000";
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_PostDate = new ZDateTime(2009, 11, 10);

			var query = new ZQuery(AccTransactionLinesSchema.AL_AH, transaction.PK);
			query.AddToFilter(AccTransactionLinesSchema.AL_GC, transaction.AH_GC);
			var line = Factory.LoadTop1<AccTransactionLines>(query);
			AssertEquals(null, line);

			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var expectedMessagePart = string.Format(CultureInfo.InvariantCulture,
					"Header: PK = {0}, Ledger = AP, Transaction Type = INV, Invoice Date = {1}, Post Date = {2}",
					transaction.PK.ToString(), transaction.AH_InvoiceDate.ToAUString(), transaction.AH_PostDate.ToAUString());

				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods(
						"TransactionWithLines do not have lines.",
						true,
						CriticalValidationErrorType.TransactionHeaderWithLinesShouldHaveLines_4,
						CriticalValidationMessageTemplate.TransactionHeaderWithLinesShouldHaveLinesMessage,
						expectedMessagePart));
			}

			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Should NOT apply validation rule outside the scope."));
		}

		public void TestAPInvoiceShouldHaveLines_BranchChanged()
		{
			var transactionWithLines = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);

			var query = new ZQuery(AccTransactionLinesSchema.AL_AH, transactionWithLines.PK);
			query.AddToFilter(AccTransactionLinesSchema.AL_GC, transactionWithLines.AH_GC);
			var line = Factory.LoadTop1<AccTransactionLines>(query);
			AssertNotEquals(null, line);
			Factory.Save();

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newCompany.PK;
			Factory.Save();

			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				transactionWithLines.AH_TransactionType = TransactionTypes.CreditNote;
				transactionWithLines.AH_GB = newBranch.PK;

				AssertOnSavingCheck(transactionWithLines, new TestCaseDefinition_ForSeparateTestsMethods(
						"TransactionWithLines do not have lines.",
						true,
						CriticalValidationErrorType.TransactionHeaderWithLinesShouldHaveLines_4,
						CriticalValidationMessageTemplate.TransactionHeaderWithLinesShouldHaveLinesMessage));

				transactionWithLines.AH_GB = GlbBranch.CurrentBranch.PK;
				transactionWithLines.AH_GB = newBranch.PK;

				AssertOnSavingCheck(transactionWithLines, new TestCaseDefinition_ForSeparateTestsMethods(
						"TransactionWithLines do not have lines.",
						true,
						CriticalValidationErrorType.TransactionHeaderWithLinesShouldHaveLines_4,
						CriticalValidationMessageTemplate.TransactionHeaderWithLinesShouldHaveLinesMessage,
						$"AH_GB: Line Branch: {GlbBranch.CurrentBranch.GB_Code}, Header Old Branch: {GlbBranch.CurrentBranch.GB_Code}, Header New Branch:{newBranch.GB_Code} , StackTrace ->"));
			}
		}

		public void TestChequeDirectPaymentCanHaveNoLines()
		{
			var transaction = Factory.New<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.CashBook;
			transaction.AH_TransactionType = TransactionTypes.DirectPayment;
			transaction.AH_PostDate = new ZDateTime(2009, 11, 10);
			transaction.AH_TransactionNum = "00001000";

			var query = new ZQuery(AccTransactionLinesSchema.AL_AH, transaction.PK);
			query.AddToFilter(AccTransactionLinesSchema.AL_GC, transaction.AH_GC);
			var line = Factory.LoadTop1<AccTransactionLines>(query);
			AssertEquals(null, line);

			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var expectedMessagePart = string.Format(CultureInfo.InvariantCulture,
					"Header: PK = {0}, Ledger = CB, Transaction Type = DPY, Invoice Date = {1}, Post Date = {2}",
					transaction.PK.ToString(), transaction.AH_InvoiceDate.ToAUString(), transaction.AH_PostDate.ToAUString());

				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods(
						"TransactionWithLines do not have lines.",
						true,
						CriticalValidationErrorType.TransactionHeaderWithLinesShouldHaveLines_4,
						CriticalValidationMessageTemplate.TransactionHeaderWithLinesShouldHaveLinesMessage,
						expectedMessagePart));
			}

			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Should NOT apply validation rule outside the scope."));

			transaction.AH_ReceiptType = ReceiptTypes.Cheque;

			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Should NOT apply validation rule because receipt type is cheque"));
			}
		}

		[TestDate(2009, 11, 10)]
		public void TestTransactionHeaderAlreadyInDB()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_PostDate = new ZDateTime(2009, 11, 10);
			transaction.AH_Desc = "XXX";

			Factory.Save();

			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var newFactory = new BusinessObjectFactory();
				var transactionLoaded = newFactory.Load<AccTransactionHeader>(transaction.PK);
				transactionLoaded.AH_Desc = "YYY";

				var query = new ZQuery(AccTransactionLinesSchema.AL_AH, transactionLoaded.PK);
				query.AddToFilter(AccTransactionLinesSchema.AL_GC, transactionLoaded.AH_GC);
				var line = newFactory.LoadTop1<AccTransactionLines>(query);
				AssertEquals("Load a persisted transaction without lines.", null, line);

				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Save persisted TransactionWithoutLines should NOT trigger critical valdiation."));
			}

			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var newFactory = new BusinessObjectFactory();
				var transactionLoaded = newFactory.Load<AccTransactionHeader>(transaction.PK);
				transactionLoaded.AH_Ledger = LedgerTypes.AccountsReceivable;

				var query = new ZQuery(AccTransactionLinesSchema.AL_AH, transactionLoaded.PK);
				query.AddToFilter(AccTransactionLinesSchema.AL_GC, transactionLoaded.AH_GC);
				var line = newFactory.LoadTop1<AccTransactionLines>(query);
				AssertEquals("Load a persisted transaction without lines.", null, line);
				AssertEquals("Ledger changed.", true, transactionLoaded.AH_LedgerInfo.HasChanges);

				var expectedMessagePart = string.Format(CultureInfo.InvariantCulture,
					"Header: PK = {0}, Ledger = AR, Transaction Type = INV, Invoice Date = {1}, Post Date = {2}",
					transactionLoaded.PK.ToString(), transactionLoaded.AH_InvoiceDate.ToAUString(), transactionLoaded.AH_PostDate.ToAUString());

				AssertOnSavingCheck(transactionLoaded, new TestCaseDefinition_ForSeparateTestsMethods(
						"Persisted TransactionWithoutLines with Ledger change should trigger critical valdiation.",
						true,
						CriticalValidationErrorType.TransactionHeaderWithLinesShouldHaveLines_4,
						CriticalValidationMessageTemplate.TransactionHeaderWithLinesShouldHaveLinesMessage,
						expectedMessagePart));
			}

			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var newFactory = new BusinessObjectFactory();
				var transactionLoaded = newFactory.Load<AccTransactionHeader>(transaction.PK);
				transactionLoaded.AH_TransactionType = TransactionTypes.CreditNote;

				var query = new ZQuery(AccTransactionLinesSchema.AL_AH, transactionLoaded.PK);
				query.AddToFilter(AccTransactionLinesSchema.AL_GC, transactionLoaded.AH_GC);
				var line = newFactory.LoadTop1<AccTransactionLines>(query);
				AssertEquals("Load a persisted transaction without lines.", null, line);
				AssertEquals("Transaction-Type changed.", true, transactionLoaded.AH_TransactionTypeInfo.HasChanges);

				var expectedMessagePart = string.Format(CultureInfo.InvariantCulture,
					"Header: PK = {0}, Ledger = AP, Transaction Type = CRD, Invoice Date = {1}, Post Date = {2}",
					transactionLoaded.PK.ToString(), transactionLoaded.AH_InvoiceDate.ToAUString(), transactionLoaded.AH_PostDate.ToAUString());

				AssertOnSavingCheck(transactionLoaded, new TestCaseDefinition_ForSeparateTestsMethods(
						"Persisted TransactionWithoutLines with Transaction-Type change should trigger critical valdiation.",
						true,
						CriticalValidationErrorType.TransactionHeaderWithLinesShouldHaveLines_4,
						CriticalValidationMessageTemplate.TransactionHeaderWithLinesShouldHaveLinesMessage,
						expectedMessagePart));
			}
		}

		[TestDate(2009, 10, 20)]
		public void TestTransactionHeaderHaveLinesInDB()
		{
			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var sourceFactory = new BusinessObjectFactory();
				var sourceTransaction = TransactionCreator.CreateTransaction(sourceFactory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
				sourceFactory.Save();

				var query = new ZDBOnlyQuery(typeof(AccTransactionLines));
				query.AddToFilter(AccTransactionLinesSchema.AL_GC, sourceTransaction.AH_GC);
				query.AddToFilter(AccTransactionLinesSchema.AL_AH, sourceTransaction.PK);
				var line = sourceFactory.LoadTop1<AccTransactionLines>(query);
				AssertNotEquals(null, line);

				var newFactory = new BusinessObjectFactory();
				var transactionHeader = newFactory.Load<AccTransactionHeader>(sourceTransaction.PK);

				AssertOnSavingCheck(transactionHeader, new TestCaseDefinition_ForSeparateTestsMethods("Should have no critical validation errors."));
			}
		}

		[TestDate(2009, 10, 20)]
		public void TestTransactionHeaderHaveLinesLoadedAsWithoutLines()
		{
			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var newFactory = new BusinessObjectFactory();
				var transactionWithLines = TransactionCreator.CreateTransaction(newFactory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);

				var query = new ZQuery(AccTransactionLinesSchema.AL_AH, transactionWithLines.PK) { FetchOnlyFromLocalCache = true };
				query.AddToFilter(AccTransactionLinesSchema.AL_GC, transactionWithLines.AH_GC);
				var line = newFactory.LoadTop1<AccTransactionLines>(query);
				AssertNotEquals(null, line);

				var transactionWithoutLines = newFactory.Load<AccTransactionHeader>(transactionWithLines.PK);

				AssertOnSavingCheck(transactionWithoutLines, new TestCaseDefinition_ForSeparateTestsMethods("Should have no critical validation errors."));
			}
		}

		public void TestTransactionHeaderShouldNotHaveLines()
		{
			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_Ledger = LedgerTypes.IncompleteTransactions;
				transaction.AH_TransactionType = TransactionTypes.IncompleteInvoice;
				transaction.AH_PostDate = new ZDateTime(2009, 11, 10);
				Factory.Save();

				var query = new ZQuery(AccTransactionLinesSchema.AL_AH, transaction.PK);
				query.AddToFilter(AccTransactionLinesSchema.AL_GC, transaction.AH_GC);
				var line = Factory.LoadTop1<AccTransactionLines>(query);
				AssertEquals(null, line);

				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Transaction do not need to have lines."));
			}
		}

		[TestDate(2009, 10, 20)]
		public void TestTransactionHeaderMustHaveTransactionNumberWithEmptyInfo()
		{
			var transaction = Factory.New<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_PostDate = new ZDateTime(2009, 11, 10);
			transaction.AH_Desc = "Test";

			AssertEquals(ZString.Empty, transaction.AH_TransactionNum);

			var expectedMessagePart = new string[] {
				CriticalValidationMessageTemplate.TransactionHeaderMustHaveTransactionNumber,
				"Header: PK = ",
				"InvoicingBaseDidNotCreateTransactionNumberOnSaving: There is no data collected for this PK.",
				"TransactionHeaderTransactionNumberSetToEmpty: There is no data collected for this PK." };

			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods(
				"This transaction should have transaction number.",
				true,
				CriticalValidationErrorType.TransactionHeaderMustHaveTransactionNumber_8,
				CriticalValidationMessageTemplate.TransactionHeaderMustHaveTransactionNumber,
				expectedMessagePart));
		}

		[TestDate(2009, 10, 20)]
		public void TestTransactionHeaderMustHaveTransactionNumber()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderTransactionNumberSetToEmpty);

			var transaction = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);

			AssertNotEquals(ZString.Empty, transaction.AH_TransactionNum);

			transaction.AH_TransactionNum = null;
			AssertEquals(ZString.Empty, transaction.AH_TransactionNum);

			var expectedMessagePart = new string[]
			{
				CriticalValidationMessageTemplate.TransactionHeaderMustHaveTransactionNumber,
				"Header: PK = ",
				"InvoicingBaseDidNotCreateTransactionNumberOnSaving: There is no data collected for this key.",
				@"TransactionHeaderTransactionNumberSetToEmpty:
AH_TransactionNum of a TransactionHeader is set as empty:
   at "
			};

			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods(
				"This transaction should have transaction number.",
				true,
				CriticalValidationErrorType.TransactionHeaderMustHaveTransactionNumber_8,
				CriticalValidationMessageTemplate.TransactionHeaderMustHaveTransactionNumber,
				expectedMessagePart));

			using (AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail when registry disabled."));
			}
		}

		[TestDate(2009, 10, 20)]
		public void TestExistingWrongTransactionWithEmptyTransactionNumber()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderTransactionNumberSetToEmpty);

			var transaction = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			transaction.AH_TransactionNum = null;

			//Intend to create an invalid transaction header with null number.
			using (AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Factory.Save();
			}

			transaction.AH_Desc = "newDesc";

			AssertEquals(ZString.Empty, transaction.AH_TransactionNum);
			Assert(!transaction.AH_TransactionNumInfo.HasChanges);

			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("No exception for existing bad data with empty transaction number."));
		}

		[TestDate(2009, 10, 20)]
		public void TestTransactionHeaderTransactionNumberChangedAfterSaved()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderTransactionNumberChanged);

			var receivableTransaction = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			var payabletransaction = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);

			Factory.Save();

			AssertNotEquals(ZString.Empty, receivableTransaction.AH_TransactionNum);
			AssertNotEquals(ZString.Empty, payabletransaction.AH_TransactionNum);

			receivableTransaction.AH_TransactionNum += "1";
			Assert(receivableTransaction.AH_TransactionNumInfo.HasChanges);
			payabletransaction.AH_TransactionNum += "1";
			Assert(payabletransaction.AH_TransactionNumInfo.HasChanges);

			var expectedMessagePart = new string[] {
					CriticalValidationMessageTemplate.TransactionNumberOfHeaderMustNotChangeOnceSaved,
					"Header: PK = ",
					@"TransactionHeaderTransactionNumberChanged:
AH_TransactionNum changed after being saved:
   at " };

			AssertOnSavingCheck(receivableTransaction, new TestCaseDefinition_ForSeparateTestsMethods(
				"This transaction should have transaction number.",
				true,
				CriticalValidationErrorType.TransactionNumberOfHeaderMustNotChangeOnceSaved_5,
				CriticalValidationMessageTemplate.TransactionNumberOfHeaderMustNotChangeOnceSaved,
				expectedMessagePart));

			AssertOnSavingCheck(payabletransaction, new TestCaseDefinition_ForSeparateTestsMethods(
				"This transaction should have transaction number.",
				false,
				CriticalValidationErrorType.TransactionNumberOfHeaderMustNotChangeOnceSaved_5,
				CriticalValidationMessageTemplate.TransactionNumberOfHeaderMustNotChangeOnceSaved,
				expectedMessagePart));

			using (AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertOnSavingCheck(receivableTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail when registry disabled."));
				AssertOnSavingCheck(payabletransaction, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail when registry disabled."));
			}
		}

		[TestDate(2009, 10, 20)]
		public void TestSkipTransactionNumberChangedAfterSavedValidation()
		{
			var incompleteTransaction = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			incompleteTransaction.AH_Ledger = LedgerTypes.IncompleteTransactions;
			incompleteTransaction.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			Factory.Save();
			AssertNotEquals(ZString.Empty, incompleteTransaction.AH_TransactionNum);

			incompleteTransaction.AH_TransactionNum += "1";
			Assert(incompleteTransaction.AH_TransactionNumInfo.HasChanges);

			AssertOnSavingCheck(incompleteTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail for incomplete transaction."));

			incompleteTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			incompleteTransaction.AH_TransactionType = TransactionTypes.Invoice;
			Assert(incompleteTransaction.AH_TransactionNumInfo.HasChanges);

			AssertOnSavingCheck(incompleteTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail when incomplete transaction convert to AP transaction."));

			var pendingTransaction = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			pendingTransaction.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			pendingTransaction.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			Factory.Save();
			AssertNotEquals(ZString.Empty, pendingTransaction.AH_TransactionNum);

			pendingTransaction.AH_TransactionNum += "1";
			Assert(pendingTransaction.AH_TransactionNumInfo.HasChanges);

			AssertOnSavingCheck(pendingTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail for pending allocate transaction."));
		}

		public void TestPostingAPTransactionConvertedFromIncompleteTransactionWithPostedLines()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();
			var transaction = creator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			transaction.AH_Ledger = LedgerTypes.IncompleteTransactions;
			transaction.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			Factory.Save();

			var insertLineQuery = $@"INSERT dbo.AccTransactionLines (AL_PK, AL_AH, AL_GE, AL_GB, AL_GC, AL_LineType, AL_LineAmount) values ('{Guid.NewGuid()}', '{transaction.PK}', '{transaction.AH_GE}', '{transaction.AH_GB}', '{transaction.AH_GC}', 'CST', 50)";
			using (var command = Db.Connection.Command(insertLineQuery))
			{
				command.ExecuteNonQuery();
			}

			var newFactory = new BusinessObjectFactory();
			var transactionInNewFactory = newFactory.Load<AccTransactionHeader>(transaction.PK);
			transactionInNewFactory.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionInNewFactory.AH_TransactionType = TransactionTypes.Invoice;

			var expectedHeaderMessage = FormattableString.Invariant($"Header: PK = {transactionInNewFactory.PK}, Ledger = AP, Transaction Type = INV, Invoice Date = {transactionInNewFactory.AH_InvoiceDate.ToAUString()}, Post Date = {transactionInNewFactory.AH_PostDate.ToAUString()}");

			AssertOnSavingCheck(transactionInNewFactory, new TestCaseDefinition_ForSeparateTestsMethods("Transaction should not be posted with post lines", true,
				CriticalValidationErrorType.INTransactionHeaderHasPostedLines_4,
				CriticalValidationMessageTemplate.INTransactionHeaderHasPostedLinesMessage,
				expectedHeaderMessage, "Posted Lines: ", "Line: PK = "));
		}

		[TestDate(2009, 10, 20)]
		public void TestSavingIncompleteTransactionWithPostedLines()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();
			var transactionWithoutLines = creator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			transactionWithoutLines.AH_Ledger = LedgerTypes.IncompleteTransactions;
			transactionWithoutLines.AH_TransactionType = TransactionTypes.IncompleteInvoice;

			AssertOnSavingCheck(transactionWithoutLines, new TestCaseDefinition_ForSeparateTestsMethods("Incomplete Trasaction without posted lines"));

			var transactionWithLines = creator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			Factory.Save();

			transactionWithLines.AH_Ledger = LedgerTypes.IncompleteTransactions;
			transactionWithLines.AH_TransactionType = TransactionTypes.IncompleteInvoice;

			var expectedHeaderMessage = FormattableString.Invariant($"Header: PK = {transactionWithLines.PK}, Ledger = IN, Transaction Type = INI, Invoice Date = {transactionWithLines.AH_InvoiceDate.ToAUString()}, Post Date = {transactionWithLines.AH_PostDate.ToAUString()}");

			AssertOnSavingCheck(transactionWithLines, new TestCaseDefinition_ForSeparateTestsMethods("Incomplete Transaction with posted lines", true,
				CriticalValidationErrorType.INTransactionHeaderHasPostedLines_4,
				CriticalValidationMessageTemplate.INTransactionHeaderHasPostedLinesMessage,
				expectedHeaderMessage, "Posted Lines: ", "Line: PK = "));
		}

		public void TestAH_OutstandingAmountLastSetHasStackTrace()
		{
			var transaction = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			Factory.Save();

			transaction.AH_OutstandingAmount = 999M;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect outstanding amount.",
				true,
				CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
				CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage,
				"AH_OutstandingAmountLastSet: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(transaction, testCase);

			transaction.AH_OutstandingAmount = 1000M;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect outstanding amount.",
				true,
				CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
				CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage,
				"AH_OutstandingAmountLastSet:",
				"AH_OutstandingAmount = 1000, AH_OutstandingAmount Old Value = 999",
				"at"
			);

			AssertOnSavingCheck(transaction, testCase);
		}

		public void TestAmountSignErrorLog()
		{
			AssertPayment();
			AssertReceipt();

			void AssertPayment()
			{
				var payment = TransactionCreator.CreateTransaction(new BusinessObjectFactory(), LedgerTypes.AccountsPayable, TransactionTypes.Payment);
				payment.AH_TransactionNum = "PAY00001";
				payment.AH_InvoiceAmount = 100m;
				payment.AH_OutstandingAmount = -100m;

				AssertOnSavingCheck(payment, new TestCaseDefinition_ForSeparateTestsMethods(
					"Incorrect outstanding amount.",
					true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage,
					@"AH_OutstandingAmount: -100
AH_LocalTotal: 100
Stack trace for sign error:",
					nameof(AssertPayment),
					nameof(TestAmountSignErrorLog)
				));
			}

			void AssertReceipt()
			{
				var receipt = TransactionCreator.CreateTransaction(new BusinessObjectFactory(), LedgerTypes.AccountsReceivable, TransactionTypes.Receipt);
				receipt.AH_TransactionNum = "REC00001";
				receipt.AH_InvoiceAmount = 100m;
				receipt.AH_OutstandingAmount = -100m;

				AssertOnSavingCheck(receipt, new TestCaseDefinition_ForSeparateTestsMethods(
					"Incorrect outstanding amount.",
					true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage,
					@"AH_OutstandingAmount: -100
AH_LocalTotal: 100
Stack trace for sign error:",
					nameof(AssertReceipt),
					nameof(TestAmountSignErrorLog)
				));
			}
		}

		public void TestInvoiceBatchErrorLineLog()
		{
			var invoiceLine1 = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			invoiceLine1.AH_InvoiceAmount = 1000M;
			invoiceLine1.AH_GSTAmount = 0M;
			invoiceLine1.AH_OutstandingAmount = invoiceLine1.AH_InvoiceAmount;

			var invoiceLine2 = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			invoiceLine2.AH_InvoiceAmount = 1000M;
			invoiceLine2.AH_GSTAmount = 0M;
			invoiceLine2.AH_OutstandingAmount = invoiceLine1.AH_InvoiceAmount;

			Factory.Save();

			var batch = TransactionCreator.CreateInvoiceBatch(Factory, invoiceLine1, invoiceLine2);
			batch.AH_OutstandingAmount = batch.AH_InvoiceAmount - 1m;

			batch.OnSaving();
			AssertOnSavingCheck(batch, new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect outstanding amount.",
				true,
				CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
				CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage,
				@"batch original AH_ExchangeRate: 1
batch lines info:
IncludeInTheBatch:Y, Cur:AUD, ExtRate:1, OsAmount:100, LocalAmount:100, Outstanding:100, TaxAmount:0
IncludeInTheBatch:Y, Cur:AUD, ExtRate:1, OsAmount:-100, LocalAmount:-100, Outstanding:-100, TaxAmount:0"
			));
		}

		public void TestTransactionHeaderIncorrectOutstandingAmount()
		{
			var compatibilityLedgerTransactionTypeMatrice = new AccTransactionHeaderCompatibilityMatrixTestHelper().LedgerTransactionTypesCompatibilityMatrix;
			foreach (string ledgerType in ledgerTypes)
			{
				string ledgerType1 = ledgerType;
				string transactionType = compatibilityLedgerTransactionTypeMatrice[ledgerType1].FirstOrDefault();
				var transaction = GetNewParentForOutstandingAmountCheck(Factory, ledgerType1, transactionType);
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Outstanding Amount is incorrect, Ledger = {0} TransactionType = {1}", ledgerType, transactionType),
					true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					"Incorrect outstanding amount.",
					"Outstanding Amount is incorrect: outstanding amount = 5, local total amount = 20, sum ap amount = 10");
				AssertOnSavingCheck(transaction, testCase);

				testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Outstanding Amount is correct, Ledger = {0} TransactionType = {1}", ledgerType, transactionType));
				transaction.AH_InvoiceAmount = 15;
				AssertOnSavingCheck(transaction, testCase);
			}

			foreach (string ledgerType in otherLedgerTypes)
			{
				string ledgerType1 = ledgerType;
				string transactionType = compatibilityLedgerTransactionTypeMatrice[ledgerType1].FirstOrDefault();
				var transaction = GetNewParentForOutstandingAmountCheck(Factory, ledgerType1, transactionType);
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Not applicable ledger for OutstandingAmount check, Ledger = {0} TransactionType = {1}", ledgerType, transactionType));
				AssertOnSavingCheck(transaction, testCase);
			}

			foreach (string ledgerType in ledgerTypes)
			{
				string ledgerType1 = ledgerType;
				string transactionType = compatibilityLedgerTransactionTypeMatrice[ledgerType1].FirstOrDefault();
				var transaction = GetNewParentForOutstandingAmountSignCheck(Factory, ledgerType1, transactionType);
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Outstanding Amount has incorrect sign, Ledger = {0} TransactionType = {1}", ledgerType, transactionType),
					true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					"Incorrect outstanding amount.",
					"Outstanding Amount is incorrect: outstanding amount = 2, local total amount = -100");
				AssertOnSavingCheck(transaction, testCase);

				testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Outstanding Amount has correct sign, Ledger = {0} TransactionType = {1}", ledgerType, transactionType));
				transaction.AH_OutstandingAmount = -2;
				AssertOnSavingCheck(transaction, testCase);
			}
		}

		public void TestTransactionHeaderIncorrectOutstandingAmount_WithOtherTaxes()
		{
			var compatibilityLedgerTransactionTypeMatrice = new AccTransactionHeaderCompatibilityMatrixTestHelper().LedgerTransactionTypesCompatibilityMatrix;
			foreach (string ledgerType in ledgerTypes)
			{
				string ledgerType1 = ledgerType;
				string transactionType = compatibilityLedgerTransactionTypeMatrice[ledgerType1].FirstOrDefault();
				var transaction = GetNewParentForOutstandingAmountCheck(Factory, ledgerType1, transactionType);
				transaction.AH_InvoiceAmount = 15;
				transaction.AH_LocalTaxAmountOtherTaxes = 5;
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Outstanding Amount is incorrect, Ledger = {0} TransactionType = {1}", ledgerType, transactionType),
					true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					"Incorrect outstanding amount.",
					"Outstanding Amount is incorrect: outstanding amount = 5, local total amount = 20, sum ap amount = 10");
				AssertOnSavingCheck(transaction, testCase);

				testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Outstanding Amount is correct, Ledger = {0} TransactionType = {1}", ledgerType, transactionType));
				transaction.AH_LocalTaxAmountOtherTaxes = 0;
				AssertOnSavingCheck(transaction, testCase);
			}

			foreach (string ledgerType in ledgerTypes)
			{
				string ledgerType1 = ledgerType;
				string transactionType = compatibilityLedgerTransactionTypeMatrice[ledgerType1].FirstOrDefault();
				var transaction = GetNewParentForOutstandingAmountSignCheck(Factory, ledgerType1, transactionType);
				transaction.AH_OutstandingAmount = -2;
				transaction.AH_InvoiceAmount = -15;
				transaction.AH_LocalTaxAmountOtherTaxes = 115;
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Outstanding Amount has incorrect sign, Ledger = {0} TransactionType = {1}", ledgerType, transactionType),
					true,
					CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
					"Incorrect outstanding amount.",
					"Outstanding Amount is incorrect: outstanding amount = -2, local total amount = 100");
				AssertOnSavingCheck(transaction, testCase);

				testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Outstanding Amount has correct sign, Ledger = {0} TransactionType = {1}", ledgerType, transactionType));
				transaction.AH_LocalTaxAmountOtherTaxes = -85;
				AssertOnSavingCheck(transaction, testCase);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();
			var compatibilityLedgerTransactionTypeMatrice = new AccTransactionHeaderCompatibilityMatrixTestHelper().LedgerTransactionTypesCompatibilityMatrix;

			foreach (string ledgerType in (new LedgerTypesList()).GetAllCodes())
			{
				if (ledgerTypesAllowedToChangeAmountAfterBeingSaved.Contains(ledgerType))
				{
					string transactionType = compatibilityLedgerTransactionTypeMatrice[ledgerType].FirstOrDefault();
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("TransactionHeaderGstAmountWasModifiedAfterBeingSaved, Ledger = {0} TransactionType = {1}", ledgerType, transactionType),
						f =>
						{
							var transactionHeaderforGst = GetTransactionSavedInDb(Factory, ledgerType, transactionType);
							transactionHeaderforGst.AH_Ledger = LedgerTypes.AccountsPayable;
							transactionHeaderforGst.AH_TransactionType = TransactionTypes.Invoice;
							transactionHeaderforGst.AH_OutstandingAmount = 3M;
							transactionHeaderforGst.AH_GSTAmount = 2M;
							return transactionHeaderforGst;
						}));
				}
				else if (ledgerTypesNotAllowedToChangeAmountAfterBeingSaved.Contains(ledgerType))
				{
					string transactionType = compatibilityLedgerTransactionTypeMatrice[ledgerType].FirstOrDefault();
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("TransactionHeaderGstAmountWasModifiedAfterBeingSaved, Ledger = {0} TransactionType = {1}", ledgerType, transactionType),
						f =>
						{
							var transactionHeaderforGst = GetTransactionSavedInDb(Factory, ledgerType, transactionType);
							transactionHeaderforGst.AH_OutstandingAmount = 3M;
							transactionHeaderforGst.AH_GSTAmount = 2M;
							return transactionHeaderforGst;
						},
						true, CriticalValidationErrorType.TransactionHeaderGstAmountWasModifiedAfterBeingSaved_2, "The tax amount was modified after being saved."));
				}
				else
				{
					Fail(string.Format("You have added a new ledger type: {0}. Please add this to either ledgerTypesAllowedToChangeAmountAfterBeingSaved or ledgerTypesNotAllowedToChangeAmountAfterBeingSaved", ledgerType));
				}
			}

			foreach (string ledgerType in ledgerTypes)
			{
				foreach (string transactionType in miscTransactionTypes)
				{
					string ledgerType1 = ledgerType;
					string transactionType1 = transactionType;

					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckOutstandingAmountIsZeroOnMatchingMiscTransactions Ledger = {0}, Transaction Type = {1} should pass because Outstanding Amount is zero", ledgerType, transactionType),
						(BusinessObjectFactory factory1) =>
						{
							AccTransactionHeader header = GetNewParentForOutstandingAmountCheck(factory1, ledgerType1, transactionType1);
							header.AH_OutstandingAmount = 0M;
							header.AH_InvoiceAmount = 0M;
							link.AP_Amount = 0m;
							return header;
						}));

					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckOutstandingAmountIsZeroOnMatchingMiscTransactions Ledger = {0}, Transaction Type = {1} should fail because Outstanding Amount is not zero ", ledgerType, transactionType),
						(BusinessObjectFactory factory1) =>
						{
							AccTransactionHeader header = GetNewParentForOutstandingAmountCheck(factory1, ledgerType1, transactionType1);
							link.AP_Amount = 15m;
							return header;
						},
						true, CriticalValidationErrorType.NonZeroOutstandingAmountOnMiscellaneousTransaction_4, "Non zero outstanding amount on miscellaneous transaction.",
						GetOutstandingAmountIsNotZeroOnMatchingMiscTransactionsErrorMessage(ledgerType1, transactionType1),
						"Match Link: Group Number = ", "Outstanding Amount Stack Trace:"));

					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckOutstandingAmountIsZeroOnMatchingMiscTransactions Ledger = {0}, Transaction Type = {1} should fail because Outstanding Amount is not zero ", ledgerType, transactionType),
						(BusinessObjectFactory factory1) =>
						{
							AccTransactionHeader header = GetNewParentForOutstandingAmountCheck(factory1, ledgerType1, transactionType1);
							link.AP_Amount = 15m;

							CriticalValidationInfoCollectorService.GetOrCreateService(factory1).AddInfoWhenAllowed(header.PK,
								CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction, () =>
								{
									var stringBuilder = new ZStringBuilder();
									stringBuilder.AppendLine("There are matching errors:");
									stringBuilder.AppendLine(@"Cannot create the Clearing Journal - The department BRN cannot be used with the branch BNE.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.");

									return stringBuilder.ToString();
								});
							return header;
						},
						true, CriticalValidationErrorType.NonZeroOutstandingAmountOnMiscellaneousTransaction_4, "Non zero outstanding amount on miscellaneous transaction.",
						GetOutstandingAmountIsNotZeroOnMatchingMiscTransactionsErrorMessage(ledgerType1, transactionType1)
						, @"There are matching errors:
Cannot create the Clearing Journal - The department BRN cannot be used with the branch BNE.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.",
						"Match Link: Group Number = ", "Outstanding Amount Stack Trace:"));
				}
			}

			foreach (string transactionType in new string[] { TransactionTypes.GLAutoJournal, TransactionTypes.GLReversingJournal })
			{
				string transactionType1 = transactionType;
				Guid headerPk = new Guid("4696e1c5-a1e9-44e5-9217-ac0c96341aec");

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Due Dates must be set for GL Journal of '{0}' type.", transactionType),
					(BusinessObjectFactory factory1) =>
					{
						AccTransactionHeader journal = (AccTransactionHeader)factory1.New(typeof(AccTransactionHeader), headerPk);
						journal.AH_GC = GlbCompany.CurrentCompany.PK;
						journal.AH_GB = GlbBranch.CurrentBranch.PK;
						journal.AH_GE = GlbDepartment.CurrentDepartment.PK;
						journal.AH_InvoiceDate = ZDateTime.Today;
						journal.AH_Ledger = LedgerTypes.General;
						journal.AH_TransactionType = transactionType1;
						journal.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
						journal.AH_TransactionNum = "VALUEFORTEST";

						Assert("AH_DueDate should not be set via AgePeriod", journal.AH_DueDate.IsEmpty);
						return journal;
					},
					true, CriticalValidationErrorType.AutoGLJournalWithoutDueDate_2, "Auto/Reversing GL Journal without Reverse/Ending Period (Due Date)", GetDueDateNotSetOnAutoOrReverseGLJournalErrorMessage(transactionType1, headerPk)));
			}

			ZGuid fromPK_Test1 = ZGuid.NewZGuid();
			ZGuid toPK_Test1 = ZGuid.NewZGuid();
			ZGuid txnGroup_Test1 = new ZGuid("dee01359-8217-4e76-a82c-735752c75f64");
			ZGuid fromPK_Test2 = ZGuid.NewZGuid();
			ZGuid toPK_Test2 = ZGuid.NewZGuid();
			ZGuid txnGroup_Test2 = new ZGuid("dee01359-8217-4e76-a82c-735752c75f65");

			result.Add(
				new TestCaseDefinitionWithDelegate_Obsolete(
					"Bank Transfer With Invoice Amounts Unbalanced (From Row)",
					f =>
					{
						var fromTxn = GetBalancedBankTransferFrom(f, fromPK_Test1, txnGroup_Test1);
						var toTxn = GetBalancedBankTransferTo(f, toPK_Test1, txnGroup_Test1);
						toTxn.AH_InvoiceAmount = 999.99M;
						return fromTxn;
					},
					true, CriticalValidationErrorType.LinesOfBankTranferShouldBalanceToZero_3,
					"Bank transfer where the buy transaction and sell transaction do not balance.",
					GetLinesOfBankTranferShouldBalanceToZeroErrorMessage(true, fromPK_Test1, toPK_Test1, txnGroup_Test1)
				));

			ZGuid fromTxnPK2 = ZGuid.Empty, toTxnPK2 = ZGuid.Empty;

			result.Add(
				new TestCaseDefinitionWithDelegate_Obsolete(
					"Bank Transfer With Invoice Amounts Unbalanced (To Row)",
					f =>
					{
						var fromTxn = GetBalancedBankTransferFrom(f, fromPK_Test2, txnGroup_Test2);
						var toTxn = GetBalancedBankTransferTo(f, toPK_Test2, txnGroup_Test2);
						toTxn.AH_InvoiceAmount = 999.99M;
						return toTxn;
					},
					true, CriticalValidationErrorType.LinesOfBankTranferShouldBalanceToZero_3,
					"Bank transfer where the buy transaction and sell transaction do not balance.",
					GetLinesOfBankTranferShouldBalanceToZeroErrorMessage(false, fromPK_Test2, toPK_Test2, txnGroup_Test2)
				));

			result.Add(
				new TestCaseDefinitionWithDelegate_Obsolete(
					"Bank Transfer With Invoice Amounts Balanced (From Row)",
					f =>
					{
						var fromTxn = GetBalancedBankTransferFrom(f, ZGuid.NewZGuid(), new Guid("dee01359-8217-4e76-a82c-735752c75f66"));
						var toTxn = GetBalancedBankTransferTo(f, ZGuid.NewZGuid(), new Guid("dee01359-8217-4e76-a82c-735752c75f66"));
						return fromTxn;
					}
					));

			result.Add(
				new TestCaseDefinitionWithDelegate_Obsolete(
					"Bank Transfer With Invoice Amounts Balanced (From Row)",
					f =>
					{
						var fromTxn = GetBalancedBankTransferFrom(f, ZGuid.NewZGuid(), new Guid("dee01359-8217-4e76-a82c-735752c75f67"));
						var toTxn = GetBalancedBankTransferTo(f, ZGuid.NewZGuid(), new Guid("dee01359-8217-4e76-a82c-735752c75f67"));
						return toTxn;
					}
					));

			foreach (KeyValuePair<string, List<string>> entry in compatibilityLedgerTransactionTypeMatrice)
			{
				var ledger = entry.Key;
				foreach (var transactionType in entry.Value)
				{
					result.Add(
						new TestCaseDefinitionWithDelegate_Obsolete(
							string.Format("Check Transaction Header compatibility: ledger={0} transactionType={1}", ledger, transactionType),
							factory =>
							{
								AccTransactionHeader transactionHeader;
								if (ledger == LedgerTypes.CashBook && transactionType == TransactionTypes.Transfer)
								{
									transactionHeader = GetBalancedBankTransferFrom(factory, ZGuid.NewZGuid(), new Guid("dee01359-8217-4e76-a82c-735752c75f67"));
									var transactionTo = GetBalancedBankTransferTo(factory, ZGuid.NewZGuid(), new Guid("dee01359-8217-4e76-a82c-735752c75f67"));
								}
								else
								{
									transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
									transactionHeader.AH_Ledger = ledger;
									transactionHeader.AH_TransactionType = transactionType;
									if (transactionType == TransactionTypes.GLAutoJournal || transactionType == TransactionTypes.GLReversingJournal)
									{
										transactionHeader.AH_DueDate = DateTime.Today.AddDays(1);
									}
									else if (ledger == LedgerTypes.CashBook && transactionType == TransactionTypes.ExchangeDifference)
									{
										transactionHeader.AH_AG = Factory.LoadTop1<AccGLHeader>(new ZQuery()).PK;
									}
								}
								return transactionHeader;
							},
							false, CriticalValidationErrorType.TransactionHeaderLedgerNotCompatibleWithTransactionHeaderType_2,
							"This Transaction ledger is not compatible with the transaction type"
						)
					);
				}
			}

			result.Add(
				new TestCaseDefinitionWithDelegate_Obsolete(
					string.Format("Check JobCosting ledger is not compatible with DirectReceipt transactionType"),
					factory =>
					{
						var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
						var ledger = LedgerTypes.JobCosting;
						var transactionType = TransactionTypes.DirectReceipt;
						Assert("JobCosting ledger is not compatible with DirectReceipt transactionType", !compatibilityLedgerTransactionTypeMatrice[ledger].Any(x => x == transactionType));
						transactionHeader.AH_Ledger = ledger;
						transactionHeader.AH_TransactionType = transactionType;
						return transactionHeader;
					},
					true, CriticalValidationErrorType.TransactionHeaderLedgerNotCompatibleWithTransactionHeaderType_2,
					"This Transaction ledger is not compatible with the transaction type"
				)
			);

			return result;
		}

		public void TestCheckGeneralLedgerAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			var cashBook = Factory.NewWithValidTestData<AccTransactionHeader>();
			cashBook.AH_Ledger = LedgerTypes.CashBook;
			cashBook.AH_TransactionType = TransactionTypes.ExchangeDifference;
			cashBook.AH_PostDate = new ZDateTime(2009, 11, 10);
			cashBook.AH_InvoiceDate = cashBook.AH_PostDate;
			cashBook.AH_AB = bankAccount.PK;
			cashBook.AH_Desc = "test bank currency adjustment";
			cashBook.AH_ExchangeRate = 1m;
			cashBook.AH_TransactionNum = "Test0000001";
			cashBook.AH_OSTotal = cashBook.AH_LocalTotal;
			cashBook.AH_AG = ZGuid.Empty;
			AssertOnSavingCheck(
				cashBook
				, new TestCaseDefinition_ForSeparateTestsMethods(
					"CashBookExchangeTransactionNeedGeneralLedgerAccount"
					, true
					, CriticalValidationErrorType.CashBookExchangeTransactionNeedGeneralLedgerAccount
					, CriticalValidationMessageTemplate.CashBookExchangeTransactionNeedGeneralLedgerAccountErrorMessage
					, "PK ="
					, $"CurrencyAdjustmentExchangeGainAccount:{ObjectFactory.Get<IAccounting>().Registry.CurrencyAdjustmentExchangeGainAccount.Value}"
					, $"CurrencyAdjustmentExchangeLossAccount:{ObjectFactory.Get<IAccounting>().Registry.CurrencyAdjustmentExchangeLossAccount.Value}"
					)
			);

			cashBook.AH_AG = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "2020.10.00")).PK.ToGuid();
			AssertOnSavingCheck(cashBook, new TestCaseDefinition_ForSeparateTestsMethods(""));
		}

		#region Implementation

		readonly string[] ledgerTypes = new[] { LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable };

		readonly string[] otherLedgerTypes = new[] { LedgerTypes.IncompleteTransactions, LedgerTypes.CashBook, LedgerTypes.General, LedgerTypes.JobCosting, LedgerTypes.TransactionsPendingAllocation, LedgerTypes.UnapprovedPayableTransactions };

		readonly string[] miscTransactionTypes = new string[] { TransactionTypes.ExchangeDifference, TransactionTypes.Discount, TransactionTypes.Overpayment };

		readonly string[] ledgerTypesAllowedToChangeAmountAfterBeingSaved = new[] { LedgerTypes.IncompleteTransactions, LedgerTypes.UnapprovedPayableTransactions, LedgerTypes.TransactionsPendingAllocation, LedgerTypes.General };

		readonly string[] ledgerTypesNotAllowedToChangeAmountAfterBeingSaved = new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable, LedgerTypes.JobCosting, LedgerTypes.CashBook };

		protected string GetOutstandingAmountIsNotZeroOnMatchingMiscTransactionsErrorMessage(string ledgerType, string transactionType)
		{
			return string.Format(
				"Header: PK = 7560dc0b-ec9a-4319-b8e7-e5b3bb30a7bb, Ledger = {0}, Transaction Type = {1}, Invoice Date = {2}, Post Date = , Invoice Amount = 20, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 5, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = VALUEFORTEST, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.",
				ledgerType, transactionType, ZDateTime.Today.ToAUString()
			);
		}

		protected string GetDueDateNotSetOnAutoOrReverseGLJournalErrorMessage(string transactionType, Guid headerPk)
		{
			StringBuilder result = new StringBuilder(string.Format("Header: PK = {2}, Ledger = GL, Transaction Type = {0}, Invoice Date = {1}, Post Date = , Invoice Amount = 0, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = VALUEFORTEST, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.",
				transactionType, ZDateTime.Today.ToAUString(), headerPk));

			return result.ToString();
		}

		string GetLinesOfBankTranferShouldBalanceToZeroErrorMessage(bool isFromTxnValidation, ZGuid fromPK, ZGuid toPK, ZGuid txnGroupId)
		{
			string fromPKDetails = string.Format(
				"Header: PK = {0}, Ledger = CB, Transaction Type = TRF, Invoice Date = {1}, Post Date = {1}, Invoice Amount = -1000, GST Amount = 0, OS Total = -1000, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = VALUEFORTEST, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Transaction Group Identifier = {2}, Business Contexts = None",
				fromPK, ZDateTime.Today.ToAUString(), txnGroupId);

			string toPKDetails = string.Format(
				"Header: PK = {0}, Ledger = CB, Transaction Type = TRF, Invoice Date = {1}, Post Date = {1}, Invoice Amount = 999.99, GST Amount = 0, OS Total = 1000, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = VALUEFORTEST, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Transaction Group Identifier = {2}, Business Contexts = None",
				toPK, ZDateTime.Today.ToAUString(), txnGroupId);

			return string.Format(@"This transaction:
{0}.

Corresponding transaction:
{1}.",
			isFromTxnValidation ? fromPKDetails : toPKDetails,
			isFromTxnValidation ? toPKDetails : fromPKDetails);
		}

		AccTransactionHeader GetNewParentForOutstandingAmountCheck(BusinessObjectFactory factory, string ledgerType, string transactionType)
		{
			Guid pk = new Guid("7560dc0b-ec9a-4319-b8e7-e5b3bb30a7bb");
			AccTransactionHeader result = factory.Load<AccTransactionHeader>(pk);
			if (result == null)
			{
				result = factory.NewWithPrimaryKey<AccTransactionHeader>(pk);
				result.AH_TransactionNum = "VALUEFORTEST";
				result.AH_InvoiceDate = ZDateTime.Today;

				link = factory.New<AccTransactionMatchLink>();
				link.AP_MatchGroupNum = "M000111";
				link.AP_AH = result.PK;
				link.AP_MatchDate = new ZDateTime(2010, 3, 2);
			}

			result.AH_InvoiceAmount = 20m;
			result.AH_OutstandingAmount = 5m;
			result.AH_Ledger = ledgerType;
			result.AH_GC = GlbCompany.CurrentCompany.PK;
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			result.AH_TransactionType = transactionType;
			result.AH_TransactionNum = "VALUEFORTEST";

			if (transactionType == TransactionTypes.GLAutoJournal || transactionType == TransactionTypes.GLReversingJournal)
			{
				result.AH_DueDate = ZDateTime.Today.AddDays(1);
			}

			link.AP_Amount = 10m;

			return result;
		}

		AccTransactionHeader GetNewParentForOutstandingAmountSignCheck(BusinessObjectFactory factory, string ledgerType, string transactionType)
		{
			Guid pk = new Guid("8897541e-a419-4b89-9989-fed284fde131");
			AccTransactionHeader result = factory.Load<AccTransactionHeader>(pk);
			if (result == null)
			{
				result = factory.NewWithPrimaryKey<AccTransactionHeader>(pk);
				result.AH_TransactionNum = "TEST0001";
				result.AH_InvoiceDate = ZDateTime.Today;
				result.AH_InvoiceAmount = -100m;

				link = factory.New<AccTransactionMatchLink>();
				link.AP_MatchGroupNum = "M000001";
				link.AP_AH = result.PK;
				link.AP_MatchDate = new ZDateTime(2016, 2, 19);
			}

			result.AH_OutstandingAmount = 2m;
			result.AH_Ledger = ledgerType;

			result.AH_GC = GlbCompany.CurrentCompany.PK;
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			result.AH_TransactionType = transactionType;

			if (transactionType == TransactionTypes.GLAutoJournal || transactionType == TransactionTypes.GLReversingJournal)
			{
				result.AH_DueDate = ZDateTime.Today.AddDays(1);
			}

			link.AP_Amount = -98m;

			return result;
		}

		AccTransactionMatchLink link;

		AccTransactionHeader GetInvalidTransactionSavedInDb(BusinessObjectFactory factory)
		{
			ZGuid pk = new ZGuid("ad951d3f-6ec0-43a3-893a-fff722b9fef9");
			AccTransactionHeader result = factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, pk));
			if (result == null)
			{
				string insertCommand = string.Format(
					"INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_Ledger, AH_TransactionNum, AH_TransactionType, AH_InvoiceDate, AH_OutstandingAmount, AH_InvoiceAmount, AH_RX_NKTransactionCurrency)" +
					"VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', 123, 20, 'AUD')",
					pk.ToString(), GlbCompany.CurrentCompany.PK.ToString(), GlbBranch.CurrentBranch.PK.ToString(), GlbDepartment.CurrentDepartment.PK.ToString(), ledgerTypes[0], "CRVALTEST001", TransactionTypes.Journal, ZDateTime.Today.ToISO8601String()
				);
				Db.Connection.ExecuteNonQuery(insertCommand);

				result = factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OutstandingAmount, 123).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			}
			AssertNotNull("Invalid Transaction should be loaded from DB", result);
			result.AH_InvoicePrinted = true;
			Assert("Should have changes", result.HasChanges);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		AccTransactionHeader GetNewParentForClearedDateCheck_NotInDB(BusinessObjectFactory factory, string ledgerType, ClearedDateTestCase clearedDateTestCase, int transactionNum, Guid headerPk)
		{
			AccTransactionHeader result = (AccTransactionHeader)factory.New(typeof(AccTransactionHeader), headerPk);
			result.AH_GC = GlbCompany.CurrentCompany.PK;
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_GE = GlbDepartment.CurrentDepartment.PK;
			result.AH_InvoiceDate = ZDateTime.Today;
			result.AH_TransactionNum = transactionNum.ToString();
			result.AH_Ledger = ledgerType;
			result.AH_TransactionType = TransactionTypes.Receipt;
			result.AH_DateClearedInCashbook = clearedDateTestCase.SetClearedDate ? ZDateTime.Today : ZDateTime.Empty;
			result.AH_IsCancelled = clearedDateTestCase.SetIsCancelled;
			result.AH_ReceiptBatchNo = clearedDateTestCase.SetBatchNo ? "TEST001" : string.Empty;
			result.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			return result;
		}

		AccTransactionHeader GetNewParentForClearedDateCheck_InDB(BusinessObjectFactory factory, string ledgerType, int transactionNum, string fieldToHaveChanges, Guid pk)
		{
			var year = ZDateTime.Today.Year;
			var createTime = new ZDateTime(year, 1, 1).ToISO8601String();
			string insertCommand = string.Format("INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DateClearedInCashbook, AH_IsCancelled, AH_ReceiptBatchNo, AH_RX_NKTransactionCurrency, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', {9}, '{10}', 'AUD', '{11}', '~BP', '{11}', '~BP')",
				pk.ToString(), GlbCompany.CurrentCompany.PK.ToString(), GlbBranch.CurrentBranch.PK.ToString(), GlbDepartment.CurrentDepartment.PK.ToString(),
				fieldToHaveChanges == "AH_Ledger" ? "CB" : ledgerType,
				fieldToHaveChanges == "AH_TransactionType" ? "DPY" : "REC",
				transactionNum.ToString(), ZDateTime.Today.ToISO8601String(),
				fieldToHaveChanges == "AH_DateClearedInCashbook" ? ZDateTime.Empty.ToISO8601String() : ZDateTime.Today.ToISO8601String(),
				fieldToHaveChanges == "AH_IsCancelled" ? 1 : 0,
				fieldToHaveChanges == "AH_ReceiptBatchNo" ? "1234" : "",
				createTime);
			Db.Connection.ExecuteNonQuery(insertCommand);

			AccTransactionHeader result = factory.Load<AccTransactionHeader>(pk);

			AssertNotNull("Invalid Transaction should be loaded from DB", result);
			Assert("Should not have changes", !result.HasChanges);

			return result;
		}

		AccTransactionHeader GetTransactionSavedInDb(BusinessObjectFactory factory, string ledgerType, string transactionType)
		{
			var newFactory = factory.CreateNewFactory();
			var transactionHeader = newFactory.New<AccTransactionHeader>();
			transactionHeader.AH_GC = GlbCompany.CurrentCompany.PK;
			transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transactionHeader.AH_InvoiceDate = ZDateTime.Today;
			transactionHeader.AH_TransactionNum = ledgerType + "01";
			transactionHeader.AH_Ledger = ledgerType;
			transactionHeader.AH_TransactionType = transactionType;
			transactionHeader.AH_InvoiceAmount = 1M;
			transactionHeader.AH_GSTAmount = 1M;
			transactionHeader.AH_OSTotal = 2M;
			transactionHeader.AH_OutstandingAmount = 2M;
			newFactory.Save();
			return transactionHeader;
		}

		AccBankAccount fromBank;
		AccBankAccount FromBank
		{
			get
			{
				if (fromBank == null)
				{
					fromBank = Factory.NewWithPrimaryKey<AccBankAccount>(new Guid("46b6a658-9a99-463f-8992-ba5c1f39e266"));
					fromBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
				return fromBank;
			}
		}

		AccBankAccount toBank;
		AccBankAccount ToBank
		{
			get
			{
				if (toBank == null)
				{
					toBank = Factory.NewWithPrimaryKey<AccBankAccount>(new Guid("f0a8365b-61a8-4760-9e37-b9da918ba5c4"));
					toBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
				return toBank;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		protected AccTransactionHeader GetBalancedBankTransferFrom(BusinessObjectFactory factory, ZGuid pk, ZGuid transactionBelongsToGroupId)
		{
			var fromTxn = (AccTransactionHeader)Factory.New(typeof(AccTransactionHeader), pk.ToGuid());
			fromTxn.AH_GC = GlbCompany.CurrentCompany.PK;
			fromTxn.AH_GB = GlbBranch.CurrentBranch.PK;
			fromTxn.AH_GE = GlbDepartment.CurrentDepartment.PK;
			fromTxn.AH_AB = FromBank.PK;
			fromTxn.AH_ChequeOrReference = "1000";
			fromTxn.AH_Desc = "CASH BOOK TRANSFER";
			fromTxn.AH_ExchangeRate = 1;
			fromTxn.AH_InvoiceAmount = -1000;
			fromTxn.AH_InvoiceDate = ZDateTime.Today;
			fromTxn.AH_PostDate = fromTxn.AH_InvoiceDate;
			fromTxn.AH_Ledger = LedgerTypes.CashBook;
			fromTxn.AH_NumberOfSupportingDocuments = 1;
			fromTxn.AH_ReceiptType = ReceiptTypes.EFT;
			fromTxn.AH_OSTotal = -1000;
			fromTxn.AH_TransactionBelongsToGroup = transactionBelongsToGroupId;
			fromTxn.AH_TransactionCount = AccTransactionHeader.TransactionCountConstants.BankTransferFromRow;
			fromTxn.AH_RX_NKTransactionCurrency = "AUD";
			fromTxn.AH_TransactionType = TransactionTypes.Transfer;
			fromTxn.AH_TransactionNum = "VALUEFORTEST";
			return fromTxn;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		protected AccTransactionHeader GetBalancedBankTransferTo(BusinessObjectFactory factory, ZGuid pk, ZGuid transactionBelongsToGroupId)
		{
			var toTxn = (AccTransactionHeader)Factory.New(typeof(AccTransactionHeader), pk.ToGuid());
			toTxn.AH_GC = GlbCompany.CurrentCompany.PK;
			toTxn.AH_GB = GlbBranch.CurrentBranch.PK;
			toTxn.AH_GE = GlbDepartment.CurrentDepartment.PK;
			toTxn.AH_AB = ToBank.PK;
			toTxn.AH_ChequeOrReference = "1000";
			toTxn.AH_Desc = "CASH BOOK TRANSFER";
			toTxn.AH_ExchangeRate = 1;
			toTxn.AH_InvoiceAmount = 1000;
			toTxn.AH_InvoiceDate = ZDateTime.Today;
			toTxn.AH_PostDate = toTxn.AH_InvoiceDate;
			toTxn.AH_Ledger = LedgerTypes.CashBook;
			toTxn.AH_NumberOfSupportingDocuments = 1;
			toTxn.AH_ReceiptType = ReceiptTypes.EFT;
			toTxn.AH_OSTotal = 1000;
			toTxn.AH_TransactionBelongsToGroup = transactionBelongsToGroupId;
			toTxn.AH_TransactionCount = AccTransactionHeader.TransactionCountConstants.BankTransferToRow;
			toTxn.AH_RX_NKTransactionCurrency = "AUD";
			toTxn.AH_TransactionType = TransactionTypes.Transfer;
			toTxn.AH_TransactionNum = "VALUEFORTEST";
			return toTxn;
		}

		int NewTransactionNum
		{
			get { return ++fTransactionNum; }
		}
		int fTransactionNum;

		ITransactionCreator transactionCreator;
		ITransactionCreator TransactionCreator
		{
			get
			{
				if (transactionCreator == null)
				{
					transactionCreator = ObjectFactory.Get<ITransactionCreator>();
				}
				return transactionCreator;
			}
		}

		protected class ClearedDateTestCase
		{
			public bool SetClearedDate { get; set; }
			public bool SetIsCancelled { get; set; }
			public bool SetBatchNo { get; set; }
			public bool ShouldFail { get; set; }
		}

		#endregion
	}
}
