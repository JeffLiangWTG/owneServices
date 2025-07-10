using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class AssistWithThisTaskHelper
	{
		/// <summary>
		/// Creates or finds an appropriate assistance task assigned to the user.
		/// </summary>
		/// <returns>True if a new assistance task was created or False if there is an appropriate assistance task.</returns>
		public static bool TryCreateAssistTaskOrGetExistingOne(IProcessTask taskRequiringAssistance, out IProcessTask assistanceTask, string staffToAssign = null, bool allowUsingAppropriateCapabilityTask = true)
		{
			var task = (ProcessTask)taskRequiringAssistance;
			CheckTaskIsNotStandalone(task);

			var settingForThisWorkflowType = GetSettingsForWorkflowType(taskRequiringAssistance);

			var userCode = staffToAssign ?? GetCurrentUserCode(task);
			CheckUserIsNotAssignedToTask(task, userCode);

			var existingAssistanceTask = FindProperExistingAssistanceTaskForStaff(task, settingForThisWorkflowType, userCode, allowUsingAppropriateCapabilityTask);

			if (existingAssistanceTask != null)
			{
				assistanceTask = existingAssistanceTask;
				return false;
			}

			assistanceTask = CreateAssistTaskCore(task, settingForThisWorkflowType);
			assistanceTask.P9_GS_NKAssignedStaffMember = userCode;
			return true;
		}

		static ProcessTask FindProperExistingAssistanceTaskForStaff(ProcessTask targetTask, AssistWithThisTaskSettingForOneWorkflowType settingForThisWorkflowType, string userCode, bool allowUsingAppropriateCapabilityTask)
		{
			Argument.NotNullOrEmpty(userCode, nameof(userCode));

			var workflow = targetTask.ProcessHeader;
			var assistanceTasks = FindProperExistingAssistanceTasks(targetTask, settingForThisWorkflowType)
				.Where(t => t.P9_GS_NKAssignedStaffMember == userCode ||
					allowUsingAppropriateCapabilityTask && !t.P9_G4_RequiredCapability.IsEmpty && t.GetIntersectionOfCapabilityAndGroup(workflow, workflow.Factory).Select(s => s.GS_Code).Contains(userCode));
			return (ProcessTask)assistanceTasks.FirstOrDefault();
		}

		/// <summary>
		/// Creates or finds an appropriate assistance task assigned to the capability.
		/// </summary>
		/// <returns>True if a new assistance task was created or False if there is an appropriate assistance task.</returns>
		public static bool TryCreateAssistTaskOrGetExistingOne(IProcessTask taskRequiringAssistance, out IProcessTask assistanceTask, ZGuid capabilityToAssignPK)
		{
			var task = (ProcessTask)taskRequiringAssistance;
			CheckTaskIsNotStandalone(task);

			var settingForThisWorkflowType = GetSettingsForWorkflowType(taskRequiringAssistance);

			var existingAssistanceTask = FindProperExistingAssistanceTaskForCapability(task, settingForThisWorkflowType, capabilityToAssignPK);

			if (existingAssistanceTask != null)
			{
				assistanceTask = existingAssistanceTask;
				return false;
			}

			assistanceTask = CreateAssistTaskCore(task, settingForThisWorkflowType);
			assistanceTask.P9_G4_RequiredCapability = capabilityToAssignPK;
			return true;
		}

		static ProcessTask FindProperExistingAssistanceTaskForCapability(ProcessTask targetTask, AssistWithThisTaskSettingForOneWorkflowType settingForThisWorkflowType, ZGuid capabilityPK)
		{
			var assistanceTasks = FindProperExistingAssistanceTasks(targetTask, settingForThisWorkflowType)
				.Where(t => t.P9_G4_RequiredCapability == capabilityPK);
			return (ProcessTask)assistanceTasks.FirstOrDefault();
		}

		public static IReadOnlyCollection<AddAssistanceTaskForStaffMenuItemInfo> GetStaffCodesAndDescriptionsForAssistanceTask(ProcessTask task, ProcessTaskCollection tasks)
		{
			var result = new List<AddAssistanceTaskForStaffMenuItemInfo>();

			if (task == null || tasks == null)
			{
				return result;
			}

			var tasksArray = tasks.Cast<ProcessTask>().WhereNotNull().ToArray();
			var factory = new BusinessObjectFactory();
			var users = new Dictionary<string, IStaff>(StringComparer.OrdinalIgnoreCase);

			var userCodes = tasksArray
				.Select(t => t.P9_GS_NKAssignedStaffMember)
				.WhereNotNull()
				.Where(w => !w.IsEmpty)
				.ToArray();

			if (userCodes.Any())
			{
				var staffQuery = new ZQuery(GlbStaffSchema.GS_Code, userCodes);
				staffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);

				var staff = factory.Load<GlbStaff>(staffQuery);

				foreach (var s in staff)
				{
					users.Add(s.GS_Code, s);
				}
			}

			var capabilityTasks = tasksArray.Where(t => !t.P9_G4_RequiredCapability.IsEmpty).ToArray();

			if (capabilityTasks.Any())
			{
				foreach (var t in capabilityTasks)
				{
					var processHeader = t.ProcessHeader;

					if (processHeader == null)
					{
						continue;
					}

					var staffInIntersectionOfCapabilityAndGroup = t.GetIntersectionOfCapabilityAndGroup(processHeader, processHeader.Factory)
						.WhereNotNull()
						.ToArray();

					foreach (var staff in staffInIntersectionOfCapabilityAndGroup)
					{
						if (staff.GS_IsActive && !users.ContainsKey(staff.GS_Code))
						{
							users.Add(staff.GS_Code, staff);
						}
					}
				}
			}

			if (Env.CurrentUser is IStaff currentUser && !users.ContainsKey(currentUser.GS_Code))
			{
				users.Add(currentUser.GS_Code, currentUser);
			}

			if (!task.P9_SystemCreateUser.IsEmpty && !users.ContainsKey(task.P9_SystemCreateUser))
			{
				var staffQuery = new ZQuery(GlbStaffSchema.GS_Code, task.P9_SystemCreateUser);
				staffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
				var creatingUser = factory.LoadTop1<GlbStaff>(staffQuery);

				if (creatingUser != null)
				{
					users.Add(creatingUser.GS_Code, creatingUser);
				}
			}

			if (users.TryGetValue(task.P9_GS_NKAssignedStaffMember, out var assignedUser))
			{
				users.Remove(assignedUser.GS_Code);
			}

			foreach (var user in users.Values.OrderBy(o => o.GS_Code))
			{
				result.Add(new AddAssistanceTaskForStaffMenuItemInfo(user.GS_Code, ResString.GetMultilingualString("ace32b18-8cc7-49e1-88a9-fd2916e3e507", "{0} - {1}", user.GS_Code, user.GS_FullName)));
			}

			return result;
		}

		public static IReadOnlyCollection<AddAssistanceTaskForCapabilityMenuItemInfo> GetCapabilityCodesAndDescriptionsForAssistanceTask(ProcessTask task, ProcessTaskCollection tasks)
		{
			var result = new List<AddAssistanceTaskForCapabilityMenuItemInfo>();
			var factory = new BusinessObjectFactory();
			GlbCapability[] primaryCapabilities = Array.Empty<GlbCapability>();

			if (task == null || tasks == null)
			{
				return result;
			}

			var tasksArray = tasks.Cast<ProcessTask>().WhereNotNull().ToArray();
			var capabilityPKs = tasksArray.Select(t => t.P9_G4_RequiredCapability).Where(c => !c.IsEmpty).Distinct();

			if (capabilityPKs.Any())
			{
				var capabilityQuery = new ZQuery(GlbCapabilitySchema.PK, capabilityPKs);
				capabilityQuery.AddToFilter(GlbCapabilitySchema.G4_IsActive, true);

				primaryCapabilities = factory.Load<GlbCapability>(capabilityQuery);

				foreach (var capability in primaryCapabilities.OrderBy(c => c.G4_Code))
				{
					result.Add(new AddAssistanceTaskForCapabilityMenuItemInfo(capability.PK, ResString.GetMultilingualString("80006b4a-3a82-4905-8a35-720bb5c0109f", "{0} - {1}", capability.G4_Code, capability.G4_Description)));
				}
			}

			var userCodes = tasksArray.Select(t => t.P9_GS_NKAssignedStaffMember).Where(w => !w.IsEmpty);

			if (userCodes.Any())
			{
				var capabilityStaffQuery = new ZDBOnlyQuery(typeof(GlbCapability));
				capabilityStaffQuery.AddToFilter(GlbCapabilitySchema.G4_IsActive, true);

				if (primaryCapabilities.Any())
				{
					capabilityStaffQuery.AddToFilter(GlbCapabilitySchema.G4_Code, SQLComparisonOperator.NotEqual, primaryCapabilities.Select(s => s.G4_Code));
				}

				var pivotSubQuery = new ZDBOnlySubQuery(typeof(GlbResourceCapabilityPivot), GlbResourceCapabilityPivotSchema.G5_G4_Capability);

				var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.PK);
				staffSubQuery.AddToFilter(GlbStaffSchema.GS_Code, userCodes);
				staffSubQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);

				pivotSubQuery.AddSubQuery(GlbResourceCapabilityPivotSchema.G5_GS_Resource, staffSubQuery, JoinCondition.And);
				capabilityStaffQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

				var secondaryCapabilities = factory.Load<GlbCapability>(capabilityStaffQuery);

				foreach (var capability in secondaryCapabilities.OrderBy(c => c.G4_Code))
				{
					result.Add(new AddAssistanceTaskForCapabilityMenuItemInfo(capability.PK, ResString.GetMultilingualString("80006b4a-3a82-4905-8a35-720bb5c0109f", "{0} - {1}", capability.G4_Code, capability.G4_Description)));
				}
			}

			return result;
		}

		static IEnumerable<IProcessTask> FindProperExistingAssistanceTasks(ProcessTask targetTask, AssistWithThisTaskSettingForOneWorkflowType settingForThisWorkflowType)
		{
			return targetTask.ProcessHeader == null
				? Array.Empty<IProcessTask>()
				: targetTask.ProcessHeader?.Tasks
					.Where(t => t.P9_Type == settingForThisWorkflowType.TaskType)
					.Where(t => t.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && t.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
					.Where(t => t.P9_Sequence == targetTask.P9_Sequence || IsTaskStartable(t));
		}

		static bool IsTaskStartable(IProcessTask task) => ObjectFactory.Get<IProcessTaskHelper>().IsTaskStartable(task);

		static IProcessTask CreateAssistTaskCore(ProcessTask taskRequiringAssistance, AssistWithThisTaskSettingForOneWorkflowType settingForThisWorkflowType)
		{
			CheckWorkflowTypeIsSetUpInRegistry(settingForThisWorkflowType, taskRequiringAssistance);

			var assistTask = (ProcessTask)taskRequiringAssistance.Clone(GetCloneArgs());
			assistTask.P9_TaskCannotBeDeleted = false;
			assistTask.P9_Type = settingForThisWorkflowType.TaskType;
			assistTask.P9_EstDuration = settingForThisWorkflowType.LowEstimateMinutes.GetDateTimeFromMinutes();
			assistTask.P9_EstimateVariationFactor = new ZDecimal(settingForThisWorkflowType.VariationFactor);
			assistTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			return assistTask;
		}

		public static bool CanBeUsedForTargetTaskWhenCreatingAssistanceTaskForCurrentUser(IProcessTask targetTask)
		{
			var processTask = (ProcessTask)targetTask;
			return CanBeUsedForTargetTask(targetTask) && targetTask.P9_GS_NKAssignedStaffMember != GetCurrentUserCode(processTask);
		}

		public static bool CanBeUsedForTargetTask(IProcessTask targetTask)
		{
			var processTask = (ProcessTask)targetTask;

			return
				processTask != null &&
				!processTask.IsDeleted &&
				!processTask.IsStandaloneTask &&
				IsEnabledForWorkflowType(processTask);
		}

		static string GetCurrentUserCode(ProcessTask task)
		{
			var factory = task.Factory;

			return factory.GetCachedValue("AssistWithThisTaskHelper.GetCurrentUserCode", () => GlbStaff.GetCurrentUser(task.Factory).GS_Code);
		}

		static bool IsEnabledForWorkflowType(ProcessTask task)
		{
			var workflowType = task.WorkflowType;
			return task.Factory.GetCachedValue("AssistWithThisTaskHelper.IsEnabledForWorkflowType_" + workflowType, () => !string.IsNullOrEmpty(GetSettingsForWorkflowType(workflowType)?.TaskType));
		}

		static AssistWithThisTaskSettingForOneWorkflowType GetSettingsForWorkflowType(IProcessTask task)
		{
			var workflowType = ((ProcessTask)task).WorkflowType;
			return GetSettingsForWorkflowType(workflowType);
		}

		static AssistWithThisTaskSettingForOneWorkflowType GetSettingsForWorkflowType(string workflowType)
		{
			var registryItem = WorkflowDataRegistry.Instance.AssistWithThisTask.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			return registryItem.GetTaskDetailsForWorkflowType(workflowType);
		}

		static BusinessObjectCloneArgs GetCloneArgs()
		{
			return new BusinessObjectCloneArgs(new[]
			{
				ProcessTasksSchema.Constants.P9_CardNote,
				ProcessTasksSchema.Constants.P9_Description,
				ProcessTasksSchema.Constants.P9_EstDuration,
				ProcessTasksSchema.Constants.P9_EstimateVariationFactor,
				ProcessTasksSchema.Constants.P9_G4_RequiredCapability,
				ProcessTasksSchema.Constants.P9_GG_AssignedGroup,
				ProcessTasksSchema.Constants.P9_GS_NKAssignedStaffMember,
				ProcessTasksSchema.Constants.P9_Notes,
				ProcessTasksSchema.Constants.P9_Type,
			});
		}

		#region Check for invalid state

		static void CheckTaskIsNotStandalone(ProcessTask task)
		{
			if (task.IsStandaloneTask)
			{
				throw new InvalidOperationException("Assist With This Task is not available for standalone tasks.");
			}
		}

		static void CheckUserIsNotAssignedToTask(ProcessTask task, ZString userCode)
		{
			if (task.P9_GS_NKAssignedStaffMember == userCode)
			{
				var exception = new InvalidOperationException("Assist With This Task / Add Assistance Task For is only valid on tasks assigned to someone other than the currently assigned user. See exception Data for task id.");
				exception.Data.Add("TaskID", task.P9_TaskID);

				throw exception;
			}
		}

		static void CheckWorkflowTypeIsSetUpInRegistry(AssistWithThisTaskSettingForOneWorkflowType settingForThisWorkflowType, ProcessTask task)
		{
			if (settingForThisWorkflowType == null || settingForThisWorkflowType.TaskType.IsEmpty)
			{
				var exception = new InvalidOperationException("Assist With This Task / Add Assistance Task For has not been set up in the registry. This functionality should not be available. See exception Data for workflow and task type.");
				exception.Data.Add("WorkflowType", task.WorkflowType);
				exception.Data.Add("TaskType", task.P9_Type);

				throw exception;
			}
		}

		#endregion
	}
}
