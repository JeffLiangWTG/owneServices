using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgCreditScoresDnBRatingModuleFilterValidation : BusinessObjectValidationTestCase
	{
		public void TestCheckFinancialStrength()
		{
			Filter.FinancialStrength = string.Empty;
			AssertNoErrors(Filter.FinancialStrengthInfo);
			Filter.FinancialStrength = FinancialStrengthList.Codes.FiveA;
			AssertNoErrors(Filter.FinancialStrengthInfo);
			Filter.FinancialStrength = "!!";
			AssertHasError(Filter.FinancialStrengthInfo, "Enter a valid selection.");
			Filter.FinancialStrength = FinancialStrengthList.Codes.G;
			AssertNoErrors(Filter.FinancialStrengthInfo);
		}

		public void TestCheckCreditAppraisal()
		{
			Filter.FinancialStrength = string.Empty;
			AssertNoErrors(Filter.CreditAppraisalInfo);
			Filter.FinancialStrength = CreditAppraisalList.Codes.Strong;
			AssertNoErrors(Filter.CreditAppraisalInfo);
			Filter.CreditAppraisal = "!";
			AssertHasError(Filter.CreditAppraisalInfo, "Enter a valid selection.");
			Filter.CreditAppraisal = CreditAppraisalList.Codes.Limited;
			AssertNoErrors(Filter.CreditAppraisalInfo);
		}

		#region Implementation

		OrgCreditScoresDnBRatingModuleFilter Filter => filter ?? (filter = new OrgCreditScoresDnBRatingModuleFilter("D&B Rating"));
		OrgCreditScoresDnBRatingModuleFilter filter;

		#endregion
	}
}
