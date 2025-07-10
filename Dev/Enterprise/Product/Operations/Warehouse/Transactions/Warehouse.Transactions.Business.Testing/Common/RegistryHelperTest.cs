using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class RegistryHelperTest : WhsTestCaseWithFactory
	{
		#region TestIJobInvoicingPlugIn_OperationsBranch_BasedOnRegistrySettings

		public void TestIJobInvoicingPlugIn_OperationsBranch_BasedOnRegistrySettings()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchAUSYD = company.Branches.AddNew();
			branchAUSYD.GB_Code = "AUS";
			branchAUSYD.GB_RL_NKHomePort = "AUSYD";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var client = Factory.NewWithValidTestData<OrgHeader>();
				var clientBranch = Factory.NewWithValidTestData<GlbBranch>();
				client.CompanyData.OB_GB_ControllingBranch = clientBranch.PK;

				var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				var warehouseBranch = Factory.NewWithValidTestData<GlbBranch>();
				warehouse.WW_GB_RelatedCompanyBranch = warehouseBranch.PK;

				Factory.Save();

				// Default to blank
				SetUpRegistry(1, 0, 0, 0);
				AssertNull("Operations Branch should be Null.", RegistryHelper.GetBillingOperationsBranch(warehouse, client));

				// Default to Warehouse Branch
				SetUpRegistry(0, 1, 0, 0);
				AssertEquals("Operations Branch should be Warehouse's Branch.", warehouseBranch, RegistryHelper.GetBillingOperationsBranch(warehouse, client));

				// Default to Organisation Branch
				SetUpRegistry(0, 0, 1, 0);
				AssertEquals("Operations Branch should be Client's Branch.", clientBranch, RegistryHelper.GetBillingOperationsBranch(warehouse, client));

				// Default to Login User Default
				SetUpRegistry(0, 0, 0, 1);
				AssertEquals("Operations Branch should be Login user's current branch.", branchAUSYD.PK, RegistryHelper.GetBillingOperationsBranch(warehouse, client).PK);

				// Default to Warehouse Branch, but however it's inactive, so should return to next level which is Organisation Branch
				warehouseBranch.GB_IsActive = false;
				SetUpRegistry(0, 1, 2, 3);
				AssertEquals("Operations Branch should be Client's Branch if warehouse's branch is inactive.", clientBranch, RegistryHelper.GetBillingOperationsBranch(warehouse, client));
			}
		}

		void SetUpRegistry(ZShort defaultToBlank, ZShort defaultToBranchRelatedToPortOrWarehouseBranch,
			ZShort defaultToBranchOfOrganisation, ZShort defaultToLoginUserDefault)
		{
			var rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = defaultToBlank;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = defaultToBranchRelatedToPortOrWarehouseBranch;
			rule.DefaultToBranchOfOrganisation = defaultToBranchOfOrganisation;
			rule.DefaultToLoginUserDefault = defaultToLoginUserDefault;

			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
		}

		#endregion
	}
}
