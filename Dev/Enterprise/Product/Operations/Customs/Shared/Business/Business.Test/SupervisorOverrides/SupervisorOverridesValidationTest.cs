using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SupervisorOverridesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateProperties()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "U!@";
			company1.GC_Name = "US COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = company1.Branches.AddNew();
			usBranch.GB_Code = "U!@";
			usBranch.GB_BranchName = "US BRANCH";
			usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var declaration = Factory.New<BaseJobDeclaration>();
			var supervisorOverrides = new SupervisorOverridesForTesting(declaration, SupervisorOverridesContext.SavingDeclaration);
			supervisorOverrides.AddMessageLogForTesting("TestMergeByDefault", "Merge By Default", false);
			var message = string.Format(ValidationMessageConstants.UserIsNotSupervisor);
			supervisorOverrides.SupervisorName = ZString.Empty;
			AssertHasErrorContaining(supervisorOverrides.SupervisorNameInfo, ValidationMessageConstants.UserIsMandatory);
			AssertNoErrorContaining(supervisorOverrides.SupervisorNameInfo, ValidationMessageConstants.UserIsNotInList);
			AssertNoError(supervisorOverrides.SupervisorNameInfo, message);

			supervisorOverrides.SupervisorName = "AAA";
			AssertNoErrorContaining(supervisorOverrides.SupervisorNameInfo, ValidationMessageConstants.UserIsMandatory);
			AssertHasErrorContaining(supervisorOverrides.SupervisorNameInfo, ValidationMessageConstants.UserIsNotInList);
			AssertNoError(supervisorOverrides.SupervisorNameInfo, message);

			var newUser = Factory.New<GlbStaff>();
			newUser.FillWithValidTestData();
			newUser.GS_Code = "BBB";
			Factory.Save();

			supervisorOverrides.SupervisorName = newUser.GS_Code;
			AssertNoErrorContaining(supervisorOverrides.SupervisorNameInfo, ValidationMessageConstants.UserIsMandatory);
			AssertNoErrorContaining(supervisorOverrides.SupervisorNameInfo, ValidationMessageConstants.UserIsNotInList);
			newUser.GS_GB_HomeBranch = usBranch.PK;
			supervisorOverrides.Validation.ValidateSupervisorName();
			AssertHasError(supervisorOverrides.SupervisorNameInfo, ValidationMessageConstants.UserNotInCurrentCompany);
			newUser.GS_GB_HomeBranch = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			supervisorOverrides.Validation.ValidateSupervisorName();
			AssertNoErrorContaining(supervisorOverrides.SupervisorNameInfo, ValidationMessageConstants.UserNotInCurrentCompany);
			AssertHasError(supervisorOverrides.SupervisorNameInfo, message);
		}
	}
}
