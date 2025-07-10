using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	public class ScheduledDelayedEventViewModel : NonPersistentBusinessObject
	{
		public ScheduledDelayedEventViewModel(IActionSchedule schedule)
		{
			Argument.NotNull(schedule, nameof(schedule));
			this.schedule = schedule;
		}

		public ZDateTime ExecutionDateTimeUtc => schedule.ExecutionDateTimeUtc;

		public ZString SourceActionReference => GetParameter("ACT");

		public ZString SourceTriggerEventCode => GetParameter("EVT");

		public ZString SourceActionOffset => GetParameter("OFF");

		public ZBool SourceTriggerIsEstimate => GetParameter("EST") == "Y";

		public ZString SourceEventUserCode => GetParameter("USR");

		public ZString SourceEventBranchCode => GetParameter("BRN");

		public ZString SourceEventDepartmentCode => GetParameter("DEP");

		public ZString EventReference => schedule.JsonParameter;

		public ZString ExecutionStatus => schedule.ExecutionStatus;

		public ZString ExecutionResult => schedule.ExecutionResult;

		public ZDateTime SystemCreateTimeUtc => schedule.SystemCreateTimeUtc;

		#region Implementation

		readonly IActionSchedule schedule;

		ObservableDictionary<string, string> ReferenceParameters
		{
			get
			{
				if (referenceParameters == null)
				{
					referenceParameters = StmALog.GetParametersFromReference(schedule.JsonParameter);
				}
				return referenceParameters;
			}
		}

		ObservableDictionary<string, string> referenceParameters;

		string GetParameter(string paramName)
		{
			return ReferenceParameters.TryGetValue(paramName, out string paramValue) ? paramValue : null;
		}

		#endregion
	}
}
