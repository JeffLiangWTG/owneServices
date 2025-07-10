using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;

namespace Enterprise.Customs.Business.XmlCredential.Testing
{
	sealed class GlbExternalPasswordConfigurationHandlerTest : ConfigurationHandlerTestCase
	{
		class GlbExternalPasswordForConfigurationTest : GlbExternalPassword
		{
			public GlbExternalPasswordForConfigurationTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString ConfigurationName => "Configuration";

			protected override ZPropertyInfo[] CredentialApplicableInfos()
			{
				var result = new List<ZPropertyInfo>();
				result.Add(GP_CurrentPasswordInfo);
				result.Add(GP_UserIDInfo);
				if (!IsInDatabase || !GP_NextPassword.IsEmpty)
				{
					result.Add(GP_NextPasswordInfo);
				}
				return result.ToArray();
			}
		}

		public void TestErrorLog()
		{
			var pstGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var pstStaff = pstGroup.Staff.AddNew();
			pstStaff.GS_Code = "#@P";
			pstStaff.GS_FullName = "POST USER";
			pstStaff.GS_LoginName = "#@P";
			pstStaff.GS_EmailAddress = "post@where.com";

			var staff1 = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff1.GS_EmailAddress = "bob@where.com";
			var glbGroup = Factory.New<GlbGroup>();
			glbGroup.GG_Code = "#@3";
			glbGroup.GG_Desc = "TEST GROUP";
			glbGroup.Staff.Add(staff1);

			var staff2 = glbGroup.Staff.AddNew();
			staff2.GS_Code = "#@2";
			staff2.GS_FullName = "STAFF 2";
			staff2.GS_LoginName = "#@2";
			staff2.GS_EmailAddress = "staff2@where.com";

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "#@4";
			staff3.GS_FullName = "STAFF 3";
			staff3.GS_LoginName = "#@4";
			staff3.GS_EmailAddress = "staff3@where.com";

			var systempPassword = Factory.New<GlbExternalPasswordForConfigurationTest>();
			systempPassword.GP_PasswordType = PasswordTypesList.Codes.NTP;
			systempPassword.GP_GC = ZGuid.Empty;
			systempPassword.GP_GG = ZGuid.Empty;
			systempPassword.GP_GS = ZGuid.Empty;
			systempPassword.GP_UserID = "ABC123";
			var companyPassword = Factory.New<GlbExternalPasswordForConfigurationTest>();
			companyPassword.GP_PasswordType = PasswordTypesList.Codes.NTP;
			companyPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			companyPassword.GP_GG = ZGuid.Empty;
			companyPassword.GP_GS = ZGuid.Empty;
			companyPassword.GP_UserID = "ABC123";
			var systemGroupPassword = Factory.New<GlbExternalPasswordForConfigurationTest>();
			systemGroupPassword.GP_PasswordType = PasswordTypesList.Codes.NTP;
			systemGroupPassword.GP_GC = ZGuid.Empty;
			systemGroupPassword.GP_GG = glbGroup.PK;
			systemGroupPassword.GP_GS = ZGuid.Empty;
			systemGroupPassword.GP_UserID = "ABC123";
			var groupPassword = Factory.New<GlbExternalPasswordForConfigurationTest>();
			groupPassword.GP_PasswordType = PasswordTypesList.Codes.NTP;
			groupPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			groupPassword.GP_GG = glbGroup.PK;
			groupPassword.GP_GS = ZGuid.Empty;
			groupPassword.GP_UserID = "ABC123";
			var systemStaffPassword = Factory.New<GlbExternalPasswordForConfigurationTest>();
			systemStaffPassword.GP_PasswordType = PasswordTypesList.Codes.NTP;
			systemStaffPassword.GP_GC = ZGuid.Empty;
			systemStaffPassword.GP_GG = ZGuid.Empty;
			systemStaffPassword.GP_GS = staff3.PK;
			systemStaffPassword.GP_UserID = "ABC123";
			var staffPassword = Factory.New<GlbExternalPasswordForConfigurationTest>();
			staffPassword.GP_PasswordType = PasswordTypesList.Codes.NTP;
			staffPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			staffPassword.GP_GG = ZGuid.Empty;
			staffPassword.GP_GS = staff3.PK;
			staffPassword.GP_UserID = "ABC123";
			Factory.Save();
			var latestOutgoingInterchange = Factory.GetLatestEHubConfigurationInterchange();

			var logger = new LoggingInformation();
			var handler = new GlbExternalPasswordConfigurationHandlerForTesting(logger);
			Configuration configuration = null;
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));

