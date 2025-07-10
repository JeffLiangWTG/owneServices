using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// This class ought to be the only source of WTA logs in the codebase.
	/// </summary>
	class WorkflowTemplateApplicationLogBuilder
	{
		public WorkflowTemplateApplicationLogBuilder(IWorkflowProvider logProvider, IEnumerable<TemplateItemApplicationMap> templateItemApplication)
		{
			this.logProvider = logProvider;
			this.bits = templateItemApplication;
		}

		readonly IWorkflowProvider logProvider;
		readonly IEnumerable<TemplateItemApplicationMap> bits;

		ZQuery GetExistingLogQuery(ProcessTaskTemplate template)
		{
			return new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, template.PK.ToString())
			{ FetchOnlyFromLocalCache = true }
				.AddToFilter(StmALogSchema.SL_Parent, logProvider.WorkflowItems.Parent.PK)
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTemplateAppliedCode)
				.AddToFilter(StmALogSchema.SL_IsCancelled, false)
				.AddToFilter(StmALogSchema.SL_IsEstimate, false);
		}

		StmALog FindPreviousLog(ProcessTaskTemplate template)
		{
			return logProvider.Logs.Find(GetExistingLogQuery(template)).FirstOrDefault(l => !l.IsInDatabase);
		}

		(int TaskCount, int TriggerCount, int MilestoneCount, int TriggerMergeCount, int MilestoneMergeCount)
		CountTaskTriggerMilestone(StmALog existingLog, IEnumerable<TemplateItemApplicationMap> map)
		{
			int GetIntegerFromParameter(string param)
			{
				if (existingLog != null && existingLog.Parameters.TryGetValue(param, out string value) && int.TryParse(value, out int result))
				{
					return result;
				}
				else
				{
					return 0;
				}
			}

			var taskCount = GetIntegerFromParameter(Constants.TaskCode);
			var triggerCount = GetIntegerFromParameter(Constants.TriggerCode);
			var milestoneCount = GetIntegerFromParameter(Constants.MilestoneCode);
			var triggerMergeCount = GetIntegerFromParameter(Constants.TriggerMergedCode);
			var milestoneMergeCount = GetIntegerFromParameter(Constants.MilestoneMergedCode);

			foreach (var item in map.SelectMany(s => s.Applications))
			{
				var task = item.TemplateTask;
				if (!task.IsDeleted)
				{
					switch (task.P9_Type)
					{
						case Constants.MilestoneCode:
							if (item.ShouldCloneTask)
							{
								milestoneCount++;
							}
							else
							{
								milestoneMergeCount++;
							}
							break;
						case Constants.TriggerCode:
							if (item.ShouldCloneTask)
							{
								triggerCount++;
							}
							else
							{
								triggerMergeCount++;
							}
							break;
						default:
							taskCount++;
							break;
					}
				}
				item.Dispose();
			}

			return (taskCount, triggerCount, milestoneCount, triggerMergeCount, milestoneMergeCount);
		}

		public IList<StmALog> Build()
		{
			var results = new List<StmALog>();

			if (WorkflowDataRegistry.Instance.EnableWorkflowTemplateAppliedEvent.Value)
			{
				foreach (var group in bits.GroupBy(b => b.Template))
				{
					var existingLog = FindPreviousLog(group.Key);
					var (taskCount, triggerCount, milestoneCount, triggerMergedCount, milestoneMergedCount) = CountTaskTriggerMilestone(existingLog, group);

					var refBuilder = EventLogReferenceBuilder.New()
						.AddMandatory(group.Key.PK.ToString())
						.AddShortenable(group.Key.P0_Name);

					if (taskCount > 0)
					{
						refBuilder.AddMandatory(Constants.TaskCode, taskCount.ToString(CultureInfo.InvariantCulture));
					}

					if (triggerCount > 0)
					{
						refBuilder.AddMandatory(Constants.TriggerCode, triggerCount.ToString(CultureInfo.InvariantCulture));
					}

					if (milestoneCount > 0)
					{
						refBuilder.AddMandatory(Constants.MilestoneCode, milestoneCount.ToString(CultureInfo.InvariantCulture));
					}

					if (triggerMergedCount > 0)
					{
						refBuilder.AddMandatory(Constants.TriggerMergedCode, triggerMergedCount.ToString(CultureInfo.InvariantCulture));
					}

					if (milestoneMergedCount > 0)
					{
						refBuilder.AddMandatory(Constants.MilestoneMergedCode, milestoneMergedCount.ToString(CultureInfo.InvariantCulture));
					}

					ServiceTaskTrackingLogHelper.AddServiceTaskDetails(refBuilder);

					if (WorkflowDataRegistry.Instance.EnableEnvironmentLoggingOnWorkflowTemplateAppliedEvent.Value)
					{
						refBuilder.AddShortenable(Constants.MachineNameCode, System.Environment.MachineName);
						refBuilder.AddMandatory(Constants.ProcessIDCode, Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture));
						refBuilder.AddMandatory(Constants.ThreadIDCode, Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture));
					}

					var eventValue = new EventValue(Events.WorkflowTemplateApplied, isEstimate: false, eventTime: ZDateTimeOffset.Now, reference: refBuilder.Build());
					existingLog?.Delete();
					results.Add(logProvider.Logs.AddNew(eventValue));
				}
			}
			else
			{
				bits.SelectMany(b => b.Applications).ForEach(f => f.Dispose());
			}

			return results;
		}

		class Constants
		{
			public const string TaskCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.TaskCode;
			public const string TriggerCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.TriggerCode;
			public const string MilestoneCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MilestoneCode;
			public const string MilestoneMergedCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MilestoneMergedCode;
			public const string TriggerMergedCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.TriggerMergedCode;
			public const string MachineNameCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MachineNameCode;
			public const string ProcessIDCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ProcessIDCode;
			public const string ThreadIDCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ThreadIDCode;
		}
	}
}
