using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContact))]
	public class OrgContactTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocumentMacroIgnore_Password()
		{
			var passwordHashInfo = typeof(OrgContact).GetProperty("OC_PasswordHash");
			var passwordSaltInfo = typeof(OrgContact).GetProperty("OC_PasswordSalt");

			Assert("OC_PasswordHash should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordHashInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
			Assert("OC_PasswordSalt should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordSaltInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
		}

		public void TestSecurityRightsNoDummies()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "~123";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "name";
			contact.OC_Email = "contact@gmail.com";
			contact.OC_WebAccessEnabled = true;

			AssertEquals(0, contact.SecurityRightsNoDummies.Count);

			var orgSec = Factory.NewWithValidTestData<OrgSecurity>();
			var sec = Factory.New<OrgSecurityContacts>();
			sec.OZ_OC = contact.PK;
			sec.OZ_OX = orgSec.PK;
			sec.OZ_Granted = true;

			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var contactReloaded = factory.Load<OrgContact>(contact.PK);
			AssertEquals(1, contactReloaded.SecurityRightsNoDummies.Count);
		}

		public void TestOverrideOrgMainAddressUsedForDocDeliveryContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = "Parent Org Address1";
			var orgOverride = Factory.NewWithValidTestData<OrgHeader>();
			orgOverride.MainAddress.Address1 = "Override Org Address1";
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "Just1n";
			orgContact.OC_OH_AddressOverride = orgOverride.PK;

			var docDeliveryContact = new DocAutoDelivery().GetDeliveryDetailsForContact(orgContact);

			AssertEquals("Override Org Address1", docDeliveryContact.Address1);
		}

		public void TestUpdatePersonOnlyIfHasChanges()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~code~";
			org.OH_FullName = "fullname";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "name";
			Factory.Save();

			var person = contact.Person;

			contact.OC_ContactName = "modified name";
			contact.HasChanges = false;
			Factory.Save();

			person.Reload();
			AssertEquals("name", person.PER_FullName);

			contact.OC_ContactName = "modified name 2";
			Factory.Save();

			person.Reload();
			AssertEquals("modified name 2", person.PER_FullName);
		}

		public void TestUpdatePerson_ForceUpdate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			var originalContactName1 = "contact1";
			var originalContactName2 = "contact2";

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = originalContactName1;
			contact1.OC_Email = "email1@testing.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = originalContactName2;
			contact2.OC_Email = "email2@testing.com";
			contact1.OC_Gender = "M";
			contact2.OC_Gender = "M";
			Factory.Save();

			AssertEquals("Precondition", contact1.OC_ContactName, contact1.Person.PER_FullName);
			AssertEquals("Precondition", contact2.OC_ContactName, contact2.Person.PER_FullName);

			var updatedContactName1 = "extra1";
			var updatedContactName2 = "extra2";

			var sqlText1 =
$@"UPDATE {OrgContactSchema.Constants.SqlSchemaName}.{OrgContactSchema.Constants.TableName}
SET {OrgContactSchema.Constants.OC_ContactName} = '{updatedContactName1}'
,{OrgContactSchema.Constants.OC_Gender} = 'F'
WHERE {OrgContactSchema.Constants.PK} = @contact1PK;

UPDATE {OrgContactSchema.Constants.SqlSchemaName}.{OrgContactSchema.Constants.TableName}
SET {OrgContactSchema.Constants.OC_ContactName} = '{updatedContactName2}'
,{OrgContactSchema.Constants.OC_Gender} = 'F'
WHERE {OrgContactSchema.Constants.PK} = @contact2PK;
";

			using (var cmd = Db.Connection.Command(sqlText1))
			{
				cmd.AddParameter("@contact1PK", SqlDbType.UniqueIdentifier, contact1.PK.ToGuid());
				cmd.AddParameter("@contact2PK", SqlDbType.UniqueIdentifier, contact2.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			contact1.Reload();
			contact2.Reload();
			AssertEquals("Precondition", originalContactName1, contact1.Person.PER_FullName);
			AssertEquals("Precondition", originalContactName2, contact2.Person.PER_FullName);
			AssertEquals("Precondition", updatedContactName1, contact1.OC_ContactName);
			AssertEquals("Precondition", updatedContactName2, contact2.OC_ContactName);
			AssertEquals("Precondition", "M", contact1.Person.PER_Gender);
			AssertEquals("Precondition", "M", contact2.Person.PER_Gender);
			AssertEquals("Precondition", "F", contact1.OC_Gender);
			AssertEquals("Precondition", "F", contact2.OC_Gender);

			contact1.UpdatePerson();
			contact2.UpdatePerson(forceUpdate: true);
			AssertEquals("Should remain unchanged", originalContactName1, contact1.Person.PER_FullName);
			AssertEquals("Should be updated", updatedContactName2, contact2.Person.PER_FullName);
			AssertEquals("Should remain unchanged", "M", contact1.Person.PER_Gender);
			AssertEquals("Should be updated", "F", contact2.Person.PER_Gender);
		}

		public void TestUpdatePersonOnlyChangedFields()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~code~";
			org.OH_FullName = "fullname";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "name";
			contact.OC_Gender = "F";
			contact.OC_Birthday = new ZDateTime(1990, 1, 1);
			Factory.Save();

			var person = contact.Person;

			contact.OC_ContactName = "modified name";
			contact.OC_Gender = "M";
			contact.OC_Birthday = new ZDateTime(2000, 1, 1);
			contact.HasChanges = false;
			Factory.Save();

			person.Reload();
			AssertEquals("name", person.PER_FullName);
			AssertEquals("F", person.PER_Gender);
			AssertEquals(new ZDateTime(1990, 1, 1), person.PER_BirthDate);

			var contactInOtherFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<OrgContact>(contact.PK);

			contactInOtherFactory.OC_Birthday = new ZDateTime(2010, 1, 1);
			contactInOtherFactory.Factory.Save();

			person.Reload();

			AssertEquals("Name wasn't modified", "name", person.PER_FullName);
			AssertEquals("Gender wasn't modified", "F", person.PER_Gender);
			AssertEquals(new ZDateTime(2010, 1, 1), person.PER_BirthDate);
		}

		public void TestPersonIsCreated()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			contact.OC_ContactName = "almost full name";
			Factory.Save();

			AssertNotEquals(ZGuid.Empty, contact.OC_PER);

			var person = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbPerson>(contact.OC_PER);
			AssertNotNull(person);
			AssertEquals("almost full name", person.PER_FullName);

			contact = new BusinessObjectFactory() { RefreshEnabled = false }.Load<OrgContact>(contact.PK);
			AssertEquals(person.PK, contact.OC_PER);
		}

		public void TestPersonIsUpdated()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "almost full name";
			Factory.Save();

			var person = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbPerson>(contact.OC_PER);
			AssertNotNull(person);
			contact = person.Factory.Load<OrgContact>(contact.PK);

			contact.OC_ContactName = "new full name";
			contact.Factory.Save();

			person = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbPerson>(person.PK);
			AssertNotNull(person);
			AssertEquals("new full name", person.PER_FullName);
		}

		public void TestUpdateFromPerson()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "shouldnotchange@test.com";
			Factory.Save();

			var person = Factory.Load<GlbPerson>(contact.OC_PER);
			AssertNotNull(person);
			contact.Factory.Save();

			person.PER_FullName = "new full name";
			person.PER_Gender = "F";
			person.PER_NameTitle = "new title";
			person.PER_HomePhone = "987654321";
			person.PER_BirthDate = ZDateTime.Today.Date;
			person.PER_RN_NKNationalityCodeISO = "ZZ";
			person.PER_PersonalInfo = "info";
			person.PER_Picture = new ZBlob(new byte[] { 1 });
			person.PER_EmailAddress = "newemail@contact.com";
			person.PER_EmailAddress2 = "newemail@contact.com";
			contact.UpdateFromPerson(person);

			AssertEquals(person.PER_FullName, contact.OC_ContactName);
			AssertEquals(person.PER_Gender, contact.OC_Gender);
			AssertEquals(person.PER_HomePhone, contact.OC_HomePhone);
			AssertEquals(person.PER_BirthDate, contact.OC_Birthday);
			AssertEquals(person.PER_RN_NKNationalityCodeISO, contact.OC_RN_NKNationality);
			AssertEquals(person.PER_PersonalInfo, contact.OC_PersonalInfo);
			AssertEquals(Array.Empty<byte>(), contact.OC_ProfilePhoto);
			AssertEquals("shouldnotchange@test.com", contact.OC_Email);
		}

		public void TestUpdateFromPerson_EnsureUniqueContactName()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			var contact1b = org1.Contacts.AddNew();
			contact1b.OC_ContactName = "User One (1)";

			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_PER = contact1b.OC_PER;
			var contact2b = org2.Contacts.AddNew();
			contact2b.OC_ContactName = "User Two";

			Factory.Save();

			var person1 = Factory.Load<GlbPerson>(contact1b.OC_PER);
			person1.PER_FullName = "User One (1)";
			Factory.Save();
			AssertEquals("User One (1)", contact1b.OC_ContactName);
			AssertEquals("User One", contact2a.OC_ContactName);

			person1.PER_FullName = "User Two";
			Factory.Save();
			AssertEquals("User Two", contact1b.OC_ContactName);
			AssertEquals("User Two (1)", contact2a.OC_ContactName);
		}

		public void TestUpdateFromPersonWithoutSecurity()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "shouldnotchange@test.com";
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					Env.Security.PersonIntelligenceView.IsAllowed = false;
					Env.Security.PersonIntelligenceEdit.IsAllowed = false;
					Env.Security.OrgContactViewPersonalInformation.IsAllowed = true;

					var newFactory = new BusinessObjectFactory();
					contact = newFactory.Load<OrgContact>(contact.PK);
					var person = newFactory.Load<GlbPerson>(contact.OC_PER);
					AssertNotNull(person);
					newFactory.Save();

					person.PER_FullName = "new full name";
					person.PER_Gender = "F";
					person.PER_NameTitle = "new title";
					person.PER_HomePhone = "987654321";
					person.PER_BirthDate = ZDateTime.Today.Date;
					person.PER_RN_NKNationalityCodeISO = "ZZ";
					person.PER_PersonalInfo = "info";
					person.PER_Picture = new ZBlob(new byte[] { 1 });
					person.PER_EmailAddress = "newemail@contact.com";
					person.PER_EmailAddress2 = "newemail@contact.com";
					contact.UpdateFromPerson(person);

					AssertEquals(person.PER_FullName, "new full name");
					AssertEquals(person.PER_Gender, "** View Denied **");
					AssertEquals(person.PER_HomePhone, "** View Denied **");
					AssertEquals(person.PER_BirthDate, ZDate.Empty);
					AssertEquals(person.PER_RN_NKNationalityCodeISO, "** View Denied **");
					AssertEquals(person.PER_PersonalInfo, "** View Denied **");
					AssertEquals(person.PER_Picture, ZBlob.Empty);
					AssertEquals(person.PER_EmailAddress, "** View Denied **");
					AssertEquals(person.PER_EmailAddress2, "** View Denied **");

					AssertEquals(person.PER_FullNameInternal, "new full name");
					AssertEquals(person.PER_GenderInternal, "F");
					AssertEquals(person.PER_HomePhoneInternal, "987654321");
					AssertEquals(person.PER_BirthDateInternal, ZDateTime.Today.Date);
					AssertEquals(person.PER_RN_NKNationalityCodeISOInternal, "ZZ");
					AssertEquals(person.PER_PersonalInfoInternal, "info");
					AssertEquals(person.PER_PictureInternal, new ZBlob(new byte[] { 1 }));
					AssertEquals(person.PER_EmailAddressInternal, "newemail@contact.com");
					AssertEquals(person.PER_EmailAddress2Internal, "newemail@contact.com");

					AssertEquals(person.PER_FullNameInternal, contact.OC_ContactName);
					AssertEquals(person.PER_GenderInternal, contact.OC_Gender);
					AssertEquals(person.PER_HomePhoneInternal, contact.OC_HomePhone);
					AssertEquals(person.PER_BirthDateInternal, contact.OC_Birthday);
					AssertEquals(person.PER_RN_NKNationalityCodeISOInternal, contact.OC_RN_NKNationality);
					AssertEquals(person.PER_PersonalInfoInternal, contact.OC_PersonalInfo);
					AssertEquals(Array.Empty<byte>(), contact.OC_ProfilePhoto);
					AssertEquals("shouldnotchange@test.com", contact.OC_Email);
				}
			}
		}

		public void TestUpdateFromPerson_MobileOnlyUpdatedIfSame()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			var person = Factory.Load<GlbPerson>(contact.OC_PER);
			Factory.Save();

			var contactMobile = "123456789";
			contact.OC_Mobile = contactMobile;
			var personMobile = "999999999";
			person.PER_MobilePhone = personMobile;
			Factory.Save();
			AssertEquals("Contact mobile should not change as it is different to old person mobile", contactMobile, contact.OC_Mobile);

			contact.OC_Mobile = personMobile;
			var newPersonMobile = "000000000";
			person.PER_MobilePhone = newPersonMobile;
			Factory.Save();
			AssertEquals("Contact mobile should change as it is the same as old person mobile", newPersonMobile, contact.OC_Mobile);
		}

		public void TestInternalPropertiesWithoutSecurity()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "shouldnotchange@test.com";
			contact.OC_PersonalInfo = "personal";
			contact.OC_Gender = "F";
			contact.OC_RN_NKNationality = "AU";
			contact.OC_HomePhone = "94111111";
			contact.OC_Birthday = new ZDateTime(2019, 01, 01);
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					Env.Security.PersonIntelligenceView.IsAllowed = false;
					Env.Security.PersonIntelligenceEdit.IsAllowed = false;
					Env.Security.OrgContactViewPersonalInformation.IsAllowed = false;
					Env.Security.OrgContactViewMobileNumber.IsAllowed = false;
					Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = false;

					var newFactory = new BusinessObjectFactory();
					contact = newFactory.Load<OrgContact>(contact.PK);

					AssertEquals("personal", contact.OC_PersonalInfoInternal);
					AssertEquals("F", contact.OC_GenderInternal);
					AssertEquals("AU", contact.OC_RN_NKNationalityInternal);
					AssertEquals(new ZDateTime(2019, 01, 01), contact.OC_BirthdayInternal);
					AssertEquals("94111111", (ZString)contact.OC_HomePhoneInfo.Value);

					AssertEquals(contact.ViewDeniedMessage, contact.OC_PersonalInfo);
					AssertEquals(contact.ViewDeniedMessage, contact.OC_Gender);
					AssertEquals(contact.ViewDeniedMessage, contact.OC_RN_NKNationality);
					AssertEquals(ZDateTime.Empty, contact.OC_Birthday);
					AssertEquals(contact.ViewDeniedMessage, contact.OC_HomePhone_Formatted);

					AssertEquals(true, contact.OC_PersonalInfoInfo.HasChanges);
					AssertEquals(true, contact.OC_GenderInfo.HasChanges);
					AssertEquals(true, contact.OC_RN_NKNationalityInfo.HasChanges);
					AssertEquals(true, contact.OC_BirthdayInfo.HasChanges);
					AssertEquals(false, contact.OC_HomePhoneInfo.HasChanges);

					AssertEquals("Internal property should not have changes", false, contact.OC_PersonalInfoInternalHasChanges);
					AssertEquals("Internal property should not have changes", false, contact.OC_GenderInternalHasChanges);
					AssertEquals("Internal property should not have changes", false, contact.OC_RN_NKNationalityInternalHasChanges);
					AssertEquals("Internal property should not have changes", false, contact.OC_BirthdayInternalHasChanges);

					contact.OC_PersonalInfo = "new info";
					contact.OC_Gender = "M";
					contact.OC_RN_NKNationality = "US";
					contact.OC_Birthday = new ZDateTime(2019, 02, 02);
					contact.OC_HomePhone = "94222222";

					AssertEquals("Internal property should have changes", true, contact.OC_PersonalInfoInternalHasChanges);
					AssertEquals("Internal property should have changes", true, contact.OC_GenderInternalHasChanges);
					AssertEquals("Internal property should have changes", true, contact.OC_RN_NKNationalityInternalHasChanges);
					AssertEquals("Internal property should have changes", true, contact.OC_BirthdayInternalHasChanges);
					AssertEquals(true, contact.OC_HomePhoneInfo.HasChanges);

					newFactory.Save();

					AssertEquals("Internal property should not have changes", false, contact.OC_PersonalInfoInternalHasChanges);
					AssertEquals("Internal property should not have changes", false, contact.OC_GenderInternalHasChanges);
					AssertEquals("Internal property should not have changes", false, contact.OC_RN_NKNationalityInternalHasChanges);
					AssertEquals("Internal property should not have changes", false, contact.OC_BirthdayInternalHasChanges);
				}
			}
		}

		public void TestHumanReadableName()
		{
			var org = Factory.New<OrgHeader>();

			var contact = org.Contacts.AddNew();
			AssertEquals("Contact", contact.HumanReadableName);

			contact.OC_ContactName = "Zay Ry Lola";
			AssertEquals("Contact (Zay Ry Lola)", contact.HumanReadableName);

			contact.Delete();
			AssertEquals("Contact", contact.HumanReadableName);
		}

		public void TestUniqueCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAA";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John";

			AssertEquals("The code should just be the contact name, because otherwise the CodeFindBox can't find the correct value", "John", ((ICodeDescription)contact).Code);
		}

		public void TestCanParticipate()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();

			var notifications = new NotificationCollection();
			((IConversationParticipant)contact).CheckCanParticipate(notifications);

			AssertEquals("This Participant cannot be added as they do not have an associated email address", notifications[0].Message);

			notifications.Clear();
			contact.OC_Email = "something@test.com";
			((IConversationParticipant)contact).CheckCanParticipate(notifications);

			AssertEquals("We do have an email, and should be allowed to participate", 0, notifications.Count);
		}

		public void TestConversationProviderName()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = ZGuid.Empty;
			contact.OC_ContactName = "Barry";

			AssertEquals("Barry", ((IConversationParticipant)contact).Name);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "BARORG";
			contact.OC_OH = org.PK;

			AssertEquals("Barry (BARORG)", ((IConversationParticipant)contact).Name);
		}

		public void TestIsSystemGenerated()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();

			Assert("Contact should not be System-generated", !contact.IsSystemGenerated);

			contact.OC_SystemCreateUser = User.ServiceUserCode;

			Assert("Contact should be System-generated", contact.IsSystemGenerated);
		}

		public void TestDeleteOrgContact()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSTORG1";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User 1";

			AssertEquals("Should be possible to delete", true, contact.CanDelete);

			Factory.Save();

			AssertEquals("Should not be possible to delete", false, contact.CanDelete);
			AssertEquals("Existing contacts cannot be deleted. Mark them as inactive instead.", contact.ReasonForNotAbleToDelete);
		}

		public void TestInvalidateByLocalDataChanges_ShouldNotInvalidateScreeningStatus()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var sequence = Convert.ToInt32(Env.NumberFountains.StmEntityScreeningLogNumber.GetNextFormatted(Factory)).ToString();
			var sql = $@"UPDATE dbo.OrgHeader SET OH_ScreeningStatus = '{ScreeningStatusesList.Codes.Clear}' WHERE OH_PK = '{header.PK}'
