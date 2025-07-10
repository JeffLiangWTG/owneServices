using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruitment.Common
{
	public enum TaskStatus
	{
		Passed,
		Failed,
		Incomplete,
		Skipped,
	}

	public class ProcessTaskView : NonPersistentBusinessObject
	{
		public const string TaskP9OutcomePass = "PSS";
		public const string TaskP9OutcomeFail = "FAI";

		public ProcessTaskView(ProcessTask pt)
		{
			_ = Argument.NotNull(pt, nameof(pt));
			task = pt;
		}

		readonly ProcessTask task;
		HRJobApplication parent => task.Parent as HRJobApplication;

		StmALog LatestCheckpointEvent =>
			parent.Logs
				.Find(l => l.SL_Parent == parent.PK)
				.Where(l =>
					l.Event != null
					&& !l.IsCancelled
					&& l.Parameters.ContainsKey("PK")
					&& l.Parameters["PK"] == task.PK.ToString()
					&& (l.Event.SE_Code == AutoEvents.RecruitmentCheckpointPassed.Code
					|| l.Event.SE_Code == AutoEvents.RecruitmentCheckpointFailed.Code))
				.OrderByDescending(l => l.InstantiationTime)
				.ThenByDescending(l => l.SL_PostedTimeUtc)
				.FirstOrDefault();

		[MaxLength(50)]
		public ZString Stage
		{
			get => task.P9_Description;
			set
			{
				task.P9_Description = value.Substring(0, Math.Max(value.Length, StageInfo.MaxLength));
				StageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StageInfo => GetZPropertyInfo(nameof(Stage));

		public ZInt Sequence
		{
			get => task.P9_Sequence;
			set
			{
				task.P9_Sequence = value;
				SequenceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SequenceInfo => GetZPropertyInfo(nameof(Sequence));

		[BusinessObjectTestExclude]
		[List("TaskStatuses")]
		public ZString Status
		{
			get
			{
				string code;

				switch (task.P9_Status)
				{
					case "CLS": // passed/failed
						if (task.P9_Outcome == TaskP9OutcomePass)
						{
							code = nameof(TaskStatus.Passed);
						}
						else if (task.P9_Outcome == TaskP9OutcomeFail)
						{
							code = nameof(TaskStatus.Failed);
						}
						else
						{
							code = nameof(TaskStatus.Skipped);
						}

						break;
					case "CAN": // skipped
						code = nameof(TaskStatus.Skipped);
						break;
					case "ASN": // incomplete 
						code = nameof(TaskStatus.Incomplete);
						break;
					default:
						code = nameof(TaskStatus.Skipped);
						break;
				}

				return TaskStatuses.GetDescriptionFromCode(code);
			}
			set
			{
				task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

				if (value == Status)
				{
					return;
				}

				var rce = LatestCheckpointEvent;
				if (rce != null && !rce.IsNull)
				{
					if (rce.IsInDatabase)
					{
						rce.Cancel();
					}
					else
					{
						rce.Delete();
					}
				}

				var code = TaskStatuses.GetCodeFromDescription(value);
				var couldConvert = Enum.TryParse<TaskStatus>(code, out var status);
				if (!couldConvert)
				{
					status = TaskStatus.Skipped;
				}

				switch (status)
				{
					case TaskStatus.Failed: // failed
						task.P9_Status = "CLS";
						task.P9_Outcome = TaskP9OutcomeFail;
						_ = parent.Logs.AddNew(AutoEvents.RecruitmentCheckpointFailed, new KeyValuePair<string, string>("PK", task.PK.ToString()));
						break;
					case TaskStatus.Passed: // passed
						task.P9_Status = "CLS";
						task.P9_Outcome = TaskP9OutcomePass;
						_ = parent.Logs.AddNew(AutoEvents.RecruitmentCheckpointPassed, new KeyValuePair<string, string>("PK", task.PK.ToString()));
						break;
					case TaskStatus.Incomplete: // incomplete
						task.P9_Status = "ASN";
						break;
					case TaskStatus.Skipped: // skipped
						task.P9_Status = "CAN";
						break;
				}

				StatusInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(nameof(Status));

		public CodeDescriptionPairList TaskStatuses
		{
			get
			{
				if (taskStatuses == null)
				{
					taskStatuses = new CodeDescriptionPairList();
					taskStatuses.AddPair(nameof(TaskStatus.Failed), Res.GetString("A0B21937-A3E8-4A28-AFDC-E554CFC805A4", "Failed"));
					taskStatuses.AddPair(nameof(TaskStatus.Passed), Res.GetString("7725BC1C-FE2C-4169-B8AC-45FA25B8AD2C", "Passed"));
					taskStatuses.AddPair(nameof(TaskStatus.Incomplete), Res.GetString("41D36129-92E8-4D7C-AF5C-01AE1BB739D0", "Incomplete"));
					taskStatuses.AddPair(nameof(TaskStatus.Skipped), Res.GetString("7FF106EE-2B60-4E48-AF6C-4FF7BD4B5782", "Skipped"));
				}
				return taskStatuses;
			}
		}
		CodeDescriptionPairList taskStatuses;
	}
}
