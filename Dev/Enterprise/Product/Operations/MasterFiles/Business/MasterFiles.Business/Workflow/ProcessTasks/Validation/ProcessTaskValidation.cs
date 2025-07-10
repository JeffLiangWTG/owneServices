using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskValidation : ProcessTaskValidationBase
	{
		public ProcessTaskValidation(ProcessTask parent)
			: base(parent)
		{
		}

		#region P9_Type

		protected override void CheckP9_Type()
		{
			base.CheckP9_Type();

			ListValidation.ErrorIfInvalidCode(Parent.P9_TypeInfo);

			if (!Parent.P9_Type.IsEmpty)
			{
				var taskTypeDefinition = Parent.Lookups.Types.Cast<WorkflowTaskType>().SingleOrDefault(t => t.Code == Parent.P9_Type);

				if (taskTypeDefinition != null && !taskTypeDefinition.IsActive)
				{
					if (Parent.IsInDatabase && !Parent.P9_TypeInfo.HasChanges)
					{
						Parent.P9_TypeInfo.AddWarning(Res.GetString("e8ec12c4-1b62-4d96-9709-209fe5e6e196", "This Task Type is inactive and should not be used."));
					}
					else
					{
						Parent.P9_TypeInfo.AddError(Res.GetString("f046219b-68b5-4ed9-8aa8-baaed96572a4", "This Task Type is inactive and must not be used."));
					}
				}
			}
		}

		#endregion

		#region P9_ActualDuration

		protected override void CheckP9_ActualDurationIsValidZDateTimeRange()
		{
			// This property is only used for hours
		}

		protected override void CheckP9_ActualDuration()
		{
			base.CheckP9_ActualDuration();

			if (!Parent.IsInDatabase || Parent.P9_StatusInfo.HasChanges || Parent.P9_ActualDurationInfo.HasChanges)
			{
				ValidateActualDurationValue(Parent.P9_ActualDurationInfo);
				ValidateP9_Status();
			}
		}

		void ValidateActualDurationValue(ZPropertyInfo info)
		{
			if (Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
			{
				var taskType = (WorkflowTaskType)Parent.Lookups.Types.FindByCode(Parent.P9_Type);

				if (taskType != null && taskType.IsRequireActualDuration && Parent.ActualDurationHours == 0)
				{
					info.AddError(ActualDurationErrorMessage);
				}
			}
		}

		public static MultilingualString ActualDurationErrorMessage => ResString.GetMultilingualString("2071606e-3391-4ad1-91e0-d3937195050b", "For closed tasks of this type, you must enter a non-zero actual duration.");

		#endregion

		#region CompletedTimeLocal / P9_CompletedTimeUtc

		protected override void CheckCompletedTimeLocal()
		{
			base.CheckCompletedTimeLocal();
			ValidateCompletionTime(Parent.CompletedTimeLocalInfo);
		}

		protected override void CheckP9_CompletedTimeUtc()
		{
			base.CheckP9_CompletedTimeUtc();
			ValidateCompletionTime(Parent.P9_CompletedTimeUtcInfo);
		}

		void ValidateCompletionTime(ZPropertyInfo info)
		{
			if (Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Closed && !Parent.P9_CompletedTimeUtc.IsValid)
			{
				info.AddError(Res.GetString("9E290267-45A6-4CC7-A5C6-738DC1E68691", "A closed task must have a completion time."));
			}
		}

		#endregion

		#region P9_Status / P9_GS_NKAssignedStaffMember / P9_GG_AssignedGroup / CheckP9_G4_RequiredCapability / WorkingOutOfBufferConditions

		protected override void CheckP9_Status()
		{
			base.CheckP9_Status();
			ValidateTemplateTaskStatus();
			ValidateActualDurationValue(Parent.P9_StatusInfo);
			ValidateAssigned(Parent.P9_StatusInfo);
			ValidateP9_GS_NKAssignedStaffMember();
			ValidateP9_GG_AssignedGroup();
			ValidateP9_G4_RequiredCapability();
			ValidateWorkingOutOfBufferConditions();
			ValidateTaskCancellationPermissions();
		}

		void ValidateTemplateTaskStatus()
		{
			if (Parent.IsTemplate && Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Working)
			{
				Parent.P9_StatusInfo.AddError(ResString.GetMultilingualString("D56CD594-E218-4A67-A7A1-618C57648DA4", "Template task status cannot be working."));
			}
		}

		void ValidateTaskCancellationPermissions()
		{
			if (Parent.IsValidationOnTaskCancellationSuspended ||
				!Parent.P9_StatusInfo.HasChanges ||
				Parent.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled ||
				Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed)
			{
				return;
			}

			var workflowTaskType = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(Parent.WorkflowType)
				.Cast<WorkflowTaskType>()
				.SingleOrDefault(w => w.Code == Parent.P9_Type);

			if (workflowTaskType != null && !workflowTaskType.CanCancelTask)
			{
				Parent.P9_StatusInfo.AddError(ResString.GetMultilingualString("457120ab-2fb9-4a5d-b6ea-123cdfd2b456", "You do not have permission to cancel this task."));
			}
		}

		void ValidateWorkingOutOfBufferConditions()
		{
			if (!Parent.P9_StatusInfo.HasChanges || Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed || Parent.P9_FH_ProcessHeader.IsEmpty || Parent.IsStandaloneTask)
			{
				return;
			}

			var originalStatus = Parent.P9_StatusInfo.OriginalValue.ToString();

			if (Parent.P9_Status != ProcessTaskStatusCodeList.Codes.Working && (originalStatus == ProcessTaskStatusCodeList.Codes.Working || originalStatus == ProcessTaskStatusCodeList.Codes.Suspended))
			{
				return;
			}

			if (Parent.P9_Status != ProcessTaskStatusCodeList.Codes.Working && Parent.P9_Status != ProcessTaskStatusCodeList.Codes.Suspended && Parent.P9_Status != ProcessTaskStatusCodeList.Codes.Closed)
			{
				return;
			}

			var workflowTaskType = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(Parent.WorkflowType)
				.Cast<WorkflowTaskType>()
				.SingleOrDefault(w => w.Code == Parent.P9_Type);

			if (workflowTaskType == null || workflowTaskType.WorkingStatusChangeType == WorkingStatusChangeTypeList.Codes.Allow)
			{
				return;
			}

			var processHeader = Parent.ProcessHeader;
			var component = processHeader.CurrentComponent;
			var headerComponentsAreBufferType = component?.FC_Type.ToString() == BMComponentTypeList.Codes.Buffer;

			if (component == null || headerComponentsAreBufferType)
			{
				return;
			}

			var headersAreStandby = processHeader.FH_IsStandby || Parent.JobHeader.FH_IsStandby;

			if (headersAreStandby)
			{
				return;
			}

			AddNotificationIfNeeded(workflowTaskType.WorkingStatusChangeType);
		}

		void AddNotificationIfNeeded(ZString changeStatus)
		{
			switch (changeStatus)
			{
				case WorkingStatusChangeTypeList.Codes.Error:
					Parent.P9_StatusInfo.AddError(ResString.GetMultilingualString("165ded1b-ef97-48b8-9717-65a1cb585b87",
						"This task’s status cannot be set to '{0}' because its workflow is not in a buffer component.", Parent.P9_Status));
					break;
				case WorkingStatusChangeTypeList.Codes.Warning:
					Parent.P9_StatusInfo.AddWarning(ResString.GetMultilingualString("9ed7abc8-97ed-475c-a1c3-0a16378f4556",
						"This task’s status should not be set to '{0}' because its workflow is not in a buffer component.", Parent.P9_Status));
					break;
				default:
					break;
			}
		}

		protected override void CheckP9_GS_NKAssignedStaffMember()
		{
			base.CheckP9_GS_NKAssignedStaffMember();
			ListValidation.ErrorIfInvalidCode(Parent.P9_GS_NKAssignedStaffMemberInfo);
			ValidateAssigned(Parent.P9_GS_NKAssignedStaffMemberInfo);
			ValidateP9_Status();
			ValidateP9_GG_AssignedGroup();
			ValidateP9_G4_RequiredCapability();
			ValidateTaskAssignmentRestrictions(Parent.P9_GS_NKAssignedStaffMemberInfo);
			CheckResourceHasCapability(Parent.P9_GS_NKAssignedStaffMemberInfo);

			ObjectFactory.Get<IProcessTaskBMSValidation>().CheckP9_GS_NKAssignedStaffMember(Parent);

			Parent.ProcessHeader?.Validation.ValidateFH_GG_ReleaseGroup();
		}

		protected override void CheckP9_GG_AssignedGroup()
		{
			base.CheckP9_GG_AssignedGroup();
			ValidateAssigned(Parent.P9_GG_AssignedGroupInfo);
			ValidateP9_GS_NKAssignedStaffMember();
			ValidateP9_Status();
			ValidateP9_G4_RequiredCapability();
			CheckTaskGroupHasMembers();
			CheckTaskGroupAllowsTaskToBeAssignedToResource();

			Parent.ProcessHeader?.Validation.ValidateFH_GG_ReleaseGroup();
		}

		protected override void CheckP9_G4_RequiredCapability()
		{
			base.CheckP9_G4_RequiredCapability();
			if (Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned)
			{
				ValidateAssigned(Parent.P9_G4_RequiredCapabilityInfo);
			}
			ValidateP9_Status();
			ValidateP9_GS_NKAssignedStaffMember();
			ValidateP9_GG_AssignedGroup();
			CheckResourceHasCapability(Parent.P9_G4_RequiredCapabilityInfo);
			CheckDefaultCapabilityAssigned();
			CheckCapabilityHasMembers();
			CheckCapabilityAllowsTaskToBeAssignedToResource();

			Parent.ProcessHeader?.Validation.ValidateFH_GG_ReleaseGroup();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGoToCaseOrDefault", Justification = "Baseline")]
		void ValidateAssigned(ZPropertyInfo info)
		{
			switch (Parent.P9_Status)
			{
				case ProcessTaskStatusCodeList.Codes.Assigned:
					if (Parent.P9_GS_NKAssignedStaffMember.IsEmpty && Parent.P9_GG_AssignedGroup.IsEmpty && Parent.P9_G4_RequiredCapability.IsEmpty)
					{
						info.AddError(ResourceGroupOrCapabilityRequiredMessage);
					}
					break;
				case ProcessTaskStatusCodeList.Codes.Working:
					if (Parent.P9_GS_NKAssignedStaffMember.IsEmpty && Parent.P9_GG_AssignedGroup.IsEmpty)
					{
						info.AddError(ResourceOrGroupRequiredMessage);
					}
					break;
				case ProcessTaskStatusCodeList.Codes.Closed:
					if (ObjectFactory.Get<IBMSRegistry>().RequireResourceToCloseTask)
					{
						if (info == Parent.P9_GS_NKAssignedStaffMemberInfo && Parent.P9_GS_NKAssignedStaffMember.IsEmpty)
						{
							info.AddError(ResourceRequiredMessage);
						}
					}
					else
					{
						goto case ProcessTaskStatusCodeList.Codes.Working;
					}
					break;
			}
		}

		static string ResourceRequiredMessage
		{
			get { return Res.GetString("2506f71b-923e-47fa-bef6-8b54f0b9f711", "For closed tasks, you must specify an assigned resource."); }
		}

		static string ResourceOrGroupRequiredMessage
		{
			get { return Res.GetString("5253e047-a142-4c43-8fb5-b86e60c233cf", "For working and closed tasks, you must specify either an assigned resource or group."); }
		}

		static string ResourceGroupOrCapabilityRequiredMessage
		{
			get { return Res.GetString("8df11928-5926-4d80-b8d6-9971b9968652", "For assigned tasks, you must specify either an assigned resource, group or capability."); }
		}

		void ValidateTaskAssignmentRestrictions(ZPropertyInfo info)
		{
			if (!Parent.IsTemplate)
			{
				Parent?.Parent?.WorkflowItems?.ValidationCache.Validate(Parent, info);
			}
		}

		static List<ZString> GetInvalidTypeList(ProcessTask parent, TaskTypeRestrictions restriction)
		{
			var list = new List<ZString>();

			if (restriction.TaskType != parent.P9_Type)
			{
				list.Add(restriction.TaskType);
			}

			if (restriction.TaskType == parent.P9_Type || restriction.RestrictionType == RestrictionTypeList.Codes.SameResource)
			{
				list.AddRange(restriction.TaskTypesCollection.Cast<RestrictedTaskTypes>().Where(t => t.Code != parent.P9_Type).Select(t => t.Code));
			}

			return list;
		}

		internal static AddValidationNotification<ProcessTask> GetRestrictionDelegate(TaskTypeRestrictions restriction)
		{
			return (parent, info) =>
			{
				string errorMessage;
				var parentType = parent.P9_Type;
				var types = string.Join(", ", GetInvalidTypeList(parent, restriction));
				var isError = restriction.NotificationType == NotificationTypeList.Codes.Error;

				if (restriction.RestrictionType == RestrictionTypeList.Codes.DifferentResource)
				{
					errorMessage = isError
						? Res.GetString("d3cf2427-6275-49f8-a2a9-c26bc8b9c9d0", "Tasks of type {0} cannot be assigned to the same resource as tasks of type {1}", parentType, types)
						: Res.GetString("a59f4abf-ec24-4254-a46c-2bc203b716ff", "Tasks of type {0} should not be assigned to the same resource as tasks of type {1}", parentType, types);
				}
				else
				{
					errorMessage = isError
						? Res.GetString("e06057a6-8172-41d6-962f-3fafa8367560", "Tasks of type {0} must be assigned to the same resource as tasks of type {1}", parentType, types)
						: Res.GetString("906522a7-deb6-4ba2-82cc-5de75bdceb2a", "Tasks of type {0} should be assigned to the same resource as tasks of type {1}", parentType, types);
				}

				if (isError)
				{
					info.AddError(errorMessage);
				}
				else
				{
					info.AddWarning(errorMessage);
				}
			};
		}

		internal static void AddProperErrorOrWarning(ProcessTask parent, TaskTypeRestrictions restriction, ZPropertyInfo info)
		{
			GetRestrictionDelegate(restriction)(parent, info);
		}

		protected void CheckTaskGroupHasMembers()
		{
			if (Parent.P9_GS_NKAssignedStaffMember.IsEmpty
				&& !Parent.P9_GG_AssignedGroup.IsEmpty
				&& (Parent.P9_G4_RequiredCapability.IsEmpty || ProcessValidationHelper.IsTaskAssignedToCapabilityWithGroupScope(Parent)))
			{
				if (ProcessValidationHelper.IsGroupEmpty_WithoutLoadingGlbStaffRecords(Parent.Factory, Parent.P9_GG_AssignedGroup))
				{
					if (WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.Value)
					{
						Parent.P9_GG_AssignedGroupInfo.AddError(TaskGroupHasNoMembersMessage);
					}
					else
					{
						Parent.P9_GG_AssignedGroupInfo.AddWarning(TaskGroupHasNoMembersMessage);
					}
				}
			}
		}

		static string TaskGroupHasNoMembersMessage => Res.GetString("9538519B-4789-4F58-97D1-8D027C041113", "The assigned task group has no members.");

		void CheckTaskGroupAllowsTaskToBeAssignedToResource()
		{
			if (ProcessValidationHelper.DoesTaskGroupDisallowTaskToBeAutoAssignedToResource(Parent))
			{
				if (WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.Value)
				{
					Parent.P9_GG_AssignedGroupInfo.AddError(TaskGroupDisallowsTaskToBeAssignedToResourceMessage);
				}
				else
				{
					Parent.P9_GG_AssignedGroupInfo.AddWarning(TaskGroupDisallowsTaskToBeAssignedToResourceMessage);
				}
			}
		}

		static string TaskGroupDisallowsTaskToBeAssignedToResourceMessage => Res.GetString("CF70073E-51DB-4716-911D-03714E12E626", "The intersection of the task group and the task capability has no resources in it.");

		void CheckResourceHasCapability(ZPropertyInfo info)
		{
			if (!Parent.P9_GS_NKAssignedStaffMember.IsEmpty && !Parent.P9_G4_RequiredCapability.IsEmpty)
			{
				var resource = Parent.AssignedStaffMember;
				var capability = Parent.RequiredCapability;
				if (resource != null && capability != null && !resource.Capabilities.Contains(capability))
				{
					if (WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.Value)
					{
						info.AddError(Res.GetString("5EFD1078-A661-43BB-AE7A-6B90440FE262", "The user {0} must possess the capability {1}.", resource.GS_Code, capability.G4_Code));
					}
					else
					{
						info.AddWarning(Res.GetString("9dd77ffa-bd1b-40d4-a8f9-491643d92ce3", "The user {0} does not possess the capability {1}.", resource.GS_Code, capability.G4_Code));
					}
				}
			}
		}

		#endregion

		protected void CheckCapabilityHasMembers()
		{
			if (Parent.P9_GS_NKAssignedStaffMember.IsEmpty && !Parent.P9_G4_RequiredCapability.IsEmpty)
			{
				if (ProcessValidationHelper.IsCapabilityEmpty_WithoutLoadingGlbStaffRecords(Parent.Factory, Parent.P9_G4_RequiredCapability))
				{
					if (WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.Value)
					{
						Parent.P9_G4_RequiredCapabilityInfo.AddError(CapabilityHasNoMembersMessage);
					}
					else
					{
						Parent.P9_G4_RequiredCapabilityInfo.AddWarning(CapabilityHasNoMembersMessage);
					}
				}
			}
		}

		static string CapabilityHasNoMembersMessage => Res.GetString("B427A1AF-F60A-4B08-9B41-B1F1EC457BCD", "There are no users possessing the assigned capability.");

		protected void CheckCapabilityAllowsTaskToBeAssignedToResource()
		{
			if (ProcessValidationHelper.DoesCapabilityDisallowTaskToBeAssignedToResource(Parent))
			{
				if (WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.Value)
				{
					Parent.P9_G4_RequiredCapabilityInfo.AddError(CapabilityDisallowsTaskToBeAssignedToResourceMessage);
				}
				else
				{
					Parent.P9_G4_RequiredCapabilityInfo.AddWarning(CapabilityDisallowsTaskToBeAssignedToResourceMessage);
				}
			}
		}

		static string CapabilityDisallowsTaskToBeAssignedToResourceMessage => Res.GetString("C92572BD-CDD3-4C22-BD91-FDAA919308D0", "The intersection of the task capability and the task group / workflow release group has no resources in it.");

		void CheckDefaultCapabilityAssigned()
		{
			if (Parent.DefaultCapabilityAssigned)
			{
				Parent.P9_G4_RequiredCapabilityInfo.AddWarning(DefaultCapabilityMessage);
			}
		}

		static string DefaultCapabilityMessage => Res.GetString("F0162A8B-6550-442D-9CD7-DC0599DB55C9", "The default capability has been applied.");

		#region P9_OA

		protected override void CheckP9_OA()
		{
			base.CheckP9_OA();
			if (!Parent.OrganisationPK.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.P9_OAInfo);
			}
		}

		#endregion

		#region P9_EstDuration

		protected override void CheckP9_EstDuration()
		{
			base.CheckP9_EstDuration();

			ObjectFactory.Get<IProcessTaskBMSValidation>().CheckP9_EstDuration(Parent);

			if (Parent.HighEstimatedDurationHours > 3600) // magic number representing the maximum hours that can be displayed correctly in ZTimeEdit controls
			{
				Parent.P9_EstDurationInfo.AddError(Res.GetString("0b501dfc-7b91-40a8-a0d8-2480ec4d5895", "The Low Estimate/Estimate Variation Factor combination entered results in a High Estimate which is too high. Please adjust the Low Estimate and/or Estimate Variation Factor values so that the High Estimate is no greater than 150 days (3600 hours)."));
			}
		}

		#endregion

		#region P9_EstimateVariationFactor

		protected override void CheckP9_EstimateVariationFactor()
		{
			base.CheckP9_EstimateVariationFactor();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.P9_EstimateVariationFactorInfo, 1);
		}

		#endregion

		#region P9_FH_ProcessHeader

		protected override void CheckP9_FH_ProcessHeader()
		{
			base.CheckP9_FH_ProcessHeader();
			ObjectFactory.Get<IProcessTaskBMSValidation>().CheckP9_FH_ProcessHeader(Parent);
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == ProcessTasksSchema.Constants.P9_FH_ProcessHeader && Parent.ProcessHeader != null && !Parent.ProcessHeader.FH_IsActive)
			{
				return false;
			}

			if ((info.Name == ProcessTasksSchema.Constants.P9_FH_ProcessHeader && Parent.IsClosed) || info.Name == ProcessTasksSchema.Constants.P9_OC)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region P9_MilestoneCompletionPivotKey

		protected override void CheckP9_MilestoneCompletionPivotKey()
		{
			base.CheckP9_MilestoneCompletionPivotKey();
			if (!Parent.P9_MilestoneCompletionPivotKey.IsEmpty)
			{
				ListValidation.WarnIfInvalidCode(Parent.P9_MilestoneCompletionPivotKeyInfo, ResString.GetMultilingualString("24618e62-ffaa-4c81-9be6-b5f62214976c", "Does not match any milestone."));
			}
		}

		#endregion

		#region Iteration

		protected override void CheckIteration()
		{
			base.CheckIteration();

			ListValidation.ErrorIfInvalidCode(Parent.IterationInfo);

			if (!string.IsNullOrEmpty(warningAboutIterationAutoAssignmentToAdd))
			{
				Parent.IterationInfo.AddWarning(warningAboutIterationAutoAssignmentToAdd);
			}
		}

		protected override IDisposable SetWarningToAddOnCheckIteration(string warningAboutIterationAutoAssignment)
		{
			warningAboutIterationAutoAssignmentToAdd = warningAboutIterationAutoAssignment;

			return new DisposableAction(() => warningAboutIterationAutoAssignmentToAdd = null);
		}

		string warningAboutIterationAutoAssignmentToAdd;

		#endregion
	}
}
