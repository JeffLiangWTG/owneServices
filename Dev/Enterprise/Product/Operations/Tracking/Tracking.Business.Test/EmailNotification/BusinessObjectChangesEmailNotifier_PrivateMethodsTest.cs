using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using BizOStatus = Enterprise.Tracking.Business.BusinessObjectChangesEmailNotifier.BizOStatus;

namespace Enterprise.Tracking.Business
{
	sealed class BusinessObjectChangesEmailNotifier_PrivateMethodsTest : TestCaseWithFactory
	{
		public void TestEmailNotificationUsesCorrectCompany()
		{
			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "RUMOW"));
			GlbCompany testCompany = Factory.New<GlbCompany>();
			testCompany.GC_RN_NKCountryCode = uNLOCO.RL_RN_NKCountryCode;
			GlbBranch testBranch = testCompany.Branches.AddNew();
			testBranch.GB_RL_NKHomePort = uNLOCO.RL_Code;

			CodeDescriptionBoolDisallowNewCollection roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			WebDataRegistry.Instance.BookingsNotificationStaffRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			DummyBizOWithNotifier bizO = Factory.New<DummyBizOWithNotifier>();

			OrgHeader org = OrgHeader.New(Factory);
			org.OH_Code = "SPRNUCSYD";
			org.OH_FullName = "Springfield Nuclear Power Plant";
			bizO.RelatedOrgDummy = org;
			bizO.StaffRolesToNotifyDummy = WebDataRegistry.Instance.BookingsNotificationStaffRoles;

			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ABC";
			staff1.GS_EmailAddress = "staff1@edi.com.au";
			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "DEF";
			staff2.GS_EmailAddress = "staff2@edi.com.au";

