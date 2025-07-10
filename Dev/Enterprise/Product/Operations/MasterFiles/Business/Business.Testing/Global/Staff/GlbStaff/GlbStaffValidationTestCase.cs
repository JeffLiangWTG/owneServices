using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class GlbStaffValidationTestCase : BusinessObjectValidationTestCase
	{
		public void TestValidateGS_LoginName_ADEnabledButNoPrefixInRegistry()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().UserLoginPrefix = string.Empty;

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Jim.Moriarty";

			AssertNoErrors(staff.GS_LoginNameInfo);
		}

		public void TestCheckGS_EmailAddress_InvalidEmail()
		{
			var staff = Factory.New<GlbStaff>();

			staff.GS_EmailAddress = "blahblah@gmail.com";
			AssertNoErrors(staff.GS_EmailAddressInfo);

			staff.GS_EmailAddress = "<blahblah@gmail.com>";
			AssertHasErrorContaining(staff.GS_EmailAddressInfo, "<blahblah@gmail.com> is not a valid email address.");
		}

		public void TestValidateGS_LoginName_ADEnabledLoginNamePrefixInRegistry()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().UserLoginPrefix = "EDI-PRD-";

			var staff = GetNewBusinessObject();
			staff.GS_LoginName = "Jim.Moriarty";
			AssertEquals(ShouldHaveADRelatedLoginNameErrors, staff.GS_LoginNameInfo.HasError("Login Name must begin with 'EDI-PRD-'."));

			staff.GS_IsSystemAccount = true;
			staff.RunPreSaveValidation();
			Assert(!staff.GS_LoginNameInfo.HasErrors());

			staff.GS_IsSystemAccount = false;
			staff.RunPreSaveValidation();
			AssertEquals(ShouldHaveADRelatedLoginNameErrors, staff.GS_LoginNameInfo.HasError("Login Name must begin with 'EDI-PRD-'."));

			staff.GS_CanLogin = false;
			staff.RunPreSaveValidation();
			Assert(!staff.GS_LoginNameInfo.HasErrors());

			staff.GS_CanLogin = true;
			staff.RunPreSaveValidation();
			AssertEquals(ShouldHaveADRelatedLoginNameErrors, staff.GS_LoginNameInfo.HasError("Login Name must begin with 'EDI-PRD-'."));

			staff.GS_LoginName = "EDI-prd-Jim.Moriarty";
			Assert(!staff.GS_LoginNameInfo.HasErrors());
		}

		protected abstract bool ShouldHaveADRelatedLoginNameErrors { get; }

		public void TestValidateGS_LoginName_InvalidCharacters()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Valid.Username";
			AssertNoErrors("Login name should have no errors", staff.GS_LoginNameInfo);

			staff.GS_LoginName = "Username.with \" a_quote";
			AssertHasError("Should be invalid", staff.GS_LoginNameInfo, "Login Name can not contain any of these symbols: \" / \\ [ ] : ; | = , + * ? < >");

			staff.GS_LoginName = "User[2]";
			AssertHasError("Should be invalid", staff.GS_LoginNameInfo, "Login Name can not contain any of these symbols: \" / \\ [ ] : ; | = , + * ? < >");
		}

		public void TestValidateGS_LoginName_ControlCharacters()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Valid.Username";
			AssertNoErrors("Login name should have no errors", staff.GS_LoginNameInfo);

			staff.GS_LoginName = "User\rName";
			AssertHasError("Should be invalid", staff.GS_LoginNameInfo, "Login Name can not contain control characters, such as line break or tab.");

			staff.GS_LoginName = "User\tName";
			AssertHasError("Should be invalid", staff.GS_LoginNameInfo, "Login Name can not contain control characters, such as line break or tab.");

			staff.GS_LoginName = "User\nName";
			AssertHasError("Should be invalid", staff.GS_LoginNameInfo, "Login Name can not contain control characters, such as line break or tab.");

			staff.GS_LoginName = @"User
