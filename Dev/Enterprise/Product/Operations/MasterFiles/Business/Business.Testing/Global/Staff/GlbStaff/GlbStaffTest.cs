using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.SpellCheck;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using WTG.Foundation.Cryptography.UserSecrets;
using WTG.NUnit;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaff))]
	partial class GlbStaffTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocumentMacroIgnore_Password()
		{
			var passwordHashInfo = typeof(GlbStaff).GetProperty("GS_PasswordHash");
			var passwordSaltInfo = typeof(GlbStaff).GetProperty("GS_PasswordSalt");
			var brokerPassword = typeof(GlbStaff).GetProperty("GS_BrokerPassword");
			var brokerWorkingPassword = typeof(GlbStaff).GetProperty("GS_BrokerWorkingPassword");

			Assert("GS_PasswordHash should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordHashInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
			Assert("GS_PasswordSalt should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordSaltInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
			Assert("GS_BrokerPassword should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(brokerPassword, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
			Assert("GS_BrokerWorkingPassword should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(brokerWorkingPassword, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
		}

		public void TestAllowEmptyPasswordForNewStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "some@ema.il";
			staff.GS_FullName = "full name";
			staff.GS_LoginName = "somelogin";
			staff.AllowEmptyPasswordForNewRecord();

			AssertEquals(0, staff.GS_PasswordHash.Length);

			Factory.Save();

			AssertEquals(0, staff.GS_PasswordHash.Length);
		}

		public void TestActivityTrackingStatusEDTEventWhenSave()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "some@ema.il";
			staff.GS_FullName = "full name";
			staff.GS_LoginName = "somelogin";
			Factory.Save();

			staff.GS_ActivityTrackingStatus = "Yes";
			Factory.Save();

			Assert("References message should match", staff.Logs.HasLogWith(StmALogSchema.SL_Reference, "Changed Enable Tracking from CMP to Yes."));
		}

		public void TestAllowPasswordChange()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = false;

			var staff = Factory.New<GlbStaff>();
			AssertEquals("Non-AD, Not DisableADPasswordChange", true, staff.AllowPasswordChange);

			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = true;
			AssertEquals("Non-AD, DisableADPasswordChange", true, staff.AllowPasswordChange);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			AssertEquals("AD, DisableADPasswordChange", false, staff.AllowPasswordChange);

			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = false;
			AssertEquals("AD, Not DisableADPasswordChange", true, staff.AllowPasswordChange);
		}

		public void TestGS_LastActivityDateInfoConcurrencyPolicy()
		{
			var staff = Factory.New<GlbStaff>();
			AssertEquals(ConcurrencyPolicy.Ignore, staff.GS_LastActivityDateInfo.ConcurrencyPolicy);
		}

		#region LoadFromLoginName

		public void TestLoadFromLoginName_StaffWithoutDomain()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "jon";
			staff.GS_DomainName = "";
			Factory.Save();

			AssertEquals(staff.PK, GlbStaff.LoadFromLoginName(Factory, "jon").PK);

			//Invalid cases
			AssertEquals(null, GlbStaff.LoadFromLoginName(Factory, "@jon"));
			AssertEquals(null, GlbStaff.LoadFromLoginName(Factory, "jon@"));
			AssertEquals(null, GlbStaff.LoadFromLoginName(Factory, "sam"));
		}

		public void TestLoadFromLoginName_StaffWithDomain()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "jon";
			staff.GS_DomainName = "domain1";
			Factory.Save();

			AssertEquals(staff.PK, GlbStaff.LoadFromLoginName(Factory, "jon").PK);
			AssertEquals(staff.PK, GlbStaff.LoadFromLoginName(Factory, "jon@domain1").PK);

			//Invalid cases
			AssertEquals(null, GlbStaff.LoadFromLoginName(Factory, "@jon"));
			AssertEquals(null, GlbStaff.LoadFromLoginName(Factory, "jon@"));
			AssertEquals(null, GlbStaff.LoadFromLoginName(Factory, "jon@domain"));
			AssertEquals(null, GlbStaff.LoadFromLoginName(Factory, "jon@domain1.com"));
			AssertEquals(null, GlbStaff.LoadFromLoginName(Factory, "jon@domain2"));
		}

		// This test only work when the unique key of GS_LoginName is extended to include GS_DomainName
		// Remove [DeveloperOnlyTest] after Work Item WI00195060
		[DeveloperOnlyTest]
		public void TestLoadFromLoginName_MultiDomains()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "jon";
			staff1.GS_DomainName = "domain1";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "jon";
			staff2.GS_DomainName = "domain2";
			Factory.Save();

			AssertExceptionThrown<UserExistsInMultipleDomainsException>("Should throw", @"User 'jon' exists in multiple domains, please enter one of the followings:
jon@domain1,
jon@domain2", () => GlbStaff.LoadFromLoginName(Factory, "jon"));

			AssertEquals(staff1.PK, GlbStaff.LoadFromLoginName(Factory, "jon@domain1").PK);
			AssertEquals(staff2.PK, GlbStaff.LoadFromLoginName(Factory, "jon@domain2").PK);
		}

		public void TestLoadFromLoginName_EnsureReloadFromDB()
		{
			var oldCity = "Hobart";
			var newCity = "Sydney";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "user";
			staff.GS_City = oldCity;
			Factory.Save();

			using (RowFactory.SetCachedTables(GlbStaff.Schema.TableName))
			{
				// Before change and to load into cache
				var staffReloadedFromNewFactory = GlbStaff.LoadFromLoginName(new BusinessObjectFactory(), staff.GS_LoginName);
				AssertEquals("Before change, old city from another factory", oldCity, staffReloadedFromNewFactory.GS_City);

				// Change from another instance or outside CW1
				var query = $@"UPDATE dbo.GlbStaff SET GS_City = @city, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_PK = '{staff.PK}'";
				using (var cmd = TestConnection.Command(query))
				{
					cmd.AddParameter("@city", System.Data.SqlDbType.NVarChar, newCity);
					cmd.ExecuteNonQuery();
				}

				staffReloadedFromNewFactory = GlbStaff.LoadFromLoginName(new BusinessObjectFactory(), staff.GS_LoginName);
				AssertEquals("New city from another factory", newCity, staffReloadedFromNewFactory.GS_City);
			}
		}

		#endregion

		#region Person

		public void TestCreateFromStaff()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();

			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "full name";
			person.PER_NameSuffix = "sfx";
			person.PER_NameTitle = "title";
			person.PER_Gender = "M";
			person.PER_HomeAddress1 = "address 1";
			person.PER_HomeAddress2 = "address 2";
			person.PER_City = "city";
			person.PER_Postcode = "12345";
			person.PER_State = "state";
			person.PER_RN_NKCountry = "AU";
			person.PER_MobilePhone = "123456789";
			person.PER_HomePhone = "987654321";
			person.PER_FaxNumber = "111";
			person.PER_BirthDate = ZDateTime.BrettsBirthday.Date;
			person.PER_FriendlyName = "friendly";
			person.PER_RN_NKNationalityCodeISO = "UA";
			person.PER_PreferredLanguage = "RSN";
			person.PER_DriversLicenseNumber = "drivers";
			person.PER_EmailAddress = "email@contact.com";
			person.PER_EmailAddress2 = "email@contact.com";
			person.PER_MobilePhone2 = "123456789";
			person.PER_Passport = "passport";

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(),
				GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var newFactory = new BusinessObjectFactory();
					var personLoaded = newFactory.Load<GlbPerson>(person.PK);

					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_LegalName);
					Assert(ZDate.Empty == personLoaded.PER_BirthDate);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_EmailAddress);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_EmailAddress2);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_Gender);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_MobilePhone);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_MobilePhone2);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_HomePhone);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_RN_NKNationalityCodeISO);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_PersonalInfo);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_HomeAddress1);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_HomeAddress2);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_City);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_State);
					Assert(personLoaded.ViewDeniedMessage == personLoaded.PER_Postcode);

					var staff = newFactory.New<GlbStaff>();
					staff.SetFromPerson(personLoaded);

					AssertEquals("full name", staff.GS_FullName);
					AssertEquals("sfx", staff.GS_NameSuffix);
					AssertEquals("title", staff.GS_NameTitle);
					AssertEquals("M", staff.GS_Gender);
					AssertEquals("address 1", staff.GS_UserAddress1);
					AssertEquals("address 2", staff.GS_UserAddress2);
					AssertEquals("city", staff.GS_City);
					AssertEquals("12345", staff.GS_Postcode);
					AssertEquals("state", staff.GS_State);
					AssertEquals("AU", staff.GS_RN_NKCountryCode);
					AssertEquals("123456789", staff.GS_MobilePhone);
					AssertEquals("987654321", staff.GS_HomePhone);
					AssertEquals("111", staff.GS_FaxNum);
					AssertEquals(ZDateTime.BrettsBirthday.Date, staff.GS_Birthdate);
					AssertEquals("friendly", staff.GS_FriendlyName);
					AssertEquals("UA", staff.GS_RN_NKNationalityCode);
					AssertEquals(Core.SharedConstants.Languages.English, staff.GS_WorkingLanguage);
					AssertEquals("", staff.GS_EmailAddress);
					AssertEquals("passport", staff.GS_Passport);
					AssertEquals("", staff.GS_Title);
					AssertEquals(person.PK, staff.GS_PER);
				}
			}
		}

		public void TestPersonIsCreated()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "almost full name";
			Factory.Save();

			AssertNotEquals(ZGuid.Empty, staff.GS_PER);

			var person = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbPerson>(staff.GS_PER);
			AssertNotNull(person);
			AssertEquals("almost full name", person.PER_FullName);

			staff = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GlbStaff>(staff.PK);
			AssertEquals(person.PK, staff.GS_PER);
		}

		public void TestPersonIsUpdated()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "almost full name";
			Factory.Save();

			var person = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbPerson>(staff.GS_PER);
			AssertNotNull(person);
			staff = person.Factory.Load<GlbStaff>(staff.PK);

			staff.GS_FullName = "new full name";
			staff.Factory.Save();

			person = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbPerson>(person.PK);
			AssertNotNull(person);
			AssertEquals("new full name", person.PER_FullName);
		}

		public void TestSetFromPerson_GS_Gender()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var person = Factory.NewWithValidTestData<GlbPerson>();

			person.PER_FullName = "full name";
			person.PER_Gender = Core.Constants.Genders.NotSpecified;
			person.PER_MobilePhone = "123456789";

			Factory.Save();

			staff.SetFromPerson(person);
			AssertEquals(Core.Constants.Genders.NotSpecified, staff.GS_Gender);

			person.PER_Gender = Core.Constants.Genders.Custom;
			staff.SetFromPerson(person);
			AssertEquals(Core.Constants.Genders.Custom, staff.GS_Gender);

			person.PER_Gender = Core.Constants.Genders.Man;
			staff.SetFromPerson(person);
			AssertEquals(Core.Constants.Genders.Man, staff.GS_Gender);
		}

		public void TestUpdateFromPerson()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "shouldnotchange@test.com";
			Factory.Save();

			var person = Factory.Load<GlbPerson>(staff.GS_PER);
			AssertNotNull(person);

			person.PER_FullName = "new full name";
			person.PER_Gender = "F";
			person.PER_NameSuffix = "mrs";
			person.PER_NameTitle = "new title";
			person.PER_HomeAddress1 = "new address 1";
			person.PER_HomeAddress2 = "new address 2";
			person.PER_City = "new city";
			person.PER_Postcode = "54321";
			person.PER_State = "new state";
			person.PER_RN_NKCountry = "NZ";
			person.PER_HomePhone = "987654321";
			person.PER_FaxNumber = "222";
			person.PER_BirthDate = ZDateTime.Today.Date;
			person.PER_RN_NKNationalityCodeISO = "ZZ";
			person.PER_EmailAddress = "newemail@contact.com";
			person.PER_EmailAddress2 = "newemail@contact.com";
			person.PER_Passport = "newpassport";

			staff.UpdateFromPerson(person);

			AssertEquals(person.PER_FullName, staff.GS_FullName);
			AssertEquals(person.PER_NameSuffix, staff.GS_NameSuffix);
			AssertEquals(person.PER_NameTitle, staff.GS_NameTitle);
			AssertEquals(person.PER_Gender, staff.GS_Gender);
			AssertEquals(person.PER_HomeAddress1, staff.GS_UserAddress1);
			AssertEquals(person.PER_HomeAddress2, staff.GS_UserAddress2);
			AssertEquals(person.PER_City, staff.GS_City);
			AssertEquals(person.PER_Postcode, staff.GS_Postcode);
			AssertEquals(person.PER_State, staff.GS_State);
			AssertEquals(person.PER_MobilePhone, staff.GS_MobilePhone);
			AssertEquals(person.PER_HomePhone, staff.GS_HomePhone);
			AssertEquals(person.PER_FaxNumber, staff.GS_FaxNum);
			AssertEquals(person.PER_BirthDate, staff.GS_Birthdate);
			AssertEquals(person.PER_RN_NKNationalityCodeISO, staff.GS_RN_NKNationalityCode);
			AssertEquals(person.PER_RN_NKCountry, staff.GS_RN_NKCountryCode);
			AssertEquals(person.PER_Passport, staff.GS_Passport);
			AssertEquals("shouldnotchange@test.com", staff.GS_EmailAddress);
		}

		public void TestUpdateFromPerson_MobileOnlyUpdatedIfSame()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			var person = Factory.Load<GlbPerson>(staff.GS_PER);
			Factory.Save();

			var staffMobile = "123456789";
			staff.GS_MobilePhone = staffMobile;
			var personMobile = "999999999";
			person.PER_MobilePhone = personMobile;
			Factory.Save();
			AssertEquals("Staff mobile should not change as it is different to old person mobile", staffMobile, staff.GS_MobilePhone);

			staff.GS_MobilePhone = personMobile;
			var newPersonMobile = "000000000";
			person.PER_MobilePhone = newPersonMobile;
			Factory.Save();
			AssertEquals("Staff mobile should change as it is the same as old person mobile", newPersonMobile, staff.GS_MobilePhone);
		}

		public void TestContactSetterOnSaving()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			GlbPerson.CreateFromStaff(Factory, staff);
			staff.GS_Gender = "F";
			Factory.Save();

			AssertEquals("F", staff.Person.PER_GenderInternal);
		}

		public void TestPersonSetOnSaving()
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				DataRegistry.Instance.ProductivityWiseModeEnabled = true;
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_PER = ZGuid.Empty;

				AssertNoExceptionThrown(Factory.Save);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestSettingFullNameInvokesDeduplication()
		{
			//Arrange
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var dedupeStarted = false;
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			var glbStaff = glbPerson.StaffCollection.AddNew();
			glbStaff.GS_PER = glbPerson.PK;
			glbStaff.GS_Code = "STN";
			EventHandler eventStarted = (o, e) => { dedupeStarted = true; };
			glbStaff.Person.DeduplicationStarted += eventStarted;
			((IDeduplicatable)glbStaff.Person).ShouldRunDeduplication = true;

			var reg = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				//Act
				glbStaff.GS_FullName = "Dr Crayfish";

				//Assert
				AssertEquals(true, dedupeStarted);
			}
			finally
			{
				glbStaff.Person.DeduplicationStarted -= eventStarted;
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);
			}
		}

		#endregion

		#region IUser
		public void TestEdiSupport()
		{
			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_Code = "SUP";
			glbStaff.GS_LoginName = "edi.support";
			glbStaff.GS_FullName = "This is for WiseCloud Support";
			glbStaff.GS_EmailAddress = "edi.support@wisetechgloba.com";
			glbStaff.GS_WorkPhone = "9999 9999";
			glbStaff.GS_FaxNum = "1111 1111";
			glbStaff.GS_Title = "Mr";

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				AssertEquals("GS_IsActiveReadOnly", true, glbStaff.GS_IsActiveReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				AssertEquals("GS_IsActiveReadOnly", false, glbStaff.GS_IsActiveReadOnly);
			}
		}

		public void TestIUserImplementation()
		{
			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_Code = "WOW";
			glbStaff.GS_LoginName = "sysadmin";
			glbStaff.GS_FullName = "WILD CHILD";
			glbStaff.GS_EmailAddress = "wild@wow.com";
			glbStaff.GS_WorkPhone = "9999 9999";
			glbStaff.GS_FaxNum = "1111 1111";
			glbStaff.GS_Title = "CHAMPION";
			glbStaff.GS_LastActivityDate = new ZDateTime(2021, 7, 9, 0, 2, 25, DateTimeKind.Utc);

			IUser user = glbStaff;
			CombineAssertions(() =>
			{
				AssertEquals("user.EmailAddress", "wild@wow.com", user.EmailAddress);
				AssertEquals("user.FullName", "WILD CHILD", user.FullName);
				AssertEquals("user.Initials", "WOW", user.Initials);
				AssertEquals("user.LoginName", "sysadmin", user.LoginName);
				AssertEquals("user.Title", "CHAMPION", user.Title);
				AssertEquals("user.WorkPhone", "9999 9999", user.WorkPhone);
				AssertEquals("user.Fax", "1111 1111", user.Fax);
				AssertEquals("user.IsActive", true, user.IsActive);
				AssertEquals("user.IsController", false, user.IsController);
				AssertEquals("user.IsRobot", false, user.IsRobot);
				AssertEquals("user.IsDeveloper", false, user.IsDeveloper);
				AssertEquals("user.IsLockedOut", false, user.IsLockedOut);
				AssertEquals("user.IsResource", false, user.IsResource);
				AssertEquals("user.IsSystemAccount", false, user.IsSystemAccount);
				AssertEquals("user.Language", "EN", user.Language);
				AssertEquals("user.PK", glbStaff.PK, user.PK);
				AssertEquals("user.LastActivityDateTimeUtc", new DateTime(2021, 7, 9, 0, 2, 25, DateTimeKind.Utc), user.LastActivityDateTimeUtc);

				AssertNoExceptionThrown("user.InitialsAndDateTime", () =>
				{
					var dump = user.InitialsAndDateTime;
				});

				glbStaff.GS_LastActivityDate = ZDateTime.Empty;
				AssertNoExceptionThrown("user.LastActivityDateTimeUtc trows no exception on empty GS_LastActivityDate", () => { _ = user.LastActivityDateTimeUtc; });
			});
		}

		public void TestIUserIsBatchProcessor()
		{
			var glbStaff = Factory.New<GlbStaff>();

			glbStaff.GS_LoginName = User.ServiceUserName;
			glbStaff.GS_Code = User.ServiceUserCode;
			glbStaff.GS_EmailAddress = "abc@cbd.com";
			glbStaff.GS_FullName = "batch processor";
			glbStaff.GS_UserAddress1 = "add";
			glbStaff.GS_City = "City";
			glbStaff.GS_State = "State";
			glbStaff.GS_WorkPhone = "12345678";
			glbStaff.GS_FaxNum = "88887777";
			glbStaff.GS_IsController = true;
			glbStaff.GS_IsSystemAccount = false;
			glbStaff.GS_IsDeveloper = false;
			glbStaff.GS_IsRobot = true;

			var user = (IUser)glbStaff;

			AssertEquals("LoginName", User.ServiceUserName, user.LoginName);
			AssertEquals("Initials", "~BP", user.Initials);
			AssertEquals("EmailAddress", "abc@cbd.com", user.EmailAddress);
			AssertEquals("FullName", "batch processor", user.FullName);
			AssertEquals("WorkPhone", "12345678", user.WorkPhone);
			AssertEquals("WorkPhone", "88887777", user.Fax);
			AssertEquals("IsController", true, user.IsController);
			AssertEquals("IsRobot", true, user.IsRobot);
			AssertEquals("IsSystemAccount", false, user.IsSystemAccount);
			AssertEquals("IsDeveloper", false, user.IsDeveloper);
			AssertEquals("IsBatchProcessor", true, user.IsBatchProcessor);

			glbStaff.GS_Code = User.InterchangeUserCode;
			AssertEquals("IsBatchProcessor", true, user.IsBatchProcessor);

			glbStaff.GS_Code = "ABC";
			AssertEquals("IsBatchProcessor", false, user.IsBatchProcessor);
		}

		public void TestIUserIsSupportUser()
		{
			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_LoginName = "TestLogin";
			var user = (IUser)glbStaff;
			AssertEquals("IsSupportUser", false, user.IsSupportUser);

			glbStaff.GS_LoginName = User.SupportUserName;
			AssertEquals("IsSupportUser", true, user.IsSupportUser);

			glbStaff.GS_LoginName = User.SupportUserName.ToLower();
			AssertEquals("IsSupportUser", true, user.IsSupportUser);
		}

		public void TestIUserVerifyPassword()
		{
			var glbStaff = Factory.New<GlbStaff>();

			glbStaff.GS_LoginName = "TestLogin";
			glbStaff.GS_Code = "TL";
			glbStaff.StaffPlainTextPassword = "password";
			Factory.Save();

			var user = (IUser)glbStaff;
			Assert("Password match", user.VerifyPassword("password"));
			Assert("Wrong password", !user.VerifyPassword("rubbish"));
		}

		public void TestIUserIsLockedOut()
		{
			var glbStaff = Factory.New<GlbStaff>();

			glbStaff.GS_LoginName = "TestLogin";
			glbStaff.GS_Code = "TL";

			var user = (IUser)glbStaff;
			Assert("Null lockout time", !user.IsLockedOut);

			var person = Factory.New<GlbPerson>();
			person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddMinutes(10);
			glbStaff.GS_PER = person.PK;
			Assert("Lockout time is in the future", user.IsLockedOut);

			person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddMinutes(-5);
			Assert("Lockout time has passed", !user.IsLockedOut);
		}

		public void TestIUserIsLockedOut_ForLoginFailureLogs()
		{
			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_LoginName = "TestLogin";
			glbStaff.GS_Code = "TL";

			var user = (IUser)glbStaff;
			Assert("Not locked out initially", !user.IsLockedOut);

			CreateLockoutUserRecord(glbStaff, 10);
			Assert("Locked out for failure login record", user.IsLockedOut);
		}

		public void TestIUserIsLockedOut_ADEnabled()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			AssertEquals("User is not locked out before enable AD", false, staff.IsLockedOut);

			//AD Enabled and staff is linked to an ADUser who has been locked out
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var adUser = new Mock<IADUser>();
			adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
			adUser.Setup(m => m.LockedOut).Returns(true);

			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			AssertEquals("Staff should be linked with AD", true, staff.IsADLinked);
			AssertEquals("AD Locked out", true, staff.IsLockedOut);

			adUser.VerifyAll();
		}

		#endregion

		#region Defaults

		public void TestDefaultWorkingHours()
		{
			var newStaff = (GlbStaff)GetNewBusinessObject();
			newStaff.GS_LoginName = "newstaff";

			Factory.Save();

			var expectedDefaultWorkingHours = "                 ******** ********";
			CombineAssertions(() =>
			{
				AssertEquals("Default working hours for Monday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.MondayWorkingHours);
				AssertEquals("Default working hours for Tuesday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.TuesdayWorkingHours);
				AssertEquals("Default working hours for Wednesday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.WednesdayWorkingHours);
				AssertEquals("Default working hours for Thursday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.ThursdayWorkingHours);
				AssertEquals("Default working hours for Friday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.FridayWorkingHours);
				AssertEquals("No working hours for Saturday", string.Empty, newStaff.WorkTimes.SaturdayWorkingHours);
			});
		}

		public void TestDefaultWorkingHours_WithRegistryValueLessThanMinimumWeeklyWorkingHoursLargerThanZero()
		{
			var newStaff = (GlbStaff)GetNewBusinessObject();
			newStaff.GS_LoginName = "newstaff";
			newStaff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

			DataRegistry.Instance.RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "02:00");

			Factory.Save();

			var expectedDefaultWorkingHours = "                 *";
			CombineAssertions(() =>
			{
				AssertEquals("Default working hours for Monday is 8:30 AM to 9:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.MondayWorkingHours);
				AssertEquals("Default working hours for Tuesday is 8:30 AM to 9:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.TuesdayWorkingHours);
				AssertEquals("Default working hours for Wednesday is 8:30 AM to 9:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.WednesdayWorkingHours);
				AssertEquals("Default working hours for Thursday is 8:30 AM to 9:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.ThursdayWorkingHours);
				AssertEquals("Default working hours for Friday is 8:30 AM to 9:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.FridayWorkingHours);
				AssertEquals("No working hours for Saturday", string.Empty, newStaff.WorkTimes.SaturdayWorkingHours);
			});
		}

		public void TestDefaultWorkingHours_WithRegistryValueIsZero()
		{
			var newStaff = (GlbStaff)GetNewBusinessObject();
			newStaff.GS_LoginName = "newstaff";
			newStaff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

			DataRegistry.Instance.RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "00:00");

			Factory.Save();

			var expectedDefaultWorkingHours = "                 ******** ********";
			CombineAssertions(() =>
			{
				AssertEquals("Default working hours for Monday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.MondayWorkingHours);
				AssertEquals("Default working hours for Tuesday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.TuesdayWorkingHours);
				AssertEquals("Default working hours for Wednesday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.WednesdayWorkingHours);
				AssertEquals("Default working hours for Thursday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.ThursdayWorkingHours);
				AssertEquals("Default working hours for Friday are 8:30 AM to 12:30 PM and then 1:00 PM to 5:00 PM", expectedDefaultWorkingHours, newStaff.WorkTimes.FridayWorkingHours);
				AssertEquals("No working hours for Saturday", string.Empty, newStaff.WorkTimes.SaturdayWorkingHours);
			});
		}

		public void TestDefaultWorkingHours_WithRegistryValueLargerThanMinimumWeeklyWorkingHours()
		{
			var newStaff = (GlbStaff)GetNewBusinessObject();
			newStaff.GS_LoginName = "newstaff";
			newStaff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

			DataRegistry.Instance.RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "10:00");

			Factory.Save();

			var expectedDefaultWorkingHours = "                 ****";
			CombineAssertions(() =>
			{
				AssertEquals("Default working hours for Monday is 8:30 AM to 10:30 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.MondayWorkingHours);
				AssertEquals("Default working hours for Tuesday is 8:30 AM to 10:30 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.TuesdayWorkingHours);
				AssertEquals("Default working hours for Wednesday is 8:30 AM to 10:30 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.WednesdayWorkingHours);
				AssertEquals("Default working hours for Thursday is 8:30 AM to 10:30 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.ThursdayWorkingHours);
				AssertEquals("Default working hours for Friday is 8:30 AM to 10:30 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.FridayWorkingHours);
				AssertEquals("No working hours for Saturday", string.Empty, newStaff.WorkTimes.SaturdayWorkingHours);
			});
		}

		public void TestDefaultWorkingHours_WithRegistryValueTooLarge()
		{
			var newStaff = (GlbStaff)GetNewBusinessObject();
			newStaff.GS_LoginName = "newstaff";
			newStaff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

			DataRegistry.Instance.RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "80:00");

			Factory.Save();

			var expectedDefaultWorkingHours = "                 ******** **********************";
			CombineAssertions(() =>
			{
				AssertEquals("Default working hours for Monday is 8:30 AM to 24:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.MondayWorkingHours);
				AssertEquals("Default working hours for Tuesday is 8:30 AM to 24:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.TuesdayWorkingHours);
				AssertEquals("Default working hours for Wednesday is 8:30 AM to 24:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.WednesdayWorkingHours);
				AssertEquals("Default working hours for Thursday is 8:30 AM to 24:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.ThursdayWorkingHours);
				AssertEquals("Default working hours for Friday is 8:30 AM to 24:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.FridayWorkingHours);
				AssertEquals("No working hours for Saturday", string.Empty, newStaff.WorkTimes.SaturdayWorkingHours);
			});
		}

		public void TestDefaultWorkingHours_WithSaturdayWorkHours()
		{
			var sing = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN");
			using (Env.SetTemporaryUserContext(User.SupportUserName, sing.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newStaff = (GlbStaff)GetNewBusinessObject();
				newStaff.GS_LoginName = "newstaff";
				newStaff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

				DataRegistry.Instance.RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "80:00");

				Factory.Save();

				var expectedDefaultWorkingHours = "                 ******** ******************";
				CombineAssertions(() =>
				{
					AssertEquals("Default working hours for Monday is 8:30 AM to 22:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.MondayWorkingHours);
					AssertEquals("Default working hours for Tuesday is 8:30 AM to 22:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.TuesdayWorkingHours);
					AssertEquals("Default working hours for Wednesday is 8:30 AM to 22:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.WednesdayWorkingHours);
					AssertEquals("Default working hours for Thursday is 8:30 AM to 22:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.ThursdayWorkingHours);
					AssertEquals("Default working hours for Friday is 8:30 AM to 22:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.FridayWorkingHours);
					AssertEquals("Default working hours for Saturday is 8:30 AM to 22:00 AM", expectedDefaultWorkingHours, newStaff.WorkTimes.SaturdayWorkingHours);
				});
			}
		}

		public void TestLoadAndCreateWorkTime()
		{
			GlbStaff tStaff = (GlbStaff)GetNewBusinessObject();
			tStaff.FillWithValidTestData();

			AssertNotNull(tStaff.WorkTimes.ParentID);
			AssertEquals(tStaff.PK, tStaff.WorkTimes.ParentID);
			AssertEquals("GS", tStaff.WorkTimes.ParentTableCode);

			tStaff.WorkTimes.MondayWorkingHours = "**              ***";
			tStaff.WorkTimes.TuesdayWorkingHours = "***********************************************";
			tStaff.WorkTimes.WednesdayWorkingHours = "      *****     ";
			tStaff.WorkTimes.ThursdayWorkingHours = "      *";
			tStaff.WorkTimes.FridayWorkingHours = "           *********";
			tStaff.WorkTimes.SaturdayWorkingHours = "           ";
			tStaff.WorkTimes.SundayWorkingHours = "           ";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			GlbStaff tStaff2 = factory2.Load<GlbStaff>(tStaff.PK);
			factory2.Save();
			AssertNotNull(tStaff2.WorkTimes.ParentID);
			AssertEquals(tStaff.WorkTimes.ParentID, tStaff2.WorkTimes.ParentID);
			AssertEquals(tStaff2.WorkTimes.ParentTableCode, "GS");
			AssertEquals(tStaff2.WorkTimes.MondayWorkingHours, "**              ***");
			AssertEquals(tStaff2.WorkTimes.TuesdayWorkingHours, "***********************************************");
			AssertEquals(tStaff2.WorkTimes.WednesdayWorkingHours, "      *****");
			AssertEquals(tStaff2.WorkTimes.ThursdayWorkingHours, "      *");
			AssertEquals(tStaff2.WorkTimes.FridayWorkingHours, "           *********");
			AssertEquals(tStaff2.WorkTimes.SaturdayWorkingHours, "");
			AssertEquals(tStaff2.WorkTimes.SundayWorkingHours, "");
		}

		public void TestWorkTimeViewModel_NoWorkPattern()
		{
			var staff = (GlbStaff)GetNewBusinessObject();
			staff.WorkTimes.MondayWorkingHours = "                 *******************           ";
			staff.WorkTimes.TuesdayWorkingHours = "                 *******************           ";
			staff.WorkTimes.WednesdayWorkingHours = "                 *******************           ";
			staff.WorkTimes.ThursdayWorkingHours = "                 *******************           ";
			staff.WorkTimes.FridayWorkingHours = "                 *******************           ";

			var viewModel = staff.WorkTimeViewModel;
			Assert(!viewModel.IsReadOnly);
			AssertEquals(viewModel.MondayWorkingHours, "                 *******************");
			AssertEquals(viewModel.TuesdayWorkingHours, "                 *******************");
			AssertEquals(viewModel.WednesdayWorkingHours, "                 *******************");
			AssertEquals(viewModel.ThursdayWorkingHours, "                 *******************");
			AssertEquals(viewModel.FridayWorkingHours, "                 *******************");
			AssertEquals(viewModel.SaturdayWorkingHours, ZString.Empty);
			AssertEquals(viewModel.SundayWorkingHours, ZString.Empty);
		}

		public void TestWorkTimeViewModel_WithCurrentWorkPattern()
		{
			var staff = (GlbStaff)GetNewBusinessObject();
			staff.WorkTimes.MondayWorkingHours = "                 *******************           ";
			staff.WorkTimes.TuesdayWorkingHours = "                 *******************           ";
			staff.WorkTimes.WednesdayWorkingHours = "                 *******************           ";
			staff.WorkTimes.ThursdayWorkingHours = "                 *******************           ";
			staff.WorkTimes.FridayWorkingHours = "                 *******************           ";

			var pattern1 = Factory.New<GlbWorkPattern>();
			pattern1.GWP_GS_Staff = staff.PK;
			pattern1.GWP_EffectiveDate = ZDateTimeOffset.UtcNow.AddDays(-3);

			var viewModel = staff.WorkTimeViewModel;
			Assert(viewModel.IsReadOnly);
			AssertEquals(viewModel.MondayWorkingHours, "                 *******************");
			AssertEquals(viewModel.TuesdayWorkingHours, "                 *******************");
			AssertEquals(viewModel.WednesdayWorkingHours, "                 *******************");
			AssertEquals(viewModel.ThursdayWorkingHours, "                 *******************");
			AssertEquals(viewModel.FridayWorkingHours, "                 *******************");
			AssertEquals(viewModel.SaturdayWorkingHours, ZString.Empty);
			AssertEquals(viewModel.SundayWorkingHours, ZString.Empty);
		}

		public void TestWorkTimeViewModel_WithNoCurrentWorkPattern()
		{
			var staff = (GlbStaff)GetNewBusinessObject();
			staff.WorkTimes.MondayWorkingHours = "                 *******************           ";
			staff.WorkTimes.TuesdayWorkingHours = "                 *******************           ";
			staff.WorkTimes.WednesdayWorkingHours = "                 *******************           ";
			staff.WorkTimes.ThursdayWorkingHours = "                 *******************           ";
			staff.WorkTimes.FridayWorkingHours = "                 *******************           ";

			var pattern1 = Factory.New<GlbWorkPattern>();
			pattern1.GWP_GS_Staff = staff.PK;
			pattern1.GWP_EffectiveDate = ZDateTimeOffset.UtcNow.AddDays(3);

			var viewModel = staff.WorkTimeViewModel;
			Assert(!viewModel.IsReadOnly);
			AssertEquals(viewModel.MondayWorkingHours, "                 *******************");
			AssertEquals(viewModel.TuesdayWorkingHours, "                 *******************");
			AssertEquals(viewModel.WednesdayWorkingHours, "                 *******************");
			AssertEquals(viewModel.ThursdayWorkingHours, "                 *******************");
			AssertEquals(viewModel.FridayWorkingHours, "                 *******************");
			AssertEquals(viewModel.SaturdayWorkingHours, ZString.Empty);
			AssertEquals(viewModel.SundayWorkingHours, ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues_DoesntBlowUpOnGetNullWithNoCompany()
		{
			string originalLoginName = Env.CurrentUser.LoginName;
			Guid originalBranchPk = Env.CurrentBranch.PK;
			Guid originalDepartmentPk = Env.CurrentDepartment.PK;
			using (Env.SetTemporaryUserContext(null))
			{
				Factory.New<GlbStaff>();
			}
		}

		#endregion

		#region Is EDI Support Login

		public void TestIsEDISupportLogin()
		{
			Assert("GlbStaff.CurrentUser.IsSupportUser", GlbStaff.CurrentUser.IsSupportUser);
		}

		#endregion

		#region Holidays / Leave Status

		[TestDate(2006, 3, 15)]
		public void TestNextWorkingDay()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			AssertEquals(new ZDate(2006, 3, 15), staff.NextWorkingDay(true));
			AssertEquals(new ZDate(2006, 3, 16), staff.NextWorkingDay(false));

			var holiday = staff.HolidaysIncBMSLeave.AddNew();
			holiday.GA_StartTime = new ZDateTime(2006, 3, 16);
			holiday.GA_EndTime = new ZDateTime(2006, 3, 17);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "ANN";

			Factory.Save();

			AssertEquals(new ZDate(2006, 3, 15), staff.NextWorkingDay(true));
			AssertEquals(new ZDate(2006, 3, 17), staff.NextWorkingDay(false));

			holiday = Factory.New<GlbStaffHoliday>();
			holiday.GA_StartTime = new ZDateTime(2006, 3, 15);
			holiday.GA_EndTime = new ZDateTime(2006, 3, 16);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "ANN";
			holiday.GA_GS = staff.PK;

			Factory.Save();

			AssertEquals(new ZDate(2006, 3, 17), staff.NextWorkingDay(true));
			AssertEquals(new ZDate(2006, 3, 17), staff.NextWorkingDay(false));
		}

		public void TestIsWorking()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			AssertEquals(true, staff.IsWorking(new ZDate(2006, 3, 15)));
			AssertEquals(false, staff.IsWorking(new ZDate(2006, 3, 19)));

			var holiday = staff.HolidaysIncBMSLeave.AddNew();
			holiday.GA_StartTime = new ZDateTime(2006, 3, 16);
			holiday.GA_EndTime = new ZDateTime(2006, 3, 17);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "ANN";

			AssertEquals(true, staff.IsWorking(new ZDate(2006, 3, 15)));
			AssertEquals(false, staff.IsWorking(new ZDate(2006, 3, 16)));
			AssertEquals(true, staff.IsWorking(new ZDate(2006, 3, 17)));
			AssertEquals(true, staff.IsWorking(new ZDate(2006, 3, 20)));
		}

		public void TestIsWorkingDateAndTime()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			AssertEquals(true, staff.IsWorking(new ZDateTime(2006, 3, 15, 13, 30, 0)));
			AssertEquals(false, staff.IsWorking(new ZDateTime(2006, 3, 19, 13, 30, 0)));

			var holiday = staff.HolidaysIncBMSLeave.AddNew();
			holiday.GA_StartTime = new ZDateTime(2006, 3, 15, 10, 30, 0);
			holiday.GA_EndTime = new ZDateTime(2006, 3, 15, 14, 30, 0);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "ANN";

			Factory.Save();

			AssertEquals(false, staff.IsWorking(new ZDateTime(2006, 3, 15, 13, 30, 0)));
			AssertEquals(true, staff.IsWorking(new ZDateTime(2006, 3, 15, 14, 31, 0)));
		}

		public void TestGetFirstAvailableDate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			AssertEquals(new ZDateTime(2006, 3, 15, 8, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15)));

			var holiday = staff.HolidaysIncBMSLeave.AddNew();
			holiday.GA_StartTime = new ZDateTime(2006, 3, 15, 10, 30, 0);
			holiday.GA_EndTime = new ZDateTime(2006, 3, 15, 14, 30, 0);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "ANN";

			Factory.Save();

			AssertEquals(new ZDateTime(2006, 3, 15, 8, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15)));
			AssertEquals(new ZDateTime(2006, 3, 15, 14, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15, 14, 0, 0)));

			var holiday2 = staff.HolidaysIncBMSLeave.AddNew();
			holiday2.GA_StartTime = new ZDateTime(2006, 3, 15, 14, 30, 0);
			holiday2.GA_EndTime = new ZDateTime(2006, 3, 15, 18, 0, 0);
			holiday2.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday2.GA_WorkHolidayType = "ANN";

			Factory.Save();

			AssertEquals(new ZDateTime(2006, 3, 16, 8, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15, 15, 0, 0)));

			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			var branchHoliday = staff.HomeBranch.GlbHolidays.AddNew();
			branchHoliday.GH_HolidayName = "First Megumin in Space";
			branchHoliday.GH_Date = new ZDateTime(2006, 3, 16);

			Factory.Save();

			AssertEquals(new ZDateTime(2006, 3, 17, 8, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15, 15, 0, 0)));

			var holidayFriday = staff.HolidaysIncBMSLeave.AddNew();
			holidayFriday.GA_StartTime = new ZDateTime(2006, 3, 17);
			holidayFriday.GA_EndTime = new ZDateTime(2006, 3, 18);
			holidayFriday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holidayFriday.GA_WorkHolidayType = "TRN";

			Factory.Save();

			AssertEquals(new ZDateTime(2006, 3, 20, 8, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15, 15, 0, 0)));

			var holidayFinishedAfterWorkingHours = staff.HolidaysIncBMSLeave.AddNew();
			holidayFinishedAfterWorkingHours.GA_StartTime = new ZDateTime(2006, 3, 20, 8, 30, 0);
			holidayFinishedAfterWorkingHours.GA_EndTime = new ZDateTime(2006, 3, 20, 18, 0, 0);
			holidayFinishedAfterWorkingHours.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holidayFinishedAfterWorkingHours.GA_WorkHolidayType = "ANN";

			Factory.Save();

			AssertEquals(new ZDateTime(2006, 3, 21, 8, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15, 15, 0, 0)));

			var holidayOverlap = staff.HolidaysIncBMSLeave.AddNew();
			holidayOverlap.GA_StartTime = new ZDateTime(2006, 3, 20);
			holidayOverlap.GA_EndTime = new ZDateTime(2006, 3, 23);
			holidayOverlap.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holidayOverlap.GA_WorkHolidayType = "ANN";

			Factory.Save();

			AssertEquals(new ZDateTime(2006, 3, 23, 8, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15, 15, 0, 0)));

			var branchHolidayOverlapWithStuffHoliday = staff.HomeBranch.GlbHolidays.AddNew();
			branchHolidayOverlapWithStuffHoliday.GH_HolidayName = "Megumin Moon Landing";
			branchHolidayOverlapWithStuffHoliday.GH_Date = new ZDateTime(2006, 3, 23);

			Factory.Save();

			AssertEquals(new ZDateTime(2006, 3, 24, 8, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15, 15, 0, 0)));

			var branchHoliday2 = staff.HomeBranch.GlbHolidays.AddNew();
			branchHoliday2.GH_HolidayName = "Explosion of the Moon";
			branchHoliday2.GH_Date = new ZDateTime(2006, 3, 24);

			Factory.Save();

			AssertEquals(new ZDateTime(2006, 3, 27, 8, 30, 0), staff.GetFirstAvailableDate(new ZDateTime(2006, 3, 15, 15, 0, 0)));
		}

		[TestDate(2014, 06, 24)]
		public void TestWorkStatusAvailabilityPercentage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.WorkTimes.SaturdayWorkingHours = staff.WorkTimes.MondayWorkingHours;
			staff.WorkTimes.SundayWorkingHours = staff.WorkTimes.MondayWorkingHours;

			Assert(!staff.IsOnLeave);
			Assert(!staff.IsOnLeaveOnDate(ZDateTime.Today));
			Assert(staff.IsWorkingToday);

			var holiday = staff.HolidaysIncBMSLeave.AddNew();
			holiday.GA_StartTime = ZDateTime.Today;
			holiday.GA_EndTime = ZDateTime.Today.AddDays(2);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "ANN";
			holiday.GA_AvailabilityPercentage = 0;

			Factory.Save();

			Assert(staff.IsOnLeave);
			AssertEquals("On Leave - Annual Leave", staff.WorkStatus);
			Assert(!staff.IsWorkingToday);

			holiday.GA_AvailabilityPercentage = 10;
			Factory.Save();

			Assert(!staff.IsOnLeave);
			AssertEquals("Working", staff.WorkStatus);
			Assert(staff.IsWorkingToday);
		}

		public void TestWorkStatus()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.WorkTimes.SaturdayWorkingHours = staff.WorkTimes.MondayWorkingHours;
			staff.WorkTimes.SundayWorkingHours = staff.WorkTimes.MondayWorkingHours;

			Assert(!staff.IsOnLeave);
			Assert(!staff.IsOnLeaveOnDate(new ZDateTime(2005, 11, 26)));
			Assert(staff.IsWorkingToday);

			var holiday1 = staff.HolidaysIncBMSLeave.AddNew();
			holiday1.GA_StartTime = new ZDateTime(2005, 11, 15);
			holiday1.GA_EndTime = new ZDateTime(2005, 11, 28);
			holiday1.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday1.GA_WorkHolidayType = "ANN";

			Factory.Save();

			Assert(!staff.IsOnLeave);
			AssertEquals("Working", staff.WorkStatus);
			Assert(staff.IsOnLeaveOnDate(new ZDateTime(2005, 11, 26)));
			Assert(staff.IsOnLeaveOnDate(new ZDateTime(2005, 11, 28)));
			Assert(!staff.IsOnLeaveOnDate(new ZDateTime(2005, 11, 29)));
			Assert(staff.IsWorkingToday);

			var holiday2 = staff.HolidaysIncBMSLeave.AddNew();
			holiday2.GA_StartTime = ZDateTime.Now.AddDays(-1);
			holiday2.GA_EndTime = ZDateTime.Now.AddDays(1);
			holiday2.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday2.GA_WorkHolidayType = "SIC";

			Factory.Save();

			Assert(staff.IsOnLeave);
			AssertEquals("On Leave - Sick Leave", staff.WorkStatus);

			holiday2.GA_WorkHolidayType = "ANN";
			Factory.Save();

			AssertEquals("On Leave - Annual Leave", staff.WorkStatus);
			Assert(staff.IsOnLeaveOnDate(new ZDateTime(2005, 11, 26)));
			Assert(staff.IsOnLeaveOnDate(new ZDateTime(2005, 11, 28)));
			Assert(!staff.IsOnLeaveOnDate(new ZDateTime(2005, 11, 29)));
			Assert(!staff.IsWorkingToday);

			holiday2.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Declined;
			Factory.Save();

			Assert(!staff.IsOnLeave);

			holiday2.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			Factory.Save();

			Assert(staff.IsOnLeave);

			holiday2.Delete();
			Factory.Save();

			Assert(!staff.IsOnLeave);
			Assert(staff.IsWorkingToday);

			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			var branchHoliday = staff.HomeBranch.GlbHolidays.AddNew();
			branchHoliday.GH_Date = ZDateTime.Today;
			branchHoliday.GH_HolidayName = "Test Branch Holiday";

			Factory.Save();

			Assert(!staff.IsOnLeave);
			Assert(!staff.IsWorkingToday);
			AssertEquals("Branch Holiday", staff.WorkStatus);

			staff.HomeBranch.GlbHolidays.DeleteAll();

			staff.WorkTimes.MondayWorkingHours = "";
			staff.WorkTimes.TuesdayWorkingHours = "";
			staff.WorkTimes.WednesdayWorkingHours = "";
			staff.WorkTimes.ThursdayWorkingHours = "";
			staff.WorkTimes.FridayWorkingHours = "";
			staff.WorkTimes.SaturdayWorkingHours = "";
			staff.WorkTimes.SundayWorkingHours = "";

			Assert(!staff.IsOnLeave);
			Assert(!staff.IsWorkingToday);
			AssertEquals("Non Work Day", staff.WorkStatus);

			staff.GS_IsActive = false;
			Assert(!staff.IsOnLeave);
			Assert(!staff.IsWorkingToday);
			AssertEquals("Ceased Employment", staff.WorkStatus);
		}

		public void TestWorkStatusWhenWorkingAway()
		{
			var registryItem = SystemDataRegistry.Instance.StaffLeaveTypes.Value;
			registryItem.Add("AMZ", (NoResString)"International amazingday", true);

			SystemDataRegistry.Instance.StaffLeaveTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.WorkTimes.SaturdayWorkingHours = staff.WorkTimes.MondayWorkingHours;
			staff.WorkTimes.SundayWorkingHours = staff.WorkTimes.MondayWorkingHours;

			Assert(!staff.IsOnLeave);
			Assert(staff.IsWorkingToday);

			var holiday = staff.HolidaysIncBMSLeave.AddNew();
			holiday.GA_StartTime = ZDateTime.Now.AddDays(-1);
			holiday.GA_EndTime = ZDateTime.Now.AddDays(1);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "SIC";

			Factory.Save();

			AssertEquals(true, staff.IsOnLeave);
			AssertEquals("On Leave - Sick Leave", staff.WorkStatus);

			holiday.GA_WorkHolidayType = "AMZ";
			AssertEquals("Staff is not on leave if their holiday type is set to working away", false, staff.IsOnLeave);
			AssertEquals("Working Away - International amazingday", staff.WorkStatus);

			holiday.GA_WorkHolidayType = "SIC";
			AssertEquals(true, staff.IsOnLeave);
			AssertEquals("On Leave - Sick Leave", staff.WorkStatus);

			holiday.GA_IsWorkingAway = true;
			AssertEquals("staff holiday is sick leave but working away is ticked thus they are not on leave", false, staff.IsOnLeave);
			AssertEquals("Working Away - Sick Leave", staff.WorkStatus);
		}

		#endregion

		#region Logging

		public void TestFieldChangesLogs()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "B1";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "B2";
			var dep1 = Factory.NewWithValidTestData<GlbDepartment>();
			dep1.GE_Code = "D1";
			var dep2 = Factory.NewWithValidTestData<GlbDepartment>();
			dep2.GE_Code = "D2";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Title = "ABC";
			staff.GS_LoginName = "abc";
			staff.GS_DomainName = "domain1";
			staff.GS_GB_HomeBranch = branch1.PK;
			staff.GS_GE_HomeDepartment = dep1.PK;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			staff.GS_EmailAddress = "abc@mail.com";
			staff.GS_UserAddress1 = "123 Ocean Dr";
			staff.GS_UserAddress2 = "The Suburbian Way";
			staff.GS_City = "Auckland";
			staff.GS_State = "AR";
			staff.GS_Postcode = "2000";
			staff.GS_RN_NKCountryCode = "NZ";
			Factory.Save();
			AssertEquals(0, staff.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == "EDT"));
			staff.GS_Title = "DEF";
			staff.GS_LoginName = "def";
			staff.GS_DomainName = "domain2";
			staff.GS_GB_HomeBranch = branch2.PK;
			staff.GS_GE_HomeDepartment = dep2.PK;
			staff.GS_ActiveDirectoryObjectGuid = new ZGuid("259dc871-c173-4fc2-87d5-55e553034a93");
			staff.GS_EmailAddress = "def@mail.com";
			staff.GS_UserAddress1 = "124 Silver St";
			staff.GS_UserAddress2 = "The Rural Side";
			staff.GS_City = "Melbourne";
			staff.GS_State = "VIC";
			staff.GS_Postcode = "3000";
			staff.GS_RN_NKCountryCode = "AU";
			Factory.Save();
			var edtlogs = staff.Logs.GetAllLogs().OfType<StmALog>().Where(x => x.SL_SE_NKEvent == "EDT");
			AssertEquals(14, edtlogs.Count());
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Title|OLD=ABC|NEW=DEF"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Login|OLD=abc|NEW=def"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Domain|OLD=domain1|NEW=domain2"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Home Branch|OLD=B1|NEW=B2"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Home Department|OLD=D1|NEW=D2"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "ADGuid|OLD=0000ffff-ffff-ffff-ffff-ffffffffffff|NEW=259dc871-c173-4fc2-87d5-55e553034a93"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Email|OLD=abc@mail.com|NEW=def@mail.com"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Address1"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Address2"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "City"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "State"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Postcode"));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Country/Region"));
			staff.GS_GB_HomeBranch = ZGuid.Empty;
			staff.GS_GE_HomeDepartment = ZGuid.Empty;
			Factory.Save();
			edtlogs = staff.Logs.GetAllLogs().OfType<StmALog>().Where(x => x.SL_SE_NKEvent == "EDT");
			AssertEquals(17, edtlogs.Count());
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Home Branch|OLD=B2|NEW="));
			AssertEquals(1, edtlogs.Count(x => x.SL_Reference == "Home Department|OLD=D2|NEW="));
		}

		public void TestStaffReviewPerformed()
		{
			ZDateTime refDateOfNextReview = new ZDateTime(2004, 5, 5, 5, 5, 5);
			ZDateTime refReviewDate = new ZDateTime(2004, 6, 6, 6, 6, 6);
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_NextReviewDate = refDateOfNextReview;
			staff.ReviewPerformed(refReviewDate);
			AssertEquals(1, staff.Logs.GetAllLogs().Count);
			AssertEquals("Reviewed on " + refReviewDate.ToShortDateString(), staff.Logs.GetAllLogs()[0].SL_Reference);
			AssertEquals("Review has taken place, Next Review Date should be cleared", ZDateTime.Empty, staff.GS_NextReviewDate);
		}

		public void TestHasLoggedIn()
		{
			var companyToLogInTo = Factory.NewWithValidTestData<GlbCompany>();
			var staffJim = Factory.NewWithValidTestData<GlbStaff>();

			Assert("Jim should not have logged in", !staffJim.HasEverLoggedIn);
			Factory.Save();
			Assert("Jim still should not have logged in", !staffJim.HasEverLoggedIn);

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(staffJim.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				companyToLogInTo.Logs.AddNew(Events.Login);
				Factory.Save();
				Assert("Jim should have logged in", staffJim.HasEverLoggedIn);
			}
		}

		public void TestGS_ActivityTrackingStatusReadOnly()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(staff1.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert("Shoud be able to edit staff1's activity tracking status", !staff1.GS_ActivityTrackingStatus_ReadOnly);
				Assert("Shoud not be able to edit staff2's activity tracking status", staff2.GS_ActivityTrackingStatus_ReadOnly);
			}
		}

		public void TestSecurityChangedLog()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			GlbSecurity security = Factory.NewWithValidTestData<GlbSecurity>();
			security.GU_SecurityRight = "Operations";
			security.GU_GS = staff.PK;
			security.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			StmALogDependentCollection collection = staff.Logs.GetAllLogs();
			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "SEC");
			BusinessObject[] businesObjects = collection.Find(query);
			AssertEquals("Should be 1 log on security modified", 1, businesObjects.Length);

			ZString slActualReference = ((StmALog)businesObjects[0]).SL_Reference;
			ZString slExpectedReference = "ADD - Operate, Is Allowed: Y, Branch: All, Dept: All, Company: All";
			AssertEquals("SL_Reference should be in correct format", slExpectedReference, slActualReference);
		}

		public void TestNoDeveloperNotificationExceptionOnLogging()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_SystemLastEditTimeUtc = ZDateTime.Now;
			staff.GS_SystemLastEditUser = "123";
			AssertEquals("No developer notification exception", "", ErrorReporter.LastMessageReported);
		}

		#endregion

		#region TestAddSecurityToAccessOrgOrWarehouse

		public void TestAddSecurityToAccessPrincipal()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "RSM";
			staff.GS_LoginName = "random.member";
			IOrgsAndWarehousesAccessProvider provider = staff;
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			AssertEquals("precondition:", 0, provider.SecurityAllowedOrgsAndWarehousesView.Count);

			provider.AddSecurityToAccessOrgOrWarehouse("PRINCIPAL");
			AssertEquals("should now have access to a principal", 1, provider.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("should have access to the correct principal", principal.PK, provider.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			IOrgsAndWarehousesAccessProvider providerInAnotherFactory = anotherFactory.Load<GlbStaff>(provider.PK);
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			AssertEquals("access to a principal should have been persisted", 1, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("access to the correct principal should have been persisted", principal.PK, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
		}

		public void TestAddSecurityToAccessWhsClient()
		{
			OrgHeader whsClient = Factory.New<OrgHeader>();
			whsClient.OH_Code = "CLIENT";
			whsClient.OH_IsWarehouseClient = true;
			Factory.Save();

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "RSM";
			staff.GS_LoginName = "random.member";
			IOrgsAndWarehousesAccessProvider provider = staff;
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			AssertEquals("precondition:", 0, provider.SecurityAllowedOrgsAndWarehousesView.Count);

			provider.AddSecurityToAccessOrgOrWarehouse("CLIENT");
			AssertEquals("should now have access to a client", 1, provider.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("should have access to the correct client", whsClient.PK, provider.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			IOrgsAndWarehousesAccessProvider providerInAnotherFactory = anotherFactory.Load<GlbStaff>(provider.PK);
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Rebuild();
			AssertEquals("access to a client should have been persisted", 1, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("access to the correct client should have been persisted", whsClient.PK, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
		}

		public void TestAddSecurityToAccessWarehouse()
		{
			IWhsTransactionTestHelper helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			BusinessObject warehouse = helper.CreateWarehouse("Warehouse", "WHS", "AA");

			Factory.Save();

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "RSM";
			staff.GS_LoginName = "random.member";
			IOrgsAndWarehousesAccessProvider provider = staff;
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			AssertEquals("precondition:", 0, provider.SecurityAllowedOrgsAndWarehousesView.Count);

			provider.AddSecurityToAccessOrgOrWarehouse("WHS");
			AssertEquals("should now have access to a warehouse", 1, provider.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("should have access to the correct warehouse", warehouse.PK, provider.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			IOrgsAndWarehousesAccessProvider providerInAnotherFactory = anotherFactory.Load<GlbStaff>(provider.PK);
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Rebuild();
			AssertEquals("access to a warehouse should have been persisted", 1, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("access to the correct warehouse should have been persisted", warehouse.PK, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
		}

		#endregion

		#region Test Security Permission For Login And Initials

		GlbStaff GetStaffWithoutPermissions()
		{
			GlbStaff staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutPermissions.GS_Code = "SWO";
			staffWithoutPermissions.GS_IsController = false;

			return staffWithoutPermissions;
		}

		GlbStaff GetStaffWithOnlyModifyResourcePermission()
		{
			GlbStaff staffWithOnlyModifyResourcePermission = Factory.NewWithValidTestData<GlbStaff>();
			staffWithOnlyModifyResourcePermission.GS_IsController = false;

			GlbSecurity securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityRight = "ResourceEdit";
			securityRecord.GU_ItemGUID = ZGuid.Empty;
			securityRecord.GU_SecurityItemIsAllowed = true;
			securityRecord.GU_GS = staffWithOnlyModifyResourcePermission.PK;
			staffWithOnlyModifyResourcePermission.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);

			return staffWithOnlyModifyResourcePermission;
		}

		//This staff has permission to modify own details, but not others
		GlbStaff GetStaffWhoCannotModifyOthers()
		{
			GlbStaff staffWhoCannotModifyOthers = Factory.NewWithValidTestData<GlbStaff>();
			staffWhoCannotModifyOthers.GS_IsController = false;
			foreach (SecurityCheckpoint checkPoint in Env.Security.AllLoadedCheckPoints)
			{
				if (checkPoint.Code.StartsWith("StaffOwn") || checkPoint.Code == "StaffModifyOwn")
				{
					GlbSecurity securityRecord = Factory.New<GlbSecurity>();
					securityRecord.GU_SecurityRight = checkPoint.Code;
					securityRecord.GU_ItemGUID = checkPoint.ItemGuid;
					securityRecord.GU_SecurityItemIsAllowed = true;
					securityRecord.GU_GS = staffWhoCannotModifyOthers.PK;
					staffWhoCannotModifyOthers.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);
				}
			}

			return staffWhoCannotModifyOthers;
		}

		GlbStaff GetStaffWithPermissions()
		{
			GlbStaff staffWithPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithPermissions.GS_Code = "SWP";
			staffWithPermissions.GS_IsController = true;
			return staffWithPermissions;
		}

		public void TestReadOnlyFieldsIfNoRightsExist()
		{
			using (Env.Security.SecurityCachingDisabler)
			{
				GlbStaff staffWithPermissions = GetStaffWithPermissions();
				GlbStaff staffWithoutPermissions = GetStaffWithoutPermissions();
				GlbStaff staffWhoCannotModifyOthers = GetStaffWhoCannotModifyOthers();
				GlbStaff staffCanOnlyModifyResouce = GetStaffWithOnlyModifyResourcePermission();
				GlbStaff staffForTest = Factory.NewWithValidTestData<GlbStaff>();
				staffForTest.GS_Code = "BAS";
				Factory.Save();

				BusinessObjectFactory factoryToLoadStaffIn = new BusinessObjectFactory();

				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithoutPermissions.PK, Env.CurrentUser.PK);

					GlbStaff loadedStaffWithoutPermission = factoryToLoadStaffIn.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutPermissions.PK));
					Assert("Login name should be readonly", loadedStaffWithoutPermission.GS_LoginNameInfo.ReadOnly);
					Assert("Initials should be readonly", loadedStaffWithoutPermission.GS_CodeInfo.ReadOnly);
					Assert("Is Active should be readonly", loadedStaffWithoutPermission.GS_IsActiveInfo.ReadOnly);
					Assert("Domain Name should be readonly", loadedStaffWithoutPermission.DomainNameInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					// GS_Code readonly because the staff member already in database
					GlbStaff loadedStaffForTest = factoryToLoadStaffIn.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffForTest.PK));
					Assert("Login name should not be readonly", !loadedStaffForTest.GS_LoginNameInfo.ReadOnly);
					Assert("Initials should be readonly", loadedStaffForTest.GS_CodeInfo.ReadOnly);
					Assert("Is Active should not be readonly", !loadedStaffForTest.GS_IsActiveInfo.ReadOnly);
					Assert("Domain Name should not be readonly", !loadedStaffForTest.DomainNameInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWhoCannotModifyOthers.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					// This Staff member should be allowed access to their own record
					// GS_Code readonly because the staff member already in database
					GlbStaff loadedStaffWhoCannotModifyOthers = factoryToLoadStaffIn.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWhoCannotModifyOthers.PK));
					Assert("Login name should not be readonly", !loadedStaffWhoCannotModifyOthers.GS_LoginNameInfo.ReadOnly);
					Assert("Initials should be readonly", loadedStaffWhoCannotModifyOthers.GS_CodeInfo.ReadOnly);
					Assert("Is Active should not be readonly", !loadedStaffWhoCannotModifyOthers.GS_IsActiveInfo.ReadOnly);
					Assert("Domain Name should not be readonly", !loadedStaffWhoCannotModifyOthers.DomainNameInfo.ReadOnly);

					//This Staff member should not be allowed access to others records
					GlbStaff loadedStaffWithPermission = factoryToLoadStaffIn.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithPermissions.PK));
					Assert("Login name should be readonly", loadedStaffWithPermission.GS_LoginNameInfo.ReadOnly);
					Assert("Initials should be readonly", loadedStaffWithPermission.GS_CodeInfo.ReadOnly);
					Assert("Is Active should be readonly", loadedStaffWithPermission.GS_IsActiveInfo.ReadOnly);
					Assert("Domain Name should be readonly", loadedStaffWithPermission.DomainNameInfo.ReadOnly);
				}

				//This Staff member should be allowed to modify RESOURCE record
				// GS_Code editable because the staff member hasn't been saved yet
				using (Env.SetTemporaryUserContext(new UserContext(staffCanOnlyModifyResouce.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					GlbStaff resource = Factory.NewWithValidTestData<GlbStaff>();
					resource.GS_IsResource = true;
					Assert("Login name should not be readonly", !resource.GS_LoginNameInfo.ReadOnly);
					Assert("Initials should not be readonly", !resource.GS_CodeInfo.ReadOnly);
					Assert("Is Active should not be readonly", !resource.GS_IsActiveInfo.ReadOnly);
					Assert("Domain Name should not be readonly", !resource.DomainNameInfo.ReadOnly);
				}

				// Local admin for another group should be able to create/modify staff which is NOT in DB yet
				var localAdmin = Factory.NewWithValidTestData<GlbStaff>();
				localAdmin.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(Staff.Groups[0].GG_Code);
				Factory.Save();

				using (Env.SetTemporaryUserContext(new UserContext(localAdmin.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var anotherStaff = Factory.NewWithValidTestData<GlbStaff>();

					Assert("Login name should not be readonly", !anotherStaff.GS_LoginNameInfo.ReadOnly);
					Assert("Initials should not be readonly", !anotherStaff.GS_CodeInfo.ReadOnly);
					Assert("Is Active should not be readonly", !anotherStaff.GS_IsActiveInfo.ReadOnly);
					Assert("Domain Name should not be readonly", !anotherStaff.DomainNameInfo.ReadOnly);
					Factory.Save();

					// Local admin for another group should NOT be able to modify staff which is in DB already
					Assert("Login name should be readonly", anotherStaff.GS_LoginNameInfo.ReadOnly);
					Assert("Initials should be readonly", anotherStaff.GS_CodeInfo.ReadOnly);
					Assert("Is Active should be readonly", anotherStaff.GS_IsActiveInfo.ReadOnly);
					Assert("Domain Name should be readonly", anotherStaff.DomainNameInfo.ReadOnly);
				}
			}
		}

		#endregion

		#region Test Login and db access rights

		[TestSemaphoreProvider()]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestIsLoggedIn()
		{
			var initialUserContext = Env.CurrentUserContext;

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "ABC";
			staff1.StaffPlainTextPassword = "Hello";
			staff1.GS_IsSystemAccount = ZBool.True;

			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff1.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "XYZ";
			staff2.StaffPlainTextPassword = "Byebye";
			staff2.GS_IsSystemAccount = ZBool.True;

			var security2 = Factory.NewWithValidTestData<GlbSecurity>();
			security2.GU_GS = staff2.PK;
			security2.GU_SecurityItemIsAllowed = true;
			security2.GU_SecurityRight = "Login";

			Factory.Save();

			try
			{
				AssertEquals(false, staff1.IsLoggedIn);
				AssertEquals(false, staff2.IsLoggedIn);
				AssertEquals(true, GlbStaff.CurrentUser.IsLoggedIn);

				Env.LoginController.LoginLocation(Env.LoginController.LoginUser(staff1.GS_LoginName, "Hello"), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
				AssertEquals(true, staff1.IsLoggedIn);
				AssertEquals(false, staff2.IsLoggedIn);
				Env.LoginController.Logout();

				Env.LoginController.LoginLocation(Env.LoginController.LoginUser(staff2.GS_LoginName, "Byebye"), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
				AssertEquals(false, staff1.IsLoggedIn);
				AssertEquals(true, staff2.IsLoggedIn);
				Env.LoginController.Logout();
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		public void TestHasEverLoggedIn()
		{
			Staff.GS_FullName = "Test staff";
			Staff.GS_LoginName = "testLogin";
			Staff.GS_Code = "TS";
			Factory.Save();
			Assert(!Staff.HasEverLoggedIn);

			var currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			var lastLogin = currentCompany.GetLogs().AddNew(Events.Login, ZDateTimeOffset.Now.AddDays(1));
			lastLogin.SL_GS_NKUser = Staff.GS_Code;
			Factory.Save();

			Assert(Staff.HasEverLoggedIn);
		}

		public void TestIsDatabaseDeveloper()
		{
			Staff.GS_FullName = "Test staff A";
			Staff.GS_LoginName = "testLoginA";
			Staff.GS_Code = "TSA";
			Staff.IsDatabaseDeveloper = true;
			Staff.IsReadOnlyDBUser = false;
			Staff.IsBackupOperator = false;

			Assert(Staff.IsDatabaseDeveloper);
			Assert(!Staff.IsReadOnlyDBUser);
			Assert(!Staff.IsBackupOperator);
		}

		public void TestIsReadOnlyDBUser()
		{
			Staff.GS_FullName = "Test staff B";
			Staff.GS_LoginName = "testLoginB";
			Staff.GS_Code = "TSB";
			Staff.IsDatabaseDeveloper = false;
			Staff.IsReadOnlyDBUser = true;
			Staff.IsBackupOperator = false;

			Assert(!Staff.IsDatabaseDeveloper);
			Assert(Staff.IsReadOnlyDBUser);
			Assert(!Staff.IsBackupOperator);
		}

		public void TestIsBackupOperator()
		{
			Staff.GS_FullName = "Test staff C";
			Staff.GS_LoginName = "testLoginC";
			Staff.GS_Code = "TSC";
			Staff.IsDatabaseDeveloper = false;
			Staff.IsReadOnlyDBUser = false;
			Staff.IsBackupOperator = true;

			Assert(!Staff.IsDatabaseDeveloper);
			Assert(!Staff.IsReadOnlyDBUser);
			Assert(Staff.IsBackupOperator);
		}

		public void TestDatabaseAccessGroupRoles()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_FullName = "Test staff D";
			staff1.GS_LoginName = "testLoginD";
			staff1.GS_Code = "TSD";

			staff1.IsDatabaseDeveloper = true;
			staff1.IsReadOnlyDBUser = true;
			staff1.IsBackupOperator = true;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new string[] { "db_datawriter", "cwRestrictedReaderRole", "db_backupoperator" }, staff1.DatabaseAccessGroupRoles);
				AssertEquals(0, staff1.DatabaseAccessGroupRolesOriginalValues.Count);
			});
		}

		public void TestDatabaseAccessGroupRoles_HRMStaffRole()
		{
			var guid_staff = Guid.NewGuid();
			var guid_group = Guid.NewGuid();

			string sqlText = $@"
				------ GlbStaff
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff}', 'TSH', 'testLoginHRMStaffRole', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				------ HRMStaff Database Access Groups
				INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsActive, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('{guid_group}', 'TG_x1x', 'Test Group HRMStaff', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), 'cwHRMStaffRole', '{guid_group}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{guid_group}', '{guid_staff}')
				";

			TestConnection.Command(sqlText).ExecuteNonQuery();

			var staff = Factory.Load<GlbStaff>(guid_staff);

			AssertEquals(1, staff.DatabaseAccessGroupRolesOriginalValues.Count);
			AssertEquals(1, staff.DatabaseAccessGroupRoles.Count);
			Assert(staff.DatabaseAccessGroupRoles.Contains("cwHRMStaffRole"));
		}

		public void TestGetGlbGroupDatabaseAccessRoles_NonDatabaseAccessRoles()
		{
			var guid_staff = Guid.NewGuid();
			var guid_group = Guid.NewGuid();

			string sqlText = $@"
				------ GlbStaff
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff}', 'TSH', 'testLoginHRMStaffRole', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				------ HRMStaff Database Access Groups
				INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsActive) VALUES ('{guid_group}', 'TG_x1x', 'Test Group HRMStaff', 1)

				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), 'DB_DATAWRITER', '{guid_group}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), 'cwrestrictedreaderrole', '{guid_group}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), 'db_BackupOperator', '{guid_group}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), 'cwhrmStaffRole', '{guid_group}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), 'NonDatabaseAccessRole1', '{guid_group}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), 'departmentManagementGroupEdit', '{guid_group}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{guid_group}', '{guid_staff}')
				";

			TestConnection.Command(sqlText).ExecuteNonQuery();

			var staff = Factory.Load<GlbStaff>(guid_staff);

			AssertEquals(4, staff.DatabaseAccessGroupRolesOriginalValues.Count);
			AssertEquals(4, staff.DatabaseAccessGroupRoles.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "db_datawriter", "cwRestrictedReaderRole", "db_backupoperator", "cwHRMStaffRole" }, staff.DatabaseAccessGroupRolesOriginalValues);
			AssertContainsExactElementsInAnyOrder(new string[] { "db_datawriter", "cwRestrictedReaderRole", "db_backupoperator", "cwHRMStaffRole" }, staff.DatabaseAccessGroupRoles);
		}

		[UseSnapshotProtection]
		public void TestDatabaseAccessGroupRolesOriginalValues()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_FullName = "Test staff E";
			staff1.GS_LoginName = "testLoginE";
			staff1.GS_Code = "TSD";

			staff1.IsDatabaseDeveloper = true;
			staff1.IsReadOnlyDBUser = true;
			staff1.IsBackupOperator = true;

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				Factory.Save();
			}

			staff1.IsDatabaseDeveloper = false;
			staff1.IsReadOnlyDBUser = false;
			staff1.IsBackupOperator = false;

			CombineAssertions(() =>
			{
				AssertEquals(0, staff1.DatabaseAccessGroupRoles.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "db_datawriter", "cwRestrictedReaderRole", "db_backupoperator" }, staff1.DatabaseAccessGroupRolesOriginalValues);
			});
		}

		#endregion

		#region Test events fired

		#region Test SalesRepStatusChange Event

		public void TestCommissionBaseWhenSalesRepChanges()
		{
			AssertEquals("Precondition: IsSalesRep is false", ZBool.False, Staff.GS_IsSalesRep);

			Staff.GS_CommissionBasis = CommissionBasisType.Codes.PRF;
			AssertEquals(CommissionBasisType.Codes.PRF, Staff.GS_CommissionBasis);
			Staff.GS_IsSalesRep = ZBool.True;
			AssertEquals(CommissionBasisType.Codes.PRF, Staff.GS_CommissionBasis);
			Staff.GS_IsSalesRep = ZBool.False;
			AssertEquals(ZString.Empty, Staff.GS_CommissionBasis);
		}

		public void TestSalesRepStatusChangeEvent()
		{
			AssertEquals("Precondition: IsSalesRep is false", ZBool.False, Staff.GS_IsSalesRep);
			SalesRepStatusChanged = false;

			Staff.GS_IsSalesRep = ZBool.True;
			AssertEquals(false, SalesRepStatusChanged);

			Staff.SalesRepStatusChanged += new GlbStaff.SalesRepStatusChangeHandler(OnSalesRepStatusChange);

			Staff.GS_IsSalesRep = ZBool.False;
			AssertEquals(true, SalesRepStatusChanged);

			SalesRepStatusChanged = false;
			Staff.GS_IsSalesRep = ZBool.True;
			AssertEquals(true, SalesRepStatusChanged);
		}

		void OnSalesRepStatusChange()
		{
			SalesRepStatusChanged = true;
		}

		bool SalesRepStatusChanged;

		#endregion

		public void TestChangeActualSecurityPermissions()
		{
			using (StaffFormForTest form = new StaffFormForTest(Staff))
			{
				Assert("Event not fired", !form.SecurityEventFired);
				Staff.OnActualSecurityPermissionsChange();
				Assert("Event fired", form.SecurityEventFired);
			}
		}

		public void TestSecurityBranchFiresChangeActualSecurityPermissions()
		{
			GlbBranch anotherBranch = Factory.New<GlbBranch>();
			anotherBranch.GB_Code = "AAA";

			using (StaffFormForTest staffForm = new StaffFormForTest(Staff))
			{
				Assert("Event not fired", !staffForm.SecurityEventFired);
				Staff.SecurityBranch = anotherBranch.PK;
				Assert("Event fired", staffForm.SecurityEventFired);
			}
		}

		public void TestSecurityDepartmentFiresChangeActualSecurityPermissions()
		{
			GlbDepartment anotherDepartment = Factory.New<GlbDepartment>();
			anotherDepartment.GE_Code = "AAA";

			using (StaffFormForTest staffForm = new StaffFormForTest(Staff))
			{
				Assert("Event not fired", !staffForm.SecurityEventFired);
				Staff.SecurityDepartment = anotherDepartment.PK;
				Assert("Event fired", staffForm.SecurityEventFired);
			}
		}

		public void TestSaveNewStaffMember()
		{
			GlbStaff staffToSave = Factory.NewWithValidTestData<GlbStaff>();
			using (StaffFormForTest form = new StaffFormForTest(staffToSave))
			{
				Assert("Event not fired", !form.SaveNewStaffFired);
				Factory.Save();
				Assert("Staff is new, Event fired", form.SaveNewStaffFired);
			}

			using (StaffFormForTest form2 = new StaffFormForTest(staffToSave))
			{
				Assert("Event not fired", !form2.SaveNewStaffFired);
				Factory.Save();
				Assert("Staff has been saved previously, Event not fired", !form2.SaveNewStaffFired);
			}
		}

		#endregion

		#region Test IContactable

		public void TestIContactable()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "name";
			staff.GS_EmailAddress = "email@example.com";
			staff.GS_MobilePhone = "1234567";
			staff.GS_IsActive = false;
			AssertEquals("name", ((IContactable)staff).Name);
			AssertEquals("email@example.com", ((IContactable)staff).Email);
			AssertEquals("1234567", ((IContactable)staff).Mobile);
			AssertEquals(false, ((IContactable)staff).IsActive);
		}

		#endregion

		#region Test IDocManagerSupport

		public void TestDocManagerCode()
		{
			AssertEquals("Code should be Staff. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "STF", ((IDocManagerSupport)Staff).DocManagerInfo.DocManagerCode);
		}

		public void TestIGlbCompanyCampaignItemRecipientMembers()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_MobilePhone = "0001mobile";
			staff.GS_Title = "Mr.";
			staff.GS_FaxNum = "FAX101";
			staff.GS_FullName = "Alan";

			IGlbCompanyCampaignItemRecipient recipient = staff;
			AssertEquals("0001mobile", recipient.Phone);
			AssertNull(recipient.Organisation);
			AssertEquals("", recipient.Salutation);
			AssertEquals("Mr.", recipient.Title);
			AssertEquals("FAX101", recipient.Fax);
			AssertEquals("Staff Alan", recipient.RelatedDocName);
		}

		#endregion

		#region Test Validation Suspended For Staff

		public void TestValidationSuspendedForStaff()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			using (staff.GetValidationSuspender())
			{
				_ = staff.StaffSecurityPermissionsCollection; //this now hits staff.Groups which runs validation because ALL group gets added
				staff.GS_NextReviewDate = ZDateTime.Invalid;
			}

			staff.ValidationSuspendedForStaff = true;
			staff.MarkAsNeedingValidationIncludingChildren();
			staff.RunPreSaveValidation();
			AssertNoNotifications("Staff validation is suspended, should NOT be validated on PreSaveValidation.", staff.GS_NextReviewDateInfo);

			staff.ValidationSuspendedForStaff = false;
			staff.MarkAsNeedingValidationIncludingChildren();
			staff.RunPreSaveValidation();
			AssertHasNotifications("Staff validation is NOT suspended, should be validated on PreSaveValidation.", staff.GS_NextReviewDateInfo);
		}

		#endregion

		[UseSnapshotProtection]
		public void TestLastAdministratorCannotBeDeactivated()
		{
			var initialStaff = Factory.New<GlbStaff>();
			initialStaff.GS_LoginName = "First Controller Staff";
			initialStaff.GS_IsActive = true;
			initialStaff.GS_IsController = true;
			Factory.Save();

			foreach (var staff in Factory.Load<GlbStaff>(new ZQuery()))
			{
				staff.GS_IsOperational = true;
				staff.GS_IsController = false;
			}

			try
			{
				Factory.Save();
				Fail("Save should throw ZCannotSaveException");
			}
			catch (ZCannotSaveException ex)
			{
				AssertEquals("You are attempting to affect the last active non operational or controller staff member.", ex.Message);
			}
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestIsDeviceOnlyFlagChangesAreLogged()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.StaffFlaggedAsDeviceOnlyCode || log.SL_SE_NKEvent == Events.StaffUnFlaggedAsDeviceOnlyCode)
				.OrderByDescending(log => log.SL_EventTime).ToList();
			AssertEquals(0, logs.Count);

			staff.GS_IsDevice = true;
			Factory.Save();

			logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.StaffFlaggedAsDeviceOnlyCode || log.SL_SE_NKEvent == Events.StaffUnFlaggedAsDeviceOnlyCode)
				.OrderByDescending(log => log.SL_EventTime).ToList();
			AssertEquals(1, logs.Count);
			AssertEquals(Events.StaffFlaggedAsDeviceOnly.Code, logs[0].SL_SE_NKEvent);

			staff.GS_IsDevice = true;
			Factory.Save();

			logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.StaffFlaggedAsDeviceOnlyCode || log.SL_SE_NKEvent == Events.StaffUnFlaggedAsDeviceOnlyCode)
				.OrderByDescending(log => log.SL_EventTime).ToList();
			AssertEquals("Doesn't add a new log if original value hasn't changed", 1, logs.Count);

			staff.GS_IsDevice = false;
			Factory.Save();

			logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.StaffFlaggedAsDeviceOnlyCode || log.SL_SE_NKEvent == Events.StaffUnFlaggedAsDeviceOnlyCode)
				.OrderByDescending(log => log.SL_EventTime).ToList();
			AssertEquals(2, logs.Count);
			AssertEquals(Events.StaffUnFlaggedAsDeviceOnly.Code, logs[0].SL_SE_NKEvent);
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestIsRobotFlagChangesAreLogged()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == "SRF" || log.SL_SE_NKEvent == "SRU")
				.OrderByDescending(log => log.SL_EventTime).ToList();
			AssertEquals(0, logs.Count);

			staff.GS_IsRobot = true;
			Factory.Save();

			logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == "SRF" || log.SL_SE_NKEvent == "SRU")
				.OrderByDescending(log => log.SL_EventTime).ToList();
			AssertEquals(1, logs.Count);
			AssertEquals("SRF", logs[0].SL_SE_NKEvent);

			staff.GS_IsRobot = true;
			Factory.Save();

			logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == "SRF" || log.SL_SE_NKEvent == "SRU")
				.OrderByDescending(log => log.SL_EventTime).ToList();
			AssertEquals("Doesn't add a new log if original value hasn't changed", 1, logs.Count);

			staff.GS_IsRobot = false;
			Factory.Save();

			logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == "SRF" || log.SL_SE_NKEvent == "SRU")
				.OrderByDescending(log => log.SL_EventTime).ToList();
			AssertEquals(2, logs.Count);
			AssertEquals("SRU", logs[0].SL_SE_NKEvent);
		}

		public void TestUserSeatEventLogged()
		{
			// New active standard user => UserSeat event should be logged
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_IsDevice = false;
			staff.GS_CanLogin = true;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 1);
			AssertSpecificUserSeatCount(staff, "NEW user", "NEW>>USR", 1);

			// Make an unrelated change => UserSeat event should NOT be logged
			staff.GS_IsSalesRep = !staff.GS_IsSalesRep;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 1);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);

			// Set user as inactive => UserSeat event should be logged
			staff.GS_IsActive = false;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 2);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2)", "USR>>INA", 1);

			// Change device status of inactive user => no UserSeat event to be logged
			staff.GS_IsDevice = true;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 2);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2)", "USR>>INA", 1);

			// Set as active and not device user => UserSeat event should be logged
			staff.GS_IsActive = true;
			staff.GS_IsDevice = false;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 3);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2)", "USR>>INA", 1);
			AssertSpecificUserSeatCount(staff, "User status change (3)", "INA>>USR", 1);

			// Set User as Cannot login => UserSeat event should be logged
			staff.GS_CanLogin = false;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 4);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2)", "USR>>INA", 1);
			AssertSpecificUserSeatCount(staff, "User status change (3)", "INA>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (4)", "USR>>HRU", 1);

			// Set User as Can login => UserSeat event should be logged
			staff.GS_CanLogin = true;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 5);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2)", "USR>>INA", 1);
			AssertSpecificUserSeatCount(staff, "User status change (3)", "INA>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (4)", "USR>>HRU", 1);
			AssertSpecificUserSeatCount(staff, "User status change (5)", "HRU>>USR", 1);

			// Set user as inactive device => UserSeat event should be logged
			staff.GS_IsActive = false;
			staff.GS_IsDevice = true;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 6);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2+6)", "USR>>INA", 2);
			AssertSpecificUserSeatCount(staff, "User status change (3)", "INA>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (4)", "USR>>HRU", 1);
			AssertSpecificUserSeatCount(staff, "User status change (5)", "HRU>>USR", 1);

			// Set User as Cannot login => UserSeat event should NOT be logged
			staff.GS_CanLogin = false;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 6);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2+6)", "USR>>INA", 2);
			AssertSpecificUserSeatCount(staff, "User status change (3)", "INA>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (4)", "USR>>HRU", 1);
			AssertSpecificUserSeatCount(staff, "User status change (5)", "HRU>>USR", 1);

			// Set HR User as active => UserSeat event should be logged
			staff.GS_IsActive = true;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 7);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2+6)", "USR>>INA", 2);
			AssertSpecificUserSeatCount(staff, "User status change (3)", "INA>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (4)", "USR>>HRU", 1);
			AssertSpecificUserSeatCount(staff, "User status change (5)", "HRU>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (7)", "INA>>HRU", 1);

			// Set can login = true => UserSeat event should be logged
			staff.GS_CanLogin = true;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 8);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2+6)", "USR>>INA", 2);
			AssertSpecificUserSeatCount(staff, "User status change (3)", "INA>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (4)", "USR>>HRU", 1);
			AssertSpecificUserSeatCount(staff, "User status change (5)", "HRU>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (7)", "INA>>HRU", 1);
			AssertSpecificUserSeatCount(staff, "User status change (8)", "HRU>>DOU", 1);

			// Set robot = true => UserSeat event should be logged
			staff.GS_IsRobot = true;
			Factory.Save();
			AssertUserSeatCount(staff, "All UST events", 9);
			AssertSpecificUserSeatCount(staff, "User status change (1)", "NEW>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (2+6)", "USR>>INA", 2);
			AssertSpecificUserSeatCount(staff, "User status change (3)", "INA>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (4)", "USR>>HRU", 1);
			AssertSpecificUserSeatCount(staff, "User status change (5)", "HRU>>USR", 1);
			AssertSpecificUserSeatCount(staff, "User status change (7)", "INA>>HRU", 1);
			AssertSpecificUserSeatCount(staff, "User status change (8)", "HRU>>DOU", 1);
			AssertSpecificUserSeatCount(staff, "User status change (9)", "DOU>>RBU", 1);
		}

		void AssertUserSeatCount(GlbStaff staff, string messagePrefix, int expectedCount)
		{
			var logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.UserSeatCode);
			AssertEquals(messagePrefix + " => UserSeat event count", expectedCount, logs.Count());
		}

		void AssertSpecificUserSeatCount(GlbStaff staff, string messagePrefix, string logReference, int expectedCount)
		{
			var logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.UserSeatCode && log.SL_Reference == logReference);
			AssertEquals(messagePrefix + " => UserSeat event count", expectedCount, logs.Count());
		}

		public void TestCLUserLogsEvents()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_IsDevice = false;
			Factory.Save();

			// Set user as Can login User true => CFL & UCL events should be logged
			staff.GS_CanLogin = true;
			Factory.Save();
			var logs = staff.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == Events.UserMarkedAsCanLoginCode);
			AssertEquals("Set user as can login", 0, logs.Count());

			// Set user as Can login User true => CFL & UCL events should be logged
			staff.GS_CanLogin = false;
			Factory.Save();
			logs = staff.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == Events.UserMarkedAsCannotLoginCode);
			AssertEquals("Set user as cannot login", 1, logs.Count());
		}

		public void TestUserSeatEventNotLoggedForResources()
		{
			// New resource user => no UserSeat logs on creation
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_IsDevice = false;
			staff.GS_IsResource = true;
			Factory.Save();

			var logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.UserSeatCode);
			AssertEquals("Resource user, UserSeat event logged?", false, logs.Any());

			// Set resource user as inactive => no UserSeat event should be logged
			staff.GS_IsActive = false;
			Factory.Save();

			logs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.UserSeatCode);
			AssertEquals("Resource user set to inactive, UserSeat event logged?", false, logs.Any());
		}

		public void TestUserSeatEventLoggedForNewDeviceUser()
		{
			// New active device user => UserSeat logged on creation
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_IsDevice = true;
			Factory.Save();

			AssertUserSeatCount(staff, "All UST events", 1);
			AssertSpecificUserSeatCount(staff, "New Device Only User", "NEW>>DOU", 1);

			// Set device user as inactive => another UserSeat event should be logged
			staff.GS_IsActive = false;
			Factory.Save();

			AssertUserSeatCount(staff, "All UST events", 2);
			AssertSpecificUserSeatCount(staff, "Device only user set as inactive", "DOU>>INA", 1);
		}

		public void TestUserSeatEventLoggedForNewRobotUser()
		{
			// New user => UserSeat logged on creation
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_IsRobot = true;
			Factory.Save();

			AssertUserSeatCount(staff, "All UST events", 1);
			AssertSpecificUserSeatCount(staff, "New User", "NEW>>RBU", 1);

			// Set robot user as inactive => another UserSeat event should be logged
			staff.GS_IsActive = false;
			Factory.Save();

			AssertUserSeatCount(staff, "All UST events", 2);
			AssertSpecificUserSeatCount(staff, "Robot user set as inactive", "RBU>>INA", 1);
		}

		public void TestUserSeatEventLoggedForNewHumanResourceUser()
		{
			// New active HR user => UserSeat logged on creation
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_CanLogin = false;
			// IsDevice value is irrelevant when CanLogin is false (still an HR user).
			staff.GS_IsDevice = true;
			Factory.Save();

			AssertUserSeatCount(staff, "All UST events", 1);
			AssertSpecificUserSeatCount(staff, "New HR User", "NEW>>HRU", 1);

			// Set HR user as inactive => another UserSeat event should be logged
			staff.GS_IsActive = false;
			Factory.Save();

			AssertUserSeatCount(staff, "All UST events", 2);
			AssertSpecificUserSeatCount(staff, "Device only user set as inactive", "HRU>>INA", 1);
		}

		public void TestUnsavedStatusChangeLogsAreDeletedIfSaveFails()
		{
			// New device user => no status change logs
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "~!@";
			staff.GS_IsDevice = true;
			Factory.Save();
			AssertStatusChangeLogs(staff, expectedUserSeat: 1, expectedFlaggedAsDevice: 0, expectedUnflaggedAsDevice: 0);

			// Set as not device + duplicated code => Should fail and hence no new logs
			staff.GS_Code = "E";
			staff.GS_IsDevice = false;

			try
			{
				Factory.Save();
				Fail("The save should have failed");
			}
			catch (ZSaveException)
			{
				AssertStatusChangeLogs(staff, expectedUserSeat: 1, expectedFlaggedAsDevice: 0, expectedUnflaggedAsDevice: 0);
			}

			// Fix code => new logs expected
			staff.GS_Code = "~!@";
			Factory.Save();
			AssertStatusChangeLogs(staff, expectedUserSeat: 2, expectedFlaggedAsDevice: 0, expectedUnflaggedAsDevice: 1);

			// Set as device + duplicated code => Should fail and hence no new logs
			staff.GS_Code = "E";
			staff.GS_IsDevice = true;

			try
			{
				Factory.Save();
				Fail("The save should have failed");
			}
			catch (ZSaveException)
			{
				AssertStatusChangeLogs(staff, expectedUserSeat: 2, expectedFlaggedAsDevice: 0, expectedUnflaggedAsDevice: 1);
			}

			// Fix code => new logs expected
			staff.GS_Code = "~!@";
			Factory.Save();
			AssertStatusChangeLogs(staff, expectedUserSeat: 3, expectedFlaggedAsDevice: 1, expectedUnflaggedAsDevice: 1);
		}

		void AssertStatusChangeLogs(GlbStaff staff, int expectedUserSeat, int expectedFlaggedAsDevice, int expectedUnflaggedAsDevice)
		{
			var userSeatLogs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.UserSeatCode);
			AssertEquals("UserSeat event count", expectedUserSeat, userSeatLogs.Count());

			var flaggedAsDeviceLogs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.StaffFlaggedAsDeviceOnlyCode);
			AssertEquals("UserSeat event count", expectedFlaggedAsDevice, flaggedAsDeviceLogs.Count());

			var unflaggedAsDeviceLogs = staff.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.StaffUnFlaggedAsDeviceOnlyCode);
			AssertEquals("UserSeat event count", expectedUnflaggedAsDevice, unflaggedAsDeviceLogs.Count());
		}

		public void TestSendUpdateExternalPasswords()
		{
			var sgWrapper = Staff.GetSGWrapper();
			sgWrapper.AccessPassword.CurrentDecryptedPassword = "NewAccess";
			sgWrapper.AccessPassword.NextDecryptedPassword = "NextAccess";

			var itWrapper = Staff.GetITWrapper();
			var itbPass = (GlbExternalPassword)itWrapper.PasswordCollection.AddNew();
			itbPass.GP_GS = Staff.PK;
			itbPass.GP_PasswordType = "ITB";

			itbPass.GP_PasswordStatus = "XYZ";
			itbPass.GP_MailBoxID = "Mail";
			itbPass.GP_UserID = "ZAC";
			itbPass.GP_Certificate = new ZBlob(System.Text.Encoding.UTF8.GetBytes("file"));
			itbPass.CurrentDecryptedCertificatePassphrase = "Pass";

			var nzWrapper = Staff.GetNZWrapper();
			nzWrapper.NZBPassword.CurrentDecryptedPassword = "NZBPass";

			sgWrapper.SGNationalTradePlatformPassword.CurrentDecryptedPassword = "SGNPass";
			sgWrapper.SGNationalTradePlatformPassword.NextDecryptedPassword = "NextSGNPass";
			sgWrapper.SGNationalTradePlatformPassword.GP_UserID = "Zac";
			sgWrapper.SGNationalTradePlatformPassword.GP_GC = GlbCompany.CurrentCompany.PK;

			sgWrapper.Tradenetv4Password.CurrentDecryptedPassword = "v4Pass";
			sgWrapper.Tradenetv4Password.NextDecryptedPassword = "Nextv4Pass";

			var auWrapper = Staff.GetAUWrapper();
			auWrapper.NUTPassword.CurrentDecryptedPassword = "nutPass";
			auWrapper.NUTPassword.GP_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var messages = Factory.Load<IEDIInterchange>(zQuery);

			AssertEquals("4 messages should be created", 4, messages.Length);
			var nexdocsMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"NEXDOCSUserLevel\""));
			AssertNotNull("One NEXDOCSUserLevel should be created", nexdocsMess);
			AssertContains("The NEXDOCSUserLevel should have a company level", "Group Type=\"Company\" Reference=\"EDI\"", nexdocsMess.EI_BodyText);

			var sgntpMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"SGCustomsNTP\""));
			AssertNotNull("One SGNTP mesage should be created", sgntpMess);
			AssertContains("The SGNTP mesage should have a company level", "Group Type=\"Company\" Reference=\"EDI\"", sgntpMess.EI_BodyText);

			var sgsgaMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"SGCustomsConfiguration\""));
			AssertNotNull("One SGSGA mesage should be created", sgsgaMess);
			AssertContains("The SGSGA mesage should have a company level", "Group Type=\"Company\" Reference=\"EDI\"", sgsgaMess.EI_BodyText);

			var itMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"ITCustomsSubscribers\""));
			AssertNotNull("One ITCustomsSubscribers should be created", itMess);
			AssertNotContains("The ITCustomsSubscribers should be at system level", "Group Type=\"Company\"", itMess.EI_BodyText);

			AssertEquals("Full Message", Message, Regex.Replace(sgntpMess.EI_BodyText, "<Password>.*</Password>", "<Password>Pass</Password>"));

			sgWrapper.AccessPassword.CurrentDecryptedPassword = "";
			sgWrapper.SGNationalTradePlatformPassword.CurrentDecryptedPassword = "";
			sgWrapper.SGNationalTradePlatformPassword.NextDecryptedPassword = "";
			sgWrapper.SGNationalTradePlatformPassword.GP_UserID = "";
			itbPass.Delete();
			auWrapper.NUTPassword.CurrentDecryptedPassword = "";

			nexdocsMess.Delete();
			sgntpMess.Delete();
			sgsgaMess.Delete();
			itMess.Delete();

			Factory.Save();
			Staff.OnSaved(true);

			messages = Factory.Load<IEDIInterchange>(zQuery);
			AssertEquals("4 messages should be created", 4, messages.Length);
			nexdocsMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"NEXDOCSUserLevel\""));
			AssertNotNull("One NEXDOCSUserLevel should be created", nexdocsMess);
			AssertContains("The NEXDOCSUserLevel should have a company level", "Group Type=\"Company\" Reference=\"EDI\"", nexdocsMess.EI_BodyText);

			sgsgaMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"SGCustomsConfiguration\""));
			AssertNotNull("One SGSGA mesage should be created", sgsgaMess);
			AssertContains("The SGSGA mesage should have a company level", "Group Type=\"Company\" Reference=\"EDI\"", sgsgaMess.EI_BodyText);

			sgntpMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"SGCustomsNTP\""));
			AssertNotNull("One SGNTP mesage should be created", sgntpMess);
			AssertContains("The SGNTP mesage should have a company level", "Group Type=\"Company\" Reference=\"EDI\"", sgntpMess.EI_BodyText);

			itMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"ITCustomsSubscribers\""));
			AssertNotNull("One ITCustomsSubscribers should be created", itMess);
			AssertNotContains("The ITCustomsSubscribers should be at system level", "Group Type=\"Company\"", itMess.EI_BodyText);
		}

#if NET
		const string Message = @"<Configuration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Name=""SGCustomsNTP"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""EDI"">
      <Group Type=""Staff"" Reference=""ZAC"">
        <Group Type=""NTP"" Status=""VAL"">
          <Credential Name=""Current"">
            <UserName>Zac</UserName>
            <Password>Pass</Password>
          </Credential>
          <Credential Name=""Next"">
            <UserName>Zac</UserName>
            <Password>Pass</Password>
          </Credential>
        </Group>
      </Group>
    </Group>
  </Group>
</Configuration>";
#else
		const string Message = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""SGCustomsNTP"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""EDI"">
      <Group Type=""Staff"" Reference=""ZAC"">
        <Group Type=""NTP"" Status=""VAL"">
          <Credential Name=""Current"">
            <UserName>Zac</UserName>
            <Password>Pass</Password>
          </Credential>
          <Credential Name=""Next"">
            <UserName>Zac</UserName>
            <Password>Pass</Password>
          </Credential>
        </Group>
      </Group>
    </Group>
  </Group>
</Configuration>";
#endif

		#region Test Loading and defaults

		public void TestSavingCreatesCode()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "Brett";
			staff1.GS_FullName = "Brett Shearer";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "John";
			staff2.GS_FriendlyName = "John Smith";

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_LoginName = "Bazza";
			staff3.GS_FriendlyName = "Bazza";
			staff3.GS_FullName = "William Tell";

			Factory.Save();

			AssertEquals("BS", staff1.GS_Code);
			AssertEquals("JS", staff2.GS_Code);
			AssertEquals("WT", staff3.GS_Code);
		}

		public void TestUniqueCodes()
		{
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var staff2 = factory2.New<GlbStaff>();
			staff2.GS_LoginName = "Login Name2";
			staff2.GS_FullName = "Full Name";

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var person1 = factory1.NewWithValidTestData<GlbPerson>();
			factory1.Save();

			var staff1 = factory1.New<GlbStaffWithDelay>();
			staff1.GS_LoginName = "Login Name1";
			staff1.GS_FullName = "Full Name";
			staff1.SetAction(() => { factory2.Save(); });
			staff1.GS_PER = person1.PK;

			try
			{
				factory1.Save();
				Fail("should throw a ZSaveConcurrencyException");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertEquals("A staff with this code already exists in database. If the code was auto-generated, please Save the form again.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("", staff1.GS_Code);
			AssertEquals("FN", staff2.GS_Code);

			factory1.Save();
			AssertEquals("FNM", staff1.GS_Code);
		}

		[ExpectException(typeof(ZCannotSaveException))]
		public void TestSaveWithEmptyCode()
		{
			Factory.New<GlbStaff>().DoNotGenerateCodeForEmptyName4Test = true;
			Factory.Save();
		}

		public virtual void TestDefaultValues()
		{
			var staff = Factory.New<GlbStaff>();
			AssertEquals(false, staff.GS_IsResource);
			AssertEquals(false, staff.GS_IsDevice);
			AssertEquals(true, staff.GS_CanLogin);

			AssertEquals("LastPasswordChangeData should be Today", ZDateTime.Today, staff.GS_LastPasswordChangeDate);
			AssertEquals("ChangePasswordAtNextLogin should be true", true, staff.GS_ChangePasswordAtNextLogin);
			AssertEquals("New Staff member's email should be published by default", true, staff.GS_PublishEmailAddress);
			AssertEquals("Staff member should be added to the All Users Group", "ALL", staff.Groups[0].GG_Code);
			AssertEquals("UseTransactionCompanyAsPreferredPayment should be true by default", true, staff.UseTransactionCompanyAsPreferredPayment);
		}

		public void TestInactiveStaffNotAddedToAllUsersGroup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var pk = staff.PK;
			Assert("Staff member should be added to the All Users Group", staff.Groups.Any(group => (group as GlbGroup).GG_Code == "ALL"));
			staff.GS_IsActive = false;
			for (int i = staff.AllGroups.Count - 1; i >= 0; --i)
			{
				staff.AllGroups.Remove(staff.AllGroups[i], true);
			}
			Factory.Save();
			var reloadedStaff = Factory.Load<GlbStaff>(pk);
			AssertEquals("Staff member should NOT be added to the All Users Group", 0, reloadedStaff.Groups.Count);
		}

		public void TestAllGroupsContainsAllUsersGroup()
		{
			var otherFactory = new BusinessObjectFactory();
			var staff = otherFactory.NewWithValidTestData<GlbStaff>();
			otherFactory.Save();

			Db.Connection.ExecuteNonQuery(FormattableString.Invariant($"DELETE FROM dbo.GlbGroupLink WHERE GK_GS = '{staff.PK}'"));

			var reloaded = Factory.Load<GlbStaff>(staff.PK);
			var allUsersGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
			Assert("AllGroups should contain AllUsers group", reloaded.AllGroups.Contains(allUsersGroup));
			Assert("ActiveGroups should contain AllUsers group", reloaded.ActiveGroups.Contains(allUsersGroup));
			Assert("Groups should contain AllUsers group", reloaded.Groups.Contains(allUsersGroup));
		}

		public void TestPostopneGroupSetupWhileCopying()
		{
			GlbStaff staff;
			using (BusinessObjectUniversalCopyFactoryService.EnsureServiceIsSetUp(Factory))
			{
				staff = Factory.New<GlbStaff>();
				AssertEquals(0, staff.Groups.Count);
			}
			AssertEquals(1, staff.Groups.Count);
			AssertEquals("Staff member should be added to the All Users Group", "ALL", staff.Groups[0].GG_Code);
		}

		public void TestOnLoaded()
		{
			Factory.Save();

			BusinessObjectFactory newFactory1 = new BusinessObjectFactory();
			GlbStaff staffLoaded = newFactory1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, Staff.PK));
			AssertEquals(GlbStaff.CurrentUser.GS_IsController, !staffLoaded.GS_IsControllerInfo.ReadOnly);

			using (Env.SetTemporaryUserContext("user that doesn't exist", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Current user should be null", GlbStaff.CurrentUser, null);

				BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
				GlbStaff staffLoadedWhenCurrentUserNull = newFactory2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, Staff.PK));
				Assert("When current user is null GS_IsController should be read only", staffLoadedWhenCurrentUserNull.GS_IsControllerInfo.ReadOnly);
			}
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestNullGS_GeoLocationIsNotNullWhenLoadingIntoDB()
		{
			// NOTE: Need to revert this UT after ARC team improved AutoRegen support to ZGeography (WI00169943).

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_GeoLocation = ZGeography.Empty;
			AssertEquals("Directly setting GS_GeoLocation to empty should be treated as invalid if not nullable", ZGeography.Empty, staff1.GS_GeoLocation);
			AssertNoExceptionThrown("Saving a new (non-nullable) GS_GeoLocation row with an empty/invalid ZGeography should not be saved as DBNull", () => Factory.Save());

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var pk2 = staff2.PK;
			AssertEquals("Default value of GS_GeoLocation for GlbStaff is an empty ZGeography", ZGeography.Empty.AsText(), staff2.GS_GeoLocation.AsText());
			Factory.Save();

			staff2.GS_GeoLocation = null;
			AssertEquals("Setting GS_GeoLocation directly to null should result in a non-null value representing 'invalid' if non-nullable or empty otherwise", ZGeography.Empty, staff2.GS_GeoLocation);
			AssertNoExceptionThrown("Updating the (non-nullable) GS_GeoLocation row with an empty/invalid ZGeography should not be saved as DBNull", () => Factory.Save());

			var reloadedStaff = Factory.Load<GlbStaff>(pk2);
			AssertEquals("Staff GS_GeoLocation should not be null when loaded from DB", ZGeography.Empty, reloadedStaff.GS_GeoLocation);
		}

		#endregion

		#region Test Related Business Objects

		public void TestCertificates()
		{
			AssertType(typeof(GenRegCertAccredMaintListCollection), Staff.Certificates);
		}

		public void TestTimeAllocation()
		{
			GlbStaff bizo = (GlbStaff)GetNewBusinessObject();
			AssertNotNull(bizo.TimeAllocationFilter);

			bizo.TimeAllocationFilter.StartTime = ZDateTime.Empty;
			bizo.TimeAllocationFilter.EndTime = ZDateTime.Empty;
			AssertEquals(0, bizo.TimeAllocationFilter.AllocationsView.Count);

			bizo.RecordOrUpdateTimeAllocation(null, "ABC", new ZDateTime(2006, 1, 1, 9, 0, 0), new ZDateTime(2006, 1, 1, 11, 30, 0), "hello");
			AssertEquals(1, bizo.TimeAllocationFilter.AllocationsView.Count);

			AssertEquals("ABC", bizo.TimeAllocationFilter.AllocationsView[0].GA_WorkHolidayType);
			AssertEquals(new ZDateTime(2006, 1, 1, 9, 0, 0), bizo.TimeAllocationFilter.AllocationsView[0].GA_StartTime);
			AssertEquals(new ZDateTime(2006, 1, 1, 11, 30, 0), bizo.TimeAllocationFilter.AllocationsView[0].GA_EndTime);
			AssertEquals("hello", bizo.TimeAllocationFilter.AllocationsView[0].GA_LeaveComment);
			AssertEquals(ZGuid.Empty, bizo.TimeAllocationFilter.AllocationsView[0].GA_ParentID);
			AssertEquals("", bizo.TimeAllocationFilter.AllocationsView[0].GA_ParentTableCode);

			bizo.TimeAllocationFilter.AllocationsView.RemoveAndDeleteAll();

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			bizo.RecordOrUpdateTimeAllocation(dummy, "DEF", new ZDateTime(2006, 1, 1, 11, 0, 0), new ZDateTime(2006, 1, 1, 13, 30, 0), "");
			AssertEquals(1, bizo.TimeAllocationFilter.AllocationsView.Count);

			AssertEquals("DEF", bizo.TimeAllocationFilter.AllocationsView[0].GA_WorkHolidayType);
			AssertEquals(new ZDateTime(2006, 1, 1, 11, 0, 0), bizo.TimeAllocationFilter.AllocationsView[0].GA_StartTime);
			AssertEquals(new ZDateTime(2006, 1, 1, 13, 30, 0), bizo.TimeAllocationFilter.AllocationsView[0].GA_EndTime);
			AssertEquals("", bizo.TimeAllocationFilter.AllocationsView[0].GA_LeaveComment);
			AssertEquals(dummy.PK, bizo.TimeAllocationFilter.AllocationsView[0].GA_ParentID);
			AssertEquals(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(dummy.TableName), bizo.TimeAllocationFilter.AllocationsView[0].GA_ParentTableCode);

			bizo.RecordOrUpdateTimeAllocation(dummy, "DEF", new ZDateTime(2006, 1, 1, 11, 0, 0), new ZDateTime(2006, 1, 1, 14, 30, 0), "");
			AssertEquals("Record updated - not added", 1, bizo.TimeAllocationFilter.AllocationsView.Count);
			AssertEquals(new ZDateTime(2006, 1, 1, 11, 0, 0), bizo.TimeAllocationFilter.AllocationsView[0].GA_StartTime);
			AssertEquals(new ZDateTime(2006, 1, 1, 14, 30, 0), bizo.TimeAllocationFilter.AllocationsView[0].GA_EndTime);
		}

		public void TestDangerousGoodsCertificateNumber()
		{
			AssertEquals("DangerousGoodsCertificateNumber", "", Staff.DangerousGoodsCertificateNumber);

			GenRegCertAccredMaintList certificate = Staff.Certificates.AddNew();
			certificate.XZ_Type = Core.Constants.StaffCertificateType.IATA;
			certificate.XZ_RefNumber = "TestIATA";
			AssertEquals("DangerousGoodsCertificateNumber", "", Staff.DangerousGoodsCertificateNumber);

			certificate = Staff.Certificates.AddNew();
			certificate.XZ_Type = Core.Constants.StaffCertificateType.DG;
			certificate.XZ_RefNumber = "TestExpired";
			certificate.XZ_ExpiryOrDueDate = new ZDateTime(2004, 1, 1);
			AssertEquals("DangerousGoodsCertificateNumber", "", Staff.DangerousGoodsCertificateNumber);

			certificate = Staff.Certificates.AddNew();
			certificate.XZ_Type = Core.Constants.StaffCertificateType.DG;
			certificate.XZ_RefNumber = "LicenseNumber";
			certificate.XZ_ExpiryOrDueDate = ZDateTime.Today;
			AssertEquals("DangerousGoodsCertificateNumber", "LicenseNumber", Staff.DangerousGoodsCertificateNumber);
		}

		public void TestGlbReleaseNoteRead()
		{
			GlbStaff staffWithNotes = Factory.NewWithValidTestData<GlbStaff>();
			GlbReleaseNote note1 = Factory.NewWithValidTestData<GlbReleaseNote>();
			GlbReleaseNote note2 = Factory.NewWithValidTestData<GlbReleaseNote>();

			GlbReleaseNoteRead note1Read = staffWithNotes.ReleaseNotesRead.AddNew();
			note1Read.GR_ReleaseNoteID = note1.PK;
			GlbReleaseNoteRead note2Read = staffWithNotes.ReleaseNotesRead.AddNew();
			note2Read.GR_ReleaseNoteID = note2.PK;
			Factory.Save();

			Assert("Note1Read should be in database", note1Read.IsInDatabase);
			Assert("Note2Read should be in database", note2Read.IsInDatabase);

			staffWithNotes.Delete();

			Factory.Save();

			Assert("Note1Read should NOT be in database", !note1Read.IsInDatabase);
			Assert("Note2Read should NOT be in database", !note2Read.IsInDatabase);
		}

		public void TestFountainsAndNumberRangeMatchingDetails()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.NumberRangeMatchingDetails.DeleteAll();
			staff.Fountains.DeleteAll();

			var stmNums1 = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<StaffViewStmNums>(staff, "1111111", type: OrgConstants.NumberFountains.Code.PatentNumber);
			var stmNums2 = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<StaffViewStmNums>(staff, "2222222", type: OrgConstants.NumberFountains.Code.PatentNumber);
			var details1 = StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(staff.Factory, staff.PK, OrgConstants.NumberFountains.Code.PatentNumber, "1111111", ownerTable: GlbStaffSchema.Constants.Prefix);

			Factory.Save();
			AssertEquals(2, staff.Fountains.Count);
			AssertEquals(1, staff.NumberRangeMatchingDetails.Count);

			staff.Delete();
			AssertEquals(true, stmNums1.IsDeleted);
			AssertEquals(true, stmNums2.IsDeleted);
			AssertEquals(true, details1.IsDeleted);
		}

		public void TestGroupSecurityPermissionsCollectionForBinding()
		{
			AssertEquals("GroupSecurityPermissionsCollectionForBinding.ReadOnly", true, Staff.GroupSecurityPermissionsCollectionForBinding.ReadOnly);
		}

		[TestDate(2014, 11, 11)]
		public void TestGetOverallCommissionRuleResponsibleFor()
		{
			var auCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var salesTeamAu = Factory.NewWithValidTestData<SalesTeam>();
			salesTeamAu.CoveredCountries.Add(auCountry);
			var usCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var salesTeamUs = Factory.NewWithValidTestData<SalesTeam>();
			salesTeamUs.CoveredCountries.Add(usCountry);
			var gbCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			var salesTeamGb = Factory.NewWithValidTestData<SalesTeam>();
			salesTeamGb.CoveredCountries.Add(gbCountry);
			var cnCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var salesTeamCn = Factory.NewWithValidTestData<SalesTeam>();
			salesTeamCn.CoveredCountries.Add(cnCountry);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			salesTeamAu.Staff.Add(staff);
			salesTeamUs.Staff.Add(staff);
			salesTeamGb.Staff.Add(staff);
			salesTeamCn.Staff.Add(staff);

			var auRuleA = salesTeamAu.CommissionRules.AddNew();
			auRuleA.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			auRuleA.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			auRuleA.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			auRuleA.ACM_SubModule = "";
			auRuleA.ACM_NKOrigin = "";
			auRuleA.ACM_NKDestination = "";
			auRuleA.FillWithValidTestData();

			var auRuleB = salesTeamAu.CommissionRules.AddNew();
			auRuleB.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			auRuleB.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			auRuleB.ACM_SubModule = "";
			auRuleB.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			auRuleB.ACM_NKOrigin = "";
			auRuleB.ACM_NKDestination = "";
			auRuleB.FillWithValidTestData();

			var auRuleC = salesTeamAu.CommissionRules.AddNew();
			auRuleC.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			auRuleC.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			auRuleC.ACM_SubModule = "";
			auRuleC.ACM_Mode = "ROA";
			auRuleC.ACM_NKOrigin = "";
			auRuleC.ACM_NKDestination = "";
			auRuleC.FillWithValidTestData();

			var usRule = salesTeamUs.CommissionRules.AddNew();
			usRule.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			usRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			usRule.ACM_SubModule = "";
			usRule.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			usRule.ACM_NKOrigin = "";
			usRule.ACM_NKDestination = "";
			usRule.FillWithValidTestData();

			var usRuleB = salesTeamUs.CommissionRules.AddNew();
			usRuleB.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			usRuleB.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			usRuleB.ACM_SubModule = "";
			usRuleB.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			usRuleB.ACM_NKOrigin = "AUSYD";
			usRuleB.ACM_NKDestination = "";
			usRuleB.FillWithValidTestData();

			var expiredGbRule = salesTeamGb.CommissionRules.AddNew();
			expiredGbRule.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			expiredGbRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			expiredGbRule.ACM_SubModule = "";
			expiredGbRule.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			expiredGbRule.ACM_NKOrigin = "";
			expiredGbRule.ACM_NKDestination = "";
			expiredGbRule.ACM_EndDate = new ZDate(2010, 1, 1);
			expiredGbRule.FillWithValidTestData();

			var cnRule = salesTeamCn.CommissionRules.AddNew();
			cnRule.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			cnRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			cnRule.ACM_SubModule = "";
			cnRule.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			cnRule.ACM_NKOrigin = "";
			cnRule.ACM_NKDestination = "";
			cnRule.FillWithValidTestData();

			var cnRuleB = salesTeamCn.CommissionRules.AddNew();
			cnRuleB.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			cnRuleB.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			cnRuleB.ACM_SubModule = "";
			cnRuleB.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			cnRuleB.ACM_NKDestination = "UAIEV";
			cnRuleB.ACM_NKOrigin = "";
			cnRuleB.FillWithValidTestData();

			var globalRuleA = staff.CommissionRules.AddNew();
			globalRuleA.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			globalRuleA.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			globalRuleA.ACM_SubModule = "";
			globalRuleA.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			globalRuleA.ACM_NKOrigin = "";
			globalRuleA.ACM_NKDestination = "";
			globalRuleA.FillWithValidTestData();

			var globalRuleB = staff.CommissionRules.AddNew();
			globalRuleB.ACM_Product = JobInvoicingConsumerTypes.QuotedBooking.Code;
			globalRuleB.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			globalRuleB.ACM_SubModule = "";
			globalRuleB.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			globalRuleB.ACM_NKOrigin = "";
			globalRuleB.ACM_NKDestination = "";
			globalRuleB.FillWithValidTestData();

			var globalRuleC = staff.CommissionRules.AddNew();
			globalRuleC.ACM_Product = JobInvoicingConsumerTypes.Project.Code;
			globalRuleC.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			globalRuleC.ACM_SubModule = "";
			globalRuleC.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			globalRuleC.ACM_NKOrigin = "";
			globalRuleC.ACM_NKDestination = "";
			globalRuleC.FillWithValidTestData();

			var currentCompanyRule = staff.CommissionRules.AddNew();
			currentCompanyRule.ACM_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyRule.ACM_Product = JobInvoicingConsumerTypes.Project.Code;
			currentCompanyRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			currentCompanyRule.ACM_SubModule = "";
			currentCompanyRule.ACM_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			currentCompanyRule.ACM_NKOrigin = "";
			currentCompanyRule.ACM_NKDestination = "";
			currentCompanyRule.FillWithValidTestData();

			Factory.Save();

			var overallAuRuleA = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == auRuleA.PK);
			var overallAuRuleB = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == auRuleB.PK);
			var overallAuRuleC = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == auRuleC.PK);
			var overallUsRule = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == usRule.PK);
			var overallUsRuleB = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == usRuleB.PK);
			var disabledOverallCnRule = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == cnRule.PK);
			disabledOverallCnRule.Status = GroupCommissionRuleStatusTypes.Codes.Disabled;
			var overallCnRuleB = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == cnRuleB.PK);
			var overallGlobalRuleA = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == globalRuleA.PK);
			var overallGlobalRuleB = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == globalRuleB.PK);
			var overallCurrentCompanyRule = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == currentCompanyRule.PK);

			var itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			var rule = GetOverallStaffCommissionRule(staff, "AUSYD", "AUSYD", itemArgs);
			AssertEquals("Should return the AU rule with matching product, service, and submodule", overallAuRuleB, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, "ROA", "", "");
			rule = GetOverallStaffCommissionRule(staff, "AUSYD", "AUSYD", itemArgs);
			AssertEquals("Should return the AU rule with matching product, service, and submodule", overallAuRuleC, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Brokerage.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			rule = GetOverallStaffCommissionRule(staff, "AUMEL", "AUMEL", itemArgs);
			AssertEquals("Should fallback to the 'ALL' AU rule, since no there is no AU rule with same product and service", overallAuRuleA, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, "AIR", "", "");
			rule = GetOverallStaffCommissionRule(staff, "AUMEL", "AUMEL", itemArgs);
			AssertEquals("Should fallback to the 'SHP' AU rule with ANY Mode, since no there is no AU rule with same product and mode",
				overallAuRuleB,
				rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			rule = GetOverallStaffCommissionRule(staff, "USOKA", "USOKA", itemArgs);
			AssertEquals("Should return the US rule with matching product, service, and submodule", overallUsRule, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "AUSYD", "");
			rule = GetOverallStaffCommissionRule(staff, "USOKA", "USOKA", itemArgs);
			AssertEquals("Should return the US rule with matching product, service, and submodule", overallUsRuleB, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "GBLON", "");
			rule = GetOverallStaffCommissionRule(staff, "USOKA", "USOKA", itemArgs);
			AssertEquals("Should fall back to the generic US rule because origin does not match", overallUsRule, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			rule = GetOverallStaffCommissionRule(staff, "GBLON", "GBLON", itemArgs);
			AssertEquals("Should return the matching global rule since GB specific rule has expired", overallGlobalRuleA, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			rule = GetOverallStaffCommissionRule(staff, "CNBJS", "CNBJS", itemArgs);
			AssertEquals("Should fall back to the global rule as args do not specify destination", overallGlobalRuleA, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "UAIEV");
			rule = GetOverallStaffCommissionRule(staff, "CNBJS", "CNBJS", itemArgs);
			AssertEquals("Should return the matching CN rule destination matches", overallCnRuleB, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "USLAX");
			rule = GetOverallStaffCommissionRule(staff, "CNJBS", "CNJBS", itemArgs);
			AssertEquals("Should return the matching global rule since destination does not match and CN specific rule has been disabled", overallGlobalRuleA, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.QuotedBooking.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			rule = GetOverallStaffCommissionRule(staff, "USOKA", "USOKA", itemArgs);
			AssertEquals("Should return the matching global rule since US specific rules do not match product, service, and submodule", overallGlobalRuleB, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Project.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			rule = GetOverallStaffCommissionRule(staff, "SGSIN", "SGSIN", itemArgs);
			AssertEquals("Company specific rule should take precedence over globalRuleC", overallCurrentCompanyRule, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, "", "", "", ZDate.Empty, "", "", "");
			rule = GetOverallStaffCommissionRule(staff, "AUSYD", "AUSYD", itemArgs);
			AssertNull("Should return null since no product, service, nor submodule provided", rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Brokerage.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			rule = GetOverallStaffCommissionRule(staff, "USOKA", "USOKA", itemArgs);
			AssertNull("Should return null since no matching US rule nor global rule", rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Brokerage.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			rule = GetOverallStaffCommissionRule(staff, "USOKA", "AUMEL", itemArgs);
			AssertEquals("Should fall back to AU rule since no rule exists for US", overallAuRuleA, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			rule = GetOverallStaffCommissionRule(staff, "AUSYD", "USOKA", itemArgs);
			AssertEquals("Primary unloco 'AU' should take precedence over fallback unloco 'US'", overallAuRuleB, rule);

			itemArgs = new CommissionItemArgs(ZGuid.Empty, JobInvoicingConsumerTypes.Shipment.Code, OrgCommissionAgreementItemLookups.AllServicesCode, "", ZDate.Empty, OrgCommissionAgreementItemLookups.AllModesCode, "", "");
			staff.GS_IsActive = false;
			rule = GetOverallStaffCommissionRule(staff, "AUSYD", "USOKA", itemArgs);
			AssertEquals("Shouldn't return anything when staff is inactive", null, rule);
		}

		OverallStaffCommissionRule GetOverallStaffCommissionRule(GlbStaff staff, ZString unloco, ZString fallbackUnloco,
			CommissionItemArgs itemArgs)
		{
			var result =
				staff.GetOverallCommissionRuleResponsibleFor(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, unloco),
					Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, fallbackUnloco), itemArgs);
			return result;
		}

		#endregion

		#region Test Properties

		public void TestEmergencyContactSameAsNextOfKin()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BAS";
			staff.GS_NextOfKin = "Mother Teresa";
			staff.GS_NextOfKinHomePhone = "3355";
			staff.GS_NextOfKinWorkPhone = "9911";
			staff.GS_NextOfKinRelationship = "AUN";

			AssertEquals("", staff.GS_EmergencyContactName);
			AssertEquals("", staff.GS_EmergencyHomePhone_Formatted);
			AssertEquals("", staff.GS_EmergencyWorkPhone_Formatted);
			AssertEquals("", staff.GS_EmergencyContactRelationship);
			AssertEquals(false, staff.GS_EmergencyContactNameInfo.ReadOnly);
			AssertEquals(false, staff.GS_EmergencyHomePhone_FormattedInfo.ReadOnly);
			AssertEquals(false, staff.GS_EmergencyWorkPhone_FormattedInfo.ReadOnly);
			AssertEquals(false, staff.GS_EmergencyContactRelationshipInfo.ReadOnly);

			staff.EmergencySameAsNextOfKin = true;
			AssertEquals("Mother Teresa", staff.GS_EmergencyContactName);
			AssertEquals("3355", staff.GS_EmergencyHomePhone_Formatted);
			AssertEquals("9911", staff.GS_EmergencyWorkPhone_Formatted);
			AssertEquals("AUN", staff.GS_EmergencyContactRelationship);
			AssertEquals(true, staff.GS_EmergencyContactNameInfo.ReadOnly);
			AssertEquals(true, staff.GS_EmergencyHomePhone_FormattedInfo.ReadOnly);
			AssertEquals(true, staff.GS_EmergencyWorkPhone_FormattedInfo.ReadOnly);
			AssertEquals(true, staff.GS_EmergencyContactRelationshipInfo.ReadOnly);

			staff.GS_NextOfKin = "Pervez Musharaf";
			AssertEquals("Pervez Musharaf", staff.GS_EmergencyContactName);

			staff.EmergencySameAsNextOfKin = false;
			AssertEquals("Pervez Musharaf", staff.GS_EmergencyContactName);
			AssertEquals("3355", staff.GS_EmergencyHomePhone_Formatted);
			AssertEquals("9911", staff.GS_EmergencyWorkPhone_Formatted);
			AssertEquals("AUN", staff.GS_EmergencyContactRelationship);
			AssertEquals(false, staff.GS_EmergencyContactNameInfo.ReadOnly);
			AssertEquals(false, staff.GS_EmergencyHomePhone_FormattedInfo.ReadOnly);
			AssertEquals(false, staff.GS_EmergencyWorkPhone_FormattedInfo.ReadOnly);
			AssertEquals(false, staff.GS_EmergencyContactRelationshipInfo.ReadOnly);

			staff.GS_NextOfKin = "Indira Gandhi";
			AssertEquals("Pervez Musharaf", staff.GS_EmergencyContactName);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			GlbStaff reloadedStaff = factory2.Load<GlbStaff>(staff.PK);
			AssertEquals(false, reloadedStaff.EmergencySameAsNextOfKin);
			reloadedStaff.EmergencySameAsNextOfKin = true;
			factory2.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			GlbStaff reloadedStaff2 = factory3.Load<GlbStaff>(staff.PK);
			AssertEquals(true, reloadedStaff2.EmergencySameAsNextOfKin);
			AssertEquals("Indira Gandhi", reloadedStaff2.GS_NextOfKin);
			AssertEquals("Indira Gandhi", reloadedStaff2.GS_EmergencyContactName);
		}

		public void TestIsCurrentUserLocalAdminForThisStaff()
		{
			var localAdminForGroup = Factory.NewWithValidTestData<GlbStaff>();
			var localAdminForStaff = Factory.NewWithValidTestData<GlbStaff>();

			Staff.FillWithValidTestData();
			localAdminForGroup.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(Staff.Groups[0].GG_Code);
			localAdminForStaff.SecurityChangeOthersView.AddSecurityToChangeOtherStaff(Staff.GS_Code);

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(localAdminForGroup.GS_LoginName))
			{
				AssertEquals("localAdminForGroup.IsCurrentUserLocalAdminForThisStaff", true, localAdminForGroup.IsCurrentUserLocalAdminForThisStaff);
				AssertEquals("localAdminForStaff.IsCurrentUserLocalAdminForThisStaff", true, localAdminForStaff.IsCurrentUserLocalAdminForThisStaff);
				AssertEquals("Staff.IsCurrentUserLocalAdminForThisStaff", true, Staff.IsCurrentUserLocalAdminForThisStaff);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(localAdminForStaff.GS_LoginName))
			{
				AssertEquals("localAdminForGroup.IsCurrentUserLocalAdminForThisStaff", false, localAdminForGroup.IsCurrentUserLocalAdminForThisStaff);
				AssertEquals("localAdminForStaff.IsCurrentUserLocalAdminForThisStaff", false, localAdminForStaff.IsCurrentUserLocalAdminForThisStaff);
				AssertEquals("Staff.IsCurrentUserLocalAdminForThisStaff", true, Staff.IsCurrentUserLocalAdminForThisStaff);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(Staff.GS_LoginName))
			{
				AssertEquals("localAdminForGroup.IsCurrentUserLocalAdminForThisStaff", false, localAdminForGroup.IsCurrentUserLocalAdminForThisStaff);
				AssertEquals("localAdminForStaff.IsCurrentUserLocalAdminForThisStaff", false, localAdminForStaff.IsCurrentUserLocalAdminForThisStaff);
				AssertEquals("Staff.IsCurrentUserLocalAdminForThisStaff", false, Staff.IsCurrentUserLocalAdminForThisStaff);
			}
		}

		public void TestIsCurrentUserLocalAdminForAtLeastOneGroup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var localAdminForGroup = Factory.NewWithValidTestData<GlbStaff>();
			var localAdminForStaff = Factory.NewWithValidTestData<GlbStaff>();

			staff.FillWithValidTestData();
			localAdminForGroup.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(staff.Groups[0].GG_Code);
			localAdminForStaff.SecurityChangeOthersView.AddSecurityToChangeOtherStaff(staff.GS_Code);

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(localAdminForGroup.GS_LoginName))
			{
				AssertEquals("localAdminForGroup.IsCurrentUserLocalAdminForAtLeastOneGroup", true, localAdminForGroup.IsCurrentUserLocalAdminForAtLeastOneGroup);
				AssertEquals("localAdminForStaff.IsCurrentUserLocalAdminForAtLeastOneGroup", true, localAdminForStaff.IsCurrentUserLocalAdminForAtLeastOneGroup);
				AssertEquals("Staff.IsCurrentUserLocalAdminForAtLeastOneGroup", true, staff.IsCurrentUserLocalAdminForAtLeastOneGroup);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(localAdminForStaff.GS_LoginName))
			{
				AssertEquals("localAdminForGroup.IsCurrentUserLocalAdminForAtLeastOneGroup", false, localAdminForGroup.IsCurrentUserLocalAdminForAtLeastOneGroup);
				AssertEquals("localAdminForStaff.IsCurrentUserLocalAdminForAtLeastOneGroup", false, localAdminForStaff.IsCurrentUserLocalAdminForAtLeastOneGroup);
				AssertEquals("Staff.IsCurrentUserLocalAdminForAtLeastOneGroup", false, staff.IsCurrentUserLocalAdminForAtLeastOneGroup);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				AssertEquals("localAdminForGroup.IsCurrentUserLocalAdminForAtLeastOneGroup", false, localAdminForGroup.IsCurrentUserLocalAdminForAtLeastOneGroup);
				AssertEquals("localAdminForStaff.IsCurrentUserLocalAdminForAtLeastOneGroup", false, localAdminForStaff.IsCurrentUserLocalAdminForAtLeastOneGroup);
				AssertEquals("Staff.IsCurrentUserLocalAdminForAtLeastOneGroup", false, staff.IsCurrentUserLocalAdminForAtLeastOneGroup);
			}
		}

		public void TestNeedsLocalAdminGroupToBeAdded()
		{
			var localAdminForGroup = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			localAdminForGroup.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(staff.Groups[0].GG_Code);

			Assert("NeedsLocalAdminGroupToBeAdded should be false for a regular user", !staff.NeedsLocalAdminGroupToBeAdded);

			using (Env.SetTemporaryUserContext(localAdminForGroup.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.StaffModifyAll.IsAllowed = true;
				Assert("NeedsLocalAdminGroupToBeAdded should be FALSE for non-saved staff when logged in as local admin with rights to modify all", !staff.NeedsLocalAdminGroupToBeAdded);

				Env.Security.StaffModifyAll.IsAllowed = false;
				Assert("NeedsLocalAdminGroupToBeAdded should be TRUE for non-saved staff when logged in as local admin for this group only", staff.NeedsLocalAdminGroupToBeAdded);

				Factory.Save();
				Assert("NeedsLocalAdminGroupToBeAdded should be FALSE for Saved staff when logged in as local admin for this group only", !staff.NeedsLocalAdminGroupToBeAdded);
			}
		}

		public void TestResourceLoginName()
		{
			GlbStaff resource = Factory.New<GlbStaff>();
			AssertEquals(ZString.Empty, resource.GS_LoginName);

			resource.GS_IsResource = true;
			AssertEquals(resource.PK.ToString(), ((INeedRow)resource).Row["GS_LoginName"].ToString());
			AssertEquals(ZString.Empty, resource.GS_LoginName);

			resource.GS_IsResource = false;
			AssertEquals(ZString.Empty, resource.GS_LoginName);
			AssertEquals(ZString.Empty, ((INeedRow)resource).Row["GS_LoginName"].ToString());
			resource.GS_LoginName = "hello";
			AssertEquals("hello", resource.GS_LoginName);
			AssertEquals("hello", ((INeedRow)resource).Row["GS_LoginName"].ToString());
		}

		public void TestCanLoginUserSetsCorrectly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "WTG_Support";
			staff.GS_CanLogin = true;
			staff.StaffPlainTextPassword = "TEST";

			Factory.Save();
			AssertEquals("Saved login information is incorrect:", "WTG_Support", staff.GS_LoginName);
			AssertEquals("Saved password is incorrect:", true, staff.VerifyPassword("TEST"));
			AssertEquals("User marked as CanLogin incorrectly:", true, staff.GS_CanLogin);

			staff.GS_CanLogin = false;
			Factory.Save();

			AssertEquals("User marked as CanLogin incorrectly:", false, staff.GS_CanLogin);
		}

		public void TestCurrentUser()
		{
			AssertEquals("CurrentUser", GlbStaff.CurrentUser.PK, GlbStaff.CurrentUser.PK);
		}

		public void TestCurrentUserNullWhenEnvIsNull()
		{
			var currentProvider = Env.GetCurrentProvider();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Staff should be the current user", staff.PK, GlbStaff.CurrentUser.PK);
			}

			using (var provider = new NullEnvProvider())
			{
				provider.Enable();

				try
				{
					AssertNull("GlbStaff.CurrentUser should be null when Env.CurrentUser is null", GlbStaff.CurrentUser);
				}
				finally
				{
					currentProvider.Enable();
				}
			}
		}

		public void TestIsCurrentUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			Assert("Staff should not be the current user", !staff.IsCurrentUser);

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert("Staff should be the current user", staff.IsCurrentUser);
			}
		}

		public void TestStaffPasswordEncryption()
		{
			Staff.StaffPlainTextPassword = "TEST";
			Staff.StaffConfirmPassword = "TEST";

			AssertEquals("Precondition: Password Hash not set", ZBlob.Empty, Staff.GS_PasswordHash);
			AssertEquals("Precondition: Password Iteration not set", 0, Staff.GS_PasswordHashIterations);
			AssertEquals("Precondition: Password Salt not set", ZBlob.Empty, Staff.GS_PasswordSalt);

			Factory.Save();
			AssertNotEquals("Password Hash should be set after Factory.Save()", ZBlob.Empty, Staff.GS_PasswordHash);
			AssertNotEquals("Password Iteration should be set after Factory.Save()", 0, Staff.GS_PasswordHashIterations);
			AssertNotEquals("Password Salt should be set after Factory.Save()", ZBlob.Empty, Staff.GS_PasswordSalt);
			AssertEquals("New password should be verified", true, Staff.VerifyPassword("TEST"));
			AssertEquals("Wrong password should not be verified", false, Staff.VerifyPassword("wrong"));
		}

		public void TestNationalIdentityNumber()
		{
			AssertEquals("NationalIdentityNumber", "", Staff.NationalIdentityNumber);

			GenRegCertAccredMaintList certificate = Staff.Certificates.AddNew();
			certificate.XZ_Type = Core.Constants.StaffCertificateType.IATA;
			certificate.XZ_RefNumber = "TestIATA";
			AssertEquals("NationalIdentityNumber", "", Staff.NationalIdentityNumber);

			certificate = Staff.Certificates.AddNew();
			certificate.XZ_Type = Core.Constants.StaffCertificateType.NID;
			certificate.XZ_RefNumber = "LicenseNumber";
			AssertEquals("NationalIdentityNumber", "LicenseNumber", Staff.NationalIdentityNumber);
		}

		public void TestNationalityType()
		{
			var sing = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN");
			using (Env.SetTemporaryUserContext(User.SupportUserName, sing.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("The country of the current company should be SG", "SG", Env.CurrentCompany.Country.Code);

				Staff.GS_RN_NKNationalityCode = "??";
				Assert("Nationality Code not in list, has errors", Staff.GS_RN_NKNationalityCodeInfo.HasErrors());

				Staff.GS_RN_NKNationalityCode = Core.Constants.CountryCodes.UnitedStates;
				Assert("Nationality Code in list, does not have errors", !Staff.GS_RN_NKNationalityCodeInfo.HasErrors());
			}
		}

		public void TestSecurityBranch()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			Staff.HasChanges = false;
			Staff.SecurityBranch = branch.PK;
			AssertEquals("SecurityBranch", branch.PK, Staff.SecurityBranch);
			AssertEquals("HasChanges", false, Staff.HasChanges);
		}

		public void TestSecurityDepartment()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			Staff.HasChanges = false;
			Staff.SecurityDepartment = department.PK;
			AssertEquals("SecurityDepartment", department.PK, Staff.SecurityDepartment);
			AssertEquals("HasChanges", false, Staff.HasChanges);
		}

		public void TestHumanReadableNameCore()
		{
			Staff.GS_Code = "JNG";
			Staff.GS_IsResource = false;
			AssertEquals("HumanReadableNameCore is human readable", "Staff (JNG)", Staff.HumanReadableName);
			Staff.GS_Code = "$EE";
			Staff.GS_IsResource = true;
			AssertEquals("HumanReadableNameCore is human readable", "Resource ($EE)", Staff.HumanReadableName);
		}

		public void TestGSCodeReadOnlyWhenIsInDatabase()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			AssertEquals("Should not be in database", false, staff.IsInDatabase);
			AssertEquals("GS_Code should not be readonly", false, staff.GS_CodeInfo.ReadOnly);

			Factory.Save();

			AssertEquals("Should be in database", true, staff.IsInDatabase);
			AssertEquals("GS_Code should be readonly", true, staff.GS_CodeInfo.ReadOnly);
		}

		#region GS_PreferredPaymentCompany

		public void TestUseTransactionCompanyAsPreferredPayment_SettingToTrueClearsGS_GC_PreferredPaymentCompany()
		{
			var staff = Factory.New<GlbStaff>();

			staff.UseTransactionCompanyAsPreferredPayment = false;
			staff.GS_GC_PreferredPaymentCompany = GlbCompany.CurrentCompany.PK;

			staff.UseTransactionCompanyAsPreferredPayment = true;
			AssertEquals(ZGuid.Empty, staff.GS_GC_PreferredPaymentCompany);
		}

		public void TestIsGlobal_OnLoaded()
		{
			var staffWithoutPreferredPaymentCompany = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutPreferredPaymentCompany.GS_GC_PreferredPaymentCompany = ZGuid.Empty;
			var staffWithPreferredPaymentCompany = Factory.NewWithValidTestData<GlbStaff>();
			staffWithPreferredPaymentCompany.GS_GC_PreferredPaymentCompany = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			AssertEquals(true, otherFactory.Load<GlbStaff>(staffWithoutPreferredPaymentCompany.PK).UseTransactionCompanyAsPreferredPayment);
			AssertEquals(false, otherFactory.Load<GlbStaff>(staffWithPreferredPaymentCompany.PK).UseTransactionCompanyAsPreferredPayment);
		}

		public void TestGS_GC_PreferredPaymentCompany_ReadOnly()
		{
			var staff = Factory.New<GlbStaff>();

			staff.UseTransactionCompanyAsPreferredPayment = true;
			AssertEquals(true, staff.GS_GC_PreferredPaymentCompanyInfo.ReadOnly);

			staff.UseTransactionCompanyAsPreferredPayment = false;
			AssertEquals(false, staff.GS_GC_PreferredPaymentCompanyInfo.ReadOnly);
		}

		#endregion

		#region Test Properties with Security Access rights

		public void TestErrorReportWhenViewDeniedIsSet_City1()
		{
			Staff.GS_City = Staff.ViewDeniedMessage;
			AssertEquals("Silent exception must be sent. LastKeyReported: ", "ViewDeniedValueSet", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestErrorReportWhenViewDeniedIsSet_City2()
		{
			Staff.GS_City = Staff.ViewDeniedMessage.SubstringSafe(0, 15);
			AssertEquals("Silent exception must be sent. LastKeyReported: ", "ViewDeniedValueSet", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestErrorReportWhenViewDeniedIsSet_State()
		{
			Staff.GS_State = Staff.ViewDeniedMessage;
			AssertEquals("Silent exception must be sent. LastKeyReported: ", "ViewDeniedValueSet", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestErrorReportWhenViewDeniedIsSet_Postcode1()
		{
			Staff.GS_Postcode = Staff.ViewDeniedMessage;
			AssertEquals("Silent exception must be sent. LastKeyReported: ", "ViewDeniedValueSet", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestErrorReportWhenViewDeniedIsSet_Postcode2()
		{
			Staff.GS_Postcode = Staff.ViewDeniedMessage.SubstringSafe(0, 10);
			AssertEquals("Silent exception must be sent. LastKeyReported: ", "ViewDeniedValueSet", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestErrorReportWhenViewDeniedIsSet_Address()
		{
			Staff.GS_UserAddress1 = Staff.ViewDeniedMessage;
			AssertEquals("Silent exception must be sent. LastKeyReported: ", "ViewDeniedValueSet", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestEnforceChangePasswordAtNextLogin()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();

			using (Env.Registry.RawRegistry.EnforceChangePasswordAtNextLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!user.IsInDatabase);
				Assert("When registry is true, ChangePasswordAtNextLogin should be readOnly when user is created", user.ChangePasswordAtNextLoginInfo.ReadOnly);

				Factory.Save();
				Assert(user.IsInDatabase);
				Assert(user.ChangePasswordAtNextLogin);
				Assert("When registry is true, ChangePasswordAtNextLogin should be readOnly when user is saved and password has not been updated yet", user.ChangePasswordAtNextLoginInfo.ReadOnly);

				user.ChangePasswordAtNextLogin = false; //When user update his password, ChangePasswordAtNextLogin is automatically change to false
				Factory.Save();
				Assert(!user.ChangePasswordAtNextLogin);
				Assert("When registry is true, ChangePasswordAtNextLogin should not be readOnly when user has updated his password", !user.ChangePasswordAtNextLoginInfo.ReadOnly);

				user.ChangePasswordAtNextLogin = true;
				Assert(user.ChangePasswordAtNextLogin);
				Assert(user.GS_ChangePasswordAtNextLoginInfo.HasChanges);
				Assert("When registry is true, ChangePasswordAtNextLogin should not be readOnly when user has change its value but not save yet", !user.ChangePasswordAtNextLoginInfo.ReadOnly);
			}

			using (Env.Registry.RawRegistry.EnforceChangePasswordAtNextLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("When registry is false, ChangePasswordAtNextLogin should not be readOnly", !user.ChangePasswordAtNextLoginInfo.ReadOnly);

				Factory.Save();
				Assert("When registry is false, ChangePasswordAtNextLogin should not be readOnly", !user.ChangePasswordAtNextLoginInfo.ReadOnly);

				user.ChangePasswordAtNextLogin = false;
				Factory.Save();
				Assert("When registry is false, ChangePasswordAtNextLogin should not be readOnly", !user.ChangePasswordAtNextLoginInfo.ReadOnly);

				user.ChangePasswordAtNextLogin = true;
				Assert("When registry is false, ChangePasswordAtNextLogin should not be readOnly", !user.ChangePasswordAtNextLoginInfo.ReadOnly);
			}
		}

		public void TestSecurityForPasswordAndSignatureControls()
		{
			var loginUser = Factory.NewWithValidTestData<GlbStaff>();
			var staffToTest = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(loginUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.StaffEdit.IsAllowed = true;
				Env.Security.StaffModifyAll.IsAllowed = false;
				AssertEquals("ChangePasswordAtNextLogin should be read only", true, staffToTest.ChangePasswordAtNextLoginInfo.ReadOnly);
				AssertEquals("PasswordNeverChanges should be read only", true, staffToTest.PasswordNeverChangesInfo.ReadOnly);

				Env.Security.StaffPasswordAndSignature.IsAllowed = true;
				AssertEquals("ChangePasswordAtNextLogin should not be read only", false, staffToTest.ChangePasswordAtNextLoginInfo.ReadOnly);
				AssertEquals("PasswordNeverChanges should not be read only", false, staffToTest.PasswordNeverChangesInfo.ReadOnly);
			}
		}

		public void TestNewStaffAllowedViewAccessDenied()
		{
			var staffWithNewStaffAllowedViewAccessDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffWithNewStaffAllowedViewAccessDenied.GS_Code = "BAS";
			staffWithNewStaffAllowedViewAccessDenied.GS_IsController = false;

			var viewDeniedSecurityRecord = Factory.New<GlbSecurity>();
			viewDeniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherStaffDetails.Code;
			viewDeniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			viewDeniedSecurityRecord.GU_GS = staffWithNewStaffAllowedViewAccessDenied.PK;
			staffWithNewStaffAllowedViewAccessDenied.GroupSecurityPermissionsCollectionForBinding.Add(viewDeniedSecurityRecord);

			var newAllowedSecurityRecord = Factory.New<GlbSecurity>();
			newAllowedSecurityRecord.GU_SecurityRight = Env.Security.StaffModifyAll.Code;
			newAllowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			newAllowedSecurityRecord.GU_GS = staffWithNewStaffAllowedViewAccessDenied.PK;
			staffWithNewStaffAllowedViewAccessDenied.GroupSecurityPermissionsCollectionForBinding.Add(newAllowedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(staffWithNewStaffAllowedViewAccessDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Precondition: Current user has changed", staffWithNewStaffAllowedViewAccessDenied.PK, Env.CurrentUser.PK);

				GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
				Assert("New Staff - Address details should not be readonly even though View access is denied", !newStaff.GS_UserAddress1Info.ReadOnly);
				Factory.Save();

				Assert("Staff member has been saved - Address details should be readonly", newStaff.GS_UserAddress1Info.ReadOnly);
			}
		}

		public void TestViewBankingDetailsProperties()
		{
			var viewDeniedMessage = "** View Denied due to Security Access **";
			var staffWithoutBankPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutBankPermissions.GS_IsController = false;
			staffWithoutBankPermissions.GS_WagesBankAccount = "1";
			staffWithoutBankPermissions.GS_WagesBankBsb = "2";
			staffWithoutBankPermissions.GS_WagesBankName = "3";
			staffWithoutBankPermissions.GS_WagesBankSwift = "4";
			staffWithoutBankPermissions.GS_EftWages = true;

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewBankingDetails.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutBankPermissions.PK;
			staffWithoutBankPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithBankPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithBankPermissions.GS_IsController = false;
			staffWithBankPermissions.GS_WagesBankAccount = "5";
			staffWithBankPermissions.GS_WagesBankBsb = "6";
			staffWithBankPermissions.GS_WagesBankName = "7";
			staffWithBankPermissions.GS_WagesBankSwift = "8";
			staffWithBankPermissions.GS_EftWages = true;

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewBankingDetails.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithBankPermissions.PK;
			staffWithBankPermissions.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutBankPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithoutBankPermissions.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();

					var staffWithBankPermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithBankPermissions.PK));
					var staffWithoutBankPermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutBankPermissions.PK));

					AssertEquals(viewDeniedMessage, staffWithBankPermissionsLoaded1.GS_WagesBankAccount);
					AssertEquals(viewDeniedMessage, staffWithBankPermissionsLoaded1.GS_WagesBankBsb);
					AssertEquals(viewDeniedMessage, staffWithBankPermissionsLoaded1.GS_WagesBankName);
					AssertEquals(viewDeniedMessage, staffWithBankPermissionsLoaded1.GS_WagesBankSwift);
					AssertEquals(false, staffWithBankPermissionsLoaded1.GS_EftWages);
					Assert(staffWithBankPermissionsLoaded1.GS_WagesBankAccountInfo.ReadOnly);
					Assert(staffWithBankPermissionsLoaded1.GS_WagesBankBsbInfo.ReadOnly);
					Assert(staffWithBankPermissionsLoaded1.GS_WagesBankNameInfo.ReadOnly);
					Assert(staffWithBankPermissionsLoaded1.GS_WagesBankSwiftInfo.ReadOnly);
					Assert(staffWithBankPermissionsLoaded1.GS_EftWagesInfo.ReadOnly);

					AssertEquals("1", staffWithoutBankPermissionsLoaded1.GS_WagesBankAccount);
					AssertEquals("2", staffWithoutBankPermissionsLoaded1.GS_WagesBankBsb);
					AssertEquals("3", staffWithoutBankPermissionsLoaded1.GS_WagesBankName);
					AssertEquals("4", staffWithoutBankPermissionsLoaded1.GS_WagesBankSwift);
					AssertEquals(true, staffWithoutBankPermissionsLoaded1.GS_EftWages);
					Assert(!staffWithoutBankPermissionsLoaded1.GS_WagesBankAccountInfo.ReadOnly);
					Assert(!staffWithoutBankPermissionsLoaded1.GS_WagesBankBsbInfo.ReadOnly);
					Assert(!staffWithoutBankPermissionsLoaded1.GS_WagesBankNameInfo.ReadOnly);
					Assert(!staffWithoutBankPermissionsLoaded1.GS_WagesBankSwiftInfo.ReadOnly);
					Assert(!staffWithoutBankPermissionsLoaded1.GS_EftWagesInfo.ReadOnly);
				}
				using (Env.SetTemporaryUserContext(new UserContext(staffWithBankPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithBankPermissions.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory();
					var staffWithoutBankPermissionsLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutBankPermissions.PK));
					//reload staff to avoid caching readonly settings

					AssertEquals("1", staffWithoutBankPermissionsLoaded2.GS_WagesBankAccount);
					AssertEquals("2", staffWithoutBankPermissionsLoaded2.GS_WagesBankBsb);
					AssertEquals("3", staffWithoutBankPermissionsLoaded2.GS_WagesBankName);
					AssertEquals("4", staffWithoutBankPermissionsLoaded2.GS_WagesBankSwift);
					AssertEquals(true, staffWithoutBankPermissionsLoaded2.GS_EftWages);
					Assert(!staffWithoutBankPermissionsLoaded2.GS_WagesBankAccountInfo.ReadOnly);
					Assert(!staffWithoutBankPermissionsLoaded2.GS_WagesBankBsbInfo.ReadOnly);
					Assert(!staffWithoutBankPermissionsLoaded2.GS_WagesBankNameInfo.ReadOnly);
					Assert(!staffWithoutBankPermissionsLoaded2.GS_WagesBankSwiftInfo.ReadOnly);
					Assert(!staffWithoutBankPermissionsLoaded2.GS_EftWagesInfo.ReadOnly);

					var staffWithBankPermissionsLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithBankPermissions.PK));
					//reload staff to avoid caching readonly settings

					AssertEquals("5", staffWithBankPermissionsLoaded2.GS_WagesBankAccount);
					AssertEquals("6", staffWithBankPermissionsLoaded2.GS_WagesBankBsb);
					AssertEquals("7", staffWithBankPermissionsLoaded2.GS_WagesBankName);
					AssertEquals("8", staffWithBankPermissionsLoaded2.GS_WagesBankSwift);
					AssertEquals(true, staffWithBankPermissionsLoaded2.GS_EftWages);

					Assert(!staffWithBankPermissionsLoaded2.GS_WagesBankAccountInfo.ReadOnly);
					Assert(!staffWithBankPermissionsLoaded2.GS_WagesBankBsbInfo.ReadOnly);
					Assert(!staffWithBankPermissionsLoaded2.GS_WagesBankNameInfo.ReadOnly);
					Assert(!staffWithBankPermissionsLoaded2.GS_WagesBankSwiftInfo.ReadOnly);
					Assert(!staffWithBankPermissionsLoaded2.GS_EftWagesInfo.ReadOnly);
				}
			}
		}

		public void TestViewNationality()
		{
			var viewDeniedMessage = "** View Denied due to Security Access **";

			var staffWithoutViewNationalityPermission = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutViewNationalityPermission.GS_RN_NKNationalityCode = "NO";

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherNationality.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutViewNationalityPermission.PK;
			staffWithoutViewNationalityPermission.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithViewNationalityPermission = Factory.NewWithValidTestData<GlbStaff>();
			staffWithViewNationalityPermission.GS_RN_NKNationalityCode = "YE";

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherNationality.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithViewNationalityPermission.PK;
			staffWithViewNationalityPermission.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutViewNationalityPermission.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var newFactory = new BusinessObjectFactory();
					var staffWithViewNationalityPermissionNewFactory = newFactory.Load<GlbStaff>(staffWithViewNationalityPermission.PK);
					var staffWithoutViewNationalityPermissionNewFactory = newFactory.Load<GlbStaff>(staffWithoutViewNationalityPermission.PK);

					AssertEquals(viewDeniedMessage, staffWithViewNationalityPermissionNewFactory.GS_RN_NKNationalityCode);
					AssertEquals(true, staffWithViewNationalityPermissionNewFactory.GS_RN_NKNationalityCodeInfo.ReadOnly);

					AssertEquals("NO", staffWithoutViewNationalityPermissionNewFactory.GS_RN_NKNationalityCode);
					AssertEquals(false, staffWithoutViewNationalityPermissionNewFactory.GS_RN_NKNationalityCodeInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithViewNationalityPermission.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var newFactory = new BusinessObjectFactory();
					var staffWithViewNationalityPermissionNewFactory = newFactory.Load<GlbStaff>(staffWithViewNationalityPermission.PK);
					var staffWithoutViewNationalityPermissionNewFactory = newFactory.Load<GlbStaff>(staffWithoutViewNationalityPermission.PK);

					AssertEquals("YE", staffWithViewNationalityPermissionNewFactory.GS_RN_NKNationalityCode);
					AssertEquals(false, staffWithViewNationalityPermissionNewFactory.GS_RN_NKNationalityCodeInfo.ReadOnly);
					AssertEquals("NO", staffWithoutViewNationalityPermissionNewFactory.GS_RN_NKNationalityCode);
					AssertEquals(false, staffWithoutViewNationalityPermissionNewFactory.GS_RN_NKNationalityCodeInfo.ReadOnly);
				}
			}
		}

		public void TestLoadingStaffWhenNoViewPermissionsDoesNotCorruptStaffDetails()
		{
			var staffWithoutEmergencyContactPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutEmergencyContactPermissions.GS_IsController = false;
			staffWithoutEmergencyContactPermissions.GS_Code = "XXX";
			staffWithoutEmergencyContactPermissions.GS_LoginName = "LoginName";

			Factory.RefreshEnabled = false;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BLH";
			staff.GS_LoginName = "Blahdy";
			staff.GS_NextOfKin = "Next Of Kin";
			staff.GS_EmergencyContactName = "Emergency Contact";
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(staffWithoutEmergencyContactPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var staff2 = factory2.Load<GlbStaff>(staff.PK);
				staff2.GS_CommissionBasis = "X";    // to force saving the object
				factory2.Save();
			}

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;
			var staff3 = factory3.Load<GlbStaff>(staff.PK);
			AssertEquals("Next Of Kin", staff3.GS_NextOfKin);
			AssertEquals("Emergency Contact", staff3.GS_EmergencyContactName);
		}

		public void TestViewEmergencyContactProperties()
		{
			ZString viewDeniedMessage = "** View Denied due to Security Access **";
			GlbStaff staffWithoutEmergencyContactPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutEmergencyContactPermissions.GS_Code = "6";
			staffWithoutEmergencyContactPermissions.GS_IsController = false;
			staffWithoutEmergencyContactPermissions.GS_EmergencyContactName = "1";
			staffWithoutEmergencyContactPermissions.GS_EmergencyHomePhone = "2";
			staffWithoutEmergencyContactPermissions.GS_EmergencyWorkPhone = "3";
			staffWithoutEmergencyContactPermissions.GS_NextOfKin = "4";
			staffWithoutEmergencyContactPermissions.GS_NextOfKinHomePhone = "5";
			staffWithoutEmergencyContactPermissions.GS_NextOfKinWorkPhone = "6";

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewEmergencyContact.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutEmergencyContactPermissions.PK;
			staffWithoutEmergencyContactPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithEmergencyContactPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithEmergencyContactPermissions.GS_Code = "7";
			staffWithEmergencyContactPermissions.GS_IsController = false;
			staffWithEmergencyContactPermissions.GS_EmergencyContactName = "7";
			staffWithEmergencyContactPermissions.GS_EmergencyHomePhone = "8";
			staffWithEmergencyContactPermissions.GS_EmergencyWorkPhone = "9";
			staffWithEmergencyContactPermissions.GS_NextOfKin = "10";
			staffWithEmergencyContactPermissions.GS_NextOfKinHomePhone = "11";
			staffWithEmergencyContactPermissions.GS_NextOfKinWorkPhone = "12";

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewEmergencyContact.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithEmergencyContactPermissions.PK;
			staffWithEmergencyContactPermissions.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutEmergencyContactPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithoutEmergencyContactPermissions.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					GlbStaff staffWithEmergencyContactPermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithEmergencyContactPermissions.PK));
					GlbStaff staffWithoutEmergencyContactPermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutEmergencyContactPermissions.PK));

					AssertEquals(viewDeniedMessage, staffWithEmergencyContactPermissionsLoaded1.GS_EmergencyContactName);
					AssertEquals(viewDeniedMessage, staffWithEmergencyContactPermissionsLoaded1.GS_EmergencyHomePhone_Formatted);
					AssertEquals(viewDeniedMessage, staffWithEmergencyContactPermissionsLoaded1.GS_EmergencyWorkPhone_Formatted);
					AssertEquals(viewDeniedMessage, staffWithEmergencyContactPermissionsLoaded1.GS_NextOfKin);
					AssertEquals(viewDeniedMessage, staffWithEmergencyContactPermissionsLoaded1.GS_NextOfKinHomePhone_Formatted);
					AssertEquals(viewDeniedMessage, staffWithEmergencyContactPermissionsLoaded1.GS_NextOfKinWorkPhone_Formatted);

					Assert(staffWithEmergencyContactPermissionsLoaded1.GS_EmergencyContactNameInfo.ReadOnly);
					Assert(staffWithEmergencyContactPermissionsLoaded1.GS_EmergencyHomePhone_FormattedInfo.ReadOnly);
					Assert(staffWithEmergencyContactPermissionsLoaded1.GS_EmergencyWorkPhone_FormattedInfo.ReadOnly);
					Assert(staffWithEmergencyContactPermissionsLoaded1.GS_NextOfKinInfo.ReadOnly);
					Assert(staffWithEmergencyContactPermissionsLoaded1.GS_NextOfKinHomePhone_FormattedInfo.ReadOnly);
					Assert(staffWithEmergencyContactPermissionsLoaded1.GS_NextOfKinWorkPhone_FormattedInfo.ReadOnly);

					AssertEquals("1", staffWithoutEmergencyContactPermissionsLoaded1.GS_EmergencyContactName);
					AssertEquals("2", staffWithoutEmergencyContactPermissionsLoaded1.GS_EmergencyHomePhone_Formatted);
					AssertEquals("3", staffWithoutEmergencyContactPermissionsLoaded1.GS_EmergencyWorkPhone_Formatted);
					AssertEquals("4", staffWithoutEmergencyContactPermissionsLoaded1.GS_NextOfKin);
					AssertEquals("5", staffWithoutEmergencyContactPermissionsLoaded1.GS_NextOfKinHomePhone_Formatted);
					AssertEquals("6", staffWithoutEmergencyContactPermissionsLoaded1.GS_NextOfKinWorkPhone_Formatted);

					Assert(!staffWithoutEmergencyContactPermissionsLoaded1.GS_EmergencyContactNameInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded1.GS_EmergencyHomePhone_FormattedInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded1.GS_EmergencyWorkPhone_FormattedInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded1.GS_NextOfKinInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded1.GS_NextOfKinHomePhone_FormattedInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded1.GS_NextOfKinWorkPhone_FormattedInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithEmergencyContactPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithEmergencyContactPermissions.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory();
					var staffWithoutEmergencyContactPermissionsLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutEmergencyContactPermissions.PK));
					//reload staff to avoid caching readonly settings

					AssertEquals("1", staffWithoutEmergencyContactPermissionsLoaded2.GS_EmergencyContactName);
					AssertEquals("2", staffWithoutEmergencyContactPermissionsLoaded2.GS_EmergencyHomePhone_Formatted);
					AssertEquals("3", staffWithoutEmergencyContactPermissionsLoaded2.GS_EmergencyWorkPhone_Formatted);
					AssertEquals("4", staffWithoutEmergencyContactPermissionsLoaded2.GS_NextOfKin);
					AssertEquals("5", staffWithoutEmergencyContactPermissionsLoaded2.GS_NextOfKinHomePhone_Formatted);
					AssertEquals("6", staffWithoutEmergencyContactPermissionsLoaded2.GS_NextOfKinWorkPhone_Formatted);

					Assert(!staffWithoutEmergencyContactPermissionsLoaded2.GS_EmergencyContactNameInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded2.GS_EmergencyHomePhone_FormattedInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded2.GS_EmergencyWorkPhone_FormattedInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded2.GS_NextOfKinInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded2.GS_NextOfKinHomePhone_FormattedInfo.ReadOnly);
					Assert(!staffWithoutEmergencyContactPermissionsLoaded2.GS_NextOfKinWorkPhone_FormattedInfo.ReadOnly);

					var staffWithEmergencyContactPermissionsLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithEmergencyContactPermissions.PK));
					//reload staff to avoid caching readonly settings

					AssertEquals("7", staffWithEmergencyContactPermissionsLoaded2.GS_EmergencyContactName);
					AssertEquals("8", staffWithEmergencyContactPermissionsLoaded2.GS_EmergencyHomePhone_Formatted);
					AssertEquals("9", staffWithEmergencyContactPermissionsLoaded2.GS_EmergencyWorkPhone_Formatted);
					AssertEquals("10", staffWithEmergencyContactPermissionsLoaded2.GS_NextOfKin);
					AssertEquals("11", staffWithEmergencyContactPermissionsLoaded2.GS_NextOfKinHomePhone_Formatted);
					AssertEquals("12", staffWithEmergencyContactPermissionsLoaded2.GS_NextOfKinWorkPhone_Formatted);

					Assert(!staffWithEmergencyContactPermissionsLoaded2.GS_EmergencyContactNameInfo.ReadOnly);
					Assert(!staffWithEmergencyContactPermissionsLoaded2.GS_EmergencyHomePhone_FormattedInfo.ReadOnly);
					Assert(!staffWithEmergencyContactPermissionsLoaded2.GS_EmergencyWorkPhone_FormattedInfo.ReadOnly);
					Assert(!staffWithEmergencyContactPermissionsLoaded2.GS_NextOfKinInfo.ReadOnly);
					Assert(!staffWithEmergencyContactPermissionsLoaded2.GS_NextOfKinHomePhone_FormattedInfo.ReadOnly);
					Assert(!staffWithEmergencyContactPermissionsLoaded2.GS_EmergencyWorkPhone_FormattedInfo.ReadOnly);
				}
			}
		}

		public void TestViewHomeAddress()
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

					Assert(staffWithHomeAddressPermissionsLoaded1.GS_UserAddress1Info.ReadOnly);
					Assert(staffWithHomeAddressPermissionsLoaded1.GS_UserAddress2Info.ReadOnly);
					Assert(staffWithHomeAddressPermissionsLoaded1.GS_CityInfo.ReadOnly);
					Assert(staffWithHomeAddressPermissionsLoaded1.GS_StateInfo.ReadOnly);
					Assert(staffWithHomeAddressPermissionsLoaded1.GS_PostcodeInfo.ReadOnly);
					Assert(staffWithHomeAddressPermissionsLoaded1.GS_HomePhone_FormattedInfo.ReadOnly);

					AssertEquals("1", staffWithoutHomeAddressPermissionsLoaded1.GS_UserAddress1);
					AssertEquals("2", staffWithoutHomeAddressPermissionsLoaded1.GS_UserAddress2);
					AssertEquals("3", staffWithoutHomeAddressPermissionsLoaded1.GS_City);
					AssertEquals("4", staffWithoutHomeAddressPermissionsLoaded1.GS_State);
					AssertEquals("5", staffWithoutHomeAddressPermissionsLoaded1.GS_Postcode);
					AssertEquals("1234567", staffWithoutHomeAddressPermissionsLoaded1.GS_HomePhone_Formatted);

					Assert(!staffWithoutHomeAddressPermissionsLoaded1.GS_UserAddress1Info.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded1.GS_UserAddress2Info.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded1.GS_CityInfo.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded1.GS_StateInfo.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded1.GS_PostcodeInfo.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded1.GS_HomePhone_FormattedInfo.ReadOnly);
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

					Assert(!staffWithoutHomeAddressPermissionsLoaded2.GS_UserAddress1Info.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded2.GS_UserAddress2Info.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded2.GS_CityInfo.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded2.GS_StateInfo.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded2.GS_PostcodeInfo.ReadOnly);
					Assert(!staffWithoutHomeAddressPermissionsLoaded2.GS_HomePhone_FormattedInfo.ReadOnly);

					var staffWithHomeAddressPermissionsLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithHomeAddressPermissions.PK));

					AssertEquals("6", staffWithHomeAddressPermissionsLoaded2.GS_UserAddress1);
					AssertEquals("7", staffWithHomeAddressPermissionsLoaded2.GS_UserAddress2);
					AssertEquals("8", staffWithHomeAddressPermissionsLoaded2.GS_City);
					AssertEquals("9", staffWithHomeAddressPermissionsLoaded2.GS_State);
					AssertEquals("10", staffWithHomeAddressPermissionsLoaded2.GS_Postcode);
					AssertEquals("+61 8 9089 0890", staffWithHomeAddressPermissionsLoaded2.GS_HomePhone_Formatted);

					Assert(!staffWithHomeAddressPermissionsLoaded2.GS_UserAddress1Info.ReadOnly);
					Assert(!staffWithHomeAddressPermissionsLoaded2.GS_UserAddress2Info.ReadOnly);
					Assert(!staffWithHomeAddressPermissionsLoaded2.GS_CityInfo.ReadOnly);
					Assert(!staffWithHomeAddressPermissionsLoaded2.GS_StateInfo.ReadOnly);
					Assert(!staffWithHomeAddressPermissionsLoaded2.GS_PostcodeInfo.ReadOnly);
					Assert(!staffWithHomeAddressPermissionsLoaded2.GS_HomePhone_FormattedInfo.ReadOnly);
				}
			}
		}

		public void TestViewOtherReferences()
		{
			ZString viewDeniedMessage = "** View Denied due to Security Access **";

			var staffWithoutViewOtherReferences = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutViewOtherReferences.GS_Pager = "References1";

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherReferences.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutViewOtherReferences.PK;
			staffWithoutViewOtherReferences.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithViewOtherReferences = Factory.NewWithValidTestData<GlbStaff>();
			staffWithViewOtherReferences.GS_Pager = "References2";

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherReferences.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithViewOtherReferences.PK;
			staffWithViewOtherReferences.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutViewOtherReferences.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithoutViewOtherReferences.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var staffWithViewOtherReferencesLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithViewOtherReferences.PK));
					var staffWithoutViewOtherReferencesLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutViewOtherReferences.PK));

					AssertEquals(viewDeniedMessage, staffWithViewOtherReferencesLoaded1.GS_Pager);
					Assert(staffWithViewOtherReferencesLoaded1.GS_PagerInfo.ReadOnly);
					AssertEquals("References1", staffWithoutViewOtherReferencesLoaded1.GS_Pager);
					Assert(!staffWithoutViewOtherReferencesLoaded1.GS_PagerInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithViewOtherReferences.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithViewOtherReferences.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory();
					var staffWithoutViewOtherReferencesLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutViewOtherReferences.PK));
					var staffWithViewOtherReferencesLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithViewOtherReferences.PK));

					AssertEquals("References1", staffWithoutViewOtherReferencesLoaded2.GS_Pager);
					Assert(!staffWithoutViewOtherReferencesLoaded2.GS_PagerInfo.ReadOnly);
					AssertEquals("References2", staffWithViewOtherReferencesLoaded2.GS_Pager);
					Assert(!staffWithViewOtherReferencesLoaded2.GS_PagerInfo.ReadOnly);
				}
			}
		}

		public void TestViewBirthdate()
		{
			var refTime = new ZDate(2004, 3, 3);
			var staffWithoutBirthDatePermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutBirthDatePermissions.GS_IsController = false;
			staffWithoutBirthDatePermissions.GS_Birthdate = refTime;

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewBirthDate.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutBirthDatePermissions.PK;
			staffWithoutBirthDatePermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithBirthDatePermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithBirthDatePermissions.GS_Birthdate = refTime;

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewBirthDate.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithBirthDatePermissions.PK;
			staffWithBirthDatePermissions.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(staffWithoutBirthDatePermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertEquals("Precondition: Current user has changed to staff without permissions", staffWithoutBirthDatePermissions.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var staffWithBirthDatePermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithBirthDatePermissions.PK));
					var staffWithoutBirthDatePermissionsLoaded1 = factoryForLoading1.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutBirthDatePermissions.PK));

					AssertEquals("Staff without permissions should not be able to view other staff birthdate", ZDateTime.Empty, staffWithBirthDatePermissionsLoaded1.GS_Birthdate);
					Assert("Staff without permissions should not be able to edit other staff birthdate", staffWithBirthDatePermissionsLoaded1.GS_BirthdateInfo.ReadOnly);

					AssertEquals("Staff without permissions should be able to view own birthdate", refTime, staffWithoutBirthDatePermissionsLoaded1.GS_Birthdate);
					Assert("Staff without permissions should be able to edit own birthdate", !staffWithoutBirthDatePermissionsLoaded1.GS_BirthdateInfo.ReadOnly);
				}
				using (Env.SetTemporaryUserContext(staffWithBirthDatePermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertEquals("Precondition: Current user has changed to staff with permissions", staffWithBirthDatePermissions.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory(); // New factory required because staff permissions already cached in Factory 1, and we need to re-load the permissions based on the new CurrentUser.
					var staffWithBirthDatePermissionsLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithBirthDatePermissions.PK));
					var staffWithoutBirthDatePermissionsLoaded2 = factoryForLoading2.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffWithoutBirthDatePermissions.PK));

					AssertEquals("Staff with permissions should be able to view other staff birthdate", refTime, staffWithoutBirthDatePermissionsLoaded2.GS_Birthdate);
					Assert("Staff with permissions should be able to edit other staff birthdate", !staffWithoutBirthDatePermissionsLoaded2.GS_BirthdateInfo.ReadOnly);

					AssertEquals("Staff with permissions should be able to view own birthdate", refTime, staffWithBirthDatePermissionsLoaded2.GS_Birthdate);
					Assert("Staff with permissions should be able to edit own staff birthdate", !staffWithoutBirthDatePermissionsLoaded2.GS_BirthdateInfo.ReadOnly);
				}
			}
		}

		public void TestLocalAdminCanViewAllDetailsAndNormalUserIsBlocked()
		{
			ZDate today = ZDate.Today;
			GlbStaff localAdmin = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff normalStaff = Factory.NewWithValidTestData<GlbStaff>();

			Staff.FillWithValidTestData();
			Staff.GS_NextOfKinWorkPhone = "1";
			Staff.GS_UserAddress1 = "2";
			Staff.GS_WagesBankAccount = "3";
			Staff.GS_Birthdate = today;

			localAdmin.SecurityChangeOthersView.AddSecurityToChangeOtherStaff(Staff.GS_Code);
			normalStaff.GS_IsOperational = true;
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(localAdmin.GS_LoginName))
			{
				GlbStaff loadedStaff = new BusinessObjectFactory().Load<GlbStaff>(Staff.PK);
				AssertEquals("GS_NextOfKinWorkPhone_Formatted", "1", loadedStaff.GS_NextOfKinWorkPhone_Formatted);
				AssertEquals("GS_UserAddress1", "2", loadedStaff.GS_UserAddress1);
				AssertEquals("GS_WagesBankAccount", "3", loadedStaff.GS_WagesBankAccount);
				AssertEquals("GS_Birthdate", today, loadedStaff.GS_Birthdate);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(normalStaff.GS_LoginName))
			{
				GlbStaff loadedStaff = new BusinessObjectFactory().Load<GlbStaff>(Staff.PK);
				// GS_HomePhone_Formatted
				AssertEquals("GS_HomePhone_Formatted", Staff.ViewDeniedMessage, loadedStaff.GS_HomePhone_Formatted);
				loadedStaff.GS_HomePhone_Formatted = loadedStaff.GS_HomePhone_Formatted;
				AssertNoErrors("GS_HomePhone_Formatted", loadedStaff.GS_HomePhone_FormattedInfo);
				// GS_NextOfKinHomePhone_Formatted
				AssertEquals("GS_NextOfKinHomePhone_Formatted", Staff.ViewDeniedMessage, loadedStaff.GS_NextOfKinHomePhone_Formatted);
				loadedStaff.GS_NextOfKinHomePhone_Formatted = loadedStaff.GS_NextOfKinHomePhone_Formatted;
				AssertNoErrors("GS_NextOfKinHomePhone_Formatted", loadedStaff.GS_NextOfKinHomePhone_FormattedInfo);
				// GS_NextOfKinWorkPhone_Formatted
				AssertEquals("GS_NextOfKinWorkPhone_Formatted", Staff.ViewDeniedMessage, loadedStaff.GS_NextOfKinWorkPhone_Formatted);
				loadedStaff.GS_NextOfKinWorkPhone_Formatted = loadedStaff.GS_NextOfKinWorkPhone_Formatted;
				AssertNoErrors("GS_NextOfKinWorkPhone_Formatted", loadedStaff.GS_NextOfKinWorkPhone_FormattedInfo);
				// GS_EmergencyHomePhone_Formatted
				AssertEquals("GS_EmergencyHomePhone_Formatted", Staff.ViewDeniedMessage, loadedStaff.GS_EmergencyHomePhone_Formatted);
				loadedStaff.GS_EmergencyHomePhone_Formatted = loadedStaff.GS_EmergencyHomePhone_Formatted;
				AssertNoErrors("GS_EmergencyHomePhone_Formatted", loadedStaff.GS_EmergencyHomePhone_FormattedInfo);
				// GS_EmergencyWorkPhone_Formatted
				AssertEquals("GS_EmergencyWorkPhone_Formatted", Staff.ViewDeniedMessage, loadedStaff.GS_EmergencyWorkPhone_Formatted);
				loadedStaff.GS_EmergencyWorkPhone_Formatted = loadedStaff.GS_EmergencyWorkPhone_Formatted;
				AssertNoErrors("GS_EmergencyWorkPhone_Formatted", loadedStaff.GS_EmergencyWorkPhone_FormattedInfo);
				// GS_UserAddress1
				AssertEquals("GS_UserAddress1", Staff.ViewDeniedMessage, loadedStaff.GS_UserAddress1);
				// GS_WagesBankAccount
				AssertEquals("GS_WagesBankAccount", Staff.ViewDeniedMessage, loadedStaff.GS_WagesBankAccount);
				// GS_WagesBankAccount
				AssertEquals("GS_Birthdate", ZDateTime.Empty, loadedStaff.GS_Birthdate);
			}
		}

		public void TestNormalStaffCanStillSaveWithInvalidPhoneNumbers()
		{
			GlbStaff normalStaff = Factory.NewWithValidTestData<GlbStaff>();
			normalStaff.GS_RN_NKCountryCode = "CN";
			GlbStaff targetStaff = Factory.NewWithValidTestData<GlbStaff>();
			targetStaff.GS_HomePhone = "abc";
			targetStaff.GS_NextOfKinHomePhone = "def";
			targetStaff.GS_NextOfKinWorkPhone = "557";
			targetStaff.GS_EmergencyHomePhone = "5151515";
			targetStaff.GS_EmergencyWorkPhone = "n s s987";
			targetStaff.GS_IsController = false;
			Factory.Save();
			Assert(targetStaff.IsInDatabase);

			using (CurrentUserChanger.SwitchToNewUserTemporarily(normalStaff.GS_LoginName))
			{
				// A normal staff sees view denied message, so the validation should be bypassed.
				AssertEquals(targetStaff.ViewDeniedMessage, targetStaff.GS_HomePhone_Formatted);
				AssertEquals(targetStaff.ViewDeniedMessage, targetStaff.GS_NextOfKinHomePhone_Formatted);
				AssertEquals(targetStaff.ViewDeniedMessage, targetStaff.GS_NextOfKinWorkPhone_Formatted);
				AssertEquals(targetStaff.ViewDeniedMessage, targetStaff.GS_EmergencyHomePhone_Formatted);
				AssertEquals(targetStaff.ViewDeniedMessage, targetStaff.GS_EmergencyWorkPhone_Formatted);
				targetStaff.Validation.ValidateAll();
				AssertNoErrors(targetStaff.GS_HomePhone_FormattedInfo);
				AssertNoErrors(targetStaff.GS_NextOfKinHomePhone_FormattedInfo);
				AssertNoErrors(targetStaff.GS_NextOfKinWorkPhone_FormattedInfo);
				AssertNoErrors(targetStaff.GS_EmergencyHomePhone_FormattedInfo);
				AssertNoErrors(targetStaff.GS_EmergencyWorkPhone_FormattedInfo);

				// Grant the security access to remove the denied message.
				Env.Security.StaffViewEmergencyContact.IsAllowed = true;
				Env.Security.StaffViewHomeAddressDetails.IsAllowed = true;
				Factory.Save();
				AssertEquals(targetStaff.GS_HomePhone, targetStaff.GS_HomePhone_Formatted);
				AssertEquals(targetStaff.GS_NextOfKinHomePhone, targetStaff.GS_NextOfKinHomePhone_Formatted);
				AssertEquals(targetStaff.GS_NextOfKinWorkPhone, targetStaff.GS_NextOfKinWorkPhone_Formatted);
				AssertEquals(targetStaff.GS_EmergencyHomePhone, targetStaff.GS_EmergencyHomePhone_Formatted);
				AssertEquals(targetStaff.GS_EmergencyWorkPhone, targetStaff.GS_EmergencyWorkPhone_Formatted);

				// Revoke the security access to StaffDetails
				Env.Security.StaffDetails.IsAllowed = false;
				Factory.Save();
				Assert(!normalStaff.IsDetailsModifiable);

				// Now the staff can see the phone numbers but read only, so validation should also be bypassed.
				using (Env.Security.SecurityCachingDisabler)
				{
					targetStaff.Validation.ValidateAll();
					AssertNoErrors(targetStaff.GS_HomePhone_FormattedInfo);
					AssertNoErrors(targetStaff.GS_NextOfKinHomePhone_FormattedInfo);
					AssertNoErrors(targetStaff.GS_NextOfKinWorkPhone_FormattedInfo);
					AssertNoErrors(targetStaff.GS_EmergencyHomePhone_FormattedInfo);
					AssertNoErrors(targetStaff.GS_EmergencyWorkPhone_FormattedInfo);
				}
			}
		}

		#endregion

		#endregion

		#region Tests DeleteUnusedStoredDefaults

		public void TestMakeStaffInactiveDeletesUnusedStoredDefaults()
		{
			AssertUnusedStoredDefaultsAreDeleted((GlbStaff staff) => { staff.GS_IsActive = false; Factory.Save(); });
		}

		public void TestDeleteStaffDeletesUnusedStoredDefaults()
		{
			AssertUnusedStoredDefaultsAreDeleted((GlbStaff staff) => staff.Delete());
		}

		void AssertUnusedStoredDefaultsAreDeleted(Action<GlbStaff> action)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsOperational = false;
			var data1ForStaff = Factory.New<StmData>();
			data1ForStaff.SD_Owner = staff.PK;
			var data2ForStaff = Factory.New<StmData>();
			data2ForStaff.SD_Owner = staff.PK;
			data2ForStaff.SD_DepartmentGuid = ZGuid.NewZGuid();

			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_IsOperational = false;
			var data1ForOtherStaff = Factory.New<StmData>();
			data1ForOtherStaff.SD_Owner = otherStaff.PK;
			var data2ForOtherStaff = Factory.New<StmData>();
			data2ForOtherStaff.SD_Owner = otherStaff.PK;
			data2ForOtherStaff.SD_DepartmentGuid = ZGuid.NewZGuid();

			Factory.Save();

			action(staff);

			AssertNull(Factory.Load<StmData>(data1ForStaff.PK));
			AssertNull(Factory.Load<StmData>(data2ForStaff.PK));
			AssertNotNull(Factory.Load<StmData>(data1ForOtherStaff.PK));
			AssertNotNull(Factory.Load<StmData>(data2ForOtherStaff.PK));
		}

		#endregion

		#region Test ISupportChangeOthersSecurity

		public void TestISupportChangeOthersSecurityMembers()
		{
			ISupportChangeOthersSecurity support = Staff;
			AssertEquals("CompleteGroupList", Staff.Lookups.CompleteGroupList, support.CompleteGroupList);
			AssertEquals("CompleteStaffList", Staff.Lookups.CompleteStaffList, support.CompleteStaffList);
			AssertEquals("GlbSecurityRightHolderColumn", GlbSecuritySchema.GU_GS, support.GlbSecurityRightHolderColumn);
			AssertEquals("PK", Staff.PK, support.PK);
			AssertNotNull("SecurityChangeOthersView", support.SecurityChangeOthersView);
		}

		#endregion

		#region External Passwords

		public void TestTradenetv4Password()
		{
			Staff.GS_LoginName = "TST";
			Staff.GS_Code = "TST";

			Staff.HasChanges = false;
			var sgStaffWrapper = Staff.GetSGWrapper();
			var s = sgStaffWrapper.Tradenetv4Password.GP_MailBoxID;
			AssertEquals(false, Staff.HasChanges);

			AddExternalPassword("ZXC", Staff.PK, GlbCompany.CurrentCompany.PK);
			AddExternalPassword("ZXC", Factory.NewWithValidTestData<GlbStaff>().PK, Factory.NewWithValidTestData<GlbCompany>().PK);
			AddExternalPassword("ZXC", Staff.PK, Factory.NewWithValidTestData<GlbCompany>().PK);

			Factory.Save();

			sgStaffWrapper.Tradenetv4Password.GP_MailBoxID = "TEST";
			AssertEquals(true, Staff.HasChanges);

			Factory.Save();

			var reloadStaff = Factory.Load<GlbStaff>(Staff.PK);
			var sgStaffWrapperForReload = reloadStaff.GetSGWrapper();

			AssertEquals("TEST", sgStaffWrapperForReload.Tradenetv4Password.GP_MailBoxID);
			AssertEquals(reloadStaff.PK, sgStaffWrapperForReload.Tradenetv4Password.GP_GS);
			AssertEquals(PasswordTypesList.Codes.SG4, sgStaffWrapperForReload.Tradenetv4Password.GP_PasswordType);
		}

		public void TestAccessPassword()
		{
			Staff.GS_LoginName = "TST";
			Staff.GS_Code = "TST";

			Staff.HasChanges = false;
			var sgStaffWrapper = Staff.GetSGWrapper();
			Staff.RegisterEditableChildObject((BusinessObject)sgStaffWrapper);
			var s = sgStaffWrapper.AccessPassword.GP_MailBoxID;
			AssertEquals(false, Staff.HasChanges);

			AddExternalPassword("ZXC", Staff.PK, GlbCompany.CurrentCompany.PK);
			AddExternalPassword("ZXC", Factory.NewWithValidTestData<GlbStaff>().PK, Factory.NewWithValidTestData<GlbCompany>().PK);
			AddExternalPassword("ZXC", Staff.PK, Factory.NewWithValidTestData<GlbCompany>().PK);

			Factory.Save();

			sgStaffWrapper.AccessPassword.GP_MailBoxID = "TEST";
			AssertEquals(true, Staff.HasChanges);

			Factory.Save();

			var reloadStaff = Factory.Load<GlbStaff>(Staff.PK);
			var sgStaffWrapperForReload = reloadStaff.GetSGWrapper();

			AssertEquals("TEST", sgStaffWrapperForReload.AccessPassword.GP_MailBoxID);
			AssertEquals(reloadStaff.PK, sgStaffWrapperForReload.AccessPassword.GP_GS);
			AssertEquals(PasswordTypesList.Codes.SGA, sgStaffWrapperForReload.AccessPassword.GP_PasswordType);
		}

		public void TestSGNationalTradePlatformPassword()
		{
			Staff.GS_LoginName = "TST";
			Staff.GS_Code = "TST";

			Staff.HasChanges = false;
			var sgStaffWrapper = Staff.GetSGWrapper();
			Staff.RegisterEditableChildObject((BusinessObject)sgStaffWrapper);
			var s = sgStaffWrapper.SGNationalTradePlatformPassword.GP_MailBoxID;
			AssertEquals(false, Staff.HasChanges);

			AddExternalPassword("ZXC", Staff.PK, GlbCompany.CurrentCompany.PK);
			AddExternalPassword("ZXC", Factory.NewWithValidTestData<GlbStaff>().PK, Factory.NewWithValidTestData<GlbCompany>().PK);
			AddExternalPassword("ZXC", Staff.PK, Factory.NewWithValidTestData<GlbCompany>().PK);

			Factory.Save();

			sgStaffWrapper.SGNationalTradePlatformPassword.GP_MailBoxID = "TEST";
			AssertEquals(true, Staff.HasChanges);

			Factory.Save();

			var reloadStaff = Factory.Load<GlbStaff>(Staff.PK);
			var sgStaffWrapperForReload = reloadStaff.GetSGWrapper();

			AssertEquals("TEST", sgStaffWrapperForReload.SGNationalTradePlatformPassword.GP_MailBoxID);
			AssertEquals(reloadStaff.PK, sgStaffWrapperForReload.SGNationalTradePlatformPassword.GP_GS);
			AssertEquals(PasswordTypesList.Codes.NTP, sgStaffWrapperForReload.SGNationalTradePlatformPassword.GP_PasswordType);
		}

		public void TestNZBPassword()
		{
			Staff.GS_LoginName = "TST";
			Staff.GS_Code = "TST";

			Staff.HasChanges = false;
			var nzStaffWrapper = Staff.GetNZWrapper();
			var s = nzStaffWrapper.NZBPassword.GP_MailBoxID;
			AssertEquals(false, Staff.HasChanges);

			AddExternalPassword("ZXC", Staff.PK, GlbCompany.CurrentCompany.PK);
			AddExternalPassword("ZXC", Factory.NewWithValidTestData<GlbStaff>().PK, Factory.NewWithValidTestData<GlbCompany>().PK);
			AddExternalPassword("ZXC", Staff.PK, Factory.NewWithValidTestData<GlbCompany>().PK);

			Factory.Save();

			nzStaffWrapper.NZBPassword.GP_UserID = "TEST";
			AssertEquals(true, Staff.HasChanges);

			Factory.Save();

			var reloadStaff = Factory.Load<GlbStaff>(Staff.PK);
			var nzStaffWrapperForReload = reloadStaff.GetNZWrapper();

			AssertEquals("TEST", nzStaffWrapperForReload.NZBPassword.GP_UserID);
			AssertEquals(reloadStaff.PK, nzStaffWrapperForReload.NZBPassword.GP_GS);
			AssertEquals(PasswordTypesList.Codes.NZB, nzStaffWrapperForReload.NZBPassword.GP_PasswordType);
		}

		public void TestNUTPassword()
		{
			Staff.GS_LoginName = "TST";
			Staff.GS_Code = "TST";

			Staff.HasChanges = false;
			var auStaffWrapper = Staff.GetAUWrapper();
			var s = auStaffWrapper.NUTPassword.GP_MailBoxID;
			AssertEquals(false, Staff.HasChanges);

			AddExternalPassword("ZXC", Staff.PK, GlbCompany.CurrentCompany.PK);
			AddExternalPassword("ZXC", Factory.NewWithValidTestData<GlbStaff>().PK, Factory.NewWithValidTestData<GlbCompany>().PK);
			AddExternalPassword("ZXC", Staff.PK, Factory.NewWithValidTestData<GlbCompany>().PK);

			Factory.Save();

			auStaffWrapper.NUTPassword.CurrentDecryptedPassword = "TEST";
			AssertEquals(true, Staff.HasChanges);

			Factory.Save();

			var reloadStaff = Factory.Load<GlbStaff>(Staff.PK);
			var auStaffWrapperForReload = reloadStaff.GetAUWrapper();
			AssertNotNullOrEmpty(auStaffWrapperForReload.NUTPassword.CurrentDecryptedPassword);
			AssertEquals(reloadStaff.PK, auStaffWrapperForReload.NUTPassword.GP_GS);
			AssertEquals(PasswordTypesList.Codes.NUT, auStaffWrapperForReload.NUTPassword.GP_PasswordType);
		}

		GlbExternalPassword AddExternalPassword(string passwordType, ZGuid staffPk, ZGuid companyPK)
		{
			GlbExternalPassword result = Factory.New<GlbExternalPassword>();
			result.GP_GC = companyPK;
			result.GP_GS = staffPk;
			result.GP_PasswordType = passwordType;
			return result;
		}

		#endregion

		#region Activity Logs

		[TestDate(2012, 12, 4, 16, 0, 0)]
		public void TestLastActivityUtc()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "testuser";
			staff.GS_Code = "TZ1";
			// Clear other activity logs
			Db.Connection.ExecuteNonQuery(string.Format("DELETE dbo.StmActivityLog WHERE S7_GS_NKUser = '{0}'", Staff.GS_Code));
			Factory.Save();

			AssertEquals(ZDateTime.Empty, staff.LastActivityUtc(10));
			AssertEquals(ZDateTime.Empty, staff.LastActivityUtc(100));

			StmActivityLog log = Factory.New<StmActivityLog>();
			log.S7_GS_NKUser = staff.GS_Code;
			log.S7_OpenDateTimeUtc = ZDateTime.UtcNow.AddHours(-1);
			log.S7_FormCaption = "Caption";
			Factory.Save();

			AssertEquals(ZDateTime.Empty, staff.LastActivityUtc(10));
			AssertEquals(ZDateTime.Empty, staff.LastActivityUtc(59));
			AssertEquals(log.S7_OpenDateTimeUtc, staff.LastActivityUtc(60));
			AssertEquals(log.S7_OpenDateTimeUtc, staff.LastActivityUtc(100));
		}

		public void TestActivityLogsFilter()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADE";

			StmActivityLog log1 = Factory.New<StmActivityLog>();
			log1.S7_GS_NKUser = staff.GS_Code;
			log1.S7_KeyStrokes = 30;
			log1.S7_MouseClicks = 10;
			log1.S7_ControlChanges = 4;
			log1.S7_ActiveTime = 600;
			log1.S7_InactiveTime = 200;

			StmActivityLog log2 = Factory.New<StmActivityLog>();
			log2.S7_GS_NKUser = staff.GS_Code;
			log2.S7_KeyStrokes = 10;
			log2.S7_MouseClicks = 25;
			log2.S7_ControlChanges = 13;
			log2.S7_ActiveTime = 100;
			log2.S7_InactiveTime = 600;

			staff.ActivityLogForUserFilterProvider.ActivityLogFilterDateFrom = new ZDateTime(2006, 1, 1, 9, 0, 0);
			staff.ActivityLogForUserFilterProvider.ActivityLogFilterDateTo = new ZDateTime(2006, 1, 1, 12, 0, 0);
			staff.ActivityLogForUserFilterProvider.ActivityLogFilterType = "ABC";
			staff.ActivityLogForUserFilterProvider.ActivityLogFilterFormCaption = "Hello";

			AssertEquals(new ZDateTime(2006, 1, 1, 9, 0, 0), staff.ActivityLogForUserFilterProvider.ActivityLogFilterDateFrom);
			AssertEquals(new ZDateTime(2006, 1, 1, 12, 0, 0), staff.ActivityLogForUserFilterProvider.ActivityLogFilterDateTo);
			AssertEquals("ABC", staff.ActivityLogForUserFilterProvider.ActivityLogFilterType);
			AssertEquals("Hello", staff.ActivityLogForUserFilterProvider.ActivityLogFilterFormCaption);

			staff.ActivityLogForUserFilterProvider.ClearActivityLogFilters();
			AssertEquals(ZDateTime.Today, staff.ActivityLogForUserFilterProvider.ActivityLogFilterDateFrom);
			AssertEquals(ZDateTime.Today.AddDays(1), staff.ActivityLogForUserFilterProvider.ActivityLogFilterDateTo);
			AssertEquals(StmActivityLogCollection.ActivityTypeAll, staff.ActivityLogForUserFilterProvider.ActivityLogFilterType);
			AssertEquals("", staff.ActivityLogForUserFilterProvider.ActivityLogFilterFormCaption);

			staff.ActivityLogForUserFilterProvider.ActivityLogFilterDateFrom = ZDateTime.Empty;
			staff.ActivityLogForUserFilterProvider.ActivityLogFilterDateTo = ZDateTime.Empty;

			staff.ActivityLogsForUser.Load();
			AssertEquals(2, staff.ActivityLogsForUser.Count);
			AssertEquals(11.7m, staff.ActivityLogForUserFilterProvider.ActivityLogTotalActiveMinutes);
			AssertEquals(13.3m, staff.ActivityLogForUserFilterProvider.ActivityLogTotalInactiveMinutes);
			AssertEquals(40, staff.ActivityLogForUserFilterProvider.ActivityLogTotalKeyStrokes);
			AssertEquals(35, staff.ActivityLogForUserFilterProvider.ActivityLogTotalMouseClicks);
			AssertEquals(17, staff.ActivityLogForUserFilterProvider.ActivityLogTotalControlChanges);
		}

		#endregion

		#region WantsToSeeDocumentTemplateErrors

		public void TestWantsToSeeDocumentTemplateErrors()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			AssertEquals("staff.WantsToSeeDocumentTemplateErrors", false, staff.WantsToSeeDocumentTemplateErrors);
			staff.SetWantsToSeeDocumentTemplateErrorsForTesting(true);
			AssertEquals("staff.WantsToSeeDocumentTemplateErrors", true, staff.WantsToSeeDocumentTemplateErrors);
			staff.SetWantsToSeeDocumentTemplateErrorsForTesting(false);
			AssertEquals("staff.WantsToSeeDocumentTemplateErrors", false, staff.WantsToSeeDocumentTemplateErrors);
		}

		#endregion

		#region IsMemberOf

		public void TestIsMemberOf()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			GlbStaff anotherStaff = Factory.New<GlbStaff>();

			GlbGroup group1 = Factory.New<GlbGroup>();
			GlbGroup group2 = Factory.New<GlbGroup>();
			GlbGroup group3 = Factory.New<GlbGroup>();

			staff.Groups.Add(group1);
			staff.Groups.Add(group2);

			anotherStaff.Groups.Add(group1);
			anotherStaff.Groups.Add(group3);

			GlbGroupCollection groupCollection12 = new GlbGroupCollection(Factory);
			groupCollection12.Add(group1);
			groupCollection12.Add(group2);

			GlbGroupCollection groupCollection13 = new GlbGroupCollection(Factory);
			groupCollection13.Add(group1);
			groupCollection13.Add(group3);

			GlbGroupCollection groupCollection123 = new GlbGroupCollection(Factory);
			groupCollection123.Add(group1);
			groupCollection123.Add(group2);
			groupCollection123.Add(group3);

			Assert("staff should be a member of groupCollection12", staff.IsMemberOf(groupCollection12));
			Assert("anotherStaff should NOT be a member of groupCollection12", !anotherStaff.IsMemberOf(groupCollection12));

			Assert("staff should NOT be a member of groupCollection12", !staff.IsMemberOf(groupCollection13));
			Assert("anotherStaff should be a member of groupCollection13", anotherStaff.IsMemberOf(groupCollection13));

			Assert("staff should NOT be a member of groupCollection123", !staff.IsMemberOf(groupCollection123));
			Assert("anotherStaff NOT should be a member of groupCollection123", !anotherStaff.IsMemberOf(groupCollection123));
		}

		#endregion

		#region Groups

		public void TestGetGroupUserLeads()
		{
			GlbGroup team1 = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup team2 = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroupLink link1 = Factory.New<GlbGroupLink>();
			GlbGroupLink link2 = Factory.New<GlbGroupLink>();
			link1.GK_GG = team1.PK;
			link2.GK_GG = team2.PK;
			link1.GK_GS = Staff.PK;
			link2.GK_GS = Staff.PK;
			link1.GK_MembershipType = MembershipTypeList.Codes.STF;
			link2.GK_MembershipType = MembershipTypeList.Codes.TLD;
			AssertEquals(team2, Staff.GetGroupThisUserLeads());
		}

		public void TestAllGroupsDeletedOnDelete()
		{
			var x = Staff.AllGroups;
			Staff.Delete();
			AssertEquals(x.Count, 0);
		}

		public void TestGroupOwners()
		{
			var factory = new BusinessObjectFactory();

			var groupA = factory.NewWithValidTestData<GlbGroup>();
			var groupB = factory.NewWithValidTestData<GlbGroup>();
			var groupC = factory.NewWithValidTestData<GlbGroup>();
			var user1 = factory.NewWithValidTestData<GlbStaff>();
			var user2 = factory.NewWithValidTestData<GlbStaff>();
			var user3 = factory.NewWithValidTestData<GlbStaff>();

			user1.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			user1.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(groupA.GG_Code);
			user1.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
			user1.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(groupB.GG_Code);
			user1.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(groupC.GG_Code);

			user2.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			user2.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(groupA.GG_Code);
			user2.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(groupB.GG_Code);
			user2.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
			user2.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(groupC.GG_Code);

			user3.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
			user3.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(groupA.GG_Code);
			user3.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(groupB.GG_Code);
			user3.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(groupC.GG_Code);

			factory.Save();

			var user1OwnedGroupsView = user1.GroupOwners;
			var user2OwnedGroupsView = user2.GroupOwners;
			var user3OwnedGroupsView = user3.GroupOwners;

			AssertEquals(user1OwnedGroupsView.Count, 1);
			AssertEquals("user1OwnedGroupsView[0].GU_GS", user1.PK, user1OwnedGroupsView[0].GU_GS);
			AssertEquals("user1OwnedGroupView[0].GU_ItemGUID", groupA.PK, user1OwnedGroupsView[0].GU_ItemGUID);
			AssertEquals(user2OwnedGroupsView.Count, 2);
			AssertEquals("user2OwnedGroupsView[0].GU_GS", user2.PK, user2OwnedGroupsView[0].GU_GS);
			AssertEquals("user2OwnedGroupView[0].GU_ItemGUID", groupA.PK, user2OwnedGroupsView[0].GU_ItemGUID);
			AssertEquals("user2OwnedGroupsView[0].GU_GS", user2.PK, user2OwnedGroupsView[1].GU_GS);
			AssertEquals("user2OwnedGroupView[0].GU_ItemGUID", groupB.PK, user2OwnedGroupsView[1].GU_ItemGUID);
			AssertEquals(user3OwnedGroupsView.Count, 0);
		}

		#endregion

		#region Capabilities

		public void TestDelete_ShouldDeleteCapabilityLinks()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			resource.Capabilities.Add(capability);

			var pivot = resource.CapabilityPivots.Single();

			Factory.Save();
			resource.Delete();

			AssertEquals(false, capability.IsDeleted);
			AssertEquals(true, pivot.IsDeleted);

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestCapabilities()
		{
			var c1 = Factory.NewWithValidTestData<GlbCapability>();
			var c2 = Factory.NewWithValidTestData<GlbCapability>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.Capabilities.Add(c1);
			staff1.Capabilities.Add(c2);

			staff2.Capabilities.Add(c2);

			AssertEquals(2, staff1.Capabilities.Count);
			AssertEquals(1, staff2.Capabilities.Count);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			staff1 = newFactory.Load<GlbStaff>(staff1.PK);
			staff2 = newFactory.Load<GlbStaff>(staff2.PK);

			AssertEquals(2, staff1.Capabilities.Count);
			AssertEquals(1, staff2.Capabilities.Count);
		}

		#endregion

		#region Group Security

		public void TestGroupSecurity()
		{
			AssertNotNull(Staff.StaffSecurity);
			Assert(Staff.StaffSecurity.Count > 0);
		}

		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", true, PreventDeleteAttribute.IsTrue(typeof(GlbStaff)));
		}

		#endregion

		#region ICertificates Members

		public void TestGetCertificateTypeList()
		{
			var certificateTypeList = (CodeDescriptionPairList)((ICertificatesProvider)Staff).GetCertificateTypeList();
			Assert("GetCertificateTypeList should only contain registry defined values", certificateTypeList.ContainsOnly(
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.APP,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.DBH,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.DGN,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.DTA,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.FIN,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.FKL,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.IAT,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.MSC,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.NID,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.PID,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.TFN,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.TRK,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.WKP,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BKG,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BCT,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CON,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CDN,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CDL,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.REP,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.RTD,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CAR,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.EDL,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.HZM,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.LVC,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.MID,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.NAI,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.NEX,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.OTD,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.PAS,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.PR1,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.PR2,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.SEN,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.AR1,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.AR2,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.MMD,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.USP,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.VIM,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.VNI,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.ACE,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.APC,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CO1,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CO2,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CO3,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CS1,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CS2,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CM1,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CNO,
				Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.COD
			));
		}

		#endregion

		#region Change Password

		public void TestChangeLoginPassword()
		{
			Staff.GS_LoginName = "testuser";
			Staff.StaffPlainTextPassword = "password";
			Factory.Save();
			using (Env.SetTemporaryUserContext("testuser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Staff.GS_ChangePasswordAtNextLogin = false;
				Staff.ResetPassword("zzz");
				Assert(Staff.GS_ChangePasswordAtNextLogin);

				Staff.ChangePassword("zzz", "NewPassword");
				Factory.Save();

				AssertEquals("Change password", true, Staff.VerifyPassword("NewPassword"));
				AssertEquals("Password change date should be updated", Env.Time.CurrentLocalDate, Staff.GS_LastPasswordChangeDate.Date);
				Assert(!Staff.GS_ChangePasswordAtNextLogin);
			}
		}

		public void TestChangeLoginPasswordHandlesConcurrencyViolation()
		{
			Staff.GS_LoginName = "testuser";
			Staff.StaffPlainTextPassword = "password";
			Factory.Save();
			using (Env.SetTemporaryUserContext("testuser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				BusinessObjectFactory factory = new BusinessObjectFactory(TestConnection);
				GlbStaff staffSecondReference = factory.Load<GlbStaff>(Staff.PK);
				staffSecondReference.GS_FullName = "Test User";
				factory.Save();

				Staff.ResetPassword("NewPassword");
				Factory.Save();
				AssertEquals("Change password", true, Staff.VerifyPassword("NewPassword"));
				AssertEquals("Password change date should be updated", Env.Time.CurrentLocalDate, Staff.GS_LastPasswordChangeDate.Date);
				AssertEquals("Fields should have been updated from database when resolving the concurrent violation", "Test User", Staff.GS_FullName);
			}
		}

		public void TestChangeLoginPasswordHandlesConcurrencyVioloationSamePasswordChange()
		{
			Staff.GS_LoginName = "testuser";
			Staff.StaffPlainTextPassword = "password";
			Factory.Save();
			using (Env.SetTemporaryUserContext("testuser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				BusinessObjectFactory factory = new BusinessObjectFactory(TestConnection);
				GlbStaff staffSecondReference = factory.Load<GlbStaff>(Staff.PK);
				staffSecondReference.ResetPassword("NewPassword");
				factory.Save();

				Staff.ResetPassword("NewPassword");
				AssertEquals("Change password", true, Staff.VerifyPassword("NewPassword"));
				AssertEquals("Password change date should be updated", Env.Time.CurrentLocalDate, Staff.GS_LastPasswordChangeDate.Date);
			}
		}

		public void TestChangePassword_PasswordToHistory()
		{
			EnvProxy.Instance.Registry.PasswordHistoryCount = 2;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = "pAssword0";
			Factory.Save();

			// change password
			staff.ChangePassword("pAssword0", "pAssword1");
			Factory.Save();
			AssertEquals("Old password has been used", true, staff.HasPasswordBeenUsed("pAssword0"));
			AssertEquals("New password has been used", true, staff.HasPasswordBeenUsed("pAssword1"));

			// reset password
			staff.ChangePassword(null, "pAssword2");
			Factory.Save();
			AssertEquals("Previous old password", false, staff.HasPasswordBeenUsed("pAssword0"));
			AssertEquals("Old password has been used", true, staff.HasPasswordBeenUsed("pAssword1"));
			AssertEquals("New password has been used", true, staff.HasPasswordBeenUsed("pAssword2"));

			staff.ChangePassword("pAssword2", "pAssword3");
			Factory.Save();
			AssertEquals("Previous old password", false, staff.HasPasswordBeenUsed("pAssword1"));
			AssertEquals("Old password has been used", true, staff.HasPasswordBeenUsed("pAssword2"));
			AssertEquals("New password has been used", true, staff.HasPasswordBeenUsed("pAssword3"));

			// empty current password/Password must be reset
			staff.LocalPasswordMustBeReset = true;
			Factory.Save();
			AssertEquals("Previous old password", false, staff.HasPasswordBeenUsed("pAssword2"));
			AssertEquals("Previois GS_PasswordHash is in password history", true, staff.HasPasswordBeenUsed("pAssword3"));
			AssertEquals("Null password", false, staff.HasPasswordBeenUsed(null));
			AssertEquals("Emppty password", false, staff.HasPasswordBeenUsed(""));

			// reset when force to reset
			staff.ChangePassword(null, "pAssword4");
			Factory.Save();
			AssertEquals("Old password has been used", true, staff.HasPasswordBeenUsed("pAssword3"));
			AssertEquals("New password has been used", true, staff.HasPasswordBeenUsed("pAssword4"));

			// reduce password history count
			EnvProxy.Instance.Registry.PasswordHistoryCount = 1;
			AssertEquals("Old password has been used", false, staff.HasPasswordBeenUsed("pAssword3"));
			AssertEquals("New password has been used", true, staff.HasPasswordBeenUsed("pAssword4"));
		}

		#endregion

		#region Sales teams

		public void TestDetachSalesTeamsIfNeeded()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			SalesTeam wiseSalesTeam = Factory.NewWithValidTestData<SalesTeam>();
			SalesTeam cargoSalesTeam = Factory.NewWithValidTestData<SalesTeam>();
			staff.GS_IsSalesRep = true;
			staff.SalesTeams.Add(wiseSalesTeam);
			staff.SalesTeams.Add(cargoSalesTeam);
			Factory.Save();
			AssertCollectionContains("Staff, who is a Sales Rep, can stay in Sales Teams", wiseSalesTeam, staff.SalesTeams);
			AssertCollectionContains("Staff, who is a Sales Rep, can stay in Sales Teams", cargoSalesTeam, staff.SalesTeams);

			staff.GS_IsSalesRep = false;
			Factory.Save();
			AssertCollectionNotContains("Staff, that used to be a Sales Rep, should be resigned from current Sales Team", wiseSalesTeam, staff.SalesTeams);
			AssertCollectionNotContains("Staff, that used to be a Sales Rep, should be resigned from current Sales Team", cargoSalesTeam, staff.SalesTeams);
		}

		public void TestGetSalesTeamResponsibleFor()
		{
			SalesTeam auSalesTeam = Factory.NewWithValidTestData<SalesTeam>();
			auSalesTeam.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));

			SalesTeam usSalesTeam = Factory.NewWithValidTestData<SalesTeam>();
			usSalesTeam.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US"));

			SalesTeam parisSalesTeam = Factory.NewWithValidTestData<SalesTeam>();
			parisSalesTeam.CoveredUnlocos.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "FRPAR"));

			GlbCompany xxCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch xxBranch = xxCompany.Branches.AddNew();
			SalesTeam sydSalesTeamForXXCompany = Factory.NewWithValidTestData<SalesTeam>();
			sydSalesTeamForXXCompany.GG_GC = xxCompany.PK;
			sydSalesTeamForXXCompany.CoveredUnlocos.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsSalesRep = true;
			staff.SalesTeams.Add(auSalesTeam);
			staff.SalesTeams.Add(usSalesTeam);
			staff.SalesTeams.Add(parisSalesTeam);
			staff.SalesTeams.Add(sydSalesTeamForXXCompany);

			Factory.Save();

			var sydUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			AssertEquals(auSalesTeam, staff.GetSalesTeamResponsibleFor(sydUnloco));

			var newyorkUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USNYC"));
			AssertEquals(usSalesTeam, staff.GetSalesTeamResponsibleFor(newyorkUnloco));

			var parisUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "FRPAR"));
			AssertEquals(parisSalesTeam, staff.GetSalesTeamResponsibleFor(parisUnloco));

			var saintMandeUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "FRZYD"));
			AssertEquals(null, staff.GetSalesTeamResponsibleFor(saintMandeUnloco));

			AssertEquals(null, staff.GetSalesTeamResponsibleFor(null));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), xxBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				sydUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
				AssertEquals(sydSalesTeamForXXCompany, staff.GetSalesTeamResponsibleFor(sydUnloco));
			}
		}

		#endregion

		#region Active Directory

		public void TestNewStaff_WhenADEnabled_ShouldSetInvalidADGuid()
		{
			var staff = Factory.New<GlbStaff>();
			AssertEquals(ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			staff = Factory.New<GlbStaff>();
			staff.Validation.ValidateAll();
			AssertEquals(ZGuid.Invalid, staff.GS_ActiveDirectoryObjectGuid);
			AssertNoErrors("Should be able to save an invalid GUID", staff.GS_ActiveDirectoryObjectGuidInfo);
		}

		public void TestGS_ActiveDirectoryObjectGuid_WhenReactivating()
		{
			var adUserMock = new Mock<IADUser>();
			adUserMock.Setup(m => m.HasExistingDirectoryEntry()).Returns(false);

			var adUser2Mock = new Mock<IADUser>();
			adUser2Mock.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);

			var adEntityProviderMock = new Mock<IADEntityProvider>();
			adEntityProviderMock.Setup(m => m.GetADUser(It.Is<IGlbStaff>(s => s.GS_LoginName != "staff2"))).Returns(adUserMock.Object);
			adEntityProviderMock.Setup(m => m.GetADUser(It.Is<IGlbStaff>(s => s.GS_LoginName == "staff2"))).Returns(adUser2Mock.Object);
			ObjectFactory.Substitute(adEntityProviderMock.Object);

			var adObjectGuid = ZGuid.NewZGuid();

			// Not AD-linked
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			staff1.GS_IsActive = false;
			Assert(!staff1.IsADLinked);

			// AD-linked and AD object exists 
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "staff2";
			staff2.GS_ActiveDirectoryObjectGuid = adObjectGuid;
			staff2.GS_IsActive = false;
			Assert(staff2.IsADLinked);

			// AD-linked but AD object missing
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff3.GS_IsActive = false;
			Assert(staff3.IsADLinked);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			staff1.GS_IsActive = true;
			staff2.GS_IsActive = true;
			staff3.GS_IsActive = true;

			AssertEquals("staff1's Empty AdObjectGuid should be set to Invalid", ZGuid.Invalid, staff1.GS_ActiveDirectoryObjectGuid);
			AssertEquals("staff2's Valid AdObjectGuid should remain", adObjectGuid, staff2.GS_ActiveDirectoryObjectGuid);
			AssertEquals("staff3's Valid AdObjectGuid should be reset as the AD Object is missing", ZGuid.Invalid, staff3.GS_ActiveDirectoryObjectGuid);

			AssertNoErrors(staff1.GS_ActiveDirectoryObjectGuidInfo);
			AssertNoErrors(staff2.GS_ActiveDirectoryObjectGuidInfo);
			AssertNoErrors(staff3.GS_ActiveDirectoryObjectGuidInfo);

			adUserMock.VerifyAll();
			adUser2Mock.VerifyAll();
		}

		public void TestIsADLinked()
		{
			var staff = Factory.New<GlbStaff>();

			staff.GS_ActiveDirectoryObjectGuid = Guid.Empty;
			Assert(!staff.IsADLinked);

			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Assert(staff.IsADLinked);
		}

		public void TestIsADLinkable()
		{
			var staff = Factory.New<GlbStaff>();
			Assert(((IADLinkedEntity)staff).IsADLinkable);

			staff.GS_IsResource = false;
			Assert(((IADLinkedEntity)staff).IsADLinkable);

			staff.GS_IsResource = true;
			Assert(!((IADLinkedEntity)staff).IsADLinkable);
		}

		public void TestADShouldUnlinkInactiveStaffs_No()
		{
			var staff = Factory.New<GlbStaff>();
			var adObjectGuid = ZGuid.NewZGuid();
			staff.GS_ActiveDirectoryObjectGuid = adObjectGuid;
			staff.GS_IsActive = true;

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().ShouldUnlinkInactiveStaff = false;

			staff.GS_IsActive = false;
			AssertEquals("Should be deactivated", false, staff.GS_IsActive);
			AssertEquals("Should remain linked", true, staff.IsADLinked);
			AssertEquals("Should keep the original AD GUID", adObjectGuid, staff.GS_ActiveDirectoryObjectGuid);
		}

		public void TestADShouldUnlinkInactiveStaffs_Yes()
		{
			var staff = Factory.New<GlbStaff>();
			var adObjectGuid = ZGuid.NewZGuid();
			staff.GS_ActiveDirectoryObjectGuid = adObjectGuid;
			staff.GS_IsActive = true;

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().ShouldUnlinkInactiveStaff = true;

			staff.GS_IsActive = false;
			AssertEquals("Should be deactivated", false, staff.GS_IsActive);
			AssertEquals("Should be unlinked", false, staff.IsADLinked);
			AssertEquals("Should clear AD GUID", ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);
		}

		public void TestDeactivationShouldResetADObjectGuidWhenItIsInvalid()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			staff.GS_IsActive = true;

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().ShouldUnlinkInactiveStaff = false;

			staff.GS_IsActive = false;
			AssertEquals("Should be deactivated", false, staff.GS_IsActive);
			AssertEquals("Should be reset", false, staff.IsADLinked);
			AssertEquals("Should be empty", ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);
		}

		public void TestDisconnectFromAD()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_IsActive = true;
			AssertEquals(1, staff.Groups.Count);

			staff.DisconnectFromAD();
			AssertEquals(ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);
			Assert(!staff.GS_IsActive);
			AssertEquals(0, staff.Groups.Count);
		}

		public void TestDisconnectFromAD_SecurityCheck()
		{
			var loginWithRight = CreateStaffWithSecurityRights("StaffModify", true);
			var loginWithoutRight = CreateStaffWithSecurityRights("StaffModify", false);
			AssertNotNull(loginWithRight);
			AssertNotNull(loginWithoutRight);
			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_IsActive = true;
			AssertEquals(1, staff.Groups.Count);

			using (Env.Instance.SetTemporaryUserContext(loginWithoutRight.GS_LoginName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertExceptionThrown<SecurityAccessDeniedException>(() => staff.DisconnectFromAD());
			}

			using (Env.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertNoExceptionThrown(() => staff.DisconnectFromAD());
			}
		}

		GlbStaff CreateStaffWithSecurityRights(string securityKey, bool allowed)
		{
			var checkPoint = Env.Instance.Security.FindCheckPoint(securityKey);
			if (checkPoint == null)
			{
				return null;
			}

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var security = Factory.New<GlbSecurity>();
			security.GU_GS = staff.PK;

			security.GU_SecurityRight = checkPoint.Code;
			security.GU_SecurityItemIsAllowed = allowed;
			return staff;
		}

		[ExpectNoExceptions]
		public void TestSaveDoesNotQueryADWhenItIsNotRequired()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var adUser = new Mock<IADUser>();
			adUser.Setup(m => m.HasExistingDirectoryEntry());
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			staff = Factory.Load<GlbStaff>(staff.PK);
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();
			adUser.Verify(m => m.HasExistingDirectoryEntry(), Times.Never);
		}

		public void TestPasswordRelatedPropertiesReadOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			AssertEquals("GS_PasswordNeverChanges", true, staff.GS_PasswordNeverChangesInfo.ReadOnly);
			AssertEquals("GS_ChangePasswordAtNextLogin", true, staff.GS_ChangePasswordAtNextLoginInfo.ReadOnly);
			AssertEquals("LocalPasswordMustBeReset", false, staff.LocalPasswordMustBeResetInfo.ReadOnly);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().SyncMode = SyncMode.ADIsMaster;
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.TwoWay;
			AssertEquals("GS_PasswordNeverChanges", true, staff.GS_PasswordNeverChangesInfo.ReadOnly);
			AssertEquals("GS_ChangePasswordAtNextLogin", true, staff.GS_ChangePasswordAtNextLoginInfo.ReadOnly);
			AssertEquals("LocalPasswordMustBeReset", true, staff.LocalPasswordMustBeResetInfo.ReadOnly);
			AssertEquals("PasswordNeverChanges", false, staff.PasswordNeverChangesInfo.ReadOnly);
			AssertEquals("ChangePasswordAtNextLogin", false, staff.ChangePasswordAtNextLoginInfo.ReadOnly);

			//AD, 1Way sync
			ObjectFactory.Get<IADRegistry>().SyncMode = SyncMode.ADIsMaster;
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.OneWay;
			AssertEquals("GS_PasswordNeverChanges", true, staff.GS_PasswordNeverChangesInfo.ReadOnly);
			AssertEquals("GS_ChangePasswordAtNextLogin", true, staff.GS_ChangePasswordAtNextLoginInfo.ReadOnly);
			AssertEquals("LocalPasswordMustBeReset", true, staff.LocalPasswordMustBeResetInfo.ReadOnly);
			AssertEquals("PasswordNeverChanges", true, staff.PasswordNeverChangesInfo.ReadOnly);
			AssertEquals("ChangePasswordAtNextLogin", true, staff.ChangePasswordAtNextLoginInfo.ReadOnly);
		}

		public void TestPasswordNeverChanges()
		{
			var adUser = new Mock<IADUser>();
			adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_PasswordNeverChanges = false;
			adUser.Setup(m => m.PasswordDoesntExpireUserAttribute).Returns(true);
			Factory.Save();
			AssertEquals("Staff should be linked with AD", true, staff.IsADLinked);
			AssertEquals("Should get PasswordNeverChanges flag from AD", true, staff.PasswordNeverChanges);

			//AD Disabled
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			AssertEquals("Should get PasswordNeverChanges flag from Enterprise", false, staff.PasswordNeverChanges);

			adUser.VerifyAll();
		}

		public void TestChangePasswordAtNextLogin()
		{
			var adUser = new Mock<IADUser>();
			adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_ChangePasswordAtNextLogin = false;
			adUser.Setup(m => m.PasswordMustChangeAtNextLogon).Returns(true);
			Factory.Save();
			AssertEquals("Staff should be linked with AD", true, staff.IsADLinked);
			AssertEquals("Should get ChangePasswordAtNextLogin flag from AD", true, staff.ChangePasswordAtNextLogin);

			//AD Disabled
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			AssertEquals("Should get ChangePasswordAtNextLogin flag from Enterprise", false, staff.ChangePasswordAtNextLogin);

			adUser.VerifyAll();
		}

		public void TestLastPasswordChangeDate()
		{
			var adUser = new Mock<IADUser>();
			adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			var adLastPasswordSet = new DateTime(2017, 11, 11);
			var cw1LastPasswordSet = new DateTime(2017, 12, 12);

			adUser.Setup(m => m.PasswordLastSet).Returns(adLastPasswordSet);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LastPasswordChangeDate = cw1LastPasswordSet;

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			AssertEquals("Staff should be linked with AD", true, staff.IsADLinked);
			AssertEquals("Should get pwsLastSet flag from AD", adLastPasswordSet, staff.LastPasswordChangeDate);

			//AD Disabled
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			AssertEquals("Should get GS_LastPasswordChangeDate flag from Enterprise", cw1LastPasswordSet, staff.LastPasswordChangeDate);

			adUser.VerifyAll();
		}

		public void TestChangeLoginPassword_LastPasswordChangeDate()
		{
			var adUser = new Mock<IADUser>();
			adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
			var adPasswordLastSet = new ZDateTime(2017, 12, 15);
			var nonAdPasswordLastSet = new ZDateTime(2017, 12, 31);
			adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
			adUser.Setup(m => m.PasswordLastSet).Returns(adPasswordLastSet.ToDateTime());
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);

			ObjectFactory.Substitute(adEntityProvider.Object);

			AssertNotEquals(nonAdPasswordLastSet, Staff.LastPasswordChangeDate);
			AssertNotEquals(adPasswordLastSet, Staff.LastPasswordChangeDate);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			TestDateAttribute.Date = nonAdPasswordLastSet.ToDateTime();
			AssertEquals(false, Staff.IsADLinked);
			Staff.ResetPassword("NewLocalP455w0rd");
			AssertEquals("AD Disabled, LastPasswordChangeDate should be GS_LastPasswordChangeDate", Staff.GS_LastPasswordChangeDate, Staff.LastPasswordChangeDate);
			AssertDateTimeWithinOneSecond("AD Disabled, LastPasswordChangeDate should be set to now", ZDateTime.Now.ToDateTime(), Staff.LastPasswordChangeDate.ToDateTime());

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			Staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			AssertEquals(true, Staff.IsADLinked);
			Staff.ResetPassword("NewDomainPassword");
			AssertEquals("AD Enabled, LastPasswordChangeDate should be returned from AD", adPasswordLastSet, Staff.LastPasswordChangeDate);
		}

		public void TestADUserDoesNotExist_ShouldReturnDefaultValues()
		{
			var adUser = new Mock<IADUser>();
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			Staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			AssertEquals(true, Staff.IsADLinked);

			//AD User does not exist, return default value
			AssertEquals(DateTime.MinValue, Staff.LastPasswordChangeDate);
			AssertEquals(false, Staff.PasswordNeverChanges);
			AssertEquals(false, Staff.ChangePasswordAtNextLogin);

			adEntityProvider.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestChangeLoginPassword_ADEnabled_OldPasswordProvided_ShouldChangePassword_ShouldNotSetPasswordMustChangeAtNextLogon()
		{
			var adEntityProvider = new Mock<IADEntityProvider>();
			var adUser = new Mock<IADUser>();
			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
				Factory.Save();
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

				adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
				adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
				adUser.Setup(m => m.PasswordMustChangeAtNextLogon).Returns(false);
				adUser.Setup(m => m.ChangePassword("0ldP@5sw0rd", "n3wP@5sw0rd"));
				adUser.Setup(m => m.PasswordMustChangeAtNextLogon).Returns(false);
				staff.ChangePassword("0ldP@5sw0rd", "n3wP@5sw0rd");
				Factory.Save();
				adUser.Verify(m => m.PasswordMustChangeAtNextLogon, Times.Never);
				adUser.Verify(m => m.ChangePassword("0ldP@5sw0rd", "n3wP@5sw0rd"));
			}
		}

		[ExpectNoExceptions]
		public void TestChangeLoginPassword_ADEnabled_OldPasswordNOTProvided_ShouldSetPassword_ShouldNotSetPasswordMustChangeAtNextLogon()
		{
			var adEntityProvider = new Mock<IADEntityProvider>();
			var adUser = new Mock<IADUser>();
			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
				Factory.Save();
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

				adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
				adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
				adUser.Setup(m => m.PasswordMustChangeAtNextLogon).Returns(false);
				adUser.Setup(m => m.SetPassword("n3wP@5sw0rd"));
				adUser.Setup(m => m.PasswordMustChangeAtNextLogon).Returns(false);
				staff.ResetPassword("n3wP@5sw0rd");
				Factory.Save();
				adUser.Setup(m => m.PasswordMustChangeAtNextLogon).Returns(false);
				adUser.Verify(m => m.SetPassword("n3wP@5sw0rd"));
				adUser.Verify(m => m.PasswordMustChangeAtNextLogon, Times.Never);
			}
		}

		public void TestCanHandleExceptionWhenADWritePermissionRemoved()
		{
			var adEntityProvider = new Mock<IADEntityProvider>();
			var adUser = new Mock<IADUser>();

			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = "dummy";
				staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

				adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
				adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
				//When AD's write permission removed during the operation, DirectoryServicesException will be thrown when setting PasswordDoesntExpire
				staff.PasswordNeverChanges = true;
				adUser.Setup(m => m.PasswordDoesntExpireUserAttribute).Throws(new DirectoryServicesException("bla"));
				AssertExceptionThrown<ZCannotSaveException>(Factory.Save);
			}

			adUser.VerifyAll();
		}

		public void TestChangeLoginPassword_ADEnabled_NotLinked()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			AssertExceptionThrown<InvalidOperationException>("Unexpected Exception", "The staff has not been synchronized to Active Directory yet, please try again later.", () => staff.ResetPassword("n3wP@5sw0rd"));
		}

		public void TestChangeLoginPassword_ADEnabled_NotExists()
		{
			var adEntityProvider = new Mock<IADEntityProvider>();
			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "coffeepot";
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

				adEntityProvider.Setup(m => m.GetADUser(staff)).Returns((IADUser)null);
				AssertExceptionThrown<InvalidOperationException>("Unexpected Exception", $"Active Directory User '{staff.GS_LoginName}' is missing, please contact your system administrator.", () => staff.ResetPassword("n3wP@5sw0rd"));
			}
		}

		[ExpectNoExceptions]
		public void TestADPasswordExpired_ShouldNotRequireWritePrivileges()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = TestConstants.ADTestUserAccount.Name;
			bool isADPasswordExpired = staff.ADPasswordExpired;
		}

		[ExpectNoExceptions]
		public void TestADPasswordMustChangeAtNextLogon_ShouldNotRequireWritePrivileges()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = TestConstants.ADTestUserAccount.Name;
			bool isADPasswordMustChangeAtNextLogon = staff.ADPasswordMustChangeAtNextLogon;
		}

		[ExpectNoExceptions]
		public void TestADNumberOfDaysTillPasswordExpiry_ShouldNotRequireWritePrivileges()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = TestConstants.ADTestUserAccount.Name;
			staff.GetADNumberOfDaysTillPasswordExpiry();
		}

		[ExpectNoExceptions]
		public void TestSynchroniseWithAD()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var syncDirectorProvider = new Mock<ISynchronisationDirectorProvider>();
			var syncDirector = new Mock<ISynchronisationDirector>();

			var adEntityProvider = new Mock<IADEntityProvider>();
			var adUser = new Mock<IADUser>();
			using (ObjectFactory.Substitute(syncDirectorProvider.Object))
			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				var staff = Factory.New<GlbStaff>();

				syncDirectorProvider.Setup(p => p.GetSyncDirector(Factory, new[] { staff })).Returns(syncDirector.Object);

				adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
				staff.SynchroniseWithAD();

				adUser.Verify(u => u.Synchronise(null), Times.Once);
				adUser.Verify(u => u.CommitChanges(), Times.Once);
				syncDirector.Verify(s => s.SyncUsersToRoboticGroupIfRequired(new[] { adUser.Object }), Times.Once);
			}

			adEntityProvider.VerifyAll();
		}
		public void TestSynchroniseWithAD_SecurityCheck()
		{
			var domainCredentials = ObjectFactory.New<IDomainCredentials>();
			domainCredentials.DomainName = TestConstants.Domain;
			domainCredentials.DomainUserName = TestConstants.ADTestUserAccount.Name;
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccount.Password;
			domainCredentials.IsDefaultDomain = true;
			domainCredentials.UserOrganisationalUnit = TestConstants.ValidOU;
			domainCredentials.GroupOrganisationalUnit = TestConstants.ValidOU;
			domainCredentials.DefaultPassword = "Changeme12345";

			ObjectFactory.Get<IADRegistry>().DefaultDomainCredentials = domainCredentials;
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var loginWithRight = CreateStaffWithSecurityRights("StaffViewHomeAddressDetails", true);
			var loginWithoutRight = CreateStaffWithSecurityRights("StaffViewHomeAddressDetails", false);
			AssertNotNull(loginWithRight);
			AssertNotNull(loginWithoutRight);
			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_IsActive = true;
			AssertEquals(1, staff.Groups.Count);

			using (Env.Instance.SetTemporaryUserContext(loginWithoutRight.GS_LoginName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertExceptionThrown<SecurityAccessDeniedException>(() => staff.SynchroniseWithAD());
			}

			using (Env.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertNoExceptionThrown(() => staff.SynchroniseWithAD());
			}
		}

		[ExpectNoExceptions]
		public void TestUnlockADAccount()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var adEntityProvider = new Mock<IADEntityProvider>();
			var adUser = new Mock<IADUser>();
			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

				adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
				adUser.Setup(m => m.UnlockAccount());
				adUser.Setup(m => m.CommitChanges()).Returns(true);
				staff.UnlockAccount();
			}

			adUser.VerifyAll();
		}

		public void TestUnlockAccount()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TestLogin";

			var person = Factory.NewWithValidTestData<GlbPerson>();
			staff.GS_PER = person.PK;
			person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddMinutes(30);
			Factory.Save();

			CreateLockoutUserRecord(staff, 10);
			AssertEquals(true, LoginAttemptRecorder.IsAnonymousUserLockedOut(staff.GS_LoginName));

			staff.UnlockAccount();
			AssertEquals(person.PER_LoginDisabledUntilUtc, ZDateTime.Empty);
			AssertEquals("Staff should have changes", true, staff.HasChanges);

			AssertEquals(false, LoginAttemptRecorder.IsAnonymousUserLockedOut(staff.GS_LoginName));
		}

		public void TestGetDownLevelLogonName()
		{
			var adEntityProvider = new Mock<IADEntityProvider>();
			var adUser = new Mock<IADUser>();
			adUser.Setup(m => m.LoginName).Returns("LoginName");
			adUser.Setup(m => m.SAMAccountName).Returns("preWin2000Username01");
			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "LoginName";

				adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
				adUser.Setup(m => m.DomainNetBiosName).Returns("DummyDomain");
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
				AssertEquals("AD Integration disabled, Staff not linked to AD:", null, staff.GetDownLevelLogonName());

				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				staff.IsBackupOperator = !staff.IsBackupOperator;
				AssertEquals("AD Integration enabled, Staff not linked to AD:", null, staff.GetDownLevelLogonName());

				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
				staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();
				staff.IsBackupOperator = !staff.IsBackupOperator;
				AssertEquals("AD Integration disabled, Staff linked to AD:", null, staff.GetDownLevelLogonName());

				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				staff.IsBackupOperator = !staff.IsBackupOperator;
				AssertEquals("AD Integration Enabled, Staff linked to AD:", "DummyDomain\\preWin2000Username01", staff.GetDownLevelLogonName());
			}
		}

		public void TestGetDownLevelLogonNameHandlesRowNotInTableException()
		{
			var staff = Factory.New<GlbStaff>();
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			staff.IsBackupOperator = !staff.IsBackupOperator;
			var adEntityProvider = new Mock<ADEntityProvider_RowNotInTable>();
			using (ObjectFactory.Substitute<IADEntityProvider>(adEntityProvider.Object))
			{
				staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();
				AssertEquals("AD Integration Enabled, RowNotInTable exception for GetADUser() is handled:", null, staff.GetDownLevelLogonName());
			}

			adEntityProvider.VerifyAll();
		}

		public void TestLocalPasswordMustBeReset()
		{
			Factory.Save(); // To ensure ChangeLocalPassword works
			Staff.ChangeLocalPassword(null, "myPassword");
			AssertEquals(false, Staff.LocalPasswordMustBeReset);

			Staff.GS_PasswordHash = ZBlob.Empty;
			Staff.GS_PasswordHashIterations = 0;
			Staff.GS_PasswordSalt = ZBlob.Empty;
			AssertEquals(true, Staff.LocalPasswordMustBeReset);

			Factory.Save();
			AssertEquals(true, Staff.LocalPasswordMustBeReset);

			var staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(Staff.PK);
			AssertEquals(true, staffReloaded.LocalPasswordMustBeReset);

			//If password is stored into the hash - GS_PasswordHash is not empty
			UserSecretsContext.DefaultContext.SaveSecret("myP@ssw0rd", UserSecretHashAlgorithm.Pbkdf2HmacSha1, DataRegistry.Instance.PasswordHashingIterationsCount, Staff.GetPasswordAdapter());
			AssertEquals(false, Staff.LocalPasswordMustBeReset);

			Factory.Save();
			AssertEquals(false, Staff.LocalPasswordMustBeReset);

			staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(Staff.PK);
			AssertEquals(false, staffReloaded.LocalPasswordMustBeReset);
		}

		public void TestLocalPasswordMustBeReset_WhenADObjectGuidChanged()
		{
			Staff.GS_PasswordHash = ZBlob.FromAscii("whatever");

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			AssertEquals(false, Staff.LocalPasswordMustBeReset);

			//Invalid Guid
			Staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			AssertEquals(false, Staff.LocalPasswordMustBeReset);

			//Empty Guid
			Staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			AssertEquals(false, Staff.LocalPasswordMustBeReset);

			//Valid Guid
			Staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			AssertEquals(true, Staff.LocalPasswordMustBeReset);

			//AD was off - setting GS_ActiveDirectoryObjectGuid wont change LocalPasswordMustBeReset
			Staff.GS_PasswordHash = ZBlob.FromAscii("whatever");
			Staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			AssertEquals(false, Staff.LocalPasswordMustBeReset);

			Staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			AssertEquals(false, Staff.LocalPasswordMustBeReset);
		}

		public void TestLocalPasswordMustBeReset_Setter()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = "something";
			Factory.Save();

			AssertEquals(false, staff.LocalPasswordMustBeReset);

			staff.LocalPasswordMustBeReset = true;
			Factory.Save();

			AssertEquals(ZBlob.Empty, staff.GS_PasswordHash);
			AssertEquals(ZBlob.Empty, staff.GS_PasswordSalt);
			AssertEquals(0, staff.GS_PasswordHashIterations);

			staff.LocalPasswordMustBeReset = false;
			Factory.Save();

			//To ensure the hash is not empty and not corrupted
			AssertNotEquals(ZBlob.Empty, staff.GS_PasswordHash);
			AssertNotEquals(ZBlob.Empty, staff.GS_PasswordSalt);
			AssertNotEquals(0, staff.GS_PasswordHashIterations);
			AssertNoExceptionThrown(() => staff.VerifyPassword("something wrong"));
		}

		#endregion

		#region ComponentMembership

		public void TestComponentMembership()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";

			var system = Factory.New<IBMSystem>();
			system.FS_Name = "Test BM System";
			var component = (IBMComponent)system.Components.AddNew();
			component.FC_Name = "Test Bucket";

			Factory.Save();

			var link = (IBMComponentResourceLink)staff.ComponentMembership.AddNew();
			AssertEquals("STF", link.FD_GS_NKResource);
			link.FD_FC_Component = component.PK;

			Assert(staff.HasChanges);
		}

		#endregion

		#region Logging for Attaching / Detaching Group

		class FakeScheduledReportHelper : IScheduledReportHelper
		{
			public List<string> GetScheduleReportsDescriptionAssignedToUser(string userCode, bool mustBeActive, int maxCount = 0)
			{
				ScheduledReportHelperInvokeCounter++;
				return new List<string>();
			}

			public string GetPrintUserSafe(ZBlob stream)
			{
				return ZString.Empty;
			}

			public byte[] SerializeOrgCodeInScheduledReportsByPK(ZBlob scheduleReport, ZGuid parentID, List<string> filterDisplayNameList)
			{
				return null;
			}

			public static int ScheduledReportHelperInvokeCounter { get; set; }
		}

		public void TestLoggingForAttachingOrDetachingGroup()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DDD";
			group1.GG_Desc = "Group DDD";

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "GGG";
			group2.GG_Desc = "Group GGG";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			staff1.GS_FullName = "Tester";

			staff1.Groups.Add(group1);

			Factory.Save();
			AssertEquals("Attached log is added to group", true, group1.Logs.Find(log => log.SL_Reference == "Attached - (TS1) Tester").Any());
			AssertEquals("Attached log is added to staff", true, staff1.Logs.Find(log => log.SL_Reference == "Attached - (DDD) Group DDD").Any());

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";
			staff2.GS_FullName = "Tester";
			staff2.Groups.Add(group1);
			staff2.Groups.Remove(group1);

			Factory.Save();
			AssertEquals("Attached log is not added to group since staff is removed", false, group1.Logs.Find(log => log.SL_Reference == "Attached - (TS2) Tester").Any());
			AssertEquals("Attached log is not added to staff since staff is removed", false, staff2.Logs.Find(log => log.SL_Reference == "Attached - (DDD) Group DDD").Any());

			staff2.Groups.Add(group1);
			Factory.Save();
			AssertEquals("Attached log is added to group", true, group1.Logs.Find(log => log.SL_Reference == "Attached - (TS2) Tester").Any());
			AssertEquals("Attached log is added to staff", true, staff2.Logs.Find(log => log.SL_Reference == "Attached - (DDD) Group DDD").Any());

			staff1.Groups.Remove(group1);
			staff1.Groups.Add(group1);
			Factory.Save();
			AssertEquals("Attached log is not added to group since staff is added back", 1, group1.Logs.Find(log => log.SL_Reference == "Attached - (TS1) Tester").Count());
			AssertEquals("Attached log is not added to staff since staff is added back", 1, staff1.Logs.Find(log => log.SL_Reference == "Attached - (DDD) Group DDD").Count());
			AssertEquals("Detached log is not added to group since staff is added back", false, group1.Logs.Find(log => log.SL_Reference == "Detached - (TS1) Tester").Any());
			AssertEquals("Detached log is not added to staff since staff is added back", false, staff1.Logs.Find(log => log.SL_Reference == "Detached - (DDD) Group DDD").Any());

			staff1.Groups.Remove(group1);
			staff1.Groups.Add(group2);
			Factory.Save();
			AssertEquals("Detached log is added to group", true, group1.Logs.Find(log => log.SL_Reference == "Detached - (TS1) Tester").Any());
			AssertEquals("Detached log is added to staff", true, staff1.Logs.Find(log => log.SL_Reference == "Detached - (DDD) Group DDD").Any());
			AssertEquals("Attached log is added to group", true, group2.Logs.Find(log => log.SL_Reference == "Attached - (TS1) Tester").Any());
			AssertEquals("Attached log is added to staff", true, staff1.Logs.Find(log => log.SL_Reference == "Attached - (GGG) Group GGG").Any());

			staff1.Delete();
			Factory.Save();
			AssertEquals("Detached log is added to group", true, group2.Logs.Find(log => log.SL_Reference == "Detached - (TS1) Tester").Any());
		}

		public void TestDontCheckScheduledReportAssignmentWhenDetachingFromGroup()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DDD";
			group1.GG_Desc = "Group DDD";

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "GGG";
			group2.GG_Desc = "Group GGG";

			using (ObjectFactory.Substitute<IScheduledReportHelper>(new FakeScheduledReportHelper()))
			{
				Staff.GS_Code = "TS1";
				Staff.GS_FullName = "Tester";
				Staff.Groups.Add(group1);
				Staff.Groups.Add(group2);
				Staff.GS_IsActive = true;
				Factory.Save();

				Staff.GS_IsActive = false;
				FakeScheduledReportHelper.ScheduledReportHelperInvokeCounter = 0;
				Staff.Groups.Remove(group1);
				Staff.Groups.Remove(group2);
				Assert(Staff.GS_IsActiveInfo.HasChanges);
				AssertEquals("Not calling Scheduled Report Assignment check", 0, FakeScheduledReportHelper.ScheduledReportHelperInvokeCounter);
			}
		}

		public void TestCheckScheduledReportAssignmentWhenChangingGS_Isactive()
		{
			using (ObjectFactory.Substitute<IScheduledReportHelper>(new FakeScheduledReportHelper()))
			{
				Staff.GS_IsActive = true;
				Staff.Factory.Save();
				FakeScheduledReportHelper.ScheduledReportHelperInvokeCounter = 0;

				Staff.GS_IsActive = false;
				AssertEquals("Validation called when deactivating staff", 1,
					FakeScheduledReportHelper.ScheduledReportHelperInvokeCounter);
				Staff.GS_IsActive = true;
				AssertEquals("Validation not called when activating staff", 1,
					FakeScheduledReportHelper.ScheduledReportHelperInvokeCounter);
				Staff.GS_IsActive = false;
				AssertEquals("Validation called when deactivating staff", 2,
					FakeScheduledReportHelper.ScheduledReportHelperInvokeCounter);
				Staff.RunPreSaveValidation();
				AssertEquals("Validation NOT called when running pre-save validation", 2,
					FakeScheduledReportHelper.ScheduledReportHelperInvokeCounter);
			}
		}

		#endregion

		#region Phone Numbers

		public void TestPhoneNumbers()
		{
			Staff.GS_PublishMobilePhone = true;
			Staff.GS_PublishHomePhone = true;
			Staff.GS_PublishFaxNum = true;
			Staff.GS_PublishWorkPhone = true;

			AssertEquals(true, Staff.GS_MobilePhone_Wrapper.IsPublishedForBinding);
			AssertEquals(true, Staff.GS_HomePhone_Wrapper.IsPublishedForBinding);
			AssertEquals(true, Staff.GS_FaxNum_Wrapper.IsPublishedForBinding);
			AssertEquals(true, Staff.GS_WorkPhone_Wrapper.IsPublishedForBinding);

			Staff.GS_RN_NKCountryCode = "AU";
			Staff.GS_MobilePhone = "0426 829 924";
			Staff.GS_HomePhone = "2 8001 2200";
			Staff.GS_FaxNum = "+86-156-0113-1981";
			Staff.GS_WorkPhone = "Work101";
			Staff.GS_Pager = "Pager101";
			Staff.GS_NextOfKinHomePhone = "0426 829 924";
			Staff.GS_NextOfKinWorkPhone = "2 8001 2200";
			Staff.GS_EmergencyHomePhone = "+86-156-0113-1981";
			Staff.GS_EmergencyWorkPhone = "ICEMobile101";

			AssertEquals("+61 426 829 924", Staff.GS_MobilePhone_Wrapper.FormattedForBinding);
			AssertEquals("+61 2 8001 2200", Staff.GS_HomePhone_Wrapper.FormattedForBinding);
			AssertEquals("+86 156 0113 1981", Staff.GS_FaxNum_Wrapper.FormattedForBinding);
			AssertEquals("Work101", Staff.GS_WorkPhone_Wrapper.FormattedForBinding);
			AssertEquals("+61 426 829 924", Staff.GS_NextOfKinHomePhone_Wrapper.FormattedForBinding);
			AssertEquals("+61 2 8001 2200", Staff.GS_NextOfKinWorkPhone_Wrapper.FormattedForBinding);
			AssertEquals("+86 156 0113 1981", Staff.GS_EmergencyHomePhone_Wrapper.FormattedForBinding);
			AssertEquals("ICEMobile101", Staff.GS_EmergencyWorkPhone_Wrapper.FormattedForBinding);

			AssertEquals("0426 829 924", Staff.GS_MobilePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("(02) 8001 2200", Staff.GS_HomePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals(string.Empty, Staff.GS_FaxNum_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals(string.Empty, Staff.GS_WorkPhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("0426 829 924", Staff.GS_NextOfKinHomePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("(02) 8001 2200", Staff.GS_NextOfKinWorkPhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals(string.Empty, Staff.GS_EmergencyHomePhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals(string.Empty, Staff.GS_EmergencyWorkPhone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);

			Staff.GS_PublishMobilePhone = false;
			Staff.GS_PublishHomePhone = false;
			Staff.GS_PublishFaxNum = false;
			Staff.GS_PublishWorkPhone = false;

			AssertEquals(false, Staff.GS_MobilePhone_Wrapper.IsPublishedForBinding);
			AssertEquals(false, Staff.GS_HomePhone_Wrapper.IsPublishedForBinding);
			AssertEquals(false, Staff.GS_FaxNum_Wrapper.IsPublishedForBinding);
			AssertEquals(false, Staff.GS_WorkPhone_Wrapper.IsPublishedForBinding);

			Staff.GS_NextOfKinHomePhone_IsManuallyVerified = true;
			AssertEquals(1, GenCustomAddOnRuleAckCount);
			Staff.Delete();
			AssertEquals(0, GenCustomAddOnRuleAckCount);
		}

		#endregion

		#region Address Validation

		public void TestValidationStatus_WhenChangingAddressFieldWhileValueIsCna_ShouldKeepItAsCna()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ValidationStatus = AddressValidationStatus.CountryNotAvailable;

			// Act & Assert.

			staff.GS_UserAddress1 = "[_MOCK_ADDRESS_1_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, staff.GS_ValidationStatus);

			staff.GS_UserAddress2 = "[_MOCK_ADDRESS_2_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, staff.GS_ValidationStatus);

			staff.GS_City = "[_MOCK_CITY_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, staff.GS_ValidationStatus);

			staff.GS_Postcode = "0000";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, staff.GS_ValidationStatus);

			staff.GS_State = "[_MOCK_STATE_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, staff.GS_ValidationStatus);

			staff.GS_RN_NKCountryCode = "XY";
			AssertEquals(AddressValidationStatus.ToBeVerified, staff.GS_ValidationStatus);
		}

		public void TestAddressValidationLangauage_EqualsWorkingLanguage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkingLanguage = Core.Constants.Languages.Japanese;

			var addressForValidation = staff as ISupportWebAddressValidation;

			AssertEquals(Core.Constants.Languages.Japanese, addressForValidation.Language);
		}

		public void TestChangingAddressResetsValidationStatus()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var address = Factory.NewWithValidTestData<GlbStaff>();
			address.GS_RN_NKCountryCode = "AU";
			address.ValidationStatus = AddressValidationStatus.Verified;

			AssertValidationStatusIsReset(address, () => address.Address1 += "A");
			AssertValidationStatusIsReset(address, () => address.Address2 += "A");
			AssertValidationStatusIsReset(address, () => address.City += "A");
			AssertValidationStatusIsReset(address, () => address.Postcode += "A");
			AssertValidationStatusIsReset(address, () => address.State += "A");
		}

		void AssertValidationStatusIsReset(ISupportWebAddressValidation address, Action action)
		{
			address.ValidationStatus = AddressValidationStatus.Verified;
			action.Invoke();
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestRaiseAddressValidationStatusChanged()
		{
			var address = Factory.NewWithValidTestData<GlbStaff>() as ISupportWebAddressValidation;
			address.ValidationStatus = AddressValidationStatus.ToBeVerified;
			address.Address2 = "";

			address.AddressValidationStatusChanged += address_AddressValidationStatusChanged;
			address.ValidationStatus = AddressValidationStatus.Verified;
			AssertEquals("It happened", address.Addressee);
		}

		void address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			((GlbStaff)sender).GS_FullName = "It happened";
		}

		public void TestValidationStatus()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<GlbStaff>();
			address.GS_RN_NKCountryCode = country.Code;
			address.GS_ValidationStatus = AddressValidationStatus.ToBeVerified;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestState()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<GlbStaff>();
			address.GS_RN_NKCountryCode = country.Code;
			address.GS_State = "NSW";
			AssertEquals("New South Wales", address.State);

			address.State = "Victoria";
			AssertEquals("VIC", address.GS_State);
		}

		public void TestNeedValidation()
		{
			var factory = new BusinessObjectFactory();

			var australia = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			australia.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var china = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			china.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;

			var address = factory.NewWithValidTestData<GlbStaff>();
			address.Address1 = "A1";
			address.Address2 = "A2";
			address.Postcode = "1234";
			address.City = "Syd";
			address.State = "NSW";
			address.GS_RN_NKCountryCode = "CN";
			Assert(address.NeedValidation);

			address.GS_RN_NKCountryCode = "AU";
			Assert(address.NeedValidation);

			factory.Save();
			Assert(address.IsInDatabase);
			Assert(!address.NeedValidation);

			address.Address1 += "A";
			Assert(address.NeedValidation);

			address.Address2 = "";
			Assert(address.NeedValidation);

			address.Address1 = "";
			Assert(!address.NeedValidation);
		}

		public void TestResetAddressMap()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var address = Factory.NewWithValidTestData<GlbStaff>();
			address.GS_RN_NKCountryCode = "AU";
			AssertAddressMap(address, address.GS_UserAddress1Info);
			AssertAddressMap(address, address.GS_UserAddress2Info);
			AssertAddressMap(address, address.GS_CityInfo);
			AssertAddressMap(address, address.GS_PostcodeInfo);
			AssertAddressMap(address, address.GS_StateInfo);
			AssertAddressMap(address, address.GS_RN_NKCountryCodeInfo);

			address.GS_RN_NKCountryCode = "AU";
			var usPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")).PK.ToGuid();
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(usPK, disabledForStaff: true));
			AssertAddressMap(address, address.GS_UserAddress1Info);
			AssertAddressMap(address, address.GS_UserAddress2Info);
			AssertAddressMap(address, address.GS_CityInfo);
			AssertAddressMap(address, address.GS_PostcodeInfo);
			AssertAddressMap(address, address.GS_StateInfo);
			AssertAddressMap(address, address.GS_RN_NKCountryCodeInfo);
		}

		void AssertAddressMap(GlbStaff address, ZPropertyInfo propertyInfo)
		{
			address.AddressMap = "ABCDE";
			propertyInfo.Value = (ZString)(propertyInfo.Name == nameof(GlbStaff.GS_RN_NKCountryCode) ? "US" : (ZString)propertyInfo.Value + "1");
			Assert(string.IsNullOrEmpty(address.AddressMap));
		}

		public void TestValidationSection()
		{
			AssertEquals(AddressValidationSection.Staff, Factory.New<GlbStaff>().ValidationSection);
		}

		#endregion

		#region GS_ValidationStatus

		public void TestValidationStatus_WhenSetToManuallyVerifiedFromOtherValue_ShouldStayAsIsUntilStaffIsReloaded()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			staff.GS_UserAddress1 = "42 FOOBAR STREET";
			staff.GS_UserAddress2 = "FUNPLACE";
			staff.GS_Postcode = "0000";
			staff.GS_City = "WHITERUN";
			staff.GS_State = "TAMRIEL";
			staff.GS_RN_NKCountryCode = "ID";

			// Act.

			staff.GS_ValidationStatus = AddressValidationStatus.ManuallyVerified;

			staff.GS_UserAddress1 = "72 O'RIORDAN STREET";
			staff.GS_UserAddress2 = "WISETECH GLOBAL";
			staff.GS_Postcode = "2015";
			staff.GS_City = "ALEXANDRIA";
			staff.GS_State = "NSW";
			staff.GS_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ManuallyVerified, staff.GS_ValidationStatus);
		}

		public void TestValidationStatus_WhenLoadedAsManuallyVerifiedFromDatabase_ShouldResetValueAfterChangingAddressField()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			staff.GS_UserAddress1 = "42 FOOBAR STREET";
			staff.GS_UserAddress2 = "FUNPLACE";
			staff.GS_Postcode = "0000";
			staff.GS_City = "WHITERUN";
			staff.GS_State = "TAMRIEL";
			staff.GS_RN_NKCountryCode = "ID";

			staff.GS_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();

			var reloadedStaff = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);

			// Act.

			reloadedStaff.GS_UserAddress1 = "72 O'RIORDAN STREET";
			reloadedStaff.GS_UserAddress2 = "WISETECH GLOBAL";
			reloadedStaff.GS_Postcode = "2015";
			reloadedStaff.GS_City = "ALEXANDRIA";
			reloadedStaff.GS_State = "NSW";
			reloadedStaff.GS_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ToBeVerified, reloadedStaff.GS_ValidationStatus);
		}

		#endregion

		#region GeoLocation

		public void TestConstructor_WhenCreatingWithDataRow_ShouldInitializeGeoLocationWithNonNullValue()
		{
			// Arrange.

			// Act.

			var staff = Factory.New<GlbStaff>();

			// Assert.

			var row = ((INeedRow)staff).Row;

			AssertEquals(ZGeography.Empty, row[GlbStaffSchema.Constants.GS_GeoLocation]);
		}

		public void TestGeoLocation_WhenGettingEmptyValue_ShouldSetItToPointZero()
		{
			// Arrange.

			var staff = Factory.New<GlbStaff>();

			// Act.

			staff.GS_GeoLocation = ZGeography.Empty;

			// Assert.

			AssertEquals(ZGeography.Empty, staff.GS_GeoLocation);
		}

		#endregion

		#region Patterns

		public void TestDeleteStaffWillDeletePatternMatchingForStaff()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = person.PK;
			var staffPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, staff);

			Factory.Save();
			staff.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Staff should be deleted", true, staff.IsDeleted);
				AssertEquals("Person should not be deleted", false, person.IsDeleted);

				AssertContainsExactElementsInAnyOrder("Staff patterns should be deleted", Array.Empty<BusinessObject>(), staffPatterns.Where(p => !p.IsDeleted));
			});
		}

		public void TestDeleteStaffWillNotAffectPatternMatchingOrResultsForPerson()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var personPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, person);
			var personResults = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingResults(Factory, person);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = person.PK;

			Factory.Save();
			staff.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Staff should be deleted", true, staff.IsDeleted);
				AssertEquals("Person should not be deleted", false, person.IsDeleted);

				AssertContainsExactElementsInAnyOrder("Person patterns should not be deleted", Array.Empty<BusinessObject>(), personPatterns.Where(p => p.IsDeleted));
				AssertContainsExactElementsInAnyOrder("Person results should not be deleted", Array.Empty<BusinessObject>(), personResults.Where(p => p.IsDeleted));
			});
		}

		#region Certificate Patterns

		public void TestDeleteStaffDeletesCertificateAndAllCertificatesAndPatterns()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var patterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, certificate);

			certificate.XZ_ParentID = staff.PK;
			certificate.XZ_ParentTableCode = staff.TablePrefix;

			Factory.Save();
			staff.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Staff should be deleted", true, staff.IsDeleted);
				AssertEquals("Certificate should also be deleted", true, certificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Staff patterns should all be deleted", Array.Empty<BusinessObject>(), patterns.Where(p => !p.IsDeleted));
			});
		}

		public void TestDeleteStaffCertificatesDeletesAllCertificatesPatterns_ButDoesNotDeleteStaffPatterns()
		{
			var staff = Factory.NewWithValidTestData<GlbPerson>();
			var staffPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, staff);

			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var certificatePatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, certificate);

			certificate.XZ_ParentID = staff.PK;
			certificate.XZ_ParentTableCode = staff.TablePrefix;

			Factory.Save();
			staff.Certificates.DeleteAll();

			CombineAssertions(() =>
			{
				AssertEquals("Staff should not be deleted", false, staff.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Staff patterns should not be deleted", Array.Empty<BusinessObject>(), staffPatterns.Where(p => p.IsDeleted));

				AssertEquals("Certificate should be deleted", true, certificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Certificate patterns should all be deleted", Array.Empty<BusinessObject>(), certificatePatterns.Where(p => !p.IsDeleted));
			});
		}

		#endregion

		#endregion

		#region Campaigns

		public void TestDelete_ShouldDeleteCampaignItem()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var campaign = (BusinessObject)Factory.New<IGlbCompanyCampaign>();
			campaign[GlbCompanyCampaignSchema.G0_CampaignName.Name] = "Test Campaign";
			campaign[GlbCompanyCampaignSchema.G0_Category.Name] = "PRINT";
			campaign[GlbCompanyCampaignSchema.G0_EmailSubject.Name] = "Email Subject";

			var campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = "GS";
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = resource.PK;

			Factory.Save();
			resource.Delete();

			AssertEquals(true, campaignItem.IsDeleted);
		}

		#endregion

		#region Managers

		public void TestDeleteStaffDeletesManagersAndReports()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var otherManager = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord = StaffManagerTestHelper.AddManager(staff, staff, "HRM");
			var otherManagerRecord = StaffManagerTestHelper.AddManager(staff, otherManager, "PRM");
			var directReportRecord = StaffManagerTestHelper.AddManager(otherManager, staff, "TRM");
			Factory.Save();

			staff.Delete();
			AssertEquals(true, managerRecord.IsDeleted);
			AssertEquals(true, otherManagerRecord.IsDeleted);
			AssertEquals(true, directReportRecord.IsDeleted);
		}

		[TestDate(2019, 07, 07)]
		public void TestDeactivateStaffShouldSetManagerEndDate()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var otherManager = Factory.NewWithValidTestData<GlbStaff>();

			var otherManagerRecord = StaffManagerTestHelper.AddManager(staff, otherManager, "PRM");
			var futureOtherManagerRecord = StaffManagerTestHelper.AddManager(staff, otherManager, "HRM", new ZDateTime(2019, 07, 08));
			var directReportRecord = StaffManagerTestHelper.AddManager(otherManager, staff, "TRM");
			Factory.Save();

			staff.GS_IsActive = false;
			AssertEquals("Deactivation should set End date to today", ZDateTime.Today, otherManagerRecord.GSM_EndDate);
			AssertEquals("Deactivation should delete future record", true, futureOtherManagerRecord.IsDeleted);
			AssertEquals("Deactivation should not affect direct reports", ZDateTime.Empty, directReportRecord.GSM_EndDate);
		}

		public void TestDeactivateStaffShouldDeleteNexdocsUserToken()
		{
			var auWrapper = Staff.GetAUWrapper();
			auWrapper.NUTPassword.CurrentDecryptedPassword = "nutPass";
			auWrapper.NUTPassword.GP_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var messages = Factory.Load<IEDIInterchange>(zQuery);

			AssertEquals("1 messages should be created", 1, messages.Length);
			var nexdocsMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"NEXDOCSUserLevel\""));
			AssertNotNull("One NEXDOCSUserLevel should be created", nexdocsMess);
			AssertContains("The NEXDOCSUserLevel should have a company level", "Group Type=\"Company\" Reference=\"EDI\"", nexdocsMess.EI_BodyText);

			nexdocsMess.Delete();
			var nutPassword = auWrapper.NUTPassword;
			AssertEquals("NUTPassword not deleted", false, nutPassword.IsDeleted);

			Staff.GS_IsActive = false;
			Factory.Save();
			AssertEquals("NUTPassword deleted when staff is deactivated", true, nutPassword.IsDeleted);

			messages = Factory.Load<IEDIInterchange>(zQuery);
			AssertEquals("1 messages should be created", 1, messages.Length);
			nexdocsMess = messages.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"NEXDOCSUserLevel\""));
			AssertNotNull("One NEXDOCSUserLevel should be created", nexdocsMess);
			AssertContains("The NEXDOCSUserLevel should have a company level", "Group Type=\"Company\" Reference=\"EDI\"", nexdocsMess.EI_BodyText);
		}

		public void TestGetMissingMandatoryReportingRoles()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "HRM", (NoResString)"Human Resources Manager", true, true, false },
				{ DefaultStaffReportingRoles.Codes.DirectManager, DefaultStaffReportingRoles.Descriptions.DirectManager, true, true, false },
				{ "CRM", (NoResString)"Customer Relationship Manager", true, false, false }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			AssertEquals("Precondition - Should be added by FillWithValidTestData", 2, staff.Managers.Count);
			staff.Managers[1].Delete();
			staff.Managers[0].Delete();
			AssertEquals("Should have deleted manager", 0, staff.Managers.Count);

			var missingRoles = staff.GetMissingMandatoryReportingRoles();
			AssertEquals("Should be missing both mandatory roles", 2, missingRoles.Count);
			AssertEquals("Should be missing HRM", true, missingRoles.ContainsCode("HRM"));
			AssertEquals("Should be missing DRM", true, missingRoles.ContainsCode(DefaultStaffReportingRoles.Codes.DirectManager));

			var managerRecord = StaffManagerTestHelper.AddManager(staff, staff, "HRM");
			missingRoles = staff.GetMissingMandatoryReportingRoles();
			AssertEquals("Should no longer be missing HRM", false, missingRoles.ContainsCode("HRM"));

			managerRecord.GSM_EffectiveDate = ZDateTime.Today.AddDays(2);
			missingRoles = staff.GetMissingMandatoryReportingRoles();
			AssertEquals("Should be missing HRM", true, missingRoles.ContainsCode("HRM"));

			missingRoles = staff.GetMissingMandatoryReportingRoles();
			AssertEquals("Should still be missing DRM", true, missingRoles.ContainsCode(DefaultStaffReportingRoles.Codes.DirectManager));

			_ = StaffManagerTestHelper.AddManager(staff, staff, "DRM");
			missingRoles = staff.GetMissingMandatoryReportingRoles();
			AssertEquals("Should no longer be missing DRM", false, missingRoles.ContainsCode(DefaultStaffReportingRoles.Codes.DirectManager));

			staff.Managers.Last().Delete();
			missingRoles = staff.GetMissingMandatoryReportingRoles();
			AssertEquals("Should be missing DRM again", true, missingRoles.ContainsCode(DefaultStaffReportingRoles.Codes.DirectManager));
		}

		[TestDate(2019, 07, 07)]
		public void TestCurrentDRMManger()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var futureDirectManager = Factory.NewWithValidTestData<GlbStaff>();
			var otherManager = Factory.NewWithValidTestData<GlbStaff>();
			var currentDirectManager = Factory.NewWithValidTestData<GlbStaff>();
			var pastDirectManager = Factory.NewWithValidTestData<GlbStaff>();

			_ = StaffManagerTestHelper.AddManager(staff, futureDirectManager, "DRM", new ZDateTime(2019, 07, 10));
			_ = StaffManagerTestHelper.AddManager(staff, otherManager, "HRM");
			_ = StaffManagerTestHelper.AddManager(staff, currentDirectManager, "DRM", new ZDateTime(2019, 07, 04));
			_ = StaffManagerTestHelper.AddManager(staff, pastDirectManager, "DRM", new ZDateTime(2019, 06, 04));

			Factory.Save();

			AssertEquals(currentDirectManager, staff.CurrentDRMManager);
		}

		#endregion

		public void TestDeleteStaffDeletesDictionary()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var dictionary = WordDictionary.GetOrAdd(Factory, staff.GS_Code);
			dictionary.AddWord("SomethingToMakeItSave");

			Factory.Save();

			staff.Delete();

			Assert("When a GlbStaff is deleted it should delete its dictionary too", dictionary.IsDeleted);
		}

		public void TestDeleteStaffWillDeletePatternMatchingForStaffAndNotAffectPerson()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = person.PK;

			var staffPatternMatchingAddress = Factory.NewWithValidTestData<PatternMatchingAddress>();
			staffPatternMatchingAddress.PMA_ParentId = staff.PK;
			staffPatternMatchingAddress.PMA_ParentTableCode = staff.TablePrefix;
			staffPatternMatchingAddress.PMA_PER = staff.GS_PER;
			var staffPatternMatchingEmail = Factory.NewWithValidTestData<PatternMatchingEmail>();
			staffPatternMatchingEmail.PME_ParentId = staff.PK;
			staffPatternMatchingEmail.PME_ParentTableCode = staff.TablePrefix;
			staffPatternMatchingEmail.PME_PER = staff.GS_PER;
			var staffPatternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			staffPatternMatchingName.PMN_ParentId = staff.PK;
			staffPatternMatchingName.PMN_ParentTableCode = staff.TablePrefix;
			staffPatternMatchingName.PMN_PER = staff.GS_PER;
			var staffPatternMatchingPhone = Factory.NewWithValidTestData<PatternMatchingPhone>();
			staffPatternMatchingPhone.PMP_ParentId = staff.PK;
			staffPatternMatchingPhone.PMP_ParentTableCode = staff.TablePrefix;
			staffPatternMatchingPhone.PMP_PER = staff.GS_PER;
			var staffPatternMatchingRegCode = Factory.NewWithValidTestData<PatternMatchingRegCode>();
			staffPatternMatchingRegCode.PMR_ParentId = staff.PK;
			staffPatternMatchingRegCode.PMR_ParentTableCode = staff.TablePrefix;
			staffPatternMatchingRegCode.PMR_PER = staff.GS_PER;

			var personPatternMatchingAddress = Factory.NewWithValidTestData<PatternMatchingAddress>();
			personPatternMatchingAddress.PMA_PER = person.PK;
			personPatternMatchingAddress.PMA_ParentTableCode = person.TablePrefix;
			personPatternMatchingAddress.PMA_ParentId = person.PK;
			var personPatternMatchingEmail = Factory.NewWithValidTestData<PatternMatchingEmail>();
			personPatternMatchingEmail.PME_PER = person.PK;
			personPatternMatchingEmail.PME_ParentTableCode = person.TablePrefix;
			personPatternMatchingEmail.PME_ParentId = person.PK;
			var personPatternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			personPatternMatchingName.PMN_PER = person.PK;
			personPatternMatchingName.PMN_ParentTableCode = person.TablePrefix;
			personPatternMatchingName.PMN_ParentId = person.PK;
			var personPatternMatchingPhone = Factory.NewWithValidTestData<PatternMatchingPhone>();
			personPatternMatchingPhone.PMP_PER = person.PK;
			personPatternMatchingPhone.PMP_ParentTableCode = person.TablePrefix;
			personPatternMatchingPhone.PMP_ParentId = person.PK;
			var personPatternMatchingRegCode = Factory.NewWithValidTestData<PatternMatchingRegCode>();
			personPatternMatchingRegCode.PMR_PER = person.PK;
			personPatternMatchingRegCode.PMR_ParentTableCode = person.TablePrefix;
			personPatternMatchingRegCode.PMR_ParentId = person.PK;

			var patternMatchingResultTk = Factory.NewWithValidTestData<PatternMatchingResult>();
			patternMatchingResultTk.PMT_TargetPK = person.PK;
			patternMatchingResultTk.PMT_TargetTableCode = person.TablePrefix;
			patternMatchingResultTk.PMT_MasterTableCode = person.TablePrefix;
			patternMatchingResultTk.PMT_FoundTimeUtc = ZDateTime.Now;
			patternMatchingResultTk.PMT_GS_NKExcludeBy = "NA";
			patternMatchingResultTk.PMT_Status = "PIG";

			var patternMatchingResultMk = Factory.NewWithValidTestData<PatternMatchingResult>();
			patternMatchingResultMk.PMT_MasterPK = person.PK;
			patternMatchingResultMk.PMT_Status = "NDU";
			patternMatchingResultMk.PMT_FoundTimeUtc = ZDateTime.Now;
			patternMatchingResultMk.PMT_GS_NKExcludeBy = "NA";
			patternMatchingResultMk.PMT_MasterTableCode = person.TablePrefix;

			var patternMatchingResultMkTK = Factory.NewWithValidTestData<PatternMatchingResult>();
			patternMatchingResultMkTK.PMT_MasterPK = person.PK;
			patternMatchingResultMkTK.PMT_TargetPK = person.PK;
			patternMatchingResultMkTK.PMT_MasterTableCode = person.TablePrefix;
			patternMatchingResultMkTK.PMT_TargetTableCode = person.TablePrefix;
			patternMatchingResultMkTK.PMT_FoundTimeUtc = ZDateTime.Now;
			patternMatchingResultMkTK.PMT_Status = "PIG";
			patternMatchingResultMkTK.PMT_GS_NKExcludeBy = "NA";

			Factory.Save();
			staff.Delete();

			Assert(staffPatternMatchingAddress.IsDeleted);
			Assert(staffPatternMatchingEmail.IsDeleted);
			Assert(staffPatternMatchingName.IsDeleted);
			Assert(staffPatternMatchingPhone.IsDeleted);
			Assert(staffPatternMatchingRegCode.IsDeleted);
			AssertEquals(false, patternMatchingResultTk.IsDeleted);
			AssertEquals(false, patternMatchingResultMk.IsDeleted);
			AssertEquals(false, patternMatchingResultMkTK.IsDeleted);
		}

		#region EmailAddresses

		public void TestEmailAddresses_MainEmailAddressIsNotSavedIntoDB()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@";

			AssertEquals("There is one record (Main Type) in the collection", 1, staff.EmailAddresses.Count);
			AssertEquals("test@", staff.EmailAddresses[0].GSE_EmailAddress);
			AssertEquals(EmailContactItemDescriptionList.Descriptions.Main, staff.EmailAddresses[0].EmailType);
			AssertEquals(staff.PK, staff.EmailAddresses[0].GSE_GS);
			Assert(!staff.EmailAddresses[0].CanDelete);

			staff.EmailAddresses[0].GSE_EmailAddress = "test@test.com";

			Factory.Save();

			AssertEquals("There is still one record (Main Type) in the collection", 1, staff.EmailAddresses.Count);
			AssertEquals("The record is not in the database", false, staff.EmailAddresses[0].IsInDatabase);
			AssertNull(new BusinessObjectFactory().Load<GlbStaffEmailAddress>(staff.EmailAddresses[0].PK));

			var newStaff = Factory.Load<GlbStaff>(staff.PK);

			AssertEquals("test@test.com", newStaff.GS_EmailAddress);
		}

		public void TestEmailAddresses_MainEmailAddressIsUpdatedAcrossFactories()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "test@test.com";
			Factory.Save();

			AssertEquals("test@test.com", staff1.EmailAddresses[0].GSE_EmailAddress);

			var staff2 = new BusinessObjectFactory().Load<GlbStaff>(staff1.PK);

			AssertEquals("test@test.com", staff2.EmailAddresses[0].GSE_EmailAddress);

			staff1.GS_EmailAddress = "change@test.com";

			AssertEquals("change@test.com", staff1.EmailAddresses[0].GSE_EmailAddress);
			AssertEquals("test@test.com", staff2.EmailAddresses[0].GSE_EmailAddress);

			Factory.Save();

			AssertEquals("change@test.com", staff2.EmailAddresses[0].GSE_EmailAddress);
		}

		#endregion

		public void TestMaskPhoneNumberForGlbStaffTable()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffWithoutEmergencyContactPermission = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutEmergencyContactPermission.GS_IsController = false;

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewEmergencyContact.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutEmergencyContactPermission.PK;
			staffWithoutEmergencyContactPermission.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithEmergencyContactPermission = Factory.NewWithValidTestData<GlbStaff>();
			staffWithEmergencyContactPermission.GS_IsController = false;

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewEmergencyContact.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithEmergencyContactPermission.PK;
			staffWithEmergencyContactPermission.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			var staffWithOtherReferencesPermission = Factory.NewWithValidTestData<GlbStaff>();
			staffWithOtherReferencesPermission.GS_IsController = false;

			var allowedOtherReferencesSecurityRecord = Factory.New<GlbSecurity>();
			allowedOtherReferencesSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherReferences.Code;
			allowedOtherReferencesSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedOtherReferencesSecurityRecord.GU_GS = staffWithOtherReferencesPermission.PK;
			staffWithOtherReferencesPermission.GroupSecurityPermissionsCollectionForBinding.Add(allowedOtherReferencesSecurityRecord);

			var staffWithoutOtherReferencesPermission = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutOtherReferencesPermission.GS_IsController = false;

			var deniedOtherReferencesSecurityRecord = Factory.New<GlbSecurity>();
			deniedOtherReferencesSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherReferences.Code;
			deniedOtherReferencesSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedOtherReferencesSecurityRecord.GU_GS = staffWithEmergencyContactPermission.PK;
			staffWithoutOtherReferencesPermission.GroupSecurityPermissionsCollectionForBinding.Add(deniedOtherReferencesSecurityRecord);

			var staffWithHomeAddressPermission = Factory.NewWithValidTestData<GlbStaff>();
			staffWithHomeAddressPermission.GS_IsController = false;

			var allowedHomeAdressSecurityRecord = Factory.New<GlbSecurity>();
			allowedHomeAdressSecurityRecord.GU_SecurityRight = Env.Security.StaffViewHomeAddressDetails.Code;
			allowedHomeAdressSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedHomeAdressSecurityRecord.GU_GS = staffWithHomeAddressPermission.PK;
			staffWithHomeAddressPermission.GroupSecurityPermissionsCollectionForBinding.Add(allowedHomeAdressSecurityRecord);

			var staffWithoutHomeAddressPermission = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutHomeAddressPermission.GS_IsController = false;

			var deniedHomeAdressSecurityRecord = Factory.New<GlbSecurity>();
			deniedHomeAdressSecurityRecord.GU_SecurityRight = Env.Security.StaffViewHomeAddressDetails.Code;
			deniedHomeAdressSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedHomeAdressSecurityRecord.GU_GS = staffWithoutHomeAddressPermission.PK;
			staffWithoutHomeAddressPermission.GroupSecurityPermissionsCollectionForBinding.Add(deniedHomeAdressSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutEmergencyContactPermission.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithoutEmergencyContactPermission.PK, Env.CurrentUser.PK);

					var logWorkPhone = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Work Phone: +61 421 742 999 => +61421742999");
					logWorkPhone.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logWorkPhone.LockForUpdatingKeyFieldsForTesting())
					{
						logWorkPhone.Master = staff;

						AssertEquals("DisplayEventReference Without Work Phone Number Masked", "Work Phone: +61 421 742 999 => +61421742999", logWorkPhone.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding Without Work Phone Number Masked", "Work Phone: +61 421 742 999 => +61421742999", logWorkPhone.SL_ReferenceForBinding);
					}

					var logMobilePhone = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Mobile Phone: +61 421 742 999 => +61421742999");
					logMobilePhone.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logMobilePhone.LockForUpdatingKeyFieldsForTesting())
					{
						logMobilePhone.Master = staff;

						AssertEquals("DisplayEventReference Without Mobile Phone Number Masked", "Mobile Phone: +61 421 742 999 => +61421742999", logMobilePhone.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding Without Mobile Phone Number Masked", "Mobile Phone: +61 421 742 999 => +61421742999", logMobilePhone.SL_ReferenceForBinding);
					}

					var logFax = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Fax: +61 421 742 999 => +61421742999");
					logFax.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logFax.LockForUpdatingKeyFieldsForTesting())
					{
						logFax.Master = staff;

						AssertEquals("DisplayEventReference Without Fax Number Masked", "Fax: +61 421 742 999 => +61421742999", logFax.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding Without Fax Number Masked", "Fax: +61 421 742 999 => +61421742999", logFax.SL_ReferenceForBinding);
					}

					var logEmergencyHomePhoneNoRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Emergency Home Phone: +61 421 742 999 => +61421742999");
					logEmergencyHomePhoneNoRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logEmergencyHomePhoneNoRight.LockForUpdatingKeyFieldsForTesting())
					{
						logEmergencyHomePhoneNoRight.Master = staff;

						AssertEquals("DisplayEventReference With Emergency Home Phone Number Masked", "Emergency Home Phone: +61 421 74X XXX => +6142174XXXX", logEmergencyHomePhoneNoRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding With Emergency Home Phone Number Masked", "Emergency Home Phone: +61 421 74X XXX => +6142174XXXX", logEmergencyHomePhoneNoRight.SL_ReferenceForBinding);
					}

					var logEmergencyWorkPhoneNoRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Emergency Work Phone: +61-421-742-999 => +61421742999");
					logEmergencyWorkPhoneNoRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logEmergencyWorkPhoneNoRight.LockForUpdatingKeyFieldsForTesting())
					{
						logEmergencyWorkPhoneNoRight.Master = staff;

						AssertEquals("DisplayEventReference With Emergency Work Phone Number Masked", "Emergency Work Phone: +61-421-74X-XXX => +6142174XXXX", logEmergencyWorkPhoneNoRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding With Emergency Work Phone Number Masked", "Emergency Work Phone: +61-421-74X-XXX => +6142174XXXX", logEmergencyWorkPhoneNoRight.SL_ReferenceForBinding);
					}

					var logNextOfKinWorkPhoneNoRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("NextOfKin Work Phone: +61 421 742 9991 => +614217429991");
					logNextOfKinWorkPhoneNoRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logNextOfKinWorkPhoneNoRight.LockForUpdatingKeyFieldsForTesting())
					{
						logNextOfKinWorkPhoneNoRight.Master = staff;

						AssertEquals("DisplayEventReference With NextOfKin Work Phone Number Masked", "NextOfKin Work Phone: +61 421 742 XXXX => +61421742XXXX", logNextOfKinWorkPhoneNoRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding With NextOfKin Work Phone Number Masked", "NextOfKin Work Phone: +61 421 742 XXXX => +61421742XXXX", logNextOfKinWorkPhoneNoRight.SL_ReferenceForBinding);
					}

					var logNextOfKinHomePhoneNoRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("NextOfKin Home Phone: +61 421 742 999 => +61421742999");
					logNextOfKinHomePhoneNoRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logNextOfKinHomePhoneNoRight.LockForUpdatingKeyFieldsForTesting())
					{
						logNextOfKinHomePhoneNoRight.Master = staff;

						AssertEquals("DisplayEventReference With NextOfKin Home Phone Number Masked", "NextOfKin Home Phone: +61 421 74X XXX => +6142174XXXX", logNextOfKinHomePhoneNoRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding With NextOfKin Home Phone Number Masked", "NextOfKin Home Phone: +61 421 74X XXX => +6142174XXXX", logNextOfKinHomePhoneNoRight.SL_ReferenceForBinding);
					}
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithEmergencyContactPermission.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithEmergencyContactPermission.PK, Env.CurrentUser.PK);

					var logEmergencyHomePhoneWithRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Emergency Home Phone: +61 421 742 999 => +61421742999");
					logEmergencyHomePhoneWithRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logEmergencyHomePhoneWithRight.LockForUpdatingKeyFieldsForTesting())
					{
						logEmergencyHomePhoneWithRight.Master = staff;

						AssertEquals("DisplayEventReference Without Emergency Home Phone Number Masked", "Emergency Home Phone: +61 421 742 999 => +61421742999", logEmergencyHomePhoneWithRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding Without Emergency Home Phone Number Masked", "Emergency Home Phone: +61 421 742 999 => +61421742999", logEmergencyHomePhoneWithRight.SL_ReferenceForBinding);
					}

					var logEmergencyWorkPhoneWithRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Emergency Work Phone: +61 421 742 999 => +61421742999");
					logEmergencyWorkPhoneWithRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logEmergencyWorkPhoneWithRight.LockForUpdatingKeyFieldsForTesting())
					{
						logEmergencyWorkPhoneWithRight.Master = staff;

						AssertEquals("DisplayEventReference Without Emergency Work Phone Number Masked", "Emergency Work Phone: +61 421 742 999 => +61421742999", logEmergencyWorkPhoneWithRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding Without Emergency Work Phone Number Masked", "Emergency Work Phone: +61 421 742 999 => +61421742999", logEmergencyWorkPhoneWithRight.SL_ReferenceForBinding);
					}

					var logNextOfKinWorkPhoneWithRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("NextOfKin Work Phone: +61 421 742 999 => +61421742999");
					logNextOfKinWorkPhoneWithRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logNextOfKinWorkPhoneWithRight.LockForUpdatingKeyFieldsForTesting())
					{
						logNextOfKinWorkPhoneWithRight.Master = staff;

						AssertEquals("DisplayEventReference Without NextOfKin Work Phone Number Masked", "NextOfKin Work Phone: +61 421 742 999 => +61421742999", logNextOfKinWorkPhoneWithRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding Without NextOfKin Work Phone Number Masked", "NextOfKin Work Phone: +61 421 742 999 => +61421742999", logNextOfKinWorkPhoneWithRight.SL_ReferenceForBinding);
					}

					var logNextOfKinHomePhoneWithRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("NextOfKin Home Phone: +61 421 742 999 => +61421742999");
					logNextOfKinHomePhoneWithRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logNextOfKinHomePhoneWithRight.LockForUpdatingKeyFieldsForTesting())
					{
						logNextOfKinHomePhoneWithRight.Master = staff;

						AssertEquals("DisplayEventReference Without NextOfKin Home Phone Number Masked", "NextOfKin Home Phone: +61 421 742 999 => +61421742999", logNextOfKinHomePhoneWithRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding Without NextOfKin Home Phone Number Masked", "NextOfKin Home Phone: +61 421 742 999 => +61421742999", logNextOfKinHomePhoneWithRight.SL_ReferenceForBinding);
					}
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithOtherReferencesPermission.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithOtherReferencesPermission.PK, Env.CurrentUser.PK);

					var logPagerWithRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Pager: +02 421 742 999 => +02421742999");
					logPagerWithRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logPagerWithRight.LockForUpdatingKeyFieldsForTesting())
					{
						logPagerWithRight.Master = staff;

						AssertEquals("DisplayEventReference Without Pager Number Masked", "Pager: +02 421 742 999 => +02421742999", logPagerWithRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding Without Pager Number Masked", "Pager: +02 421 742 999 => +02421742999", logPagerWithRight.SL_ReferenceForBinding);
					}
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutOtherReferencesPermission.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithoutOtherReferencesPermission.PK, Env.CurrentUser.PK);

					var logPagerNoRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Pager: +02 421 742 999 => +02421742999");
					logPagerNoRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logPagerNoRight.LockForUpdatingKeyFieldsForTesting())
					{
						logPagerNoRight.Master = staff;

						AssertEquals("DisplayEventReference With Pager Number Masked", "Pager: +02 421 74X XXX => +0242174XXXX", logPagerNoRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding With Pager Number Masked", "Pager: +02 421 74X XXX => +0242174XXXX", logPagerNoRight.SL_ReferenceForBinding);
					}
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithHomeAddressPermission.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithHomeAddressPermission.PK, Env.CurrentUser.PK);

					var logHomePhoneWithRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Home Phone: +02 421 742 999 => +02421742999");
					logHomePhoneWithRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logHomePhoneWithRight.LockForUpdatingKeyFieldsForTesting())
					{
						logHomePhoneWithRight.Master = staff;

						AssertEquals("DisplayEventReference Without Home Number Masked", "Home Phone: +02 421 742 999 => +02421742999", logHomePhoneWithRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding Without Home Number Masked", "Home Phone: +02 421 742 999 => +02421742999", logHomePhoneWithRight.SL_ReferenceForBinding);
					}
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutHomeAddressPermission.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffWithoutHomeAddressPermission.PK, Env.CurrentUser.PK);

					var logHomePhoneNoRight = Factory.New<StmALog>().SetReferenceFreeTextWithLock("Home Phone: +02 421 742 999 => +02421742999");
					logHomePhoneNoRight.SetEventCodeWithLock(AutoEvents.EditedARecord);

					using (logHomePhoneNoRight.LockForUpdatingKeyFieldsForTesting())
					{
						logHomePhoneNoRight.Master = staff;

						AssertEquals("DisplayEventReference With Home Number Masked", "Home Phone: +02 421 74X XXX => +0242174XXXX", logHomePhoneNoRight.DisplayEventReference);
						AssertEquals("SL_ReferenceForBinding With Home Number Masked", "Home Phone: +02 421 74X XXX => +0242174XXXX", logHomePhoneNoRight.SL_ReferenceForBinding);
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestStaffGroupsCountChangedShouldBeSuspendedBeforeLogin()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			using (Env.SetTemporaryUserContext(null))
			{
				var groups = staff.Groups;
			}
		}

		public void TestVerifyPassword()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			UserSecretsContext.DefaultContext.SaveSecret("pswrd", UserSecretHashAlgorithm.Pbkdf2HmacSha1, 10, staff.GetPasswordAdapter());
			AssertEquals(true, staff.VerifyPassword("pswrd"));

			staff.GS_PasswordHashIterations = 100;
			AssertEquals(false, staff.VerifyPassword("pswrd"));

			staff.GS_PasswordHash = ZBlob.Empty;
			AssertEquals(false, staff.VerifyPassword("pswrd"));

			AssertEquals(false, staff.VerifyPassword(null));
		}

		public void TestVerifyPassword_LocalPasswordMustBeReset()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.LocalPasswordMustBeReset = true;
			AssertEquals("System account empty password should not throw and return false", false, staff.VerifyPassword("pswrd"));
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestVerifyPassword_CorruptedPassword()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			UserSecretsContext.DefaultContext.SaveSecret("pswrd", UserSecretHashAlgorithm.Pbkdf2HmacSha1, 10, staff.GetPasswordAdapter());
			staff.GS_PasswordSalt = null;
			AssertEquals("Corrupted password should not throw and return false", false, staff.VerifyPassword("pswrd"));

			AssertStartsWith("Last Report Start With", "The staff password is corrupted, please investigate why this happen", ErrorReporter.LastMessageReported);
			AssertContains("GS_PasswordSalt:", ErrorReporter.LastMessageReported);
			AssertContains($"GS_PK: {staff.PK}", ErrorReporter.LastMessageReported);
			Assert(ErrorReporter.LastExceptionReported is ArgumentException);
			AssertContains("Salt is not at least eight bytes.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestVerifyPassword_WhenChangeFromDifferentInstance()
		{
			var oldPassword = "oldPa$$word";
			var newPassword = "newP@ssword";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "user";
			staff.StaffPlainTextPassword = oldPassword;
			Factory.Save();

			using (RowFactory.SetCachedTables(GlbStaff.Schema.TableName))
			{
				// Before password change and to load to the cache
				AssertEquals("Old password", true, staff.VerifyPassword(oldPassword));
				var staffReloadedFromNewFactory = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
				AssertEquals("Old password from another factory before change", true, staffReloadedFromNewFactory.VerifyPassword(oldPassword));

				// password change from another instance/Glow/direct sql
				var iterationCount = 10;
				var (_, newPasswordHash, newSalt, _) = UserSecretsContext.DefaultContext.DeriveRawComponents(newPassword, UserSecretHashAlgorithm.Pbkdf2HmacSha1, iterationCount);
				var query = $@"UPDATE dbo.GlbStaff SET GS_PasswordHash = @passwordHash, GS_PasswordSalt = @passwordSalt, GS_PasswordHashIterations = @iterationCount, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_PK = '{staff.PK}'";
				using (var cmd = TestConnection.Command(query))
				{
					cmd.AddParameter("@passwordHash", System.Data.SqlDbType.VarBinary, newPasswordHash);
					cmd.AddParameter("@passwordSalt", System.Data.SqlDbType.VarBinary, newSalt);
					cmd.AddParameter("@iterationCount", System.Data.SqlDbType.Int, iterationCount);
					cmd.ExecuteNonQuery();
				}

				staffReloadedFromNewFactory = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
				AssertEquals("New password from another factory", true, staffReloadedFromNewFactory.VerifyPassword(newPassword));
				AssertEquals("Old password from another factory", false, staffReloadedFromNewFactory.VerifyPassword(oldPassword));

				AssertEquals("New password", true, staff.VerifyPassword(newPassword));
				AssertEquals("Old password", false, staff.VerifyPassword(oldPassword));
			}
		}

		public void TestHasNonPermittedDuplicateEmail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "firstUser@wisetechglobal.com";

			AssertEquals("No other staff", false, ((IExamUrlRecipient)staff).HasNonPermittedDuplicateEmail);
			Factory.Save();

			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_EmailAddress = "firstUser@wisetechglobal.com";
			AssertEquals("Duplicate email", true, ((IExamUrlRecipient)otherStaff).HasNonPermittedDuplicateEmail);
		}

		public void TestHasNonPermittedDuplicateEmailUnrelatedLearningCenterApplicant()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			contact.OC_PER = person.PK;
			var applicant = (IHRJobApplicant)person.ApplicantCollection.AddNew();
			applicant.HA_PER = ZGuid.Empty;
			applicant.HA_FullName = "name";
			applicant.UpdateEmailAddress("firstUser@wisetechglobal.com");

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = applicant.PK;
				log.SL_Table = HRJobApplicantSchema.Constants.TableName;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = AutoEvents.EditedARecord.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_Reference = "Related Contact: Confire (blabla) | ID: " + contact.PK;
			}

			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "firstUser@wisetechglobal.com";

			AssertEquals("Precondition: Same email address", staff.GS_EmailAddress, applicant.HA_EmailAddress);
			AssertNotEquals("Precondition: Different person", staff.GS_PER, applicant.HA_PER);
			AssertEquals("Duplicate email - Non related learning center applicant has matching email", true, ((IExamUrlRecipient)staff).HasNonPermittedDuplicateEmail);
			Factory.Save();
		}

		public void TestHasNonPermittedDuplicateEmailNonLearningCenterApplicant()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = (IHRJobApplicant)person.ApplicantCollection.AddNew();
			applicant.HA_PER = person.PK;
			applicant.UpdateEmailAddress("firstUser@wisetechglobal.com");

			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "firstUser@wisetechglobal.com";
			staff.GS_PER = person.PK;

			AssertEquals("Precondition: Same email address", staff.GS_EmailAddress, applicant.HA_EmailAddress);
			AssertEquals("Precondition: Same person", staff.GS_PER, applicant.HA_PER);
			AssertEquals("Precondition: Job applicant (non learning center)", false, applicant.IsLearningCenterUser);
			AssertEquals("Duplicate email - Non learning center applicant on same person has matching email", true, ((IExamUrlRecipient)staff).HasNonPermittedDuplicateEmail);
			Factory.Save();
		}

		public void TestShouldAddEmploymentCommencedLogOnSaving()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAR";
			staff.GS_EmploymentDate = new ZDateTime(2019, 01, 01);
			Factory.Save();
			AssertEquals("Precondition", "AAR", staff.GS_Code);
			AssertEquals("Precondition", new ZDateTime(2019, 01, 01), staff.GS_EmploymentDate);

			var employmentCommencedLogs = staff.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.EmploymentCommencedCode).ToArray();
			AssertEquals("Only 1 log should be created", 1, employmentCommencedLogs.Length);
			var commencedLog = employmentCommencedLogs[0];
			AssertEmploymentCommencedLog(commencedLog, staff.GS_EmploymentDate, FormattableString.Invariant($"{staff.GS_Code} - Employment Commenced"));
		}

		public void TestShouldAddCertificateReceivedOnFactorySaving()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var cert1 = staff.Certificates.AddNew();
			cert1.XZ_Type = "BRK";

			var cert2 = staff.Certificates.AddNew();
			cert2.XZ_Type = "CO1";

			var cert3 = staff.Certificates.AddNew();
			cert3.XZ_Type = "CO2";

			var cert4 = staff.Certificates.AddNew();
			cert4.XZ_Type = "CO3";

			var cert5 = staff.Certificates.AddNew();
			cert5.XZ_Type = "CS1";

			var cert6 = staff.Certificates.AddNew();
			cert6.XZ_Type = "CS2";

			var cert7 = staff.Certificates.AddNew();
			cert7.XZ_Type = "CM1";

			Factory.Save();

			var certificateReceivedLogs = staff.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.CertificateReceivedCode).OrderBy(log => log.SL_Reference).ToArray();
			AssertEquals(7, certificateReceivedLogs.Length);
			AssertEquals("BRK", "|NAM=Broker", certificateReceivedLogs[0].SL_Reference);
			AssertEquals("CM1", "|NAM=UK Cargo Manager|TYP=Security Training", certificateReceivedLogs[1].SL_Reference);
			AssertEquals("CO3", "|NAM=UK Cargo Operative Screening - Refresher|TYP=Security Training", certificateReceivedLogs[2].SL_Reference);
			AssertEquals("CO2", "|NAM=UK Cargo Operative Screening|TYP=Security Training", certificateReceivedLogs[3].SL_Reference);
			AssertEquals("CO1", "|NAM=UK Cargo Operative|TYP=Security Training", certificateReceivedLogs[4].SL_Reference);
			AssertEquals("CS2", "|NAM=UK Cargo Supervisor - Refresher|TYP=Security Training", certificateReceivedLogs[5].SL_Reference);
			AssertEquals("CS1", "|NAM=UK Cargo Supervisor|TYP=Security Training", certificateReceivedLogs[6].SL_Reference);

			var cert8 = staff.Certificates.AddNew();
			cert8.XZ_Type = "CDN";

			staff.GS_SystemLastEditTimeUtc = ZDateTime.UtcNow; //in real application run, this will cause the row to change and OnSaving to run

			Factory.Save();

			certificateReceivedLogs = staff.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.CertificateReceivedCode).ToArray();
			AssertEquals(8, certificateReceivedLogs.Length);
			AssertEquals("|NAM=Citizenship document number", certificateReceivedLogs[7].SL_Reference);
		}

		public void TestShouldCancelAndCreateEmploymentCommencedLogEventTimeOnSavingIfExists()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAR";
			var originalEmploymentDate = new ZDateTime(2019, 01, 01);
			staff.GS_EmploymentDate = originalEmploymentDate;
			Factory.Save();
			AssertEquals("Precondition", "AAR", staff.GS_Code);
			AssertEquals("Precondition", originalEmploymentDate, staff.GS_EmploymentDate);

			var employmentCommencedLogs = staff.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.EmploymentCommencedCode).ToArray();
			AssertEquals("Precondition: Only 1 log should be created", 1, employmentCommencedLogs.Length);
			var commencedLog = employmentCommencedLogs[0];
			AssertEmploymentCommencedLog(commencedLog, staff.GS_EmploymentDate, FormattableString.Invariant($"{staff.GS_Code} - Employment Commenced"));

			staff.GS_EmploymentDate = new ZDateTime(2019, 04, 05);
			Factory.Save();
			AssertEquals("Precondition", new ZDateTime(2019, 04, 05), staff.GS_EmploymentDate);

			var reloadedEmploymentCommencedLogs = staff.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.EmploymentCommencedCode).ToArray();
			AssertEquals("A new log should have been created", 2, reloadedEmploymentCommencedLogs.Length);
			var reloadedCommencedLog = reloadedEmploymentCommencedLogs.FirstOrDefault(x => x.SL_IsCancelled);

			AssertEquals("Should cancel the existing log", commencedLog.PK, reloadedCommencedLog.PK);
			AssertEmploymentCommencedLog(reloadedCommencedLog, originalEmploymentDate, FormattableString.Invariant($"{staff.GS_Code} - Employment Commenced"));
			var newLog = reloadedEmploymentCommencedLogs.FirstOrDefault(x => !x.SL_IsCancelled);
			AssertEmploymentCommencedLog(newLog, staff.GS_EmploymentDate, FormattableString.Invariant($"{staff.GS_Code} - Employment Commenced"));
		}

		void AssertEmploymentCommencedLog(StmALog commencedLog, ZDateTime employmentDate, ZString reference)
		{
			AssertEquals("Event time should match employment date", employmentDate, commencedLog.SL_EventTime);
			AssertEquals("Reference", reference, commencedLog.SL_Reference);
			AssertEquals("Should be deferred", true, commencedLog.SL_FireWorkflow);
		}

		[UseSnapshotProtection]
		public void TestSQLUserName()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with tests
			// TestSqlUserNameADIntegrationDisabledWiseCloudHosted,
			// TestSqlUserNameADIntegrationDisabledSelfHosted,
			// TestSQLUserNameAdIntegrationEnabledHostedInWiseCloud
			// and TestSQLUserNameAdIntegrationEnabledSelfHosted inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "UserName001";
			staff.IsDatabaseDeveloper = true;
			Factory.Save();

			AssertEquals($"EnterpriseDbUser_{Db.DatabaseName}_UserName001", staff.SQLUserName);
		}

		[UseSnapshotProtection]
		public void TestSQLUserName_NotExist()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with tests
			// TestLoginNameIsNullForStaffMembersWitNoDatabaseAccess inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "UserName002";
			Factory.Save();

			AssertEquals(null, staff.SQLUserName);
		}

		[UseSnapshotProtection]
		public void TestSQLUserName_AD()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with tests
			// TestSqlUserNameADIntegrationDisabledWiseCloudHosted,
			// TestSqlUserNameADIntegrationDisabledSelfHosted,
			// TestSQLUserNameAdIntegrationEnabledHostedInWiseCloud
			// and TestSQLUserNameAdIntegrationEnabledSelfHosted inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var currentUserName = TestConstants.ADTestAdminAccount.Name;
			var domainName = TestConstants.DomainPreWin2000;

			var sqlUsername = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{currentUserName}";
			var windowsUsername = "SAND\\ADTest_Admin";

			try
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				DbUserManagerTest.DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);

				var factory = new BusinessObjectFactory(TestConnection);
				factory.Save();

				DbUserManagerTest.SetDomainCredentialCollection();

				var staff = factory.New<GlbStaff>();
				staff.GS_LoginName = currentUserName;
				staff.DomainName = domainName;
				staff.IsDatabaseDeveloper = true;
				staff.IsBackupOperator = true;
				staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

				EnvProxy.SetHostedLocationForTest("NCW");
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
				factory.Save();
				AssertEquals("Self-hosted. AD Integration disabled, Staff not linked to AD:", sqlUsername, staff.SQLUserName);
				DbUserManagerTest.DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);

				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				staff.IsDatabaseDeveloper = !staff.IsDatabaseDeveloper;
				factory.Save();
				AssertEquals("Self-hosted, AD Integration enabled, Staff linked to AD:", windowsUsername, staff.SQLUserName);
				DbUserManagerTest.DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);

				EnvProxy.SetHostedLocationForTest("SYD");
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
				staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();
				staff.IsDatabaseDeveloper = !staff.IsDatabaseDeveloper;
				factory.Save();
				AssertEquals("Hosted. AD Integration disabled, Staff not linked to AD:", sqlUsername, staff.SQLUserName);
				DbUserManagerTest.DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);

				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				staff.IsDatabaseDeveloper = !staff.IsDatabaseDeveloper;
				factory.Save();
				AssertEquals("Hosted, AD Integration Enabled, Staff linked to AD:", sqlUsername, staff.SQLUserName);
			}
			finally
			{
				DbUserManagerTest.DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);
			}
		}

		public void TestFeatureEnabledForUser()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var group = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);
			var featureTest = factory.NewWithValidTestData<StmFeatureTest>();
			featureTest.SFT_GG_Group = group.PK;
			featureTest.SFT_FeatureName = Guid.NewGuid().ToString();
			factory.Save();

			var result = RunCheckFeatureEnabledForUserForTest(staff, group, featureTest);

			Assert(result);
		}

		public void TestFeatureEnabledForUserFalseWhenDisabledInRegistry()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = false;
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var group = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);
			var featureTest = factory.NewWithValidTestData<StmFeatureTest>();
			featureTest.SFT_GG_Group = group.PK;
			featureTest.SFT_FeatureName = Guid.NewGuid().ToString();
			factory.Save();

			var result = RunCheckFeatureEnabledForUserForTest(staff, group, featureTest);

			Assert(!result);
		}

		public void TestWinzorEnabledForUser()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var group = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);
			var featureTest = factory.NewWithValidTestData<StmFeatureTest>();
			featureTest.SFT_GG_Group = group.PK;
			featureTest.SFT_FeatureName = StmFeatureTest.WinzorFeatureCode;
			factory.Save();

			var result = RunCheckFeatureEnabledForUserForTest(staff, group, featureTest);

			Assert(result);
		}

		public void TestFeatureDisabledForUser()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var group = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);
			factory.Save();
			var result = RunCheckFeatureEnabledForUserForTest(staff, group, null);

			Assert(!result);
		}

		bool RunCheckFeatureEnabledForUserForTest(GlbStaff staff, GlbGroup group, StmFeatureTest featureTest)
		{
			return staff.CheckFeatureEnabledForUser(featureTest?.SFT_FeatureName ?? Guid.NewGuid().ToString());
		}

		#region Gender

		public void TestCanSetCustomGender()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Gender = "C";
			staff.GS_GenderCustomTerm = "Test";

			Assert(!staff.GS_GenderCustomTerm_ReadOnly);
			AssertNoExceptionThrown("Should be allowed to save", Factory.Save);
			Assert(staff.GS_GenderCustomTerm == "Test");
		}

		public void TestCannotSetCustomGender()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Gender = "A";
			Factory.Save();
			Assert(staff.GS_GenderCustomTerm_ReadOnly);

			staff.GS_GenderCustomTerm = "Test";
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "The UPDATE statement conflicted with the CHECK constraint \"Constraint_GS_GenderCustomTerm\"", true), "Should not be allowed to save");
		}

		public void TestChangingGenderClearsCustomTerm()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Gender = "C";
			staff.GS_GenderCustomTerm = "Test";

			Factory.Save();

			staff.GS_Gender = "A";
			Factory.Save();

			Assert(staff.GS_GenderCustomTerm.IsEmpty);
		}

		#endregion

		public void TestGS_SqlLoginPasswordHashLength()
		{
			var staffIndex = 0;

			TestNoException(10);
			TestNoException(256);

			TestException(257);
			TestException(1000);

			void TestNoException(int length)
			{
				// Arrange
				var factory = new BusinessObjectFactory();
				var staff = NewStaff(factory);

				// Act
				// Assert
				AssertNoExceptionThrown(() =>
				{
					SetSqlLoginPasswordHash(staff, length);
				});
			}

			void TestException(int length)
			{
				// Arrange
				var factory = new BusinessObjectFactory();
				var staff = NewStaff(factory);

				// Act
				// Assert
				AssertExceptionThrown<MaxLengthExceededException>(
					"Cannot save when hash size is greater than 256",
					$"Max length of SQL login password hash is {GlbStaff.Schema.GS_SqlLoginPasswordHashMaxLength}, current is {length}.",
					() =>
					{
						SetSqlLoginPasswordHash(staff, length);
					});
			}

			GlbStaff NewStaff(BusinessObjectFactory factory)
			{
				++staffIndex;

				var staff = factory.New<GlbStaff>();
				staff.GS_EmailAddress = $"User{staffIndex}@ema.il";
				staff.GS_FullName = $"User{staffIndex} full name";
				staff.GS_LoginName = $"User{staffIndex}login";
				return staff;
			}

			void SetSqlLoginPasswordHash(GlbStaff staff, int sqlLoginPasswordHashLength)
			{
				staff.GS_SqlLoginPasswordHash = Enumerable.Repeat((byte)1, sqlLoginPasswordHashLength).ToArray();
			}
		}

		public void TestGS_SqlLoginPasswordHashGet()
		{
			// Arrange
			var hash = new byte[] { 1, 0, 106, 122, 165, 47, 170, 181, 254, 226, 225, 115, 30, 18, 69, 84, 143, 28, 58, 249, 200, 53, 22, 118, 242, 1 };

			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "User@ema.il";
			staff.GS_FullName = "User full name";
			staff.GS_LoginName = "Userlogin";
			staff.IsDatabaseDeveloper = true;
			staff.GS_SqlLoginPasswordHash = hash;

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				try
				{
					// Act
					Factory.Save();

					// Assert
					var factory = new BusinessObjectFactory(TestConnection);
					var staffInDb = factory.Load<GlbStaff>(staff.PK);

					AssertContainsExactElementsInExactOrder(hash, (byte[])staffInDb.GS_SqlLoginPasswordHash);
				}
				finally
				{
					staff.IsDatabaseDeveloper = false;
					Factory.Save();
				}
			}
		}

		#region staff db access and setting sql password hash

		public void TestRandomSqlPassworddHashIsSetWhenStaffMemberIsGrantedDbAccessForTheFirstTime_ModernSecurityOff()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			const string staffCode = "TS~";

			var staffLoginName = $"Tester_{Guid.NewGuid():N}";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true; // db access role

			// Act
			factory.Save();

			// Assert
			AssertNotEquals(
				DBNull.Value,
				TestConnection.ExecuteScalar(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));
		}

		[UseSnapshotProtection]
		public void TestSqlPasswordHashIsNotChangedWhenStaffMemberIsGrantedAdditionalDbAccess_ModernSecrurityOff()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			const string staffCode = "TS~";

			var staffLoginName = $"Tester_{Guid.NewGuid():N}";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true; // db access role
			staff.IsDatabaseDeveloper = false;
			factory.Save();

			var currentPasswordHash = TestConnection.ExecuteScalar<byte[]>(
				"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName));

			AssertNotEquals(
				DBNull.Value,
				currentPasswordHash);

			staff.IsDatabaseDeveloper = true;

			// Act
			factory.Save();

			// Assert
			AssertEquals(
				currentPasswordHash,
				TestConnection.ExecuteScalar<byte[]>(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));
		}

		public void TestSqlPassworddHashIsNotSetWhenStaffMemberIsGrantedDbAccess_ModernSecrurityOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			const string staffCode = "TS~";
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true; // db access role

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				// Act
				factory.Save();
			}

			// Assert
			AssertEquals("No password should be set when modern security is on before SetPasswordForStaff is called.",
				DBNull.Value,
				TestConnection.ExecuteScalar(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));
		}

		public void TestSqlPasswordHashIsNotModifiedWhenStaffMemberIsGrantedDbAccess_ModernSecrurityOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			const string staffCode = "TS~";
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";
			const string staffSqlLoginPassword = "pA$$w0rD!";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true; // db access role
			staff.IsDatabaseDeveloper = false;

			var dbUserManager = new DbUserManager();
			dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				factory.Save();

				var currentPasswordHash = TestConnection.ExecuteScalar<byte[]>(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName));

				AssertNotEquals(
					DBNull.Value,
					currentPasswordHash);

				staff.IsDatabaseDeveloper = true;

				// Act
				factory.Save();

				// Assert
				AssertEquals(
					currentPasswordHash,
					TestConnection.ExecuteScalar<byte[]>(
						"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
						cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));
			}
		}

		public void TestRemovingDbAccessClearsSqlPasswordHash_ModernSecurityOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			const string staffCode = "TS~";
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";
			const string staffSqlLoginPassword = "pA$$w0rD!";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true; // db access role

			var dbUserManager = new DbUserManager();
			dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				factory.Save();

				var currentPasswordHash = TestConnection.ExecuteScalar<byte[]>(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName));

				AssertNotEquals(
					DBNull.Value,
					currentPasswordHash);

				staff.IsReadOnlyDBUser = false;

				// Act
				factory.Save();
			}

			// Assert
			AssertEquals(
				DBNull.Value,
				TestConnection.ExecuteScalar(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));
		}

		public void TestRemovingDbAccessClearsSqlPasswordHash_ModernSecurityOff()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			const string staffCode = "TS~";
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true; // db access role
			factory.Save();

			var currentPasswordHash = TestConnection.ExecuteScalar<byte[]>(
				"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName));

			AssertNotEquals(
				DBNull.Value,
				currentPasswordHash);

			staff.IsReadOnlyDBUser = false;

			// Act
			factory.Save();

			// Assert
			AssertEquals(
				DBNull.Value,
				TestConnection.ExecuteScalar(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));
		}

		public void TestSqlPasswordHashIsNotModifiedWhenStaffMemberIsRemovedOneOfDbAccessOptions_ModernSecrurityOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			const string staffCode = "TS~";
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";
			const string staffSqlLoginPassword = "pA$$w0rD!";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true;
			staff.IsDatabaseDeveloper = true;

			new DbUserManager().SetPasswordForStaff(staff, staffSqlLoginPassword);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				factory.Save();

				var currentPasswordHash = TestConnection.ExecuteScalar<byte[]>(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName));

				AssertNotEquals(
					DBNull.Value,
					currentPasswordHash);

				staff.IsDatabaseDeveloper = false;

				// Act
				factory.Save();

				// Assert
				AssertEquals(
					currentPasswordHash,
					TestConnection.ExecuteScalar<byte[]>(
						"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
						cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));
			}
		}

		public void TestSqlPasswordHashIsNotModifiedWhenStaffMemberIsRemovedOneOfDbAccessOptions_ModernSecrurityOff()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			const string staffCode = "TS~";
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true;
			staff.IsDatabaseDeveloper = true;

			new DbUserManager().SetPasswordForStaff(staff, "some!2randoMPwd");

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				factory.Save();

				var currentPasswordHash = TestConnection.ExecuteScalar<byte[]>(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName));

				AssertNotEquals(
					DBNull.Value,
					currentPasswordHash);

				staff.IsDatabaseDeveloper = false;

				// Act
				factory.Save();

				// Assert
				AssertEquals(
					currentPasswordHash,
					TestConnection.ExecuteScalar<byte[]>(
						"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
						cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));
			}
		}

		#endregion staff db access and setting sql password hash

		public void TestScimProperties_ScimDisabled_EditingDisabled()
		{
			SetupScim(false, false);

			var staff = GetStaff();
			staff.GS_ExternalId = "123";

			var controlledProps = GetScimControlledPropertyInfos(staff);

			AssertProperties(controlledProps, true);
		}

		public void TestScimProperties_ScimEnabled_EditingDisabled()
		{
			SetupScim(true, false);

			var staff = GetStaff();
			staff.GS_ExternalId = "123";

			var controlledProps = GetScimControlledPropertyInfos(staff);

			AssertProperties(controlledProps, true);
		}

		public void TestScimProperties_ScimDisabled_EditingEnabled()
		{
			SetupScim(false, true);

			var staff = GetStaff();
			staff.GS_ExternalId = "123";

			var controlledProps = GetScimControlledPropertyInfos(staff);

			AssertProperties(controlledProps, false);
		}

		public void TestScimProperties_ScimEnabled_EditingEnabled()
		{
			SetupScim(true, true);

			var staff = GetStaff();
			staff.GS_ExternalId = "123";

			var controlledProps = GetScimControlledPropertyInfos(staff);

			AssertProperties(controlledProps, false);
		}

		public void TestScimUserCheckboxTicked_MappingNotExists()
		{
			SetupScim(true, true);

			var staff = GetStaff();

			AssertEquals(1, staff.Groups.Count);

			staff.IsDatabaseDeveloper = true;
			staff.IsReadOnlyDBUser = true;
			staff.IsBackupOperator = true;

			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			staff = factory.Load<GlbStaff>(staff.PK);

			AssertEquals(4, staff.Groups.Count);
		}

		public void TestScimUserCheckboxTicked_MappingExists()
		{
			SetupScim(true, true);

			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "IsDatabaseDeveloper", GroupDescriptionMapping = "1" },
				new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "IsReadOnlyDBUser", GroupDescriptionMapping = "2" },
				new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "IsBackupOperator", GroupDescriptionMapping = "3" },
				new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = GlbStaffSchema.Constants.GS_IsSalesRep, GroupDescriptionMapping = "4" }
			};
			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "1";

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "2";

			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Desc = "3";

			var group11 = Factory.NewWithValidTestData<GlbGroup>();
			group11.GG_Desc = "11";

			var group4 = Factory.NewWithValidTestData<GlbGroup>();
			group4.GG_Desc = "4";

			var staff = GetStaff();

			AssertEquals(1, staff.Groups.Count);

			staff.IsDatabaseDeveloper = true;
			staff.IsReadOnlyDBUser = true;
			staff.IsBackupOperator = true;

			staff.Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			staff = factory.Load<GlbStaff>(staff.PK);

			AssertEquals(7, staff.Groups.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "1", "2", "3", "ALL STAFF", "Backup Operator", "Database Developer", "Database Reader" }, staff.Groups.Select(g => g.GG_Desc));

			staff.IsDatabaseDeveloper = false;
			staff.IsReadOnlyDBUser = false;
			staff.IsBackupOperator = false;

			staff.Factory.Save();
			factory = new BusinessObjectFactory() { RefreshEnabled = false };
			staff = factory.Load<GlbStaff>(staff.PK);

			AssertEquals(1, staff.Groups.Count);
		}

		public void TestScimUserCheckboxTicked_MappingExists_MultipleGroups()
		{
			SetupScim(true, true);

			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = GlbStaffSchema.Constants.GS_IsSalesRep, GroupDescriptionMapping = "sales rep group" }
			};
			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "sales rep group";

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "sales rep group";

			var staff = GetStaff();

			AssertEquals(1, staff.Groups.Count);

			AssertNoExceptionThrown(() =>
			{
				staff.GS_IsSalesRep = true;
			}
			);

			staff.Factory.Save();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			staff = factory.Load<GlbStaff>(staff.PK);

			AssertEquals(3, staff.Groups.Count);
		}

		public void TestScimUserCheckboxTicked_MappingExists_MultipleGroups_OneInactive()
		{
			SetupScim(true, true);

			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = GlbStaffSchema.Constants.GS_IsSalesRep, GroupDescriptionMapping = "sales rep group" }
			};
			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "sales rep group";

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "sales rep group";
			group2.GG_IsActive = false;

			var staff = GetStaff();

			AssertEquals(1, staff.Groups.Count);

			AssertNoExceptionThrown(() =>
			{
				staff.GS_IsSalesRep = true;
			}
			);

			staff.Factory.Save();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			staff = factory.Load<GlbStaff>(staff.PK);

			AssertEquals(2, staff.Groups.Count);
		}

		public void TestScimUserCheckboxTickUntickTick()
		{
			SetupScim(true, true);

			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "IsDatabaseDeveloper", GroupDescriptionMapping = "1" },
			};
			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "1";

			var staff = GetStaff();

			AssertEquals(1, staff.Groups.Count);

			staff.IsDatabaseDeveloper = true;

			staff.Factory.Save();

			staff.IsDatabaseDeveloper = false;

			staff.Factory.Save();

			staff.IsDatabaseDeveloper = true;

			staff.Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			staff = factory.Load<GlbStaff>(staff.PK);

			AssertEquals(3, staff.Groups.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "1", "ALL STAFF", "Database Developer" }, staff.Groups.Select(g => g.GG_Desc));

			staff.IsDatabaseDeveloper = false;

			staff.Factory.Save();
			factory = new BusinessObjectFactory() { RefreshEnabled = false };
			staff = factory.Load<GlbStaff>(staff.PK);

			AssertEquals(1, staff.Groups.Count);
		}

		public void TestIsControlledByScim()
		{
			SetupScim(false, false);
			var staff = GetStaff();
			staff.GS_ExternalId = "123";

			AssertEquals(true, staff.IsControlledByScim);

			SetupScim(false, true);
			AssertEquals(false, staff.IsControlledByScim);
		}

		public void TestRevertScimProperty_IsController()
		{
			SetupScim(true, false);

			var staff = GetStaff();
			staff.GS_ExternalId = "123";

			var collection = new StaffColumnToGroupDescriptionScimMappingCollection();
			collection.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsController", GroupDescriptionMapping = "1" });

			staff.GS_CanLogin = true;
			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			AssertEquals(false, staff.GS_IsController);
		}

		public void TestRevertScimProperty_IsRobot()
		{
			SetupScim(true, false);

			var staff = GetStaff();
			staff.GS_ExternalId = "123";

			var collection = new StaffColumnToGroupDescriptionScimMappingCollection();
			collection.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsRobot", GroupDescriptionMapping = "1" });

			staff.GS_CanLogin = true;
			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			AssertEquals(false, staff.GS_IsRobot);
		}

		public void TestScimPropertiesDynamic()
		{
			SetupScim(true, false);

			var staff = GetStaff();
			staff.GS_ExternalId = "123";

			var controlledProps = GetScimControlledPropertyInfos(staff);

			AssertProperties(controlledProps, true);

			var collection = new StaffColumnToGroupDescriptionScimMappingCollection();
			collection.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_CanLogin", GroupDescriptionMapping = "1" });

			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			staff = GetStaff();
			staff.GS_ExternalId = "1";
			controlledProps = GetScimControlledPropertyInfos(staff);

			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_CanLogin"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsController"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsDevice"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsSalesRep"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsDriver"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsRobot"));

			AssertProperties(controlledProps, true);

			collection.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsController", GroupDescriptionMapping = "2" });

			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			staff = GetStaff();
			staff.GS_ExternalId = "2";
			controlledProps = GetScimControlledPropertyInfos(staff);

			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_CanLogin"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsController"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsDevice"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsSalesRep"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsDriver"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsRobot"));

			AssertProperties(controlledProps, true);

			collection.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsDevice", GroupDescriptionMapping = "3" });

			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			staff = GetStaff();
			staff.GS_ExternalId = "3";
			controlledProps = GetScimControlledPropertyInfos(staff);

			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_CanLogin"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsController"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsDevice"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsSalesRep"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsDriver"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsRobot"));

			AssertProperties(controlledProps, true);

			collection.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsSalesRep", GroupDescriptionMapping = "4" });

			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			staff = GetStaff();
			staff.GS_ExternalId = "4";
			controlledProps = GetScimControlledPropertyInfos(staff);

			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_CanLogin"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsController"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsDevice"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsSalesRep"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsDriver"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsRobot"));

			AssertProperties(controlledProps, true);

			collection.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsDriver", GroupDescriptionMapping = "5" });

			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			staff = GetStaff();
			staff.GS_ExternalId = "5";
			controlledProps = GetScimControlledPropertyInfos(staff);

			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_CanLogin"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsController"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsDevice"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsSalesRep"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsDriver"));
			AssertEquals(false, controlledProps.Any(p => p.Name == "GS_IsRobot"));

			AssertProperties(controlledProps, true);

			collection.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsRobot", GroupDescriptionMapping = "6" });

			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			staff = GetStaff();
			staff.GS_ExternalId = "6";
			controlledProps = GetScimControlledPropertyInfos(staff);

			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_CanLogin"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsController"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsDevice"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsSalesRep"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsDriver"));
			AssertEquals(true, controlledProps.Any(p => p.Name == "GS_IsRobot"));

			AssertProperties(controlledProps, true);
		}

		void AssertProperties(ZPropertyInfo[] controlledProps, bool expectedControlled)
		{
			foreach (var property in controlledProps)
			{
				AssertEquals(property.Name, expectedControlled, property.ReadOnly);
			}
		}

		void SetupScim(bool scimEnabled, bool editingEnabled)
		{
			SystemDataRegistry.Instance.EnableScimService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, scimEnabled);
			SystemDataRegistry.Instance.ScimAllowLocalEditing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, editingEnabled);
		}

		GlbStaff GetStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			return staff;
		}

		ZPropertyInfo[] GetScimControlledPropertyInfos(GlbStaff staff)
		{
			var properties = new List<ZPropertyInfo>()
			{
				staff.GS_IsActiveInfo,
				staff.GS_LoginNameInfo,
				staff.GS_GivenNameInfo,
				staff.GS_MiddleNameInfo,
				staff.GS_SurnameInfo,
				staff.GS_FullNameInfo,
				staff.GS_NameTitleInfo,
				staff.GS_NameSuffixInfo,
				staff.GS_MobilePhoneInfo,
				staff.GS_MobilePhone_FormattedInfo,
				staff.GS_WorkPhoneInfo,
				staff.GS_WorkPhone_FormattedInfo,
				staff.GS_FaxNumInfo,
				staff.GS_FaxNum_FormattedInfo,
				staff.GS_UserAddress1Info,
				staff.GS_CityInfo,
				staff.GS_StateInfo,
				staff.GS_PostcodeInfo,
				staff.GS_RN_NKCountryCodeInfo,
				staff.GS_EmailAddressInfo,
				staff.GS_FriendlyNameInfo,
				staff.GS_WorkingLanguageInfo
			};

			var dynamicProps = new ZPropertyInfo[]
				{
					staff.GS_IsControllerInfo,
					staff.GS_CanLoginInfo,
					staff.GS_IsDeviceInfo,
					staff.GS_IsDriverInfo,
					staff.GS_IsSalesRepInfo,
					staff.GS_IsRobotInfo,
					staff.IsDatabaseDeveloperInfo,
					staff.IsBackupOperatorInfo,
					staff.IsReadOnlyDBUserInfo
				};

			foreach (var property in dynamicProps)
			{
				if (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.Value.Cast<StaffColumnToGroupDescriptionScimMapping>().Any(m => m.StaffColumnName.EqualsIgnoringCase(property.Name)))
				{
					properties.Add(property);
				}
			}

			return properties.ToArray();
		}

		#region Implementation

		int GenCustomAddOnRuleAckCount
		{
			get { return Factory.Load<GenCustomAddOnRuleAck>(new ZQuery(GenCustomAddOnRuleAckSchema.XK_RuleID, Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation)).Length; }
		}

		StmLoginFailureLog CreateLockoutUserRecord(GlbStaff glbStaff, int lockoutMinutes)
		{
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lockoutMinutes);

			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = glbStaff.GS_LoginName;
			loginFailureLog.SFL_TableCode = GlbStaffSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			return loginFailureLog;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			AssertEquals(1, Staff.Groups.Count);
			return Staff;
		}

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "tfn";
			staff.GS_DomainName = "";
			factory.Save();

			return staff;
		}

		GlbStaff Staff;

		IGlbStaffLoginAttemptRecorder loginAttemptRecorder;
		IGlbStaffLoginAttemptRecorder LoginAttemptRecorder => loginAttemptRecorder ?? (loginAttemptRecorder = ObjectFactory.Get<IGlbStaffLoginAttemptRecorder>());

		protected override void SetUp()
		{
			base.SetUp();
			SetupScim(false, false);
			Staff = (GlbStaff)GetNewBusinessObject();
			Staff.GS_Code = "ZAC";
		}

		#endregion
	}
}
