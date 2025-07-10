using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentParticipantProvider : IForwardingShipmentParticipantProvider
	{
		public IEnumerable<IConversationParticipant> GetAdditionalParticipants(ForwardingShipment shipment, JobConversationParticipant sender)
		{
			if (shipment == null || sender?.Parent is GlbStaff)
			{
				return Enumerable.Empty<IConversationParticipant>();
			}

			var contact = sender?.Parent as OrgContact;
			var isBooking = shipment.JS_IsBooking && !shipment.JS_IsForwardRegistered;
			var homeBranch = contact?.BranchForLogin ?? GlbBranch.CurrentBranch;
			var isImport = CalculateIsImport(shipment, homeBranch);
			var relatedOrg = GetRelatedOrg(shipment, isImport) ?? contact?.ParentOrg;
			var controllingBranch = GetRelatedBranch(relatedOrg) ?? homeBranch;

			var optionRegistryItem = isBooking ? WebDataRegistry.Instance.BookingNotificationOptions : WebDataRegistry.Instance.ShipmentNotificationOptions;
			var groupRegistryItem = isBooking ? WebDataRegistry.Instance.BookingNotificationEmailGroup : WebDataRegistry.Instance.ShipmentNotificationEmailGroup;
			var groupRegistryValue = groupRegistryItem.GetFallBackValueAtAllLevels(
				controllingBranch?.GB_GC.ToGuid() ?? Guid.Empty,
				controllingBranch?.PK.ToGuid() ?? Guid.Empty,
				Guid.Empty);
			var roleRegistryItem = isBooking ? WebDataRegistry.Instance.BookingsNotificationStaffRoles : WebDataRegistry.Instance.ShipmentNotificationStaffRoles;
			var roleRegistryValue = roleRegistryItem.GetFallBackValueAtAllLevels(
				controllingBranch?.GB_GC.ToGuid() ?? Guid.Empty,
				controllingBranch?.PK.ToGuid() ?? Guid.Empty,
				Guid.Empty);

			var factory = shipment.Factory;
			var result = new List<IConversationParticipant>();

			switch (optionRegistryItem.Value)
			{
				case EmailNotificationSendingRules.ALL:
					result.AddRange(GetStaffParticipantsFromGroupRegistryItem(factory, groupRegistryValue));
					result.AddRange(GetStaffParticipantsFromRoleRegistryItem(shipment, relatedOrg, isImport, roleRegistryValue));

					break;
				case EmailNotificationSendingRules.GRP:
					result.AddRange(GetStaffParticipantsFromGroupRegistryItem(factory, groupRegistryValue));
					if (result.Count == 0)
					{
						result.AddRange(GetStaffParticipantsFromRoleRegistryItem(shipment, relatedOrg, isImport, roleRegistryValue));
					}

					break;
				case EmailNotificationSendingRules.ROL:
					result.AddRange(GetStaffParticipantsFromRoleRegistryItem(shipment, relatedOrg, isImport, roleRegistryValue));
					if (result.Count == 0)
					{
						result.AddRange(GetStaffParticipantsFromGroupRegistryItem(factory, groupRegistryValue));
					}

					break;
				case EmailNotificationSendingRules.NON:
				default:
					break;
			}

			return result;
		}

		IEnumerable<IConversationParticipant> GetStaffParticipantsFromGroupRegistryItem(BusinessObjectFactory factory, Guid groupPk)
		{
			var group = factory.Load<GlbGroup>(groupPk);
			return group.Staff.Cast<GlbStaff>().Where(x => !string.IsNullOrEmpty(x.GS_EmailAddress));
		}

		IEnumerable<IConversationParticipant> GetStaffParticipantsFromRoleRegistryItem(ForwardingShipment shipment, OrgHeader relatedOrg, bool isImport, CodeDescriptionBoolCollection roles)
		{
			if (relatedOrg == null)
			{
				return Enumerable.Empty<IConversationParticipant>();
			}

			var mode = GetMode(shipment);
			var direction = isImport ? OrgStaffAssignmentsCollection.Direction.Import : OrgStaffAssignmentsCollection.Direction.Export;

			var selectedRoles = roles.Cast<CodeDescriptionBool>().Where(p => p.Bool).Select(x => x.Code);
			var assignments = GetStaffAssignments(relatedOrg);

			var results = new List<GlbStaff>();

			foreach (var role in selectedRoles)
			{
				LoadStaffAssignedForRole(role);
			}

			if (results.Count == 0)
			{
				LoadStaffAssignedForRole(OrgStaffAssignmentsLookups.AllServices);
			}

			return results;

			void LoadStaffAssignedForRole(ZString role)
			{
				var staffNk = assignments.GetStaffAssignment(role, direction, mode);
				var staff = shipment.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNk);
				if (staff != null && !string.IsNullOrEmpty(staff.GS_EmailAddress))
				{
					results.Add(staff);
				}
			}
		}

		OrgStaffAssignmentsCollection.AirSea GetMode(ForwardingShipment shipment)
		{
			if (shipment.IsAir)
			{
				return OrgStaffAssignmentsCollection.AirSea.Air;
			}
			if (shipment.IsSea)
			{
				return OrgStaffAssignmentsCollection.AirSea.Sea;
			}
			if (shipment.IsRail)
			{
				return OrgStaffAssignmentsCollection.AirSea.Rail;
			}
			if (shipment.IsRoad)
			{
				return OrgStaffAssignmentsCollection.AirSea.Road;
			}
			if (shipment.IsCourier)
			{
				return OrgStaffAssignmentsCollection.AirSea.Post;
			}

			return OrgStaffAssignmentsCollection.AirSea.None;
		}

		bool CalculateIsImport(ForwardingShipment shipment, GlbBranch homeBranch)
		{
			return ImportExportHelper.IsImport(shipment.JS_RL_NKOrigin, shipment.JS_RL_NKDestination, homeBranch.HomePort);
		}

		OrgHeader GetRelatedOrg(ForwardingShipment shipment, bool isImport)
		{
			if (isImport && shipment.ConsigneeDeliveryAddress != null && !shipment.ConsigneeDeliveryAddress.E2_AddressOverride)
			{
				return shipment.ConsigneeDeliveryAddress.Organisation;
			}
			else if (shipment.ConsignorPickupAddress != null && !shipment.ConsignorPickupAddress.E2_AddressOverride)
			{
				return shipment.ConsignorPickupAddress.Organisation;
			}

			return null;
		}

		static OrgStaffAssignmentsCollection GetStaffAssignments(OrgHeader relatedOrg)
		{
			var relatedCompany = GetRelatedBranch(relatedOrg)?.Company;
			return relatedCompany != null ? relatedOrg.GetStaffAssignmentsForGlbCompany(relatedCompany) : relatedOrg.StaffAssignments;
		}

		static GlbBranch GetRelatedBranch(OrgHeader relatedOrg)
		{
			if (relatedOrg == null)
			{
				return null;
			}

			return GlbBranch.FindControllingBranchWithFallBackToAnyCompany(relatedOrg)
				?? GlbBranch.FindByHomePortWithFallBackToRelatedPort(relatedOrg.Factory, relatedOrg.ClosestPort);
		}
	}
}