			configuration = new Configuration();
			configuration.Timestamp = systempPassword.GP_SystemLastEditTimeUtc.AddHours(-1);
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));

			var systemGroup = configuration.Group.AddNew();
			systemGroup.Type = Constants.GroupTypes.GroupType;
			var systemGroupAnnotation1 = systemGroup.Annotations.AddNew();
			systemGroupAnnotation1.Value = "SYSTEM NOTE 1";
			var systemGroupAnnotation2 = systemGroup.Annotations.AddNew();
			systemGroupAnnotation2.Value = "SYSTEM NOTE 2";
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Expected group type to be 'System' but 'Group' was found.", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			systemGroup.Type = Constants.GroupTypes.SystemType;
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			systemGroup.Items = new object[] { ZInt.Zero };
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Unknown Group.Item type 'CargoWise.Types.ZInt'.", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			var systemCredential = new Credential() { Name = Constants.CredentialDetails.NextPassword, UserName = "ABC123" };
			var systemPasswordGroup = new Group()
			{
				Type = "!@#",
				Status = PasswordStatusList.Codes.Invalid,
				Items = new object[] { systemCredential }
			};
			systemGroup.Items = new object[] { systemPasswordGroup };
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));

			systemCredential.Name = Constants.CredentialDetails.Current;
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='!@#', Company='NONE', Group='NONE', Staff='NONE').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			systemPasswordGroup.Type = ZString.Empty;
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='', Company='NONE', Group='NONE', Staff='NONE').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			systemPasswordGroup.Type = "NTP";
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "Password (Type='NTP', Company='NONE', Group='NONE', Staff='NONE') has newer changes than XML data.", GetUserLogStrings(logger.UserLogStrings));

			configuration.Timestamp = systempPassword.GP_SystemLastEditTimeUtc.AddHours(1);
			logger.ClearLogs();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(Env.OutgoingMailManager.EmailsCreated[0], "Password Status Has Been Updated to 'Invalid (INV)'", @"Password Type: 'SG National Trade Platform (NTP)'
Owner: System
Password Status: 'Invalid (INV)'
Status Reason:
System: SYSTEM NOTE 1 SYSTEM NOTE 2", new[] { pstStaff.GS_EmailAddress });
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			systempPassword.Reload();
			AssertEquals("systempPassword.GP_PasswordStatus", "INV", systempPassword.GP_PasswordStatus);
			AssertEquals("systempPassword.GP_StatusReason", "System: SYSTEM NOTE 1 SYSTEM NOTE 2", systempPassword.GP_StatusReason);

			systemPasswordGroup.Status = PasswordStatusList.Codes.Valid;
			logger.ClearLogs();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			systempPassword.Reload();
			AssertEquals("systempPassword.GP_PasswordStatus", "OK", systempPassword.GP_PasswordStatus);
			AssertEquals("systempPassword.GP_StatusReason", ZString.Empty, systempPassword.GP_StatusReason);

			var companyGroup = new Group() { Type = Constants.GroupTypes.CompanyType };
			var companyGroupAnnotation1 = companyGroup.Annotations.AddNew();
			companyGroupAnnotation1.Value = "COMPANY NOTE 1";
			var companyGroupAnnotation2 = companyGroup.Annotations.AddNew();
			companyGroupAnnotation2.Value = "COMPANY NOTE 2";
			systemGroup.Items = new object[] { companyGroup };
			configuration.Timestamp = companyPassword.GP_SystemLastEditTimeUtc.AddHours(-1);
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));

			var companyCredential = new Credential() { Name = Constants.CredentialDetails.Current, UserName = "ABC123" };
			var companyPasswordGroup = new Group()
			{
				Type = "!@#",
				Status = PasswordStatusList.Codes.Invalid,
				Items = new object[] { companyCredential }
			};
			companyGroup.Items = new object[] { companyPasswordGroup };
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Company reference cannot be empty.", GetUserLogStrings(logger.UserLogStrings));

			companyGroup.Reference = "#@!";
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Could not find Company with reference '#@!'.", GetUserLogStrings(logger.UserLogStrings));

			companyGroup.Reference = GlbCompany.CurrentCompany.GC_Code;
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='!@#', Company='EDI', Group='NONE', Staff='NONE').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			companyPasswordGroup.Type = ZString.Empty;
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='', Company='EDI', Group='NONE', Staff='NONE').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			companyPasswordGroup.Type = "NTP";
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "Password (Type='NTP', Company='EDI', Group='NONE', Staff='NONE') has newer changes than XML data.", GetUserLogStrings(logger.UserLogStrings));

			configuration.Timestamp = companyPassword.GP_SystemLastEditTimeUtc.AddHours(1);
			logger.ClearLogs();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(Env.OutgoingMailManager.EmailsCreated[0], "Password Status Has Been Updated to 'Invalid (INV)'", @"Password Type: 'SG National Trade Platform (NTP)'
Owner: Company = 'EDI'
Password Status: 'Invalid (INV)'
Status Reason:
System: SYSTEM NOTE 1 SYSTEM NOTE 2
Company: COMPANY NOTE 1 COMPANY NOTE 2", new[] { pstStaff.GS_EmailAddress });
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			companyPassword.Reload();
			AssertEquals("companyPassword.GP_PasswordStatus", "INV", companyPassword.GP_PasswordStatus);
			AssertEquals("companyPassword.GP_StatusReason", "System: SYSTEM NOTE 1 SYSTEM NOTE 2\r\nCompany: COMPANY NOTE 1 COMPANY NOTE 2", companyPassword.GP_StatusReason);

			companyPasswordGroup.Status = PasswordStatusList.Codes.Valid;
			logger.ClearLogs();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			companyPassword.Reload();
			AssertEquals("companyPassword.GP_PasswordStatus", "OK", companyPassword.GP_PasswordStatus);
			AssertEquals("companyPassword.GP_StatusReason", ZString.Empty, companyPassword.GP_StatusReason);

			var groupGroup = new Group() { Type = Constants.GroupTypes.GroupType };
			var groupGroupAnnotation1 = groupGroup.Annotations.AddNew();
			groupGroupAnnotation1.Value = "GROUP NOTE 1";
			var groupGroupAnnotation2 = groupGroup.Annotations.AddNew();
			groupGroupAnnotation2.Value = "GROUP NOTE 2";
			companyGroup.Items = new object[] { groupGroup };
			configuration.Timestamp = groupPassword.GP_SystemLastEditTimeUtc.AddHours(-1);
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));

			var groupCredential = new Credential() { Name = Constants.CredentialDetails.Current, UserName = "ABC123" };
			var groupPasswordGroup = new Group()
			{
				Type = "!@#",
				Status = PasswordStatusList.Codes.Invalid,
				Items = new object[] { groupCredential }
			};
			groupGroup.Items = new object[] { groupPasswordGroup };
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Group reference cannot be empty.", GetUserLogStrings(logger.UserLogStrings));

			groupGroup.Reference = "#@!";
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Could not find Group with reference '#@!'.", GetUserLogStrings(logger.UserLogStrings));

			groupGroup.Reference = glbGroup.GG_Code;
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='!@#', Company='EDI', Group='#@3', Staff='NONE').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			groupPasswordGroup.Type = ZString.Empty;
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='', Company='EDI', Group='#@3', Staff='NONE').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			groupPasswordGroup.Type = "NTP";
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "Password (Type='NTP', Company='EDI', Group='#@3', Staff='NONE') has newer changes than XML data.", GetUserLogStrings(logger.UserLogStrings));

			configuration.Timestamp = groupPassword.GP_SystemLastEditTimeUtc.AddHours(1);
			logger.ClearLogs();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(Env.OutgoingMailManager.EmailsCreated[0], "Password Status Has Been Updated to 'Invalid (INV)'", @"Password Type: 'SG National Trade Platform (NTP)'
Owner: Company = 'EDI', Group = '#@3'
Password Status: 'Invalid (INV)'
Status Reason:
System: SYSTEM NOTE 1 SYSTEM NOTE 2
Company: COMPANY NOTE 1 COMPANY NOTE 2
Group: GROUP NOTE 1 GROUP NOTE 2", new[] { staff1.GS_EmailAddress, staff2.GS_EmailAddress });
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			groupPassword.Reload();
			AssertEquals("groupPassword.GP_PasswordStatus", "INV", groupPassword.GP_PasswordStatus);
			AssertEquals("groupPassword.GP_StatusReason", "System: SYSTEM NOTE 1 SYSTEM NOTE 2\r\nCompany: COMPANY NOTE 1 COMPANY NOTE 2\r\nGroup: GROUP NOTE 1 GROUP NOTE 2", groupPassword.GP_StatusReason);

			groupPasswordGroup.Status = PasswordStatusList.Codes.Valid;
			logger.ClearLogs();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			groupPassword.Reload();
			AssertEquals("groupPassword.GP_PasswordStatus", "OK", groupPassword.GP_PasswordStatus);
			AssertEquals("groupPassword.GP_StatusReason", ZString.Empty, groupPassword.GP_StatusReason);

			var systemGroupGroup = new Group() { Type = Constants.GroupTypes.GroupType };
			var systemGroupGroupAnnotation1 = systemGroupGroup.Annotations.AddNew();
			systemGroupGroupAnnotation1.Value = "SYSTEM GROUP NOTE 1";
			var systemGroupGroupAnnotation2 = systemGroupGroup.Annotations.AddNew();
			systemGroupGroupAnnotation2.Value = "SYSTEM GROUP NOTE 2";
			systemGroup.Items = new object[] { systemGroupGroup };
			configuration.Timestamp = systemGroupPassword.GP_SystemLastEditTimeUtc.AddHours(-1);
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));

			var systemGroupCredential = new Credential() { Name = Constants.CredentialDetails.Current, UserName = "ABC123" };
			var systemGroupPasswordGroup = new Group()
			{
				Type = "!@#",
				Status = PasswordStatusList.Codes.Invalid,
				Items = new object[] { systemGroupCredential }
			};
			systemGroupGroup.Items = new object[] { systemGroupPasswordGroup };
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Group reference cannot be empty.", GetUserLogStrings(logger.UserLogStrings));

			systemGroupGroup.Reference = "#@!";
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Could not find Group with reference '#@!'.", GetUserLogStrings(logger.UserLogStrings));

			systemGroupGroup.Reference = glbGroup.GG_Code;
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='!@#', Company='NONE', Group='#@3', Staff='NONE').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			systemGroupPasswordGroup.Type = ZString.Empty;
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='', Company='NONE', Group='#@3', Staff='NONE').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			systemGroupPasswordGroup.Type = "NTP";
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "Password (Type='NTP', Company='NONE', Group='#@3', Staff='NONE') has newer changes than XML data.", GetUserLogStrings(logger.UserLogStrings));

			configuration.Timestamp = systemGroupPassword.GP_SystemLastEditTimeUtc.AddHours(1);
			logger.ClearLogs();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(Env.OutgoingMailManager.EmailsCreated[0], "Password Status Has Been Updated to 'Invalid (INV)'", @"Password Type: 'SG National Trade Platform (NTP)'
Owner: Group = '#@3'
Password Status: 'Invalid (INV)'
Status Reason:
System: SYSTEM NOTE 1 SYSTEM NOTE 2
Group: SYSTEM GROUP NOTE 1 SYSTEM GROUP NOTE 2", new[] { staff1.GS_EmailAddress, staff2.GS_EmailAddress });
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			systemGroupPassword.Reload();
			AssertEquals("systemGroupPassword.GP_PasswordStatus", "INV", systemGroupPassword.GP_PasswordStatus);
			AssertEquals("systemGroupPassword.GP_StatusReason", "System: SYSTEM NOTE 1 SYSTEM NOTE 2\r\nGroup: SYSTEM GROUP NOTE 1 SYSTEM GROUP NOTE 2", systemGroupPassword.GP_StatusReason);

			systemGroupPasswordGroup.Status = PasswordStatusList.Codes.Valid;
			logger.ClearLogs();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			systemGroupPassword.Reload();
			AssertEquals("systemGroupPassword.GP_PasswordStatus", "OK", systemGroupPassword.GP_PasswordStatus);
			AssertEquals("systemGroupPassword.GP_StatusReason", ZString.Empty, systemGroupPassword.GP_StatusReason);

			var systemStaffGroup = new Group() { Type = Constants.GroupTypes.StaffType };
			var systemStaffGroupAnnotation1 = systemStaffGroup.Annotations.AddNew();
			systemStaffGroupAnnotation1.Value = "SYSTEM STAFF NOTE 1";
			var systemStaffGroupAnnotation2 = systemStaffGroup.Annotations.AddNew();
			systemStaffGroupAnnotation2.Value = "SYSTEM STAFF NOTE 2";
			systemGroup.Items = new object[] { systemStaffGroup };
			configuration.Timestamp = systemStaffPassword.GP_SystemLastEditTimeUtc.AddHours(-1);
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));

			var systemStaffCredential = new Credential() { Name = Constants.CredentialDetails.Current, UserName = "ABC123" };
			var systemStaffPasswordGroup = new Group()
			{
				Type = "!@#",
				Status = PasswordStatusList.Codes.Invalid,
				Items = new object[] { systemStaffCredential }
			};
			systemStaffGroup.Items = new object[] { systemStaffPasswordGroup };
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Staff reference cannot be empty.", GetUserLogStrings(logger.UserLogStrings));

			systemStaffGroup.Reference = "#@!";
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Could not find Staff with reference '#@!'.", GetUserLogStrings(logger.UserLogStrings));

			systemStaffGroup.Reference = staff3.GS_Code;
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='!@#', Company='NONE', Group='NONE', Staff='#@4').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			systemStaffPasswordGroup.Type = ZString.Empty;
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='', Company='NONE', Group='NONE', Staff='#@4').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			systemStaffPasswordGroup.Type = "NTP";
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "Password (Type='NTP', Company='NONE', Group='NONE', Staff='#@4') has newer changes than XML data.", GetUserLogStrings(logger.UserLogStrings));

			configuration.Timestamp = systemStaffPassword.GP_SystemLastEditTimeUtc.AddHours(1);
			logger.ClearLogs();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(Env.OutgoingMailManager.EmailsCreated[0], "Password Status Has Been Updated to 'Invalid (INV)'", @"Password Type: 'SG National Trade Platform (NTP)'
Owner: Staff = '#@4'
Password Status: 'Invalid (INV)'
Status Reason:
System: SYSTEM NOTE 1 SYSTEM NOTE 2
Staff: SYSTEM STAFF NOTE 1 SYSTEM STAFF NOTE 2", new[] { staff3.GS_EmailAddress });
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			systemStaffPassword.Reload();
			AssertEquals("systemStaffPassword.GP_PasswordStatus", "INV", systemStaffPassword.GP_PasswordStatus);
			AssertEquals("systemStaffPassword.GP_StatusReason", "System: SYSTEM NOTE 1 SYSTEM NOTE 2\r\nStaff: SYSTEM STAFF NOTE 1 SYSTEM STAFF NOTE 2", systemStaffPassword.GP_StatusReason);

			systemStaffPasswordGroup.Status = PasswordStatusList.Codes.Valid;
			logger.ClearLogs();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			systemStaffPassword.Reload();
			AssertEquals("systemStaffPassword.GP_PasswordStatus", "OK", systemStaffPassword.GP_PasswordStatus);
			AssertEquals("systemStaffPassword.GP_StatusReason", ZString.Empty, systemStaffPassword.GP_StatusReason);

			var staffGroup = new Group() { Type = Constants.GroupTypes.StaffType };
			var staffGroupAnnotation1 = staffGroup.Annotations.AddNew();
			staffGroupAnnotation1.Value = "STAFF NOTE 1";
			var staffGroupAnnotation2 = staffGroup.Annotations.AddNew();
			staffGroupAnnotation2.Value = "STAFF NOTE 2";
			companyGroup.Items = new object[] { staffGroup };
			systemGroup.Items = new object[] { companyGroup };
			configuration.Timestamp = staffPassword.GP_SystemLastEditTimeUtc.AddHours(-1);
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));

			var staffCredential = new Credential() { Name = Constants.CredentialDetails.Current, UserName = "ABC123" };
			var staffPasswordGroup = new Group()
			{
				Type = "!@#",
				Status = PasswordStatusList.Codes.Invalid,
				Items = new object[] { staffCredential }
			};
			staffGroup.Items = new object[] { staffPasswordGroup };
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Staff reference cannot be empty.", GetUserLogStrings(logger.UserLogStrings));

			staffGroup.Reference = "#@!";
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "Could not find Staff with reference '#@!'.", GetUserLogStrings(logger.UserLogStrings));

			staffGroup.Reference = staff3.GS_Code;
			logger.ClearLogs();
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='!@#', Company='EDI', Group='NONE', Staff='#@4').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			staffPasswordGroup.Type = ZString.Empty;
			AssertEquals(false, handler.Process(configuration));
			AssertEquals("Log", "No password found matching (Type='', Company='EDI', Group='NONE', Staff='#@4').", GetUserLogStrings(logger.UserLogStrings));

			logger.ClearLogs();
			staffPasswordGroup.Type = "NTP";
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "Password (Type='NTP', Company='EDI', Group='NONE', Staff='#@4') has newer changes than XML data.", GetUserLogStrings(logger.UserLogStrings));

			configuration.Timestamp = staffPassword.GP_SystemLastEditTimeUtc.AddHours(1);
			logger.ClearLogs();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(Env.OutgoingMailManager.EmailsCreated[0], "Password Status Has Been Updated to 'Invalid (INV)'", @"Password Type: 'SG National Trade Platform (NTP)'
Owner: Company = 'EDI', Staff = '#@4'
Password Status: 'Invalid (INV)'
Status Reason:
System: SYSTEM NOTE 1 SYSTEM NOTE 2
Company: COMPANY NOTE 1 COMPANY NOTE 2
Staff: STAFF NOTE 1 STAFF NOTE 2", new[] { staff3.GS_EmailAddress });
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			staffPassword.Reload();
			AssertEquals("staffPassword.GP_PasswordStatus", "INV", staffPassword.GP_PasswordStatus);
			AssertEquals("staffPassword.GP_StatusReason", "System: SYSTEM NOTE 1 SYSTEM NOTE 2\r\nCompany: COMPANY NOTE 1 COMPANY NOTE 2\r\nStaff: STAFF NOTE 1 STAFF NOTE 2", staffPassword.GP_StatusReason);

			staffPasswordGroup.Status = PasswordStatusList.Codes.Valid;
			logger.ClearLogs();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(true, handler.Process(configuration));
			AssertEquals("Log", "", GetUserLogStrings(logger.UserLogStrings));
			AssertEquals("Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("No new interchange", latestOutgoingInterchange.PK, new BusinessObjectFactory().GetLatestEHubConfigurationInterchange().PK);
			staffPassword.Reload();
			AssertEquals("staffPassword.GP_PasswordStatus", "OK", staffPassword.GP_PasswordStatus);
			AssertEquals("staffPassword.GP_StatusReason", ZString.Empty, staffPassword.GP_StatusReason);
		}

		class GlbExternalPasswordConfigurationHandlerForTesting : GlbExternalPasswordConfigurationHandler
		{
			public GlbExternalPasswordConfigurationHandlerForTesting(LoggingInformation logger)
			: base(logger)
			{
			}
		}
	}
}
