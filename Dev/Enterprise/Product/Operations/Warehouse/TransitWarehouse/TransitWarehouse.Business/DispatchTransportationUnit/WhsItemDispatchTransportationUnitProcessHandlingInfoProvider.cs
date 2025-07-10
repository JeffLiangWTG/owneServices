using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchTransportationUnitProcessHandlingInfoProvider : ProcessHandlingInfo
	{
		public WhsItemDispatchTransportationUnitProcessHandlingInfoProvider(WhsItemDispatchTransportationUnit unit)
			: base(unit)
		{
			this.transportationUnit = Argument.NotNull(unit, "WhsItemDispatchTransportationUnit");
		}
		readonly WhsItemDispatchTransportationUnit transportationUnit;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var result = new List<CascadingLink>();

			var consignments = transportationUnit.Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader, transportationUnit.PK))
								.Select(p => p.DispatchConsignment)
								.Where(d => d != null)
								.Distinct();

			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, consignments.Select(c => c.PK));
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, logBeingAdded.SL_SE_NKEvent);
			query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, WhsItemDispatchConsignmentSchema.Constants.Prefix);
			query.AddToFilter(ProcessTasksSchema.P9_RespondToCascadedEvents, true);
			var processTasks = transportationUnit.Factory.Load<ProcessTask>(query);

			foreach (var consignment in consignments)
			{
				var target = new CascadingLink
				{
					Parent = consignment,
					Triggers = processTasks.Where(p => p.P9_ParentID == consignment.PK).ToArray()
				};

				result.Add(target);
			}

			return result;
		}
	}
}
