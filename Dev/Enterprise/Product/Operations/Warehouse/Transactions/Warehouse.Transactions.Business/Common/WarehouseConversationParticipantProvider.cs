using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseConversationParticipantProvider : IWarehouseConversationParticipantProvider
	{
		public IEnumerable<IConversationParticipant> GetAdditionalParticipants(WhsDocket docket, JobConversationParticipant sender)
		{
			if (docket == null || (sender?.Parent is GlbStaff))
			{
				return Enumerable.Empty<IConversationParticipant>();
			}

			switch (docket.WD_DocketType)
			{
				case DocketType.Codes.Receive:
					var receiveOptionRegistryItem = WebDataRegistry.Instance.WarehouseReceiptsNotificationOptions;
					var receiveGroupRegistryItem = WebDataRegistry.Instance.WarehouseReceiptsNotificationEmailGroup;
					var receiveRoleRegistryItem = WebDataRegistry.Instance.WarehouseReceiptsNotificationStaffRoles;

					return GetAdditionalParticipants(docket, receiveOptionRegistryItem, receiveGroupRegistryItem, receiveRoleRegistryItem);
				case DocketType.Codes.Order:
					var orderOptionRegistryItem = WebDataRegistry.Instance.WarehouseOrdersNotificationOptions;
					var orderGroupRegistryItem = WebDataRegistry.Instance.WarehouseOrdersNotificationEmailGroup;
					var orderRoleRegistryItem = WebDataRegistry.Instance.WarehouseOrdersNotificationStaffRoles;

					return GetAdditionalParticipants(docket, orderOptionRegistryItem, orderGroupRegistryItem, orderRoleRegistryItem);
				default:
					return Enumerable.Empty<IConversationParticipant>();
			}
		}

		IEnumerable<IConversationParticipant> GetAdditionalParticipants(WhsDocket docket, CodePairRegistryItem optionRegistryItem, GuidRegistryItem groupRegistryItem, CodeDescriptionBoolRegistryItem roleRegistryItem)
		{
			var factory = docket.Factory;
			var relatedOrg = docket.Client;
			var controllingBranch = GetRelatedBranch(relatedOrg);
			var groupRegistryValue = groupRegistryItem.GetFallBackValueAtAllLevels(
				controllingBranch?.GB_GC.ToGuid() ?? Guid.Empty,
				controllingBranch?.PK.ToGuid() ?? Guid.Empty,
				Guid.Empty);
			var roleRegistryValue = roleRegistryItem.GetFallBackValueAtAllLevels(
				controllingBranch?.GB_GC.ToGuid() ?? Guid.Empty,
				controllingBranch?.PK.ToGuid() ?? Guid.Empty,
				Guid.Empty);

			var result = new List<IConversationParticipant>();

			switch (optionRegistryItem.Value)
			{
				case EmailNotificationSendingRules.ALL:
					result.AddRange(GetStaffParticipantsFromGroupRegistryItem(factory, groupRegistryValue));
					result.AddRange(GetStaffParticipantsFromRoleRegistryItem(factory, relatedOrg, roleRegistryValue));

					break;
				case EmailNotificationSendingRules.GRP:
					result.AddRange(GetStaffParticipantsFromGroupRegistryItem(factory, groupRegistryValue));
					if (result.Count == 0)
					{
						result.AddRange(GetStaffParticipantsFromRoleRegistryItem(factory, relatedOrg, roleRegistryValue));
					}

					break;
				case EmailNotificationSendingRules.ROL:
					result.AddRange(GetStaffParticipantsFromRoleRegistryItem(factory, relatedOrg, roleRegistryValue));
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

		IEnumerable<IConversationParticipant> GetStaffParticipantsFromRoleRegistryItem(BusinessObjectFactory factory, OrgHeader relatedOrg, CodeDescriptionBoolCollection roles)
		{
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
				var staffNk = assignments.GetStaffAssignment(role, OrgStaffAssignmentsLookups.WarehouseServices);
				var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNk);
				if (staff != null && !string.IsNullOrEmpty(staff.GS_EmailAddress))
				{
					results.Add(staff);
				}
			}
		}

		static OrgStaffAssignmentsCollection GetStaffAssignments(OrgHeader relatedOrg)
		{
			var relatedCompany = GetRelatedBranch(relatedOrg)?.Company;

			return relatedCompany != null ? relatedOrg.GetStaffAssignmentsForGlbCompany(relatedCompany) : relatedOrg.StaffAssignments;
		}

		static GlbBranch GetRelatedBranch(OrgHeader relatedOrg)
		{
			var relatedBranch = GlbBranch.FindControllingBranchWithFallBackToAnyCompany(relatedOrg) ?? GlbBranch.FindByHomePortWithFallBackToRelatedPort(relatedOrg.Factory, relatedOrg.ClosestPort);

			return relatedBranch;
		}
	}
}
