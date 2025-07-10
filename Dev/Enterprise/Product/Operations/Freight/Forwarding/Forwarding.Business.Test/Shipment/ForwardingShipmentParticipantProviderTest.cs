using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentParticipantProviderTest : TestCaseWithFactory
	{
		public void TestGetBookingAdditionalParticipants_ALL()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, true, false, true);
		}

		public void TestGetBookingAdditionalParticipants_GRP()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.GRP, true, false, true);
		}

		public void TestGetBookingAdditionalParticipants_ROL()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, true, false, true);
		}

		public void TestGetBookingAdditionalParticipants_NON()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.NON, true, false, false);
		}

		public void TestGetBookingAdditionalParticipants_ALL_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, true, false, false);
		}

		public void TestGetBookingAdditionalParticipants_GRP_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.GRP, true, false, false);
		}

		public void TestGetBookingAdditionalParticipants_ROL_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, true, false, false);
		}

		public void TestGetShipmentAdditionalParticipants_ALL()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, false, false, true);
		}

		public void TestGetShipmentAdditionalParticipants_GRP()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.GRP, false, false, true);
		}

		public void TestGetShipmentAdditionalParticipants_ROL()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, false, false, true);
		}

		public void TestGetShipmentAdditionalParticipants_NON()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.NON, false, false, false);
		}

		public void TestGetShipmentAdditionalParticipants_ALL_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, false, false, false);
		}

		public void TestGetShipmentAdditionalParticipants_GRP_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.GRP, false, false, false);
		}

		public void TestGetShipmentAdditionalParticipants_ROL_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, false, false, false);
		}

		public void TestGetBookingAdditionalParticipants_ALL_Import()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, true, true, true);
		}

		public void TestGetBookingAdditionalParticipants_ROL_Import()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, true, true, true);
		}

		public void TestGetBookingAdditionalParticipants_ALL_Import_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, true, true, false);
		}

		public void TestGetBookingAdditionalParticipants_ROL_Import_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, true, true, false);
		}

		public void TestGetShipmentAdditionalParticipants_ALL_Import()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, false, true, true);
		}

		public void TestGetShipmentAdditionalParticipants_ROL_Import()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, false, true, true);
		}

		public void TestGetShipmentAdditionalParticipants_ALL_Import_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, false, true, false);
		}

		public void TestGetShipmentAdditionalParticipants_ROL_Import_NoControllingBranch()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ROL, false, true, false);
		}

		public void TestGetShipmentAdditionalParticipants_ALL_Import_NoControllingBranch_ParentOrg()
		{
			AssertAdditionalParticipants(EmailNotificationSendingRules.ALL, false, true, false, true);
		}

		void AssertAdditionalParticipants(string rule, bool isBooking, bool isImport, bool assignControllingBranch, bool assignContactOrg = false)
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var fallbackGroup = Factory.NewWithValidTestData<GlbGroup>();
			var fallbackRoles = StaffRolesNotificationHelper.GetRoles();
			fallbackRoles[0].Bool = true;
			var roles = StaffRolesNotificationHelper.GetRoles();
			roles[1].Bool = true;
			var optionRegistryItem = isBooking ? WebDataRegistry.Instance.BookingNotificationOptions : WebDataRegistry.Instance.ShipmentNotificationOptions;
			var groupRegistryItem = isBooking ? WebDataRegistry.Instance.BookingNotificationEmailGroup : WebDataRegistry.Instance.ShipmentNotificationEmailGroup;
			var roleRegistryItem = isBooking ? WebDataRegistry.Instance.BookingsNotificationStaffRoles : WebDataRegistry.Instance.ShipmentNotificationStaffRoles;

			var contact = GetContact();
			var controllingBranch = Factory.NewWithValidTestData<GlbBranch>();

			using (optionRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rule))
			using (groupRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackGroup.PK.ToGuid()))
			using (groupRegistryItem.SetTemporaryValue(Guid.Empty, controllingBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid()))
			using (roleRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackRoles))
			using (roleRegistryItem.SetTemporaryValue(Guid.Empty, controllingBranch.PK.ToGuid(), Guid.Empty, roles))
			{
				var (shipment, relatedOrg) = SetUpTestData(controllingBranch, isBooking, isImport, Core.Constants.TransportModes.Air);
				if (assignControllingBranch)
				{
					if (isImport)
					{
						shipment.ConsigneeDeliveryAddress.Organisation.CompanyData.OB_GB_ControllingBranch = controllingBranch.PK;
					}
					else
					{
						shipment.ConsignorPickupAddress.Organisation.CompanyData.OB_GB_ControllingBranch = controllingBranch.PK;
					}
				}

				if (assignContactOrg)
				{
					contact.Header.CompanyData.OB_GB_ControllingBranch = controllingBranch.PK;
					shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
					shipment.ConsignorPickupAddress.E2_AddressOverride = true;
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

				OrgStaffAssignmentsCollection staffAssignments;

				if (assignControllingBranch)
				{
					staffAssignments = relatedOrg.GetStaffAssignmentsForGlbCompany(controllingBranch.Company);
				}
				else if (assignContactOrg)
				{
					staffAssignments = contact.Header.GetStaffAssignmentsForGlbCompany(controllingBranch.Company);
				}
				else
				{
					staffAssignments = relatedOrg.StaffAssignments;
				}

				staffAssignments.SetStaffAssignment(roleRegistryItem.Value[0].Code, fallbackRolStaff.GS_Code, isImport ? OrgStaffAssignmentsCollection.Direction.Import : OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);
				staffAssignments.SetStaffAssignment(roleRegistryItem.Value[1].Code, rolStaff.GS_Code, isImport ? OrgStaffAssignmentsCollection.Direction.Import : OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				Factory.Save();

				var participant = GetContactParticipant(contact);

				var result = provider.GetAdditionalParticipants(shipment, participant);

				if (rule == EmailNotificationSendingRules.ALL)
				{
					AssertSequencesEqual(assignControllingBranch || assignContactOrg ? new[] { grpStaff, rolStaff } : new[] { grpFallbackStaff, fallbackRolStaff }, result);
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

		public void TestGetBookingAdditionalParticipants_ROL_AllFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			using (WebDataRegistry.Instance.BookingNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL))
			using (WebDataRegistry.Instance.BookingNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.BookingsNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				var (shipment, relatedOrg) = SetUpTestData(contact.BranchForLogin, true, false, Core.Constants.TransportModes.Air);

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "grp@test.com";

				var rolStaffWithoutEmail = Factory.NewWithValidTestData<GlbStaff>();
				relatedOrg.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, rolStaffWithoutEmail.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(OrgStaffAssignmentsLookups.AllServices, rolStaff.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(shipment, participant);

				AssertSequencesEqual(new[] { rolStaff }, result);
			}
		}

		public void TestGetBookingAdditionalParticipants_ROL_GRPFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			using (WebDataRegistry.Instance.BookingNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL))
			using (WebDataRegistry.Instance.BookingNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.BookingsNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				var (shipment, relatedOrg) = SetUpTestData(contact.BranchForLogin, true, false, Core.Constants.TransportModes.Air);

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "test@test.com";

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(shipment, participant);

				AssertSequencesEqual(new[] { grpStaff }, result);
			}
		}

		public void TestGetBookingAdditionalParticipants_GRP_ROLFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			using (WebDataRegistry.Instance.BookingNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.GRP))
			using (WebDataRegistry.Instance.BookingNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.BookingsNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				var (shipment, relatedOrg) = SetUpTestData(contact.BranchForLogin, true, false, Core.Constants.TransportModes.Air);

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, rolStaff.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(shipment, participant);

				AssertSequencesEqual(new[] { rolStaff }, result);
			}
		}

		public void TestGetShipmentAdditionalParticipants_ROL_AllFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			using (WebDataRegistry.Instance.ShipmentNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL))
			using (WebDataRegistry.Instance.ShipmentNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.ShipmentNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				var (shipment, relatedOrg) = SetUpTestData(contact.BranchForLogin, false, false, Core.Constants.TransportModes.Air);

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "grp@test.com";

				var rolStaffWithoutEmail = Factory.NewWithValidTestData<GlbStaff>();
				relatedOrg.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.BookingsNotificationStaffRoles.Value[0].Code, rolStaffWithoutEmail.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(OrgStaffAssignmentsLookups.AllServices, rolStaff.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(shipment, participant);

				AssertSequencesEqual(new[] { rolStaff }, result);
			}
		}

		public void TestGetShipmentAdditionalParticipants_ROL_GRPFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			using (WebDataRegistry.Instance.ShipmentNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ROL))
			using (WebDataRegistry.Instance.ShipmentNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.ShipmentNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				var (shipment, relatedOrg) = SetUpTestData(contact.BranchForLogin, false, false, Core.Constants.TransportModes.Air);

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "test@test.com";

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(shipment, participant);

				AssertSequencesEqual(new[] { grpStaff }, result);
			}
		}

		public void TestGetShipmentAdditionalParticipants_GRP_ROLFallback()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			using (WebDataRegistry.Instance.ShipmentNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.GRP))
			using (WebDataRegistry.Instance.ShipmentNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.ShipmentNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				var (shipment, relatedOrg) = SetUpTestData(contact.BranchForLogin, false, false, Core.Constants.TransportModes.Air);

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.ShipmentNotificationStaffRoles.Value[0].Code, rolStaff.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(shipment, participant);

				AssertSequencesEqual(new[] { rolStaff }, result);
			}
		}

		public void TestConvertedBookingAdditionalParticipants_UsesShipment()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			using (WebDataRegistry.Instance.ShipmentNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.GRP))
			using (WebDataRegistry.Instance.ShipmentNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.ShipmentNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var contact = GetContact();
				var (shipment, relatedOrg) = SetUpTestData(contact.BranchForLogin, true, false, Core.Constants.TransportModes.Air);
				shipment.JS_IsForwardRegistered = true;
				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(WebDataRegistry.Instance.ShipmentNotificationStaffRoles.Value[0].Code, rolStaff.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				var participant = GetContactParticipant(contact);
				var result = provider.GetAdditionalParticipants(shipment, participant);

				AssertSequencesEqual(new[] { rolStaff }, result);
			}
		}

		public void TestStaffSender_NoAdditionalParticipants()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			participant.JCP_ParticipantTableCode = staff.TablePrefix;
			participant.JCP_ParticipantID = staff.PK;
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var result = provider.GetAdditionalParticipants(shipment, participant);

			AssertSequencesEqual(Enumerable.Empty<IConversationParticipant>(), result);
		}

		public void TestNoSender_SystemMessage()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			using (WebDataRegistry.Instance.ShipmentNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ALL))
			using (WebDataRegistry.Instance.ShipmentNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.ShipmentNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var (shipment, relatedOrg) = SetUpTestData(GlbBranch.CurrentBranch, false, false, Core.Constants.TransportModes.Air);

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "grp@test.com";

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(OrgStaffAssignmentsLookups.AllServices, rolStaff.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				var result = provider.GetAdditionalParticipants(shipment, null);

				AssertSequencesEqual(new[] { grpStaff, rolStaff }, result);
			}
		}

		public void TestNoSender_SystemMessage_NoRelatedOrg()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			using (WebDataRegistry.Instance.ShipmentNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationSendingRules.ALL))
			using (WebDataRegistry.Instance.ShipmentNotificationEmailGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (WebDataRegistry.Instance.ShipmentNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles))
			{
				var (shipment, relatedOrg) = SetUpTestData(GlbBranch.CurrentBranch, false, false, Core.Constants.TransportModes.Air);

				shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
				shipment.ConsignorPickupAddress.E2_AddressOverride = true;

				var grpStaff = group.Staff.AddNew();
				grpStaff.GS_EmailAddress = "grp@test.com";

				var rolStaff = Factory.NewWithValidTestData<GlbStaff>();
				rolStaff.GS_EmailAddress = "rol@test.com";
				relatedOrg.StaffAssignments.SetStaffAssignment(OrgStaffAssignmentsLookups.AllServices, rolStaff.GS_Code, OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Air);

				var result = provider.GetAdditionalParticipants(shipment, null);

				AssertSequencesEqual(new[] { grpStaff }, result);
			}
		}

		(ForwardingShipment shipment, OrgHeader relatedOrg) SetUpTestData(GlbBranch homeBranch, bool isBooking, bool isImport, string transportMode)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_IsBooking = isBooking;
			shipment.JS_IsForwardRegistered = !isBooking;
			shipment.JS_TransportMode = transportMode;
			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();

			if (isImport)
			{
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = homeBranch.GB_RL_NKHomePort;
				shipment.ConsigneeDeliveryAddress.E2_OA_Address = relatedOrg.MainAddress.PK;
			}
			else
			{
				shipment.JS_RL_NKOrigin = homeBranch.GB_RL_NKHomePort;
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.ConsignorPickupAddress.E2_OA_Address = relatedOrg.MainAddress.PK;
			}

			return (shipment, relatedOrg);
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

			provider = new ForwardingShipmentParticipantProvider();
		}
		ForwardingShipmentParticipantProvider provider;
	}
}
