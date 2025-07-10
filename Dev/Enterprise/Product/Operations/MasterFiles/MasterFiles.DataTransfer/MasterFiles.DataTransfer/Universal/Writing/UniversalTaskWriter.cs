using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.Management;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalTaskWriter : IUniversalTaskWriter
	{
		public void PopulateTasks(IEnumerable<IProcessTask> tasks, IDataObject destination)
		{
			if (destination is ITaskCollectionParent taskCollectionParent)
			{
				var taskData = tasks.Cast<ProcessTask>().Select(TransformToUniversalTask).ToList();

				if (taskData.Any())
				{
					taskCollectionParent.SetTaskCollection(() => taskData);
				}
			}
		}

		static Task TransformToUniversalTask(ProcessTask processTask)
		{
			var task = new Task
			{
				TaskID = processTask.P9_TaskID,
				Sequence = processTask.P9_Sequence,
				Description = processTask.P9_Description,
				TaskNotes = processTask.P9_NotesAsString,
				CardNote = processTask.P9_CardNote,
				Type = new CodeDescriptionPair { Code = processTask.P9_Type, Description = processTask.TypeDescription },
				Status = new CodeDescriptionPair { Code = processTask.P9_Status, Description = processTask.StatusDescription },
				EstimatedDuration = GetDuration(processTask.P9_EstDuration),
				EstimateVariationFactor = processTask.P9_EstimateVariationFactor,
				EstimatedStartTimeUTC = new ZDateTimeOffset(processTask.P9_ScheduledDateUtc, TimeSpan.Zero),
				CompletedTimeUTC = new ZDateTimeOffset(processTask.P9_CompletedTimeUtc, TimeSpan.Zero),
				ActualDuration = GetDuration(processTask.P9_ActualDuration),
				ActualStartTimeUTC = new ZDateTimeOffset(processTask.P9_ActualDateUtc, TimeSpan.Zero),
			};

			if (!processTask.P9_EstDuration.IsEmpty)
			{
				task.EstimatedDuration = processTask.P9_EstDuration.ToTimeSpan();
			}

			if (!processTask.P9_ActualDuration.IsEmpty)
			{
				task.ActualDuration = processTask.P9_ActualDuration.ToTimeSpan();
			}

			var staff = processTask.AssignedStaffMember;
			var capability = processTask.RequiredCapability;
			var group = processTask.AssignedGroup;

			if (staff != null)
			{
				task.AssignedStaff = Staff.New(staff);
			}
			if (capability != null)
			{
				task.AssignedCapability = Capability.New(capability);
			}
			if (group != null)
			{
				task.AssignedGroup = Group.New(group);
			}

			return task;
		}

		static TimeSpan GetDuration(ZDateTime processTaskValue)
		{
			return processTaskValue.IsEmpty ? TimeSpan.Zero : processTaskValue.ToTimeSpan();
		}
	}
}
