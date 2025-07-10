using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetInstructionProcessHandlingInfo : ProcessHandlingInfo
	{
		public DtbConsignmentRunSheetInstructionProcessHandlingInfo(DtbConsignmentRunSheetInstruction instruction)
			: base(instruction)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			var result = Enumerable.Empty<IBaseTrigger>();
			var runSheet = Instruction.RunSheet;
			if (runSheet != null)
			{
				result = TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(runSheet, logBeingAdded, TriggerLineTypes.Codes.RunSheetInstruction).Select(s => s.trigger);
			}
			return result;
		}

		DtbConsignmentRunSheetInstruction Instruction
		{
			get { return (DtbConsignmentRunSheetInstruction)LogParent; }
		}
	}
}