			org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, staff1.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Sea);
			org.GetStaffAssignmentsForGlbCompany(testCompany).SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, staff2.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Sea);

			EmailDef email = new EmailDef();
			bizO.Notifier.AddEmailRecipientsByStaffRoles(email);
			AssertEquals("Company wasn't specified -- should use current Company", "staff1@edi.com.au", email.Recipients[0]);

			bizO.EventBranchSetterForTest = testBranch;

			email = new EmailDef();
			bizO.Notifier.AddEmailRecipientsByStaffRoles(email);
			AssertEquals("Should use specified Company", "staff2@edi.com.au", email.Recipients[0]);
		}

		public void TestAddEmailRecipientsByStaffRoles()
		{
			CodeDescriptionBoolDisallowNewCollection roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);

			WebDataRegistry.Instance.BookingsNotificationStaffRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			DummyBizOWithNotifier bizO = Factory.New<DummyBizOWithNotifier>();

			EmailDef email = new EmailDef();

			bizO.Notifier.AddEmailRecipientsByStaffRoles(email);
			AssertEquals("No EmailAddressesToNotify without Buyer and without EmailGroupRegistryItem assigned", 0, email.Recipients.Count);

			RefUNLOCO lOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "RUMOW"));
			GlbCompany testCompany = Factory.New<GlbCompany>();
			testCompany.GC_RN_NKCountryCode = lOCO.RL_RN_NKCountryCode;
			GlbBranch testBranch = testCompany.Branches.AddNew();
			testBranch.GB_RL_NKHomePort = lOCO.RL_Code;

			bizO.EventBranchSetterForTest = null;
			email = new EmailDef();

			OrgHeader org = OrgHeader.New(Factory);
			org.OH_Code = "SPRNUCSYD";
			org.OH_FullName = "Springfield Nuclear Power Plant";
			bizO.RelatedOrgDummy = org;
			bizO.StaffRolesToNotifyDummy = WebDataRegistry.Instance.BookingsNotificationStaffRoles;

			bizO.Notifier.AddEmailRecipientsByStaffRoles(email);
			AssertEquals("No EmailAddressesToNotify without StaffAssignments", 0, email.Recipients.Count);

			GlbStaff newStaff1 = Factory.New<GlbStaff>();
			newStaff1.GS_Code = "ABC";
			Assert("No EmailAddress", newStaff1.GS_EmailAddress.IsEmpty);

			org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, newStaff1.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Sea);
			bizO.Notifier.AddEmailRecipientsByStaffRoles(email);
			AssertEquals("No EmailAddressesToNotify because no EmailAddress specified", 0, email.Recipients.Count);

			GlbStaff newStaff2 = Factory.New<GlbStaff>();
			newStaff2.GS_Code = "DEF";
			Assert("No EmailAddress", newStaff2.GS_EmailAddress.IsEmpty);

			org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, newStaff2.GS_Code, OrgStaffAssignmentsLookups.AllServices);
			bizO.Notifier.AddEmailRecipientsByStaffRoles(email);
			AssertEquals("No EmailAddressesToNotify because no EmailAddress specified for both Staffs", 0, email.Recipients.Count);

			newStaff2.GS_EmailAddress = "staff2@edi.com.au";
			email = new EmailDef();
			bizO.Notifier.AddEmailFooter(bizO.Notifier.AddEmailRecipientsByStaffRoles(email), 0, email);

			string footer = String.Format(@"This email was sent to an assigned staff for {0} {1} to those who have next roles:
Account Manager.
You can change this in the setting located at
System Registry: Web and Visibility -> Forwarding -> Bookings -> Notification Staff Roles -> System.", org.OH_Code, org.OH_FullName);

			AssertContains("Footer is in StaffRoles format with parent org used as relative org", footer, email.FooterText);
			AssertEquals("Should be one EmailAddress", 1, email.Recipients.Count);
			AssertEquals("Second Staff's EmailAddress", "staff2@edi.com.au", email.Recipients[0]);

			newStaff1.GS_EmailAddress = "staff1@edi.com.au";
			email = new EmailDef();
			bizO.Notifier.AddEmailRecipientsByStaffRoles(email);
			AssertEquals("Should be one EmailAddress", 1, email.Recipients.Count);
			AssertEquals("First Staff's EmailAddress, because it is more specific than the second one", "staff1@edi.com.au", email.Recipients[0]);

			GlbStaff newStaff3 = Factory.New<GlbStaff>();
			newStaff3.GS_Code = "GHE";
			newStaff3.GS_EmailAddress = "staff3@edi.com.au";

			org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[2].Code, newStaff3.GS_Code, OrgStaffAssignmentsLookups.AllServices);
			email = new EmailDef();
			bizO.Notifier.AddEmailRecipientsByStaffRoles(email);
			AssertEquals("Should be two EmailAddresses", 2, email.Recipients.Count);
			AssertEquals("First Staff's EmailAddress", "staff1@edi.com.au", email.Recipients[0]);
			AssertEquals("Third Staff's EmailAddress", "staff3@edi.com.au", email.Recipients[1]);
		}

		public void TestGetEmailStatus()
		{
			DummyBizOWithNotifier bizO = DummyBizOWithNotifier.New(Factory, null, null, null);

			AssertEquals("not in database", false, bizO.IsInDatabase);
			AssertEquals("HasChanges", false, bizO.HasChanges);
			AssertEquals("not changed initially", BizOStatus.NotChanged, bizO.Notifier.GetEmailStatus());

			bizO.Z0_Description = "test";
			AssertEquals("not in database", false, bizO.IsInDatabase);
			AssertEquals("HasChanges", true, bizO.HasChanges);
			AssertEquals("Not in DB, but has changes", BizOStatus.Created, bizO.Notifier.GetEmailStatus());

			Factory.Save();
			AssertEquals("in database", true, bizO.IsInDatabase);
			AssertEquals("HasChanges", false, bizO.HasChanges);

			bizO.UserEditableNoteHelper.EditableNoteText = "Note";
			AssertEquals("HasChanges", true, bizO.HasChanges);
			AssertEquals("modified and User Note added", BizOStatus.Modified | BizOStatus.UserNoteAdded, bizO.Notifier.GetEmailStatus());

			Factory.Save();
			AssertEquals("HasChanges", false, bizO.HasChanges);
			bizO.UserEditableNoteHelper.EditableNoteText = "Note Changed";
			AssertEquals("HasChanges", true, bizO.HasChanges);
			AssertEquals("modiifed and User Note changed", BizOStatus.Modified | BizOStatus.UserNoteChanged, bizO.Notifier.GetEmailStatus());

			Factory.Save();
			AssertEquals("not changed", BizOStatus.NotChanged, bizO.Notifier.GetEmailStatus());

			bizO.Z0_Description = "test changed";
			AssertEquals("HasChanges", true, bizO.HasChanges);
			AssertEquals("modiifed but no User Note changed", BizOStatus.Modified, bizO.Notifier.GetEmailStatus());

			Factory.Save();
			AssertEquals("HasChanges", false, bizO.HasChanges);
			bizO.IsCancelled = true;
			AssertEquals("HasChanges", true, bizO.HasChanges);
			AssertEquals("cancelled", BizOStatus.Cancelled, bizO.Notifier.GetEmailStatus());
		}

		public void TestGetRegistryItemAndBranchLocation()
		{
			DummyBizOWithNotifier bizO = DummyBizOWithNotifier.New(Factory, null, null, null);

			GlbCompany testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "TST";
			testCompany.GC_Name = "Test Company";

			GlbBranch testBranch = testCompany.Branches.AddNew();
			testBranch.GB_Code = "TST";
			testBranch.GB_BranchName = "TestBranch";

			GuidRegistryItem testItem = new GuidRegistryItem("TestRegistryItem", (NoResString)"", (NoResString)"TestRegistryItem", (NoResString)"", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);

			AssertEquals("No branch specified, should be Enterprise", "System Registry:  -> TestRegistryItem -> System", bizO.Notifier.GetRegistryItemAndBranchLocation(testItem));

			bizO.EventBranchSetterForTest = testBranch;

			AssertEquals("Should be Test Branch because branch was specified", "System Registry:  -> TestRegistryItem -> Companies -> Test Company -> Branches -> TestBranch", bizO.Notifier.GetRegistryItemAndBranchLocation(testItem));
		}

		public void TestGetStaffEmailAddress()
		{
			DummyBizOWithNotifier bizO = DummyBizOWithNotifier.New(Factory, null, null, null);

			ZString staffLanguage;
			Assert("Empty result for Empty Staff Guid", bizO.Notifier.GetStaffEmailAndLanguage(Factory, ZGuid.Empty, out staffLanguage).IsEmpty);

			GlbStaff staff = Factory.New<GlbStaff>();
			Assert("Staff EmailAddress is Empty", staff.GS_EmailAddress.IsEmpty);
			Assert("Empty result for Staff with empty EmailAddress", bizO.Notifier.GetStaffEmailAndLanguage(Factory, staff.PK, out staffLanguage).IsEmpty);

			staff.GS_EmailAddress = "test@edi.com.au";
			AssertEquals("Returns Staff EmailAddress", "test@edi.com.au", bizO.Notifier.GetStaffEmailAndLanguage(Factory, staff.PK, out staffLanguage));
		}

		public void TestGetRegistryItemValueByLocation()
		{
			DummyBizOWithNotifier bizO = DummyBizOWithNotifier.New(Factory, null, null, null);

			GlbCompany testCompany = Factory.New<GlbCompany>();
			GlbBranch testBranch = testCompany.Branches.AddNew();
			Factory.Save();

			GuidRegistryItem testItem = new GuidRegistryItem("TestRegistryItem", (NoResString)"TestCategory", (NoResString)"Test Caption", (NoResString)"Some hint", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
			Guid testSystemValueGuid = Guid.NewGuid();
			testItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testSystemValueGuid);

			AssertEquals("Global assignment", testSystemValueGuid, bizO.Notifier.GetRegistryItemValueByLocation(testItem));

			Guid testBranchValueGuid = Guid.NewGuid();
			testItem.SetValue(Guid.Empty, testBranch.PK.ToGuid(), Guid.Empty, testBranchValueGuid);

			AssertEquals("Global assignment", testSystemValueGuid, bizO.Notifier.GetRegistryItemValueByLocation(testItem));

			bizO.EventBranchSetterForTest = testBranch;

			AssertEquals("Branch assignment", testBranchValueGuid, bizO.Notifier.GetRegistryItemValueByLocation(testItem));
		}

		public void TestPrepareEmailMessage()
		{
			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "RUMOW"));
			GlbCompany testCompany = Factory.New<GlbCompany>();
			testCompany.GC_RN_NKCountryCode = uNLOCO.RL_RN_NKCountryCode;

			CodeDescriptionBoolDisallowNewCollection roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			WebDataRegistry.Instance.BookingsNotificationStaffRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@edi.com.au";
			staff1.GS_Code = "SAS";

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "TST";
			org.OH_FullName = "Test Company";
			org.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, staff1.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Sea);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John";

			DummyBizOWithNotifier bizO = DummyBizOWithNotifier.New(Factory, null, contact, null);
			bizO.RelatedOrgDummy = org;
			bizO.StaffRolesToNotifyDummy = WebDataRegistry.Instance.BookingsNotificationStaffRoles;

			AssertEquals("HasChanges", false, bizO.HasChanges);
			EmailDef email = bizO.Notifier.PrepareEmailMessage();
			AssertEquals("no changes -- no email", ZString.Empty, email.Body);

			bizO.Notifier.EmailStatus = BizOStatus.Created;
			bizO.fÑontrollerForEnterpriseSetterForTest = null;

			email = bizO.Notifier.PrepareEmailMessage();
			AssertNotNull("email should be prepared because object was created", email);
			AssertEquals("Subject", "DummyBizo 123 has been placed", email.Subject);

			Stream stream = typeof(BusinessObjectChangesEmailNotifier).Assembly.GetManifestResourceStream(PathOfExpectedfEmailBody);
			ZString expectedEmailBody = new StreamReader(stream).ReadToEnd();
			expectedEmailBody = expectedEmailBody.Replace("*WHEN*", ZDateTime.Now.ToLongTimeString());
			email.FooterText = ZString.Empty;
			AssertMultilineASCIIEquals("Message Body", expectedEmailBody, email.Body);

			bizO.Notifier.EmailStatus = BizOStatus.Modified;
			email = bizO.Notifier.PrepareEmailMessage();
			AssertNotNull("email should be prepared because object was modified", email);
			AssertEquals("Subject", "DummyBizo 123 has been modified", email.Subject);
			AssertContains("Message Body", "Changed By: Test Company (John)", email.Body);

			bizO.Notifier.EmailStatus = BizOStatus.Cancelled;
			email = bizO.Notifier.PrepareEmailMessage();
			AssertNotNull("email should be prepared because object was cancelled", email);
			AssertEquals("Subject", "DummyBizo 123 " + "has been canceled", email.Subject);
			AssertContains("Message Body", string.Format("Canceled By: {0} ({1})", "Test Company", "John"), email.Body);
		}

		#region TestHTMLFormatting

		public void TestHTMLFormatting()
		{
			DummyBizOWithNotifier bizO = DummyBizOWithNotifier.New(Factory, null, null, null);

			PropertyChangeInfo[] propInfos = bizO.GetPropertiesForEmailReporting();
			AssertContains("Precondition: Original value", "<>", propInfos[2].OriginalValue);
			AssertContains("Precondition: Updated value", "<>", propInfos[2].UpdatedValue);

			ZString html = bizO.Notifier.GetHtmlTableForShortFormatProperties();

			AssertNotContains("ShortFormatProperties: HTML should not contain non encoded symbols", "<>", html);
			AssertContains("ShortFormatProperties: HTML should be encoded", "&lt;&gt;", html);

			bizO.UserEditableNoteHelper.EditableNoteText = "5 <> 4";
			html = bizO.Notifier.GetHtmlTableForWebEditableNote();

			AssertNotContains("WebEditableNote: HTML should not contain non encoded symbols", "<>", html);
			AssertContains("WebEditableNote: HTML should be encoded", "&lt;&gt;", html);
		}

		#endregion

		const string PathOfExpectedfEmailBody = "Enterprise.Tracking.Business.EmailNotification.Test.ExpectedEmailBody.htm";
	}
}
