using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBranchDepartmentCombinationValidation_JobHeaderValidation()
		{
			var bizObj = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { bizObj.JH_GB = branch; bizObj.JH_GE = department; }, bizObj.JH_GEInfo);
		}

		public void TestCheckJH_GB_TaxBranch()
		{
			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var nonCurrentCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			nonCurrentCompanyBranch.GB_GC = nonCurrentCompany.PK;

			var currentCompanyUnActiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyUnActiveBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyUnActiveBranch.GB_IsActive = false;
			Factory.Save();

			AssertCheckJH_GB_TaxBranch(true);
			AssertCheckJH_GB_TaxBranch(false);

			void AssertCheckJH_GB_TaxBranch(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					var header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

					header.JH_GB_TaxBranch = ZGuid.Empty;
					AssertEquals(enableTaxBranchReporting, header.JH_GB_TaxBranchInfo.HasError("Please enter a Job Tax Branch."));

					header.JH_GB_TaxBranch = nonCurrentCompanyBranch.PK;
					AssertEquals(enableTaxBranchReporting, header.JH_GB_TaxBranchInfo.HasError("Enter a valid Job Tax Branch."));

					header.JH_GB_TaxBranch = currentCompanyUnActiveBranch.PK;
					AssertEquals(enableTaxBranchReporting, header.JH_GB_TaxBranchInfo.HasError("Enter a valid Job Tax Branch."));

					header.JH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
					AssertEquals(false, header.JH_GB_TaxBranchInfo.HasErrors());

					header.JH_GB_TaxBranch = ZGuid.Invalid;
					AssertEquals(true, header.JH_GB_TaxBranchInfo.HasError("Enter a valid Job Tax Branch."));
				}
			}
		}
	}
}
