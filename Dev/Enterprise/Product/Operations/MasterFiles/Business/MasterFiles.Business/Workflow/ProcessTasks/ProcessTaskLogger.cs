using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.MasterFiles.Business
{
	class ProcessTaskLogger
	{
		public ProcessTaskLogger(ProcessTask processTask)
		{
			this.processTask = processTask;
		}

		readonly ProcessTask processTask;
		bool RaisingEventLog { get; set; }

		internal StmALog CreateRecreateOrUpdateEventLog(EventValue eventValue, bool forceAdd = false)
		{
			if (!RaisingEventLog)
			{
				RaisingEventLog = true;
				try
				{
					return processTask.ParentBusinessObject.GetLogs().CreateRecreateOrUpdateEventLog(eventValue, forceAdd);
				}
				finally
				{
					RaisingEventLog = false;
				}
			}
			return null;
		}

		internal void AddExceptionRaisedLog(ZString exceptionCode) => AddExceptionLog(exceptionCode, Events.ExceptionRaised);
		internal void AddExceptionActionedLog(ZString exceptionCode) => AddExceptionLog(exceptionCode, Events.ExceptionActioned);

		void AddExceptionLog(ZString exceptionCode, Event eventReference)
		{
			if (exceptionCode.IsEmpty || !(processTask.Parent is IStmALogParent parent))
			{
				return;
			}

			parent.Logs.AddNew(eventReference, BuildReference(exceptionCode, eventReference));
		}

		string BuildReference(ZString exceptionCode, Event eventReference)
		{
			if (WorkflowDataRegistry.Instance.ExceptionEXRReference.Value || eventReference == Events.ExceptionActioned)
			{
				var referenceBuilder = EventLogReferenceBuilder.New();

				referenceBuilder.AddMandatory(EventReferenceParameters.Type, exceptionCode);

				if (!processTask.P9_SE_NKMilestoneEvent.IsEmpty)
				{
					referenceBuilder.AddMandatory(EventReferenceParameters.EventCode, processTask.P9_SE_NKMilestoneEvent);
				}

				referenceBuilder.AddMandatory(EventReferenceParameters.Description, processTask.P9_Description);

				return referenceBuilder.Build();
			}
			else
			{
				var reference = $"Type:[{exceptionCode}]";

				if (!processTask.P9_SE_NKMilestoneEvent.IsEmpty)
				{
					reference += $"; Event:[{processTask.P9_SE_NKMilestoneEvent}]";
				}

				return reference;
			}
		}

		internal void RemoveExceptionLog(ZString exceptionCode, Event eventReference)
		{
			if (!(processTask.Parent is IStmALogParent parent))
			{
				return;
			}

			var log = parent.Logs.MostRecentLogByPostedTime(eventReference, log => log.SL_Reference == BuildReference(exceptionCode, eventReference));
			if (log != null && !log.IsInDatabase)
			{
				log.Cancel();
			}
		}

		#region StatusChangeLog Impl

		public class StatusSpan : ISpan<DateTime>
		{
			public StatusSpan(ZString status, ZDateTimeOffset from, ZDateTimeOffset to)
			{
				Status = status;
				From = from;
				To = to;
			}

			public ZString Status { get; }
			public ZDateTimeOffset From { get; }
			public ZDateTimeOffset To { get; }

			DateTime ISpan<DateTime>.Start => From.ToDateTime();
			DateTime ISpan<DateTime>.End => To.ToDateTime();
		}

		#endregion

		#region Task Penetration Reset Log

		internal void AddTaskPenetrationResetLog()
		{
			var taskPenetrationResetEventParameters = ObjectFactory.Get<TaskPenetrationResetEventParametersStrategy>("BMTaskPenetrationResetEventParametersStrategy");
			var additionalParams = taskPenetrationResetEventParameters.GetLogReferenceParameters(processTask).ToArray();

			var builder = EventLogReferenceBuilder.New()
				.AddMandatory(ProcessTaskStatusChangeLog.FromParameterCode, processTask.P9_GS_NKAssignedStaffMemberInfo.OriginalValue.ToString())
				.AddMandatory(ProcessTaskStatusChangeLog.ToParameterCode, processTask.P9_GS_NKAssignedStaffMember);

			foreach (var param in additionalParams)
			{
				builder.AddMandatory(param.Key, param.Value);
			}
			processTask.Logs.AddNew(Events.TaskPenetrationReset, builder.Build());
		}

		#endregion
	}
}
