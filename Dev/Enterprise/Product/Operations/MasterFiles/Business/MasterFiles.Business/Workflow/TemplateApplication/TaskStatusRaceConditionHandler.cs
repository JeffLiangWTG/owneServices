using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class TaskStatusRaceConditionHandler
	{
		public static void Process(IEnumerable<BusinessObject> objects)
		{
			// find all tasks in the objects to be written that need to be checked for WRK status violation
			var tasks = objects.OfType<ProcessTask>().Where(t => TaskIsCandidateForStatusViolation(t)).ToList();

			using (var manager = new DisposableManager())
			{
				for (int i = 0; i < tasks.Count; i++)
				{
					var task = tasks[i];

					var key = "taskStatusLock_" + task.P9_GS_NKAssignedStaffMember;
					if (Db.Connection.TryGetLock(key, TimeSpan.FromSeconds(5), out var appLock))
					{
						manager.Subscribe(appLock);
					}
					else
					{
						var message = FormattableString.Invariant($"{nameof(Db.Connection.TryGetLock)} failed in {nameof(TaskStatusRaceConditionHandler)} on task {task.P9_TaskID} for staff {task.P9_GS_NKAssignedStaffMember} (task {i} out of {tasks.Count})");
						TemplateApplicationRaceConditionHandlerBase<ProcessTask>.ThrowConcurrencyException(task, message);
					}

					// find all tasks is the database that will cause WRK status violation for a given task
					var sql = "SELECT P9_TaskID FROM dbo.ProcessTasks " + SqlWhereClauseForStatusViolationTasks();

					var taskIds = new List<string>();
					Db.Connection.ExecuteReader(sql,
						c => c.AddParameter("@StaffCode", ProcessTasksSchema.P9_GS_NKAssignedStaffMember.SqlDbType, task.P9_GS_NKAssignedStaffMember.ToString()),
						r =>
						{
							taskIds.Add(r.GetString(0));
						});

					// remove all tasks that in the objects to be written will fix the possible WRK status violation for a given task
					var finalTaskIds = new List<string>(taskIds);
					foreach (var taskId in taskIds)
					{
						if (objects.OfType<ProcessTask>().Any(t => TaskIsFixingStatusViolation(t, taskId, task.P9_GS_NKAssignedStaffMember)))
						{
							finalTaskIds.Remove(taskId);
						}
					}

					// include the task we are looking at in the report
					if (!finalTaskIds.Contains(task.P9_TaskID))
					{
						finalTaskIds.Add(task.P9_TaskID);
					}

					if (finalTaskIds.Count > 1)
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
						var message = FormattableString.Invariant($"Unreconcilable concurrency error due to multiple WRK tasks for staff {task.P9_GS_NKAssignedStaffMember} detected: {string.Join(", ", finalTaskIds)}");
						TemplateApplicationRaceConditionHandlerBase<ProcessTask>.ThrowConcurrencyException(task, message);
					}
				}
			}
		}

		static bool TaskIsCandidateForStatusViolation(ProcessTask t)
		{
			return !t.IsDeleted &&
				   t.IsTask &&
				   t.P9_Status == ProcessTaskStatusCodeList.Codes.Working &&
				   !t.P9_GS_NKAssignedStaffMember.IsEmpty;
		}

		static string SqlWhereClauseForStatusViolationTasks()
		{
			return @"WHERE P9_GS_NKAssignedStaffMember = @StaffCode
AND P9_Type NOT IN ('TRG', 'MIL', 'EXC')
AND P9_Status = '" + ProcessTaskStatusCodeList.Codes.Working + "'";
		}

		static bool TaskIsFixingStatusViolation(ProcessTask t, ZString taskId, ZString staffCode)
		{
			return !t.IsDeleted &&
				   t.P9_TaskID == taskId &&
				   (t.P9_Status != ProcessTaskStatusCodeList.Codes.Working ||
					t.P9_GS_NKAssignedStaffMember != staffCode);
		}
	}
}
