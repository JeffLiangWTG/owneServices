using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using static CargoWise.EntityFramework.BusinessObjectFactory;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class BusinessObjectChangesEmailNotifierTest : TestCaseWithFactory
	{
		public void TestAddEmailRecipientsLookingAtNotificationOptions()
		{
			GlbStaff groupStaff = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			groupStaff.GS_EmailAddress = "staffG@edi.com.au";
			GlbStaff roleStaff = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			roleStaff.GS_EmailAddress = "staffR@edi.com.au";

			DummyBizOWithNotifier testBizO = DummyBizOWithNotifier.New(Factory, null, Helper.TestContact, WebDataRegistry.Instance.BookingNotificationEmailGroup);
			testBizO.Z0_Number = 1234;

			CodeDescriptionBoolDisallowNewCollection roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			WebDataRegistry.Instance.BookingsNotificationStaffRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);
			testBizO.StaffRolesToNotifyDummy = WebDataRegistry.Instance.BookingsNotificationStaffRoles;

			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "RUMOW"));
			GlbCompany testCompany = Factory.New<GlbCompany>();
			testCompany.GC_RN_NKCountryCode = uNLOCO.RL_RN_NKCountryCode;
			GlbBranch testBranch = testCompany.Branches.AddNew();
			testBranch.GB_RL_NKHomePort = uNLOCO.RL_Code;

			GlbGroup company1SydneyGroup = Factory.New<GlbGroup>();
			company1SydneyGroup.GG_Code = "SYD1";
			company1SydneyGroup.GG_Desc = "Company 1 Sydney";
			company1SydneyGroup.GG_IsActive = ZBool.True;
			company1SydneyGroup.Staff.Add(groupStaff);
			WebDataRegistry.Instance.BookingNotificationEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, company1SydneyGroup.PK.ToGuid());

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TST";
			org.OH_FullName = "Test Company";
			org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, roleStaff.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Sea);
			testBizO.RelatedOrgDummy = org;

			TriggerEmailCreation(testBizO);

			AssertEquals("Emails should be sent both to grp-recepient and to rol-recipient by one email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			const string roleFooter = @"This email was sent to:

 - an assigned staff for TST Test Company to those who have next roles:
Account Manager.
You can change this in the setting located at
System Registry: Web and Visibility -> Forwarding -> Bookings -> Notification Staff Roles -> System.

 - the email group defined at System Registry:
System Registry: Web and Visibility -> Forwarding -> Bookings -> Notification Group -> System.";
			AssertEquals("Footer is in Roles notofication format", roleFooter, Env.OutgoingMailManager.EmailsCreated[0].FooterText);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("staffR@edi.com.au", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals("staffG@edi.com.au", Env.OutgoingMailManager.EmailsCreated[0].Recipients[1].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			WebDataRegistry.Instance.BookingNotificationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL);
			TriggerEmailCreation(testBizO);

			AssertEquals("Email should be sent to rol-recipient only", 1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("staffR@edi.com.au", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			roleStaff.GS_EmailAddress = string.Empty;
			TriggerEmailCreation(testBizO);

			AssertEquals("Email should be sent to grp-recepient as there are no rol-recipients", 1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("staffG@edi.com.au", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			roleStaff.GS_EmailAddress = "staffR@edi.com.au";
			WebDataRegistry.Instance.BookingNotificationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.GRP);
			TriggerEmailCreation(testBizO);

			AssertEquals("Email should be sent to grp-recepient only", 1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("staffG@edi.com.au", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			groupStaff.GS_EmailAddress = "";
			TriggerEmailCreation(testBizO);

			AssertEquals("Email should be sent to roles as there are no grp-recepients", 1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("staffR@edi.com.au", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			groupStaff.GS_EmailAddress = "staffG@edi.com.au";
			WebDataRegistry.Instance.BookingNotificationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.NON);
			TriggerEmailCreation(testBizO);

			AssertEquals("Should be no emails created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestEmailNotificationIsNotSentWhenHasChangesNotChanged()
		{
			DummyBizOWithNotifier testBizO = CreateBizOAndStaff(true);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			testBizO.NonHasChangesAffectingProperty = true;

			AssertEquals("HasChanges should still be false", false, testBizO.HasChanges);

			testBizO.Factory.Save();

			AssertEquals("Email count should be zero", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestEmailNotificationIsSentOnCreate()
		{
			AssertEmailDetails(GetExpectedEmailLocation(placed), CurrentLocation, TwoRecipients, CreateBizOAndStaff(true));
		}

		public void TestEmailNotificationIsSentOnModify()
		{
			DummyBizOWithNotifier testBizO = CreateBizOAndStaff(true);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			testBizO.Z0_Date = ZDateTime.Now;
			testBizO.Factory.Save();

			AssertEmailDetails(GetExpectedEmailLocation(modified), CurrentLocation, TwoRecipients, testBizO);
		}

		public void TestEmailNotificationIsSentOnCancel()
		{
			DummyBizOWithNotifier testBizO = CreateBizOAndStaff(true);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			testBizO.IsCancelled = ZBool.True;
			testBizO.Factory.Save();

			AssertEmailDetails(GetExpectedEmailLocation(cancelled), CurrentLocation, TwoRecipients, testBizO);
		}

		public void TestEmailNotificationIsSentOnWebUserNoteAddedWhenCreated()
		{
			DummyBizOWithNotifier testBizO = CreateBizOAndStaff(false);

			testBizO.UserEditableNoteHelper.EditableNoteText = "Note";
			testBizO.Factory.Save();

			AssertEquals("IsWebUserEditableNoteAdded shout be reset to False", false, testBizO.UserEditableNoteHelper.IsNoteAdded);
			AssertEmailDetails(GetExpectedEmailLocation(placed, newInstruction), CurrentLocation, TwoRecipients, testBizO);
		}

		public void TestEmailNotificationIsSentOnWebUserNoteAddedWhenModified()
		{
			DummyBizOWithNotifier testBizO = CreateBizOAndStaff(true);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			testBizO.UserEditableNoteHelper.EditableNoteText = "Note";
			testBizO.Z0_Date = ZDateTime.Now;
			testBizO.Factory.Save();

			AssertEquals("IsWebUserEditableNoteAdded shout be reset to False", false, testBizO.UserEditableNoteHelper.IsNoteAdded);
			AssertEmailDetails(GetExpectedEmailLocation(modified, newInstruction), CurrentLocation, TwoRecipients, testBizO);
		}

		public void TestEmailNotificationIsSentOnWebUserNoteAddedWhenNotModified()
		{
			DummyBizOWithNotifier testBizO = CreateBizOAndStaff(true);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			testBizO.UserEditableNoteHelper.EditableNoteText = "Note";
			AssertEquals("BizO.HasChanges", true, testBizO.HasChanges);
			testBizO.Factory.Save();

			AssertEquals("IsWebUserEditableNoteAdded shout be reset to False", false, testBizO.UserEditableNoteHelper.IsNoteAdded);
			AssertEmailDetails(GetExpectedEmailLocation(modified, newInstruction), CurrentLocation, TwoRecipients, testBizO);
		}

		public void TestEmailNotificationIsSentOnWebUserNoteAddedWhenCancelled()
		{
			DummyBizOWithNotifier testBizO = CreateBizOAndStaff(true);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			testBizO.IsCancelled = true;
			testBizO.UserEditableNoteHelper.EditableNoteText = "Note";
			testBizO.Factory.Save();

			AssertEquals("IsWebUserEditableNoteAdded shout be reset to False", false, testBizO.UserEditableNoteHelper.IsNoteAdded);
			AssertEmailDetails(GetExpectedEmailLocation(cancelled, newInstruction), CurrentLocation, TwoRecipients, testBizO);
		}

		public void TestEmailNotificationIsSentOnWebUserNoteChanged()
		{
			DummyBizOWithNotifier testBizO = CreateBizOAndStaff(false);

			testBizO.UserEditableNoteHelper.EditableNoteText = "Note";
			testBizO.Factory.Save();
			testBizO.Notifier = new BusinessObjectChangesEmailNotifier(testBizO);

			AssertEquals("Email count should be one", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			testBizO.UserEditableNoteHelper.EditableNoteText = "Note 1";
			testBizO.Z0_Date = ZDateTime.Now;
			testBizO.Factory.Save();
			testBizO.Notifier = new BusinessObjectChangesEmailNotifier(testBizO);

			AssertEquals("IsWebUserEditableNoteChanged shout be reset to False", false, testBizO.UserEditableNoteHelper.IsNoteChanged);
			AssertEmailDetails(GetExpectedEmailLocation(modified, modifiedInstruction), CurrentLocation, TwoRecipients, testBizO);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			testBizO.IsCancelled = true;
			testBizO.UserEditableNoteHelper.EditableNoteText = "Note 2";
			testBizO.Factory.Save();

			AssertEquals("IsWebUserEditableNoteChanged shout be reset to False", false, testBizO.UserEditableNoteHelper.IsNoteChanged);
			AssertEmailDetails(GetExpectedEmailLocation(cancelled, modifiedInstruction), CurrentLocation, TwoRecipients, testBizO);
		}

		public void TestEmailNotificationIsSentOneTimeOnly()
		{
			DummyBizOWithNotifier testBizO = CreateBizOAndStaff(false);
			testBizO.Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			SavedEventHandler savedHandler = null;
			savedHandler = (factory, success) =>
			{
				factory.Saved -= savedHandler;
				testBizO.Z0_Code = "Test2";
				factory.Save();
			};

			testBizO.Factory.Saved += savedHandler;
			testBizO.Notifier = new BusinessObjectChangesEmailNotifier(testBizO);

			testBizO.Z0_Code = "Test1";
			testBizO.Factory.Save();
			AssertEquals("Email count should be one", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestEmailNotificationUsesBranchAndCompany()
		{
			#region Company / Staff Setup

			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Name = "Logistics Company One Ltd";
			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "AAA";
			branch1.GB_BranchName = "SYD1";
			branch1.GB_RL_NKHomePort = "AUSYD";

			GlbBranch branch2 = company1.Branches.AddNew();
			branch2.GB_Code = "AAB";
			branch2.GB_BranchName = "MEL1";
			branch2.GB_RL_NKHomePort = "AUMEL";

			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Name = "Logistics Company Two Ltd";
			GlbBranch branch3 = company2.Branches.AddNew();
			branch3.GB_Code = "ABA";
			branch3.GB_BranchName = "SYD2";
			branch3.GB_RL_NKHomePort = "AUSYD";

			GlbBranch branch4 = company2.Branches.AddNew();
			branch4.GB_Code = "ABB";
			branch4.GB_BranchName = "MEL2";
			branch4.GB_RL_NKHomePort = "AUMEL";

			GlbStaff staff1 = CreateStaff("staff1@edi.com.au");
			GlbStaff staff2 = CreateStaff("staff2@edi.com.au");
			GlbStaff staff3 = CreateStaff("staff3@edi.com.au");
			GlbStaff staff4 = CreateStaff("staff4@edi.com.au");
			GlbStaff staff5 = CreateStaff("staff5@edi.com.au");
			GlbStaff staff6 = CreateStaff("staff6@edi.com.au");
			GlbStaff staff7 = CreateStaff("staff7@edi.com.au");
			GlbStaff staff8 = CreateStaff("staff8@edi.com.au");
			GlbStaff staff9 = CreateStaff("staff9@edi.com.au");
			GlbStaff staff10 = CreateStaff("staff10@edi.com.au");

			GlbGroup company1SydneyGroup = Factory.New<GlbGroup>();
			company1SydneyGroup.GG_Code = "SYD1";
			company1SydneyGroup.GG_Desc = "Company 1 Sydney";
			company1SydneyGroup.GG_IsActive = ZBool.True;
			company1SydneyGroup.Staff.Add(staff1);
			company1SydneyGroup.Staff.Add(staff2);
			WebDataRegistry.Instance.BookingNotificationEmailGroup.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1SydneyGroup.PK.ToGuid());

			GlbGroup company1MelbourneGroup = Factory.New<GlbGroup>();
			company1MelbourneGroup.GG_Code = "MEL1";
			company1MelbourneGroup.GG_Desc = "Company 1 Melbourne";
			company1MelbourneGroup.GG_IsActive = ZBool.True;
			company1MelbourneGroup.Staff.Add(staff3);
			company1MelbourneGroup.Staff.Add(staff4);
			WebDataRegistry.Instance.BookingNotificationEmailGroup.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, company1MelbourneGroup.PK.ToGuid());

			GlbGroup company2SydneyGroup = Factory.New<GlbGroup>();
			company2SydneyGroup.GG_Code = "SYD2";
			company2SydneyGroup.GG_Desc = "Company 2 Sydney";
			company2SydneyGroup.GG_IsActive = ZBool.True;
			company2SydneyGroup.Staff.Add(staff5);
			company2SydneyGroup.Staff.Add(staff6);
			WebDataRegistry.Instance.BookingNotificationEmailGroup.SetValue(Guid.Empty, branch3.PK.ToGuid(), Guid.Empty, company2SydneyGroup.PK.ToGuid());

			GlbGroup company2MelbourneGroup = Factory.New<GlbGroup>();
			company2MelbourneGroup.GG_Code = "MEL2";
			company2MelbourneGroup.GG_Desc = "Company 2 Melbourne";
			company2MelbourneGroup.GG_IsActive = ZBool.True;
			company2MelbourneGroup.Staff.Add(staff7);
			company2MelbourneGroup.Staff.Add(staff8);
			WebDataRegistry.Instance.BookingNotificationEmailGroup.SetValue(Guid.Empty, branch4.PK.ToGuid(), Guid.Empty, company2MelbourneGroup.PK.ToGuid());

			Factory.Save();

			#endregion

			DummyBizOWithNotifier testBizO = DummyBizOWithNotifier.New(Factory, null, Helper.TestContact, WebDataRegistry.Instance.BookingNotificationEmailGroup);
			testBizO.Z0_Number = 1234;

			TriggerEmailCreation(testBizO);

			string expectedEmailSubject = "DummyBizo 123 has been placed";
			string expectedLocation = String.Format("{0} -> System", BookingsEmailGroupRegistryItem);

			//Branch is null so it sends to ALL STAFF
			AssertEmailDetails(expectedEmailSubject, expectedLocation, new StringCollection { "staff1@edi.com.au",
																									"staff2@edi.com.au",
																									"staff3@edi.com.au",
																									"staff4@edi.com.au",
																									"staff5@edi.com.au",
																									"staff6@edi.com.au",
																									"staff7@edi.com.au",
																									"staff8@edi.com.au",
																									"staff9@edi.com.au",
																									"staff10@edi.com.au" }, testBizO);

			testBizO.EventBranchSetterForTest = GlbBranch.CurrentBranch;
			TriggerEmailCreation(testBizO);

			expectedEmailSubject = "DummyBizo 123 has been modified";
			expectedLocation = String.Format("{0} -> Companies -> {1} -> Branches -> {2}",
				BookingsEmailGroupRegistryItem,
				GlbCompany.CurrentCompany.GC_Name, GlbBranch.CurrentBranch.GB_BranchName);

			//NO concrete staff was set in current branch -- it sends to default ALL STAFF
			AssertEmailDetails(expectedEmailSubject, expectedLocation, new StringCollection { "staff1@edi.com.au",
																									"staff2@edi.com.au",
																									"staff3@edi.com.au",
																									"staff4@edi.com.au",
																									"staff5@edi.com.au",
																									"staff6@edi.com.au",
																									"staff7@edi.com.au",
																									"staff8@edi.com.au",
																									"staff9@edi.com.au",
																									"staff10@edi.com.au" }, testBizO);

			testBizO.EventBranchSetterForTest = branch1;
			TriggerEmailCreation(testBizO);

			expectedLocation = String.Format("{0} -> Companies -> {1} -> Branches -> {2}",
				BookingsEmailGroupRegistryItem,
				company1.GC_Name, branch1.GB_BranchName);

			//It will send to concrete Sydney branch staff
			AssertEmailDetails(expectedEmailSubject, expectedLocation, new StringCollection { "staff1@edi.com.au",
																									"staff2@edi.com.au" }, testBizO);

			testBizO.EventBranchSetterForTest = branch4;
			TriggerEmailCreation(testBizO);

			expectedLocation = String.Format("{0} -> Companies -> {1} -> Branches -> {2}",
				BookingsEmailGroupRegistryItem,
				company2.GC_Name, branch4.GB_BranchName);

			//It will send to concrete Melbourne branch staff
			AssertEmailDetails(expectedEmailSubject, expectedLocation, new StringCollection { "staff7@edi.com.au",
																									"staff8@edi.com.au" }, testBizO);
		}

		public void TestFooterInGroupFormatAndSentToAllUsers()
		{
			WebDataRegistry.Instance.BookingNotificationEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			CreateBizOAndStaff(true);

			AssertEquals("Email count should be one", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("email is not null", notificationEmail);
			string footer = String.Format(@"This email was sent to the email group defined at
System Registry: {0}.
Since there are no valid email addresses set up in this group this email has been sent to all users.", CurrentLocation);
			AssertContains("Footer is in Group notofication format sent to all users", footer, notificationEmail.FooterText);

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestEmailLanguage()
		{
			var staff1 = CreateStaff("staff1@edi.com.au");
			var staff2 = CreateStaff("staff2@edi.com.au");
			var staff3 = CreateStaff("staff3@edi.com.au");
			var staff4 = CreateStaff("staff4@edi.com.au");
			staff2.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			staff3.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			staff4.GS_WorkingLanguage = Core.SharedConstants.Languages.Spanish;

			using (var languageTestHelper = new LanguageTestHelper())
			{
				var bizO = DummyBizOWithNotifier.New(Factory, GlbBranch.CurrentBranch, Helper.TestContact, WebDataRegistry.Instance.BookingNotificationEmailGroup);
				bizO.Z0_Number = 1234;

				var roles = StaffRolesNotificationHelper.GetRoles();
				StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
				WebDataRegistry.Instance.BookingsNotificationStaffRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);
				WebDataRegistry.Instance.BookingNotificationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL);

				var org = OrgHeader.New(Factory);
				org.OH_Code = "SPRNUCSYD";
				org.OH_FullName = "Springfield Nuclear Power Plant";
				bizO.RelatedOrgDummy = org;
				bizO.StaffRolesToNotifyDummy = WebDataRegistry.Instance.BookingsNotificationStaffRoles;

				var group = Factory.New<GlbGroup>();
				WebDataRegistry.Instance.BookingNotificationEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

				org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, staff1.GS_Code, OrgStaffAssignmentsLookups.AllServices);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.EnglishAmerican);

				org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, staff2.GS_Code, OrgStaffAssignmentsLookups.AllServices);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.French);

				org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[1].Code, staff3.GS_Code, OrgStaffAssignmentsLookups.AllServices);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.French);

				org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[2].Code, staff4.GS_Code, OrgStaffAssignmentsLookups.AllServices);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.EnglishAmerican);

				WebDataRegistry.Instance.BookingNotificationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ALL);
				org.StaffAssignments.RemoveAndDeleteAll();
				org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, staff2.GS_Code, OrgStaffAssignmentsLookups.AllServices);

				group.Staff.Add(staff3);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.French);

				group.Staff.Add(staff4);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.EnglishAmerican);

				group.Staff.RemoveAll();
				WebDataRegistry.Instance.BookingNotificationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.GRP);

				group.Staff.Add(staff1);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.EnglishAmerican);

				group.Staff.Add(staff2);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.EnglishAmerican);

				group.Staff.Remove(staff1);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.French);

				group.Staff.Add(staff3);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.French);

				group.Staff.Add(staff4);
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.EnglishAmerican);

				group.Staff.RemoveAll();
				WebDataRegistry.Instance.BookingNotificationEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
				TriggerEmailCreation(bizO);
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.EnglishAmerican);
			}
		}

		internal class LanguageTestHelper : IDisposable
		{
			public LanguageTestHelper()
			{
				research = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Core.SharedConstants.Languages.EnglishAmerican);
				eng = Res.GetLanguageInstance(Core.SharedConstants.Languages.EnglishAmerican).UseMockData();
				chs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData();
				frn = Res.GetLanguageInstance(Core.SharedConstants.Languages.French).UseMockData();

				eng.SetResourceGetter(delegate(string key)
				{ return GetData(key, "☺"); });
				chs.SetResourceGetter(delegate(string key)
				{ return GetData(key, "好"); });
				frn.SetResourceGetter(delegate(string key)
				{ return GetData(key, "♥"); });

				currentLanguageChange = Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified);
			}

			ResourceStringData GetData(string key, string placeholder)
			{
				if (key.StartsWith("StaffAssignmentRoles"))
				{
					return null;
				}
				else
				{
					var data = research.Get(key);
					string actualCaption = data == null ? string.Empty : string.IsNullOrEmpty(data.Caption) ? data.FullDescription : data.Caption;
					return new ResourceStringData(key, placeholder + " " + actualCaption);
				}
			}

			public void Dispose()
			{
				currentLanguageChange.Dispose();
				frn.Dispose();
				chs.Dispose();
				eng.Dispose();
			}

			public void AssertEmailLanguage(string language)
			{
				AssertEquals("Email count should be one", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				switch (language)
				{
					case Core.SharedConstants.Languages.EnglishAmerican:
						AssertNotContains("Email should be in English", "好", email.Subject);
						AssertNotContains("Email should be in English", "♥", email.Subject);
						AssertContains("Email should be in English", "☺", email.Subject);
						AssertNotContains("Email should be in English", "好", email.Body);
						AssertNotContains("Email should be in English", "♥", email.Body);
						AssertContains("Email should be in English", "☺", email.Body);
						break;

					case Core.SharedConstants.Languages.French:
						AssertNotContains("Email should be in French", "好", email.Subject);
						AssertNotContains("Email should be in French", "☺", email.Subject);
						AssertContains("Email should be in French", "♥", email.Subject);
						AssertNotContains("Email should be in French", "好", email.Body);
						AssertNotContains("Email should be in French", "☺", email.Body);
						AssertContains("Email should be in French", "♥", email.Body);
						break;

					default:
						throw new InvalidOperationException();
				}
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}

			readonly ISimpleResourceStringCache research;
			readonly IMockResourceStringCache eng;
			readonly IMockResourceStringCache chs;
			readonly IMockResourceStringCache frn;
			readonly IDisposable currentLanguageChange;
		}

		#region Implementation

		void TriggerEmailCreation(DummyBizOWithNotifier testBizO)
		{
			testBizO.Z0_Date = ZDateTime.Now;
			AssertNoErrorsAndNoEmails(testBizO);
			testBizO.Factory.Save();
			testBizO.Notifier = new BusinessObjectChangesEmailNotifier(testBizO);
		}

		static string GetExpectedEmailLocation(params string[] consts)
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("DummyBizo 123 has been ");
			foreach (string param in consts)
			{
				result.Append(param);
			}
			return result.ToString();
		}

		const string placed = "placed";
		const string modified = "modified";
		const string cancelled = "canceled";
		const string newInstruction = " and has new Special Instructions";
		const string modifiedInstruction = " and has modified Special Instructions";

		DummyBizOWithNotifier CreateBizOAndStaff(bool saveBizO)
		{
			DummyBizOWithNotifier bizO = DummyBizOWithNotifier.New(Factory, GlbBranch.CurrentBranch, Helper.TestContact, WebDataRegistry.Instance.BookingNotificationEmailGroup);
			bizO.Z0_Number = 1234;
			CreateStaff("staff1@edi.com.au");
			CreateStaff("staff2@edi.com.au");
			AssertNoErrorsAndNoEmails(bizO);
			if (saveBizO)
			{
				bizO.Factory.Save();
				bizO.Notifier = new BusinessObjectChangesEmailNotifier(bizO);
			}
			return bizO;
		}

		static void AssertNoErrorsAndNoEmails(DummyBizOWithNotifier bizO)
		{
			bizO.RunPreSaveValidation();
			AssertEquals("Precondition: Should be no errors", false, bizO.HasNotifications());
			AssertEquals("PreCondition: Email count should be zero", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		StringCollection TwoRecipients
		{
			get
			{
				if (fTwoRecipients == null)
				{
					fTwoRecipients = new StringCollection { "staff1@edi.com.au", "staff2@edi.com.au" };
				}
				return fTwoRecipients;
			}
		}
		StringCollection fTwoRecipients;

		string CurrentLocation
		{
			get
			{
				if (fCurrentLocation == null)
				{
					fCurrentLocation = String.Format("{0} -> Companies -> {1} -> Branches -> {2}",
													 BookingsEmailGroupRegistryItem,
													 GlbCompany.CurrentCompany.GC_Name,
																					 GlbBranch.CurrentBranch.GB_BranchName);
				}
				return fCurrentLocation;
			}
		}
		string fCurrentLocation;

		string BookingsEmailGroupRegistryItem
		{
			get
			{
				if (fBookingsEmailGroupRegistryItem == null)
				{
					fBookingsEmailGroupRegistryItem = String.Format("{0} -> {1}",
														WebDataRegistry.Instance.BookingNotificationEmailGroup.Category.Replace("/", " -> "),
														WebDataRegistry.Instance.BookingNotificationEmailGroup.Caption);
				}
				return fBookingsEmailGroupRegistryItem;
			}
		}
		string fBookingsEmailGroupRegistryItem;

		GlbStaff CreateStaff(string emailAddress)
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			staff.GS_EmailAddress = emailAddress;
			return staff;
		}

		void AssertEmailDetails(string expectedEmailSubject, string expectedLocation, StringCollection expectedRecipients, DummyBizOWithNotifier testBizO)
		{
			AssertEquals("Email count should be one", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Notification Email", notificationEmail);
			AssertEquals("Notification Email Subject", expectedEmailSubject, notificationEmail.Subject);

			AssertContains("Email Subject", expectedEmailSubject, notificationEmail.Body);
			AssertContains("Human Readable Name", "DummyBizo", notificationEmail.Body);
			AssertContains("Number", "123", notificationEmail.Body);
			AssertContains("Organisation Name", Helper.TestOrg.OH_FullName, notificationEmail.Body);
			AssertContains("Contact Name", Helper.TestContact.OC_ContactName, notificationEmail.Body);
			AssertContains("Email Subject", expectedEmailSubject, notificationEmail.Body);
			AssertContains("HyperLink", ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, testBizO.PK), notificationEmail.Body);

			string footer = String.Format(@"This email was sent to the email group defined at
System Registry: {0}", expectedLocation);

			AssertContains("Footer is in Group notofication format", footer, notificationEmail.FooterText);

			foreach (string recipient in expectedRecipients)
			{
				AssertCollectionContains("Recipients contain recipient", recipient, notificationEmail.Recipients.ToStringCollection());
			}
			AssertEquals("Notification Email number of recipients", expectedRecipients.Count, notificationEmail.Recipients.Count);

			foreach (string recipient in expectedRecipients)
			{
				bool found = false;
				for (int i = 0; i < notificationEmail.Recipients.Count; i++)
				{
					if (notificationEmail.Recipients[i].Equals(recipient))
					{
						found = true;
						break;
					}
				}
				Assert(String.Format("Recipient {0} not found", recipient), found);
			}
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public ZWebTestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new ZWebTestHelper(Factory);
					Factory.Save();
					fHelper.TestContact.OC_ContactName = "Homer Simpson";
					fHelper.TestOrg.OH_FullName = "Springfield Nuclear Power Plant";
					fHelper.TestSiteUser.Login(fHelper.TestOrg.OH_Code, fHelper.TestContact.OC_Email, fHelper.TestContact.PasswordForTesting);
				}
				return fHelper;
			}
		}
		ZWebTestHelper fHelper;

		#endregion
	}
}