Name";
			AssertHasError("Should be invalid", staff.GS_LoginNameInfo, "Login Name can not contain control characters, such as line break or tab.");
		}

		public void TestValidateGS_NextOfKinRelationship()
		{
			Bizo.GS_NextOfKinRelationship = Bizo.Lookups.Relationships[0].Code;
			AssertNoErrors(Bizo.GS_NextOfKinRelationshipInfo);

			Bizo.GS_NextOfKinRelationship = "XYZ";
			AssertHasErrors(Bizo.GS_NextOfKinRelationshipInfo);
		}

		public void TestValidateGS_EmergencyContactRelationship()
		{
			Bizo.GS_EmergencyContactRelationship = Bizo.Lookups.Relationships[0].Code;
			AssertNoErrors(Bizo.GS_EmergencyContactRelationshipInfo);

			Bizo.GS_EmergencyContactRelationship = "XYZ";
			AssertHasErrors(Bizo.GS_EmergencyContactRelationshipInfo);
		}

		public void TestValidateGS_CommissionBasis()
		{
			Bizo.GS_CommissionBasis = ZString.Empty;
			AssertNoErrors(Bizo.GS_CommissionBasisInfo);

			Bizo.GS_CommissionBasis = new ZString("TST");
			AssertHasErrors(Bizo.GS_CommissionBasisInfo);

			Bizo.GS_CommissionBasis = CommissionBasisType.Codes.PRF;
			AssertNoErrors(Bizo.GS_CommissionBasisInfo);

			Bizo.GS_CommissionBasis = new ZString("NON");
			AssertHasErrors(Bizo.GS_CommissionBasisInfo);

			Bizo.GS_CommissionBasis = CommissionBasisType.Codes.REV;
			AssertNoErrors(Bizo.GS_CommissionBasisInfo);
		}

		public void TestValidateTwoFactorAuthentication()
		{
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "JBA";
			staff.GS_LoginName = "test";
			Factory.Save();

			staff.GS_IsTwoFactorAuthenticationEnabled = true;

			staff.GS_EmailAddress = "valid@test.com";
			AssertNoErrors(staff.GS_EmailAddressInfo);

			staff.GS_EmailAddress = ZString.Empty;
			AssertHasError(staff.GS_EmailAddressInfo, "An email address is required to enable two factor authentication.");
		}

		public void TestValidateStaffScheduleTaskRecipient()
		{
			Bizo.GS_Code = "ZAC";
			Bizo.GS_LoginName = "BIZ";
			Bizo.GS_EmailAddress = ZString.Empty;
			AssertNoErrors(Bizo.GS_EmailAddressInfo);

			Bizo.GS_EmailAddress = "test@test";
			AssertHasErrors("Can not set email address to an invalid value", Bizo.GS_EmailAddressInfo);

			Bizo.GS_EmailAddress = "test@test.com";
			AssertNoErrors(Bizo.GS_EmailAddressInfo);

			var scheduleTaskRecipient = (IStmScheduleTaskRecipient)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTaskRecipient>());
			scheduleTaskRecipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			scheduleTaskRecipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			scheduleTaskRecipient.S6_GS_NKRecipient = Bizo.GS_Code;

			Factory.Save();

			Bizo.GS_EmailAddress = "test@test.com";
			AssertNoErrorContaining(Bizo.GS_EmailAddressInfo, "Email cannot set to empty because staff is assigned to recipient of scheduled reports.");

			Bizo.GS_EmailAddress = ZString.Empty;
			AssertHasError(Bizo.GS_EmailAddressInfo, "Email cannot set to empty because staff is assigned to recipient of scheduled reports.");
		}

		public void TestValidateGroupScheduleTaskRecipient()
		{
			var group = Factory.New<GlbGroup>();
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "ST1";
			group.Staff.Add(staff1);

			var scheduleTaskRecipient = (IStmScheduleTaskRecipient)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTaskRecipient>());
			scheduleTaskRecipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			scheduleTaskRecipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			scheduleTaskRecipient.S6_GG = group.PK;

			Factory.Save();

			staff1.GS_EmailAddress = "test@test.com";
			AssertNoErrorContaining(staff1.GS_EmailAddressInfo, "Email cannot set to empty because staff is assigned to group recipient of scheduled reports.");

			staff1.GS_EmailAddress = ZString.Empty;
			AssertHasError(staff1.GS_EmailAddressInfo, "Email cannot set to empty because staff is assigned to group recipient of scheduled reports.");

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "ST2";
			staff2.GS_EmailAddress = "test@test.com";
			group.Staff.Add(staff2);

			Factory.Save();

			staff1.GS_EmailAddress = "test@test.com";
			staff1.GS_EmailAddress = ZString.Empty;
			AssertNoErrorContaining(staff1.GS_EmailAddressInfo, "Email cannot set to empty because staff is assigned to group recipient of scheduled reports.");
		}

		public void TestValidateGS_Code()
		{
			Bizo.GS_Code = "$$$";
			GlbStaff bizo2 = Factory.New<GlbStaff>();

			bizo2.GS_Code = "$$$";
			AssertHasError(bizo2.GS_CodeInfo, "Code must be unique in the system");

			bizo2.GS_Code = "";
			AssertNoErrors("First available code will be used even if Full Name is not specified", bizo2.GS_CodeInfo);

			bizo2.GS_FullName = "John Smith";
			bizo2.Validation.ValidateGS_Code();
			AssertNoErrors(bizo2.GS_CodeInfo);
		}

		public void TestValidateGS_FullName()
		{
			Bizo.GS_FullName = "";
			Assert("GS_FullName is empty, expecting error", Bizo.GS_FullNameInfo.HasErrors());

			Bizo.GS_FullName = "Hello";
			Assert("GS_FullName is not empty, not expecting error", !Bizo.GS_FullNameInfo.HasNotifications());
		}

		public void TestValidateGS_CountryCode()
		{
			ZString viewDeniedMessage = "** View Denied due to Security Access **";

			var staffWithoutHomeAddressPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutHomeAddressPermissions.GS_IsController = false;
			staffWithoutHomeAddressPermissions.GS_IsOperational = true;
			staffWithoutHomeAddressPermissions.GS_UserAddress1 = "1";
			staffWithoutHomeAddressPermissions.GS_UserAddress2 = "2";
			staffWithoutHomeAddressPermissions.GS_City = "3";
			staffWithoutHomeAddressPermissions.GS_State = "4";
			staffWithoutHomeAddressPermissions.GS_Postcode = "5";
			staffWithoutHomeAddressPermissions.GS_HomePhone = "1234567";
			staffWithoutHomeAddressPermissions.GS_RN_NKCountryCode = "AU";

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewHomeAddressDetails.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutHomeAddressPermissions.PK;
			staffWithoutHomeAddressPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithHomeAddressPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithHomeAddressPermissions.GS_UserAddress1 = "6";
			staffWithHomeAddressPermissions.GS_UserAddress2 = "7";
			staffWithHomeAddressPermissions.GS_City = "8";
			staffWithHomeAddressPermissions.GS_State = "9";
			staffWithHomeAddressPermissions.GS_Postcode = "10";
			staffWithHomeAddressPermissions.GS_HomePhone = "890890890";
			staffWithHomeAddressPermissions.GS_RN_NKCountryCode = "US";

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewHomeAddressDetails.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithHomeAddressPermissions.PK;
			staffWithHomeAddressPermissions.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutHomeAddressPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithoutHomeAddressPermissions.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var staffWithHomeAddressPermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithHomeAddressPermissions.PK));
					var staffWithoutHomeAddressPermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutHomeAddressPermissions.PK));

					AssertEquals(viewDeniedMessage, staffWithHomeAddressPermissionsLoaded1.GS_UserAddress1);
					AssertEquals(viewDeniedMessage, staffWithHomeAddressPermissionsLoaded1.GS_UserAddress2);
					AssertEquals(viewDeniedMessage, staffWithHomeAddressPermissionsLoaded1.GS_City);
					AssertEquals(viewDeniedMessage, staffWithHomeAddressPermissionsLoaded1.GS_State);
					AssertEquals(viewDeniedMessage, staffWithHomeAddressPermissionsLoaded1.GS_Postcode);
					AssertEquals(viewDeniedMessage, staffWithHomeAddressPermissionsLoaded1.GS_HomePhone_Formatted);
					AssertEquals(viewDeniedMessage, staffWithHomeAddressPermissionsLoaded1.GS_RN_NKCountryCode);

					staffWithHomeAddressPermissions.GS_RN_NKCountryCode = "";
					AssertEquals("Check Must Enter should not happen for staff with out home address permissions", 0, staffWithHomeAddressPermissions.GS_RN_NKCountryCodeInfo.Notifications.Count());
				}
			}

			using (Env.SetTemporaryUserContext(new UserContext(staffWithHomeAddressPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				AssertEquals("Precondition: Current user has changed", staffWithHomeAddressPermissions.PK, Env.CurrentUser.PK);

				var factoryForLoading2 = new BusinessObjectFactory();
				var staffWithoutHomeAddressPermissionsLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutHomeAddressPermissions.PK));

				AssertEquals("1", staffWithoutHomeAddressPermissionsLoaded2.GS_UserAddress1);
				AssertEquals("2", staffWithoutHomeAddressPermissionsLoaded2.GS_UserAddress2);
				AssertEquals("3", staffWithoutHomeAddressPermissionsLoaded2.GS_City);
				AssertEquals("4", staffWithoutHomeAddressPermissionsLoaded2.GS_State);
				AssertEquals("5", staffWithoutHomeAddressPermissionsLoaded2.GS_Postcode);
				AssertEquals("1234567", staffWithoutHomeAddressPermissionsLoaded2.GS_HomePhone_Formatted);
				AssertEquals("AU", staffWithoutHomeAddressPermissionsLoaded2.GS_RN_NKCountryCode);

				staffWithoutHomeAddressPermissions.GS_RN_NKCountryCode = "US";
				staffWithoutHomeAddressPermissions.GS_RN_NKCountryCode = "";
				AssertEquals("Check Must Enter should happen for staff with home address permissions", 1, staffWithoutHomeAddressPermissions.GS_RN_NKCountryCodeInfo.Notifications.Count());
			}
		}

		public void TestValidateGS_Gender()
		{
			ZString viewDeniedMessage = "** View Denied due to Security Access **";

			var staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutPermissions.GS_IsController = false;
			staffWithoutPermissions.GS_IsOperational = true;

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherGender.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutPermissions.PK;
			staffWithoutPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithPermissions = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherGender.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithPermissions.PK;
			staffWithPermissions.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithoutPermissions.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var staffWithPermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithPermissions.PK));
					var staffWithoutPermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutPermissions.PK));
					AssertEquals(viewDeniedMessage, staffWithPermissionsLoaded1.GS_Gender);
				}
			}

			using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				AssertEquals("Precondition: Current user has changed", staffWithPermissions.PK, Env.CurrentUser.PK);

				var factoryForLoading2 = new BusinessObjectFactory();
				var staffWithoutPermissionsLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutPermissions.PK));
				AssertNotEquals(viewDeniedMessage, staffWithoutPermissionsLoaded2.GS_Gender);
			}
		}

		#region TestValidateStaffSensitiveInfoWithSecurityCheck

		public void TestValidateStaffSensitiveInfoWithSecurityCheck()
		{
			var glbStaffPK = Guid.NewGuid();
			var dummyStaff = Factory.NewWithPrimaryKey<GlbStaff>(glbStaffPK);

			dummyStaff.GS_RN_NKCountryCode = "AU";
			dummyStaff.AddressCode = "AddressCode";
			dummyStaff.AddressMap = "AddressMap";
			dummyStaff.GS_Birthdate = new ZDate(2016, 08, 30);
			dummyStaff.GS_BrokerID = "GS_BrokerID";
			dummyStaff.GS_BrokerPasswordStatus = "BPS";
			dummyStaff.GS_EftWages = true;
			dummyStaff.GS_EmergencyContactEmail = "jahom.chen@wisetechglobal.com";
			dummyStaff.GS_EmergencyHomePhone = "61212345678";
			dummyStaff.GS_EmergencyWorkPhone = "61212345678";
			dummyStaff.GS_HomePhone = "61212345678";
			dummyStaff.GS_NextOfKinEmail = "jahom.chen@wisetechglobal.com";
			dummyStaff.GS_NextOfKinHomePhone = "61212345678";
			dummyStaff.GS_NextOfKinWorkPhone = "61212345678";
			dummyStaff.GS_Passport = "GS_Passport";
			dummyStaff.GS_PersonalEDIMailBox = "PersonalEDIMailBox";
			dummyStaff.GS_ResidencyExpiry = new ZDate(2016, 08, 30);
			dummyStaff.GS_ResidencyStatus = "RYS";
			dummyStaff.GS_SecurityCardNumber = "SecurityCardNumber";
			dummyStaff.GS_WorkExtension = "WExtension";

			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var viewDeniedMessage = "** View Denied due to Security Access **";
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				using (CurrentUserChanger.SwitchToNewUserTemporarily(user.GS_LoginName))
				{
					ChangeStaffViewEnvSecurity(true);
					var staffFromDB = Factory.Load<GlbStaff>(glbStaffPK);

					AssertEquals("(02) 1234 5678", staffFromDB.GS_EmergencyHomePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
					AssertEquals("(02) 1234 5678", staffFromDB.GS_HomePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
					AssertEquals("(02) 1234 5678", staffFromDB.GS_NextOfKinHomePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
					AssertEquals("(02) 1234 5678", staffFromDB.GS_NextOfKinWorkPhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
					AssertEquals("AddressCode", staffFromDB.AddressCode);
					AssertEquals("AddressMap", staffFromDB.AddressMap);
					AssertEquals(new ZDate(2016, 08, 30), staffFromDB.GS_Birthdate);
					AssertEquals("GS_BrokerID", staffFromDB.GS_BrokerID);
					AssertEquals("BPS", staffFromDB.GS_BrokerPasswordStatus);
					AssertEquals(ZBool.True, staffFromDB.GS_EftWages);
					AssertEquals("jahom.chen@wisetechglobal.com", staffFromDB.GS_EmergencyContactEmail);
					AssertEquals("61212345678", staffFromDB.GS_EmergencyHomePhone);
					AssertEquals("(02) 1234 5678", staffFromDB.GS_EmergencyHomePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals("61212345678", staffFromDB.GS_EmergencyWorkPhone);
					AssertEquals("(02) 1234 5678", staffFromDB.GS_EmergencyWorkPhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals("61212345678", staffFromDB.GS_HomePhone);
					AssertEquals("(02) 1234 5678", staffFromDB.GS_HomePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals("jahom.chen@wisetechglobal.com", staffFromDB.GS_NextOfKinEmail);
					AssertEquals("61212345678", staffFromDB.GS_NextOfKinHomePhone);
					AssertEquals("(02) 1234 5678", staffFromDB.GS_NextOfKinHomePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals("61212345678", staffFromDB.GS_NextOfKinWorkPhone);
					AssertEquals("(02) 1234 5678", staffFromDB.GS_NextOfKinWorkPhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals("GS_Passport", staffFromDB.GS_Passport);
					AssertEquals("PersonalEDIMailBox", staffFromDB.GS_PersonalEDIMailBox);
					AssertEquals(new ZDate(2016, 08, 30), staffFromDB.GS_ResidencyExpiry);
					AssertEquals("RYS", staffFromDB.GS_ResidencyStatus);
					AssertEquals("SecurityCardNumber", staffFromDB.GS_SecurityCardNumber);
					AssertEquals("WExtension", staffFromDB.GS_WorkExtension);
					AssertEquals(string.Empty, staffFromDB.NationalIdentityNumber);

					ChangeStaffViewEnvSecurity(false);
					staffFromDB = new BusinessObjectFactory().Load<GlbStaff>(glbStaffPK);

					AssertEquals(viewDeniedMessage, staffFromDB.GS_EmergencyHomePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_HomePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_NextOfKinHomePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_NextOfKinWorkPhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
					AssertEquals(viewDeniedMessage, staffFromDB.AddressCode);
					AssertEquals(viewDeniedMessage, staffFromDB.AddressMap);
					AssertEquals(ZDate.Empty, staffFromDB.GS_Birthdate);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_BrokerID);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_BrokerPasswordStatus);
					AssertEquals(ZBool.False, staffFromDB.GS_EftWages);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_EmergencyContactEmail);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_EmergencyHomePhone);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_EmergencyHomePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_EmergencyWorkPhone);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_EmergencyWorkPhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_HomePhone);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_HomePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_NextOfKinEmail);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_NextOfKinHomePhone);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_NextOfKinHomePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_NextOfKinWorkPhone);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_NextOfKinWorkPhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_Passport);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_PersonalEDIMailBox);
					AssertEquals(ZDate.Empty, staffFromDB.GS_ResidencyExpiry);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_ResidencyStatus);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_SecurityCardNumber);
					AssertEquals(viewDeniedMessage, staffFromDB.GS_WorkExtension);
					AssertEquals(viewDeniedMessage, staffFromDB.NationalIdentityNumber);
				}
			}
		}

		void ChangeStaffViewEnvSecurity(bool flag)
		{
			Env.Security.StaffViewBirthDate.IsAllowed = flag;
			Env.Security.StaffViewBankingDetails.IsAllowed = flag;
			Env.Security.StaffViewHomeAddressDetails.IsAllowed = flag;
			Env.Security.StaffViewEmergencyContact.IsAllowed = flag;
			Env.Security.StaffViewOtherReferences.IsAllowed = flag;
			Env.Security.StaffViewOtherCertificates.IsAllowed = flag;
			Env.Security.StaffViewOtherTitle.IsAllowed = flag;
			Env.Security.StaffViewOtherMobilePhone.IsAllowed = flag;
			Env.Security.StaffViewOtherBrokerInfo.IsAllowed = flag;
			Env.Security.StaffViewOtherPassport.IsAllowed = flag;
			Env.Security.StaffViewOtherPersonalEDIMailBox.IsAllowed = flag;
			Env.Security.StaffViewOtherResidencyStatus.IsAllowed = flag;
			Env.Security.StaffViewOtherSecurityCardNumber.IsAllowed = flag;
			Env.Security.StaffViewOtherWorkExtension.IsAllowed = flag;
		}

		#endregion

		public void TestNoValidationErrorForEmptyGS_RN_NKCountryCodeAndGS_CityInResources()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_IsResource = true;
			resource.GS_RN_NKCountryCode = "";
			resource.GS_City = "";

			Factory.Save();

			AssertEquals("Resources are still validated with an empty country", 0, resource.GS_RN_NKCountryCodeInfo.Notifications.Count());
			AssertEquals("Resources are still validated with an empty city", 0, resource.GS_CityInfo.Notifications.Count());
		}

		public void TestValidateGS_IsRobotWhenNotOperational()
		{
			EnvProxy.SetHostedLocationForTest("SYD"); // Set environment to a hosted location - SYD
			DataRegistry.Instance.SetEnableRPAOnWiseCloudForTest(true);
			var staff = Factory.New<GlbStaff>();

			staff.GS_IsOperational = false;
			staff.RunPreSaveValidation();
			AssertEquals("If not robot, no error", false, staff.GS_IsRobotInfo.HasErrors());

			staff.GS_IsRobot = true;
			AssertEquals("Error message should match.", true, staff.GS_IsRobotInfo.HasError("A non operational user cannot be a robot."));

			staff.GS_IsOperational = true;
			staff.RunPreSaveValidation();
			AssertEquals("If operational, no error", false, staff.GS_IsRobotInfo.HasErrors());
		}

		public void TestValidateGS_IsRobotWhenSelfHosted()
		{
			EnvProxy.SetHostedLocationForTest("NCW"); // set environment to self hosted - Not CargoWise
			var staff = Factory.New<GlbStaff>();

			staff.RunPreSaveValidation();
			AssertEquals("If not hosted, no validation necessary", false, staff.GS_IsRobotInfo.HasErrors());

			staff.GS_IsRobot = true;
			staff.RunPreSaveValidation();
			AssertEquals("If not hosted, no validation necessary", false, staff.GS_IsRobotInfo.HasErrors());
		}

		public void TestValidateGS_IsRobotWhenHosted()
		{
			EnvProxy.SetHostedLocationForTest("SYD"); // Set environment to a hosted location - SYD
			DataRegistry.Instance.SetEnableRPAOnWiseCloudForTest(false);
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsOperational = true;

			staff.RunPreSaveValidation();
			AssertEquals("If hosted but not flagged, no errors.", false, staff.GS_IsRobotInfo.HasErrors());

			staff.GS_IsRobot = true;
			staff.RunPreSaveValidation();
			AssertEquals("If hosted without registry flagged, should have errors.", true, staff.GS_IsRobotInfo.HasErrors());
			AssertEquals("Error message should match.", true, staff.GS_IsRobotInfo.HasError("This checkbox is used to indicate that this user is used for Robotic Process Automation. To use this feature, RPA needs to be enabled for your environment."));
		}

		public void TestValidateGS_IsRobotWhenHostedAndRegistrySet()
		{
			EnvProxy.SetHostedLocationForTest("SYD"); // Set environment to a hosted location - SYD
			DataRegistry.Instance.SetEnableRPAOnWiseCloudForTest(true);
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsOperational = true;

			staff.GS_IsRobot = true;
			staff.RunPreSaveValidation();
			AssertEquals("If hosted and flagged, should have no error.", false, staff.GS_IsRobotInfo.HasErrors());
		}

		public void TestValidateGS_WorkingLanguage()
		{
			Bizo.GS_WorkingLanguage = Constants.Languages.German;
			AssertHasWarning(Bizo.GS_WorkingLanguageInfo, string.Format("Selecting this language will mean the screens are shown in German for this user."));

			Bizo.GS_WorkingLanguage = Constants.Languages.English;
			AssertNoWarnings(Bizo.GS_WorkingLanguageInfo);

			Bizo.GS_WorkingLanguage = "ENG";
			AssertHasError(Bizo.GS_WorkingLanguageInfo, "Enter a valid Language.");

			Bizo.GS_WorkingLanguage = "";
			AssertHasError(Bizo.GS_WorkingLanguageInfo, "Please enter a Language.");
		}

		public void TestValidateGS_GC_PreferredPaymentCompany_MandatoryIfSalesRepAndNotUseTransactionCompanyAsPreferredPayment()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsSalesRep = false;
			staff.UseTransactionCompanyAsPreferredPayment = false;
			staff.Validation.ValidateGS_GC_PreferredPaymentCompany();
			AssertMandatoryValidationError(staff.GS_GC_PreferredPaymentCompanyInfo, false);

			staff.GS_IsSalesRep = true;
			staff.Validation.ValidateGS_GC_PreferredPaymentCompany();
			AssertMandatoryValidationError(staff.GS_GC_PreferredPaymentCompanyInfo, true);

			staff.GS_GC_PreferredPaymentCompany = GlbCompany.CurrentCompany.PK;
			AssertMandatoryValidationError(staff.GS_GC_PreferredPaymentCompanyInfo, false);

			staff.GS_GC_PreferredPaymentCompany = ZGuid.Empty;
			AssertMandatoryValidationError(staff.GS_GC_PreferredPaymentCompanyInfo, true);

			staff.UseTransactionCompanyAsPreferredPayment = true;
			AssertMandatoryValidationError(staff.GS_GC_PreferredPaymentCompanyInfo, false);
		}

		#region Implementation

		protected GlbStaff Bizo
		{
			get
			{
				if (fBizo == null)
				{
					fBizo = GetNewBusinessObject();
				}
				return fBizo;
			}
		}

		GlbStaff fBizo;

		protected virtual GlbStaff GetNewBusinessObject()
		{
			var result = Factory.New<GlbStaff>();
			result.GS_Code = "ZAC";
			return result;
		}

		#endregion
	}
}