INSERT INTO dbo.StmEntityScreeningLog (PJ_PK, PJ_Sequence, PJ_Status, PJ_ParentID, PJ_ParentTableCode, PJ_SystemCreateTimeUtc, PJ_SystemCreateUser)
VALUES (NEWID(), '{sequence}', '{DeniedPartyConstants.LogsScreeningStatus.ScreenedClear}', '{header.PK}', 'OH', GETDATE(), '~BP')";

			TestConnection.ExecuteNonQuery(sql);

			header.Reload();

			CombineAssertions("Should not invalidate screening status", () =>
			{
				var contact = header.Contacts.AddNew();
				AssertHasInvalidatedScreeningStatuses(() => { contact.OC_ContactName = "hello"; });
				AssertHasInvalidatedScreeningStatuses(() => { contact.OC_Birthday = ZDateTime.Now; });
				AssertHasInvalidatedScreeningStatuses(() => { contact.OC_Email = "bob"; });
			});

			void AssertHasInvalidatedScreeningStatuses(Action dataChanged)
			{
				dataChanged.Invoke();
				Factory.Save();

				AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);
				AssertEquals(1, header.ScreeningLogCollection.Count);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.ScreenedClear, header.ScreeningLogCollection[0].PJ_Status);
			}
		}

		public void TestPhoneFallbackToOrganisation()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.MainAddress.OA_Phone = "444";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_Phone = "333";

			AssertEquals("333", contact1.PhoneFallbackToOrganisation);

			contact1.OC_Phone = "";
			AssertEquals("444", contact1.PhoneFallbackToOrganisation);
		}

		public void TestFaxFallbackToOrganisation()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.MainAddress.OA_Fax = "444";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_Fax = "333";

			AssertEquals("333", contact1.FaxFallbackToOrganisation);

			contact1.OC_Fax = "";
			AssertEquals("444", contact1.FaxFallbackToOrganisation);
		}

		public void TestEmailFallbackToOrganisation()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.MainAddress.OA_Email = "test@edi.com.au";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "noone@edi.com.au";

			AssertEquals("noone@edi.com.au", contact1.EmailFallbackToOrganisation);

			contact1.OC_Email = "";
			AssertEquals("test@edi.com.au", contact1.EmailFallbackToOrganisation);
		}

		public void TestOrgClosestPortAndLocation()
		{
			var org = OrgHeader.New(Factory);
			org.OH_RL_NKClosestPort = "AUSYD";

			var address1 = org.Addresses.AddNew();
			var contact1 = org.Contacts.AddNew();
			AssertEquals("Contact defaults to Organisation's UNLOCO", "AUSYD", contact1.OrgClosestPort.Code);
			AssertEquals("Contact defaults to Organisation's UNLOCO", "AUSYD", contact1.Location);

			address1.OA_RL_NKRelatedPortCode = "SGSIN";
			contact1.OC_OA_OrgAddress = address1.PK;
			AssertEquals("Contact defaults to Address' UNLOCO", "SGSIN", contact1.OrgClosestPort.Code);
			AssertEquals("Contact defaults to Address' UNLOCO", "SGSIN", contact1.Location);

			var org2 = OrgHeader.New(Factory);
			org2.OH_RL_NKClosestPort = "GBLON";
			contact1.OC_OH_AddressOverride = org2.PK;
			contact1.OC_OA_OrgAddress = org2.MainAddress.PK;
			AssertEquals("Contact defaults to Organisation 2's UNLOCO", "GBLON", contact1.OrgClosestPort.Code);
			AssertEquals("Contact defaults to Organisation 2's UNLOCO", "GBLON", contact1.Location);

			var address2 = org2.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "";
			contact1.OC_OA_OrgAddress = address2.PK;
			AssertEquals("Contact defaults to Organisation 2's UNLOCO", "GBLON", contact1.OrgClosestPort.Code);
			AssertEquals("Contact defaults to Organisation 2's UNLOCO", "GBLON", contact1.Location);

			var contact2 = Factory.New<OrgContact>();
			AssertEquals("Contact OrgClosestPort is null", null, contact2.OrgClosestPort);
			AssertEquals("Contact Location is blank", "", contact2.Location);
		}

		public void TestLocation_WithInvalidAddress()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address = org.Addresses.AddNew();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_OA_OrgAddress = ZGuid.NewZGuid();
			AssertEquals("Location with invalid address, expect no exception", "", contact.Location);
		}

		public void TestIsCustomerServiceContact()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgDocument doc = contact.Documents.AddNew();

			Assert(!contact.IsCustomerServiceContact);

			doc.OD_DocumentGroup = "SAL";
			Assert(!contact.IsCustomerServiceContact);

			doc.OD_DocumentGroup = "CSV";
			Assert(contact.IsCustomerServiceContact);

			contact.Documents.RemoveAndDelete(doc);
			Assert(!contact.IsCustomerServiceContact);

			AssertEquals(0, contact.Documents.Count);

			contact.IsCustomerServiceContact = true;
			AssertEquals(1, contact.Documents.Count);

			AssertEquals("CSV", contact.Documents[0].OD_DocumentGroup);
		}

		public void TestIsCommissionAgreementRecipientContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			var doc = contact.Documents.AddNew();

			Assert(!contact.IsCommissionAgreementRecipientContact);

			doc.OD_DocumentGroup = "SAL";
			Assert(!contact.IsCommissionAgreementRecipientContact);

			doc.OD_DocumentGroup = ContactType.CommissionAgreementRecipient.Code;
			Assert(contact.IsCommissionAgreementRecipientContact);

			contact.Documents.RemoveAndDelete(doc);
			Assert(!contact.IsCommissionAgreementRecipientContact);
		}

		public void TestCampaigns()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ZUbin";
			Factory.Save();

			AssertEquals("Precondition: No campaigns for this contact", 0, contact.Campaigns.Count);

			BusinessObject campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			BusinessObject item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = contact.PK;

			Factory.Save();

			OrgContact reloaded = new BusinessObjectFactory().Load<OrgContact>(contact.PK);
			AssertEquals("1 campaign for this contact", 1, reloaded.Campaigns.Count);

			AssertEquals("Campaigns are readonly", true, reloaded.Campaigns.ReadOnly);

			BusinessObject reloadedItem = (BusinessObject)reloaded.Campaigns[0];
			reloaded.Delete();
			Assert(reloaded.IsDeleted);
			Assert("Dependent campaign items are deleted", reloadedItem.IsDeleted);
		}

		public void TestBranchAddress()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Test Address";
			OrgContact contact = org.Contacts.AddNew();
			AssertNull("Branch Address Null", contact.BranchAddress);

			contact.OC_OA_OrgAddress = org.MainAddress.PK;
			AssertEquals("Address should be TestAddress", "Test Address", contact.BranchAddress.OA_Address1);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			AssertEquals("business object with related logs", 0, contact.BusinessObjectsWithRelatedEvents.Length);

			contact.Documents.AddNew();
			AssertEquals("business object with related logs", 1, contact.BusinessObjectsWithRelatedEvents.Length);
		}

		public void TestIsEnglish()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OrgContact contact = org.Contacts.AddNew();
			AssertEquals("Contact is English by default", true, contact.IsEnglish);

			contact.OC_Language = Core.Constants.Languages.Hindi;
			AssertEquals("Contact is not English", false, contact.IsEnglish);

			contact.OC_Language = Constants.Languages.English;
			AssertEquals("Contact is English", true, contact.IsEnglish);
		}

		public void TestDeletingContactDeletesSecurityRights()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User";
			contact.OC_WebAccessEnabled = true;
			OrgSecurity orgSecurityRight = org.SecurityRights.Cast<OrgSecurity>().First(s => s.OX_Granted == ExpectedDefaultSecurityRightAccess);
			OrgSecurityContacts securityRight = contact.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().First(s => s.OZ_OX == orgSecurityRight.PK);
			securityRight.OZ_Granted = !ExpectedDefaultSecurityRightAccess;

			Factory.Save();

			AssertEquals("Contact security right is in the database", true, securityRight.IsInDatabase);

			contact.Delete();

			AssertEquals("Security Right has no changes", false, securityRight.HasChanges);
			AssertEquals("Security Right is deleted", true, securityRight.IsDeleted);

			Factory.Save();

			AssertEquals("Security Right is NOT in the database", false, securityRight.IsInDatabase);
		}

		public void TestDeletingContactDeletesSecurityRightsAsThroughBinding()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			Assert("Precondition: Org should not have changes", !org.HasChanges);

			OrgContact contact = (OrgContact)((IBindingList)org.Contacts).AddNew();
			Assert("Org should not have changes", !org.HasChanges);
			Assert("Precondition: Security rights were added to this 'dummy' contact'", contact.SecurityRightsForBindingOnly.Count > 0);

			((ICancelAddNew)org.Contacts).CancelNew(org.Contacts.Count - 1);

			Assert("Org should not have changes", !org.HasChanges);
		}

		public void TestWebAccess()
		{
			OrgSecurity orgSecurityRight = Company.SecurityRights.Cast<OrgSecurity>().First(s => s.OX_Granted == ExpectedDefaultSecurityRightAccess);
			OrgSecurityContacts securityRight = Contact.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().First(s => s.OZ_OX == orgSecurityRight.PK);

			Assert("Precondition: Security rights exist", Contact.SecurityRightsForBindingOnly.Count > 0);

			Contact.OC_WebAccessEnabled = true;
			securityRight.OZ_Granted = false;
			AssertEquals("Security rights is granted", false, securityRight.OZ_Granted);

			Contact.OC_WebAccessEnabled = false;
			AssertEquals("Security rights is denied", false, securityRight.OZ_Granted);

			Contact.OC_WebAccessEnabled = true;
			AssertEquals("Security rights is granted", ExpectedDefaultSecurityRightAccess, securityRight.OZ_Granted);
		}

		public void TestWebAccess_ShouldNotInheritFromParentIfAlreadyEnabledPreviously()
		{
			OrgSecurity orgSecurityRight = Company.SecurityRights.Cast<OrgSecurity>().First(s => s.OX_Granted == ExpectedDefaultSecurityRightAccess);
			OrgSecurityContacts securityRight = Contact.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().First(s => s.OZ_OX == orgSecurityRight.PK);

			Contact.OC_WebAccessEnabled = true;
			AssertEquals("Should inherit from parent", ExpectedDefaultSecurityRightAccess, securityRight.OZ_Granted);

			securityRight.OZ_Granted = !ExpectedDefaultSecurityRightAccess;
			AssertEquals("Explicitly defined for this Contact", !ExpectedDefaultSecurityRightAccess, securityRight.OZ_Granted);

			Contact.OC_WebAccessEnabled = true;
			AssertEquals("Should not re-inherit from Parent", !ExpectedDefaultSecurityRightAccess, securityRight.OZ_Granted);
		}

		protected virtual bool ExpectedDefaultSecurityRightAccess
		{
			get { return true; }
		}

		public void TestDelete()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test User";
			contact1.OC_Phone_IsManuallyVerified = true;
			contact1.OC_Fax_IsManuallyVerified = true;

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_Mobile_IsManuallyVerified = true;

			OrgContactAttribute attrib1 = contact1.Attributes.AddNew();
			var allocation = contact1.Allocations.AddNew();
			Factory.Save();
			AssertEquals("Is in the database", true, attrib1.IsInDatabase);
			var acks1 = new GenCustomAddOnRuleAckCollection(contact1);
			var acks2 = new GenCustomAddOnRuleAckCollection(contact2);
			AssertEquals("Precondition", 2, acks1.Count);
			AssertEquals("Precondition", 1, acks2.Count);

			contact1.Delete();
			AssertEquals("Deleted", true, attrib1.IsDeleted);
			AssertEquals("Allocation attribute should be deleted", true, allocation.IsDeleted);

			AssertEquals(0, acks1.Count);
			AssertEquals(1, acks2.Count);

			Factory.Save();
			AssertEquals("NOT in the database", false, attrib1.IsInDatabase);
			AssertEquals("Allocation attribute should NOT in the database either", false, allocation.IsInDatabase);
		}

		public void TestDeleteContactShouldDeleteRelatedGlbGroupOrgContactLink()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "Buyer";
			group.GG_Type = GlbGroupTypeList.Codes.Organisation;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "name";
			contact.OC_Email = "email@email.com";

			var orgContactLink = Factory.NewWithValidTestData<GlbGroupOrgContactLink>();
			orgContactLink.GCK_GG_Group = group.PK;
			orgContactLink.GCK_OC_Contact = contact.PK;
			Factory.Save();

			contact.Delete();
			AssertEquals("GlbGroupOrgContactLink should be deleted", true, orgContactLink.IsDeleted);
		}

		public void TestDelete_PrimaryPersonRelationship_SingleContact()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.Person.SetPrimaryRelationship(orgContact);
			Factory.Save();

			var personPK = orgContact.Person.PK;
			var primaryPK = orgContact.Person.PrimaryRelationship.PK;

			orgContact.Delete();
			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var personReloaded = factory.Load<GlbPerson>(personPK);
			var primaryReloaded = factory.Load<GlbPersonPrimaryRelationship>(primaryPK);

			AssertNull(personReloaded);
			AssertNull(primaryReloaded);
		}

		public void TestDeleteOrg_PrimaryPersonRelationship_SingleContact()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.Person.SetPrimaryRelationship(orgContact);
			Factory.Save();

			var personPK = orgContact.Person.PK;
			var primaryPK = orgContact.Person.PrimaryRelationship.PK;

			orgContact.ParentOrg.Delete();
			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var personReloaded = factory.Load<GlbPerson>(personPK);
			var primaryReloaded = factory.Load<GlbPersonPrimaryRelationship>(primaryPK);

			AssertNull(personReloaded);
			AssertNull(primaryReloaded);
		}

		public void TestDelete_PrimaryPersonRelationship_MultipleContacts()
		{
			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			orgContact1.Person.SetPrimaryRelationship(orgContact1);
			Factory.Save();

			var orgContact2 = orgContact1.ParentOrg.Contacts.AddNew();
			orgContact2.OC_PER = orgContact1.OC_PER;
			orgContact2.OC_ContactName = "name 2";
			Factory.Save();

			var orgContact3 = orgContact1.ParentOrg.Contacts.AddNew();
			orgContact3.OC_PER = orgContact1.OC_PER;
			orgContact3.OC_ContactName = "name 3";
			Factory.Save();

			var orgContact4 = orgContact1.ParentOrg.Contacts.AddNew();
			orgContact4.OC_PER = orgContact1.OC_PER;
			orgContact4.OC_IsActive = false;
			orgContact4.OC_ContactName = "name 4";
			Factory.Save();

			var personPK = orgContact1.Person.PK;
			var primaryPK = orgContact1.Person.PrimaryRelationship.PK;

			Factory.Save();
			orgContact1.Delete();
			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var personReloaded = factory.Load<GlbPerson>(personPK);
			var primaryReloaded = factory.Load<GlbPersonPrimaryRelationship>(primaryPK);

			AssertNotNull(personReloaded);
			AssertNotNull(primaryReloaded);

			primaryReloaded = factory.Load<GlbPersonPrimaryRelationship>(primaryPK);
			AssertEquals("should pick latest created active contact", orgContact3.PK, primaryReloaded.PPR_PrimaryId);
		}

		public void TestDelete_PrimaryPersonRelationship_MultipleChildObj()
		{
			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			orgContact1.Person.SetPrimaryRelationship(orgContact1);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "~s1";
			staff.GS_FullName = "name";
			staff.GS_PER = orgContact1.OC_PER;
			Factory.Save();

			var orgContact2 = orgContact1.ParentOrg.Contacts.AddNew();
			orgContact2.OC_PER = orgContact1.OC_PER;
			orgContact2.OC_ContactName = "name 1";
			Factory.Save();

			var orgContact3 = orgContact1.ParentOrg.Contacts.AddNew();
			orgContact3.OC_PER = orgContact1.OC_PER;
			orgContact3.OC_ContactName = "name 2";
			Factory.Save();

			var orgContact4 = orgContact1.ParentOrg.Contacts.AddNew();
			orgContact4.OC_PER = orgContact1.OC_PER;
			orgContact4.OC_IsActive = false;
			orgContact4.OC_ContactName = "name4";
			Factory.Save();

			var personPK = orgContact1.Person.PK;
			var primaryPK = orgContact1.Person.PrimaryRelationship.PK;

			Factory.Save();
			orgContact1.Delete();
			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var personReloaded = factory.Load<GlbPerson>(personPK);
			var primaryReloaded = factory.Load<GlbPersonPrimaryRelationship>(primaryPK);

			AssertNotNull(personReloaded);
			AssertNotNull(primaryReloaded);

			primaryReloaded = factory.Load<GlbPersonPrimaryRelationship>(primaryPK);
			AssertEquals("should pick active staff if exists", staff.PK, primaryReloaded.PPR_PrimaryId);
		}

		public void TestDelete_PrimaryPersonRelationship_MultipleChildObj_InactiveStaff()
		{
			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			orgContact1.Person.SetPrimaryRelationship(orgContact1);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "~s1";
			staff.GS_FullName = "name";
			staff.GS_PER = orgContact1.OC_PER;
			staff.GS_IsActive = false;
			Factory.Save();

			var orgContact2 = orgContact1.ParentOrg.Contacts.AddNew();
			orgContact2.OC_PER = orgContact1.OC_PER;
			orgContact2.OC_ContactName = "name 2";
			Factory.Save();

			var orgContact3 = orgContact1.ParentOrg.Contacts.AddNew();
			orgContact3.OC_PER = orgContact1.OC_PER;
			orgContact3.OC_ContactName = "name 3";
			Factory.Save();

			var orgContact4 = orgContact1.ParentOrg.Contacts.AddNew();
			orgContact4.OC_PER = orgContact1.OC_PER;
			orgContact4.OC_IsActive = false;
			orgContact4.OC_ContactName = "name 4";
			Factory.Save();

			var personPK = orgContact1.Person.PK;
			var primaryPK = orgContact1.Person.PrimaryRelationship.PK;

			Factory.Save();
			orgContact1.Delete();
			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var personReloaded = factory.Load<GlbPerson>(personPK);
			var primaryReloaded = factory.Load<GlbPersonPrimaryRelationship>(primaryPK);

			AssertNotNull(personReloaded);
			AssertNotNull(primaryReloaded);

			primaryReloaded = factory.Load<GlbPersonPrimaryRelationship>(primaryPK);
			AssertEquals("should pick latest created active contact because staff is inactive", orgContact3.PK, primaryReloaded.PPR_PrimaryId);
		}

		public void TestDetailsVerifiedLog()
		{
			Contact.OC_ContactName = "Zubin Appoo";
			Contact.OC_DetailsVerified = new ZDateTime(2005, 5, 28);
			Factory.Save();
			StmALog[] auditedLogs = Contact.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.RecordAudited.Code));
			AssertEquals(1, auditedLogs.Length);
			AssertEquals("Zubin Appoo details verified - 28-May-05", auditedLogs[0].SL_Reference);

			Factory.Save();
			auditedLogs = Contact.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.RecordAudited.Code));
			AssertEquals(1, auditedLogs.Length);
			AssertEquals("Zubin Appoo details verified - 28-May-05", auditedLogs[0].SL_Reference);

			Contact.OC_DetailsVerified = new ZDateTime(2005, 8, 15);
			Factory.Save();
			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.RecordAudited.Code);
			filter.OrderBy = StmALogSchema.SL_Reference.Name + " DESC";
			auditedLogs = Contact.Logs.Find(filter);
			AssertEquals(2, auditedLogs.Length);
			AssertEquals("Zubin Appoo details verified - 28-May-05", auditedLogs[0].SL_Reference);
			AssertEquals("Zubin Appoo details verified - 15-Aug-05", auditedLogs[1].SL_Reference);

			Contact.OC_DetailsVerified = ZDateTime.Empty;
			Factory.Save();
			auditedLogs = Contact.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.RecordAudited.Code));
			AssertEquals("no new log added", 2, auditedLogs.Length);
		}

		public void TestOC_IsPrimaryContact()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Test User Only - Fred Nerk";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Test User Only - Jason Smith";

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_IsActive = true;
			OrgContact contact1 = orgHeader1.Contacts.AddNew();
			contact1.OC_ContactName = "Test1";
			contact1.OC_OH = orgHeader1.PK;
			contact1.OC_IsActive = true;

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_IsActive = true;
			OrgContact contact2 = orgHeader2.Contacts.AddNew();
			contact2.OC_ContactName = "Test2";
			contact2.OC_OH = orgHeader2.PK;
			contact2.OC_IsActive = true;

			contact1.OC_PER = person1.PK;
			contact1.OC_Title = "boss";
			contact1.OC_IsActive = false;

			contact2.OC_PER = person2.PK;
			contact2.OC_Title = "developer";

			var primaryRelationshipPivot = Factory.NewWithValidTestData<GlbPersonPrimaryRelationship>();
			primaryRelationshipPivot.PPR_PER = person1.PK;
			primaryRelationshipPivot.Primary = contact1;

			Factory.Save();
			AssertEquals(contact1, Factory.Load<OrgContact>(contact1.PK));
			AssertEquals(contact1.PK, primaryRelationshipPivot.PPR_PrimaryId);

			AssertEquals(contact1.OC_IsPrimaryContact, true);
			AssertEquals(contact2.OC_IsPrimaryContact, false);
		}

		public void TestClone()
		{
			OrgHeader header = OrgHeader.New(Factory);
			header.OH_RL_NKClosestPort = "AUSYD";
			OrgContact contact = header.Contacts.AddNew();
			contact.OC_ContactName = "Contact to Clone";
			contact.Documents.AddNew();
			contact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact.Documents.AddNew();
			contact.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;

			OrgContact clonedContact = (OrgContact)contact.Clone();
			foreach (ZPropertyInfo property in contact.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				AssertEquals("Property " + property.Name, contact[property.Name], clonedContact[property.Name]);
			}
			AssertEquals("Number of documents", contact.Documents.Count, clonedContact.Documents.Count);
		}

		public void TestOrganisationCode()
		{
			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			OrgContact testContact = Factory.New(typeof(OrgContact)) as OrgContact;

			AssertEquals("No OC_OH", "", testContact.OrganisationCode);
			testContact.OC_OH = testOrg.PK;
			AssertEquals("Org code not set yet", "", testContact.OrganisationCode);
			testOrg.OH_Code = "HELLOWORLD";
			AssertEquals("Org code", "HELLOWORLD", testContact.OrganisationCode);
		}

		public void TestParentOrg()
		{
			Contact.OC_ContactName = "Test Contact";
			Factory.Save();

			OrgContact newContact = Factory.New<OrgContact>();
			newContact.OC_OH = Company.PK;
			AssertNotNull("Contact ParentOrg is not null", newContact.ParentOrg);
			AssertEquals("Contact has correct ParentOrg", Company.PK, newContact.ParentOrg.PK);
		}

		public void TestIContactable()
		{
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "name";
			contact.OC_Email = "email";
			contact.OC_Mobile = "1234567";
			contact.OC_IsActive = false;
			AssertEquals("name", ((IContactable)contact).Name);
			AssertEquals("email", ((IContactable)contact).Email);
			AssertEquals("1234567", ((IContactable)contact).Mobile);
			AssertEquals(false, ((IContactable)contact).IsActive);

			AssertEquals("name", ((IPasswordEmailSource)contact).Salutation);
			contact.OC_Salutation = "salutation";
			AssertEquals("salutation", ((IPasswordEmailSource)contact).Salutation);
			AssertEquals("name", ((IContactable)contact).Name);
		}

		public void TestOrganisationOrAddressOverride()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgHeader orgOverride = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();

			AssertEquals("Should just use the parent organisation if no override", org.PK, contact.OrganisationOrAddressOverride.PK);
			contact.OC_OH_AddressOverride = orgOverride.PK;
			AssertEquals("Should use the org override when defined", orgOverride.PK, contact.OrganisationOrAddressOverride.PK);
		}

		public void TestDefaultLanguage()
		{
			var org = Factory.New<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			AssertEquals("Default language", Constants.Languages.English, contact1.OC_Language);
			contact1.OC_Language = "CHS";
			AssertEquals("Old language code should be corrected after setting", Core.SharedConstants.Languages.ChineseSimplified, contact1.OC_Language);
		}

		public void TestDefaultAttachmentType()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			AssertEquals("Default attachment type should be set to registry", OrganisationsDataRegistry.Instance.DefaultAttachmentType.Value, contact.OC_AttachmentType);
			OrganisationsDataRegistry.Instance.DefaultAttachmentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgConstants.AttachmentType.XLSX);
			var contact1 = org.Contacts.AddNew();
			AssertEquals("Default attachment type should be set to new value when registry is changed", OrgConstants.AttachmentType.XLSX, contact1.OC_AttachmentType);
		}

		public void TestYearsInCompany()
		{
			OrgContact contact = Factory.New<OrgContact>();

			contact.OC_YearJoinedCompany = ZDateTime.Today;
			AssertEquals("Should be 0 (Zero) years", "( 0 Years )", contact.YearsInCompany);

			contact.OC_YearJoinedCompany = ZDateTime.Today.AddYears(-1);
			AssertEquals("Should be 1 Year", "( 1 Year )", contact.YearsInCompany);

			contact.OC_YearJoinedCompany = ZDateTime.Today.AddYears(-10);
			AssertEquals("Should be 10 Years", "( 10 Years )", contact.YearsInCompany);

			contact.OC_YearJoinedCompany = ZDateTime.Today.AddYears(-60);
			AssertEquals("Should be blank", "", contact.YearsInCompany);

			contact.OC_YearJoinedCompany = ZDateTime.Today.AddYears(1);
			AssertEquals("Should be blank", "", contact.YearsInCompany);

			contact.OC_YearJoinedCompany = ZDateTime.Empty;
			AssertEquals("Should be blank", "", contact.YearsInCompany);

			contact.OC_YearJoinedCompany = ZDateTime.Invalid;
			AssertEquals("Should be blank", "", contact.YearsInCompany);
		}

		public void TestYearsInIndustry()
		{
			OrgContact contact = Factory.New<OrgContact>();

			contact.OC_YearJoinedIndustry = ZDateTime.Today;
			AssertEquals("Should be 0 (Zero) years", "( 0 Years )", contact.YearsInIndustry);

			contact.OC_YearJoinedIndustry = ZDateTime.Today.AddYears(-1);
			AssertEquals("Should be 1 Year", "( 1 Year )", contact.YearsInIndustry);

			contact.OC_YearJoinedIndustry = ZDateTime.Today.AddYears(-10);
			AssertEquals("Should be 10 Years", "( 10 Years )", contact.YearsInIndustry);

			contact.OC_YearJoinedIndustry = ZDateTime.Today.AddYears(-60);
			AssertEquals("Should be blank", "", contact.YearsInIndustry);

			contact.OC_YearJoinedIndustry = ZDateTime.Today.AddYears(1);
			AssertEquals("Should be blank", "", contact.YearsInIndustry);

			contact.OC_YearJoinedIndustry = ZDateTime.Empty;
			AssertEquals("Should be blank", "", contact.YearsInIndustry);

			contact.OC_YearJoinedIndustry = ZDateTime.Invalid;
			AssertEquals("Should be blank", "", contact.YearsInIndustry);
		}

		public void TestIGlbCompanyCampaignItemRecipientMembers()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "Org";
			OrgContact orgContact = Factory.New<OrgContact>();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_ContactName = "Alan";

			IGlbCompanyCampaignItemRecipient recipient = orgContact;
			AssertEquals("123YES", recipient.Phone);
			AssertEquals(null, recipient.Organisation);
			AssertEquals("Admiral", recipient.Salutation);
			AssertEquals("Mr.", recipient.Title);
			AssertEquals("293999", recipient.Fax);
			AssertEquals("Contact Alan", recipient.RelatedDocName);

			orgContact.OC_OH = header.PK;
			AssertEquals(header, recipient.Organisation);
			AssertEquals("Contact Alan (Org)", recipient.RelatedDocName);
		}

		public void TestDefaultSalutation()
		{
			OrganisationsDataRegistry.Instance.ContactSalutation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContactSalutationCollection());

			string englishDefault = Enterprise.Core.Constants.DefaultSalutations.DefaultSalutation.ToString(Constants.Languages.EnglishAmerican);
			string chineseDefault = Enterprise.Core.Constants.DefaultSalutations.DefaultSalutation.ToString(Core.Constants.Languages.ChineseSimplified);

			OrgContact contact1 = Factory.NewWithValidTestData(typeof(OrgContact)) as OrgContact;
			OrgContact contact2 = Factory.NewWithValidTestData(typeof(OrgContact)) as OrgContact;
			Factory.Save();

			contact1.OC_Language = Constants.Languages.EnglishAmerican;
			contact1.OC_ContactName = "Contact1";
			contact2.OC_Language = Core.Constants.Languages.ChineseSimplified;
			contact2.OC_ContactName = "Contact2";
			Factory.Save();

			OrgContact c1 = Factory.Load<OrgContact>(contact1.PK);
			OrgContact c2 = Factory.Load<OrgContact>(contact2.PK);
			AssertEquals(1, c1.Salutations.Count);
			AssertEquals(englishDefault.Replace(Core.Constants.SalutationMacros.Name, c1.OC_ContactName), c1.Salutations[0].Code);
			AssertEquals(1, c2.Salutations.Count);
			AssertEquals(chineseDefault.Replace(Core.Constants.SalutationMacros.Name, c2.OC_ContactName), c2.Salutations[0].Code);
		}

		public void TestSalutations()
		{
			ContactSalutationCollection salutations = new ContactSalutationCollection();
			SalutationHelper.LoadDefaultSalutations(salutations);

			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Language = Constants.Languages.EnglishAmerican;
			contact1.OC_Gender = Constants.Genders.Man;
			contact1.OC_ContactName = "Tiny Small";

			List<ContactSalutation> expectedSalutations = salutations.Cast<ContactSalutation>().Where(s => (s.Gender == Constants.SalutationGenders.Man || s.Gender == Constants.SalutationGenders.All)).ToList();
			foreach (ContactSalutation es in expectedSalutations)
			{
				es.Salutation = es.Salutation.Replace(Core.Constants.SalutationMacros.Name, contact1.OC_ContactName).Replace(Core.Constants.SalutationMacros.JobCategory, contact1.OC_JobCategory).Replace("  ", " ").Replace(" ,", ",");
			}

			var salutationList = expectedSalutations.Select(s => s.Salutation.ToString().Trim()).Distinct().ToList();
			AssertEquals(salutationList.Count, contact1.Salutations.Count);
			foreach (CodeDescriptionPair pair in contact1.Salutations)
			{
				Assert(pair.Code, salutationList.Exists(s => s == pair.Code));
				Assert(pair.Code, pair.Code.Length <= contact1.OC_SalutationInfo.MaxLength);
			}

			salutations = new ContactSalutationCollection();
			SalutationHelper.LoadDefaultSalutations(salutations);

			OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Language = Constants.Languages.EnglishAmerican;
			contact2.OC_Gender = Constants.Genders.Woman;
			contact2.OC_ContactName = "Looooooooooooooooooooong Name";
			contact2.OC_JobCategory = "Director of Evveeeeeeeeerrrrything";

			expectedSalutations = salutations.Cast<ContactSalutation>().Where(s => (s.Gender == Constants.SalutationGenders.Woman || s.Gender == Constants.SalutationGenders.All)).ToList();
			foreach (ContactSalutation es in expectedSalutations)
			{
				es.Salutation = es.Salutation.Replace(Core.Constants.SalutationMacros.Name, contact2.OC_ContactName).Replace(Core.Constants.SalutationMacros.JobCategory, contact2.OC_JobCategory).Replace("  ", " ").Replace(" ,", ",");
			}

			salutationList = expectedSalutations.Select(s => s.Salutation.ToString().Trim()).Distinct().ToList();
			AssertNotEquals("overlong salutations removed", salutationList.Count, contact2.Salutations.Count);
			foreach (CodeDescriptionPair pair in contact2.Salutations)
			{
				Assert(salutationList.Exists(s => s == pair.Code));
				Assert(pair.Code.Length <= contact1.OC_SalutationInfo.MaxLength);
			}
		}

		public void TestWebWarehouseEligibility()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var warehouseEligibility = contact.WebWarehouseEligibility;
			AssertNotNull(warehouseEligibility);
			AssertEquals("Collection should be cached", warehouseEligibility, contact.WebWarehouseEligibility);
		}

		public void TestSubscriptionsForContactsWithTheSameEmail()
		{
			const string email = "e@ma.il";
			const string category = "CAT";
			const string type = "TP";

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = email;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = email;

			var subscription1 = contact1.Subscriptions.AddNew();
			subscription1.GCS_Email = email;
			subscription1.GCS_MediaCategory = category;
			subscription1.GCS_IsSubscribed = true;

			var subscription2 = contact1.Subscriptions.AddNew();
			subscription2.GCS_Email = email;
			subscription2.GCS_MediaCategory = category;
			subscription2.GCS_MediaType = type;
			subscription1.GCS_IsSubscribed = false;

			Factory.Save();

			AssertEquals("Subscriptions count", 2, contact1.Subscriptions.Count);
			AssertEquals("Subscriptions count", 2, contact2.Subscriptions.Count);

			AssertCollectionContains(subscription1, contact2.Subscriptions);
			AssertCollectionContains(subscription2, contact2.Subscriptions);
		}

		public void TestSubscriptionsNotLoaded()
		{
			const string email = "e@ma.il";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			Assert("Should not be loaded", !contact.SubscriptionsLoadedAndNotEmpty);

			contact.OC_Email = "";
			Assert("Should not be loaded", !contact.SubscriptionsLoadedAndNotEmpty);
			contact.Validation.ValidateAll();
			Assert("Should not be loaded", !contact.SubscriptionsLoadedAndNotEmpty);

			AddSubscriptionToEmail(email);

			contact.OC_Email = email;
			Assert("Should not be loaded", !contact.SubscriptionsLoadedAndNotEmpty);
			contact.Validation.ValidateAll();
			Assert("Should be loaded", contact.SubscriptionsLoadedAndNotEmpty);
		}

		void AddSubscriptionToEmail(string email)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = email;
			var subscription = contact.Subscriptions.AddNew();
			AssertEquals("Check email", email, subscription.GCS_Email);
		}

		public void TestChangeActiveStatusShouldValidateDocuments()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			var document = contact.Documents.AddNew();
			Factory.Save();
			contact.OC_IsActive = false;

			AssertHasWarning(document.OD_OCInfo, "This Contact is inactive.");
		}

		public void TestJobCategoryDescription_SettingJCDSetsOC_JobCategoryCorrectly()
		{
			var jobCategories = new CodeDescriptionBoolCollection(OrgContactSchema.OC_JobCategory.MaxLength)
						{
								{ "LEA", (NoResString)"CEO/Managing Director", true },
								{ "CSM", (NoResString)"Customer Service Manager", true }
						};
			OrganisationsDataRegistry.Instance.ContactJobCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobCategories);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			contact.JobCategoryDescription = "Customer Service Manager";
			AssertEquals("OC_JobCategory is set to the correct code value.", "CSM", contact.OC_JobCategory);
			AssertEquals("JobCategoryDescription returns correct description value", "Customer Service Manager", contact.JobCategoryDescription);
		}

		public void TestDeduplicationActionOccurredEvent()
		{
			var reg = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				DeduplicationAction actionInvoked = DeduplicationAction.None;
				OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
				var person = contact.Person;
				person.DeduplicationActionOccurred += (o, e) => actionInvoked = e.InvokedAction;

				person.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null);
				AssertEquals(DeduplicationAction.None, actionInvoked);
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				person.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null);
				AssertEquals(DeduplicationAction.Ignore, actionInvoked);
				person.PropagateDeduplicationActionOccurred(DeduplicationAction.Link, null, null, null);
				AssertEquals(DeduplicationAction.Link, actionInvoked);
				person.PropagateDeduplicationActionOccurred(DeduplicationAction.Merge, null, null, null);
				AssertEquals(DeduplicationAction.Merge, actionInvoked);
				person.PropagateDeduplicationActionOccurred(DeduplicationAction.OpenMaster, null, null, null);
				AssertEquals(DeduplicationAction.OpenMaster, actionInvoked);
				person.PropagateDeduplicationActionOccurred(DeduplicationAction.OpenTarget, null, null, null);
				AssertEquals(DeduplicationAction.OpenTarget, actionInvoked);
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);
			}
		}
		public void TestSettingFullNameDoesNotInvokeDeduplication()
		{
			//Arrange
			var factory = new BusinessObjectFactory();
			var testContact = factory.NewWithValidTestData<OrgContact>();
			var dedupeStarted = false;
			testContact.Person.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testContact.Person).ShouldRunDeduplication = false;

			//Act
			testContact.OC_ContactName = "Dr Crayfish";

			//Assert
			AssertEquals(false, dedupeStarted);
		}

		public void TestSettingFullNameInvokesDeduplication()
		{
			//Arrange
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var dedupeStarted = false;
			var factory = new BusinessObjectFactory();
			var testContact = factory.NewWithValidTestData<OrgContact>();

			factory.Save();
			testContact.Person.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testContact.Person).ShouldRunDeduplication = true;

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				//Act
				testContact.OC_ContactName = "Dr Crayfish";
				factory.Save();

				//Assert
				AssertEquals(true, dedupeStarted);
			}

			testContact.Person.DeduplicationStarted -= (o, e) => { dedupeStarted = true; };
			AsyncHelper.WaitAllActiveTasksForTest();
		}

		public void TestDedupeItemsAreAlwaysSynchronisedToPerson()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			var person = contact.Person;
			contact.OC_ContactName = "New Name";
			contact.OC_Birthday = new ZDate(1995, 4, 9);
			contact.OC_HomePhone = "+61 455 555 551";
			contact.OC_Mobile = "+61 455 555 551";

			contact.OC_RN_NKNationality = "AU";
			Factory.Save();

			AssertEquals("Name should be synchronised between contact and person", contact.Name, person.PER_FullName);
			AssertEquals("Birthdays should be synchronised between contact and person", contact.OC_Birthday, person.PER_BirthDate);
			AssertEquals("Home phone should be synchronised between contact and person", contact.OC_HomePhone, person.PER_HomePhone);
			AssertEquals("Mobile phone should be synchronised between contact and person", contact.OC_Mobile, person.PER_MobilePhone);
			AssertEquals("Nationality should be synchronised between contact and person", contact.OC_RN_NKNationality, person.PER_RN_NKNationalityCodeISO);
		}

		public void TestPreventHookTriggerFindSuggestedJobCategoriesMultipleTimes()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			var hookTimes = 0;
			contact.TriggerFindSuggestedJobCategories += (sender, e) => { hookTimes++; };
			contact.TriggerFindSuggestedJobCategories += (sender, e) => { hookTimes++; };
			contact.TriggerFindSuggestedJobCategories += (sender, e) => { hookTimes++; };
			contact.TriggerFindSuggestedJobCategories += (sender, e) => { hookTimes++; };

			contact.OC_Title = "NewTitle";
			AssertEquals("Event was only hooked once.", 1, hookTimes);
		}

		#region Patterns

		public void TestDeleteAllPatternsWhenDeletingOrgContact()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();

			var contactPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, orgContact);
			Factory.Save();
			orgContact.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Contact should be deleted", true, orgContact.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Contact patterns should be deleted", Array.Empty<BusinessObject>(), contactPatterns.Where(p => !p.IsDeleted));
			});
		}

		public void TestDeleteAllPatternsWhenDeletingOrgContact_DoesNotDeleteParentPatternsOrResults()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var personPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, person);
			var personResults = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingResults(Factory, person);

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_PER = person.PK;

			Factory.Save();
			orgContact.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Contact should be deleted", true, orgContact.IsDeleted);
				AssertEquals("Person should not be deleted", false, person.IsDeleted);

				AssertContainsExactElementsInAnyOrder("Person patterns should not be deleted", Array.Empty<BusinessObject>(), personPatterns.Where(p => p.IsDeleted));
				AssertContainsExactElementsInAnyOrder("Person results should not be deleted", Array.Empty<BusinessObject>(), personResults.Where(p => p.IsDeleted));
			});
		}

		#region Certificate Patterns

		public void TestDeleteContactDeletesCertificatePatterns_ButDoesNotDeleteCertificates()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var patterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, certificate);

			certificate.XZ_ParentID = contact.PK;
			certificate.XZ_ParentTableCode = contact.TablePrefix;

			Factory.Save();
			contact.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Contact should be deleted", true, contact.IsDeleted);
				AssertEquals("Certificate should not be deleted", false, certificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Certificate patterns should all be deleted", Array.Empty<BusinessObject>(), patterns.Where(p => !p.IsDeleted));
			});
		}

		public void TestDeleteContactCertificatesDeletesAllCertificatesAndPatterns_ButDoesNotDeleteContactOrContactPatterns()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var contactPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, contact);

			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var certificatePatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, certificate);

			certificate.XZ_ParentID = contact.PK;
			certificate.XZ_ParentTableCode = contact.TablePrefix;

			Factory.Save();
			contact.Certificates.DeleteAll();

			CombineAssertions(() =>
			{
				AssertEquals("Contact should not be deleted", false, contact.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Contact patterns should not be deleted", Array.Empty<BusinessObject>(), contactPatterns.Where(p => p.IsDeleted));

				AssertEquals("Certificate should be deleted", true, certificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Certificate patterns should all be deleted", Array.Empty<BusinessObject>(), certificatePatterns.Where(p => !p.IsDeleted));
			});
		}

		#endregion

		#endregion

		#region ContactItems

		public void TestGetAllLinkedForOrgContactItems()
		{
			var contactItem = Factory.NewWithValidTestData<OrgContactItem>();
			var contactItem1 = Factory.NewWithValidTestData<OrgContactItem>();
			contactItem.OI_OC = Contact.PK;
			AssertEquals("1 contact item expected in ContactItems", 1, Contact.ContactItems.Count);
			AssertCollectionContains("ContactItems", contactItem, Contact.ContactItems);
			AssertCollectionNotContains("ContactItems", contactItem1, Contact.ContactItems);
			contactItem1.OI_OC = Contact.PK;
			AssertEquals("2 contact items expected in ContactItems", 2, Contact.ContactItems.Count);
			AssertCollectionContains("ContactItems", contactItem1, Contact.ContactItems);
		}

		public void TestContactsReturnsEmptyCollection()
		{
			AssertEquals("Newly added OrgContact ContactItems count", 0, Contact.ContactItems.Count);
		}

		#endregion

		#region Phone IsManuallyVerified

		public void TestOC_Fax_IsManuallyVerified()
		{
			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgContact.Schema.OC_Fax_IsManuallyVerified, OrgContactSchema.Constants.Prefix, OrgContactSchema.Constants.OC_Fax, orgContact1, orgContact2);
		}

		public void TestOC_HomePhone_IsManuallyVerified()
		{
			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgContact.Schema.OC_HomePhone_IsManuallyVerified, OrgContactSchema.Constants.Prefix, OrgContactSchema.Constants.OC_HomePhone, orgContact1, orgContact2);
		}

		public void TestOC_Mobile_IsManuallyVerified()
		{
			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgContact.Schema.OC_Mobile_IsManuallyVerified, OrgContactSchema.Constants.Prefix, OrgContactSchema.Constants.OC_Mobile, orgContact1, orgContact2);
		}

		public void TestOC_OtherPhone_IsManuallyVerified()
		{
			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgContact.Schema.OC_OtherPhone_IsManuallyVerified, OrgContactSchema.Constants.Prefix, OrgContactSchema.Constants.OC_OtherPhone, orgContact1, orgContact2);
		}

		public void TestOC_Pager_IsManuallyVerified()
		{
			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgContact.Schema.OC_Pager_IsManuallyVerified, OrgContactSchema.Constants.Prefix, OrgContactSchema.Constants.OC_Pager, orgContact1, orgContact2);
		}

		public void TestOC_Phone_IsManuallyVerified()
		{
			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgContact.Schema.OC_Phone_IsManuallyVerified, OrgContactSchema.Constants.Prefix, OrgContactSchema.Constants.OC_Phone, orgContact1, orgContact2);
		}

		public void TestSettingPhoneNumbersResetsIsManuallyVerifiedFlag()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			contact.OC_Fax_IsManuallyVerified = true;
			Assert("Precondition", contact.OC_Fax_IsManuallyVerified);
			contact.OC_Fax_Formatted = "+61 425 465 800";
			Assert(!contact.OC_Fax_IsManuallyVerified);

			contact.OC_HomePhone_IsManuallyVerified = true;
			Assert("Precondition", contact.OC_HomePhone_IsManuallyVerified);
			contact.OC_HomePhone_Formatted = "+61 425 465 800";
			Assert(!contact.OC_HomePhone_IsManuallyVerified);

			contact.OC_Mobile_IsManuallyVerified = true;
			Assert("Precondition", contact.OC_Mobile_IsManuallyVerified);
			contact.OC_Mobile_Formatted = "+61 425 465 800";
			Assert(!contact.OC_Mobile_IsManuallyVerified);

			contact.OC_OtherPhone_IsManuallyVerified = true;
			Assert("Precondition", contact.OC_OtherPhone_IsManuallyVerified);
			contact.OC_OtherPhone_Formatted = "+61 425 465 800";
			Assert(!contact.OC_OtherPhone_IsManuallyVerified);

			contact.OC_Pager_IsManuallyVerified = true;
			Assert("Precondition", contact.OC_Pager_IsManuallyVerified);
			contact.OC_Pager_Formatted = "+61 425 465 800";
			Assert(!contact.OC_Pager_IsManuallyVerified);

			contact.OC_Phone_IsManuallyVerified = true;
			Assert("Precondition", contact.OC_Phone_IsManuallyVerified);
			contact.OC_Phone_Formatted = "+61 425 465 800";
			Assert(!contact.OC_Phone_IsManuallyVerified);
		}

		#endregion

		#region Related Business Objects

		public void TestDocuments()
		{
			AssertEquals("Newly added Documents count", 0, Contact.Documents.Count);
		}

		public void TestEffectiveContactAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			AssertEquals("Should take the address off the OrgHeader.MainAddress initially", contact.EffectiveContactAddress.PK, org.MainAddress.PK);

			contact.OC_OH_AddressOverride = Factory.New(typeof(OrgHeader)).PK;
			AssertEquals("Should take the address off the OrgContactSchema.Constants.OC_OA_OrgAddress when populated", contact.EffectiveContactAddress.PK, contact.AddressOverride.MainAddress.PK);

			OrgAddress addressForOverride = (Factory.New<OrgHeader>()).Addresses.AddNew();
			contact.OC_OA_OrgAddress = addressForOverride.PK;
			AssertEquals("Should take the address off OrgContact.OrgAddress when populated", contact.EffectiveContactAddress.PK, addressForOverride.PK);
		}

		public void TestEffectiveContactAddressDoesntGetNullRefException()
		{
			OrgContact nullContact = Factory.GetNull<OrgContact>();
			AssertEquals("nullContact.EffectiveContactAddress", null, nullContact.EffectiveContactAddress);
		}

		#endregion

		#region Property overrides

		public void TestOC_AttachmentTypeReadOnly()
		{
			Contact.OC_NotifyMode = Constants.ContactNotifyModes.Email;
			Assert("OC_AttachmentType should not be readonly", !Contact.OC_AttachmentTypeInfo.ReadOnly);
			AssertEquals("OC_AttachmentType", OrgConstants.AttachmentType.PDF, Contact.OC_AttachmentType);

			Contact.OC_NotifyMode = Constants.ContactNotifyModes.EPrint;
			Assert("OC_AttachmentType should not be readonly", !Contact.OC_AttachmentTypeInfo.ReadOnly);
			AssertEquals("OC_AttachmentType", OrgConstants.AttachmentType.PDF, Contact.OC_AttachmentType);

			Contact.OC_NotifyMode = Constants.ContactNotifyModes.Fax;
			Assert("OC_AttachmentType should be readonly", Contact.OC_AttachmentTypeInfo.ReadOnly);
			AssertEquals("OC_AttachmentType", "", Contact.OC_AttachmentType);
		}

		public void TestPhone_WithOverridingAddressWithCountryCode()
		{
			// Arrange
			var overridingAddressHeader = Factory.NewWithValidTestData<OrgHeader>();
			var overridingAddress = overridingAddressHeader.Addresses.AddNew();
			overridingAddress.FillWithValidTestData();
			overridingAddress.OA_RN_NKCountryCode = "CN";
			Contact.OC_OA_OrgAddress = overridingAddress.PK;
			// Act
			Contact.OC_Phone_Formatted = "010 65 28 16 49";
			// Assert
			ZString expected = "+861065281649";
			ZString expectedFormatted = "+86 10 6528 1649";
			AssertEquals("Number saved correctly", expected, Contact.OC_Phone);
			AssertEquals("Number formatted correctly", expectedFormatted, Contact.OC_Phone_Formatted);
		}

		public void TestFax_WithOverridingAddressWithUnloco()
		{
			// Arrange
			var overridingAddressHeader = Factory.NewWithValidTestData<OrgHeader>();
			var overridingAddress = overridingAddressHeader.Addresses.AddNew();
			overridingAddress.FillWithValidTestData();
			overridingAddress.OA_RL_NKRelatedPortCode = "AU";
			Contact.OC_OA_OrgAddress = overridingAddress.PK;
			// Act
			Contact.OC_Fax_Formatted = "2-9025-1100";
			// Assert
			ZString expected = "+61290251100";
			ZString expectedFormatted = "+61 2 9025 1100";
			AssertEquals("Number saved correctly", expected, Contact.OC_Fax);
			AssertEquals("Number formatted correctly", expectedFormatted, Contact.OC_Fax_Formatted);
		}

		public void TestMobile_WithOverridingHeaderWithAddressesFromTheSameCountry()
		{
			// Arrange
			var overridingHeader = Factory.NewWithValidTestData<OrgHeader>();
			overridingHeader.MainAddress.OA_RN_NKCountryCode = "CN";
			Contact.OC_OH_AddressOverride = overridingHeader.PK;
			// Act
			Contact.OC_Mobile_Formatted = "156 011 3 19 81";
			// Assert
			ZString expected = "+8615601131981";
			ZString expectedFormatted = "+86 156 0113 1981";
			AssertEquals("Number saved correctly", expected, Contact.OC_Mobile);
			AssertEquals("Number formatted correctly", expectedFormatted, Contact.OC_Mobile_Formatted);
		}

		public void TestHomePhone_WithHeaderWithAddressesFromTheSameCountry()
		{
			// Arrange
			Company.MainAddress.OA_RN_NKCountryCode = "AU";
			// Act
			Contact.OC_HomePhone_Formatted = "2 9025 1100";
			// Assert
			ZString expected = "+61290251100";
			ZString expectedFormatted = "+61 2 9025 1100";
			AssertEquals("Number saved correctly", expected, Contact.OC_HomePhone);
			AssertEquals("Number formatted correctly", expectedFormatted, Contact.OC_HomePhone_Formatted);
		}

		public void TestPager()
		{
			Contact.OC_Pager_Formatted = "61 2 9025 1100";
			ZString expected = "+61290251100";
			ZString expectedFormatted = "+61 2 9025 1100";
			AssertEquals("Number saved correctly", expected, Contact.OC_Pager);
			AssertEquals("Number formatted correctly", expectedFormatted, Contact.OC_Pager_Formatted);
		}

		public void TestOC_OtherPhone()
		{
			Contact.OC_OtherPhone_Formatted = "61 2 9025 1101";
			ZString expected = "+61290251101";
			ZString expectedFormatted = "+61 2 9025 1101";
			AssertEquals("Number saved correctly", expected, Contact.OC_OtherPhone);
			AssertEquals("Number formatted correctly", expectedFormatted, Contact.OC_OtherPhone_Formatted);
		}

		public void TestOC_WebAccessEnabled_ReadOnly()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Assert(!contact.OC_WebAccessEnabled_ReadOnly);
		}

		public void TestIsValidForWebLogin()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = true;
			Assert(!contact.IsValidForWebLogin);

			contact.SetHashedPassword("1234");
			Assert(contact.IsValidForWebLogin);

			contact.OC_WebAccessEnabled = false;
			Assert(!contact.IsValidForWebLogin);

			contact.OC_IsActive = false;
			contact.OC_WebAccessEnabled = true;
			Assert(!contact.IsValidForWebLogin);
		}

		public void TestOC_WebAccessEnabledShouldNotLoadOtherSecurities()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var security1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_Granted, true))[0];
			var security2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_Granted, false))[0];

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Alex";
			contact.OC_Email = "alex@alex.xx";
			var otherContact = org.Contacts.AddNew();
			otherContact.OC_ContactName = "Blex";
			otherContact.OC_Email = "blex@blex.xx";

			contact.OC_WebAccessEnabled = true;
			otherContact.OC_WebAccessEnabled = true;

			var securityContact = security1.ContactSecurityRights.Where(z => z.Contact.OC_ContactName == "Alex").First();
			var securityOtherContact = security2.ContactSecurityRights.Where(z => z.Contact.OC_ContactName == "Blex").First();
			securityContact.OZ_Granted = false;
			securityContact.HasChanges = true;
			securityOtherContact.OZ_Granted = true;
			securityOtherContact.HasChanges = true;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadedSecurityContact = factory2.Load<OrgSecurityContacts>(securityContact.PK);
			AssertNotNull("Security should exist in db", loadedSecurityContact);
			loadedSecurityContact = factory2.Load<OrgSecurityContacts>(securityOtherContact.PK);
			AssertNotNull("Security should exist in db", loadedSecurityContact);

			contact.OC_WebAccessEnabled = false;
			Factory.Save();

			loadedSecurityContact = factory2.Load<OrgSecurityContacts>(securityContact.PK);
			AssertNull("Db should have deleted the first security contact", loadedSecurityContact);

			factory2 = new BusinessObjectFactory();
			var reloadedContact = factory2.Load<OrgContact>(contact.PK);
			reloadedContact.OC_WebAccessEnabled = true;

			var loadedSecurityContacts = ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.Select(x => x as OrgSecurityContacts).WhereNotNull().ToList();
			AssertEquals("No other security contacts should be loaded when OC_WebAccessEnabled is set", 0, loadedSecurityContacts.Count);
		}

		#endregion

		#region TestGetNumberForPhoneType

		public void TestGetNumberForPhoneType()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Test Address";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_HomePhone = "111";
			contact.OC_Mobile = "222";
			contact.OC_Phone = "333";
			contact.OC_OtherPhone = "999";

			AssertEquals("111", contact.GetNumberForPhoneType(PhoneTypeList.Codes.HOM));
			AssertEquals("222", contact.GetNumberForPhoneType(PhoneTypeList.Codes.MOB));
			AssertEquals("333", contact.GetNumberForPhoneType(PhoneTypeList.Codes.WRK));
			AssertEquals("999", contact.GetNumberForPhoneType(PhoneTypeList.Codes.OTH));
			AssertEquals("", contact.GetNumberForPhoneType("BADTYPE"));

			contact.OC_OA_OrgAddress = org.MainAddress.PK;
			contact.BranchAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			contact.OC_HomePhone = "02 5555 9999";
			contact.OC_Mobile = "0420 019 999";
			contact.OC_OtherPhone = "02 3333 6666";
			contact.OC_Phone = "0420 011 022";

			AssertEquals("+61 2 5555 9999", contact.GetNumberForPhoneType(PhoneTypeList.Codes.HOM, true));
			AssertEquals("+61 420 019 999", contact.GetNumberForPhoneType(PhoneTypeList.Codes.MOB, true));
			AssertEquals("+61 2 3333 6666", contact.GetNumberForPhoneType(PhoneTypeList.Codes.OTH, true));
			AssertEquals("+61 420 011 022", contact.GetNumberForPhoneType(PhoneTypeList.Codes.WRK, true));
			AssertEquals("", contact.GetNumberForPhoneType("BADTYPE"));
		}

		#endregion

		#region Setters for Overrides

		public void TestSetOrganisationForAddressOverride()
		{
			Contact.OC_OH = Company.PK;
			Contact.OnLoaded();
			AssertEquals("Address control organisation PK", Company.PK, Contact.WorkingAddressPK_ZAddress.OrgPK);
			AssertEquals("Address control organisation PK", Company.MainAddress.PK, Contact.WorkingAddressPK);

			var companyOverride = Factory.New<OrgHeader>();
			Contact.OC_OH_AddressOverride = companyOverride.PK;
			AssertEquals("Address control organisation PK", companyOverride.PK, Contact.WorkingAddressPK_ZAddress.OrgPK);
			AssertEquals("Address control organisation PK", Contact.OC_OA_OrgAddress, Contact.WorkingAddressPK);

			Contact.OC_OH_AddressOverride = ZGuid.Empty;
			AssertEquals("Address control organisation PK", Company.PK, Contact.WorkingAddressPK_ZAddress.OrgPK);
			AssertEquals("Address control organisation PK", Company.MainAddress.PK, Contact.WorkingAddressPK);
		}

		public void TestOrgAddressOverride()
		{
			BusinessObjectFactory saveFactory = new BusinessObjectFactory();
			OrgHeader org = saveFactory.New<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.MainAddress.OA_Address1 = "Test Address";
			org.OH_RL_NKClosestPort = "AUSYD";

			OrgAddress address = org.Addresses.AddNew();
			address.OA_Address1 = "Address Override";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;

			saveFactory.Save();

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			OrgHeader loadedOrg = loadFactory.Load<OrgHeader>(org.PK);

			AssertEquals("Org.Contacts.Count", 1, loadedOrg.Contacts.Count);
			AssertNotNull("Address override for contact should not be null", loadedOrg.Contacts[0].OC_OA_OrgAddress);
			Assert("Address override for contact should not be empty guid", loadedOrg.Contacts[0].OC_OA_OrgAddress != ZGuid.Empty);
		}

		public void TestOrgContactMaster()
		{
			BusinessObjectFactory saveFactory = new BusinessObjectFactory();
			OrgHeader org = saveFactory.New<OrgHeader>();
			org.OH_FullName = "Master Organisation";
			org.MainAddress.OA_Address1 = "Address 1 on Master";
			org.OH_RL_NKClosestPort = "AUSYD";

			OrgAddress address = org.Addresses.AddNew();
			address.OA_Address1 = "Address Override For Master";

			OrgContact newContactFromOrgHeader = org.Contacts.AddNew();
			newContactFromOrgHeader.OC_OA_OrgAddress = address.PK;

			saveFactory.Save();

			OrgContact contactFromFactory = Factory.Load<OrgContact>(newContactFromOrgHeader.PK);
			AssertNotNull("Master should be retreived even if it was from the factory", contactFromFactory.ParentOrg);
		}

		#endregion

		#region CodeLists

		//		public void TestOC_Type_List()
		//		{
		//			Assert("OC_Type_List.Count > 0", Contact.OC_Type_List.Count > 0);
		//		}

		public void TestOC_NotifyMode_List()
		{
			Assert("OC_NotifyMode_List.Count > 0", Contact.OC_NotifyMode_List.Count > 0);
		}

		public void TestOC_AttachmentType_List()
		{
			Assert("OC_AttachmentType_List.Count > 0", Contact.OC_AttachmentType_List.Count > 0);
		}

		#endregion

		#region Document Delivery Details

		public void TestPortOverrideOnAddressUsedForDelivery()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_RL_NKClosestPort = "NZAKL";

			OrgAddress address = org.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			address.OA_Address1 = "Hello";
			address.OA_RL_NKRelatedPortCode = "INBOM";

			OrgContact contact = org.Contacts.AddNew();
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;
			contact.WorkingAddressPK = address.PK;
			DocDeliveryContact deliveryContact = contact.DocDeliveryDetails(null, menuItem);
			AssertEquals("UNLOCO from Address taken rather than from Org", "INBOM", deliveryContact.UNLOCO.Code);

			address.OA_RL_NKRelatedPortCode = "";
			deliveryContact = contact.DocDeliveryDetails(null, menuItem);
			AssertEquals("UNLOCO from Org taken as Address has no UNLOCO", "NZAKL", deliveryContact.UNLOCO.Code);
		}

		public void TestDocDeliveryDetails()
		{
			Contact.OC_Email = "Company1 Email";
			Company.OH_RL_NKClosestPort = "USBTV";
			AddAddressDetailsToOrg(Company, "Company1");
			DocDeliveryContact expectedDetails = CreateNewDocDeliveryContact(Contact, Company);

			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(null, null), expectedDetails);
		}

		public void TestContactHasAddressOverrideToAddressFromSameOrganisation()
		{
			AddAddressDetailsToOrg(Company, "Company1");

			OrgAddress overrideAddress = Company.Addresses.AddNew();
			AddAddressDetailsToAddress(overrideAddress, "Override", "USLAX");
			overrideAddress.OA_CompanyNameOverride = "Company Name Override";

			Contact.OC_OA_OrgAddress = overrideAddress.PK;

			DocDeliveryContact expectedContact = CreateNewDocDeliveryContact(Contact, Company);
			expectedContact.CompanyName = overrideAddress.OA_CompanyNameOverride;
			expectedContact.Address1 = overrideAddress.OA_Address1;
			expectedContact.Address2 = overrideAddress.OA_Address2;
			expectedContact.City = overrideAddress.OA_City;
			expectedContact.State = overrideAddress.OA_State;
			expectedContact.PostCode = overrideAddress.OA_PostCode;
			expectedContact.Phone = overrideAddress.OA_Phone;
			expectedContact.Fax = overrideAddress.OA_Fax;
			expectedContact.Email = overrideAddress.OA_Email;
			expectedContact.UNLOCO = overrideAddress.Header.UNLOCO;

			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(null, null), expectedContact);
		}

		public void TestCorrectCompanyNameAndPhoneNumbers()
		{
			Company.OH_FullName = "Normal Name";
			Contact.OC_Email = "Main Email";
			AddAddressDetailsToAddress(Company.MainAddress, "Main", "AUSYD");
			Company.MainAddress.OA_CompanyNameOverride = "Main Name Override";
			Company.MainAddress.OA_Phone = "Main Phone";
			Company.MainAddress.OA_Fax = "Main Fax";
			Company.MainAddress.OA_Email = "Main Email";

			DocDeliveryContact expectedContact = CreateNewDocDeliveryContact(Contact, Company, Company.MainAddress);
			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(null, null), expectedContact);

			OrgAddress aRMAddress = Company.Addresses.AddNew();
			AddAddressDetailsToAddress(aRMAddress, "ARM", "AUBNE");
			aRMAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			aRMAddress.OA_CompanyNameOverride = "ARM Name Override";
			aRMAddress.OA_Phone = "ARM Phone";
			aRMAddress.OA_Fax = "ARM Fax";
			aRMAddress.OA_Email = "ARM Email";
			Contact.OC_Email = aRMAddress.OA_Email;

			expectedContact = CreateNewDocDeliveryContact(Contact, Company, aRMAddress);
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;
			Contact.WorkingAddressPK = aRMAddress.PK;
			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(null, menuItem), expectedContact);
		}

		public void TestContactHasOrganisationOverride()
		{
			StmMenuItem wHSMenuItem = CreateNewMenuItem(ContactType.Warehouse.Code);
			AddAddressDetailsToOrg(Company, "Company1");

			OrgHeader orgOverride = Factory.New<OrgHeader>();
			AddAddressDetailsToOrg(orgOverride, "Override");
			orgOverride.OH_RL_NKClosestPort = "USLAX";

			Contact.OC_OH_AddressOverride = orgOverride.PK;
			DocDeliveryContact expectedContact = CreateNewDocDeliveryContact(Contact, orgOverride);
			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(null, null), expectedContact);
		}

		public void TestContactHasAddressOverrideToAddressFromDifferentOrganisation()
		{
			AddAddressDetailsToOrg(Company, "Company1");

			OrgHeader orgOverride = Factory.New<OrgHeader>();
			AddAddressDetailsToOrg(orgOverride, "Override");
			orgOverride.OH_RL_NKClosestPort = "USLAX";

			OrgAddress overrideAddress = orgOverride.Addresses.AddNew();
			AddAddressDetailsToAddress(overrideAddress, "Address", "USNYC");
			overrideAddress.OA_CompanyNameOverride = "Company Name Override";

			Contact.OC_OH_AddressOverride = orgOverride.PK;
			Contact.OC_OA_OrgAddress = overrideAddress.PK;

			DocDeliveryContact expectedContact = CreateNewDocDeliveryContact(Contact, orgOverride);
			expectedContact.CompanyName = overrideAddress.OA_CompanyNameOverride;
			expectedContact.Address1 = overrideAddress.OA_Address1;
			expectedContact.Address2 = overrideAddress.OA_Address2;
			expectedContact.City = overrideAddress.OA_City;
			expectedContact.State = overrideAddress.OA_State;
			expectedContact.PostCode = overrideAddress.OA_PostCode;
			expectedContact.Phone = overrideAddress.OA_Phone;
			expectedContact.Fax = overrideAddress.OA_Fax;
			expectedContact.Email = overrideAddress.OA_Email;

			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(null, null), expectedContact);
		}

		public void TestContactAttachmentType()
		{
			Contact.OC_NotifyMode = "EML";
			Contact.OC_AttachmentType = "PDF";

			DocDeliveryContact expectedDetails = CreateNewDocDeliveryContact(Contact, Company);
			expectedDetails.DeliveryMethod = Contact.OC_NotifyMode;
			expectedDetails.AttachmentType = Contact.OC_AttachmentType;
			expectedDetails.Email = Contact.OC_Email;
			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(null, null), expectedDetails);

			OrgDocument document = Contact.Documents.AddNew();
			document.OD_DeliverBy = "EML";
			document.OD_AttachmentType = "XLSX";
			expectedDetails.DeliveryMethod = document.OD_DeliverBy;
			expectedDetails.AttachmentType = document.OD_AttachmentType;
			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(document, null), expectedDetails);
		}

		public void TestSendIndividually()
		{
			Contact.OC_NotifyMode = "EML";
			Contact.OC_AttachmentType = "PDF";

			var expectedDetails = CreateNewDocDeliveryContact(Contact, Company);
			expectedDetails.DeliveryMethod = Contact.OC_NotifyMode;
			expectedDetails.AttachmentType = Contact.OC_AttachmentType;
			expectedDetails.Email = Contact.OC_Email;
			expectedDetails.SendIndividually = false;
			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(null, null), expectedDetails);

			Contact.OC_NotifyMode = "PRN";
			expectedDetails.DeliveryMethod = Contact.OC_NotifyMode;
			expectedDetails.SendIndividually = true;
			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(null, null), expectedDetails);

			var document = Contact.Documents.AddNew();
			document.OD_DeliverBy = "EML";
			document.OD_AttachmentType = "PDF";
			document.OD_SendIndividually = true;
			expectedDetails.DeliveryMethod = document.OD_DeliverBy;
			expectedDetails.AttachmentType = document.OD_AttachmentType;
			expectedDetails.SendIndividually = document.OD_SendIndividually;
			AssertContactDetailsAreCorrect(Contact.DocDeliveryDetails(document, null), expectedDetails);
		}

		public void TestContactCorrectDeliveryMethodAndEmailSubjectMacro()
		{
			Contact.OC_NotifyMode = "EML";
			Contact.OC_AttachmentType = "PDF";
			Contact.OC_Email = "Company1 Email";

			OrgDocument document = Contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Warehouse.Code;
			document.OD_DeliverBy = "PRN";
			document.OD_AttachmentType = "";
			document.OD_EmailSubjectMacro = "Some <Macro>";
			var carbonCopyRecipient = document.CarbonCopyRecipients.AddNew();
			carbonCopyRecipient.ODR_EmailAddress = "test1@test.com";
			document.CarbonCopyRecipients.Add(carbonCopyRecipient);
			var blindCarbonCopyRecipient = document.BlindCarbonCopyRecipients.AddNew();
			blindCarbonCopyRecipient.ODR_EmailAddress = "test2@test.com";
			document.BlindCarbonCopyRecipients.Add(blindCarbonCopyRecipient);

			Company.OH_RL_NKClosestPort = "USBTV";
			AddAddressDetailsToOrg(Company, "Company1");
			DocDeliveryContact expectedDetails = CreateNewDocDeliveryContact(Contact, Company);
			expectedDetails.DeliveryMethod = document.OD_DeliverBy;
			expectedDetails.AttachmentType = document.OD_AttachmentType;
			expectedDetails.EmailSubjectMacro = "Some <Macro>";

			var actualDetails = Contact.DocDeliveryDetails(document, null);
			AssertContactDetailsAreCorrect(actualDetails, expectedDetails);
			AssertEquals(carbonCopyRecipient.ODR_EmailAddress, actualDetails.EmailCarbonCopyRecipientsAsString);
			AssertEquals(blindCarbonCopyRecipient.ODR_EmailAddress, actualDetails.EmailBlindCarbonCopyRecipientsAsString);
		}

		public void TestAddressDetailsWhenNoMaster()
		{
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Test Contact";

			DocDeliveryContact details = contact.DocDeliveryDetails(null, null);
			AssertEquals("Delivery details Address1", "*** NO ORGANIZATION DETAILS FOUND ***", details.Address1);
		}

		public void TestCorrectAddressForDocument()
		{
			OrgAddress aRMAddress = Company.Addresses.AddNew();
			aRMAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			aRMAddress.OA_Address1 = "ARM Address";

			OrgAddress aPMAddress = Company.Addresses.AddNew();
			aPMAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			aPMAddress.OA_Address1 = "APM Address";

			OrgAddress pSTAddress = Company.Addresses.AddNew();
			pSTAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);
			pSTAddress.OA_Address1 = "PST Address";

			Company.MainAddress.OA_Address1 = "Main Address";

			OrgDocument doc = Contact.Documents.AddNew();

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;
			Contact.WorkingAddressPK = aRMAddress.PK;
			DocDeliveryContact deliveryDetails = Contact.DocDeliveryDetails(doc, menuItem);
			AssertEquals("ARM Address was returned", deliveryDetails.Address1, aRMAddress.OA_Address1);

			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Payables;
			Contact.WorkingAddressPK = aPMAddress.PK;
			deliveryDetails = Contact.DocDeliveryDetails(doc, menuItem);
			AssertEquals("APM Address was returned", deliveryDetails.Address1, aPMAddress.OA_Address1);

			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Office;
			Contact.WorkingAddressPK = Company.MainAddress.PK;
			deliveryDetails = Contact.DocDeliveryDetails(doc, menuItem);
			AssertEquals("OFC Address was returned", deliveryDetails.Address1, Company.MainAddress.OA_Address1);

			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Office;
			doc.OD_DeliverBy = Constants.ContactNotifyModes.Print;
			Contact.WorkingAddressPK = pSTAddress.PK;
			deliveryDetails = Contact.DocDeliveryDetails(doc, menuItem);
			AssertEquals("PST Address was returned", deliveryDetails.Address1, pSTAddress.OA_Address1);
		}

		public void TestDeliveryContactMenuItemSet()
		{
			StmMenuItem wHSMenuItem = CreateNewMenuItem(ContactType.Warehouse.Code);

			DocDeliveryContact deliveryContact = Contact.DocDeliveryDetails(null, wHSMenuItem);
			AssertEquals("Menu Item set on the Delivery contact", wHSMenuItem, deliveryContact.MenuItem);
		}

		#endregion

		#region ReadOnly Security

		public void TestPersonalAttributesCollectionIsReadOnly()
		{
			bool oldContactModifyValue = Env.Security.OrgContactModify.IsAllowed;

			try
			{
				OrgContact testContact = OrgInDB.Contacts.AddNew();
				Env.Security.OrgContactModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testContact.Attributes.ReadOnly);

				Env.Security.OrgContactModify.IsAllowed = false;
				ResetOrgInDB();
				testContact = OrgInDB.Contacts.AddNew();
				Assert("Access Disallowed - ReadOnly", testContact.Attributes.ReadOnly);
			}
			finally
			{
				Env.Security.OrgContactModify.IsAllowed = oldContactModifyValue;
			}
		}

		public void TestAllocationCollectionIsReadOnly()
		{
			bool oldAllocationInfoValue = Env.Security.OrgContactModify.IsAllowed;
			bool oldValue = Env.Security.OrgContactViewPersonalInformation.IsAllowed;

			try
			{
				var testContact = OrgInDB.Contacts.AddNew();
				Env.Security.OrgContactModify.IsAllowed = true;
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testContact.Allocations.ReadOnly);

				Env.Security.OrgContactModify.IsAllowed = false;
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = false;
				ResetOrgInDB();
				testContact = OrgInDB.Contacts.AddNew();
				Assert("Access Disallowed - ReadOnly", testContact.Allocations.ReadOnly);
			}
			finally
			{
				Env.Security.OrgContactModify.IsAllowed = oldAllocationInfoValue;
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = oldValue;
			}
		}

		public void TestDocumentsCollectionIsReadOnly()
		{
			bool oldDocumentDeliveryValue = Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed;

			try
			{
				OrgContact testContact = OrgInDB.Contacts.AddNew();
				Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testContact.Documents.ReadOnly);

				Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = false;
				ResetOrgInDB();
				testContact = OrgInDB.Contacts.AddNew();
				Assert("Access Disallowed - ReadOnly", testContact.Documents.ReadOnly);
			}
			finally
			{
				Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = oldDocumentDeliveryValue;
			}
		}

		public void TestWebSecurityRightsCollectionIsReadOnly()
		{
			bool oldWebSecurityRightsValue = Env.Security.OrgDetailsModifyWebSecurity.IsAllowed;

			try
			{
				OrgContact testContact = OrgInDB.Contacts.AddNew();
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testContact.SecurityRightsForBindingOnly.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				ResetOrgInDB();
				testContact = OrgInDB.Contacts.AddNew();
				Assert("Access Disallowed - ReadOnly", testContact.SecurityRightsForBindingOnly.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = oldWebSecurityRightsValue;
			}
		}

		public void TestReadOnlySecurity()
		{
			OrgContact parentlessContact = Factory.New<OrgContact>();
			Assert("Access Allowed (No Parent Org) - Not ReadOnly", !parentlessContact.OC_IsActiveInfo.ReadOnly);

			bool oldContactsValue = Env.Security.OrgContactModifyContactDetails.IsAllowed;
			bool oldWebSecurityRightsValue = Env.Security.OrgDetailsModifyWebSecurity.IsAllowed;
			bool oldDocumentDeliveryValue = Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed;
			bool oldPersonalInfoValue = Env.Security.OrgContactModifyPersonalInformation.IsAllowed;

			try
			{
				Env.Security.OrgContactModifyContactDetails.IsAllowed = ZBool.True;
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.True;
				Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = ZBool.True;
				Env.Security.OrgContactModifyPersonalInformation.IsAllowed = ZBool.True;

				OrgContact testContact = OrgInDB.Contacts.AddNew();
				testContact.OC_WebAccessEnabled = ZBool.True; //enable the password field
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_WebAccessEnabledInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_SalutationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_NotifyModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_AttachmentTypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_BirthdayInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_PersonalInfoInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_YearJoinedCompanyInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.False;
				Assert("Access NOT Allowed - Not ReadOnly", testContact.OC_WebAccessEnabledInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_SalutationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_NotifyModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_AttachmentTypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_BirthdayInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_PersonalInfoInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_YearJoinedCompanyInfo.ReadOnly);

				Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = ZBool.False;
				Assert("Access NOT Allowed - Not ReadOnly", testContact.OC_SalutationInfo.ReadOnly);
				Assert("Access NOT Allowed - Not ReadOnly", testContact.OC_NotifyModeInfo.ReadOnly);
				Assert("Access NOT Allowed - Not ReadOnly", testContact.OC_AttachmentTypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_BirthdayInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_PersonalInfoInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_YearJoinedCompanyInfo.ReadOnly);

				Env.Security.OrgContactModifyPersonalInformation.IsAllowed = ZBool.False;
				Assert("Access NOT Allowed - Not ReadOnly", testContact.OC_BirthdayInfo.ReadOnly);
				Assert("Access NOT Allowed - Not ReadOnly", testContact.OC_PersonalInfoInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_YearJoinedCompanyInfo.ReadOnly);

				Env.Security.OrgContactModifyContactDetails.IsAllowed = ZBool.False;
				Assert("Access NOT Allowed - Not ReadOnly", testContact.OC_YearJoinedCompanyInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgContactModifyContactDetails.IsAllowed = oldContactsValue;
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = oldWebSecurityRightsValue;
				Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = oldDocumentDeliveryValue;
				Env.Security.OrgContactModifyPersonalInformation.IsAllowed = oldPersonalInfoValue;
			}
		}

		public void TestReadOnlySecurity2()
		{
			OrgContact parentlessContact = Factory.New<OrgContact>();
			Assert("Access Allowed (No Parent Org) - Not ReadOnly", !parentlessContact.OC_WebAccessEnabledInfo.ReadOnly);

			bool oldWebSecurityRightsValue = Env.Security.OrgDetailsModifyWebSecurity.IsAllowed;
			bool oldNewWebSecurityRightsValue = Env.Security.OrgDetailsNewWebSecurity.IsAllowed;

			try
			{
				OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
				OrgContact testContact = newOrg.Contacts.AddNew();

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.True;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = ZBool.True;
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_WebAccessEnabledInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.True;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = ZBool.False;
				Assert("Access Disallowed - ReadOnly", testContact.OC_WebAccessEnabledInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.False;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = ZBool.True;
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_WebAccessEnabledInfo.ReadOnly);

				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = ZBool.False;
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.False;
				Assert("Access Disallowed - ReadOnly", testContact.OC_WebAccessEnabledInfo.ReadOnly);

				testContact = OrgInDB.Contacts.AddNew();

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.True;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = ZBool.True;
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_WebAccessEnabledInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.True;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = ZBool.False;
				Assert("Access Allowed - Not ReadOnly", !testContact.OC_WebAccessEnabledInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.False;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = ZBool.True;
				Assert("Access Disallowed - ReadOnly", testContact.OC_WebAccessEnabledInfo.ReadOnly);

				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = ZBool.False;
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = ZBool.False;
				Assert("Access Disallowed - ReadOnly", testContact.OC_WebAccessEnabledInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = oldWebSecurityRightsValue;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = oldNewWebSecurityRightsValue;
			}
		}

		public void TestViewPersonalInformationAllowed()
		{
			var viewDeniedMessage = "** View Denied due to Security Access **";
			bool originalViewPersonalInformationSecurity = Env.Security.OrgContactViewPersonalInformation.IsAllowed;

			try
			{
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = true;

				var testContact = OrgInDB.Contacts.AddNew();
				testContact.OC_Birthday = ZDateTime.BrettsBirthday;

				AssertNotEquals(ZDateTime.Empty, testContact.OC_Birthday);
				AssertNotEquals(viewDeniedMessage, testContact.OC_RN_NKNationality);
				AssertNotEquals(viewDeniedMessage, testContact.OC_Gender);
				AssertNotEquals(viewDeniedMessage, testContact.OC_PersonalInfo);
			}
			finally
			{
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = originalViewPersonalInformationSecurity;
			}
		}

		public void TestViewPersonalInformationNotAllowed()
		{
			var viewDeniedMessage = "** View Denied due to Security Access **";
			bool originalViewPersonalInformationSecurity = Env.Security.OrgContactViewPersonalInformation.IsAllowed;

			try
			{
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = false;

				var testContact = OrgInDB.Contacts.AddNew();
				testContact.OC_Birthday = ZDateTime.BrettsBirthday;

				AssertEquals(ZDateTime.Empty, testContact.OC_Birthday);
				AssertEquals(viewDeniedMessage, testContact.OC_RN_NKNationality);
				AssertEquals(viewDeniedMessage, testContact.OC_Gender);
				AssertEquals(viewDeniedMessage, testContact.OC_PersonalInfo);
			}
			finally
			{
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = originalViewPersonalInformationSecurity;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(OrgContact)));
		}

		#endregion

		#region IGlbEmailAddressProvider

		public void TestNonDeliveryReport()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact A";
			contact.OC_Email = "test@tester.com";

			AssertEquals(false, contact.IsNDR);
			AssertEquals(ZDateTime.Empty, contact.DeliveryReportTimeUtc);

			GlbEmailAddress emailAddr = Factory.New<GlbEmailAddress>();
			emailAddr.GI_DeliveryReportTimeUtc = new ZDateTime(2002, 2, 2);
			emailAddr.GI_DeliveryStatus = "NDR";
			emailAddr.GI_EmailAddress = "test@tester.com";

			AssertEquals(true, contact.IsNDR);
			AssertEquals(new ZDateTime(2002, 2, 2), contact.DeliveryReportTimeUtc);
		}

		#endregion

		#region ICertificatesProvider

		public void TestGetDefaultDescription()
		{
			// Arrange
			var certificatesProvider = (ICertificatesProvider)Contact;
			var certificateTypeList = (CertificateTypePairList)certificatesProvider.GetCertificateTypeList();
			// Act
			string code = certificateTypeList[0].Code;
			string expectedDescription = certificateTypeList.GetDescriptionFromCode(code);
			string actualDescription = certificatesProvider.GetDefaultDescription(code);
			// Assert
			AssertEquals(expectedDescription, actualDescription);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var contact = OrgHeader.New(Factory).Contacts.AddNew();
			contact.OC_Email = "aaa@a.com";
			return contact;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin";

			return contact;
		}

		protected override IEnumerable<string> GetAdditionalIgnoreTablesForFetchHintsCheck()
		{
			var result = base.GetAdditionalIgnoreTablesForFetchHintsCheck().ToList();
			result.Add(OrgHeaderSchema.Constants.TableName);
			return result;
		}

		OrgContact Contact;
		OrgHeader Company;

		protected override void SetUp()
		{
			base.SetUp();
			Company = Factory.NewWithValidTestData<OrgHeader>();
			Company.OH_RL_NKClosestPort = "AUSYD";
			Contact = Company.Contacts.AddNew();
		}

		void AssertContactDetailsAreCorrect(DocDeliveryContact actualContact, DocDeliveryContact expectedContact)
		{
			AssertEquals("Org PK Set", expectedContact.OrgHeaderPK, actualContact.OrgHeaderPK);

			AssertEquals("Contact Name", expectedContact.Name, actualContact.Name);
			AssertEquals("Company Name", expectedContact.CompanyName, actualContact.CompanyName);
			AssertEquals("Delivery Method", expectedContact.DeliveryMethod, actualContact.DeliveryMethod);
			AssertEquals("Attachment Type", expectedContact.AttachmentType, actualContact.AttachmentType);

			AssertEquals("Address1", expectedContact.Address1, actualContact.Address1);
			AssertEquals("Address2", expectedContact.Address2, actualContact.Address2);
			AssertEquals("City", expectedContact.City, actualContact.City);
			AssertEquals("State", expectedContact.State, actualContact.State);
			AssertEquals("PostCode", expectedContact.PostCode, actualContact.PostCode);
			AssertEquals("Email", expectedContact.Email, actualContact.Email);
			AssertEquals("Fax", expectedContact.Fax, actualContact.Fax);
			AssertEquals("Phone", expectedContact.Phone, actualContact.Phone);
			AssertEquals("UNLOCO", expectedContact.UNLOCO.PK, actualContact.UNLOCO.PK);
			AssertEquals("EmailSubjectMacro", expectedContact.EmailSubjectMacro, actualContact.EmailSubjectMacro);
			AssertEquals("SendIndividually", expectedContact.SendIndividually, actualContact.SendIndividually);
		}

		DocDeliveryContact CreateNewDocDeliveryContact(OrgContact contact, OrgHeader org)
		{
			DocDeliveryContact docContact = new DocDeliveryContact(Factory);

			docContact.OrgHeaderPK = org.PK;
			docContact.Name = contact.OC_ContactName;
			docContact.CompanyName = org.OH_FullName;
			docContact.DeliveryMethod = contact.OC_NotifyMode;
			docContact.AttachmentType = contact.OC_AttachmentType;
			docContact.Address1 = org.MainAddress.OA_Address1;
			docContact.Address2 = org.MainAddress.OA_Address2;
			docContact.City = org.MainAddress.OA_City;
			docContact.State = org.MainAddress.OA_State;
			docContact.PostCode = org.MainAddress.OA_PostCode;
			docContact.Fax = org.MainAddress.OA_Fax;
			docContact.Email = org.MainAddress.OA_Email;
			docContact.Phone = org.MainAddress.OA_Phone;
			docContact.UNLOCO = org.ClosestPort;

			return docContact;
		}

		DocDeliveryContact CreateNewDocDeliveryContact(OrgContact contact, OrgHeader org, OrgAddress address)
		{
			DocDeliveryContact docContact = new DocDeliveryContact(Factory);

			docContact.OrgHeaderPK = contact.Header.PK;
			docContact.Name = contact.OC_ContactName;
			docContact.DeliveryMethod = contact.OC_NotifyMode;
			docContact.CompanyName = address.OA_CompanyNameOverride.IsEmpty ? org.OH_FullName : address.OA_CompanyNameOverride;
			docContact.AttachmentType = contact.OC_AttachmentType;
			docContact.Address1 = address.OA_Address1;
			docContact.Address2 = address.OA_Address2;
			docContact.City = address.OA_City;
			docContact.State = address.OA_State;
			docContact.PostCode = address.OA_PostCode;
			docContact.Fax = address.OA_Fax;
			docContact.Email = address.OA_Email;
			docContact.Phone = address.OA_Phone;
			docContact.UNLOCO = org.ClosestPort;

			return docContact;
		}

		void AddAddressDetailsToOrg(OrgHeader org, ZString companyName)
		{
			org.MainAddress.OA_Address1 = companyName + " Address1";
			org.MainAddress.OA_Address2 = companyName + " Address2";
			org.MainAddress.OA_City = companyName + " City";
			org.MainAddress.OA_State = companyName + " State";
			org.MainAddress.OA_PostCode = companyName;
			org.MainAddress.OA_Fax = companyName + " Fax";
			org.MainAddress.OA_Email = companyName + " Email";
			org.MainAddress.OA_Phone = companyName + " Phone";
		}

		void AddAddressDetailsToAddress(OrgAddress address, ZString companyName, ZString uNLOCO)
		{
			address.OA_Code = OrgConstants.AddressType.PickupAndDelivery;
			address.OA_Address1 = companyName + " Address1";
			address.OA_Address2 = companyName + " Address2";
			address.OA_City = companyName + " City";
			address.OA_State = companyName + " State";
			address.OA_Phone = companyName + " Phone";
			address.OA_Fax = companyName + " Fax";
			address.OA_Email = companyName + " Email";
			address.OA_PostCode = companyName;
			address.Header.OH_RL_NKClosestPort = uNLOCO;
		}

		StmMenuItem CreateNewMenuItem(ZString contactType)
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = contactType;
			return menuItem;
		}

		OrgContact GetNewContactWithPlainTextPassword(string password)
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.SetHashedPassword(password);
			Factory.Save();
			return orgContact;
		}

		OrgContact GetNewContactWithHashedPassword(string password)
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.SetHashedPassword(password);
			Factory.Save();
			return orgContact;
		}

		#endregion

		#region Duplicationtests

		public void TestDDRCreatedWhenDuplicateDetected()
		{
			InitDedupContactInfo(out var contact, out var target1, out var target2, out var targetLists, out var scoreResults);

			AssertNull("Precondition", contact.referenceForDDR);

			contact.AddOrUpdateDDRLogInfo(ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", contact, targetLists, scoreResults, null));
			var referenceStr = $"Match: {target2.OC_ContactName} (91%), {target1.OC_ContactName} (76%)";
			var stmAlogsForDDR = LoadExistingDDRLogs(contact.PK);
			AssertEquals("No logs added", 0, stmAlogsForDDR.Length);
			AssertEquals(referenceStr, contact.referenceForDDR);

			Factory.Save();
			stmAlogsForDDR = LoadExistingDDRLogs(contact.PK);
			AssertEquals("New log added", 1, stmAlogsForDDR.Length);
			Assert(stmAlogsForDDR[0].IsInDatabase);
			AssertEquals(referenceStr, stmAlogsForDDR[0].SL_Reference);
			AssertNull(contact.referenceForDDR);

			contact.OC_ContactName = "New Name 1";
			Factory.Save();
			stmAlogsForDDR = LoadExistingDDRLogs(contact.PK);
			AssertEquals("No new logs added", 1, stmAlogsForDDR.Length);

			contact.AddOrUpdateDDRLogInfo(ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", contact, targetLists, scoreResults, null));
			Factory.Save();
			stmAlogsForDDR = LoadExistingDDRLogs(contact.PK);
			AssertEquals("No new logs added", 1, stmAlogsForDDR.Length);

			contact.OC_ContactName = "New Name 2";
			Factory.Save();
			stmAlogsForDDR = LoadExistingDDRLogs(contact.PK);
			AssertEquals("New log added", 2, stmAlogsForDDR.Length);
		}

		public void TestNewDDROverridesTheFormerOne()
		{
			InitDedupContactInfo(out var contact, out var target1, out var target2, out var targetLists, out var scoreResults);
			AssertNull("Precondition", contact.referenceForDDR);

			contact.AddOrUpdateDDRLogInfo(ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", contact, targetLists, scoreResults, null));
			var referenceStr = $"Match: {target2.OC_ContactName} (91%), {target1.OC_ContactName} (76%)";
			AssertEquals(referenceStr, contact.referenceForDDR);

			scoreResults[0].Score = 0.99;
			contact.AddOrUpdateDDRLogInfo(ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", contact, targetLists, scoreResults, null));
			referenceStr = $"Match: {target1.OC_ContactName} (99%), {target2.OC_ContactName} (91%)";
			AssertEquals("Should replace to new reference", referenceStr, contact.referenceForDDR);

			Factory.Save();
			var stmAlogsForDDR = LoadExistingDDRLogs(contact.PK);
			AssertEquals("New log added", 1, stmAlogsForDDR.Length);
			AssertEquals("Should equal to new reference", referenceStr, stmAlogsForDDR[0].SL_Reference);
		}

		public void TestWhenShouldRunDeduplicationIsTrue_ShouldFindDuplicatesWhenOnSavingAndRetainValueForNewContacts()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				bool wasCalled = false;
				var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
				Factory.Save();
				var newOrgContact = glbPerson.ContactCollection.AddNew();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				newOrgContact.OC_OH = orgHeader.PK;
				newOrgContact.OC_ContactName = "ABC";
				newOrgContact.OC_PER = glbPerson.PK;
				((IDeduplicatable)glbPerson).ShouldRunDeduplication = true;
				glbPerson.DeduplicationStarted += (o, e) => wasCalled = true;

				Factory.Save();

				Assert("Should Find Duplicates", wasCalled);
				AssertEquals("ShouldRunDeduplication should still be true", true, ((IDeduplicatable)glbPerson).ShouldRunDeduplication);
			}
		}

		public void TestWhenShouldRunDeduplicationIsFalse_ShouldNotFindDuplicatesWhenOnSavingAndRetainValueForNewContacts()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				bool wasCalled = false;
				var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
				var newOrgContact = glbPerson.ContactCollection.AddNew();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				newOrgContact.OC_OH = orgHeader.PK;
				newOrgContact.OC_ContactName = "ABC";
				newOrgContact.OC_PER = glbPerson.PK;
				((IDeduplicatable)glbPerson).ShouldRunDeduplication = false;
				glbPerson.DeduplicationStarted += (o, e) => wasCalled = true;

				Factory.Save();

				AssertEquals("Should not Find Duplicates", false, wasCalled);
				AssertEquals("ShouldRunDeduplication should still be false", false, ((IDeduplicatable)glbPerson).ShouldRunDeduplication);
			}
		}

		public void TestWhenShouldRunDeduplicationIsTrue_ShouldFindDuplicatesWhenOnSavingAndRetainValueForExistingContacts()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				bool wasCalled = false;
				var existingOrgContact = Factory.NewWithValidTestData<OrgContact>();
				var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				glbPerson.ContactCollection.Add(existingOrgContact);
				existingOrgContact.OC_OH = orgHeader.PK;
				existingOrgContact.OC_ContactName = "ABC";
				existingOrgContact.OC_PER = glbPerson.PK;
				((IDeduplicatable)glbPerson).ShouldRunDeduplication = true;
				glbPerson.DeduplicationStarted += (o, e) => wasCalled = true;

				Factory.Save();

				Assert("Should Find Duplicates", wasCalled);
				AssertEquals("ShouldRunDeduplication should still be true", true, ((IDeduplicatable)glbPerson).ShouldRunDeduplication);
			}
		}

		public void TestWhenShouldRunDeduplicationIsFalse_ShouldNotFindDuplicatesWhenOnSavingAndRetainValueForExistingContacts()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				bool wasCalled = false;
				var existingOrgContact = Factory.NewWithValidTestData<OrgContact>();
				var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				glbPerson.ContactCollection.Add(existingOrgContact);
				existingOrgContact.OC_OH = orgHeader.PK;
				existingOrgContact.OC_ContactName = "ABC";
				existingOrgContact.OC_PER = glbPerson.PK;
				((IDeduplicatable)glbPerson).ShouldRunDeduplication = false;
				glbPerson.DeduplicationStarted += (o, e) => wasCalled = true;

				Factory.Save();

				AssertEquals("Should not Find Duplicates", false, wasCalled);
				AssertEquals("ShouldRunDeduplication should still be false", false, ((IDeduplicatable)glbPerson).ShouldRunDeduplication);
			}
		}

		public void TestDeduplicationStartEventPermissions()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			var reg = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			var security = Env.Security.OrgContactModify.IsAllowed;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Env.Security.OrgContactModify.IsAllowed = false;
				bool wasCalled = false;
				var org = Factory.NewWithValidTestData<OrgContact>();
				org.Person.DeduplicationStarted += (o, e) => wasCalled = true;
				Factory.Save();

				org.OC_ContactName = "Don't run FindDuplicates";
				Assert(!wasCalled);
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.OrgContactModify.IsAllowed = false;

				org.OC_ContactName = "Don't run FindDuplicates 2";
				Assert(!wasCalled);

				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.OrgContactModify.IsAllowed = true;
				((IDeduplicatable)org.Person).ShouldRunDeduplication = true;
				org.OC_ContactName = "Run FindDuplicates";
				Factory.Save();
				Assert(wasCalled);
				wasCalled = false;

				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Env.Security.OrgContactModify.IsAllowed = true;
				org.OC_ContactName = "Don't run FindDuplicates";
				Assert(!wasCalled);
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);
				Env.Security.OrgContactModify.IsAllowed = security;
			}
		}

		public void TestDeduplicationNotStartedWhenContactContainsError()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eventStarted = false;
				var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
				var orgContact = glbPerson.ContactCollection.AddNew();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgContact.OC_OH = orgHeader.PK;
				orgContact.OC_ContactName = "ABC";
				orgContact.OC_PER = glbPerson.PK;

				((IDeduplicatable)glbPerson).ShouldRunDeduplication = true;
				glbPerson.DeduplicationStarted += (o, e) => eventStarted = true;

				orgContact.OC_ContactName = "";
				AssertEquals(false, eventStarted);

				orgContact.OC_Phone_Formatted = "INVALID";
				glbPerson.FindDuplicates(orgContact);
				AssertEquals(false, eventStarted);

				orgContact.OC_ContactName = "ABC";
				AssertEquals(false, eventStarted);

				orgContact.OC_Phone_Formatted = "+1 201-555-5554";
				glbPerson.FindDuplicates(orgContact);
				AssertEquals(true, eventStarted);
			}
		}

		public void TestValidateDuplicateResult()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var person = contact.Person;
			person.FindDuplicates(contact);
			person.ValidateDuplicationResult(true);
			AssertHasWarning(contact.OC_ContactNameInfo, "The contact details you have entered resulted in potential duplicates. Please confirm that they are actual duplicates.");
		}

		public void TestContactNameChange_DbHits()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			for (int i = 1; i <= 5; ++i)
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "user" + i.ToString(CultureInfo.InvariantCulture);
			}
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var orgInFactory2 = factory2.Load<OrgHeader>(org.PK);
			orgInFactory2.Contacts[0].OC_ContactName = "New Name";

			var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 1 },
				};
			AssertDbHits(expectedDbHits, factory2);
		}

		void InitDedupContactInfo(out OrgContact contact, out OrgContact target1, out OrgContact target2, out List<CargoWise.Glow.Model.Interfaces.IOrgContact> targetLists, out List<ScoringResult> scoreResults)
		{
			contact = Factory.NewWithValidTestData<OrgContact>();
			target1 = Factory.NewWithValidTestData<OrgContact>();
			target2 = Factory.NewWithValidTestData<OrgContact>();
			targetLists = ObjectFactory.Get<IMasterDataProvider>().GetContactTargetLists(target1, target2);
			scoreResults = new List<ScoringResult>()
			{
				new ScoringResult()
				{
					MasterPK = contact.PK.ToGuid(),
					TargetPK = target1.PK.ToGuid(),
					Score = 0.76
				},
				new ScoringResult()
				{
					MasterPK = contact.PK.ToGuid(),
					TargetPK = target2.PK.ToGuid(),
					Score = 0.91
				}
			};
		}
		StmALog[] LoadExistingDDRLogs(ZGuid pk)
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, pk);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DuplicateDetectedForReviewCode);
			var log = Factory.Load<StmALog>(filter);
			return log;
		}
		#endregion

		public void TestOrgContactUniqueIndexFailureHandler()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Mr A";
			contact1.OC_Email = "a@aaa.com";
			contact1.Header.OH_Code = "ORG_0001";

			var contact2 = contact1.Header.Contacts.AddNew();
			contact2.OC_ContactName = "Mr B";
			contact2.OC_Email = "b@bbb.com";
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			contact2.Person.PER_FullName = "Mr A";

			Factory.Saving += f => { f.ServiceContainer.AddAfterOnSavingService(new DuplicateContactNameService(contact1.Header, "Mr A (1)")); };

			try
			{
				Factory.Save();
			}
			catch (Exception ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				Factory.ServiceContainer.RemoveAfterOnSavingService<DuplicateContactNameService>();
			}

			AssertEquals(@"You are trying to synchronize the Person's name to a Contact name (Mr A (1)) that already exists on Organization (ORG_0001).
The contact name must be unique within each organization.
Please enter a different name or alter the existing contact first.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		class DuplicateContactNameService : IAfterOnSavingBOProcessingService
		{
			public DuplicateContactNameService(OrgHeader org, ZString duplicateName)
			{
				Org = org;
				DuplicateName = duplicateName;
			}
			readonly OrgHeader Org;
			readonly ZString DuplicateName;

			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var org = anotherFactory.Load<OrgHeader>(Org.PK);
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = DuplicateName;
				anotherFactory.Save();
			}
		}

		public void TestScreeningStatus_ClearOrg_ContactChangeActiveStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			contact.OC_Email = "a@aaa.com";
			contact.OC_IsActive = true;
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			AssertEquals("PreCondition", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);

			contact.OC_IsActive = false;
			Factory.Save();
			AssertEquals("Status should still be clear after deactivating contact", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);

			contact.OC_IsActive = true;
			Factory.Save();
			AssertEquals("Status should still be clear after activating contact", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);
		}

		public void TestScreeningStatus_MatchedOrg_ContactChangeActiveStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			contact.OC_Email = "a@aaa.com";
			contact.OC_IsActive = true;
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("PreCondition", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			contact.OC_IsActive = false;
			Factory.Save();
			AssertEquals("Status should still be matched after deactivating contact", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			contact.OC_IsActive = true;
			Factory.Save();
			AssertEquals("Status should still be matched after activating contact", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
		}

		public void TestScreeningStatus_MatchedOrg_AddContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			contact.OC_Email = "a@aaa.com";
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("PreCondition", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact2";
			contact2.OC_Email = "b@bbb.com";
			Factory.Save();

			AssertEquals("Matched status should remain matched after adding contact", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
		}

		public void TestScreeningStatus_ClearOrg_AddContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			contact.OC_Email = "a@aaa.com";
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			AssertEquals("PreCondition", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact2";
			contact2.OC_Email = "b@bbb.com";
			Factory.Save();

			AssertEquals("Clear status should remain clear after adding contact", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);
		}

		public void TestWorkingAddressPK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			contact.OC_Email = "a@aaa.com";

			org.OH_RL_NKClosestPort = "AUSYD";
			var mainAddress = org.Addresses.MainAddress;
			var otherAddress = org.Addresses.AddNew();
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			otherAddress.OA_RL_NKRelatedPortCode = "NZAKL";

			AssertEquals(mainAddress.PK, contact.WorkingAddressPK);
			contact.WorkingAddressPK = otherAddress.PK;
			AssertEquals(otherAddress.PK, contact.OC_OA_OrgAddress);
			AssertEquals(otherAddress.PK, contact.WorkingAddressPK);
		}

		public void TestVerifyPassword()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var contact = header.Contacts.AddNew();

			contact.SetHashedPassword("pswrd");
			Assert(contact.HasPassword);
			Assert(contact.VerifyPassword("pswrd"));
			Assert(!contact.VerifyPassword("pAswrd"));

			contact.OC_PasswordHashIterations = 100;
			Assert(!contact.VerifyPassword("pswrd"));

			contact.OC_PasswordHash = ZBlob.Empty;
			Assert(!contact.VerifyPassword("pswrd"));

			Factory.Save();

			contact.Person.SetHashedPassword("personpass");
			Assert("Contact password is no longer valid once person password is set", !contact.VerifyPassword("pswrd"));
			Assert(contact.HasPassword);
			Assert(contact.VerifyPassword("personpass"));

			contact.SetHashedPassword(string.Empty);
			Assert("Should have password since person password exists", contact.HasPassword);
		}

		public void TestReloadPerson()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var contact = header.Contacts.AddNew();
			var originalPerson = GlbPerson.CreateFromContact(contact.Factory, contact);
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			AssertNotNull("Preconidtion: The contact has a person.", contact.Person);

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.OrgContact set OC_PER = '{newPerson.PK}' WHERE OC_PK = '{contact.PK}'");
			originalPerson.Delete();
			Factory.Save();

			AssertNull("Person is null due to original person was deleted.", contact.Person);
			AssertEquals(originalPerson.PK, contact.OC_PER);

			contact.ReloadPerson();
			AssertNotNull("The contact has a new person.", contact.Person);
			AssertEquals(newPerson.PK, contact.OC_PER);
		}

		public void TestContactOnSavingShouldUpdatePerson()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			contact.OC_Gender = "F";
			contact.OC_PersonalInfo = "001";
			Factory.Save();
			AssertEquals("F", contact.Person.PER_GenderInternal);
			AssertEquals("001", contact.Person.PER_PersonalInfoInternal);

			contact.OC_Gender = "M";
			contact.OC_PersonalInfo = "002";
			Factory.Save();
			AssertEquals("M", contact.Person.PER_GenderInternal);
			AssertEquals("002", contact.Person.PER_PersonalInfoInternal);

			contact.OC_Gender = "N";
			contact.OC_PersonalInfo = "";
			Factory.Save();
			AssertEquals("N", contact.Person.PER_GenderInternal);
			AssertEquals("", contact.Person.PER_PersonalInfoInternal);
		}

		public void TestContactNameWithoutNumberSuffix()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Sam";
			AssertEquals("Sam", contact.ContactNameWithoutNumberSuffix);

			contact.OC_ContactName = "Sam (1)";
			AssertEquals("Sam", contact.ContactNameWithoutNumberSuffix);

			contact.OC_ContactName = "Sam(0)";
			AssertEquals("Sam", contact.ContactNameWithoutNumberSuffix);

			contact.OC_ContactName = "Sam (999)";
			AssertEquals("Sam", contact.ContactNameWithoutNumberSuffix);

			contact.OC_ContactName = "Sam (10086)";
			AssertEquals("Sam (10086)", contact.ContactNameWithoutNumberSuffix);

			contact.OC_ContactName = "Sam ()";
			AssertEquals("Sam ()", contact.ContactNameWithoutNumberSuffix);
		}

		public void TestSetIsNDRShouldSetHasChanges()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			AssertEquals("Precondition", false, contact.HasChanges);
			AssertEquals("Precondition", false, contact.IsNDRInfo.HasChanges);

			contact.IsNDR = true;
			AssertEquals("HasChanges should be set to true", true, contact.HasChanges);
			Factory.Save();

			var contactInNewFactory = new BusinessObjectFactory().Load<OrgContact>(contact.PK);
			AssertEquals("Should be NDR", true, contactInNewFactory.IsNDR);
			AssertEquals("Precondition", false, contactInNewFactory.HasChanges);

			contactInNewFactory.IsNDR = true;
			AssertEquals("HasChanges should not change since the value is the same", false, contactInNewFactory.HasChanges);

			contactInNewFactory.IsNDR = false;
			AssertEquals("HasChanges should be set to true", true, contactInNewFactory.HasChanges);
		}

		public void TestGenerateResetPasswordUrlWhenBranchOrCompanyInactive()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var company = contact.BranchForLogin.Company;
			var companyName = company.GC_Name;
			var branch = contact.BranchForLogin;
			var branchName = branch.GB_BranchName;

			AssertExceptionThrown(
				string.Format("You need to provide a Web Tracker URL for {0} in the System Registry under path {1}", companyName, "Web > Web Component URLs > WebTracker URL"),
				typeof(Exception),
				() => contact.GeneratePasswordInstructionUrl("", PasswordInstructionType.Reset));

			branch.GB_IsActive = false;
			AssertExceptionThrown(
				string.Format("The Organization lacks an active controlling branch. The inactive branch name is {0}.", branchName),
				typeof(Exception), () => contact.GeneratePasswordInstructionUrl("", PasswordInstructionType.Reset));

			branch.GB_IsActive = true;
			company.GC_IsActive = false;

			AssertExceptionThrown(
				string.Format("The Organization lacks an active controlling branch. The inactive company name is {0}. The branch name is {1}.", companyName, branchName),
				typeof(Exception), () => contact.GeneratePasswordInstructionUrl("", PasswordInstructionType.Reset));

			//even if both are inactive, as long as we get a web tracker url, that's all we care about!
			branch.GB_IsActive = false;
			company.GC_IsActive = false;

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://localhost/webtraker");
			var url = contact.GeneratePasswordInstructionUrl("111", PasswordInstructionType.Reset);
			AssertEquals("http://localhost/webtraker/Admin/ResetMasterPassword.aspx?ResetKey=111", url);
		}

		public void TestGenerateResetPasswordUrl()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var company = contact.BranchForLogin.Company;
			var companyName = company.GC_Name;

			AssertExceptionThrown(
				string.Format("You need to provide a Web Tracker URL for {0} in the System Registry under path {1}", companyName, "Web > Web Component URLs > WebTracker URL"),
				typeof(Exception),
				() => contact.GeneratePasswordInstructionUrl("", PasswordInstructionType.Reset));

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://localhost/webtraker");
			var url = contact.GeneratePasswordInstructionUrl("111", PasswordInstructionType.Reset);
			AssertEquals("http://localhost/webtraker/Admin/ResetMasterPassword.aspx?ResetKey=111", url);
		}

		public void TestGenerateSetPasswordUrl()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			AssertExceptionThrown<WebSiteUrlNotSetException>(() => orgContact.GeneratePasswordInstructionUrl("", PasswordInstructionType.Set));

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(orgContact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtraker");
			var url = orgContact.GeneratePasswordInstructionUrl("111", PasswordInstructionType.Set);
			AssertEquals("http://localhost/webtraker/Admin/SetMasterPassword.aspx?SetKey=111", url);
		}

		public void TestCheckContactHasValidEmail()
		{
			var contact1EmailInvalid = Factory.NewWithValidTestData<OrgContact>();
			contact1EmailInvalid.OC_Email = "";
			var contact2EmailInvalid = Factory.NewWithValidTestData<OrgContact>();
			contact2EmailInvalid.OC_Email = null;
			var contact3EmailValid = Factory.NewWithValidTestData<OrgContact>();

			Factory.Save();

			AssertEquals(contact1EmailInvalid.CheckContactHasValidEmail(), false);
			AssertEquals(contact2EmailInvalid.CheckContactHasValidEmail(), false);
			AssertEquals(contact3EmailValid.CheckContactHasValidEmail(), true);
		}

		public void TestGetPasswordChangedOrSentLog()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "email1@testing.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = "contact2";
			contact2.OC_Email = "email2@testing.com";

			var passwordLogContact1 = Factory.New<StmALog>();
			using (passwordLogContact1.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact1.SL_Parent = contact1.PK;
				passwordLogContact1.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
			}

			var passwordLogContact2 = Factory.New<StmALog>();
			using (passwordLogContact2.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact2.SL_Parent = contact2.PK;
				passwordLogContact2.SL_SE_NKEvent = AutoEvents.WebAccessPasswordEmailSentCode;
			}

			AssertEquals("Should return changed log", passwordLogContact1.PK, contact1.GetPasswordChangedOrSentLog().PK);
			AssertEquals("Should return sent log", passwordLogContact2.PK, contact2.GetPasswordChangedOrSentLog().PK);
		}

		public void TestPasswordInstructionType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "email1@testing.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = "contact2";
			contact2.OC_Email = "email2@testing.com";

			var passwordLogContact1 = Factory.New<StmALog>();
			using (passwordLogContact1.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact1.SL_Parent = contact1.PK;
				passwordLogContact1.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
			}

			var passwordLogContact2 = Factory.New<StmALog>();
			using (passwordLogContact2.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact2.SL_Parent = contact2.PK;
				passwordLogContact2.SL_SE_NKEvent = AutoEvents.WebAccessPasswordEmailSentCode;
			}

			AssertEquals("Password Instruction Type should be reset", PasswordInstructionType.Reset, contact1.GetPasswordInstructionType());
			AssertEquals("Password Instruction Type should be set", PasswordInstructionType.Set, contact2.GetPasswordInstructionType());
		}

		public void TestPasswordInstructionUrl()
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/webtraker");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "email1@testing.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = "contact2";
			contact2.OC_Email = "email2@testing.com";

			var passwordLogContact1 = Factory.New<StmALog>();
			using (passwordLogContact1.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact1.SL_Parent = contact1.PK;
				passwordLogContact1.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
				passwordLogContact1.SL_Table = OrgContactSchema.Constants.TableName;
			}

			var passwordLogContact2 = Factory.New<StmALog>();
			using (passwordLogContact2.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact2.SL_Parent = contact2.PK;
				passwordLogContact2.SL_SE_NKEvent = string.Empty;
				passwordLogContact2.SL_Table = OrgContactSchema.Constants.TableName;
			}

			Factory.Save();

			AssertEquals("Precondition", PasswordInstructionType.Reset, contact1.GetPasswordInstructionType());
			AssertEquals("Precondition", PasswordInstructionType.Set, contact2.GetPasswordInstructionType());

			AssertEquals(FormattableString.Invariant($"http://localhost/webtraker/Admin/ResetMasterPassword.aspx?ResetKey={OrgContact.TokenMacro}"), contact1.PasswordInstructionMacroUrl);
			AssertEquals(FormattableString.Invariant($"http://localhost/webtraker/Admin/SetMasterPassword.aspx?SetKey=(*SetOrResetPasswordToken*)"), contact2.PasswordInstructionMacroUrl);
		}

		public void TestPasswordInstructionSentBy()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS1";
			staff.GS_FullName = "Staff 1";

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/webtraker");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "email1@testing.com";
			Factory.Save();
			AssertEquals("", contact1.PasswordInstructionSentBy);

			var log = contact1.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = "WPE";
				log.SL_GS_NKUser = "GS1";
			}
			Factory.Save();
			AssertEquals("Staff 1", contact1.PasswordInstructionSentBy);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_GS_NKUser = "ABC";
			}
			Factory.Save();
			AssertEquals("", contact1.PasswordInstructionSentBy);
		}

		public void TestEmailOrgCodes()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ADAGIO";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ALLEGRO";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "APART";
			var contact1 = org1.Contacts.AddNew();

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "email1@testing.com";
			Factory.Save();

			var matchingEmailContact = org2.Contacts.AddNew();
			var inactiveContact = org3.Contacts.AddNew();
			var webAccessDisabledContact = org3.Contacts.AddNew();
			var nonMatchingEmailContact = org3.Contacts.AddNew();
			var nonMatchingPersonContact = org3.Contacts.AddNew();

			matchingEmailContact.OC_WebAccessEnabled = true;
			matchingEmailContact.OC_ContactName = "matchingEmailContact";
			matchingEmailContact.OC_Email = "email1@testing.com";
			matchingEmailContact.OC_PER = contact1.OC_PER;
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_ContactName = "inactiveContact";
			inactiveContact.OC_Email = "email1@testing.com";
			inactiveContact.OC_PER = contact1.OC_PER;
			webAccessDisabledContact.OC_WebAccessEnabled = false;
			webAccessDisabledContact.OC_ContactName = "webAccessDisabledContact";
			webAccessDisabledContact.OC_Email = "email1@testing.com";
			webAccessDisabledContact.OC_PER = contact1.OC_PER;
			nonMatchingEmailContact.OC_WebAccessEnabled = true;
			nonMatchingEmailContact.OC_ContactName = "nonMatchingEmailContact";
			nonMatchingEmailContact.OC_Email = "othermail@testing.com";
			nonMatchingEmailContact.OC_PER = contact1.OC_PER;
			nonMatchingPersonContact.OC_WebAccessEnabled = true;
			nonMatchingPersonContact.OC_ContactName = "nonMatchingPersonContact";
			nonMatchingPersonContact.OC_Email = "email1@testing.com";
			Factory.Save();

			AssertEquals("ADAGIO, ALLEGRO", contact1.EmailOrgCodes);
		}

		StmLoginFailureLog CreateLockoutUserRecord(OrgContact contact, string companyCode = "")
		{
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = contact.OC_Email + " " + companyCode;
			loginFailureLog.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			return loginFailureLog;
		}

		public void TestUnlock()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTORG";
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_PER = person.PK;
			orgContact.OC_Email = "test@wise.com";

			CreateLockoutUserRecord(orgContact);
			CreateLockoutUserRecord(orgContact, org.OH_Code);

			AssertEquals(true, LoginAttemptRecorder.IsLockedOut("", orgContact.OC_Email, null));
			AssertEquals(true, LoginAttemptRecorder.IsLockedOut(org.OH_Code, orgContact.OC_Email, null));

			LoginAttemptRecorder.Unlock(org.OH_Code, orgContact.OC_Email, true);

			AssertEquals("OrgContact LoginDisabledUntilUtc is empty", ZDateTime.Empty, orgContact.LockoutDateTimeLocal);
			AssertEquals(false, LoginAttemptRecorder.IsLockedOut("", orgContact.OC_Email, null));
			AssertEquals(false, LoginAttemptRecorder.IsLockedOut(org.OH_Code, orgContact.OC_Email, null));
		}

		public void TestLockoutDateTimeLocal()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_PER = person.PK;
			orgContact.OC_Email = "test@wise.com";

			var loginFailureLog = CreateLockoutUserRecord(orgContact, orgContact.OrgCode);

			AssertEquals("OrgContact LockoutDateTimeLocal is set",
								loginFailureLog.SFL_SystemCreateTimeUtc.AddMinutes(WebDataRegistry.Instance.WebLoginLockoutMinutes.Value),
								orgContact.LockoutDateTimeLocal.ToUniversalBranchTime());
		}

		public void TestRemovePasswordAndHashShouldCleanAllPasswordFields()
		{
			var orgContact1 = GetNewContactWithPlainTextPassword("password");
			orgContact1.RemovePasswordAndHash();

			AssertEquals(orgContact1.OC_PasswordHash, ZBlob.Empty);
			AssertEquals(orgContact1.OC_PasswordSalt, ZBlob.Empty);
			AssertEquals(orgContact1.OC_PasswordHashIterations, ZInt.Zero);

			var orgContact2 = GetNewContactWithHashedPassword("password");
			orgContact2.RemovePasswordAndHash();

			AssertEquals(orgContact2.OC_PasswordHash, ZBlob.Empty);
			AssertEquals(orgContact2.OC_PasswordSalt, ZBlob.Empty);
			AssertEquals(orgContact2.OC_PasswordHashIterations, ZInt.Zero);
		}

		public void TestShouldAddLogOnSavingPasswordDelete()
		{
			ErrorReporter.Clear();
			var orgContact1 = GetNewContactWithHashedPassword("password");
			orgContact1.RemovePasswordAndHash();
			Factory.Save();

			var passwordLogContact = Factory.New<StmALog>();
			using (passwordLogContact.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact.SL_Parent = orgContact1.PK;
				passwordLogContact.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(orgContact1.GetPasswordChangedOrSentLog().SL_Reference, $"Password was Reset, user = {Env.CurrentUser.PK}");
		}

		public void TestWebAccessSupersededShouldUpdateOC_IsActive()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Assert("Precondition", contact.OC_IsActive);
			Assert("Precondition", !contact.WebAccessSuperseded);

			contact.SupersedeWebAccess();
			Assert("Should also be updated", !contact.OC_IsActive);
		}

		public void TestShouldAddEventLogForSupersedeOnSaving()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			contact.SupersedeWebAccess();
			AssertEquals("Should not have added log yet", 0, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.WebAccessSupersededCode));

			Factory.Save();
			AssertEquals("Should have added log for supersede", 1, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.WebAccessSupersededCode));

			contact.OC_ContactName = "Fred";
			Factory.Save();
			AssertEquals("Should not add any more logs for supersede", 1, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.WebAccessSupersededCode));
		}

		public void TestShouldCancelWebAccessSupersededOnSetOC_IsActive()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			Factory.Save();
			AssertEquals("Precondition", true, contact.WebAccessSuperseded);

			contact.OC_IsActive = true;
			AssertEquals("Web access superseded flag should be reset", false, contact.WebAccessSuperseded);
			AssertEquals("Supersede log should not be cancelled yet", 0, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.WebAccessSupersededCode && x.SL_IsCancelled));
			Factory.Save();
			AssertEquals("Web access superseded should be reset", false, contact.WebAccessSuperseded);
			AssertEquals("Supersede log should be cancelled", 1, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.WebAccessSupersededCode && x.SL_IsCancelled));
		}

		[TestDate(2020, 01, 01)]
		public void TestShouldCancelAllWebAccessSupersededOnReset()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			Factory.Save();
			AssertEquals("Precondition", true, contact.WebAccessSuperseded);
			AssertEquals("Precondition: Should have added log for supersede", 1, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.WebAccessSupersededCode));

			TestDateAttribute.AddDays(1);
			contact.SupersedeWebAccess();
			Factory.Save();
			AssertEquals("Precondition", true, contact.WebAccessSuperseded);
			AssertEquals("Precondition: Should have added another log for supersede", 2, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.WebAccessSupersededCode));

			contact.CancelWebAccessSuperseded();
			AssertEquals("Supersede log should not be cancelled yet", 0, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.WebAccessSupersededCode && x.SL_IsCancelled));
			AssertEquals("Web access superseded should be reset", false, contact.WebAccessSuperseded);
			Factory.Save();
			AssertEquals("Supersede logs should both be cancelled", 2, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.WebAccessSupersededCode && x.SL_IsCancelled));
			AssertEquals("Web access superseded should be reset", false, contact.WebAccessSuperseded);
		}

		public void TestSenderForLogs()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Man";
			var contactAsPasswordEmailSource = (IPasswordInstructionEmailSource)contact;
			Assert("Precondition", contact.OC_PER.IsEmpty);
			AssertEquals("Sender for logs should be the contact itself", contact.PK, contactAsPasswordEmailSource.SenderForLogs.PK);
			Assert("Precondition", contact.OC_PER.IsEmpty);

			Factory.Save();
			Assert("Precondition", !contact.OC_PER.IsEmpty);
			AssertEquals("Sender for logs should be the contact itself", contact.PK, contactAsPasswordEmailSource.SenderForLogs.PK);

			contact.Person.SetHashedPassword("hello");
			AssertEquals("Sender for logs should be the Person", contact.Person.PK, contactAsPasswordEmailSource.SenderForLogs.PK);
			Factory.Save();
			AssertEquals("Sender for logs should be the Person", contact.Person.PK, contactAsPasswordEmailSource.SenderForLogs.PK);

			contact.Person.RemovePasswordHash();
			AssertEquals("Sender for logs should be the Person since person password is in the removal process", contact.Person.PK, contactAsPasswordEmailSource.SenderForLogs.PK);
			Factory.Save();
			AssertEquals("Sender for logs should be the contact itself", contact.PK, contactAsPasswordEmailSource.SenderForLogs.PK);
		}

		public void TestGetPasswordInstructionType_PersonWebAccessPasswordChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "email1@testing.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = "contact2";
			contact2.OC_Email = "email2@testing.com";
			Factory.Save();

			var passwordLogContact1 = Factory.New<StmALog>();
			using (passwordLogContact1.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact1.SL_Parent = contact1.Person.PK;
				passwordLogContact1.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
			}

			var passwordLogContact2 = Factory.New<StmALog>();
			using (passwordLogContact2.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact2.SL_Parent = contact2.Person.PK;
				passwordLogContact2.SL_SE_NKEvent = AutoEvents.WebAccessPasswordEmailSentCode;
			}

			AssertEquals("Should return changed log", passwordLogContact1.PK, contact1.GetPasswordChangedOrSentLog().PK);
			AssertEquals("Should return sent log", passwordLogContact2.PK, contact2.GetPasswordChangedOrSentLog().PK);
		}

		public void TestGetPasswordChangedOrSentLog_Person()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "email1@testing.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = "contact2";
			contact2.OC_Email = "email2@testing.com";
			Factory.Save();

			var passwordLogContact1 = Factory.New<StmALog>();
			using (passwordLogContact1.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact1.SL_Parent = contact1.Person.PK;
				passwordLogContact1.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
			}

			var passwordLogContact2 = Factory.New<StmALog>();
			using (passwordLogContact2.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact2.SL_Parent = contact2.Person.PK;
				passwordLogContact2.SL_SE_NKEvent = AutoEvents.WebAccessPasswordEmailSentCode;
			}

			AssertEquals("Password Instruction Type should be reset", PasswordInstructionType.Reset, contact1.GetPasswordInstructionType());
			AssertEquals("Password Instruction Type should be set", PasswordInstructionType.Set, contact2.GetPasswordInstructionType());
		}

		public void TestShouldSendMasterPasswordFor1To1ContactToPerson()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var contactAsEmailSource = contact as IPasswordInstructionEmailSource;
			AssertEquals(true, contactAsEmailSource.ShouldSendMasterPassword);
			Factory.Save();

			AssertEquals("Should be 1 contact", 1, contact.Person.ContactCollection.Count);
			AssertEquals(true, contactAsEmailSource.ShouldSendMasterPassword);

			var extraContact = contact.Person.ContactCollection.AddNew();
			extraContact.OC_ContactName = "Contact2";

			AssertEquals("Should be 2 contacts", 2, contact.Person.ContactCollection.Count);
			AssertEquals(false, contactAsEmailSource.ShouldSendMasterPassword);

			extraContact.Delete();
			AssertEquals(true, contactAsEmailSource.ShouldSendMasterPassword);

			contact.SetHashedPassword("1234");
			AssertEquals(true, contactAsEmailSource.ShouldSendMasterPassword);

			contact.Person.SetHashedPassword("1234");
			AssertEquals(true, contactAsEmailSource.ShouldSendMasterPassword);
		}

		public void TestIsClearingPersonPasswordOverride()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_PER = person.PK;
			var contactAsEmailSource = contact as IPasswordInstructionEmailSource;
			AssertEquals(true, contactAsEmailSource.ShouldSendMasterPassword);
			var extraContact = contact.Person.ContactCollection.AddNew();
			extraContact.OC_ContactName = "Contact2";

			AssertEquals("Precondition: Should be 2 contacts", 2, contact.Person.ContactCollection.Count);
			AssertEquals("Precondition", false, contactAsEmailSource.ShouldSendMasterPassword);

			contact.IsClearingPersonPasswordOverride = true;
			AssertEquals("Should be overriden", true, contactAsEmailSource.ShouldSendMasterPassword);
		}

		public void TestPasswordHistory()
		{
			WebDataRegistry.Instance.WebPasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			var person = contact.Person;
			contact.SetHashedPassword("p1");
			AssertEquals(true, contact.HasPasswordBeenUsed("p1"));

			contact.SetHashedPassword("p2", "p1");
			AssertEquals(true, contact.HasPasswordBeenUsed("p1"));
			AssertEquals(true, contact.HasPasswordBeenUsed("p2"));

			contact.SetHashedPassword("p3");
			AssertEquals(true, contact.HasPasswordBeenUsed("p1"));
			AssertEquals(true, contact.HasPasswordBeenUsed("p2"));
			AssertEquals(true, contact.HasPasswordBeenUsed("p3"));

			contact.SetHashedPassword("p4", "p3");
			AssertEquals(false, contact.HasPasswordBeenUsed("p1"));
			AssertEquals(true, contact.HasPasswordBeenUsed("p2"));
			AssertEquals(true, contact.HasPasswordBeenUsed("p3"));
			AssertEquals(true, contact.HasPasswordBeenUsed("p4"));
			AssertEquals(2, PasswordHistoryHelper.GetPasswordHistories(contact).Count);

			contact.RemovePasswordAndHash();
			AssertEquals(true, contact.OC_PasswordHash.IsEmpty);
			AssertEquals(0, PasswordHistoryHelper.GetPasswordHistories(contact).Count);
			AssertEquals(false, person.HasPasswordBeenUsed("p1"));
			AssertEquals(false, person.HasPasswordBeenUsed("p2"));
			AssertEquals(true, person.HasPasswordBeenUsed("p3"));
			AssertEquals(true, person.HasPasswordBeenUsed("p4"));
			AssertEquals(2, PasswordHistoryHelper.GetPasswordHistories(person).Count);
		}

		public void TestContactExcelPassword()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Tester";
			staff.GS_Code = "SSS";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_Email = "test@test.com";
			orgContact.ExcelPasswordForOpening = "OpeningPassword";
			orgContact.ExcelPasswordForModifying = "ModifyingPassword";
			Factory.Save();

			var orgContact1 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<OrgContact>(orgContact.PK);
			AssertEquals("OpeningPassword", orgContact1.ExcelPasswordForOpening);
			AssertEquals("ModifyingPassword", orgContact1.ExcelPasswordForModifying);

			AssertEquals(false, orgContact1.ExcelPasswordForOpening_ReadOnly);
			AssertEquals(false, orgContact1.ExcelPasswordForModifying_ReadOnly);
			AssertEquals("OpeningPassword", orgContact1.ExcelPasswordForOpening);
			AssertEquals("ModifyingPassword", orgContact1.ExcelPasswordForModifying);

			using (EnvProxy.Instance.SetTemporaryUserContext("Tester", Environment.Env.CurrentBranchPK, Environment.Env.CurrentDepartmentPK))
			{
				AssertEquals(true, orgContact1.ExcelPasswordForOpening_ReadOnly);
				AssertEquals(true, orgContact1.ExcelPasswordForModifying_ReadOnly);
				AssertEquals(orgContact1.ViewDeniedMessage, orgContact1.ExcelPasswordForOpening);
				AssertEquals(orgContact1.ViewDeniedMessage, orgContact1.ExcelPasswordForModifying);
			}
		}

		public void TestExcelPasswordChangeLogs()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_Email = "test@test.com";
			orgContact.OC_ContactName = "Test contact";
			orgContact.ExcelPasswordForOpening = "OpeningPassword";
			orgContact.ExcelPasswordForModifying = "ModifyingPassword";
			Factory.Save();

			orgContact.ExcelPasswordForOpening = "";
			orgContact.ExcelPasswordForModifying = "";
			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, orgContact.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "EPC");

			var logs = new BusinessObjectFactory() { RefreshEnabled = false }.Load<StmALog>(query);
			AssertEquals(4, logs.Length);
			AssertEquals(1, logs.Where(x => x.SL_Reference == $"Org Contact = {orgContact.PK} Test contact Excel Open Password was Changed, user = {Env.CurrentUser.PK}").Count());
			AssertEquals(1, logs.Where(x => x.SL_Reference == $"Org Contact = {orgContact.PK} Test contact Excel Modify Password was Changed, user = {Env.CurrentUser.PK}").Count());
			AssertEquals(1, logs.Where(x => x.SL_Reference == $"Org Contact = {orgContact.PK} Test contact Excel Open Password was Removed, user = {Env.CurrentUser.PK}").Count());
			AssertEquals(1, logs.Where(x => x.SL_Reference == $"Org Contact = {orgContact.PK} Test contact Excel Modify Password was Removed, user = {Env.CurrentUser.PK}").Count());
		}

		public void TestIsAutoLoggedFalse()
		{
			var contact = Factory.NewWithValidTestData<OrgContactForTest>();
			AssertEquals(false, contact.IsAutoLoggedForTest);
		}

		public void TestNoAuditLogForOrgContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "abc@def.com";
			Factory.Save();

			contact.OC_Email = "123@456.com";
			Factory.Save();

			contact.Delete();
			Factory.Save();

			var auditLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, contact.PK));
			AssertEquals(0, auditLogs.Length);
		}

		IOrgContactLoginAttemptRecorder LoginAttemptRecorder => loginAttemptRecorder ?? (loginAttemptRecorder = ObjectFactory.Get<IOrgContactLoginAttemptRecorder>());
		IOrgContactLoginAttemptRecorder loginAttemptRecorder;
	}

	class OrgContactForTest : OrgContact
	{
		public OrgContactForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsAutoLoggedForTest => IsAutoLogged;
	}
}
