using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickProcessHandlingInfo : ProcessHandlingInfo
	{
		public WhsPickProcessHandlingInfo(WhsPick pick)
			: base(pick)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var orders = Pick.Orders;

			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, orders.Select(order => order.PK));
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, logBeingAdded.SL_SE_NKEvent);
			query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, WhsDocketSchema.Constants.Prefix);
			query.AddToFilter(ProcessTasksSchema.P9_RespondToCascadedEvents, true);
			var processTasks = Pick.Factory.Load<ProcessTask>(query);

			var links = orders.Select(order => new CascadingLink
			{
				Parent = order,
				Triggers = processTasks.Where(processTask => processTask.P9_ParentID == order.PK).ToArray()
			});

			return links;
		}

		protected override bool IsEventLogApplicableForCascading(IStmALog logBeingAdded)
		{
			return logBeingAdded.SL_SE_NKEvent == Events.PackingCommencedCode;
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			return Enumerable.Empty<PropagationLink>();
		}

		WhsPick Pick => (WhsPick)LogParent;
	}
}
