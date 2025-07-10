using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// We check for equality based on template PK.
	/// This gets rid of the possibility of weird duplication bugs when templates or workflow items change.
	///
	/// Anything that couldn't be grouped by template PK is then grouped by a pile of heuristics.
	/// </summary>
	class DefaultTaskToTemplateComparer : IEqualityComparer<ProcessTask>
	{
		static ZGuid TemplatePK(ProcessTask task) => task.TemplateID;
		public bool Equals(ProcessTask x, ProcessTask y) => TemplatePK(x).Equals(TemplatePK(y));
		public int GetHashCode(ProcessTask obj) => TemplatePK(obj).GetHashCode();
	}

	class TaskToTemplateComparer : IEqualityComparer<ProcessTask>
	{
		public bool Equals(ProcessTask t1, ProcessTask t2)
		{
			return EqualsCore(t1, t2);
		}

		protected virtual bool EqualsCore(ProcessTask t1, ProcessTask t2)
		{
			return t1.TemplateID.Equals(t2.TemplateID) && t1.P9_Type.Equals(t2.P9_Type);
		}

		public int GetHashCode(ProcessTask task)
		{
			return GetHashCodeCore(task);
		}

		protected virtual int GetHashCodeCore(ProcessTask task)
		{
			return task.TemplateID.GetHashCode();
		}
	}

	class TriggerToTemplateComparer : IEqualityComparer<ProcessTask>
	{
		public bool Equals(ProcessTask t1, ProcessTask t2)
		{
			var result = t1.P9_Type.Equals(t2.P9_Type) &&
				t1.P9_SE_NKMilestoneEvent.Equals(t2.P9_SE_NKMilestoneEvent) &&
				t1.P9_Description.Equals(t2.P9_Description) &&
				t1.P9_Condition1.Equals(t2.P9_Condition1) &&
				t1.P9_Condition2.Equals(t2.P9_Condition2) &&
				t1.TemplateConditions.OriginCountryCode.Equals(t2.TemplateConditions.OriginCountryCode) &&
				t1.TemplateConditions.DestinationCountryCode.Equals(t2.TemplateConditions.DestinationCountryCode) &&
				t1.P9_RespondToCascadedEvents.Equals(t2.P9_RespondToCascadedEvents) &&
				t1.TriggerConditions.TriggerContextCode.Equals(t2.TriggerConditions.TriggerContextCode) &&
				t1.P9_TriggerField.Equals(t2.P9_TriggerField);

			if (!result)
			{
				return result;
			}

			if (t1.TemplateConditions.Condition2ValueStyle != TemplateConditionValueStyle.Unused || t2.TemplateConditions.Condition2ValueStyle != TemplateConditionValueStyle.Unused)
			{
				result &= t1.ShouldUseCondition2Hash.Equals(t2.ShouldUseCondition2Hash);
				if (t1.ShouldUseCondition2Hash)
				{
					result &= t1.P9_Condition2ValueHash.Equals(t2.P9_Condition2ValueHash);
				}
				else
				{
					result &= t1.P9_Condition2Value.Equals(t2.P9_Condition2Value);
				}
			}

			if (t1.TriggerConditions.TriggerContextCode.Equals(TriggerUserContextList.Codes.Specified))
			{
				result &= t1.TriggerConditions.TriggerStaffCode.Equals(t2.TriggerConditions.TriggerStaffCode) &&
					t1.TriggerConditions.TriggerBranch.Equals(t2.TriggerConditions.TriggerBranch) &&
					t1.TriggerConditions.TriggerDepartment.Equals(t2.TriggerConditions.TriggerDepartment) &&
					t1.TriggerConditions.TriggerCompany.Equals(t2.TriggerConditions.TriggerCompany);
			}

			if (t1.P9_RespondToCascadedEvents || t2.P9_RespondToCascadedEvents)
			{
				result &= t1.P9_CascadedEventsContext.Equals(t2.P9_CascadedEventsContext);
			}

			result &= t1.GetTriggerConditionsComparer().Equals(t1, t2) ||
					t2.GetTriggerConditionsComparer().Equals(t1, t2);
			return result;
		}

		public int GetHashCode(ProcessTask trigger)
		{
			return trigger.P9_SE_NKMilestoneEvent.GetHashCode() ^
				trigger.P9_Description.GetHashCode();
		}
	}

	class TaskToTemplateTriggerConditionsComparer : IEqualityComparer<ProcessTask>
	{
		public bool Equals(ProcessTask task, ProcessTask template)
		{
			if (!task.P9_TriggerCondition.Equals(template.P9_TriggerCondition))
			{
				return false;
			}
			else if (!task.TriggerConditions.CanEnterTriggerConditionValue)
			{
				return task.P9_TriggerConditionValue.Equals(template.P9_TriggerConditionValue);
			}
			else
			{
				return true;
			}
		}

		public int GetHashCode(ProcessTask obj) => 0;
	}

	class TaskToTemplateReapplicationComparer : TaskToTemplateComparer
	{
		protected override bool EqualsCore(ProcessTask t1, ProcessTask t2)
		{
			return base.EqualsCore(t1, t2) && t1.P9_Sequence.Equals(t2.P9_Sequence);
		}

		protected override int GetHashCodeCore(ProcessTask task)
		{
			return base.GetHashCodeCore(task) ^ task.P9_Sequence.GetHashCode();
		}
	}
}
