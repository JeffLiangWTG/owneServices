using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ProcessManagement.Business
{
	public static class NotificationRecipientCalculator
	{
		public static string[] GetEmailAddressesForTriggerActionNotification(ProcessTaskNotification triggerAction, BusinessObject parent)
		{
			var staffWithEmails = Array.Empty<GlbStaff>();
			var workRequest = (WorkRequest)parent;

			if (workRequest != null)
			{
				switch (triggerAction.PQ_TriggerParty)
				{
					case MessageRecipientPartyTypeList.Codes.Client:
						return GetEmailAddressesForClientRecipient(workRequest);

					case MessageRecipientPartyTypeList.Codes.NotificationGroup:
						staffWithEmails = GetValidStaffAccordingToFallbackSequence(workRequest, new[] { RecipientSourceTypeList.Codes.NotificationGroup, LastEditingStaffSourceType });
						break;

					case MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup:
						staffWithEmails = GetValidStaffAccordingToFallbackSequence(workRequest, new[] { RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup, RecipientSourceTypeList.Codes.NotificationGroup, LastEditingStaffSourceType });
						break;

					case MessageRecipientPartyTypeList.Codes.LastCompletedTaskResource:
						staffWithEmails = GetValidStaffAccordingToFallbackSequence(workRequest, new[] { RecipientSourceTypeList.Codes.LastCompletedTaskResource, RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup, RecipientSourceTypeList.Codes.NotificationGroup, LastEditingStaffSourceType });
						break;

					default:
						staffWithEmails = Array.Empty<GlbStaff>();
						break;
				}
			}

			return staffWithEmails.Select(s => s.GS_EmailAddress.ToString()).ToArray();
		}

		public static GlbStaff[] GetStaffForEConversationNotifications(WorkRequest workRequest)
		{
			var orderedFallbacks = ProcessManagementRegistry.Instance.RecipientDeterminationFallbackForNonSubscribedCSTickets.Value.SourceCollection
				.Cast<RecipientSource>()
				.OrderBy(x => x.FallbackSequence)
				.Select(x => x.SourceType.ToString());

			return GetValidStaffAccordingToFallbackSequence(workRequest, orderedFallbacks);
		}

		#region Fallback Logic

		const string LastEditingStaffSourceType = "EDT"; // This can't be configured anywhere, but is a valid fallback. TODO: make this configurable.

		static GlbStaff[] GetValidStaffAccordingToFallbackSequence(WorkRequest workRequest, IEnumerable<string> fallbackSequence)
		{
			foreach (var sourceType in fallbackSequence)
			{
				var staff = GetStaffForSourceType(workRequest, sourceType);

				if (staff.Any())
				{
					return staff;
				}
			}

			return Array.Empty<GlbStaff>();
		}

		static GlbStaff[] GetStaffForSourceType(WorkRequest workRequest, string sourceType)
		{
			switch (sourceType)
			{
				case RecipientSourceTypeList.Codes.LastCompletedTaskResource:
					return GetLastCompletedTaskStaff(workRequest);

				case RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup:
					return GetJobLevelWorkflowReleaseGroupMembers(workRequest);

				case RecipientSourceTypeList.Codes.NotificationGroup:
					return GetNotificationGroupMembers(workRequest);

				case LastEditingStaffSourceType:
					return GetLastEditingStaff(workRequest);

				default:
					return Array.Empty<GlbStaff>();
			}
		}

		#endregion

		#region Email Business Logic

		static string[] GetEmailAddressesForClientRecipient(WorkRequest workRequest)
		{
			var email = workRequest?.Client?.OC_Email.ToString();

			return !string.IsNullOrEmpty(email) ? new[] { email } : Array.Empty<string>();
		}

		static GlbStaff[] GetLastEditingStaff(WorkRequest workRequest)
		{
			var lastEditLog = workRequest.GetLogs().MostRecentLogByEventTime(AutoEvents.EditedARecord, log => !log.User.GS_IsSystemAccount && !log.User.GS_EmailAddress.IsEmpty);

			if (lastEditLog != null)
			{
				return new[] { (GlbStaff)lastEditLog.User };
			}

			return Array.Empty<GlbStaff>();
		}

		static GlbStaff[] GetNotificationGroupMembers(WorkRequest workRequest)
		{
			var branchPK = workRequest?.WKR_GB_Branch ?? ZGuid.Empty;
			var departmentPK = workRequest?.WKR_GE_Department ?? ZGuid.Empty;

			var groupPK = ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.GetFallBackValueAtAllLevels(Guid.Empty, branchPK.IsValid ? branchPK.ToGuid() : Guid.Empty, departmentPK.IsValid ? departmentPK.ToGuid() : Guid.Empty);
			var groupMembers = GetGroupMembersWithEmailAddresses(groupPK, workRequest.Factory);

			if (!groupMembers.IsNullOrEmpty())
			{
				return groupMembers;
			}

			return GetLastEditingStaff(workRequest);
		}

		static GlbStaff[] GetJobLevelWorkflowReleaseGroupMembers(WorkRequest workRequest)
		{
			if (workRequest != null)
			{
				var jobLevelWorkflow = ProcessJobHeaderProvider.GetForParent(workRequest, workRequest.Factory, addDefaultProcessHeaderIfNone: false);

				if (jobLevelWorkflow != null && jobLevelWorkflow.FH_GG_ReleaseGroup.IsValid)
				{
					var groupMembers = GetGroupMembersWithEmailAddresses(jobLevelWorkflow.FH_GG_ReleaseGroup, workRequest.Factory);

					if (!groupMembers.IsNullOrEmpty())
					{
						return groupMembers;
					}
				}
			}

			return Array.Empty<GlbStaff>();
		}

		static GlbStaff[] GetLastCompletedTaskStaff(WorkRequest workRequest)
		{
			if (workRequest != null)
			{
				var lastCompletedTask = workRequest.WorkflowItems.Tasks.Cast<ProcessTask>().Where(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed).MaxBySafe(task => task.P9_CompletedTimeUtc);
				var assignedStaff = lastCompletedTask?.AssignedStaffMember;

				if (assignedStaff != null && !assignedStaff.GS_EmailAddress.IsEmpty)
				{
					return new[] { assignedStaff };
				}
			}

			return Array.Empty<GlbStaff>();
		}

		static GlbStaff[] GetGroupMembersWithEmailAddresses(ZGuid groupPK, BusinessObjectFactory factory)
		{
			var group = factory.Load<GlbGroup>(groupPK);

			return group != null
				? group.Staff.Cast<GlbStaff>().Where(s => !s.GS_EmailAddress.IsEmpty).ToArray()
				: Array.Empty<GlbStaff>();
		}

		#endregion
	}
}
