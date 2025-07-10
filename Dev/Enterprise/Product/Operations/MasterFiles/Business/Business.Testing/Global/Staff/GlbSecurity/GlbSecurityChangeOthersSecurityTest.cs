using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbSecurityChangeOthersSecurityTest : TestCaseWithFactory
	{
		GlbGroup groupWeAreTryingToModify;
		GlbStaff staffWeAreTryingToModify;
		GlbStaff staffWithoutModifyPermissions;
		IDisposable userContextChange;
		ZBool savedCacheSetting;

		protected override void SetUp()
		{
			base.SetUp();

			groupWeAreTryingToModify = Factory.NewWithValidTestData<GlbGroup>();
			staffWeAreTryingToModify = Factory.NewWithValidTestData<GlbStaff>();

			staffWithoutModifyPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutModifyPermissions.GS_IsController = false;

			GlbSecurity deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.GroupsModify.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutModifyPermissions.PK;
			staffWithoutModifyPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			GlbSecurity changeGroupSecurity = Factory.New<GlbSecurity>();
			changeGroupSecurity.GU_SecurityRight = GlbSecurity.ChangeOtherGroupSecurityRightName;
			changeGroupSecurity.GU_GS = staffWithoutModifyPermissions.PK;
			changeGroupSecurity.GU_SecurityItemIsAllowed = true;
			changeGroupSecurity.GU_ItemGUID = groupWeAreTryingToModify.PK;

			GlbSecurity changeStaffSecurity = Factory.New<GlbSecurity>();
			changeStaffSecurity.GU_SecurityRight = GlbSecurity.ChangeOtherGroupSecurityRightName;
			changeStaffSecurity.GU_GS = staffWithoutModifyPermissions.PK;
			changeStaffSecurity.GU_SecurityItemIsAllowed = true;
			changeStaffSecurity.GU_ItemGUID = staffWeAreTryingToModify.PK;

			Factory.Save();
			savedCacheSetting = Env.Security.CachingEnabled;

			Env.Security.CachingEnabled = false;
			userContextChange = Env.SetTemporaryUserContext(staffWithoutModifyPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			AssertEquals("Precondition: Current user has changed to staff without permissions", staffWithoutModifyPermissions.PK, Env.CurrentUser.PK);
		}

		protected override void TearDown()
		{
			userContextChange.Dispose();
			Env.Security.CachingEnabled = savedCacheSetting;

			base.TearDown();
		}

		public void TestReadOnlyRecordsAreNotValidated()
		{
			GlbSecurity security = staffWeAreTryingToModify.GroupSecurityPermissionsCollectionForBinding.AddNew();
			security.GU_GG = groupWeAreTryingToModify.PK;
			security.GU_SecurityRight = "Moo";
			security.GU_SecurityItemIsAllowed = true;
			AssertNoErrors(security.GU_SecurityItemIsAllowedInfo);
		}

		public void TestSecurityRightForInvalidDetails()
		{
			GlbSecurity security = groupWeAreTryingToModify.SecurityPermissions.AddNew();
			security.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertHasError(security.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(security));
		}

		public void TestSecurityRightsForGroupCannotBeGrantedIfCurrentUserDoesNotHaveThem()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.Receivables.Code;
			usersSecurity.GU_SecurityItemIsAllowed = false;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = groupWeAreTryingToModify.SecurityPermissions.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = usersSecurity.GU_SecurityRight;
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertHasError(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(securityAttemptingToGrant));

			securityAttemptingToGrant.GU_SecurityItemIsAllowed = false;
			AssertHasError(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(securityAttemptingToGrant));
		}

		public void TestSecurityRightsForGroupCanBeGrantedIfCurrentUserHasThem()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.Receivables.Code;
			usersSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = groupWeAreTryingToModify.SecurityPermissions.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = usersSecurity.GU_SecurityRight;
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);
		}

		public void TestSecurityRightsForGroupCanBeGrantedIfCurrentUserHasThem_ForEditSpecificSettingsChildCheckpoints()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.SystemRegistryEditSpecificSettings.Code;
			usersSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = groupWeAreTryingToModify.SecurityPermissions.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = "RegCatAccounting";
			securityAttemptingToGrant.GU_ItemGUID = new ZGuid();
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);
		}

		public void TestSecurityRightsForGroupCanBeGrantedIfCurrentUserHasThem_ForAllowedWarehousesSecurityRightName()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.WhsAllowedWarehouses.Code;
			usersSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = groupWeAreTryingToModify.SecurityPermissions.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = "AllowedWarehousesSecurityRightName";
			securityAttemptingToGrant.GU_ItemGUID = new ZGuid();
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);
		}

		public void TestSecurityRightsForGroupCanBeGrantedIfCurrentUserHasThem_ForAllowedPrincipalsSecurityRightName()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.AgencyPrincipalAccess.Code;
			usersSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = groupWeAreTryingToModify.SecurityPermissions.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = "AllowedPrincipalsSecurityRightName";
			securityAttemptingToGrant.GU_ItemGUID = new ZGuid();
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);
		}

		public void TestSecurityRightsForGroupCanBeGrantedIfCurrentUserHasThem_ForAllowedClientsSecurityRightName()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.WhsAllowedClients.Code;
			usersSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = groupWeAreTryingToModify.SecurityPermissions.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = "AllowedClientsSecurityRightName";
			securityAttemptingToGrant.GU_ItemGUID = new ZGuid();
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);
		}

		public void TestSecurityRightsForGroupCanBeGrantedIfCurrentUserIsNonOperational()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			var originalOperational = ((GlbStaff)Env.CurrentUser).GS_IsOperational;
			((GlbStaff)Env.CurrentUser).GS_IsOperational = false;
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.Receivables.Code;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = groupWeAreTryingToModify.SecurityPermissions.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = usersSecurity.GU_SecurityRight;
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);

			securityAttemptingToGrant.GU_SecurityItemIsAllowed = false;
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);

			((GlbStaff)Env.CurrentUser).GS_IsOperational = originalOperational;
		}

		public void TestSecurityRightsForStaffCannotBeGrantedIfCurrentUserDoesNotHaveThem()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.Receivables.Code;
			usersSecurity.GU_SecurityItemIsAllowed = false;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = staffWeAreTryingToModify.StaffSecurityPermissionsCollection.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = usersSecurity.GU_SecurityRight;
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertHasError(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(securityAttemptingToGrant));

			securityAttemptingToGrant.GU_SecurityItemIsAllowed = false;
			AssertHasError(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(securityAttemptingToGrant));
		}

		public void TestSecurityRightsForStaffCanBeGrantedIfCurrentUserHasThem()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.Receivables.Code;
			usersSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = staffWeAreTryingToModify.StaffSecurityPermissionsCollection.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = usersSecurity.GU_SecurityRight;
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);
		}

		public void TestSecurityRightsForStaffCanBeGrantedIfCurrentUserIsNonOperational()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			var originalOperational = ((GlbStaff)Env.CurrentUser).GS_IsOperational;
			((GlbStaff)Env.CurrentUser).GS_IsOperational = false;
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityRight = Env.Security.Receivables.Code;

			Factory.Save();

			GlbSecurity securityAttemptingToGrant = staffWeAreTryingToModify.StaffSecurityPermissionsCollection.AddNew();
			securityAttemptingToGrant.GU_SecurityItemIsAllowed = true;
			securityAttemptingToGrant.GU_SecurityRight = usersSecurity.GU_SecurityRight;
			securityAttemptingToGrant.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);

			securityAttemptingToGrant.GU_SecurityItemIsAllowed = false;
			AssertNoErrors(securityAttemptingToGrant.GU_SecurityItemIsAllowedInfo);

			((GlbStaff)Env.CurrentUser).GS_IsOperational = originalOperational;
		}

		public void TestSecurityRightIsOnlyValidatedIfItHasChanged()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();

			GlbBranch branch1 = company1.Branches.AddNew();
			company1.Branches.AdditionalFilter = new ZQuery(GlbBranchSchema.PK, branch1.PK);
			GlbBranch branch2 = company2.Branches.AddNew();
			company1.Branches.AdditionalFilter.AddToFilter(JoinCondition.And, GlbBranchSchema.PK, branch2.PK);

			branch1.FillWithValidTestData();
			branch2.FillWithValidTestData();

			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			GlbSecurity userSecurity1 = Factory.New<GlbSecurity>();
			GlbSecurity userSecurity2 = Factory.New<GlbSecurity>();

			userSecurity1.GU_GS = Env.CurrentUser.PK;
			userSecurity1.GU_SecurityRight = Env.Security.Receivables.Code;
			userSecurity1.GU_SecurityItemIsAllowed = false;

			userSecurity2.GU_GS = Env.CurrentUser.PK;
			userSecurity2.GU_GC = company1.PK;
			userSecurity2.GU_GE = department1.PK;
			userSecurity2.GU_SecurityRight = Env.Security.Receivables.Code;
			userSecurity2.GU_SecurityItemIsAllowed = true;

			GlbSecurity existingSecurity = staffWeAreTryingToModify.StaffSecurityPermissionsCollection.AddNew();

			existingSecurity.GU_GC = company2.PK;
			existingSecurity.GU_GE = department2.PK;
			existingSecurity.GU_SecurityRight = Env.Security.Receivables.Code;
			existingSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			existingSecurity.GU_SecurityItemIsAllowed = false;
			AssertHasError(existingSecurity.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(existingSecurity));

			existingSecurity.GU_SecurityItemIsAllowed = true;
			AssertNoErrors(existingSecurity.GU_SecurityItemIsAllowedInfo);

			existingSecurity.GU_GC = company1.PK;
			existingSecurity.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertHasError(existingSecurity.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(existingSecurity));

			existingSecurity.GU_GC = company2.PK;
			existingSecurity.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(existingSecurity.GU_SecurityItemIsAllowedInfo);

			existingSecurity.GU_GB = branch1.PK;
			existingSecurity.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertHasError(existingSecurity.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(existingSecurity));

			existingSecurity.GU_GB = ZGuid.Empty;
			existingSecurity.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(existingSecurity.GU_SecurityItemIsAllowedInfo);

			existingSecurity.GU_GE = department1.PK;
			existingSecurity.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertHasError(existingSecurity.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(existingSecurity));

			existingSecurity.GU_GE = department2.PK;
			existingSecurity.Validation.ValidateGU_SecurityItemIsAllowed();
			AssertNoErrors(existingSecurity.GU_SecurityItemIsAllowedInfo);
		}

		public void TestSecurityRight_DepartmentIsValidatedIfCompanyOrBranchHasChanged()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var userSecurity = Factory.New<GlbSecurity>();

			userSecurity.DepartmentCode = department.GE_Code;
			AssertHasErrors("Both CompanyCode and BranchCode are null, DepartmentCode validation failed", userSecurity.DepartmentCodeInfo);

			userSecurity.CompanyCode = company.GC_Code;
			AssertNoErrors("CompanyCode is not null, DepartmentCode validation passed", userSecurity.DepartmentCodeInfo);

			userSecurity.CompanyCode = ZString.Empty;
			AssertHasErrors("Both CompanyCode and BranchCode are null, DepartmentCode validation failed", userSecurity.DepartmentCodeInfo);

			userSecurity.BranchCode = branch.GB_Code;
			AssertNoErrors("BranchCode is not null, DepartmentCode validation passed", userSecurity.DepartmentCodeInfo);

			userSecurity.CompanyCode = company.GC_Code;
			AssertNoErrors("Both CompanyCode and BranchCode are not null, DepartmentCode validation passed", userSecurity.DepartmentCodeInfo);

			userSecurity.CompanyCode = ZString.Empty;
			userSecurity.BranchCode = ZString.Empty;
			AssertHasErrors("Both CompanyCode and BranchCode are null, DepartmentCode validation failed", userSecurity.DepartmentCodeInfo);
		}

		public void TestSecurityRightValidationForGroupOwner()
		{
			GlbSecurity usersSecurity = Factory.New<GlbSecurity>();
			usersSecurity.GU_GS = Env.CurrentUser.PK;
			usersSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			var validation = new GlbSecurityValidation(usersSecurity);
			AssertHasError(usersSecurity.GU_SecurityItemIsAllowedInfo, GlbSecurityValidation.CannotGrantOthersRightsYouDoNotHaveYourself(usersSecurity));

			usersSecurity.GU_SecurityRight = GlbSecurity.GroupOwnerSecurityRightName;
			validation = new GlbSecurityValidation(usersSecurity);
			AssertNoErrors(usersSecurity.GU_SecurityItemIsAllowedInfo);
		}
	}
}
