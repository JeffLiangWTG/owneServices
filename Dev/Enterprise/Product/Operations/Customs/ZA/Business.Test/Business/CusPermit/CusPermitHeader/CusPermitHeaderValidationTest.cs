using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusPermitHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCPH_EndDate()
		{
			permitHeader.CPH_StartDate = ZDate.Today;
			permitHeader.CPH_EndDate = permitHeader.CPH_StartDate.AddDays(-1);
			AssertHasError(permitHeader.CPH_EndDateInfo, CusPermitHeaderValidation.EndDateBeforeStartDate);
			permitHeader.CPH_EndDate = permitHeader.CPH_StartDate;
			AssertNoError(permitHeader.CPH_EndDateInfo, CusPermitHeaderValidation.EndDateBeforeStartDate);
			permitHeader.CPH_EndDate = ZDate.Empty;
			AssertHasErrorContaining(permitHeader.CPH_EndDateInfo, MandatoryValidation.MustBeEntered);
			permitHeader.CPH_StartDate = ZDate.Empty;
			permitHeader.CPH_EndDate = ZDate.Today;
			AssertNoErrors(permitHeader.CPH_EndDateInfo);
		}

		public void TestCheckCPH_SubType()
		{
			permitHeader.CPH_Type = PermitTypeList.Codes.RCC;
			permitHeader.CPH_SubType = ZString.Empty;
			AssertHasErrorContaining(permitHeader.CPH_SubTypeInfo, MandatoryValidation.MustBeEntered);
			permitHeader.CPH_Type = PermitTypeList.Codes.PRC;
			permitHeader.CPH_SubType = ZString.Empty;
			AssertHasErrorContaining(permitHeader.CPH_SubTypeInfo, MandatoryValidation.MustBeEntered);
			permitHeader.CPH_Type = PermitTypeList.Codes.VALA;
			permitHeader.CPH_SubType = ZString.Empty;
			AssertHasErrorContaining(permitHeader.CPH_SubTypeInfo, MandatoryValidation.MustBeEntered);
			permitHeader.CPH_SubType = PermitSubTypeList.Codes.ACO;
			AssertNoErrorContaining(permitHeader.CPH_SubTypeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCPH_Number()
		{
			permitHeader.CPH_Type = PermitTypeList.Codes.PRC;
			permitHeader.CPH_Number = "ACO123";
			AssertHasError(permitHeader.CPH_NumberInfo, "Permit Number cannot begin with ACO or ATO or LEG or LVE or MHV.");
			permitHeader.CPH_Type = PermitTypeList.Codes.VALA;
			permitHeader.Validation.ValidateCPH_Number();
			AssertHasError(permitHeader.CPH_NumberInfo, "Permit Number cannot begin with ACO or ATO or LEG or LVE or MHV.");
			permitHeader.CPH_Number = "AAA123";
			AssertNoError(permitHeader.CPH_NumberInfo, "Permit Number cannot begin with ACO or ATO or LEG or LVE or MHV.");
		}

		public void TestCheckCPH_StartDate()
		{
			permitHeader.CPH_StartDate = ZDate.Today;
			permitHeader.CPH_Type = PermitTypeList.Codes.VALA;
			var transaction = permitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero);
			transaction.FillWithValidTestData();
			transaction.CPL_TransactionDate = ZDate.Today.AddDays(2);
			transaction.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			var transaction2 = permitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero);
			transaction2.FillWithValidTestData();
			transaction2.CPL_TransactionDate = ZDate.Today.AddDays(1);
			transaction2.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.CUS;
			permitHeader.Validation.ValidateCPH_StartDate();
			AssertNoError(permitHeader.CPH_StartDateInfo, "Start Date Cannot be later than the earliest Transaction Date.");
			transaction.CPL_TransactionDate = ZDate.Today.AddDays(-4);
			permitHeader.Validation.ValidateCPH_StartDate();
			AssertHasError(permitHeader.CPH_StartDateInfo, "Start Date Cannot be later than the earliest Transaction Date.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			permitHeader = Factory.New<CusPermitHeader>();
		}

		CusPermitHeader permitHeader;
	}
}
