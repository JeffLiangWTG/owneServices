using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountLocationCreator : IWhsCycleCountLocationCreator
	{
		public WhsCycleCountLocation CreateCycleCountLocation(WhsLocation location, int priority = 0)
		{
			Argument.NotNull(location, nameof(location));

			return CreateCycleCountLocation(location.Factory, location.PK, location.LocationType.WLT_DefaultCycleCountGranularity, priority);
		}

		public WhsCycleCountLocation CreateCycleCountLocation(BusinessObjectFactory factory, ZGuid locationPK, ZString granularity, int priority = 0)
			=> CreateCycleCountLocations(factory, new[] { new WhsCycleCountLocationInfo(locationPK.ToGuid(), granularity.ToString(), (byte)priority) }).SingleOrDefault();

		public IEnumerable<WhsCycleCountLocation> CreateCycleCountLocations(BusinessObjectFactory factory, IEnumerable<ZGuid> locationPKs, int priority = 0)
		{
			var infos = new List<WhsCycleCountLocationInfo>();

			var locations = factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.PK, locationPKs));
			var locatonTypes = factory.Load<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.PK, locations.Select(l => l.WLV_WLT_LocationType).Distinct())).ToDictionary(l => l.PK);
			foreach (var location in locations)
			{
				infos.Add(new WhsCycleCountLocationInfo(location.PK.ToGuid(), locatonTypes[location.WLV_WLT_LocationType].WLT_DefaultCycleCountGranularity, (byte)priority));
			}
			return CreateCycleCountLocations(factory, infos);
		}

		public IEnumerable<WhsCycleCountLocation> CreateCycleCountLocations(BusinessObjectFactory factory, IEnumerable<WhsCycleCountLocationInfo> cycleCountLocInfos)
		{
			Argument.NotNull(factory, nameof(factory));

			var query = new ZDBOnlyQuery(typeof(WhsCycleCountLocation));
			query.AddToFilter(WhsCycleCountLocationSchema.WCL_EndTime, null);

			var varianceSubQuery = new ZDBOnlySubQuery(typeof(WhsCycleCountLocationVariance), WhsCycleCountLocationVarianceSchema.WCC_WCL_CycleCountLocation);
			varianceSubQuery.AddToFilter(WhsCycleCountLocationVarianceSchema.WCC_Status, CycleCountVarianceStatus.Codes.Open);
			query.AddSubQuery(varianceSubQuery, JoinCondition.Or);
			query.AddToFilter(WhsCycleCountLocationSchema.WCL_WL_Location, cycleCountLocInfos.Select(c => c.LocationPK));

			var existingCycleCountLocPKs = factory.Load<WhsCycleCountLocation>(query).Select(c => c.WCL_WL_Location).ToHashSet();
			var newCycleCountLocInfos = cycleCountLocInfos
				.Where(cl => !existingCycleCountLocPKs.Contains(cl.LocationPK))
				.ToArray();
			var warehouseTaskManagementCollection = LoadWarehouseTaskManagementEnabledFromCycleCountLocations(factory, newCycleCountLocInfos.Select(s => new ZGuid(s.LocationPK)).ToArray());
			var createdCycleCounts = new List<WhsCycleCountLocation>();
			foreach (var cycleCountLocInfo in newCycleCountLocInfos)
			{
				var newCount = factory.New<WhsCycleCountLocation>();
				newCount.WCL_WL_Location = cycleCountLocInfo.LocationPK;
				newCount.WCL_Granularity = cycleCountLocInfo.Granularity;
				newCount.WCL_Priority = cycleCountLocInfo.Priority;
				newCount.WCL_TaskPlanningStatus = InitialTaskPlanningStatusValue(warehouseTaskManagementCollection[cycleCountLocInfo.LocationPK]);
				createdCycleCounts.Add(newCount);
			}

			return createdCycleCounts;
		}

		string InitialTaskPlanningStatusValue(bool isTaskManagementEnabled) => isTaskManagementEnabled ? TaskPlanningStatus.Codes.Ready : string.Empty;

		Dictionary<ZGuid, bool> LoadWarehouseTaskManagementEnabledFromCycleCountLocations(BusinessObjectFactory factory, IReadOnlyCollection<ZGuid> pks)
		{
			var collection = new DynamicBusinessObjectCollection(factory);
			var sql = @"
SELECT 
	DISTINCT WL_PK, WW_GG_ReleaseGroup
FROM 	
	dbo.WhsLocation
	JOIN dbo.WhsRow	ON WL_WR = WR_PK
	JOIN dbo.WhsWarehouse ON WR_WW_Whs = WW_PK
WHERE WL_PK IN (SELECT Value FROM @LocationPKs)";

			collection.Load(sql, new[]
			{
				ZSqlParameter.New("@LocationPKs", pks, WhsLocationSchema.PK, isTableValued: true),
			});

			return collection.ToDictionary(result => (ZGuid)result["WL_PK"], result => ((ZGuid)result["WW_GG_ReleaseGroup"]).IsValid);
		}
	}
}
