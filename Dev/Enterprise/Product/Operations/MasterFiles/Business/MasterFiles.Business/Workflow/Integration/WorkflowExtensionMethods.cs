using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class WorkflowExtensionMethods
	{
		#region IRootTypeProvider

		public static Type[] GetRootTypes(this IWorkflowTypeProvider typeProvider, Type triggerType = null)
		{
			var rootTypes = new List<Type>(3);

			void AddProcessHeader(string workflowTypeCode)
			{
				if (ProcessJobHeaderProvider.SupportsPAVE(workflowTypeCode, typeProvider.Factory))
				{
					rootTypes.Add(typeof(IProcessJobHeader));
				}
			}

			void AddTriggerType()
			{
				if (triggerType != null && !rootTypes.Contains(triggerType))
				{
					rootTypes.Add(triggerType);
				}
				else if (typeProvider is IWorkflowItem)
				{
					var workflowType = typeProvider.GetType();
					if (!rootTypes.Contains(workflowType))
					{
						rootTypes.Add(workflowType);
					}
				}
			}

			if (typeProvider is ILineTriggerSupport line && WorkflowDescriptors.Instance.TryGetValue(line.LineTriggerType, out var lineDescriptor))
			{
				var task = (ProcessTask)typeProvider;
				var jobType = lineDescriptor.WorkflowProviderType;
				if (jobType != null)
				{
					rootTypes.Add(GetCountrySpecificType(jobType, task));
					AddProcessHeader(lineDescriptor.Code);
				}
				else
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"Descriptor Missing type {lineDescriptor.GetType().FullName}"), "Workflow Descriptors should all have that Workflow Provider Type.");
				}

				AddTriggerType();

				if (typeProvider.IsTemplate)
				{
					var triggerParentType = task.WorkflowDescriptorCore.WorkflowProviderType;
					rootTypes.Add(GetCountrySpecificType(triggerParentType, task));
				}
				else
				{
					var triggerParentType = task.GetJob().GetType();
					rootTypes.Add(triggerParentType);
				}
			}
			else
			{
				var descriptor = typeProvider.GetWorkflowDescriptor();
				if (descriptor != null)
				{
					if (!typeProvider.IsTemplate && typeProvider is ProcessTask task)
					{
						rootTypes.Add(task.GetJob().GetType());
						var macroTypes = descriptor.MacroTypes(task);
						if (macroTypes.Any())
						{
							rootTypes.AddRange(macroTypes);
						}
					}
					else
					{
						var triggerParentType = descriptor.WorkflowProviderType;
						rootTypes.Add(triggerParentType);
					}
					rootTypes.AddAdditionalRootType(typeProvider, descriptor);
					AddProcessHeader(descriptor.Code);
				}
				else
				{
					// Code path can be accessed via ProcessCompanyLinkRule form when descriptor is invariant.
				}
				AddTriggerType();
			}

			return rootTypes.ToArray();
		}

		static void AddAdditionalRootType(this List<Type> rootTypes, IWorkflowTypeProvider typeProvider, WorkflowDescriptor descriptor)
		{
			if (typeProvider is ProcessTask task)
			{
				var additionalRootType = descriptor.GetAdditionalRootType(task.TemplateConditions, task.TriggerConditions);
				if (additionalRootType != null && !rootTypes.Contains(additionalRootType))
				{
					rootTypes.Add(additionalRootType);
				}
			}
		}

		public static Type GetCountrySpecificTypeIfApplicable(this IWorkflowTypeProvider typeProvider)
		{
			var parentType = typeProvider.GetWorkflowDescriptor()?.WorkflowProviderType;
			if (parentType != null && typeProvider is IWorkflowItem workflowItem)
			{
				parentType = GetCountrySpecificType(parentType, workflowItem);
			}
			return parentType;
		}

		static Type GetCountrySpecificType(Type parentType, IWorkflowItem workflowItem)
		{
			if (workflowItem != null && TypeDecider.GetTypeDeciderFromType(parentType) is CountrySpecificTypeDecider countryTypeDecider)
			{
				var company = workflowItem.GetCompany();
				if (company == null && workflowItem.GetJob() is ProcessTaskTemplate template)
				{
					company = template.Branch?.Company ?? template.Company;
				}
				if (company != null)
				{
					parentType = countryTypeDecider.GetTypeForCountryCode(company.GC_RN_NKCountryCode);
				}
			}

			return parentType;
		}

		public static BusinessObject[] GetRoots(this IBaseTrigger trigger, BusinessObject triggerJob)
		{
			if (triggerJob.IsDeleted)
			{
				return Array.Empty<BusinessObject>();
			}

			var rootTypes = new List<BusinessObject>(3);

			void AddProcessHeader(string workflowTypeCode, IWorkflowProviderCore parent)
			{
				if (ProcessJobHeaderProvider.SupportsPAVE(workflowTypeCode, trigger.Factory))
				{
					var processJobHeader = ProcessJobHeaderProvider.GetForParent(parent, trigger.Factory, addDefaultProcessHeaderIfNone: false) as BusinessObject;
					if (processJobHeader != null)
					{
						rootTypes.Add(processJobHeader);
					}
				}
			}

			if (trigger is ILineTriggerSupport line && WorkflowDescriptors.Instance.TryGetValue(line.LineTriggerType, out var lineDescriptor))
			{
				var jobType = lineDescriptor.WorkflowProviderType;
				if (jobType != null)
				{
					if (triggerJob != null)
					{
						rootTypes.Add(triggerJob);
						if (triggerJob is IWorkflowProviderCore workflowProvider)
						{
							AddProcessHeader(lineDescriptor.Code, workflowProvider);
						}
					}

					rootTypes.Add((BusinessObject)trigger);
					rootTypes.Add(trigger.GetJob());
				}
			}
			else if (trigger != null && WorkflowDescriptors.Instance.TryGetValue(trigger.WorkflowProcessType, out var descriptor))
			{
				var triggerParentType = descriptor.WorkflowProviderType;
				if (triggerParentType != null)
				{
					if (triggerJob is IWorkflowProviderCore workflowProvider)
					{
						rootTypes.Add(triggerJob);
						AddProcessHeader(descriptor.Code, workflowProvider);
					}
					else
					{
						//the triggerJob can be a child of the workflow provider
						var parent = trigger.GetJob();
						if (parent.IsDeleted)
						{
							return Array.Empty<BusinessObject>();
						}
						if (parent is IWorkflowProviderCore workflowProvider1)
						{
							rootTypes.Add(parent);
							rootTypes.Add(triggerJob);
							AddProcessHeader(descriptor.Code, workflowProvider1);
						}
					}
				}
				rootTypes.Add((BusinessObject)trigger);
			}

			return rootTypes.ToArray();
		}

		#endregion

		#region WorkflowItem Types

		public static bool IsTask(this IWorkflowItem workflowItem)
		{
			switch (workflowItem.WorkflowItemType)
			{
				case Constants.Workflow.MilestoneType:
				case Constants.Workflow.WorkflowTriggerType:
				case Constants.Workflow.ExceptionType:
					return false;

				default:
					return true;
			}
		}

		public static bool ShouldFieldBeReadonly(this IWorkflowItem workflowItem, PropertyDescriptor propertyDescriptor)
		{
			var processTask = workflowItem as ProcessTask;
			var result = processTask?.GetShouldPropertiesBeReadOnly(propertyDescriptor);
			return result ?? false;
		}

		public static bool IsMilestone(this IWorkflowItem workflowItem)
		{
			return workflowItem.WorkflowItemType == Constants.Workflow.MilestoneType;
		}

		public static bool IsTrigger(this IWorkflowItem workflowItem)
		{
			return workflowItem.WorkflowItemType == Constants.Workflow.WorkflowTriggerType;
		}

		#endregion

		#region WorkflowDescriptor

		public static WorkflowDescriptor GetWorkflowDescriptor(this IWorkflowTypeProvider workflowItem)
		{
			var processTask = workflowItem as ProcessTask;

			if (processTask != null)
			{
				return processTask.WorkflowDescriptor;
			}
			else
			{
				return WorkflowDescriptors.Instance.TryGetValueSafe(workflowItem.WorkflowProcessType);
			}
		}

		#endregion

		#region Triggers

		public static ActiveBusinessObjectCollection<ProcessTaskNotification> CompletionTriggerActionsCollection(this IBaseTrigger trigger)
		{
			return (ActiveBusinessObjectCollection<ProcessTaskNotification>)trigger.TriggerActions;
		}

		public static StmEvent GetEvent(this IBaseTrigger trigger)
		{
			return !trigger.TriggerEventCode.IsEmpty ? trigger.Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, trigger.TriggerEventCode) : null;
		}

		public static string GetDescriptionWithReference(this IBaseTrigger trigger)
		{
			var result = trigger.Description;
			var reference = (trigger as ProcessTask)?.ReferenceCode;

			if (reference.HasValue && !reference.Value.IsEmpty)
			{
				result += " (" + reference + ")";
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer logging information")]
		public static string GetDiagnosticLogInfo(this IWorkflowItem workflowItem)
		{
			var parentLogInfo = ZString.Empty;
			var parentBizo = workflowItem.GetJob();

			if (parentBizo != null)
			{
				var humanReadableName = parentBizo.HumanReadableName;
				var jobNumber = JobNumberResolver.GetJobNumber(parentBizo);

				if (!jobNumber.IsEmpty)
				{
					jobNumber = !humanReadableName.Contains(jobNumber, StringComparison.OrdinalIgnoreCase)
						? string.Format(CultureInfo.InvariantCulture, (NoResString)"(Job Number: {0})", jobNumber)
						: string.Empty;
				}
				else
				{
					jobNumber = string.Format(CultureInfo.InvariantCulture, "(PK: {0})", parentBizo.PK);
				}

				parentLogInfo = jobNumber.IsEmpty
					? string.Format(CultureInfo.InvariantCulture, "{0}\r\n", parentBizo.HumanReadableName)
					: string.Format(CultureInfo.InvariantCulture, "{0} {1}\r\n", parentBizo.HumanReadableName, jobNumber);
			}

			var result = new StringBuilder(parentLogInfo);
			result.Append(workflowItem.HumanReadableName);
			result.Append(" (");

			result.Append((NoResString)"Type: ");
			result.Append(workflowItem.WorkflowItemType);

			if (workflowItem.IsTrigger() && workflowItem is IBaseTrigger trigger)
			{
				result.AppendFormat(CultureInfo.InvariantCulture, ", Event: {0}, Field: {1}, Trigger Condition: {2}, Trigger Condition Value: {3}, Last Fired Time: {4}, Trigger Context Code: {5}, Trigger Branch: {6}, Trigger Staff Code: {7}",
					trigger.TriggerEventCode,
					trigger.TriggerFieldName,
					trigger.TriggerCondition,
					trigger.TriggerConditionValue,
					(trigger as IWorkflowTrigger)?.LastFiredTime.ToZDateTime(),
					trigger.TriggerContextCode,
					trigger.TriggerBranch,
					trigger.TriggerStaffCode
					);

				var context = FormattableString.Invariant($@", Company: {Env.CurrentCompany.Code} - Branch: {Env.CurrentBranch.Code} - Dept: {Env.CurrentDepartment.Code} - Country: {((ZString)Env.CurrentBranch.NKUNLOCO).SubstringSafe(0, 2)}");
				result.Append(context);
			}
			else if (workflowItem.IsMilestone() && workflowItem is IMilestoneDateDefaultable milestone)
			{
				result.AppendFormat(
					CultureInfo.InvariantCulture,
					", Event: {0}, Actual Date: {1}",
					milestone.TriggerEventCode,
					milestone.ActualDate);
			}

			result.AppendFormat(
				CultureInfo.InvariantCulture,
				", Parent Table Code: {0}, Parent ID: {1}",
				workflowItem.ParentTableCode,
				workflowItem.ParentID);

			result.Append(", PK: ");
			result.Append(workflowItem.Identifier);
			result.Append(")");

			return result.ToString();
		}

		#endregion

		#region Workflow Parent

		public static BusinessObject GetJob(this IWorkflowItem workflowItem)
		{
			return workflowItem.IsDeleted ? null : workflowItem.GetParentBusinessObject(workflowItem.ParentID, false);
		}

#if DEBUG
		public static IWorkflowProvider GetParent(this IWorkflowItem workflowItem)
		{
			return workflowItem.GetParentBusinessObject() as IWorkflowProvider;
		}

		public static BusinessObject GetParentBusinessObject(this IWorkflowItem workflowItem)
		{
			return workflowItem.GetParentBusinessObject(workflowItem.ParentID, false);
		}
#endif

		public static BusinessObject GetParentBusinessObject(this IWorkflowItem trigger, IQueuedLog log)
		{
			return GetParentBusinessObject(trigger, GetWTEEventData(log));
		}

		public static BusinessObject GetParentBusinessObject(this IWorkflowItem trigger, StmALog log)
		{
			return GetParentBusinessObject(trigger, GetWTEEventData(log)); // TODO: Better shared interface for StmALog and StmJobQueue
		}

		public static bool IsLineTriggerEvent(this IWorkflowItem trigger, IQueuedLog log)
		{
			return IsLineTriggerEvent(trigger, GetWTEEventData(log));
		}

		static bool IsLineTriggerEvent(IWorkflowItem trigger, WorkflowTriggerEventData data)
		{
			return data != null && trigger is ILineTriggerSupport line && !line.LineTriggerType.IsEmpty;
		}

		static WorkflowTriggerEventData GetWTEEventData(IQueuedLog log)
		{
			return log != null && log.SJ_SE_NKEvent == Events.WorkflowTriggerEventCode ? new WorkflowTriggerEventData(log) : null;
		}

		static WorkflowTriggerEventData GetWTEEventData(StmALog log)
		{
			return log != null && log.SL_SE_NKEvent == Events.WorkflowTriggerEventCode ? new WorkflowTriggerEventData(log) : null;
		}

		public static BusinessObject GetParentBusinessObject(this IWorkflowItem trigger, WorkflowTriggerEventData data)
		{
			if (IsLineTriggerEvent(trigger, data))
			{
				if (data.TriggeringLogParentPK.IsEmpty)
				{
					return null;
				}
				else
				{
					return trigger.GetParentBusinessObject(data.TriggeringLogParentPK, true);
				}
			}
			else
			{
				return trigger.GetJob();
			}
		}

		static BusinessObject GetParentBusinessObject(this IWorkflowItem workflowItem, ZGuid parentPK, bool loadLine)
		{
			if (workflowItem != null && !workflowItem.IsDeleted && parentPK.IsValid)
			{
				var parentType = GetParentType(workflowItem, loadLine);
				if (parentType != null && parentType != typeof(BusinessObject))
				{
					return workflowItem.Factory.Load(parentType, parentPK);
				}
				else
				{
					return null;
				}
			}

			return null;
		}

		static Type GetParentType(IWorkflowItem workflowItem, bool getLineType)
		{
			if (getLineType)
			{
				if (workflowItem is ILineTriggerSupport lineTrigger)
				{
					var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(lineTrigger.LineTriggerType);
					return descriptor?.WorkflowProviderType;
				}
				else
				{
					return null;
				}
			}
			else
			{
				var parentType = (workflowItem as ProcessTask)?.ParentType;
				if (parentType != null)
				{
					return parentType;
				}
				else
				{
					if (workflowItem.ParentTableCode == ProcessTaskTemplateSchema.Constants.Prefix)
					{
						return typeof(ProcessTaskTemplate);
					}
					else
					{
						return workflowItem.GetWorkflowDescriptor()?.WorkflowProviderType;
					}
				}
			}
		}

		public static string GetParentJobDetails(this IWorkflowItem workflowItem)
		{
			var result = string.Empty;
			var parentBizo = workflowItem.GetParentBusinessObject(workflowItem.ParentID, false);

			if (parentBizo != null)
			{
				result = parentBizo.HumanReadableName;

				try
				{
					var parentJobDesc = DescriptionPropertyAttribute.DescriptionFromBusinessObject(parentBizo);

					if (!result.Contains(parentJobDesc))
					{
						result += " - " + parentJobDesc;
					}
				}
				catch (NoCodePropertyException)
				{
				}
			}

			return result;
		}

		#endregion

		#region Workflow Item Related Entities

		public static IGlbCompany GetCompany(this IWorkflowItem workflowItem)
		{
			return workflowItem.Factory.Load<IGlbCompany>(workflowItem.CompanyPK);
		}

		public static IGlbStaff GetAssignedStaffMember(this IWorkflowItem workflowItem)
		{
			var assignable = workflowItem as IAssignedWorkflowItem;

			return assignable != null ? workflowItem.Factory.LoadFromNaturalKey<IGlbStaff>(GlbStaffSchema.GS_Code, assignable.AssignedStaffCode) : null;
		}

		public static IGlbGroup GetAssignedGroup(this IWorkflowItem workflowItem)
		{
			var assignable = workflowItem as IAssignedWorkflowItem;

			return assignable != null ? workflowItem.Factory.Load<IGlbGroup>(assignable.AssignedGroupPK) : null;
		}

		#endregion

		#region Workflow Templates

		[DebuggerStepThrough]
		public static ApplyWorkflowTemplateResult ApplyWorkflowTemplates(this IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters = null)
		{
			if (parameters == null)
			{
				return new ProcessTask.Loader(((IBusiness)workflowProvider).Factory).CreateTasksAndMilestonesFromTemplateIfRequired(workflowProvider, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}
			else
			{
				var newParameters = TemplateApplicationParameters.ApplyIgnoreHasChangesToExistingParameters(parameters);
				return new ProcessTask.Loader(((IBusiness)workflowProvider).Factory).CreateTasksAndMilestonesFromTemplateIfRequired(workflowProvider, newParameters);
			}
		}

		public static bool AreTemplateConditionsMet(this IWorkflowProvider workflowProvider, ITemplateConditionalWorkflowItem templateConditional)
		{
			var templateConditionEvaluator = ObjectFactory.New<IWorkflowTemplateConditionEvaluator>();
			return templateConditionEvaluator.AreConditionsMetForTemplateApplication(workflowProvider, templateConditional);
		}

		public static bool IsCondition2Met(this ProcessTaskCollection collection, ITemplateConditionalWorkflowItem workflowItem)
		{
			var templateConditionEvaluator = ObjectFactory.New<IWorkflowTemplateConditionEvaluator>();
			return templateConditionEvaluator.IsCondition2Met((IWorkflowProvider)collection.Parent, workflowItem);
		}

		#endregion

		#region Containment Barriers

		public static bool IsQualityContainmentBarrierTask(this IProcessTask task)
		{
			var workflowItem = (IWorkflowItem)task;
			var taskTypeCode = workflowItem.WorkflowItemType;

			if (!taskTypeCode.IsEmpty)
			{
				var taskType = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskType(workflowItem.WorkflowProcessType, taskTypeCode);
				return taskType != null && taskType.ContainmentBarrierIterationType != ContainmentBarrierIterationTypeList.Codes.NCB;
			}

			return false;
		}

		#endregion

		#region Tasks

		public static void CancelNonStartedTasksAndClosePartiallyCompletedTasks(this IWorkflowProvider workflowProvider)
		{
			foreach (ProcessTask task in workflowProvider.WorkflowItems.Tasks.ToArray())
			{
				if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Working || task.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended)
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				}
				else if (!task.IsClosed)
				{
					task.CancelAndSuspendValidationOnTaskCancellation();
				}
			}
		}

		#endregion

		#region IWorkflowTask

		public static bool IsOpen(this IWorkflowTask task)
		{
			return ProcessTask.GetOpenTaskStatuses().Contains(task.P9_Status.ToString());
		}

		public static GlbStaff GetAssignedStaffMember(this IWorkflowTask task, BusinessObjectFactory factory)
		{
			return factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public static GlbGroup GetAssignedTaskGroup(this IWorkflowTask task, BusinessObjectFactory factory)
		{
			return factory.Load<GlbGroup>(task.P9_GG_AssignedGroup);
		}

		public static GlbCapability GetRequiredCapability(this IWorkflowTask task, BusinessObjectFactory factory)
		{
			return factory.Load<GlbCapability>(task.P9_G4_RequiredCapability);
		}

		public static bool RequiresResourceWithCapability(this IWorkflowTask task)
		{
			return task.P9_GS_NKAssignedStaffMember.IsEmpty && task.P9_G4_RequiredCapability.IsValid;
		}

		public static string GetCapabilityName(this IWorkflowTask task, BusinessObjectFactory factory)
		{
			var capability = task.GetRequiredCapability(factory);

			return capability != null ? capability.G4_Description : ZString.Empty;
		}

		public static IEnumerable<GlbStaff> GetIntersectionOfCapabilityAndGroup(this IWorkflowTask task, IWorkflow workflow, BusinessObjectFactory factory, GlbCapability targetCapability = null)
		{
			var capability = targetCapability ?? task.GetRequiredCapability(factory);
			var group = capability == null || capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GroupScope
				? task.GetAssignedTaskGroup(factory) ?? workflow?.GetReleaseGroup(factory)
				: null;

			var resourcesWithCapability = capability?.ResourcesWithCapability.Cast<GlbStaff>();
			var resourcesWithinGroup = group?.Staff.Cast<GlbStaff>();

			var intersection = capability != null && group != null
				? resourcesWithCapability.Intersect(resourcesWithinGroup)
				: capability != null
					? resourcesWithCapability
					: group != null
						? resourcesWithinGroup
						: Enumerable.Empty<GlbStaff>();

			return intersection;
		}

		#endregion

		#region IWorkflow

		public static ZDateTime GetRelevantEarliestStartDateUtc(this IWorkflow workflow)
		{
			if (workflow.EarliestStartDateUtc != default)
			{
				return workflow.EarliestStartDateUtc;
			}
			else
			{
				return workflow.JobEarliestStartDateUtc;
			}
		}

		public static GlbGroup GetReleaseGroup(this IWorkflow workflow, BusinessObjectFactory factory)
		{
			return factory.Load<GlbGroup>(workflow.ReleaseGroupPK);
		}

		#endregion

		#region IProcessTask

		public static IEnumerable<GlbStaff> GetIntersectionOfCapabilityAndGroup(this IProcessTask task, IProcessHeader processHeader, BusinessObjectFactory factory)
		{
			return GetIntersectionOfCapabilityAndGroup(task as IWorkflowTask, processHeader as IWorkflow, factory);
		}

		#endregion
	}
}
