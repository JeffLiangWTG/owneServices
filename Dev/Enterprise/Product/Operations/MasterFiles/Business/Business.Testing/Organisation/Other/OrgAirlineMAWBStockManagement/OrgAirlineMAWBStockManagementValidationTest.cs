using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business.Testing
{
	[CodeAlive("See WI00292226")]
	sealed class OrgAirlineMAWBStockManagementValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOHM_MAWBStockThreshold()
		{
			var stockManagement = Factory.New<OrgAirlineMAWBStockManagement>();
			var errorMessage = "The minimum MAWB Threshold value cannot be lower than 1 and the maximum cannot exceed 9999.";

			stockManagement.OHM_MAWBStockThreshold = 0;
			stockManagement.Validation.ValidateOHM_MAWBStockThreshold();
			AssertHasError(stockManagement.OHM_MAWBStockThresholdInfo, errorMessage);

			stockManagement.OHM_MAWBStockThreshold = 10000;
			stockManagement.Validation.ValidateOHM_MAWBStockThreshold();
			AssertHasError(stockManagement.OHM_MAWBStockThresholdInfo, errorMessage);

			stockManagement.OHM_MAWBStockThreshold = 1;
			stockManagement.Validation.ValidateOHM_MAWBStockThreshold();
			AssertNoError(stockManagement.OHM_MAWBStockThresholdInfo, errorMessage);
		}

		public void TestValidateNoDuplicateBranches()
		{
			var errorMessage = "This organization already contains a MAWB configuration for global.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var stockManagement1 = Factory.NewWithValidTestData<OrgAirlineMAWBStockManagement>();
			stockManagement1.OHM_GB_Branch = branch.PK;
			stockManagement1.OHM_OH_Carrier = orgHeader.PK;

			var stockManagement2 = Factory.NewWithValidTestData<OrgAirlineMAWBStockManagement>();
			stockManagement2.OHM_OH_Carrier = orgHeader.PK;
			stockManagement2.OHM_GB_Branch = branch.PK;
			AssertHasError(stockManagement2.OHM_GB_BranchInfo, errorMessage);

			stockManagement2.OHM_GB_Branch = ZGuid.NewZGuid();
			AssertNoError(stockManagement2.OHM_GB_BranchInfo, errorMessage);
		}

		public void TestValidateNoDuplicateBranches_NoDuplicateDefaults()
		{
			var errorMessage = "This organization already contains a MAWB configuration for global.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var stockManagement1 = Factory.NewWithValidTestData<OrgAirlineMAWBStockManagement>();
			stockManagement1.OHM_OH_Carrier = orgHeader.PK;

			var stockManagement2 = Factory.NewWithValidTestData<OrgAirlineMAWBStockManagement>();
			stockManagement2.OHM_OH_Carrier = orgHeader.PK;

			stockManagement2.Validation.ValidateAll();

			AssertHasError(stockManagement2.OHM_GB_BranchInfo, errorMessage);

			stockManagement2.OHM_GB_Branch = branch.PK;
			AssertNoError(stockManagement2.OHM_GB_BranchInfo, errorMessage);
		}
	}
}
