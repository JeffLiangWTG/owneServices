using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderFilterProvider
	{
		public ZQuery GetCTOFilter()
		{
			ZQuery airCTO = new ZQuery(OrgHeaderSchema.OH_IsAirCTO, true);
			ZQuery seaCTO = new ZQuery(OrgHeaderSchema.OH_IsSeaCTO, true);
			ZQuery miscServ = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			ZQuery cTO = new ZQuery(airCTO, JoinCondition.Or, seaCTO);
			return new ZQuery(miscServ, JoinCondition.And, cTO);
		}

		public ZQuery GetDepotFilter()
		{
			var depotQuery = new ZQuery(OrgHeaderSchema.OH_IsPackDepot, true);
			depotQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsUnpackDepot, true);
			depotQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsRoadFreightDepot, true);
			depotQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsRailHead, true);

			var query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			query.AddToFilter(depotQuery, JoinCondition.And);

			return query;
		}

		public ZQuery GetWarehouseFilter()
		{
			return new ZQuery(OrgHeaderSchema.OH_IsWarehouseClient, true);
		}

		public ZQuery GetCTOOrDepotOrWarehouseFilter()
		{
			ZQuery cTOOrDepotFilter = new ZQuery(GetCTOFilter(), JoinCondition.Or, GetDepotFilter());
			return new ZQuery(cTOOrDepotFilter, JoinCondition.Or, GetWarehouseFilter());
		}
	}
}
