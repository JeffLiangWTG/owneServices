using System;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChequeBook))]
	class AccChequeBookTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccChequeBook>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestMessageIfChequeBookUsesSamePrinter()
		{
			var printer = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			Factory.Save();

			ChequeBook.AK_SQ = printer.PK;
			ChequeBook.Validation.ValidateAK_SQ();
			Book2.AK_Code = "Book2";
			Book2.AK_AutoPrintCheque = ZBool.True;
			Book2.AK_SQ = printer.PK;
			Book2.Validation.ValidateAK_SQ();

			AssertEquals("MessageIfChequeBookUsesSamePrinter", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(((ZString)printer[StmPrintQueueSchema.SQ_QueueName]), Book2.AK_CurrentNo), Book2.MessageIfChequeBookUsesSamePrinter());
		}

		public void TestIsDataVersionsAutoLogged()
		{
			AccountingMasterFilesRegistry.Instance.CheckBookDataVersionAutoLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ((IDataVersionLoggingSupported)ChequeBook).IsDataVersionsAutoLogged);

			AccountingMasterFilesRegistry.Instance.CheckBookDataVersionAutoLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ((IDataVersionLoggingSupported)ChequeBook).IsDataVersionsAutoLogged);
		}

		public void TestNoStmALogs()
		{
			var chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chequeBook.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				chequeBook.AK_Code = "UU";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chequeBook.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		#region Test IsReferencedByJobCharge

		public void TestIsReferencedByJobCharge()
		{
			JobCharge jC = TestFactory.New<JobCharge>();
			jC.JR_AK = ZGuid.NewZGuid();
			Assert(!ChequeBook.IsReferencedByJobCharge());

			jC.JR_AK = ChequeBook.PK;
			Assert(ChequeBook.IsReferencedByJobCharge());
		}

		#endregion

		#region Test GetReferencedCompany

		public void TestGetReferencedCompany()
		{
			GlbCompany company = TestFactory.LoadTop1<GlbCompany>(new ZQuery());
			AccBankAccount account = TestFactory.New<AccBankAccount>();

			ChequeBook.AK_AB = account.PK;
			account.AB_GC = company.PK;

			Assert(company.PK == ChequeBook.GetReferencedCompany().PK);
		}

		#endregion

		#region Test ReadOnly

		public void TestBankAccountFieldReadOnly()
		{
			JobCharge charge = TestFactory.New(typeof(JobCharge)) as JobCharge;
			charge.JR_GB = TestFactory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			charge.JR_AC = TestFactory.LoadTop1(typeof(AccChargeCode), new ZQuery()).PK;
			charge.JR_GE = TestFactory.LoadTop1(typeof(GlbDepartment), new ZQuery()).PK;
			charge.JR_AK = ChequeBook.PK;
			ChequeBook.OnLoaded();
			Assert("Bank Account field should be read-only", ChequeBook.AK_ABInfo.ReadOnly);
		}

		public void TestBranchFieldReadOnly()
		{
			AccBankAccount bankAccount = TestFactory.New(typeof(AccBankAccount)) as AccBankAccount;
			bankAccount.AB_GB = ZGuid.Empty;
			ChequeBook.AK_AB = bankAccount.PK;
			Assert("Branch field should be writeable", !ChequeBook.AK_GBInfo.ReadOnly);

			bankAccount.AB_GB = TestFactory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;

			// force read-only update
			ChequeBook.AK_AB = ZGuid.Empty;
			ChequeBook.AK_AB = bankAccount.PK;

			Assert("Branch field should be read only", ChequeBook.AK_GBInfo.ReadOnly);
		}

		#endregion

		#region Test StartNoOverlapsExistingRanges

		public void TestStartNoOverlapsExistingRanges()
		{
			AccBankAccount bankAccount = TestFactory.New(typeof(AccBankAccount)) as AccBankAccount;
			ChequeBook.AK_AB = bankAccount.PK;

			AccChequeBook chequeBook2 = TestFactory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook2.AK_AB = bankAccount.PK;

			chequeBook2.AK_StartNo = 4;
			chequeBook2.AK_LastNo = 7;

			ChequeBook.AK_StartNo = 3;
			Assert("Should be no errors since no overlap at this stage", !ChequeBook.StartNoOverlapsExistingRanges());

			ChequeBook.AK_StartNo = 4;
			Assert("Expect an error since there is overlap", ChequeBook.StartNoOverlapsExistingRanges());

			ChequeBook.AK_StartNo = 6;
			Assert("Expect an error since there is overlap", ChequeBook.StartNoOverlapsExistingRanges());

			ChequeBook.AK_StartNo = 7;
			Assert("Expect an error since there is overlap", ChequeBook.StartNoOverlapsExistingRanges());

			ChequeBook.AK_StartNo = 8;
			Assert("Should be no errors since no overlap", !ChequeBook.StartNoOverlapsExistingRanges());
		}

		#endregion

		#region Test LastNoOverlapsExistingRanges

		public void TestLastNoOverlapsExistingRanges()
		{
			AccBankAccount bankAccount = TestFactory.New(typeof(AccBankAccount)) as AccBankAccount;
			ChequeBook.AK_AB = bankAccount.PK;

			AccChequeBook chequeBook2 = TestFactory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook2.AK_AB = bankAccount.PK;

			chequeBook2.AK_StartNo = 4;
			chequeBook2.AK_LastNo = 7;

			ChequeBook.AK_StartNo = 1;
			ChequeBook.AK_LastNo = 3;
			Assert("Should be no errors since no overlap", !ChequeBook.LastNoOverlapsExistingRanges());

			ChequeBook.AK_LastNo = 4;
			Assert("Expect an error since there is overlap", ChequeBook.LastNoOverlapsExistingRanges());

			ChequeBook.AK_LastNo = 6;
			Assert("Expect an error since there is overlap", ChequeBook.LastNoOverlapsExistingRanges());

			ChequeBook.AK_StartNo = 5;
			Assert("Expect an error since there is overlap", ChequeBook.LastNoOverlapsExistingRanges());

			ChequeBook.AK_LastNo = 7;
			Assert("Expect an error since there is overlap", ChequeBook.LastNoOverlapsExistingRanges());

			ChequeBook.AK_LastNo = 9;
			Assert("Expect no errors since last no. does not overlap", !ChequeBook.LastNoOverlapsExistingRanges());

			ChequeBook.AK_StartNo = 8;
			Assert("Should be no errors since no overlap", !ChequeBook.LastNoOverlapsExistingRanges());

			ChequeBook.AK_StartNo = 1;
			Assert("Expect an error since there is overlap", ChequeBook.LastNoOverlapsExistingRanges());
		}

		#endregion

		#region TestUntickingAK_AutoPrintChequeWillResetPrinter

		public void TestUntickingAK_AutoPrintChequeWillResetPrinter()
		{
			ChequeBook.AK_AutoPrintCheque = ZBool.True;
			BusinessObject printer = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			ChequeBook.AK_SQ = printer.PK;
			ChequeBook.AK_AutoPrintCheque = ZBool.False;
			Assert("Printer should be reset", ChequeBook.AK_SQ.IsEmpty);
		}

		#endregion

		#region TestAK_SQReadOnly

		public void TestAK_SQReadOnly()
		{
			Assert("AK_SQ should be read only by default", ChequeBook.AK_SQInfo.ReadOnly);
			ChequeBook.AK_AutoPrintCheque = ZBool.True;
			Assert("AK_SQ should not be read only", !ChequeBook.AK_SQInfo.ReadOnly);
			ChequeBook.AK_AutoPrintCheque = ZBool.False;
			Assert("AK_SQ should become to be read only again", ChequeBook.AK_SQInfo.ReadOnly);
		}

		#endregion

		#region TestIsAutoPrint

		public void TestIsAutoPrint()
		{
			AccBankAccount testBank = TestFactory.NewWithValidTestData<AccBankAccount>();
			Assert("IsAutoPrint should be false by default", !ChequeBook.IsAutoPrint);
			ChequeBook.AK_AutoPrintCheque = ZBool.True;
			ChequeBook.AK_AB = testBank.PK;
			Assert("IsAutoPrint should remain false", !ChequeBook.IsAutoPrint);
			StmTemplate chequeTemplate = TestFactory.NewWithValidTestData<StmTemplate>();
			ChequeBook.BankAccount.AB_SO_ChequeTemplate = chequeTemplate.PK;
			Assert("IsAutoPrint should change to true", ChequeBook.IsAutoPrint);
			ChequeBook.AK_AutoPrintCheque = ZBool.False;
			Assert("IsAutoPrint should change false", !ChequeBook.IsAutoPrint);
		}

		#endregion

		#region TEST: Test Cheque Number In Cheque Book

		public void TestChequeNumberIsInChequeBook()
		{
			AccBankAccount bankAccount = CreateBankAccount(Factory, AUD);
			AccChequeBook chequeBook = CreateChequeBook("TESTCB", 5000, 5304, 5499, bankAccount);

			AssertEquals("In Cheque Book", true, chequeBook.IsChequeInBook(5000));
			AssertEquals("In Cheque Book", true, chequeBook.IsChequeInBook(5499));
			AssertEquals("In Cheque Book", false, chequeBook.IsChequeInBook(4999));
			AssertEquals("In Cheque Book", false, chequeBook.IsChequeInBook(5500));
		}

		#endregion

		#region TEST: Test Cheque Has Been Posted

		public void TestHasChequeBeenPosted()
		{
			AccBankAccount bankAccount = CreateBankAccount(Factory, AUD);
			AccChequeBook chequeBook = CreateChequeBook("TESTCB", 5000, 5253, 5499, bankAccount);
			int chequeNumber = 5252;

			AssertEquals("Has Cheque Been Posted", false, chequeBook.HasChequeBeenPosted(chequeNumber));
			AccTransactionHeader directPayment = CreateTransactionHeader(ZArchitecture.Core.ReceiptTypes.Cheque, bankAccount, chequeNumber.ToString());
			Factory.Save();
			AssertEquals("Has Cheque Been Posted", true, chequeBook.HasChequeBeenPosted(chequeNumber));
		}
		#endregion

		#region TEST: Test Update Current Number

		public void TestUpdateCurrentNumber()
		{
			AccBankAccount bankAccount = CreateBankAccount(Factory, AUD);
			AccChequeBook chequeBook = CreateChequeBook("TESTCB", 8000, 8345, 8499, bankAccount);
			Factory.Save();

			AssertEquals("Current Number", 8345m, chequeBook.AK_CurrentNo);
			AccChequeBook.UpdateCurrentNumber(chequeBook.PK, 8349);
			AssertEquals("Current Number", 8349m, chequeBook.AK_CurrentNo);

			AccChequeBook.UpdateCurrentNumber(chequeBook.PK, 8350.5);
			AssertEquals("Current Number should not change", 8349m, chequeBook.AK_CurrentNo);

			// Make Sure Been Saved to the DB
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccChequeBook newChequeBook = newFactory.Load<AccChequeBook>(chequeBook.PK);
			AssertEquals("Current Number", 8349m, newChequeBook.AK_CurrentNo);
		}
		#endregion

		#region Validate Cheque Number Digits

		public void TestValidateChequeNumberDigits()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 5;
			AccChequeBook chequeBook = CreateChequeBook("TESTCB", 10000, 20000, 30000, bankAccount);

			AssertEquals("Valid cheque numbers", false, chequeBook.AK_Calc_LastNoStringInfo.HasErrors());

			chequeBook.AK_Calc_StartNoString = "1";
			AssertEquals("Should be 00001", "00001", chequeBook.AK_Calc_StartNoString);

			chequeBook.AK_Calc_LastNoString = "5";
			AssertEquals("Should be 00005", "00005", chequeBook.AK_Calc_LastNoString);

			chequeBook.AK_Calc_CurrentNoString = "3";
			AssertEquals("Should be 00003", "00003", chequeBook.AK_Calc_CurrentNoString);

			chequeBook.AK_Calc_LastNoString = "123456";
			chequeBook.AK_Calc_StartNoString = "100001";
			AssertEquals("Invalid cheque number", true, chequeBook.AK_Calc_StartNoStringInfo.HasErrors());

			chequeBook.AK_Calc_StartNoString = "10000";
			chequeBook.AK_Calc_LastNoString = "123456";
			AssertEquals("Invalid cheque number", true, chequeBook.AK_Calc_LastNoStringInfo.HasErrors());

			chequeBook.AK_Calc_StartNoString = "10000";
			chequeBook.AK_Calc_LastNoString = "50000";
			chequeBook.AK_Calc_CurrentNoString = "500005";
			AssertEquals("Invalid cheque number", true, chequeBook.AK_Calc_CurrentNoStringInfo.HasErrors());

			chequeBook.AK_Calc_StartNoString = "100001";
			chequeBook.AK_Calc_LastNoString = "100009";
			chequeBook.AK_Calc_CurrentNoString = "100005";
			AssertEquals("Invalid cheque number", true, chequeBook.AK_Calc_CurrentNoStringInfo.HasErrors());

			chequeBook.AK_Calc_StartNoString = "56";
			AssertEquals("Invalid cheque number", false, chequeBook.AK_Calc_StartNoStringInfo.HasErrors());

			chequeBook.AK_Calc_LastNoString = "666";
			AssertEquals("Invalid cheque number", false, chequeBook.AK_Calc_LastNoStringInfo.HasErrors());

			chequeBook.AK_Calc_CurrentNoString = "3";
			AssertEquals("Invalid cheque number", true, chequeBook.AK_Calc_CurrentNoStringInfo.HasErrors());
		}

		#endregion

		public void TestIsReferencedByAccHotCheque()
		{
			var bankAccount = CreateBankAccount(Factory, AUD);
			var chequeBook = CreateChequeBook("TESTCB1", 5000, 5304, 5499, bankAccount);
			Factory.Save();

			var bankAccount2 = CreateBankAccount(Factory, AUD);
			var chequeBook2 = CreateChequeBook("TESTCB2", 5000, 5304, 5499, bankAccount2);

			var sql = $"INSERT INTO dbo.AccHotCheque (AQ_PK, AQ_AK, AQ_ActualOrMaxIndicator, AQ_SystemCreateTimeUtc, AQ_SystemCreateUser, AQ_SystemLastEditTimeUtc, AQ_SystemLastEditUser) values (NEWID(), '{chequeBook.PK}', '{ZArchitecture.Core.ActualOrMaxIndicator.Actual}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);

			AssertEquals(true, chequeBook.IsReferencedByAccHotCheque());
			AssertEquals(false, chequeBook2.IsReferencedByAccHotCheque());
		}

		#region ICancellable

		public void TestPreventDelete()
		{
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(AccChequeBook)));
		}

		#endregion

		#region Implementation

		protected override System.Collections.Generic.Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				System.Collections.Generic.Dictionary<string, IZType> result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result[Enterprise.MasterFiles.Business.AccChequeBook.CalculatedFieldNames.AK_Calc_CurrentNoString] = (ZString)"3";
				result[Enterprise.MasterFiles.Business.AccChequeBook.CalculatedFieldNames.AK_Calc_StartNoString] = (ZString)"9";
				result[Enterprise.MasterFiles.Business.AccChequeBook.CalculatedFieldNames.AK_Calc_LastNoString] = (ZString)"11";
				return result;
			}
		}

		AccChequeBook CreateChequeBook(string code, int firstNumber, int currentNumber, int lastNumber, AccBankAccount bankAccount)
		{
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_Calc_StartNoString = firstNumber.ToString();
			chequeBook.AK_Calc_LastNoString = lastNumber.ToString();
			chequeBook.AK_Code = code;
			chequeBook.AK_Calc_CurrentNoString = currentNumber.ToString();
			chequeBook.AK_GB = Env.CurrentBranch.PK;
			return chequeBook;
		}

		protected AccTransactionHeader CreateTransactionHeader(string receiptType, AccBankAccount bankAccount, string chequeNumber)
		{
			AccTransactionHeader directPayment = Factory.NewWithValidTestData<AccTransactionHeader>();
			directPayment.AH_Ledger = LedgerTypes.CashBook;
			directPayment.AH_TransactionType = TransactionTypes.DirectPayment;
			directPayment.AH_GB = Env.CurrentBranch.PK;
			directPayment.AH_GE = Env.CurrentDepartment.PK;
			directPayment.AH_PostDate = ZDateTime.Now;
			directPayment.AH_InvoiceDate = ZDateTime.Now;
			directPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			directPayment.AH_AB = bankAccount.PK;
			directPayment.AH_ChequeOrReference = chequeNumber;
			return directPayment;
		}

		protected AccBankAccount CreateBankAccount(BusinessObjectFactory factory, RefCurrency currency)
		{
			AccBankAccount bankAccount = factory.New<AccBankAccount>();
			bankAccount.AB_RX_NKAccountCurrency = currency.RX_Code;
			bankAccount.AB_GC = Env.CurrentCompany.PK;
			bankAccount.AB_GB = Env.CurrentBranch.PK;
			bankAccount.AB_AG = CreateGLHeader(factory).PK;
			bankAccount.AB_Code = "ABCBANK";
			return bankAccount;
		}

		protected AccGLHeader CreateGLHeader(BusinessObjectFactory factory)
		{
			var gLHeader = factory.NewWithValidTestData<AccGLHeader>();
			gLHeader.AG_AccountNum = "1234.56.78";
			return gLHeader;
		}

		protected RefCurrency fAUD;
		protected RefCurrency AUD
		{
			get
			{
				if (fAUD == null)
				{
					fAUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				}
				return fAUD;
			}
		}

		protected BusinessObjectFactory TestFactory;
		protected AccChequeBook ChequeBook, Book2;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			ChequeBook = TestFactory.New(typeof(AccChequeBook)) as AccChequeBook;
			ChequeBook.AK_GB = TestFactory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;

			Book2 = TestFactory.New(typeof(AccChequeBook)) as AccChequeBook;
			Book2.AK_GB = ChequeBook.AK_GB;
		}
		#endregion
	}
}
