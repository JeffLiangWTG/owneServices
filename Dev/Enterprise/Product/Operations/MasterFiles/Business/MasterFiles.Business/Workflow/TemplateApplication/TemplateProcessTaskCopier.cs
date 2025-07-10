using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class TemplateProcessTaskCopier
	{
		public static T Clone<T>(ProcessTask source) where T : ProcessTask
		{
			return (T)Clone(source, typeof(T));
		}

		public static ProcessTask Clone(ProcessTask source, Type clonedType)
		{
			ProcessTask result = (ProcessTask)source.Factory.New(clonedType);
			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				List<string> excludedProperties = new List<string>();
				excludedProperties.Add(ProcessTasksSchema.Constants.P9_TaskID);
				excludedProperties.Add(ProcessTasksSchema.Constants.P9_ParentID);
				excludedProperties.Add(ProcessTasksSchema.Constants.P9_ParentTableCode);
				excludedProperties.Add(ProcessTasksSchema.Constants.P9_FH_ProcessHeader);

				if (!string.Equals(source.TriggerConditions.TriggerContextCode, TriggerUserContextList.Codes.Specified, StringComparison.OrdinalIgnoreCase))
				{
					excludedProperties.Add(ProcessTasksSchema.Constants.P9_GS_NKAssignedStaffMember);
				}
				var isTemplateProcessTask = source is TemplateProcessTask;
				var isTargetTemplate = result is TemplateProcessTask;

				if (isTemplateProcessTask)
				{
					if (!string.Equals(source.TriggerConditions.TriggerContextCode, TriggerUserContextList.Codes.Specified, StringComparison.OrdinalIgnoreCase) || source.P9_GC.IsEmpty)
					{
						excludedProperties.Add(ProcessTasksSchema.Constants.P9_GC);
					}
					excludedProperties.Add(ProcessTasksSchema.Constants.P9_SE_NKExceptionEvent);
					if (!source.IsTask)
					{
						excludedProperties.Add(ProcessTasksSchema.Constants.P9_ActualDate);
						excludedProperties.Add(ProcessTasksSchema.Constants.P9_ActualDateUtc);
						excludedProperties.Add(ProcessTasksSchema.Constants.P9_Status);
					}

					if (ProcessTasksLookups.IsMacroCondition(source.P9_Condition2))
					{
						excludedProperties.Add(ProcessTasksSchema.Constants.P9_Condition2Value); // P9_Condition2Value for UDF conditions should be loaded from dbo.StmNote
					}
				}
				if (source.P9_Type != Core.Constants.Workflow.WorkflowTriggerType)
				{
					result.SetP9_GS_NKAssignedStaffMemberCore(source.P9_GS_NKAssignedStaffMember);
				}
				BusinessObjectCloneArgs args = new BusinessObjectCloneArgs(excludedProperties.ToArray(), clonedType, true);
				result.CopyPersistentValuesFrom(source, args);

				if (isTemplateProcessTask)
				{
					result.P9_SE_NKExceptionEvent = source.P9_SE_NKExceptionEvent;
					if (isTargetTemplate)
					{
						result.TemplateConditions.TemplateCondition2Value = source.TemplateConditions.TemplateCondition2Value;
					}
					else if (ProcessTasksLookups.IsMacroCondition(source.P9_Condition2) && source.IsMilestoneOrWorkflowTrigger)
					{
						result.P9_Condition2ValueHash = source.P9_Condition2ValueHash; // We are only comparing UDF conditions for triggers and milestones
					}
				}

				((ILightValidationInternals)result).IsValid = ((ILightValidationInternals)source).IsValid;
			}

			return result;
		}
	}
}
