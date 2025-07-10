using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccGLHeaderSubAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateASA_SubClass()
		{
			var header = Factory.NewWithValidTestData<AccGLHeader>();
			var subAccountTypes1 = header.SubAccountTypes.AddNew();
			var subAccountTypes2 = header.SubAccountTypes.AddNew();

			subAccountTypes1.ASA_SubClassDisplayName = "";
			subAccountTypes2.ASA_SubClassDisplayName = "123";
			AssertHasErrorContaining(subAccountTypes1.ASA_SubClassDisplayNameInfo, "Please enter a");
			AssertHasErrorContaining(subAccountTypes2.ASA_SubClassDisplayNameInfo, "Enter a valid");

			subAccountTypes1.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;
			subAccountTypes2.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;
			AssertNoErrorContaining(subAccountTypes1.ASA_SubClassDisplayNameInfo, "Please enter a");
			AssertNoErrorContaining(subAccountTypes2.ASA_SubClassDisplayNameInfo, "Enter a valid");
			AssertHasError(subAccountTypes2.ASA_SubClassDisplayNameInfo, "This Sub Account Type is already defined");

			subAccountTypes2.ASA_SubClassDisplayName = Core.Constants.SubAccountType.StaffGroup;
			AssertNoError(subAccountTypes2.ASA_SubClassDisplayNameInfo, "This Sub Account Type is already defined");

			header.AG_AccountType = Core.Constants.AccountType.Consolidation;
			header.RunPreSaveValidation();
			AssertHasErrorContaining(subAccountTypes1.ASA_SubClassDisplayNameInfo, "Sub Account is only applicable for 'P&L' and 'BSH' type GL Account.");
			AssertHasErrorContaining(subAccountTypes2.ASA_SubClassDisplayNameInfo, "Sub Account is only applicable for 'P&L' and 'BSH' type GL Account.");

			var subAccountTypes3 = header.SubAccountTypes.AddNew();
			subAccountTypes3.ASA_SubClassDisplayName = "";
			AssertHasErrorContaining(subAccountTypes3.ASA_SubClassDisplayNameInfo, "Sub Account is only applicable for 'P&L' and 'BSH' type GL Account.");
		}
	}
}
