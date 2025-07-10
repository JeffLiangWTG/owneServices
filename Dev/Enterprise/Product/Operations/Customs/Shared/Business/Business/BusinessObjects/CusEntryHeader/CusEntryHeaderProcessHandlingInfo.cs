using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Customs.Business
{
	public class CusEntryHeaderProcessHandlingInfo : ProcessHandlingInfo
	{
		public CusEntryHeaderProcessHandlingInfo(CusEntryHeader header)
			: base(header)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded) => Enumerable.Empty<CascadingLink>();

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			var relatedDeclaration = CusEntryHeader.Declaration;
			var relatedShipment = relatedDeclaration?.Shipment;
			var logParent = (IStmALogParent)relatedShipment ?? relatedDeclaration;
			return logParent != null ? TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(logParent, logBeingAdded, TriggerLineTypes.Codes.CusEntryHeader).Select(p => p.trigger) : Enumerable.Empty<IBaseTrigger>();
		}

		CusEntryHeader CusEntryHeader => (CusEntryHeader)LogParent;
	}
}
