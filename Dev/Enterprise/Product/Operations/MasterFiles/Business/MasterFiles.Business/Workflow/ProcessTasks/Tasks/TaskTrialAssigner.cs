using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class TaskTrialAssigner
	{
		public void AssignResourceToTasks(ZString staff, IEnumerable<ProcessTask> tasks)
		{
			foreach (var task in tasks)
			{
				AssignResourceToTask(staff, task);
			}
		}

		public void AssignResourceToTask(ZString staff, ProcessTask task)
		{
			if (GetAssignedStaffCode(task).IsEmpty)
			{
				assignments.Add(task, staff);
				TaskAssignmentHelper.PopulateRelatedTasksWithSameResource(task, AssignResourceToTask, GetAssignedStaffCode, requireValidResource: false);
			}
		}

		public ZString GetAssignedStaffCode(ProcessTask task)
		{
			ZString staff;
			return assignments.TryGetValue(task, out staff) ? staff : task.P9_GS_NKAssignedStaffMember;
		}

		public bool AreAssignmentsAllowedByDIFRestrictions
		{
			get
			{
				foreach (var task in assignments.Keys)
				{
					if (!TaskAssignmentHelper.DoDirectDIFRestrictionsAllowResourceToBeAssignedToTask(assignments[task], task, GetAssignedStaffCode))
					{
						return false;
					}
				}
				return true;
			}
		}

		public double CapacityInMinutesConsumedByAssignedTasks => assignments.Select(a => a.Key).Sum(t => t.StandardEstimatedDuration.GetMinutesFromDateTimeSpan());

		readonly Dictionary<ProcessTask, ZString> assignments = new Dictionary<ProcessTask, ZString>();
	}
}
