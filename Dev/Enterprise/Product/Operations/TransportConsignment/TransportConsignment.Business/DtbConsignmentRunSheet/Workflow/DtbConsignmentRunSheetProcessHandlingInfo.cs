using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetProcessHandlingInfo : ProcessHandlingInfo
	{
		public DtbConsignmentRunSheetProcessHandlingInfo(DtbConsignmentRunSheet runSheet)
			: base(runSheet)
		{
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			foreach (var instruction in RunSheet.RunSheetInstructions)
			{
				yield return new PropagationLink(instruction, new[] { RunSheet }, "RunSheet");
			}
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		DtbConsignmentRunSheet RunSheet
		{
			get { return (DtbConsignmentRunSheet)LogParent; }
		}
	}
}
