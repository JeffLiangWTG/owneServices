using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class EstimateSalesAnalysisFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestStatus()
		{
			var analysisFilter = new EstimateSalesAnalysisFilter();

			analysisFilter.Status = "XXX";
			AssertListValidationInvalidCodeError(analysisFilter.StatusInfo, true);

			analysisFilter.Status = OrgSalesActualsStatusList.Codes.Traded;
			AssertListValidationInvalidCodeError(analysisFilter.StatusInfo, false);
		}
	}
}
