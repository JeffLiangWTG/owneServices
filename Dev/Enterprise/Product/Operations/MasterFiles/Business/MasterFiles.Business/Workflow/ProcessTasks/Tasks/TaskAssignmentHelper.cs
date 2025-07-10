using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class TaskAssignmentHelper
	{
		#region APIs

		public delegate void AssignResourceToTaskDelegate(ZString resource, ProcessTask task);
		public delegate ZString GetResourceAssignedToTaskDelegate(ProcessTask task);

		public static void PopulateRelatedTasksWithSameResource(ProcessTask task)
		{
			PopulateRelatedTasksWithSameResource(task,
				assignDelegate: (r, t) => t.SetP9_GS_NKAssignedStaffMemberCore(r),
				getResourceDelegate: t => t.P9_GS_NKAssignedStaffMember);
		}

		public static HashSet<ZGuid> PopulateRelatedTasksWithSameResourceAndReport(ProcessTask task)
		{
			var assignedTaskPKs = new HashSet<ZGuid>();

			void assignDelegate(ZString r, ProcessTask t)
			{
				t.SetP9_GS_NKAssignedStaffMemberCore(r);
				if (!t.Equals(task))
				{
					assignedTaskPKs.Add(t.PK);
				}
			}

			PopulateRelatedTasksWithSameResource(task,
				assignDelegate: assignDelegate,
				getResourceDelegate: t => t.P9_GS_NKAssignedStaffMember);

			return assignedTaskPKs;
		}

		public static void PopulateRelatedTasksWithSameResource(ProcessTask task, AssignResourceToTaskDelegate assignDelegate, GetResourceAssignedToTaskDelegate getResourceDelegate, bool requireValidResource = true)
		{
			var resourceCode = getResourceDelegate(task);

			if (requireValidResource)
			{
				var factory = task.Factory;
				var resource = factory.LoadFromNaturalKey(typeof(GlbStaff), GlbStaffSchema.GS_Code, resourceCode);

				if (resource == null)
				{
					return;
				}
			}
			else
			{
				if (resourceCode.IsEmpty)
				{
					return;
				}
			}

			using (task.SuspendAssignmentRestrictionValidation())
			{
				if (WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.Value)
				{
					TryToAssignRelatedTasks_ConsideringCapability(task, assignDelegate, getResourceDelegate);
				}
				else
				{
					TryToAssignRelatedTasks(task, assignDelegate, getResourceDelegate);
				}
			}
		}

		static void TryToAssignRelatedTasks_ConsideringCapability(
			ProcessTask assignedTask,
			AssignResourceToTaskDelegate assignDelegate,
			GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			var otherCapabilities = assignedTask.AssignedStaffMember?.Capabilities.Select(cap => cap.PK) ?? Enumerable.Empty<ZGuid>();
			foreach (var restriction in GetTaskAssignmentSAMRestrictionsByTaskType(assignedTask))
			{
				var relatedTasks = GetValidProcessTaskListAccordingToScope(assignedTask, restriction.Scope, usedInTaskValidation: false, getResourceDelegate: getResourceDelegate).ToArray();

				foreach (var relatedTask in relatedTasks)
				{
					if (relatedTask.PK != assignedTask.PK
						&& IsRelatedTaskPartOfSAMRestrictionsAndUnassigned(restriction, relatedTask, getResourceDelegate)
						&& otherCapabilities.Contains(relatedTask.P9_G4_RequiredCapability))
					{
						AssignRelatedTask(assignedTask, relatedTask, assignDelegate, getResourceDelegate);
					}
				}
			}
		}

		static void TryToAssignRelatedTasks(
			ProcessTask assignedTask,
			AssignResourceToTaskDelegate assignDelegate,
			GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			foreach (var restriction in GetTaskAssignmentSAMRestrictionsByTaskType(assignedTask))
			{
				var relatedTasks = GetValidProcessTaskListAccordingToScope(assignedTask, restriction.Scope, usedInTaskValidation: false, getResourceDelegate: getResourceDelegate).ToArray();

				foreach (var relatedTask in relatedTasks)
				{
					if (assignedTask.PK != relatedTask.PK && IsRelatedTaskPartOfSAMRestrictionsAndUnassigned(restriction, relatedTask, getResourceDelegate))
					{
						AssignRelatedTask(assignedTask, relatedTask, assignDelegate, getResourceDelegate);
					}
				}
			}
		}

		static void AssignRelatedTask(
			ProcessTask assignedTask,
			ProcessTask toAssignTask,
			AssignResourceToTaskDelegate assignDelegate,
			GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			assignDelegate(getResourceDelegate(assignedTask), toAssignTask);
		}

		public static bool DoRestrictionsAllowResourceToBeAssignedToTasks(ZString staff, IEnumerable<ProcessTask> tasks)
		{
			var assigner = new TaskTrialAssigner();
			foreach (var task in tasks)
			{
				if (assigner.GetAssignedStaffCode(task).IsEmpty)
				{
					assigner.AssignResourceToTask(staff, task);
				}
			}
			return assigner.AreAssignmentsAllowedByDIFRestrictions;
		}

		public static bool DoDirectDIFRestrictionsAllowResourceToBeAssignedToTask(ZString staff, ProcessTask task, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			foreach (var restriction in GetTaskAssignmentDIFRestrictionsByTaskType(task))
			{
				var taskList = GetValidProcessTaskListAccordingToScope(task, restriction.Scope, usedInTaskValidation: true, getResourceDelegate: getResourceDelegate);
				foreach (var sameResourceTaskInScope in taskList.Where(t => getResourceDelegate(t) == staff).ToArray())
				{
					if (sameResourceTaskInScope.PK != task.PK && AreTwoTasksUnderDIFRestrictions(restriction, task, sameResourceTaskInScope))
					{
						return false;
					}
				}
			}
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service string for trial assignment")]
		public static double GetCapacityRequiredByTasksRespectingSAMRestrictions(IEnumerable<ProcessTask> tasks)
		{
			var assigner = new TaskTrialAssigner();
			const string staffCode = "Unique string to mark all tasks envolved in assignment";
			assigner.AssignResourceToTasks(staffCode, tasks);
			return assigner.CapacityInMinutesConsumedByAssignedTasks;
		}

		public static IEnumerable<ProcessTask> GetValidProcessTaskListAccordingToScope(ProcessTask task, ZString scope, bool usedInTaskValidation)
		{
			return GetValidProcessTaskListAccordingToScope(task, scope, usedInTaskValidation, getResourceDelegate: t => t.P9_GS_NKAssignedStaffMember);
		}

		public static IEnumerable<ProcessTask> GetValidProcessTaskListAccordingToScope(ProcessTask task, ZString scope, bool usedInTaskValidation, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			switch (scope)
			{
				case ScopeList.Codes.Job:
					return GetTasksInJob(task, usedInTaskValidation, getResourceDelegate);
				case ScopeList.Codes.Workflow:
					return GetTasksInCurrentWorkflow(task, usedInTaskValidation, getResourceDelegate);
				case ScopeList.Codes.Prerequisites:
					return GetTasksInCurrentAndPrerequisiteWorkflows(task, usedInTaskValidation, getResourceDelegate);
				case ScopeList.Codes.Postrequisites:
					return GetTasksInCurrentAndPostrequisiteWorkflows(task, usedInTaskValidation, getResourceDelegate);
				case ScopeList.Codes.Child:
					return GetTasksInCurrentAndChildWorkflows(task, usedInTaskValidation, getResourceDelegate);
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid Restriction Scope: {0}", scope));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static Dictionary<GlbCapability, List<ProcessTask>> GetTaskAllocationBlocks(ProcessTask[] tasksCollection, bool requireCapabilityMatch)
		{
			var groupingDictionary = GroupByCapabilities(tasksCollection).ToDictionary(g => g.Key, g => g.ToList());

			foreach (var (capability, tasks) in groupingDictionary.Select(kv => (kv.Key, kv.Value)))
			{
				var relatedTasks = GetRelatedTasksBasedOnSAMRestrictions(tasks, requireCapabilityMatch).ToArray();
				RegroupRelatedTasks(groupingDictionary, capability, relatedTasks);
			}

			return groupingDictionary;
		}

		public static bool DifRestrictionsExistInRegistry(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(nameof(DifRestrictionsExistInRegistry), () => WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.Value.ContainsActiveDifRestrictions);
		}

		#endregion

		#region Implementations

		static TaskTypeRestrictions[] GetTaskAssignmentRestrictionsByTaskType(ProcessTask task)
		{
			var factory = task.Factory;
			var type = task.P9_Type;
			return factory.GetCachedValue(type, () => WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.Value.FindRestrictionsByTaskType(type)).ToArray();
		}

		static TaskTypeRestrictions[] GetTaskAssignmentSAMRestrictionsByTaskType(ProcessTask task)
		{
			return GetTaskAssignmentRestrictionsByTaskType(task).Where(r => r.RestrictionType == RestrictionTypeList.Codes.SameResource).ToArray();
		}

		static TaskTypeRestrictions[] GetTaskAssignmentDIFRestrictionsByTaskType(ProcessTask task)
		{
			return GetTaskAssignmentRestrictionsByTaskType(task).Where(r => r.RestrictionType == RestrictionTypeList.Codes.DifferentResource).ToArray();
		}

		static bool IsRelatedTaskPartOfSAMRestrictions(TaskTypeRestrictions restriction, ProcessTask relatedTask)
		{
			return restriction.WorkflowType == relatedTask.WorkflowType && (restriction.TaskType == relatedTask.P9_Type || restriction.TaskTypesCollection.Any(t => ((RestrictedTaskTypes)t).Code == relatedTask.P9_Type));
		}

		static bool IsRelatedTaskPartOfSAMRestrictionsAndUnassigned(TaskTypeRestrictions restriction, ProcessTask relatedTask)
		{
			return IsRelatedTaskPartOfSAMRestrictionsAndUnassigned(restriction, relatedTask, getResourceDelegate: t => t.P9_GS_NKAssignedStaffMember);
		}

		static bool AreTwoTasksUnderDIFRestrictions(TaskTypeRestrictions restriction, ProcessTask task1, ProcessTask task2)
		{
			return restriction.TaskType == task1.P9_Type && restriction.TaskTypesCollection.Any(t => ((RestrictedTaskTypes)t).Code == task2.P9_Type) ||
				restriction.TaskType == task2.P9_Type && restriction.TaskTypesCollection.Any(t => ((RestrictedTaskTypes)t).Code == task1.P9_Type);
		}

		static bool IsRelatedTaskPartOfSAMRestrictionsAndUnassigned(TaskTypeRestrictions restriction, ProcessTask relatedTask, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			return IsRelatedTaskPartOfSAMRestrictions(restriction, relatedTask) && getResourceDelegate(relatedTask).IsEmpty;
		}

		static IEnumerable<ProcessTask> GetRelatedTasks(ProcessTask task, bool usedInTaskValidation, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			if (task != null && task.P9_ParentID.IsEmpty)
			{
				return Enumerable.Empty<ProcessTask>();
			}

			var parent = task?.Parent;

			if (parent == null)
			{
				ReportGetRelatedTasksError(task);

				return Enumerable.Empty<ProcessTask>();
			}

			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, parent.PK)
				.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, task.P9_ParentTableCode)
				.AddToFilter(ProcessTasks.GetNonTasksExclusionQuery());
			var tasks = task.Factory.Load<ProcessTask>(query);

			return tasks.Where(toCompare => IsTaskRelated(task, toCompare, usedInTaskValidation, getResourceDelegate));
		}

		static bool IsTaskRelated(ProcessTask baseTask, ProcessTask toCompare, bool usedInTaskValidation, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			return baseTask.PK != toCompare.PK
				&& DoTasksShareCompanyScope(baseTask, toCompare)
				&& (usedInTaskValidation ? toCompare.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled : toCompare.IsOpen)
				&& (usedInTaskValidation ? !getResourceDelegate(toCompare).IsEmpty : getResourceDelegate(toCompare).IsEmpty);
		}

		static bool DoTasksShareCompanyScope(ProcessTask baseTask, ProcessTask toCompare)
		{
			return (baseTask.P9_ShareTasksForAllCompanies && toCompare.P9_ShareTasksForAllCompanies)
				|| (!baseTask.P9_ShareTasksForAllCompanies && !toCompare.P9_ShareTasksForAllCompanies && baseTask.P9_GC == toCompare.P9_GC);
		}

		#region SuppressResourceStringsCheckRegion

		static void ReportGetRelatedTasksError(ProcessTask task)
		{
			string error;

			if (task == null)
			{
				error = "Error: GetRelatedTasks was called when task was null";
			}
			else
			{
				error = $@"Error: GetRelatedTasks was called when task.ParentTaskCollection?.Tasks was null.
task:												{task.ToString() ?? "null"}
task.ParentTaskCollection:							{task.ParentTaskCollection?.ToString() ?? "null"}

task.IsDeleted:										{task.IsDeleted.ToString() ?? "null"}
task.P9_ParentID:									{task.P9_ParentID.ToString() ?? "null"}
task.P9_ParentID.IsValid:							{task.P9_ParentID.IsValid.ToString() ?? "null"}
task.P9_ParentTableCode:							{task.P9_ParentTableCode.ToString() ?? "null"}
task.ParentType:									{task.ParentType?.ToString() ?? "null"}
task.GetWorkflowDescriptor().WorkflowProviderType:	{task.GetWorkflowDescriptor()?.WorkflowProviderType?.ToString() ?? "null"}
(task as ILineTriggerSupport):						{(task as ILineTriggerSupport)?.ToString() ?? "null"}
(task as ILineTriggerSupport).LineTriggerType:		{(task as ILineTriggerSupport)?.LineTriggerType.ToString() ?? "null"}";
			}

			ErrorReporter.ReportOnce("GetRelatedTasksNull", error);
		}

		#endregion

		static IEnumerable<ProcessTask> GetTasksInJob(ProcessTask task, bool usedInTaskValidation, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			return GetRelatedTasks(task, usedInTaskValidation, getResourceDelegate);
		}

		static IEnumerable<ProcessTask> WithProcessHeader(this IEnumerable<ProcessTask> tasks, ProcessTask task)
		{
			var processHeader = task.ProcessHeader;

			if (processHeader == null)
			{
				return Enumerable.Empty<ProcessTask>();
			}

			return tasks.Where(t => t.ProcessHeader != null);
		}

		static bool IsInSameProcessHeader(ProcessTask taskA, ProcessTask taskB) => taskA.ProcessHeader.PK == taskB.ProcessHeader.PK;

		static IEnumerable<ProcessTask> GetTasksInCurrentWorkflow(ProcessTask task, bool usedInTaskValidation, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			return GetRelatedTasks(task, usedInTaskValidation, getResourceDelegate)
				.WithProcessHeader(task)
				.Where(t => IsInSameProcessHeader(t, task));
		}

		static IEnumerable<ProcessTask> GetTasksInCurrentAndPrerequisiteWorkflows(ProcessTask task, bool usedInTaskValidation, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			return GetRelatedTasks(task, usedInTaskValidation, getResourceDelegate)
				.WithProcessHeader(task)
				.Where(t => IsInSameProcessHeader(t, task) || task.ProcessHeader.PrerequisiteLinks.Any(l => l.FP_FH_HeaderFrom == t.ProcessHeader.PK));
		}

		static IEnumerable<ProcessTask> GetTasksInCurrentAndPostrequisiteWorkflows(ProcessTask task, bool usedInTaskValidation, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			return GetRelatedTasks(task, usedInTaskValidation, getResourceDelegate)
				.WithProcessHeader(task)
				.Where(t => IsInSameProcessHeader(t, task) || task.ProcessHeader.LinksFromMeToOthers.Children.Any(l => ((IProcessHeaderLink)l).FP_FH_HeaderTo == t.ProcessHeader.PK));
		}

		static IEnumerable<ProcessTask> GetTasksInCurrentAndChildWorkflows(ProcessTask task, bool usedInTaskValidation, GetResourceAssignedToTaskDelegate getResourceDelegate)
		{
			return GetRelatedTasks(task, usedInTaskValidation, getResourceDelegate)
				.WithProcessHeader(task)
				.Where(t => IsInSameProcessHeader(t, task) || task.ProcessHeader.LinksFromOthersToMe.Children.Except(task.ProcessHeader.PrerequisiteLinks).Any(l => ((IProcessHeaderLink)l).FP_FH_HeaderFrom == t.ProcessHeader.PK));
		}

		static IEnumerable<IGrouping<GlbCapability, ProcessTask>> GroupByCapabilities(ProcessTask[] tasksCollection)
		{
			return tasksCollection.Where(t => t.RequiresResourceWithCapability).OrderBy(t => t.P9_Sequence).GroupBy(t => t.RequiredCapability);
		}

		static IEnumerable<ProcessTask> GetRelatedTasksBasedOnSAMRestrictions(IEnumerable<ProcessTask> tasks, bool requireCapabilityMatch)
		{
			foreach (var task in tasks)
			{
				foreach (var restriction in GetTaskAssignmentSAMRestrictionsByTaskType(task))
				{
					foreach (var relatedTask in GetValidProcessTaskListAccordingToScope(task, restriction.Scope, usedInTaskValidation: false).ToArray())
					{
						if (relatedTask.PK != task.PK
							&& IsRelatedTaskPartOfSAMRestrictionsAndUnassigned(restriction, relatedTask)
							&& (!requireCapabilityMatch
								|| task.P9_G4_RequiredCapability == relatedTask.P9_G4_RequiredCapability))
						{
							yield return relatedTask;
						}
					}
				}
			}
		}

		static void RegroupRelatedTasks(Dictionary<GlbCapability, List<ProcessTask>> groupingDictionary, GlbCapability capability, IEnumerable<ProcessTask> relatedTasks)
		{
			AddRelatedTasksIntoGroupingDictionary(groupingDictionary, capability, relatedTasks);
			RemoveRelatedTasksFromOtherGroups(groupingDictionary, capability, relatedTasks);
		}

		static void AddRelatedTasksIntoGroupingDictionary(Dictionary<GlbCapability, List<ProcessTask>> groupingDictionary, GlbCapability capability, IEnumerable<ProcessTask> relatedTasks)
		{
			foreach (var task in relatedTasks)
			{
				var groupedTasks = groupingDictionary.Single(g => g.Key == capability).Value;

				if (task.P9_G4_RequiredCapability.Equals(capability.PK) && !groupedTasks.Contains(task))
				{
					groupedTasks.Add(task);
				}
			}
		}

		static void RemoveRelatedTasksFromOtherGroups(Dictionary<GlbCapability, List<ProcessTask>> groupingDictionary, GlbCapability capability, IEnumerable<ProcessTask> relatedTasks)
		{
			foreach (var task in relatedTasks)
			{
				foreach (var grouping in groupingDictionary)
				{
					if (grouping.Key != capability && grouping.Value.Contains(task))
					{
						grouping.Value.Remove(task);
					}
				}
			}
		}

		#endregion
	}
}
