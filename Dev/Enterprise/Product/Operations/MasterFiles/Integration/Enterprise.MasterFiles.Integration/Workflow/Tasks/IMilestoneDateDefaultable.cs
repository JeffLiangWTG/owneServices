using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IMilestoneDateDefaultable : ITriggerConditions
	{
		BusinessObject Parent { get; }
		bool HasTemplate { get; }
		bool IsLineTrigger { get; }
		bool IsWorkflowTrigger { get; }
		bool IsEstimateTrigger { get; }
		bool IsInDatabase { get; }
		ZDateTimeOffset ScheduledDate { get; set; }
		ZDateTimeOffset ActualDate { get; set; }
		ZString ActualDateUpdateType { get; set; }
		ZDateTime TemplateCreateTimeUtc { get; }
		bool TrySetActualDateForEvent(IStmALog actualLog, BusinessObject bizo, ZDateTimeOffset time);
		bool GetLogIsValidForDateDefaulting(IStmALog log);

		ZString TemplateCondition1 { get; } // Needed for identifying which transport leg a Consol/Container event is assosciated with.
	}
}
