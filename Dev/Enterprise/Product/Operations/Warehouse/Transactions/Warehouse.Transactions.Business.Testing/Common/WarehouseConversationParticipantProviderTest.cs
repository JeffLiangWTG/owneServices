using System;
using System.Linq;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WarehouseConversationParticipantProviderTest : WhsTestCaseWithFactory
	{
		public void TestGetOrderAdditionalParticipants_ALL()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, order, true);
		}

		public void TestGetOrderAdditionalParticipants_GRP()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.GRP, order, true);
		}

		public void TestGetOrderAdditionalParticipants_ROL()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, order, true);
		}

		public void TestGetOrderAdditionalParticipants_NON()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.NON, order, false);
		}

		public void TestGetOrderAdditionalParticipants_ALL_NoControllingBranch()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, order, false);
		}

		public void TestGetOrderAdditionalParticipants_GRP_NoControllingBranch()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.GRP, order, false);
		}

		public void TestGetOrderAdditionalParticipants_ROL_NoControllingBranch()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, order, false);
		}

		public void TestGetReceiveAdditionalParticipants_ALL()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, receive, true);
		}

		public void TestGetReceiveAdditionalParticipants_GRP()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.GRP, receive, true);
		}

		public void TestGetReceiveAdditionalParticipants_ROL()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, receive, true);
		}

		public void TestGetReceiveAdditionalParticipants_NON()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.NON, receive, false);
		}

		public void TestGetReceiveAdditionalParticipants_ALL_NoControllingBranch()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, receive, false);
		}

		public void TestGetReceiveAdditionalParticipants_GRP_NoControllingBranch()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.GRP, receive, false);
		}

		public void TestGetReceiveAdditionalParticipants_ROL_NoControllingBranch()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, receive, false);
		}

		void AssertAdditionalParticipants(string rule, WhsDocket docket, bool assignControllingBranch)
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var fallbackGroup = Factory.NewWithValidTestData<GlbGroup>();
			var fallbackRoles = StaffRolesNotificationHelper.GetRoles();
			fallbackRoles[0].Bool = true;
			var roles = StaffRolesNotificationHelper.GetRoles();
			roles[1].Bool = true;
			var optionRegistryItem = docket.WD_DocketType == DocketType.Codes.Order ? WebDataRegistry.Instance.WarehouseOrdersNotificationOptions : WebDataRegistry.Instance.WarehouseReceiptsNotificationOptions;
			var groupRegistryItem = docket.WD_DocketType == DocketType.Codes.Order ? WebDataRegistry.Instance.WarehouseOrdersNotificationEmailGroup : WebDataRegistry.Instance.WarehouseReceiptsNotificationEmailGroup;
			var roleRegistryItem = docket.WD_DocketType == DocketType.Codes.Order ? WebDataRegistry.Instance.WarehouseOrdersNotificationStaffRoles : WebDataRegistry.Instance.WarehouseReceiptsNotificationStaffRoles;

			var contact = docket.Client.Contacts.AddNew();
			var controllingBranch = Factory.NewWithValidTestData<GlbBranch>();

			using (optionRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rule))
			using (groupRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackGroup.PK.ToGuid()))
			using (groupRegistryItem.SetTemporaryValue(Guid.Empty, controllingBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid()))
			using (roleRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackRoles))
			using (roleRegistryItem.SetTemporaryValue(Guid.Empty, controllingBranch.PK.ToGuid(), Guid.Empty, roles))
			{
				var relatedOrg = docket.Client;
				if (assignControllingBranch)
				{
					relatedOrg.CompanyData.OB_GB_ControllingBranch = controllingBranch.PK;
				}

				var grpFallbackStaff = fallbackGroup.Staff.AddNew();
				grpFallbackStaff.GS_LoginName = "fallback-grp";
				grpFallbackStaff.GS_EmailAddress = "fallback-grp@test.com";

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_LoginName = "grp";
				grpStaff.GS_EmailAddress = "grp@test.com";

				var fallbackRolStaff = Factory.NewWithValidTestData<GlbStaff>();
				fallbackRolStaff.GS_EmailAddress = "fallback-rol@test.com";
				fallbackRolStaff.GS_GB_HomeBranch = controllingBranch.PK;

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				rolStaff.GS_GB_HomeBranch = controllingBranch.PK;

				var staffAssignments = assignControllingBranch
					? relatedOrg.GetStaffAssignmentsForGlbCompany(controllingBranch.Company)
					: relatedOrg.StaffAssignments;
				staffAssignments.SetStaffAssignment(roleRegistryItem.Value[0].Code, fallbackRolStaff.GS_Code, OrgStaffAssignmentsLookups.WarehouseServices);
				staffAssignments.SetStaffAssignment(roleRegistryItem.Value[1].Code, rolStaff.GS_Code, OrgStaffAssignmentsLookups.WarehouseServices);

				Factory.Save();

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(docket, participant);

				if (rule == EmailNotificationSendingRules.ALL)
				{
					AssertSequencesEqual(assignControllingBranch ? new[] { grpStaff, rolStaff } : new[] { grpFallbackStaff, fallbackRolStaff }, result);
				}
				else if (rule == EmailNotificationSendingRules.GRP)
				{
					AssertSequencesEqual(new[] { assignControllingBranch ? grpStaff : grpFallbackStaff }, result);
				}
				else if (rule == EmailNotificationSendingRules.ROL)
				{
					AssertSequencesEqual(new[] { assignControllingBranch ? rolStaff : fallbackRolStaff }, result);
				}
				else
				{
					AssertSequencesEqual(Enumerable.Empty<IConversationParticipant>(), result);
				}
			}
		}

		public void TestGetOrderAdditionalParticipants_ROL_AllFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			var order = Factory.NewWithValidTestData<WhsOrder>();
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL))
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				order.WD_OH_Client = contact.OC_OH;
				var relatedOrg = order.Client;

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "grp@test.com";

				var rolStaffWithoutEmail = Factory.NewWithValidTestData<GlbStaff>();
				relatedOrg.StaffAssignments.SetStaffAssignment(roles[0].Code, rolStaffWithoutEmail.GS_Code, OrgStaffAssignmentsLookups.WarehouseServices);

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(roles[0].Code, rolStaff.GS_Code, OrgStaffAssignmentsLookups.WarehouseServices);

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(order, participant);

				AssertSequencesEqual(new[] { rolStaff }, result);
			}
		}

		public void TestGetOrderAdditionalParticipants_ROL_GRPFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			var order = Factory.NewWithValidTestData<WhsOrder>();
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL))
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				order.WD_OH_Client = contact.OC_OH;
				var relatedOrg = order.Client;

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "test@test.com";

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(order, participant);

				AssertSequencesEqual(new[] { grpStaff }, result);
			}
		}

		public void TestGetOrderAdditionalParticipants_GRP_ROLFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			var order = Factory.NewWithValidTestData<WhsOrder>();
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.GRP))
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				order.WD_OH_Client = contact.OC_OH;
				var relatedOrg = order.Client;

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(roles[0].Code, rolStaff.GS_Code, OrgStaffAssignmentsLookups.WarehouseServices);

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(order, participant);

				AssertSequencesEqual(new[] { rolStaff }, result);
			}
		}

		public void TestGetReceiveAdditionalParticipants_ROL_AllFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			using (WebDataRegistry.Instance.WarehouseReceiptsNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL))
			using (WebDataRegistry.Instance.WarehouseReceiptsNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.WarehouseReceiptsNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				receive.WD_OH_Client = contact.OC_OH;
				var relatedOrg = receive.Client;

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "grp@test.com";

				var rolStaffWithoutEmail = Factory.NewWithValidTestData<GlbStaff>();
				relatedOrg.StaffAssignments.SetStaffAssignment(roles[0].Code, rolStaffWithoutEmail.GS_Code, OrgStaffAssignmentsLookups.WarehouseServices);

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(roles[0].Code, rolStaff.GS_Code, OrgStaffAssignmentsLookups.WarehouseServices);
				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(receive, participant);

				AssertSequencesEqual(new[] { rolStaff }, result);
			}
		}

		public void TestGetReceiveAdditionalParticipants_ROL_GRPFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			using (WebDataRegistry.Instance.WarehouseReceiptsNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL))
			using (WebDataRegistry.Instance.WarehouseReceiptsNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.WarehouseReceiptsNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				receive.WD_OH_Client = contact.OC_OH;
				var relatedOrg = receive.Client;

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "test@test.com";

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(receive, participant);

				AssertSequencesEqual(new[] { grpStaff }, result);
			}
		}

		public void TestGetReceiveAdditionalParticipants_GRP_ROLFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			using (WebDataRegistry.Instance.WarehouseReceiptsNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.GRP))
			using (WebDataRegistry.Instance.WarehouseReceiptsNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.WarehouseReceiptsNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				receive.WD_OH_Client = contact.OC_OH;
				var relatedOrg = receive.Client;

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(roles[0].Code, rolStaff.GS_Code, OrgStaffAssignmentsLookups.WarehouseServices);

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(receive, participant);

				AssertSequencesEqual(new[] { rolStaff }, result);
			}
		}

		public void TestStaffSender_NoAdditionalParticipants()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			participant.JCP_ParticipantTableCode = staff.TablePrefix;
			participant.JCP_ParticipantID = staff.PK;
			var order = Factory.NewWithValidTestData<WhsOrder>();

			var result = provider.GetAdditionalParticipants(order, participant);

			AssertSequencesEqual(Enumerable.Empty<IConversationParticipant>(), result);
		}

		public void TestNoSender_SystemMessage()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			var order = Factory.NewWithValidTestData<WhsOrder>();
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ALL))
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.WarehouseOrdersNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				order.WD_OH_Client = contact.OC_OH;
				var relatedOrg = order.Client;

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "grp@test.com";

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(roles[0].Code, rolStaff.GS_Code, OrgStaffAssignmentsLookups.WarehouseServices);

				var result = provider.GetAdditionalParticipants(order, null);

				AssertSequencesEqual(new[] { grpStaff, rolStaff }, result);
			}
		}

		OrgContact GetContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.BranchForLogin.GB_RL_NKHomePort = "AUSYD";

			return contact;
		}

		JobConversationParticipant GetContactParticipant(OrgContact contact)
		{
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			participant.JCP_ParticipantTableCode = contact.TablePrefix;
			participant.JCP_ParticipantID = contact.PK;

			return participant;
		}

		protected override void SetUp()
		{
			base.SetUp();

			provider = new WarehouseConversationParticipantProvider();
		}
		IWarehouseConversationParticipantProvider provider;
	}
}
