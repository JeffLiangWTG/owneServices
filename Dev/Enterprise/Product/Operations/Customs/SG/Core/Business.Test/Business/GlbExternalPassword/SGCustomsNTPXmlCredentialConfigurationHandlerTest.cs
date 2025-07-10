using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class SGCustomsNTPXmlCredentialConfigurationHandlerTest : Customs.Business.XmlCredential.Testing.ConfigurationHandlerTestCase
	{
		public void TestProcess()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "bob@where.com";
			var sgWrapper = staff.GetSGWrapper();
			var ntpPassword = sgWrapper.SGNationalTradePlatformPassword;
			ntpPassword.GP_UserID = "ABC123";
			ntpPassword.CurrentDecryptedPassword = "HELLO";
			ntpPassword.NextDecryptedPassword = "GOODBYE";
			ntpPassword.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			Factory.Save();
			var outgoingInterchange = ConfigurationTestHelper.GetLatestEHubConfigurationInterchange(Factory);
			Configuration configuration;
			using (var reader = outgoingInterchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			AssertEquals("configuration.Name", "SGCustomsNTP", configuration.Name);
			var systemGroup = configuration.Group[0];
			AssertEquals("systemGroup.Type", "System", systemGroup.Type);
			AssertEquals("systemGroup.Reference", "EDIDAT", systemGroup.Reference);
			AssertEquals("systemGroup.Status", "", systemGroup.Status);
			AssertEquals("systemGroup.Annotations.Count", 0, systemGroup.Annotations.Count);
			AssertEquals("systemGroup.Items.Length", 1, systemGroup.Items.Length);
			systemGroup.Annotations.AddNew().Value = "SYSTEM NOTE";
			var companyGroup = (Group)systemGroup.Items[0];
			AssertEquals("companyGroup.Type", "Company", companyGroup.Type);
			AssertEquals("companyGroup.Reference", "EDI", companyGroup.Reference);
			AssertEquals("companyGroup.Status", "", companyGroup.Status);
			AssertEquals("companyGroup.Annotations.Count", 0, companyGroup.Annotations.Count);
			AssertEquals("companyGroup.Items.Length", 1, companyGroup.Items.Length);
			companyGroup.Annotations.AddNew().Value = "COMPANY NOTE";
			var staffGroup = (Group)companyGroup.Items[0];
			AssertEquals("staffGroup.Type", "Staff", staffGroup.Type);
			AssertEquals("staffGroup.Reference", "E", staffGroup.Reference);
			AssertEquals("staffGroup.Status", "", staffGroup.Status);
			AssertEquals("staffGroup.Annotations.Count", 0, staffGroup.Annotations.Count);
			AssertEquals("staffGroup.Items.Length", 1, staffGroup.Items.Length);
			staffGroup.Annotations.AddNew().Value = "STAFF NOTE";
			var ntpPasswordGroup = (Group)staffGroup.Items[0];
			AssertEquals("ntpPasswordGroup.Type", "NTP", ntpPasswordGroup.Type);
			AssertEquals("ntpPasswordGroup.Reference", "", ntpPasswordGroup.Reference);
			AssertEquals("ntpPasswordGroup.Status", PasswordStatusList.Codes.Invalid, ntpPasswordGroup.Status);
			AssertEquals("ntpPasswordGroup.Annotations.Count", 0, ntpPasswordGroup.Annotations.Count);
			AssertEquals("ntpPasswordGroup.Items.Length", 2, ntpPasswordGroup.Items.Length);
			var ntpPasswordGroupCredential1 = (Credential)ntpPasswordGroup.Items[0];
			AssertEquals("ntpPasswordGroupCredential1.Name", "Current", ntpPasswordGroupCredential1.Name);
			AssertEquals("ntpPasswordGroupCredential1.UserName", "ABC123", ntpPasswordGroupCredential1.UserName);
			AssertNotEquals("ntpPasswordGroupCredential1.Password", ZString.Empty, ntpPasswordGroupCredential1.Password);
			var ntpPasswordGroupCredential2 = (Credential)ntpPasswordGroup.Items[1];
			AssertEquals("ntpPasswordGroupCredential2.Name", "Next", ntpPasswordGroupCredential2.Name);
			AssertEquals("ntpPasswordGroupCredential2.UserName", "ABC123", ntpPasswordGroupCredential2.UserName);
			AssertNotEquals("ntpPasswordGroupCredential2.Password", ZString.Empty, ntpPasswordGroupCredential2.Password);
			configuration.Timestamp = ZDateTime.UtcNow;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var logger = new LoggingInformation();
			var handler = new SGCustomsNTPXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);
			AssertEquals("log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(Env.OutgoingMailManager.EmailsCreated[0], "Password Status Has Been Updated to 'Invalid (INV)'", @"Password Type: 'SG National Trade Platform (NTP)'
Owner: Company = 'EDI', Staff = 'E'
Password Status: 'Invalid (INV)'
Status Reason:
System: SYSTEM NOTE
Company: COMPANY NOTE
Staff: STAFF NOTE", new[] { staff.GS_EmailAddress });
			var factory2 = new BusinessObjectFactory();
			var ntpPasswordLoaded = factory2.Load<GlbExternalPassword>(ntpPassword.PK);
			AssertEquals("ntpPasswordLoaded.GP_UserID", "ABC123", ntpPasswordLoaded.GP_UserID);
			AssertEquals("ntpPasswordLoaded.CurrentDecryptedPassword", "HELLO", ntpPasswordLoaded.CurrentDecryptedPassword);
			AssertEquals("ntpPasswordLoaded.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, ntpPasswordLoaded.GP_PasswordStatus);
			AssertEquals("ntpPasswordLoaded.NextDecryptedPassword", "GOODBYE", ntpPasswordLoaded.NextDecryptedPassword);
			AssertEquals("ntpPasswordLoaded.GP_StatusReason", "System: SYSTEM NOTE\r\nCompany: COMPANY NOTE\r\nStaff: STAFF NOTE", ntpPasswordLoaded.GP_StatusReason);
			ntpPasswordGroup.Status = PasswordStatusList.Codes.Valid;
			ntpPasswordGroupCredential1.Password = null;
			configuration.Timestamp = ZDateTime.UtcNow.AddSeconds(4);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			handler = new SGCustomsNTPXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);
			AssertEquals("log", "", GetUserLogStrings(logger.UserLogStrings));
			factory2 = new BusinessObjectFactory();
			ntpPasswordLoaded = factory2.Load<GlbExternalPassword>(ntpPassword.PK);
			AssertEquals("ntpPasswordLoaded.GP_UserID", "ABC123", ntpPasswordLoaded.GP_UserID);
			AssertEquals("ntpPasswordLoarded.CurrentDecryptedPassword", "HELLO", ntpPasswordLoaded.CurrentDecryptedPassword);
			AssertEquals("ntpPasswordLoaded.GP_PasswordStatus", Core.Constants.PasswordOK, ntpPasswordLoaded.GP_PasswordStatus);
			AssertEquals("ntpPasswordLoaded.NextDecryptedPassword", ZString.Empty, ntpPasswordLoaded.NextDecryptedPassword);
			AssertEquals("ntpPasswordLoaded.GP_StatusReason", ZString.Empty, ntpPasswordLoaded.GP_StatusReason);
			AssertEquals("Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			ntpPasswordGroupCredential1.Password = GetEServiceEncryptedPassword();
			configuration.Timestamp = ZDateTime.UtcNow.AddSeconds(4);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			handler = new SGCustomsNTPXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);
			AssertEquals("log", "", GetUserLogStrings(logger.UserLogStrings));
			factory2 = new BusinessObjectFactory();
			ntpPasswordLoaded = factory2.Load<GlbExternalPassword>(ntpPassword.PK);
			AssertEquals("ntpPasswordLoaded.GP_UserID", "ABC123", ntpPasswordLoaded.GP_UserID);
			AssertEquals("ntpPasswordLoaded.CurrentDecryptedPassword", ActualEServiceEncryptedPassword, ntpPasswordLoaded.CurrentDecryptedPassword);
			AssertEquals("ntpPasswordLoaded.GP_PasswordStatus", Core.Constants.PasswordOK, ntpPasswordLoaded.GP_PasswordStatus);
			AssertEquals("ntpPasswordLoaded.NextDecryptedPassword", ZString.Empty, ntpPasswordLoaded.NextDecryptedPassword);
			AssertEquals("ntpPasswordLoaded.GP_StatusReason", ZString.Empty, ntpPasswordLoaded.GP_StatusReason);
			AssertEquals("Emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(Env.OutgoingMailManager.EmailsCreated[0], "Password Status Has Been Updated to 'Password Valid (OK)'", @"Password Type: 'SG National Trade Platform (NTP)'
Owner: Company = 'EDI', Staff = 'E'
Password Status: 'Password Valid (OK)'
Status Reason:
System: SYSTEM NOTE
Company: COMPANY NOTE
Staff: STAFF NOTE", new[] { staff.GS_EmailAddress });
		}
	}
}
