
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicant))]
	sealed class HRJobApplicantTests : EnterpriseBusinessObjectTestCase
	{
		#region Test Securities

		GlbStaff staffDenied;
		GlbStaff staffGranted;
		HRJobApplicant applicant;

		void SetupEditSecurity(bool name, bool dob, bool gender, bool mobile, bool homePhone, bool fax, bool address, bool nationality, bool passport, bool licence)
		{
			staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffGranted = Factory.NewWithValidTestData<GlbStaff>();

			applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_UserAddress1 = "1";
			applicant.HA_UserAddress2 = "2";
			applicant.HA_City = "3";
			applicant.HA_State = "4";
			applicant.HA_Postcode = "5";
			applicant.HA_RN_NKCountry = "AU";
			applicant.HA_Title = "Mr";
			applicant.HA_NameSuffix = "Suf";
			applicant.HA_FullName = "Full Name";
			applicant.HA_Birthdate = ZDate.BrettsBirthday;
			applicant.HA_Gender = "M";
			applicant.HA_MobilePhone = "0499702888";
			applicant.HA_HomePhone = "0280012200";
			applicant.HA_FaxNum = "0280012201";
			applicant.HA_RN_NKNationalityCodeISO = "AU";
			applicant.HA_Passport = "A0123456";
			applicant.HA_DriversLicenseNumber = "A01234567";

			if (name)
			{
				AddEditSecurity(Env.Security.HRJobApplicantName);
			}

			if (dob)
			{
				AddEditSecurity(Env.Security.HRJobApplicantBirthdate);
			}

			if (gender)
			{
				AddEditSecurity(Env.Security.HRJobApplicantGender);
			}

			if (mobile)
			{
				AddEditSecurity(Env.Security.HRJobApplicantMobilePhone);
			}

			if (homePhone)
			{
				AddEditSecurity(Env.Security.HRJobApplicantHomePhone);
			}

			if (fax)
			{
				AddEditSecurity(Env.Security.HRJobApplicantFax);
			}

			if (address)
			{
				AddEditSecurity(Env.Security.HRJobApplicantUserAddress);
			}

			if (nationality)
			{
				AddEditSecurity(Env.Security.HRJobApplicantNationality);
			}

			if (passport)
			{
				AddEditSecurity(Env.Security.HRJobApplicantPassport);
			}

			if (licence)
			{
				AddEditSecurity(Env.Security.HRJobApplicantDriversLicenseNumber);
			}

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantView, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantView, true, staffDenied.PK));

			Factory.Save();
		}

		void AddEditSecurity(SecurityCheckpoint checkpoint)
		{
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint, false, staffDenied.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint.Parent, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint, true, staffGranted.PK));
		}

		void SetupSecurity(bool name, bool dob, bool gender, bool mobile, bool homePhone, bool fax, bool address, bool nationality, bool passport, bool licence)
		{
			staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffGranted = Factory.NewWithValidTestData<GlbStaff>();

			applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_UserAddress1 = "1";
			applicant.HA_UserAddress2 = "2";
			applicant.HA_City = "3";
			applicant.HA_State = "4";
			applicant.HA_Postcode = "5";
			applicant.HA_RN_NKCountry = "AU";
			applicant.HA_Title = "Mr";
			applicant.HA_NameSuffix = "Suf";
			applicant.HA_FullName = "Full Name";
			applicant.HA_Birthdate = ZDate.BrettsBirthday;
			applicant.HA_Gender = "M";
			applicant.HA_MobilePhone = "0499702888";
			applicant.HA_HomePhone = "0280012200";
			applicant.HA_FaxNum = "0280012201";
			applicant.HA_RN_NKNationalityCodeISO = "AU";
			applicant.HA_Passport = "A0123456";
			applicant.HA_DriversLicenseNumber = "A01234567";

			if (name)
			{
				AddSecurity(Env.Security.HRJobApplicantViewName);
			}

			if (dob)
			{
				AddSecurity(Env.Security.HRJobApplicantViewBirthdate);
			}

			if (gender)
			{
				AddSecurity(Env.Security.HRJobApplicantViewGender);
			}

			if (mobile)
			{
				AddSecurity(Env.Security.HRJobApplicantViewMobilePhone);
			}

			if (homePhone)
			{
				AddSecurity(Env.Security.HRJobApplicantViewHomePhone);
			}

			if (fax)
			{
				AddSecurity(Env.Security.HRJobApplicantViewFax);
			}

			if (address)
			{
				AddSecurity(Env.Security.HRJobApplicantViewUserAddress);
			}

			if (nationality)
			{
				AddSecurity(Env.Security.HRJobApplicantViewNationality);
			}

			if (passport)
			{
				AddSecurity(Env.Security.HRJobApplicantViewPassport);
			}

			if (licence)
			{
				AddSecurity(Env.Security.HRJobApplicantViewDriversLicenseNumber);
			}

			Factory.Save();
		}

		void AddSecurity(SecurityCheckpoint checkpoint)
		{
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint, false, staffDenied.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint.Parent, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint, true, staffGranted.PK));

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantEdit, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantEdit, true, staffDenied.PK));
		}

		GlbSecurity GetSecurity(SecurityCheckpoint checkpoint, bool granted, ZGuid staffPk)
		{
			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = checkpoint.Code;
			security.GU_SecurityItemIsAllowed = granted;
			security.GU_GS = staffPk;

			return security;
		}

		public void TestApplicantBypassesPersonSecurity()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantEdit, true, staffDenied.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantView, true, staffDenied.PK));

			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_HomeAddress1 = "1";
			person.PER_HomeAddress2 = "2";
			person.PER_City = "3";
			person.PER_State = "4";
			person.PER_Postcode = "5";
			person.PER_LegalName = "legal";
			person.PER_FullName = "full";
			person.PER_BirthDate = ZDate.BrettsBirthday;
			person.PER_EmailAddress = "email@test.com";
			person.PER_EmailAddress2 = "email2@test.com";
			person.PER_Gender = "M";
			person.PER_MobilePhone = "0499702888";
			person.PER_MobilePhone2 = "0499702893";
			person.PER_FaxNumber = "0280012201";
			person.PER_HomePhone = "0280012200";
			person.PER_RN_NKNationalityCodeISO = "UA";
			person.PER_RN_NKCountry = "AU";
			person.PER_PersonalInfo = "personal";
			person.PER_Picture = new ZBlob(new byte[] { 1, 2, 3 });
			person.PER_Passport = "A0123456";
			person.PER_PassportExpiryDate = ZDate.BrettsBirthday;
			person.PER_PassportPlaceOfIssue = "UA";
			person.PER_DriversLicenseNumber = "A01234567";

			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_PER = person.PK;
			applicant.HA_EmailAddress = "applicant@gmail.com";

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertNotEquals("1", person.PER_HomeAddress1);
					AssertNotEquals("2", person.PER_HomeAddress2);
					AssertNotEquals("3", person.PER_City);
					AssertNotEquals("4", person.PER_State);
					AssertNotEquals("5", person.PER_Postcode);
					AssertNotEquals(ZDate.BrettsBirthday, person.PER_BirthDate);
					AssertNotEquals("M", person.PER_Gender);
					AssertNotEquals("0499702888", person.PER_MobilePhone);
					AssertNotEquals("0280012200", person.PER_HomePhone);
					AssertNotEquals("UA", person.PER_RN_NKNationalityCodeISO);
					AssertNotEquals("AU", person.PER_RN_NKCountry);
					AssertNotEquals("A0123456", person.PER_Passport);
					AssertNotEquals("A01234567", person.PER_DriversLicenseNumber);

					AssertEquals("1", applicant.HA_UserAddress1);
					AssertEquals("2", applicant.HA_UserAddress2);
					AssertEquals("3", applicant.HA_City);
					AssertEquals("4", applicant.HA_State);
					AssertEquals("5", applicant.HA_Postcode);
					AssertEquals(ZDate.BrettsBirthday, applicant.HA_Birthdate);
					AssertEquals("M", applicant.HA_Gender);
					AssertEquals("0499702888", applicant.HA_MobilePhone);
					AssertEquals("+61 499 702 888", applicant.HA_MobilePhone_Formatted);
					AssertEquals("0280012200", applicant.HA_HomePhone);
					AssertEquals("+61 2 8001 2200", applicant.HA_HomePhone_Formatted);
					AssertEquals("UA", applicant.HA_RN_NKNationalityCodeISO);
					AssertEquals("AU", applicant.HA_RN_NKCountry);
					AssertEquals("A0123456", applicant.HA_Passport);
					AssertEquals("A01234567", applicant.HA_DriversLicenseNumber);
				}
			}
		}

		public void TestNameSecurity()
		{
			AssertSecurity(true, false, false, false, false, false, false, false, false, false);
		}

		public void TestDateOfBirthSecurity()
		{
			AssertSecurity(false, true, false, false, false, false, false, false, false, false);
		}

		public void TestGenderSecurity()
		{
			AssertSecurity(false, false, true, false, false, false, false, false, false, false);
		}

		public void TestMobileSecurity()
		{
			AssertSecurity(false, false, false, true, false, false, false, false, false, false);
		}

		public void TestHomePhoneSecurity()
		{
			AssertSecurity(false, false, false, false, true, false, false, false, false, false);
		}

		public void TestFaxSecurity()
		{
			AssertSecurity(false, false, false, false, false, true, false, false, false, false);
		}

		public void TestAddressSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, true, false, false, false);
		}

		public void TestViewNationalitySecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, true, false, false);
		}

		public void TestPassportSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, true, false);
		}

		public void TestDriversLicenceSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, false, true);
		}

		public void TestNameSecurityEdit()
		{
			AssertEditSecurity(true, false, false, false, false, false, false, false, false, false);
		}

		public void TestDateOfBirthSecurityEdit()
		{
			AssertEditSecurity(false, true, false, false, false, false, false, false, false, false);
		}

		public void TestGenderSecurityEdit()
		{
			AssertEditSecurity(false, false, true, false, false, false, false, false, false, false);
		}

		public void TestMobileSecurityEdit()
		{
			AssertEditSecurity(false, false, false, true, false, false, false, false, false, false);
		}

		public void TestHomePhoneSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, true, false, false, false, false, false);
		}

		public void TestFaxSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, true, false, false, false, false);
		}
		public void TestAddressSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, true, false, false, false);
		}

		public void TestNationalitySecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, false, true, false, false);
		}

		public void TestPassportSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, false, false, true, false);
		}

		public void TestDriversLicenceSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, false, false, false, true);
		}

		void AssertSecurity(bool name, bool dob, bool gender, bool mobile, bool homePhone, bool fax, bool address, bool nationality, bool passport, bool licence)
		{
			SetupSecurity(name, dob, gender, mobile, homePhone, fax, address, nationality, passport, licence);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffDenied.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var applicantLoaded = factoryForLoading1.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, applicant.PK));

					AssertEquals(name, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_Title);
					AssertEquals(name, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_NameSuffix);
					AssertEquals(name, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_FullName);
					AssertEquals(dob, ZDate.Empty == applicantLoaded.HA_Birthdate);
					AssertEquals(gender, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_Gender);
					AssertEquals(mobile, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_MobilePhone);
					AssertEquals(homePhone, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_HomePhone);
					AssertEquals(fax, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_FaxNum);
					AssertEquals(nationality, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_RN_NKNationalityCodeISO);
					AssertEquals(address, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_UserAddress1);
					AssertEquals(address, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_UserAddress2);
					AssertEquals(address, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_City);
					AssertEquals(address, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_State);
					AssertEquals(address, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_Postcode);
					// AssertEquals(address, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_RN_NKCountry);
					AssertEquals(passport, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_Passport);
					AssertEquals(licence, applicantLoaded.ViewDeniedMessage == applicantLoaded.HA_DriversLicenseNumber);

					AssertEquals(name, applicantLoaded.HA_TitleInfo.ReadOnly);
					AssertEquals(name, applicantLoaded.HA_NameSuffixInfo.ReadOnly);
					AssertEquals(name, applicantLoaded.HA_FullNameInfo.ReadOnly);
					AssertEquals(dob, applicantLoaded.HA_BirthdateInfo.ReadOnly);
					AssertEquals(gender, applicantLoaded.HA_GenderInfo.ReadOnly);
					AssertEquals(mobile, applicantLoaded.HA_MobilePhoneInfo.ReadOnly);
					AssertEquals(homePhone, applicantLoaded.HA_HomePhoneInfo.ReadOnly);
					AssertEquals(fax, applicantLoaded.HA_FaxNumInfo.ReadOnly);
					AssertEquals(nationality, applicantLoaded.HA_RN_NKNationalityCodeISOInfo.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_UserAddress1Info.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_UserAddress2Info.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_CityInfo.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_StateInfo.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_PostcodeInfo.ReadOnly);
					// AssertEquals(address, applicantLoaded.HA_RN_NKCountryInfo.ReadOnly);
					AssertEquals(passport, applicantLoaded.HA_PassportInfo.ReadOnly);
					AssertEquals(licence, applicantLoaded.HA_DriversLicenseNumberInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffGranted.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory();
					var applicantLoaded2 = factoryForLoading2.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, applicant.PK));

					AssertEquals(name, "Mr" == applicantLoaded2.HA_Title);
					AssertEquals(name, "Suf" == applicantLoaded2.HA_NameSuffix);
					AssertEquals(name, "Full Name" == applicantLoaded2.HA_FullName);
					AssertEquals(dob, ZDate.BrettsBirthday == applicantLoaded2.HA_Birthdate);
					AssertEquals(gender, "M" == applicantLoaded2.HA_Gender);
					AssertEquals(mobile, "0499702888" == applicantLoaded2.HA_MobilePhone);
					AssertEquals(homePhone, "0280012200" == applicantLoaded2.HA_HomePhone);
					AssertEquals(fax, "0280012201" == applicantLoaded2.HA_FaxNum);
					AssertEquals(nationality, "AU" == applicantLoaded2.HA_RN_NKNationalityCodeISO);
					AssertEquals(address, "1" == applicantLoaded2.HA_UserAddress1);
					AssertEquals(address, "2" == applicantLoaded2.HA_UserAddress2);
					AssertEquals(address, "3" == applicantLoaded2.HA_City);
					AssertEquals(address, "4" == applicantLoaded2.HA_State);
					AssertEquals(address, "5" == applicantLoaded2.HA_Postcode);
					// AssertEquals(address, "AU" == applicantLoaded2.HA_RN_NKCountry);
					AssertEquals(passport, "A0123456" == applicantLoaded2.HA_Passport);
					AssertEquals(licence, "A01234567" == applicantLoaded2.HA_DriversLicenseNumber);

					AssertEquals(name, !applicantLoaded2.HA_TitleInfo.ReadOnly);
					AssertEquals(name, !applicantLoaded2.HA_NameSuffixInfo.ReadOnly);
					AssertEquals(name, !applicantLoaded2.HA_FullNameInfo.ReadOnly);
					AssertEquals(dob, !applicantLoaded2.HA_BirthdateInfo.ReadOnly);
					AssertEquals(gender, !applicantLoaded2.HA_GenderInfo.ReadOnly);
					AssertEquals(mobile, !applicantLoaded2.HA_MobilePhoneInfo.ReadOnly);
					AssertEquals(homePhone, !applicantLoaded2.HA_HomePhoneInfo.ReadOnly);
					AssertEquals(fax, !applicantLoaded2.HA_FaxNumInfo.ReadOnly);
					AssertEquals(nationality, !applicantLoaded2.HA_RN_NKNationalityCodeISOInfo.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_UserAddress1Info.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_UserAddress2Info.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_CityInfo.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_StateInfo.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_PostcodeInfo.ReadOnly);
					// AssertEquals(address, !applicantLoaded2.HA_RN_NKCountryInfo.ReadOnly);
					AssertEquals(passport, !applicantLoaded2.HA_PassportInfo.ReadOnly);
					AssertEquals(licence, !applicantLoaded2.HA_DriversLicenseNumberInfo.ReadOnly);
				}
			}
		}

		void AssertEditSecurity(bool name, bool dob, bool gender, bool mobile, bool homePhone, bool fax, bool address, bool nationality, bool passport, bool licence)
		{
			SetupEditSecurity(name, dob, gender, mobile, homePhone, fax, address, nationality, passport, licence);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffDenied.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var applicantLoaded = factoryForLoading1.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, applicant.PK));

					AssertEquals(name, applicantLoaded.HA_TitleInfo.ReadOnly);
					AssertEquals(name, applicantLoaded.HA_NameSuffixInfo.ReadOnly);
					AssertEquals(name, applicantLoaded.HA_FullNameInfo.ReadOnly);
					AssertEquals(dob, applicantLoaded.HA_BirthdateInfo.ReadOnly);
					AssertEquals(gender, applicantLoaded.HA_GenderInfo.ReadOnly);
					AssertEquals(mobile, applicantLoaded.HA_MobilePhoneInfo.ReadOnly);
					AssertEquals(homePhone, applicantLoaded.HA_HomePhoneInfo.ReadOnly);
					AssertEquals(fax, applicantLoaded.HA_FaxNumInfo.ReadOnly);
					AssertEquals(nationality, applicantLoaded.HA_RN_NKNationalityCodeISOInfo.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_UserAddress1Info.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_UserAddress2Info.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_CityInfo.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_StateInfo.ReadOnly);
					AssertEquals(address, applicantLoaded.HA_PostcodeInfo.ReadOnly);
					// AssertEquals(address, applicantLoaded.HA_RN_NKCountryInfo.ReadOnly);
					AssertEquals(passport, applicantLoaded.HA_PassportInfo.ReadOnly);
					AssertEquals(licence, applicantLoaded.HA_DriversLicenseNumberInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffGranted.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory();
					var applicantLoaded2 = factoryForLoading2.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, applicant.PK));

					AssertEquals(name, !applicantLoaded2.HA_TitleInfo.ReadOnly);
					AssertEquals(name, !applicantLoaded2.HA_NameSuffixInfo.ReadOnly);
					AssertEquals(name, !applicantLoaded2.HA_FullNameInfo.ReadOnly);
					AssertEquals(dob, !applicantLoaded2.HA_BirthdateInfo.ReadOnly);
					AssertEquals(gender, !applicantLoaded2.HA_GenderInfo.ReadOnly);
					AssertEquals(mobile, !applicantLoaded2.HA_MobilePhoneInfo.ReadOnly);
					AssertEquals(homePhone, !applicantLoaded2.HA_HomePhoneInfo.ReadOnly);
					AssertEquals(fax, !applicantLoaded2.HA_FaxNumInfo.ReadOnly);
					AssertEquals(nationality, !applicantLoaded2.HA_RN_NKNationalityCodeISOInfo.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_UserAddress1Info.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_UserAddress2Info.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_CityInfo.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_StateInfo.ReadOnly);
					AssertEquals(address, !applicantLoaded2.HA_PostcodeInfo.ReadOnly);
					// AssertEquals(address, !applicantLoaded2.HA_RN_NKCountryInfo.ReadOnly);
					AssertEquals(passport, !applicantLoaded2.HA_PassportInfo.ReadOnly);
					AssertEquals(licence, !applicantLoaded2.HA_DriversLicenseNumberInfo.ReadOnly);
				}
			}
		}

		#endregion

	}
}
