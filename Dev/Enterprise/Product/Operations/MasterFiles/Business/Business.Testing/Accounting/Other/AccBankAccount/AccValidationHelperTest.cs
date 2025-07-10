using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidateChequeDigits()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 5;

			var chequeBook = Factory.New<AccChequeBook>();
			chequeBook.AK_AB = bankAccount.PK;

			chequeBook.AK_StartNo = 12345;
			AssertNoErrors("Valid cheque number", chequeBook.AK_StartNoInfo);

			chequeBook.AK_StartNo = 1234;
			AssertNoErrors("Valid cheque number", chequeBook.AK_StartNoInfo);

			chequeBook.AK_StartNo = 123456;
			AssertHasErrors("Invalid cheque number", chequeBook.AK_StartNoInfo);
		}

		public void TestPadChequeDigitsWithLeadingZeros()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 5;

			var chequeBook = Factory.New<AccChequeBook>();
			chequeBook.AK_AB = bankAccount.PK;

			chequeBook.AK_StartNo = 1;
			AssertEquals("Cheque number should be 00001", "00001", chequeBook.AK_Calc_CurrentNoString);

			chequeBook.AK_StartNo = 12345;
			AssertEquals("Cheque number should be 12345", "12345", chequeBook.AK_Calc_CurrentNoString);
		}

		public void TestCheckBankSWIFT()
		{
			Assert(!AccValidationHelper.CheckBankSWIFT("123456ABSSS"));
			Assert(!AccValidationHelper.CheckBankSWIFT("ABCDE1234LLL"));
			Assert(!AccValidationHelper.CheckBankSWIFT("ABCDEF12X8888"));
			Assert(!AccValidationHelper.CheckBankSWIFT("ABCDEF1O888"));
			Assert(!AccValidationHelper.CheckBankSWIFT("ABCDEF1X888"));
			Assert(AccValidationHelper.CheckBankSWIFT("ABCDEF2X888"));
		}
	}
}
