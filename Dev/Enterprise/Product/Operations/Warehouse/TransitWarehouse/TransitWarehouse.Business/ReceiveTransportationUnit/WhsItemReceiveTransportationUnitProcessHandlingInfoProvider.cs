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
	public class WhsItemReceiveTransportationUnitProcessHandlingInfoProvider : ProcessHandlingInfo
	{
		public WhsItemReceiveTransportationUnitProcessHandlingInfoProvider(WhsItemReceiveTransportationUnit transportationUnit)
			: base(transportationUnit)
		{
			this.transportationUnit = Argument.NotNull(transportationUnit, "WhsItemReceiveTransportationUnit");
		}
		readonly WhsItemReceiveTransportationUnit transportationUnit;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var rcnQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveConsignment));
			var packageQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment);
			packageQuery.AddToFilter(JoinCondition.Or, WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader, transportationUnit.PK);

			var asnPivotQuery = new ZDBOnlySubQuery(typeof(WhsItemReceiveASNRTUPivot), WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN);
			asnPivotQuery.AddToFilter(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, transportationUnit.PK);
			packageQuery.AddSubQuery(WhsItemPackageStateSchema.WPS_WRP_ReceiveExpectedPacking, asnPivotQuery, JoinCondition.Or);

			rcnQuery.AddSubQuery(packageQuery, JoinCondition.And);

			var consignments = transportationUnit.Factory.Load<WhsItemReceiveConsignment>(rcnQuery).ToList();

			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, consignments.Select(c => c.PK));
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, logBeingAdded.SL_SE_NKEvent);
			query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, WhsItemReceiveConsignmentSchema.Constants.Prefix);
			query.AddToFilter(ProcessTasksSchema.P9_RespondToCascadedEvents, true);
			var processTasks = transportationUnit.Factory.Load<ProcessTask>(query);

			return consignments.ConvertAll(consignment => new CascadingLink()
			{
				Parent = consignment,
				Triggers = processTasks.Where(p => p.P9_ParentID == consignment.PK).ToArray()
			});
		}
	}
}
