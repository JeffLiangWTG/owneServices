using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.SqlSecurity;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class GlbStaffValidationRealTest : GlbStaffValidationTestCase
	{
		public void TestGS_IsActive_UnpublishCustomizedDocumentsAndReports()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_IsActive = true;
			staff.GS_LoginName = "Test";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_BusinessContext = "Context";
			menuItem.SU_MenuPath = "menu/path2";
			menuItem.SU_MenuName = "docName";
			menuItem.SU_ContactType = "CNE";
			menuItem.SU_GS_NKStaffCode = staff.GS_Code;
			Factory.Save();
			staff.GS_IsActive = false;
			staff.Validation.ValidateGS_IsActive();
			AssertHasError("Should display the error as this staff still has one unpublished document or report", staff.GS_IsActiveInfo, "Staff cannot be deactivated because it has least one unpublished document or report linked to it:\r\nContext : docName : menu/path2 : CNE.");

			menuItem.SU_IsPublished = true;
			Factory.Save();

			var glbStaff = Factory.Load<GlbStaff>(Bizo.PK);
			glbStaff.GS_IsActive = false;
			glbStaff.Validation.ValidateGS_IsActive();
			AssertNoErrors(glbStaff.GS_IsActiveInfo);
		}

		public void TestGS_IsActive()
		{
			Bizo.GS_IsActive = false;
			AssertNoErrors(Bizo.GS_IsActiveInfo);

			Bizo.GS_IsActive = true;
			Bizo.Groups.RemoveAll();
			AssertEquals("ALL group remained", 1, Bizo.Groups.Count);

			Bizo.GS_IsActive = false;
			AssertEquals("No group memberships - ALL is removed when inactive", 0, Bizo.Groups.Count);
			Bizo.Validation.ValidateGS_IsActive();
			AssertNoErrors(Bizo.GS_IsActiveInfo);

			Bizo.GS_IsActive = true;
			AssertNoErrors(Bizo.GS_IsActiveInfo);
			AssertEquals("ALL group is back", 1, Bizo.Groups.Count);

			Bizo.GS_IsActive = false;
			AssertEquals("No group memberships - ALL is removed when inactive", 0, Bizo.Groups.Count);
			Bizo.Validation.ValidateGS_IsActive();
			AssertNoErrors(Bizo.GS_IsActiveInfo);
		}

		public void TestGS_IsActive_SG()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Singapore))
			{
				Bizo.GS_IsActive = true;
				var sgStaffWrapper = Bizo.GetSGWrapper();
				sgStaffWrapper.AccessPassword.GP_UserID = "Test ID";
				sgStaffWrapper.AccessPassword.CurrentDecryptedPassword = "12345";
				sgStaffWrapper.AccessPassword.NextDecryptedPassword = "12345";
				Bizo.GS_IsActive = false;
				AssertHasError("Should display the error as this staff still has a Access credential with user ID and password", Bizo.GS_IsActiveInfo, "An ACCESS user credential is detected for this staff member. Please remove the ACCESS credential before deactivating the User.");

				Bizo.GS_IsActive = true;
				sgStaffWrapper.AccessPassword.GP_UserID = ZString.Empty;
				sgStaffWrapper.AccessPassword.CurrentDecryptedPassword = ZString.Empty;
				sgStaffWrapper.AccessPassword.NextDecryptedPassword = ZString.Empty;
				Bizo.GS_IsActive = false;
				AssertNoErrors("Should not display the error as this staff has an Access credential with user ID but no password", Bizo.GS_IsActiveInfo);

				Bizo.GS_IsActive = true;
				sgStaffWrapper.AccessPassword.GP_UserID = "Test ID";
				sgStaffWrapper.AccessPassword.CurrentDecryptedPassword = ZString.Empty;
				Bizo.GS_IsActive = false;
				AssertHasError("Should display the error as this staff still has a Access credential with user ID", Bizo.GS_IsActiveInfo, "An ACCESS user credential is detected for this staff member. Please remove the ACCESS credential before deactivating the User.");

				Bizo.GS_IsActive = true;
				sgStaffWrapper.AccessPassword.GP_UserID = ZString.Empty;
				sgStaffWrapper.AccessPassword.CurrentDecryptedPassword = "12345";
				Bizo.GS_IsActive = false;
				AssertHasError("Should display the error as this staff still has a Access credential with password", Bizo.GS_IsActiveInfo, "An ACCESS user credential is detected for this staff member. Please remove the ACCESS credential before deactivating the User.");
			}
		}

		public void TestGS_IsActiveManagers()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.New<GlbStaff>();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord = StaffManagerTestHelper.AddManager(staff, manager, "HRM");
			var directReportRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM");

			staff.GS_IsActive = false;
			AssertHasWarning("Should have direct report warning", staff.GS_IsActiveInfo, "Please deactivate and/or transfer any current or future direct reports (This is automatically done on the Staff Form).");
			AssertHasError("Should have manager error", staff.GS_IsActiveInfo, "Please deactivate any current or future managers.");

			managerRecord.GSM_EndDate = ZDateTime.Today;
			staff.Validation.ValidateGS_IsActive();
			AssertHasWarning("Should have direct report warning", staff.GS_IsActiveInfo, "Please deactivate and/or transfer any current or future direct reports (This is automatically done on the Staff Form).");
			AssertNoError("Should not have manager error", staff.GS_IsActiveInfo, "Please deactivate any current or future managers.");

			directReportRecord.GSM_EndDate = ZDateTime.Today;
			staff.Validation.ValidateGS_IsActive();
			AssertNoWarning("Should not have direct report warning", staff.GS_IsActiveInfo, "Please deactivate and/or transfer any current or future direct reports (This is automatically done on the Staff Form).");
			AssertNoError("Should not have manager error", staff.GS_IsActiveInfo, "Please deactivate any current or future managers.");
		}

		public void TestGS_IsActive_EDIClient()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "TT";
			staff1.GS_IsActive = true;
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "TT2";
			staff2.GS_IsActive = true;

			var commParty1 = Factory.NewWithValidTestData<EDICommunicationParty>();
			commParty1.ECP_GS_SecurityProxy = staff1.PK;
			commParty1.ECP_IsActive = true;

			var commParty2 = Factory.NewWithValidTestData<EDICommunicationParty>();
			commParty2.ECP_GS_SecurityProxy = staff2.PK;
			commParty2.ECP_IsActive = false;

			Assert(!staff1.GS_IsActiveInfo.HasErrors());
			Assert(!staff2.GS_IsActiveInfo.HasErrors());
			staff1.GS_IsActive = false;
			AssertHasError(staff1.GS_IsActiveInfo, "Staff cannot be deactivated because it has an active EDI client profile linked to it.");
			staff2.GS_IsActive = false;
			Assert("Staff can be deactivated if it has an inactive EDI client profile linked to it.", !staff2.GS_IsActiveInfo.HasErrors());
		}

		public void TestCheckGS_EmailAddress()
		{
			Bizo.GS_EmailAddress = ZString.Empty;
			AssertNoErrors("Email address is not mandatory", Bizo.GS_EmailAddressInfo);
			Bizo.GS_EmailAddress = "sdfsd";
			AssertHasErrors("Email address should not be valid", Bizo.GS_EmailAddressInfo);
			Bizo.GS_EmailAddress = "blah@mail.com";
			AssertNoErrors("Email address is valid", Bizo.GS_EmailAddressInfo);
		}

		public void TestDatabasePermissions()
		{
			var staff = Factory.New<GlbStaff>();
			AssertNoWarning(staff.IsReadOnlyDBUserInfo, "Staff should probably be granted read permission.");
			staff.IsDatabaseDeveloper = true;
			AssertHasWarning(staff.IsReadOnlyDBUserInfo, "Staff should probably be granted read permission.");
			staff.IsReadOnlyDBUser = true;
			AssertNoWarning(staff.IsReadOnlyDBUserInfo, "Staff should probably be granted read permission.");
			staff.IsDatabaseDeveloper = false;
			AssertNoWarning(staff.IsReadOnlyDBUserInfo, "Staff should probably be granted read permission.");
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_DatabaseDeveloper_NoFlexibleGroup()
		{
			var staffPk = PrepareStaffTestData(0, DbRoleTypes.DbDataWriterRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsDatabaseDeveloper = false;

			AssertNoWarnings(staffInOtherFactory.IsDatabaseDeveloperInfo);
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_DatabaseDeveloper_OneFlexibleGroup()
		{
			var staffPk = PrepareStaffTestData(1, DbRoleTypes.DbDataWriterRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsDatabaseDeveloper = false;

			AssertHasWarning(staffInOtherFactory.IsDatabaseDeveloperInfo, "Group G_Test_0 with the Database Developer role is also associated with this staff.");
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_DatabaseDeveloper_FiveFlexibleGroups()
		{
			var staffPk = PrepareStaffTestData(5, DbRoleTypes.DbDataWriterRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsDatabaseDeveloper = false;

			AssertHasWarning(staffInOtherFactory.IsDatabaseDeveloperInfo, @"==============================================================
5 groups with the Database Developer role are associated with this staff:
G_Test_0,
G_Test_1,
G_Test_2,
G_Test_3,
G_Test_4
==============================================================
");
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_DatabaseDeveloper_ElevenFlexibleGroups()
		{
			var staffPk = PrepareStaffTestData(11, DbRoleTypes.DbDataWriterRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsDatabaseDeveloper = false;

			AssertHasWarning(staffInOtherFactory.IsDatabaseDeveloperInfo, @"==============================================================
11 groups with the Database Developer role are associated with this staff. The first 10 are shown:
G_Test_0,
G_Test_1,
G_Test_10,
G_Test_2,
G_Test_3,
G_Test_4,
G_Test_5,
G_Test_6,
G_Test_7,
G_Test_8
==============================================================
");
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_DatabaseReader_OneFlexibleGroup()
		{
			var staffPk = PrepareStaffTestData(1, DbRoleTypes.CwRestrictedReaderRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsReadOnlyDBUser = false;

			AssertHasWarning(staffInOtherFactory.IsReadOnlyDBUserInfo, "Group G_Test_0 with the Database Reader role is also associated with this staff.");
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_DatabaseReader_FiveFlexibleGroups()
		{
			var staffPk = PrepareStaffTestData(5, DbRoleTypes.CwRestrictedReaderRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsReadOnlyDBUser = false;

			AssertHasWarning(staffInOtherFactory.IsReadOnlyDBUserInfo, @"==============================================================
5 groups with the Database Reader role are associated with this staff:
G_Test_0,
G_Test_1,
G_Test_2,
G_Test_3,
G_Test_4
==============================================================
");
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_DatabaseReader_ElevenFlexibleGroups()
		{
			var staffPk = PrepareStaffTestData(11, DbRoleTypes.CwRestrictedReaderRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsReadOnlyDBUser = false;

			AssertHasWarning(staffInOtherFactory.IsReadOnlyDBUserInfo, @"==============================================================
11 groups with the Database Reader role are associated with this staff. The first 10 are shown:
G_Test_0,
G_Test_1,
G_Test_10,
G_Test_2,
G_Test_3,
G_Test_4,
G_Test_5,
G_Test_6,
G_Test_7,
G_Test_8
==============================================================
");
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_BackupOperator_OneFlexibleGroup()
		{
			var staffPk = PrepareStaffTestData(1, DbRoleTypes.DbBackupOperatorRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsBackupOperator = false;

			AssertHasWarning(staffInOtherFactory.IsBackupOperatorInfo, "Group G_Test_0 with the Backup Operator role is also associated with this staff.");
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_BackupOperator_FiveFlexibleGroups()
		{
			var staffPk = PrepareStaffTestData(5, DbRoleTypes.DbBackupOperatorRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsBackupOperator = false;

			AssertHasWarning(staffInOtherFactory.IsBackupOperatorInfo, @"==============================================================
5 groups with the Backup Operator role are associated with this staff:
G_Test_0,
G_Test_1,
G_Test_2,
G_Test_3,
G_Test_4
==============================================================
");
		}

		public void TestWarningMessageFromFlexibleDatabaseAccessGroup_BackupOperator_ElevenFlexibleGroups()
		{
			var staffPk = PrepareStaffTestData(11, DbRoleTypes.DbBackupOperatorRole);
			var staffInOtherFactory = GetStaffInOtherFactoryMethod(staffPk);

			staffInOtherFactory.IsBackupOperator = false;

			AssertHasWarning(staffInOtherFactory.IsBackupOperatorInfo, @"==============================================================
11 groups with the Backup Operator role are associated with this staff. The first 10 are shown:
G_Test_0,
G_Test_1,
G_Test_10,
G_Test_2,
G_Test_3,
G_Test_4,
G_Test_5,
G_Test_6,
G_Test_7,
G_Test_8
==============================================================
");
		}

		static GlbStaff GetStaffInOtherFactoryMethod(ZGuid staffPk)
		{
			var staffQuery = new ZDBOnlyQuery(typeof(GlbStaff));
			staffQuery.AddToFilter(GlbStaffSchema.PK, staffPk);

			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.RefreshEnabled = false;
			var staffInOtherFactory = anotherFactory.LoadTop1<GlbStaff>(staffQuery);

			return staffInOtherFactory;
		}

		ZGuid PrepareStaffTestData(int groupNumbers, string groupRoleName)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TestStaff001";
			staff.GS_Code = "T01";

			switch (groupRoleName)
			{
				case DbRoleTypes.DbDataWriterRole:
					var fixedGroupDatabaseDevleoper = Factory.Load<GlbGroup>(GlbGroup.DbDeveloperGroupPK);
					fixedGroupDatabaseDevleoper.Staff.Add(staff);
					break;
				case DbRoleTypes.CwRestrictedReaderRole:
					var fixedGroupDatabaseReader = Factory.Load<GlbGroup>(GlbGroup.DbReaderGroupPK);
					fixedGroupDatabaseReader.Staff.Add(staff);
					break;
				case DbRoleTypes.DbBackupOperatorRole:
					var fixedGroupBackupDeveloper = Factory.Load<GlbGroup>(GlbGroup.BackupOperatorGroupPK);
					fixedGroupBackupDeveloper.Staff.Add(staff);
					break;
			}

			for (var i = 0; i < groupNumbers; i++)
			{
				var flexibleGroup = Factory.NewWithValidTestData<GlbGroup>();
				flexibleGroup.GG_Code = $"G_Test_{i}";

				var flexibleGroupRole = Factory.NewWithValidTestData<GlbGroupRole>();
				flexibleGroupRole.GGR_GG_Group = flexibleGroup.PK;
				flexibleGroupRole.GGR_RoleName = groupRoleName;
				flexibleGroup.Staff.Add(staff);
			}

			Factory.Save();

			return staff.PK;
		}

		public void TestCheckGS_DepartureDate()
		{
			ZDateTime testDate = ZDateTime.Now;

			Bizo.GS_EmploymentDate = ZDateTime.Empty;
			Bizo.GS_DepartureDate = testDate;
			AssertNoErrors("Employement date not entered, departure date should NOT have errors", Bizo.GS_DepartureDateInfo);

			Bizo.GS_EmploymentDate = ZDateTime.Invalid;
			Bizo.GS_DepartureDate = testDate;
			AssertNoErrors("Employement date invalid, departure date should NOT have errors", Bizo.GS_DepartureDateInfo);

			Bizo.GS_EmploymentDate = testDate.AddDays(2);
			Bizo.GS_DepartureDate = testDate;
			AssertHasErrors("Employement date after departure date, departure date should have errors", Bizo.GS_DepartureDateInfo);

			Bizo.GS_EmploymentDate = testDate;
			Bizo.GS_DepartureDate = testDate.AddDays(2);
			AssertNoErrors("Employement date before departure date, departure date should NOT have errors", Bizo.GS_DepartureDateInfo);

			Bizo.GS_EmploymentDate = testDate;
			Bizo.GS_DepartureDate = ZDateTime.Invalid;
			AssertHasErrors("Departure date invalid, should have errors", Bizo.GS_DepartureDateInfo);
		}

		public void TestValidateGS_Birthdate()
		{
			Bizo.GS_Birthdate = ZDateTime.Now.Date.AddYears(-20);
			AssertNoErrors("Date of Birth should not have errors.", Bizo.GS_BirthdateInfo);

			Bizo.GS_Birthdate = ZDateTime.Now.Date;
			AssertNoErrors("Date of Birth should not have errors.", Bizo.GS_BirthdateInfo);

			Bizo.GS_Birthdate = ZDate.Today.AddDays(1);
			AssertHasError("Date of Birth should have errors", Bizo.GS_BirthdateInfo, "Birthdate cannot be in the future.");

			Bizo.GS_Birthdate = ZDate.Invalid;
			AssertHasError("Date of Birth should have errors", Bizo.GS_BirthdateInfo, "Enter a valid Date of Birth.");
		}

		public void TestValidateGS_LoginName()
		{
			Bizo.GS_LoginName = "";
			Assert("GS_LoginName is empty, expecting error", Bizo.GS_LoginNameInfo.HasErrors());

			Bizo.GS_LoginName = "H";
			Assert("GS_LoginName is shorter than 2 characters, expecting error", Bizo.GS_LoginNameInfo.HasNotifications());

			Bizo.GS_LoginName = "Hello";
			Assert("GS_LoginName is longer than 2 characters, not expecting error", !Bizo.GS_LoginNameInfo.HasNotifications());

			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "Hello";

			Assert("GS_LoginName is not unique, expecting error", staff2.GS_LoginNameInfo.HasNotifications());
		}

		public void TestValidateGS_LoginName_Prefix()
		{
			Bizo.GS_LoginName = "McLaren";
			Bizo.GS_CanLogin = true;
			Bizo.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			AssertNoErrors("GS_LoginName not expecting error", Bizo.GS_LoginNameInfo);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().UserLoginPrefix = "p1.";

			Bizo.RunPreSaveValidation();
			AssertHasError("GS_LoginName expecting error", Bizo.GS_LoginNameInfo, "Login Name must begin with 'p1.'.");

			Bizo.GS_CanLogin = false;
			Bizo.RunPreSaveValidation();
			AssertNoErrors("GS_LoginName not expecting error", Bizo.GS_LoginNameInfo);

			Bizo.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Bizo.RunPreSaveValidation();
			AssertHasError("GS_LoginName expecting error", Bizo.GS_LoginNameInfo, "Login Name must begin with 'p1.'.");

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			Bizo.RunPreSaveValidation();
			AssertNoErrors("GS_LoginName not expecting error", Bizo.GS_LoginNameInfo);
		}

		public void TestValidateGS_LoginName_ADEnabledAndDBAccess()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var dbStaff = Factory.NewWithValidTestData<GlbStaff>();
			dbStaff.GS_LoginName = "dbuser";
			dbStaff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var nonDbStaff = Factory.NewWithValidTestData<GlbStaff>();
			nonDbStaff.GS_LoginName = "nondbuser";
			nonDbStaff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();
			dbStaff.IsDatabaseDeveloper = true; // don't do this before save as it will cause creation of sql user

			dbStaff.GS_LoginName = "maggot";
			dbStaff.RunPreSaveValidation();
			AssertHasError("Rename is not allowed for db user", dbStaff.GS_LoginNameInfo, "Login Name cannot be changed while the staff has database access and Active Directory Integration is enabled. Please disable database access and try again.");

			dbStaff.GS_LoginName = "DBUser";
			dbStaff.RunPreSaveValidation();
			AssertNoErrors("Case change is allowed for db user", nonDbStaff.GS_LoginNameInfo);

			nonDbStaff.GS_LoginName = "manggo";
			nonDbStaff.RunPreSaveValidation();
			AssertNoErrors("Rename is allowed for non-db user", nonDbStaff.GS_LoginNameInfo);

			nonDbStaff.GS_LoginName = "NonDBUser";
			nonDbStaff.RunPreSaveValidation();
			AssertNoErrors("Case change is allowed for non-db user", nonDbStaff.GS_LoginNameInfo);
		}

		public void TestAreDifferentAccent()
		{
			AssertAccentDifferent("A", "A", false);
			AssertAccentDifferent("A", "a", false);
			AssertAccentDifferent("A", "E", false);
			AssertAccentDifferent("AE", "AE", false);
			AssertAccentDifferent("AE", "ae", false);
			AssertAccentDifferent("AE", "Æ", true);
			AssertAccentDifferent("AE", "æ", true);
			AssertAccentDifferent("AE", "AÉ", true);
			AssertAccentDifferent("AE", "ÁE", true);
			AssertAccentDifferent("ß", "ss", true);
			AssertAccentDifferent("à", "a", true);
			AssertAccentDifferent("aeßOE!", "Æssœ!", true);

			void AssertAccentDifferent(string a, string b, bool expected)
			{
				var sameInEnglish = GlbStaffValidationReal.AreSameWhenTransliterateToEnglish(a, b);
				var differentString = !string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

				CombineAssertions(() =>
				{
					AssertEquals($"Different string but same in English: {a} vs {b}", expected, differentString && sameInEnglish);
					AssertEquals($"AreDifferentAccent: {a} vs {b}", expected, GlbStaffValidationReal.AreDifferentAccent(a, b));
				});
			}
		}

		public void TestValidateGS_LoginName_AccentConflicts()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var directorySearcherProviderMock = new Mock<IDirectorySearcherProvider>(MockBehavior.Strict);
			var directorySearcherMock = new Mock<IDirectorySearcher>();
			directorySearcherProviderMock.Setup(a => a.GetDirectorySearcher(It.IsAny<IDomainCredentials>(), It.IsAny<bool>())).Returns(directorySearcherMock.Object);

			var loginName = "TestAE";
			var loginNameAccent1 = "TestÆ";
			var loginNameAccent2 = "TestÁÉ";
			var loginNameAnother = "TestAnother";
			var groupName = "GroupA";

			var directoryEntryUser = DummyDirectoryEntryWrapper.CreateUser(loginName);

			//This is to mock the REAL behaviour in AD where FindUser with username containing accent will return the same user without accent
			directorySearcherMock.Setup(a => a.FindUser(loginName, string.Empty)).Returns(directoryEntryUser);
			directorySearcherMock.Setup(a => a.FindUser(loginNameAccent1, string.Empty)).Returns(directoryEntryUser);
			directorySearcherMock.Setup(a => a.FindUser(loginNameAccent2, string.Empty)).Returns(directoryEntryUser);
			directorySearcherMock.Setup(a => a.FindUser(loginNameAnother, string.Empty)).Returns(DummyDirectoryEntryWrapper.CreateUser("another"));

			directorySearcherMock.Setup(a => a.FindGroup(groupName, string.Empty)).Returns(DummyDirectoryEntryWrapper.CreateGroup(groupName));

			using (ObjectFactory.Substitute(directorySearcherProviderMock.Object))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = loginName;
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
				Factory.Save();

				staff.GS_LoginName = loginNameAccent1;
				AssertNoErrors("Should allow accent change if staff is not linked", staff.GS_LoginNameInfo);

				staff.GS_LoginName = loginNameAnother;
				AssertNoErrors("Should allow rename to another AD user if it is not linked", staff.GS_LoginNameInfo);

				//Staff has been linked
				staff.GS_LoginName = loginName;
				staff.GS_ActiveDirectoryObjectGuid = directoryEntryUser.Guid;
				Factory.Save();

				staff.GS_LoginName = loginNameAccent1;
				AssertHasError("Should not allow accent change when it is linked", staff.GS_LoginNameInfo, string.Format(@"Please specify a different Login Name as Active Directory will not accept this Login Name due to the difference only in the diacritic marks (e.g. à to a) or ligature (e.g. Æ to AE or ß to ss).
({2}: {0}, Active Directory: {1})", loginNameAccent1, loginName, Core.Constants.ProductName));

				staff.GS_LoginName = loginNameAccent2;
				AssertHasError("Should not allow accent change when it is linked", staff.GS_LoginNameInfo, string.Format(@"Please specify a different Login Name as Active Directory will not accept this Login Name due to the difference only in the diacritic marks (e.g. à to a) or ligature (e.g. Æ to AE or ß to ss).
({2}: {0}, Active Directory: {1})", loginNameAccent2, loginName, Core.Constants.ProductName));

				staff.GS_LoginName = loginNameAnother;
				AssertHasError("Should not allow rename to another AD user when it is linked", staff.GS_LoginNameInfo, "Please specify a different Login Name as it has been used by another user in Active Directory.");

				staff.GS_LoginName = loginName.ToUpper();
				AssertNoErrors("Should allow case change", staff.GS_LoginNameInfo);

				staff.GS_LoginName = loginName + "X";
				AssertNoErrors("Should allow rename if no conflict", staff.GS_LoginNameInfo);

				Factory.Save();
				//after rename to TestAEX, and now rename back to TestAE before sync should be accepted
				staff.GS_LoginName = loginName;
				AssertNoErrors("Should allow rename back to the original name", staff.GS_LoginNameInfo);

				//Before sync, rename to accent should still be rejected
				staff.GS_LoginName = loginNameAccent1;
				AssertHasError("Should not allow accent change when it is linked", staff.GS_LoginNameInfo, string.Format(@"Please specify a different Login Name as Active Directory will not accept this Login Name due to the difference only in the diacritic marks (e.g. à to a) or ligature (e.g. Æ to AE or ß to ss).
({2}: {0}, Active Directory: {1})", loginNameAccent1, loginName, Core.Constants.ProductName));

				staff.GS_LoginName = groupName;
				AssertHasError("Should not allow rename to an existing AD group", staff.GS_LoginNameInfo, "Please specify a different Login Name as it has been used by a group in Active Directory.");

				//AD Disable, rename to different accent is allowed
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
				staff.GS_LoginName = loginNameAccent2;
				AssertNoErrors("Should allow accent change when AD Disabled", staff.GS_LoginNameInfo);
				Factory.Save();

				// Now we start with CW and AD login already in different accent
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

				// Accent change is allowed if it is same with AD login name
				staff.GS_LoginName = loginName;
				AssertNoErrors("Should allow accent change if it is match with AD logon name", staff.GS_LoginNameInfo);

				// Accent change is not allowed if it is different to the AD login name
				staff.GS_LoginName = loginNameAccent1;
				AssertHasError("Should not allow accent change when it is linked", staff.GS_LoginNameInfo, string.Format(@"Please specify a different Login Name as Active Directory will not accept this Login Name due to the difference only in the diacritic marks (e.g. à to a) or ligature (e.g. Æ to AE or ß to ss).
({2}: {0}, Active Directory: {1})", loginNameAccent1, loginName, Core.Constants.ProductName));

				//Set CW login to be totally different
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
				staff.GS_LoginName = "xxx";
				AssertNoErrors("Rename to xxx", staff.GS_LoginNameInfo);
				Factory.Save();
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

				// Not allow new login name to have different accent to AD login name
				staff.GS_LoginName = loginNameAccent1;
				AssertHasError("Should not accept new login name with different accent to AD login name", staff.GS_LoginNameInfo, string.Format(@"Please specify a different Login Name as Active Directory will not accept this Login Name due to the difference only in the diacritic marks (e.g. à to a) or ligature (e.g. Æ to AE or ß to ss).
({2}: {0}, Active Directory: {1})", loginNameAccent1, loginName, Core.Constants.ProductName));

				staff.GS_LoginName = loginNameAccent2;
				AssertHasError("Should not accept new login name with different accent to AD login name", staff.GS_LoginNameInfo, string.Format(@"Please specify a different Login Name as Active Directory will not accept this Login Name due to the difference only in the diacritic marks (e.g. à to a) or ligature (e.g. Æ to AE or ß to ss).
({2}: {0}, Active Directory: {1})", loginNameAccent2, loginName, Core.Constants.ProductName));

				// Allow if it is same as AD loginname
				staff.GS_LoginName = loginName;
				AssertNoErrors("Should accpet if it is same as AD login", staff.GS_LoginNameInfo);
			}
		}

		public void TestValidateGS_LoginName_ADUserAlreadyLinkedToAnotherStaff()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var directorySearcherProviderMock = new Mock<IDirectorySearcherProvider>(MockBehavior.Strict);
			var directorySearcherMock = new Mock<IDirectorySearcher>();
			directorySearcherProviderMock.Setup(a => a.GetDirectorySearcher(It.IsAny<IDomainCredentials>(), It.IsAny<bool>())).Returns(directorySearcherMock.Object);

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Elijah.Wood");

			directorySearcherMock.Setup(a => a.FindUser("Elijah.Wood", string.Empty)).Returns(directoryEntry);
			directorySearcherMock.Setup(a => a.FindUser("Another.Guy", string.Empty)).Returns(DummyDirectoryEntryWrapper.CreateUser("Another.Guy"));
			directorySearcherMock.Setup(a => a.FindGroup("Managers", string.Empty)).Returns(DummyDirectoryEntryWrapper.CreateGroup("Managers"));

			using (ObjectFactory.Substitute(directorySearcherProviderMock.Object))
			{
				var existingStaff = Factory.NewWithValidTestData<GlbStaff>();
				existingStaff.GS_LoginName = "test1";
				existingStaff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;

				Factory.Save();

				//Conflict with linked user
				var newStaff1 = Factory.New<GlbStaff>();
				newStaff1.GS_LoginName = "Elijah.Wood";
				AssertHasError(newStaff1.GS_LoginNameInfo, "Please specify a different Login Name as it has been used by an Active Directory user which has been linked to another staff member.");

				//No conflict with not linked but existing user
				newStaff1.GS_LoginName = "Another.Guy";
				AssertNoErrors(newStaff1.GS_LoginNameInfo);

				//No conflict at all
				newStaff1.GS_LoginName = "Test";
				AssertNoErrors(newStaff1.GS_LoginNameInfo);

				//Conflict with group existing in AD
				var newStaff2 = Factory.New<GlbStaff>();
				newStaff2.GS_LoginName = "Managers";
				AssertHasError(newStaff2.GS_LoginNameInfo, "Please specify a different Login Name as it has been used by a group in Active Directory.");

				//No conflict
				newStaff2.GS_LoginName = "Managers2";
				AssertNoErrors(newStaff2.GS_LoginNameInfo);
			}
		}

		public void TestValidateGS_LoginName_ADEnabled_WithCOMException()
		{
			var expectedMessage = string.Format(@"Cannot connect to Active Directory: {0}.
Please contact your system administrator or try again later.", "blah");
			CheckExceptionOnGS_LoginName(expectedMessage, new COMException("blah"));
		}

		public void TestValidateGS_LoginName_ADEnabled_WithInvalidOUException()
		{
			var expectedMessage = string.Format(@"Cannot connect to Active Directory: Invalid OU: {0}.
Please contact your system administrator or try again later.", TestConstants.InvalidOU);
			CheckExceptionOnGS_LoginName(expectedMessage, new InvalidOUException(TestConstants.InvalidOU));
		}

		public void TestValidateGS_LoginName_ADEnabled_WithDirectoryServicesException()
		{
			CheckExceptionOnGS_LoginName(@"Cannot connect to Active Directory: domain1.
Please contact your system administrator or try again later.",
				new DirectoryServicesException("domain1"));
		}

		void CheckExceptionOnGS_LoginName(string expectedMessage, Exception exception)
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var directorySearcherProviderMock = new Mock<IDirectorySearcherProvider>(MockBehavior.Strict);
			var directorySearcherMock = new Mock<IDirectorySearcher>();
			directorySearcherProviderMock.Setup(a => a.GetDirectorySearcher(It.IsAny<IDomainCredentials>(), It.IsAny<bool>())).Returns(directorySearcherMock.Object);

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Elijah.Wood");

			directorySearcherMock.Setup(a => a.FindUser("Elijah.Wood", string.Empty)).Throws(exception);

			using (ObjectFactory.Substitute(directorySearcherProviderMock.Object))
			{
				var existingStaff = Factory.NewWithValidTestData<GlbStaff>();
				existingStaff.GS_LoginName = "test1";
				existingStaff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;

				AssertNoError(existingStaff.GS_LoginNameInfo, expectedMessage);

				Factory.Save();

				existingStaff.GS_LoginName = "Elijah.Wood";
				AssertHasError(existingStaff.GS_LoginNameInfo, expectedMessage);
			}
		}

		public void TestValidateDBAccessFields()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var dbStaff = Factory.NewWithValidTestData<GlbStaff>();
			dbStaff.GS_LoginName = "dbStaff";
			dbStaff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			// don't do this before save as it will cause creation of sql user, which will cause unnecessary grief  
			dbStaff.IsDatabaseDeveloper = true;
			dbStaff.IsBackupOperator = true;
			dbStaff.IsReadOnlyDBUser = true;

			dbStaff.RunPreSaveValidation();
			var errorMessage = "Login Name 'dbStaff' could not be found or is not accessible in Active Directory, please ensure the staff is synchronized to Active Directory and the Domain is accessible before enable this option. If the staff was created or renamed recently, please try again in a few minutes to allow it to be synchronized with Active Directory.";
			AssertHasError(dbStaff.IsDatabaseDeveloperInfo, errorMessage);
			AssertHasError(dbStaff.IsBackupOperatorInfo, errorMessage);
			AssertHasError(dbStaff.IsReadOnlyDBUserInfo, errorMessage);

			//AD User exists - no error
			var adUser = new Mock<IADUser>();
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(p => p.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			adUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);
			adUser.Setup(a => a.LoginName).Returns(dbStaff.GS_LoginName);
			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				dbStaff.RunPreSaveValidation();
				AssertNoErrors(dbStaff.IsDatabaseDeveloperInfo);
				AssertNoErrors(dbStaff.IsBackupOperatorInfo);
				AssertNoErrors(dbStaff.IsReadOnlyDBUserInfo);
			}

			adUser.VerifyAll();
		}

		public void TestValidateDBAccessFields_AdUpnAndSAMAccountNameDifferent()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var dbStaff = Factory.NewWithValidTestData<GlbStaff>();
			dbStaff.GS_LoginName = "dbStaff";
			dbStaff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			// don't do this before save as it will cause creation of sql user, which will cause unnecessary grief  
			dbStaff.IsDatabaseDeveloper = true;
			dbStaff.IsBackupOperator = true;
			dbStaff.IsReadOnlyDBUser = true;

			//AD User with different UPN and SAMAccountName
			var adUser = new Mock<IADUser>();
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(p => p.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			adUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);
			adUser.Setup(a => a.LoginName).Returns("something.else");
			adUser.Setup(a => a.SAMAccountName).Returns(dbStaff.GS_LoginName);

			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				dbStaff.RunPreSaveValidation();
				AssertNoErrors(dbStaff.IsDatabaseDeveloperInfo);
				AssertNoErrors(dbStaff.IsBackupOperatorInfo);
				AssertNoErrors(dbStaff.IsReadOnlyDBUserInfo);
			}

			adUser.VerifyAll();
		}

		public void TestValidateGS_CodeCannotBeChangedWhenEventsAssociated()
		{
			ZBool previousCachingSetting = Env.Security.CachingEnabled;

			GlbCompany companyToLogInTo = Factory.NewWithValidTestData<GlbCompany>();

			GlbStaff staffJim = Factory.NewWithValidTestData<GlbStaff>();
			staffJim.GS_Code = "...";
			AssertNoErrors("Jim has not logged in, GS_Code should not have an error when changed", staffJim.GS_CodeInfo);

			Factory.Save();

			Env.Security.CachingEnabled = false;
			using (Env.SetTemporaryUserContext(staffJim.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				try
				{
					companyToLogInTo.Logs.AddNew(Events.Login); //Add event for Jim logging in
					Factory.Save();

					staffJim.GS_Code = "PPP";
					AssertHasErrors("Jim has logged in, GS_Code should have an error when changed", staffJim.GS_CodeInfo);

					GlbStaff staffWithoutCodeEntered = Factory.New<GlbStaff>();
					staffWithoutCodeEntered.GS_Code = "{{";
					AssertNoErrors("Staff without initials yet entered and has not logged in, GS_Code should not have an error when changed", staffWithoutCodeEntered.GS_CodeInfo);
				}
				finally
				{
					Env.Security.CachingEnabled = previousCachingSetting;
				}
			}
		}

		public void TestValidateGS_UserAddress1()
		{
			Bizo.GS_UserAddress1 = "Hello";
			Assert("GS_UserAddress1 is not empty, not expecting error", !Bizo.GS_UserAddress1Info.HasNotifications());

			Bizo.GS_UserAddress1 = "";
			Assert("GS_UserAddress1 is empty, expecting error", Bizo.GS_UserAddress1Info.HasErrors());
		}

		public void TestValidateGS_UserAddress2()
		{
			Bizo.GS_UserAddress2 = "Blah";
			AssertNoWarnings(Bizo.GS_UserAddress2Info);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			Bizo.Validation.ValidateGS_UserAddress2();

			AssertHasWarning(Bizo.GS_UserAddress2Info, "Active Directory does not support multiple address values. This value will not be synced with Active Directory.");
		}

		public void TestValidateGS_City()
		{
			Bizo.GS_City = "Hello";
			Assert("GS_City is not empty, not expecting error", !Bizo.GS_CityInfo.HasNotifications());

			Bizo.GS_City = "";
			Assert("GS_City is empty for a non-resource, expecting error", Bizo.GS_CityInfo.HasErrors());
		}

		public void TestValidateGS_EmploymentDate()
		{
			Bizo.GS_EmploymentDate = ZDateTime.Invalid;
			Assert("GS_EmploymentDate is invalid. Expecting error", Bizo.GS_EmploymentDateInfo.HasErrors());

			Bizo.GS_EmploymentDate = ZDateTime.Now;
			Assert("GS_EmploymentDate is valid. NOT expecting error", !Bizo.GS_EmploymentDateInfo.HasErrors());

			Bizo.GS_EmploymentDate = ZDateTime.Empty;
			Assert("GS_EmploymentDate is empty. NOT Expecting error", !Bizo.GS_EmploymentDateInfo.HasErrors());

			Bizo.GS_EmploymentDate = new ZDateTime(1990, 1, 1);
			Assert("GS_EmploymentDate is > 10 years old, but still valid. NOT expecting error", !Bizo.GS_EmploymentDateInfo.HasErrors());
		}

		public void TestValidateStaffPassword()
		{
			Bizo.StaffConfirmPassword = "TEST123";
			Bizo.StaffPlainTextPassword = "TEST";
			Assert("Should have error to state that password does not match", Bizo.StaffPlainTextPasswordInfo.HasErrors());

			Bizo.StaffPlainTextPassword = "TEST123";
			Assert("No errors", !Bizo.StaffPlainTextPasswordInfo.HasErrors());

			Factory.Save();

			// Now test saved staff
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbStaff savedStaff = newFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, Bizo.PK));
			savedStaff.StaffPlainTextPassword = ZString.Empty;
			Assert("No errors", !Bizo.StaffPlainTextPasswordInfo.HasErrors());
		}

		public void TestValidationOfPasswordOnCannotLoginUser()
		{
			Bizo.GS_CanLogin = false;
			Factory.Save();
			AssertEquals("User marked as Cannot Login but we still need password", true, Bizo.StaffPlainTextPasswordInfo.HasErrors());

			Bizo.StaffPlainTextPassword = "TEST123";
			Bizo.StaffConfirmPassword = "TEST123";

			Factory.Save();
			AssertEquals("Password is set and Bizo.Password.HasErrors is false", false, Bizo.StaffPlainTextPasswordInfo.HasErrors());
		}

		public void TestValidateStaffConfirmPassword()
		{
			Bizo.StaffPlainTextPassword = "TEST123";
			Bizo.StaffConfirmPassword = "TESTABCDE";
			Assert("Should have error to state that password does not match", Bizo.StaffConfirmPasswordInfo.HasErrors());

			Bizo.StaffConfirmPassword = "TEST123";
			Assert("No errors", !Bizo.StaffConfirmPasswordInfo.HasErrors());

			Factory.Save();

			// Now test saved staff
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbStaff savedStaff = newFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, Bizo.PK));
			savedStaff.StaffConfirmPassword = ZString.Empty;
			Assert("No errors", !Bizo.StaffConfirmPasswordInfo.HasErrors());
		}

		public void TestUserPasswordValidatedAgainstRegistrySettings()
		{
			int passwordMinUpperAlphasBeforeTest = Env.Registry.PasswordMinUpperAlphas;
			Env.Registry.PasswordMinUpperAlphas = 4;
			Bizo.StaffPlainTextPassword = "password";
			Bizo.StaffConfirmPassword = "password";
			Bizo.RunPreSaveValidation();
			Assert("Password does not have minimum 4 upper case characters, should have errors", Bizo.StaffPlainTextPasswordInfo.HasErrors());

			Bizo.StaffPlainTextPassword = "PASSword";
			Bizo.StaffConfirmPassword = "PASSword";
			Bizo.RunPreSaveValidation();
			Assert("Password has the minimum 4 upper case characters, should NOT have errors", !Bizo.StaffPlainTextPasswordInfo.HasErrors());
			Env.Registry.PasswordMinUpperAlphas = passwordMinUpperAlphasBeforeTest;
		}

		public void TestValidateADPasswordPolicy()
		{
			var adUser = new Mock<IADUser>();
			adUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(a => a.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			staff.PasswordNeverChanges = false;
			staff.ChangePasswordAtNextLogin = false;
			staff.RunPreSaveValidation();
			AssertNoErrors(staff.PasswordNeverChangesInfo);
			AssertNoErrors(staff.ChangePasswordAtNextLoginInfo);
			AssertNoWarnings(staff.PasswordNeverChangesInfo);
			AssertNoWarnings(staff.ChangePasswordAtNextLoginInfo);
			Assert(!staff.PasswordNeverChangesInfo.ReadOnly);
			Assert(!staff.ChangePasswordAtNextLoginInfo.ReadOnly);

			staff.PasswordNeverChanges = true;
			staff.ChangePasswordAtNextLogin = false;
			staff.RunPreSaveValidation();
			AssertNoErrors(staff.PasswordNeverChangesInfo);
			AssertNoErrors(staff.ChangePasswordAtNextLoginInfo);
			AssertNoWarnings(staff.PasswordNeverChangesInfo);
			AssertNoWarnings(staff.ChangePasswordAtNextLoginInfo);
			Assert(!staff.PasswordNeverChangesInfo.ReadOnly);
			Assert(!staff.ChangePasswordAtNextLoginInfo.ReadOnly);

			staff.PasswordNeverChanges = true;
			staff.ChangePasswordAtNextLogin = true;
			staff.RunPreSaveValidation();
			AssertHasError(staff.PasswordNeverChangesInfo, "You cannot select both 'Change Password at Next Login' and 'Password Never Expires'.");
			AssertHasError(staff.ChangePasswordAtNextLoginInfo, "You cannot select both 'Change Password at Next Login' and 'Password Never Expires'.");
			AssertNoWarnings(staff.PasswordNeverChangesInfo);
			AssertNoWarnings(staff.ChangePasswordAtNextLoginInfo);
			Assert(!staff.PasswordNeverChangesInfo.ReadOnly);
			Assert(!staff.ChangePasswordAtNextLoginInfo.ReadOnly);

			staff.PasswordNeverChanges = false;
			staff.ChangePasswordAtNextLogin = true;
			staff.RunPreSaveValidation();
			AssertNoErrors(staff.PasswordNeverChangesInfo);
			AssertNoErrors(staff.ChangePasswordAtNextLoginInfo);
			AssertNoWarnings(staff.PasswordNeverChangesInfo);
			AssertNoWarnings(staff.ChangePasswordAtNextLoginInfo);
			Assert(!staff.PasswordNeverChangesInfo.ReadOnly);
			Assert(!staff.ChangePasswordAtNextLoginInfo.ReadOnly);

			staff.PasswordNeverChanges = false;
			staff.ChangePasswordAtNextLogin = false;
			staff.RunPreSaveValidation();
			AssertNoErrors(staff.PasswordNeverChangesInfo);
			AssertNoErrors(staff.ChangePasswordAtNextLoginInfo);
			AssertNoWarnings(staff.PasswordNeverChangesInfo);
			AssertNoWarnings(staff.ChangePasswordAtNextLoginInfo);
			Assert(!staff.PasswordNeverChangesInfo.ReadOnly);
			Assert(!staff.ChangePasswordAtNextLoginInfo.ReadOnly);

			adUser.VerifyAll();
		}

		public void TestValidationWontCrashInCertainEdgeCase()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			staff.GS_SystemCreateTimeUtc = ZDateTime.Empty;
			staff.GS_UserAddress1 = "sydney";
			AssertNoExceptionThrown(() => staff.RunPreSaveValidation());

			staff.GS_SystemCreateTimeUtc = ZDateTime.Invalid;
			staff.GS_UserAddress1 = "hobart";
			AssertNoExceptionThrown(() => staff.RunPreSaveValidation());
		}

		public void TestValidatePasswordNeverChangesOverwrittenByADGroupPolicyWarning()
		{
			var adUser = new Mock<IADUser>();
			adUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(a => a.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			//No warning when the effective value is false, user attribute is false and has no changes
			staff.RunPreSaveValidation();
			AssertNoWarnings(staff.PasswordNeverChangesInfo);

			//No warning when effective value is false and user attribute is changing to true
			adUser.Setup(m => m.PasswordDoesntExpireUserAttribute).Returns(false);
			staff.PasswordNeverChanges = true;
			staff.RunPreSaveValidation();
			AssertNoWarnings(staff.PasswordNeverChangesInfo);

			//Effective value is now mocked to true
			adUser.Setup(u => u.PasswordDoesntExpire).Returns(true);

			//Show warning when effective value is true but user attribute is false
			adUser.Setup(m => m.PasswordDoesntExpireUserAttribute).Returns(false);
			staff.RunPreSaveValidation();
			AssertHasWarning(staff.PasswordNeverChangesInfo, "When unticked, this setting could be overridden by Password Policy.");

			//Show warning when effective value is true and user attribute is changing to false
			adUser.Setup(m => m.PasswordDoesntExpireUserAttribute).Returns(true);
			staff.PasswordNeverChanges = false;
			staff.RunPreSaveValidation();
			AssertHasWarning(staff.PasswordNeverChangesInfo, "When unticked, this setting could be overridden by Password Policy.");

			adUser.VerifyAll();
		}

		public void TestADOneWaySyncPasswordSettingsWarnings()
		{
			var adUser = new Mock<IADUser>();
			adUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(a => a.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			ObjectFactory.Get<IADRegistry>().SyncMode = SyncMode.ADIsMaster;
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.OneWay;

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.StaffPasswordAndSignature.IsAllowed = false;
				AssertEquals("ChangePasswordAtNextLogin should be read only", true, staff.ChangePasswordAtNextLoginInfo.ReadOnly);
				AssertEquals("PasswordNeverChanges should be read only", true, staff.PasswordNeverChangesInfo.ReadOnly);

				staff.RunPreSaveValidation();

				AssertHasWarning("AD enabled", staff.ChangePasswordAtNextLoginInfo, "This setting should be set in the domain.");
				AssertHasWarning("AD enabled", staff.PasswordNeverChangesInfo, "This setting should be set in the domain.");
			}

			// No AD Integration - should not show AD 1Way sync'c warning
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.StaffPasswordAndSignature.IsAllowed = false;
				AssertEquals("ChangePasswordAtNextLogin should be read only", true, staff.ChangePasswordAtNextLoginInfo.ReadOnly);
				AssertEquals("PasswordNeverChanges should be read only", true, staff.PasswordNeverChangesInfo.ReadOnly);

				staff.RunPreSaveValidation();

				AssertNoWarnings("AD disabled", staff.ChangePasswordAtNextLoginInfo);
				AssertNoWarnings("AD disabled", staff.PasswordNeverChangesInfo);
			}

			adUser.VerifyAll();
		}

		public void TestValidateADPasswordPolicyWhenADUserDoesNotExist()
		{
			var adUser = new Mock<IADUser>();
			adUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(false);
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(a => a.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "coffeepot";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			staff.PasswordNeverChanges = false;
			staff.ChangePasswordAtNextLogin = false;
			staff.RunPreSaveValidation();
			AssertHasWarning(staff.PasswordNeverChangesInfo, "Could not retrieve the value of this field because the Active Directory User 'coffeepot' is missing or is not accessible, please contact your system administrator.");
			AssertHasWarning(staff.ChangePasswordAtNextLoginInfo, "Could not retrieve the value of this field because the Active Directory User 'coffeepot' is missing or is not accessible, please contact your system administrator.");
			Assert(staff.PasswordNeverChangesInfo.ReadOnly);
			Assert(staff.ChangePasswordAtNextLoginInfo.ReadOnly);

			adUser.VerifyAll();
		}

		public void TestValidateADPasswordPolicyWhenADEnabledButNotLinked()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "coffeepot";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			staff.PasswordNeverChanges = false;
			staff.ChangePasswordAtNextLogin = false;
			staff.RunPreSaveValidation();
			AssertHasWarning(staff.PasswordNeverChangesInfo, "This option is not synchronized as the staff has not been linked to Active Directory.");
			AssertHasWarning(staff.ChangePasswordAtNextLoginInfo, "This option is not synchronized as the staff has not been linked to Active Directory.");
			Assert(!staff.PasswordNeverChangesInfo.ReadOnly);
			Assert(!staff.ChangePasswordAtNextLoginInfo.ReadOnly);
		}

		public void TestValidateGroupsForLocalAdmin()
		{
			var localAdminForGroup = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			localAdminForGroup.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(staff.Groups[0].GG_Code);

			using (Env.SetTemporaryUserContext(localAdminForGroup.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.StaffModifyAll.IsAllowed = false;
				staff.RunPreSaveValidation();

				Assert(staff.StaffMemberBelongsToGroupsLabelTextInfo.HasError("User needs to be added to Local Administrator's group before it can be saved."));

				Env.Security.StaffModifyAll.IsAllowed = true;
				staff.RunPreSaveValidation();
				Assert(!staff.StaffMemberBelongsToGroupsLabelTextInfo.HasError("User needs to be added to Local Administrator's group before it can be saved."));

				Env.Security.StaffModifyAll.IsAllowed = false;
				Factory.Save();
				staff.RunPreSaveValidation();
				Assert(!staff.StaffMemberBelongsToGroupsLabelTextInfo.HasError("User needs to be added to Local Administrator's group before it can be saved."));
			}
		}

		public void TestValidatePasswords_WhenADIntegrationEnabled_ShouldNotRequirePasswords()
		{
			var staff = Factory.New<GlbStaff>();
			staff.StaffPlainTextPassword = ZString.Empty;
			staff.StaffConfirmPassword = ZString.Empty;

			AssertHasErrors(staff.StaffPlainTextPasswordInfo);
			AssertHasErrors(staff.StaffConfirmPasswordInfo);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			staff.Validation.ValidateAll();

			AssertNoErrors(staff.StaffPlainTextPasswordInfo);
			AssertNoErrors(staff.StaffConfirmPasswordInfo);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLargeGS_ProfilePhoto_WhenADIntegrationEnabled()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			var largeImage = new Bitmap(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\Testing\160kb.bmp"));
			var smallImage = new Bitmap(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\Testing\100kb.bmp"));

			staff.ProfileImage = largeImage;
			staff.Validation.ValidateGS_ProfilePhoto();
			AssertHasError(staff.GS_ProfilePhotoInfo, "This image is too large to synchronize with Active Directory. It should be an image with a size no greater than 100KB.");

			staff.ProfileImage = smallImage;
			staff.Validation.ValidateGS_ProfilePhoto();
			AssertNoError(staff.GS_ProfilePhotoInfo, "This image is too large to synchronize with Active Directory. It should be an image with a size no greater than 100KB.");

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;

			staff.ProfileImage = largeImage;
			staff.Validation.ValidateGS_ProfilePhoto();
			AssertNoError(staff.GS_ProfilePhotoInfo, "This image is too large to synchronize with Active Directory. It should be an image with a size no greater than 100KB.");
		}

		public void TestBADImageFormatGS_ProfilePhoto_WhenADIntegrationEnabled()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_ProfilePhoto = new ZBlob(new byte[] { 1, 2, 3 });
			staff.Validation.ValidateGS_ProfilePhoto();
			AssertHasError(staff.GS_ProfilePhotoInfo, "The image supplied is invalid or corrupt. It cannot be synchronized with Active Directory.");
		}

		public virtual void TestValidateGS_GE_HomeDepartment()
		{
			AssertHomeDepartmentError(true);
		}

		protected void AssertHomeDepartmentError(bool expValResult)
		{
			Bizo.GS_GE_HomeDepartment = ZGuid.Empty;
			AssertMandatoryValidationError(Bizo.GS_GE_HomeDepartmentInfo, expValResult);

			Bizo.Validation.ValidateGS_GE_HomeDepartment();
			AssertMandatoryValidationError(Bizo.GS_GE_HomeDepartmentInfo, expValResult);

			Bizo.GS_IsSystemAccount = true;
			Bizo.Validation.ValidateGS_GE_HomeDepartment();
			AssertMandatoryValidationError(Bizo.GS_GE_HomeDepartmentInfo, false);

			Bizo.GS_IsSystemAccount = false;
			Bizo.GS_IsResource = true;
			Bizo.Validation.ValidateGS_GE_HomeDepartment();
			AssertMandatoryValidationError(Bizo.GS_GE_HomeDepartmentInfo, false);
		}

		public virtual void TestValidateGS_GB_HomeBranch()
		{
			AssertHomeBranchError(true);
		}

		protected void AssertHomeBranchError(bool expValResult)
		{
			Bizo.GS_GB_HomeBranch = ZGuid.Empty;
			AssertMandatoryValidationError(Bizo.GS_GB_HomeBranchInfo, expValResult);

			Bizo.Validation.ValidateGS_GB_HomeBranch();
			AssertMandatoryValidationError(Bizo.GS_GB_HomeBranchInfo, expValResult);

			Bizo.GS_IsSystemAccount = true;
			Bizo.Validation.ValidateGS_GB_HomeBranch();
			AssertMandatoryValidationError(Bizo.GS_GB_HomeBranchInfo, false);

			Bizo.GS_IsSystemAccount = false;
			Bizo.GS_IsResource = true;
			Bizo.Validation.ValidateGS_GB_HomeBranch();
			AssertMandatoryValidationError(Bizo.GS_GB_HomeBranchInfo, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestValidateGS_RN_NKNationalityCode()
		{
			var initialUserContext = Env.CurrentUserContext;
			Env.SetUserContext(new UserContext("", Guid.Empty, Guid.Empty));

			try
			{
				AssertNoExceptionThrown(() => Bizo.GS_RN_NKNationalityCode = "AU");
				Assert("GS_RN_NKNationalityCode is not empty, not expecting error", !Bizo.GS_CityInfo.HasNotifications());
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		public void TestValidateDomainName()
		{
			//No domains in the registry
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Jim.Moriarty";
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			staff.DomainName = "";
			AssertNoErrors(staff.DomainNameInfo);
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			staff.DomainName = "";
			AssertNoErrors(staff.DomainNameInfo);

			//No domains in the registry but domain is set on the staff
			staff.DomainName = "fake.domain";
			AssertNoErrors(staff.DomainNameInfo);

			// 1 domain
			var domain1 = ObjectFactory.Get<IDomainCredentials>();
			domain1.DomainName = "domain1";
			var aDRegistry = ObjectFactory.Get<IADRegistry>();
			aDRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1 };
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			staff.DomainName = "";
			AssertNoErrors(staff.DomainNameInfo);
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			staff.DomainName = "";
			AssertNoErrors(staff.DomainNameInfo);

			// 2 domains
			var domain2 = ObjectFactory.Get<IDomainCredentials>();
			domain2.DomainName = "domain2";
			aDRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1, domain2 };
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			staff.DomainName = "";
			AssertHasWarning(staff.DomainNameInfo, "The default domain will be used during the next synchronization with Active Directory.");
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			staff.DomainName = "";
			AssertHasWarning(staff.DomainNameInfo, "The default domain will be used during the next synchronization with Active Directory.");

			staff.DomainName = "something";
			AssertHasError(staff.DomainNameInfo, "Enter a valid Domain Name.");

			staff.DomainName = "domain1";
			AssertNoErrors(staff.DomainNameInfo);

			staff.DomainName = "domain2";
			AssertNoErrors(staff.DomainNameInfo);
		}

		public void TestFieldsSyncedOneWayFromAD()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			AssertFieldsSyncedOneWayFromAD("Login1", shouldHaveWarnings: false, isActive: true, isSystem: false);

			//EnterpriseIsMaster + TwoWay => no warnings
			ObjectFactory.Get<IADRegistry>().SyncMode = SyncMode.EnterpriseIsMaster;
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.TwoWay;
			AssertFieldsSyncedOneWayFromAD("Login2", shouldHaveWarnings: false, isActive: true, isSystem: false);

			//EnterpriseIsMaster + OneWay => no warnings
			ObjectFactory.Get<IADRegistry>().SyncMode = SyncMode.EnterpriseIsMaster;
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.OneWay;
			AssertFieldsSyncedOneWayFromAD("Login3", shouldHaveWarnings: false, isActive: true, isSystem: false);

			//ADIsMaster + TwoWay => no warnings
			ObjectFactory.Get<IADRegistry>().SyncMode = SyncMode.ADIsMaster;
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.TwoWay;
			AssertFieldsSyncedOneWayFromAD("Login4", shouldHaveWarnings: false, isActive: true, isSystem: false);

			//ADIsMaster + OneWay => warnings
			ObjectFactory.Get<IADRegistry>().SyncMode = SyncMode.ADIsMaster;
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.OneWay;
			AssertFieldsSyncedOneWayFromAD("Login5", shouldHaveWarnings: true, isActive: true, isSystem: false);

			//ADIsMaster + OneWay + disabled staff => no warnings
			AssertFieldsSyncedOneWayFromAD("Login6", shouldHaveWarnings: false, isActive: false, isSystem: false);

			//ADIsMaster + OneWay + system staff => no warnings
			AssertFieldsSyncedOneWayFromAD("Login7", shouldHaveWarnings: false, isActive: true, isSystem: true);

			//Integration disabled => no warnings
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			AssertFieldsSyncedOneWayFromAD("Login8", shouldHaveWarnings: false, isActive: true, isSystem: false);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestAttemptToRemoveDbDeveloperAccessFromStaffThatOwnsSchemaInUserRepositoryResultsInValidationError()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				AdoTestUtils.DropDbIfExists(adminConnection, Db.DatabaseName + DbUserRepository.RepositoryDbSuffix);
				(new DbUserRepository()).CreateRepositoryDatabase();

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName);
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.IsDatabaseDeveloper = true;
				staff.GS_LoginName = "DbUserManagerTest_Staff'Login";
				new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");

				try
				{
					Factory.Save();

					sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None);

					DbUserManagerForTesting.CreateUserSchemaOnUserRepositoryDb(staff.GS_LoginName, adminConnection);

					AssertEquals(false, staff.IsDatabaseDeveloperInfo.HasError("Unable to drop user from Database role due to some existing database schema(s) owned by this user. Please address this first and then reload the form in order to remove the user from this role."));

					staff.IsDatabaseDeveloper = false;
					staff.RunPreSaveValidation();

					AssertEquals(true, staff.IsDatabaseDeveloperInfo.HasError("Unable to drop user from Database role due to some existing database schema(s) owned by this user. Please address this first and then reload the form in order to remove the user from this role."));
				}
				finally
				{
					// crean up user repository changes
					AdoTestUtils.DropDbIfExists(adminConnection, Db.DatabaseName + DbUserRepository.RepositoryDbSuffix);

					// Clean-up SQL Login
					staff.IsDatabaseDeveloper = false;
					Factory.Save();
					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);
				}
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestAttemptToDeactivateDbDeveloperStaffThatOwnsSchemaInUserRepositoryResultsInValidationError()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				AdoTestUtils.DropDbIfExists(adminConnection, Db.DatabaseName + DbUserRepository.RepositoryDbSuffix);
				(new DbUserRepository()).CreateRepositoryDatabase();

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName);
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.IsDatabaseDeveloper = true;
				staff.GS_LoginName = "DbUserManagerTest_Staff'Login";
				new DbUserManager().SetPasswordForStaff(staff, "P@ssW0rd!");

				try
				{
					Factory.Save();
					sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None);

					DbUserManagerForTesting.CreateUserSchemaOnUserRepositoryDb(staff.GS_LoginName, adminConnection);

					AssertEquals(false, staff.IsDatabaseDeveloperInfo.HasError("Unable to drop user from Database role due to some existing database schema(s) owned by this user. Please address this first and then reload the form in order to remove the user from this role."));

					staff.GS_IsActive = false;
					staff.RunPreSaveValidation();

					AssertEquals(true, staff.IsDatabaseDeveloperInfo.HasError("Unable to drop user from Database role due to some existing database schema(s) owned by this user. Please address this first and then reload the form in order to remove the user from this role."));
				}
				finally
				{
					// crean up user repository changes
					AdoTestUtils.DropDbIfExists(adminConnection, Db.DatabaseName + DbUserRepository.RepositoryDbSuffix);

					// Clean-up SQL Login
					staff.IsDatabaseDeveloper = false;
					Factory.Save();
					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);
				}
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestAttemptToDropStaffThatOwnsSchemaWithObjectsResultsInValidationError()
		{
			// LLZ: This test is irrelevant for when UseModernSqlSecuritySystem is on, because no dropping of schema is performed
			// This test is replaced with TestAttemptToDropStaffThatOwnsEmptySchemaResultsInValidationError
			// UseModernSqlSecuritySystem is removed, this test should be removed
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.IsDatabaseDeveloper = true;
			staff.GS_LoginName = "DbUserManagerTest_Staff'Login";
			try
			{
				Factory.Save();
				AssertEquals(false, staff.IsDatabaseDeveloperInfo.HasError("Unable to drop user from Database role due to database objects existing in schemas owned by this user. Please address this first and then reload the form in order to remove the user from this role."));

				using (var adminConnection = Db.NewAdminConnection())
				{
					DbUserManagerForTesting.CreateUserSchemaOnMainDb(staff.GS_LoginName, adminConnection);
				}

				DbUserManagerForTesting.CreateDummyTableForSchemaOnMainDb(staff.GS_LoginName, TestConnection);

				staff.IsDatabaseDeveloper = false;
				staff.RunPreSaveValidation();

				AssertEquals(true, staff.IsDatabaseDeveloperInfo.HasError("Unable to drop user from Database role due to database objects existing in schemas owned by this user. Please address this first and then reload the form in order to remove the user from this role."));
			}
			finally
			{
				// Clean-up SQL Login
				DbUserManagerForTesting.DropDummyTableForSchemaOnMainDb(staff.GS_LoginName, TestConnection);
				staff.IsDatabaseDeveloper = false;
				Factory.Save();
			}
		}

		void AssertFieldsSyncedOneWayFromAD(string initialLoginName, bool shouldHaveWarnings, bool isActive, bool isSystem)
		{
			var expectedWarningMessage = "This field is synchronized from Active Directory. Its value will be overridden next time the synchronization occurs.";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = initialLoginName;
			staff.GS_IsActive = isActive;
			staff.GS_IsSystemAccount = isSystem;
			Factory.Save();

			AssertEquals(false, staff.GS_CityInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_EmailAddressInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_FaxNumInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_FaxNum_FormattedInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_FullNameInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_HomePhone_FormattedInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_LoginNameInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_MobilePhone_FormattedInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_PagerInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_PostcodeInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_ProfilePhotoInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_StateInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_TitleInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_UserAddress1Info.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_WorkExtensionInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_WorkPhone_FormattedInfo.HasWarning(expectedWarningMessage));

			AssertEquals(false, staff.GS_IsActiveInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_UserAddress2Info.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_RN_NKCountryCodeInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_NextOfKinHomePhoneInfo.HasWarning(expectedWarningMessage));

			staff.GS_City = "ABC";
			staff.GS_EmailAddress = "email@domain.com";
			staff.GS_FaxNum_Formatted = "123456";
			staff.GS_FullName = "FullName";
			staff.GS_HomePhone_Formatted = "741258";
			staff.GS_LoginName = initialLoginName + "X";
			staff.GS_MobilePhone_Formatted = "852369";
			staff.GS_Pager = "ABC";
			staff.GS_Postcode = "1111";
			staff.GS_ProfilePhoto = new byte[] { 0, 1, 3 };
			staff.GS_State = "NSW";
			staff.GS_Title = "Title";
			staff.GS_UserAddress1 = "UserAddress1";
			staff.GS_WorkExtension = "123";
			staff.GS_WorkPhone_Formatted = "753951";

			staff.GS_UserAddress2 = "CBA";
			staff.GS_RN_NKCountryCode = "AU";
			staff.GS_NextOfKinHomePhone_Formatted = "0425465800";

			AssertEquals(shouldHaveWarnings, staff.GS_CityInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_EmailAddressInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_FaxNum_FormattedInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_FullNameInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_HomePhone_FormattedInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_LoginNameInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_MobilePhone_FormattedInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_PagerInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_PostcodeInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_ProfilePhotoInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_StateInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_TitleInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_UserAddress1Info.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_WorkExtensionInfo.HasWarning(expectedWarningMessage));
			AssertEquals(shouldHaveWarnings, staff.GS_WorkPhone_FormattedInfo.HasWarning(expectedWarningMessage));

			AssertEquals(false, staff.GS_IsActiveInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_UserAddress2Info.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_RN_NKCountryCodeInfo.HasWarning(expectedWarningMessage));
			AssertEquals(false, staff.GS_NextOfKinHomePhone_FormattedInfo.HasWarning(expectedWarningMessage));
		}

		protected override bool ShouldHaveADRelatedLoginNameErrors => true;

		protected override void TearDown()
		{
			TestConnection.ExecuteNonQuery($@"
IF EXISTS 
	(SELECT name  
	FROM sys.server_principals
	WHERE name = 'TestStaff001')
BEGIN
	DROP LOGIN [TestStaff001]
END");
		}
	}
}


