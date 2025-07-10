using System;
using System.Linq;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	class SupplyTypeConfigurationLookupsTest : JobConfigurationSelectorLookupsTest
	{
		public void TestIncotermList()
		{
			BizObj.JobType = JobTypesList.Codes.SHP;

			var testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var taxOverride = testChargeCode.TaxOverrides.AddNew();
			var accChargeTaxOverrideLookups = new AccChargeTaxOverrideLookups(taxOverride);

			AssertEquals("IncotermList Count is same as IncotermList in ChargeTaxOverride.", accChargeTaxOverrideLookups.Incoterms.Count, supplyTypeConfigurationLookups.IncotermList.Count);
			AssertContainsExactElementsInAnyOrder("IncotermList is same as IncotermList in ChargeTaxOverride.", accChargeTaxOverrideLookups.Incoterms.GetAllCodes(), supplyTypeConfigurationLookups.IncotermList.GetAllCodes());
		}

		public void TestDepartmentList()
		{
			BizObj.JobType = JobTypesList.Codes.SHP;

			var testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode.AC_GC = CompanyPK;
			var chargeGLPostingOverride = testChargeCode.GLPostingOverrides.AddNew();
			Factory.Save();

			var chargeGLPostingOverrideDepartmentCodeList = chargeGLPostingOverride.Lookups.Departments.Select(x => x.GE_Code);
			var supplyTypeConfigurationDepartmentCodeList = supplyTypeConfigurationLookups.DepartmentList.Select(x => x.GE_Code);

			AssertContainsExactElementsInAnyOrder(chargeGLPostingOverrideDepartmentCodeList, supplyTypeConfigurationDepartmentCodeList);
		}

		public void TestSupplyTypeList()
		{
			BizObj.JobType = JobTypesList.Codes.SHP;
			var registryCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			if (BizObj is SupplyTypeConfiguration)
			{
				registryCompanyPK = CompanyPK;
			}

			var collection = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.GetFallBackValueAtAllLevels(registryCompanyPK, Guid.Empty, Guid.Empty);
			foreach (CodeDescriptionBool code in collection)
			{
				code.Bool = true;
			}

			AssertContainsExactElementsInAnyOrder(
				new string[] { "LOX", "LOA", "INX", "INA", "DSB", "LOC", "INT" },
				supplyTypeConfigurationLookups.SupplyTypeList.GetAllCodes()
			);

			((CodeDescriptionBool)collection.FindByCode("LOC")).Bool = false;
			((CodeDescriptionBool)collection.FindByCode("INT")).Bool = false;
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetValue(registryCompanyPK, Guid.Empty, Guid.Empty, collection);

			AssertContainsExactElementsInAnyOrder(
				new string[] { "LOX", "LOA", "INX", "INA", "DSB" },
				supplyTypeConfigurationLookups.SupplyTypeList.GetAllCodes()
			);
		}

		#region Implementation

		protected new ISupplyTypeSelector BizObj
		{
			get { return (ISupplyTypeSelector)base.BizObj; }
			set { base.BizObj = value; }
		}

		protected override void SetUp()
		{
			CompanyPK = Factory.NewWithValidTestData<GlbCompany>().PK.ToGuid();
			BranchPK = Factory.NewWithValidTestData<GlbBranch>().PK.ToGuid();
			DepartmentPK = Factory.NewWithValidTestData<GlbDepartment>().PK.ToGuid();

			base.SetUp();
		}

		protected override IJobConfigurationSelector GetNewBizObj => new SupplyTypeConfiguration(new FallbackLevel(CompanyPK, BranchPK, DepartmentPK));

		SupplyTypeConfigurationLookups fSupplyTypeConfigurationLookups;
		SupplyTypeConfigurationLookups supplyTypeConfigurationLookups => fSupplyTypeConfigurationLookups ?? (fSupplyTypeConfigurationLookups = new SupplyTypeConfigurationLookups(BizObj));

		Guid CompanyPK;

		Guid BranchPK;

		Guid DepartmentPK;

		#endregion
	}
}
