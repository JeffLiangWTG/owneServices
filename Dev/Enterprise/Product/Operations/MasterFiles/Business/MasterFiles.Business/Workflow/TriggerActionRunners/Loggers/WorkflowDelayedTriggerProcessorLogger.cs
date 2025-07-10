using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	static class WorkflowDelayedTriggerProcessorLogger
	{
		public static string TriggerFiredLog(BusinessObject trigger)
		 => $"Fired delayed trigger [{trigger.HumanReadableName}].";

		public static string TriggerAlreadyFiredLog(BusinessObject trigger)
		 => $"Trigger [{trigger.HumanReadableName}] has already fired within the delay duration.";

		public static string TriggerDeletedLog()
			=> (NoResString)"Cancelled: the trigger has been deleted.";

		public static string SourceEventCancelledLog(BusinessObject trigger)
			=> $"Trigger [{trigger.HumanReadableName}] not fired as source event is cancelled.";

		public static string DuplicatesSuppressed(int duplicateCount)
			=> $"{duplicateCount} duplicates suppressed.";

		public static string TriggerCannotBeNegative(BusinessObject trigger)
			=> $"Trigger [{trigger.HumanReadableName}] countdown cannot be negative.";
	}
}
