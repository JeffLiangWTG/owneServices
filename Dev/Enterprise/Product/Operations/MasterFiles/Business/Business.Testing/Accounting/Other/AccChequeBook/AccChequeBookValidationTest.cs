using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChequeBookValidationTest : BusinessObjectValidationTestCase
	{
		BusinessObjectFactory TestFactory;
		AccChequeBook ChequeBook, Book2;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			ChequeBook = TestFactory.New(typeof(AccChequeBook)) as AccChequeBook;
			ChequeBook.AK_GB = TestFactory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;

			Book2 = TestFactory.New(typeof(AccChequeBook)) as AccChequeBook;
			Book2.AK_GB = ChequeBook.AK_GB;
		}

		#region Test Validation
		public void TestValidateAK_Code()
		{
			// ChequeBook -> Account1 -> Company1
			GlbCompany company1 = TestFactory.New<GlbCompany>();
			company1.GC_StartDate = ZDateTime.Now;
			company1.GC_RX_NKLocalCurrency = TestFactory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			company1.GC_RN_NKCountryCode = TestFactory.LoadTop1<RefCountry>(new ZQuery()).Code;
			company1.GC_Code = "!!!";

			//Company1.GC_ = ZDateTime.Now;
			GlbCompany company2 = TestFactory.New<GlbCompany>();
			company2.GC_StartDate = ZDateTime.Now;
			company2.GC_RX_NKLocalCurrency = company1.GC_RX_NKLocalCurrency;
			company2.GC_RN_NKCountryCode = company1.GC_RN_NKCountryCode;
			company2.GC_Code = "@@@";

			AccBankAccount account1 = TestFactory.New<AccBankAccount>();
			account1.AB_AG = TestFactory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			account1.AB_RX_NKAccountCurrency = TestFactory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			account1.AB_BSB = "###";
			account1.AB_Code = "%%%";

			AccBankAccount account2 = TestFactory.New<AccBankAccount>();
			account2.AB_AG = TestFactory.NewWithValidTestData(typeof(AccGLHeader)).PK;
			account2.AB_RX_NKAccountCurrency = account1.AB_RX_NKAccountCurrency;
			account2.AB_BSB = "$$$";
			account2.AB_Code = "###";

			account1.AB_GC = company1.PK;
			account2.AB_GC = company2.PK;

			ChequeBook.AK_AB = account1.PK;
			Book2.AK_AB = account1.PK;

			ChequeBook.AK_Code = "";
			ChequeBook.Validation.ValidateAK_Code();
			Assert("Cheque book must have a AK_Code", ChequeBook.AK_CodeInfo.HasErrors());

			ChequeBook.AK_Code = "blah";
			ChequeBook.Validation.ValidateAK_Code();
			Assert(!ChequeBook.AK_CodeInfo.HasErrors());

			// Book2 -> Account2 -> Company1
			Book2.AK_Code = "blah";
			ChequeBook.AK_Code = "blah";

			TestFactory.Save();

			ChequeBook.Validation.ValidateAK_Code();
			Assert("Can't have two chequebooks with the same AK_Code in same company", ChequeBook.AK_CodeInfo.HasErrors());

			Book2.AK_AB = account2.PK;
			TestFactory.Save();
			ChequeBook.Validation.ValidateAK_Code();
			Assert("Can have two chequebooks with the same AK_Code in different companies", !ChequeBook.AK_CodeInfo.HasErrors());

			// Book2 -> Account2 -> Company2
			Book2.AK_Code = "bblah";
			Book2.AK_AB = account2.PK;

			TestFactory.Save();
			ChequeBook.Validation.ValidateAK_Code();
			Assert(!ChequeBook.AK_CodeInfo.HasErrors());
		}

		public void TestValidateAK_Desc()
		{
			ChequeBook.AK_Desc = "";
			ChequeBook.Validation.ValidateAK_Desc();
			Assert(ChequeBook.AK_DescInfo.HasErrors());

			ChequeBook.AK_Desc = "description";
			ChequeBook.Validation.ValidateAK_Desc();
			Assert(!ChequeBook.AK_DescInfo.HasErrors());
		}

		public void TestValidateAK_AB()
		{
			ChequeBook.AK_AB = ZGuid.Empty;
			ChequeBook.Validation.ValidateAK_AB();
			Assert(ChequeBook.AK_ABInfo.HasErrors());
		}

		public void TestValidateAK_GB()
		{
			ChequeBook.AK_GB = ZGuid.Empty;
			ChequeBook.Validation.ValidateAK_GB();
			Assert(ChequeBook.AK_GBInfo.HasErrors());
		}

		public void TestValidateAK_StartNo()
		{
			ChequeBook.AK_StartNo = 9;
			ChequeBook.AK_LastNo = 10;
			ChequeBook.AK_StartNo = 11;
			Assert("Start number cannot be greater than last number", ChequeBook.AK_StartNoInfo.HasErrors());

			ChequeBook.AK_StartNo = 9;
			ChequeBook.AK_LastNo = 11;
			Assert(!ChequeBook.AK_StartNoInfo.HasErrors());
		}

		public void TestValidateAK_Calc_StartNoString()
		{
			ChequeBook.AK_Calc_StartNoString = "9";
			ChequeBook.AK_Calc_LastNoString = "10";
			ChequeBook.AK_Calc_StartNoString = "11";
			Assert("Start number cannot be greater than last number", ChequeBook.AK_Calc_StartNoStringInfo.HasErrors());

			ChequeBook.AK_Calc_StartNoString = "9";
			ChequeBook.AK_Calc_LastNoString = "11";
			Assert(!ChequeBook.AK_Calc_StartNoStringInfo.HasErrors());
		}

		public void TestValidateAK_LastNo()
		{
			ChequeBook.AK_StartNo = 9;
			ChequeBook.AK_LastNo = 6;
			Assert("Last number must be greater than start number", ChequeBook.AK_LastNoInfo.HasErrors());

			ChequeBook.AK_StartNo = 9;
			ChequeBook.AK_LastNo = 12;
			Assert(!ChequeBook.AK_LastNoInfo.HasErrors());
		}

		public void TestValidateAK_Calc_LastNoString()
		{
			ChequeBook.AK_Calc_StartNoString = "9";
			ChequeBook.AK_Calc_LastNoString = "6";
			Assert("Last number must be greater than start number", ChequeBook.AK_Calc_LastNoStringInfo.HasErrors());

			ChequeBook.AK_Calc_StartNoString = "9";
			ChequeBook.AK_Calc_LastNoString = "12";
			Assert(!ChequeBook.AK_Calc_LastNoStringInfo.HasErrors());
		}

		public void TestValidateAK_CurrentNo()
		{
			var factory = new BusinessObjectFactory();
			var glHeader = factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "999.888.88";
			glHeader.AG_AccountType = "BSH";

			var bankAccount = factory.New<AccBankAccount>();
			bankAccount.AB_Code = "TSTBNK";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_AG = glHeader.PK;

			var printer = factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();

			factory.Save();

			var chequeBook = factory.New<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = true;
			chequeBook.AK_SQ = printer.PK;
			chequeBook.AK_AB = bankAccount.PK;

			chequeBook.AK_StartNo = 9;
			chequeBook.AK_LastNo = 12;
			chequeBook.AK_CurrentNo = 9;

			Assert(!chequeBook.AK_CurrentNoInfo.HasErrors());
			AssertNoError(chequeBook.AK_CurrentNoInfo, "Current number must be between start number and last number");

			factory.Save();

			chequeBook.AK_CurrentNo = 8;
			Assert(chequeBook.AK_CurrentNoInfo.HasErrors());
			AssertHasError(chequeBook.AK_CurrentNoInfo, "Current number must be between start number and last number");

			chequeBook.AK_CurrentNo = 13;
			Assert(chequeBook.AK_CurrentNoInfo.HasErrors());
			AssertHasError(chequeBook.AK_CurrentNoInfo, "Current number must be between start number and last number");
			AssertNoErrorContaining(chequeBook.AK_CurrentNoInfo, "This cheque book is configured as auto cheque and you can only change the current number to a greater number");

			chequeBook.AK_CurrentNo = 11;
			Assert(!chequeBook.AK_CurrentNoInfo.HasErrors());
			AssertNoError(chequeBook.AK_CurrentNoInfo, "Current number must be between start number and last number");
			AssertNoErrorContaining(chequeBook.AK_CurrentNoInfo, "This cheque book is configured as auto cheque and you can only change the current number to a greater number");

			factory.Save();

			chequeBook.AK_CurrentNo = 10;
			Assert(chequeBook.AK_CurrentNoInfo.HasErrors());
			AssertNoError(chequeBook.AK_CurrentNoInfo, "Current number must be between start number and last number");
			AssertHasErrorContaining(chequeBook.AK_CurrentNoInfo, "This cheque book is configured as auto cheque and you can only change the current number to a greater number");
			AssertHasError(chequeBook.AK_CurrentNoInfo, "This cheque book is configured as auto cheque and you can only change the current number to a greater number. Current No: 11");

			chequeBook.AK_CurrentNo = 12;
			Assert(!chequeBook.AK_CurrentNoInfo.HasErrors());
			AssertNoError(chequeBook.AK_CurrentNoInfo, "Current number must be between start number and last number");
			AssertNoErrorContaining(chequeBook.AK_CurrentNoInfo, "This cheque book is configured as auto cheque and you can only change the current number to a greater number");

			ChequeBook.AK_CurrentNo = 11.5;
			Assert("Start number must be an integer", ChequeBook.AK_CurrentNoInfo.HasErrors());

			var chequeBook2 = factory.New<AccChequeBook>();
			chequeBook2.AK_AutoPrintCheque = false;
			chequeBook2.AK_AB = bankAccount.PK;

			chequeBook2.AK_StartNo = 9;
			chequeBook2.AK_LastNo = 12;
			chequeBook2.AK_CurrentNo = 10;

			Assert(!chequeBook.AK_CurrentNoInfo.HasErrors());

			factory.Save();

			chequeBook2.AK_CurrentNo = 9;

			Assert("Even if the check book has a lower current check number, as it is not a auto check book no validation error is found", !chequeBook.AK_CurrentNoInfo.HasErrors());
		}

		public void TestValidateAK_Calc_CurrentNoString()
		{
			ChequeBook.AK_Calc_StartNoString = "9";
			ChequeBook.AK_Calc_LastNoString = "12";
			ChequeBook.AK_Calc_CurrentNoString = "9";
			Assert(!ChequeBook.AK_Calc_CurrentNoStringInfo.HasErrors());

			ChequeBook.AK_Calc_CurrentNoString = "8";
			Assert(ChequeBook.AK_Calc_CurrentNoStringInfo.HasErrors());

			ChequeBook.AK_Calc_CurrentNoString = "13";
			Assert(ChequeBook.AK_Calc_CurrentNoStringInfo.HasErrors());

			ChequeBook.AK_Calc_CurrentNoString = "12";
			Assert(!ChequeBook.AK_Calc_CurrentNoStringInfo.HasErrors());

			ChequeBook.AK_Calc_CurrentNoString = "10";
			Assert(!ChequeBook.AK_Calc_CurrentNoStringInfo.HasErrors());
		}

		public void TestValidateAK_AutoPrintCheque()
		{
			Assert("Should be defaulted to No", !ChequeBook.AK_AutoPrintCheque);
			AccBankAccount testBank = TestFactory.NewWithValidTestData<AccBankAccount>();
			ChequeBook.AK_AutoPrintCheque = ZBool.True;
			ChequeBook.AK_AB = testBank.PK;
			StmTemplate chequeTemplate = TestFactory.NewWithValidTestData<StmTemplate>();

			ChequeBook.Validation.ValidateAK_AutoPrintCheque();
			Assert("IsAutoPrint field should have the ChequeTemplate warning", ChequeBook.AK_AutoPrintChequeInfo.HasWarnings());

			ChequeBook.BankAccount.AB_SO_ChequeTemplate = chequeTemplate.PK;
			ChequeBook.Validation.ValidateAK_AutoPrintCheque();
			Assert("IsAutoPrint should change to true", ChequeBook.IsAutoPrint);
			Assert("IsAutoPrint field should not have the ChequeTemplate warning", !ChequeBook.AK_AutoPrintChequeInfo.HasWarnings());
		}

		public void TestValidateAK_SQ()
		{
			string warningSamePrinterMessage = AccChequeBook.WarningSamePrinterMessageStart + "'Book1'" + AccChequeBook.WarningSamePrinterMessageEnd;

			Assert("Should be no errors so far", !ChequeBook.AK_SQInfo.HasErrors());
			ChequeBook.AK_Code = "Book1";
			ChequeBook.AK_AutoPrintCheque = ZBool.False;
			ChequeBook.Validation.ValidateAK_SQ();
			Assert("Should be no errors so far", !ChequeBook.AK_SQInfo.HasErrors());
			ChequeBook.AK_AutoPrintCheque = ZBool.True;
			ChequeBook.Validation.ValidateAK_SQ();
			Assert("Should be an error as field has become to be mandatory", ChequeBook.AK_SQInfo.HasErrors());

			BusinessObject printer = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			ChequeBook.AK_SQ = printer.PK;
			ChequeBook.Validation.ValidateAK_SQ();
			Assert("Should be no errors as printer now set", !ChequeBook.AK_SQInfo.HasErrors());

			Book2.AK_Code = "Book2";
			Book2.AK_AutoPrintCheque = ZBool.True;
			Book2.AK_SQ = printer.PK;
			Book2.Validation.ValidateAK_SQ();
			Assert("Should have warning about another Cheque Book with the same printer", Book2.AK_SQInfo.HasWarning(warningSamePrinterMessage));
		}

		#endregion
	}
}
